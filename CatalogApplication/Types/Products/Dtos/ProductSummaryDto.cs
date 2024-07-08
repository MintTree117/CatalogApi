namespace CatalogApplication.Types.Products.Dtos;

internal sealed class ProductSummaryDto
{
    // ReSharper disable once UnusedMember.Global
    public ProductSummaryDto() { }
    
    public ProductSummaryDto(
        Guid Id,
        Guid BrandId,
        string Name,
        string BrandName,
        string Image,
        bool IsFeatured,
        bool IsInStock,
        decimal Price,
        decimal? SalePrice,
        DateTime? SaleEndDate,
        DateTime ReleaseDate,
        float Rating,
        int NumberRatings,
        int ShippingDays)
    {
        this.Id = Id;
        this.BrandId = BrandId;
        this.Name = Name;
        this.BrandName = BrandName;
        this.Image = Image;
        this.IsFeatured = IsFeatured;
        this.IsInStock = IsInStock;
        this.Price = Price;
        this.SalePrice = SalePrice;
        this.SaleEndDate = SaleEndDate;
        this.ReleaseDate = ReleaseDate;
        this.Rating = Rating;
        this.NumberRatings = NumberRatings;
        this.ShippingDays = ShippingDays;
    }
    public Guid Id { get; set; }
    public Guid BrandId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public bool IsInStock { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public DateTime? SaleEndDate { get; set; }
    public DateTime ReleaseDate { get; set; }
    public float Rating { get; set; }
    public int NumberRatings { get; set; }
    public int ShippingDays { get; set; }
}