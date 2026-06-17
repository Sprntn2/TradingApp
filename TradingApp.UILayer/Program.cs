using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TradingApp.BusinessLayer.Services;
using TradingApp.DataLayer;
using TradingApp.UILayer.Hubs;
using TradingApp.UILayer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.AddDbContext<TradingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TradingDB")));

builder.Services.AddSingleton<TradingCache>();
builder.Services.AddSingleton<ITradingCache>(sp => sp.GetRequiredService<TradingCache>());
builder.Services.AddSingleton<IPriceUpdateNotifier, SignalRPriceUpdateNotifier>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<TradingCache>());
builder.Services.AddHostedService<TradingSimulator>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = scope.ServiceProvider.GetRequiredService<TradingDbContext>();
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Database migration failed. Application startup is being terminated.");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapHub<TradingHub>("/hubs/trading");
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
