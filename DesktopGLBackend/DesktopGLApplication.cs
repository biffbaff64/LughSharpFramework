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

using LughSharp.Lugh.Core;
using LughSharp.Lugh.Graphics;
using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Graphics.Utils;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Collections;
using LughSharp.Lugh.Utils.Exceptions;
using LughSharp.Lugh.Utils.Logging;

using Platform = LughSharp.Lugh.Core.Platform;

namespace DesktopGLBackend;

/// <summary>
/// Creates, and manages, an application to for Windows OpenGL backends.
/// </summary>
[PublicAPI]
public class DesktopGLApplication : IApplication, IDisposable
{
    /// <summary>
    /// Pewrsistant properties manager instance.
    /// </summary>
    public Dictionary< string, IPreferences > Preferences { get; set; } = [ ];

    /// <summary>
    /// Container for the list of available DesktopGLWindows used by the application.
    /// </summary>
    public List< DesktopGLWindow > Windows { get; set; } = [ ];

    /// <summary>
    /// Holds a list of LifeCycle listeners to process while the application
    /// is active.
    /// </summary>
    public List< ILifecycleListener > LifecycleListeners { get; set; } = [ ];

    /// <summary>
    /// Application Configuration Settings
    /// </summary>
    public DesktopGLApplicationConfiguration? AppConfig { get; set; }

    public List< IRunnable.Runnable > Runnables         { get; set; } = [ ];
    public List< IRunnable.Runnable > ExecutedRunnables { get; set; } = [ ];

    public IClipboard?      Clipboard     { get; set; }
    public GLVersion?       GLVersion     { get; set; }
    public OpenGLProfile    OGLProfile    { get; set; }
    public DesktopGLWindow? CurrentWindow { get; set; }

    // ========================================================================
    // ========================================================================

    private const int UNINITIALISED_FRAMERATE = -2;

    // ========================================================================

    private static   GlfwErrorCallback? _errorCallback;
    private readonly Sync?              _sync;
    private          bool               _glfwInitialised = false;
    private          IPreferences       _prefs;
    private          bool               _running  = true;
    private          bool               _disposed = false;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates a new Desktop Gl Application using the provided <see cref="DesktopGLApplicationConfiguration" />.
    /// </summary>
    /// <param name="listener"> The <see cref="IApplicationListener" /> to use. </param>
    /// <param name="config"> The <see cref="DesktopGLApplicationConfiguration" /> to use.</param>
    public DesktopGLApplication( IApplicationListener listener, DesktopGLApplicationConfiguration config )
    {
        // ====================================================================
        // ====================================================================
        // Essential first actions. Do not move.
        //
        // This MUST be the first call, so that the Logger and Engine.App global are
        // initialised correctly.
        Engine.Api.Initialise( this );

        // ====================================================================
        // ====================================================================

        // Enable GLProfiling in preferences
        _prefs = GetPreferences( "desktopgl.lugh.engine.preferences" );
        _prefs.PutBool( "profiling", config.GLProfilingEnabled );
        _prefs.Flush();

        // Config.Title becomes the name of the ApplicationListener
        // if it has no value at this point.
        AppConfig       =   DesktopGLApplicationConfiguration.Copy( config );
        AppConfig.Title ??= listener.GetType().Name;

        // ====================================================================

        // Initialise the global environment shortcuts. 'Engine.Audio', 'Engine.Files',
        // and 'Engine.Net' are instances of classes implementing IAudio, IFiles, and
        // INet respectively, and are used to access LughSharp members 'Audio', 'Files',
        // and 'Network' are instances of classes which extend the aforementioned classes,
        // and are used in backend code only.
        // Note: Engine.Graphics is set later, during window creation as each window that
        // is created will have its own IGraphics instance.
        Engine.Api.Audio = AudioManager.CreateAudio( AppConfig );
        Engine.Api.Files = new LughSharp.Lugh.Files.Files();
        Engine.Api.Net   = new DesktopGLNet( AppConfig );

        Clipboard = new DesktopGLClipboard();
        _sync     = new Sync();

        InitialiseGlfw();

        Windows.Add( CreateWindow( AppConfig, listener, 0 ) );
    }

    // ========================================================================

