using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderStatus> OrderStatuses { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }

    public ApplicationDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var orderBuilder = modelBuilder.Entity<Order>();
        orderBuilder.HasIndex(x => x.Id);
        orderBuilder.HasIndex(x => x.UserId);
        orderBuilder.HasIndex(x => x.OfferId);

        var deliveryAddress = orderBuilder.OwnsOne(x => x.DeliveryAddress);
        deliveryAddress.Property(x => x.AddressLine)
            .IsRequired()
            .HasMaxLength(100);
        deliveryAddress.Property(x => x.Country)
            .IsRequired()
            .HasMaxLength(100);
        deliveryAddress.Property(x => x.City)
            .IsRequired()
            .HasMaxLength(100);
        deliveryAddress.Property(x => x.AddressLine)
            .IsRequired()
            .HasMaxLength(100);
        deliveryAddress.Property(x => x.PostalCode)
            .IsRequired()
            .HasMaxLength(20);

        modelBuilder.Entity<OrderStatus>()
            .HasIndex(x => x.Id);

        modelBuilder.Entity<PaymentMethod>()
            .HasIndex(x => x.Id);
    }
}
