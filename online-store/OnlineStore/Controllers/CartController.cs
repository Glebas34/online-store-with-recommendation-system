using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Entities;
using System.Security.Claims;

namespace OnlineStore.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly AppDbContext _context;

    public CartController(AppDbContext context)
    {
        _context = context;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    // Получаем или создаём корзину текущего пользователя
    private async Task<Cart> GetOrCreateCartAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidOperationException("UserId is null or empty. User must be authenticated.");
        }

        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            try
            {
                cart = new Cart { UserId = userId, Items = [] };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating cart for user {userId}: {ex.Message}");
                throw;
            }
        }

        return cart;
    }

    // Просмотр корзины
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        var cart = await GetOrCreateCartAsync(userId);

        return View(cart.Items.ToList());
    }

    // Добавление товара в корзину
    [HttpPost]
    public async Task<IActionResult> Add(string productId)
    {
        var userId = GetCurrentUserId();
        var cart = await GetOrCreateCartAsync(userId);

        var existing = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (existing != null)
        {
            existing.Quantity++;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = productId,
                Quantity = 1
            });
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    // Удаление товара из корзины
    [HttpPost]
    public async Task<IActionResult> Remove(string productId)
    {
        var userId = GetCurrentUserId();
        var cart = await GetOrCreateCartAsync(userId);

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(string productId, int newQuantity)
    {
        if (string.IsNullOrWhiteSpace(productId) || newQuantity < 0)
        {
            TempData["Error"] = "Некорректный запрос.";
            return RedirectToAction("Index");
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var cart = await GetOrCreateCartAsync(userId);

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            if (newQuantity == 0)
            {
                _context.CartItems.Remove(item);
            }
            else
            {
                item.Quantity = newQuantity;
            }

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }


    // Очистка корзины
    [HttpPost]
    public async Task<IActionResult> Clear()
    {
        var userId = GetCurrentUserId();
        var cart = await GetOrCreateCartAsync(userId);

        _context.CartItems.RemoveRange(cart.Items);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}
