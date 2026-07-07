using LegacyOS.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace LegacyOS.Api.Features.Products;

public static class ProductEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products");

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
                    p.IsActive
                })
                .ToListAsync();

            return Results.Ok(products);
        });

        group.MapPost("/", async (CreateProductRequest request, LegacyOSDbContext db) =>
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                ShortName = request.ShortName,
                Description = request.Description,
                ProductType = request.ProductType,
                Price = request.Price,
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
                product.IsActive
            });
        });

        return group;
    }
}

public class CreateProductRequest
{
    public string Name { get; set; } = "";

    public string ShortName { get; set; } = "";

    public string? Description { get; set; }

    public ProductType ProductType { get; set; } = ProductType.Other;

    public decimal Price { get; set; }
}