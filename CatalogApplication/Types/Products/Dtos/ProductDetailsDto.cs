namespace CatalogApplication.Types.Products.Dtos;

internal sealed class ProductDetailsDto
{
    // ReSharper disable once UnusedMember.Global: FOR JSON
    public ProductDetailsDto() { }

    public ProductDetailsDto(
        Guid id,
        Guid brandId,
        string name,
        string brandName,
        string image,
        bool isFeatured,
        bool isInStock,
        decimal price,
        decimal? salePrice,
        decimal? shippingPrice,
        DateTime? saleEndDate,
        DateTime releaseDate,
        float rating,
        int numberRatings,
        decimal weight,
        string dimensions,
        int shippingDays,
        List<Guid>? categoryIds,
        string? description,
        string? xml)
    {
        Id = id;
        BrandId = brandId;
        Name = name;
        BrandName = brandName;
        Image = image;
        IsFeatured = isFeatured;
        IsInStock = isInStock;
        Price = price;
        SalePrice = salePrice;
        ShippingPrice = shippingPrice;
        SaleEndDate = saleEndDate;
        ReleaseDate = releaseDate;
        Rating = rating;
        NumberRatings = numberRatings;
        Weight = weight;
        Dimensions = dimensions;
        ShippingDays = shippingDays;
        CategoryIds = categoryIds;
        Description = description;
        Xml = xml;
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
    public decimal? ShippingPrice { get; set; }
    public DateTime? SaleEndDate { get; set; }
    public DateTime ReleaseDate { get; set; }
    public float Rating { get; set; }
    public int NumberRatings { get; set; }
    public decimal Weight { get; set; }
    public string Dimensions { get; set; } = string.Empty;
    public int ShippingDays { get; set; }
    public List<Guid>? CategoryIds { get; set; }
    public string? Description { get; set; }
    public string? Xml { get; set; }
}