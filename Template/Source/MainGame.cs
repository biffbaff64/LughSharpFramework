using System.Runtime.Versioning;
using System.Text;

using JetBrains.Annotations;

using LughSharp.Lugh.Assets;
using LughSharp.Lugh.Assets.Loaders;
using LughSharp.Lugh.Core;
using LughSharp.Lugh.Files;
using LughSharp.Lugh.Graphics;
using LughSharp.Lugh.Graphics.Cameras;
using LughSharp.Lugh.Graphics.G2D;
using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Graphics.OpenGL.Enums;
using LughSharp.Lugh.Graphics.Utils;
using LughUtils.source.Maths;
using LughSharp.Lugh.Utils;

using LughUtils.source;
using LughUtils.source.Exceptions;
using LughUtils.source.Logging;
using LughSharp.Tests.Source;

using NUnit.Framework.Api;

using Color = LughSharp.Lugh.Graphics.Color;

namespace Template.Source;

/// <summary>
/// TEST class, used for testing the framework.
/// </summary>
[PublicAPI]
public partial class MainGame : Game
{
    private const string TEST_ASSET1 = Assets.ROVER_WHEEL;
    private const int    TEST_WIDTH  = 100;
    private const int    TEST_HEIGHT = 100;

    // ========================================================================

    private readonly Vector3 _cameraPos = Vector3.Zero;

    private OrthographicGameCamera? _orthoGameCam;
    private SpriteBatch?            _spriteBatch;
    private AssetManager?           _assetManager;
    private Texture?                _image1;
    private Texture?                _whitePixelTexture;
    private bool                    _disposed = false;
    
    // ========================================================================
    // ========================================================================

    /// <inheritdoc />
    [SupportedOSPlatform( "windows" )]
    public override void Create()
    {
        _spriteBatch  = new SpriteBatch();
        _assetManager = new AssetManager();

        _image1            = null;
        _whitePixelTexture = null;

        CreateCamera();

        RunTests();

        Logger.Debug( "Done" );
    }

    // ========================================================================

    /// <inheritdoc />
    public override void Update()
    {
        UpdateTests();
    }

    /// <inheritdoc />
    public override void Render()
    {
        // Clear and set viewport
        ScreenUtils.Clear( Color.Blue );

        if ( _orthoGameCam is { IsInUse: true } )
        {
            _spriteBatch?.Begin();
            _spriteBatch?.SetProjectionMatrix( _orthoGameCam.Camera.Combined );

            _orthoGameCam.Viewport?.Apply();
            _orthoGameCam.Update();

            if ( _image1 != null )
            {
                _spriteBatch?.Draw( _image1, 0, 0 );
            }
            
            _spriteBatch?.End();
        }
    }

    /// <inheritdoc />
    public override void Resize( int width, int height )
    {
        _orthoGameCam?.ResizeViewport( width, height );
    }

    // ========================================================================
    
    /// <inheritdoc />
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
            _image1 = _assetManager.GetAs< Texture >( TEST_ASSET1 );
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