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

using System.Numerics;

using JetBrains.Annotations;

namespace Extensions.Source.Box2D;

/// <summary>
/// A body definition holds all the data needed to construct a rigid body. You can safely
/// re-use body definitions. Shapes are added to a body after construction.
/// </summary>
[PublicAPI]
public class BodyDef
{
    /// <summary>
    /// The body type.
    /// </summary>
    [PublicAPI]
    public enum BodyType
    {
        /// <summary>
        /// zero mass, zero velocity, may be manually moved
        /// </summary>
        StaticBody = 0,

        /// <summary>
        /// zero mass, non-zero velocity set by user,cmoved by solver
        /// </summary>
        KinematicBody = 1,

        /// <summary>
        /// positive mass, non-zero velocity determined by forces, moved by solver
        /// </summary>
        DynamicBody = 2,
    }

    /// <summary>
    /// The body type: static, kinematic, or dynamic.
    /// <para>
    /// Note: if a dynamic body would have zero mass, the mass is set to one. 
    /// </para>
    /// </summary>
    public BodyType Type = BodyType.StaticBody;

    /// <summary>
    /// The world position of the body. Avoid creating bodies at the origin since this can lead
    /// to many overlapping shapes. 
    /// </summary>
    public readonly Vector2 Position;

    /// <summary>
    /// The world angle of the body in radians. 
    /// </summary>
    public float Angle;

    /// <summary>
    /// The linear velocity of the body's origin in world co-ordinates. 
    /// </summary>
    public readonly Vector2 LinearVelocity;

    /// <summary>
    /// The angular velocity of the body. 
    /// </summary>
    public float AngularVelocity;

    /// <summary>
    /// Linear damping is use to reduce the linear velocity. The damping parameter can be
    /// larger than 1.0f but the damping effect becomes sensitive to the time step when
    /// the damping parameter is large.
    /// </summary>
    public float LinearDamping;

    /// <summary>
    /// Angular damping is use to reduce the angular velocity. The damping parameter can be
    /// larger than 1.0f but the damping effect becomes sensitive to the time step when the
    /// damping parameter is large.
    /// </summary>
    public float AngularDamping;

    /// <summary>
    /// Set this flag to false if this body should never fall asleep.
    /// Note that this increases CPU usage. 
    /// </summary>
    public bool AllowSleep = true;

    /// <summary>
    /// Is this body initially awake or sleeping? 
    /// </summary>
    public bool Awake = true;

    /// <summary>
    /// Should this body be prevented from rotating? Useful for characters. 
    /// </summary>
    public bool FixedRotation;

    /// <summary>
    /// Is this a fast moving body that should be prevented from tunneling through other
    /// moving bodies? Note that all bodies are prevented from tunneling through kinematic
    /// and static bodies.This setting is only considered on dynamic bodies.
    /// <para>
    /// WARNING You should use this flag sparingly since it increases processing time.
    /// </para>
    /// </summary>
    public bool Bullet;

    /// <summary>
    /// Does this body start out active? 
    /// </summary>
    public bool Active = true;

    /// <summary>
    /// Scale the gravity applied to this body. 
    /// </summary>
    public float GravityScale = 1;
}

// ============================================================================
// ============================================================================