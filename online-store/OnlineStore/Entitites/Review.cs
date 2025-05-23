using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OnlineStore.Entities;

public class Review
{
    public string Id { get; } = Guid.NewGuid().ToString();
    //[ForeignKey("User")]
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public string Comment { get; set; }
    public float Rating { get; set; }

    //[ForeignKey("Product")]
    public string ProductId { get; set; }
    public Product Product { get; set; }
    public DateTime CreatedAt { get; set; }
}
