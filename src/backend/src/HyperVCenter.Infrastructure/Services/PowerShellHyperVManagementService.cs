using System.Management.Automation;
using System.Management.Automation.Runspaces;
using HyperVCenter.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace HyperVCenter.Infrastructure.Services;

public class PowerShellHyperVManagementService : IHyperVManagementService
{
    private readonly ILogger<PowerShellHyperVManagementService> _logger;

    public PowerShellHyperVManagementService(ILogger<PowerShellHyperVManagementService> logger)
    {
        _logger = logger;
    }

    public async Task<HostInfo> GetHostInfoAsync(string hostname, string username, string password, CancellationToken ct)
    {
        var results = await RunRemoteAsync(hostname, username, password, @"
            $os = Get-WmiObject Win32_OperatingSystem
            $cpu = @(Get-WmiObject Win32_Processor)
            [PSCustomObject]@{
                OsVersion = $os.Caption
                ProcessorCount = ($cpu | Measure-Object -Property NumberOfLogicalProcessors -Sum).Sum
                TotalMemoryBytes = [long]$os.TotalVisibleMemorySize * 1024
            }
        ", ct);

        var obj = results.First();
        return new HostInfo(
            obj.Properties["OsVersion"].Value?.ToString() ?? "Unknown",
            Convert.ToInt32(obj.Properties["ProcessorCount"].Value),
            Convert.ToInt64(obj.Properties["TotalMemoryBytes"].Value));
    }

    public async Task<IReadOnlyList<VmInfo>> GetVirtualMachinesAsync(string hostname, string username, string password, CancellationToken ct)
    {
        var results = await RunRemoteAsync(hostname, username, password,
            "Get-VM | Select-Object Id, Name, State, ProcessorCount, MemoryAssigned", ct);

        return results.Select(r => new VmInfo(
            Guid.Parse(r.Properties["Id"].Value.ToString()!),
            r.Properties["Name"].Value.ToString()!,
            r.Properties["State"].Value.ToString()!,
            Convert.ToInt32(r.Properties["ProcessorCount"].Value),
            Convert.ToInt64(r.Properties["MemoryAssigned"].Value)
        )).ToList();
    }

    public async Task StartVmAsync(string hostname, string username, string password, Guid vmId, CancellationToken ct) =>
        await RunRemoteAsync(hostname, username, password, $"Get-VM -Id '{vmId}' | Start-VM", ct);

    public async Task StopVmAsync(string hostname, string username, string password, Guid vmId, bool force, CancellationToken ct) =>
        await RunRemoteAsync(hostname, username, password,
            force ? $"Get-VM -Id '{vmId}' | Stop-VM -Force" : $"Get-VM -Id '{vmId}' | Stop-VM", ct);

    public async Task PauseVmAsync(string hostname, string username, string password, Guid vmId, CancellationToken ct) =>
        await RunRemoteAsync(hostname, username, password, $"Get-VM -Id '{vmId}' | Suspend-VM", ct);

    public async Task SaveVmAsync(string hostname, string username, string password, Guid vmId, CancellationToken ct) =>
        await RunRemoteAsync(hostname, username, password, $"Get-VM -Id '{vmId}' | Save-VM", ct);

    public async Task RestartVmAsync(string hostname, string username, string password, Guid vmId, CancellationToken ct) =>
        await RunRemoteAsync(hostname, username, password, $"Get-VM -Id '{vmId}' | Restart-VM -Force", ct);

    public async Task<Guid> CreateVmAsync(string hostname, string username, string password, string name, int cpuCount, long memoryBytes, CancellationToken ct)
    {
        var results = await RunRemoteAsync(hostname, username, password, $@"
            $vm = New-VM -Name '{name}' -MemoryStartupBytes {memoryBytes} -Generation 2
            Set-VMProcessor -VM $vm -Count {cpuCount}
            $vm.Id
        ", ct);

        return Guid.Parse(results.First().BaseObject.ToString()!);
    }

    private async Task<IReadOnlyList<PSObject>> RunRemoteAsync(
        string hostname, string username, string password, string script, CancellationToken ct)
    {
        var securePassword = new System.Security.SecureString();
        foreach (var c in password)
            securePassword.AppendChar(c);

        var credential = new PSCredential(username, securePassword);
        var connectionInfo = new WSManConnectionInfo(
            new Uri($"http://{hostname}:5985/wsman"),
            "http://schemas.microsoft.com/powershell/Microsoft.PowerShell",
            credential)
        {
            AuthenticationMechanism = AuthenticationMechanism.Negotiate,
            OperationTimeout = 30000,
            OpenTimeout = 15000,
        };

        using var runspace = RunspaceFactory.CreateRunspace(connectionInfo);
        await Task.Run(() => runspace.Open(), ct);

        using var ps = PowerShell.Create();
        ps.Runspace = runspace;
        ps.AddScript(script);

        var results = await Task.Run(() => ps.Invoke(), ct);

        if (ps.HadErrors)
        {
            var errors = string.Join(Environment.NewLine, ps.Streams.Error.Select(e => e.ToString()));
            _logger.LogError("PowerShell errors on {Hostname}: {Errors}", hostname, errors);
            throw new InvalidOperationException($"PowerShell command failed: {errors}");
        }

        return results.ToList();
    }
}
