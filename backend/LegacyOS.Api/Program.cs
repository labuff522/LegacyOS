using LegacyOS.Api.Data;
using LegacyOS.Api.Features.Families;
using LegacyOS.Api.Features.Registration;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI
builder.Services.AddOpenApi();

// PostgreSQL
builder.Services.AddDbContext<LegacyOSDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("LegacyOS")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", async (LegacyOSDbContext db, IConfiguration config) =>
{
    var connectionString = config.GetConnectionString("LegacyOS");

    try
    {
        await db.Database.OpenConnectionAsync();
        await db.Database.CloseConnectionAsync();

        return Results.Ok(new
        {
            status = "LegacyOS Online",
            database = "Connected",
            hasConnectionString = !string.IsNullOrWhiteSpace(connectionString)
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            status = "LegacyOS Online",
            database = "Not Connected",
            hasConnectionString = !string.IsNullOrWhiteSpace(connectionString),
            error = ex.Message
        });
    }
});
app.MapFamilyEndpoints();
app.MapRegistrationEndpoints();

app.Run();

builder.Services.AddDbContext<LegacyOSDbContext>(options =>
    options.UseNpgsql("Host=localhost;Port=5432;Database=lwaDB;Username=postgres;Password=Thisshouldworkdick1@"));