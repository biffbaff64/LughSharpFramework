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

using System.Runtime.InteropServices;

using JetBrains.Annotations;

using LughSharp.Source.Maths;

namespace LughSharp.Source.Physics.Box2D.Joints;

/// <summary>
/// The pulley joint is connected to two bodies and two fixed ground points. The
/// pulley supports a ratio such that:
/// <code>
/// length1 + ratio * length2 &lt;= constant
/// </code>
/// <br/>
/// The force transmitted is scaled by the ratio. The pulley also enforces
/// a maximum length limit on both sides. This is useful to prevent one side of
/// the pulley hitting the top.
/// </summary>
[PublicAPI]
public class PulleyJoint : Joint
{
    private readonly float[] _tmp           = new float[ 2 ];
    private readonly Vector2 _groundAnchorA = new(); 
    private readonly Vector2 _groundAnchorB = new();

    // ========================================================================

    public PulleyJoint( World world, long addr )
        : base( world, addr )
    {
    }

    public Vector2 GetGroundAnchorA()
    {
        jniGetGroundAnchorA( Addr, _tmp );

        _groundAnchorA.Set( _tmp[ 0 ], _tmp[ 1 ] );

        return _groundAnchorA;
    }

    public Vector2 GetGroundAnchorB()
    {
        jniGetGroundAnchorB( Addr, _tmp );

        _groundAnchorB.Set( _tmp[ 0 ], _tmp[ 1 ] );

        return _groundAnchorB;
    }

    /// <summary>
    /// Get the current length of the segment attached to body1.
    /// </summary>
    public float GetLength1()
    {
        return jniGetLength1( Addr );
    }

    /// <summary>
    /// Get the current length of the segment attached to body2.
    /// </summary>
    public float GetLength2()
    {
        return jniGetLength2( Addr );
    }

    /// <summary>
    /// Get the pulley ratio.
    /// </summary>
    public float GetRatio()
    {
        return jniGetRatio( Addr );
    }

    // ========================================================================
    // ========================================================================
    
    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetGroundAnchorA( long addr, float[] anchor );
    /*
        b2PulleyJoint* joint = (b2PulleyJoint*)addr;
        anchor[0] = joint->GetGroundAnchorA().x;
        anchor[1] = joint->GetGroundAnchorA().y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetGroundAnchorB( long addr, float[] anchor );
    /*
        b2PulleyJoint* joint = (b2PulleyJoint*)addr;
        anchor[0] = joint->GetGroundAnchorB().x;
        anchor[1] = joint->GetGroundAnchorB().y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetLength1( long addr );
    /*
        b2PulleyJoint* joint = (b2PulleyJoint*)addr;
        return joint->GetLengthA();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetLength2( long addr );
    /*
        b2PulleyJoint* joint = (b2PulleyJoint*)addr;
        return joint->GetLengthB();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetRatio( long addr );
    /*
        b2PulleyJoint* joint = (b2PulleyJoint*)addr;
        return joint->GetRatio();
    */
}

// ============================================================================
// ============================================================================
