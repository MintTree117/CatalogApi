namespace CatalogApplication.Utilities;

internal static class Utils
{
    internal static List<Guid>? ParseGuidList( string? value )
    {
        if (string.IsNullOrEmpty( value ))
            return null;

        IEnumerable<Guid> guids = value.Split( ',' ).Select( static x => Guid.TryParse( x, out Guid guid ) ? guid : Guid.Empty );
        return guids.ToList();
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
}