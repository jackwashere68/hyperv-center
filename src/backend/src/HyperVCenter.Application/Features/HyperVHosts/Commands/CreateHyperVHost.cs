using FluentValidation;
using HyperVCenter.Application.Common.Interfaces;
using HyperVCenter.Domain.Entities;
using HyperVCenter.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HyperVCenter.Application.Features.HyperVHosts.Commands;

// Command
public record CreateHyperVHostCommand(
    string Name,
    string Hostname,
    Guid CredentialId,
    string? Notes) : IRequest<HyperVHostDto>;

// Handler
public class CreateHyperVHostHandler : IRequestHandler<CreateHyperVHostCommand, HyperVHostDto>
{
    private readonly IApplicationDbContext _context;

    public CreateHyperVHostHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HyperVHostDto> Handle(
        CreateHyperVHostCommand request,
        CancellationToken cancellationToken)
    {
        var credential = await _context.Credentials.FindAsync(
            new object[] { request.CredentialId }, cancellationToken);

        var host = new HyperVHost
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Hostname = request.Hostname,
            CredentialId = request.CredentialId,
            Status = HostStatus.Unknown,
            Notes = request.Notes,
        };

        _context.HyperVHosts.Add(host);
        await _context.SaveChangesAsync(cancellationToken);

        return new HyperVHostDto(
            host.Id, host.Name, host.Hostname,
            host.CredentialId, credential!.Name,
            host.Status, host.Notes,
            null, null,
            host.CreatedAt, host.UpdatedAt);
    }
}

// Validator
public class CreateHyperVHostValidator : AbstractValidator<CreateHyperVHostCommand>
{
    public CreateHyperVHostValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(256).WithMessage("Name must not exceed 256 characters.");

        RuleFor(x => x.Hostname)
            .NotEmpty().WithMessage("Hostname is required.")
            .MaximumLength(256).WithMessage("Hostname must not exceed 256 characters.");

        RuleFor(x => x.CredentialId)
            .NotEmpty().WithMessage("Credential is required.")
            .MustAsync(async (id, ct) => await context.Credentials.AnyAsync(c => c.Id == id, ct))
            .WithMessage("The specified credential does not exist.");
    }
}
