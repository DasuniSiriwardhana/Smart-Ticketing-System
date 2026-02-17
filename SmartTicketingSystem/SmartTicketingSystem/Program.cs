using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using SmartTicketingSystem.Authorization;
using SmartTicketingSystem.Data;
using SmartTicketingSystem.SeedData;
using SmartTicketingSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// DB
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ✅ Identity (ONLY ONCE)
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

        })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Fix Register page crash
builder.Services.AddTransient<IEmailSender, DummyEmailSender>();

// MVC + Razor Pages (Identity UI uses Razor Pages)
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Authorization Handler (your custom role check from YOUR tables)
builder.Services.AddScoped<IAuthorizationHandler, HasAppRoleHandler>();

// Policies (MAKE SURE these strings match Role.rolename in your DB exactly)
builder.Services.AddAuthorization(options =>
{
    // Single role
    options.AddPolicy("AdminOnly",
        policy => policy.Requirements.Add(new HasAppRoleRequirement("Admin")));

    options.AddPolicy("OrganizerOnly",
        policy => policy.Requirements.Add(new HasAppRoleRequirement("Organizer")));

    options.AddPolicy("ExternalMemberOnly",
        policy => policy.Requirements.Add(new HasAppRoleRequirement("ExternalMember")));

    options.AddPolicy("UniversityMemberOnly",
        policy => policy.Requirements.Add(new HasAppRoleRequirement("UniversityMember")));

    // Multiple roles
    options.AddPolicy("AdminOrOrganizer",
        policy => policy.Requirements.Add(new HasAppRoleRequirement("Admin", "Organizer")));

    options.AddPolicy("MemberOnly",
        policy => policy.Requirements.Add(
            new HasAppRoleRequirement("ExternalMember", "UniversityMember", "Organizer", "Admin")));
});

var app = builder.Build();

// Seed roles + default data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Initialize(services);
}

// Pipeline
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

// Auth must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
