using HyperVCenter.Domain.Common;
using HyperVCenter.Domain.Enums;

namespace HyperVCenter.Domain.Entities;

public class VirtualMachine : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public VmState State { get; set; } = VmState.PoweredOff;
    public int CpuCount { get; set; } = 1;
    public long MemoryBytes { get; set; } = 1073741824; // 1 GB
    public string? Notes { get; set; }
}
