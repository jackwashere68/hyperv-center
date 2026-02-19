using HyperVCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HyperVCenter.Infrastructure.Persistence.Configurations;

public class CredentialConfiguration : IEntityTypeConfiguration<Credential>
{
    public void Configure(EntityTypeBuilder<Credential> builder)
    {
        builder.ToTable("credentials");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(c => c.Username)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(c => c.EncryptedPassword)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();
    }
}
