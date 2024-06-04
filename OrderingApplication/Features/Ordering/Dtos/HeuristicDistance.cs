namespace OrderingApplication.Features.Ordering.Dtos;

public readonly record struct HeuristicDistance(
    int MagnitudeX,
    int MagnitudeY )
{
    public static HeuristicDistance Max() => new( int.MaxValue, int.MaxValue );
}

public static class HeuristicDistanceExtentions
{
    public static bool IsLessThan( this HeuristicDistance distance, HeuristicDistance other )
    {
        return true;
    }
}