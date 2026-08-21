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

using LughSharp.Source.Collections;
using LughSharp.Source.Utils.Pooling;

namespace LughSharp.Source.Physics.Box2D;

[PublicAPI]
public sealed class World
{
    /// <summary>
    /// pool for bodies, initialised in the constructor.
    /// </summary>
    public readonly Pool< Body > FreeBodies;

    /// <summary>
    /// pool for fixtures
    /// </summary>
    public readonly Pool< Fixture > FreeFixtures = new( 100, 200 )
    {
        NewObjectFactory = () => new Fixture( null, 0 ),
    };

    // ========================================================================

    /// <summary>
    /// the address of the world instance
    /// </summary>
    public long Addr;

    // NOTE:
    // This class in LibGDX used a LongMap<> for the following.
    // LongMap uses a long key and a value of type T.
    // I've switched to a Dictionary<> for this.

    /// <summary>
    /// all known bodies 
    /// </summary>
    public readonly Dictionary< long, Body > Bodies = new( 100 );

    /// <summary>
    /// all known fixtures 
    /// </summary>
    public readonly Dictionary< long, Fixture > Fixtures = new( 100 );

    /// <summary>
    /// all known joints 
    /// </summary>
    public readonly Dictionary< long, Joint > Joints = new( 100 );

    /// <summary>
    /// Contact listener 
    /// </summary>
    public IContactListener? ContactListener;

    // ========================================================================

    private readonly Manifold        _manifold     = new( 0 );
    private readonly List< Contact > _contacts     = [ ];
    private readonly List< Contact > _freeContacts = [ ];
    private readonly float[]         _tmpGravity   = new float[ 2 ];
    private readonly Vector2         _gravity      = new();

    private Vector2 _rayPoint     = new();
    private Vector2 _rayNormal    = new();
    private long[]  _contactAddrs = new long[ 200 ];

    private readonly Contact        _contact;
    private readonly ContactImpulse _impulse;

    private IQueryCallback?   _queryCallback;
    private bool              _useDefaultContactFilter;
    private IContactFilter?   _contactFilter;
    private IRayCastCallback? _rayCastCallback;

    // ========================================================================

    /// <summary>
    /// Construct a world object.
    /// </summary>
    /// <param name="gravity"> the world gravity vector. </param>
    /// <param name="doSleep"> improve performance by not simulating inactive bodies. </param> 
    public World( Vector2 gravity, bool doSleep )
    {
        FreeBodies = new Pool< Body >( 100, 200 )
        {
            NewObjectFactory = () => new Body( this, 0 ),
        };

        _contact = new Contact( this, 0 );
        _impulse = new ContactImpulse( this, 0 );

        Addr = NewWorld( gravity.X, gravity.Y, doSleep );

        _contacts.EnsureCapacity( _contactAddrs.Length );
        _freeContacts.EnsureCapacity( _contactAddrs.Length );

        for ( int i = 0; i < _contactAddrs.Length; i++ )
        {
            _freeContacts.Add( new Contact( this, 0 ) );
        }
    }

    /// <summary>
    /// Register a destruction listener. The listener is owned by you and must
    /// remain in scope. 
    /// </summary>
    public void SetDestructionListener( IDestructionListener listener )
    {
    }

    /// <summary>
    /// Register a contact filter to provide specific control over collision. Otherwise
    /// the default filter is used (b2_defaultFilter). The listener is owned by you and
    /// must remain in scope.
    /// </summary>
    public void SetContactFilter( IContactFilter? filter )
    {
        this._contactFilter = filter;
        SetUseDefaultContactFilter( filter == null );
    }
    
    /// <summary>
    /// Internal method called from JNI
    /// </summary>
    /// <returns> whether the native default IContactFilter should be used </returns>
    private bool GetUseDefaultContactFilter()
    {
        return _useDefaultContactFilter;
    }

    /// <summary>
    /// Sets flag to tell the native code not to call the Java World class if use is true
    /// </summary>
    private void SetUseDefaultContactFilter( bool use )
    {
        _useDefaultContactFilter = use;
    }

    /// <summary>
    /// Register a contact event listener. The listener is owned by you and must remain in scope. 
    /// </summary>
    public void SetContactListener( IContactListener listener )
    {
        this.ContactListener = listener;
    }

    /// <summary>
    /// Create a rigid body given a definition. No reference to the definition is retained.
    /// Bodies created by this method are pooled internally by the World object.
    /// They will be freed upon calling <see cref="World.DestroyBody(Body)"/>
    /// <br/>
    /// <b>
    /// WARNING: This function is locked during callbacks.
    /// </b>
    /// </summary>
    public Body CreateBody( BodyDef def )
    {
        long bodyAddr = jniCreateBody
            (
             Addr,
             ( int )def.Type,
             def.Position.X,
             def.Position.Y,
             def.Angle,
             def.LinearVelocity.X,
             def.LinearVelocity.Y,
             def.AngularVelocity,
             def.LinearDamping,
             def.AngularDamping,
             def.AllowSleep,
             def.Awake,
             def.FixedRotation,
             def.Bullet,
             def.Active,
             def.GravityScale
            );

        Body body = FreeBodies.Obtain();
        body.Reset( bodyAddr );
        this.Bodies[ body.Addr ] = body;

        return body;
    }

