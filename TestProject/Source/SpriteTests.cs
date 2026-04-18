// ///////////////////////////////////////////////////////////////////////////////
// MIT License
// 
// Copyright (c) 2024, 2025, 2026 Circa64 Software Projects / Richard Ikin.
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

using Extensions.Source;

using JetBrains.Annotations;

using LughSharp.Core;
using LughSharp.Core.Assets;
using LughSharp.Core.Files;
using LughSharp.Core.Graphics.Atlases;
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Graphics.Images;
using LughSharp.Core.Maths;
using LughSharp.Core.Utils.Logging;

namespace TestProject.Source;

[PublicAPI]
public class SpriteTests : IDisposable
{
    private AssetManager                _assetManager;
    private Vector2                     _spritePosition = Vector2.Zero;
    private Sprite2D?                   _sprite;
    private Sprite2D?                   _sprite2;
    private int                         _xdirection;
    private int                         _ydirection;
    private float                       _xspeed;
    private float                       _yspeed;
    private TextureRegion[]?            _animFrames;
    private Animation< TextureRegion >? _animation;
    private float                       _elapsedAnimTime;

    // ========================================================================

    public SpriteTests( AssetManager assetManager )
    {
        _assetManager = assetManager;
    }

    public void Create()
    {
        Logger.Debug( "Creating Sprite Tests" );

        var path = @$"{Files.ContentRoot}\packedimages\output\animations.atlas";

        var animator = new Animator( _assetManager );

        _animation = animator.CreateAnimation( path, "coin", 24, 24 );

        _sprite = new Sprite2D( _animation!.GetKeyFrame( 0 , false) );
        _sprite.SetBounds();
        _sprite.SetOriginCenter();
        _sprite.SetPosition( 200, 600 );

        _sprite2 = new Sprite2D( new Texture2D( Assets.Boulder48X48 ) );
        _sprite2.SetBounds();
        _sprite2.SetOriginCenter();
        _sprite2.SetPosition( 400, 300 );

        Logger.Debug( "Finished." );
    }

    public void Update( float delta )
    {
        if ( _sprite != null && _animation != null )
        {
            _sprite.SetRegion( _animation.GetKeyFrame( _elapsedAnimTime, true ) );
            _elapsedAnimTime += delta;
        }
    }

    public void Draw( SpriteBatch batch )
    {
        _sprite?.Draw( batch );
        _sprite2?.Draw( batch );
    }

    private void Bounce( int index )
    {
        if ( _sprite == null )
        {
            return;
        }

        if ( _sprite.GetX() <= -_sprite.Width )
        {
            _xdirection = 1;
        }
        else if ( _sprite.GetX() >= Engine.Graphics.WindowWidth )
        {
            _xdirection = -1;
        }

        if ( _sprite.GetY() <= -_sprite.Height )
        {
            _ydirection = 1;
        }
        else if ( _sprite.GetY() >= Engine.Graphics.WindowHeight )
        {
            _ydirection = -1;
        }

        _sprite.TranslateX( _xspeed * _xdirection * Engine.DeltaTime );
        _sprite.TranslateY( _yspeed * _ydirection * Engine.DeltaTime );
    }

    public void Dispose()
    {
        // TODO:
        // Should Sprite() implement IDisposable, or should I leave that up to
        // any extending classes? Maybe implement IDisposable in Sprite() and
        // allow ( and encourage ) extending classes to override it?
//        _sprite = null;

        GC.SuppressFinalize( this );
    }
}

// ============================================================================
// ============================================================================