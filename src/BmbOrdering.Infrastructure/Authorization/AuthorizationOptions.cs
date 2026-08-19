namespace BmbOrdering.Infrastructure.Authorization;

public sealed class AuthorizationOptions
{
    public const string SectionName = "Authorization";

    public string[] AdministratorEmails { get; set; } =
        Array.Empty<string>();
}
