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
/// A mouse joint is used to make a point on a body track a specified world point.
/// This a soft constraint with a maximum force. This allows the constraint to
/// stretch and without applying huge forces.
/// <para>
/// NOTE: this joint is not documented in the manual because it was developed to be
/// used in the testbed. If you want to learn how to use the mouse joint, look at
/// the testbed.
/// </para>
/// </summary>
[PublicAPI]
public class MouseJoint : Joint
{
    private readonly float[] _tmp    = new float[ 2 ];
    private readonly Vector2 _target = new();

    // ========================================================================

    public MouseJoint( World world, long addr )
        : base( world, addr )
    {
    }

    /// <summary>
    /// Use this to update the target point. 
    /// </summary>
    public void SetTarget( Vector2 target )
    {
        jniSetTarget( Addr, target.X, target.Y );
    }

    public Vector2 GetTarget()
    {
        jniGetTarget( Addr, _tmp );

        _target.X = _tmp[ 0 ];
        _target.Y = _tmp[ 1 ];

        return _target;
    }

    /// <summary>
    /// Set/get the maximum force in Newtons. 
    /// </summary>
    public void SetMaxForce( float force )
    {
        jniSetMaxForce( Addr, force );
    }

    /// <summary>
    /// Set/get the maximum force in Newtons. 
    /// </summary>
    public float GetMaxForce()
    {
        return jniGetMaxForce( Addr );
    }

    /// <summary>
    /// Set/get the frequency in Hertz. 
    /// </summary>
    public void SetFrequency( float hz )
    {
        jniSetFrequency( Addr, hz );
    }

    /// <summary>
    /// Set/get the frequency in Hertz. 
    /// </summary>
    public float GetFrequency()
    {
        return jniGetFrequency( Addr );
    }

    /// <summary>
    /// Set/get the damping ratio (dimensionless). 
    /// </summary>
    public void SetDampingRatio( float ratio )
    {
        jniSetDampingRatio( Addr, ratio );
    }

    /// <summary>
    /// Set/get the damping ratio (dimensionless). 
    /// </summary>
    public float GetDampingRatio()
    {
        return jniGetDampingRatio( Addr );
    }

    // ========================================================================
    // ========================================================================

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetTarget( long addr, float x, float y );
    /*
        b2MouseJoint* joint = (b2MouseJoint*)addr;
        joint->SetTarget( b2Vec2(x, y ) );
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetTarget( long addr, float[] target );
    /*
        b2MouseJoint* joint = (b2MouseJoint*)addr;
        target[0] = joint->GetTarget().x;
        target[1] = joint->GetTarget().y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetMaxForce( long addr, float force );
    /*
        b2MouseJoint* joint = (b2MouseJoint*)addr;
        joint->SetMaxForce( force );
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetMaxForce( long addr );
    /*
        b2MouseJoint* joint = (b2MouseJoint*)addr;
        return joint->GetMaxForce();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetFrequency( long addr, float hz );
    /*
        b2MouseJoint* joint = (b2MouseJoint*)addr;
        joint->SetFrequency(hz);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetFrequency( long addr );
    /*
        b2MouseJoint* joint = (b2MouseJoint*)addr;
        return joint->GetFrequency();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetDampingRatio( long addr, float ratio );
    /*
        b2MouseJoint* joint = (b2MouseJoint*)addr;
        joint->SetDampingRatio( ratio );
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetDampingRatio( long addr );
    /*
        b2MouseJoint* joint = (b2MouseJoint*)addr;
        return joint->GetDampingRatio();
    */
}

// ============================================================================
// ============================================================================
