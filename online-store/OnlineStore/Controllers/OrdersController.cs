using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Entities;
using System.Security.Claims;

namespace OnlineStore.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly AppDbContext _context;

    public OrdersController(AppDbContext context)
    {
        _context = context;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var userId = GetCurrentUserId();
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.Items.Any())
        {
            TempData["Error"] = "Корзина пуста.";
            return RedirectToAction("Index", "Cart");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateConfirmed()
    {
        var userId = GetCurrentUserId();
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.Items.Any())
            return RedirectToAction("Index", "Cart");

        var order = new Order
        {
            UserId = userId,
            Items = cart.Items.Select(ci => new OrderItem
            {
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                UnitPrice = ci.Product.Price
            }).ToList()
        };

        _context.Orders.Add(order);

        _context.CartItems.RemoveRange(cart.Items);
        await _context.SaveChangesAsync();

        return RedirectToAction("History");
    }

    public async Task<IActionResult> History()
    {
        var userId = GetCurrentUserId();
        var orders = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return View(orders);
    }
}
