using HyperVCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HyperVCenter.Infrastructure.Persistence.Configurations;

public class VirtualMachineConfiguration : IEntityTypeConfiguration<VirtualMachine>
{
    public void Configure(EntityTypeBuilder<VirtualMachine> builder)
    {
        builder.ToTable("virtual_machines");

        builder.HasKey(vm => vm.Id);

        builder.Property(vm => vm.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(vm => vm.Host)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(vm => vm.State)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(vm => vm.Notes)
            .HasMaxLength(2000);

        builder.Property(vm => vm.CreatedAt)
            .IsRequired();
    }
}
