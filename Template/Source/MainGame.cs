using System.Runtime.Versioning;

using Extensions.Source.Freetype;

using JetBrains.Annotations;

using LughSharp.Core.Assets;
using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Cameras;
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Graphics.OpenGL.Enums;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.Main;
using LughSharp.Core.Maps.Tiled;
using LughSharp.Core.Maps.Tiled.Loaders;
using LughSharp.Core.Maps.Tiled.Renderers;
using LughSharp.Core.Maths;
using LughSharp.Core.Scenes.Scene2D;
using LughSharp.Core.Scenes.Scene2D.UI;
using LughSharp.Core.Scenes.Scene2D.Utils;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Logging;
using LughSharp.Tests.Source;

using Color = LughSharp.Core.Graphics.Color;

namespace Template.Source;

// ============================================================================
// TODO:
//  Freetype Font generation
//  Sprite Scrolling
//  GameSprite helper class
//  UI System ( Scene2D )
//  TiledMap Animated Tiles
//  Audio
//  3D Graphics
//  Network / HTTP etc.
//  Json handling to replace System Json
//  Maybe add a "Content" folder to the project and copy assets there on build, then load from there instead of the "Assets" folder?
//  2D Particle System
//  Ninepatch support for UI
// ============================================================================

/// <summary>
/// TEST class, used for testing the framework.
/// </summary>
[PublicAPI]
public class MainGame : Game
{
    public bool IsDrawingStage { get; set; } = true;

    // ========================================================================

    private readonly Vector3 _cameraPos = Vector3.Zero;

    private SpriteBatch?                _spriteBatch;
    private AssetManager?               _assetManager;
    private OrthographicGameCamera?     _tiledCam;
    private OrthographicGameCamera?     _gameCam;
    private Texture?                    _image1;
    private Texture?                    _image2;
    private TextureRegion?              _star;
    private Stage?                      _stage;
    private Scene2DImage?               _hudActor;
    private Window?                     _windowActor;
    private ImageButton?                _imageButton;
    private BitmapFont?                 _font;
    private Sprite?                     _sprite;
    private Sprite?                     _sprite2;
    private bool                        _disposed;
    private ILughTest?                  _test;
    private TmxMapLoader?               _tmxMapLoader;
    private OrthogonalTiledMapRenderer? _mapRenderer;
    private TiledMap?                   _tiledMap;
    private Vector2                     _spritePosition = Vector2.Zero;
    private float                       _scale          = 1.0f;
    private int                         _direction      = -1;
    private int                         _mapPosX;
    private int                         _mapPosY;
    private int                         _mapDirX = 1;
    private int                         _mapDirY = 1;
    private int                         _mapWidth;
    private int                         _mapHeight;

    // ========================================================================
    // ========================================================================

    /// <inheritdoc />
    [SupportedOSPlatform( "windows" )]
    public override void Create()
    {
        _spriteBatch  = new SpriteBatch();
        _assetManager = new AssetManager();

        CreateCameras();
        CreateAssets();
        CreateMap();

        Logger.Debug( "Done" );
    }

    // ========================================================================

    /// <inheritdoc />
    public override void Update()
    {
//        ScaleSprite();
        ScrollMap();
    }

