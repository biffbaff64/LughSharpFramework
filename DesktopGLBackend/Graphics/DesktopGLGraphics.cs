// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Richard Ikin.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ///////////////////////////////////////////////////////////////////////////////

using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.FrameBuffers;
using LughSharp.Core.Graphics.OpenGL;
using LughSharp.Core.Graphics.OpenGL.Enums;
using LughSharp.Core.Graphics.Utils;
using LughSharp.Core.Main;
using LughSharp.Core.Utils;
using LughUtils.source.Exceptions;
using LughUtils.source.Logging;

namespace DesktopGLBackend.Graphics;

/// <summary>
/// Represents the graphics system for the DesktopGL backend, handling rendering,
/// window management,  display modes, and various graphical operations.
/// <para>
/// Inherits from <see cref="GraphicsDevice"/> and implements <see cref="IDisposable"/>.
/// </para>
/// </summary>
[PublicAPI]
public partial class DesktopGLGraphics : GraphicsDevice, IDisposable
{
    public DesktopGLWindow? GLWindow { get; set; }

    // ========================================================================

    private IGraphicsDevice.DisplayMode? _displayModeBeforeFullscreen;

    private int  _fps;
    private long _frameCounterStart = 0;
    private long _frameId;
    private int  _frames;
    private long _lastFrameTime = -1;
    private int  _windowHeightBeforeFullscreen;
    private int  _windowPosXBeforeFullscreen;
    private int  _windowPosYBeforeFullscreen;
    private int  _windowWidthBeforeFullscreen;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates a new GLGraphics instance for Desktop backends, using the
    /// given <see cref="DesktopGLWindow"/> as the main window.
    /// </summary>
    public DesktopGLGraphics( DesktopGLWindow glWindow )
    {
        GLWindow = glWindow;

        UpdateFramebufferInfo();
        UpdateGLVersion();

        DotGLFW.Glfw.SetWindowSizeCallback( GLWindow.GlfwWindow, ResizeCallback );
    }

    // ========================================================================

    /// <inheritdoc />
    public override int Width => GLWindow?.AppConfig.HdpiMode == HdpiMode.Pixels
        ? BackBufferWidth
        : LogicalWidth;

    /// <inheritdoc />
    public override int Height => GLWindow?.AppConfig.HdpiMode == HdpiMode.Pixels
        ? BackBufferHeight
        : LogicalHeight;

    /// <inheritdoc />
    public override GraphicsBackend.BackendType GraphicsType => GraphicsBackend.BackendType.OpenGLES;

    // ========================================================================

    /// <summary>
    /// Whether the app is full screen or not.
    /// </summary>
    public override bool IsFullscreen
    {
        get
        {
            Guard.Against.Null( GLWindow );

            return DotGLFW.Glfw.GetWindowMonitor( GLWindow.GlfwWindow ) != null;
        }
    }

    /// <summary>
    /// Whether an IGraphics implementation supports display mode changing or not.
    /// Display mode changing refers to the ability to switch between different
    /// screen resolutions, refresh rates, and fullscreen/windowed modes.
    /// </summary>
    /// <returns><c>true</c> if display mode changing is supported, <c>false</c> otherwise.</returns>
    public override bool SupportsDisplayModeChange()
    {
        // Display mode change is enabled by default on Windows Desktop.
        return true;
    }

    /// <summary>
    /// Handles the resize event for the GLFW window.
    /// </summary>
    /// <param name="windowHandle">The handle to the GLFW window being resized.</param>
    /// <param name="width">The new width of the window after resizing.</param>
    /// <param name="height">The new height of the window after resizing.</param>
    public void ResizeCallback( DotGLFW.Window windowHandle, int width, int height )
    {
        RenderWindow( windowHandle, width, height );
    }

    /// <summary>
    /// Updates the framebuffer information based on the current state of the window.
    /// This method queries the framebuffer size and logical window size, updating
    /// attributes such as backbuffer dimensions, logical dimensions, and buffer format.
    /// </summary>
    private void UpdateFramebufferInfo()
    {
        if ( ( GLWindow == null ) || ( GLWindow.GlfwWindow == null ) )
        {
            throw new GdxRuntimeException( "GLWindow ( or GlfwWindow ) is null!" );
        }

        DotGLFW.Glfw.GetFramebufferSize( GLWindow.GlfwWindow, out var tmpWidth, out var tmpHeight );

        BackBufferWidth  = tmpWidth;
        BackBufferHeight = tmpHeight;

        DotGLFW.Glfw.GetWindowSize( GLWindow.GlfwWindow, out tmpWidth, out tmpHeight );

        LogicalWidth  = tmpWidth;
        LogicalHeight = tmpHeight;

        BufferConfig = new FramebufferConfig
        {
            R                = GLWindow.AppConfig.Red,
            G                = GLWindow.AppConfig.Green,
            B                = GLWindow.AppConfig.Blue,
            A                = GLWindow.AppConfig.Alpha,
            Depth            = GLWindow.AppConfig.Depth,
            Stencil          = GLWindow.AppConfig.Stencil,
            Samples          = GLWindow.AppConfig.Samples,
            CoverageSampling = false,
        };
        
        GL.Enable( EnableCap.FramebufferSrgb );
    }

