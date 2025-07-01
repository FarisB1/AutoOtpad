// PromotionController.cs
using AutoOtpad.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AutoOtpad.Controllers
{
    public class PromotionController : Controller
    {
        private readonly AppDbContext _context;

        public PromotionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Promotion/
        public async Task<IActionResult> Index()
        {
            var promotions = await _context.Promotions.Include(p => p.Part).ToListAsync();
            return View(promotions);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Parts = new SelectList(await _context.Parts.ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Promotion promotion)
        {
            // Debug: Log what we received
            System.Diagnostics.Debug.WriteLine($"PartId received: {promotion.PartId}");
            System.Diagnostics.Debug.WriteLine($"Name received: {promotion.Name}");
            System.Diagnostics.Debug.WriteLine($"DiscountPercentage received: {promotion.DiscountPercentage}");

            // Check if Part exists
            if (promotion.PartId > 0)
            {
                var partExists = await _context.Parts.AnyAsync(p => p.Id == promotion.PartId);
                if (!partExists)
                {
                    ModelState.AddModelError("PartId", "Izabrani dio ne postoji.");
                }
            }
            else
            {
                ModelState.AddModelError("PartId", "Morate izabrati dio.");
            }

            // Validate dates
            if (promotion.StartDate >= promotion.EndDate)
            {
                ModelState.AddModelError("EndDate", "Datum kraja mora biti nakon datuma početka.");
            }

            // Check ModelState validity
            if (!ModelState.IsValid)
            {
                // Debug: Show validation errors
                foreach (var error in ModelState)
                {
                    System.Diagnostics.Debug.WriteLine($"Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }

                ViewBag.Parts = new SelectList(await _context.Parts.ToListAsync(), "Id", "Name", promotion.PartId);
                return View(promotion);
            }

            promotion.IsActive = promotion.StartDate <= DateTime.Now && promotion.EndDate >= DateTime.Now;

            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) return NotFound();

            ViewBag.Parts = new SelectList(await _context.Parts.ToListAsync(), "Id", "Name", promotion.PartId);
            return View(promotion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Promotion promotion)
        {
            if (id != promotion.Id) return NotFound();

            if (promotion.StartDate >= promotion.EndDate)
            {
                ModelState.AddModelError("EndDate", "Datum kraja mora biti nakon datuma početka.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Parts = new SelectList(await _context.Parts.ToListAsync(), "Id", "Name", promotion.PartId);
                return View(promotion);
            }

            promotion.IsActive = promotion.StartDate <= DateTime.Now && promotion.EndDate >= DateTime.Now;

            _context.Promotions.Update(promotion);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var promotion = await _context.Promotions
                .Include(p => p.Part)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (promotion == null) return NotFound();

            return View(promotion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) return NotFound();

            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

    }
}