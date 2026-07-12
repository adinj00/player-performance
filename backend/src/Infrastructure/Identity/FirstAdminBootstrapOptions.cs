using System.ComponentModel.DataAnnotations;

namespace PlayerPerformance.Infrastructure.Identity;

public sealed class FirstAdminBootstrapOptions
{
    public const string SectionName = "Bootstrap:FirstAdmin";

    public bool Enabled { get; init; }

    public string? Name { get; init; } = "Administrator";

    public string? Email { get; init; }

    public string? TemporaryPassword { get; init; }

    public string? GetEmptyStoreValidationError()
    {
        if (!Enabled)
        {
            return "First admin bootstrap is required while the user store is empty. Set Bootstrap:FirstAdmin:Enabled=true and provide the required first-admin configuration.";
        }

        if (string.IsNullOrWhiteSpace(Email) || !new EmailAddressAttribute().IsValid(Email))
        {
            return "Bootstrap:FirstAdmin:Email must be a valid email address while the user store is empty.";
        }

        if (string.IsNullOrWhiteSpace(Name) || Name.Trim().Length > 120)
        {
            return "Bootstrap:FirstAdmin:Name is required and must be at most 120 characters while the user store is empty.";
        }

        if (string.IsNullOrWhiteSpace(TemporaryPassword))
        {
            return "Bootstrap:FirstAdmin:TemporaryPassword is required while the user store is empty.";
        }

        return null;
    }
}
