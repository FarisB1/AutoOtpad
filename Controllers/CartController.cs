using AutoOtpad.Helpers;
using AutoOtpad.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class CartController : Controller
{
    private readonly AppDbContext _context;

    public CartController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
        return View(cart);
    }

    public async Task<IActionResult> AddToCart(int partId)
    {
        var part = await _context.Parts.FindAsync(partId);
        if (part == null) return NotFound();

        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

        var promotion = await _context.Promotions
                        .FirstOrDefaultAsync(p => p.PartId == part.Id && p.IsActive && p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now);

        decimal finalPrice = part.Price;
        if (promotion != null)
        {
            finalPrice = part.Price - (part.Price * promotion.DiscountPercentage / 100);
        }

        var existingItem = cart.FirstOrDefault(c => c.PartId == partId);
        if (existingItem != null)
        {
            existingItem.Quantity++;
        }
        else
        {
            cart.Add(new CartItem
            {
                PartId = partId,
                PartName = part.Name,
                Price = finalPrice,
                Quantity = 1,
                ImagePath = part.ImagePath
            });
        }

        HttpContext.Session.SetObjectAsJson("Cart", cart);
        return RedirectToAction("Index");
    }

    public IActionResult ClearCart()
    {
        HttpContext.Session.Remove("Cart");
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder()
    {
        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
        if (cart == null || !cart.Any()) return RedirectToAction("Index", "Cart");

        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized(); // korisnik nije prijavljen
        int userId = int.Parse(userIdStr);

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.Now,
            Status = "Pending",
            Items = new List<OrderItem>()
        };

        foreach (var item in cart)
        {
            var part = await _context.Parts.FindAsync(item.PartId);
            if (part == null)
            {
                ModelState.AddModelError("", $"Dio s ID-em {item.PartId} ne postoji.");
                return View("Index", cart);
            }

            if (part.QuantityInStock < item.Quantity)
            {
                ModelState.AddModelError("", $"Nema dovoljno zaliha za dio {part.Name}.");
                return View("Index", cart);
            }
            var promotion = await _context.Promotions
                        .FirstOrDefaultAsync(p => p.PartId == part.Id && p.IsActive && p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now);

            decimal finalPrice = part.Price;
            if (promotion != null)
            {
                finalPrice = part.Price - (part.Price * promotion.DiscountPercentage / 100);
            }
            // Ažuriraj zalihe
            part.QuantityInStock -= item.Quantity;
            part.NeedsRestock = part.QuantityInStock <= part.ReorderLevel;
            
            order.Items.Add(new OrderItem
            {
                PartId = part.Id,
                Quantity = item.Quantity,
                Price = finalPrice
            });

            _context.Parts.Update(part);
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        HttpContext.Session.Remove("Cart");
        return RedirectToAction("MyOrders", "Order");
    }


}
