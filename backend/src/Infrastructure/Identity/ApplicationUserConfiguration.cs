using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlayerPerformance.Infrastructure.Identity;

internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("staff_users");

        builder.Property(user => user.AccountStatus)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(user => user.RequiresPasswordChange)
            .HasDefaultValue(true);

        builder.Property(user => user.CreatedUtc)
            .HasColumnName("created_utc");

        builder.Property(user => user.UpdatedUtc)
            .HasColumnName("updated_utc");

        builder.Property(user => user.Email)
            .HasMaxLength(256);

        builder.Property(user => user.NormalizedEmail)
            .HasMaxLength(256);

        builder.Property(user => user.UserName)
            .HasMaxLength(256);

        builder.Property(user => user.NormalizedUserName)
            .HasMaxLength(256);

        builder.HasIndex(user => user.NormalizedEmail)
            .IsUnique()
            .HasDatabaseName("EmailIndex");
    }
}
