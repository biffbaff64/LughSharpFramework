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
/// Rope joint definition. This requires two body anchor points and a maximum
/// lengths.
/// <br/>
/// <b>
/// Note: by default the connected objects will not collide. see collideConnected
/// in b2JointDef.
/// </b>
/// </summary>
[PublicAPI]
public class RopeJointDef : JointDef
{
    /// <summary>
    /// The local anchor point relative to bodyA's origin. 
    /// </summary>
    public readonly Vector2 LocalAnchorA = new( -1, 0 );

    /// <summary>
    /// The local anchor point relative to bodyB's origin. 
    /// </summary>
    public readonly Vector2 LocalAnchorB = new( 1, 0 );

    /// <summary>
    /// The maximum length of the rope. Warning: this must be larger than b2_linearSlop
    /// or the joint will have no effect. 
    /// </summary>
    public float MaxLength { get; set; }

    // ========================================================================

    public RopeJointDef()
    {
        Type = JointType.RopeJoint;
    }
}

// ============================================================================
// ============================================================================
