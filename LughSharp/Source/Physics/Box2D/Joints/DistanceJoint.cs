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
/// A distance joint constrains two points on two bodies to remain at a fixed
/// distance from each other. You can view this as a massless, rigid rod.
/// </summary>
[PublicAPI]
public class DistanceJoint : Joint
{
    private readonly float[] _tmp          = new float[ 2 ];
    private readonly Vector2 _localAnchorA = new();
    private readonly Vector2 _localAnchorB = new();

    // ========================================================================
    
    public DistanceJoint( World world, long addr )
        : base( world, addr )
    {
    }

    public Vector2 GetLocalAnchorA()
    {
        jniGetLocalAnchorA( Addr, _tmp );

        _localAnchorA.Set( _tmp[ 0 ], _tmp[ 1 ] );

        return _localAnchorA;
    }

    public Vector2 GetLocalAnchorB()
    {
        jniGetLocalAnchorB( Addr, _tmp );
        
        _localAnchorB.Set( _tmp[ 0 ], _tmp[ 1 ] );

        return _localAnchorB;
    }

    /// <summary>
    /// Set the natural length. Manipulating the length can lead to non-physical
    /// behavior when the frequency is zero.
    /// </summary>
    public void SetLength( float length )
    {
        jniSetLength( Addr, length );
    }

    /// <summary>
    /// Get the natural length. Manipulating the length can lead to non-physical
    /// behavior when the frequency is zero.
    /// </summary>
    public float GetLength()
    {
        return jniGetLength( Addr );
    }

    /// <summary>
    /// Set frequency in Hz.
    /// </summary>
    public void SetFrequency( float hz )
    {
        jniSetFrequency( Addr, hz );
    }

    /// <summary>
    /// Get frequency in Hz.
    /// </summary>
    public float GetFrequency()
    {
        return jniGetFrequency( Addr );
    }

    /// <summary>
    /// Set damping ratio.
    /// </summary>
    public void SetDampingRatio( float ratio )
    {
        jniSetDampingRatio( Addr, ratio );
    }

    /// <summary>
    /// Get damping ratio.
    /// </summary>
    public float GetDampingRatio()
    {
        return jniGetDampingRatio( Addr );
    }

    // ========================================================================
    // ========================================================================
    
    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLocalAnchorA( long addr, float[] anchor );
    /*
        b2DistanceJoint* joint = (b2DistanceJoint*)addr;
        anchor[0] = joint->GetLocalAnchorA().x;
        anchor[1] = joint->GetLocalAnchorA().y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLocalAnchorB( long addr, float[] anchor );
    /*
        b2DistanceJoint* joint = (b2DistanceJoint*)addr;
        anchor[0] = joint->GetLocalAnchorB().x;
        anchor[1] = joint->GetLocalAnchorB().y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetLength( long addr, float length );
    /*
        b2DistanceJoint* joint = (b2DistanceJoint*)addr;
        joint->SetLength( length );
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetLength( long addr );
    /*
        b2DistanceJoint* joint = (b2DistanceJoint*)addr;
        return joint->GetLength();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetFrequency( long addr, float hz );
    /*
        b2DistanceJoint* joint = (b2DistanceJoint*)addr;
        joint->SetFrequency( hz );
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetFrequency( long addr );
    /*
        b2DistanceJoint* joint = (b2DistanceJoint*)addr;
        return joint->GetFrequency();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetDampingRatio( long addr, float ratio );
    /*
        b2DistanceJoint* joint = (b2DistanceJoint*)addr;
        joint->SetDampingRatio( ratio );
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetDampingRatio( long addr );
    /*
        b2DistanceJoint* joint = (b2DistanceJoint*)addr;
        return joint->GetDampingRatio();
    */
}

// ============================================================================
// ============================================================================
