# IIS HTTPS Deployment

This runbook describes a framework-dependent Release deployment to IIS. Adapt hostnames, identities, certificates, and SQL credentials to the target environment.

## 1. Server prerequisites

1. Install the IIS Web Server role and IIS Management Console.
2. Install the ASP.NET Core Hosting Bundle matching .NET 6.
3. If the Hosting Bundle was installed before IIS, repair or reinstall it after IIS.
4. Restart the server, or restart the Windows Process Activation Service and World Wide Web Publishing Service.
5. Install a valid TLS certificate for the production hostname.
6. Provision the production SQL Server database and a least-privileged application login.

The Hosting Bundle installs the .NET runtime and ASP.NET Core Module required for IIS hosting. Because .NET 6 is out of support, isolate the server and plan an upgrade to a supported LTS version.

## 2. Verify and publish

From the repository root on the build machine:

```powershell
dotnet restore BmbOrdering.sln
dotnet build BmbOrdering.sln --configuration Release --no-restore
dotnet test BmbOrdering.sln --configuration Release --no-build
dotnet publish .\src\BmbOrdering.Api\BmbOrdering.Api.csproj `
  --configuration Release `
  --no-build `
  --output .\artifacts\publish
```

The publish output includes the static client and a generated `web.config` for the ASP.NET Core Module. Do not delete it.

## 3. Prepare the database

Generate a reviewable idempotent SQL migration script:

```powershell
New-Item -ItemType Directory -Force .\artifacts\sql | Out-Null

dotnet tool restore

dotnet tool run dotnet-ef migrations script `
  --idempotent `
  --project .\src\BmbOrdering.Infrastructure\BmbOrdering.Infrastructure.csproj `
  --startup-project .\src\BmbOrdering.Api\BmbOrdering.Api.csproj `
  --output .\artifacts\sql\deploy.sql
```

Have an authorized database operator review and execute `deploy.sql` against the production database. Do not run development LocalDB commands against production.

## 4. Create the IIS application pool

In IIS Manager:

1. Open **Application Pools** and choose **Add Application Pool**.
2. Name it `BmbOrderingAppPool`.
3. Set **.NET CLR version** to **No Managed Code**.
4. Use **Integrated** pipeline mode.
5. Keep **Enable 32-Bit Applications** disabled for an x64 deployment.
6. Use a dedicated service identity where organizational policy requires it.

Grant the application-pool identity read and execute permission on the deployment directory. Grant write permission only to explicit directories that truly require it; this application does not write uploaded content to disk.

## 5. Deploy files

1. Create a directory such as `C:\inetpub\BmbOrdering`.
2. Stop the site or application pool.
3. Copy the contents of `artifacts\publish` into that directory.
4. Confirm `BmbOrdering.Api.dll`, `web.config`, `appsettings.json`, and `wwwroot` are present.
5. Reapply the intended directory ACLs.

Use a versioned release folder or backup before replacing an existing deployment so rollback remains possible.

## 6. Configure production settings

Set the keys documented in [Configuration and secrets](configuration.md) for `BmbOrderingAppPool`:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__OrderingDatabase=<production connection string>
Jwt__Issuer=BmbOrdering.Api
Jwt__Audience=BmbOrdering.Web
Jwt__SigningKey=<random secret containing at least 32 bytes>
Jwt__ExpirationMinutes=30
Authorization__AdministratorEmails__0=<approved administrator email>
```

Prefer application-pool-scoped environment variables or the organization's secret-management mechanism. Do not commit production values or copy them into this document.

The SQL identity requires only the permissions needed to read and modify the application tables. Schema migration can use a separate deployment identity.

## 7. Create the HTTPS-only site

In IIS Manager:

1. Choose **Sites > Add Website**.
2. Set the physical path to `C:\inetpub\BmbOrdering`.
3. Select `BmbOrderingAppPool`.
4. Add an `https` binding on port `443`.
5. Enter the production hostname.
6. Select the valid TLS certificate.
7. Do not add an HTTP binding. If one already exists, remove it.

Not exposing an HTTP binding satisfies the HTTPS-only requirement and avoids clients sending credentials before a redirect. If the organization requires a redirect-only HTTP endpoint, configure it separately at IIS and ensure the application itself is never served over HTTP.

For IIS versions that support native HSTS, enable it only after certificate and HTTPS operation are verified. Start with a short duration before increasing it.

## 8. Start and verify

Start the application pool and site, then verify:

1. `https://<hostname>/` loads the web client.
2. Registration and login work.
3. A protected endpoint returns `401` without a token.
4. Customer order creation, retrieval, and deletion work.
5. Administrator customer and order views work.
6. The SQL database contains the expected records.
7. `http://<hostname>/` is unavailable or handled only by the approved redirect endpoint.
8. The certificate chain is trusted and the hostname matches.
9. No secrets or stack traces appear in responses or logs.

Swagger is disabled in Production by design.

## 9. Troubleshooting

### HTTP 500.30 or application fails to start

- Confirm the matching .NET 6 Hosting Bundle is installed.
- Recycle the application pool after installing the bundle.
- Confirm required environment variables exist.
- Check Windows Event Viewer and IIS logs.
- Temporarily enable ASP.NET Core Module stdout logging only for diagnosis, grant write access to the log folder, then disable it afterward.

### Database connection failure

- Validate the production connection string outside the application.
- Confirm SQL Server network access and firewall rules.
- Confirm the IIS application identity or SQL login has the required permissions.
- Confirm the migration script was applied.

### Redirect loop or incorrect scheme

- Confirm the site has a valid HTTPS binding.
- Confirm no conflicting redirect exists at another proxy layer.
- Confirm ASP.NET Core Module/IIS integration is active.

### 401 or 403 responses

- `401`: verify token signature, issuer, audience, and expiry.
- `403`: verify the token contains the required role and the login email is listed for administrator access when applicable.

## 10. Rollback

1. Stop the site or application pool.
2. Restore the previous versioned publish directory.
3. Restore database state only through an approved rollback script or backup plan.
4. Start the application pool.
5. Repeat the smoke tests.

Never delete or reverse a production migration without reviewing its data-loss impact.

## Official references

- [Publish an ASP.NET Core app to IIS](https://learn.microsoft.com/aspnet/core/tutorials/publish-to-iis/)
- [Host ASP.NET Core on Windows with IIS](https://learn.microsoft.com/aspnet/core/host-and-deploy/iis/)
- [ASP.NET Core Module for IIS](https://learn.microsoft.com/aspnet/core/host-and-deploy/aspnet-core-module)
- [Enforce HTTPS in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/enforcing-ssl)
