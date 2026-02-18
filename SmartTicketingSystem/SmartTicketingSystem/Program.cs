using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using SmartTicketingSystem.Authorization;
using SmartTicketingSystem.Data;
using SmartTicketingSystem.SeedData;
using SmartTicketingSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// DATABASE PART
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// IDENTITY
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Fix Register page crash
builder.Services.AddTransient<IEmailSender, DummyEmailSender>();

// AUTH COOKIE SETTINGS
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";

    // Auto logout after inactivity
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);

    // Refresh expiration while user is active
    options.SlidingExpiration = true;

    // Session cookie: removed when browser fully closes
    options.Cookie.MaxAge = null;

    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// SESSION CONFIGURATION (NOT FOR LOGIN; for app temporary data)
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// MVC + RAZOR PAGES
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// POLICIES

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("OrganizerOnly", p => p.RequireRole("Organizer"));
    options.AddPolicy("ExternalMemberOnly", p => p.RequireRole("ExternalMember"));
    options.AddPolicy("UniversityMemberOnly", p => p.RequireRole("UniversityMember"));

    options.AddPolicy("AdminOrOrganizer", p => p.RequireRole("Admin", "Organizer"));
    options.AddPolicy("AdminOrUniversityMember", p => p.RequireRole("Admin", "UniversityMember"));

    options.AddPolicy("MemberOnly", p =>
        p.RequireRole("ExternalMember", "UniversityMember", "Organizer", "Admin"));
});

var app = builder.Build();

// SEED DATA
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Initialize(services);
}

// PIPELINE
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseSession();          // session features
app.UseAuthentication();   // identity cookie auth
app.UseAuthorization();    // policies/roles

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
