using FluentValidation;
using HyperVCenter.Application.Common.Interfaces;
using HyperVCenter.Domain.Entities;
using MediatR;

namespace HyperVCenter.Application.Features.Credentials.Commands;

// Command
public record CreateCredentialCommand(
    string Name,
    string Username,
    string Password) : IRequest<CredentialDto>;

// Handler
public class CreateCredentialHandler : IRequestHandler<CreateCredentialCommand, CredentialDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IEncryptionService _encryption;

    public CreateCredentialHandler(IApplicationDbContext context, IEncryptionService encryption)
    {
        _context = context;
        _encryption = encryption;
    }

    public async Task<CredentialDto> Handle(
        CreateCredentialCommand request,
        CancellationToken cancellationToken)
    {
        var credential = new Credential
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Username = request.Username,
            EncryptedPassword = _encryption.Encrypt(request.Password),
        };

        _context.Credentials.Add(credential);
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
public class CreateCredentialValidator : AbstractValidator<CreateCredentialCommand>
{
    public CreateCredentialValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(256).WithMessage("Name must not exceed 256 characters.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(256).WithMessage("Username must not exceed 256 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
