using HyperVCenter.Application.Common.Interfaces;
using HyperVCenter.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HyperVCenter.Application.Features.VirtualMachines.Commands;

public record StartVmCommand(Guid Id) : IRequest<VirtualMachineDto>;
public record StopVmCommand(Guid Id, bool Force = false) : IRequest<VirtualMachineDto>;
public record PauseVmCommand(Guid Id) : IRequest<VirtualMachineDto>;
public record SaveVmCommand(Guid Id) : IRequest<VirtualMachineDto>;
public record RestartVmCommand(Guid Id) : IRequest<VirtualMachineDto>;
public record DeleteVmCommand(Guid Id) : IRequest<bool>;

public class VmActionHandler :
    IRequestHandler<StartVmCommand, VirtualMachineDto>,
    IRequestHandler<StopVmCommand, VirtualMachineDto>,
    IRequestHandler<PauseVmCommand, VirtualMachineDto>,
    IRequestHandler<SaveVmCommand, VirtualMachineDto>,
    IRequestHandler<RestartVmCommand, VirtualMachineDto>,
    IRequestHandler<DeleteVmCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IEncryptionService _encryption;
    private readonly IHyperVManagementService _hyperV;

    public VmActionHandler(
        IApplicationDbContext context,
        IEncryptionService encryption,
        IHyperVManagementService hyperV)
    {
        _context = context;
        _encryption = encryption;
        _hyperV = hyperV;
    }

    public async Task<VirtualMachineDto> Handle(StartVmCommand request, CancellationToken ct)
    {
        var (vm, host, password) = await LoadVmWithCredentials(request.Id, ct);
        await _hyperV.StartVmAsync(host.Hostname, host.Credential.Username, password, vm.ExternalId!.Value, ct);
        vm.State = VmState.Running;
        await _context.SaveChangesAsync(ct);
        return ToDto(vm);
    }

    public async Task<VirtualMachineDto> Handle(StopVmCommand request, CancellationToken ct)
    {
        var (vm, host, password) = await LoadVmWithCredentials(request.Id, ct);
        await _hyperV.StopVmAsync(host.Hostname, host.Credential.Username, password, vm.ExternalId!.Value, request.Force, ct);
        vm.State = VmState.Off;
        await _context.SaveChangesAsync(ct);
        return ToDto(vm);
    }

    public async Task<VirtualMachineDto> Handle(PauseVmCommand request, CancellationToken ct)
    {
        var (vm, host, password) = await LoadVmWithCredentials(request.Id, ct);
        await _hyperV.PauseVmAsync(host.Hostname, host.Credential.Username, password, vm.ExternalId!.Value, ct);
        vm.State = VmState.Paused;
        await _context.SaveChangesAsync(ct);
        return ToDto(vm);
    }

    public async Task<VirtualMachineDto> Handle(SaveVmCommand request, CancellationToken ct)
    {
        var (vm, host, password) = await LoadVmWithCredentials(request.Id, ct);
        await _hyperV.SaveVmAsync(host.Hostname, host.Credential.Username, password, vm.ExternalId!.Value, ct);
        vm.State = VmState.Saved;
        await _context.SaveChangesAsync(ct);
        return ToDto(vm);
    }

    public async Task<VirtualMachineDto> Handle(RestartVmCommand request, CancellationToken ct)
    {
        var (vm, host, password) = await LoadVmWithCredentials(request.Id, ct);
        await _hyperV.RestartVmAsync(host.Hostname, host.Credential.Username, password, vm.ExternalId!.Value, ct);
        vm.State = VmState.Running;
        await _context.SaveChangesAsync(ct);
        return ToDto(vm);
    }

    public async Task<bool> Handle(DeleteVmCommand request, CancellationToken ct)
    {
        var vm = await _context.VirtualMachines.FindAsync(new object[] { request.Id }, ct);
        if (vm is null) return false;
        _context.VirtualMachines.Remove(vm);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    private async Task<(Domain.Entities.VirtualMachine vm, Domain.Entities.HyperVHost host, string password)>
        LoadVmWithCredentials(Guid vmId, CancellationToken ct)
    {
        var vm = await _context.VirtualMachines
            .Include(v => v.HyperVHost)
                .ThenInclude(h => h.Credential)
            .FirstOrDefaultAsync(v => v.Id == vmId, ct)
            ?? throw new KeyNotFoundException($"VM {vmId} not found.");

        var password = _encryption.Decrypt(vm.HyperVHost.Credential.EncryptedPassword);
        return (vm, vm.HyperVHost, password);
    }

    private static VirtualMachineDto ToDto(Domain.Entities.VirtualMachine vm) => new(
        vm.Id,
        vm.Name,
        vm.HyperVHostId,
        vm.HyperVHost.Name,
        vm.ExternalId,
        vm.State,
        vm.CpuCount,
        vm.MemoryBytes,
        vm.Notes,
        vm.CreatedAt,
        vm.UpdatedAt);
}
