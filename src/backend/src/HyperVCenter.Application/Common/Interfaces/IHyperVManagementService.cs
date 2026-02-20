namespace HyperVCenter.Application.Common.Interfaces;

public record HostInfo(string OsVersion, int ProcessorCount, long TotalMemoryBytes);
public record VmInfo(Guid Id, string Name, string State, int CpuCount, long MemoryBytes);

public record VmHardwareInfo(
    int Generation,
    string Version,
    string Path,
    TimeSpan Uptime,
    bool DynamicMemoryEnabled,
    long MemoryStartup,
    long MemoryMinimum,
    long MemoryMaximum,
    long MemoryAssigned,
    long MemoryDemand,
    int ProcessorCount,
    string? Notes,
    string AutomaticStartAction,
    string AutomaticStopAction,
    string CheckpointType,
    IReadOnlyList<VmDiskInfo> Disks,
    IReadOnlyList<VmNetworkAdapterInfo> NetworkAdapters,
    IReadOnlyList<VmSnapshotInfo> Snapshots);

public record VmDiskInfo(
    string ControllerType,
    int ControllerNumber,
    int ControllerLocation,
    string Path,
    string VhdFormat,
    string VhdType,
    long CurrentSize,
    long MaxSize);

public record VmNetworkAdapterInfo(
    string Name,
    string SwitchName,
    string MacAddress,
    IReadOnlyList<string> IpAddresses);

public record VmSnapshotInfo(
    Guid Id,
    string Name,
    DateTime CreationTime,
    string? ParentSnapshotName);

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
    Task<VmHardwareInfo> GetVmHardwareAsync(string hostname, string username, string password, Guid vmId, CancellationToken ct);
}
