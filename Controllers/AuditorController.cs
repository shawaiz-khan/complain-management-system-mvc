using ComplainManagementSystem.context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComplainManagementSystem.Controllers
{
    public class AuditorController : Controller
    {
        private readonly AppDbContext _db;

        public AuditorController(AppDbContext db)
        {
            _db = db;
        }

        private bool IsAuditor() => HttpContext.Session.GetString("UserRole") == "auditor";

        public IActionResult Index()
        {
            if (!IsAuditor()) return RedirectToAction("Login", "Auth");

            var complaints = _db.Complaints
                                .Include(c => c.User)
                                .OrderByDescending(c => c.SubmittedDate)
                                .ToList();

            ViewBag.Complaints  = complaints;
            ViewBag.Total       = complaints.Count;
            ViewBag.Pending     = complaints.Count(c => c.Status == "pending");
            ViewBag.InProgress  = complaints.Count(c => c.Status == "progress");
            ViewBag.Resolved    = complaints.Count(c => c.Status == "resolved");
            ViewBag.Role        = "auditor";
            ViewBag.UserName    = HttpContext.Session.GetString("UserName");
            return View();
        }
    }
}
