using System.Reflection;
using BmbOrdering.Api.Controllers;
using BmbOrdering.Application.Common.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BmbOrdering.ArchitectureTests;

public sealed class ApiConventionTests
{
    [Fact]
    public void Controllers_AreSealedAndHaveApiControllerAttribute()
    {
        var controllerTypes = GetControllerTypes();

        Assert.NotEmpty(controllerTypes);

        foreach (var controllerType in controllerTypes)
        {
            Assert.True(
                controllerType.IsSealed,
                $"Controller {controllerType.FullName} must be sealed.");
            Assert.EndsWith("Controller", controllerType.Name);
            Assert.NotNull(
                controllerType.GetCustomAttribute<ApiControllerAttribute>());
        }
    }

    [Fact]
    public void Controllers_ExplicitlyDeclareAuthorizationPolicy()
    {
        foreach (var controllerType in GetControllerTypes())
        {
            var hasAuthorize = controllerType
                .IsDefined(typeof(AuthorizeAttribute), inherit: true);
            var allowsAnonymous = controllerType
                .IsDefined(typeof(AllowAnonymousAttribute), inherit: true);

            Assert.True(
                hasAuthorize ^ allowsAnonymous,
                $"Controller {controllerType.FullName} must declare exactly " +
                "one of Authorize or AllowAnonymous.");
        }
    }

    [Fact]
    public void AdminCustomersController_RequiresAdministratorRole()
    {
        var authorize = typeof(AdminCustomersController)
            .GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorize);
        Assert.Equal(RoleNames.Administrator, authorize!.Roles);
    }

    private static Type[] GetControllerTypes()
    {
        return typeof(AuthenticationController).Assembly
            .GetTypes()
            .Where(type =>
                type.IsClass &&
                !type.IsAbstract &&
                typeof(ControllerBase).IsAssignableFrom(type))
            .ToArray();
    }
}
