using Microsoft.AspNetCore.SignalR;
using tec_parts_replenishment_transpor_web.Commons;
using tec_parts_replenishment_transpor_web.Hubs;
using tec_parts_replenishment_transpor_web.MiddlewareExtensions;
using tec_parts_replenishment_transpor_web.SubscribeTableDependencies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR(hubOptions => { 
    hubOptions.EnableDetailedErrors = true;
    hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(10); 
    hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(5);
});

// DI
builder.Services.AddSingleton<ReplenishmentHub>();
builder.Services.AddSingleton<SubscribeReplenishmentTableDependency>();
builder.Services.AddSingleton<InventoryAdjustmentHub>();
builder.Services.AddSingleton<SubscribeInventoryAdjustmentTableDependency>();


var app = builder.Build();

var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseWebSockets();

app.MapHub<ReplenishmentHub>("replenishmentHub");
app.MapHub<InventoryAdjustmentHub>("inventoryAdjustmentHub");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Top}/{action=Index}/{id?}");
});

app.UseSqlTableDependency<SubscribeReplenishmentTableDependency>(connectionString);
app.UseSqlTableDependency<SubscribeInventoryAdjustmentTableDependency>(connectionString);

app.Run();
