using FluentValidation;
using HyperVCenter.Application.Common.Interfaces;
using HyperVCenter.Domain.Entities;
using HyperVCenter.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HyperVCenter.Application.Features.Clusters.Commands;

// Command
public record CreateClusterCommand(
    string Name,
    Guid CredentialId,
    string[] NodeHostnames,
    string? Notes) : IRequest<ClusterDto>;

// Handler
public class CreateClusterHandler : IRequestHandler<CreateClusterCommand, ClusterDto>
{
    private readonly IApplicationDbContext _context;

    public CreateClusterHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClusterDto> Handle(
        CreateClusterCommand request,
        CancellationToken cancellationToken)
    {
        var credential = await _context.Credentials.FindAsync(
            new object[] { request.CredentialId }, cancellationToken);

        var cluster = new Cluster
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CredentialId = request.CredentialId,
            Status = ClusterStatus.Unknown,
            Notes = request.Notes,
        };

        foreach (var hostname in request.NodeHostnames)
        {
            var node = new HyperVHost
            {
                Id = Guid.NewGuid(),
                Name = hostname,
                Hostname = hostname,
                CredentialId = request.CredentialId,
                Status = HostStatus.Unknown,
                ClusterId = cluster.Id,
            };
            cluster.Nodes.Add(node);
        }

        _context.Clusters.Add(cluster);
        await _context.SaveChangesAsync(cancellationToken);

        return new ClusterDto(
            cluster.Id, cluster.Name,
            cluster.CredentialId, credential!.Name,
            cluster.Status, cluster.Notes,
            cluster.Nodes.Count,
            cluster.CreatedAt, cluster.UpdatedAt);
    }
}

// Validator
public class CreateClusterValidator : AbstractValidator<CreateClusterCommand>
{
    public CreateClusterValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(256).WithMessage("Name must not exceed 256 characters.");

        RuleFor(x => x.CredentialId)
            .NotEmpty().WithMessage("Credential is required.")
            .MustAsync(async (id, ct) => await context.Credentials.AnyAsync(c => c.Id == id, ct))
            .WithMessage("The specified credential does not exist.");

        RuleFor(x => x.NodeHostnames)
            .NotEmpty().WithMessage("At least one node hostname is required.");

        RuleForEach(x => x.NodeHostnames)
            .NotEmpty().WithMessage("Node hostname must not be empty.")
            .MaximumLength(256).WithMessage("Node hostname must not exceed 256 characters.");
    }
}
