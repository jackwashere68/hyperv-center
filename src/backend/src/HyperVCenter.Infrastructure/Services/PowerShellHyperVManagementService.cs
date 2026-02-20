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

    public async Task<VmHardwareInfo> GetVmHardwareAsync(string hostname, string username, string password, Guid vmId, CancellationToken ct)
    {
        var results = await RunRemoteAsync(hostname, username, password, $@"
            $vm = Get-VM -Id '{vmId}'

            $disks = @(Get-VMHardDiskDrive -VM $vm | ForEach-Object {{
                $vhd = $null
                try {{ $vhd = Get-VHD -Path $_.Path -ErrorAction SilentlyContinue }} catch {{}}
                [PSCustomObject]@{{
                    ControllerType = $_.ControllerType.ToString()
                    ControllerNumber = $_.ControllerNumber
                    ControllerLocation = $_.ControllerLocation
                    Path = $_.Path
                    VhdFormat = if ($vhd) {{ $vhd.VhdFormat.ToString() }} else {{ 'Unknown' }}
                    VhdType = if ($vhd) {{ $vhd.VhdType.ToString() }} else {{ 'Unknown' }}
                    CurrentSize = if ($vhd) {{ $vhd.FileSize }} else {{ 0 }}
                    MaxSize = if ($vhd) {{ $vhd.Size }} else {{ 0 }}
                }}
            }})

            $nics = @(Get-VMNetworkAdapter -VM $vm | ForEach-Object {{
                [PSCustomObject]@{{
                    Name = $_.Name
                    SwitchName = $_.SwitchName
                    MacAddress = $_.MacAddress
                    IpAddresses = @($_.IPAddresses)
                }}
            }})

            $snapshots = @(Get-VMSnapshot -VM $vm -ErrorAction SilentlyContinue | ForEach-Object {{
                [PSCustomObject]@{{
                    Id = $_.Id
                    Name = $_.Name
                    CreationTime = $_.CreationTime.ToString('o')
                    ParentSnapshotName = $_.ParentSnapshotName
                }}
            }})

            [PSCustomObject]@{{
                Generation = $vm.Generation
                Version = $vm.Version
                Path = $vm.Path
                Uptime = $vm.Uptime.TotalSeconds
                DynamicMemoryEnabled = $vm.DynamicMemoryEnabled
                MemoryStartup = $vm.MemoryStartup
                MemoryMinimum = $vm.MemoryMinimum
                MemoryMaximum = $vm.MemoryMaximum
                MemoryAssigned = $vm.MemoryAssigned
                MemoryDemand = $vm.MemoryDemand
                ProcessorCount = $vm.ProcessorCount
                Notes = $vm.Notes
                AutomaticStartAction = $vm.AutomaticStartAction.ToString()
                AutomaticStopAction = $vm.AutomaticStopAction.ToString()
                CheckpointType = $vm.CheckpointType.ToString()
                Disks = $disks
                NetworkAdapters = $nics
                Snapshots = $snapshots
            }}
        ", ct);

        var obj = results.First();
        return new VmHardwareInfo(
            Convert.ToInt32(obj.Properties["Generation"].Value),
            obj.Properties["Version"].Value?.ToString() ?? "",
            obj.Properties["Path"].Value?.ToString() ?? "",
            TimeSpan.FromSeconds(Convert.ToDouble(obj.Properties["Uptime"].Value)),
            Convert.ToBoolean(obj.Properties["DynamicMemoryEnabled"].Value),
            Convert.ToInt64(obj.Properties["MemoryStartup"].Value),
            Convert.ToInt64(obj.Properties["MemoryMinimum"].Value),
            Convert.ToInt64(obj.Properties["MemoryMaximum"].Value),
            Convert.ToInt64(obj.Properties["MemoryAssigned"].Value),
            Convert.ToInt64(obj.Properties["MemoryDemand"].Value),
            Convert.ToInt32(obj.Properties["ProcessorCount"].Value),
            obj.Properties["Notes"].Value?.ToString(),
            obj.Properties["AutomaticStartAction"].Value?.ToString() ?? "",
            obj.Properties["AutomaticStopAction"].Value?.ToString() ?? "",
            obj.Properties["CheckpointType"].Value?.ToString() ?? "",
            ParseDisks(obj.Properties["Disks"].Value),
            ParseNetworkAdapters(obj.Properties["NetworkAdapters"].Value),
            ParseSnapshots(obj.Properties["Snapshots"].Value));
    }

    private static IReadOnlyList<VmDiskInfo> ParseDisks(object? value)
    {
        if (value is not System.Collections.IEnumerable items)
            return Array.Empty<VmDiskInfo>();

        return items.Cast<PSObject>().Select(d => new VmDiskInfo(
            d.Properties["ControllerType"].Value?.ToString() ?? "",
            Convert.ToInt32(d.Properties["ControllerNumber"].Value),
            Convert.ToInt32(d.Properties["ControllerLocation"].Value),
            d.Properties["Path"].Value?.ToString() ?? "",
            d.Properties["VhdFormat"].Value?.ToString() ?? "Unknown",
            d.Properties["VhdType"].Value?.ToString() ?? "Unknown",
            Convert.ToInt64(d.Properties["CurrentSize"].Value),
            Convert.ToInt64(d.Properties["MaxSize"].Value)
        )).ToList();
    }

    private static IReadOnlyList<VmNetworkAdapterInfo> ParseNetworkAdapters(object? value)
    {
        if (value is not System.Collections.IEnumerable items)
            return Array.Empty<VmNetworkAdapterInfo>();

        return items.Cast<PSObject>().Select(n => new VmNetworkAdapterInfo(
            n.Properties["Name"].Value?.ToString() ?? "",
            n.Properties["SwitchName"].Value?.ToString() ?? "",
            n.Properties["MacAddress"].Value?.ToString() ?? "",
            ParseStringArray(n.Properties["IpAddresses"].Value)
        )).ToList();
    }

    private static IReadOnlyList<VmSnapshotInfo> ParseSnapshots(object? value)
    {
        if (value is not System.Collections.IEnumerable items)
            return Array.Empty<VmSnapshotInfo>();

        return items.Cast<PSObject>().Select(s => new VmSnapshotInfo(
            Guid.Parse(s.Properties["Id"].Value?.ToString() ?? Guid.Empty.ToString()),
            s.Properties["Name"].Value?.ToString() ?? "",
            DateTime.Parse(s.Properties["CreationTime"].Value?.ToString() ?? DateTime.MinValue.ToString("o")),
            s.Properties["ParentSnapshotName"].Value?.ToString()
        )).ToList();
    }

    private static IReadOnlyList<string> ParseStringArray(object? value)
    {
        if (value is not System.Collections.IEnumerable items)
            return Array.Empty<string>();

        return items.Cast<object>().Select(o => o.ToString() ?? "").ToList();
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
