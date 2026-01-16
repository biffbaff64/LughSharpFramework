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

using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Exceptions;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace LughSharp.Core.Maths;

/// <summary>
/// Rectangle class that is independent of any backends.
/// This has no drawing methods, just those that handle the shape.
/// </summary>
[PublicAPI]
public class Rectangle : IShape2D, IEquatable< Rectangle >
{
    public static readonly Rectangle Tmp = new();

    // ========================================================================

    public float X      { get; set; }
    public float Y      { get; set; }
    public float Width  { get; set; }
    public float Height { get; set; }

    // ========================================================================

    private static readonly Rectangle _empty = new(); // Do Not Modifiy contents

    // ========================================================================

    /// <summary>
    /// Constructs a new rectangle with all values set to zero
    /// </summary>
    public Rectangle() : this( 0, 0, 0, 0 )
    {
    }

    /// <summary>
    /// Constructs a new rectangle with the given corner point in the bottom left and dimensions.
    /// </summary>
    /// <param name="x"> The corner point x-coordinate </param>
    /// <param name="y"> The corner point y-coordinate </param>
    /// <param name="width"> The width </param>
    /// <param name="height"> The height  </param>
    public Rectangle( float x, float y, float width, float height )
    {
        X      = x;
        Y      = y;
        Width  = width;
        Height = height;
    }

