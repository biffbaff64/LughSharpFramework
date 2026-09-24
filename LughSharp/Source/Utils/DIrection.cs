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

namespace LughSharp.Source.Utils;

/// <summary>
/// Represents a direction in a 2D space using X and Y coordinates.
/// </summary>
/// <remarks>
/// The <c>Direction</c> class allows manipulation and querying of directions along the
/// X and Y axes. Directions are represented as integers, with specific constants such
/// as <c>DirectionStill</c> used to denote a lack of movement.
/// </remarks>
[PublicAPI]
public class Direction( int x, int y )
{
    /// <summary>
    /// The X coordinate of the direction.
    /// </summary>
    public int X { get; set; } = x;

    /// <summary>
    /// The Y coordinate of the direction.
    /// </summary>
    public int Y { get; set; } = y;

    // ========================================================================

    /// <summary>
    /// Default constructor, initialises both <c>X</c> and <c>Y</c> to zero.
    /// </summary>
    public Direction() : this( 0, 0 )
    {
    }

    /// <summary>
    /// Sets the <c>X</c> and <c>Y</c> properties to the specified values.
    /// </summary>
    /// <param name="x"> The new X value. </param>
    /// <param name="y"> The new Y value. </param>
    public void Set( int x, int y )
    {
        this.X = x;
        this.Y = y;
    }

    /// <summary>
    /// Sets the <c>X</c> and <c>Y</c> properties to the X and Y values
    /// from the supplied <see cref="Direction"/> object.
    /// </summary>
    public void Set( Direction dir )
    {
        this.X = dir.X;
        this.Y = dir.Y;
    }
    
    /// <summary>
    /// Clears X and Y directions by setting them to DirectionStill.
    /// </summary>
    public void StandStill()
    {
        X = Movement.DirectionStill;
        Y = Movement.DirectionStill;
    }

    /// <summary>
    /// Returns <c>true</c> if either the X or Y direction is not DirectionStill, otherwise <c>false</c>.
    /// </summary>
    public bool HasDirection()
    {
        return ( X != Movement.DirectionStill ) || ( Y != Movement.DirectionStill );
    }

    /// <summary>
    /// Returns <c>true</c> if the X direction is not DirectionStill, otherwise <c>false</c>.
    /// </summary>
    public bool HasXDirection() => ( X != Movement.DirectionStill );

    /// <summary>
    /// Returns <c>true</c> if the Y direction is not DirectionStill, otherwise <c>false</c>.
    /// </summary>
    public bool HasYDirection() => ( Y != Movement.DirectionStill );

    /// <summary>
    /// Returns the X direction, but flipped. i.e If current X direction is DirectionLeft
    /// this will return DirectionRight
    /// </summary>
    public int GetFlippedX() => X * -1;

    /// <summary>
    /// Returns the Y direction, but flipped. i.e If current Y direction is DirectionUp this
    /// will return DirectionDown.
    /// </summary>
    public int GetFlippedY() => Y * -1;

    /// <summary>
    /// Toggle both X and Y directions.
    /// </summary>
    public void Toggle()
    {
        if ( this.X != Movement.DirectionStill )
        {
            ToggleX();
        }

        if ( this.Y != Movement.DirectionStill )
        {
            ToggleY();
        }
    }

    /// <summary>
    /// Toggle X direction.
    /// </summary>
    public void ToggleX() => this.X *= -1;

    /// <summary>
    /// Toggle Y direction.
    /// </summary>
    public void ToggleY() => this.Y *= -1;

    /// <inheritdoc/>
    public override string ToString()
    {
        return Movement.TranslateDirection( X, Y ).name;
    }
}

// ============================================================================
// ============================================================================
