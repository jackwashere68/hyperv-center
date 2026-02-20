namespace HyperVCenter.Application.Features.VirtualMachines;

public record VmHardwareDto(
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
    IReadOnlyList<VmDiskDto> Disks,
    IReadOnlyList<VmNetworkAdapterDto> NetworkAdapters,
    IReadOnlyList<VmSnapshotDto> Snapshots);

public record VmDiskDto(
    string ControllerType,
    int ControllerNumber,
    int ControllerLocation,
    string Path,
    string VhdFormat,
    string VhdType,
    long CurrentSize,
    long MaxSize);

public record VmNetworkAdapterDto(
    string Name,
    string SwitchName,
    string MacAddress,
    IReadOnlyList<string> IpAddresses);

public record VmSnapshotDto(
    Guid Id,
    string Name,
    DateTime CreationTime,
    string? ParentSnapshotName);
