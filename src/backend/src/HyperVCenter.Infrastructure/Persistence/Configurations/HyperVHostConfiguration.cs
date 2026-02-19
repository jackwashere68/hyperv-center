using HyperVCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HyperVCenter.Infrastructure.Persistence.Configurations;

public class HyperVHostConfiguration : IEntityTypeConfiguration<HyperVHost>
{
    public void Configure(EntityTypeBuilder<HyperVHost> builder)
    {
        builder.ToTable("hyperv_hosts");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Name).HasMaxLength(256).IsRequired();
        builder.Property(h => h.Hostname).HasMaxLength(256).IsRequired();
        builder.Property(h => h.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(h => h.Notes).HasMaxLength(2000);
        builder.Property(h => h.CreatedAt).IsRequired();

        builder.HasOne(h => h.Credential)
            .WithMany()
            .HasForeignKey(h => h.CredentialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.Cluster)
            .WithMany(c => c.Nodes)
            .HasForeignKey(h => h.ClusterId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
