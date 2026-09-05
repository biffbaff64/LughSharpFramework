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

using LughSharp.Source.Graphics.Utils;

namespace LughSharp.Source.Physics.Box2D;

[PublicAPI]
public class Box2DDebugRenderer
{
    public static readonly Color ShapeNotActive = new( 0.5f, 0.5f, 0.3f, 1 );
    public static readonly Color ShapeStatic    = new( 0.5f, 0.9f, 0.5f, 1 );
    public static readonly Color ShapeKinematic = new( 0.5f, 0.5f, 0.9f, 1 );
    public static readonly Color ShapeNotAwake  = new( 0.6f, 0.6f, 0.6f, 1 );
    public static readonly Color ShapeAwake     = new( 0.9f, 0.7f, 0.7f, 1 );
    public static readonly Color JointColor     = new( 0.5f, 0.8f, 0.8f, 1 );
    public static readonly Color AabbColor      = new( 1.0f, 0, 1.0f, 1f );
    public static readonly Color VelocityColor  = new( 1.0f, 0, 0f, 1f );

    /** the immediate mode renderer to output our debug drawings **/
    protected ShapeRenderer Renderer;

    /** vertices for polygon rendering **/
    private static readonly Vector2[] _vertices = new Vector2[ 1000 ];

    private static readonly Vector2 _lower = new();
    private static readonly Vector2 _upper = new();

    private static readonly List< Body >  _bodies = new();
    private static readonly List< Joint > _joints = new();

    private bool _drawBodies;
    private bool _drawJoints;
    private bool _drawAabBs;
    private bool _drawInactiveBodies;
    private bool _drawVelocities;
    private bool _drawContacts;

    // ========================================================================

    public Box2DDebugRenderer()
        : this( true, true, false, true, false, true )
    {
    }

    public Box2DDebugRenderer( bool drawBodies, bool drawJoints, bool drawAabBs, bool drawInactiveBodies,
                               bool drawVelocities, bool drawContacts )
    {
        // next we setup the immediate mode renderer
        Renderer = new ShapeRenderer();

        // initialize vertices array
        for ( var i = 0; i < _vertices.Length; i++ )
        {
            _vertices[ i ] = new Vector2();
        }

        this._drawBodies         = drawBodies;
        this._drawJoints         = drawJoints;
        this._drawAabBs          = drawAabBs;
        this._drawInactiveBodies = drawInactiveBodies;
        this._drawVelocities     = drawVelocities;
        this._drawContacts       = drawContacts;
    }

    /** This assumes that the projection matrix has already been set. */
    public void Render( World world, Matrix4 projMatrix )
    {
        Renderer.ProjectionMatrix = projMatrix;
        RenderBodies( world );
    }

    private void RenderBodies( World world )
    {
        Renderer.Begin( ShapeRenderer.ShapeRenderType.Lines );

        if ( _drawBodies || _drawAabBs )
        {
            world.GetBodies( _bodies );

            foreach ( Body body in _bodies )
            {
                if ( body.IsActive || _drawInactiveBodies )
                {
                    RenderBody( body );
                }
            }
        }

        if ( _drawJoints )
        {
            world.GetJoints( _joints );

            foreach ( Joint joint in  _joints )
            {
                DrawJoint( joint );
            }
        }

        Renderer.End();

        if ( _drawContacts )
        {
            Renderer.Begin( ShapeRenderer.ShapeRenderType.Points );

            List< Contact > contactList = world.GetContactList();
            
            foreach ( Contact contact in contactList )
            {
                DrawContact( contact );
            }
            
            Renderer.End();
        }
    }

    protected void RenderBody( Body body )
    {
        Transform transform = body.GetTransform();
        
        List< Fixture > fixtureList = body.GetFixtureList();
        
        foreach ( Fixture fixture in fixtureList )
        {
            if ( _drawBodies )
            {
                DrawShape( fixture, transform, GetColorByBody( body ) );

                if ( _drawVelocities )
                {
                    Vector2 position = body.GetPosition();
                    DrawSegment( position, body.GetLinearVelocity().Add( position ), VelocityColor );
                }
            }

            if ( _drawAabBs )
            {
                DrawAabb( fixture, transform );
            }
        }
    }

    private Color GetColorByBody( Body body )
    {
        if ( !body.IsActive )
        {
            return ShapeNotActive;
        }

        if ( body.GetBodyType() == BodyDef.BodyType.Static )
        {
            return ShapeStatic;
        }

        if ( body.GetBodyType() == BodyDef.BodyType.Kinematic )
        {
            return ShapeKinematic;
        }

        return body.IsAwake ? ShapeNotAwake : ShapeAwake;
    }

