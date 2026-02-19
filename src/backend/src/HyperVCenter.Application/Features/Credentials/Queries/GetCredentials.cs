using HyperVCenter.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HyperVCenter.Application.Features.Credentials.Queries;

// Query
public record GetCredentialsQuery : IRequest<IReadOnlyList<CredentialDto>>;

// Handler
public class GetCredentialsHandler : IRequestHandler<GetCredentialsQuery, IReadOnlyList<CredentialDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCredentialsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CredentialDto>> Handle(
        GetCredentialsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Credentials
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CredentialDto(
                c.Id,
                c.Name,
                c.Username,
                c.CreatedAt,
                c.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
