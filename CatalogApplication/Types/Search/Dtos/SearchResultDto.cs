namespace CatalogApplication.Types.Search.Dtos;

internal readonly record struct SearchResultDto(
    int TotalMatches,
    List<SearchItemDto> Results,
    List<int> ShippingDays );