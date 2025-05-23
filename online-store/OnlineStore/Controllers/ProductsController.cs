using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Entities;

public class ProductsController : Controller
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    // ✅ Просмотр всех товаров — доступен всем
    [AllowAnonymous]
    public async Task<IActionResult> Index(
        string? title, 
        string? author, 
        decimal? minPrice, 
        decimal? maxPrice, 
        int page = 1, 
        int pageSize = 10)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(p => EF.Functions.ILike(p.Title, $"%{title}%"));

        if (!string.IsNullOrWhiteSpace(author))
            query = query.Where(p => EF.Functions.ILike(p.Description, $"%{author}%"));

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        var totalItems = await query.CountAsync();
        var products = await query
            //.OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        ViewBag.CurrentPage = page;

        return View(products);
    }


    // ✅ Просмотр карточки товара
    [AllowAnonymous]
    public async Task<IActionResult> Details(string id)
    {
        var product = await _context.Products
            .Include(p => p.Reviews)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return NotFound();

        return View(product);
    }



    // ✅ Создание товара (форма) — только для админов
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    // ✅ Создание товара (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Product model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _context.Products.Add(model);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    // ✅ Редактирование товара (форма)
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(string id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        return View(product);
    }

    // ✅ Редактирование товара (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(Product model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _context.Products.Update(model);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    // ✅ Удаление товара
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}
