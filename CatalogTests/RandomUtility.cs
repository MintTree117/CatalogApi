namespace CatalogTests;

using System;

internal sealed class RandomUtility
{
    readonly Random random = new();
    
    public int GetRandomInt() => 
        random.Next();
    public int GetRandomInt( int max ) => 
        random.Next( max );
    public int GetRandomInt( int min, int max ) => 
        random.Next( min, max );
    public double GetRandomDouble() => 
        random.NextDouble();
    public double GetRandomDouble( double max ) => 
        random.NextDouble() * max;
    public double GetRandomDouble( double min, double max ) => 
        min + (random.NextDouble() * (max - min));
    public List<int> GetRandomInts( int max, int count )
    {
        List<int> ints = [];
        for ( int i = 0; i < count; i++ )
            ints.Add( GetRandomInt( max ) );
        return ints;
    }
    public List<int> GetRandomInts( int min, int max, int count )
    {
        List<int> ints = [];
        for ( int i = 0; i < count; i++ )
            ints.Add( GetRandomInt( min, max ) );
        return ints;
    }
}