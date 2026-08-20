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
/// Pulley joint definition. This requires two ground anchors, two dynamic body
/// anchor points, max lengths for each side, and a pulley ratio.
/// </summary>
[PublicAPI]
public class PulleyJointDef : JointDef
{
    /// <summary>
    /// The first ground anchor in world coordinates. This point never moves. 
    /// </summary>
    public readonly Vector2 GroundAnchorA = new( -1, 1 );

    /// <summary>
    /// The second ground anchor in world coordinates. This point never moves. 
    /// </summary>
    public readonly Vector2 GroundAnchorB = new( 1, 1 );

    /// <summary>
    /// The local anchor point relative to bodyA's origin. 
    /// </summary>
    public readonly Vector2 LocalAnchorA = new( -1, 0 );

    /// <summary>
    /// The local anchor point relative to bodyB's origin. 
    /// </summary>
    public readonly Vector2 LocalAnchorB = new( 1, 0 );

    /// <summary>
    /// The a reference length for the segment attached to bodyA. 
    /// </summary>
    public float LengthA { get; set; }

    /// <summary>
    /// The a reference length for the segment attached to bodyB. 
    /// </summary>
    public float LengthB { get; set; }

    /// <summary>
    /// The pulley ratio, used to simulate a block-and-tackle. 
    /// </summary>
    public float Ratio { get; set; } = 1;

    // ========================================================================

    private static readonly float _minPulleyLength = 2.0f;

    // ========================================================================

    public PulleyJointDef()
    {
        Type             = JointType.PulleyJoint;
        CollideConnected = true;
    }

    /// <summary>
    /// Initialize the bodies, anchors, lengths, max lengths, and ratio using the
    /// world anchors. 
    /// </summary>
    public void Initialize( Body bodyA, Body bodyB, Vector2 groundAnchorA, Vector2 groundAnchorB, Vector2 anchorA,
                            Vector2 anchorB,
                            float ratio )
    {
        this.BodyA = bodyA;
        this.BodyB = bodyB;
        this.GroundAnchorA.Set( groundAnchorA );
        this.GroundAnchorB.Set( groundAnchorB );
        this.LocalAnchorA.Set( bodyA.GetLocalPoint( anchorA ) );
        this.LocalAnchorB.Set( bodyB.GetLocalPoint( anchorB ) );
        LengthA    = anchorA.Distance( groundAnchorA );
        LengthB    = anchorB.Distance( groundAnchorB );
        this.Ratio = ratio;
    }
}

// ============================================================================
// ============================================================================
