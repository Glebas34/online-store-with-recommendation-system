using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Entities;
using OnlineStore.Data;
using System.Globalization;
using CsvHelper.Configuration.Attributes;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        /*if (!await context.Database.EnsureCreatedAsync())
        {
            Console.WriteLine("ℹ️ База данных уже существует. Инициализация пропущена.");
            return;
        }*/
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine("⚙️ Начинаем инициализацию базы данных...");

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        if (await userManager.FindByEmailAsync("admin@example.com") is null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@example.com",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        var booksPath = "Data/books_train.csv";
        var ratingsPath = "Data/train_ratings.csv";
        if (!File.Exists(booksPath) || !File.Exists(ratingsPath))
        {
            Console.WriteLine("❌ CSV-файлы не найдены.");
            return;
        }

        var productDict = new Dictionary<string, BookRecord>();
        using (var reader = new StreamReader(booksPath))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true }))
        {
            foreach (var book in csv.GetRecords<BookRecord>())
                productDict[book.BookId] = book;
        }

        var userSet = new HashSet<string>();
        var products = new Dictionary<string, Product>();
        var reviews = new List<Review>();
        var carts = new List<Cart>();
        var orders = new List<Order>();

        using var ratingReader = new StreamReader(ratingsPath);
        using var ratingCsv = new CsvReader(ratingReader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true });

        var ratingGroups = ratingCsv.GetRecords<RatingRecord>()
            .GroupBy(r => r.UserId);

        foreach (var group in ratingGroups)
        {
            var userId = group.Key;
            var userName = $"user{userId}";
            var userEmail = $"{userName}@example.com";

            var user = new ApplicationUser
            {
                Id = userId,
                UserName = userName,
                Email = userEmail,
                EmailConfirmed = true
            };

            if (await userManager.FindByIdAsync(userId) == null)
            {
                var result = await userManager.CreateAsync(user, "User123!");
                if (!result.Succeeded) continue;
            }

            var cart = new Cart
            {
                UserId = user.Id,
                Items = new List<CartItem>()
            };

            if (await context.Carts.FirstOrDefaultAsync(c => c.UserId == userId) == null)
            {
                carts.Add(cart);
            }

            var orderItems = new List<OrderItem>();

            foreach (var r in group)
            {
                // Продукт
                if (!products.ContainsKey(r.BookId) && productDict.TryGetValue(r.BookId, out var book))
                {
                    var price = new Random().Next(100, 1000); // Цена от 100 до 1000 ₽
                    var imageUrl = string.IsNullOrWhiteSpace(book.ImageUrl) ||
                                   (!book.ImageUrl.StartsWith("http") && !book.ImageUrl.StartsWith("https"))
                                   ? "https://via.placeholder.com/150"
                                   : book.ImageUrl;

                    if (await context.Products.FirstOrDefaultAsync(p => p.Id == r.BookId) is null)
                    {
                        products[r.BookId] = new Product
                        {
                            Id = book.BookId,
                            Title = string.IsNullOrWhiteSpace(book.Title) ? $"Книга #{book.BookId}" : book.Title,
                            Description = $"Автор: {book.Authors ?? "неизвестен"}. Рейтинг: {book.AverageRating}",
                            Price = price,
                            ImageUrl = imageUrl
                        };
                    }
                }

                if (products.TryGetValue(r.BookId, out var product))
                {
                    if (await context.Reviews.FirstOrDefaultAsync(review => review.UserId == user.Id && product.Id == r.BookId) is null)
                    {
                        // Отзыв
                        reviews.Add(new Review
                        {
                            ProductId = product.Id,
                            UserId = user.Id,
                            Rating = (int)Math.Round(r.Rating),
                            Comment = $"Автоматический отзыв: {r.Rating}",
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    // Заказ
                    orderItems.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = 1,
                        UnitPrice = product.Price
                    });
                }
                if (await context.Products.FindAsync(r.BookId) != null)
                {
                    products.Remove(r.BookId);
                }
            }

            if (orderItems.Any())
            {
                orders.Add(new Order
                {
                    UserId = user.Id,
                    Items = orderItems,
                    OrderDate = DateTime.UtcNow
                });
            }
        }

        foreach (var product in products.Values)
        {
            if (await context.Products.FindAsync(product.Id) != null)
            {
                products.Remove(product.Id);

            }
        }
        context.Products.AddRange(products.Values);
        await context.SaveChangesAsync();
        context.Reviews.AddRange(reviews);
        await context.SaveChangesAsync();
        context.Carts.AddRange(carts);
        await context.SaveChangesAsync();
        context.Orders.AddRange(orders);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Загружено: {userSet.Count} пользователей, {products.Count} книг, {reviews.Count} отзывов, {carts.Count} корзин, {orders.Count} заказов.");
    }

    private class RatingRecord
    {
        [Name("user_id")]
        public string UserId { get; set; }

        [Name("book_id")]
        public string BookId { get; set; }

        [Name("rating")]
        public float Rating { get; set; }
    }

    private class BookRecord
    {
        [Name("book_id")]
        public string BookId { get; set; }

        [Name("title")]
        public string Title { get; set; }

        [Name("authors")]
        public string Authors { get; set; }

        [Name("average_rating")]
        public float AverageRating { get; set; }

        [Name("image_url")]
        public string ImageUrl { get; set; }
    }
}
