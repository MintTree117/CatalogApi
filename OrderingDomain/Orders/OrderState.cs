namespace OrderingDomain.Orders;

public enum OrderState
{
    Processing,
    Processed,
    Fulfilling,
    Shipping,
    Delivered,
    Cancelled,
    Returned,
    Suspended
}