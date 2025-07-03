using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity (MUST be before builder.Build())
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Add this to your services configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30); // Persistent cookie duration
    options.SlidingExpiration = true;
});

// Add this with your other service registrations
builder.Services.AddScoped<ProjectManagementSystem.Services.Interface.IActivityLogger,
                           ProjectManagementSystem.Services.ActivityLogger>();
var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Authentication MUST come before Authorization
app.UseAuthentication();
app.UseAuthorization();

// Database Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // Seed Companies
        var context = services.GetRequiredService<ApplicationDbContext>();
        await CompanySeeder.SeedCompanies(context);
        logger.LogInformation("Company seeding completed");

        // Seed Identity
        await IdentitySeeder.SeedRolesAndAdminAsync(services);
        logger.LogInformation("Identity seeding completed");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Seeding failed");
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Login}/{id?}");

app.Run();