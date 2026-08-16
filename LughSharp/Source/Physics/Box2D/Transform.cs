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

namespace LughSharp.Source.Physics.Box2D;

/// <summary>
/// Encodes a rigid body's position and orientation (a rotation plus a translation).
/// Faithful port of libgdx <c>Transform</c>: the four floats are packed into
/// <see cref="Vals"/> as [posX, posY, cos, sin].
/// </summary>
[PublicAPI]
public class Transform
{
    public const int PosX = 0;
    public const int PosY = 1;
    public const int Cos  = 2;
    public const int Sin  = 3;

    /// <summary>The packed values: [posX, posY, cos(angle), sin(angle)].</summary>
    public float[] Vals { get; } = new float[ 4 ];

    // ========================================================================
    
    private readonly Vector2 _position    = new();
    private readonly Vector2 _orientation = new();
    
    // ========================================================================

    /// <summary>
    /// Default constructor. Creates a new <see cref="Transform"/> with no position
    /// and no rotation.
    /// </summary>
    public Transform()
    {
    }

    /// <summary>
    /// Creates a new <see cref="Transform"/> with the given position and angle.
    /// </summary>
    /// <param name="position"> The position of the transform. </param>
    /// <param name="angle"> The angle of the transform. </param>
    public Transform( Vector2 position, float angle )
    {
        SetPosition( position );
        SetRotation( angle );
    }

    /// <summary>
    /// Creates a new <see cref="Transform"/> with the given position and orientation.
    /// </summary>
    /// <param name="position"> The position of the transform. </param>
    /// <param name="orientation"> The orientation of the transform. </param>
    public Transform( Vector2 position, Vector2 orientation )
    {
        SetPosition( position );
        SetOrientation( orientation );
    }

    /// <summary>
    /// Transforms the given point (rotate then translate) in place and returns it.
    /// </summary>
    public Vector2 Mul( Vector2 v )
    {
        var x = Vals[ PosX ] + ( Vals[ Cos ] * v.X ) - ( Vals[ Sin ] * v.Y );
        var y = Vals[ PosY ] + ( Vals[ Sin ] * v.X ) + ( Vals[ Cos ] * v.Y );

        v.X = x;
        v.Y = y;

        return v;
    }

    /// <summary>
    /// The position of the body's origin. Note: the same instance is returned each call.
    /// </summary>
    public Vector2 GetPosition()
    {
        return _position.Set( Vals[ PosX ], Vals[ PosY ] );
    }

    /// <summary>
    /// Sets the position of the body's origin.
    /// </summary>
    /// <param name="position"> The position of the body's origin. </param>
    public void SetPosition( Vector2 position )
    {
        Vals[ PosX ] = position.X;
        Vals[ PosY ] = position.Y;
    }

    /// <summary>
    /// The rotation in radians.
    /// </summary>
    public float GetRotation()
    {
        return MathF.Atan2( Vals[ Sin ], Vals[ Cos ] );
    }

    /// <summary>
    /// Sets the rotation in radians.
    /// </summary>
    /// <param name="angle"> The rotation in radians. </param>
    public void SetRotation( float angle )
    {
        Vals[ Cos ] = MathF.Cos( angle );
        Vals[ Sin ] = MathF.Sin( angle );
    }

    /// <summary>
    /// The orientation (cos, sin). Note: the same instance is returned each call.
    /// </summary>
    public Vector2 GetOrientation()
    {
        return _orientation.Set( Vals[ Cos ], Vals[ Sin ] );
    }

    /// <summary>
    /// Sets the orientation (cos, sin).
    /// </summary>
    /// <param name="orientation"> The orientation (cos, sin). </param>
    public void SetOrientation( Vector2 orientation )
    {
        Vals[ Cos ] = orientation.X;
        Vals[ Sin ] = orientation.Y;
    }
}

// ============================================================================
// ============================================================================
