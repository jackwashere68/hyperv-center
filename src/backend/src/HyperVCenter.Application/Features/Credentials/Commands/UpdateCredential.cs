using FluentValidation;
using HyperVCenter.Application.Common.Interfaces;
using MediatR;

namespace HyperVCenter.Application.Features.Credentials.Commands;

// Command
public record UpdateCredentialCommand(
    Guid Id,
    string Name,
    string Username,
    string? Password) : IRequest<CredentialDto?>;

// Handler
public class UpdateCredentialHandler : IRequestHandler<UpdateCredentialCommand, CredentialDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IEncryptionService _encryption;

    public UpdateCredentialHandler(IApplicationDbContext context, IEncryptionService encryption)
    {
        _context = context;
        _encryption = encryption;
    }

    public async Task<CredentialDto?> Handle(
        UpdateCredentialCommand request,
        CancellationToken cancellationToken)
    {
        var credential = await _context.Credentials.FindAsync(
            new object[] { request.Id }, cancellationToken);

        if (credential is null) return null;

        credential.Name = request.Name;
        credential.Username = request.Username;

        if (request.Password is not null)
        {
            credential.EncryptedPassword = _encryption.Encrypt(request.Password);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new CredentialDto(
            credential.Id,
            credential.Name,
            credential.Username,
            credential.CreatedAt,
            credential.UpdatedAt);
    }
}

// Validator
public class UpdateCredentialValidator : AbstractValidator<UpdateCredentialCommand>
{
    public UpdateCredentialValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(256).WithMessage("Name must not exceed 256 characters.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(256).WithMessage("Username must not exceed 256 characters.");
    }
}
