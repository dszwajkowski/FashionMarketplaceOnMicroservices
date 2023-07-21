using IdentityService.Data;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Configuration;

internal static class DatabaseConfiguration
{
    internal static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
    }

    internal static void MigrateDatabase(this WebApplication app)
    {
        using (var serviceScope = app.Services.CreateScope())
        {
            var dbcontext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
            ArgumentNullException.ThrowIfNull(dbcontext, nameof(dbcontext));
            dbcontext.Database.Migrate();
        }
    }
}
