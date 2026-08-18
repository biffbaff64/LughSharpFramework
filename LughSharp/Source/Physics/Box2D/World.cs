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

    /** the address of the world instance **/
    public long Addr;

    // NOTE:
    // This class in LibGDX used a LongMap<> for the following.
    // LongMap uses a long key and a value of type T.
    // I've switched to a Dictionary<> for this.

    /** all known bodies **/
    public readonly Dictionary< long, Body > Bodies = new( 100 );

    /** all known fixtures **/
    public readonly Dictionary< long, Fixture > Fixtures = new( 100 );

    /** all known joints **/
    public readonly Dictionary< long, Joint > Joints = new( 100 );

    /** Contact listener **/
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

    /** Construct a world object.
     * @param gravity the world gravity vector.
     * @param doSleep improve performance by not simulating inactive bodies. */
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

    /** Register a destruction listener. The listener is owned by you and must remain in scope. */
    public void SetDestructionListener( IDestructionListener listener )
    {
    }

    /** Register a contact filter to provide specific control over collision. Otherwise the default filter is used
     * (b2_defaultFilter). The listener is owned by you and must remain in scope. */
    public void SetContactFilter( IContactFilter? filter )
    {
        this._contactFilter = filter;
        SetUseDefaultContactFilter( filter == null );
    }

    /** Internal method called from JNI
     * @return whether the native default IContactFilter should be used */
    private bool GetUseDefaultContactFilter()
    {
        return _useDefaultContactFilter;
    }

    /** Sets flag to tell the native code not to call the Java World class if use is true **/
    private void SetUseDefaultContactFilter( bool use )
    {
        _useDefaultContactFilter = use;
    }

    /** Register a contact event listener. The listener is owned by you and must remain in scope. */
    public void SetContactListener( IContactListener listener )
    {
        this.ContactListener = listener;
    }

    /** Create a rigid body given a definition. No reference to the definition is retained.
     * Bodies created by this method are pooled internally by the World object.
     * They will be freed upon calling {@link World#destroyBody(Body)}
     * @see Pool
     * @warning This function is locked during callbacks. */
    public Body CreateBody( BodyDef def )
    {
        long bodyAddr = JniCreateBody
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

    /** Destroy a rigid body given a definition. No reference to the definition is retained. This function is locked during
     * callbacks.
     * @warning This automatically deletes all associated shapes and joints.
     * @warning This function is locked during callbacks. */
    public void DestroyBody( Body body )
    {
        List< JointEdge > jointList = body.GetJointList();

        while ( jointList.Count > 0 )
        {
            DestroyJoint( body.GetJointList()[ 0 ].Joint );
        }

        JniDestroyBody( Addr, body.Addr );

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

    /** Internal method for fixture destruction with notifying custom
     * contact listener
     * @param body
     * @param fixture */
    void DestroyFixture( Body body, Fixture fixture )
    {
        JniDestroyFixture( Addr, body.Addr, fixture.Addr );
    }

    /** Internal method for body deactivation with notifying custom
     * contact listener
     * @param body */
    void DeactivateBody( Body body )
    {
        JniDeactivateBody( Addr, body.Addr );
    }

    /** Create a joint to constrain bodies together. No reference to the definition is retained. This may cause the connected bodies
     * to cease colliding.
     * @warning This function is locked during callbacks. */
    public Joint CreateJoint( JointDef def )
    {
        long   jointAddr = CreateProperJoint( def );
        Joint? joint     = null;

        if ( def.Type == JointType.DistanceJoint ) joint = new DistanceJoint( this, jointAddr );
        if ( def.Type == JointType.FrictionJoint ) joint = new FrictionJoint( this, jointAddr );

        if ( def.Type == JointType.GearJoint )
        {
            joint = new GearJoint( this, jointAddr, ( ( GearJointDef )def ).joint1, ( ( GearJointDef )def ).joint2 );
        }
        
        if ( def.Type == JointType.MotorJoint ) joint     = new MotorJoint( this, jointAddr );
        if ( def.Type == JointType.MouseJoint ) joint     = new MouseJoint( this, jointAddr );
        if ( def.Type == JointType.PrismaticJoint ) joint = new PrismaticJoint( this, jointAddr );
        if ( def.Type == JointType.PulleyJoint ) joint    = new PulleyJoint( this, jointAddr );
        if ( def.Type == JointType.RevoluteJoint ) joint  = new RevoluteJoint( this, jointAddr );
        if ( def.Type == JointType.RopeJoint ) joint      = new RopeJoint( this, jointAddr );
        if ( def.Type == JointType.WeldJoint ) joint      = new WeldJoint( this, jointAddr );
        if ( def.Type == JointType.WheelJoint ) joint     = new WheelJoint( this, jointAddr );

        if ( joint == null ) throw new LughRuntimeException( "Unknown joint type: " + def.Type );

        Joints.put( joint.Addr, joint );
        JointEdge jointEdgeA = new JointEdge( def.bodyB, joint );
        JointEdge jointEdgeB = new JointEdge( def.bodyA, joint );

        joint.jointEdgeA = jointEdgeA;
        joint.jointEdgeB = jointEdgeB;
        
        def.bodyA.joints.add( jointEdgeA );
        def.bodyB.joints.add( jointEdgeB );

        return joint;
    }

    private long CreateProperJoint( JointDef def )
    {
        if ( def.type == JointType.DistanceJoint )
        {
            DistanceJointDef d = ( DistanceJointDef )def;

            return JniCreateDistanceJoint
                (
                 Addr,
                 d.bodyA.Addr,
                 d.bodyB.Addr,
                 d.collideConnected,
                 d.localAnchorA.X,
                 d.localAnchorA.Y,
                 d.localAnchorB.X,
                 d.localAnchorB.Y,
                 d.length,
                 d.frequencyHz,
                 d.dampingRatio
                );
        }

        if ( def.type == JointType.FrictionJoint )
        {
            FrictionJointDef d = ( FrictionJointDef )def;

            return JniCreateFrictionJoint
                (
                 Addr,
                 d.bodyA.Addr,
                 d.bodyB.Addr,
                 d.collideConnected,
                 d.localAnchorA.X,
                 d.localAnchorA.Y,
                 d.localAnchorB.X,
                 d.localAnchorB.Y,
                 d.maxForce,
                 d.maxTorque
                );
        }

        if ( def.type == JointType.GearJoint )
        {
            GearJointDef d = ( GearJointDef )def;

            return JniCreateGearJoint
                ( Addr, d.bodyA.Addr, d.bodyB.Addr, d.collideConnected, d.joint1.Addr, d.joint2.Addr, d.ratio );
        }

        if ( def.type == JointType.MotorJoint )
        {
            MotorJointDef d = ( MotorJointDef )def;

            return JniCreateMotorJoint
                (
                 Addr,
                 d.bodyA.Addr,
                 d.bodyB.Addr,
                 d.collideConnected,
                 d.linearOffset.X,
                 d.linearOffset.Y,
                 d.angularOffset,
                 d.maxForce,
                 d.maxTorque,
                 d.correctionFactor
                );
        }

        if ( def.type == JointType.MouseJoint )
        {
            MouseJointDef d = ( MouseJointDef )def;

            return JniCreateMouseJoint
                (
                 Addr,
                 d.bodyA.Addr,
                 d.bodyB.Addr,
                 d.collideConnected,
                 d.target.X,
                 d.target.Y,
                 d.maxForce,
                 d.frequencyHz,
                 d.dampingRatio
                );
        }

        if ( def.type == JointType.PrismaticJoint )
        {
            PrismaticJointDef d = ( PrismaticJointDef )def;

            return JniCreatePrismaticJoint
                (
                 Addr,
                 d.bodyA.Addr,
                 d.bodyB.Addr,
                 d.collideConnected,
                 d.localAnchorA.X,
                 d.localAnchorA.Y,
                 d.localAnchorB.X,
                 d.localAnchorB.Y,
                 d.localAxisA.X,
                 d.localAxisA.Y,
                 d.referenceAngle,
                 d.enableLimit,
                 d.lowerTranslation,
                 d.upperTranslation,
                 d.enableMotor,
                 d.maxMotorForce,
                 d.motorSpeed
                );
        }

        if ( def.type == JointType.PulleyJoint )
        {
            PulleyJointDef d = ( PulleyJointDef )def;

            return JniCreatePulleyJoint
                (
                 Addr,
                 d.bodyA.Addr,
                 d.bodyB.Addr,
                 d.collideConnected,
                 d.groundAnchorA.X,
                 d.groundAnchorA.Y,
                 d.groundAnchorB.X,
                 d.groundAnchorB.Y,
                 d.localAnchorA.X,
                 d.localAnchorA.Y,
                 d.localAnchorB.X,
                 d.localAnchorB.Y,
                 d.lengthA,
                 d.lengthB,
                 d.ratio
                );
        }

        if ( def.type == JointType.RevoluteJoint )
        {
            RevoluteJointDef d = ( RevoluteJointDef )def;

            return JniCreateRevoluteJoint
                (
                 Addr,
                 d.bodyA.Addr,
                 d.bodyB.Addr,
                 d.collideConnected,
                 d.localAnchorA.X,
                 d.localAnchorA.Y,
                 d.localAnchorB.X,
                 d.localAnchorB.Y,
                 d.referenceAngle,
                 d.enableLimit,
                 d.lowerAngle,
                 d.upperAngle,
                 d.enableMotor,
                 d.motorSpeed,
                 d.maxMotorTorque
                );
        }

        if ( def.type == JointType.RopeJoint )
        {
            RopeJointDef d = ( RopeJointDef )def;

            return JniCreateRopeJoint
                (
                 Addr,
                 d.bodyA.Addr,
                 d.bodyB.Addr,
                 d.collideConnected,
                 d.localAnchorA.X,
                 d.localAnchorA.Y,
                 d.localAnchorB.X,
                 d.localAnchorB.Y,
                 d.maxLength
                );
        }

        if ( def.type == JointType.WeldJoint )
        {
            WeldJointDef d = ( WeldJointDef )def;

            return JniCreateWeldJoint
                (
                 Addr,
                 d.bodyA.Addr,
                 d.bodyB.Addr,
                 d.collideConnected,
                 d.localAnchorA.X,
                 d.localAnchorA.Y,
                 d.localAnchorB.X,
                 d.localAnchorB.Y,
                 d.referenceAngle,
                 d.frequencyHz,
                 d.dampingRatio
                );
        }

        if ( def.type == JointType.WheelJoint )
        {
            WheelJointDef d = ( WheelJointDef )def;

            return JniCreateWheelJoint
                (
                 Addr,
                 d.bodyA.Addr,
                 d.bodyB.Addr,
                 d.collideConnected,
                 d.localAnchorA.X,
                 d.localAnchorA.Y,
                 d.localAnchorB.X,
                 d.localAnchorB.Y,
                 d.localAxisA.X,
                 d.localAxisA.Y,
                 d.enableMotor,
                 d.maxMotorTorque,
                 d.motorSpeed,
                 d.frequencyHz,
                 d.dampingRatio
                );
        }

        return 0;
    }

    /** Destroy a joint. This may cause the connected bodies to begin colliding.
     * @warning This function is locked during callbacks. */
    public void DestroyJoint( Joint joint )
    {
        joint.setUserData( null );
        Joints.remove( joint.Addr );
        joint.jointEdgeA.other.joints.removeValue( joint.jointEdgeB, true );
        joint.jointEdgeB.other.joints.removeValue( joint.jointEdgeA, true );
        JniDestroyJoint( Addr, joint.Addr );
    }

    /** Take a time step. This performs collision detection, integration, and constraint solution.
     * @param timeStep the amount of time to simulate, this should not vary.
     * @param velocityIterations for the velocity constraint solver.
     * @param positionIterations for the position constraint solver. */
    public void Step( float timeStep, int velocityIterations, int positionIterations )
    {
        JniStep( Addr, timeStep, velocityIterations, positionIterations );
    }

    /** Manually clear the force buffer on all bodies. By default, forces are cleared automatically after each call to Step. The
     * default behavior is modified by calling SetAutoClearForces. The purpose of this function is to support sub-stepping.
     * Sub-stepping is often used to maintain a fixed sized time step under a variable frame-rate. When you perform sub-stepping
     * you will disable auto clearing of forces and instead call ClearForces after all sub-steps are complete in one pass of your
     * game loop. {@link #setAutoClearForces(bool)} */
    public void ClearForces()
    {
        JniClearForces( Addr );
    }

    /** Enable/disable warm starting. For testing. */
    public void SetWarmStarting( bool flag )
    {
        JniSetWarmStarting( Addr, flag );
    }

    /** Enable/disable continuous physics. For testing. */
    public void SetContinuousPhysics( bool flag )
    {
        JniSetContiousPhysics( Addr, flag );
    }

    /** Get the number of broad-phase proxies. */
    public int GetProxyCount()
    {
        return JniGetProxyCount( Addr );
    }

    /** Get the number of bodies. */
    public int GetBodyCount()
    {
        return JniGetBodyCount( Addr );
    }

    /** Get the number of fixtures. */
    public int GetFixtureCount()
    {
        return fixtures.size;
    }

    /** Get the number of joints. */
    public int GetJointCount()
    {
        return JniGetJointcount( Addr );
    }

    /** Get the number of contacts (each may have 0 or more contact points). */
    public int GetContactCount()
    {
        return JniGetContactCount( Addr );
    }

    /** Change the global gravity vector. */
    public void SetGravity( Vector2 gravity )
    {
        JniSetGravity( Addr, gravity.X, gravity.Y );
    }

    public Vector2 GetGravity()
    {
        JniGetGravity( Addr, _tmpGravity );
        _gravity.X = _tmpGravity[ 0 ];
        _gravity.Y = _tmpGravity[ 1 ];

        return _gravity;
    }

    /** Is the world locked (in the middle of a time step). */
    public bool IsLocked()
    {
        return JniIsLocked( Addr );
    }

    /** Set flag to control automatic clearing of forces after each time step. */
    public void SetAutoClearForces( bool flag )
    {
        JniSetAutoClearForces( Addr, flag );
    }

    /** Get the flag that controls automatic clearing of forces after each time step. */
    public bool GetAutoClearForces()
    {
        return JniGetAutoClearForces( Addr );
    }

    /** Query the world for all fixtures that potentially overlap the provided AABB.
     * @param callback a user implemented callback class.
     * @param lowerX the x coordinate of the lower left corner
     * @param lowerY the y coordinate of the lower left corner
     * @param upperX the x coordinate of the upper right corner
     * @param upperY the y coordinate of the upper right corner */
    public void QueryAabb( QueryCallback callback, float lowerX, float lowerY, float upperX, float upperY )
    {
        _queryCallback = callback;
        JniQueryAABB( Addr, lowerX, lowerY, upperX, upperY );
    }

//
// /// Ray-cast the world for all fixtures in the path of the ray. Your callback
// /// controls whether you get the closest point, any point, or n-points.
// /// The ray-cast ignores shapes that contain the starting point.
// /// @param callback a user implemented callback class.
// /// @param point1 the ray starting point
// /// @param point2 the ray ending point
// void RayCast(b2RayCastCallback* callback, const b2Vec2& point1, const b2Vec2& point2) const;
//
// /// Get the world contact list. With the returned contact, use b2Contact::GetNext to get
// /// the next contact in the world list. A NULL contact indicates the end of the list.
// /// @return the head of the world contact list.
// /// @warning contacts are
// b2Contact* GetContactList();

    /** Returns the list of {@link Contact} instances produced by the last call to {@link #step(float, int, int)}. Note that the
     * returned list will have O(1) access times when using indexing. contacts are created and destroyed in the middle of a time
     * step. Use {@link IContactListener} to avoid missing contacts
     * @return the contact list */
    public Array< Contact > GetContactList()
    {
        int numContacts = GetContactCount();

        if ( numContacts > _contactAddrs.length )
        {
            int newSize = 2 * numContacts;
            _contactAddrs = new long[ newSize ];
            _contacts.ensureCapacity( newSize );
            _freeContacts.ensureCapacity( newSize );
        }

        if ( numContacts > _freeContacts.size )
        {
            int freeConts = _freeContacts.size;
            for ( int i = 0; i < numContacts - freeConts; i++ )
                _freeContacts.add( new Contact( this, 0 ) );
        }

        JniGetContactList( Addr, _contactAddrs );

        _contacts.clear();

        for ( int i = 0; i < numContacts; i++ )
        {
            Contact contact = _freeContacts.get( i );
            contact.Addr = _contactAddrs[ i ];
            _contacts.add( contact );
        }

        return _contacts;
    }

    /** @param bodies an Array in which to place all bodies currently in the simulation */
    public void GetBodies( Array< Body > bodies )
    {
        bodies.clear();
        bodies.ensureCapacity( this.Bodies.size );

        for ( Iterator< Body > iter = this.Bodies.values(); iter.hasNext(); )
        {
            bodies.add( iter.next() );
        }
    }

    /** @param fixtures an Array in which to place all fixtures currently in the simulation */
    public void GetFixtures( Array< Fixture > fixtures )
    {
        fixtures.clear();
        fixtures.ensureCapacity( this.fixtures.size );

        for ( Iterator< Fixture > iter = this.fixtures.values(); iter.hasNext(); )
        {
            fixtures.add( iter.next() );
        }
    }

    /** @param joints an Array in which to place all joints currently in the simulation */
    public void GetJoints( Array< Joint > joints )
    {
        joints.clear();
        joints.ensureCapacity( this.Joints.size );

        for ( Iterator< Joint > iter = this.Joints.values(); iter.hasNext(); )
        {
            joints.add( iter.next() );
        }
    }

    public void Dispose()
    {
        JniDispose( Addr );
    }

    /** Internal method called from JNI in case a contact happens
     * @param fixtureA
     * @param fixtureB
     * @return whether the things collided */
    private bool ContactFilter( long fixtureA, long fixtureB )
    {
        if ( contactFilter != null )
            return contactFilter.ShouldCollide( fixtures.get( fixtureA ), fixtures.get( fixtureB ) );
        else
        {
            Filter filterA = fixtures.get( fixtureA ).getFilterData();
            Filter filterB = fixtures.get( fixtureB ).getFilterData();

            if ( filterA.groupIndex == filterB.groupIndex && filterA.groupIndex != 0 )
            {
                return filterA.groupIndex > 0;
            }

            bool collide = ( filterA.maskBits & filterB.categoryBits ) != 0
                        && ( filterA.categoryBits & filterB.maskBits ) != 0;

            return collide;
        }
    }

    private void BeginContact( long contactAddr )
    {
        if ( ContactListener != null )
        {
            _contact.Addr = contactAddr;
            ContactListener.beginContact( _contact );
        }
    }

    private void EndContact( long contactAddr )
    {
        if ( ContactListener != null )
        {
            _contact.Addr = contactAddr;
            ContactListener.endContact( _contact );
        }
    }

    private void PreSolve( long contactAddr, long manifoldAddr )
    {
        if ( ContactListener != null )
        {
            _contact.Addr  = contactAddr;
            _manifold.Addr = manifoldAddr;
            ContactListener.preSolve( _contact, _manifold );
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
            return _queryCallback.ReportFixture( Fixtures[ addr ] );
        else
            return false;
    }

    /** Ray-cast the world for all fixtures in the path of the ray. The ray-cast ignores shapes that contain the starting point.
     * @param callback a user implemented callback class.
     * @param point1 the ray starting point
     * @param point2 the ray ending point */
    public void RayCast( IRayCastCallback callback, Vector2 point1, Vector2 point2 )
    {
        RayCast( callback, point1.X, point1.Y, point2.X, point2.Y );
    }

    /** Ray-cast the world for all fixtures in the path of the ray. The ray-cast ignores shapes that contain the starting point.
     * @param callback a user implemented callback class.
     * @param point1X the ray starting point X
     * @param point1Y the ray starting point Y
     * @param point2X the ray ending point X
     * @param point2Y the ray ending point Y */
    public void RayCast( IRayCastCallback callback, float point1X, float point1Y, float point2X, float point2Y )
    {
        _rayCastCallback = callback;
        JniRayCast( Addr, point1X, point1Y, point2X, point2Y );
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
        else
        {
            return 0.0f;
        }
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
    private static extern long JniCreateBody( long addr, int type, float positionX, float positionY, float angle,
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
    private static extern void JniDestroyBody( long addr, long bodyAddr );
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
    private static extern void JniDestroyFixture( long addr, long bodyAddr, long fixtureAddr );
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
    private static extern void JniDeactivateBody( long addr, long bodyAddr );
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
    private static extern long JniCreateWheelJoint( long addr, long bodyA, long bodyB, bool collideConnected,
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
    private static extern long JniCreateRopeJoint( long addr, long bodyA, long bodyB, bool collideConnected,
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
    private static extern long JniCreateDistanceJoint( long addr, long bodyA, long bodyB, bool collideConnected,
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
    private static extern long JniCreateFrictionJoint( long addr, long bodyA, long bodyB, bool collideConnected,
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
    private static extern long JniCreateGearJoint( long addr, long bodyA, long bodyB, bool collideConnected,
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
    private static extern long JniCreateMotorJoint( long addr, long bodyA, long bodyB, bool collideConnected,
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
    private static extern long JniCreateMouseJoint( long addr, long bodyA, long bodyB, bool collideConnected,
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
    private static extern long JniCreatePrismaticJoint( long addr, long bodyA, long bodyB, bool collideConnected,
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
    private static extern long JniCreatePulleyJoint( long addr, long bodyA, long bodyB, bool collideConnected,
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
    private static extern long JniCreateRevoluteJoint( long addr, long bodyA, long bodyB, bool collideConnected,
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
    private static extern long JniCreateWeldJoint( long addr, long bodyA, long bodyB, bool collideConnected,
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
    private static extern void JniDestroyJoint( long addr, long jointAddr );
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
    private static extern void JniStep( long addr, float timeStep, int velocityIterations, int positionIterations );
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
    private static extern void JniClearForces( long addr );
    /*
        b2World* world = (b2World*)addr;
        world->ClearForces();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void JniSetWarmStarting( long addr, bool flag );
    /*
        b2World* world = (b2World*)addr;
        world->SetWarmStarting(flag);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void JniSetContiousPhysics( long addr, bool flag );
    /*
        b2World* world = (b2World*)addr;
        world->SetContinuousPhysics(flag);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern int JniGetProxyCount( long addr );
    /*
        b2World* world = (b2World*)addr;
        return world->GetProxyCount();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern int JniGetBodyCount( long addr );
    /*
        b2World* world = (b2World*)addr;
        return world->GetBodyCount();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern int JniGetJointcount( long addr );
    /*
        b2World* world = (b2World*)addr;
        return world->GetJointCount();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern int JniGetContactCount( long addr );
    /*
        b2World* world = (b2World*)addr;
        return world->GetContactCount();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void JniSetGravity( long addr, float gravityX, float gravityY );
    /*
        b2World* world = (b2World*)addr;
        world->SetGravity( b2Vec2( gravityX, gravityY ) );
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void JniGetGravity( long addr, float[] gravity );
    /*
        b2World* world = (b2World*)addr;
        b2Vec2 g = world->GetGravity();
        gravity[0] = g.X;
        gravity[1] = g.Y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern bool JniIsLocked( long addr );
    /*
        b2World* world = (b2World*)addr;
        return world->IsLocked();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void JniSetAutoClearForces( long addr, bool flag );
    /*
        b2World* world = (b2World*)addr;
        world->SetAutoClearForces(flag);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern bool JniGetAutoClearForces( long addr );
    /*
        b2World* world = (b2World*)addr;
        return world->GetAutoClearForces();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void JniQueryAABB( long addr, float lowX, float lowY, float upX, float upY );
    /*
        b2World* world = (b2World*)addr;
        b2AABB aabb;
        aabb.lowerBound = b2Vec2( lowX, lowY );
        aabb.upperBound = b2Vec2( upX, upY );

        CustomQueryCallback callback( env, object );
        world->QueryAABB( &callback, aabb );
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void JniGetContactList( long addr, long[] contacts );
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
    private static extern void JniDispose( long addr );
    /*
        b2World* world = (b2World*)(addr);
        delete world;
    */

    /** Sets the box2d velocity threshold globally, for all World instances.
     * @param threshold the threshold, default 1.0f */
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
    private static extern void JniRayCast( long addr, float aX, float aY, float bX, float bY );
    /*
        b2World *world = (b2World*)addr;
        CustomRayCastCallback callback( env, object );
        world->RayCast( &callback, b2Vec2(aX,aY), b2Vec2(bX,bY) );
    */
}

// ============================================================================
// ============================================================================
