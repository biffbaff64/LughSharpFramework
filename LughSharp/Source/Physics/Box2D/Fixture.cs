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
public class Fixture( Body body, long addr )
{
    public long    Addr     { get; set; } = addr;
    public object? UserData { get; set; }

    // ========================================================================
    
    private Body _body = body;

    /// the shape, initialized lazy 
    protected Shape? Shape;

    /// the fixture filter data 
    private readonly Filter _filter = new();

    /// flag to indicate if filter data needs to be updated with a Native call 
    private bool _dirtyFilter = true;

    /// Get the contact filtering data. 
    private readonly short[] _tmp = new short[ 3 ];

    // ========================================================================

    protected void Reset( Body body, long addr )
    {
        this._body        = body;
        this.Addr         = addr;
        this.Shape        = null;
        this.UserData     = null;
        this._dirtyFilter = true;
    }

    /// <summary>
    /// Get the type of the child shape. You can use this to down cast to the concrete shape.
    /// </summary>
    /// <returns> the shape type. </returns> 
    public Shape.ShapeTypes GetFixtureType()
    {
        int type = jniGetType( Addr );

        switch ( type )
        {
            case 0:
                return Shape.ShapeTypes.Circle;

            case 1:
                return Shape.ShapeTypes.Edge;

            case 2:
                return Shape.ShapeTypes.Polygon;

            case 3:
                return Shape.ShapeTypes.Chain;

            default:
                throw new LughRuntimeException( "Unknown shape type!" );
        }
    }

    /// <summary>
    /// Returns the shape of this fixture 
    /// </summary>
    public Shape GetShape()
    {
        if ( Shape == null )
        {
            long shapeAddr = jniGetShape( Addr );

            if ( shapeAddr == 0 ) throw new LughRuntimeException( "Null shape address!" );

            int type = Shape.jniGetType( shapeAddr );

            switch ( type )
            {
                case 0:
                    Shape = new CircleShape( shapeAddr );

                    break;

                case 1:
                    Shape = new EdgeShape( shapeAddr );

                    break;

                case 2:
                    Shape = new PolygonShape( shapeAddr );

                    break;

                case 3:
                    Shape = new ChainShape( shapeAddr );

                    break;

                default:
                    throw new LughRuntimeException( "Unknown shape type!" );
            }
        }

        return Shape;
    }

    /// <summary>
    /// Set if this fixture is a sensor. 
    /// </summary>
    public void SetSensor( bool sensor )
    {
        jniSetSensor( Addr, sensor );
    }

    /// <summary>
    /// Is this fixture a sensor (non-solid)?
    /// </summary>
    /// @return the true if the shape is a sensor. 
    public bool IsSensor()
    {
        return jniIsSensor( Addr );
    }

    /// <summary>
    /// Set the contact filtering data. This will not update contacts until the next time
    /// step when either parent body is active and awake. This automatically calls Refilter. 
    /// </summary>
    public void SetFilterData( Filter filter )
    {
        jniSetFilterData( Addr, filter.CategoryBits, filter.MaskBits, filter.GroupIndex );
        this._filter.Set( filter );
        _dirtyFilter = false;
    }

    /// <summary>
    /// Get the contact filtering data. Modifying the returned Filter without calling
    /// <see cref="SetFilterData"/> can result in unpredictable behaviour. 
    /// </summary>
    public Filter GetFilterData()
    {
        if ( _dirtyFilter )
        {
            jniGetFilterData( Addr, _tmp );

            _filter.MaskBits     = _tmp[ 0 ];
            _filter.CategoryBits = _tmp[ 1 ];
            _filter.GroupIndex   = _tmp[ 2 ];
            _dirtyFilter         = false;
        }

        return _filter;
    }

    /// <summary>
    /// Call this if you want to establish collision that was previously disabled
    /// by b2ContactFilter::ShouldCollide. 
    /// </summary>
    public void Refilter()
    {
        jniRefilter( Addr );
    }

    /// <summary>
    /// Get the parent body of this fixture. This is NULL if the fixture is not attached. 
    /// </summary>
    public Body GetBody()
    {
        return _body;
    }

    /// <summary>
    /// Test a point for containment in this fixture.
    /// </summary>
    /// <param name="p"> a point in world coordinates. </param> 
    public bool TestPoint( Vector2 p )
    {
        return jniTestPoint( Addr, p.X, p.Y );
    }

