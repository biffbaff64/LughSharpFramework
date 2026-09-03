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
public class Body
{
    public readonly Vector2 LocalPoint2 = new();
    public readonly Vector2 LocalVector = new();
    public readonly Vector2 LinVelWorld = new();
    public readonly Vector2 LinVelLoc   = new();

    public long    Addr     { get; set; }
    public Object? UserData { get; set; }
    public World   World    { get; init; }

    private readonly Transform         _transform      = new();
    private readonly Vector2           _position       = new();
    private readonly Vector2           _worldCenter    = new();
    private readonly Vector2           _localCenter    = new();
    private readonly Vector2           _linearVelocity = new();
    private readonly MassData          _massData       = new();
    private readonly Vector2           _localPoint     = new();
    private readonly Vector2           _worldVector    = new();
    private readonly float[]           _tmp            = new float[ 4 ];
    private readonly Buffer< byte >    _tmpBuff;
    private readonly long              _tmpBuffAddress;
    private          List< Fixture >   _fixtures = new( 2 );
    public           List< JointEdge > Joints    = new( 2 );

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Constructs a new body with the given address
    /// <param name="world"> the world </param>
    /// <param name="addr"> the address </param>
    /// </summary>
    public Body( World world, long addr )
    {
        this.World = world;
        this.Addr  = addr;

        _tmpBuff = Buffer< byte >.Allocate( 2 * 4 );
        _tmpBuff.SetOrder( ByteOrder.NativeOrder );

        _tmpBuffAddress = 0L; //TODO: // BufferUtils.GetUnsafeBufferAddress( _tmpBuff );
    }

    /// <summary>
    /// Resets this body after fetching it from the <see cref="World.FreeBodies"/> Pool. 
    /// </summary>
    public void Reset( long addr )
    {
        this.Addr     = addr;
        this.UserData = null;

        for ( int i = 0; i < _fixtures.Count; i++ )
        {
            this.World.FreeFixtures.Free( _fixtures[ i ] );
        }

        _fixtures.Clear();
        this.Joints.Clear();
    }

    /// <summary>
    /// Creates a fixture and attach it to this body. Use this function if you need to
    /// set some fixture parameters, like friction. Otherwise you can create the fixture
    /// directly from a shape. If the density is non-zero, this function automatically
    /// updates the mass of the body. Contacts are not created until the next time step.
    /// </summary>
    /// <param name="def"> the fixture definition. </param>
    /// <remarks> This function is locked during callbacks. </remarks>
    public Fixture CreateFixture( FixtureDef def )
    {
        long fixtureAddr = jniCreateFixture
            (
             Addr,
             def.Shape.Addr,
             def.Friction,
             def.Restitution,
             def.Density,
             def.IsSensor,
             def.Filter.CategoryBits,
             def.Filter.MaskBits,
             def.Filter.GroupIndex
            );

        Fixture fixture = this.World.FreeFixtures.Obtain();
        fixture.Reset( this, fixtureAddr );

        this.World.Fixtures[ fixture.Addr ] = fixture;
        this._fixtures.Add( fixture );

        return fixture;
    }

    /// <summary>
    /// Creates a fixture from a shape and attach it to this body. This is a convenience
    /// function. Use b2FixtureDef if you need to set parameters like friction, restitution,
    /// user data, or filtering. If the density is non-zero, this function automatically
    /// updates the mass of the body.
    /// </summary>
    /// <param name="shape"> the shape to be cloned. </param>
    /// <param name="density"> the shape density (set to zero for static bodies). </param>
    /// <remarks> This function is locked during callbacks. </remarks> 
    public Fixture CreateFixture( Shape shape, float density )
    {
        long fixtureAddr = jniCreateFixture( Addr, shape.Addr, density );

        Fixture fixture = this.World.FreeFixtures.Obtain();
        fixture.Reset( this, fixtureAddr );

        this.World.Fixtures[ fixture.Addr ] = fixture;
        this._fixtures.Add( fixture );

        return fixture;
    }