    /// <summary>
    /// Destroy a rigid body given a definition. No reference to the definition is retained.
    /// <br/>
    /// <b>
    /// WARNING: This automatically deletes all associated shapes and joints.
    /// WARNING: This function is locked during callbacks.
    /// </b>
    /// </summary>
    public void DestroyBody( Body body )
    {
        List< JointEdge > jointList = body.GetJointList();

        while ( jointList.Count > 0 )
        {
            DestroyJoint( body.GetJointList()[ 0 ].Joint );
        }

        jniDestroyBody( Addr, body.Addr );

        body.UserData = null;
        this.Bodies.Remove( body.Addr );

        List< Fixture > fixtureList = body.GetFixtureList();

        while ( fixtureList.Count > 0 )
        {
            Fixture fixtureToDelete = fixtureList.RemoveIndex( 0 );

            fixtureToDelete.UserData = null;
            this.Fixtures.Remove( fixtureToDelete.Addr );

            FreeFixtures.Free( fixtureToDelete );
        }

        FreeBodies.Free( body );
    }
    
    /// <summary>
    /// Internal method for fixture destruction with notifying custom contact listener
    /// </summary>
    /// <param name="body"></param>
    /// <param name="fixture"></param>
    public void DestroyFixture( Body body, Fixture fixture )
    {
        jniDestroyFixture( Addr, body.Addr, fixture.Addr );
    }
    
    /// <summary>
    /// Internal method for body deactivation with notifying custom contact listener
    /// </summary>
    /// <param name="body"></param>
    public void DeactivateBody( Body body )
    {
        jniDeactivateBody( Addr, body.Addr );
    }

    /// <summary>
    /// Create a joint to constrain bodies together. No reference to the definition is
    /// retained. This may cause the connected bodies to cease colliding.
    /// <br/>
    /// <b>
    /// WARNING: This function is locked during callbacks.
    /// </b>
    /// </summary>
    public Joint CreateJoint( JointDef def )
    {
        long   jointAddr = CreateProperJoint( def );
        Joint? joint     = null;

        if ( def.Type == JointDef.JointType.DistanceJoint ) joint = new DistanceJoint( this, jointAddr );
        if ( def.Type == JointDef.JointType.FrictionJoint ) joint = new FrictionJoint( this, jointAddr );

        if ( def.Type == JointDef.JointType.GearJoint )
        {
            joint = new GearJoint( this, jointAddr, ( ( GearJointDef )def ).Joint1, ( ( GearJointDef )def ).Joint2 );
        }

        if ( def.Type == JointDef.JointType.MotorJoint ) joint     = new MotorJoint( this, jointAddr );
        if ( def.Type == JointDef.JointType.MouseJoint ) joint     = new MouseJoint( this, jointAddr );
        if ( def.Type == JointDef.JointType.PrismaticJoint ) joint = new PrismaticJoint( this, jointAddr );
        if ( def.Type == JointDef.JointType.PulleyJoint ) joint    = new PulleyJoint( this, jointAddr );
        if ( def.Type == JointDef.JointType.RevoluteJoint ) joint  = new RevoluteJoint( this, jointAddr );
        if ( def.Type == JointDef.JointType.RopeJoint ) joint      = new RopeJoint( this, jointAddr );
        if ( def.Type == JointDef.JointType.WeldJoint ) joint      = new WeldJoint( this, jointAddr );
        if ( def.Type == JointDef.JointType.WheelJoint ) joint     = new WheelJoint( this, jointAddr );

        if ( joint == null ) throw new LughRuntimeException( "Unknown joint type: " + def.Type );

        Joints[ joint.Addr ] = joint;
        JointEdge jointEdgeA = new JointEdge( def.BodyB, joint );
        JointEdge jointEdgeB = new JointEdge( def.BodyA, joint );

        joint.JointEdgeA = jointEdgeA;
        joint.JointEdgeB = jointEdgeB;

        def.BodyA?.Joints.Add( jointEdgeA );
        def.BodyB?.Joints.Add( jointEdgeB );

        return joint;
    }

