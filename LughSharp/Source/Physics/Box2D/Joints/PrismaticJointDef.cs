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

namespace LughSharp.Source.Physics.Box2D.Joints;

/// <summary>
/// Prismatic joint definition. This requires defining a line of motion using an axis
/// and an anchor point. The definition uses local anchor points and a local axis so
/// that the initial configuration can violate the constraint slightly.
/// <para>
/// The joint translation is zero when the local anchor points coincide in world space.
/// Using local anchors and a local axis helps when saving and loading a game.
/// </para>
/// <br/><para>
/// <b>
/// WARNING: At least one body should by dynamic with a non-fixed rotation.
/// </b>
/// </para>
/// </summary>
[PublicAPI]
public class PrismaticJointDef : JointDef
{
    /// <summary>
    /// The local anchor point relative to body1's origin. 
    /// </summary>
    public readonly Vector2 LocalAnchorA = new();

    /// <summary>
    /// The local anchor point relative to body2's origin. 
    /// </summary>
    public readonly Vector2 LocalAnchorB = new();

    /// <summary>
    /// The local translation axis in body1. 
    /// </summary>
    public readonly Vector2 LocalAxisA = new( 1, 0 );

    /// <summary>
    /// The constrained angle between the bodies: body2_angle - body1_angle. 
    /// </summary>
    public float ReferenceAngle { get; set; }

    /// <summary>
    /// Enable/disable the joint limit. 
    /// </summary>
    public bool EnableLimit { get; set; }

    /// <summary>
    /// The lower translation limit, usually in meters. 
    /// </summary>
    public float LowerTranslation { get; set; }

    /// <summary>
    /// The upper translation limit, usually in meters. 
    /// </summary>
    public float UpperTranslation { get; set; }

    /// <summary>
    /// Enable/disable the joint motor. 
    /// </summary>
    public bool EnableMotor { get; set; }

    /// <summary>
    /// The maximum motor torque, usually in N-m. 
    /// </summary>
    public float MaxMotorForce { get; set; }

    /// <summary>
    /// The desired motor speed in radians per second. 
    /// </summary>
    public float MotorSpeed { get; set; }

    // ========================================================================
    
    public PrismaticJointDef()
    {
        Type = JointType.PrismaticJoint;
    }

    /// <summary>
    /// Initialize the bodies, anchors, axis, and reference angle using the world
    /// anchor and world axis.
    /// </summary>
    public void Initialize( Body bodyA, Body bodyB, Vector2 anchor, Vector2 axis )
    {
        this.BodyA = bodyA;
        this.BodyB = bodyB;
        LocalAnchorA.Set( bodyA.GetLocalPoint( anchor ) );
        LocalAnchorB.Set( bodyB.GetLocalPoint( anchor ) );
        LocalAxisA.Set( bodyA.GetLocalVector( axis ) );
        ReferenceAngle = bodyB.GetAngle() - bodyA.GetAngle();
    }
}

// ============================================================================
// ============================================================================
