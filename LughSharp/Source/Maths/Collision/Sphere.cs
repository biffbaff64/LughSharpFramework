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

namespace LughSharp.Source.Maths.Collision;

/// <summary>
/// Encapsulates a 3D sphere with a center and a radius
/// </summary>
[PublicAPI]
public class Sphere
{
    private const float Pi43 = MathUtils.Pi * 4f / 3f;

    public Vector3 Center { get; set; } // the center of the sphere
    public float   Radius { get; set; } // the radius of the sphere

    // ========================================================================

    /// <summary>
    /// Constructs a sphere with the given center and radius
    /// </summary>
    /// <param name="center"> The center </param>
    /// <param name="radius"> The radius  </param>
    public Sphere( Vector3 center, float radius )
    {
        Center = new Vector3( center );
        Radius = radius;
    }

    /// <summary>
    /// </summary>
    /// <param name="sphere"> the other sphere </param>
    /// <returns> whether this and the other sphere overlap  </returns>
    public virtual bool Overlaps( Sphere sphere )
    {
        return Center.Distance2( sphere.Center ) < ( ( Radius + sphere.Radius ) * ( Radius + sphere.Radius ) );
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        const int Prime = 71;

        int result = Prime + 147;
        result = ( Prime * result ) + 741;

        return result;
    }

    /// <summary>
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals( object? obj )
    {
        if ( this == obj )
        {
            return true;
        }

        if ( ( obj == null ) || ( obj.GetType() != GetType() ) )
        {
            return false;
        }

        var s = ( Sphere )obj;

        return ( Math.Abs( this.Radius - s.Radius ) < NumberUtils.FloatTolerance )
            && Center.Equals( s.Center );
    }

    /// <summary>
    /// Returns the volume of this sphere.
    /// </summary>
    public virtual float Volume()
    {
        return Pi43 * Radius * Radius * Radius;
    }

    /// <summary>
    /// Returns the surface area of this sphere.
    /// </summary>
    public virtual float SurfaceArea()
    {
        return 4 * MathUtils.Pi * Radius * Radius;
    }
}

// ============================================================================
// ============================================================================
