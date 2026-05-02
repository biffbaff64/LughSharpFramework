using Extensions.Source;

using JetBrains.Annotations;

using LughSharp.Source;
using LughSharp.Source.Assets;
using LughSharp.Source.Graphics;
using LughSharp.Source.Graphics.Atlases;
using LughSharp.Source.Graphics.Cameras;
using LughSharp.Source.Graphics.Fonts;
using LughSharp.Source.Graphics.G2D;
using LughSharp.Source.Graphics.Images;
using LughSharp.Source.Input;
using LughSharp.Source.IO;
using LughSharp.Source.Maths;
using LughSharp.Source.Scene2D;
using LughSharp.Source.Scene2D.UI;
using LughSharp.Source.Utils;
using LughSharp.Source.Utils.Logging;
using LughSharp.Tests.Source;

namespace TestProject.Source;

[PublicAPI]
public class MainGame : LughGame
{
    private readonly bool _isDrawingStage = true;

    // ========================================================================

    private SpriteBatch?            _spriteBatch;
    private AssetManager?           _assetManager;
    private OrthographicGameCamera? _backgroundCam;
    private OrthographicGameCamera? _tiledMapCam;
    private OrthographicGameCamera? _spriteCam;
    private OrthographicGameCamera? _hudCam;
    private Vector3                 _cameraPos = Vector3.Zero;
    private Texture2D?              _backgroundTexture;
    private Texture2D?              _texture;
    private TextureRegion?          _textureRegion;
    private NinePatch?              _ninePatch;
    private BitmapFont?             _font;
    private BitmapFont?             _freetypeFont;
    private InputMultiplexer        _inputMultiplexer = new();
    private AtlasLoader?            _atlasLoader;
    private Stage?                  _stage;

    private bool _disposed;

    private readonly FontTests  _fontTests  = new();
    private readonly StageTests _stageTests = new();
    private readonly ImageTests _imageTests = new();

    private MapTests?    _mapTests;
    private SpriteTests? _spriteTests;
    private InputTest?   _inputTest;
    private TableTest?   _tableTest;

    // ========================================================================
    // ========================================================================

    /// <inheritdoc />
    public override void Create()
    {
        // Do not create a new SpriteBatch before this point,
        // as OpenGL will not be ready.
        _spriteBatch  = new SpriteBatch();
        _assetManager = new AssetManager();
        _atlasLoader  = new AtlasLoader( _assetManager );

        _atlasLoader.RegisterAtlas( Assets.AnimationsAtlas )
                    .RegisterAtlas( Assets.ObjectsAtlas )
                    .RegisterAtlas( Assets.InputAtlas )
                    .RegisterAtlas( Assets.TextAtlas )
                    .Load();

        CreateCameras();

        // --------------------------------------
        if ( _hudCam != null )
        {
            _stage = new Stage( _hudCam.Viewport );
        }
        // --------------------------------------
        
        CreateAssets();
        
        // --------------------------------------
//        _inputTest = new InputTest( ref _inputMultiplexer );
//        _inputTest.Setup();
        // --------------------------------------
//        _mapTests = new MapTests();
//        _mapTests.CreateMap();
        // --------------------------------------
//        _stageTests.CreateStage( _hudCam, ref _stage, ref _inputMultiplexer );
        // --------------------------------------
//        _font         = _fontTests.CreateBitmapFont();
//        _freetypeFont = _fontTests.CreateFreeTypeFont();
        // --------------------------------------
//        _spriteTests = new SpriteTests( _assetManager );
//        _spriteTests.Create();
        // --------------------------------------
        if ( _stage != null )
        {
            _tableTest = new TableTest( ref _stage );
            _tableTest.Setup();
        }
        // --------------------------------------

        _font = new BitmapFont( new FileInfo( Assets.DefaultSkinFont ) );

        if ( _inputMultiplexer.Processors.Size > 0 )
        {
            Engine.Input.InputProcessor = _inputMultiplexer;
        }

        Logger.Debug( "Done" );
    }

    /// <inheritdoc />
    public override void Update( float delta )
    {
        _inputTest?.Update();
        _spriteTests?.Update( delta );
        // --------------------------------------

        _mapTests?.ScrollMap( ref _tiledMapCam );

        if ( Engine.Input.IsKeyJustPressed( IInput.Keys.Enter ) )
        {
            Shake.ScreenShakeAllowed = true;
            Shake.Start( _backgroundCam?.Camera );
        }
    }

