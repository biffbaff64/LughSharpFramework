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
/// A weld joint essentially glues two bodies together. A weld joint may distort
/// somewhat because the island constraint solver is approximate.
/// </summary>
/// <param name="world"> The physics world. </param>
/// <param name="addr"> The native address. ( To be removed ) </param>
[PublicAPI]
public class WeldJoint( World world, long addr ) : Joint( world, addr )
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
    /// 
    /// </summary>
    public float ReferenceAngle => jniGetReferenceAngle( Addr );

    /// <summary>
    /// 
    /// </summary>
    public float Frequency
    {
        get => jniGetFrequency( Addr );
        set => jniSetFrequency( Addr, value );
    }
    
    /// <summary>
    /// 
    /// </summary>
    public float DampingRatio
    {
        get => jniGetDampingRatio( Addr );
        set => jniSetDampingRatio( Addr, value );
    }
    
    // ========================================================================
    // ========================================================================
    
    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLocalAnchorA( long addr, float[] anchor );
    /*
        b2WeldJoint* joint = (b2WeldJoint*)addr;
        anchor[0] = joint->GetLocalAnchorA().x;
        anchor[1] = joint->GetLocalAnchorA().y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLocalAnchorB( long addr, float[] anchor );
    /*
        b2WeldJoint* joint = (b2WeldJoint*)addr;
        anchor[0] = joint->GetLocalAnchorB().x;
        anchor[1] = joint->GetLocalAnchorB().y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetReferenceAngle( long addr ); 
    /*
        b2WeldJoint* joint = (b2WeldJoint*)addr;
        return joint->GetReferenceAngle();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetFrequency( long addr );
    /*
        b2WeldJoint* joint = (b2WeldJoint*)addr;
        return joint->GetFrequency();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetFrequency( long addr, float hz );
    /*
        b2WeldJoint* joint = (b2WeldJoint*)addr;
        joint->SetFrequency(hz);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetDampingRatio( long addr );
    /*
        b2WeldJoint* joint = (b2WeldJoint*)addr;
        return joint->GetDampingRatio();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetDampingRatio( long addr, float ratio );
    /*
        b2WeldJoint* joint = (b2WeldJoint*)addr;
        joint->SetDampingRatio(ratio);
    */
}

// ============================================================================
// ============================================================================
