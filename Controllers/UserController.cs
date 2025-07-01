using AutoOtpad.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class UserController : Controller
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /User/Register
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // POST: /User/Register
    [HttpPost]
    public async Task<IActionResult> Register(string username, string email, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ModelState.AddModelError("", "All fields are required.");
            return View();
        }

        // Check if user already exists
        var exists = await _context.Users.AnyAsync(u => u.Username == username || u.Email == email);
        if (exists)
        {
            ModelState.AddModelError("", "Username or email already taken.");
            return View();
        }

        // Hash password (simple example, use a better approach like ASP.NET Identity in production)
        var hashedPassword = ComputeSha256Hash(password);

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = hashedPassword,
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return RedirectToAction("Login");
    }

    // GET: /User/Login
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // POST: /User/Login
    [HttpPost]
    public async Task<IActionResult> Login(string usernameOrEmail, string password)
    {
        if (string.IsNullOrEmpty(usernameOrEmail) || string.IsNullOrEmpty(password))
        {
            ModelState.AddModelError("", "All fields are required.");
            return View();
        }

        var hashedPassword = ComputeSha256Hash(password);

        var user = await _context.Users.FirstOrDefaultAsync(u =>
            (u.Username == usernameOrEmail || u.Email == usernameOrEmail)
            && u.PasswordHash == hashedPassword);

        if (user == null)
        {
            ModelState.AddModelError("", "Invalid login attempt.");
            return View();
        }

        // For demo: Just set a session variable or cookie (implement proper auth in production)
        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("Username", user.Username);
        HttpContext.Session.SetString("Role", user.Role);


        return RedirectToAction("Index", "Home");
    }
    
    [HttpGet]
    public IActionResult Logout()
    {
        // Clear all session variables
        HttpContext.Session.Clear();

        // Optionally: Add a TempData message (shown after redirect)
        TempData["Message"] = "You have been logged out.";

        // Redirect to login or home
        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> Account()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            return RedirectToAction("Login");

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return RedirectToAction("Login");

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Account(string username, string email, string password)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            return RedirectToAction("Login");

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return RedirectToAction("Login");

        if (!string.IsNullOrEmpty(username))
            user.Username = username;

        if (!string.IsNullOrEmpty(email))
            user.Email = email;

        if (!string.IsNullOrEmpty(password) && password.Length >= 6)
            user.PasswordHash = ComputeSha256Hash(password); // Rehash password

        await _context.SaveChangesAsync();

        // Update session values
        HttpContext.Session.SetString("Username", user.Username);

        ViewData["Message"] = "Account updated successfully.";
        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> EditRole(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        var roles = new List<string> { "Admin", "User" };
        ViewBag.RoleOptions = new SelectList(roles, user.Role);

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRole(int id, string Role)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        user.Role = Role;
        await _context.SaveChangesAsync();

        return RedirectToAction("AllUsers"); // ili gdje već prikazuješ korisnike
    }

    public async Task<IActionResult> AllUsers()
    {
        var users = await _context.Users.ToListAsync();
        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return RedirectToAction("AllUsers");
    }

    private static string ComputeSha256Hash(string rawData)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        var builder = new StringBuilder();
        foreach (var b in bytes) builder.Append(b.ToString("x2"));
        return builder.ToString();
    }
}
