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
/// Friction joint definition.
/// </summary>
[PublicAPI]
public class FrictionJointDef : JointDef
{
    /// <summary>
    /// The local anchor point relative to bodyA's origin.
    /// </summary>
    public readonly Vector2 LocalAnchorA = new();

    /// <summary>
    /// The local anchor point relative to bodyB's origin.
    /// </summary>
    public readonly Vector2 LocalAnchorB = new();

    /// <summary>
    /// The maximum friction force in N.
    /// </summary>
    public float MaxForce { get; set; }

    /// <summary>
    /// The maximum friction torque in N-m.
    /// </summary>
    public float MaxTorque { get; set; }

    // ========================================================================
    
    public FrictionJointDef()
    {
        Type = JointType.FrictionJoint;
    }

    /// <summary>
    /// Initialize the bodies, anchors, axis, and reference angle using the
    /// world anchor and world axis.
    /// </summary>
    public void Initialize( Body bodyA, Body bodyB, Vector2 anchor )
    {
        this.BodyA = bodyA;
        this.BodyB = bodyB;
        LocalAnchorA.Set( bodyA.GetLocalPoint( anchor ) );
        LocalAnchorB.Set( bodyB.GetLocalPoint( anchor ) );
    }
}

// ============================================================================
// ============================================================================
