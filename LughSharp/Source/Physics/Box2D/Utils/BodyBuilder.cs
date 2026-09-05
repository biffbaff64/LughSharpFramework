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

namespace LughSharp.Source.Physics.Box2D.Utils;

[PublicAPI]
public class BodyBuilder( float ppm, World world )
{
    // ========================================================================

    /// <summary>
    ///  Creates a new body from the given rectangle.
    /// </summary>
    /// <param name="rectangle"> The rectangle to create the body from. </param>
    /// <param name="bodyCategory"> The category of the body. </param>
    /// <param name="collidesWith"> The categories that the body collides with. </param>
    /// <param name="bodyType"> The <see cref="PhysicsBodyType"/> of the body. </param>
    /// <returns></returns>
    public Body NewBody( Rectangle rectangle, short bodyCategory, short collidesWith, PhysicsBodyType bodyType )
    {
        Body body;
        var descriptor = new B2BodyDescriptor
        {
            BodyType = bodyType
        };

        switch ( bodyType )
        {
            // ----------------------------------------------------------------

            case PhysicsBodyType.Dynamic:
            case PhysicsBodyType.DynamicSensor:
            case PhysicsBodyType.DynamicBouncy:
            case PhysicsBodyType.Kinematic:
            case PhysicsBodyType.KinematicSensor:
                descriptor.Shape   = CreatePolygonShape( rectangle );
                descriptor.Density = PhysicsConstants.DefaultDensity;

                if ( bodyType == PhysicsBodyType.DynamicBouncy )
                {
                    descriptor.Friction    = PhysicsConstants.LowFriction;
                    descriptor.Restitution = PhysicsConstants.HighRestitution;
                }
                else
                {
                    descriptor.Friction    = PhysicsConstants.DefaultFriction;
                    descriptor.Restitution = PhysicsConstants.LowRestitution;
                }

                descriptor.Filter = new CollisionFilter
                    (
                     bodyCategory,
                     collidesWith,
                     ( ( bodyType == PhysicsBodyType.DynamicSensor )
                    || ( bodyType == PhysicsBodyType.KinematicSensor ) )
                    );

                if ( ( bodyType == PhysicsBodyType.Kinematic )
                  || ( bodyType == PhysicsBodyType.KinematicSensor ) )
                {
                    body = CreateKinematicBody( rectangle, descriptor );
                }
                else
                {
                    body = CreateDynamicBox( rectangle, descriptor );
                }

                break;

            // ----------------------------------------------------------------

            case PhysicsBodyType.DynamicHeavy:
                descriptor.Shape       = CreatePolygonShape( rectangle );
                descriptor.Density     = PhysicsConstants.FullDensity;
                descriptor.Friction    = PhysicsConstants.FullFriction;
                descriptor.Restitution = PhysicsConstants.ZeroRestitution;
                descriptor.Filter = new CollisionFilter
                    (
                     bodyCategory,
                     collidesWith,
                     false
                    );
                body = CreateDynamicBox( rectangle, descriptor );

                break;

            // ----------------------------------------------------------------

            case PhysicsBodyType.DynamicPushable:
                descriptor.Shape       = CreatePolygonShape( rectangle );
                descriptor.Density     = PhysicsConstants.DefaultDensity;
                descriptor.Friction    = PhysicsConstants.MediumLowFriction;
                descriptor.Restitution = PhysicsConstants.LowRestitution;
                descriptor.Filter = new CollisionFilter
                    (
                     bodyCategory,
                     collidesWith,
                     false
                    );
                body = CreateDynamicBox( rectangle, descriptor );

                break;

            // ----------------------------------------------------------------

            case PhysicsBodyType.DynamicCircle:
            case PhysicsBodyType.DynamicCircleSensor:
                var circle = new Circle
                {
                    X      = rectangle.X,
                    Y      = rectangle.Y,
                    Radius = ( rectangle.Width / 2 ) / ppm
                };
                descriptor.Shape       = CreateCircleShape( circle );
                descriptor.Density     = PhysicsConstants.DefaultDensity;
                descriptor.Friction    = PhysicsConstants.LowFriction;
                descriptor.Restitution = PhysicsConstants.MediumLowRestitution;
                descriptor.Filter = new CollisionFilter
                    (
                     bodyCategory,
                     collidesWith,
                     ( bodyType == PhysicsBodyType.DynamicCircleSensor )
                    );
                body = CreateDynamicCircle( circle, descriptor );

                break;

            // ----------------------------------------------------------------

            case PhysicsBodyType.Static:
            case PhysicsBodyType.StaticSensor:
                descriptor.Shape       = CreatePolygonShape( rectangle );
                descriptor.Density     = PhysicsConstants.FullDensity;
                descriptor.Friction    = PhysicsConstants.FullFriction;
                descriptor.Restitution = PhysicsConstants.MediumLowRestitution;
                descriptor.Filter = new CollisionFilter
                    (
                     bodyCategory,
                     collidesWith,
                     ( bodyType == PhysicsBodyType.StaticSensor )
                    );
                body = CreateStaticBody( rectangle, descriptor );

                break;

            // ----------------------------------------------------------------

            default:
                throw new LughRuntimeException( $"UNKNOWN BODY TYPE SPECIFIED: {bodyType}" );
        }

        descriptor.Dispose();

        return body;
    }

