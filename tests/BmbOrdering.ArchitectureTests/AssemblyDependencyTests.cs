using System.Reflection;
using BmbOrdering.Api.Controllers;
using BmbOrdering.Application.Authentication.Register;
using BmbOrdering.Domain.Customers;
using BmbOrdering.Infrastructure.Persistence;

namespace BmbOrdering.ArchitectureTests;

public sealed class AssemblyDependencyTests
{
    private static readonly Assembly DomainAssembly =
        typeof(Customer).Assembly;

    private static readonly Assembly ApplicationAssembly =
        typeof(RegisterCustomerHandler).Assembly;

    private static readonly Assembly InfrastructureAssembly =
        typeof(OrderingDbContext).Assembly;

    [Fact]
    public void Domain_DoesNotReferenceOuterLayers()
    {
        AssertDoesNotReference(
            DomainAssembly,
            "BmbOrdering.Application",
            "BmbOrdering.Infrastructure",
            "BmbOrdering.Api");
    }

    [Fact]
    public void Application_DoesNotReferenceInfrastructureOrApi()
    {
        AssertDoesNotReference(
            ApplicationAssembly,
            "BmbOrdering.Infrastructure",
            "BmbOrdering.Api");
    }

    [Fact]
    public void Infrastructure_DoesNotReferenceApi()
    {
        AssertDoesNotReference(
            InfrastructureAssembly,
            typeof(AuthenticationController).Assembly.GetName().Name!);
    }

    private static void AssertDoesNotReference(
        Assembly assembly,
        params string[] forbiddenAssemblyNames)
    {
        var references = assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name is not null)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var forbiddenAssemblyName in forbiddenAssemblyNames)
        {
            Assert.False(
                references.Contains(forbiddenAssemblyName),
                $"{assembly.GetName().Name} must not reference " +
                $"{forbiddenAssemblyName}.");
        }
    }
}
