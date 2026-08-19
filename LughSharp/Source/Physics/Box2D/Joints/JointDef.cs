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
public class JointDef
{
    [PublicAPI]
    public enum JointType
    {
        Unknown,
        RevoluteJoint,
        PrismaticJoint,
        DistanceJoint,
        PulleyJoint,
        MouseJoint,
        GearJoint,
        WheelJoint,
        WeldJoint,
        FrictionJoint,
        RopeJoint,
        MotorJoint
    }

    public static readonly JointType[] ValueTypes = new[]
    {
        JointType.Unknown,
        JointType.RevoluteJoint,
        JointType.PrismaticJoint,
        JointType.DistanceJoint,
        JointType.PulleyJoint,
        JointType.MouseJoint,
        JointType.GearJoint,
        JointType.WheelJoint,
        JointType.WeldJoint,
        JointType.FrictionJoint,
        JointType.RopeJoint,
        JointType.MotorJoint
    };

    /// <summary>
    /// The joint type is set automatically for concrete joint types.
    /// </summary>
    public JointType Type = JointType.Unknown;

    /// <summary>
    /// The first attached body.
    /// </summary>
    public Body BodyA = null!;

    /// <summary>
    /// The second attached body
    /// </summary>
    public Body BodyB = null!;

    /// <summary>
    /// Set this flag to true if the attached bodies should collide.
    /// </summary>
    public bool CollideConnected;
}

// ============================================================================
// ============================================================================
