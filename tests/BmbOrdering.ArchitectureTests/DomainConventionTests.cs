using BmbOrdering.Domain.Customers;

namespace BmbOrdering.ArchitectureTests;

public sealed class DomainConventionTests
{
    [Fact]
    public void DomainEntities_AreSealedAndHaveNoPublicConstructors()
    {
        var entityTypes = GetDomainEntityTypes();

        Assert.NotEmpty(entityTypes);

        foreach (var entityType in entityTypes)
        {
            Assert.True(
                entityType.IsSealed,
                $"Domain entity {entityType.FullName} must be sealed.");
            Assert.Empty(entityType.GetConstructors());
        }
    }

    [Fact]
    public void DomainEntityProperties_DoNotHavePublicSetters()
    {
        foreach (var entityType in GetDomainEntityTypes())
        {
            var publiclySettableProperties = entityType
                .GetProperties()
                .Where(property =>
                    property.SetMethod?.IsPublic == true)
                .Select(property => property.Name)
                .ToArray();

            Assert.True(
                publiclySettableProperties.Length == 0,
                $"Domain entity {entityType.FullName} exposes public setters: " +
                string.Join(", ", publiclySettableProperties));
        }
    }

    private static Type[] GetDomainEntityTypes()
    {
        return typeof(Customer).Assembly
            .GetTypes()
            .Where(type =>
                type.IsClass &&
                type.IsPublic &&
                (type.Namespace == "BmbOrdering.Domain.Customers" ||
                 type.Namespace == "BmbOrdering.Domain.Orders"))
            .ToArray();
    }
}
