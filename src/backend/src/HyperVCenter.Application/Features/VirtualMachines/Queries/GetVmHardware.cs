using HyperVCenter.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HyperVCenter.Application.Features.VirtualMachines.Queries;

public record GetVmHardwareQuery(Guid Id) : IRequest<VmHardwareDto>;

public class GetVmHardwareHandler : IRequestHandler<GetVmHardwareQuery, VmHardwareDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IEncryptionService _encryption;
    private readonly IHyperVManagementService _hyperV;

    public GetVmHardwareHandler(
        IApplicationDbContext context,
        IEncryptionService encryption,
        IHyperVManagementService hyperV)
    {
        _context = context;
        _encryption = encryption;
        _hyperV = hyperV;
    }

    public async Task<VmHardwareDto> Handle(GetVmHardwareQuery request, CancellationToken ct)
    {
        var vm = await _context.VirtualMachines
            .Include(v => v.HyperVHost)
                .ThenInclude(h => h.Credential)
            .FirstOrDefaultAsync(v => v.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"VM {request.Id} not found.");

        if (vm.ExternalId is null)
            throw new InvalidOperationException($"VM {request.Id} has no external ID.");

        var password = _encryption.Decrypt(vm.HyperVHost.Credential.EncryptedPassword);

        var hw = await _hyperV.GetVmHardwareAsync(
            vm.HyperVHost.Hostname,
            vm.HyperVHost.Credential.Username,
            password,
            vm.ExternalId.Value,
            ct);

        return new VmHardwareDto(
            hw.Generation,
            hw.Version,
            hw.Path,
            hw.Uptime,
            hw.DynamicMemoryEnabled,
            hw.MemoryStartup,
            hw.MemoryMinimum,
            hw.MemoryMaximum,
            hw.MemoryAssigned,
            hw.MemoryDemand,
            hw.ProcessorCount,
            hw.Notes,
            hw.AutomaticStartAction,
            hw.AutomaticStopAction,
            hw.CheckpointType,
            hw.Disks.Select(d => new VmDiskDto(
                d.ControllerType, d.ControllerNumber, d.ControllerLocation,
                d.Path, d.VhdFormat, d.VhdType, d.CurrentSize, d.MaxSize)).ToList(),
            hw.NetworkAdapters.Select(n => new VmNetworkAdapterDto(
                n.Name, n.SwitchName, n.MacAddress, n.IpAddresses.ToList())).ToList(),
            hw.Snapshots.Select(s => new VmSnapshotDto(
                s.Id, s.Name, s.CreationTime, s.ParentSnapshotName)).ToList());
    }
}
