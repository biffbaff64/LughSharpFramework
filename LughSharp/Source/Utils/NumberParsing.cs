

using System.Globalization;

namespace LughSharp.Source.Utils;

/// <summary>
/// Provides utility methods for parsing numeric values from strings into various
/// numeric types, including unsigned integers, integers, floating-point numbers, and
/// long integers.
/// <para>
/// These methods support parsing with a specified invariant culture and handle default
/// values and TryParse functionality for safe parsing.
/// </para>
/// </summary>
[PublicAPI]
public static class NumberParsing
{
    // ----------------------------------------------------
    // UInts
    // ----------------------------------------------------
    
    /// <summary>
    /// Parses a string into an unsigned integer value using the invariant culture.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>The parsed unsigned integer value.</returns>
    public static uint ParseUint( string value )
    {
        return uint.Parse( value, NumberStyles.Integer, CultureInfo.InvariantCulture );
    }

    /// <summary>
    /// Attempts to parse a string into an unsigned integer value using the invariant culture.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <param name="defaultValue">The default value to return if parsing fails.</param>
    /// <returns>The parsed unsigned integer value, or the default value if parsing fails.</returns>
    public static uint ParseUint( string? value, uint defaultValue )
    {
        return string.IsNullOrWhiteSpace( value )
                   ? defaultValue
                   : uint.Parse( value, NumberStyles.Integer, CultureInfo.InvariantCulture );
    }

    /// <summary>
    /// Attempts to parse a string into an unsigned integer value using the invariant culture.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <param name="result">The parsed unsigned integer value, or 0 if parsing fails.</param>
    /// <returns>True if parsing was successful, false otherwise.</returns>
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
    
    /// <summary>
    /// Parses a string into an integer value using the invariant culture.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>The parsed integer value.</returns>
    public static int ParseInt( string value )
    {
        return int.Parse( value, NumberStyles.Integer, CultureInfo.InvariantCulture );
    }

    /// <summary>
    /// Attempts to parse a string into an integer value using the invariant culture.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <param name="defaultValue">The default value to return if parsing fails.</param>
    /// <returns>The parsed integer value, or the default value if parsing fails.</returns>
    public static int ParseInt( string? value, int defaultValue )
    {
        return string.IsNullOrWhiteSpace( value )
                   ? defaultValue
                   : int.Parse( value, NumberStyles.Integer, CultureInfo.InvariantCulture );
    }

    /// <summary>
    /// Attempts to parse a string into an integer value using the invariant culture.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <param name="result">The parsed integer value, or 0 if parsing fails.</param>
    /// <returns>True if parsing was successful, false otherwise.</returns>
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
    
    /// <summary>
    /// Parses a string into a float value using the invariant culture.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>The parsed float value.</returns>
    public static float ParseFloat( string value )
    {
        return float.Parse( value, NumberStyles.Float, CultureInfo.InvariantCulture );
    }

    /// <summary>
    /// Attempts to parse a string into a float value using the invariant culture.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <param name="defaultValue">The default value to return if parsing fails.</param>
    /// <returns>The parsed float value, or the default value if parsing fails.</returns>
    public static float ParseFloat( string? value, float defaultValue )
    {
        return string.IsNullOrWhiteSpace( value )
                   ? defaultValue
                   : float.Parse( value, NumberStyles.Float, CultureInfo.InvariantCulture );
    }

    /// <summary>
    /// Attempts to parse a string into a float value using the invariant culture.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <param name="result">The parsed float value, or 0 if parsing fails.</param>
    /// <returns>True if parsing was successful, false otherwise.</returns>
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
    
    /// <summary>
    /// Parses a string into a long value using the invariant culture.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>The parsed long value.</returns>
    public static long ParseLong( string value )
    {
        return long.Parse( value, NumberStyles.Integer, CultureInfo.InvariantCulture );
    }

    /// <summary>
    /// Attempts to parse a string into a long value using the invariant culture.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <param name="defaultValue">The default value to return if parsing fails.</param>
    /// <returns>The parsed long value, or the default value if parsing fails.</returns>
    public static long ParseLong( string? value, long defaultValue )
    {
        return string.IsNullOrWhiteSpace( value )
                   ? defaultValue
                   : long.Parse( value, NumberStyles.Integer, CultureInfo.InvariantCulture );
    }

    /// <summary>
    /// Attempts to parse a string into a long value using the invariant culture.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <param name="result">The parsed long value, or 0 if parsing fails.</param>
    /// <returns>True if parsing was successful, false otherwise.</returns>
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