    /// <summary>
    /// Creates a Dynamic Box2D body which can be assigned to a <see cref="Sprite2D"/>.
    /// Dynamic bodies are objects which move around and are affected by forces and other
    /// dynamic, kinematic and static objects.
    /// <br/>
    /// This body will have a <b>Polygon</b> shape.
    /// <br/>
    /// Dynamic bodies are suitable for any object which needs to move and be affected
    /// by forces.
    /// </summary>
    /// <param name="rectangle"></param>
    /// <param name="descriptor"></param>
    /// <returns></returns>
    public Body CreateDynamicBox( Rectangle rectangle, B2BodyDescriptor descriptor )
    {
        PolygonShape shape   = CreatePolygonShape( rectangle );
        BodyDef      bodyDef = CreateBodyDef( BodyDef.BodyType.Dynamic, rectangle );
        FixtureDef fixtureDef = CreateFixtureDef
            (
             descriptor.Filter,
             shape,
             descriptor.Density,
             descriptor.Friction,
             descriptor.Restitution
            );

        Body body = BuildBody( bodyDef, fixtureDef );

        if ( fixtureDef.IsSensor )
        {
            body.GravityScale = 0;
        }

        shape.Dispose();

        return body;
    }

    /// <summary>
    /// Creates a Dynamic Box2D body which can be assigned to a <see cref="Sprite2D"/>.
    /// Dynamic bodies are objects which move around and are affected by forces and other
    /// dynamic, kinematic and static objects.
    /// <br/>
    /// This body will have a <b>Circle</b> shape.
    /// <br/>
    /// Dynamic bodies are suitable for any object which needs to move and be affected
    /// by forces.
    /// </summary>
    /// <param name="circle"></param>
    /// <param name="descriptor"></param>
    /// <returns></returns>
    public Body CreateDynamicCircle( Circle circle, B2BodyDescriptor descriptor )
    {
        BodyDef bodyDef = CreateBodyDef( BodyDef.BodyType.Dynamic, circle );
        FixtureDef fixtureDef = CreateFixtureDef
            (
             descriptor.Filter,
             CreateCircleShape( circle ),
             descriptor.Density,
             descriptor.Friction,
             descriptor.Restitution
            );

        Body body = BuildBody( bodyDef, fixtureDef );

        if ( fixtureDef.IsSensor )
        {
            body.GravityScale = 0;
        }

        return body;
    }

