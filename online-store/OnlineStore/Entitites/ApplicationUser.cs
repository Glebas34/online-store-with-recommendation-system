using Microsoft.AspNetCore.Identity;

namespace OnlineStore.Entities;

public class ApplicationUser : IdentityUser
{
    //public string FullName { get; set; }
    //public string CartId { get; set; }
    public Cart Cart { get; set; }
    public ICollection<Order> Orders { get; set; }
}
