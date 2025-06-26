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

using System.Globalization;

namespace LughSharp.Lugh.Utils;

[PublicAPI]
public static class StringUtils
{
    public static string ToHexString( int? value )
    {
        return value == null ? "null" : ( ( int )value ).ToString( "X" ).ToUpper();
    }

    // ========================================================================

    public static string? ValueOf( object? obj )
    {
        return obj == null ? "null" : obj.ToString();
    }

    public static string ValueOf( char[]? data )
    {
        return data == null ? "null" : new string( data );
    }

    public static string ValueOf( char[]? data, int offset, int count )
    {
        return data == null ? "null" : new string( data, offset, count );
    }

    public static string ValueOf( bool b )
    {
        return b.ToString().ToLower();
    }

    public static string ValueOf( char c )
    {
        return c.ToString();
    }

    public static string ValueOf( int i )
    {
        return i.ToString();
    }

    public static string ValueOf( long l )
    {
        return l.ToString();
    }

    public static string ValueOf( float f )
    {
        return f.ToString( CultureInfo.InvariantCulture );
    }

    public static string ValueOf( double d )
    {
        return d.ToString( CultureInfo.InvariantCulture );
    }
}