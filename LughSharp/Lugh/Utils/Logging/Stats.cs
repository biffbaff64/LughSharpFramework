// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Red 7 Projects
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

namespace LughSharp.Lugh.Utils.Logging;

[PublicAPI]
public class Stats
{
    private static IPreferences? _prefs;

    static Stats()
    {
        _prefs = Api.App.GetPreferences( "FrameworkMeters.stats" );
    }

    public static int GetMeter( string meter )
    {
        return _prefs?.GetInteger( meter, 0 ) ?? 0;
    }

    public static void SetMeter( string meter, int amount )
    {
        if ( _prefs != null )
        {
            _prefs.PutInteger( meter, amount );
            _prefs.Flush();
        }
    }

    public static void AddToMeter( string meter, int amount )
    {
        if ( _prefs != null )
        {
            _prefs.PutInteger( meter, ( _prefs.GetInteger( meter, 0 ) + amount ) );
            _prefs.Flush();
        }
    }

    public static void DecMeter( string meter )
    {
        if ( _prefs != null )
        {
            _prefs.PutInteger( meter, ( _prefs.GetInteger( meter, 0 ) - 1 ) );
            _prefs.Flush();
        }
    }

    public static void IncMeter( string meter )
    {
        if ( _prefs != null )
        {
            _prefs.PutInteger( meter, ( _prefs.GetInteger( meter, 0 ) + 1 ) );
            _prefs.Flush();
        }
    }

    public static void ClearMeter( string meter )
    {
        if ( _prefs != null )
        {
            _prefs.PutInteger( meter, 0 );
            _prefs.Flush();
        }
    }

    public static void ClearMeters()
    {
        if ( _prefs != null )
        {
            var pd = _prefs.ToDictionary();

            foreach ( var key in pd.Keys )
            {
                _prefs.PutInteger( key, 0 );
            }
            
            _prefs.Flush();
        }
    }
}

// ============================================================================
// ============================================================================