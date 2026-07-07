using LegacyOS.Api.Data;
using LegacyOS.Api.Features.Families;
using LegacyOS.Api.Features.Memberships;
using LegacyOS.Api.Features.Registration;
using LegacyOS.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using LegacyOS.Api.Features.Products;
using LegacyOS.Api.Features.Activities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://localhost:5174")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<LegacyOSDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("LegacyOS")));

builder.Services.AddScoped<FamilyRegistrationService>();

var app = builder.Build();

await DatabaseInitializer.InitializeAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("LocalFrontend");

app.MapGet("/health", async (LegacyOSDbContext db) =>
{
    try
    {
        await db.Database.OpenConnectionAsync();
        await db.Database.CloseConnectionAsync();

        return Results.Ok(new
        {
            status = "LegacyOS Online",
            database = "Connected"
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            status = "LegacyOS Online",
            database = "Not Connected",
            error = ex.Message
        });
    }
});

app.MapFamilyEndpoints();
app.MapRegistrationEndpoints();
app.MapMembershipEndpoints();
app.MapProductEndpoints();
app.MapActivityEndpoints();

app.Run();