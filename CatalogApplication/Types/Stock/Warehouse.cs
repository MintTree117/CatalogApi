using CatalogApplication.Types._Common.Geography;

namespace CatalogApplication.Types.Stock;

internal readonly record struct Warehouse(
    Guid Id,
    string QueryUrl,
    int PosX,
    int PosY );