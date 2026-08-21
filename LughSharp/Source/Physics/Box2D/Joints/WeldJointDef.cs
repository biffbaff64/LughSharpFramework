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
public class WeldJointDef : JointDef
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
    /// The mass-spring-damper frequency in Hertz. Rotation only. Disable softness with a value of 0. 
    /// </summary>
    public float FrequencyHz { get; set; }

    /// <summary>
    /// The damping ratio. 0 = no damping, 1 = critical damping. 
    /// </summary>
    public float DampingRatio { get; set; }

    // ========================================================================

    public WeldJointDef()
    {
        Type = JointType.WeldJoint;
    }

    /// <summary>
    /// Initialize the bodies, anchors, and reference angle using a world anchor point. 
    /// </summary>
    public void Initialize( Body body1, Body body2, Vector2 anchor )
    {
        this.BodyA = body1;
        this.BodyB = body2;
        this.LocalAnchorA.Set( body1.GetLocalPoint( anchor ) );
        this.LocalAnchorB.Set( body2.GetLocalPoint( anchor ) );
        ReferenceAngle = body2.GetAngle() - body1.GetAngle();
    }
}

// ============================================================================
// ============================================================================
