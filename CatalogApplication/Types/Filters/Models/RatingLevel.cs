namespace CatalogApplication.Types.Filters.Models;

internal readonly record struct RatingLevel(
    Guid Id,
    RatingLevelName Level );

internal enum RatingLevelName
{
    One,
    Two,
    Three,
    Four,
    Five
}