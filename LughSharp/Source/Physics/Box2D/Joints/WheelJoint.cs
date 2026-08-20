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
/// A wheel joint. This joint provides two degrees of freedom: translation along
/// an axis fixed in body1 and rotation in the plane. You can use a joint limit
/// to restrict the range of motion and a joint motor to drive the rotation or to
/// model rotational friction.
/// <b>This joint is designed for vehicle suspensions.</b>
/// </summary>
/// <param name="world"> The physics world. </param>
/// <param name="addr"> The native address. ( To be removed ) </param>
[PublicAPI]
public class WheelJoint( World world, long addr ) : Joint( world, addr )
{
    private readonly float[] _tmp          = new float[ 2 ];
    private readonly Vector2 _localAnchorA = new();
    private readonly Vector2 _localAnchorB = new();
    private readonly Vector2 _localAxisA   = new();

    // ========================================================================

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
    /// Get/Set the motor speed, usually in radians per second.
    /// </summary>
    public float MotorSpeed
    {
        get => jniGetMotorSpeed( Addr );
        set => jniSetMotorSpeed( Addr, value );
    }
    
    /// <summary>
    /// Set/Get the maximum motor force, usually in N-m.
    /// </summary>
    public float MaxMotorTorque
    {
        get => jniGetMaxMotorTorque( Addr );
        set => jniSetMaxMotorTorque( Addr, value );
    }

    /// <summary>
    /// Get the current motor torque given the inverse time step, usually in N-m.
    /// </summary>
    public float GetMotorTorque( float invDt ) => jniGetMotorTorque( Addr, invDt );

    /// <summary>
    /// Set/Get the spring frequency in hertz. Setting the frequency to
    /// zero disables the spring.
    /// </summary>
    public float SpringFrequencyHz
    {
        get => jniGetSpringFrequencyHz( Addr );
        set => jniSetSpringFrequencyHz( Addr, value );
    }
    
    /// <summary>
    /// Set/Get the spring damping ratio.
    /// </summary>
    public float SpringDampingRatio
    {
        set => jniSetSpringDampingRatio( Addr, value );
        get => jniGetSpringDampingRatio( Addr );
    }

    // ========================================================================
    // ========================================================================
    
    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLocalAnchorA( long addr, float[] anchor );
    /*
        b2WheelJoint* joint = (b2WheelJoint*)addr;
        anchor[0] = joint->GetLocalAnchorA().x;
        anchor[1] = joint->GetLocalAnchorA().y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLocalAnchorB( long addr, float[] anchor );
    /*
        b2WheelJoint* joint = (b2WheelJoint*)addr;
        anchor[0] = joint->GetLocalAnchorB().x;
        anchor[1] = joint->GetLocalAnchorB().y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLocalAxisA( long addr, float[] anchor );
    /*
        b2WheelJoint* joint = (b2WheelJoint*)addr;
        anchor[0] = joint->GetLocalAxisA().x;
        anchor[1] = joint->GetLocalAxisA().y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetJointTranslation( long addr );
    /*
        b2WheelJoint* joint = (b2WheelJoint*)addr;
        return joint->GetJointTranslation();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetJointSpeed( long addr );
    /*
        b2WheelJoint* joint = (b2WheelJoint*)addr;
        return joint->GetJointSpeed();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern bool jniIsMotorEnabled( long addr );
    /*
        b2WheelJoint* joint = (b2WheelJoint*)addr;
        return joint->IsMotorEnabled();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniEnableMotor( long addr, bool flag );
    /*
        b2WheelJoint* joint = (b2WheelJoint*)addr;
        joint->EnableMotor(flag);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetMotorSpeed( long addr, float speed );
    /*
        b2WheelJoint* joint = (b2WheelJoint*)addr;
        joint->SetMotorSpeed(speed);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetMotorSpeed( long addr );
    /*
        b2WheelJoint* joint = (b2WheelJoint*)addr;
        return joint->GetMotorSpeed();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetMaxMotorTorque( long addr, float torque );
    /*
        b2WheelJoint* joint = (b2WheelJoint*)addr;
        joint->SetMaxMotorTorque(torque);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetMaxMotorTorque( long addr );
    /*
        b2WheelJoint* joint = (b2WheelJoint*)addr;
        return joint->GetMaxMotorTorque();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetMotorTorque( long addr, float invDt );
    /*
        b2WheelJoint* joint = (b2WheelJoint*)addr;
        return joint->GetMotorTorque(invDt);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetSpringFrequencyHz( long addr, float hz );
    /*
        b2WheelJoint* joint = (b2WheelJoint*)addr;
        joint->SetSpringFrequencyHz(hz);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetSpringFrequencyHz( long addr );
    /*
        b2WheelJoint* joint = (b2WheelJoint*)addr;
        return joint->GetSpringFrequencyHz();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetSpringDampingRatio( long addr, float ratio );
    /*
        b2WheelJoint* joint = (b2WheelJoint*)addr;
        joint->SetSpringDampingRatio(ratio);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetSpringDampingRatio( long addr );
    /*
        b2WheelJoint* joint = (b2WheelJoint*)addr;
        return joint->GetSpringDampingRatio();
    */
}

// ============================================================================
// ============================================================================
