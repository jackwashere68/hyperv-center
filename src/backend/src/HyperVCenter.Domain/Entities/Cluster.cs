using HyperVCenter.Domain.Common;
using HyperVCenter.Domain.Enums;

namespace HyperVCenter.Domain.Entities;

public class Cluster : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid CredentialId { get; set; }
    public Credential Credential { get; set; } = null!;
    public ClusterStatus Status { get; set; } = ClusterStatus.Unknown;
    public string? Notes { get; set; }
    public ICollection<HyperVHost> Nodes { get; set; } = new List<HyperVHost>();
}
