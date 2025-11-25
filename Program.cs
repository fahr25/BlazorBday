using BlazorBday.Components;
using BlazorBday.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<CartService>();

builder.Services.AddControllers();

builder.Services.AddHttpClient();
builder.Services.AddSqlite<MarketShopDbContext>("Data Source=Data/marketshop.db");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

// Initialize database context
var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = scopeFactory.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MarketShopDbContext>();

    if (db.Database.EnsureCreated())
    {
        MarketSeedData.Initialize(db);
    }
}

app.Run();
