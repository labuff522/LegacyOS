namespace LegacyOS.Api.Features.Products;

public class Product
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public string ShortName { get; set; } = "";

    public string? Description { get; set; }

    public ProductType ProductType { get; set; } = ProductType.Other;

    public decimal Price { get; set; }

    public bool IsSessionPackage { get; set; }

    public bool HasUnlimitedSessions { get; set; }

    public int? SessionCount { get; set; }

    public int? ValidityDays { get; set; }
    public int? InstallmentCount { get; set; }
    public int? BillingDayOfMonth { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}
