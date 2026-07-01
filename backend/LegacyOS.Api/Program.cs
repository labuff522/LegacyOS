using LegacyOS.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<LegacyOSDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("LegacyOS")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", async (LegacyOSDbContext db) =>
{
    var connected = await db.Database.CanConnectAsync();

    return Results.Ok(new
    {
        status = "LegacyOS Online",
        database = connected ? "Connected" : "Not Connected",
        version = "pre-alpha"
    });
})
.WithName("HealthCheck");

app.Run();