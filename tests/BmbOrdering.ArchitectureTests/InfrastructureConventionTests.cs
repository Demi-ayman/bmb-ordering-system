using BmbOrdering.Application.Abstractions.Persistence;
using BmbOrdering.Infrastructure.Persistence.Repositories;

namespace BmbOrdering.ArchitectureTests;

public sealed class InfrastructureConventionTests
{
    [Fact]
    public void RepositoryImplementations_AreSealedAndImplementApplicationAbstractions()
    {
        var repositoryTypes = typeof(CustomerRepository).Assembly
            .GetTypes()
            .Where(type =>
                type.IsClass &&
                type.IsPublic &&
                !type.IsAbstract &&
                type.Namespace ==
                    "BmbOrdering.Infrastructure.Persistence.Repositories")
            .ToArray();

        Assert.NotEmpty(repositoryTypes);

        foreach (var repositoryType in repositoryTypes)
        {
            Assert.True(
                repositoryType.IsSealed,
                $"Repository {repositoryType.FullName} must be sealed.");

            var persistenceInterfaces = repositoryType
                .GetInterfaces()
                .Where(type =>
                    type.Namespace ==
                    typeof(ICustomerRepository).Namespace)
                .ToArray();

            Assert.NotEmpty(persistenceInterfaces);
        }
    }
}
