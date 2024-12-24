using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PMS.Application.Abstractions;
using PMS.Infrastructure.Configurations;
using PMS.Application.Interfaces;
using PMS.Infrastructure.Services;
using PMS.Infrastructure.Authorizations;
using PMS.Application.Abstractions.Data;
using PMS.Infrastructure.Database.Data;
using PMS.Core.Domain.Central.Users;
using PMS.Infrastructure.Database.Context;
using PMS.Core.Domain.Central.Departments.Prisons;
using PMS.Infrastructure.Database.Repositories.Central;
using PMS.Core.Domain.Central.Departments.Managements;
using PMS.Core.Domain.Central.Departments.Prisons.JudgmentClassifications;
using PMS.Core.Domain.Central.Departments.Related.Courts;
using PMS.Core.Domain.Central.Departments.Prisons.ConvictCategories;
using PMS.Core.Domain.Central.Departments.Related.ArrestCenters;
using PMS.Core.Domain.Central.Departments.Managements.AdministrativeUnits;
using PMS.Core.Domain.Central.Departments.Related.ArrestAgencies;
using PMS.Core.Domain.Central.Departments.Related.RequestSources;

namespace PMS.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("JWTSettings"));
        services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
        services.AddDatabase(configuration);
        services.AddIdentityUnitOfWork();
        services.AddAuthentication(configuration);
        services.AddAuthorization();
        services.AddPermissions();
        services.AddServices();
        services.AddRepositories();
        services.AddHttpContextAccessor();
    }

    private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MainContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<MainContext>()
        .AddDefaultTokenProviders();
    }

    private static IServiceCollection AddIdentityUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork<MainContext>>();
        return services;
    }

    private static void AddAuthorization(this IServiceCollection services)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        services.AddTransient<IClaimsTransformation, ClaimsTransformer>();

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
    }

    private static void AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                RequireExpirationTime = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero,
                ValidAudience = configuration["JWTSettings:Audience"],
                ValidIssuer = configuration["JWTSettings:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["JWTSettings:Key"]!))
            };
        });
    }

    private static void AddPermissions(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            foreach (var permission in Permissions.AllPermissions)
            {
                options.AddPolicy(permission, policy => policy.RequireClaim("Permission", permission));
            }
        });
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailSender, EmailService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<INumberGeneratorService, NumberGeneratorService>();
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IPrisonRepository, PrisonRepository>();
        services.AddScoped<IManagementRepository, ManagementRepository>();
        services.AddScoped<IJudgmentClassificationRepository, JudgmentClassificationRepository>();
        services.AddScoped<ICourtRepository, CourtRepository>();
        services.AddScoped<IConvictCategoryRepository, ConvictCategoryRepository>();
        services.AddScoped<IArrestCenterRepository, ArrestCenterRepository>();
        services.AddScoped<IArrestAgencyRepository, ArrestAgencyRepository>();
        services.AddScoped<IRequestSourceRepository, RequestSourceRepository>();
        services.AddScoped<IAdministrativeUnitRepository, AdministrativeUnitRepository>();
    }
}