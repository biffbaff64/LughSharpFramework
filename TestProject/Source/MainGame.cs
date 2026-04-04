using System;
using System.IO;
using System.Runtime.Versioning;

using JetBrains.Annotations;

using LughSharp.Core.Assets;
using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Atlases;
using LughSharp.Core.Graphics.Cameras;
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Graphics.Images;
using LughSharp.Core.Graphics.OpenGL.Enums;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.Input;
using LughSharp.Core.Main;
using LughSharp.Core.Maps.Tiled;
using LughSharp.Core.Maps.Tiled.Loaders;
using LughSharp.Core.Maps.Tiled.Renderers;
using LughSharp.Core.Maths;
using LughSharp.Core.SceneGraph2D;
using LughSharp.Core.SceneGraph2D.RegistryStyles;
using LughSharp.Core.SceneGraph2D.UI;
using LughSharp.Core.SceneGraph2D.UI.Styles;
using LughSharp.Core.SceneGraph2D.Utils;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Logging;
using LughSharp.Tests.Source;

namespace TestProject.Source;

[PublicAPI]
public class MainGame : LughGame
{
    private readonly bool _isDrawingStage = true;

    // ========================================================================

    private SpriteBatch?            _spriteBatch;
    private AssetManager?           _assetManager;
    private OrthographicGameCamera? _tiledMapCam;
    private OrthographicGameCamera? _spriteCam;
    private OrthographicGameCamera? _hudCam;
    private Texture?                _texture;
    private TextureRegion?          _textureRegion;
    private BitmapFont?             _font;
    private BitmapFont?             _freetypeFont;
    private Sprite2D?               _sprite;
    private InputMultiplexer        _inputMultiplexer = new();

    private bool _disposed;

    private readonly MapTests    _mapTests    = new();
    private readonly FontTests   _fontTests   = new();
    private readonly SpriteTests _spriteTests = new();
    private readonly StageTests  _stageTests  = new();

    // ========================================================================
    // ========================================================================

    /// <inheritdoc />
    public override void Create()
    {
        // Do not create a new SpriteBatch before this point,
        // as OpenGL will not be ready.
        _spriteBatch  = new SpriteBatch();
        _assetManager = new AssetManager();

        CreateCameras();
//        CreateAssets();
        _stageTests.CreateStage( _hudCam, ref _inputMultiplexer );
//        _font         = _fontTests.CreateBitmapFont();
//        _freetypeFont = _fontTests.CreateFreeTypeFont();
        _sprite = _spriteTests.CreateSprite( new TextureRegion( new Texture( Assets.KeyCollected ) ) );
        _mapTests.CreateMap();

        if ( _inputMultiplexer.Processors.Size > 0 )
        {
            Engine.Input.InputProcessor = _inputMultiplexer;

            Logger.Debug( "InputProcessor set." );
        }

        Logger.Debug( "Done" );
    }

    /// <inheritdoc />
    public override void Update()
    {
        _spriteTests.ScaleSprite( _sprite );
        _mapTests.ScrollMap( ref _tiledMapCam );
    }

