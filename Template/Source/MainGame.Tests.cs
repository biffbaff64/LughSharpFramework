using LughSharp.Lugh.Core;
using LughSharp.Lugh.Files;
using LughSharp.Lugh.Graphics;
using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Graphics.OpenGL.Enums;
using LughSharp.Lugh.Graphics.Utils;
using LughSharp.Tests.Source;
using LughUtils.source.Exceptions;
using LughUtils.source.Logging;

namespace Template.Source;

public partial class MainGame
{
    private static bool _first = true;

    // ========================================================================

    private void RunTests()
    {
        Logger.Divider();
        Logger.Divider();
        // --------------------------------------
        IOUtils.DebugPaths();
        // --------------------------------------
        Logger.Divider();
        Logger.Divider();
        // --------------------------------------
//        var test = new AssetManagerTest();
        var test = new TexturePackerTest();
//        var test = new ImagePackerTest();
        // --------------------------------------
        test.Setup();
        test.Run();
        test.TearDown();
        // --------------------------------------
        Logger.Divider();
        Logger.Divider();
    }

    private void UpdateTests()
    {
    }

    private static void CheckViewportCoverage()
    {
        var viewport = new int[ 4 ];

        Engine.GL.GetIntegerv( IGL.GL_VIEWPORT, ref viewport );

        var windowWidth  = Engine.Api.Graphics.Width;
        var windowHeight = Engine.Api.Graphics.Height;

        var isFullyCovered = ( viewport[ 0 ] == 0 )             // Left edge at 0
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
        else
        {
            if ( _first )
            {
                Logger.Debug( "Viewport fully covers the window." );
                _first = false;
            }
        }
    }

    private void CreateImage1Texture()
    {
        var pixmap = new Pixmap( TEST_WIDTH, TEST_HEIGHT, LughFormat.RGBA8888 );

        _image1 = new Texture( new PixmapTextureData( pixmap,
                                                      LughFormat.RGBA8888,
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

            _image1 = _assetManager?.LoadSingleAsset< Texture >( filename );

            if ( _image1 == null )
            {
                Logger.Debug( "Failed to create texture object." );

                return;
            }

            Logger.Debug( $"Texture loaded - Width: {_image1.Width}, Height: {_image1.Height}" );
            Logger.Debug( $"Format: {PixelFormat.GetFormatString( _image1.TextureData.GetPixelFormat() )}" );
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
        if ( _whitePixelTexture != null )
        {
            return;
        }

        var pixmap = new Pixmap( 100, 100, LughFormat.RGBA8888 );

        pixmap.SetColor( Color.White );
        pixmap.FillWithCurrentColor();

        var textureData = new PixmapTextureData( pixmap, LughFormat.RGBA8888, false, false );

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

    public void DrawViewportBounds( float thickness = 2f )
    {
        if ( _whitePixelTexture == null )
        {
            CreateWhitePixelTexture();

            return;
        }

        // Get and verify viewport dimensions
        var viewport = new int[ 4 ];
        Engine.GL.GetIntegerv( ( int )GLParameter.Viewport, ref viewport );

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

    private void DebugViewportState()
    {
        var viewport = new int[ 4 ];
        Engine.GL.GetIntegerv( ( int )GLParameter.Viewport, ref viewport );

        Logger.Debug( $"Viewport: X={viewport[ 0 ]}, Y={viewport[ 1 ]}, Width={viewport[ 2 ]}, Height={viewport[ 3 ]}" );

        // Check scissors test
        var scissorEnabled = Engine.GL.IsEnabled( ( int )EnableCap.ScissorTest );
        Logger.Debug( $"Scissor Test Enabled: {scissorEnabled}" );

        if ( scissorEnabled )
        {
            var scissors = new int[ 4 ];

            Engine.GL.GetIntegerv( ( int )GLParameter.ScissorBox, ref scissors );

            Logger.Debug( $"Scissors: X={scissors[ 0 ]}, Y={scissors[ 1 ]}, " +
                          $"Width={scissors[ 2 ]}, Height={scissors[ 3 ]}" );
        }
    }
}