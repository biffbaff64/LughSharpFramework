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

using LughSharp.Source.Maths;

namespace LughSharp.Physics2D.Source.Box2D;

/// <summary>
/// Encodes a rigid body's position and orientation (a rotation plus a translation).
/// Faithful port of libgdx <c>Transform</c>: the four floats are packed into
/// <see cref="Vals"/> as [posX, posY, cos, sin].
/// </summary>
public class Transform
{
    public const int POS_X = 0;
    public const int POS_Y = 1;
    public const int COS   = 2;
    public const int SIN   = 3;

    /// <summary>The packed values: [posX, posY, cos(angle), sin(angle)].</summary>
    public float[] Vals { get; } = new float[4];

    private readonly Vector2 _position    = new();
    private readonly Vector2 _orientation = new();

    public Transform()
    {
    }

    public Transform(Vector2 position, float angle)
    {
        SetPosition(position);
        SetRotation(angle);
    }

    public Transform(Vector2 position, Vector2 orientation)
    {
        SetPosition(position);
        SetOrientation(orientation);
    }

    /// <summary>
    /// Transforms the given point (rotate then translate) in place and returns it.
    /// </summary>
    public Vector2 Mul(Vector2 v)
    {
        var x = Vals[ POS_X ] + (Vals[ COS ] * v.X) - (Vals[ SIN ] * v.Y);
        var y = Vals[ POS_Y ] + (Vals[ SIN ] * v.X) + (Vals[ COS ] * v.Y);

        v.X = x;
        v.Y = y;

        return v;
    }

    /// <summary>The position of the body's origin. Note: the same instance is returned each call.</summary>
    public Vector2 GetPosition()
    {
        return _position.Set(Vals[ POS_X ], Vals[ POS_Y ]);
    }

    public void SetPosition(Vector2 position)
    {
        Vals[ POS_X ] = position.X;
        Vals[ POS_Y ] = position.Y;
    }

    /// <summary>The rotation in radians.</summary>
    public float GetRotation()
    {
        return MathF.Atan2(Vals[ SIN ], Vals[ COS ]);
    }

    public void SetRotation(float angle)
    {
        Vals[ COS ] = MathF.Cos(angle);
        Vals[ SIN ] = MathF.Sin(angle);
    }

    /// <summary>The orientation (cos, sin). Note: the same instance is returned each call.</summary>
    public Vector2 GetOrientation()
    {
        return _orientation.Set(Vals[ COS ], Vals[ SIN ]);
    }

    public void SetOrientation(Vector2 orientation)
    {
        Vals[ COS ] = orientation.X;
        Vals[ SIN ] = orientation.Y;
    }
}

// ============================================================================
// ============================================================================
