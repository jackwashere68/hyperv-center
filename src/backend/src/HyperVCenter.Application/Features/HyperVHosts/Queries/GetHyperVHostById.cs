using HyperVCenter.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HyperVCenter.Application.Features.HyperVHosts.Queries;

public record GetHyperVHostByIdQuery(Guid Id) : IRequest<HyperVHostDto?>;

public class GetHyperVHostByIdHandler : IRequestHandler<GetHyperVHostByIdQuery, HyperVHostDto?>
{
    private readonly IApplicationDbContext _context;

    public GetHyperVHostByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HyperVHostDto?> Handle(
        GetHyperVHostByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.HyperVHosts
            .AsNoTracking()
            .Include(h => h.Credential)
            .Include(h => h.Cluster)
            .Where(h => h.Id == request.Id)
            .Select(h => new HyperVHostDto(
                h.Id, h.Name, h.Hostname,
                h.CredentialId, h.Credential.Name,
                h.Status, h.Notes,
                h.ClusterId, h.Cluster != null ? h.Cluster.Name : null,
                h.OsVersion, h.ProcessorCount, h.TotalMemoryBytes,
                h.LastSyncedAt, h.LastSyncError,
                h.CreatedAt, h.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
