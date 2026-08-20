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
/// A revolute joint constrains two bodies to share a common point while they are
/// free to rotate about the point. The relative rotation about the shared point
/// is the joint angle. You can limit the relative rotation with a joint limit that
/// specifies a lower and upper angle. You can use a motor to drive the relative
/// rotation about the shared point. A maximum motor torque is provided so that
/// infinite forces are not generated.
/// </summary>
[PublicAPI]
public class RevoluteJoint : Joint
{
    private readonly float[] _tmp          = new float[ 2 ];
    private readonly Vector2 _localAnchorA = new();
    private readonly Vector2 _localAnchorB = new();

    // ========================================================================

    public RevoluteJoint( World world, long addr )
        : base( world, addr )
    {
    }

    /// <summary>
    /// Get the current joint angle in radians.
    /// </summary>
    public float GetJointAngle()
    {
        return jniGetJointAngle( Addr );
    }

    /// <summary>
    /// Get the current joint angle speed in radians per second.
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
    /// Get the lower joint limit in radians.
    /// </summary>
    public float GetLowerLimit()
    {
        return jniGetLowerLimit( Addr );
    }

    /// <summary>
    /// Get the upper joint limit in radians.
    /// </summary>
    public float GetUpperLimit()
    {
        return jniGetUpperLimit( Addr );
    }

    /// <summary>
    /// Set the joint limits in radians.
    /// </summary>
    /// <param name="lower"></param>
    /// <param name="upper"></param>
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
    /// Set the motor speed in radians per second.
    /// </summary>
    public void SetMotorSpeed( float speed )
    {
        jniSetMotorSpeed( Addr, speed );
    }

    /// <summary>
    /// Get the motor speed in radians per second.
    /// </summary>
    public float GetMotorSpeed()
    {
        return jniGetMotorSpeed( Addr );
    }

    /// <summary>
    /// Set the maximum motor torque, usually in N-m.
    /// </summary>
    public void SetMaxMotorTorque( float torque )
    {
        jniSetMaxMotorTorque( Addr, torque );
    }

    /// <summary>
    /// Get the current motor torque, usually in N-m.
    /// </summary>
    public float GetMotorTorque( float invDt )
    {
        return jniGetMotorTorque( Addr, invDt );
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
    /// Get the current motor torque, usually in N-m.
    /// </summary>
    public float GetReferenceAngle()
    {
        return jniGetReferenceAngle( Addr );
    }

    public float GetMaxMotorTorque()
    {
        return jniGetMaxMotorTorque( Addr );
    }
    // ========================================================================
    // ========================================================================

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetJointAngle( long addr );
    /*
        b2RevoluteJoint* joint = (b2RevoluteJoint*)addr;
        return joint->GetJointAngle();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetJointSpeed( long addr );
    /*
        b2RevoluteJoint* joint = (b2RevoluteJoint*)addr;
        return joint->GetJointSpeed();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern bool jniIsLimitEnabled( long addr ); /*
        b2RevoluteJoint* joint = (b2RevoluteJoint*)addr;
        return joint->IsLimitEnabled();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniEnableLimit( long addr, bool flag ); /*
        b2RevoluteJoint* joint = (b2RevoluteJoint*)addr;
        joint->EnableLimit(flag);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetLowerLimit( long addr ); /*
        b2RevoluteJoint* joint = (b2RevoluteJoint*)addr;
        return joint->GetLowerLimit();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetUpperLimit( long addr ); /*
        b2RevoluteJoint* joint = (b2RevoluteJoint*)addr;
        return joint->GetUpperLimit();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetLimits( long addr, float lower, float upper ); /*
        b2RevoluteJoint* joint = (b2RevoluteJoint*)addr;
        joint->SetLimits(lower, upper );
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern bool jniIsMotorEnabled( long addr ); /*
        b2RevoluteJoint* joint = (b2RevoluteJoint*)addr;
        return joint->IsMotorEnabled();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniEnableMotor( long addr, bool flag ); /*
        b2RevoluteJoint* joint = (b2RevoluteJoint*)addr;
        joint->EnableMotor(flag);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetMotorSpeed( long addr, float speed ); /*
        b2RevoluteJoint* joint = (b2RevoluteJoint*)addr;
        joint->SetMotorSpeed(speed);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetMotorSpeed( long addr ); /*
        b2RevoluteJoint* joint = (b2RevoluteJoint*)addr;
        return joint->GetMotorSpeed();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetMaxMotorTorque( long addr, float torque ); /*
        b2RevoluteJoint* joint = (b2RevoluteJoint*)addr;
        joint->SetMaxMotorTorque(torque);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetMotorTorque( long addr, float invDt ); /*
        b2RevoluteJoint* joint = (b2RevoluteJoint*)addr;
        return joint->GetMotorTorque(invDt);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLocalAnchorA( long addr, float[] anchor ); /*
        b2RevoluteJoint* joint = (b2RevoluteJoint*)addr;
        anchor[0] = joint->GetLocalAnchorA().x;
        anchor[1] = joint->GetLocalAnchorA().y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLocalAnchorB( long addr, float[] anchor ); /*
        b2RevoluteJoint* joint = (b2RevoluteJoint*)addr;
        anchor[0] = joint->GetLocalAnchorB().x;
        anchor[1] = joint->GetLocalAnchorB().y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetReferenceAngle( long addr ); /*
        b2RevoluteJoint* joint = (b2RevoluteJoint*)addr;
        return joint->GetReferenceAngle();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetMaxMotorTorque( long addr ); /*
        b2RevoluteJoint* joint = (b2RevoluteJoint*)addr;
        return joint->GetMaxMotorTorque();
    */
}

// ============================================================================
// ============================================================================
