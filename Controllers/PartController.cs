using AutoOtpad.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoOtpad.Controllers
{
    public class PartController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PartController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(string? search, string? vehicleMake, string? condition, decimal? minPrice, decimal? maxPrice)
        {
            ViewBag.UserId = HttpContext.Session.GetString("UserId");
            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewBag.Role = HttpContext.Session.GetString("Role");
            var query = _context.Parts.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.Contains(search));

            if (!string.IsNullOrEmpty(vehicleMake))
                query = query.Where(p => p.VehicleMake == vehicleMake);

            if (!string.IsNullOrEmpty(condition))
                query = query.Where(p => p.Condition == condition);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            var allParts = await query.ToListAsync();
            foreach (var part in allParts)
            {
                part.NeedsRestock = part.QuantityInStock <= part.ReorderLevel;

                var promo = await _context.Promotions
                    .FirstOrDefaultAsync(p => p.PartId == part.Id && p.IsActive && p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now);

                if (promo != null)
                {
                    var discount = part.Price * promo.DiscountPercentage / 100;
                    part.DiscountedPrice = part.Price - discount;
                }
                else
                {
                    part.DiscountedPrice = null;
                }
            }

            await _context.SaveChangesAsync();


            var model = new PartFilterViewModel
            {
                Parts = allParts,
                Search = search,
                VehicleMake = vehicleMake,
                Condition = condition,
                MinPrice = minPrice ?? 0,
                MaxPrice = maxPrice ?? 10000,
                AvailableMakes = await _context.Parts.Select(p => p.VehicleMake).Distinct().ToListAsync(),
                AvailableConditions = await _context.Parts.Select(p => p.Condition).Distinct().ToListAsync()
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Part part, IFormFile imageFile)
        {
            if (!ModelState.IsValid) return View(part);

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var savePath = Path.Combine(_env.WebRootPath, "images", "parts");

                if (!Directory.Exists(savePath))
                    Directory.CreateDirectory(savePath);

                var filePath = Path.Combine(savePath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                part.ImagePath = "/images/parts/" + fileName;
            }

            part.NeedsRestock = part.QuantityInStock <= part.ReorderLevel;

            _context.Parts.Add(part);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var part = await _context.Parts.FindAsync(id);
            if (part == null) return NotFound();
            return View(part);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Part updatedPart, IFormFile? ImageFile, string? ExistingImagePath)
        {
            if (id != updatedPart.Id) return NotFound();
            if (!ModelState.IsValid) return View(updatedPart);

            var part = await _context.Parts.FindAsync(id);
            if (part == null) return NotFound();

            part.Name = updatedPart.Name;
            part.VehicleMake = updatedPart.VehicleMake;
            part.PartType = updatedPart.PartType;
            part.Condition = updatedPart.Condition;
            part.Price = updatedPart.Price;
            part.QuantityInStock = updatedPart.QuantityInStock;
            part.IsTested = updatedPart.IsTested;
            part.Status = updatedPart.Status;
            part.ReorderLevel = updatedPart.ReorderLevel;

            part.NeedsRestock = part.QuantityInStock <= part.ReorderLevel;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine("wwwroot/images/parts", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }
                part.ImagePath = "/images/parts/" + fileName;
            }
            else
            {
                part.ImagePath = ExistingImagePath;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var part = await _context.Parts.FindAsync(id);
            if (part == null) return NotFound();

            _context.Parts.Remove(part);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Test(int id)
        {
            var part = await _context.Parts.FindAsync(id);
            if (part == null) return NotFound();
            return View(part);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Test(int id, string testResult, bool isTested)
        {
            var part = await _context.Parts.FindAsync(id);
            if (part == null) return NotFound();

            part.TestResult = testResult;
            part.IsTested = isTested;

            _context.Parts.Update(part);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
