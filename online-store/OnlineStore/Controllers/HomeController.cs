using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Entities;
using OnlineStore.Models;
using OnlineStore.Services;

namespace OnlineStore.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;
    private readonly RecommendationService _recommendation;

    public HomeController(AppDbContext context, RecommendationService recommendation)
    {
        _context = context;
        _recommendation = recommendation;
    }

    public async Task<IActionResult> Index()
    {
        List<string> recommendedIds = new();
        List<Product> recommendedProducts = new();
        List<Product> popularProducts;

        if (User.Identity.IsAuthenticated)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            recommendedIds = await _recommendation.GetRecommendations(userId);
            Console.WriteLine("Рекомендации:");
            foreach (var recommendedId in recommendedIds)
                Console.Write(recommendedId + " ");
            Console.WriteLine();

            if (recommendedIds.Any())
            {
                recommendedProducts = await _context.Products
                    .Where(p => recommendedIds.Contains(p.Id))
                    .ToListAsync();
            }
        }

        // Загружаем популярные товары (всегда)
        var popularIds = await _recommendation.GetPopularBooks();
        popularProducts = await _context.Products
            .Where(p => popularIds.Contains(p.Id))
            .ToListAsync();

        var model = new HomeViewModel
        {
            Recommendations = recommendedProducts,
            PopularBooks = popularProducts
        };

        return View(model);
    }

}

