using System.Diagnostics;

namespace HyperVCenter.Web.WebSockets;

/// <summary>
/// Background service that keeps WSL2 alive by running a persistent sleep process.
/// Without this, WSL2 shuts down after idle timeout, killing guacd.
/// </summary>
public class WslKeepAliveService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<WslKeepAliveService> _logger;

    public WslKeepAliveService(IConfiguration configuration, ILogger<WslKeepAliveService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var guacdHost = _configuration["Guacamole:GuacdHost"] ?? "localhost";
        if (!guacdHost.Equals("wsl", StringComparison.OrdinalIgnoreCase))
            return; // Not using WSL mode

        _logger.LogInformation("WslKeepAliveService started — keeping WSL2 alive for guacd");

        while (!stoppingToken.IsCancellationRequested)
        {
            Process? process = null;
            try
            {
                // Start a long-running WSL process to prevent WSL from shutting down
                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "wsl",
                        Arguments = "-d Ubuntu -- bash -c \"while true; do sleep 60; done\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    }
                };
                process.Start();
                _logger.LogDebug("WSL keep-alive process started (PID {Pid})", process.Id);

                // Wait for the process to exit or cancellation
                await process.WaitForExitAsync(stoppingToken);
                _logger.LogWarning("WSL keep-alive process exited (code {ExitCode}), restarting...",
                    process.ExitCode);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WSL keep-alive error, retrying in 5 seconds...");
            }
            finally
            {
                if (process is { HasExited: false })
                {
                    try { process.Kill(); } catch { }
                }
                process?.Dispose();
            }

            // Wait before restarting
            try { await Task.Delay(5000, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }

        _logger.LogInformation("WslKeepAliveService stopped");
    }
}
