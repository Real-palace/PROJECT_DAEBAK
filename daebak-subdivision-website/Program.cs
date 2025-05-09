using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using daebak_subdivision_website.Models;
using daebak_subdivision_website.Controllers; // Add this line

var builder = WebApplication.CreateBuilder(args);

// ✅ 1. Configure Database Connection (EF Core)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Remove or comment out this line:
// builder.Services.AddScoped<AnnouncementsController>();

// ✅ 2. Add Authentication & Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";   // Redirect to login page if not authenticated
        options.AccessDeniedPath = "/Shared/AccessDenied"; // Redirect if access is denied
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // Keep user logged in for 7 days
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
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
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.IsEssential = true;
});

// Configure antiforgery
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.HeaderName = "X-XSRF-TOKEN";
});

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.WithOrigins("https://localhost:7023")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
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

// Add CORS middleware before UseAuthorization
app.UseCors("AllowAll");

// ✅ 7. Enable Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// ✅ 8. Ensure Correct Default Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
