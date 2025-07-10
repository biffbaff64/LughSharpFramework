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

using System.Diagnostics.CodeAnalysis;

namespace LughSharp.Lugh.Graphics.SysDraw;

/// <summary>
/// This is essentially the <see cref="System.Drawing.Rectangle"/> class with
/// basic modifications for code style and framework preferences.
/// It has been taken to remove the need for <see cref="System.Drawing"/>.
/// </summary>
[PublicAPI]
public struct Rectangle : IEquatable< Rectangle >
{
    public static readonly Rectangle Empty;

    // ========================================================================

    /// <summary>
    /// Initializes a new instance of the <see cref='Rectangle'/> class with the
    /// specified location and size.
    /// </summary>
    public Rectangle( int x, int y, int width, int height )
    {
        this.X      = x;
        this.Y      = y;
        this.Width  = width;
        this.Height = height;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref='Rectangle'/> class with the
    /// specified location and size.
    /// </summary>
    public Rectangle( Point location, Size size )
    {
        X      = location.X;
        Y      = location.Y;
        Width  = size.Width;
        Height = size.Height;
    }

    // ========================================================================

    /// <summary>
    /// Gets or sets the coordinates of the upper-left corner of the rectangular region represented by this
    /// <see cref='Rectangle'/>.
    /// </summary>
    public Point Location
    {
        get => new Point( X, Y );
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }

    /// <summary>
    /// Gets or sets the size of this <see cref='Rectangle'/>.
    /// </summary>
    public Size Size
    {
        get => new Size( Width, Height );
        set
        {
            Width  = value.Width;
            Height = value.Height;
        }
    }

    /// <summary>
    /// Gets or sets the x-coordinate of the upper-left corner of the rectangular region defined by this
    /// <see cref='Rectangle'/>.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Gets or sets the y-coordinate of the upper-left corner of the rectangular region defined by this
    /// <see cref='Rectangle'/>.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Gets or sets the width of the rectangular region defined by this <see cref='Rectangle'/>.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the width of the rectangular region defined by this <see cref='Rectangle'/>.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Gets the x-coordinate of the upper-left corner of the rectangular region defined by this
    /// <see cref='Rectangle'/> .
    /// </summary>
    public int Left => X;

    /// <summary>
    /// Gets the y-coordinate of the upper-left corner of the rectangular region defined by this
    /// <see cref='Rectangle'/>.
    /// </summary>
    public int Top => Y;

    /// <summary>
    /// Gets the x-coordinate of the lower-right corner of the rectangular region defined by this
    /// <see cref='Rectangle'/>.
    /// </summary>
    public int Right => unchecked( X + Width );

    /// <summary>
    /// Gets the y-coordinate of the lower-right corner of the rectangular region defined by this
    /// <see cref='Rectangle'/>.
    /// </summary>
    public int Bottom => unchecked( Y + Height );

    /// <summary>
    /// Tests whether this <see cref='Rectangle'/> has a <see cref='Rectangle.Width'/>
    /// or a <see cref='Rectangle.Height'/> of 0.
    /// </summary>
    public bool IsEmpty => ( Height == 0 ) && ( Width == 0 ) && ( X == 0 ) && ( Y == 0 );

    /// <summary>
    /// Tests whether two <see cref='Rectangle'/> objects have equal location and size.
    /// </summary>
    public static bool operator ==( Rectangle left, Rectangle right ) =>
        ( left.X == right.X ) && ( left.Y == right.Y ) && ( left.Width == right.Width ) && ( left.Height == right.Height );

    /// <summary>
    /// Tests whether two <see cref='Rectangle'/> objects differ in location or size.
    /// </summary>
    public static bool operator !=( Rectangle left, Rectangle right ) => !( left == right );

    /// <summary>
    /// Converts a RectangleF to a Rectangle by performing a ceiling operation on all the coordinates.
    /// </summary>
    public static Rectangle Ceiling( RectangleF value )
    {
        unchecked
        {
            return new Rectangle(
                                 ( int )Math.Ceiling( value.X ),
                                 ( int )Math.Ceiling( value.Y ),
                                 ( int )Math.Ceiling( value.Width ),
                                 ( int )Math.Ceiling( value.Height ) );
        }
    }

    /// <summary>
    /// Converts a RectangleF to a Rectangle by performing a truncate operation on all the coordinates.
    /// </summary>
    public static Rectangle Truncate( RectangleF value )
    {
        unchecked
        {
            return new Rectangle(
                                 ( int )value.X,
                                 ( int )value.Y,
                                 ( int )value.Width,
                                 ( int )value.Height );
        }
    }

    /// <summary>
    /// Converts a RectangleF to a Rectangle by performing a round operation on all the coordinates.
    /// </summary>
    public static Rectangle Round( RectangleF value )
    {
        unchecked
        {
            return new Rectangle(
                                 ( int )Math.Round( value.X ),
                                 ( int )Math.Round( value.Y ),
                                 ( int )Math.Round( value.Width ),
                                 ( int )Math.Round( value.Height ) );
        }
    }

    /// <summary>
    /// Creates a new <see cref='Rectangle'/> with the specified location and size.
    /// </summary>
    public static Rectangle FromLeftTopRightBottom( int left, int top, int right, int bottom )
    {
        return new Rectangle( left, top, unchecked( right - left ), unchecked( bottom - top ) );
    }

    /// <summary>
    /// Determines if the specified point is contained within the rectangular region defined by this
    /// <see cref='Rectangle'/> .
    /// </summary>
    public bool Contains( int x, int y ) => ( X <= x ) && ( x < ( X + Width ) ) && ( Y <= y ) && ( y < ( Y + Height ) );

    /// <summary>
    /// Determines if the specified point is contained within the rectangular region defined by this
    /// <see cref='Rectangle'/> .
    /// </summary>
    public bool Contains( Point pt ) => Contains( pt.X, pt.Y );

    /// <summary>
    /// Determines if the rectangular region represented by <paramref name="rect"/> is entirely contained within the
    /// rectangular region represented by this <see cref='Rectangle'/> .
    /// </summary>
    public bool Contains( Rectangle rect ) =>
        ( X <= rect.X ) && ( ( rect.X + rect.Width ) <= ( X + Width ) ) &&
        ( Y <= rect.Y ) && ( ( rect.Y + rect.Height ) <= ( Y + Height ) );

    /// <summary>
    /// Inflates this <see cref='Rectangle'/> by the specified amount.
    /// </summary>
    public void Inflate( int width, int height )
    {
        unchecked
        {
            X -= width;
            Y -= height;

            Width  += 2 * width;
            Height += 2 * height;
        }
    }

    /// <summary>
    /// Inflates this <see cref='Rectangle'/> by the specified amount.
    /// </summary>
    public void Inflate( Size size ) => Inflate( size.Width, size.Height );

    /// <summary>
    /// Creates a <see cref='Rectangle'/> that is inflated by the specified amount.
    /// </summary>
    public static Rectangle Inflate( Rectangle rect, int x, int y )
    {
        rect.Inflate( x, y );

        return rect;
    }

    /// <summary>
    /// Creates a Rectangle that represents the intersection between this Rectangle and rect.
    /// </summary>
    public void Intersect( Rectangle rect )
    {
        var result = Intersect( rect, this );

        X      = result.X;
        Y      = result.Y;
        Width  = result.Width;
        Height = result.Height;
    }

    /// <summary>
    /// Creates a rectangle that represents the intersection between a and b. If there is no intersection, an
    /// empty rectangle is returned.
    /// </summary>
    public static Rectangle Intersect( Rectangle a, Rectangle b )
    {
        var x1 = Math.Max( a.X, b.X );
        var x2 = Math.Min( a.X + a.Width, b.X + b.Width );
        var y1 = Math.Max( a.Y, b.Y );
        var y2 = Math.Min( a.Y + a.Height, b.Y + b.Height );

        if ( ( x2 >= x1 ) && ( y2 >= y1 ) )
        {
            return new Rectangle( x1, y1, x2 - x1, y2 - y1 );
        }

        return Empty;
    }

    /// <summary>
    /// Determines if this rectangle intersects with rect.
    /// </summary>
    public bool IntersectsWith( Rectangle rect )
    {
        return ( rect.X < ( X + Width ) )
               && ( X < ( rect.X + rect.Width ) )
               && ( rect.Y < ( Y + Height ) )
               && ( Y < ( rect.Y + rect.Height ) );
    }

    /// <summary>
    /// Creates a rectangle that represents the union between a and b.
    /// </summary>
    public static Rectangle Union( Rectangle a, Rectangle b )
    {
        var x1 = Math.Min( a.X, b.X );
        var x2 = Math.Max( a.X + a.Width, b.X + b.Width );
        var y1 = Math.Min( a.Y, b.Y );
        var y2 = Math.Max( a.Y + a.Height, b.Y + b.Height );

        return new Rectangle( x1, y1, x2 - x1, y2 - y1 );
    }

    /// <summary>
    /// Adjusts the location of this rectangle by the specified amount.
    /// </summary>
    public void Offset( Point pos ) => Offset( pos.X, pos.Y );

    /// <summary>
    /// Adjusts the location of this rectangle by the specified amount.
    /// </summary>
    public void Offset( int x, int y )
    {
        unchecked
        {
            X += x;
            Y += y;
        }
    }

    // ========================================================================

    /// <summary>
    /// Tests whether <paramref name="obj"/> is a <see cref='Rectangle'/> with the same location
    /// and size of this Rectangle.
    /// </summary>
    public override bool Equals( [NotNullWhen( true )] object? obj )
    {
        return obj is Rectangle rectangle && Equals( rectangle );
    }

    public bool Equals( Rectangle other )
    {
        return this == other;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine( X, Y, Width, Height );
    }

    /// <summary>
    /// Converts the attributes of this <see cref='Rectangle'/> to a human readable string.
    /// </summary>
    public override string ToString()
    {
        return $"{{X={X},Y={Y},Width={Width},Height={Height}}}";
    }
}

// ========================================================================
// ========================================================================