namespace OrderProcessing.Api.Models
{
    //why not set the numbers for the enum values? Because we don't need to, the default values will be 0, 1, 2, and 3 for Pending, Confirmed, Cancelled, and Shipped respectively. If we wanted to set them explicitly, we could do so like this:
    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Shipped
    }
}
