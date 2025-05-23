using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using System.Security.Claims;

public class CartSummaryViewComponent : ViewComponent
{
    private readonly AppDbContext _context;

    public CartSummaryViewComponent(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        int count = 0;

        if (HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                count = cart?.Items.Sum(i => i.Quantity) ?? 0;
            }
        }

        return View(count);
    }
}
