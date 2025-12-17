using System;
using System.Runtime.Versioning;
using JetBrains.Annotations;
using LughSharp.Core.Assets;
using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Cameras;
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Main;
using LughSharp.Core.Scenes.Scene2D;
using LughSharp.Core.Scenes.Scene2D.UI;
using LughSharp.Core.Utils;
using LughUtils.source.Maths;
using LughUtils.source.Logging;
using Color = LughSharp.Core.Graphics.Color;

namespace Template.Source;

/// <summary>
/// TEST class, used for testing the framework.
/// </summary>
[PublicAPI]
public class MainGame : Game
{
    private const string TEST_ASSET1 = Assets.BACKGROUND_IMAGE;
    private const int    TEST_WIDTH  = 64;
    private const int    TEST_HEIGHT = 64;

    // ========================================================================

    private readonly Vector3 _cameraPos = Vector3.Zero;

    private OrthographicGameCamera? _orthoGameCam;
    private OrthographicGameCamera? _hudCam;
    private SpriteBatch?            _spriteBatch;
    private AssetManager?           _assetManager;
    private Texture?                _image1;
    private Texture?                _hudImage;
    private bool                    _disposed;
    private Stage?                  _stage;

    // ========================================================================
    // ========================================================================

    /// <inheritdoc />
    [SupportedOSPlatform( "windows" )]
    public override void Create()
    {
        _spriteBatch  = new SpriteBatch();
        _assetManager = new AssetManager();

        CreateCameras();

        _stage    = new Stage( _hudCam?.Viewport, _spriteBatch );

        Logger.Debug( "Done" );
    }

    // ========================================================================

    /// <inheritdoc />
    public override void Update()
    {
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
                _spriteBatch?.Draw( _image1, 0, 0 );
            }

            _spriteBatch?.End();
        }

        if ( _hudCam is { IsInUse: true } )
        {
            _hudCam.Viewport?.Apply( centerCamera: true );
            _spriteBatch?.SetProjectionMatrix( _hudCam.Camera.Combined );
            _spriteBatch?.Begin();

            if ( _hudImage != null )
            {
                _spriteBatch?.Draw( _hudImage, 0, 0 );
            }

            _spriteBatch?.End();
        }
        
        // ----- Draw the Stage, if enabled -----
        if ( _stage != null )
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
    }

    // ========================================================================

    /// <summary>
    /// Called when the <see cref="IApplication"/> is destroyed. Preceded by a
    /// call to <see cref="Game.Pause"/>.
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
                _hudImage?.Dispose();
                _assetManager?.Dispose();
                _orthoGameCam?.Dispose();
                _hudCam?.Dispose();
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
        _orthoGameCam.SetZoomDefault( CameraData.DEFAULT_ZOOM );

        // Set initial camera position
        _orthoGameCam.SetPosition( Vector3.Zero );
        _orthoGameCam.Update();

        // --------------------------------------

        _hudCam = new OrthographicGameCamera( Engine.Api.Graphics.Width,
                                              Engine.Api.Graphics.Height,
                                              name: "HUDCamera" );

        _hudCam.SetZoomDefault( CameraData.DEFAULT_ZOOM );

        _hudCam.Camera.Near = CameraData.DEFAULT_NEAR_PLANE;
        _hudCam.Camera.Far  = CameraData.DEFAULT_FAR_PLANE;
        _hudCam.IsInUse     = true;

        // Set initial camera position
        _hudCam.SetPosition( Vector3.Zero );
        _hudCam.Update();
    }

    private Actor? _hudActor;
    
    private void CreateAssets()
    {
        _image1   = new Texture( Assets.BACKGROUND_IMAGE );
        _hudImage = new Texture( Assets.COMPLETE_STAR );

        var hudScene = new Texture( Assets.HUD_PANEL );
        _hudActor = new Image( hudScene );
        
        _stage?.AddActor( _hudActor );
    }
}

// ============================================================================
// ============================================================================