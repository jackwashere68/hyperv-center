using HyperVCenter.Domain.Enums;

namespace HyperVCenter.Application.Features.HyperVHosts;

public record HyperVHostDto(
    Guid Id,
    string Name,
    string Hostname,
    Guid CredentialId,
    string CredentialName,
    HostStatus Status,
    string? Notes,
    Guid? ClusterId,
    string? ClusterName,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
