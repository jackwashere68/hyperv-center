using HyperVCenter.Domain.Common;

namespace HyperVCenter.Domain.Entities;

public class Credential : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string EncryptedPassword { get; set; } = string.Empty;
}
