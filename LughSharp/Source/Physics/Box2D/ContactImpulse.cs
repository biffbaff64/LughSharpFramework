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

namespace LughSharp.Source.Physics.Box2D;

/// <summary>
/// Contact impulses for reporting. Impulses are used instead of forces because
/// sub-step forces may approach infinity for rigid body collisions. These match
/// up one-to-one with the contact points in b2Manifold.
/// </summary>
[PublicAPI]
public class ContactImpulse
{
    public long Addr;

    private readonly float[] _tmp             = new float[ 2 ];
    private readonly float[] _normalImpulses  = new float[ 2 ];
    private readonly float[] _tangentImpulses = new float[ 2 ];

    private readonly World _world;

    // ========================================================================

    public ContactImpulse( World world, long addr )
    {
        this._world = world;
        this.Addr   = addr;
    }

    public float[] GetNormalImpulses()
    {
        jniGetNormalImpulses( Addr, _normalImpulses );

        return _normalImpulses;
    }

    public float[] GetTangentImpulses()
    {
        jniGetTangentImpulses( Addr, _tangentImpulses );

        return _tangentImpulses;
    }

    public int GetCount()
    {
        return jniGetCount( Addr );
    }

    // ========================================================================
    // ========================================================================

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetNormalImpulses( long addr, float[] values );
    /*
        b2ContactImpulse* contactImpulse = (b2ContactImpulse*)addr;
        values[0] = contactImpulse->normalImpulses[0];
        values[1] = contactImpulse->normalImpulses[1];
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetTangentImpulses( long addr, float[] values );
    /*
        b2ContactImpulse* contactImpulse = (b2ContactImpulse*)addr;
        values[0] = contactImpulse->tangentImpulses[0];
        values[1] = contactImpulse->tangentImpulses[1];
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern int jniGetCount( long addr );
    /*
        b2ContactImpulse* contactImpulse = (b2ContactImpulse*)addr;
        return contactImpulse->count;
    */
}

// ============================================================================
// ============================================================================
