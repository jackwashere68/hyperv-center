using System.Diagnostics;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using HyperVCenter.Application.Features.VirtualMachines.Queries;
using MediatR;

namespace HyperVCenter.Web.WebSockets;

public static class VmConsoleHandler
{
    public static void MapVmConsole(this WebApplication app)
    {
        app.Map("/api/virtualmachines/{id:guid}/console", HandleConsoleRequest);
    }

    private static async Task HandleConsoleRequest(
        HttpContext context,
        Guid id,
        IMediator mediator,
        IConfiguration configuration,
        ILogger<Program> logger)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("WebSocket connection required.");
            return;
        }

        // Get VM connection info
        VmConsoleConnectionDto connectionInfo;
        try
        {
            connectionInfo = await mediator.Send(new GetVmConsoleConnectionQuery(id));
        }
        catch (KeyNotFoundException ex)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync(ex.Message);
            return;
        }
        catch (InvalidOperationException ex)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync(ex.Message);
            return;
        }

        var ws = await context.WebSockets.AcceptWebSocketAsync("guacamole");

        logger.LogInformation("VM console WebSocket opened for VM '{VmName}' ({VmId}) on host {Host}",
            connectionInfo.VmName, id, connectionInfo.Hostname);

        // Connect to guacd
        var guacdHost = configuration["Guacamole:GuacdHost"] ?? "localhost";
        var guacdPort = int.Parse(configuration["Guacamole:GuacdPort"] ?? "4822");

        // Resolve WSL2 IP dynamically when configured with "wsl"
        if (guacdHost.Equals("wsl", StringComparison.OrdinalIgnoreCase))
        {
            guacdHost = await ResolveWslIpAsync(logger);
        }

        TcpClient? guacdClient = null;
        try
        {
            guacdClient = new TcpClient();
            await guacdClient.ConnectAsync(guacdHost, guacdPort);
            var guacdStream = guacdClient.GetStream();

            // Resolve hostname for guacd (may need to translate localhost to reachable IP)
            var rdpHostname = connectionInfo.Hostname;
            var hostOverride = configuration["Guacamole:RdpHostOverride"];
            if (!string.IsNullOrEmpty(hostOverride) &&
                (rdpHostname.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                 rdpHostname == "127.0.0.1" || rdpHostname == "::1"))
            {
                rdpHostname = hostOverride;
                logger.LogDebug("Overriding RDP hostname from '{Original}' to '{Override}' for guacd",
                    connectionInfo.Hostname, rdpHostname);
            }

            var resolvedInfo = connectionInfo with { Hostname = rdpHostname };

            // Perform Guacamole handshake with guacd
            await PerformGuacdHandshake(guacdStream, resolvedInfo, ws, logger, context.RequestAborted);

            // Bridge WebSocket ↔ guacd bidirectionally
            await BridgeConnection(ws, guacdStream, logger, context.RequestAborted);
        }
        catch (SocketException ex)
        {
            logger.LogError(ex, "Failed to connect to guacd at {Host}:{Port}", guacdHost, guacdPort);
            if (ws.State == WebSocketState.Open)
            {
                var errorInstruction = GuacamoleProtocol.EncodeInstruction("error",
                    "Cannot connect to remote desktop gateway. Ensure guacd is running.", "519");
                await ws.SendAsync(
                    Encoding.UTF8.GetBytes(errorInstruction),
                    WebSocketMessageType.Text, true, CancellationToken.None);
                await ws.CloseAsync(WebSocketCloseStatus.InternalServerError,
                    "guacd connection failed", CancellationToken.None);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "VM console error for VM {VmId}", id);
            if (ws.State == WebSocketState.Open)
            {
                var errorInstruction = GuacamoleProtocol.EncodeInstruction("error",
                    ex.Message, "519");
                await ws.SendAsync(
                    Encoding.UTF8.GetBytes(errorInstruction),
                    WebSocketMessageType.Text, true, CancellationToken.None);
                await ws.CloseAsync(WebSocketCloseStatus.InternalServerError,
                    "Console error", CancellationToken.None);
            }
        }
        finally
        {
            guacdClient?.Dispose();
            if (ws.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                try
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure,
                        "Session ended", CancellationToken.None);
                }
                catch { /* Best effort */ }
            }
            logger.LogInformation("VM console session ended for VM {VmId}", id);
        }
    }

    private static async Task PerformGuacdHandshake(
        NetworkStream guacdStream,
        VmConsoleConnectionDto info,
        WebSocket ws,
        ILogger logger,
        CancellationToken ct)
    {
        // 1. Send "select" instruction to guacd
        var selectInstruction = GuacamoleProtocol.EncodeInstruction("select", "rdp");
        await WriteToGuacd(guacdStream, selectInstruction, ct);
        logger.LogDebug("Sent to guacd: {Instruction}", selectInstruction.TrimEnd(';'));

        // 2. Read "args" instruction from guacd
        var argsRaw = await GuacamoleProtocol.ReadInstructionAsync(guacdStream, ct)
            ?? throw new InvalidOperationException("guacd closed connection during handshake.");
        logger.LogDebug("Received from guacd: {Instruction}", argsRaw.TrimEnd(';'));

        var (opcode, argNames) = GuacamoleProtocol.ParseInstruction(argsRaw);
        if (opcode != "args")
            throw new InvalidOperationException($"Expected 'args' from guacd, got '{opcode}'.");

        // 3. Send size, audio, video, image, timezone immediately with defaults
        //    guacamole-common-js does NOT send handshake messages — it waits for server instructions.
        //    We must complete the handshake with guacd immediately.
        await WriteToGuacd(guacdStream,
            GuacamoleProtocol.EncodeInstruction("size", "1024", "768", "96"), ct);
        await WriteToGuacd(guacdStream,
            GuacamoleProtocol.EncodeInstruction("audio"), ct);
        await WriteToGuacd(guacdStream,
            GuacamoleProtocol.EncodeInstruction("video"), ct);
        await WriteToGuacd(guacdStream,
            GuacamoleProtocol.EncodeInstruction("image", "image/png", "image/jpeg"), ct);
        await WriteToGuacd(guacdStream,
            GuacamoleProtocol.EncodeInstruction("timezone", "America/New_York"), ct);

        // 4. Build and send "connect" instruction with our connection parameters
        var connectArgs = BuildConnectArgs(argNames, info);
        var connectInstruction = GuacamoleProtocol.EncodeInstruction("connect", connectArgs);
        await WriteToGuacd(guacdStream, connectInstruction, ct);
        logger.LogDebug("Sent connect to guacd with {ArgCount} parameters", connectArgs.Length);

        // 5. Read "ready" from guacd
        var readyRaw = await GuacamoleProtocol.ReadInstructionAsync(guacdStream, ct)
            ?? throw new InvalidOperationException("guacd closed connection after connect.");
        logger.LogDebug("Received from guacd: {Instruction}", readyRaw.TrimEnd(';'));

        var (readyOpcode, readyArgs) = GuacamoleProtocol.ParseInstruction(readyRaw);
        if (readyOpcode == "error")
        {
            throw new InvalidOperationException(
                $"guacd error: {(readyArgs.Length > 0 ? readyArgs[0] : "unknown")}");
        }

        // 6. Forward ready to client — this triggers guacamole-common-js Client state → CONNECTED
        await ws.SendAsync(
            Encoding.UTF8.GetBytes(readyRaw),
            WebSocketMessageType.Text, true, ct);

        logger.LogInformation("Guacamole handshake complete for VM '{VmName}'", info.VmName);
    }

    private static string[] BuildConnectArgs(string[] argNames, VmConsoleConnectionDto info)
    {
        // Map known parameter names to values
        var paramMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["hostname"] = info.Hostname,
            ["port"] = info.Port.ToString(),
            ["username"] = info.Username,
            ["password"] = info.Password,
            ["domain"] = "",
            ["security"] = "vmconnect",
            ["ignore-cert"] = "true",
            ["preconnection-blob"] = info.VmExternalId,
            ["disable-auth"] = "",
            ["enable-font-smoothing"] = "true",
            ["enable-wallpaper"] = "true",
            ["enable-theming"] = "true",
            ["enable-full-window-drag"] = "true",
            ["enable-desktop-composition"] = "true",
            ["enable-menu-animations"] = "true",
            ["color-depth"] = "32",
            ["resize-method"] = "display-update",
            ["enable-audio"] = "",
            ["disable-audio"] = "true",
            ["enable-audio-input"] = "",
            ["enable-printing"] = "",
            ["enable-drive"] = "",
            ["create-drive-path"] = "",
            ["console"] = "",
            ["server-layout"] = "",
            ["timezone"] = "",
            ["client-name"] = "HyperV Center",
            ["gateway-hostname"] = "",
            ["gateway-port"] = "",
            ["gateway-username"] = "",
            ["gateway-password"] = "",
            ["gateway-domain"] = "",
            ["width"] = "",
            ["height"] = "",
            ["dpi"] = "",
            ["disable-gfx"] = "",
            ["force-lossless"] = "",
        };

        // Build args in the order guacd expects
        // Echo back any VERSION_* arg (guacd 1.5+ protocol version negotiation)
        return argNames.Select(name =>
            name.StartsWith("VERSION_", StringComparison.OrdinalIgnoreCase)
                ? name
                : paramMap.GetValueOrDefault(name, "")
        ).ToArray();
    }

    private static async Task BridgeConnection(
        WebSocket ws, NetworkStream guacdStream, ILogger logger, CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        long wsToGuacdBytes = 0, wsToGuacdMsgs = 0;
        long guacdToWsBytes = 0, guacdToWsMsgs = 0;

        var wsToGuacd = Task.Run(async () =>
        {
            var buffer = new byte[65536];
            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    var result = await ws.ReceiveAsync(buffer, cts.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        logger.LogDebug("WebSocket close received");
                        break;
                    }
                    if (result.Count > 0)
                    {
                        wsToGuacdMsgs++;
                        wsToGuacdBytes += result.Count;
                        // Log first 10 messages for debugging
                        if (wsToGuacdMsgs <= 10)
                        {
                            var preview = Encoding.UTF8.GetString(buffer, 0, Math.Min(result.Count, 80));
                            logger.LogDebug("WS→guacd #{Num} ({Bytes}b): {Preview}",
                                wsToGuacdMsgs, result.Count, preview);
                        }
                        await guacdStream.WriteAsync(buffer.AsMemory(0, result.Count), cts.Token);
                        await guacdStream.FlushAsync(cts.Token);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            {
                logger.LogDebug("WebSocket closed prematurely");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "WS→guacd pipe error");
            }
            finally
            {
                logger.LogInformation("WS→guacd totals: {Msgs} messages, {Bytes} bytes",
                    wsToGuacdMsgs, wsToGuacdBytes);
                await cts.CancelAsync();
            }
        }, cts.Token);

        var guacdToWs = Task.Run(async () =>
        {
            var readBuffer = new byte[65536];
            // Accumulation buffer to ensure we only send complete Guacamole instructions
            // (each instruction ends with ';'). This mimics the standard Guacamole servlet behavior.
            var accumulator = new byte[262144]; // 256KB accumulator
            var accumulatorLen = 0;
            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    var read = await guacdStream.ReadAsync(readBuffer, cts.Token);
                    if (read == 0)
                    {
                        logger.LogDebug("guacd stream closed (EOF)");
                        break;
                    }

                    // Append to accumulator
                    if (accumulatorLen + read > accumulator.Length)
                    {
                        // Grow accumulator if needed
                        var newBuf = new byte[Math.Max(accumulator.Length * 2, accumulatorLen + read)];
                        Buffer.BlockCopy(accumulator, 0, newBuf, 0, accumulatorLen);
                        accumulator = newBuf;
                    }
                    Buffer.BlockCopy(readBuffer, 0, accumulator, accumulatorLen, read);
                    accumulatorLen += read;

                    // Find the last ';' in the accumulator — everything up to and including it
                    // is complete instructions that can be forwarded
                    var lastSemicolon = -1;
                    for (var i = accumulatorLen - 1; i >= 0; i--)
                    {
                        if (accumulator[i] == (byte)';')
                        {
                            lastSemicolon = i;
                            break;
                        }
                    }

                    if (lastSemicolon < 0) continue; // No complete instruction yet

                    var sendLen = lastSemicolon + 1;
                    guacdToWsMsgs++;
                    guacdToWsBytes += sendLen;

                    // Log first 10 sends for debugging
                    if (guacdToWsMsgs <= 10)
                    {
                        var preview = Encoding.UTF8.GetString(accumulator, 0, Math.Min(sendLen, 120));
                        logger.LogDebug("guacd→WS #{Num} ({Bytes}b): {Preview}",
                            guacdToWsMsgs, sendLen, preview);
                    }
                    if (guacdToWsMsgs % 100 == 0)
                    {
                        logger.LogDebug("guacd→WS stats: {Msgs} msgs, {KB}KB total",
                            guacdToWsMsgs, guacdToWsBytes / 1024);
                    }

                    if (ws.State == WebSocketState.Open)
                    {
                        await ws.SendAsync(
                            new ArraySegment<byte>(accumulator, 0, sendLen),
                            WebSocketMessageType.Text, true, cts.Token);
                    }
                    else
                    {
                        logger.LogWarning("WebSocket not open (state={State}), dropping {Bytes} bytes from guacd",
                            ws.State, sendLen);
                        break;
                    }

                    // Shift remaining partial instruction to the front
                    var remaining = accumulatorLen - sendLen;
                    if (remaining > 0)
                        Buffer.BlockCopy(accumulator, sendLen, accumulator, 0, remaining);
                    accumulatorLen = remaining;
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "guacd→WS pipe error");
            }
            finally
            {
                logger.LogInformation("guacd→WS totals: {Msgs} messages, {KB}KB",
                    guacdToWsMsgs, guacdToWsBytes / 1024);
                await cts.CancelAsync();
            }
        }, cts.Token);

        await Task.WhenAny(wsToGuacd, guacdToWs);
        await cts.CancelAsync();

        try { await Task.WhenAll(wsToGuacd, guacdToWs); }
        catch (OperationCanceledException) { }
    }

    private static async Task WriteToGuacd(NetworkStream stream, string instruction, CancellationToken ct)
    {
        var bytes = Encoding.UTF8.GetBytes(instruction);
        await stream.WriteAsync(bytes, ct);
        await stream.FlushAsync(ct);
    }

    private static async Task<string> ResolveWslIpAsync(ILogger logger)
    {
        // Ensure Docker and guacd are running inside WSL2
        logger.LogDebug("Ensuring guacd is running in WSL2...");
        await RunProcessAsync("wsl", "-d Ubuntu -u root -- bash -c \"systemctl start docker 2>/dev/null; docker start guacd 2>/dev/null\"");

        // Give guacd a moment to bind its port
        await Task.Delay(2000);

        // Resolve WSL2 IP
        var output = await RunProcessAsync("wsl", "hostname -I");
        var ip = output.Trim().Split(' ')[0];
        if (string.IsNullOrEmpty(ip))
            throw new InvalidOperationException("Could not resolve WSL2 IP address.");

        logger.LogDebug("Resolved WSL2 IP: {WslIp}", ip);
        return ip;
    }

    private static async Task<string> RunProcessAsync(string fileName, string arguments)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        return output;
    }
}
