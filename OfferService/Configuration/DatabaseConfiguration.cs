using Microsoft.EntityFrameworkCore;
using OfferService.Data;

namespace OfferService.Configuration;

internal static class DatabaseConfiguration
{
    public static void AddDatabase(this IServiceCollection services, string connectionString)
    {
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
