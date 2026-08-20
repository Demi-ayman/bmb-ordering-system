using System.Xml.Linq;

namespace BmbOrdering.ArchitectureTests;

public sealed class ProjectDependencyTests
{
    [Theory]
    [MemberData(nameof(ProjectDependencies))]
    public void ProjectReferences_FollowCleanArchitectureDirection(
        string projectName,
        string[] expectedDependencies)
    {
        var projectPath = Path.Combine(
            FindRepositoryRoot(),
            "src",
            projectName,
            $"{projectName}.csproj");

        var document = XDocument.Load(projectPath);
        var actualDependencies = document
            .Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => Path.GetFileNameWithoutExtension(path!))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        var expected = expectedDependencies
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expected, actualDependencies);
    }

    public static IEnumerable<object[]> ProjectDependencies()
    {
        yield return new object[]
        {
            "BmbOrdering.Domain",
            Array.Empty<string>()
        };
        yield return new object[]
        {
            "BmbOrdering.Application",
            new[] { "BmbOrdering.Domain" }
        };
        yield return new object[]
        {
            "BmbOrdering.Infrastructure",
            new[]
            {
                "BmbOrdering.Application",
                "BmbOrdering.Domain"
            }
        };
        yield return new object[]
        {
            "BmbOrdering.Api",
            new[]
            {
                "BmbOrdering.Application",
                "BmbOrdering.Infrastructure"
            }
        };
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(
            AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(
                    Path.Combine(directory.FullName, "BmbOrdering.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            "Could not locate the repository root.");
    }
}