    /// <summary>
    /// Destroy a fixture. This removes the fixture from the broad-phase and destroys
    /// all contacts associated with this fixture. This will automatically adjust the
    /// mass of the body if the body is dynamic and the fixture has positive density. All
    /// fixtures attached to a body are implicitly destroyed when the body is destroyed.
    /// </summary>
    /// <param name="fixture"> the fixture to be removed. </param>
    /// <remarks> This function is locked during callbacks. </remarks>
    public void DestroyFixture( Fixture fixture )
    {
        this.World.DestroyFixture( this, fixture );
        fixture.UserData = null;
        this.World.Fixtures.Remove( fixture.Addr );
        this._fixtures.Remove( fixture );
        this.World.FreeFixtures.Free( fixture );
    }

    /// <summary>
    /// Set the position of the body's origin and rotation. This breaks any contacts and
    /// wakes the other bodies. Manipulating a body's transform may cause non-physical
    /// behavior.
    /// </summary>
    /// <param name="position"> the world position of the body's local origin. </param>
    /// <param name="angle"> the world rotation in radians. </param> 
    public void SetTransform( Vector2 position, float angle )
    {
        jniSetTransform( Addr, position.X, position.Y, angle );
    }

    /// <summary>
    /// Set the position of the body's origin and rotation. This breaks any contacts and
    /// wakes the other bodies. Manipulating a body's transform may cause non-physical
    /// behavior.
    /// </summary>
    /// <param name="x"> the world position on the x-axis </param>
    /// <param name="y"> the world position on the y-axis </param>
    /// <param name="angle"> the world rotation in radians. </param>
    public void SetTransform( float x, float y, float angle )
    {
        jniSetTransform( Addr, x, y, angle );
    }

    /// <summary>
    /// Get the body transform for the body's origin. 
    /// </summary>
    public Transform GetTransform()
    {
        jniGetTransform( Addr, _transform.Vals );

        return _transform;
    }

    /// <summary>
    /// Get the world body origin position.
    /// Note that the same Vector2 instance is returned each time this method is called.
    /// </summary>
    /// <returns> the world position of the body's origin. </returns>
    public Vector2 GetPosition()
    {
        jniGetPosition( Addr, _tmpBuffAddress );

        _position.X = _tmpBuff.GetFloat( 0 );
        _position.Y = _tmpBuff.GetFloat( 4 );

        return _position;
    }

    /// <summary>
    /// Get the angle in radians.
    /// </summary>
    /// <returns> the current world rotation angle in radians. </returns>
    public float GetAngle()
    {
        return jniGetAngle( Addr );
    }

    /// <summary>
    /// Get the world position of the center of mass.
    /// Note that the same Vector2 instance is returned each time this method is called. 
    /// </summary>
    public Vector2 GetWorldCenter()
    {
        jniGetWorldCenter( Addr, _tmpBuffAddress );
        
        _worldCenter.X = _tmpBuff.GetFloat( 0 );
        _worldCenter.Y = _tmpBuff.GetFloat( 4 );

        return _worldCenter;
    }

    /// <summary>
    /// Get the local position of the center of mass.
    /// Note that the same Vector2 instance is returned each time this method is called. 
    /// </summary>
    public Vector2 GetLocalCenter()
    {
        jniGetLocalCenter( Addr, _tmpBuffAddress );

        _localCenter.X = _tmpBuff.GetFloat( 0 );
        _localCenter.Y = _tmpBuff.GetFloat( 4 );

        return _localCenter;
    }

    /// <summary>
    /// Set the linear velocity of the center of mass. 
    /// </summary>
    public void SetLinearVelocity( Vector2 v )
    {
        jniSetLinearVelocity( Addr, v.X, v.Y );
    }

    /// <summary>
    /// Set the linear velocity of the center of mass. 
    /// </summary>
    public void SetLinearVelocity( float vX, float vY )
    {
        jniSetLinearVelocity( Addr, vX, vY );
    }

    /// <summary>
    /// Get the linear velocity of the center of mass.
    /// Note that the same Vector2 instance is returned each time this method is called. 
    /// </summary>
    public Vector2 GetLinearVelocity()
    {
        jniGetLinearVelocity( Addr, _tmpBuffAddress );
        _linearVelocity.X = _tmpBuff.GetFloat( 0 );
        _linearVelocity.Y = _tmpBuff.GetFloat( 4 );

        return _linearVelocity;
    }

    /// <summary>
    /// Set the angular velocity. 
    /// </summary>
    public void SetAngularVelocity( float omega )
    {
        jniSetAngularVelocity( Addr, omega );
    }

