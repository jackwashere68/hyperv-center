namespace HyperVCenter.Application.Common.Interfaces;

public record HostInfo(string OsVersion, int ProcessorCount, long TotalMemoryBytes);
public record VmInfo(Guid Id, string Name, string State, int CpuCount, long MemoryBytes);

public interface IHyperVManagementService
{
    Task<HostInfo> GetHostInfoAsync(string hostname, string username, string password, CancellationToken ct);
    Task<IReadOnlyList<VmInfo>> GetVirtualMachinesAsync(string hostname, string username, string password, CancellationToken ct);
    Task StartVmAsync(string hostname, string username, string password, Guid vmId, CancellationToken ct);
    Task StopVmAsync(string hostname, string username, string password, Guid vmId, bool force, CancellationToken ct);
    Task PauseVmAsync(string hostname, string username, string password, Guid vmId, CancellationToken ct);
    Task SaveVmAsync(string hostname, string username, string password, Guid vmId, CancellationToken ct);
    Task RestartVmAsync(string hostname, string username, string password, Guid vmId, CancellationToken ct);
    Task<Guid> CreateVmAsync(string hostname, string username, string password, string name, int cpuCount, long memoryBytes, CancellationToken ct);
}
