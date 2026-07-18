using ComplainManagementSystem.context;
using ComplainManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace ComplainManagementSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _db;

        public AuthController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /Auth/Login
        public IActionResult Login()
        {
            // If already logged in, redirect to the right dashboard
            var role = HttpContext.Session.GetString("UserRole");
            if (role == "admin")    return RedirectToAction("Index", "Admin");
            if (role == "auditor")  return RedirectToAction("Index", "Auditor");
            if (role == "user")     return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // Find user with matching email and password (plain text)
            var user = _db.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == password);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            // Save login info to session
            HttpContext.Session.SetString("UserId",   user.Id.ToString());
            HttpContext.Session.SetString("UserName", user.FirstName + " " + user.LastName);
            HttpContext.Session.SetString("UserRole", user.Role);

            // Redirect based on role
            if (user.Role == "admin")   return RedirectToAction("Index", "Admin");
            if (user.Role == "auditor") return RedirectToAction("Index", "Auditor");
            return RedirectToAction("Index", "Home");
        }

        // GET: /Auth/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        public IActionResult Register(string firstName, string lastName, string email, string password)
        {
            // Check if email already exists
            if (_db.Users.Any(u => u.Email == email))
            {
                ViewBag.Error = "An account with this email already exists.";
                return View();
            }

            // Create new user (role defaults to "user")
            var user = new User
            {
                FirstName    = firstName,
                LastName     = lastName,
                Email        = email,
                PasswordHash = password, // stored as plain text
                Role         = "user"
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            TempData["Success"] = "Account created! You can now log in.";
            return RedirectToAction("Login");
        }

        // GET: /Auth/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
