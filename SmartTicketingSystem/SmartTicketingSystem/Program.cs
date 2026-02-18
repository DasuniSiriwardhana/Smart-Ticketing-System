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

// AUTHORIZATION HANDLER
builder.Services.AddScoped<IAuthorizationHandler, HasAppRoleHandler>();

// POLICIES

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly",
        policy => policy.Requirements.Add(new HasAppRoleRequirement("Admin")));

    options.AddPolicy("OrganizerOnly",
        policy => policy.Requirements.Add(new HasAppRoleRequirement("Organizer")));

    options.AddPolicy("ExternalMemberOnly",
        policy => policy.Requirements.Add(new HasAppRoleRequirement("ExternalMember")));

    options.AddPolicy("UniversityMemberOnly",
        policy => policy.Requirements.Add(new HasAppRoleRequirement("UniversityMember")));

    options.AddPolicy("AdminOrOrganizer",
        policy => policy.Requirements.Add(new HasAppRoleRequirement("Admin", "Organizer")));

    options.AddPolicy("AdminOrUniversityMember",
        policy => policy.Requirements.Add(new HasAppRoleRequirement("Admin", "UniversityMember")));

    options.AddPolicy("MemberOnly",
        policy => policy.Requirements.Add(
            new HasAppRoleRequirement("ExternalMember", "UniversityMember", "Organizer", "Admin")));
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
