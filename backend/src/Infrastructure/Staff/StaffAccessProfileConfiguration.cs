using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayerPerformance.Infrastructure.Identity;

namespace PlayerPerformance.Infrastructure.Staff;

internal sealed class StaffAccessProfileConfiguration : IEntityTypeConfiguration<StaffAccessProfile>
{
    public void Configure(EntityTypeBuilder<StaffAccessProfile> builder)
    {
        builder.ToTable("staff_access_profiles");
        builder.HasKey(profile => profile.UserId);

        builder.Property(profile => profile.PrimaryRole)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(profile => profile.CanVerifyReports).HasDefaultValue(false);
        builder.Property(profile => profile.CanImportData).HasDefaultValue(false);
        builder.Property(profile => profile.CanViewMedicalDetails).HasDefaultValue(false);
        builder.Property(profile => profile.CreatedUtc).HasColumnName("created_utc");
        builder.Property(profile => profile.UpdatedUtc).HasColumnName("updated_utc");

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<StaffAccessProfile>(profile => profile.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
