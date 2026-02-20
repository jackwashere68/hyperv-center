using HyperVCenter.Application.Common.Interfaces;
using HyperVCenter.Application.Common.Mappings;
using HyperVCenter.Domain.Entities;
using HyperVCenter.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HyperVCenter.Application.Features.HyperVHosts.Commands;

public record SyncHyperVHostCommand(Guid Id) : IRequest<HyperVHostDto>;

public class SyncHyperVHostHandler : IRequestHandler<SyncHyperVHostCommand, HyperVHostDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IEncryptionService _encryption;
    private readonly IHyperVManagementService _hyperV;

    public SyncHyperVHostHandler(
        IApplicationDbContext context,
        IEncryptionService encryption,
        IHyperVManagementService hyperV)
    {
        _context = context;
        _encryption = encryption;
        _hyperV = hyperV;
    }

    public async Task<HyperVHostDto> Handle(SyncHyperVHostCommand request, CancellationToken cancellationToken)
    {
        var host = await _context.HyperVHosts
            .Include(h => h.Credential)
            .Include(h => h.Cluster)
            .Include(h => h.VirtualMachines)
            .FirstOrDefaultAsync(h => h.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Host {request.Id} not found.");

        var password = _encryption.Decrypt(host.Credential.EncryptedPassword);

        try
        {
            var hostInfo = await _hyperV.GetHostInfoAsync(
                host.Hostname, host.Credential.Username, password, cancellationToken);

            host.OsVersion = hostInfo.OsVersion;
            host.ProcessorCount = hostInfo.ProcessorCount;
            host.TotalMemoryBytes = hostInfo.TotalMemoryBytes;
            host.Status = HostStatus.Online;

            var vms = await _hyperV.GetVirtualMachinesAsync(
                host.Hostname, host.Credential.Username, password, cancellationToken);

            var existingVms = host.VirtualMachines
                .Where(v => v.ExternalId.HasValue)
                .ToDictionary(v => v.ExternalId!.Value);
            var discoveredIds = new HashSet<Guid>();

            foreach (var vmInfo in vms)
            {
                discoveredIds.Add(vmInfo.Id);

                if (existingVms.TryGetValue(vmInfo.Id, out var existing))
                {
                    existing.Name = vmInfo.Name;
                    existing.State = VmStateMapper.MapFromHyperV(vmInfo.State);
                    existing.CpuCount = vmInfo.CpuCount;
                    existing.MemoryBytes = vmInfo.MemoryBytes;
                }
                else
                {
                    var newVm = new VirtualMachine
                    {
                        Id = Guid.NewGuid(),
                        Name = vmInfo.Name,
                        HyperVHostId = host.Id,
                        ExternalId = vmInfo.Id,
                        State = VmStateMapper.MapFromHyperV(vmInfo.State),
                        CpuCount = vmInfo.CpuCount,
                        MemoryBytes = vmInfo.MemoryBytes,
                    };
                    _context.VirtualMachines.Add(newVm);
                }
            }

            // Remove VMs no longer on the host
            var removedVms = host.VirtualMachines
                .Where(v => v.ExternalId.HasValue && !discoveredIds.Contains(v.ExternalId.Value))
                .ToList();
            foreach (var vm in removedVms)
                _context.VirtualMachines.Remove(vm);

            host.LastSyncedAt = DateTime.UtcNow;
            host.LastSyncError = null;
        }
        catch (Exception ex)
        {
            host.Status = HostStatus.Error;
            host.LastSyncError = ex.Message;
            host.LastSyncedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new HyperVHostDto(
            host.Id, host.Name, host.Hostname,
            host.CredentialId, host.Credential.Name,
            host.Status, host.Notes,
            host.ClusterId, host.Cluster?.Name,
            host.OsVersion, host.ProcessorCount, host.TotalMemoryBytes,
            host.LastSyncedAt, host.LastSyncError,
            host.CreatedAt, host.UpdatedAt);
    }
}
