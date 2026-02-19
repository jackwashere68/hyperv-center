using HyperVCenter.Application.Common.Interfaces;
using MediatR;

namespace HyperVCenter.Application.Features.HyperVHosts.Commands;

public record DeleteHyperVHostCommand(Guid Id) : IRequest<bool>;

public class DeleteHyperVHostHandler : IRequestHandler<DeleteHyperVHostCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteHyperVHostHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteHyperVHostCommand request, CancellationToken cancellationToken)
    {
        var host = await _context.HyperVHosts.FindAsync(
            new object[] { request.Id }, cancellationToken);

        if (host is null) return false;

        _context.HyperVHosts.Remove(host);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
