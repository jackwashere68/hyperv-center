using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text.Json;
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
            $vmName = $vm.Name

            $disks = @(Get-VMHardDiskDrive -VMName $vmName | ForEach-Object {{
                $vhd = $null
                try {{ $vhd = Get-VHD -Path $_.Path -ErrorAction SilentlyContinue }} catch {{}}
                @{{
                    ControllerType = $_.ControllerType.ToString()
                    ControllerNumber = $_.ControllerNumber
                    ControllerLocation = $_.ControllerLocation
                    Path = $_.Path
                    VhdFormat = $(if ($vhd) {{ $vhd.VhdFormat.ToString() }} else {{ 'Unknown' }})
                    VhdType = $(if ($vhd) {{ $vhd.VhdType.ToString() }} else {{ 'Unknown' }})
                    CurrentSize = $(if ($vhd) {{ $vhd.FileSize }} else {{ 0 }})
                    MaxSize = $(if ($vhd) {{ $vhd.Size }} else {{ 0 }})
                }}
            }})

            $nics = @(Get-VMNetworkAdapter -VMName $vmName | ForEach-Object {{
                @{{
                    Name = $_.Name
                    SwitchName = $(if ($_.SwitchName) {{ $_.SwitchName }} else {{ '' }})
                    MacAddress = $_.MacAddress
                    IpAddresses = @($_.IPAddresses)
                }}
            }})

            $snapshots = @(Get-VMSnapshot -VMName $vmName -ErrorAction SilentlyContinue | ForEach-Object {{
                @{{
                    Id = $_.Id.ToString()
                    Name = $_.Name
                    CreationTime = $_.CreationTime.ToString('o')
                    ParentSnapshotName = $_.ParentSnapshotName
                }}
            }})

            @{{
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
            }} | ConvertTo-Json -Depth 5 -Compress
        ", ct);

        var json = results.First().BaseObject.ToString()!;
        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var data = JsonSerializer.Deserialize<HardwareJsonModel>(json, opts)!;

        return new VmHardwareInfo(
            data.Generation,
            data.Version ?? "",
            data.Path ?? "",
            TimeSpan.FromSeconds(data.Uptime),
            data.DynamicMemoryEnabled,
            data.MemoryStartup,
            data.MemoryMinimum,
            data.MemoryMaximum,
            data.MemoryAssigned,
            data.MemoryDemand,
            data.ProcessorCount,
            data.Notes,
            data.AutomaticStartAction ?? "",
            data.AutomaticStopAction ?? "",
            data.CheckpointType ?? "",
            (data.Disks ?? []).Select(d => new VmDiskInfo(
                d.ControllerType ?? "", d.ControllerNumber, d.ControllerLocation,
                d.Path ?? "", d.VhdFormat ?? "Unknown", d.VhdType ?? "Unknown",
                d.CurrentSize, d.MaxSize)).ToList(),
            (data.NetworkAdapters ?? []).Select(n => new VmNetworkAdapterInfo(
                n.Name ?? "", n.SwitchName ?? "", n.MacAddress ?? "",
                n.IpAddresses ?? [])).ToList(),
            (data.Snapshots ?? []).Select(s => new VmSnapshotInfo(
                Guid.TryParse(s.Id, out var id) ? id : Guid.Empty,
                s.Name ?? "",
                DateTime.TryParse(s.CreationTime, out var dt) ? dt : DateTime.MinValue,
                s.ParentSnapshotName)).ToList());
    }

    private record HardwareJsonModel
    {
        public int Generation { get; init; }
        public string? Version { get; init; }
        public string? Path { get; init; }
        public double Uptime { get; init; }
        public bool DynamicMemoryEnabled { get; init; }
        public long MemoryStartup { get; init; }
        public long MemoryMinimum { get; init; }
        public long MemoryMaximum { get; init; }
        public long MemoryAssigned { get; init; }
        public long MemoryDemand { get; init; }
        public int ProcessorCount { get; init; }
        public string? Notes { get; init; }
        public string? AutomaticStartAction { get; init; }
        public string? AutomaticStopAction { get; init; }
        public string? CheckpointType { get; init; }
        public List<DiskJsonModel>? Disks { get; init; }
        public List<NicJsonModel>? NetworkAdapters { get; init; }
        public List<SnapshotJsonModel>? Snapshots { get; init; }
    }

    private record DiskJsonModel
    {
        public string? ControllerType { get; init; }
        public int ControllerNumber { get; init; }
        public int ControllerLocation { get; init; }
        public string? Path { get; init; }
        public string? VhdFormat { get; init; }
        public string? VhdType { get; init; }
        public long CurrentSize { get; init; }
        public long MaxSize { get; init; }
    }

    private record NicJsonModel
    {
        public string? Name { get; init; }
        public string? SwitchName { get; init; }
        public string? MacAddress { get; init; }
        public List<string>? IpAddresses { get; init; }
    }

    private record SnapshotJsonModel
    {
        public string? Id { get; init; }
        public string? Name { get; init; }
        public string? CreationTime { get; init; }
        public string? ParentSnapshotName { get; init; }
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
