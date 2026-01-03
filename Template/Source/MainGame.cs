using System;
using System.Runtime.Versioning;
using Extensions.Source.Drawing.Freetype;
using JetBrains.Annotations;
using LughSharp.Core.Assets;
using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Cameras;
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.Main;
using LughSharp.Core.Maths;
using LughSharp.Core.Scenes.Scene2D;
using LughSharp.Core.Scenes.Scene2D.UI;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Logging;
using LughSharp.Tests.Source;
using Color = LughSharp.Core.Graphics.Color;

namespace Template.Source;

/// <summary>
/// TEST class, used for testing the framework.
/// </summary>
[PublicAPI]
public class MainGame : Game
{
    public bool IsDrawingStage { get; set; } = true;

    // ========================================================================

    private readonly Vector3 _cameraPos = Vector3.Zero;

    private OrthographicGameCamera?    _orthoGameCam;
    private OrthographicGameCamera?    _hudCam;
    private SpriteBatch?               _spriteBatch;
    private AssetManager?              _assetManager;
    private Texture?                   _image1;
    private Texture?                   _star;
    private Texture?                   _star2;
    private Stage?                     _stage;
    private Actor?                     _hudActor;
    private BitmapFont?                _font;
    private Sprite?                    _sprite;
    private bool                       _disposed;
    private Vector2                    _spritePosition = Vector2.Zero;
    private BitmapFont.BitmapFontData? _fontData;
    private ILughTest?                 _test;

    // ========================================================================
    // ========================================================================

    /// <inheritdoc />
    [SupportedOSPlatform( "windows" )]
    public override void Create()
    {
        _spriteBatch  = new SpriteBatch();
        _assetManager = new AssetManager();

        CreateCameras();

        CreateStage();
        CreateFont();       // Not working yet
        CreateAssets();
        CreateSprite();
//        CreateFreeTypeFont();

        Logger.Debug( "Done" );
    }

    // ========================================================================

    /// <inheritdoc />
    public override void Update()
    {
        if ( _sprite != null )
        {
//            _spritePosition.X += 4;
//            if ( _spritePosition.X > Engine.Api.Graphics.Width )
//            {
//                _spritePosition.X = -_sprite!.Width;
//            }

            _sprite?.Rotate( -1.0f );
            _sprite?.SetScale( 0.5f );
//        _sprite?.Scroll( 0.001f, 0.0f );
        }
    }

    /// <inheritdoc />
    public override void Render()
    {
        // Clear and set viewport
        ScreenUtils.Clear( color: Color.Blue, clearDepth: true );

        _spriteBatch?.EnableBlending();

        if ( _orthoGameCam is { IsInUse: true } )
        {
            _orthoGameCam.Viewport?.Apply( centerCamera: true );
            _spriteBatch?.SetProjectionMatrix( _orthoGameCam.Camera.Combined );
            _spriteBatch?.Begin();

            if ( _image1 != null )
            {
                _spriteBatch?.Draw( _image1,
                                    ( Engine.Api.Graphics.Width - _image1.Width ) / 2f,
                                    ( Engine.Api.Graphics.Height - _image1.Height ) / 2f );
            }

            if ( _star != null )
            {
                _spriteBatch?.Draw( _star, 0, 0 );
            }

            if ( _star2 != null )
            {
                _spriteBatch?.Draw( _star2, 320, 240 );
            }

            if ( _sprite != null )
            {
                _sprite?.SetPosition( _spritePosition.X, _spritePosition.Y );
                _sprite?.Draw( _spriteBatch! );
            }

            _test?.Render( _spriteBatch! );

            _ = _font?.Draw( _spriteBatch!, "HELLO WORLD", 100, 100 );
            
            _spriteBatch?.End();
        }

//        if ( _hudCam is { IsInUse: true } )
//        {
//            _hudCam.Viewport?.Apply( centerCamera: true );
//            _spriteBatch?.SetProjectionMatrix( _hudCam.Camera.Combined );
//            _spriteBatch?.Begin();
//
//            _spriteBatch?.End();
//        }

        // ----- Draw the Stage, if enabled -----
        if ( _stage != null && IsDrawingStage )
        {
            _stage.Act( Math.Min( Engine.Api.DeltaTime, ( 1.0f / 60.0f ) ) );
            _stage.Draw();
        }
    }

