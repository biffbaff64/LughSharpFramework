// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Circa64 Software Projects / Richard Ikin.
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

namespace LughSharp.Source.Utils;

/// <summary>
/// Provides bit flag constants for alignment.
/// </summary>
[PublicAPI]
[Flags]
public enum Align
{
    None   = 0,
    Center = 1 << 0,
    Top    = 1 << 1,
    Bottom = 1 << 2,
    Left   = 1 << 3,
    Right  = 1 << 4,
    
    Special = 1 << 5,
    
    TopLeft      = Top | Left,
    TopCenter    = Top | Center,
    TopRight     = Top | Right,
    MiddleLeft   = Center | Left,
    MiddleCenter = Center,
    MiddleRight  = Center | Right,
    BottomLeft   = Bottom | Left,
    BottomCenter = Bottom | Center,
    BottomRight  = Bottom | Right,
}

/// <summary>
/// Provides utility methods and constants for handling alignment
/// using bit flags.
/// </summary>
[PublicAPI]
public sealed class AlignUtils
{
    /// <summary>
    /// Determines whether the specified position is aligned to the left.
    /// </summary>
    /// <param name="position">The alignment position value.</param>
    /// <returns><c>true</c> if aligned to the left; otherwise, <c>false</c>.</returns>
    public static bool IsLeft( Align position )
    {
        return position.HasFlag( Align.Left );
    }

    /// <summary>
    /// Determines whether the specified position is aligned to the right.
    /// </summary>
    /// <param name="position">The alignment position value.</param>
    /// <returns><c>true</c> if aligned to the right; otherwise, <c>false</c>.</returns>
    public static bool IsRight( Align position )
    {
        return position.HasFlag( Align.Right );
    }

    /// <summary>
    /// Determines whether the specified position is aligned to the top.
    /// </summary>
    /// <param name="position">The alignment position value.</param>
    /// <returns><c>true</c> if aligned to the top; otherwise, <c>false</c>.</returns>
    public static bool IsTop( Align position )
    {
        return position.HasFlag( Align.Top );
    }

    /// <summary>
    /// Determines whether the specified position is aligned to the bottom.
    /// </summary>
    /// <param name="position">The alignment position value.</param>
    /// <returns><c>true</c> if aligned to the bottom; otherwise, <c>false</c>.</returns>
    public static bool IsBottom( Align position )
    {
        return position.HasFlag( Align.Bottom );
    }

    /// <summary>
    /// Determines whether the specified position is horizontally centered (not left or right).
    /// </summary>
    /// <param name="position">The alignment position value.</param>
    /// <returns><c>true</c> if horizontally centered; otherwise, <c>false</c>.</returns>
    public static bool IsCenterHorizontal( Align position )
    {
        return !position.HasFlag( Align.Left ) && !position.HasFlag( Align.Right );
    }

    /// <summary>
    /// Determines whether the specified position is vertically centered (not top or bottom).
    /// </summary>
    /// <param name="position">The alignment position value.</param>
    /// <returns><c>true</c> if vertically centered; otherwise, <c>false</c>.</returns>
    public static bool IsCenterVertical( Align position )
    {
        return !position.HasFlag( Align.Top ) && !position.HasFlag( Align.Bottom );
    }

    // ========================================================================

    /// <summary>
    /// Returns a string representation of the specified alignment position.
    /// </summary>
    /// <param name="position">The alignment position value.</param>
    /// <returns>A string describing the alignment.</returns>
    public static string ToString( Align position )
    {
        var buffer = new StringBuilder( "[" );

        if ( position.HasFlag( Align.Top ) )
        {
            buffer.Append( "Top" );
        }
        else if ( position.HasFlag( Align.Bottom ) )
        {
            buffer.Append( "Bottom" );
        }
        else
        {
            buffer.Append( "Center" );
        }

        buffer.Append( "] [" );

        if ( position.HasFlag( Align.Left ) )
        {
            buffer.Append( "Left" );
        }
        else if ( position.HasFlag( Align.Right ) )
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