    /// <summary>
    /// Get the angular velocity. 
    /// </summary>
    public float GetAngularVelocity()
    {
        return jniGetAngularVelocity( Addr );
    }

    /// <summary>
    /// Apply a force at a world point. If the force is not applied at the center of mass,
    /// it will generate a torque and affect the angular velocity. This wakes up the body.
    /// </summary>
    /// <param name="force"> the world force vector, usually in Newtons (N). </param>
    /// <param name="point"> the world position of the point of application. </param>
    /// <param name="wake"> up the body </param> 
    public void ApplyForce( Vector2 force, Vector2 point, bool wake )
    {
        jniApplyForce( Addr, force.X, force.Y, point.X, point.Y, wake );
    }

    /// <summary>
    /// Apply a force at a world point. If the force is not applied at the center of mass,
    /// it will generate a torque and affect the angular velocity. This wakes up the body.
    /// </summary>
    /// <param name="forceX"> the world force vector on x, usually in Newtons (N). </param>
    /// <param name="forceY"> the world force vector on y, usually in Newtons (N). </param>
    /// <param name="pointX"> the world position of the point of application on x. </param>
    /// <param name="pointY"> the world position of the point of application on y. </param>
    /// <param name="wake"> up the body </param>
    public void ApplyForce( float forceX, float forceY, float pointX, float pointY, bool wake )
    {
        jniApplyForce( Addr, forceX, forceY, pointX, pointY, wake );
    }

    /// <summary>
    /// Apply a force to the center of mass. This wakes up the body.
    /// </summary>
    /// <param name="force"> the world force vector, usually in Newtons (N). </param> 
    public void ApplyForceToCenter( Vector2 force, bool wake )
    {
        jniApplyForceToCenter( Addr, force.X, force.Y, wake );
    }

    /// <summary>
    /// Apply a force to the center of mass. This wakes up the body.
    /// </summary>
    /// <param name="forceX"> the world force vector, usually in Newtons (N). </param>
    /// <param name="forceY"> the world force vector, usually in Newtons (N). </param>
    public void ApplyForceToCenter( float forceX, float forceY, bool wake )
    {
        jniApplyForceToCenter( Addr, forceX, forceY, wake );
    }

    /// <summary>
    /// Apply an angular impulse.
    /// </summary>
    /// <param name="impulse"> the angular impulse in units of kg*m*m/s </param> 
    public void ApplyAngularImpulse( float impulse, bool wake )
    {
        jniApplyAngularImpulse( Addr, impulse, wake );
    }

    /// <summary>
    /// Apply a torque. This affects the angular velocity without affecting the linear
    /// velocity of the center of mass. This wakes up the body.
    /// </summary>
    /// <param name="torque"> about the z-axis (out of the screen), usually in N-m. </param>
    /// <param name="wake"> up the body </param>
    public void ApplyTorque( float torque, bool wake )
    {
        jniApplyTorque( Addr, torque, wake );
    }

    /// <summary>
    /// Apply an impulse at a point. This immediately modifies the velocity. It also
    /// modifies the angular velocity if the point of application is not at the center
    /// of mass. This wakes up the body.
    /// </summary>
    /// <param name="impulse"> the world impulse vector, usually in N-seconds or kg-m/s. </param>
    /// <param name="point"> the world position of the point of application. </param>
    /// <param name="wake"> up the body </param>
    public void ApplyLinearImpulse( Vector2 impulse, Vector2 point, bool wake )
    {
        jniApplyLinearImpulse( Addr, impulse.X, impulse.Y, point.X, point.Y, wake );
    }

    /// <summary>
    /// Apply an impulse at a point. This immediately modifies the velocity. It also
    /// modifies the angular velocity if the point of application is not at the center
    /// of mass. This wakes up the body.
    /// </summary>
    /// <param name="impulseX"> the world impulse vector on the x-axis, usually in N-seconds or kg-m/s. </param>
    /// <param name="impulseY"> the world impulse vector on the y-axis, usually in N-seconds or kg-m/s. </param>
    /// <param name="pointX"> the world position of the point of application on the x-axis. </param>
    /// <param name="pointY"> the world position of the point of application on the y-axis. </param>
    /// <param name="wake"> up the body </param>
    public void ApplyLinearImpulse( float impulseX, float impulseY, float pointX, float pointY, bool wake )
    {
        jniApplyLinearImpulse( Addr, impulseX, impulseY, pointX, pointY, wake );
    }

