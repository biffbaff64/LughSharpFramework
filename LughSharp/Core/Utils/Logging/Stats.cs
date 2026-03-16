// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Circa64 Software Projects
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// /////////////////////////////////////////////////////////////////////////////

using JetBrains.Annotations;

namespace LughSharp.Core.Utils.Logging;

/// <summary>
/// A Utility class for storing and retrieving statistical meters.
/// <para>
/// Meters are held in a "%USERPROFILE%/.statsmeters.stats" file, and managed by
/// an instance of <see cref="Preferences" />.
/// </para>
/// </summary>
[PublicAPI]
public class Stats
{
    private static Preferences _prefs;
    private static object      _lock = new();
    
    // ========================================================================
    
    static Stats()
    {
        _prefs = new Preferences( "statsmeters.stats" );
    }

    /// <summary>
    /// Retrieve the specified meter from the meter storage.
    /// </summary>
    /// <param name="meter"> The meter name. </param>
    /// <returns> The value stored in the meter. </returns>
    public static int GetMeter( string meter )
    {
        lock ( _lock )
        {
            return _prefs?.GetInteger( meter, 0 ) ?? 0;
        }
    }

    /// <summary>
    /// Sets the specified meter to the specified value.
    /// </summary>
    /// <param name="meter"> The meter name. </param>
    /// <param name="amount"> The new value. </param>
    public static void SetMeter( string meter, int amount )
    {
        lock ( _lock )
        {
            _prefs.PutInteger( meter, amount );
            _prefs.Flush();
        }
    }

    /// <summary>
    /// Adds the specified amount to the specified meter.
    /// </summary>
    /// <param name="meter"> The meter. </param>
    /// <param name="amount"> The amount to add. </param>
    public static void AddToMeter( string meter, int amount )
    {
        lock ( _lock )
        {
            _prefs.PutInteger( meter, _prefs.GetInteger( meter, 0 ) + amount );
            _prefs.Flush();
        }
    }

    /// <summary>
    /// Subtracts the specified amount from the specified meter.
    /// </summary>
    /// <param name="meter"> The meter. </param>
    /// <param name="amount"> The amount to subtract. </param>
    public static void SubMeter( string meter, int amount )
    {
        lock ( _lock )
        {
            _prefs.PutInteger( meter, _prefs.GetInteger( meter, 0 ) - amount );
            _prefs.Flush();
        }
    }

    /// <summary>
    /// Decrements the specified meter by 1.
    /// Use <see cref="AddToMeter(string, int)"/> to decrement by a specific amount.
    /// </summary>
    /// <param name="meter"></param>
    public static void DecMeter( string meter )
    {
        lock ( _lock )
        {
            _prefs.PutInteger( meter, _prefs.GetInteger( meter, 0 ) - 1 );
            _prefs.Flush();
        }
    }

    /// <summary>
    /// Increments the specified meter by 1.
    /// Use <see cref="AddToMeter(string, int)"/> to increment by a specific amount.
    /// </summary>
    /// <param name="meter"></param>
    public static void IncMeter( string meter )
    {
        lock ( _lock )
        {
            _prefs.PutInteger( meter, _prefs.GetInteger( meter, 0 ) + 1 );
            _prefs.Flush();
        }
    }

    /// <summary>
    /// Clears the specified meter.
    /// </summary>
    /// <param name="meter"> The meter to clear. </param>
    public static void ClearMeter( string meter )
    {
        lock ( _lock )
        {
            _prefs.PutInteger( meter, 0 );
            _prefs.Flush();
        }
    }

    /// <summary>
    /// Clears all meters.
    /// </summary>
    public static void ClearMeters()
    {
        lock ( _lock )
        {
            Dictionary< string, object > pd = _prefs.ToDictionary();

            foreach ( string key in pd.Keys )
            {
                _prefs.PutInteger( key, 0 );
            }

            _prefs.Flush();
        }
    }
}

// ============================================================================
// ============================================================================