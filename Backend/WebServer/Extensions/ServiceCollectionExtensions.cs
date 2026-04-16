using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;
using WebServer.Configuration;
using WebServer.Domain.Models;
using WebServer.Domain.Repositories;
using WebServer.Features.Auth;
using WebServer.Infrastructure.Data;

namespace WebServer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWalForceApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddProblemDetails();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            var bearerScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "opaque",
                In = ParameterLocation.Header,
                Description = "Paste only the access token returned by /api/auth/login. Do not include the 'Bearer ' prefix."
            };

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "WalForce Backend API",
                Version = "v1",
                Description = "Prototype backend API for WalForce employee and manager views."
            });

            options.AddSecurityDefinition("Bearer", bearerScheme);

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", document, null),
                    []
                }
            });
        });

        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));
        services.Configure<CorsOptions>(configuration.GetSection(CorsOptions.SectionName));
        services.Configure<DataOptions>(configuration.GetSection(DataOptions.SectionName));

        var authOptions = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();
        var corsOptions = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();

        services.AddCors(options =>
        {
            options.AddPolicy(CorsOptions.PolicyName, policy =>
            {
                if (corsOptions.AllowedOrigins.Length > 0)
                {
                    policy
                        .WithOrigins(corsOptions.AllowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
            });
        });

        services
            .AddAuthentication(SimpleBearerDefaults.AuthenticationScheme)
            .AddScheme<AuthenticationSchemeOptions, SimpleBearerAuthenticationHandler>(
                SimpleBearerDefaults.AuthenticationScheme,
                _ => { });

        services.AddAuthorization();
        services.AddSingleton<IPasswordHasher<UserRecord>, PasswordHasher<UserRecord>>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<JsonFileDataStore>();
        services.AddSingleton<IUserRepository, JsonUserRepository>();
        services.AddSingleton<IScheduleRepository, JsonScheduleRepository>();
        services.AddSingleton<IAvailabilityRepository, JsonAvailabilityRepository>();

        return services;
    }
}
