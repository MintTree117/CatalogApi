namespace CatalogApplication.Types.Products.Dtos;

internal sealed class ProductDetailsDto
{
    // ReSharper disable once UnusedMember.Global: FOR JSON
    public ProductDetailsDto() { }

    public ProductDetailsDto(Guid id,
        Guid brandId,
        bool isFeatured,
        bool isInStock,
        string name,
        string image,
        decimal price,
        decimal salePrice,
        float rating,
        int numberRatings,
        int shippingDays,
        List<Guid>? categoryIds,
        string? description,
        string? xml)
    {
        Id = id;
        BrandId = brandId;
        IsFeatured = isFeatured;
        IsInStock = isInStock;
        Name = name;
        Image = image;
        Price = price;
        SalePrice = salePrice;
        Rating = rating;
        NumberRatings = numberRatings;
        ShippingDays = shippingDays;
        CategoryIds = categoryIds;
        Description = description;
        Xml = xml;
    }
    public Guid Id { get; set; }
    public Guid BrandId { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsInStock { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal SalePrice { get; set; }
    public float Rating { get; set; }
    public int NumberRatings { get; set; }
    public int ShippingDays { get; set; }
    public List<Guid>? CategoryIds { get; set; }
    public string? Description { get; set; }
    public string? Xml { get; set; }
}