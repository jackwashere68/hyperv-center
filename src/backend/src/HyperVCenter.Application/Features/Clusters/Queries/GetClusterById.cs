using HyperVCenter.Application.Common.Interfaces;
using HyperVCenter.Application.Features.HyperVHosts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HyperVCenter.Application.Features.Clusters.Queries;

public record GetClusterByIdQuery(Guid Id) : IRequest<ClusterDetailDto?>;

public class GetClusterByIdHandler : IRequestHandler<GetClusterByIdQuery, ClusterDetailDto?>
{
    private readonly IApplicationDbContext _context;

    public GetClusterByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClusterDetailDto?> Handle(
        GetClusterByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Clusters
            .AsNoTracking()
            .Include(c => c.Credential)
            .Include(c => c.Nodes)
                .ThenInclude(n => n.Credential)
            .Where(c => c.Id == request.Id)
            .Select(c => new ClusterDetailDto(
                c.Id, c.Name,
                c.CredentialId, c.Credential.Name,
                c.Status, c.Notes,
                c.Nodes.Select(n => new HyperVHostDto(
                    n.Id, n.Name, n.Hostname,
                    n.CredentialId, n.Credential.Name,
                    n.Status, n.Notes,
                    n.ClusterId, c.Name,
                    n.CreatedAt, n.UpdatedAt)).ToList(),
                c.CreatedAt, c.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
