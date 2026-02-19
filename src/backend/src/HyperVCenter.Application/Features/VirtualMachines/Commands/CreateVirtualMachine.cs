using FluentValidation;
using HyperVCenter.Application.Common.Interfaces;
using HyperVCenter.Domain.Entities;
using HyperVCenter.Domain.Enums;
using MediatR;

namespace HyperVCenter.Application.Features.VirtualMachines.Commands;

// Command
public record CreateVirtualMachineCommand(
    string Name,
    string Host,
    int CpuCount,
    long MemoryBytes,
    string? Notes) : IRequest<VirtualMachineDto>;

// Handler
public class CreateVirtualMachineHandler : IRequestHandler<CreateVirtualMachineCommand, VirtualMachineDto>
{
    private readonly IApplicationDbContext _context;

    public CreateVirtualMachineHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VirtualMachineDto> Handle(
        CreateVirtualMachineCommand request,
        CancellationToken cancellationToken)
    {
        var vm = new VirtualMachine
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Host = request.Host,
            State = VmState.PoweredOff,
            CpuCount = request.CpuCount,
            MemoryBytes = request.MemoryBytes,
            Notes = request.Notes,
        };

        _context.VirtualMachines.Add(vm);
        await _context.SaveChangesAsync(cancellationToken);

        return new VirtualMachineDto(
            vm.Id,
            vm.Name,
            vm.Host,
            vm.State,
            vm.CpuCount,
            vm.MemoryBytes,
            vm.Notes,
            vm.CreatedAt,
            vm.UpdatedAt);
    }
}

// Validator
public class CreateVirtualMachineValidator : AbstractValidator<CreateVirtualMachineCommand>
{
    public CreateVirtualMachineValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(256).WithMessage("Name must not exceed 256 characters.");

        RuleFor(x => x.Host)
            .NotEmpty().WithMessage("Host is required.")
            .MaximumLength(256).WithMessage("Host must not exceed 256 characters.");

        RuleFor(x => x.CpuCount)
            .GreaterThan(0).WithMessage("CPU count must be at least 1.");

        RuleFor(x => x.MemoryBytes)
            .GreaterThan(0).WithMessage("Memory must be greater than 0.");
    }
}