    /// <inheritdoc />
    public override void Render( float delta )
    {
        // Clear and set viewport
        ScreenUtils.Clear( Color.Blue, true );

        // --------------------------------------

        if ( _spriteBatch != null )
        {
            _spriteBatch.EnableBlending();

            if ( _backgroundCam is { IsInUse: true } )
            {
                _backgroundCam.Viewport?.Apply( true );
                _spriteBatch.SetProjectionMatrix( _backgroundCam.Camera.Combined );
                _spriteBatch.Begin();

                _cameraPos.X = ( _backgroundCam.Camera.ViewportWidth / 2 );
                _cameraPos.Y = ( _backgroundCam.Camera.ViewportHeight / 2 );
                _cameraPos.Z = 0;

                _backgroundCam.SetPosition( _cameraPos, CameraData.NoZoom, true );
                
                if ( _backgroundTexture != null )
                {
                    _spriteBatch.Draw( _backgroundTexture,
                                       _backgroundCam.Camera.Position.X - ( _backgroundTexture.Width / 2f ),
                                       _backgroundCam.Camera.Position.Y - ( _backgroundTexture.Height / 2f ) );
                }

                _spriteBatch.End();
            }
        }

        // --------------------------------------

        if ( _mapTests?.MapRenderer != null && _tiledMapCam is { IsInUse: true } )
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
                    _spriteBatch.Draw( _texture, 200, 200 );
                }

                // TextureRegion
                if ( _textureRegion != null )
                {
                    _spriteBatch.Draw( _textureRegion, 400, 100 );
                }

                // NinePatch
                if ( _ninePatch != null )
                {
                    _ninePatch.Draw( _spriteBatch, 500, 100, 100, 10 );
                }

                _spriteTests?.Draw( _spriteBatch );

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

                _hudCam.SetPosition( Vector3.Zero, CameraData.NoZoom, false );
                _hudCam.Update();

                _inputTest?.Render( _spriteBatch );

                _spriteBatch.End();
            }
        }

        // ----- Draw the Stage, if enabled -----
        if ( _stage != null && _isDrawingStage )
        {
            _stage.Act( Math.Min( Engine.DeltaTime, 1.0f / 60.0f ) );
            _stage.Draw();
        }
    }

    /// <inheritdoc />
    public override void Resize( int width, int height )
    {
        _tiledMapCam?.ResizeViewport( width, height );
        _spriteCam?.ResizeViewport( width, height );

        _stage?.Viewport.Update( width, height );
    }

    private void CreateCameras()
    {
        _backgroundCam = new OrthographicGameCamera( 480, 270 )
        {
            Name    = "BackgroundCamera",
            IsInUse = true,
            Position = new Vector3( 0, 0, CameraData.DefaultZ )
        };
        _backgroundCam.Update();

        // --------------------------------------

        _tiledMapCam = new OrthographicGameCamera( Engine.Graphics.WindowWidth,
                                                   Engine.Graphics.WindowHeight )
        {
            Name    = "TiledMapCamera",
            IsInUse = true,
            Position = new Vector3( 0, 0, CameraData.DefaultZ )
        };
        _tiledMapCam.Update();

        // --------------------------------------

        _spriteCam = new OrthographicGameCamera( Engine.Graphics.WindowWidth,
                                                 Engine.Graphics.WindowHeight )
        {
            Name     = "SpriteCamera",
            IsInUse  = true,
            Position = new Vector3( 0, 0, CameraData.DefaultZ )
        };
        _spriteCam.Update();

        // --------------------------------------

        _hudCam = new OrthographicGameCamera( Engine.Graphics.WindowWidth,
                                              Engine.Graphics.WindowHeight )
        {
            Name     = "HudCamera",
            IsInUse  = true,
            Position = new Vector3( 0, 0, CameraData.DefaultZ )
        };
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
                _spriteTests?.Dispose();
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
//        _texture           = new Texture2D( Assets.Solid112X112 );
//        _textureRegion     = new TextureRegion( new Texture2D( Assets.CompleteStar ) );
//        _ninePatch         = new NinePatch( new Texture2D( Assets.Bar9 ), 1, 1, 1, 1 );
        _backgroundTexture = new Texture2D( Assets.Background );
        
        var scene2DImage = new Scene2DImage( new Texture2D( Assets.HudPanel ) )
        {
            IsVisible = true
        };

        scene2DImage.SetPosition( 0, 0 );
        _stage?.AddActor( scene2DImage );
    }
}

// ============================================================================
// ============================================================================