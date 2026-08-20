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

[PublicAPI]
public class MotorJointDef : JointDef
{
    /// <summary>
    /// Position of bodyB minus the position of bodyA, in bodyA's frame, in meters.
    /// </summary>
    public readonly Vector2 LinearOffset = new();

    /// <summary>
    /// The bodyB angle minus bodyA angle in radians.
    /// </summary>
    public float AngularOffset { get; set; }

    /// <summary>
    /// The maximum motor force in N.
    /// </summary>
    public float MaxForce { get; set; } = 1.0f;

    /// <summary>
    /// The maximum motor torque in N-m.
    /// </summary>
    public float MaxTorque { get; set; } = 1.0f;

    /// <summary>
    /// Position correction factor in the range [0,1].
    /// </summary>
    public float CorrectionFactor { get; set; } = 0.3f;

    // ========================================================================

    public MotorJointDef()
    {
        Type = JointType.MotorJoint;
    }

    /// <summary>
    /// Initialize the bodies and offsets using the current transforms.
    /// </summary>
    public void Initialize( Body body1, Body body2 )
    {
        this.BodyA = body1;
        this.BodyB = body2;
        this.LinearOffset.Set( BodyA.GetLocalPoint( BodyB.GetPosition() ) );
        this.AngularOffset = BodyB.GetAngle() - BodyA.GetAngle();
    }
}

// ============================================================================
// ============================================================================
