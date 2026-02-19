using HyperVCenter.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HyperVCenter.Application.Features.Credentials.Queries;

// Query
public record GetCredentialByIdQuery(Guid Id) : IRequest<CredentialDto?>;

// Handler
public class GetCredentialByIdHandler : IRequestHandler<GetCredentialByIdQuery, CredentialDto?>
{
    private readonly IApplicationDbContext _context;

    public GetCredentialByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CredentialDto?> Handle(
        GetCredentialByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Credentials
            .AsNoTracking()
            .Where(c => c.Id == request.Id)
            .Select(c => new CredentialDto(
                c.Id,
                c.Name,
                c.Username,
                c.CreatedAt,
                c.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
