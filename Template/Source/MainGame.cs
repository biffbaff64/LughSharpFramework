using System;
using System.IO;
using System.Runtime.Versioning;

using Extensions.Source.Freetype;

using JetBrains.Annotations;

using LughSharp.Core.Assets;
using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Atlases;
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
using LughSharp.Core.Scenes.Scene2D.Styles;
using LughSharp.Core.Scenes.Scene2D.UI;
using LughSharp.Core.Scenes.Scene2D.Utils;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Logging;
using LughSharp.Tests.Source;

using Newtonsoft.Json.Linq;

using ButtonStyle = LughSharp.Core.Scenes.Scene2D.UI.ButtonStyle;
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
    private OrthographicGameCamera?     _tiledMapCam;
    private OrthographicGameCamera?     _spriteCam;
    private Texture?                    _image1;
    private Texture?                    _image2;
    private TextureRegion?              _star;
    private Stage?                      _stage;
    private Scene2DImage?               _hudActor;
    private Window?                     _windowActor;
    private ImageButton?                _imageButton;
    private Button?                     _button;
    private ProgressBar?                _progressBar;
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
        // Do not create a new SpriteBatch before this point, as OpenGL will not be ready.
        _spriteBatch  = new SpriteBatch();
        _assetManager = new AssetManager();

        CreateCameras();
        CreateAssets();
        CreateStage();
        CreateFont();
        CreateFreeTypeFont();
        CreateSprites();
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
        ScreenUtils.Clear( Color.Blue, true );

        // --------------------------------------

        if ( _mapRenderer != null && _tiledMapCam is { IsInUse: true } )
        {
            _tiledMapCam.Update();
            
            _mapRenderer.SetView( _tiledMapCam.Camera );
            _mapRenderer.Render();
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

                _spriteBatch.End();
            }
        }

        // ----- Draw the Stage, if enabled -----
        if ( _stage != null && IsDrawingStage )
        {
            _stage.Act( Math.Min( Engine.Api.DeltaTime, 1.0f / 60.0f ) );
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
        _tiledMapCam?.ResizeViewport( width, height );
        _spriteCam?.ResizeViewport( width, height );

        _stage?.Viewport.Update( width, height );
    }

    // ========================================================================

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or
    /// resetting unmanaged resources.
    /// </summary>
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
                _image1?.Dispose();
                _image2?.Dispose();
                _stage?.Dispose();
                _font?.Dispose();

                // TODO:
                // Should Sprite() implement IDisposable, or should I leave that up to
                // any extending classes? Maybe implement IDisposable in Sprite() and
                // allow ( and encourage ) extending classes to override it?
                _sprite  = null;
                _sprite2 = null;
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
        const float Zoom = 1f;

        _tiledMapCam = new OrthographicGameCamera( Engine.Api.Graphics.WindowWidth,
                                                   Engine.Api.Graphics.WindowHeight,
                                                   name: "TILEDCamera" );

        _tiledMapCam.Camera.Near = CameraData.DefaultNearPlane;
        _tiledMapCam.Camera.Far  = CameraData.DefaultFarPlane;
        _tiledMapCam.IsInUse     = true;
        _tiledMapCam.SetZoomDefault( Zoom );
        _tiledMapCam.SetPosition( new Vector3( 0, 0, CameraData.DefaultZ ) );
        _tiledMapCam.Update();

        // --------------------------------------

        _spriteCam = new OrthographicGameCamera( Engine.Api.Graphics.WindowWidth,
                                                 Engine.Api.Graphics.WindowHeight,
                                                 name: "HUDCamera" );

        _spriteCam.Camera.Near = CameraData.DefaultNearPlane;
        _spriteCam.Camera.Far  = CameraData.DefaultFarPlane;
        _spriteCam.IsInUse     = true;
        _spriteCam.SetZoomDefault( Zoom );

        // Set initial camera position
        _spriteCam.SetPosition( new Vector3( 0, 0, CameraData.DefaultZ ) );
        _spriteCam.Update();
    }

    private void CreateAssets()
    {
        _image1 = new Texture( Assets.CompleteStar );
        _image2 = new Texture( Assets.CompleteStar );
        _star   = new TextureRegion( new Texture( Assets.CompleteStar ) );
    }

    private void CreateStage()
    {
        if ( _spriteCam == null )
        {
            throw new InvalidOperationException( "HUD camera must be created before creating the stage!" );
        }

        var skin = new Skin( new FileInfo( Assets.ProgressBarSkin ) );
        _stage = new Stage( _spriteCam.Viewport );

        // --------------------------------------
        
//        _hudActor = new Scene2DImage( new Texture( Assets.HudPanel ) )
//        {
//            IsVisible = true
//        };
//        _hudActor.SetPosition( 0, 0 );
//        _stage?.AddActor( _hudActor );

        // --------------------------------------

//        var windowStyle = new WindowStyle
//        {
//            TitleFont      = _font,
//            TitleFontColor = Color.White,
//            Background     = new TextureRegionDrawable( new Texture( Assets.WindowBackground ) )
//        };
//        _windowActor = new Window( "Window Title", windowStyle )
//        {
//            IsVisible = true,
//        };
//        _windowActor.SetPosition( 200, 180 );
//        _stage?.AddActor( _windowActor );

        // --------------------------------------
        
        var button = new Button( skin, "default" );
        button.SetPosition( 20, 200 );
        _stage?.AddActor( button );
        
        // --------------------------------------

        var imageButton = new ImageButton( skin, "default" )
        {
            IsVisible = true
        };
        imageButton.SetPosition( 0, 0 );
        _stage?.AddActor( imageButton );
        
        // --------------------------------------

//        var atlas         = new TextureAtlas( new FileInfo( Assets.ProgressBarSkinAtlas ) );
//        var styleRegistry = new StyleRegistry();
//        styleRegistry.CreateStyleDefaults( atlas );
//        var style = styleRegistry.Get< ProgressBar.ProgressBarStyle >( "ProgressBarStyle" );
//        _progressBar = new ProgressBar( 0f, 10f, 1f, false, style );
//        _stage?.AddActor( _progressBar );

        // --------------------------------------

        var progressBar = new ProgressBar( 0f, 10f, 1f, false, skin );
        progressBar.SetPosition( 20, 550 );
        _stage?.AddActor( progressBar );
        
        // --------------------------------------

        var checkBox = new CheckBox( "default", skin );
        checkBox.SetPosition( 400, 400 );
        _stage?.AddActor( checkBox );
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
//        var generator = new FreeTypeFontGenerator( Engine.Api.Files.Internal( Assets.AmbleRegular26Font ) );
//        var parameter = new FreeTypeFontGenerator.FreeTypeFontParameter
//        {
//            Size = 40
//        };
//
//        _font = generator.GenerateFont( parameter );
//        _font.SetColor( Color.White );
    }

    private void CreateSprites()
    {
        _sprite = new Sprite( new TextureRegion( new Texture( Assets.KeyCollected ) ) );
        _sprite.SetBounds();
        _sprite.SetOriginCenter();
        _sprite.SetColor( Color.White );

        _sprite2 = new Sprite( new TextureRegion( new Texture( Assets.PauseExitButton ) ) );
        _sprite2.SetBounds();
        _sprite2.SetOriginCenter();
        _sprite2.SetColor( Color.White );
    }

    private void CreateMap()
    {
//        _tmxMapLoader = new TmxMapLoader();
//        _tiledMap     = _tmxMapLoader.Load( Assets.Room1Map );
//        _mapRenderer  = new OrthogonalTiledMapRenderer( _tiledMap );

//        _mapWidth  = _tiledMap.Properties.Get< int >( "width" );
//        _mapWidth  *= _tiledMap.Properties.Get< int >( "tilewidth" );

//        _mapHeight = _tiledMap.Properties.Get< int >( "height" );
//        _mapHeight *= _tiledMap.Properties.Get< int >( "tileheight" );
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
        if ( _tiledMapCam != null && _tiledMap != null )
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

            _tiledMapCam.Camera.Translate( _mapDirX, _mapDirY );
        }
    }
}

// ============================================================================
// ============================================================================