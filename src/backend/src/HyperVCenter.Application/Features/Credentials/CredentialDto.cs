namespace HyperVCenter.Application.Features.Credentials;

public record CredentialDto(
    Guid Id,
    string Name,
    string Username,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
