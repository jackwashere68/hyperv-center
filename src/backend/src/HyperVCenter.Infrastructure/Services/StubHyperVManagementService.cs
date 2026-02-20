using HyperVCenter.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace HyperVCenter.Infrastructure.Services;

public class StubHyperVManagementService : IHyperVManagementService
{
    private readonly ILogger<StubHyperVManagementService> _logger;

    public StubHyperVManagementService(ILogger<StubHyperVManagementService> logger)
    {
        _logger = logger;
    }

    public Task<HostInfo> GetHostInfoAsync(string hostname, string username, string password, CancellationToken ct)
    {
        _logger.LogInformation("[Stub] GetHostInfo for {Hostname}", hostname);
        return Task.FromResult(new HostInfo(
            "Microsoft Windows Server 2022 Datacenter",
            8,
            34359738368)); // 32 GB
    }

    public Task<IReadOnlyList<VmInfo>> GetVirtualMachinesAsync(string hostname, string username, string password, CancellationToken ct)
    {
        _logger.LogInformation("[Stub] GetVirtualMachines for {Hostname}", hostname);
        IReadOnlyList<VmInfo> vms = new List<VmInfo>
        {
            new(Guid.NewGuid(), "DC01", "Running", 2, 4294967296),      // 4 GB
            new(Guid.NewGuid(), "SQL01", "Off", 4, 8589934592),          // 8 GB
            new(Guid.NewGuid(), "WEB01", "Running", 2, 2147483648),      // 2 GB
        };
        return Task.FromResult(vms);
    }

    public Task StartVmAsync(string hostname, string username, string password, Guid vmId, CancellationToken ct)
    {
        _logger.LogInformation("[Stub] StartVM {VmId} on {Hostname}", vmId, hostname);
        return Task.CompletedTask;
    }

    public Task StopVmAsync(string hostname, string username, string password, Guid vmId, bool force, CancellationToken ct)
    {
        _logger.LogInformation("[Stub] StopVM {VmId} on {Hostname} (force={Force})", vmId, hostname, force);
        return Task.CompletedTask;
    }

    public Task PauseVmAsync(string hostname, string username, string password, Guid vmId, CancellationToken ct)
    {
        _logger.LogInformation("[Stub] PauseVM {VmId} on {Hostname}", vmId, hostname);
        return Task.CompletedTask;
    }

    public Task SaveVmAsync(string hostname, string username, string password, Guid vmId, CancellationToken ct)
    {
        _logger.LogInformation("[Stub] SaveVM {VmId} on {Hostname}", vmId, hostname);
        return Task.CompletedTask;
    }

    public Task RestartVmAsync(string hostname, string username, string password, Guid vmId, CancellationToken ct)
    {
        _logger.LogInformation("[Stub] RestartVM {VmId} on {Hostname}", vmId, hostname);
        return Task.CompletedTask;
    }

    public Task<Guid> CreateVmAsync(string hostname, string username, string password, string name, int cpuCount, long memoryBytes, CancellationToken ct)
    {
        _logger.LogInformation("[Stub] CreateVM {Name} on {Hostname}", name, hostname);
        return Task.FromResult(Guid.NewGuid());
    }

    public Task<VmHardwareInfo> GetVmHardwareAsync(string hostname, string username, string password, Guid vmId, CancellationToken ct)
    {
        _logger.LogInformation("[Stub] GetVmHardware {VmId} on {Hostname}", vmId, hostname);
        return Task.FromResult(new VmHardwareInfo(
            Generation: 2,
            Version: "9.0",
            Path: @"C:\Hyper-V\Virtual Machines",
            Uptime: TimeSpan.FromHours(48.5),
            DynamicMemoryEnabled: true,
            MemoryStartup: 4294967296,     // 4 GB
            MemoryMinimum: 536870912,      // 512 MB
            MemoryMaximum: 8589934592,     // 8 GB
            MemoryAssigned: 4294967296,    // 4 GB
            MemoryDemand: 3221225472,      // 3 GB
            ProcessorCount: 4,
            Notes: "Domain controller for lab environment",
            AutomaticStartAction: "StartIfPreviouslyRunning",
            AutomaticStopAction: "Save",
            CheckpointType: "Production",
            Disks: new List<VmDiskInfo>
            {
                new("SCSI", 0, 0, @"C:\Hyper-V\DC01\DC01.vhdx", "VHDX", "Dynamic", 21474836480, 107374182400),
                new("SCSI", 0, 1, @"C:\Hyper-V\DC01\DC01-Data.vhdx", "VHDX", "Dynamic", 5368709120, 53687091200),
            },
            NetworkAdapters: new List<VmNetworkAdapterInfo>
            {
                new("Network Adapter", "Default Switch", "00155D010203", new List<string> { "192.168.1.10", "fe80::1" }),
            },
            Snapshots: new List<VmSnapshotInfo>
            {
                new(Guid.NewGuid(), "Before Updates", DateTime.UtcNow.AddDays(-7), null),
                new(Guid.NewGuid(), "After Updates", DateTime.UtcNow.AddDays(-3), "Before Updates"),
            }));
    }
}
