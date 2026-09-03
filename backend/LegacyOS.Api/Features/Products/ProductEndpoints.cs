using LegacyOS.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace LegacyOS.Api.Features.Products;

public static class ProductEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products").RequireAuthorization("StaffOnly");

        group.MapGet("/", async (LegacyOSDbContext db) =>
        {
            var products = await db.Products
                .OrderBy(p => p.Name)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.ShortName,
                    p.Description,
                    ProductType = p.ProductType.ToString(),
                    p.Price,
                    p.IsSessionPackage,
                    p.HasUnlimitedSessions,
                    p.SessionCount,
                    p.ValidityDays,
                    p.IsActive
                })
                .ToListAsync();

            return Results.Ok(products);
        });

        group.MapPost("/", async (CreateProductRequest request, LegacyOSDbContext db) =>
        {
            var validation = Validate(request.IsSessionPackage, request.HasUnlimitedSessions, request.SessionCount, request.ValidityDays);
            if (validation is not null) return validation;
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                ShortName = request.ShortName,
                Description = request.Description,
                ProductType = request.ProductType,
                Price = request.Price,
                IsSessionPackage = request.IsSessionPackage,
                HasUnlimitedSessions = request.HasUnlimitedSessions,
                SessionCount = request.HasUnlimitedSessions ? null : request.SessionCount,
                ValidityDays = request.IsSessionPackage ? request.ValidityDays : null,
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };

            db.Products.Add(product);
            await db.SaveChangesAsync();

            return Results.Created($"/products/{product.Id}", new
            {
                product.Id,
                product.Name,
                product.ShortName,
                product.Description,
                ProductType = product.ProductType.ToString(),
                product.Price,
                product.IsSessionPackage,
                product.HasUnlimitedSessions,
                product.SessionCount,
                product.ValidityDays,
                product.IsActive
            });
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest request, LegacyOSDbContext db) =>
        {
            var validation = Validate(request.IsSessionPackage, request.HasUnlimitedSessions, request.SessionCount, request.ValidityDays);
            if (validation is not null) return validation;
            var product = await db.Products.SingleOrDefaultAsync(x => x.Id == id);
            if (product is null) return Results.NotFound();
            product.Name = request.Name.Trim(); product.ShortName = request.ShortName.Trim();
            product.Description = request.Description?.Trim(); product.ProductType = request.ProductType;
            product.Price = request.Price; product.IsSessionPackage = request.IsSessionPackage;
            product.HasUnlimitedSessions = request.IsSessionPackage && request.HasUnlimitedSessions;
            product.SessionCount = product.HasUnlimitedSessions || !product.IsSessionPackage ? null : request.SessionCount;
            product.ValidityDays = product.IsSessionPackage ? request.ValidityDays : null;
            product.IsActive = request.IsActive;
            await db.SaveChangesAsync(); return Results.NoContent();
        });

        return group;
    }

    private static IResult? Validate(bool isSessionPackage, bool unlimited, int? count, int? validityDays)
    {
        if (!isSessionPackage) return null;
        if (validityDays is null or <= 0) return Results.BadRequest(new { message = "Session packages require a positive validity period." });
        if (!unlimited && count is null or <= 0) return Results.BadRequest(new { message = "Limited packages require a positive session count." });
        return null;
    }
}

public class CreateProductRequest
{
    public string Name { get; set; } = "";

    public string ShortName { get; set; } = "";

    public string? Description { get; set; }

    public ProductType ProductType { get; set; } = ProductType.Other;

    public decimal Price { get; set; }
    public bool IsSessionPackage { get; set; }
    public bool HasUnlimitedSessions { get; set; }
    public int? SessionCount { get; set; }
    public int? ValidityDays { get; set; }
}

public class UpdateProductRequest : CreateProductRequest { public bool IsActive { get; set; } = true; }
