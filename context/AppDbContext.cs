using Microsoft.EntityFrameworkCore;

using ComplainManagementSystem.Models;

namespace ComplainManagementSystem.context
{
    public class AppDbContext : DbContext {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<User> Users { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
    }
}