using AutoOtpad.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoOtpad.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _context.Users.FirstOrDefaultAsync(); // Simulacija prijavljenog korisnika
            var orders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .Include(o => o.Items)
                .ThenInclude(i => i.Part)
                .ToListAsync();

            return View(orders);
        }


        // GET: Order/Create
        public IActionResult Create()
        {
            ViewBag.Parts = _context.Parts.ToList();
            return View();
        }

        // POST: Order/Create
        [HttpPost]
        public async Task<IActionResult> Create(int[] partIds, int[] quantities)
        {
            var user = await _context.Users.FirstOrDefaultAsync(); // Pretpostavimo da je ulogiran

            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                Status = "Pending",
                Items = new List<OrderItem>()
            };

            for (int i = 0; i < partIds.Length; i++)
            {
                var part = await _context.Parts.FindAsync(partIds[i]);
                if (part != null)
                {
                    if (part.QuantityInStock >= quantities[i])
                    {
                        part.QuantityInStock -= quantities[i];
                        part.NeedsRestock = part.QuantityInStock <= part.ReorderLevel;

                        var promotion = await _context.Promotions
                        .FirstOrDefaultAsync(p => p.PartId == part.Id && p.IsActive && p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now);

                        decimal finalPrice = part.Price;
                        if (promotion != null)
                        {
                            finalPrice = part.Price - (part.Price * promotion.DiscountPercentage / 100);
                        }


                        order.Items.Add(new OrderItem
                        {
                            PartId = part.Id,
                            Quantity = quantities[i],
                            Price = finalPrice
                        });


                        _context.Parts.Update(part);
                    }
                    else
                    {
                        // Opcionalno: vrati poruku ako nema dovoljno na lageru
                        ModelState.AddModelError("", $"Nema dovoljno zaliha za dio {part.Name}.");
                        ViewBag.Parts = await _context.Parts.ToListAsync();
                        return View();
                    }
                }
            }


            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = status;
            await _context.SaveChangesAsync();

            // Obavijest korisniku
            var user = await _context.Users.FindAsync(order.UserId);
            // TODO: Implementiraj email notifikaciju

            return RedirectToAction("AllOrders");
        }


        public async Task<IActionResult> MyOrders()
        {
            var userId = HttpContext.Session.GetString("UserId");// get current logged-in user's ID
            var orders = await _context.Orders
                .Where(o => o.UserId == int.Parse(userId))
                .Include(o => o.Items)
                .ThenInclude(i => i.Part)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> AllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .ThenInclude(i => i.Part)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            _context.OrderItems.RemoveRange(order.Items); // Remove related items first
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("AllOrders");
        }

    }

}