    private long CreateProperJoint( JointDef def )
    {
        if ( def.Type == JointDef.JointType.DistanceJoint )
        {
            DistanceJointDef d = ( DistanceJointDef )def;

            return jniCreateDistanceJoint
                (
                 Addr,
                 d.BodyA.Addr,
                 d.BodyB.Addr,
                 d.CollideConnected,
                 d.LocalAnchorA.X,
                 d.LocalAnchorA.Y,
                 d.LocalAnchorB.X,
                 d.LocalAnchorB.Y,
                 d.Length,
                 d.FrequencyHz,
                 d.DampingRatio
                );
        }

        if ( def.Type == JointDef.JointType.FrictionJoint )
        {
            FrictionJointDef d = ( FrictionJointDef )def;

            return jniCreateFrictionJoint
                (
                 Addr,
                 d.BodyA.Addr,
                 d.BodyB.Addr,
                 d.CollideConnected,
                 d.LocalAnchorA.X,
                 d.LocalAnchorA.Y,
                 d.LocalAnchorB.X,
                 d.LocalAnchorB.Y,
                 d.MaxForce,
                 d.MaxTorque
                );
        }

        if ( def.Type == JointDef.JointType.GearJoint )
        {
            GearJointDef d = ( GearJointDef )def;

            return jniCreateGearJoint
                (
                 Addr,
                 d.BodyA.Addr,
                 d.BodyB.Addr,
                 d.CollideConnected,
                 d.Joint1.Addr,
                 d.Joint2.Addr,
                 d.Ratio
                );
        }

        if ( def.Type == JointDef.JointType.MotorJoint )
        {
            MotorJointDef d = ( MotorJointDef )def;

            return jniCreateMotorJoint
                (
                 Addr,
                 d.BodyA.Addr,
                 d.BodyB.Addr,
                 d.CollideConnected,
                 d.LinearOffset.X,
                 d.LinearOffset.Y,
                 d.AngularOffset,
                 d.MaxForce,
                 d.MaxTorque,
                 d.CorrectionFactor
                );
        }

        if ( def.Type == JointDef.JointType.MouseJoint )
        {
            MouseJointDef d = ( MouseJointDef )def;

            return jniCreateMouseJoint
                (
                 Addr,
                 d.BodyA.Addr,
                 d.BodyB.Addr,
                 d.CollideConnected,
                 d.Target.X,
                 d.Target.Y,
                 d.MaxForce,
                 d.FrequencyHz,
                 d.DampingRatio
                );
        }

        if ( def.Type == JointDef.JointType.PrismaticJoint )
        {
            PrismaticJointDef d = ( PrismaticJointDef )def;

            return jniCreatePrismaticJoint
                (
                 Addr,
                 d.BodyA.Addr,
                 d.BodyB.Addr,
                 d.CollideConnected,
                 d.LocalAnchorA.X,
                 d.LocalAnchorA.Y,
                 d.LocalAnchorB.X,
                 d.LocalAnchorB.Y,
                 d.LocalAxisA.X,
                 d.LocalAxisA.Y,
                 d.ReferenceAngle,
                 d.EnableLimit,
                 d.LowerTranslation,
                 d.UpperTranslation,
                 d.EnableMotor,
                 d.MaxMotorForce,
                 d.MotorSpeed
                );
        }

        if ( def.Type == JointDef.JointType.PulleyJoint )
        {
            PulleyJointDef d = ( PulleyJointDef )def;

            return jniCreatePulleyJoint
                (
                 Addr,
                 d.BodyA.Addr,
                 d.BodyB.Addr,
                 d.CollideConnected,
                 d.GroundAnchorA.X,
                 d.GroundAnchorA.Y,
                 d.GroundAnchorB.X,
                 d.GroundAnchorB.Y,
                 d.LocalAnchorA.X,
                 d.LocalAnchorA.Y,
                 d.LocalAnchorB.X,
                 d.LocalAnchorB.Y,
                 d.LengthA,
                 d.LengthB,
                 d.Ratio
                );
        }

        if ( def.Type == JointDef.JointType.RevoluteJoint )
        {
            RevoluteJointDef d = ( RevoluteJointDef )def;

            return jniCreateRevoluteJoint
                (
                 Addr,
                 d.BodyA.Addr,
                 d.BodyB.Addr,
                 d.CollideConnected,
                 d.LocalAnchorA.X,
                 d.LocalAnchorA.Y,
                 d.LocalAnchorB.X,
                 d.LocalAnchorB.Y,
                 d.ReferenceAngle,
                 d.EnableLimit,
                 d.LowerAngle,
                 d.UpperAngle,
                 d.EnableMotor,
                 d.MotorSpeed,
                 d.MaxMotorTorque
                );
        }

        if ( def.Type == JointDef.JointType.RopeJoint )
        {
            RopeJointDef d = ( RopeJointDef )def;

            return jniCreateRopeJoint
                (
                 Addr,
                 d.BodyA.Addr,
                 d.BodyB.Addr,
                 d.CollideConnected,
                 d.LocalAnchorA.X,
                 d.LocalAnchorA.Y,
                 d.LocalAnchorB.X,
                 d.LocalAnchorB.Y,
                 d.MaxLength
                );
        }

        if ( def.Type == JointDef.JointType.WeldJoint )
        {
            WeldJointDef d = ( WeldJointDef )def;

            return jniCreateWeldJoint
                (
                 Addr,
                 d.BodyA.Addr,
                 d.BodyB.Addr,
                 d.CollideConnected,
                 d.LocalAnchorA.X,
                 d.LocalAnchorA.Y,
                 d.LocalAnchorB.X,
                 d.LocalAnchorB.Y,
                 d.ReferenceAngle,
                 d.FrequencyHz,
                 d.DampingRatio
                );
        }

        if ( def.Type == JointDef.JointType.WheelJoint )
        {
            WheelJointDef d = ( WheelJointDef )def;

            return jniCreateWheelJoint
                (
                 Addr,
                 d.BodyA.Addr,
                 d.BodyB.Addr,
                 d.CollideConnected,
                 d.LocalAnchorA.X,
                 d.LocalAnchorA.Y,
                 d.LocalAnchorB.X,
                 d.LocalAnchorB.Y,
                 d.LocalAxisA.X,
                 d.LocalAxisA.Y,
                 d.EnableMotor,
                 d.MaxMotorTorque,
                 d.MotorSpeed,
                 d.FrequencyHz,
                 d.DampingRatio
                );
        }

        return 0;
    }
    
    /// <summary>
    /// Destroy a joint. This may cause the connected bodies to begin colliding.
    /// <br/>
    /// <b>
    /// WARNING: This function is locked during callbacks.
    /// </b>
    /// </summary>
    public void DestroyJoint( Joint joint )
    {
        joint.UserData = null;
        Joints.Remove( joint.Addr );
        joint.JointEdgeA.Other.Joints.Remove( joint.JointEdgeB );
        joint.JointEdgeB.Other.Joints.Remove( joint.JointEdgeA );

        jniDestroyJoint( Addr, joint.Addr );
    }

    /// <summary>
    /// Take a time step. This performs collision detection, integration, and
    /// constraint solution.
    /// </summary>
    /// <param name="timeStep"> the amount of time to simulate, this should not vary. </param>
    /// <param name="velocityIterations"> for the velocity constraint solver. </param>
    /// <param name="positionIterations"> for the position constraint solver. </param>
    public void Step( float timeStep, int velocityIterations, int positionIterations )
    {
        jniStep( Addr, timeStep, velocityIterations, positionIterations );
    }
    