    /// <inheritdoc />
    public override void Render()
    {
        // Clear and set viewport
        ScreenUtils.Clear( color: Color.Blue, clearDepth: true );

        // --------------------------------------

        if ( _tiledCam is { IsInUse: true } )
        {
            _tiledCam.Update();

            _mapRenderer?.SetView( _tiledCam.Camera );
            _mapRenderer?.Render();
        }

        // --------------------------------------

        if ( _spriteBatch != null )
        {
            _spriteBatch.EnableBlending();

            if ( _gameCam is { IsInUse: true } )
            {
                _gameCam.Viewport?.Apply( centerCamera: true );
                _spriteBatch.SetProjectionMatrix( _gameCam.Camera.Combined );
                _spriteBatch.Begin();

                _gameCam.Position.X = 0;
                _gameCam.Position.Y = 0;
                _gameCam.Position.Z = 0;
                _gameCam.Update();

                if ( _image2 != null )
                {
                    _spriteBatch.Draw( _image2, 0, 0 );
                }

                if ( _star != null )
                {
                    _spriteBatch.Draw( _star, 10, 80 );
                }

                if ( _image1 != null )
                {
                    _spriteBatch.Draw( _image1,
                                       ( Engine.Api.Graphics.Width - _image1.Width ) / 2f,
                                       ( Engine.Api.Graphics.Height - _image1.Height ) / 2f,
                                       _image1.Width,
                                       _image1.Height );
                }

                if ( _sprite != null )
                {
                    _sprite?.SetPosition( 40, 120 );
                    _sprite?.Draw( _spriteBatch );
                }

                if ( _sprite2 != null )
                {
                    _sprite2.SetPosition( 320, 240 );
                    _sprite2.Draw( _spriteBatch );
                }

                _test?.Render( _spriteBatch );

                _font?.Draw( _spriteBatch, "[GREEN]HELLO[] [WHITE]WORLD[]", 400, 400 );

                _spriteBatch.End();
            }
        }

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
        _tiledCam?.ResizeViewport( width, height );
        _gameCam?.ResizeViewport( width, height );

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
//                _star?.Dispose();
                _assetManager?.Dispose();
                _tiledCam?.Dispose();
                _gameCam?.Dispose();
                _font?.Dispose();

                // TODO:
                // Should Sprite() implement IDisposable, or should I leave that up to
                // any extending classes? Maybe imlplement IDisposable in Sprite() and
                // allow ( and encouraging ) extending classes to override it?
                _sprite = null;
                _sprite2  = null;
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
        const float ZOOM = 1f;

        _tiledCam = new OrthographicGameCamera( Engine.Api.Graphics.Width,
                                                Engine.Api.Graphics.Height,
                                                name: "TILEDCamera" );

        _tiledCam.Camera.Near = CameraData.DEFAULT_NEAR_PLANE;
        _tiledCam.Camera.Far  = CameraData.DEFAULT_FAR_PLANE;
        _tiledCam.IsInUse     = true;
        _tiledCam.SetZoomDefault( ZOOM );
        _tiledCam.SetPosition( new Vector3( 0, 0, CameraData.DEFAULT_Z ) );
        _tiledCam.Update();

        // --------------------------------------

        _gameCam = new OrthographicGameCamera( Engine.Api.Graphics.Width,
                                               Engine.Api.Graphics.Height,
                                               name: "HUDCamera" );

        _gameCam.Camera.Near = CameraData.DEFAULT_NEAR_PLANE;
        _gameCam.Camera.Far  = CameraData.DEFAULT_FAR_PLANE;
        _gameCam.IsInUse     = true;
        _gameCam.SetZoomDefault( ZOOM );

        // Set initial camera position
        _gameCam.SetPosition( new Vector3( 0, 0, CameraData.DEFAULT_Z ) );
        _gameCam.Update();
    }

    private void CreateAssets()
    {
        _image1 = new Texture( Assets.COMPLETE_STAR );
        _image2 = new Texture( Assets.COMPLETE_STAR );
        _star   = new TextureRegion( new Texture( Assets.COMPLETE_STAR ) );

        CreateStage();
        CreateFont();
//        CreateFreeTypeFont(); // Not working yet
        CreateSprites();
    }

