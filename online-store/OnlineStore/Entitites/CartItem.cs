using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineStore.Entities;

public class CartItem
{
    public string Id { get; } = Guid.NewGuid().ToString();
    //[ForeignKey("Cart")]
    public string CartId { get; set; }
    public Cart Cart { get; set; }

    //[ForeignKey("Product")]
    public string ProductId { get; set; }
    public Product Product { get; set; }

    public int Quantity { get; set; }
}