    /// <summary>
    /// Get the total mass of the body.
    /// </summary>
    /// <returns> the mass, usually in kilograms (kg). </returns>
    public float GetMass()
    {
        return jniGetMass( Addr );
    }

    /// <summary>
    /// Get the rotational inertia of the body about the local origin.
    /// </summary>
    /// <returns> the rotational inertia, usually in kg-m^2. </returns>
    public float GetInertia()
    {
        return jniGetInertia( Addr );
    }

    /// <summary>
    /// Get the mass data of the body.
    /// </summary>
    /// <returns> a struct containing the mass, inertia and center of the body. </returns>
    public MassData GetMassData()
    {
        jniGetMassData( Addr, _tmp );

        _massData.Mass     = _tmp[ 0 ];
        _massData.Center.X = _tmp[ 1 ];
        _massData.Center.Y = _tmp[ 2 ];
        _massData.I        = _tmp[ 3 ];

        return _massData;
    }

    /// <summary>
    /// Set the mass properties to override the mass properties of the fixtures. Note
    /// that this changes the center of mass position. Note that creating or destroying
    /// fixtures can also alter the mass. This function has no effect if the body isn't
    /// dynamic.
    /// </summary>
    /// <param name="data"> the mass properties. </param> 
    public void SetMassData( MassData data )
    {
        jniSetMassData( Addr, data.Mass, data.Center.X, data.Center.Y, data.I );
    }

    /// <summary>
    /// This resets the mass properties to the sum of the mass properties of the fixtures.
    /// This normally does not need to be called unless you called SetMassData to override
    /// the mass and you later want to reset the mass. 
    /// </summary>
    public void ResetMassData()
    {
        jniResetMassData( Addr );
    }

    /// <summary>
    /// Get the world coordinates of a point given the local coordinates.
    /// Note that the same Vector2 instance is returned each time this method is called.
    /// </summary>
    /// <param name="localPoint"> a point on the body measured relative the the body's origin. </param>
    /// <returns> the same point expressed in world coordinates. </returns>
    public Vector2 GetWorldPoint( Vector2 localPoint )
    {
        jniGetWorldPoint( Addr, localPoint.X, localPoint.Y, _tmpBuffAddress );
        this._localPoint.X = _tmpBuff.GetFloat( 0 );
        this._localPoint.Y = _tmpBuff.GetFloat( 4 );

        return this._localPoint;
    }

    /// <summary>
    /// Get the world coordinates of a vector given the local coordinates.
    /// Note that the same Vector2 instance is returned each time this method is called.
    /// </summary>
    /// <param name="localVector"> a vector fixed in the body. </param>
    /// <returns> the same vector expressed in world coordinates. </returns>
    public Vector2 GetWorldVector( Vector2 localVector )
    {
        jniGetWorldVector( Addr, localVector.X, localVector.Y, _tmpBuffAddress );
        _worldVector.X = _tmpBuff.GetFloat( 0 );
        _worldVector.Y = _tmpBuff.GetFloat( 4 );

        return _worldVector;
    }

    /// <summary>
    /// Gets a local point relative to the body's origin given a world point.
    /// Note that the same Vector2 instance is returned each time this method is called.
    /// </summary>
    /// <param name="worldPoint"> a point in world coordinates. </param>
    /// <returns> the corresponding local point relative to the body's origin. </returns>
    public Vector2 GetLocalPoint( Vector2 worldPoint )
    {
        jniGetLocalPoint( Addr, worldPoint.X, worldPoint.Y, _tmpBuffAddress );
        LocalPoint2.X = _tmpBuff.GetFloat( 0 );
        LocalPoint2.Y = _tmpBuff.GetFloat( 4 );

        return LocalPoint2;
    }

    /// <summary>
    /// Gets a local vector given a world vector.
    /// Note that the same Vector2 instance is returned each time this method is called.
    /// </summary>
    /// <param name="worldVector"> a vector in world coordinates. </param>
    /// <returns> the corresponding local vector. </returns>
    public Vector2 GetLocalVector( Vector2 worldVector )
    {
        jniGetLocalVector( Addr, worldVector.X, worldVector.Y, _tmpBuffAddress );
        LocalVector.X = _tmpBuff.GetFloat( 0 );
        LocalVector.Y = _tmpBuff.GetFloat( 4 );

        return LocalVector;
    }

