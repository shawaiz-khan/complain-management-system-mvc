using Microsoft.EntityFrameworkCore;
using ComplainManagementSystem.context;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!db.Users.Any(u => u.Email == "admin@cms.com"))
    {
        db.Users.Add(new ComplainManagementSystem.Models.User
        {
            FirstName    = "System",
            LastName     = "Admin",
            Email        = "admin@cms.com",
            PasswordHash = "admin123",
            Role         = "admin"
        });
    }

    if (!db.Users.Any(u => u.Email == "auditor@cms.com"))
    {
        db.Users.Add(new ComplainManagementSystem.Models.User
        {
            FirstName    = "Campus",
            LastName     = "Auditor",
            Email        = "auditor@cms.com",
            PasswordHash = "auditor123",
            Role         = "auditor"
        });
    }

    db.SaveChanges();
}

app.Run();