    /// <summary>
    /// Updates the current graphics state by calculating the frame delta time,
    /// updating frames per second (FPS), and incrementing the frame identifier.
    /// This method is typically called on each render loop iteration.
    /// </summary>
    public override void Update()
    {
        var time = TimeUtils.NanoTime();

        if ( _lastFrameTime == -1 )
        {
            _lastFrameTime = time;
        }

        DeltaTime      = ( time - _lastFrameTime ) / ( float )TimeUtils.NANOSECONDS_PER_SECOND;
        _lastFrameTime = time;

        if ( ( time - _frameCounterStart ) >= TimeUtils.NANOSECONDS_PER_SECOND )
        {
            _fps               = _frames;
            _frames            = 0;
            _frameCounterStart = time;
        }

        _frames++;
        _frameId++;
    }

    /// <summary>
    /// Returns whether cubemap seamless feature is supported.
    /// </summary>
    public override bool SupportsCubeMapSeamless()
    {
        return SupportsExtension( "GL_ARB_seamless_cube_map" );
    }

    /// <summary>
    /// Enable or disable cubemap seamless feature. Default is true if supported.
    /// Should only be called if this feature is supported.
    /// </summary>
    /// <param name="enable"></param>
    public override void EnableCubeMapSeamless( bool enable )
    {
        if ( SupportsCubeMapSeamless() )
        {
            Engine.GL.EnableOrDisable( EnableCap.TextureCubemapSeamless, enable );
        }
    }

    /// <inheritdoc />
    public override bool SetWindowedMode( int width, int height )
    {
        GLWindow?.Input.ResetPollingStates();

        var monitor = DotGLFW.Glfw.GetPrimaryMonitor();

        if ( !IsFullscreen )
        {
            if ( ( width != LogicalWidth ) || ( height != LogicalHeight ) )
            {
                //Center window
                DotGLFW.Glfw.GetMonitorWorkarea( monitor, out var x, out var y, out var w, out var h );
                DotGLFW.Glfw.SetWindowPos( GLWindow!.GlfwWindow, x, y );

                GLWindow?.SetPosition( x + ( ( w - width ) / 2 ), y + ( ( h - height ) / 2 ) );
            }

            DotGLFW.Glfw.SetWindowSize( GLWindow!.GlfwWindow, width, height );
        }
        else
        {
            if ( _displayModeBeforeFullscreen == null )
            {
                BackupCurrentWindow();
            }

            if ( ( width != _windowWidthBeforeFullscreen ) || ( height != _windowHeightBeforeFullscreen ) )
            {
                DotGLFW.Glfw.GetMonitorWorkarea( monitor, out var x, out var y, out var w, out var h );

                DotGLFW.Glfw.SetWindowMonitor( GLWindow!.GlfwWindow,
                                       monitor,
                                       x + ( ( w - width ) / 2 ),
                                       y + ( ( h - height ) / 2 ),
                                       width,
                                       height,
                                       _displayModeBeforeFullscreen!.RefreshRate );
            }
            else
            {
                DotGLFW.Glfw.SetWindowMonitor( GLWindow!.GlfwWindow,
                                       monitor,
                                       _windowPosXBeforeFullscreen,
                                       _windowPosYBeforeFullscreen,
                                       width,
                                       height,
                                       _displayModeBeforeFullscreen!.RefreshRate );
            }
        }

        UpdateFramebufferInfo();

        return true;
    }

    /// <inheritdoc />
    public override void SetUndecorated( bool undecorated )
    {
        Guard.Against.Null( GLWindow );

        GLWindow.AppConfig.WindowDecorated = !undecorated;

        DotGLFW.Glfw.SetWindowAttrib( GLWindow.GlfwWindow, DotGLFW.WindowAttrib.Decorated, undecorated );
    }

    /// <inheritdoc />
    public override void SetResizable( bool resizable )
    {
        Guard.Against.Null( GLWindow );

        GLWindow.AppConfig.WindowResizable = resizable;

        DotGLFW.Glfw.SetWindowAttrib( GLWindow.GlfwWindow, DotGLFW.WindowAttrib.Resizable, resizable );
    }

