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
/// Wheel joint definition. This requires defining a line of motion using an axis
/// and an anchor point. The definition uses local anchor points and a local axis
/// so that the initial configuration can violate the constraint slightly. The
/// joint translation is zero when the local anchor points coincide in world space.
/// Using local anchors and a local axis helps when saving and loading a game.
/// </summary>
[PublicAPI]
public class WheelJointDef : JointDef
{
    /// The local anchor point relative to body1's origin. 
    public readonly Vector2 LocalAnchorA = new();

    /// The local anchor point relative to body2's origin. 
    public readonly Vector2 LocalAnchorB = new();

    /// The local translation axis in body1. 
    public readonly Vector2 LocalAxisA = new( 1, 0 );

    /// Enable/disable the joint motor. 
    public bool EnableMotor;

    /// The maximum motor torque, usually in N-m. 
    public float MaxMotorTorque;

    /// The desired motor speed in radians per second. 
    public float MotorSpeed;

    /// Suspension frequency, zero indicates no suspension 
    public float FrequencyHz = 2;

    /// Suspension damping ratio, one indicates critical damping 
    public float DampingRatio = 0.7f;

    // ========================================================================
    
    public WheelJointDef()
    {
        Type = JointType.WheelJoint;
    }

    public void Initialize( Body bodyA, Body bodyB, Vector2 anchor, Vector2 axis )
    {
        this.BodyA = bodyA;
        this.BodyB = bodyB;
        LocalAnchorA.Set( bodyA.GetLocalPoint( anchor ) );
        LocalAnchorB.Set( bodyB.GetLocalPoint( anchor ) );
        LocalAxisA.Set( bodyA.GetLocalVector( axis ) );
    }
}

// ============================================================================
// ============================================================================
