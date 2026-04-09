using Extensions.Source;

using JetBrains.Annotations;

using LughSharp.Core;
using LughSharp.Core.Assets;
using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Cameras;
using LughSharp.Core.Graphics.Fonts;
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Graphics.Images;
using LughSharp.Core.Input;
using LughSharp.Core.Maths;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Logging;

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
    private NinePatch?              _ninePatch;
    private BitmapFont?             _font;
    private BitmapFont?             _freetypeFont;
    private InputMultiplexer        _inputMultiplexer = new();
    
    private bool    _disposed;

    private readonly MapTests    _mapTests    = new();
    private readonly FontTests   _fontTests   = new();
    private readonly SpriteTests _spriteTests = new();
    private readonly StageTests  _stageTests  = new();
    private readonly ImageTests  _imageTests  = new();

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
        CreateAssets();
//        _mapTests.CreateMap();
        _stageTests.CreateStage( _hudCam, ref _inputMultiplexer );
//        _font         = _fontTests.CreateBitmapFont();
//        _freetypeFont = _fontTests.CreateFreeTypeFont();
//        _spriteTests.Create();

        _font = new BitmapFont( new FileInfo( Assets.ArialFont ) );
        
        if ( _inputMultiplexer.Processors.Size > 0 )
        {
            Engine.Input.InputProcessor = _inputMultiplexer;

            Logger.Debug( "InputProcessor set." );
        }

        Logger.Debug( "Done" );
    }

    /// <inheritdoc />
    public override void Update( float delta )
    {
        _spriteTests.Update( delta );
        _mapTests.ScrollMap( ref _tiledMapCam );
    }

    /// <inheritdoc />
    public override void Render( float delta )
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
                    _spriteBatch.Draw( _texture, 20, 20 );
                }

                // TextureRegion
//                if ( _textureRegion != null )
//                {
//                    _spriteBatch.Draw( _textureRegion, 400, 100 );
//                }

                // NinePatch
//                if ( _ninePatch != null )
//                {
//                    _ninePatch.Draw( _spriteBatch, 500, 100, 100, 10 );
//                }

                _spriteTests.Draw( _spriteBatch );

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

                _font?.Draw( _spriteBatch, $"X:{Engine.Input.GetX()} Y:{Engine.Input.GetY()}", 10, 700 );

                _spriteBatch.End();
            }
        }

        // ----- Draw the Stage, if enabled -----
        if ( _stageTests.Stage != null )
        {
            Scene2DUtils.Update( _stageTests.Stage, _isDrawingStage );
        }
    }

    /// <inheritdoc />
    public override void Resize( int width, int height )
    {
        _tiledMapCam?.ResizeViewport( width, height );
        _spriteCam?.ResizeViewport( width, height );

        _stageTests.Stage?.Viewport.Update( width, height );
    }

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
        _hudCam.SetPosition( new Vector3( 0, 0, CameraData.DefaultZ ) );
        _hudCam.Update();
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
                _spriteTests.Dispose();
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

    private void CreateAssets()
    {
        _texture       = new Texture( Assets.Solid112X112 );
        _textureRegion = new TextureRegion( new Texture( Assets.CompleteStar ) );
        _ninePatch     = new NinePatch( new Texture( Assets.Bar9 ), 1, 1, 1, 1 );
    }
}

// ============================================================================
// ============================================================================