    /// <summary>
    /// Get the world linear velocity of a world point attached to this body.
    /// Note that the same Vector2 instance is returned each time this method is called.
    /// </summary>
    /// <param name="worldPoint"> a point in world coordinates. </param>
    /// <returns> the world velocity of a point. </returns>
    public Vector2 GetLinearVelocityFromWorldPoint( Vector2 worldPoint )
    {
        jniGetLinearVelocityFromWorldPoint( Addr, worldPoint.X, worldPoint.Y, _tmpBuffAddress );
        LinVelWorld.X = _tmpBuff.GetFloat( 0 );
        LinVelWorld.Y = _tmpBuff.GetFloat( 4 );

        return LinVelWorld;
    }

    /// <summary>
    /// Get the world velocity of a local point.
    /// Note that the same Vector2 instance is returned each time this method is called.
    /// </summary>
    /// <param name="localPoint"> a point in local coordinates. </param>
    /// <returns> the world velocity of a point. </returns>
    public Vector2 GetLinearVelocityFromLocalPoint( Vector2 localPoint )
    {
        jniGetLinearVelocityFromLocalPoint( Addr, localPoint.X, localPoint.Y, _tmpBuffAddress );
        LinVelLoc.X = _tmpBuff.GetFloat( 0 );
        LinVelLoc.Y = _tmpBuff.GetFloat( 4 );

        return LinVelLoc;
    }

    /// <summary>
    /// Get the linear damping of the body. 
    /// </summary>
    public float GetLinearDamping()
    {
        return jniGetLinearDamping( Addr );
    }

    /// <summary>
    /// Set the linear damping of the body. 
    /// </summary>
    public void SetLinearDamping( float linearDamping )
    {
        jniSetLinearDamping( Addr, linearDamping );
    }

    /// <summary>
    /// Get the angular damping of the body. 
    /// </summary>
    public float GetAngularDamping()
    {
        return jniGetAngularDamping( Addr );
    }

    /// <summary>
    /// Set the angular damping of the body. 
    /// </summary>
    public void SetAngularDamping( float angularDamping )
    {
        jniSetAngularDamping( Addr, angularDamping );
    }

    /// <summary>
    /// Set the type of this body. This may alter the mass and velocity. 
    /// </summary>
    public void SetType( BodyDef.BodyType type )
    {
        jniSetType( Addr, ( int )type );
    }

    /// <summary>
    /// Get the type of this body. 
    /// </summary>
    public BodyDef.BodyType GetBodyType()
    {
        int type = jniGetType( Addr );

        if ( type == 0 ) return BodyDef.BodyType.Static;
        if ( type == 1 ) return BodyDef.BodyType.Kinematic;
        if ( type == 2 ) return BodyDef.BodyType.Dynamic;

        return BodyDef.BodyType.Static;
    }

    /// <summary>
    /// Should this body be treated like a bullet for continuous collision detection? 
    /// </summary>
    public void SetBullet( bool flag )
    {
        jniSetBullet( Addr, flag );
    }

    /// <summary>
    /// Is this body treated like a bullet for continuous collision detection? 
    /// </summary>
    public bool IsBullet()
    {
        return jniIsBullet( Addr );
    }

    /// <summary>
    /// You can disable sleeping on this body. If you disable sleeping, the 
    /// </summary>
    public void SetSleepingAllowed( bool flag )
    {
        jniSetSleepingAllowed( Addr, flag );
    }

    /// <summary>
    /// Is this body allowed to sleep 
    /// </summary>
    public bool IsSleepingAllowed()
    {
        return jniIsSleepingAllowed( Addr );
    }

    /// <summary>
    /// Set the sleep state of the body. A sleeping body has very low CPU cost.
    /// </summary>
    /// <param name="flag"> set to true to wake the body, false to put it to sleep. </param> 
    public void SetAwake( bool flag )
    {
        jniSetAwake( Addr, flag );
    }

    /// <summary>
    /// Get the sleeping state of this body.
    /// </summary>
    /// <returns> true if the body is not sleeping. </returns>
    public bool IsAwake()
    {
        return jniIsAwake( Addr );
    }

