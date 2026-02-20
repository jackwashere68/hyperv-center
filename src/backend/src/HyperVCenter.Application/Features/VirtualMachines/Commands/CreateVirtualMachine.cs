using FluentValidation;
using HyperVCenter.Application.Common.Interfaces;
using HyperVCenter.Domain.Entities;
using HyperVCenter.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HyperVCenter.Application.Features.VirtualMachines.Commands;

// Command
public record CreateVirtualMachineCommand(
    string Name,
    Guid HyperVHostId,
    int CpuCount,
    long MemoryBytes,
    string? Notes) : IRequest<VirtualMachineDto>;

// Handler
public class CreateVirtualMachineHandler : IRequestHandler<CreateVirtualMachineCommand, VirtualMachineDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IEncryptionService _encryption;
    private readonly IHyperVManagementService _hyperV;

    public CreateVirtualMachineHandler(
        IApplicationDbContext context,
        IEncryptionService encryption,
        IHyperVManagementService hyperV)
    {
        _context = context;
        _encryption = encryption;
        _hyperV = hyperV;
    }

    public async Task<VirtualMachineDto> Handle(
        CreateVirtualMachineCommand request,
        CancellationToken cancellationToken)
    {
        var host = await _context.HyperVHosts
            .Include(h => h.Credential)
            .FirstOrDefaultAsync(h => h.Id == request.HyperVHostId, cancellationToken)
            ?? throw new KeyNotFoundException($"Host {request.HyperVHostId} not found.");

        var password = _encryption.Decrypt(host.Credential.EncryptedPassword);

        var externalId = await _hyperV.CreateVmAsync(
            host.Hostname, host.Credential.Username, password,
            request.Name, request.CpuCount, request.MemoryBytes, cancellationToken);

        var vm = new VirtualMachine
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            HyperVHostId = request.HyperVHostId,
            ExternalId = externalId,
            State = VmState.Off,
            CpuCount = request.CpuCount,
            MemoryBytes = request.MemoryBytes,
            Notes = request.Notes,
        };

        _context.VirtualMachines.Add(vm);
        await _context.SaveChangesAsync(cancellationToken);

        return new VirtualMachineDto(
            vm.Id,
            vm.Name,
            vm.HyperVHostId,
            host.Name,
            vm.ExternalId,
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
    public CreateVirtualMachineValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(256).WithMessage("Name must not exceed 256 characters.");

        RuleFor(x => x.HyperVHostId)
            .NotEmpty().WithMessage("Host is required.")
            .MustAsync(async (id, ct) => await context.HyperVHosts.AnyAsync(h => h.Id == id, ct))
            .WithMessage("The specified host does not exist.");

        RuleFor(x => x.CpuCount)
            .GreaterThan(0).WithMessage("CPU count must be at least 1.");

        RuleFor(x => x.MemoryBytes)
            .GreaterThan(0).WithMessage("Memory must be greater than 0.");
    }
}