    /// <inheritdoc />
    public override void SetVSync( bool vsync )
    {
        Guard.Against.Null( GLWindow );

        GLWindow.AppConfig.VSyncEnabled = vsync;

        DotGLFW.Glfw.SwapInterval( vsync ? 1 : 0 );
    }

    /// <inheritdoc />
    public override void SetForegroundFps( int fps )
    {
        Guard.Against.Null( GLWindow );

        GLWindow.AppConfig.ForegroundFPS = fps;
    }

    /// <inheritdoc />
    public override bool SupportsExtension( string extension )
    {
        return DotGLFW.Glfw.ExtensionSupported( extension );
    }

    /// <inheritdoc />
    public override void RequestRendering()
    {
        GLWindow?.RequestRendering();
    }

    /// <inheritdoc />
    public override ICursor NewCursor( Pixmap pixmap, int xHotspot, int yHotspot )
    {
        return new DesktopGLCursor( GLWindow!, pixmap, xHotspot, yHotspot );
    }

    /// <summary>
    /// Browsers that support cursor:url() and support the png format (the pixmap is
    /// converted to a data-url of type image/png) should also support custom cursors.
    /// Will set the mouse cursor image to the image represented by the Cursor. It is
    /// recommended to call this function in the main render thread, and maximum one
    /// time per frame.
    /// </summary>
    /// <param name="cursor">
    /// The mouse cursor as a <see cref="ICursor"/>
    /// </param>
    public override void SetCursor( ICursor cursor )
    {
        Guard.Against.Null( GLWindow );

        DotGLFW.Glfw.SetCursor( GLWindow.GlfwWindow, ( ( DesktopGLCursor )cursor ).GlfwCursor );
    }

    /// <summary>
    /// Sets one of the predefined <see cref="ICursor.SystemCursor"/>s.
    /// </summary>
    /// <param name="systemCursor">The system cursor to use.</param>
    public override void SetSystemCursor( ICursor.SystemCursor systemCursor )
    {
        DesktopGLCursor.SetSystemCursor( GLWindow!.GlfwWindow!, systemCursor );
    }

    // ========================================================================

    /// <inheritdoc />
    public override IGraphicsDevice.DisplayMode[] GetDisplayModes()
    {
        Guard.Against.Null( GLWindow );

        return GLWindow.AppConfig.GetDisplayModes( DotGLFW.Glfw.GetPrimaryMonitor() );
    }

    /// <inheritdoc />
    public override IGraphicsDevice.DisplayMode[] GetDisplayModes( DotGLFW.Monitor monitor )
    {
        Guard.Against.Null( GLWindow );

        return GLWindow.AppConfig.GetDisplayModes( monitor );
    }

    /// <inheritdoc />
    public override IGraphicsDevice.DisplayMode GetDisplayMode()
    {
        Guard.Against.Null( GLWindow );

        return GLWindow.AppConfig.GetDisplayMode( DotGLFW.Glfw.GetPrimaryMonitor() );
    }

    /// <inheritdoc />
    public override IGraphicsDevice.DisplayMode GetDisplayMode( DotGLFW.Monitor monitor )
    {
        Guard.Against.Null( GLWindow );

        return GLWindow.AppConfig.GetDisplayMode( monitor );
    }

    /// <inheritdoc />
    public override HdpiMode? GetHdpiMode()
    {
        return GLWindow?.AppConfig.HdpiMode;
    }

    /// <inheritdoc />
    public override bool SetFullscreenMode( IGraphicsDevice.DisplayMode displayMode )
    {
        Guard.Against.Null( GLWindow );

        GLWindow.Input.ResetPollingStates();

        var newMode = ( DesktopGLDisplayMode )displayMode;

        if ( IsFullscreen )
        {
            var currentMode = ( DesktopGLDisplayMode )GetDisplayMode();

            if ( ( currentMode.MonitorHandle == newMode.MonitorHandle )
                 && ( currentMode.RefreshRate == newMode.RefreshRate ) )
            {
                // same monitor and refresh rate
                DotGLFW.Glfw.SetWindowSize( GLWindow.GlfwWindow, newMode.Width, newMode.Height );
            }
            else
            {
                // different monitor and/or refresh rate
                DotGLFW.Glfw.SetWindowMonitor( GLWindow.GlfwWindow,
                                       newMode.MonitorHandle,
                                       0,
                                       0,
                                       newMode.Width,
                                       newMode.Height,
                                       newMode.RefreshRate );
            }
        }
        else
        {
            // store window position so we can restore it when switching
            // from fullscreen to windowed later
            BackupCurrentWindow();

            // switch from windowed to fullscreen
            DotGLFW.Glfw.SetWindowMonitor( GLWindow.GlfwWindow,
                                   newMode.MonitorHandle,
                                   0,
                                   0,
                                   newMode.Width,
                                   newMode.Height,
                                   newMode.RefreshRate );
        }

        UpdateFramebufferInfo();

        SetVSync( GLWindow!.AppConfig.VSyncEnabled );

        return true;
    }

