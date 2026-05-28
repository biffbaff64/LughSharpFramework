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

namespace LughSharp.Source.Maths.Collision;

/// <summary>
/// A simplified Rectangle class, providing only the minimum required properties.
/// </summary>
[PublicAPI]
public class Box
{
    public float X      { get; set; }
    public float Y      { get; set; }
    public float Width  { get; set; }
    public float Height { get; set; }

    // ========================================================================

    /// <summary>
    /// Initializes a new instance of the Box class with all coordinates and dimensions
    /// set to zero.
    /// </summary>
    public Box()
    {
        X      = 0;
        Y      = 0;
        Width  = 0;
        Height = 0;
    }

    /// <summary>
    /// Constructs a new Box with the specified position and size.
    /// </summary>
    /// <param name="x">The x-coordinate of the top-left corner of the box.</param>
    /// <param name="y">The y-coordinate of the top-left corner of the box.</param>
    /// <param name="width">The width of the box.</param>
    /// <param name="height">The height of the box.</param>
    public Box( float x, float y, float width, float height )
    {
        X      = x;
        Y      = y;
        Width  = width;
        Height = height;
    }

    /// <summary>
    /// Determines whether the specified point is contained within the bounds of the box.
    /// </summary>
    /// <param name="x">The x-coordinate of the point to test for containment.</param>
    /// <param name="y">The y-coordinate of the point to test for containment.</param>
    /// <returns>
    /// true if the point defined by the specified coordinates is within the box; otherwise, false.
    /// </returns>
    public bool Contains( float x, float y )
    {
        return x >= X && x <= X + Width && y >= Y && y <= Y + Height;
    }

    /// <summary>
    /// Determines whether the specified point is contained within the current region.
    /// </summary>
    /// <param name="point">The point to test for containment within the region.</param>
    /// <returns>true if the specified point is contained within the region; otherwise, false.</returns>
    public bool Contains( Vector2 point )
    {
        return Contains( point.X, point.Y );
    }

    /// <summary>
    /// Determines whether the current box completely contains the specified box.
    /// </summary>
    /// <param name="other">The box to test for containment within the current box.</param>
    /// <returns>true if the current box entirely contains the specified box; otherwise, false.</returns>
    public bool Contains( Box other )
    {
        return X <= other.X && Y <= other.Y && X + Width >= other.X + other.Width
            && Y + Height >= other.Y + other.Height;
    }

    /// <summary>
    /// Determines whether the current box completely contains the specified rectangle.
    /// </summary>
    /// <param name="other">The rectangle to test for containment within the current box.</param>
    /// <returns>
    /// <c>true</c> if the current box entirely contains the specified rectangle; otherwise, <c>false</c>.
    /// </returns>
    public bool Contains( Rectangle other )
    {
        return Contains( other.X, other.Y ) && Contains( other.X + other.Width, other.Y + other.Height );
    }

    /// <summary>
    /// Determines whether this box intersects with the specified box.
    /// </summary>
    /// <param name="other">The box to test for intersection with this box.</param>
    /// <returns>true if the boxes intersect; otherwise, false.</returns>
    public bool Intersects( Box other )
    {
        return X < other.X + other.Width && X + Width > other.X && Y < other.Y + other.Height && Y + Height > other.Y;
    }

    /// <summary>
    /// Determines whether this rectangle intersects with the specified rectangle.
    /// </summary>
    /// <param name="other">The rectangle to test for intersection with this rectangle.</param>
    /// <returns>true if the rectangles intersect; otherwise, false.</returns>
    public bool Intersects( Rectangle other )
    {
        return Intersects( other.X, other.Y, other.Width, other.Height );
    }

    /// <summary>
    /// Determines whether the specified rectangle intersects with the current box. The
    /// intersection is determined by comparing the positions and dimensions of both rectangles.
    /// Rectangles that only touch at the edge or corner are not considered intersecting.
    /// </summary>
    /// <param name="x">
    /// The x-coordinate of the upper-left corner of the rectangle to test for intersection.
    /// </param>
    /// <param name="y">
    /// The y-coordinate of the upper-left corner of the rectangle to test for intersection.
    /// </param>
    /// <param name="width">
    /// The width of the rectangle to test for intersection. <b>Must be non-negative.</b>
    /// </param>
    /// <param name="height">
    /// The height of the rectangle to test for intersection. <b>Must be non-negative.</b>
    /// </param>
    /// <returns>
    /// <c>true</c> if the specified rectangle intersects with the current box; otherwise,
    /// <c>false</c>.
    /// </returns>
    public bool Intersects( float x, float y, float width, float height )
    {
        return x < X + Width && x + width > X && y < Y + Height && y + height > Y;
    }
}

// ============================================================================
// ============================================================================