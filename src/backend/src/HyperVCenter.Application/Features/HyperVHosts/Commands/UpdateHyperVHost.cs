using FluentValidation;
using HyperVCenter.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HyperVCenter.Application.Features.HyperVHosts.Commands;

// Command
public record UpdateHyperVHostCommand(
    Guid Id,
    string Name,
    string Hostname,
    Guid CredentialId,
    string? Notes) : IRequest<HyperVHostDto?>;

// Handler
public class UpdateHyperVHostHandler : IRequestHandler<UpdateHyperVHostCommand, HyperVHostDto?>
{
    private readonly IApplicationDbContext _context;

    public UpdateHyperVHostHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HyperVHostDto?> Handle(
        UpdateHyperVHostCommand request,
        CancellationToken cancellationToken)
    {
        var host = await _context.HyperVHosts
            .Include(h => h.Credential)
            .Include(h => h.Cluster)
            .FirstOrDefaultAsync(h => h.Id == request.Id, cancellationToken);

        if (host is null) return null;

        host.Name = request.Name;
        host.Hostname = request.Hostname;
        host.CredentialId = request.CredentialId;
        host.Notes = request.Notes;

        await _context.SaveChangesAsync(cancellationToken);

        var credential = await _context.Credentials.FindAsync(
            new object[] { host.CredentialId }, cancellationToken);

        return new HyperVHostDto(
            host.Id, host.Name, host.Hostname,
            host.CredentialId, credential!.Name,
            host.Status, host.Notes,
            host.ClusterId, host.Cluster?.Name,
            host.OsVersion, host.ProcessorCount, host.TotalMemoryBytes,
            host.LastSyncedAt, host.LastSyncError,
            host.CreatedAt, host.UpdatedAt);
    }
}

// Validator
public class UpdateHyperVHostValidator : AbstractValidator<UpdateHyperVHostCommand>
{
    public UpdateHyperVHostValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

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
