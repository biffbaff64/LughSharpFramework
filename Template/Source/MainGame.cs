using System;
using System.Runtime.Versioning;

using Extensions.Source.Drawing.Freetype;

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

    private SpriteBatch?                _spriteBatch1;
    private SpriteBatch?                _spriteBatch2;
    private AssetManager?               _assetManager;
    private OrthographicGameCamera?     _tiledCam;
    private OrthographicGameCamera?     _gameCam;
    private Texture?                    _image1;
    private Texture?                    _star;
    private Stage?                      _stage;
    private Actor?                      _hudActor;
    private BitmapFont?                 _font;
    private Sprite?                     _sprite;
    private Sprite?                     _star2;
    private bool                        _disposed;
    private ILughTest?                  _test;
    private TmxMapLoader?               _tmxMapLoader;
    private OrthogonalTiledMapRenderer? _mapRenderer;
    private TiledMap?                   _tiledMap;
    private Vector2                     _spritePosition = Vector2.Zero;
    private float                       _scale          = 1.0f;
    private int                         _direction      = -1;
    private List< TiledMapTileLayer >   _tileLayers     = [ ];
    private Vector2                     _mapPos         = Vector2.Zero;
    private Vector2                     _mapDir         = Vector2.One;

    // ========================================================================
    // ========================================================================

    /// <inheritdoc />
    [SupportedOSPlatform( "windows" )]
    public override void Create()
    {
        _spriteBatch1 = new SpriteBatch();
        _spriteBatch2 = new SpriteBatch();
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

//            _sprite?.Rotate( -1.0f );
//            _sprite?.SetScale( 0.5f );
//        _sprite?.Scroll( 0.001f, 0.0f );
        }