    /// <summary>
    /// Set the active state of the body.
    /// <li>An inactive body is not simulated and cannot be collided with or woken up.</li>
    /// <li>If you pass a flag of true, all fixtures will be added to the broad-phase.</li>
    /// <li>If you pass a flag of false, all fixtures will be removed from the broad-phase
    /// and all contacts will be destroyed.</li>
    /// <li>Fixtures and joints are otherwise unaffected.</li>
    /// <li>You may continue to create/destroy fixtures and joints on inactive bodies.</li>
    /// <li>Fixtures on an inactive body are implicitly inactive and will not participate
    /// in collisions, ray-casts, or queries.</li>
    /// <li>Joints connected to an inactive body are implicitly inactive.</li>
    /// <li>An inactive body is still owned by a b2World object and remains in the body list.</li> 
    /// </summary>
    public void SetActive( bool flag )
    {
        if ( flag )
        {
            jniSetActive( Addr, flag );
        }
        else
        {
            this.World.DeactivateBody( this );
        }
    }

    /// <summary>
    /// Get the active state of the body. 
    /// </summary>
    public bool IsActive()
    {
        return jniIsActive( Addr );
    }

    /// <summary>
    /// Gets / Sets the fixed rotation for this body.
    /// Note: Setting fixed rotation causes the mass to be reset.
    /// </summary>
    public bool FixedRotation
    {
        get => jniIsFixedRotation( Addr );
        set => jniSetFixedRotation( Addr, value );
    }

    /// <summary>
    /// Get the list of all fixtures attached to this body. Do not modify the list! 
    /// </summary>
    public List< Fixture > GetFixtureList()
    {
        return _fixtures;
    }

    /// <summary>
    /// Get the list of all joints attached to this body. Do not modify the list! 
    /// </summary>
    public List< JointEdge > GetJointList()
    {
        return Joints;
    }

    /// <summary>
    /// The gravity scale of the body.
    /// </summary>
    public float GravityScale
    {
        get => jniGetGravityScale( Addr );
        set => jniSetGravityScale( Addr, value );
    }