    /// <summary>
    /// Manually clear the force buffer on all bodies. By default, forces are
    /// cleared automatically after each call to Step. The default behavior is
    /// modified by calling SetAutoClearForces.The purpose of this function is
    /// to support sub-stepping.
    /// <para>
    /// Sub-stepping is often used to maintain a fixed sized time step under a
    /// variable frame-rate. When you perform sub-stepping you will disable auto
    /// clearing of forces and instead call ClearForces after all sub-steps are
    /// complete in one pass of your game loop.
    /// <code>
    /// {
    ///     SetAutoClearForces( bool );
    /// }
    /// </code>
    /// </para>
    /// </summary>
    public void ClearForces()
    {
        jniClearForces( Addr );
    }

    /// <summary>
    /// Enable/disable warm starting. For testing. 
    /// </summary>
    public void SetWarmStarting( bool flag )
    {
        jniSetWarmStarting( Addr, flag );
    }

    /// <summary>
    /// Enable/disable continuous physics. For testing. 
    /// </summary>
    public void SetContinuousPhysics( bool flag )
    {
        jniSetContiousPhysics( Addr, flag );
    }

    /// <summary>
    /// Get the number of broad-phase proxies. 
    /// </summary>
    public int GetProxyCount()
    {
        return jniGetProxyCount( Addr );
    }

    /// <summary>
    /// Get the number of bodies. 
    /// </summary>
    public int GetBodyCount()
    {
        return jniGetBodyCount( Addr );
    }

    /// <summary>
    /// Get the number of fixtures. 
    /// </summary>
    public int GetFixtureCount()
    {
        return Fixtures.Count;
    }

    /// <summary>
    /// Get the number of joints. 
    /// </summary>
    public int GetJointCount()
    {
        return jniGetJointcount( Addr );
    }

    /// <summary>
    /// Get the number of contacts (each may have 0 or more contact points). 
    /// </summary>
    public int GetContactCount()
    {
        return jniGetContactCount( Addr );
    }

    /// <summary>
    /// Change the global gravity vector. 
    /// </summary>
    public void SetGravity( Vector2 gravity )
    {
        jniSetGravity( Addr, gravity.X, gravity.Y );
    }

    public Vector2 GetGravity()
    {
        jniGetGravity( Addr, _tmpGravity );
        _gravity.X = _tmpGravity[ 0 ];
        _gravity.Y = _tmpGravity[ 1 ];

        return _gravity;
    }

    /// <summary>
    /// Is the world locked (in the middle of a time step). 
    /// </summary>
    public bool IsLocked()
    {
        return jniIsLocked( Addr );
    }

    /// <summary>
    /// Set flag to control automatic clearing of forces after each time step. 
    /// </summary>
    public void SetAutoClearForces( bool flag )
    {
        jniSetAutoClearForces( Addr, flag );
    }

    /// <summary>
    /// Get the flag that controls automatic clearing of forces after each time step. 
    /// </summary>
    public bool GetAutoClearForces()
    {
        return jniGetAutoClearForces( Addr );
    }

    /// <summary>
    /// Query the world for all fixtures that potentially overlap the provided AABB.
    /// </summary>
    /// <param name="callback"> a user implemented callback class. </param>
    /// <param name="lowerX"> the x coordinate of the lower left corner </param>
    /// <param name="lowerY"> the y coordinate of the lower left corner </param>
    /// <param name="upperX"> the x coordinate of the upper right corner </param>
    /// <param name="upperY"> the y coordinate of the upper right corner </param>
    public void QueryAabb( IQueryCallback callback, float lowerX, float lowerY, float upperX, float upperY )
    {
        _queryCallback = callback;
        jniQueryAABB( Addr, lowerX, lowerY, upperX, upperY );
    }
    
    /// <summary>
    /// Returns the list of <see cref="Contact"/> instances produced by the last call to
    /// <see cref="Step(float, int, int)"/>. Note that the returned list will have O( 1 )
    /// access times when using indexing.contacts are created and destroyed in the middle
    /// of a time step.
    /// <para>
    /// Use <see cref="IContactListener"/> to avoid missing contacts.
    /// </para>
    /// </summary>
    /// <returns> the contact list </returns>
    public List< Contact > GetContactList()
    {
        int numContacts = GetContactCount();

        if ( numContacts > _contactAddrs.Length )
        {
            int newSize = 2 * numContacts;
            _contactAddrs = new long[ newSize ];
            _contacts.EnsureCapacity( newSize );
            _freeContacts.EnsureCapacity( newSize );
        }

        if ( numContacts > _freeContacts.Count )
        {
            int freeConts = _freeContacts.Count;

            for ( int i = 0; i < numContacts - freeConts; i++ )
            {
                _freeContacts.Add( new Contact( this, 0 ) );
            }
        }

        jniGetContactList( Addr, _contactAddrs );

        _contacts.Clear();

        for ( int i = 0; i < numContacts; i++ )
        {
            Contact contact = _freeContacts[ i ];
            contact.Addr = _contactAddrs[ i ];
            _contacts.Add( contact );
        }

        return _contacts;
    }

    /// <summary>
    /// Gets all bodies currently in the simulation and places them in the provided list.
    /// </summary>
    /// <param name="bodies">
    /// an Array in which to place all bodies currently in the simulation
    /// </param> 
    public void GetBodies( List< Body > bodies )
    {
        bodies.Clear();
        bodies.EnsureCapacity( this.Bodies.Count );
        bodies.AddRange( Bodies.Values );
    }

