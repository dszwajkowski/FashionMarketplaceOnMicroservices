using OrderService.Models;

namespace OrderService.Data;

internal class SeedDatabase
{
    internal static void SeedDb(ApplicationDbContext dbContext)
    {
        AddOrderStatuses(dbContext);
        AddPaymentMethods(dbContext);
        dbContext.SaveChanges();
    }

    private static void AddOrderStatuses(ApplicationDbContext dbContext)
    {
        if (!dbContext.OrderStatuses.Any())
        {
            foreach (var status in Enum.GetValues(typeof(OrderStatuses)).Cast<OrderStatuses>())
            {
                var orderStatus = new OrderStatus
                {
                    Id = (int)status,
                    Name = status.ToString()
                };
                dbContext.OrderStatuses.Add(orderStatus);
            }
        }
    }

    private static void AddPaymentMethods(ApplicationDbContext dbContext)
    {
        if (!dbContext.PaymentMethods.Any())
        {
            foreach (var payment in Enum.GetValues(typeof(PaymentMethods)).Cast<PaymentMethods>())
            {
                var paymentMethod = new PaymentMethod
                {
                    Id = (int)payment,
                    Name = payment.ToString()
                };
                dbContext.PaymentMethods.Add(paymentMethod);
            }
        }
    }
}
