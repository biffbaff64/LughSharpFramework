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
/// Friction joint. This is used for top-down friction. It provides 2D translational
/// friction and angular friction.
/// </summary>
[PublicAPI]
public class FrictionJoint : Joint
{
    private readonly float[] _tmp          = new float[ 2 ];
    private readonly Vector2 _localAnchorA = new();
    private readonly Vector2 _localAnchorB = new();

    // ========================================================================
    
    public FrictionJoint( World world, long addr )
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
    /// Set the maximum friction force in N.
    /// </summary>
    public void SetMaxForce( float force )
    {
        jniSetMaxForce( Addr, force );
    }

    /// <summary>
    /// Get the maximum friction force in N.
    /// </summary>
    public float GetMaxForce()
    {
        return jniGetMaxForce( Addr );
    }

    /// <summary>
    /// Set the maximum friction torque in N*m.
    /// </summary>
    public void SetMaxTorque( float torque )
    {
        jniSetMaxTorque( Addr, torque );
    }

    /// <summary>
    /// Get the maximum friction torque in N*m.
    /// </summary>
    public float GetMaxTorque()
    {
        return jniGetMaxTorque( Addr );
    }
    
    // ========================================================================
    // ========================================================================

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLocalAnchorA( long addr, float[] anchor );
    /*
        b2FrictionJoint* joint = (b2FrictionJoint*)addr;
        anchor[0] = joint->GetLocalAnchorA().x;
        anchor[1] = joint->GetLocalAnchorA().y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLocalAnchorB( long addr, float[] anchor );
    /*
        b2FrictionJoint* joint = (b2FrictionJoint*)addr;
        anchor[0] = joint->GetLocalAnchorB().x;
        anchor[1] = joint->GetLocalAnchorB().y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetMaxForce( long addr, float force );
    /*
        b2FrictionJoint* joint = (b2FrictionJoint*)addr;
        joint->SetMaxForce( force );
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetMaxForce( long addr );
    /*
        b2FrictionJoint* joint = (b2FrictionJoint*)addr;
        return joint->GetMaxForce();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetMaxTorque( long addr, float torque );
    /*
        b2FrictionJoint* joint = (b2FrictionJoint*)addr;
        joint->SetMaxTorque( torque );
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetMaxTorque( long addr );
    /*
        b2FrictionJoint* joint = (b2FrictionJoint*)addr;
        return joint->GetMaxTorque();
    */
}

// ============================================================================
// ============================================================================
