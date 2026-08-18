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

[PublicAPI]
public class Contact
{
    /// the address 
    protected long Addr;

    /// the world 
    protected World World;

    /// the world manifold 
    protected WorldManifold WorldManifold = new WorldManifold();

    private float[] _tmp = new float[ 8 ];

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="world"></param>
    /// <param name="addr"></param>
    public Contact( World world, long addr )
    {
        this.Addr  = addr;
        this.World = world;
    }

    /// <summary>
    /// Get the world manifold. 
    /// </summary>
    public WorldManifold GetWorldManifold()
    {
        int numContactPoints = JniGetWorldManifold( Addr, _tmp );

        WorldManifold.NumContactPoints = numContactPoints;
        WorldManifold.NormalValue.Set( _tmp[ 0 ], _tmp[ 1 ] );

        for ( int i = 0; i < numContactPoints; i++ )
        {
            Vector2 point = WorldManifold.PointsValue[ i ];
            point.X = _tmp[ 2 + ( i * 2 ) ];
            point.Y = _tmp[ 2 + ( i * 2 ) + 1 ];
        }

        WorldManifold.SeparationsValue[ 0 ] = _tmp[ 6 ];
        WorldManifold.SeparationsValue[ 1 ] = _tmp[ 7 ];

        return WorldManifold;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool IsTouching()
    {
        return JniIsTouching( Addr );
    }

    /// <summary>
    /// Enable/disable this contact. This can be used inside the pre-solve contact listener.
    /// The contact is only disabled for the current time step (or sub-step in continuous
    /// collisions). 
    /// </summary>
    public void SetEnabled( bool flag )
    {
        JniSetEnabled( Addr, flag );
    }

    /// <summary>
    /// Has this contact been disabled? 
    /// </summary>
    public bool IsEnabled()
    {
        return JniIsEnabled( Addr );
    }

    /// <summary>
    /// Get the first fixture in this contact. 
    /// </summary>
    public Fixture GetFixtureA()
    {
        return World.Fixtures[ JniGetFixtureA( Addr ) ];
    }

    /// <summary>
    /// Get the second fixture in this contact. 
    /// </summary>
    public Fixture GetFixtureB()
    {
        return World.Fixtures[ JniGetFixtureB( Addr ) ];
    }

    /// <summary>
    /// Get the child primitive index for fixture A. 
    /// </summary>
    public int GetChildIndexA()
    {
        return JniGetChildIndexA( Addr );
    }

    /// <summary>
    /// Get the child primitive index for fixture B. 
    /// </summary>
    public int GetChildIndexB()
    {
        return JniGetChildIndexB( Addr );
    }

    /// <summary>
    /// Override the default friction mixture. You can call this in b2ContactListener::PreSolve.
    /// This value persists until set or reset. 
    /// </summary>
    public void SetFriction( float friction )
    {
        JniSetFriction( Addr, friction );
    }

    /// <summary>
    /// Get the friction. 
    /// </summary>
    public float GetFriction()
    {
        return JniGetFriction( Addr );
    }

    /// <summary>
    /// Reset the friction mixture to the default value. 
    /// </summary>
    public void ResetFriction()
    {
        JniResetFriction( Addr );
    }

    /// <summary>
    /// Override the default restitution mixture. You can call this in b2ContactListener::PreSolve.
    /// The value persists until you set or reset. 
    /// </summary>
    public void SetRestitution( float restitution )
    {
        JniSetRestitution( Addr, restitution );
    }

    /// <summary>
    /// Get the restitution. 
    /// </summary>
    public float GetRestitution()
    {
        return JniGetRestitution( Addr );
    }

    /// <summary>
    /// Reset the restitution to the default value. 
    /// </summary>
    public void ResetRestitution()
    {
        JniResetRestitution( Addr );
    }

    /// <summary>
    /// Get the tangent speed. 
    /// </summary>
    public float GetTangentSpeed()
    {
        return JniGetTangentSpeed( Addr );
    }

    /// <summary>
    /// Set the tangent speed. 
    /// </summary>
    public void SetTangentSpeed( float speed )
    {
        JniSetTangentSpeed( Addr, speed );
    }

    // ========================================================================
    // ========================================================================
    
    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern int JniGetWorldManifold( long addr, float[] tmp );
    /*
        b2Contact* contact = (b2Contact*)addr;
        b2WorldManifold manifold;
        contact->GetWorldManifold(&manifold);
        int numPoints = contact->GetManifold()->pointCount;

        tmp[0] = manifold.normal.x;
        tmp[1] = manifold.normal.y;

        for( int i = 0; i < numPoints; i++ )
        {
            tmp[2 + i*2] = manifold.points[i].x;
            tmp[2 + i*2+1] = manifold.points[i].y;
        }

        tmp[6] = manifold.separations[0];
        tmp[7] = manifold.separations[1];

        return numPoints;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern bool JniIsTouching( long addr );
    /*
        b2Contact* contact = (b2Contact*)addr;
        return contact->IsTouching();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void JniSetEnabled( long addr, bool flag );
    /*
        b2Contact* contact = (b2Contact*)addr;
        contact->SetEnabled(flag);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern bool JniIsEnabled( long addr );
    /*
        b2Contact* contact = (b2Contact*)addr;
        return contact->IsEnabled();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long JniGetFixtureA( long addr );
    /*
        b2Contact* contact = (b2Contact*)addr;
        return (jlong)contact->GetFixtureA();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long JniGetFixtureB( long addr );
    /*
        b2Contact* contact = (b2Contact*)addr;
        return (jlong)contact->GetFixtureB();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern int JniGetChildIndexA( long addr );
    /*
        b2Contact* contact = (b2Contact*)addr;
        return contact->GetChildIndexA();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern int JniGetChildIndexB( long addr );
    /*
        b2Contact* contact = (b2Contact*)addr;
        return contact->GetChildIndexB();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void JniSetFriction( long addr, float friction );
    /*
        b2Contact* contact = (b2Contact*)addr;
        contact->SetFriction(friction);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float JniGetFriction( long addr );
    /*
        b2Contact* contact = (b2Contact*)addr;
        return contact->GetFriction();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void JniResetFriction( long addr );
    /*
        b2Contact* contact = (b2Contact*)addr;
        contact->ResetFriction();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void JniSetRestitution( long addr, float restitution );
    /*
        b2Contact* contact = (b2Contact*)addr;
        contact->SetRestitution(restitution);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float JniGetRestitution( long addr );
    /*
        b2Contact* contact = (b2Contact*)addr;
        return contact->GetRestitution();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void JniResetRestitution( long addr );
    /*
        b2Contact* contact = (b2Contact*)addr;
        contact->ResetRestitution();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void JniSetTangentSpeed( long addr, float speed );
    /*
        b2Contact* contact = (b2Contact*)addr;
        contact->SetTangentSpeed(speed);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float JniGetTangentSpeed( long addr );
    /*
        b2Contact* contact = (b2Contact*)addr;
        return contact->GetTangentSpeed();
    */
}

// ============================================================================
// ============================================================================
