using FluentValidation;
using HyperVCenter.Application.Common.Interfaces;
using HyperVCenter.Application.Common.Mappings;
using HyperVCenter.Domain.Entities;
using HyperVCenter.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HyperVCenter.Application.Features.HyperVHosts.Commands;

// Command
public record CreateHyperVHostCommand(
    string Name,
    string Hostname,
    Guid CredentialId,
    string? Notes) : IRequest<HyperVHostDto>;

// Handler
public class CreateHyperVHostHandler : IRequestHandler<CreateHyperVHostCommand, HyperVHostDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IEncryptionService _encryption;
    private readonly IHyperVManagementService _hyperV;

    public CreateHyperVHostHandler(
        IApplicationDbContext context,
        IEncryptionService encryption,
        IHyperVManagementService hyperV)
    {
        _context = context;
        _encryption = encryption;
        _hyperV = hyperV;
    }

    public async Task<HyperVHostDto> Handle(
        CreateHyperVHostCommand request,
        CancellationToken cancellationToken)
    {
        var credential = await _context.Credentials.FindAsync(
            new object[] { request.CredentialId }, cancellationToken);

        var host = new HyperVHost
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Hostname = request.Hostname,
            CredentialId = request.CredentialId,
            Status = HostStatus.Unknown,
            Notes = request.Notes,
        };

        _context.HyperVHosts.Add(host);
        await _context.SaveChangesAsync(cancellationToken);

        // Attempt connectivity validation and VM discovery
        var password = _encryption.Decrypt(credential!.EncryptedPassword);
        try
        {
            var hostInfo = await _hyperV.GetHostInfoAsync(
                host.Hostname, credential.Username, password, cancellationToken);

            host.OsVersion = hostInfo.OsVersion;
            host.ProcessorCount = hostInfo.ProcessorCount;
            host.TotalMemoryBytes = hostInfo.TotalMemoryBytes;
            host.Status = HostStatus.Online;

            var vms = await _hyperV.GetVirtualMachinesAsync(
                host.Hostname, credential.Username, password, cancellationToken);

            foreach (var vmInfo in vms)
            {
                var vm = new VirtualMachine
                {
                    Id = Guid.NewGuid(),
                    Name = vmInfo.Name,
                    HyperVHostId = host.Id,
                    ExternalId = vmInfo.Id,
                    State = VmStateMapper.MapFromHyperV(vmInfo.State),
                    CpuCount = vmInfo.CpuCount,
                    MemoryBytes = vmInfo.MemoryBytes,
                };
                _context.VirtualMachines.Add(vm);
            }

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
            host.CredentialId, credential.Name,
            host.Status, host.Notes,
            null, null,
            host.OsVersion, host.ProcessorCount, host.TotalMemoryBytes,
            host.LastSyncedAt, host.LastSyncError,
            host.CreatedAt, host.UpdatedAt);
    }
}

// Validator
public class CreateHyperVHostValidator : AbstractValidator<CreateHyperVHostCommand>
{
    public CreateHyperVHostValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(256).WithMessage("Name must not exceed 256 characters.");

        RuleFor(x => x.Hostname)
            .NotEmpty().WithMessage("Hostname is required.")
            .MaximumLength(256).WithMessage("Hostname must not exceed 256 characters.");

        RuleFor(x => x.CredentialId)
            .NotEmpty().WithMessage("Credential is required.")
            .MustAsync(async (id, ct) => await context.Credentials.AnyAsync(c => c.Id == id, ct))
            .WithMessage("The specified credential does not exist.");
    }
}
