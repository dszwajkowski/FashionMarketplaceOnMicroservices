using Microsoft.AspNetCore.Identity;
using IdentityService.Auth;
using IdentityService.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using IdentityService.Models;

namespace IdentityService.Configuration;

internal static class IdentityConfiguration
{
    internal static int MinimumPasswordLength = 6;

    internal static void ConfigureIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<User, IdentityRole>(o =>
        {
            o.SignIn.RequireConfirmedAccount = true;
            o.Lockout.MaxFailedAccessAttempts = 5;
            o.Password.RequiredLength = MinimumPasswordLength;
            o.Password.RequireLowercase = true;
            o.Password.RequireUppercase = true;
            o.Password.RequireDigit = true;
            o.Password.RequireNonAlphanumeric = false;
        })
            .AddEntityFrameworkStores<ApplicationDbContext>();

        var jwtSettings = new JwtSettings();
        configuration.Bind(nameof(jwtSettings), jwtSettings);
        jwtSettings.ValidationParameters = new TokenValidationParameters()
        {
            IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.ASCII.GetBytes(jwtSettings.Secret)),
            ValidateIssuerSigningKey = true,
            ValidateIssuer = false,
            ValidateAudience = false,
            RequireExpirationTime = true,
            ValidateLifetime = true
        };
        services.AddSingleton(jwtSettings);

        if (string.IsNullOrEmpty(jwtSettings.Secret))
        {
            throw new InvalidOperationException("Jwt secret not found.");
        }

        services.AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(o =>
            {
                o.SaveToken = true;
                o.TokenValidationParameters = jwtSettings.ValidationParameters;
            });

        services.AddScoped<IAuthService, AuthService>();

        services.AddAuthorization();
    }

    internal class JwtSettings
    {
        public string Secret { get; set; } = null!;
        public TokenValidationParameters ValidationParameters { get; set; } = null!;
    }
}
