using Microsoft.EntityFrameworkCore;
using daebak_subdivision_website.Models; // Adjust based on your project namespace

var builder = WebApplication.CreateBuilder(args);

// ✅ 1. Configure Database Connection (EF Core)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ 2. Add Session Support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ 3. Add MVC Services
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ✅ 4. Configure HTTP Request Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession(); // ✅ Enable session management

// ✅ 5. Ensure Correct Default Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); // Default route set to Home/Index

app.Run();
