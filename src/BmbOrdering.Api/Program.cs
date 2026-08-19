using System.Security.Claims;
using System.Text;
using BmbOrdering.Application.Authentication.Login;
using BmbOrdering.Application.Authentication.Register;
using BmbOrdering.Infrastructure;
using BmbOrdering.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using BmbOrdering.Api.Middleware;
using BmbOrdering.Api.Security;
using BmbOrdering.Application.Abstractions.Authentication;
using BmbOrdering.Application.Orders.Create;
using BmbOrdering.Application.Orders.GetById;
using BmbOrdering.Application.Orders.GetForCurrentCustomer;
using BmbOrdering.Application.Orders.Delete;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "BMB Ordering API",
            Version = "v1"
        });

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Enter: Bearer {your JWT token}",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
});

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<RegisterCustomerValidator>();
builder.Services.AddScoped<RegisterCustomerHandler>();
builder.Services.AddScoped<LoginCustomerValidator>();
builder.Services.AddScoped<LoginCustomerHandler>();
builder.Services.AddScoped<CreateOrderValidator>();
builder.Services.AddScoped<CreateOrderHandler>();
builder.Services.AddScoped<GetOrderByIdValidator>();
builder.Services.AddScoped<GetOrderByIdHandler>();
builder.Services.AddScoped<GetCustomerOrdersHandler>();
builder.Services.AddScoped<DeleteOrderValidator>();
builder.Services.AddScoped<DeleteOrderHandler>();

var jwtSection =
    builder.Configuration.GetSection(JwtOptions.SectionName);

builder.Services.Configure<JwtOptions>(jwtSection);

var jwtOptions =
    jwtSection.Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "JWT configuration is missing.");

if (string.IsNullOrWhiteSpace(jwtOptions.Issuer))
{
    throw new InvalidOperationException(
        "JWT issuer is not configured.");
}

if (string.IsNullOrWhiteSpace(jwtOptions.Audience))
{
    throw new InvalidOperationException(
        "JWT audience is not configured.");
}

if (Encoding.UTF8.GetByteCount(jwtOptions.SigningKey) < 32)
{
    throw new InvalidOperationException(
        "JWT signing key must contain at least 32 bytes.");
}

var signingKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(jwtOptions.SigningKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,

                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<
    ICurrentUserContext,
    CurrentUserContext>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();