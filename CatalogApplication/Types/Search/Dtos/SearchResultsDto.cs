namespace CatalogApplication.Types.Search.Dtos;

internal readonly record struct SearchResultsDto(
    int TotalMatches,
    List<SearchItemDto> Results,
    List<int> ShippingDays );