using HyperVCenter.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HyperVCenter.Application.Features.HyperVHosts.Queries;

public record GetHyperVHostsQuery : IRequest<IReadOnlyList<HyperVHostDto>>;

public class GetHyperVHostsHandler : IRequestHandler<GetHyperVHostsQuery, IReadOnlyList<HyperVHostDto>>
{
    private readonly IApplicationDbContext _context;

    public GetHyperVHostsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<HyperVHostDto>> Handle(
        GetHyperVHostsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.HyperVHosts
            .AsNoTracking()
            .Include(h => h.Credential)
            .Include(h => h.Cluster)
            .OrderBy(h => h.Name)
            .Select(h => new HyperVHostDto(
                h.Id, h.Name, h.Hostname,
                h.CredentialId, h.Credential.Name,
                h.Status, h.Notes,
                h.ClusterId, h.Cluster != null ? h.Cluster.Name : null,
                h.OsVersion, h.ProcessorCount, h.TotalMemoryBytes,
                h.LastSyncedAt, h.LastSyncError,
                h.CreatedAt, h.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
