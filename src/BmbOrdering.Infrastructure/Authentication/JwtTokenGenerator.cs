using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BmbOrdering.Application.Abstractions.Authentication;
using BmbOrdering.Application.Abstractions.Time;
using BmbOrdering.Domain.Customers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BmbOrdering.Infrastructure.Authentication;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private const int MinimumSigningKeyBytes = 32;

    private readonly JwtOptions _options;
    private readonly IClock _clock;

    public JwtTokenGenerator(
        IOptions<JwtOptions> options,
        IClock clock)
    {
        _options = options.Value;
        _clock = clock;
    }

    public AccessToken GenerateToken(
        Customer customer,
        IReadOnlyCollection<string> roles)
    {
        ValidateOptions();

        var issuedAtUtc = _clock.UtcNow;
        var expiresAtUtc =
            issuedAtUtc.AddMinutes(_options.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                customer.Id.ToString()),
            new(
                ClaimTypes.NameIdentifier,
                customer.Id.ToString()),
            new(
                ClaimTypes.Name,
                customer.FullName),
            new(
                ClaimTypes.Email,
                customer.Email),
            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

        claims.AddRange(
            roles
                .Distinct(StringComparer.Ordinal)
                .Select(role => new Claim(ClaimTypes.Role, role)));

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.SigningKey));

        var signingCredentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: issuedAtUtc,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        var tokenValue =
            new JwtSecurityTokenHandler().WriteToken(token);

        return new AccessToken(
            tokenValue,
            expiresAtUtc);
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.Issuer))
        {
            throw new InvalidOperationException(
                "JWT issuer is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.Audience))
        {
            throw new InvalidOperationException(
                "JWT audience is not configured.");
        }

        if (Encoding.UTF8.GetByteCount(_options.SigningKey) <
            MinimumSigningKeyBytes)
        {
            throw new InvalidOperationException(
                "JWT signing key must contain at least 32 bytes.");
        }

        if (_options.ExpirationMinutes <= 0)
        {
            throw new InvalidOperationException(
                "JWT expiration must be greater than zero minutes.");
        }
    }
}