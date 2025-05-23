using OnlineStore.Entities;

namespace OnlineStore.Models
{
    public class HomeViewModel
    {
        public List<Product> Recommendations { get; set; } = new();
        public List<Product> PopularBooks { get; set; } = new();
    }
}
