using LegacyOS.Api.Data;
using LegacyOS.Api.Features.Families;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI / Swagger
builder.Services.AddOpenApi();

// PostgreSQL
builder.Services.AddDbContext<LegacyOSDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("LegacyOS")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Health Check
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

// Family Endpoints
app.MapFamilyEndpoints();

app.Run();