    /// <summary>
    /// Constructs a rectangle based on the given rectangle
    /// </summary>
    /// <param name="rect"> The rectangle  </param>
    public Rectangle( Rectangle rect )
    {
        X      = rect.X;
        Y      = rect.Y;
        Width  = rect.Width;
        Height = rect.Height;
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="x"> point x coordinate </param>
    /// <param name="y"> point y coordinate </param>
    /// <returns> whether the point is contained in the rectangle</returns>
    public bool Contains( float x, float y )
    {
        return ( X <= x ) && ( ( X + Width ) >= x ) && ( Y <= y ) && ( ( Y + Height ) >= y );
    }

    /// <summary>
    /// </summary>
    /// <param name="point"> The coordinates vector </param>
    /// <returns> whether the point is contained in the rectangle  </returns>
    public bool Contains( Vector2 point )
    {
        return Contains( point.X, point.Y );
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="x"> bottom-left x coordinate </param>
    /// <param name="y"> bottom-left y coordinate </param>
    /// <param name="width"> width </param>
    /// <param name="height"> height </param>
    /// <returns> this rectangle for chaining  </returns>
    public Rectangle Set( float x, float y, float width, float height )
    {
        X      = x;
        Y      = y;
        Width  = width;
        Height = height;

        return this;
    }

    /// <summary>
    /// Return the current rectangle position.
    /// </summary>
    public Vector2 GetPosition()
    {
        return new Vector2( X, Y );
    }

    /// <summary>
    /// Set the x and y coordinates of the bottom left corner,
    /// from the supplied vector.
    /// </summary>
    /// <returns>This rectangle for chaining.</returns>
    public Rectangle SetPosition( Vector2 position )
    {
        X = position.X;
        Y = position.Y;

        return this;
    }

    /// <summary>
    /// Set the x and y coordinates of the bottom left corner,
    /// from the supplied x and y values.
    /// </summary>
    /// <returns>This rectangle for chaining.</returns>
    public Rectangle SetPosition( float x, float y )
    {
        X = x;
        Y = y;

        return this;
    }

    // ========================================================================

    /// <summary>
    /// Sets the size of this rectangle from the values provided.
    /// </summary>
    /// <param name="width">The new width.</param>
    /// <param name="height">The new height.</param>
    /// <returns>This rectangle for chaining.</returns>
    public Rectangle SetSize( float width, float height )
    {
        Width  = width;
        Height = height;

        return this;
    }

    /// <summary>
    /// Sets the squared size of this rectangle from the value provided.
    /// </summary>
    /// <param name="sizeXY">The new width and height value.</param>
    /// <returns>This rectangle for chaining.</returns>
    public Rectangle SetSize( float sizeXY )
    {
        Width  = sizeXY;
        Height = sizeXY;

        return this;
    }

    /// <summary>
    /// Gets the current size of this rectangle.
    /// </summary>
    public Vector2 GetSize()
    {
        return new Vector2( Width, Height );
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="circle"> the circle </param>
    /// <returns> whether the circle is contained in the rectangle  </returns>
    public bool Contains( Circle circle )
    {
        return ( ( circle.X - circle.Radius ) >= X )
               && ( ( circle.X + circle.Radius ) <= ( X + Width ) )
               && ( ( circle.Y - circle.Radius ) >= Y )
               && ( ( circle.Y + circle.Radius ) <= ( Y + Height ) );
    }

    /// <summary>
    /// </summary>
    /// <param name="rectangle"> the other <see cref="Rectangle"/>.</param>
    /// <returns> whether the other rectangle is contained in this rectangle.</returns>
    public bool Contains( Rectangle rectangle )
    {
        var xmin = rectangle.X;
        var xmax = xmin + rectangle.Width;

        var ymin = rectangle.Y;
        var ymax = ymin + rectangle.Height;

        return ( xmin > X )
               && ( xmin < ( X + Width ) )
               && ( xmax > X )
               && ( xmax < ( X + Width ) )
               && ( ymin > Y )
               && ( ymin < ( Y + Height ) )
               && ( ymax > Y )
               && ( ymax < ( Y + Height ) );
    }

    /// <summary>
    /// Determines if the specified point is contained within the rectangular region defined by this
    /// <see cref='Rectangle'/> .
    /// </summary>
    public bool Contains( int x, int y )
    {
        return ( X <= x ) && ( x < ( X + Width ) ) && ( Y <= y ) && ( y < ( Y + Height ) );
    }

    /// <summary>
    /// Determines if the specified point is contained within the rectangular region defined by this
    /// <see cref='Rectangle'/> .
    /// </summary>
    public bool Contains( Point pt )
    {
        return Contains( pt.X, pt.Y );
    }

    // ========================================================================

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
    public void Inflate( Size size )
    {
        Inflate( size.Width, size.Height );
    }

    /// <summary>
    /// Creates a <see cref='Rectangle'/> that is inflated by the specified amount.
    /// </summary>
    public static Rectangle Inflate( Rectangle rect, int x, int y )
    {
        rect.Inflate( x, y );

        return rect;
    }

    // ========================================================================

    /// <summary>
    /// Checks for overlap between this rectangle and the specified rectangle.
    /// </summary>
    /// <param name="r"> the other <see cref="Rectangle"/> </param>
    /// <returns> whether this rectangle overlaps the other rectangle.  </returns>
    public bool Overlaps( Rectangle r )
    {
        return ( X < ( r.X + r.Width ) ) && ( ( X + Width ) > r.X ) && ( Y < ( r.Y + r.Height ) ) && ( ( Y + Height ) > r.Y );
    }

    // ========================================================================

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

        return _empty;
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

    // ========================================================================

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

    // ========================================================================

    /// <summary>
    /// Adjusts the location of this rectangle by the specified amount.
    /// </summary>
    public void AdjustLocation( Point pos )
    {
        AdjustLocation( pos.X, pos.Y );
    }

    /// <summary>
    /// Adjusts the location of this rectangle by the specified amount.
    /// </summary>
    public void AdjustLocation( int x, int y )
    {
        X += x;
        Y += y;
    }

    // ========================================================================

    /// <summary>
    /// Sets the values of the given rectangle to this rectangle.
    /// </summary>
    /// <param name="rect"> the other rectangle </param>
    /// <returns> this rectangle for chaining  </returns>
    public Rectangle Set( Rectangle rect )
    {
        X      = rect.X;
        Y      = rect.Y;
        Width  = rect.Width;
        Height = rect.Height;

        return this;
    }

    // ========================================================================

    /// <summary>
    /// Merges this rectangle with the other rectangle.
    /// The rectangle should not have negative width or negative height.
    /// </summary>
    /// <param name="rect"> the other rectangle </param>
    /// <returns> this rectangle for chaining  </returns>
    public Rectangle Merge( Rectangle rect )
    {
        var minX = Math.Min( X, rect.X );
        var maxX = Math.Max( X + Width, rect.X + rect.Width );

        X     = minX;
        Width = maxX - minX;

        var minY = Math.Min( Y, rect.Y );
        var maxY = Math.Max( Y + Height, rect.Y + rect.Height );

        Y      = minY;
        Height = maxY - minY;

        return this;
    }

    /// <summary>
    /// Merges this rectangle with a point.
    /// The rectangle should not have negative width or negative height.
    /// </summary>
    /// <param name="x"> the x coordinate of the point </param>
    /// <param name="y"> the y coordinate of the point </param>
    /// <returns> this rectangle for chaining  </returns>
    public Rectangle Merge( float x, float y )
    {
        var minX = Math.Min( X, x );
        var maxX = Math.Max( X + Width, x );

        X     = minX;
        Width = maxX - minX;

        var minY = Math.Min( Y, y );
        var maxY = Math.Max( Y + Height, y );

        Y      = minY;
        Height = maxY - minY;

        return this;
    }

    /// <summary>
    /// Merges this rectangle with a point.
    /// The rectangle should not have negative width or negative height.
    /// </summary>
    /// <param name="vec"> the vector describing the point </param>
    /// <returns> this rectangle for chaining  </returns>
    public Rectangle Merge( Vector2 vec )
    {
        return Merge( vec.X, vec.Y );
    }

    /// <summary>
    /// Merges this rectangle with a list of points.
    /// The rectangle should not have negative width or negative height.
    /// </summary>
    /// <param name="vecs"> the vectors describing the points </param>
    /// <returns> this rectangle for chaining  </returns>
    public Rectangle Merge( IEnumerable< Vector2 > vecs )
    {
        var minX = X;
        var maxX = X + Width;
        var minY = Y;
        var maxY = Y + Height;

        foreach ( var v in vecs )
        {
            minX = Math.Min( minX, v.X );
            maxX = Math.Max( maxX, v.X );
            minY = Math.Min( minY, v.Y );
            maxY = Math.Max( maxY, v.Y );
        }

        X      = minX;
        Width  = maxX - minX;
        Y      = minY;
        Height = maxY - minY;

        return this;
    }

    // ========================================================================

    /// <summary>
    /// Calculates the aspect ratio ( width / height ) of this rectangle
    /// </summary>
    /// <returns>
    /// The aspect ratio of this rectangle.
    /// Returns Float.NaN if height is 0 to avoid ArithmeticException.
    /// </returns>
    public float GetAspectRatio()
    {
        return Height == 0 ? float.NaN : Width / Height;
    }

    // ========================================================================

    /// <summary>
    /// Calculates the center of the rectangle. Results are located in the given Vector2
    /// </summary>
    /// <param name="vector"> the Vector2 to use </param>
    /// <returns> the given vector with results stored inside  </returns>
    public Vector2 GetCenter( Vector2 vector )
    {
        vector.X = X + ( Width / 2 );
        vector.Y = Y + ( Height / 2 );

        return vector;
    }

    /// <summary>
    /// Moves this rectangle so that its center point is located at a given position
    /// </summary>
    /// <param name="x"> the position's x </param>
    /// <param name="y"> the position's y </param>
    /// <returns> this for chaining  </returns>
    public Rectangle SetCenter( float x, float y )
    {
        SetPosition( x - ( Width / 2 ), y - ( Height / 2 ) );

        return this;
    }

    /// <summary>
    /// Moves this rectangle so that its center point is located at a given position
    /// </summary>
    /// <param name="position"> the position </param>
    /// <returns> this for chaining  </returns>
    public Rectangle SetCenter( Vector2 position )
    {
        SetPosition( position.X - ( Width / 2 ), position.Y - ( Height / 2 ) );

        return this;
    }

    // ========================================================================

    /// <summary>
    /// Fits this rectangle around another rectangle while maintaining aspect
    /// ratio. This scales and centers the rectangle to the other rectangle
    /// (e.g. Having a camera translate and scale to show a given area)
    /// </summary>
    /// <param name="rect"> the other rectangle to fit this rectangle around </param>
    /// <returns> this rectangle for chaining </returns>
    /// <see cref="Scaling "/>
    public Rectangle FitOutside( Rectangle rect )
    {
        var ratio = GetAspectRatio();

        if ( ratio > rect.GetAspectRatio() )
        {
            // Wider than tall
            SetSize( rect.Height * ratio, rect.Height );
        }
        else
        {
            // Taller than wide
            SetSize( rect.Width, rect.Width / ratio );
        }

        SetPosition( ( rect.X + ( rect.Width / 2 ) ) - ( Width / 2 ), ( rect.Y + ( rect.Height / 2 ) ) - ( Height / 2 ) );

        return this;
    }

    /// <summary>
    /// Fits this rectangle into another rectangle while maintaining aspect ratio.
    /// This scales and centers the rectangle to the other rectangle (e.g. Scaling
    /// a texture within a arbitrary cell without squeezing)
    /// </summary>
    /// <param name="rect"> the other rectangle to fit this rectangle inside </param>
    /// <returns> this rectangle for chaining </returns>
    /// <see cref="Scaling "/>
    public Rectangle FitInside( Rectangle rect )
    {
        var ratio = GetAspectRatio();

        if ( ratio < rect.GetAspectRatio() )
        {
            // Taller than wide
            SetSize( rect.Height * ratio, rect.Height );
        }
        else
        {
            // Wider than tall
            SetSize( rect.Width, rect.Width / ratio );
        }

        SetPosition( ( rect.X + ( rect.Width / 2 ) ) - ( Width / 2 ), ( rect.Y + ( rect.Height / 2 ) ) - ( Height / 2 ) );

        return this;
    }

    // ========================================================================

    /// <summary>
    /// Converts this <code>Rectangle</code> to a string in the
    /// format <code>[x,y,width,height]</code>.
    /// </summary>
    /// <returns> a string representation of this object.</returns>
    public override string ToString()
    {
        return "[" + X + "," + Y + "," + Width + "," + Height + "]";
    }

    /// <summary>
    /// Sets this {@code Rectangle} to the value represented by the
    /// specified string according to the format of <see cref="ToString()"/>.
    /// </summary>
    /// <param name="v"> the string. </param>
    /// <returns> this rectangle for chaining  </returns>
    public Rectangle FromString( string v )
    {
        var s0 = v.IndexOf( ',', 1 );
        var s1 = v.IndexOf( ',', s0 + 1 );
        var s2 = v.IndexOf( ',', s1 + 1 );

        // Note: v[ ^1 ] is equivalent to v[ v.Length - 1 ]

        if ( ( s0 != -1 ) && ( s1 != -1 ) && ( s2 != -1 ) && ( v[ 0 ] == '[' ) && ( v[ ^1 ] == ']' ) )
        {
            try
            {
                var x      = float.Parse( v.Substring( 1, s0 - 1 ) );
                var y      = float.Parse( v.Substring( s0 + 1, s1 - ( s0 + 1 ) ) );
                var width  = float.Parse( v.Substring( s1 + 1, s2 - ( s1 + 1 ) ) );
                var height = float.Parse( v.Substring( s2 + 1, v.Length - 1 - ( s2 + 1 ) ) );

                return Set( x, y, width, height );
            }
            catch ( FormatException )
            {
                throw new RuntimeException( "Malformed Rectangle: " + v );
            }
        }

        throw new RuntimeException( "Malformed Rectangle: " + v );
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public float Area()
    {
        return Width * Height;
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public float Perimeter()
    {
        return 2 * ( Width + Height );
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        const int PRIME = 31;

        //TODO: Do this properly without referencing Tmp.
        var result = PRIME + NumberUtils.FloatToRawIntBits( Tmp.X );
        result = ( PRIME * result ) + NumberUtils.FloatToRawIntBits( Tmp.Y );
        result = ( PRIME * result ) + NumberUtils.FloatToRawIntBits( Tmp.Width );
        result = ( PRIME * result ) + NumberUtils.FloatToRawIntBits( Tmp.Height );

        return result;
    }

    /// <inheritdoc />
    public bool Equals( Rectangle? other )
    {
        return ReferenceEquals( this, other );
    }

    /// <summary>
    /// Tests whether <paramref name="obj"/> is a <see cref='Rectangle'/> with the same location
    /// and size of this Rectangle.
    /// </summary>
    public override bool Equals( object? obj )
    {
        if ( this == obj )
        {
            return true;
        }

        if ( obj == null )
        {
            return false;
        }

        if ( GetType() != obj.GetType() )
        {
            return false;
        }

        var other = ( Rectangle )obj;

        if ( NumberUtils.FloatToRawIntBits( Height ) != NumberUtils.FloatToRawIntBits( other.Height ) )
        {
            return false;
        }

        if ( NumberUtils.FloatToRawIntBits( Width ) != NumberUtils.FloatToRawIntBits( other.Width ) )
        {
            return false;
        }

        if ( NumberUtils.FloatToRawIntBits( X ) != NumberUtils.FloatToRawIntBits( other.X ) )
        {
            return false;
        }

        return NumberUtils.FloatToRawIntBits( Y ) == NumberUtils.FloatToRawIntBits( other.Y );
    }
}