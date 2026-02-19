using HyperVCenter.Domain.Common;
using HyperVCenter.Domain.Enums;

namespace HyperVCenter.Domain.Entities;

public class HyperVHost : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public Guid CredentialId { get; set; }
    public Credential Credential { get; set; } = null!;
    public HostStatus Status { get; set; } = HostStatus.Unknown;
    public string? Notes { get; set; }
    public Guid? ClusterId { get; set; }
    public Cluster? Cluster { get; set; }
}
