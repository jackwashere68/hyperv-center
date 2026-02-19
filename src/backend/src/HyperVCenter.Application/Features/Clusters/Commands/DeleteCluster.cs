using HyperVCenter.Application.Common.Interfaces;
using MediatR;

namespace HyperVCenter.Application.Features.Clusters.Commands;

public record DeleteClusterCommand(Guid Id) : IRequest<bool>;

public class DeleteClusterHandler : IRequestHandler<DeleteClusterCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteClusterHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteClusterCommand request, CancellationToken cancellationToken)
    {
        var cluster = await _context.Clusters.FindAsync(
            new object[] { request.Id }, cancellationToken);

        if (cluster is null) return false;

        _context.Clusters.Remove(cluster);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
