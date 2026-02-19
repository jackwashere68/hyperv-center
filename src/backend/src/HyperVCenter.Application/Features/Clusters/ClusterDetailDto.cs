using HyperVCenter.Application.Features.HyperVHosts;
using HyperVCenter.Domain.Enums;

namespace HyperVCenter.Application.Features.Clusters;

public record ClusterDetailDto(
    Guid Id,
    string Name,
    Guid CredentialId,
    string CredentialName,
    ClusterStatus Status,
    string? Notes,
    IReadOnlyList<HyperVHostDto> Nodes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
