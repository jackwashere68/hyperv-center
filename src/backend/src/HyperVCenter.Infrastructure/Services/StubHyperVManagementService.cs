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
}