    /// <inheritdoc />
    public override void Render()
    {
        // Clear and set viewport
        ScreenUtils.Clear( Color.Blue, true );

        // --------------------------------------

        if ( _mapTests.MapRenderer != null && _tiledMapCam is { IsInUse: true } )
        {
            _tiledMapCam.Update();

            _mapTests.MapRenderer.SetView( _tiledMapCam.Camera );
            _mapTests.MapRenderer.Render();
        }

        // --------------------------------------

        if ( _spriteBatch != null )
        {
            _spriteBatch.EnableBlending();

            if ( _spriteCam is { IsInUse: true } )
            {
                _spriteCam.Viewport?.Apply( true );
                _spriteBatch.SetProjectionMatrix( _spriteCam.Camera.Combined );
                _spriteBatch.Begin();

                _spriteCam.Position.X = 0;
                _spriteCam.Position.Y = 0;
                _spriteCam.Position.Z = 0;
                _spriteCam.Update();

                // Texture
                if ( _texture != null )
                {
                    _spriteBatch.Draw( _texture, 300, 300 );
                }

                // TextureRegion
                if ( _textureRegion != null )
                {
                    _spriteBatch.Draw( _textureRegion, 400, 100 );
                }

                if ( _sprite != null )
                {
                    _sprite.SetPosition( 100, 100 );
                    _sprite.Draw( _spriteBatch );
                }

                _spriteBatch.End();
            }

            // The Camera used for the Stage doesn't actually need updating
            // here, as the Stage will update it internally before drawing,
            // but we'll update it here anyway to test printing of BitmapFonts.

            if ( _hudCam is { IsInUse: true } )
            {
                _hudCam.Viewport?.Apply( true );
                _spriteBatch.SetProjectionMatrix( _hudCam.Camera.Combined );
                _spriteBatch.Begin();

                _hudCam.Position.X = 0;
                _hudCam.Position.Y = 0;
                _hudCam.Position.Z = 0;
                _hudCam.Update();

                _font?.Draw( _spriteBatch );

                _spriteBatch.End();
            }
        }

        // ----- Draw the Stage, if enabled -----
        _stageTests.Update( _isDrawingStage );
    }

    /// <inheritdoc />
    public override void Resize( int width, int height )
    {
        _tiledMapCam?.ResizeViewport( width, height );
        _spriteCam?.ResizeViewport( width, height );

        _stageTests.Stage?.Viewport.Update( width, height );
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        Dispose( true );
        GC.SuppressFinalize( this );
    }

    protected override void Dispose( bool disposing )
    {
        if ( !_disposed )
        {
            if ( disposing )
            {
                _assetManager?.Dispose();
                _spriteBatch?.Dispose();
                _tiledMapCam?.Dispose();
                _spriteCam?.Dispose();
                _texture?.Dispose();
                _stageTests.Dispose();
                _font?.Dispose();

                // TODO:
                // Should Sprite() implement IDisposable, or should I leave that up to
                // any extending classes? Maybe implement IDisposable in Sprite() and
                // allow ( and encourage ) extending classes to override it?
                _sprite = null;
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
        _tiledMapCam = new OrthographicGameCamera( Engine.Graphics.WindowWidth,
                                                   Engine.Graphics.WindowHeight,
                                                   name: "TILEDCamera" );

        _tiledMapCam.Camera.Near = CameraData.DefaultNearPlane;
        _tiledMapCam.Camera.Far  = CameraData.DefaultFarPlane;
        _tiledMapCam.IsInUse     = true;
        _tiledMapCam.SetZoomDefault( 1f );
        _tiledMapCam.SetPosition( new Vector3( 0, 0, CameraData.DefaultZ ) );
        _tiledMapCam.Update();


        // --------------------------------------

        _spriteCam = new OrthographicGameCamera( Engine.Graphics.WindowWidth,
                                                 Engine.Graphics.WindowHeight,
                                                 name: "SPRITECamera" );

        _spriteCam.Camera.Near = CameraData.DefaultNearPlane;
        _spriteCam.Camera.Far  = CameraData.DefaultFarPlane;
        _spriteCam.IsInUse     = true;
        _spriteCam.SetZoomDefault( 1f );

        // Set initial camera position
        _spriteCam.SetPosition( new Vector3( 0, 0, CameraData.DefaultZ ) );
        _spriteCam.Update();

        // --------------------------------------

        _hudCam = new OrthographicGameCamera( Engine.Graphics.WindowWidth,
                                              Engine.Graphics.WindowHeight,
                                              name: "HUDCamera" );

        _hudCam.Camera.Near = CameraData.DefaultNearPlane;
        _hudCam.Camera.Far  = CameraData.DefaultFarPlane;
        _hudCam.IsInUse     = true;
        _hudCam.SetZoomDefault( 1f );

        // Set initial camera position
        _hudCam.SetPosition( new Vector3( 0, 0, CameraData.DefaultZ ) );
        _hudCam.Update();
    }

    private void CreateAssets()
    {
        _texture       = new Texture( Assets.CompleteStar );
        _textureRegion = new TextureRegion( new Texture( Assets.CompleteStar ) );
    }
}

// ============================================================================
// ============================================================================