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
using LughSharp.Lugh.Maths;
using LughSharp.Lugh.Utils;

using LughUtils.source;
using LughUtils.source.Exceptions;
using LughUtils.source.Logging;
using LughSharp.Tests.Source;

using Color = LughSharp.Lugh.Graphics.Color;

namespace Template.Source;

/// <summary>
/// TEST class, used for testing the framework.
/// </summary>
[PublicAPI]
public class MainGame : Game
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

        _assetManager.TestSetup();
        _assetManager.TestRun();
        
//        Logger.Divider();
//        Logger.Divider();
//        var test = new AssetManagerTest();
//        var test = new TexturePackerTest();
//        var test = new PNGLoadAndExamineTest();
//        test.Setup();
//        test.Run();
//        test.TearDown();
//        Logger.Divider();
//        Logger.Divider();

//        var imagePath = $"{IOUtils.AssetsRoot}packedimages/objects/rover_wheel.png";
//        _assetManager.Load< Texture >( imagePath );
//        _assetManager.FinishLoading();
//        var image = _assetManager.Get< Texture >( imagePath );
//        PNGDecoder.AnalysePNG( image, true );
//        var data  = image?.GetAsPNG();
//        if ( data != null )
//        {
//            PNGDecoder.AnalyseAndWritePNG( $"{IOUtils.AssetsRoot}newimage.png", data, true );
//        }

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

    /// <inheritdoc />
    public override void Dispose()
    {
        Logger.Checkpoint();

        Dispose( true );
        GC.SuppressFinalize( this );
    }

    protected override void Dispose( bool disposing )
    {
        if ( disposing )
        {
            _spriteBatch?.Dispose();
            _image1?.Dispose();
            _whitePixelTexture?.Dispose();
            _assetManager?.Dispose();
            _orthoGameCam?.Dispose();
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

    private static void CheckViewportCoverage()
    {
        var viewport = new int[ 4 ];

        Engine.GL.GetIntegerv( IGL.GL_VIEWPORT, ref viewport );

        var windowWidth  = Engine.Api.Graphics.Width;
        var windowHeight = Engine.Api.Graphics.Height;

        var isFullyCovered = ( viewport[ 0 ] == 0 )                // Left edge at 0
                             && ( viewport[ 1 ] == 0 )             // Bottom edge at 0
                             && ( viewport[ 2 ] == windowWidth )   // Width matches
                             && ( viewport[ 3 ] == windowHeight ); // Height matches

        if ( !isFullyCovered )
        {
            Logger.Debug( "WARNING: Viewport doesn't cover entire window!" );
            Logger.Debug( $"Window: {windowWidth}x{windowHeight}" );
            Logger.Debug( $"Viewport: {viewport[ 2 ]}x{viewport[ 3 ]} at ({viewport[ 0 ]},{viewport[ 1 ]})" );

            if ( ( viewport[ 0 ] != 0 ) || ( viewport[ 1 ] != 0 ) )
            {
                Logger.Debug( "Viewport is offset from window origin!" );
            }

            if ( ( viewport[ 2 ] != windowWidth ) || ( viewport[ 3 ] != windowHeight ) )
            {
                Logger.Debug( "Viewport size doesn't match window size!" );
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void CreateImage1Texture()
    {
        var pixmap = new Pixmap( TEST_WIDTH, TEST_HEIGHT, Pixmap.Format.RGBA8888 );

        _image1 = new Texture( new PixmapTextureData( pixmap,
                                                      Pixmap.Format.RGBA8888,
                                                      false,
                                                      false,
                                                      true ) );
        _image1.Name = "TestImage";

        pixmap.Dispose();

        // Validate texture creation
        if ( !Engine.GL.IsGLTexture( _image1.TextureID ) )
        {
            Logger.Debug( "Failed to create texture" );
        }
    }

    private void LoadImage1Texture()
    {
        Logger.Checkpoint();
        
        try
        {
            var filename = $"{IOUtils.AssetsRoot}title_background.png";

            _image1 = new Texture( new FileInfo( filename ) );

            if ( _image1 == null )
            {
                Logger.Debug( "Failed to create texture object." );

                return;
            }

            Logger.Debug( $"Texture loaded - Width: {_image1.Width}, Height: {_image1.Height}" );
            Logger.Debug( $"Format: {PixelFormatUtils.GetFormatString( _image1.TextureData.GetPixelFormat() )}" );
            Logger.Debug( $"Length: {_image1.GetImageData()?.Length}" );
            
            _image1.Upload();
            _image1.Bind( 0 ); // Set active texture and bind to texture unit 0

            Logger.Debug( "Texture uploaded to GPU and bound to texture unit 0" );
            
            var width  = new int[ 1 ];
            var height = new int[ 1 ];

            Engine.GL.GetTexLevelParameteriv( IGL.GL_TEXTURE_2D, 0, IGL.GL_TEXTURE_WIDTH, ref width );
            Engine.GL.GetTexLevelParameteriv( IGL.GL_TEXTURE_2D, 0, IGL.GL_TEXTURE_HEIGHT, ref height );

            Logger.Debug( $"Initial texture dimensions in GPU: {width[ 0 ]}x{height[ 0 ]}" );

            Engine.GL.TexParameteri( ( int )TextureTarget.Texture2D,
                                     ( int )TextureParameter.MinFilter,
                                     ( int )TextureFilterMode.Nearest );
            Engine.GL.TexParameteri( ( int )TextureTarget.Texture2D,
                                     ( int )TextureParameter.MagFilter,
                                     ( int )TextureFilterMode.Nearest );

            Logger.Debug( "TextureMinFilter set to GL_NEAREST" );
            Logger.Debug( "TextureMagFilter set to GL_NEAREST" );

            PNGDecoder.AnalysePNG( filename, true );
        }
        catch ( Exception ex )
        {
            Logger.Error( $"Exception while loading texture: {ex.Message}" );
            _image1?.Dispose();
            _image1 = null;
        }
    }

    private void CreateWhitePixelTexture()
    {
        Logger.Checkpoint();

        if ( _whitePixelTexture != null )
        {
            return;
        }

        var pixmap = new Pixmap( 100, 100, Pixmap.Format.RGBA8888 );
        pixmap.SetColor( Color.White );
        pixmap.FillWithCurrentColor();

        var textureData = new PixmapTextureData( pixmap, Pixmap.Format.RGBA8888, false, false );

        _whitePixelTexture      = new Texture( textureData );
        _whitePixelTexture.Name = "WhitePixel";

        if ( _whitePixelTexture != null )
        {
            // Set texture parameters
            Engine.GL.BindTexture( IGL.GL_TEXTURE_2D, _whitePixelTexture.TextureID );
            Engine.GL.TexParameteri( IGL.GL_TEXTURE_2D, IGL.GL_TEXTURE_MIN_FILTER, IGL.GL_NEAREST );
            Engine.GL.TexParameteri( IGL.GL_TEXTURE_2D, IGL.GL_TEXTURE_MAG_FILTER, IGL.GL_NEAREST );
            Engine.GL.TexParameteri( IGL.GL_TEXTURE_2D, IGL.GL_TEXTURE_WRAP_S, IGL.GL_CLAMP_TO_EDGE );
            Engine.GL.TexParameteri( IGL.GL_TEXTURE_2D, IGL.GL_TEXTURE_WRAP_T, IGL.GL_CLAMP_TO_EDGE );

            // Validate texture creation
            if ( !Engine.GL.IsGLTexture( _whitePixelTexture.TextureID ) )
            {
                throw new GdxRuntimeException( "Failed to create texture" );
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="thickness"></param>
    public void DrawViewportBounds( float thickness = 2f )
    {
        if ( _whitePixelTexture == null )
        {
            Logger.Debug( "white pixel texture not initialized" );

            return;
        }

        // Get and verify viewport dimensions
        var viewport = new int[ 4 ];
        Engine.GL.GetIntegerv( ( int )GLParameter.Viewport, ref viewport );

        Logger.Debug( $"Viewport dimensions: {viewport[ 2 ]}x{viewport[ 3 ]}" );

        var width  = viewport[ 2 ];
        var height = viewport[ 3 ];

        if ( ( width <= 0 ) || ( height <= 0 ) )
        {
            Logger.Debug( $"Invalid viewport dimensions: {width}x{height}" );

            return;
        }

        try
        {
            if ( _whitePixelTexture != null )
            {
                _spriteBatch?.Draw( _whitePixelTexture, width / 2f, height / 2f );
                Engine.Api.GLErrorCheck( "MainGame::DrawViewportBounds" );
            }
        }
        catch ( Exception ex )
        {
            Logger.Debug( $"Error during drawing: {ex.Message}" );
        }
    }

    // ========================================================================

//    private void DebugViewportState()
//    {
//        var viewport = new int[ 4 ];
//        Engine.GL.GetIntegerv( ( int )GLParameter.Viewport, ref viewport );
//
//        Logger.Debug( $"Viewport: X={viewport[ 0 ]}, Y={viewport[ 1 ]}, Width={viewport[ 2 ]}, Height={viewport[ 3 ]}" );
//
//        // Check scissors test
//        var scissorEnabled = Engine.GL.IsEnabled( ( int )EnableCap.ScissorTest );
//        Logger.Debug( $"Scissor Test Enabled: {scissorEnabled}" );
//
//        if ( scissorEnabled )
//        {
//            var scissors = new int[ 4 ];
//
//            Engine.GL.GetIntegerv( ( int )GLParameter.ScissorBox, ref scissors );
//
//            Logger.Debug( $"Scissors: X={scissors[ 0 ]}, Y={scissors[ 1 ]}, " +
//                          $"Width={scissors[ 2 ]}, Height={scissors[ 3 ]}" );
//        }
//    }
}

// ============================================================================
// ============================================================================