    // ========================================================================
    // ========================================================================

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long jniCreateFixture( long addr, long shapeAddr,
                                                 float friction, float restitution, float density,
                                                 bool isSensor, short filterCategoryBits, short filterMaskBits,
                                                 short filterGroupIndex );
    /*
        b2Body* body = (b2Body*)addr;
        b2Shape* shape = (b2Shape*)shapeAddr;
        b2FixtureDef fixtureDef;

        fixtureDef.shape = shape;
        fixtureDef.friction = friction;
        fixtureDef.restitution = restitution;
        fixtureDef.density = density;
        fixtureDef.isSensor = isSensor;
        fixtureDef.filter.maskBits = filterMaskBits;
        fixtureDef.filter.categoryBits = filterCategoryBits;
        fixtureDef.filter.groupIndex = filterGroupIndex;

        return (jlong)body->CreateFixture( &fixtureDef );
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long jniCreateFixture( long addr, long shapeAddr, float density );
    /*
        b2Body* body = (b2Body*)addr;
        b2Shape* shape = (b2Shape*)shapeAddr;
        return (jlong)body->CreateFixture( shape, density );
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetTransform( long addr, float positionX, float positionY, float angle );
    /*
        b2Body* body = (b2Body*)addr;
        body->SetTransform(b2Vec2(positionX, positionY), angle);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetTransform( long addr, float[] vals );
    /*
        b2Body* body = (b2Body*)addr;
        b2Transform t = body->GetTransform();
        vals[0] = t.p.X;
        vals[1] = t.p.Y;
        vals[2] = t.q.c;
        vals[3] = t.q.s;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetPosition( long addr, long positionAddr );
    /*
        b2Body* body = (b2Body*)addr;
        float* position = (float*) positionAddr;
        b2Vec2 p = body->GetPosition();
        position[0] = p.X;
        position[1] = p.Y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetAngle( long addr );
    /*
        b2Body* body = (b2Body*)addr;
        return body->GetAngle();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetWorldCenter( long addr, long worldCenterAddr );
    /*
        b2Body* body = (b2Body*)addr;
        float* worldCenter = (float*) worldCenterAddr;
        b2Vec2 w = body->GetWorldCenter();
        worldCenter[0] = w.X;
        worldCenter[1] = w.Y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLocalCenter( long addr, long localCenterAddr );
    /*
        b2Body* body = (b2Body*)addr;
        float* localCenter = (float*) localCenterAddr;
        b2Vec2 w = body->GetLocalCenter();
        localCenter[0] = w.X;
        localCenter[1] = w.Y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetLinearVelocity( long addr, float x, float y );
    /*
        b2Body* body = (b2Body*)addr;
        body->SetLinearVelocity(b2Vec2(x, y));
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLinearVelocity( long addr, long linearVelocityAddr );
    /*
        b2Body* body = (b2Body*)addr;
        float* linearVelocity = (float*) linearVelocityAddr;
        b2Vec2 l = body->GetLinearVelocity();
        linearVelocity[0] = l.X;
        linearVelocity[1] = l.Y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetAngularVelocity( long addr, float omega );
    /*
        b2Body* body = (b2Body*)addr;
        body->SetAngularVelocity(omega);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetAngularVelocity( long addr );
    /*
        b2Body* body = (b2Body*)addr;
        return body->GetAngularVelocity();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniApplyForce( long addr, float forceX, float forceY, float pointX, float pointY,
                                              bool wake );
    /*
        b2Body* body = (b2Body*)addr;
        body->ApplyForce(b2Vec2(forceX, forceY), b2Vec2(pointX, pointY), wake);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniApplyTorque( long addr, float torque, bool wake ); /*
        b2Body* body = (b2Body*)addr;
        body->ApplyTorque(torque, wake);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniApplyLinearImpulse( long addr, float impulseX, float impulseY, float pointX,
                                                      float pointY,
                                                      bool wake );
    /*
        b2Body* body = (b2Body*)addr;
        body->ApplyLinearImpulse( b2Vec2( impulseX, impulseY ), b2Vec2( pointX, pointY ), wake);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniApplyAngularImpulse( long addr, float impulse, bool wake );
    /*
        b2Body* body = (b2Body*)addr;
        body->ApplyAngularImpulse(impulse, wake);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetMass( long addr );
    /*
        b2Body* body = (b2Body*)addr;
        return body->GetMass();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetInertia( long addr );
    /*
        b2Body* body = (b2Body*)addr;
        return body->GetInertia();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniApplyForceToCenter( long addr, float forceX, float forceY, bool wake );
    /*
        b2Body* body = (b2Body*)addr;
        body->ApplyForceToCenter(b2Vec2(forceX, forceY), wake);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetMassData( long addr, float[] massData );
    /*
        b2Body* body = (b2Body*)addr;
        b2MassData m;
        body->GetMassData(&m);
        massData[0] = m.mass;
        massData[1] = m.center.X;
        massData[2] = m.center.Y;
        massData[3] = m.I;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetMassData( long addr, float mass, float centerX, float centerY, float I );
    /*
        b2Body* body = (b2Body*)addr;
        b2MassData m;
        m.mass = mass;
        m.center.X = centerX;
        m.center.Y = centerY;
        m.I = I;
        body->SetMassData(&m);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniResetMassData( long addr );
    /*
        b2Body* body = (b2Body*)addr;
        body->ResetMassData();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetWorldPoint( long addr,
                                                 float localPointX,
                                                 float localPointY,
                                                 long worldPointAddr );
    /*
        b2Body* body = (b2Body*)addr;
        float* worldPoint = (float*) worldPointAddr;
        b2Vec2 w = body->GetWorldPoint( b2Vec2( localPointX, localPointY ) );
        worldPoint[0] = w.X;
        worldPoint[1] = w.Y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetWorldVector( long addr, float localVectorX, float localVectorY,
                                                  long worldVectorAddr );
    /*
        b2Body* body = (b2Body*)addr;
        float* worldVector = (float*) worldVectorAddr;
        b2Vec2 w = body->GetWorldVector( b2Vec2( localVectorX, localVectorY ) );
        worldVector[0] = w.X;
        worldVector[1] = w.Y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLocalPoint( long addr,
                                                 float worldPointX,
                                                 float worldPointY,
                                                 long localPointAddr );
    /*
        b2Body* body = (b2Body*)addr;
        float* localPoint = (float*) localPointAddr;
        b2Vec2 w = body->GetLocalPoint( b2Vec2( worldPointX, worldPointY ) );
        localPoint[0] = w.X;
        localPoint[1] = w.Y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLocalVector( long addr, float worldVectorX, float worldVectorY,
                                                  long worldVectorAddr );
    /*
        b2Body* body = (b2Body*)addr;
        float* worldVector = (float*) worldVectorAddr;
        b2Vec2 w = body->GetLocalVector( b2Vec2( worldVectorX, worldVectorY ) );
        worldVector[0] = w.X;
        worldVector[1] = w.Y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLinearVelocityFromWorldPoint( long addr, float worldPointX,
                                                                   float worldPointY,
                                                                   long linVelWorldAddr );
    /*
        b2Body* body = (b2Body*)addr;
        float* linVelWorld = (float*) linVelWorldAddr;
        b2Vec2 w = body->GetLinearVelocityFromWorldPoint( b2Vec2( worldPointX, worldPointY ) );
        linVelWorld[0] = w.X;
        linVelWorld[1] = w.Y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLinearVelocityFromLocalPoint( long addr,
                                                                   float localPointX,
                                                                   float localPointY,
                                                                   long linVelLocAddr );
    /*
        b2Body* body = (b2Body*)addr;
        float* linVelLoc = (float*) linVelLocAddr;
        b2Vec2 w = body->GetLinearVelocityFromLocalPoint( b2Vec2( localPointX, localPointY ) );
        linVelLoc[0] = w.X;
        linVelLoc[1] = w.Y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetLinearDamping( long addr );
    /*
        b2Body* body = (b2Body*)addr;
        return body->GetLinearDamping();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetLinearDamping( long addr, float linearDamping );
    /*
        b2Body* body = (b2Body*)addr;
        body->SetLinearDamping(linearDamping);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetAngularDamping( long addr );
    /*
        b2Body* body = (b2Body*)addr;
        return body->GetAngularDamping();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetAngularDamping( long addr, float angularDamping );
    /*
        b2Body* body = (b2Body*)addr;
        body->SetAngularDamping(angularDamping);
    */

//    inline b2BodyType getBodyType( int type )
//    {
//        switch( type )
//        {
//        case 0: return b2_staticBody;
//        case 1: return b2_kinematicBody;
//        case 2: return b2_dynamicBody;
//        default:
//            return b2_staticBody;
//        }
//    }

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetType( long addr, int type );
    /*
        b2Body* body = (b2Body*)addr;
        body->SetType(getBodyType(type));
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern int jniGetType( long addr );
    /*
        b2Body* body = (b2Body*)addr;
        return body->GetType();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetBullet( long addr, bool flag );
    /*
        b2Body* body = (b2Body*)addr;
        body->SetBullet(flag);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern bool jniIsBullet( long addr );
    /*
        b2Body* body = (b2Body*)addr;
        return body->IsBullet();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetSleepingAllowed( long addr, bool flag );
    /*
        b2Body* body = (b2Body*)addr;
        body->SetSleepingAllowed(flag);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern bool jniIsSleepingAllowed( long addr );
    /*
        b2Body* body = (b2Body*)addr;
        return body->IsSleepingAllowed();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetAwake( long addr, bool flag );
    /*
        b2Body* body = (b2Body*)addr;
        body->SetAwake(flag);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern bool jniIsAwake( long addr );
    /*
        b2Body* body = (b2Body*)addr;
        return body->IsAwake();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetActive( long addr, bool flag );
    /*
        b2Body* body = (b2Body*)addr;
        body->SetActive(flag);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern bool jniIsActive( long addr );
    /*
        b2Body* body = (b2Body*)addr;
        return body->IsActive();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetFixedRotation( long addr, bool flag );
    /*
        b2Body* body = (b2Body*)addr;
        body->SetFixedRotation(flag);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern bool jniIsFixedRotation( long addr );
    /*
        b2Body* body = (b2Body*)addr;
        return body->IsFixedRotation();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetGravityScale( long addr );
    /*
        b2Body* body = (b2Body*)addr;
        return body->GetGravityScale();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetGravityScale( long addr, float scale );
    /*
        b2Body* body = (b2Body*)addr;
        body->SetGravityScale(scale);
    */
}

// ============================================================================
// ============================================================================
