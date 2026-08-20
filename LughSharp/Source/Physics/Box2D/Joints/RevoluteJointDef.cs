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
/// Revolute joint definition. This requires defining an anchor point where the
/// bodies are joined. The definition uses local anchor points so that the initial
/// configuration can violate the constraint slightly. You also need to specify the
/// initial relative angle for joint limits. This helps when saving and loading a
/// game. The local anchor points are measured from the body's origin rather than
/// the center of mass because:
/// <br/>
/// 1. You might not know where the center of mass will be.
/// <br/>
/// 2. If you add/remove shapes from a body and recompute the mass, the joints will be broken.
/// <br/>
/// </summary>
[PublicAPI]
public class RevoluteJointDef : JointDef
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
    /// The body2 angle minus body1 angle in the reference state (radians). 
    /// </summary>
    public float ReferenceAngle { get; set; }

    /// <summary>
    /// A flag to enable joint limits. 
    /// </summary>
    public bool EnableLimit { get; set; }

    /// <summary>
    /// The lower angle for the joint limit (radians). 
    /// </summary>
    public float LowerAngle { get; set; }

    /// <summary>
    /// The upper angle for the joint limit (radians). 
    /// </summary>
    public float UpperAngle { get; set; }

    /// <summary>
    /// A flag to enable the joint motor. 
    /// </summary>
    public bool EnableMotor { get; set; }

    /// <summary>
    /// The desired motor speed. Usually in radians per second. 
    /// </summary>
    public float MotorSpeed { get; set; }

    /// <summary>
    /// The maximum motor torque used to achieve the desired motor speed. Usually in N-m. 
    /// </summary>
    public float MaxMotorTorque { get; set; }

    // ========================================================================

    public RevoluteJointDef()
    {
        Type = JointType.RevoluteJoint;
    }

    /// <summary>
    /// Initialize the bodies, anchors, and reference angle using a world anchor point. 
    /// </summary>
    public void Initialize( Body bodyA, Body bodyB, Vector2 anchor )
    {
        this.BodyA = bodyA;
        this.BodyB = bodyB;
        LocalAnchorA.Set( bodyA.GetLocalPoint( anchor ) );
        LocalAnchorB.Set( bodyB.GetLocalPoint( anchor ) );
        ReferenceAngle = bodyB.GetAngle() - bodyA.GetAngle();
    }
}

// ============================================================================
// ============================================================================
