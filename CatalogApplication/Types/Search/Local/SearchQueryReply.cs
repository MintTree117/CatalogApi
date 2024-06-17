using CatalogApplication.Types.Search.Dtos;

namespace CatalogApplication.Types.Search.Local;

internal readonly record struct SearchQueryReply(
    int TotalMatches,
    List<SearchItemDto> Results );