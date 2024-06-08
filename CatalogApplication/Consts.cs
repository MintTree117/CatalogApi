namespace CatalogApplication;

internal static class Consts
{
    internal const float MaxRating = 5;
    internal const string Divider = "-------------------------------------------------------------------------------------------";

    internal static HashSet<Guid> Guids = [];

    public static Guid NewGuidSafe()
    {
        const int safety = 5;
        for ( int i = 0; i < safety; i++ ) {
            Guid g = Guid.NewGuid();
            if (Guids.Add( g ))
                return g;
        }
        throw new Exception( "Failed to generate a safe GUID." );
    }
}