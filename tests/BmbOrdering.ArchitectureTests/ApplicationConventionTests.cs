using BmbOrdering.Application.Authentication.Register;

namespace BmbOrdering.ArchitectureTests;

public sealed class ApplicationConventionTests
{
    [Fact]
    public void ApplicationHandlers_AreSealedAndFollowNamingConvention()
    {
        var applicationAssembly =
            typeof(RegisterCustomerHandler).Assembly;

        var handlerTypes = applicationAssembly
            .GetTypes()
            .Where(type =>
                type.IsClass &&
                !type.IsAbstract &&
                type.Name.EndsWith(
                    "Handler",
                    StringComparison.Ordinal))
            .ToArray();

        Assert.NotEmpty(handlerTypes);

        foreach (var handlerType in handlerTypes)
        {
            Assert.True(
                handlerType.IsSealed,
                $"Application handler {handlerType.FullName} must be sealed.");
            Assert.StartsWith(
                "BmbOrdering.Application.",
                handlerType.Namespace!);
        }
    }

    [Fact]
    public void ApplicationAbstractions_AreInterfacesPrefixedWithI()
    {
        var applicationAssembly =
            typeof(RegisterCustomerHandler).Assembly;

        var abstractionInterfaces = applicationAssembly
            .GetTypes()
            .Where(type =>
                type.IsInterface &&
                type.Namespace?.StartsWith(
                    "BmbOrdering.Application.Abstractions",
                    StringComparison.Ordinal) == true)
            .ToArray();

        Assert.NotEmpty(abstractionInterfaces);

        foreach (var abstraction in abstractionInterfaces)
        {
            Assert.StartsWith("I", abstraction.Name);
        }
    }
}
