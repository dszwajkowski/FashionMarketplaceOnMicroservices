using Microsoft.EntityFrameworkCore;
using OfferService.Models;

namespace OfferService.Data;
public class ApplicationDbContext : DbContext
{
    public DbSet<Offer> Offers { get; set; }
    public DbSet<OfferPhoto> OfferPhotos { get; set; }
    public DbSet<User> Users { get; set; }


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
            o.HasIndex(x => x.Id);
            o.HasIndex(x => x.UserId); // todo add more indexes for filtering
            o.HasMany(x => x.Photos);  
        });

        builder.Entity<OfferPhoto>(o =>
        {
            o.Property(x => x.OfferId)
                .HasConversion(ulidConverter);
            o.HasIndex(x => x.Id);
            o.HasIndex(x => x.OfferId);
        });

        builder.Entity<User>(o =>
        {
            o.HasIndex(x => x.Id);
        });
    }
}
