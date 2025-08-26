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

using DesktopGLBackend.Graphics;

using LughSharp.Lugh.Core;
using LughSharp.Lugh.Files;
using LughSharp.Lugh.Graphics;
using LughSharp.Lugh.Graphics.G2D;
using LughSharp.Lugh.Maths;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;

using Platform = LughSharp.Lugh.Core.Platform;

namespace DesktopGLBackend.Window;

/// <summary>
/// Wrapper/Manager class for a <see cref="GLFW.Window" />.
/// </summary>
[PublicAPI]
public partial class DesktopGLWindow : IDisposable
{
    /// <summary>
    /// Represents the underlying GLFW window used by the DesktopGL backend.
    /// This property provides access to the native GLFW window instance, allowing
    /// for direct interaction with the GLFW API.
    /// </summary>
    public GLFW.Window? GlfwWindow { get; set; }

    /// <summary>
    /// Represents the listener for window events in the DesktopGL backend.
    /// This property allows event-driven interaction with the window lifecycle
    /// through the implementation of the <see cref="IDesktopGLWindowListener"/> interface.
    /// It provides hooks for handling window creation, focus changes, iconification,
    /// maximization, close requests, file drops, and refresh requests.
    /// </summary>
    public IDesktopGLWindowListener? WindowListener { get; set; }

    /// <summary>
    /// Represents the main application listener for the DesktopGL backend.
    /// This property provides access to an implementation of <see cref="IApplicationListener"/>,
    /// handling core application lifecycle events such as creation, resizing, updating,
    /// rendering, and pausing/resuming the application.
    /// </summary>
    public IApplicationListener? ApplicationListener { get; set; }

    /// <summary>
    /// Represents the input handling system for the DesktopGL backend.
    /// This property provides access to input-related functionality and allows
    /// for interaction with devices such as keyboards, mice, and controllers.
    /// </summary>
    public IDesktopGLInput Input { get; set; } = null!;

    /// <summary>
    /// Represents the configuration settings for the DesktopGL application.
    /// This property allows accessing and modifying application-specific
    /// configurations, such as rendering settings, window behavior, or high DPI modes.
    /// </summary>
    public DesktopGLApplicationConfiguration AppConfig { get; set; }

    /// <summary>
    /// Provides graphics-related functionality and operations for the DesktopGL backend.
    /// This property initializes and manages the graphical context, handling rendering
    /// processes and maintaining compatibility with the application's configuration.
    /// </summary>
    public DesktopGLGraphics Graphics { get; set; } = null!;

    /// <summary>
    /// Represents the application instance associated with the desktop OpenGL backend.
    /// This property provides access to the application's managing logic, configurations,
    /// and its lifecycle, facilitating integration with application-specific behavior.
    /// </summary>
    public DesktopGLApplication Application { get; set; }

    /// <summary>
    /// Indicates whether the listener has been successfully initialized.
    /// This property ensures that the <see cref="IApplicationListener"/> has been
    /// properly set up prior to performing operations that require it.
    /// </summary>
    public bool ListenerInitialised { get; set; } = false;

    // ========================================================================

    private List< IRunnable.Runnable > _executedRunnables = [ ];
    private List< IRunnable.Runnable > _runnables         = [ ];

    private Vector2 _tmpV2            = new();
    private bool    _focused          = false;
    private bool    _iconified        = false;
    private bool    _requestRendering = false;
    private bool    _disposed         = false;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates a new DesktopGLWindow instance, using the given <see cref="IApplicationListener" />,
    /// <see cref="DesktopGLApplicationConfiguration" />, and attaching it to the given
    /// <see cref="DesktopGLApplication" />.
    /// </summary>
    public DesktopGLWindow( IApplicationListener listener,
                            DesktopGLApplicationConfiguration config,
                            DesktopGLApplication application )
    {
        ApplicationListener = listener;
        WindowListener      = config.WindowListener;
        AppConfig           = DesktopGLApplicationConfiguration.Copy( config );
        Application         = application;

        if ( ApplicationListener == null )
        {
            Logger.Warning( "Provided ApplicationListener is NULL!" );
        }

        if ( WindowListener == null )
        {
            Logger.Warning( "Provided config.WindowListener is NULL!" );
        }
    }

