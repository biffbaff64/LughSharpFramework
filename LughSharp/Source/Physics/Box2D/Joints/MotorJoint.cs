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
/// A motor joint is used to control the relative motion between two bodies. A
/// typical usage is to control the movement of a dynamic body with respect to
/// the ground.
/// </summary>
[PublicAPI]
public class MotorJoint : Joint
{
    private readonly float[] _tmp = new float[ 2 ];
    private readonly Vector2 _linearOffset = new Vector2();

    // ========================================================================
    
    public MotorJoint( World world, long addr )
        : base( world, addr )
    {
    }

    public Vector2 GetLinearOffset()
    {
        jniGetLinearOffset( Addr, _tmp );
        
        _linearOffset.Set( _tmp[ 0 ], _tmp[ 1 ] );

        return _linearOffset;
    }

    public void SetLinearOffset( Vector2 linearOffset )
    {
        jniSetLinearOffset( Addr, linearOffset.X, linearOffset.Y );
    }

    public float GetAngularOffset()
    {
        return jniGetAngularOffset( Addr );
    }

    public void SetAngularOffset( float angularOffset )
    {
        jniSetAngularOffset( Addr, angularOffset );
    }

    public float GetMaxForce()
    {
        return jniGetMaxForce( Addr );
    }

    public void SetMaxForce( float maxForce )
    {
        jniSetMaxForce( Addr, maxForce );
    }

    public float GetMaxTorque()
    {
        return jniGetMaxTorque( Addr );
    }

    public void SetMaxTorque( float maxTorque )
    {
        jniSetMaxTorque( Addr, maxTorque );
    }

    public float GetCorrectionFactor()
    {
        return jniGetCorrectionFactor( Addr );
    }

    public void SetCorrectionFactor( float correctionFactor )
    {
        jniSetCorrectionFactor( Addr, correctionFactor );
    }

    // ========================================================================
    // ========================================================================
    
    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLinearOffset( long addr, float[] linearOffset );
    /*
        b2MotorJoint* joint = (b2MotorJoint*)addr;
        linearOffset[0] = joint->GetLinearOffset().x;
        linearOffset[1] = joint->GetLinearOffset().y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetLinearOffset( long addr, float linearOffsetX, float linearOffsetY );
    /*
        b2MotorJoint* joint = (b2MotorJoint*)addr;
        joint->SetLinearOffset(b2Vec2(linearOffsetX, linearOffsetY));
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetAngularOffset( long addr );
    /*
        b2MotorJoint* joint = (b2MotorJoint*)addr;
        return joint->GetAngularOffset();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetAngularOffset( long addr, float angularOffset );
    /*
        b2MotorJoint* joint = (b2MotorJoint*)addr;
        joint->SetAngularOffset(angularOffset);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetMaxForce( long addr );
    /*
        b2MotorJoint* joint = (b2MotorJoint*)addr;
        return joint->GetMaxForce();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetMaxForce( long addr, float maxForce );
    /*
        b2MotorJoint* joint = (b2MotorJoint*)addr;
        joint->SetMaxForce(maxForce);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetMaxTorque( long addr );
    /*
        b2MotorJoint* joint = (b2MotorJoint*)addr;
        return joint->GetMaxTorque();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetMaxTorque( long addr, float maxTorque );
    /*
        b2MotorJoint* joint = (b2MotorJoint*)addr;
        joint->SetMaxTorque(maxTorque);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetCorrectionFactor( long addr );
    /*
        b2MotorJoint* joint = (b2MotorJoint*)addr;
        return joint->GetCorrectionFactor();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetCorrectionFactor( long addr, float correctionFactor );
    /*
        b2MotorJoint* joint = (b2MotorJoint*)addr;
        joint->SetCorrectionFactor(correctionFactor);
    */
}

// ============================================================================
// ============================================================================