    /// <summary>
    /// Gets all fixtures currently in the simulation and places them in the provided list.
    /// </summary>
    /// <param name="fixtures">
    /// an Array in which to place all fixtures currently in the simulation
    /// </param> 
    public void GetFixtures( List< Fixture > fixtures )
    {
        fixtures.Clear();
        fixtures.EnsureCapacity( this.Fixtures.Count );
        fixtures.AddRange( Fixtures.Values );
    }

    /// <summary>
    /// Gets all joints currently in the simulation and places them in the provided list.
    /// </summary>
    /// <param name="joints">
    /// an Array in which to place all joints currently in the simulation
    /// </param> 
    public void GetJoints( List< Joint > joints )
    {
        joints.Clear();
        joints.EnsureCapacity( this.Joints.Count );
        joints.AddRange( Joints.Values );
    }

    public void Dispose()
    {
        jniDispose( Addr );
    }
    
    /// <summary>
    /// Internal method called from JNI in case a contact happens
    /// </summary>
    /// <param name="fixtureA"></param>
    /// <param name="fixtureB"></param>
    /// <returns>whether the things collided</returns>
    private bool ContactFilter( long fixtureA, long fixtureB )
    {
        if ( _contactFilter != null )
        {
            return _contactFilter.ShouldCollide( Fixtures[ fixtureA ], Fixtures[ fixtureB ] );
        }
        else
        {
            Filter filterA = Fixtures[ fixtureA ].GetFilterData();
            Filter filterB = Fixtures[ fixtureB ].GetFilterData();

            if ( filterA.GroupIndex == filterB.GroupIndex && filterA.GroupIndex != 0 )
            {
                return filterA.GroupIndex > 0;
            }

            bool collide = ( filterA.MaskBits & filterB.CategoryBits ) != 0
                        && ( filterA.CategoryBits & filterB.MaskBits ) != 0;

            return collide;
        }
    }

    private void BeginContact( long contactAddr )
    {
        if ( ContactListener != null )
        {
            _contact.Addr = contactAddr;
            ContactListener.BeginContact( _contact );
        }
    }

    private void EndContact( long contactAddr )
    {
        if ( ContactListener != null )
        {
            _contact.Addr = contactAddr;
            ContactListener.EndContact( _contact );
        }
    }

    private void PreSolve( long contactAddr, long manifoldAddr )
    {
        if ( ContactListener != null )
        {
            _contact.Addr  = contactAddr;
            _manifold.Addr = manifoldAddr;
            ContactListener.PreSolve( _contact, _manifold );
        }
    }

    private void PostSolve( long contactAddr, long impulseAddr )
    {
        if ( ContactListener != null )
        {
            _contact.Addr = contactAddr;
            _impulse.Addr = impulseAddr;

            ContactListener.PostSolve( _contact, _impulse );
        }
    }

    private bool ReportFixture( long addr )
    {
        if ( _queryCallback != null )
        {
            return _queryCallback.ReportFixture( Fixtures[ addr ] );
        }

        return false;
    }

    /// <summary>
    /// Ray-cast the world for all fixtures in the path of the ray. The ray-cast
    /// ignores shapes that contain the starting point.
    /// </summary>
    /// <param name="callback"> a user implemented callback class. </param>
    /// <param name="point1"> the ray starting point </param>
    /// <param name="point2"> the ray ending point </param>
    public void RayCast( IRayCastCallback callback, Vector2 point1, Vector2 point2 )
    {
        RayCast( callback, point1.X, point1.Y, point2.X, point2.Y );
    }

    /// <summary>
    /// Ray-cast the world for all fixtures in the path of the ray. The ray-cast
    /// ignores shapes that contain the starting point.
    /// </summary>
    /// <param name="callback"> a user implemented callback class. </param>
    /// <param name="point1X"> the ray starting point X </param>
    /// <param name="point1Y"> the ray starting point Y </param>
    /// <param name="point2X"> the ray ending point X </param>
    /// <param name="point2Y"> the ray ending point Y </param>
    public void RayCast( IRayCastCallback callback, float point1X, float point1Y, float point2X, float point2Y )
    {
        _rayCastCallback = callback;
        jniRayCast( Addr, point1X, point1Y, point2X, point2Y );
    }

    private float ReportRayFixture( long addr, float pX, float pY, float nX, float nY, float fraction )
    {
        if ( _rayCastCallback != null )
        {
            _rayPoint.X  = pX;
            _rayPoint.Y  = pY;
            _rayNormal.X = nX;
            _rayNormal.Y = nY;

            return _rayCastCallback.ReportRayFixture( Fixtures[ addr ], _rayPoint, _rayNormal, fraction );
        }

        return 0.0f;
    }

