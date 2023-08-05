namespace OrderService.Models;

public enum OrderStatus
{
    Approved,
    WaitingForPayment,
    ToBeShipped,
    Shipped,
    Delivered,
    Closed
}
