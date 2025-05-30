using Microsoft.AspNetCore.Identity;

namespace OnlineStore.Entities;

public class ApplicationUser : IdentityUser
{    public Cart Cart { get; set; }
    public ICollection<Order> Orders { get; set; }
}
