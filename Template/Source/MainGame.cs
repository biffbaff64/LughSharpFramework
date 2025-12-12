using System;
using System.Runtime.Versioning;
using System.Text;
using Extensions.Source.Tools;
using JetBrains.Annotations;
using LughSharp.Core.Assets;
using LughSharp.Core.Assets.Loaders;
using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Cameras;
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Main;
using LughSharp.Core.Utils;
using LughSharp.Core.Graphics;
using LughUtils.source.Maths;
using LughUtils.source.Exceptions;
using LughUtils.source.Logging;
using Color = LughSharp.Core.Graphics.Color;

namespace Template.Source;

/// <summary>
/// TEST class, used for testing the framework.
/// </summary>
[PublicAPI]
public partial class MainGame : Game
{
    private const string TEST_ASSET1 = Assets.BACKGROUND_IMAGE;
    private const int    TEST_WIDTH  = 64;
    private const int    TEST_HEIGHT = 64;

    // ========================================================================

    private readonly Vector3 _cameraPos = Vector3.Zero;

    private OrthographicGameCamera? _orthoGameCam;
    private SpriteBatch?            _spriteBatch;
    private AssetManager?           _assetManager;
    private Texture?                _image1;
    private Texture?                _whitePixelTexture;
    private bool                    _disposed;

    // ========================================================================
    // ========================================================================

    /// <inheritdoc />
    [SupportedOSPlatform( "windows" )]
    public override void Create()
    {
        _spriteBatch  = new SpriteBatch();
        _assetManager = new AssetManager();

        CreateCamera();
        
        var pixmap = new Pixmap( TEST_WIDTH, TEST_HEIGHT, LughFormat.RGBA8888 );
        pixmap.SetColor( Color.White );
        pixmap.FillWithCurrentColor();
        Logger.Debug( $"pixmap width: {pixmap.Width}, height: {pixmap.Height}" );
        Logger.Debug( $"Bytes per Pixel: {pixmap.Gdx2DPixmap.BytesPerPixel}" );
        Logger.Debug( $"pixmap ByteBuffer length: {pixmap.ByteBuffer.Length}" );
        
        Logger.Block();

        _image1 = new Texture( Assets.COMPLETE_STAR );
        _image1.DebugPrint();
        
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
        ScreenUtils.Clear( Color.Blue );

        if ( _orthoGameCam is { IsInUse: true } )
        {
            _orthoGameCam.Viewport?.Apply();
            _spriteBatch?.SetProjectionMatrix( _orthoGameCam.Camera.Combined );
            _spriteBatch?.Begin();

            if ( _image1 != null )
            {
                _spriteBatch?.Draw( _image1, 0, 0 );
            }

            _spriteBatch?.End();
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
                _whitePixelTexture?.Dispose();
                _assetManager?.Dispose();
                _orthoGameCam?.Dispose();
            }

            _disposed = true;
        }
    }

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    private void CreateCamera()
    {
        _orthoGameCam = new OrthographicGameCamera( Engine.Api.Graphics.Width,
                                                    Engine.Api.Graphics.Height,
                                                    ppm: 1f );
        _orthoGameCam.Camera.Near = 1.0f;
        _orthoGameCam.Camera.Far  = 100.0f;
        _orthoGameCam.IsInUse     = true;
        _orthoGameCam.SetZoomDefault( CameraData.DEFAULT_ZOOM );

        // Set initial camera position
        _cameraPos.X = 0f;
        _cameraPos.Y = 0f;
        _cameraPos.Z = 0f;
        _orthoGameCam.SetPosition( _cameraPos );
        _orthoGameCam.Update();
    }

    private void LoadAssets()
    {
        GdxRuntimeException.ThrowIfNull( _assetManager );

        Logger.Divider();
        Logger.Debug( "Loading assets...", true );
        Logger.Divider();

        _assetManager.Load< Texture >( TEST_ASSET1, new TextureLoader.TextureLoaderParameters() );
        _assetManager.FinishLoading();

        if ( _assetManager.Contains( TEST_ASSET1 ) )
        {
            _image1 = _assetManager.Get< Texture >( TEST_ASSET1 );
        }

        if ( _image1 == null )
        {
            Logger.Debug( "Asset loading failed" );
        }
        else
        {
            Logger.Debug( "Asset loaded" );

            #if DEBUG
            Logger.Debug( $"Loaded image type: {_image1.GetType()}" );

            var data = _image1.GetImageData();
            var sb   = new StringBuilder();

            if ( data != null )
            {
                sb.AppendLine();

                for ( var i = 0; i < 20; i++ )
                {
                    for ( var j = 0; j < 20; j++ )
                    {
                        sb.Append( $"[{data[ ( i * 20 ) + j ]:X}]" );
                    }

                    sb.AppendLine();
                }

                Logger.Debug( sb.ToString() );
            }
            #endif
        }
    }
}

// ============================================================================
// ============================================================================