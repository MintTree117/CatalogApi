using CatalogApplication.Types._Common;
using CatalogApplication.Types._Common.Geography;

namespace CatalogApplication.Types.Stock;

internal readonly record struct Warehouse(
    Guid Id,
    AddressDto AddressDto,
    string Name,
    string QueryUrl );