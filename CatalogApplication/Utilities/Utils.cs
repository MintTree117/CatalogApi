using CatalogApplication.Repositories.Features;
using CatalogApplication.Types._Common.Geography;
using CatalogApplication.Types.Products.Dtos;

namespace CatalogApplication.Utilities;

internal static class Utils
{
    const string PosX = "PosX";
    const string PosY = "PosY";
    
    internal static List<Guid>? ParseGuidList( string? value )
    {
        if (string.IsNullOrEmpty( value ))
            return null;

        return value
            .Split( ',' )
            .Select( static x =>
                Guid.TryParse( x, out Guid guid ) ? guid : Guid.Empty )
            .ToList();
    }
    internal static Guid? ParseGuid( string? value )
    {
        if (Guid.TryParse( value, out Guid result ))
            return result;
        
        return null;
    }
    internal static int? ParseInt( string? value )
    {
        if (int.TryParse( value, out int result ))
            return result;
        
        return null;
    }
    internal static bool? ParseBool( string? value )
    {
        if (bool.TryParse( value, out bool result ))
            return result;
        
        return null;
    }
    internal static async Task ApplyShippingEstimates( List<Guid> productIds, List<ProductSummaryDto> results, IQueryCollection query, InventoryRepository inventory )
    {
        AddressDto? address = null;
        var posX = Utils.ParseInt( query[PosX] );
        var posY = Utils.ParseInt( query[PosY] );
        
        if (posX is not null && posY is not null)
            address = new AddressDto( posX.Value, posX.Value );
        
        var estimates = await inventory.GetShippingEstimates( productIds, address );
        
        for ( int i = 0; i < results.Count && i < estimates.Count; i++ )
        {
            var dto = results[i];
            dto.ShippingDays = estimates[i];
        }
    }
}