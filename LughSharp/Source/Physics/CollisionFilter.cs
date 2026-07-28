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

namespace LughSharp.Source.Physics;

/// <summary>
/// A Filter object used to filter collision pairs. This class is primarily intended for
/// use with Box2D, but can be used with other physics engines as well.
/// </summary>
[PublicAPI]
public class CollisionFilter
{
    /// <summary>
    /// The value to set the <c>Box2D FixtureDef.Filter.CategoryBits</c> property.
    /// </summary>
    public short BodyCategory { get; set; }

    /// <summary>
    /// The value to set the <c>Box2D FixtureDef.Filter.MaskBits</c> property.
    /// </summary>
    public short CollidesWith { get; set; }

    /// <summary>
    /// The value to set the <c>Box2D FixtureDef.IsFilter</c> property.
    /// </summary>
    public bool IsSensor { get; set; }

    // ========================================================================

    /// <summary>
    /// Creates a new CollisionFilter object.
    /// </summary>
    /// <param name="bodyCategory"> The value to set the <c>Box2D FixtureDef.Filter.CategoryBits</c> property. </param>
    /// <param name="collidesWith"> The value to set the <c>Box2D FixtureDef.Filter.MaskBits</c> property. </param>
    /// <param name="sensor"> The value to set the <c>Box2D FixtureDef.IsFilter</c> property. </param>
    public CollisionFilter( short bodyCategory, short collidesWith, bool sensor )
    {
        this.BodyCategory = bodyCategory;
        this.CollidesWith = collidesWith;
        this.IsSensor     = sensor;
    }
}

// ============================================================================
// ============================================================================
