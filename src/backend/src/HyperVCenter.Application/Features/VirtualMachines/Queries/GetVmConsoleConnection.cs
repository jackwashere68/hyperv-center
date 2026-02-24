using HyperVCenter.Application.Common.Interfaces;
using HyperVCenter.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HyperVCenter.Application.Features.VirtualMachines.Queries;

public record VmConsoleConnectionDto(
    string Hostname,
    int Port,
    string Username,
    string Password,
    string VmExternalId,
    string VmName);

public record GetVmConsoleConnectionQuery(Guid VmId) : IRequest<VmConsoleConnectionDto>;

public class GetVmConsoleConnectionHandler(
    IApplicationDbContext context,
    IEncryptionService encryption)
    : IRequestHandler<GetVmConsoleConnectionQuery, VmConsoleConnectionDto>
{
    public async Task<VmConsoleConnectionDto> Handle(
        GetVmConsoleConnectionQuery request, CancellationToken ct)
    {
        var vm = await context.VirtualMachines
            .Include(v => v.HyperVHost)
                .ThenInclude(h => h.Credential)
            .FirstOrDefaultAsync(v => v.Id == request.VmId, ct)
            ?? throw new KeyNotFoundException($"VM {request.VmId} not found.");

        if (vm.ExternalId is null)
            throw new InvalidOperationException(
                $"VM '{vm.Name}' has no Hyper-V external ID. It may not have been synced yet.");

        if (vm.HyperVHost.Status != HostStatus.Online)
            throw new InvalidOperationException(
                $"Host '{vm.HyperVHost.Name}' is not online (status: {vm.HyperVHost.Status}).");

        var password = encryption.Decrypt(vm.HyperVHost.Credential.EncryptedPassword);

        return new VmConsoleConnectionDto(
            Hostname: vm.HyperVHost.Hostname,
            Port: 2179,
            Username: vm.HyperVHost.Credential.Username,
            Password: password,
            VmExternalId: vm.ExternalId.Value.ToString(),
            VmName: vm.Name);
    }
}
