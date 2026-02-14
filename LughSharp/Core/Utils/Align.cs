// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Richard Ikin.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ///////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using JetBrains.Annotations;

namespace LughSharp.Core.Utils;

///// <summary>
///// Provides bit flag constants for alignment.
///// </summary>
//[PublicAPI]
//[Flags]
//public enum Align
//{
//    None   = 0,
//    Center = 1 << 0,
//    Top    = 1 << 1,
//    Bottom = 1 << 2,
//    Left   = 1 << 3,
//    Right  = 1 << 4,
//
//    TopLeft     = Top | Left,
//    TopRight    = Top | Right,
//    BottomLeft  = Bottom | Left,
//    BottomRight = Bottom | Right,
//}

/// <summary>
/// Provides utility methods and constants for handling alignment
/// using bit flags.
/// </summary>
[PublicAPI]
public sealed class Align
{
    public const int NONE   = 0;
    public const int CENTER = 1 << 0;
    public const int TOP    = 1 << 1;
    public const int BOTTOM = 1 << 2;
    public const int LEFT   = 1 << 3;
    public const int RIGHT  = 1 << 4;

    public const int TOP_LEFT     = TOP | LEFT;
    public const int TOP_RIGHT    = TOP | RIGHT;
    public const int BOTTOM_LEFT  = BOTTOM | LEFT;
    public const int BOTTOM_RIGHT = BOTTOM | RIGHT;

    // ========================================================================

    /// <summary>
    /// Determines whether the specified position is aligned to the left.
    /// </summary>
    /// <param name="position">The alignment position value.</param>
    /// <returns><c>true</c> if aligned to the left; otherwise, <c>false</c>.</returns>
    public static bool IsLeft( int position )
    {
        return ( position & LEFT ) != 0;
    }

    /// <summary>
    /// Determines whether the specified position is aligned to the right.
    /// </summary>
    /// <param name="position">The alignment position value.</param>
    /// <returns><c>true</c> if aligned to the right; otherwise, <c>false</c>.</returns>
    public static bool IsRight( int position )
    {
        return ( position & RIGHT ) != 0;
    }

    /// <summary>
    /// Determines whether the specified position is aligned to the top.
    /// </summary>
    /// <param name="position">The alignment position value.</param>
    /// <returns><c>true</c> if aligned to the top; otherwise, <c>false</c>.</returns>
    public static bool IsTop( int position )
    {
        return ( position & TOP ) != 0;
    }

    /// <summary>
    /// Determines whether the specified position is aligned to the bottom.
    /// </summary>
    /// <param name="position">The alignment position value.</param>
    /// <returns><c>true</c> if aligned to the bottom; otherwise, <c>false</c>.</returns>
    public static bool IsBottom( int position )
    {
        return ( position & BOTTOM ) != 0;
    }

    /// <summary>
    /// Determines whether the specified position is horizontally centered (not left or right).
    /// </summary>
    /// <param name="position">The alignment position value.</param>
    /// <returns><c>true</c> if horizontally centered; otherwise, <c>false</c>.</returns>
    public static bool IsCenterHorizontal( int position )
    {
        return ( ( position & LEFT ) == 0 ) && ( ( position & RIGHT ) == 0 );
    }

    /// <summary>
    /// Determines whether the specified position is vertically centered (not top or bottom).
    /// </summary>
    /// <param name="position">The alignment position value.</param>
    /// <returns><c>true</c> if vertically centered; otherwise, <c>false</c>.</returns>
    public static bool IsCenterVertical( int position )
    {
        return ( ( position & TOP ) == 0 ) && ( ( position & BOTTOM ) == 0 );
    }

    // ========================================================================

    /// <summary>
    /// Returns a string representation of the specified alignment position.
    /// </summary>
    /// <param name="position">The alignment position value.</param>
    /// <returns>A string describing the alignment.</returns>
    public static string ToString( int position )
    {
        var buffer = new StringBuilder( "[" );

        if ( ( position & TOP ) != 0 )
        {
            buffer.Append( "Top" );
        }
        else if ( ( position & BOTTOM ) != 0 )
        {
            buffer.Append( "Bottom" );
        }
        else
        {
            buffer.Append( "Center" );
        }

        buffer.Append( "] [" );

        if ( ( position & LEFT ) != 0 )
        {
            buffer.Append( "Left" );
        }
        else if ( ( position & RIGHT ) != 0 )
        {
            buffer.Append( "Right" );
        }
        else
        {
            buffer.Append( "Center" );
        }

        buffer.Append( ']' );

        return buffer.ToString();
    }
}

// ============================================================================
// ============================================================================
