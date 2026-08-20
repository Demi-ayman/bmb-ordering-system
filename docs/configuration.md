# Configuration and Secrets

## Configuration sources

ASP.NET Core loads configuration from `appsettings.json`, environment-specific files, User Secrets in Development, and environment variables. Later providers override earlier providers.

Hierarchical environment-variable keys use a double underscore (`__`). For example, `Jwt__SigningKey` maps to `Jwt:SigningKey`.

## Required settings

| Key | Secret | Description |
|---|---|---|
| `ConnectionStrings:OrderingDatabase` | Yes | SQL Server connection string |
| `Jwt:Issuer` | No | Expected token issuer |
| `Jwt:Audience` | No | Expected token audience |
| `Jwt:SigningKey` | Yes | HMAC key containing at least 32 bytes |
| `Jwt:ExpirationMinutes` | No | Positive access-token lifetime |
| `Authorization:AdministratorEmails` | Usually | Emails that receive the Administrator role at login |

The repository deliberately excludes the connection string and signing key from tracked JSON files.

## Development with User Secrets

Initialize only if the project loses its existing `UserSecretsId`:

```powershell
dotnet user-secrets init --project .\src\BmbOrdering.Api\BmbOrdering.Api.csproj
```

Set secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:OrderingDatabase" "Server=(localdb)\MSSQLLocalDB;Database=BmbOrderingDb;Trusted_Connection=True;TrustServerCertificate=True" --project .\src\BmbOrdering.Api\BmbOrdering.Api.csproj

dotnet user-secrets set "Jwt:SigningKey" "replace-with-a-random-secret-containing-at-least-32-bytes" --project .\src\BmbOrdering.Api\BmbOrdering.Api.csproj
```

List configured keys without copying the values into documentation or source control:

```powershell
dotnet user-secrets list --project .\src\BmbOrdering.Api\BmbOrdering.Api.csproj
```

## Production environment variables

Configure these on the IIS application pool or through the organization's approved secret-management system:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__OrderingDatabase=<production SQL Server connection string>
Jwt__Issuer=BmbOrdering.Api
Jwt__Audience=BmbOrdering.Web
Jwt__SigningKey=<cryptographically random secret of at least 32 bytes>
Jwt__ExpirationMinutes=30
Authorization__AdministratorEmails__0=<approved administrator email>
```

Additional administrators use increasing array indexes:

```text
Authorization__AdministratorEmails__1=second.admin@example.com
Authorization__AdministratorEmails__2=third.admin@example.com
```

Restart or recycle the application pool after configuration changes.

## Administrator assignment

Registration never accepts a role. A login receives:

- `Customer` for every registered account.
- `Customer` and `Administrator` when the stored email matches a configured administrator email case-insensitively.

Changing the administrator list affects newly generated tokens. Existing tokens retain their claims until expiry.

## Secret-handling rules

- Never commit production connection strings, JWT keys, passwords, or tokens.
- Do not place secrets in `appsettings.json` or publish profiles.
- Use a different signing key per environment.
- Generate a high-entropy key; do not reuse the sample text.
- Restrict access to IIS configuration and the deployment directory.
- Rotate a compromised signing key immediately; existing tokens signed with the old key will become invalid.
- Avoid printing `dotnet user-secrets list` output in screenshots or logs.

## References

- [ASP.NET Core configuration](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/)
- [Safe storage of app secrets in development](https://learn.microsoft.com/aspnet/core/security/app-secrets/)