    // ============================================================================
    // ============================================================================

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long NewWorld( float gravityX, float gravityY, bool doSleep );
    /*
        // we leak one global ref.
        if(!worldClass) {
            worldClass = (jclass)env->NewGlobalRef(env->GetObjectClass(object));
            beginContactID = env->GetMethodID(worldClass, "beginContact", "(J)V" );
            endContactID = env->GetMethodID( worldClass, "endContact", "(J)V" );
            preSolveID = env->GetMethodID( worldClass, "preSolve", "(JJ)V" );
            postSolveID = env->GetMethodID( worldClass, "postSolve", "(JJ)V" );
            reportFixtureID = env->GetMethodID(worldClass, "reportFixture", "(J)Z" );
            reportRayFixtureID = env->GetMethodID(worldClass, "reportRayFixture", "(JFFFFF)F" );
            shouldCollideID = env->GetMethodID( worldClass, "contactFilter", "(JJ)Z");
            useDefaultContactFilterID = env->GetMethodID( worldClass, "getUseDefaultContactFilter", "()Z" );
        }

        b2World* world = new b2World( b2Vec2( gravityX, gravityY ));
        world->SetAllowSleeping( doSleep );
        return (jlong)world;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long jniCreateBody( long addr, int type, float positionX, float positionY, float angle,
                                              float linearVelocityX,
                                              float linearVelocityY, float angularVelocity, float linearDamping,
                                              float angularDamping, bool allowSleep, bool awake,
                                              bool fixedRotation, bool bullet, bool active, float inertiaScale );
    /*
        b2BodyDef bodyDef;
        bodyDef.type = getBodyType(type);
        bodyDef.position.Set( positionX, positionY );
        bodyDef.angle = angle;
        bodyDef.linearVelocity.Set( linearVelocityX, linearVelocityY );
        bodyDef.angularVelocity = angularVelocity;
        bodyDef.linearDamping = linearDamping;
        bodyDef.angularDamping = angularDamping;
        bodyDef.allowSleep = allowSleep;
        bodyDef.awake = awake;
        bodyDef.fixedRotation = fixedRotation;
        bodyDef.bullet = bullet;
        bodyDef.active = active;
        bodyDef.gravityScale = inertiaScale;

        b2World* world = (b2World*)addr;
        b2Body* body = world->CreateBody( &bodyDef );
        return (jlong)body;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniDestroyBody( long addr, long bodyAddr );
    /*
        b2World* world = (b2World*)addr;
        b2Body* body = (b2Body*)bodyAddr;
        CustomContactFilter contactFilter(env, object);
        CustomContactListener contactListener(env,object);
        world->SetContactFilter(&contactFilter);
        world->SetContactListener(&contactListener);
        world->DestroyBody(body);
        world->SetContactFilter(&defaultFilter);
        world->SetContactListener(0);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniDestroyFixture( long addr, long bodyAddr, long fixtureAddr );
    /*
        b2World* world = (b2World*)(addr);
        b2Body* body = (b2Body*)(bodyAddr);
        b2Fixture* fixture = (b2Fixture*)(fixtureAddr);
        CustomContactFilter contactFilter(env, object);
        CustomContactListener contactListener(env, object);
        world->SetContactFilter(&contactFilter);
        world->SetContactListener(&contactListener);
        body->DestroyFixture(fixture);
        world->SetContactFilter(&defaultFilter);
        world->SetContactListener(0);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniDeactivateBody( long addr, long bodyAddr );
    /*
        b2World* world = (b2World*)(addr);
        b2Body* body = (b2Body*)(bodyAddr);
        CustomContactFilter contactFilter(env, object);
        CustomContactListener contactListener(env, object);
        world->SetContactFilter(&contactFilter);
        world->SetContactListener(&contactListener);
        body->SetActive(false);
        world->SetContactFilter(&defaultFilter);
        world->SetContactListener(0);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long jniCreateWheelJoint( long addr, long bodyA, long bodyB, bool collideConnected,
                                                    float localAnchorAx,
                                                    float localAnchorAy, float localAnchorBx, float localAnchorBy,
                                                    float localAxisAx, float localAxisAy, bool enableMotor,
                                                    float maxMotorTorque, float motorSpeed, float frequencyHz,
                                                    float dampingRatio );
    /*
        b2World* world = (b2World*)addr;
        b2WheelJointDef def;
        def.bodyA = (b2Body*)bodyA;
        def.bodyB = (b2Body*)bodyB;
        def.collideConnected = collideConnected;
        def.localAnchorA = b2Vec2(localAnchorAX, localAnchorAY);
        def.localAnchorB = b2Vec2(localAnchorBX, localAnchorBY);
        def.localAxisA = b2Vec2(localAxisAX, localAxisAY);
        def.enableMotor = enableMotor;
        def.maxMotorTorque = maxMotorTorque;
        def.motorSpeed = motorSpeed;
        def.frequencyHz = frequencyHz;
        def.dampingRatio = dampingRatio;

        return (jlong)world->CreateJoint(&def);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long jniCreateRopeJoint( long addr, long bodyA, long bodyB, bool collideConnected,
                                                   float localAnchorAx,
                                                   float localAnchorAy, float localAnchorBx, float localAnchorBy,
                                                   float maxLength );
    /*
        b2World* world = (b2World*)addr;
        b2RopeJointDef def;
        def.bodyA = (b2Body*)bodyA;
        def.bodyB = (b2Body*)bodyB;
        def.collideConnected = collideConnected;
        def.localAnchorA = b2Vec2(localAnchorAX, localAnchorAY);
        def.localAnchorB = b2Vec2(localAnchorBX, localAnchorBY);
        def.maxLength = maxLength;

        return (jlong)world->CreateJoint(&def);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long jniCreateDistanceJoint( long addr, long bodyA, long bodyB, bool collideConnected,
                                                       float localAnchorAx,
                                                       float localAnchorAy, float localAnchorBx, float localAnchorBy,
                                                       float length, float frequencyHz, float dampingRatio );
    /*
       b2World* world = (b2World*)addr;
       b2DistanceJointDef def;
       def.bodyA = (b2Body*)bodyA;
       def.bodyB = (b2Body*)bodyB;
       def.collideConnected = collideConnected;
       def.localAnchorA = b2Vec2(localAnchorAX, localAnchorAY);
       def.localAnchorB = b2Vec2(localAnchorBX, localAnchorBY);
       def.length = length;
       def.frequencyHz = frequencyHz;
       def.dampingRatio = dampingRatio;

       return (jlong)world->CreateJoint(&def);
   */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long jniCreateFrictionJoint( long addr, long bodyA, long bodyB, bool collideConnected,
                                                       float localAnchorAx,
                                                       float localAnchorAy, float localAnchorBx, float localAnchorBy,
                                                       float maxForce, float maxTorque );
    /*
        b2World* world = (b2World*)addr;
        b2FrictionJointDef def;
        def.bodyA = (b2Body*)bodyA;
        def.bodyB = (b2Body*)bodyB;
        def.collideConnected = collideConnected;
        def.localAnchorA = b2Vec2(localAnchorAX, localAnchorAY);
        def.localAnchorB = b2Vec2(localAnchorBX, localAnchorBY);
        def.maxForce = maxForce;
        def.maxTorque = maxTorque;
        return (jlong)world->CreateJoint(&def);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long jniCreateGearJoint( long addr, long bodyA, long bodyB, bool collideConnected,
                                                   long joint1,
                                                   long joint2,
                                                   float ratio );
    /*
       b2World* world = (b2World*)addr;
       b2GearJointDef def;
       def.bodyA = (b2Body*)bodyA;
       def.bodyB = (b2Body*)bodyB;
       def.collideConnected = collideConnected;
       def.joint1 = (b2Joint*)joint1;
       def.joint2 = (b2Joint*)joint2;
       def.ratio = ratio;
       return (jlong)world->CreateJoint(&def);
   */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long jniCreateMotorJoint( long addr, long bodyA, long bodyB, bool collideConnected,
                                                    float linearOffsetX,
                                                    float linearOffsetY, float angularOffset, float maxForce,
                                                    float maxTorque,
                                                    float correctionFactor );
    /*
        b2World* world = (b2World*)addr;
        b2MotorJointDef def;
        def.bodyA = (b2Body*)bodyA;
        def.bodyB = (b2Body*)bodyB;
        def.collideConnected = collideConnected;
        def.linearOffset = b2Vec2( linearOffsetX, linearOffsetY );
        def.angularOffset = angularOffset;
        def.maxForce = maxForce;
        def.maxTorque = maxTorque;
        def.correctionFactor = correctionFactor;
        return (jlong)world->CreateJoint(&def);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long jniCreateMouseJoint( long addr, long bodyA, long bodyB, bool collideConnected,
                                                    float targetX,
                                                    float targetY, float maxForce, float frequencyHz,
                                                    float dampingRatio );
    /*
        b2World* world = (b2World*)addr;
        b2MouseJointDef def;
        def.bodyA = (b2Body*)bodyA;
        def.bodyB = (b2Body*)bodyB;
        def.collideConnected = collideConnected;
        def.target = b2Vec2( targetX, targetY );
        def.maxForce = maxForce;
        def.frequencyHz = frequencyHz;
        def.dampingRatio = dampingRatio;
        return (jlong)world->CreateJoint(&def);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long jniCreatePrismaticJoint( long addr, long bodyA, long bodyB, bool collideConnected,
                                                        float localAnchorAx,
                                                        float localAnchorAy, float localAnchorBx, float localAnchorBy,
                                                        float localAxisAx, float localAxisAy, float referenceAngle,
                                                        bool enableLimit, float lowerTranslation,
                                                        float upperTranslation,
                                                        bool enableMotor, float maxMotorForce,
                                                        float motorSpeed );
    /*
        b2World* world = (b2World*)addr;
        b2PrismaticJointDef def;
        def.bodyA = (b2Body*)bodyA;
        def.bodyB = (b2Body*)bodyB;
        def.collideConnected = collideConnected;
        def.localAnchorA = b2Vec2(localAnchorAX, localAnchorAY);
        def.localAnchorB = b2Vec2(localAnchorBX, localAnchorBY);
        def.localAxisA = b2Vec2( localAxisAX, localAxisAY );
        def.referenceAngle = referenceAngle;
        def.enableLimit = enableLimit;
        def.lowerTranslation = lowerTranslation;
        def.upperTranslation = upperTranslation;
        def.enableMotor = enableMotor;
        def.maxMotorForce = maxMotorForce;
        def.motorSpeed = motorSpeed;
        return (jlong)world->CreateJoint(&def);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long jniCreatePulleyJoint( long addr, long bodyA, long bodyB, bool collideConnected,
                                                     float groundAnchorAx, float groundAnchorAy,
                                                     float groundAnchorBx, float groundAnchorBy,
                                                     float localAnchorAx, float localAnchorAy,
                                                     float localAnchorBx, float localAnchorBy,
                                                     float lengthA, float lengthB,
                                                     float ratio );
    /*
        b2World* world = (b2World*)addr;
        b2PulleyJointDef def;
        def.bodyA = (b2Body*)bodyA;
        def.bodyB = (b2Body*)bodyB;
        def.collideConnected = collideConnected;
        def.groundAnchorA = b2Vec2( groundAnchorAX, groundAnchorAY );
        def.groundAnchorB = b2Vec2( groundAnchorBX, groundAnchorBY );
        def.localAnchorA = b2Vec2(localAnchorAX, localAnchorAY);
        def.localAnchorB = b2Vec2(localAnchorBX, localAnchorBY);
        def.lengthA = lengthA;
        def.lengthB = lengthB;
        def.ratio = ratio;

        return (jlong)world->CreateJoint(&def);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long jniCreateRevoluteJoint( long addr, long bodyA, long bodyB, bool collideConnected,
                                                       float localAnchorAx,
                                                       float localAnchorAy, float localAnchorBx, float localAnchorBy,
                                                       float referenceAngle, bool enableLimit, float lowerAngle,
                                                       float upperAngle, bool enableMotor, float motorSpeed,
                                                       float maxMotorTorque );
    /*
        b2World* world = (b2World*)addr;
        b2RevoluteJointDef def;
        def.bodyA = (b2Body*)bodyA;
        def.bodyB = (b2Body*)bodyB;
        def.collideConnected = collideConnected;
        def.localAnchorA = b2Vec2(localAnchorAX, localAnchorAY);
        def.localAnchorB = b2Vec2(localAnchorBX, localAnchorBY);
        def.referenceAngle = referenceAngle;
        def.enableLimit = enableLimit;
        def.lowerAngle = lowerAngle;
        def.upperAngle = upperAngle;
        def.enableMotor = enableMotor;
        def.motorSpeed = motorSpeed;
        def.maxMotorTorque = maxMotorTorque;
        return (jlong)world->CreateJoint(&def);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long jniCreateWeldJoint( long addr, long bodyA, long bodyB, bool collideConnected,
                                                   float localAnchorAx, float localAnchorAy,
                                                   float localAnchorBx, float localAnchorBy,
                                                   float referenceAngle, float frequencyHz, float dampingRatio );
    /*
        b2World* world = (b2World*)addr;
        b2WeldJointDef def;
        def.bodyA = (b2Body*)bodyA;
        def.bodyB = (b2Body*)bodyB;
        def.collideConnected = collideConnected;
        def.localAnchorA = b2Vec2(localAnchorAx, localAnchorAy);
        def.localAnchorB = b2Vec2(localAnchorBx, localAnchorBy);
        def.referenceAngle = referenceAngle;
        def.frequencyHz = frequencyHz;
        def.dampingRatio = dampingRatio;

        return (jlong)world->CreateJoint(&def);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniDestroyJoint( long addr, long jointAddr );
    /*
        b2World* world = (b2World*)addr;
        b2Joint* joint = (b2Joint*)jointAddr;
        CustomContactFilter contactFilter(env, object);
        CustomContactListener contactListener(env,object);
        world->SetContactFilter(&contactFilter);
        world->SetContactListener(&contactListener);
        world->DestroyJoint( joint );
        world->SetContactFilter(&defaultFilter);
        world->SetContactListener(0);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniStep( long addr, float timeStep, int velocityIterations, int positionIterations );
    /*
        b2World* world = (b2World*)addr;
        CustomContactFilter contactFilter(env, object);
        CustomContactListener contactListener(env,object);
        world->SetContactFilter(&contactFilter);
        world->SetContactListener(&contactListener);
        world->Step( timeStep, velocityIterations, positionIterations );
        world->SetContactFilter(&defaultFilter);
        world->SetContactListener(0);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniClearForces( long addr );
    /*
        b2World* world = (b2World*)addr;
        world->ClearForces();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetWarmStarting( long addr, bool flag );
    /*
        b2World* world = (b2World*)addr;
        world->SetWarmStarting(flag);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetContiousPhysics( long addr, bool flag );
    /*
        b2World* world = (b2World*)addr;
        world->SetContinuousPhysics(flag);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern int jniGetProxyCount( long addr );
    /*
        b2World* world = (b2World*)addr;
        return world->GetProxyCount();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern int jniGetBodyCount( long addr );
    /*
        b2World* world = (b2World*)addr;
        return world->GetBodyCount();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern int jniGetJointcount( long addr );
    /*
        b2World* world = (b2World*)addr;
        return world->GetJointCount();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern int jniGetContactCount( long addr );
    /*
        b2World* world = (b2World*)addr;
        return world->GetContactCount();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetGravity( long addr, float gravityX, float gravityY );
    /*
        b2World* world = (b2World*)addr;
        world->SetGravity( b2Vec2( gravityX, gravityY ) );
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetGravity( long addr, float[] gravity );
    /*
        b2World* world = (b2World*)addr;
        b2Vec2 g = world->GetGravity();
        gravity[0] = g.X;
        gravity[1] = g.Y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern bool jniIsLocked( long addr );
    /*
        b2World* world = (b2World*)addr;
        return world->IsLocked();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetAutoClearForces( long addr, bool flag );
    /*
        b2World* world = (b2World*)addr;
        world->SetAutoClearForces(flag);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern bool jniGetAutoClearForces( long addr );
    /*
        b2World* world = (b2World*)addr;
        return world->GetAutoClearForces();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniQueryAABB( long addr, float lowX, float lowY, float upX, float upY );
    /*
        b2World* world = (b2World*)addr;
        b2AABB aabb;
        aabb.lowerBound = b2Vec2( lowX, lowY );
        aabb.upperBound = b2Vec2( upX, upY );

        CustomQueryCallback callback( env, object );
        world->QueryAABB( &callback, aabb );
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetContactList( long addr, long[] contacts );
    /*
        b2World* world = (b2World*)addr;

        b2Contact* contact = world->GetContactList();
        int i = 0;
        while( contact != 0 )
        {
            contacts[i++] = (long long)contact;
            contact = contact->GetNext();
        }
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniDispose( long addr );
    /*
        b2World* world = (b2World*)(addr);
        delete world;
    */

    /** Sets the box2d velocity threshold globally, for all World instances.
     * <param name="threshold the threshold, default 1.0f */
    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    public static extern void setVelocityThreshold( float threshold );
    /*
        b2_velocityThreshold = threshold;
    */

    /** @return the global box2d velocity threshold. */
    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    public static extern float getVelocityThreshold();
    /*
        return b2_velocityThreshold;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniRayCast( long addr, float aX, float aY, float bX, float bY );
    /*
        b2World *world = (b2World*)addr;
        CustomRayCastCallback callback( env, object );
        world->RayCast( &callback, b2Vec2(aX,aY), b2Vec2(bX,bY) );
    */
}

// ============================================================================
// ============================================================================
