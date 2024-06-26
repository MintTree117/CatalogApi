using CatalogApplication.Types.Products.Dtos;

namespace CatalogApplication.Types.Products.Models;

internal readonly record struct SearchQueryReply(
    int TotalMatches,
    List<ProductSummaryDto> Results );