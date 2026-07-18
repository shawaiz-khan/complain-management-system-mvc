using ComplainManagementSystem.context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComplainManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;

        public AdminController(AppDbContext db)
        {
            _db = db;
        }

        // Helper: is the current session an admin?
        private bool IsAdmin() => HttpContext.Session.GetString("UserRole") == "admin";

        // GET: /Admin/Index
        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var complaints = _db.Complaints
                                .Include(c => c.User)
                                .OrderByDescending(c => c.SubmittedDate)
                                .ToList();

            var users = _db.Users.ToList();

            ViewBag.Complaints  = complaints;
            ViewBag.Users       = users;
            ViewBag.Total       = complaints.Count;
            ViewBag.Pending     = complaints.Count(c => c.Status == "pending");
            ViewBag.InProgress  = complaints.Count(c => c.Status == "progress");
            ViewBag.Resolved    = complaints.Count(c => c.Status == "resolved");
            ViewBag.Role        = "admin";
            ViewBag.UserName    = HttpContext.Session.GetString("UserName");
            return View();
        }

        // POST: /Admin/ChangeRole — change a user's role
        [HttpPost]
        public IActionResult ChangeRole(int userId, string role)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var user = _db.Users.Find(userId);
            if (user != null)
            {
                user.Role = role;
                _db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
