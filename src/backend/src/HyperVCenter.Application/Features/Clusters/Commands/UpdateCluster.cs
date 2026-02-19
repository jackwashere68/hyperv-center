using FluentValidation;
using HyperVCenter.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HyperVCenter.Application.Features.Clusters.Commands;

// Command
public record UpdateClusterCommand(
    Guid Id,
    string Name,
    Guid CredentialId,
    string? Notes) : IRequest<ClusterDto?>;

// Handler
public class UpdateClusterHandler : IRequestHandler<UpdateClusterCommand, ClusterDto?>
{
    private readonly IApplicationDbContext _context;

    public UpdateClusterHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClusterDto?> Handle(
        UpdateClusterCommand request,
        CancellationToken cancellationToken)
    {
        var cluster = await _context.Clusters
            .Include(c => c.Nodes)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (cluster is null) return null;

        cluster.Name = request.Name;
        cluster.CredentialId = request.CredentialId;
        cluster.Notes = request.Notes;

        await _context.SaveChangesAsync(cancellationToken);

        var credential = await _context.Credentials.FindAsync(
            new object[] { cluster.CredentialId }, cancellationToken);

        return new ClusterDto(
            cluster.Id, cluster.Name,
            cluster.CredentialId, credential!.Name,
            cluster.Status, cluster.Notes,
            cluster.Nodes.Count,
            cluster.CreatedAt, cluster.UpdatedAt);
    }
}

// Validator
public class UpdateClusterValidator : AbstractValidator<UpdateClusterCommand>
{
    public UpdateClusterValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(256).WithMessage("Name must not exceed 256 characters.");

        RuleFor(x => x.CredentialId)
            .NotEmpty().WithMessage("Credential is required.")
            .MustAsync(async (id, ct) => await context.Credentials.AnyAsync(c => c.Id == id, ct))
            .WithMessage("The specified credential does not exist.");
    }
}