    /// <summary>
    /// Creates a DesktopGLWindow by initializing the specified GLFW window, setting
    /// up the input and graphics components, and configuring GLVersion and callback
    /// behaviors.
    /// </summary>
    /// <param name="window">
    /// The GLFW window instance to initialize and associate with this DesktopGLWindow.
    /// </param>
    public void Create( GLFW.Window window )
    {
        GlfwWindow         = window;
        Input              = Application.CreateInput( this );
        Graphics           = new DesktopGLGraphics( this );
        Graphics.GLVersion = Application.GLVersion;

        Engine.Api.Input    = Input;
        Engine.Api.Graphics = Graphics;

        Glfw.SetWindowFocusCallback( window, GdxFocusCallback );
        Glfw.SetWindowIconifyCallback( window, GdxIconifyCallback );
        Glfw.SetWindowMaximizeCallback( window, GdxMaximizeCallback );
        Glfw.SetWindowCloseCallback( window, GdxWindowCloseCallback );
        Glfw.SetDropCallback( window, GdxDropCallback );
        Glfw.SetWindowRefreshCallback( window, GdxRefreshCallback );

        WindowListener?.Created( this );
    }

    /// <summary>
    /// Update this window.
    /// </summary>
    /// <returns> True if the window should render itself. </returns>
    public bool Update()
    {
        if ( !ListenerInitialised )
        {
            InitialiseListener();
        }

        lock ( _runnables )
        {
            foreach ( var runnable in _runnables )
            {
                _executedRunnables.Add( runnable );
            }

            _runnables.Clear();
        }

        foreach ( var runnable in _executedRunnables )
        {
            runnable();
        }

        var shouldRender = ( _executedRunnables.Count > 0 ) || Graphics.ContinuousRendering;

        _executedRunnables.Clear();

        if ( !_iconified )
        {
            Input.Update();
        }

        lock ( this )
        {
            shouldRender      |= _requestRendering && !_iconified;
            _requestRendering =  false;
        }

        if ( shouldRender )
        {
            Graphics.Update();
            ApplicationListener?.Update();
            ApplicationListener?.Render();

            Glfw.SwapBuffers( GlfwWindow );
        }

        if ( !_iconified )
        {
            Input.PrepareNext();
        }

        return shouldRender;
    }

    /// <summary>
    /// Post a <see cref="IRunnable.Runnable" /> to this window's event queue. Use this if
    /// you access statics like <see cref="Engine.Graphics" /> in your runnable instead
    /// of <see cref="DesktopGLApplication.PostRunnable(IRunnable.Runnable)" />".
    /// </summary>
    public void PostRunnable( IRunnable.Runnable runnable )
    {
        lock ( _runnables )
        {
            _runnables.Add( runnable );
        }
    }

    /// <summary>
    /// Makes this the currently active window.
    /// </summary>
    public void MakeCurrent()
    {
        Engine.Api.Graphics = Graphics;
        Engine.Api.Input    = Input;

        Glfw.MakeContextCurrent( GlfwWindow );

        Graphics.CurrentContext = Glfw.GetCurrentContext();
    }

    /// <summary>
    /// Reguest the window to be drawn.
    /// </summary>
    public void RequestRendering()
    {
        lock ( this )
        {
            _requestRendering = true;
        }
    }

    /// <summary>
    /// Returns <b>true</b> if this window should close. It establishes this
    /// via <see cref="Glfw.WindowShouldClose(GLFW.Window)" />
    /// </summary>
    /// <returns></returns>
    public bool ShouldClose()
    {
        return Glfw.WindowShouldClose( GlfwWindow );
    }

    /// <summary>
    /// Return the window X position in logical coordinates. All monitors span a virtual
    /// surface together. The coordinates are relative to the first monitor in the
    /// virtual surface.
    /// </summary>
    public int PositionX => ( int )GetPosition().X;

    /// <summary>
    /// Return the window Y position in logical coordinates. All monitors span a virtual
    /// surface together. The coordinates are relative to the first monitor in the
    /// virtual surface.
    /// </summary>
    public int PositionY => ( int )GetPosition().Y;

    /// <inheritdoc cref="Glfw.SetWindowPos(GLFW.Window,int,int)" />
    public void SetPosition( int x, int y )
    {
        Glfw.SetWindowPos( GlfwWindow, x, y );
    }

