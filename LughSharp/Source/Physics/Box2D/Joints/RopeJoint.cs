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
/// A rope joint enforces a maximum distance between two points on two bodies. It
/// has no other effect.
/// <para>
/// <b>
/// Warning: if you attempt to change the maximum length during the simulation you
/// will get some non-physical behavior. A model that would allow you to dynamically
/// modify the length would have some sponginess, so I chose not to implement it that
/// way. See b2DistanceJoint if you want to dynamically control length.
/// </b>
/// </para>
/// </summary>
/// <param name="world"> The physics world. </param>
/// <param name="addr"> The native address. ( To be removed ) </param>
[PublicAPI]
public class RopeJoint( World world, long addr ) : Joint( world, addr )
{
    private readonly float[] _tmp          = new float[ 2 ];
    private readonly Vector2 _localAnchorA = new();
    private readonly Vector2 _localAnchorB = new();

    // ========================================================================

    /// <summary>
    /// Gets the local anchor point A in body A's frame.
    /// </summary>
    public Vector2 GetLocalAnchorA()
    {
        jniGetLocalAnchorA( Addr, _tmp );
        
        _localAnchorA.Set( _tmp[ 0 ], _tmp[ 1 ] );

        return _localAnchorA;
    }

    /// <summary>
    /// Gets the local anchor point B in body B's frame.
    /// </summary>
    public Vector2 GetLocalAnchorB()
    {
        jniGetLocalAnchorB( Addr, _tmp );
        
        _localAnchorB.Set( _tmp[ 0 ], _tmp[ 1 ] );

        return _localAnchorB;
    }

    /// <summary>
    /// The maximum length of the rope.
    /// </summary>
    public float MaxRopeLength
    {
        get => jniGetMaxLength( Addr );
        set => jniSetMaxLength( Addr, value );
    }

    // ========================================================================
    // ========================================================================
    
    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLocalAnchorA( long addr, float[] anchor );
    /*
        b2RopeJoint* joint = (b2RopeJoint*)addr;
        anchor[0] = joint->GetLocalAnchorA().x;
        anchor[1] = joint->GetLocalAnchorA().y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLocalAnchorB( long addr, float[] anchor );
    /*
        b2RopeJoint* joint = (b2RopeJoint*)addr;
        anchor[0] = joint->GetLocalAnchorB().x;
        anchor[1] = joint->GetLocalAnchorB().y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetMaxLength( long addr );
    /*
        b2RopeJoint* rope = (b2RopeJoint*)addr;
        return rope->GetMaxLength();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetMaxLength( long addr, float length );
    /*
        b2RopeJoint* rope = (b2RopeJoint*)addr;
        rope->SetMaxLength(length);
    */
}

// ============================================================================
// ============================================================================
