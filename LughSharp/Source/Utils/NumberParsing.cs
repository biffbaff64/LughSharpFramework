

using System.Globalization;

namespace LughSharp.Source.Utils;

[PublicAPI]
public static class NumberParsing
{
    // ----------------------------------------------------
    // UInts
    // ----------------------------------------------------
    
    public static uint ParseUint( string value )
    {
        return uint.Parse( value, NumberStyles.Integer, CultureInfo.InvariantCulture );
    }

    public static uint ParseUint( string? value, uint defaultValue )
    {
        return string.IsNullOrWhiteSpace( value )
                   ? defaultValue
                   : uint.Parse( value, NumberStyles.Integer, CultureInfo.InvariantCulture );
    }

    public static bool TryParseUint( string? value, out uint result )
    {
        return uint.TryParse( value,
                              NumberStyles.Integer,
                              CultureInfo.InvariantCulture,
                              out result );
    }
    
    // ----------------------------------------------------
    // Ints
    // ----------------------------------------------------
    
    public static int ParseInt( string value )
    {
        return int.Parse( value, NumberStyles.Integer, CultureInfo.InvariantCulture );
    }

    public static int ParseInt( string? value, int defaultValue )
    {
        return string.IsNullOrWhiteSpace( value )
                   ? defaultValue
                   : int.Parse( value, NumberStyles.Integer, CultureInfo.InvariantCulture );
    }

    public static bool TryParseInt( string? value, out int result )
    {
        return int.TryParse( value,
                              NumberStyles.Integer,
                              CultureInfo.InvariantCulture,
                              out result );
    }
    
    // ----------------------------------------------------
    // Floats
    // ----------------------------------------------------
    
    public static float ParseFloat( string value )
    {
        return float.Parse( value, NumberStyles.Float, CultureInfo.InvariantCulture );
    }

    public static float ParseFloat( string? value, float defaultValue )
    {
        return string.IsNullOrWhiteSpace( value )
                   ? defaultValue
                   : float.Parse( value, NumberStyles.Float, CultureInfo.InvariantCulture );
    }

    public static bool TryParseFloat( string? value, out float result )
    {
        return float.TryParse( value,
                               NumberStyles.Float,
                               CultureInfo.InvariantCulture,
                               out result );
    }
    
    // ----------------------------------------------------
    // Longs
    // ----------------------------------------------------
    
    public static long ParseLong( string value )
    {
        return long.Parse( value, NumberStyles.Integer, CultureInfo.InvariantCulture );
    }

    public static long ParseLong( string? value, long defaultValue )
    {
        return string.IsNullOrWhiteSpace( value )
                   ? defaultValue
                   : long.Parse( value, NumberStyles.Integer, CultureInfo.InvariantCulture );
    }

    public static bool TryParseLong( string? value, out long result )
    {
        return long.TryParse( value,
                               NumberStyles.Integer,
                               CultureInfo.InvariantCulture,
                               out result );
    }
}

// ============================================================================
// ============================================================================

