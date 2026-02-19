using HyperVCenter.Domain.Enums;

namespace HyperVCenter.Application.Features.Clusters;

public record ClusterDto(
    Guid Id,
    string Name,
    Guid CredentialId,
    string CredentialName,
    ClusterStatus Status,
    string? Notes,
    int NodeCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
