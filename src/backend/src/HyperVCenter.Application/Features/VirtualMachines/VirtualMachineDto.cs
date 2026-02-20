using HyperVCenter.Domain.Enums;

namespace HyperVCenter.Application.Features.VirtualMachines;

public record VirtualMachineDto(
    Guid Id,
    string Name,
    Guid HyperVHostId,
    string HostName,
    Guid? ExternalId,
    VmState State,
    int CpuCount,
    long MemoryBytes,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
