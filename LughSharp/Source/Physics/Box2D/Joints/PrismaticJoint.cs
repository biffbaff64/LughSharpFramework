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
/// A prismatic joint. This joint provides one degree of freedom: translation along
/// an axis fixed in body1. Relative rotation is prevented. You can use a joint limit
/// to restrict the range of motion and a joint motor to drive the motion or to model
/// joint friction.
/// </summary>
[PublicAPI]
public class PrismaticJoint : Joint
{
    private readonly float[] _tmp          = new float[ 2 ];
    private readonly Vector2 _localAnchorA = new();
    private readonly Vector2 _localAnchorB = new();
    private readonly Vector2 _localAxisA   = new();

    // ========================================================================

    public PrismaticJoint( World world, long addr )
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

    public Vector2 GetLocalAxisA()
    {
        jniGetLocalAxisA( Addr, _tmp );
        _localAxisA.Set( _tmp[ 0 ], _tmp[ 1 ] );

        return _localAxisA;
    }

    /// <summary>
    /// Get the current joint translation, usually in meters. 
    /// </summary>
    public float GetJointTranslation()
    {
        return jniGetJointTranslation( Addr );
    }

    /// <summary>
    /// Get the current joint translation speed, usually in meters per second. 
    /// </summary>
    public float GetJointSpeed()
    {
        return jniGetJointSpeed( Addr );
    }

    /// <summary>
    /// Is the joint limit enabled? 
    /// </summary>
    public bool IsLimitEnabled()
    {
        return jniIsLimitEnabled( Addr );
    }

    /// <summary>
    /// Enable/disable the joint limit. 
    /// </summary>
    public void EnableLimit( bool flag )
    {
        jniEnableLimit( Addr, flag );
    }

    /// <summary>
    /// Get the lower joint limit, usually in meters. 
    /// </summary>
    public float GetLowerLimit()
    {
        return jniGetLowerLimit( Addr );
    }

    /// <summary>
    /// Get the upper joint limit, usually in meters. 
    /// </summary>
    public float GetUpperLimit()
    {
        return jniGetUpperLimit( Addr );
    }

    /// <summary>
    /// Set the joint limits, usually in meters. 
    /// </summary>
    public void SetLimits( float lower, float upper )
    {
        jniSetLimits( Addr, lower, upper );
    }

    /// <summary>
    /// Is the joint motor enabled? 
    /// </summary>
    public bool IsMotorEnabled()
    {
        return jniIsMotorEnabled( Addr );
    }

    /// <summary>
    /// Enable/disable the joint motor. 
    /// </summary>
    public void EnableMotor( bool flag )
    {
        jniEnableMotor( Addr, flag );
    }

    /// <summary>
    /// Set the motor speed, usually in meters per second. 
    /// </summary>
    public void SetMotorSpeed( float speed )
    {
        jniSetMotorSpeed( Addr, speed );
    }

    /// <summary>
    /// Get the motor speed, usually in meters per second. 
    /// </summary>
    public float GetMotorSpeed()
    {
        return jniGetMotorSpeed( Addr );
    }

    /// <summary>
    /// Set the maximum motor force, usually in N. 
    /// </summary>
    public void SetMaxMotorForce( float force )
    {
        jniSetMaxMotorForce( Addr, force );
    }

    /// <summary>
    /// Get the current motor force given the inverse time step, usually in N. 
    /// </summary>
    public float GetMotorForce( float invDt )
    {
        return jniGetMotorForce( Addr, invDt );
    }

    /// <summary>
    /// Get the max motor force, usually in N. 
    /// </summary>
    public float GetMaxMotorForce()
    {
        return jniGetMaxMotorForce( Addr );
    }

    /// <summary>
    /// Get the reference angle.  
    /// </summary>
    public float GetReferenceAngle()
    {
        return jniGetReferenceAngle( Addr );
    }

    // ========================================================================
    // ========================================================================

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLocalAnchorA( long addr, float[] anchor );
    /*
        b2PrismaticJoint* joint = (b2PrismaticJoint*)addr;
        anchor[0] = joint->GetLocalAnchorA().x;
        anchor[1] = joint->GetLocalAnchorA().y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLocalAnchorB( long addr, float[] anchor );
    /*
        b2PrismaticJoint* joint = (b2PrismaticJoint*)addr;
        anchor[0] = joint->GetLocalAnchorB().x;
        anchor[1] = joint->GetLocalAnchorB().y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLocalAxisA( long addr, float[] anchor );
    /*
        b2PrismaticJoint* joint = (b2PrismaticJoint*)addr;
        anchor[0] = joint->GetLocalAxisA().x;
        anchor[1] = joint->GetLocalAxisA().y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetJointTranslation( long addr );
    /*
        b2PrismaticJoint* joint = (b2PrismaticJoint*)addr;
        return joint->GetJointTranslation();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetJointSpeed( long addr );
    /*
        b2PrismaticJoint* joint = (b2PrismaticJoint*)addr;
        return joint->GetJointSpeed();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern bool jniIsLimitEnabled( long addr );
    /*
        b2PrismaticJoint* joint = (b2PrismaticJoint*)addr;
        return joint->IsLimitEnabled();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniEnableLimit( long addr, bool flag );
    /*
        b2PrismaticJoint* joint = (b2PrismaticJoint*)addr;
        joint->EnableLimit(flag);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetLowerLimit( long addr );
    /*
        b2PrismaticJoint* joint = (b2PrismaticJoint*)addr;
        return joint->GetLowerLimit();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetUpperLimit( long addr );
    /*
        b2PrismaticJoint* joint = (b2PrismaticJoint*)addr;
        return joint->GetUpperLimit();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetLimits( long addr, float lower, float upper );
    /*
        b2PrismaticJoint* joint = (b2PrismaticJoint*)addr;
        joint->SetLimits(lower, upper );
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern bool jniIsMotorEnabled( long addr );
    /*
        b2PrismaticJoint* joint = (b2PrismaticJoint*)addr;
        return joint->IsMotorEnabled();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniEnableMotor( long addr, bool flag );
    /*
        b2PrismaticJoint* joint = (b2PrismaticJoint*)addr;
        joint->EnableMotor(flag);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetMotorSpeed( long addr, float speed );
    /*
        b2PrismaticJoint* joint = (b2PrismaticJoint*)addr;
        joint->SetMotorSpeed(speed);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetMotorSpeed( long addr );
    /*
        b2PrismaticJoint* joint = (b2PrismaticJoint*)addr;
        return joint->GetMotorSpeed();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetMaxMotorForce( long addr, float force );
    /*
        b2PrismaticJoint* joint = (b2PrismaticJoint*)addr;
        joint->SetMaxMotorForce(force);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetMotorForce( long addr, float invDt );
    /*
        b2PrismaticJoint* joint = (b2PrismaticJoint*)addr;
        return joint->GetMotorForce(invDt);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetMaxMotorForce( long addr );
    /*
        b2PrismaticJoint* joint = (b2PrismaticJoint*)addr;
        return joint->GetMaxMotorForce();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetReferenceAngle( long addr );
    /*
        b2PrismaticJoint* joint = (b2PrismaticJoint*)addr;
        return joint->GetReferenceAngle();
    */
}

// ============================================================================
// ============================================================================
