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

using Vector2 = LughSharp.Source.Maths.Vector2;

namespace LughSharp.Physics2D.Source.Box2D;

/// <summary>
/// A body definition holds all the data needed to construct a rigid body. You can
/// safely re-use body definitions.
/// </summary>
[PublicAPI]
public class BodyDef
{
    /// <summary>
    /// The body type, mirroring libgdx <c>BodyDef.BodyType</c> (and Box2D's <c>b2BodyType</c>).
    /// <para>
    /// static   : zero mass, zero velocity, may be manually moved.
    /// kinematic : zero mass, non-zero velocity set by user, moved by the solver.
    /// dynamic  : positive mass, velocity determined by forces.
    /// </para>
    /// The underlying integer values (0/1/2) match Box2D exactly.
    /// </summary>
    [PublicAPI]
    public enum BodyType
    {
        Static    = 0,
        Kinematic = 1,
        Dynamic   = 2,
    }

    /// <summary>
    /// The body type: static, kinematic, or dynamic. Note: if a dynamic body
    /// would have zero mass, the mass is set to one.
    /// </summary>
    public BodyType Type { get; set; } = BodyType.Static;

    /// <summary>
    /// The world position of the body. Avoid creating bodies at the origin
    /// since this can lead to many overlapping shapes.
    /// </summary>
    public Vector2 Position { get; set; } = new();

    /// <summary>
    /// The world angle of the body in radians.
    /// </summary>
    public float Angle { get; set; }

    /// <summary>
    /// The linear velocity of the body's origin in world co-ordinates.
    /// </summary>
    public Vector2 LinearVelocity { get; set; } = new();

    /// <summary>
    /// The angular velocity of the body.
    /// </summary>
    public float AngularVelocity { get; set; }

    /// <summary>
    /// Linear damping is used to reduce the linear velocity. Units are 1/time.
    /// </summary>
    public float LinearDamping { get; set; }

    /// <summary>
    /// Angular damping is used to reduce the angular velocity. Units are 1/time.
    /// </summary>
    public float AngularDamping { get; set; }

    /// <summary>
    /// Set this flag to false if this body should never fall asleep.
    /// </summary>
    public bool AllowSleep { get; set; } = true;

    /// <summary>
    /// Is this body initially awake or sleeping?
    /// </summary>
    public bool Awake { get; set; } = true;

    /// <summary>
    /// Should this body be prevented from rotating? Useful for characters.
    /// </summary>
    public bool FixedRotation { get; set; }

    /// <summary>
    /// Is this a fast moving body that should be prevented from tunneling
    /// through other moving bodies? (continuous collision detection)
    /// </summary>
    public bool Bullet { get; set; }

    /// <summary>
    /// Does this body start out active?
    /// </summary>
    public bool Active { get; set; } = true;

    /// <summary>
    /// Scale the gravity applied to this body.
    /// </summary>
    public float GravityScale { get; set; } = 1.0f;
}

// ============================================================================
// ============================================================================
