using HyperVCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HyperVCenter.Infrastructure.Persistence.Configurations;

public class ClusterConfiguration : IEntityTypeConfiguration<Cluster>
{
    public void Configure(EntityTypeBuilder<Cluster> builder)
    {
        builder.ToTable("clusters");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(256).IsRequired();
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(c => c.Notes).HasMaxLength(2000);
        builder.Property(c => c.CreatedAt).IsRequired();

        builder.HasOne(c => c.Credential)
            .WithMany()
            .HasForeignKey(c => c.CredentialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Nodes)
            .WithOne(h => h.Cluster)
            .HasForeignKey(h => h.ClusterId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
