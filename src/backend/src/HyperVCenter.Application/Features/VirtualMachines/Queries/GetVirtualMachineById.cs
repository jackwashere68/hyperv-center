using HyperVCenter.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HyperVCenter.Application.Features.VirtualMachines.Queries;

// Query
public record GetVirtualMachineByIdQuery(Guid Id) : IRequest<VirtualMachineDto?>;

// Handler
public class GetVirtualMachineByIdHandler : IRequestHandler<GetVirtualMachineByIdQuery, VirtualMachineDto?>
{
    private readonly IApplicationDbContext _context;

    public GetVirtualMachineByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VirtualMachineDto?> Handle(
        GetVirtualMachineByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.VirtualMachines
            .AsNoTracking()
            .Where(vm => vm.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}