    /// <summary>
    /// Gets the current window position in logical coordinates. All monitors span a
    /// virtual surface together. The coordinates are relative to the first monitor
    /// in the virtual surface.
    /// </summary>
    /// <returns>A Vector2 holding the window X and Y.</returns>
    public Vector2 GetPosition()
    {
        Glfw.GetWindowPos( GlfwWindow, out var xPos, out var yPos );

        return _tmpV2.Set( xPos, yPos );
    }

    /// <summary>
    /// Sets the visibility of the window.
    /// Invisible windows will still call their <see cref="IApplicationListener" />
    /// </summary>
    public void SetVisible( bool visible )
    {
        if ( visible )
        {
            Glfw.ShowWindow( GlfwWindow );
        }
        else
        {
            Glfw.HideWindow( GlfwWindow );
        }
    }

    /// <summary>
    /// Closes this window and pauses and disposes the associated <see cref="IApplicationListener" />.
    /// This function sets the value of the close flag of the specified window. This can be used to
    /// override the user's attempt to close the window, or to signal that it should be closed.
    /// </summary>
    public void CloseWindow()
    {
        Glfw.SetWindowShouldClose( GlfwWindow, true );
    }

    /// <summary>
    /// Minimizes (iconifies) the window. Iconified windows do not call their
    /// <see cref="IApplicationListener" /> until the window is restored.
    /// </summary>
    public void IconifyWindow()
    {
        Glfw.IconifyWindow( GlfwWindow );
    }

    /// <summary>
    /// Sets the icon that will be used in the window's title bar. Has no effect in macOS,
    /// which doesn't use window icons.
    /// </summary>
    /// <param name="images">
    /// One or more images. The one closest to the system's desired size will be scaled.
    /// Good sizes include 16x16, 32x32 and 48x48. Pixmap format <see cref="Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888" />
    /// is preferred so the images will not have to be copied and converted.
    /// <b>
    /// The chosen image
    /// is copied, and the provided Pixmaps are not disposed.
    /// </b>
    /// </param>
    public void SetIcon( params Pixmap[] images )
    {
        if ( GlfwWindow != null )
        {
            SetIcon( GlfwWindow, images );
        }
    }

    /// <summary>
    /// This function restores the specified window if it was previously iconified
    /// (minimized) or maximized. If the window is already restored, this function
    /// does nothing. If the specified window is a full screen window, the resolution
    /// chosen for the window is restored on the selected monitor.
    /// </summary>
    public void RestoreWindow()
    {
        Glfw.RestoreWindow( GlfwWindow );
    }

    /// <summary>
    /// This function maximizes the specified window if it was previously not maximized.
    /// If the window is already maximized, this function does nothing. If the specified
    /// window is a full screen window, this function does nothing.
    /// </summary>
    public void MaximizeWindow()
    {
        Glfw.MaximizeWindow( GlfwWindow );
    }

    /// <summary>
    /// Brings the window to front and sets input focus. The window should
    /// already be visible and not iconified.
    /// </summary>
    public void FocusWindow()
    {
        Glfw.FocusWindow( GlfwWindow );
    }

    /// <summary>
    /// Sets the windows title.
    /// </summary>
    /// <param name="title"> String holding the Title text. </param>
    public void SetTitle( string title )
    {
        Glfw.SetWindowTitle( GlfwWindow, title );
    }

    /// <summary>
    /// Sets minimum and maximum size limits for the window. If the window
    /// is full screen or not resizable, these limits are ignored. Use -1
    /// to indicate an unrestricted dimension.
    /// </summary>
    public void SetSizeLimits( int minWidth, int minHeight, int maxWidth, int maxHeight )
    {
        SetSizeLimits( GlfwWindow, minWidth, minHeight, maxWidth, maxHeight );
    }

