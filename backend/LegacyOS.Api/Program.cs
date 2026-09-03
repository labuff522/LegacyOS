using LegacyOS.Api.Data;
using LegacyOS.Api.Features.Families;
using LegacyOS.Api.Features.Memberships;
using LegacyOS.Api.Features.Registration;
using LegacyOS.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using LegacyOS.Api.Features.Products;
using LegacyOS.Api.Features.Activities;
using LegacyOS.Api.Features.Portal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using LegacyOS.Api.Features.Purchases;
using LegacyOS.Api.Features.UsaWrestling;
using LegacyOS.Api.Features.Dashboard;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:5173", "http://localhost:5174"];
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<LegacyOSDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("LegacyOS")));

builder.Services.AddScoped<FamilyRegistrationService>();
builder.Services.AddScoped<IPasswordHasher<PortalUser>, PasswordHasher<PortalUser>>();
builder.Services.AddHttpClient<StripeCheckoutService>(client => client.BaseAddress = new Uri("https://api.stripe.com/"));
builder.Services.AddAuthentication(PortalTokenAuthenticationHandler.AuthenticationScheme)
    .AddScheme<AuthenticationSchemeOptions, PortalTokenAuthenticationHandler>(
        PortalTokenAuthenticationHandler.AuthenticationScheme, _ => { });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole(PortalRoles.Customer));
    options.AddPolicy("StaffOnly", policy => policy.RequireRole(PortalRoles.Staff));
});

var app = builder.Build();

await DatabaseInitializer.InitializeAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("LocalFrontend");
app.UseAuthentication();
app.UseAuthorization();

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
app.MapPortalEndpoints();
app.MapPurchaseEndpoints();
app.MapUsaWrestlingEndpoints();
app.MapDashboardEndpoints();

app.Run();

public partial class Program;
