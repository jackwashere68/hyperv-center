using HyperVCenter.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HyperVCenter.Application.Features.VirtualMachines.Queries;

// Query
public record GetVirtualMachinesQuery : IRequest<IReadOnlyList<VirtualMachineDto>>;

// Handler
public class GetVirtualMachinesHandler : IRequestHandler<GetVirtualMachinesQuery, IReadOnlyList<VirtualMachineDto>>
{
    private readonly IApplicationDbContext _context;

    public GetVirtualMachinesHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<VirtualMachineDto>> Handle(
        GetVirtualMachinesQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.VirtualMachines
            .AsNoTracking()
            .OrderBy(vm => vm.Name)
            .Select(vm => new VirtualMachineDto(
                vm.Id,
                vm.Name,
                vm.Host,
                vm.State,
                vm.CpuCount,
                vm.MemoryBytes,
                vm.Notes,
                vm.CreatedAt,
                vm.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