    private void DrawAabb( Fixture fixture, Transform transform )
    {
        if ( fixture.GetFixtureType() == Shape.ShapeTypes.Circle )
        {
            var shape  = ( CircleShape )fixture.GetShape();
            float       radius = shape.GetRadius();
            
            _vertices[ 0 ].Set( shape.GetPosition() );
            transform.Mul( _vertices[ 0 ] );
            
            _lower.Set( _vertices[ 0 ].X - radius, _vertices[ 0 ].Y - radius );
            _upper.Set( _vertices[ 0 ].X + radius, _vertices[ 0 ].Y + radius );

            // define vertices in ccw fashion...
            _vertices[ 0 ].Set( _lower.X, _lower.Y );
            _vertices[ 1 ].Set( _upper.X, _lower.Y );
            _vertices[ 2 ].Set( _upper.X, _upper.Y );
            _vertices[ 3 ].Set( _lower.X, _upper.Y );

            DrawSolidPolygon( _vertices, 4, AabbColor, true );
        }
        else if ( fixture.GetFixtureType() == Shape.ShapeTypes.Polygon )
        {
            var shape       = ( PolygonShape )fixture.GetShape();
            int          vertexCount = shape.GetVertexCount();

            shape.GetVertex( 0, _vertices[ 0 ] );
            _lower.Set( transform.Mul( _vertices[ 0 ] ) );
            _upper.Set( _lower );

            for ( var i = 1; i < vertexCount; i++ )
            {
                shape.GetVertex( i, _vertices[ i ] );
                transform.Mul( _vertices[ i ] );
                
                _lower.X = Math.Min( _lower.X, _vertices[ i ].X );
                _lower.Y = Math.Min( _lower.Y, _vertices[ i ].Y );
                _upper.X = Math.Max( _upper.X, _vertices[ i ].X );
                _upper.Y = Math.Max( _upper.Y, _vertices[ i ].Y );
            }

            // define vertices in ccw fashion...
            _vertices[ 0 ].Set( _lower.X, _lower.Y );
            _vertices[ 1 ].Set( _upper.X, _lower.Y );
            _vertices[ 2 ].Set( _upper.X, _upper.Y );
            _vertices[ 3 ].Set( _lower.X, _upper.Y );

            DrawSolidPolygon( _vertices, 4, AabbColor, true );
        }
    }

    private static Vector2 _t    = new();
    private static Vector2 _axis = new();

    private void DrawShape( Fixture fixture, Transform transform, Color color )
    {
        if ( fixture.GetFixtureType() == Shape.ShapeTypes.Circle )
        {
            var circle = ( CircleShape )fixture.GetShape();
            
            _t.Set( circle.GetPosition() );
            transform.Mul( _t );
            
            DrawSolidCircle
                (
                 _t,
                 circle.GetRadius(),
                 _axis.Set( transform.Vals[ Transform.Cos ], transform.Vals[ Transform.Sin ] ),
                 color
                );

            return;
        }

        if ( fixture.GetFixtureType() == Shape.ShapeTypes.Edge )
        {
            var edge = ( EdgeShape )fixture.GetShape();
            
            edge.GetVertex1( _vertices[ 0 ] );
            edge.GetVertex2( _vertices[ 1 ] );
            transform.Mul( _vertices[ 0 ] );
            transform.Mul( _vertices[ 1 ] );
            
            DrawSolidPolygon( _vertices, 2, color, true );

            return;
        }

        if ( fixture.GetFixtureType() == Shape.ShapeTypes.Polygon )
        {
            var chain       = ( PolygonShape )fixture.GetShape();
            int          vertexCount = chain.GetVertexCount();

            for ( var i = 0; i < vertexCount; i++ )
            {
                chain.GetVertex( i, _vertices[ i ] );
                transform.Mul( _vertices[ i ] );
            }

            DrawSolidPolygon( _vertices, vertexCount, color, true );

            return;
        }

        if ( fixture.GetFixtureType() == Shape.ShapeTypes.Chain )
        {
            var chain       = ( ChainShape )fixture.GetShape();
            int        vertexCount = chain.GetVertexCount();

            for ( var i = 0; i < vertexCount; i++ )
            {
                chain.GetVertex( i, _vertices[ i ] );
                transform.Mul( _vertices[ i ] );
            }

            DrawSolidPolygon( _vertices, vertexCount, color, false );
        }
    }

    private readonly Vector2 _f = new();
    private readonly Vector2 _v = new();
    private readonly Vector2 _lv = new();

