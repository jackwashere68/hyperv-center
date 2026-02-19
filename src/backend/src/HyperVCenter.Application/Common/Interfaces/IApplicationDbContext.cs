using HyperVCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HyperVCenter.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<VirtualMachine> VirtualMachines { get; }
    DbSet<Credential> Credentials { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
