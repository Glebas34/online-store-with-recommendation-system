using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineStore.Entities;

public class Order
{
    public string Id { get; } = Guid.NewGuid().ToString();
    //[ForeignKey("User")]
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public ICollection<OrderItem> Items { get; set; }
}
