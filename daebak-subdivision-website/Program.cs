using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using daebak_subdivision_website.Models; // Adjust based on your project namespace

var builder = WebApplication.CreateBuilder(args);

// ✅ 1. Configure Database Connection (EF Core)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ 2. Add Authentication & Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";   // Redirect to login page if not authenticated
        options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect if access is denied
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // Keep user logged in for 7 days
        options.SlidingExpiration = true;
    });

// ✅ 3. Add Authorization (for security)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("ADMIN"));
});

// ✅ 4. Add Session Support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ 5. Add MVC Services
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ✅ 6. Configure HTTP Request Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ✅ 7. Enable Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// ✅ 8. Ensure Correct Default Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
