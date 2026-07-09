namespace PlayerPerformance.UnitTests.Architecture;

public sealed class ProjectDependencyTests
{
    [Fact]
    public void DomainProject_ShouldNotReferenceApplicationInfrastructureOrApi_WhenInspectingAssemblyReferences()
    {
        var references = GetReferencedAssemblyNames(Assembly.Load("PlayerPerformance.Domain"));

        Assert.DoesNotContain("PlayerPerformance.Application", references);
        Assert.DoesNotContain("PlayerPerformance.Infrastructure", references);
        Assert.DoesNotContain("PlayerPerformance.Api", references);
    }

    [Fact]
    public void ApplicationProject_ShouldNotReferenceInfrastructureOrApi_WhenInspectingAssemblyReferences()
    {
        var references = GetReferencedAssemblyNames(Assembly.Load("PlayerPerformance.Application"));

        Assert.DoesNotContain("PlayerPerformance.Infrastructure", references);
        Assert.DoesNotContain("PlayerPerformance.Api", references);
    }

    [Fact]
    public void InfrastructureProject_ShouldNotReferenceApi_WhenInspectingAssemblyReferences()
    {
        var references = GetReferencedAssemblyNames(Assembly.Load("PlayerPerformance.Infrastructure"));

        Assert.DoesNotContain("PlayerPerformance.Api", references);
    }

    private static IReadOnlyCollection<string> GetReferencedAssemblyNames(Assembly assembly)
    {
        return assembly
            .GetReferencedAssemblies()
            .Select(static reference => reference.Name)
            .OfType<string>()
            .ToArray();
    }
}