    /// <inheritdoc />
    public override void UpdateViewport( int x, int y, int width, int height, int source = 0 )
    {
        if ( ( width == 0 ) || ( height == 0 ) )
        {
            throw new GdxRuntimeException( "Viewport dimensions must be greater than zero!" );
        }

        // Set the viewport
        Engine.GL.Viewport( x, y, width, height );

        // Check if viewport was set correctly
        var viewport = new int[ 4 ];

        Engine.GL.GetIntegerv( IGL.GL_VIEWPORT, ref viewport );

        // Verify viewport dimensions match what we set
        if ( ( viewport[ 0 ] != x ) || ( viewport[ 1 ] != y ) ||
             ( viewport[ 2 ] != width ) || ( viewport[ 3 ] != height ) )
        {
            Logger.Error( "Viewport dimensions mismatch!"
                            + $"\nRequested: [{x}, {y}, {width}, {height}]"
                            + $"\nActual: [{viewport[ 0 ]}, {viewport[ 1 ]}, "
                            + $"{viewport[ 2 ]}, {viewport[ 3 ]}]" );
        }

        // --------------------------------------------------------------------
    }

    /// <inheritdoc />
    public override (float X, float Y) GetPpiXY()
    {
        return ( GetPpcXY().X * 2.54f, GetPpcXY().Y * 2.54f );
    }

    /// <inheritdoc />
    public override (float X, float Y) GetPpcXY()
    {
        DotGLFW.Glfw.GetMonitorPhysicalSize( DotGLFW.Glfw.GetPrimaryMonitor(), out var sizeX, out var sizeY );

        return ( ( GetDisplayMode().Width / ( float )sizeX ) * 10,
            ( GetDisplayMode().Height / ( float )sizeY ) * 10 );
    }

    // ========================================================================
    // ========================================================================

    public override void RenderWindow( DotGLFW.Window? windowHandle, int width, int height )
    {
        UpdateFramebufferInfo();

        if ( !GLWindow!.ListenerInitialised )
        {
            return;
        }

        GLWindow.MakeCurrent();

        UpdateViewport( 0, 0, BackBufferWidth, BackBufferHeight, 1 );

        GLWindow.ApplicationListener?.Resize( Width, Height );
        GLWindow.ApplicationListener?.Update();
        GLWindow.ApplicationListener?.Render();

        DotGLFW.Glfw.SwapBuffers( windowHandle );
    }

    /// <summary>
    /// Makes a backup of the current windows position and display mode.
    /// </summary>
    private void BackupCurrentWindow()
    {
        if ( GLWindow == null )
        {
            return;
        }

        _windowPosXBeforeFullscreen   = GLWindow.PositionX;
        _windowPosYBeforeFullscreen   = GLWindow.PositionY;
        _windowWidthBeforeFullscreen  = LogicalWidth;
        _windowHeightBeforeFullscreen = LogicalHeight;
        _displayModeBeforeFullscreen  = GetDisplayMode();
    }

    private void UpdateGLVersion()
    {
//        var vendorString   = GdxApi.GL.GetString( IGL.GL_VENDOR );
//        var rendererString = GdxApi.GL.GetString( IGL.GL_RENDERER );

//        AppVersion = new AppVersion( Platform.ApplicationType.WindowsGL,
//                                   vendorString,
//                                   rendererString );

        EnableCubeMapSeamless( true );
    }

    // ========================================================================

    public override int GetSafeInsetLeft()
    {
        return 0;
    }

    public override int GetSafeInsetTop()
    {
        return 0;
    }

    public override int GetSafeInsetBottom()
    {
        return 0;
    }

    public override int GetSafeInsetRight()
    {
        return 0;
    }

    public override long GetFrameID()
    {
        return _frameId;
    }

    public override int GetFramesPerSecond()
    {
        return _fps;
    }

    // ========================================================================

    #region IDisposable implementation

    protected static void Dispose( bool disposing )
    {
        if ( disposing )
        {
            //TODO:
        }
    }

    public void Dispose()
    {
        Dispose( true );
    }

    #endregion IDisposable implementation
}

// ============================================================================
// ============================================================================
