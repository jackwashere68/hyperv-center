using HyperVCenter.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HyperVCenter.Application.Features.Clusters.Queries;

public record GetClustersQuery : IRequest<IReadOnlyList<ClusterDto>>;

public class GetClustersHandler : IRequestHandler<GetClustersQuery, IReadOnlyList<ClusterDto>>
{
    private readonly IApplicationDbContext _context;

    public GetClustersHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ClusterDto>> Handle(
        GetClustersQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Clusters
            .AsNoTracking()
            .Include(c => c.Credential)
            .Include(c => c.Nodes)
            .OrderBy(c => c.Name)
            .Select(c => new ClusterDto(
                c.Id, c.Name,
                c.CredentialId, c.Credential.Name,
                c.Status, c.Notes,
                c.Nodes.Count,
                c.CreatedAt, c.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