    private void CreateStage()
    {
        if ( _gameCam == null )
        {
            throw new InvalidOperationException( "HUD camera must be created before creating the stage!" );
        }

        _stage = new Stage( _gameCam.Viewport );
        _hudActor = new Scene2DImage( new Texture( Assets.HUD_PANEL ) )
        {
            IsVisible = true
        };
        _hudActor.SetPosition( 0, 0 );

//        var style = new ImageButton.ImageButtonStyle
//        {
//            ImageUp   = new TextureRegionDrawable( new Texture( Assets.BUTTON_B_UP ) ),
//            ImageDown = new TextureRegionDrawable( new Texture( Assets.BUTTON_B_DOWN ) )
//        };
//        _imageButton = new ImageButton( style )
//        {
//            IsVisible = true
//        };
//        _imageButton.SetPosition( 0, 0 );

//        var windowStyle = new Window.WindowStyle
//        {
//            TitleFont = _font,
//            TitleFontColor = Color.White,
//            Background = new TextureRegionDrawable( new Texture( Assets.WINDOW_BACKGROUND ) ),
//        };
//        _windowActor = new Window( "Window Title", windowStyle )
//        {
//            IsVisible = true,
//        };
//        _windowActor.SetPosition( 200, 180 );
        
        _stage?.AddActor( _hudActor );
//        _stage?.AddActor( _imageButton );
//        _stage?.AddActor( _windowActor );
        _stage?.DebugAll = true;
    }

    private void CreateFont()
    {
        _font = new BitmapFont();
        _font.SetColor( Color.White );
        _font.GetRegion().Texture?.SetFilter( TextureFilterMode.Nearest, TextureFilterMode.Nearest );
        _font.FontData.MarkupEnabled = true;
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

    private void CreateSprites()
    {
        _sprite = new Sprite( new TextureRegion( new Texture( Assets.KEY_COLLECTED ) ) );
        _sprite.SetBounds();
        _sprite.SetOriginCenter();
        _sprite.SetColor( Color.White );
        
        _sprite2  = new Sprite( new TextureRegion( new Texture( Assets.PAUSE_EXIT_BUTTON ) ) );
        _sprite2.SetBounds();
        _sprite2.SetOriginCenter();
        _sprite2.SetColor( Color.White );
    }

    private void CreateMap()
    {
        _tmxMapLoader = new TmxMapLoader();
        _tiledMap     = _tmxMapLoader.Load( Assets.ROOM1_MAP );
        _mapRenderer  = new OrthogonalTiledMapRenderer( _tiledMap );

        _mapWidth  = _tiledMap.Properties.Get< int >( "width" );
        _mapHeight = _tiledMap.Properties.Get< int >( "height" );

        _mapWidth  *= _tiledMap.Properties.Get< int >( "tilewidth" );
        _mapHeight *= _tiledMap.Properties.Get< int >( "tileheight" );

        Logger.Debug( $"Map width: {_mapWidth}, height: {_mapHeight}" );
    }

    private void ScaleSprite()
    {
        if ( _sprite != null )
        {
            if ( _direction == -1 )
            {
                _scale -= 0.01f;

                if ( _scale <= 0.25f )
                {
                    _direction = 1;
                    _scale     = 0.25f;
                }
            }

            if ( _direction == 1 )
            {
                _scale += 0.01f;

                if ( _scale >= 1.0f )
                {
                    _direction = -1;
                    _scale     = 1.0f;
                }
            }

            _sprite?.SetScale( _scale );
            _sprite?.Rotate( -1.0f );
            _sprite?.Scroll( 0.001f, 0.0f );
        }
    }

    private void ScrollMap()
    {
        if ( _tiledCam != null && _tiledMap != null )
        {
            _mapPosX += _mapDirX;

            if ( _mapPosX > _mapWidth && _mapDirX == 1 )
            {
                _mapDirX = -1;
            }
            else if ( _mapPosX <= 0 && _mapDirX == -1 )
            {
                _mapDirX = 1;
            }

            _mapPosY += _mapDirY;

            if ( _mapPosY >= _mapHeight && _mapDirY == 1 )
            {
                _mapDirY = -1;
            }
            else if ( _mapPosY <= 0 && _mapDirY == -1 )
            {
                _mapDirY = 1;
            }

            _tiledCam.Camera.Translate( _mapDirX, _mapDirY );
        }
    }
}

// ============================================================================
// ============================================================================