using HyperVCenter.Domain.Common;
using HyperVCenter.Domain.Enums;

namespace HyperVCenter.Domain.Entities;

public class VirtualMachine : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid HyperVHostId { get; set; }
    public HyperVHost HyperVHost { get; set; } = null!;
    public Guid? ExternalId { get; set; }
    public VmState State { get; set; } = VmState.Off;
    public int CpuCount { get; set; } = 1;
    public long MemoryBytes { get; set; } = 1073741824; // 1 GB
    public string? Notes { get; set; }
}
