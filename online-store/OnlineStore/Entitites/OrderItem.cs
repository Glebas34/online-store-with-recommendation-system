using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineStore.Entities;

public class OrderItem
{
    public string Id { get; } = Guid.NewGuid().ToString();

    //[ForeignKey("Order")]
    public string OrderId { get; set; }
    public Order Order { get; set; }

    //[ForeignKey("Product")]
    public string ProductId { get; set; }
    public Product Product { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
