using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Entities;
using OnlineStore.Services;

[Authorize]
public class ReviewController : Controller
{
    private readonly AppDbContext _context;
    private readonly KafkaProducerService _kafka;

    public ReviewController(AppDbContext context, KafkaProducerService kafka)
    {
        _context = context;
        _kafka = kafka;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    [HttpPost]
    public async Task<IActionResult> Create(string productId, int rating, string comment)
    {
        if (rating < 1 || rating > 5)
        {
            TempData["Error"] = "Оценка должна быть от 1 до 5.";
            return RedirectToAction("Details", "Products", new { id = productId });
        }

        var product = await _context.Products.FindAsync(productId);
        if (product == null) return NotFound();

        var userId = GetCurrentUserId();

        var alreadyReviewed = await _context.Reviews
            .AnyAsync(r => r.ProductId == productId && r.UserId == userId);
        if (alreadyReviewed)
        {
            TempData["Error"] = "Вы уже оставили отзыв.";
            return RedirectToAction("Details", "Products", new { id = productId });
        }

        var review = new Review
        {
            ProductId = productId,
            UserId = userId,
            Rating = rating,
            Comment = comment,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        await _kafka.SendEventAsync((userId, productId, rating));

        TempData["Success"] = "Спасибо за отзыв!";
        return RedirectToAction("Details", "Products", new { id = productId });
    }

    [HttpPost]
public async Task<IActionResult> CreateAjax([FromBody] ReviewRequest model)
{
    if (model.Rating < 1 || model.Rating > 5)
        return BadRequest("Оценка должна быть от 1 до 5.");

    var product = await _context.Products
        .Include(p => p.Reviews)
        .ThenInclude(r => r.User)
        .FirstOrDefaultAsync(p => p.Id == model.ProductId);

    if (product == null)
        return NotFound();

    var userId = GetCurrentUserId();

    if (product.Reviews.Any(r => r.User?.Id == userId.ToString()))
        return BadRequest("Вы уже оставили отзыв.");

    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.ToString());

    if (user == null)
        return BadRequest("Пользователь не найден.");

    var review = new Review
    {
        ProductId = model.ProductId,
        UserId = user.Id,
        User = user,
        Rating = model.Rating,
        Comment = model.Comment,
        CreatedAt = DateTime.UtcNow
    };

    _context.Reviews.Add(review);
    await _context.SaveChangesAsync();

    await _kafka.SendEventAsync((user.Id, model.ProductId, model.Rating));

    return Json(new
    {
        user = user.UserName,
        rating = model.Rating,
        comment = model.Comment,
        createdAt = review.CreatedAt.ToLocalTime().ToString("g"),
        avgRating = product.Reviews.Append(review).Average(r => r.Rating)
    });
}

    public class ReviewRequest
    {
        public string ProductId { get; set; }
        public float Rating { get; set; }
        public string Comment { get; set; }
    }
}