    /// <summary>
    /// The entry point for running code using this framework. At this point at least one
    /// window will have been created, Glfw will have been set up, and the framework properly
    /// initialised. This passes control to <see cref="Loop()" /> and stays there until the
    /// app is finished. At this point <see cref="CleanupWindows" /> is called, followed by
    /// <see cref="Cleanup" />.
    /// </summary>
    public void Run()
    {
        try
        {
            Loop();
            CleanupWindows();
        }
        catch ( Exception e )
        {
            throw e is SystemException ? e : new GdxRuntimeException( e );
        }
        finally
        {
            Cleanup();
        }
    }

    /// <summary>
    /// Framework Main Loop.
    /// </summary>
    protected void Loop()
    {
        Logger.Divider();
        Logger.Debug( "Entering Framework Loop", true );
        Logger.Divider();
        Logger.Divider();

        // ====================================================================
        
        List< DesktopGLWindow > closedWindows = [ ];

        while ( _running && ( Windows.Count > 0 ) )
        {
            var haveWindowsRendered = false;
            var targetFramerate     = UNINITIALISED_FRAMERATE;

            closedWindows.Clear();

            lock ( this )
            {
                // Update active windows.
                // SwapBuffers is called in window.Update().
                foreach ( var window in Windows )
                {
                    window.MakeCurrent();

                    CurrentWindow = window;

                    if ( targetFramerate == UNINITIALISED_FRAMERATE )
                    {
                        targetFramerate = window.AppConfig.ForegroundFPS;
                    }

                    lock ( LifecycleListeners )
                    {
                        haveWindowsRendered |= window.Update();
                    }

                    if ( window.ShouldClose() )
                    {
                        closedWindows.Add( window );
                    }
                }
            }

            bool shouldRequestRendering;

            lock ( Runnables )
            {
                shouldRequestRendering = Runnables.Count > 0;

                ExecutedRunnables.Clear();

                foreach ( var runnable in Runnables )
                {
                    ExecutedRunnables.Add( runnable );
                }

                Runnables.Clear();
            }

            // Handle all Runnables.
            foreach ( var runnable in ExecutedRunnables )
            {
                runnable();
            }

            if ( shouldRequestRendering )
            {
                // This section MUST follow Runnables execution so changes made by
                // Runnables are reflected in the following render.
                foreach ( var window in Windows )
                {
                    if ( !window.Graphics.ContinuousRendering )
                    {
                        window.RequestRendering();
                    }
                }
            }

            // Tidy up any closed windows
            foreach ( var window in closedWindows )
            {
                if ( Windows.Count == 1 )
                {
                    // Lifecycle listener methods have to be called before ApplicationListener
                    // methods. The application will be disposed when ALL windows have been
                    // disposed, which is the case, when there is only 1 window left, which is
                    // in the process of being disposed.
                    for ( var i = LifecycleListeners.Count - 1; i >= 0; i-- )
                    {
                        var l = LifecycleListeners[ i ];

                        l.Pause();
                        l.Dispose();
                    }

                    LifecycleListeners.Clear();
                }

                window.Dispose();
                Windows.Remove( window );
            }

            if ( !haveWindowsRendered )
            {
                // Sleep a few milliseconds in case no rendering was requested
                // with continuous rendering disabled.
                try
                {
                    Thread.Sleep( 1000 / AppConfig!.IdleFPS );
                }
                catch ( ThreadInterruptedException )
                {
                    // ignore
                }
            }
            else if ( targetFramerate > 0 )
            {
                // sleep as needed to meet the target framerate
                _sync?.SyncFrameRate( targetFramerate );
            }

            Engine.Api.Audio?.Update();

            // Glfw.SwapBuffers is called in window.Update().
            Glfw.PollEvents();
        }

        Logger.Debug( "Ending framework loop" );
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// </summary>
    /// <exception cref="GdxRuntimeException"></exception>
    public void InitialiseGlfw()
    {
        try
        {
            if ( !_glfwInitialised )
            {
                DesktopGLNativesLoader.Load(); //TODO: Is this still necessary?

                ErrorCallback();

                Glfw.SetErrorCallback( _errorCallback );
                Glfw.InitHint( InitHint.JoystickHatButtons, false );

                if ( !Glfw.Init() )
                {
                    Glfw.GetError( out var error );

                    Logger.Debug( $"Failed to initialise Glfw: {error}" );

                    Glfw.Terminate();

                    Environment.Exit( 1 );
                }

                _glfwInitialised = true;
            }
        }
        catch ( Exception e )
        {
            throw new ApplicationException( $"Failure in InitialiseGLFW() : {e}" );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private static void ErrorCallback()
    {
        _errorCallback = ( error, description ) =>
        {
            Logger.Warning( $"ErrorCode: {error}, {description}" );

            if ( error == ErrorCode.InvalidEnum )
            {
                Logger.Warning( "Invalid Error!!" );
            }
        };
    }

    /// <summary>
    /// Initialise the main Window <see cref="WindowHint" />s.
    /// </summary>
    /// <param name="config"> The <see cref="DesktopGLApplicationConfiguration"/> to use. </param>
    private void SetWindowHints( DesktopGLApplicationConfiguration config )
    {
        ArgumentNullException.ThrowIfNull( config );

        Glfw.DefaultWindowHints();

        Glfw.WindowHint( WindowHint.Visible, config.InitialVisibility );
        Glfw.WindowHint( WindowHint.Resizable, config.WindowResizable );
        Glfw.WindowHint( WindowHint.Maximized, config.WindowMaximized );
        Glfw.WindowHint( WindowHint.AutoIconify, config.AutoIconify );
        Glfw.WindowHint( WindowHint.Decorated, config.WindowDecorated );

        Glfw.WindowHint( WindowHint.RedBits, config.Red );
        Glfw.WindowHint( WindowHint.GreenBits, config.Green );
        Glfw.WindowHint( WindowHint.BlueBits, config.Blue );
        Glfw.WindowHint( WindowHint.AlphaBits, config.Alpha );
        Glfw.WindowHint( WindowHint.StencilBits, config.Stencil );
        Glfw.WindowHint( WindowHint.DepthBits, config.Depth );
        Glfw.WindowHint( WindowHint.Samples, config.Samples );

        OGLProfile = GLUtils.DEFAULT_OPENGL_PROFILE;

        if ( config.GLContextMajorVersion is > 0 )
        {
            Glfw.WindowHint( WindowHint.ContextVersionMajor, ( int )config.GLContextMajorVersion );
        }
        else
        {
            Glfw.WindowHint( WindowHint.ContextVersionMajor, GLUtils.DEFAULT_GL_MAJOR );
        }

        if ( config.GLContextMinorVersion is > 0 )
        {
            Glfw.WindowHint( WindowHint.ContextVersionMinor, ( int )config.GLContextMinorVersion );
        }
        else
        {
            Glfw.WindowHint( WindowHint.ContextVersionMinor, GLUtils.DEFAULT_GL_MINOR );
        }

        Glfw.WindowHint( WindowHint.OpenGLForwardCompat, GLUtils.DEFAULT_OPENGL_FORWARDCOMPAT );
        Glfw.WindowHint( WindowHint.OpenGLProfile, OGLProfile );
        Glfw.WindowHint( WindowHint.ClientAPI, GLUtils.DEFAULT_CLIENT_API );

        Glfw.WindowHint( WindowHint.DoubleBuffer, true );

        if ( config.TransparentFramebuffer )
        {
            Glfw.WindowHint( WindowHint.TransparentFramebuffer, true );
        }

        if ( config.Debug )
        {
            Glfw.WindowHint( WindowHint.OpenGLDebugContext, true );
        }
    }

    /// <inheritdoc />
    public IPreferences GetPreferences( string name )
    {
        if ( Preferences.ContainsKey( name ) )
        {
            return Preferences.Get( name )!;
        }

        IPreferences prefs = new DesktopGLPreferences( name );

        Preferences.Put( name, prefs );

        return prefs;
    }

    /// <inheritdoc />
    public Platform.ApplicationType AppType
    {
        get => Platform.ApplicationType.WindowsGL;
        set { }
    }

    /// <inheritdoc />
    public void PostRunnable( IRunnable.Runnable runnable )
    {
        lock ( Runnables )
        {
            Runnables.Add( runnable );
        }
    }

    /// <summary>
    /// Creates the input device for this application window.
    /// </summary>
    public virtual IDesktopGLInput CreateInput( DesktopGLWindow window )
    {
        return new DefaultDesktopGLInput( window );
    }

    /// <summary>
    /// Returns the Android API level on Android, the major OS version on iOS (5, 6, 7, ..),
    /// or 0 on the desktop.
    /// </summary>
    public virtual int GetVersion()
    {
        return 0;
    }

    /// <summary>
    /// Schedule an exit from the application. On android, this will cause a call to
    /// Pause() and Dispose() at the next opportunity. It will not immediately finish
    /// your application. On iOS this should be avoided in production as it breaks
    /// Apples guidelines
    /// </summary>
    public virtual void Exit()
    {
        _running = false;
    }

    /// <inheritdoc />
    public void AddLifecycleListener( ILifecycleListener listener )
    {
        lock ( LifecycleListeners )
        {
            LifecycleListeners.Add( listener );
        }
    }

    /// <inheritdoc />
    public void RemoveLifecycleListener( ILifecycleListener listener )
    {
        lock ( LifecycleListeners )
        {
            LifecycleListeners.Remove( listener );
        }
    }

    // ========================================================================
    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Cleans up, and disposes of, any windows that have been closed.
    /// </summary>
    protected void CleanupWindows()
    {
        Logger.Debug( $"LifeCycleListeners count: {LifecycleListeners.Count}" );
        Logger.Debug( $"Windows count: {Windows.Count}" );

        lock ( LifecycleListeners )
        {
            foreach ( var lifecycleListener in LifecycleListeners )
            {
                lifecycleListener.Pause();
                lifecycleListener.Dispose();
            }
        }

        foreach ( var window in Windows )
        {
            window.Dispose();
        }

        Windows.Clear();
    }

    // ========================================================================

    /// <inheritdoc />
    public void Cleanup()
    {
        DesktopGLCursor.DisposeSystemCursors();
        Engine.Api.Audio.Dispose();
        _errorCallback = null;

        Glfw.Terminate();
    }

    // ========================================================================

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose( true );

        GC.SuppressFinalize( this );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="disposing"></param>
    private void Dispose( bool disposing )
    {
        if ( !_disposed )
        {
            if ( disposing )
            {
                // Release managed resources here
            }

            _disposed = true;
        }
    }

    ~DesktopGLApplication()
    {
        Dispose( false );
    }

    // ========================================================================

    #region window creation handlers

    /// <summary>
    /// Creates a new <see cref="DesktopGLWindow" /> using the provided listener and
    /// <see cref="DesktopGLApplicationConfiguration" />.
    /// <para>
    /// This function only instantiates a <see cref="DesktopGLWindow" /> and
    /// returns immediately. The actual window creation is postponed with
    /// <see cref="DesktopGLApplication.PostRunnable(IRunnable.Runnable)" /> until after all
    /// existing windows are updated.
    /// </para>
    /// </summary>
    public DesktopGLWindow NewWindow( IApplicationListener listener, DesktopGLApplicationConfiguration windowConfig )
    {
        GdxRuntimeException.ThrowIfNull( AppConfig );

        AppConfig.SetWindowConfiguration( windowConfig );

        return CreateWindow( AppConfig, listener, 0 );
    }

    /// <summary>
    /// Creates a new <see cref="DesktopGLWindow" /> using the
    /// </summary>
    /// <param name="config"></param>
    /// <param name="listener"></param>
    /// <param name="sharedContext"></param>
    /// <returns></returns>
    public DesktopGLWindow CreateWindow( DesktopGLApplicationConfiguration config,
                                         IApplicationListener listener,
                                         long sharedContext )
    {
        // Create the manager for the main window
        var dglWindow = new DesktopGLWindow( listener, config, this );

        if ( sharedContext == 0 )
        {
            // the main window is created immediately
            dglWindow = CreateWindow( dglWindow, config, 0 );
        }
        else
        {
            // creation of additional windows is deferred to avoid GL context trouble
            PostRunnable( () =>
            {
                dglWindow = CreateWindow( dglWindow, config, sharedContext );
                Windows.Add( dglWindow );
            } );
        }

        return dglWindow;
    }

    /// <summary>
    /// </summary>
    /// <param name="dglWindow"></param>
    /// <param name="config"></param>
    /// <param name="sharedContext"></param>
    public DesktopGLWindow CreateWindow( DesktopGLWindow? dglWindow,
                                         DesktopGLApplicationConfiguration config,
                                         long sharedContext )
    {
        ArgumentNullException.ThrowIfNull( dglWindow );

        var windowHandle = CreateGlfwWindow( config, sharedContext );

        dglWindow.Create( windowHandle );
        dglWindow.SetVisible( config.InitialVisibility );

        // Clear the display buffers
        for ( var i = 0; i < 2; i++ )
        {
            Engine.GL.BindFramebuffer( IGL.GL_FRAMEBUFFER, 0 );
            Engine.GL.ClearColor( config.InitialBackgroundColor.R,
                                  config.InitialBackgroundColor.G,
                                  config.InitialBackgroundColor.B,
                                  config.InitialBackgroundColor.A );

            Engine.GL.Clear( IGL.GL_COLOR_BUFFER_BIT );
            Glfw.SwapBuffers( windowHandle );
        }

        // the call above to CreateGlfwWindow switches the OpenGL context to the
        // newly created window, ensure that the invariant "currentWindow is the
        // window with the current active OpenGL context" holds
        CurrentWindow?.MakeCurrent();

        return dglWindow;
    }

    /// <summary>
    /// </summary>
    /// <param name="config"></param>
    /// <param name="sharedContextWindow"></param>
    /// <returns></returns>
    /// <exception cref="GdxRuntimeException"></exception>
    private GLFW.Window CreateGlfwWindow( DesktopGLApplicationConfiguration config, long sharedContextWindow )
    {
        SetWindowHints( config );

        GLFW.Window? windowHandle;

        if ( config.FullscreenMode != null )
        {
            // Create a fullscreen window

            Glfw.WindowHint( WindowHint.RefreshRate, config.FullscreenMode.RefreshRate );

            windowHandle = Glfw.CreateWindow( config.FullscreenMode.Width,
                                              config.FullscreenMode.Height,
                                              config.Title ?? "",
                                              config.FullscreenMode.MonitorHandle,
                                              GLFW.Window.NULL );
        }
        else
        {
            // Create a 'windowed' window

            windowHandle = Glfw.CreateWindow( config.WindowWidth,
                                              config.WindowHeight,
                                              config.Title ?? "",
                                              GLFW.Monitor.NULL,
                                              GLFW.Window.NULL );
        }

        if ( windowHandle.Equals( null ) )
        {
            throw new NullReferenceException( "Failed to create window!" );
        }

        DesktopGLWindow.SetSizeLimits( windowHandle,
                                       config.WindowMinWidth,
                                       config.WindowMinHeight,
                                       config.WindowMaxWidth,
                                       config.WindowMaxHeight );

        if ( config.FullscreenMode == null )
        {
            if ( config is { WindowX: -1, WindowY: -1 } )
            {
                var windowWidth  = Math.Max( config.WindowWidth, config.WindowMinWidth );
                var windowHeight = Math.Max( config.WindowHeight, config.WindowMinHeight );

                if ( config.WindowMaxWidth > -1 )
                {
                    windowWidth = Math.Min( windowWidth, config.WindowMaxWidth );
                }

                if ( config.WindowMaxHeight > -1 )
                {
                    windowHeight = Math.Min( windowHeight, config.WindowMaxHeight );
                }

                var monitorHandle = Glfw.GetPrimaryMonitor();

                if ( config is { WindowMaximized: true, MaximizedMonitor: not null } )
                {
                    monitorHandle = config.MaximizedMonitor.MonitorHandle;
                }

                Glfw.GetMonitorWorkarea( monitorHandle, out var areaX, out var areaY, out var areaW, out var areaH );

                Glfw.SetWindowPos( windowHandle,
                                   ( areaX + ( areaW / 2 ) ) - ( windowWidth / 2 ),
                                   ( areaY + ( areaH / 2 ) ) - ( windowHeight / 2 ) );
            }
            else
            {
                Glfw.SetWindowPos( windowHandle, config.WindowX, config.WindowY );
            }

            if ( config.WindowMaximized )
            {
                Glfw.MaximizeWindow( windowHandle );
            }
        }

        if ( config.WindowIconPaths != null )
        {
            DesktopGLWindow.SetIcon( windowHandle, config.WindowIconPaths, config.WindowIconFileType );
        }

        Glfw.MakeContextCurrent( windowHandle );
        Glfw.SwapInterval( config.VSyncEnabled ? 1 : 0 );
        GLUtils.CreateCapabilities();

        GLVersion = new GLVersion( Platform.ApplicationType.WindowsGL, OGLProfile );

        if ( config.Debug )
        {
            GLDebugControl.EnableGLDebugOutput();
        }

        return windowHandle;
    }

    #endregion window creation handlers
}