using DotNetEnv;

namespace PlayerPerformance.Api.Configuration;

internal static class ConfigurationExtensions
{
    public static void AddLocalDotEnvIfPresent(this ConfigurationManager configuration, string contentRootPath, IHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
        {
            return;
        }
        var backendRoot = FindBackendRoot(contentRootPath);
        if (backendRoot is null)
        {
            return;
        }

        var dotEnvPath = Path.Combine(backendRoot, ".env");
        if (!File.Exists(dotEnvPath))
        {
            return;
        }

        Env.NoClobber().Load(dotEnvPath);
        configuration.AddEnvironmentVariables();
    }

    private static string? FindBackendRoot(string startPath)
    {
        var directory = new DirectoryInfo(startPath);

        while (directory is not null)
        {
            var solutionPath = Path.Combine(directory.FullName, "PlayerPerformance.sln");
            if (File.Exists(solutionPath))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