//        if ( _tiledMap != null )
//        {
//            _mapPos.X += _mapDir.X;
//            
//            if ( _mapPos.X > _tiledMap.Properties.Get< int >( "width" )
//                || _mapPos.X <= 0 )
//            {
//                _mapDir.X *= -1;
//            }
//            
//            _mapPos.Y += _mapDir.Y;
//            
//            if ( _mapPos.Y >= _tiledMap.Properties.Get< int >( "height" )
//                || _mapPos.Y <= 0 )
//            {
//                _mapDir.Y *= -1;
//            }
//        }
    }

    /// <inheritdoc />
    public override void Render()
    {
        // Clear and set viewport
        ScreenUtils.Clear( color: Color.Blue, clearDepth: true );

        if ( _spriteBatch1 != null )
        {
            _spriteBatch1.EnableBlending();

            if ( _tiledCam is { IsInUse: true } )
            {
                _tiledCam.Viewport?.Apply( centerCamera: true );
                _tiledCam.Position.X = _mapPos.X;
                _tiledCam.Position.Y = _mapPos.Y;
                _tiledCam.Update();

                _mapRenderer?.SetView( _tiledCam.Camera );
                _mapRenderer?.Render();
            }
        }

        if ( _spriteBatch2 != null )
        {
            _spriteBatch2.EnableBlending();

            if ( _gameCam is { IsInUse: true } )
            {
                _gameCam.Viewport?.Apply( centerCamera: true );
                _spriteBatch2.SetProjectionMatrix( _gameCam.Camera.Combined );
                _spriteBatch2.Begin();

                _gameCam.Position.X = 0;
                _gameCam.Position.Y = 0;
                _gameCam.Position.Z = 0;
                _gameCam.Update();

                if ( _star != null )
                {
                    _spriteBatch2.Draw( _star, 0, 0 );
                }

                if ( _image1 != null )
                {
                    _spriteBatch2.Draw( _image1,
                                        ( Engine.Api.Graphics.Width - _image1.Width ) / 2f,
                                        ( Engine.Api.Graphics.Height - _image1.Height ) / 2f );
                }

                if ( _sprite != null )
                {
                    _sprite?.SetPosition( _spritePosition.X, _spritePosition.Y );
                    _sprite?.Draw( _spriteBatch2 );
                }

                if ( _star2 != null )
                {
                    _star2.SetPosition( 320, 240 );
                    _star2.Draw( _spriteBatch2 );
                }

                if ( _hudActor is { UserObject: Texture } )
                {
                    _spriteBatch2.Draw( ( Texture )_hudActor.UserObject, 0, 0 );
                }

                _test?.Render( _spriteBatch2 );

                _font?.Draw( _spriteBatch2, "[GREEN]HELLO[] [WHITE]WORLD[]", 400, 400 );

                _spriteBatch2.End();
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
                _spriteBatch1?.Dispose();
                _spriteBatch2?.Dispose();
                _image1?.Dispose();
                _star?.Dispose();
                _assetManager?.Dispose();
                _tiledCam?.Dispose();
                _gameCam?.Dispose();
                _font?.Dispose();

                // TODO:
                // Should Sprite() implement IDisposable, or should I leave that up to
                // any extending classes? Maybe imlplement IDisposable in Sprite() and
                // allow ( and encouraging ) extending classes to override it?
                _sprite = null;
                _star2  = null;
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
        Logger.Checkpoint();

        var zoom = 1f;

        _tiledCam = new OrthographicGameCamera( Engine.Api.Graphics.Width,
                                                Engine.Api.Graphics.Height,
                                                name: "MainCamera" );

        _tiledCam.Camera.Near = CameraData.DEFAULT_NEAR_PLANE;
        _tiledCam.Camera.Far  = CameraData.DEFAULT_FAR_PLANE;
        _tiledCam.IsInUse     = true;
        _tiledCam.SetZoomDefault( zoom );

        // Set initial camera position
        _tiledCam.SetPosition( new Vector3( 0, 0, CameraData.DEFAULT_Z ) );
        _tiledCam.Update();

        // --------------------------------------

        _gameCam = new OrthographicGameCamera( Engine.Api.Graphics.Width,
                                               Engine.Api.Graphics.Height,
                                               name: "HUDCamera" );

        _gameCam.Camera.Near = CameraData.DEFAULT_NEAR_PLANE;
        _gameCam.Camera.Far  = CameraData.DEFAULT_FAR_PLANE;
        _gameCam.IsInUse     = true;
        _gameCam.SetZoomDefault( zoom );

        // Set initial camera position
        _gameCam.SetPosition( new Vector3( 0, 0, CameraData.DEFAULT_Z ) );
        _gameCam.Update();
    }

    private void CreateAssets()
    {
        Logger.Checkpoint();

        _image1 = new Texture( Assets.COMPLETE_STAR );
        _star   = new Texture( Assets.COMPLETE_STAR );

        CreateStage();
        CreateFont();
//        CreateFreeTypeFont(); // Not working yet
        CreateSprite();
    }

    private void CreateStage()
    {
        Logger.Checkpoint();

        if ( _gameCam == null )
        {
            throw new InvalidOperationException( "HUD camera must be created before creating the stage!" );
        }

        _stage = new Stage( _gameCam.Viewport, _spriteBatch2 );

        _hudActor           = new Scene2DImage( new Texture( Assets.HUD_PANEL ) );
        _hudActor.IsVisible = true;
        _hudActor.SetPosition( 100, 100 );

        _stage?.AddActor( _hudActor );
        _stage?.DebugAll = true;
    }

    private void CreateFont()
    {
        Logger.Checkpoint();

        _font = new BitmapFont();
        _font.SetColor( Color.White );
        _font.GetRegion().Texture?.SetFilter( TextureFilterMode.Nearest, TextureFilterMode.Nearest );
        _font.FontData.MarkupEnabled = true;
    }

    private void CreateFreeTypeFont()
    {
        Logger.Checkpoint();

        var generator = new FreeTypeFontGenerator( Engine.Api.Files.Internal( Assets.AMBLE_REGULAR_26_FONT ) );
        var parameter = new FreeTypeFontGenerator.FreeTypeFontParameter
        {
            Size = 40,
        };

        _font = generator.GenerateFont( parameter );
        _font.SetColor( Color.White );
    }

    private void CreateSprite()
    {
        Logger.Checkpoint();

        _sprite = new Sprite( new TextureRegion( new Texture( Assets.KEY_COLLECTED ) ) );
        _star2  = new Sprite( new TextureRegion( new Texture( Assets.COMPLETE_STAR ) ) );

        _spritePosition.Set( 40, 120 );
//        _sprite.SetPosition( _spritePosition.X, _spritePosition.Y );
//        _sprite.SetBounds();
//        _sprite.SetOriginCenter();
//        _sprite.SetColor( Color.White );
    }

    private void CreateMap()
    {
        Logger.Checkpoint();

        _tmxMapLoader = new TmxMapLoader();
        _tiledMap     = _tmxMapLoader.Load( Assets.ROOM1_MAP );
        _mapRenderer  = new OrthogonalTiledMapRenderer( _tiledMap );
        _tileLayers   = _tiledMap.Layers.OfType< TiledMapTileLayer >().ToList();
    }
}

// ============================================================================
// ============================================================================