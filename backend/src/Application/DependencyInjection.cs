using Microsoft.Extensions.DependencyInjection;

namespace PlayerPerformance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
