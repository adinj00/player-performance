namespace PlayerPerformance.Domain.Common.Guards;

public static class Guard
{
    public static T AgainstNull<T>(T? value, string parameterName)
        where T : class
    {
        return value ?? throw new ArgumentNullException(parameterName);
    }

    public static string AgainstNullOrWhiteSpace(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null, empty, or whitespace.", parameterName);
        }

        return value;
    }

    public static Guid AgainstDefault(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Value cannot be the default GUID.");
        }

        return value;
    }
}
