namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct SearchReply(
    int TotalMatches,
    List<SearchDto> Results );