    /// <summary>
    /// Sets minimum and maximum size limits for the given window. If the window
    /// is full screen or not resizable, these limits are ignored.
    /// Use -1 to indicate an unrestricted dimension.
    /// </summary>
    /// <param name="handle"> The window. </param>
    /// <param name="minWidth"> The minimum window width. </param>
    /// <param name="minHeight"> The minimum window height. </param>
    /// <param name="maxWidth"> The maximum window width. </param>
    /// <param name="maxHeight"> The maximum window height. </param>
    public static void SetSizeLimits( GLFW.Window? handle, int minWidth, int minHeight, int maxWidth, int maxHeight )
    {
        Glfw.SetWindowSizeLimits( handle,
                                  minWidth > -1 ? minWidth : -1,
                                  minHeight > -1 ? minHeight : -1,
                                  maxWidth > -1 ? maxWidth : -1,
                                  maxHeight > -1 ? maxHeight : -1 );
    }

    /// <summary>
    /// Sets the icon that will be used in the window's title bar. Has no effect in macOS,
    /// which doesn't use window icons.
    /// </summary>
    public static void SetIcon( GLFW.Window window, string[] imagePaths, PathTypes imagePathType )
    {
        if ( Platform.IsMac )
        {
            return;
        }

        var pixmaps = new Pixmap[ imagePaths.Length ];

        for ( var i = 0; i < imagePaths.Length; i++ )
        {
            pixmaps[ i ] = new Pixmap( Engine.Api.Files.GetFileHandle( imagePaths[ i ], imagePathType ) );
        }

        SetIcon( window, pixmaps );

        foreach ( var pixmap in pixmaps )
        {
            pixmap.Dispose();
        }
    }

    /// <summary>
    /// Sets the icon that will be used in the window's title bar. Has no effect in macOS,
    /// which doesn't use window icons.
    /// </summary>
    /// <param name="window"> The applicable window. </param>
    /// <param name="images">
    /// One or more images. The one closest to the system's desired size will be scaled.
    /// Good sizes include 16x16, 32x32 and 48x48. Pixmap format <see cref="Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888" />
    /// is preferred so the images will not have to be copied and converted.
    /// <b>
    /// The chosen image
    /// is copied, and the provided Pixmaps are not disposed.
    /// </b>
    /// </param>
    public static void SetIcon( GLFW.Window window, Pixmap[] images )
    {
        if ( Platform.IsMac )
        {
            return;
        }

        List< GLFW.Image > buffer = new( images.Length );

        Pixmap?[] tmpPixmaps = new Pixmap[ images.Length ];

        for ( var i = 0; i < images.Length; i++ )
        {
            if ( images[ i ].GetColorFormat() != Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888 )
            {
                var rgba = new Pixmap( images[ i ].Width, images[ i ].Height, Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888 );

                rgba.Blending = Pixmap.BlendTypes.None;
                rgba.DrawPixmap( images[ i ], 0, 0 );

                tmpPixmaps[ i ] = rgba;
            }

            GLFW.Image icon = new()
            {
                Width  = images[ i ].Width,
                Height = images[ i ].Height,
                Pixels = images[ i ].PixelData,
            };

            buffer.Add( icon );
        }

        Glfw.SetWindowIcon( window, buffer.ToArray() );

        foreach ( var pixmap in tmpPixmaps )
        {
            pixmap?.Dispose();
        }
    }

    /// <summary>
    /// Initialises the <see cref="IApplicationListener" />.
    /// </summary>
    private void InitialiseListener()
    {
        if ( !ListenerInitialised )
        {
            ApplicationListener?.Create();
            ApplicationListener?.Resize( Graphics.Width, Graphics.Height );
            ListenerInitialised = true;
        }
    }

    // ========================================================================
    // ========================================================================

    #region dispose pattern

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose( true );

        GC.SuppressFinalize( this );
    }

    protected void Dispose( bool disposing )
    {
        if ( !_disposed )
        {
            if ( disposing )
            {
                ApplicationListener?.Pause();
                ApplicationListener?.Dispose();
                DesktopGLCursor.DisposeGLCursor( this );
                Graphics.Dispose();
                Input.Dispose();

//                Glfw.SetWindowFocusCallback( GlfwWindow, null );
//                Glfw.SetWindowIconifyCallback( GlfwWindow, null );
//                Glfw.SetWindowCloseCallback( GlfwWindow, null );
//                Glfw.SetDropCallback( GlfwWindow, null );
                Glfw.DestroyWindow( GlfwWindow );
            }

            _disposed = true;
        }
    }

    #endregion dispose pattern
}