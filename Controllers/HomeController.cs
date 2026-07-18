using ComplainManagementSystem.context;
using ComplainManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ComplainManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        private bool IsLoggedIn() => HttpContext.Session.GetString("UserId") != null;

        public IActionResult Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

            var userId     = int.Parse(HttpContext.Session.GetString("UserId")!);
            var complaints = _db.Complaints
                                .Where(c => c.UserId == userId)
                                .OrderByDescending(c => c.SubmittedDate)
                                .ToList();

            ViewBag.Complaints  = complaints;
            ViewBag.Total       = complaints.Count;
            ViewBag.Pending     = complaints.Count(c => c.Status == "pending");
            ViewBag.InProgress  = complaints.Count(c => c.Status == "progress");
            ViewBag.Resolved    = complaints.Count(c => c.Status == "resolved");
            ViewBag.Role        = HttpContext.Session.GetString("UserRole");
            ViewBag.UserName    = HttpContext.Session.GetString("UserName");
            return View();
        }

        public IActionResult Create()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            ViewBag.Role     = HttpContext.Session.GetString("UserRole");
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View();
        }

        [HttpPost]
        public IActionResult Create(string subject, string description, int categoryId)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

            var userId = int.Parse(HttpContext.Session.GetString("UserId")!);
            var complaint = new Complaint
            {
                Subject       = subject,
                Description   = description,
                CategoryId    = categoryId,
                Status        = "pending",
                UserId        = userId,
                SubmittedDate = DateTime.Now
            };

            _db.Complaints.Add(complaint);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

            var userId    = int.Parse(HttpContext.Session.GetString("UserId")!);
            var role      = HttpContext.Session.GetString("UserRole");
            var complaint = _db.Complaints.Include(c => c.User).FirstOrDefault(c => c.Id == id);

            if (complaint == null) return NotFound();

            if (role == "user" && complaint.UserId != userId)
                return RedirectToAction("Index");

            ViewBag.Complaint = complaint;
            ViewBag.Role      = role;
            ViewBag.UserName  = HttpContext.Session.GetString("UserName");
            return View();
        }

        [HttpPost]
        public IActionResult UpdateStatus(int id, string status, string adminComments)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

            var role = HttpContext.Session.GetString("UserRole");
            if (role != "admin" && role != "auditor") return RedirectToAction("Index");

            var complaint = _db.Complaints.Find(id);
            if (complaint == null) return NotFound();

            complaint.Status        = status;
            complaint.AdminComments = adminComments;
            _db.SaveChanges();

            if (role == "admin")   return RedirectToAction("Index", "Admin");
            if (role == "auditor") return RedirectToAction("Index", "Auditor");
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
