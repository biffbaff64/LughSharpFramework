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

using LughSharp.Source.Graphics.G2D;

namespace LughSharp.Source.Physics.Box2D.Graphics;

/// <summary>
/// ParticleEmitterBox2D uses box2d rayCasting to achieve continuous collision detection
/// against box2d fixtures.
/// <br/>
/// If a particle detects collision it changes its direction before actual collision
/// would occur.
/// <br/>
/// Velocity is 100% reflected.
/// <br/>
/// Particles can't collide with other particles.
/// <br/>
/// These particles do not have any other physical attributes or functionality.
/// </summary>
[PublicAPI]
public class ParticleEmitterBox2D : ParticleEmitter
{
    private readonly World   _world;
    private readonly Vector2 _startPoint = new();
    private readonly Vector2 _endPoint   = new();
    private          bool    _particleCollided;
    private          float   _normalAngle;

    // If velocities squared is shorter than this it could lead 0 length
    // rayCast that cause c++ assertion at box2d
    private static readonly float _epsilon = 0.001f;

    // default visibility to prevent synthetic accessor creation
    private readonly IRayCastCallback _rayCallBack;

    public class RayCallBackImpl( ParticleEmitterBox2D parent ) : IRayCastCallback
    {
        public float ReportRayFixture( Fixture fixture, Vector2 point, Vector2 normal, float fraction )
        {
            parent._particleCollided = true;
            parent._normalAngle      = MathUtils.Atan2( normal.Y, normal.X ) * MathUtils.RadiansToDegrees;

            return fraction;
        }
    }

    /// <summary>
    /// Constructs default ParticleEmitterBox2D. Box2d World is used for rayCasting.
    /// Assumes that particles use same unit system that box2d world does.
    /// </summary>
    /// <param name="world"></param>
    public ParticleEmitterBox2D( World world )
    {
        this._world       = world;
        this._rayCallBack = new RayCallBackImpl( this );
    }

    /// <summary>
    /// Constructs ParticleEmitterBox2D using bufferedReader. Box2d World is used for
    /// rayCasting. Assumes that particles use same unit system that box2d world does.
    /// </summary>
    /// <param name="world"></param>
    /// <param name="reader"></param>
    public ParticleEmitterBox2D( World world, StreamReader reader )
        : base( reader )
    {
        this._world       = world;
        this._rayCallBack = new RayCallBackImpl( this );
    }

    /// <summary>
    /// Constructs ParticleEmitterBox2D fully copying given emitter attributes. Box2d
    /// World is used for rayCasting. Assumes that particles use same unit system that
    /// box2d world does.
    /// </summary>
    /// <param name="world"> The Box2D world. </param>
    /// <param name="emitter">
    /// The <see cref="ParticleEmitter"/> whose attributes to copy.
    /// </param>
    public ParticleEmitterBox2D( World world, ParticleEmitter emitter )
        : base( emitter )
    {
        this._world       = world;
        this._rayCallBack = new RayCallBackImpl( this );
    }

    protected ParticleEmitter.Particle NewParticle( Sprite2D sprite )
    {
        return new ParticleBox2D( sprite, this );
    }

    /// <summary>
    /// Particle that can collide to box2d fixtures
    /// </summary>
    private class ParticleBox2D( Sprite2D sprite, ParticleEmitterBox2D parent ) : Particle( sprite )
    {
        /// <summary>
        /// translate particle given amount. Continuous collision detection achieved by
        /// using RayCast from oldPos to newPos.
        /// </summary>
        /// <param name="velocityX"></param>
        /// <param name="velocityY"></param>
        public override void Translate( float velocityX, float velocityY )
        {
            // If velocities squares summed is shorter than Epsilon it could lead ~0
            // length rayCast that cause nasty c++ assertion inside box2d. This is so
            // short distance that moving particle has no effect so this return early.
            if ( ( ( velocityX * velocityX ) + ( velocityY * velocityY ) ) < _epsilon )
            {
                return;
            }

            // Position offset is half of sprite texture size.
            float x = GetX() + ( Width / 2f );
            float y = GetY() + ( Height / 2f );

            // collision flag to false
            parent._particleCollided = false;
            parent._startPoint.Set( x, y );
            parent._endPoint.Set( x + velocityX, y + velocityY );
            parent._world.RayCast( parent._rayCallBack, parent._startPoint, parent._endPoint );

            // If ray collided bool has set to true at rayCallBack
            if ( parent._particleCollided )
            {
                // perfect reflection
                Angle     =  ( 2f * parent._normalAngle ) - Angle - 180f;
                AngleCos  =  MathUtils.CosDeg( Angle );
                AngleSin  =  MathUtils.SinDeg( Angle );
                velocityX *= AngleCos;
                velocityY *= AngleSin;
            }

            base.Translate( velocityX, velocityY );
        }
    }
}

// ============================================================================
// ============================================================================
