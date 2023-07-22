using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OfferService.Models;

namespace OfferService.Data;
public class ApplicationDbContext : IdentityDbContext
{
    public DbSet<Offer> Offers { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var ulidConverter = new UlidConverters.UlidToStringConverter();

        builder.Entity<Offer>(o =>
        {
            o.Property(x => x.Id)
              .HasConversion(ulidConverter);
            o.HasMany(x => x.Photos);
        });  

        builder.Entity<OfferModel>()
            .Property(x => x.OfferId)
            .HasConversion(ulidConverter);
    }
}