    /// <summary>
    /// Test a point for containment in this fixture.
    /// </summary>
    /// <param name="x"> the x-coordinate </param>
    /// <param name="y"> the y-coordinate </param>
    public bool TestPoint( float x, float y )
    {
        return jniTestPoint( Addr, x, y );
    }

    /// <summary>
    /// Set the density of this fixture. This will <b>not</b> automatically adjust the
    /// mass of the body. You must call b2Body::ResetMassData to update the body's mass. 
    /// </summary>
    public void SetDensity( float density )
    {
        jniSetDensity( Addr, density );
    }

    /// <summary>
    /// Get the density of this fixture. 
    /// </summary>
    public float GetDensity()
    {
        return jniGetDensity( Addr );
    }

    /// <summary>
    /// Get the coefficient of friction. 
    /// </summary>
    public float GetFriction()
    {
        return jniGetFriction( Addr );
    }

    /// <summary>
    /// Set the coefficient of friction. 
    /// </summary>
    public void SetFriction( float friction )
    {
        jniSetFriction( Addr, friction );
    }

    /// <summary>
    /// Get the coefficient of restitution. 
    /// </summary>
    public float GetRestitution()
    {
        return jniGetRestitution( Addr );
    }

    /// <summary>
    /// Set the coefficient of restitution. 
    /// </summary>
    public void SetRestitution( float restitution )
    {
        jniSetRestitution( Addr, restitution );
    }

    // ========================================================================
    // ========================================================================

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern int jniGetType( long addr );
    /*
        b2Fixture* fixture = (b2Fixture*)addr;
        b2Shape::Type type = fixture->GetType();
        switch( type )
        {
        case b2Shape::e_circle: return 0;
        case b2Shape::e_edge: return 1;
        case b2Shape::e_polygon: return 2;
        case b2Shape::e_chain: return 3;
        default:
            return -1;
        }
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long jniGetShape( long addr );
    /*
        b2Fixture* fixture = (b2Fixture*)addr;
        return (jlong)fixture->GetShape();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetSensor( long addr, bool sensor );
    /*
        b2Fixture* fixture = (b2Fixture*)addr;
        fixture->SetSensor(sensor);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern bool jniIsSensor( long addr );
    /*
        b2Fixture* fixture = (b2Fixture*)addr;
        return fixture->IsSensor();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetFilterData( long addr, short categoryBits, short maskBits, short groupIndex );
    /*
        b2Fixture* fixture = (b2Fixture*)addr;
        b2Filter filter;
        filter.categoryBits = categoryBits;
        filter.maskBits = maskBits;
        filter.groupIndex = groupIndex;
        fixture->SetFilterData(filter);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetFilterData( long addr, short[] filter );
    /*
        b2Fixture* fixture = (b2Fixture*)addr;
        unsigned short* filterOut = (unsigned short*)filter;
        b2Filter f = fixture->GetFilterData();
        filterOut[0] = f.maskBits;
        filterOut[1] = f.categoryBits;
        filterOut[2] = f.groupIndex;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniRefilter( long addr );
    /*
        b2Fixture* fixture = (b2Fixture*)addr;
        fixture->Refilter();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern bool jniTestPoint( long addr, float x, float y );
    /*
        b2Fixture* fixture = (b2Fixture*)addr;
        return fixture->TestPoint( b2Vec2( x, y ) );
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetDensity( long addr, float density );
    /*
        b2Fixture* fixture = (b2Fixture*)addr;
        fixture->SetDensity(density);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetDensity( long addr );
    /*
        b2Fixture* fixture = (b2Fixture*)addr;
        return fixture->GetDensity();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetFriction( long addr );
    /*
        b2Fixture* fixture = (b2Fixture*)addr;
        return fixture->GetFriction();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetFriction( long addr, float friction );
    /*
        b2Fixture* fixture = (b2Fixture*)addr;
        fixture->SetFriction(friction);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetRestitution( long addr );
    /*
        b2Fixture* fixture = (b2Fixture*)addr;
        return fixture->GetRestitution();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetRestitution( long addr, float restitution );
    /*
        b2Fixture* fixture = (b2Fixture*)addr;
        fixture->SetRestitution(restitution);
    */
}

// ============================================================================
// ============================================================================