    private void DrawSolidCircle( Vector2 center, float radius, Vector2 axis, Color color )
    {
        float angle    = 0;
        float angleInc = 2 * ( float )Math.PI / 20;

        Renderer.SetColor( color.R, color.G, color.B, color.A );

        for ( var i = 0; i < 20; i++, angle += angleInc )
        {
            _v.Set( ( ( float )Math.Cos( angle ) * radius ) + center.X,
                   ( ( float )Math.Sin( angle ) * radius ) + center.Y );

            if ( i == 0 )
            {
                _lv.Set( _v );
                _f.Set( _v );

                continue;
            }

            Renderer.Line( _lv.X, _lv.Y, _v.X, _v.Y );
            _lv.Set( _v );
        }

        Renderer.Line( _f.X, _f.Y, _lv.X, _lv.Y );
        Renderer.Line( center.X, center.Y, 0, center.X + ( axis.X * radius ), center.Y + ( axis.Y * radius ), 0 );
    }

    private void DrawSolidPolygon( Vector2[] vertices, int vertexCount, Color color, bool closed )
    {
        Renderer.SetColor( color.R, color.G, color.B, color.A );
        _lv.Set( vertices[ 0 ] );
        _f.Set( vertices[ 0 ] );

        for ( var i = 1; i < vertexCount; i++ )
        {
            Vector2 v = vertices[ i ];
            Renderer.Line( _lv.X, _lv.Y, v.X, v.Y );
            _lv.Set( v );
        }

        if ( closed ) Renderer.Line( _f.X, _f.Y, _lv.X, _lv.Y );
    }

    private void DrawJoint( Joint joint )
    {
        Body      bodyA = joint.GetBodyA();
        Body      bodyB = joint.GetBodyB();
        Transform xf1   = bodyA.GetTransform();
        Transform xf2   = bodyB.GetTransform();

        Vector2 x1 = xf1.GetPosition();
        Vector2 x2 = xf2.GetPosition();
        Vector2 p1 = joint.GetAnchorA();
        Vector2 p2 = joint.GetAnchorB();

        if ( joint.GetJointType() == JointDef.JointType.DistanceJoint )
        {
            DrawSegment( p1, p2, JointColor );
        }
        else if ( joint.GetJointType() == JointDef.JointType.PulleyJoint )
        {
            var pulley = ( PulleyJoint )joint;
            Vector2     s1     = pulley.GetGroundAnchorA();
            Vector2     s2     = pulley.GetGroundAnchorB();

            DrawSegment( s1, p1, JointColor );
            DrawSegment( s2, p2, JointColor );
            DrawSegment( s1, s2, JointColor );
        }
        else if ( joint.GetJointType() == JointDef.JointType.MouseJoint )
        {
            DrawSegment( joint.GetAnchorA(), joint.GetAnchorB(), JointColor );
        }
        else
        {
            DrawSegment( x1, p1, JointColor );
            DrawSegment( p1, p2, JointColor );
            DrawSegment( x2, p2, JointColor );
        }
    }

    private void DrawSegment( Vector2 x1, Vector2 x2, Color color )
    {
        Renderer.SetColor( color.R, color.G, color.B, color.A );
        Renderer.Line( x1.X, x1.Y, x2.X, x2.Y );
    }

    private void DrawContact( Contact contact )
    {
        WorldManifold worldManifold = contact.GetWorldManifold();

        if ( worldManifold.NumContactPoints == 0 )
        {
            return;
        }

        Vector2 point = worldManifold.ContactPoints[ 0 ];

        Color color = GetColorByBody( contact.GetFixtureA().GetBody() );
        
        Renderer.SetColor( color.R, color.G, color.B, color.A );
        Renderer.Point( point.X, point.Y, 0 );
    }

    public bool IsDrawBodies()
    {
        return _drawBodies;
    }

    public void SetDrawBodies( bool drawBodies )
    {
        this._drawBodies = drawBodies;
    }

    public bool IsDrawJoints()
    {
        return _drawJoints;
    }

    public void SetDrawJoints( bool drawJoints )
    {
        this._drawJoints = drawJoints;
    }

    public bool IsDrawAabBs()
    {
        return _drawAabBs;
    }

    public void SetDrawAabBs( bool drawAabBs )
    {
        this._drawAabBs = drawAabBs;
    }

    public bool IsDrawInactiveBodies()
    {
        return _drawInactiveBodies;
    }

    public void SetDrawInactiveBodies( bool drawInactiveBodies )
    {
        this._drawInactiveBodies = drawInactiveBodies;
    }

    public bool IsDrawVelocities()
    {
        return _drawVelocities;
    }

    public void SetDrawVelocities( bool drawVelocities )
    {
        this._drawVelocities = drawVelocities;
    }

    public bool IsDrawContacts()
    {
        return _drawContacts;
    }

    public void SetDrawContacts( bool drawContacts )
    {
        this._drawContacts = drawContacts;
    }

    public static Vector2 GetAxis()
    {
        return _axis;
    }

    public static void SetAxis( Vector2 axis )
    {
        _axis = axis;
    }

    public void Dispose()
    {
        Renderer.Dispose();
    }
}

// ============================================================================
// ============================================================================
