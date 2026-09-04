using LegacyOS.Api.Data;
using Microsoft.EntityFrameworkCore;
namespace LegacyOS.Api.Features.Discounts;
public static class DiscountEndpoints
{
    public static RouteGroupBuilder MapDiscountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/discount-codes").RequireAuthorization("StaffOnly");
        group.MapGet("/", async (LegacyOSDbContext db) => Results.Ok(await db.DiscountCodes.OrderBy(x => x.Code).Select(x => new
        { x.Id, x.Code, x.Description, discountType = x.DiscountType.ToString(), x.Value, x.ProductId, productName = x.Product != null ? x.Product.Name : null,
          x.StartsOn, x.EndsOn, x.MaxRedemptions, x.RedemptionCount, x.IsAutomaticSibling, x.SiblingStartPosition, x.SiblingEndPosition, x.IsActive }).ToListAsync()));
        group.MapPost("/", SaveAsync); group.MapPut("/{id:guid}", SaveAsync);
        return group;
    }
    private static async Task<IResult> SaveAsync(DiscountRequest request, LegacyOSDbContext db, Guid? id = null)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (code.Length < 3 || request.Value <= 0 || request.DiscountType == DiscountType.Percentage && request.Value > 100 ||
            request.IsAutomaticSibling && (request.SiblingStartPosition is < 2 || request.SiblingEndPosition < request.SiblingStartPosition))
            return Results.BadRequest(new { message = "Enter a valid code and discount value." });
        if (await db.DiscountCodes.AnyAsync(x => x.Code == code && x.Id != id)) return Results.Conflict(new { message = "Discount code already exists." });
        var item = id is Guid value ? await db.DiscountCodes.SingleOrDefaultAsync(x => x.Id == value) : new DiscountCode { Id = Guid.NewGuid(), CreatedOn = DateTime.UtcNow };
        if (item is null) return Results.NotFound();
        item.Code = code; item.Description = request.Description?.Trim(); item.DiscountType = request.DiscountType; item.Value = request.Value;
        item.ProductId = request.ProductId; item.StartsOn = request.StartsOn; item.EndsOn = request.EndsOn; item.MaxRedemptions = request.MaxRedemptions; item.IsActive = request.IsActive;
        item.IsAutomaticSibling = request.IsAutomaticSibling;
        item.SiblingStartPosition = request.IsAutomaticSibling ? request.SiblingStartPosition ?? 2 : null;
        item.SiblingEndPosition = request.IsAutomaticSibling ? request.SiblingEndPosition ?? 4 : null;
        if (id is null) db.DiscountCodes.Add(item); await db.SaveChangesAsync();
        return id is null ? Results.Created($"/discount-codes/{item.Id}", new { item.Id }) : Results.NoContent();
    }
}
public record DiscountRequest(string Code, string? Description, DiscountType DiscountType, decimal Value, Guid? ProductId,
    DateTime? StartsOn, DateTime? EndsOn, int? MaxRedemptions, bool IsActive = true, bool IsAutomaticSibling = false,
    int? SiblingStartPosition = null, int? SiblingEndPosition = null);
