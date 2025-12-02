using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using BlazorBday.Components;
using BlazorBday.Data;
using BlazorBday.Models;
using BlazorBday.Repositories;
using BlazorBday.Services;


var builder = WebApplication.CreateBuilder(args);

//========= Declare services to the container. =========//
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<OrderStateService>();

builder.Services.AddControllersWithViews();

// enable session for keeping OrderDraft
builder.Services.AddHttpContextAccessor(); // Add this for IHttpContextAccessor
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

// Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ISubcategoryRepository, SubcategoryRepository>();
builder.Services.AddScoped<IAgencyRepository, AgencyRepository>();

builder.Services.AddHttpClient();
builder.Services.AddDbContext<MarketShopDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DevConnection")));

// ASP.NET Core Identity for admin authentication
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings (can be adjusted for production)
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<MarketShopDbContext>()
.AddDefaultTokenProviders();

// Cookie settings for authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
} else
{
    app.UseDeveloperExceptionPage();

    // apply pending EF Core migrations on startup (development only)
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<MarketShopDbContext>();
        db.Database.Migrate();

        // Seed initial admin user
        await MarketSeedData.InitializeAsync(scope.ServiceProvider);

        // Seed product data
        var seeder = new MarketSeedData(db);
        await seeder.SeedAsync();
    }
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // <-- enable session middleware

app.UseAuthentication(); // <-- must come before UseAuthorization
app.UseAuthorization();

app.UseAntiforgery(); // <-- must come after UseAuthentication and UseAuthorization

// Map Blazor components first (gives priority to Blazor routes)
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map MVC controller routes (will be used when Blazor routes don't match)
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");


app.Run();