    /// <summary>
    /// Creates a Kinematic Box2D body which can be assigned to a <see cref="Sprite2D"/>.
    /// Kinematic bodies are somewhat in between static and dynamic bodies.
    /// Like static bodies, they do not react to forces, but like dynamic bodies,
    /// they do have the ability to move. Kinematic bodies are great for things
    /// where you, the programmer, want to be in full control of a body's motion,
    /// such as a moving platform in a platform game.
    /// It is possible to set the position on a kinematic body directly, but it's
    /// usually better to set a velocity instead, and letting Box2D take care of
    /// position updates.
    /// </summary>
    public Body CreateKinematicBody( Rectangle rectangle, B2BodyDescriptor descriptor )
    {
        PolygonShape shape   = CreatePolygonShape( rectangle );
        BodyDef      bodyDef = CreateBodyDef( BodyDef.BodyType.Kinematic, rectangle );
        FixtureDef fixtureDef = CreateFixtureDef
            (
             descriptor.Filter,
             shape,
             descriptor.Density,
             descriptor.Friction,
             descriptor.Restitution
            );

        Body body = BuildBody( bodyDef, fixtureDef );

        if ( fixtureDef.IsSensor )
        {
            body.GravityScale = 0;
        }

        shape.Dispose();

        return body;
    }

    /// <summary>
    /// Creates a Static Box2D body.
    /// <para/>
    /// Static bodies are objects which do not move and are not affected by forces.
    /// Dynamic bodies are affected by static bodies. Static bodies are perfect for
    /// ground, walls, and any object which does not need to move. Static bodies
    /// require less computing power.
    /// </summary>
    public Body CreateStaticBody( Rectangle rectangle, B2BodyDescriptor descriptor )
    {
        PolygonShape shape   = CreatePolygonShape( rectangle );
        BodyDef      bodyDef = CreateBodyDef( BodyDef.BodyType.Static, rectangle );
        FixtureDef fixtureDef = CreateFixtureDef
            (
             descriptor.Filter,
             shape,
             descriptor.Density,
             descriptor.Friction,
             descriptor.Restitution
            );
        Body body = BuildBody( bodyDef, fixtureDef );

        shape.Dispose();

        return body;
    }

    private Body BuildBody( BodyDef bodyDef, FixtureDef fixtureDef )
    {
        Body body = world.CreateBody( bodyDef );
        body.CreateFixture( fixtureDef );

        return body;
    }

    private BodyDef CreateBodyDef( BodyDef.BodyType bodyType, Rectangle rectangle )
    {
        var bodyDef = new BodyDef
        {
            Type          = bodyType,
            FixedRotation = true
        };

        bodyDef.Position.Set
            (
             ( rectangle.X + ( rectangle.Width / 2f ) ) / ppm,
             ( rectangle.Y + ( rectangle.Height / 2f ) ) / ppm
            );

        return bodyDef;
    }

    private BodyDef CreateBodyDef( BodyDef.BodyType bodyType, Circle circle )
    {
        var bodyDef = new BodyDef
        {
            Type          = bodyType,
            FixedRotation = true
        };

        // circle.x / circle.y are in pixels; circle.radius is already in
        // world units (metres). Convert the origin to metres, then add the
        // radius so the body is centred on the sprite rather than its corner.
        bodyDef.Position.Set
            (
             ( circle.X / ppm ) + circle.Radius,
             ( circle.Y / ppm ) + circle.Radius
            );

        return bodyDef;
    }

    private FixtureDef CreateFixtureDef( CollisionFilter filter,
                                         Shape shape,
                                         float density,
                                         float friction,
                                         float restitution )
    {
        var fixtureDef = new FixtureDef
        {
            Shape       = shape,
            Density     = density,
            Friction    = friction,
            Restitution = restitution,
            Filter =
            {
                MaskBits     = filter.CollidesWith,
                CategoryBits = filter.BodyCategory
            },
            IsSensor = filter.IsSensor
        };

        return fixtureDef;
    }

    private PolygonShape CreatePolygonShape( Rectangle rectangle )
    {
        var shape = new PolygonShape();

        shape.SetAsBox
            (
             ( ( rectangle.Width / 2f ) / ppm ),
             ( ( rectangle.Height / 2f ) / ppm )
            );

        return shape;
    }

    private CircleShape CreateCircleShape( Circle circle )
    {
        var shape = new CircleShape();
        shape.SetRadius( circle.Radius );

        return shape;
    }
}

// ============================================================================
// ============================================================================
