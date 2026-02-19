using HyperVCenter.Application.Common.Interfaces;
using MediatR;

namespace HyperVCenter.Application.Features.Credentials.Commands;

// Command
public record DeleteCredentialCommand(Guid Id) : IRequest<bool>;

// Handler
public class DeleteCredentialHandler : IRequestHandler<DeleteCredentialCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteCredentialHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(
        DeleteCredentialCommand request,
        CancellationToken cancellationToken)
    {
        var credential = await _context.Credentials.FindAsync(
            new object[] { request.Id }, cancellationToken);

        if (credential is null)
            return false;

        _context.Credentials.Remove(credential);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