    /// <summary>
    /// Called when the <see cref="IApplication"/> is resized. This can happen at
    /// any point during a non-paused state but will never happen before a call
    /// to <see cref="Game.Create"/>
    /// </summary>
    /// <param name="width">The new width in pixels.</param>
    /// <param name="height">The new height in pixels.</param>
    public override void Resize( int width, int height )
    {
        _orthoGameCam?.ResizeViewport( width, height );
        _hudCam?.ResizeViewport( width, height );

        _stage?.Viewport.Update( width, height );
    }

    // ========================================================================

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or
    /// resetting unmanaged resources.
    /// </summary>
    public override void Dispose()
    {
        Logger.Checkpoint();

        Dispose( true );
        GC.SuppressFinalize( this );
    }

    protected override void Dispose( bool disposing )
    {
        if ( !_disposed )
        {
            if ( disposing )
            {
                _spriteBatch?.Dispose();
                _image1?.Dispose();
                _star?.Dispose();
                _star2?.Dispose();
                _assetManager?.Dispose();
                _orthoGameCam?.Dispose();
                _hudCam?.Dispose();
                _font?.Dispose();
            }

            _disposed = true;
        }
    }

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    private void CreateCameras()
    {
        _orthoGameCam = new OrthographicGameCamera( Engine.Api.Graphics.Width,
                                                    Engine.Api.Graphics.Height,
                                                    name: "MainCamera" );

        _orthoGameCam.Camera.Near = CameraData.DEFAULT_NEAR_PLANE;
        _orthoGameCam.Camera.Far  = CameraData.DEFAULT_FAR_PLANE;
        _orthoGameCam.IsInUse     = true;
        _orthoGameCam.SetZoomDefault( 1f );

        // Set initial camera position
        _orthoGameCam.SetPosition( new Vector3( 0, 0, CameraData.DEFAULT_Z ) );
        _orthoGameCam.Update();

        // --------------------------------------

        _hudCam = new OrthographicGameCamera( Engine.Api.Graphics.Width,
                                              Engine.Api.Graphics.Height,
                                              name: "HUDCamera" );

        _hudCam.Camera.Near = CameraData.DEFAULT_NEAR_PLANE;
        _hudCam.Camera.Far  = CameraData.DEFAULT_FAR_PLANE;
        _hudCam.IsInUse     = true;
        _hudCam.SetZoomDefault( 1f );

        // Set initial camera position
        _hudCam.SetPosition( new Vector3( 0, 0, CameraData.DEFAULT_Z ) );
        _hudCam.Update();
    }

    private void CreateAssets()
    {
        _image1 = new Texture( Assets.BACKGROUND_IMAGE );
        _star   = new Texture( Assets.COMPLETE_STAR );
        _star2  = new Texture( Assets.COMPLETE_STAR );
    }

    private void CreateStage()
    {
        _stage    = new Stage( _hudCam?.Viewport, _spriteBatch );
        _hudActor = new Scene2DImage( new Texture( Assets.HUD_PANEL ) );

        _hudActor.IsVisible   = true;
        _hudActor.DebugActive = true;
        _hudActor.SetPosition( 0, 0 );
        _stage?.AddActor( _hudActor );
    }

    private void CreateSprite()
    {
        var spriteImage = new Texture( Assets.KEY_COLLECTED );

        _spritePosition.Set( 40, 120 );

        _sprite = new Sprite( new TextureRegion( spriteImage ) );

        _sprite.SetPosition( _spritePosition.X, _spritePosition.Y );
        _sprite.SetBounds();
        _sprite.SetOriginCenter();
        _sprite.SetColor( Color.White );
        _sprite?.SetFlip( true, true );
    }

    private void CreateFont()
    {
        _fontData = new BitmapFont.BitmapFontData( Engine.Api.Files.Internal( Assets.AMBLE_REGULAR_26_FONT ) );

        _font = new BitmapFont();
        _font.SetColor( Color.White );
        _font.UseIntegerPositions = true;
    }

    private void CreateFreeTypeFont()
    {
        var generator = new FreeTypeFontGenerator( Engine.Api.Files.Internal( Assets.AMBLE_REGULAR_26_FONT ) );
        var parameter = new FreeTypeFontGenerator.FreeTypeFontParameter
        {
            Size = 40,
        };

        _font = generator.GenerateFont( parameter );
        _font.SetColor( Color.White );
    }
}

// ============================================================================
// ============================================================================