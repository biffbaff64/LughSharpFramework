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
/// A gear joint is used to connect two joints together. Either joint can be a revolute
/// or prismatic joint. You specify a gear ratio to bind the motions together:
/// <code>
/// coordinate1 + ratio * coordinate2 = constant
/// </code>
/// The ratio can be negative or positive. If one joint is a revolute joint and the
/// other joint is a prismatic joint, then the ratio will have units of length or
/// units of 1/length.
/// <br/>
/// <para>
/// <b>
/// WARNING
/// <br/>
///    The revolute and prismatic joints must be attached to fixed bodies (which must
///    be body1 on those joints).
/// </b>
/// </para>
/// <br/>
/// </summary>
[PublicAPI]
public class GearJoint : Joint
{
    public Joint Joint1 { get; set; }
    public Joint Joint2 { get; set; }

    // ========================================================================
    
    public GearJoint( World world, long addr, Joint joint1, Joint joint2 )
        : base( world, addr )
    {
        this.Joint1 = joint1;
        this.Joint2 = joint2;
    }

    public float GearRatio
    {
        get => jniGetRatio( Addr );
        set => jniSetRatio( Addr, value );
    }
    
    // ========================================================================
    // ========================================================================

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long jniGetJoint1( long addr );
    /*
        b2GearJoint* joint =  (b2GearJoint*)addr;
        b2Joint* joint1 = joint->GetJoint1();
        return (jlong)joint1;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long jniGetJoint2( long addr );
    /*
        b2GearJoint* joint =  (b2GearJoint*)addr;
        b2Joint* joint2 = joint->GetJoint2();
        return (jlong)joint2;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetRatio( long addr, float ratio );
    /*
        b2GearJoint* joint =  (b2GearJoint*)addr;
        joint->SetRatio( ratio );
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetRatio( long addr );
    /*
        b2GearJoint* joint =  (b2GearJoint*)addr;
        return joint->GetRatio();
    */
}

// ============================================================================
// ============================================================================
