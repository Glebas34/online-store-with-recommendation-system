using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineStore.Entities;

public class Cart
{
    public string Id { get; } = Guid.NewGuid().ToString();
    //[ForeignKey("User")]
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    public ICollection<CartItem> Items { get; set; }
}
