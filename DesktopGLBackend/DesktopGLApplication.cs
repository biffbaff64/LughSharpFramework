// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Circa64 Software Projects / Richard Ikin.
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

using DesktopGLBackend.Audio;
using DesktopGLBackend.Input;
using DesktopGLBackend.Utils;
using DesktopGLBackend.Window;

using JetBrains.Annotations;

using LughSharp.Source;
using LughSharp.Source.Collections;
using LughSharp.Source.Graphics.OpenGL;
using LughSharp.Source.IO;
using LughSharp.Source.Utils;
using LughSharp.Source.Utils.Exceptions;
using LughSharp.Source.Utils.Logging;

using Monitor = DotGLFW.Monitor;
using Platform = LughSharp.Source.Platform;

namespace DesktopGLBackend;

/// <summary>
/// Creates, and manages, an application to for Windows OpenGL backends.
/// </summary>
[PublicAPI]
public class DesktopGLApplication : IApplication
{
    /// <summary>
    /// Persistant properties manager instance.
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

    public List< Action >        Runnables         { get; set; } = [ ];
    public List< Action >        ExecutedRunnables { get; set; } = [ ];
    public IClipboard?           Clipboard         { get; set; }
    public DotGLFW.OpenGLProfile OglProfile        { get; set; }
    public DesktopGLWindow?      CurrentWindow     { get; set; }

    // ========================================================================
    // ========================================================================

    private const int UninitialisedFramerate = -2;

    // ========================================================================

    private static   DotGLFW.GlfwErrorCallback? _errorCallback;
    private readonly Sync?                      _sync;
    private          bool                       _glfwInitialised;
    private          IPreferences               _prefs;
    private          bool                       _running = true;
    private          bool                       _disposed;

    /// <summary>
    /// Synchronization lock used to manage thread-safe access to the runnable actions
    /// queue, ensuring the proper execution order and avoiding concurrency issues.
    /// </summary>
    private readonly System.Threading.Lock _runnablesLock = new();

    /// <summary>
    /// A thread synchronization lock used to ensure safe access to the lifecycle
    /// listeners collection in a multithreaded environment.
    /// </summary>
    private readonly System.Threading.Lock _lifecycleListenersLock = new();

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates a new Desktop Gl Application using the provided <see cref="DesktopGLApplicationConfiguration"/>.
    /// </summary>
    /// <param name="listener"> The <see cref="IApplicationListener"/> to use. </param>
    /// <param name="config"> The <see cref="DesktopGLApplicationConfiguration"/> to use.</param>
    public DesktopGLApplication( IApplicationListener listener, DesktopGLApplicationConfiguration config )
    {
        // ====================================================================
        // ====================================================================
        // ESSENTIAL FIRST ACTIONS. DO NOT MOVE.
        //
        // This MUST be the first call, so that the Logger and Engine.App
        // global are initialised correctly.
        Engine.Initialise( this );

        // ====================================================================
        // ====================================================================

        // Enable GLProfiling in preferences
        _prefs = GetPreferences( "desktopgl.lugh.engine.preferences" );
        _prefs.PutBoolean( "GL Profiling", config.GLProfilingEnabled );
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
        // Note: Engine.Graphics and Engine.Input are set later, during window creation as
        // each window that is created will have its own instances.
        Engine.Audio = AudioManager.CreateAudio( AppConfig );
        Engine.Files = new Files();
        Engine.Net   = new DesktopGLNet( AppConfig );

        Clipboard = new DesktopGLClipboard();
        _sync     = new Sync();

        InitialiseGlfw();

        // The primary window has no context to share with, so it is created
        // immediately (see CreateWindow) and tracked here.
        Windows.Add( CreateWindow( AppConfig, listener, null ) );

        Engine.Graphics.SetBackend( Platform.ApplicationType.WindowsGL, OglProfile );
    }

    // ========================================================================

    /// <summary>
    /// The entry point for running code using this framework. At this point at least one
    /// window will have been created, Glfw will have been set up, and the framework properly
    /// initialised. This passes control to <see cref="Loop()"/> and stays there until the
    /// app is finished. At this point <see cref="CleanupWindows"/> is called, followed by
    /// <see cref="Cleanup"/>.
    /// </summary>
    public void Run()
    {
        try
        {
            Loop();
        }
        finally
        {
            CleanupWindows();
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

        // ====================================================================

        List< DesktopGLWindow > closedWindows = [ ];

        while ( _running && ( Windows.Count > 0 ) )
        {
            var haveWindowsRendered = false;
            int targetFramerate     = UninitialisedFramerate;

            closedWindows.Clear();

            lock ( this )
            {
                // Update active windows.
                // SwapBuffers is called in window.Update().
                foreach ( DesktopGLWindow window in Windows )
                {
                    window.MakeCurrent();

                    CurrentWindow = window;

                    if ( targetFramerate == UninitialisedFramerate )
                    {
                        targetFramerate = window.AppConfig.ForegroundFPS;
                    }

                    lock ( _lifecycleListenersLock )
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

            lock ( _runnablesLock )
            {
                shouldRequestRendering = Runnables.Count > 0;

                ExecutedRunnables.Clear();
                ExecutedRunnables.AddRange( Runnables );
                Runnables.Clear();
            }

            // Handle all Runnables.
            foreach ( Action runnable in ExecutedRunnables )
            {
                runnable.Invoke();
            }

            if ( shouldRequestRendering )
            {
                // This section MUST follow Runnables execution so changes made by
                // Runnables are reflected in the following render.
                foreach ( DesktopGLWindow window in Windows )
                {
                    if ( !window.Graphics.ContinuousRendering )
                    {
                        window.RequestRendering();
                    }
                }
            }

            // Tidy up any closed windows
            foreach ( DesktopGLWindow window in closedWindows )
            {
                if ( Windows.Count == 1 )
                {
                    // Lifecycle listener methods have to be called before ApplicationListener
                    // methods. The application will be disposed when ALL windows have been
                    // disposed, which is the case, when there is only 1 window left, which is
                    // in the process of being disposed.
                    for ( int i = LifecycleListeners.Count - 1; i >= 0; i-- )
                    {
                        ILifecycleListener l = LifecycleListeners[ i ];

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

            Engine.Audio?.Update();

            // Glfw.SwapBuffers is called in window.Update().
            DotGLFW.Glfw.PollEvents();
        }

        Logger.Debug( "Ending framework loop" );
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// </summary>
    /// <exception cref="LughRuntimeException"></exception>
    public void InitialiseGlfw()
    {
        try
        {
            if ( !_glfwInitialised )
            {
                _errorCallback = ( error, description ) =>
                                 {
                                     Logger.Error( $"ErrorCode: {error}, {description}" );

                                     if ( error == DotGLFW.ErrorCode.InvalidEnum )
                                     {
                                         Logger.Error( "Invalid Error!!" );
                                     }
                                 };

                DotGLFW.Glfw.SetErrorCallback( _errorCallback );
                DotGLFW.Glfw.InitHint( DotGLFW.InitHint.JoystickHatButtons, false );

                if ( !DotGLFW.Glfw.Init() )
                {
                    DotGLFW.Glfw.GetError( out string? error );

                    Logger.Error( $"Failed to initialise Glfw: {error}" );

                    DotGLFW.Glfw.Terminate();

                    Environment.Exit( 1 );
                }

                _glfwInitialised = true;
            }
        }
        catch ( Exception e )
        {
            throw new LughRuntimeException( $"Failure in InitialiseGLFW() : {e}" );
        }
    }

    /// <summary>
    /// Initialise the main Window <see cref="DotGLFW.WindowHint"/>s.
    /// </summary>
    /// <param name="config"> The <see cref="DesktopGLApplicationConfiguration"/> to use. </param>
    private void SetWindowHints( DesktopGLApplicationConfiguration config )
    {
        Guard.Against.Null( config );

        DotGLFW.Glfw.DefaultWindowHints();

        DotGLFW.Glfw.WindowHint( DotGLFW.WindowHint.Visible, config.InitialVisibility );
        DotGLFW.Glfw.WindowHint( DotGLFW.WindowHint.Resizable, config.WindowResizable );
        DotGLFW.Glfw.WindowHint( DotGLFW.WindowHint.Maximized, config.WindowMaximized );
        DotGLFW.Glfw.WindowHint( DotGLFW.WindowHint.AutoIconify, config.AutoIconify );
        DotGLFW.Glfw.WindowHint( DotGLFW.WindowHint.Decorated, config.WindowDecorated );

        DotGLFW.Glfw.WindowHint( DotGLFW.WindowHint.RedBits, config.Red );
        DotGLFW.Glfw.WindowHint( DotGLFW.WindowHint.GreenBits, config.Green );
        DotGLFW.Glfw.WindowHint( DotGLFW.WindowHint.BlueBits, config.Blue );
        DotGLFW.Glfw.WindowHint( DotGLFW.WindowHint.AlphaBits, config.Alpha );
        DotGLFW.Glfw.WindowHint( DotGLFW.WindowHint.StencilBits, config.Stencil );
        DotGLFW.Glfw.WindowHint( DotGLFW.WindowHint.DepthBits, config.Depth );
        DotGLFW.Glfw.WindowHint( DotGLFW.WindowHint.Samples, config.Samples );

        OglProfile = LughGL.DefaultOpenglProfile;

        DotGLFW.Glfw.WindowHint
            (
             DotGLFW.WindowHint.ContextVersionMajor,
             config.GLContextMajorVersion > 0
                 ? config.GLContextMajorVersion
                 : LughGL.DefaultGLMajor
            );

        DotGLFW.Glfw.WindowHint
            (
             DotGLFW.WindowHint.ContextVersionMinor,
             config.GLContextMinorVersion > 0
                 ? config.GLContextMinorVersion
                 : LughGL.DefaultGLMinor
            );

        DotGLFW.Glfw.WindowHint( DotGLFW.WindowHint.OpenGLForwardCompat, LughGL.DefaultOpenglForwardcompat );
        DotGLFW.Glfw.WindowHint( DotGLFW.WindowHint.OpenGLProfile, OglProfile );
        DotGLFW.Glfw.WindowHint( DotGLFW.WindowHint.ClientAPI, LughGL.DefaultClientApi );

        DotGLFW.Glfw.WindowHint( DotGLFW.WindowHint.DoubleBuffer, true );

        if ( config.TransparentFramebuffer )
        {
            DotGLFW.Glfw.WindowHint( DotGLFW.WindowHint.TransparentFramebuffer, true );
        }

        if ( config.Debug )
        {
            DotGLFW.Glfw.WindowHint( DotGLFW.WindowHint.OpenGLDebugContext, true );
        }
    }

    /// <summary>
    /// Returns the <see cref="IPreferences"/> instance of this Application. It can be
    /// used to store application settings across runs.
    /// </summary>
    /// <param name="name"> the name of the preferences, must be useable as a file name. </param>
    /// <returns> The preferences. </returns>
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

    /// <summary>
    /// What <see cref="Platform.ApplicationType"/> the application has.
    /// </summary>
    public Platform.ApplicationType AppType => Platform.ApplicationType.WindowsGL;

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
    public virtual void ApplicationExit()
    {
        _running = false;
    }

    /// <summary>
    /// Adds a new <see cref="ILifecycleListener"/> to the application. This can be
    /// used by extensions to hook into the lifecycle more easily.
    /// The <see cref="IApplicationListener"/> methods are sufficient for application
    /// level development.
    /// </summary>
    public void AddLifecycleListener( ILifecycleListener listener )
    {
        lock ( LifecycleListeners )
        {
            LifecycleListeners.Add( listener );
        }
    }

    /// <summary>
    /// Removes the specified <see cref="ILifecycleListener"/>
    /// </summary>
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
        lock ( LifecycleListeners )
        {
            foreach ( ILifecycleListener lifecycleListener in LifecycleListeners )
            {
                lifecycleListener.Pause();
                lifecycleListener.Dispose();
            }
        }

        foreach ( DesktopGLWindow window in Windows )
        {
            window.Dispose();
        }

        Windows.Clear();
    }

    // ========================================================================

    /// <summary>
    /// Cleanup everything before shutdown.
    /// </summary>
    public void Cleanup()
    {
        DesktopGLCursor.DisposeSystemCursors();
        Engine.Audio.Dispose();
        _errorCallback = null;

        DotGLFW.Glfw.Terminate();
    }

    // ========================================================================

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing,
    /// or resetting unmanaged resources.
    /// </summary>
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

    /// <summary>
    /// Allows an object to try to free resources and perform other cleanup operations
    /// before it is reclaimed by garbage collection.
    /// </summary>
    ~DesktopGLApplication()
    {
        Dispose( false );
    }

    // ========================================================================

    #region window creation handlers

    /// <summary>
    /// Creates a new <see cref="DesktopGLWindow"/> using the provided listener and
    /// <see cref="DesktopGLApplicationConfiguration"/>.
    /// <para>
    /// This function only instantiates a <see cref="DesktopGLWindow"/> and
    /// returns immediately. The actual window creation is postponed with
    /// <see cref="DesktopGLApplication.PostRunnable"/> until after all
    /// existing windows are updated.
    /// </para>
    /// </summary>
    public DesktopGLWindow NewWindow( IApplicationListener listener, DesktopGLApplicationConfiguration windowConfig )
    {
        Guard.Against.Null( AppConfig );

        if ( Windows.Count == 0 )
        {
            throw new LughRuntimeException( "Cannot create a new window before the primary window exists." );
        }

        AppConfig.SetWindowConfiguration( windowConfig );

        // Additional windows share the primary window's OpenGL context and are created
        // via the deferred path (a non-null shared context routes CreateWindow to defer
        // creation to the main loop and register the window in Windows itself).
        return CreateWindow( AppConfig, listener, Windows[ 0 ].GlfwWindow );
    }

    /// <summary>
    /// Creates a new <see cref="DesktopGLWindow"/> using the
    /// </summary>
    /// <param name="config"></param>
    /// <param name="listener"></param>
    /// <param name="sharedContext">
    /// The window whose OpenGL context this window should share, or <c>null</c>
    /// (or <see cref="DotGLFW.Window.NULL"/>) for the primary window, which shares
    /// no context and is created immediately.
    /// </param>
    /// <returns></returns>
    public DesktopGLWindow CreateWindow( DesktopGLApplicationConfiguration config,
                                         IApplicationListener listener,
                                         DotGLFW.Window? sharedContext )
    {
        // Create the manager for the main window
        var dglWindow = new DesktopGLWindow( listener, config, this );

        if ( ( sharedContext is null ) || ( sharedContext == DotGLFW.Window.NULL ) )
        {
            // the main window is created immediately
            dglWindow = CreateWindow( dglWindow, config, sharedContext );
        }
        else
        {
            // creation of additional windows is deferred to avoid GL context trouble
            // ReSharper disable once HeapView.ClosureAllocation
            // ReSharper disable once HeapView.DelegateAllocation
            PostRunnable
                ( () =>
                  {
                      dglWindow = CreateWindow( dglWindow, config, sharedContext );
                      Windows.Add( dglWindow );
                  }
                );
        }

        return dglWindow;
    }

    /// <summary>
    /// Posts an <see cref="Action"/> to the event queue.
    /// </summary>
    public void PostRunnable( Action runnable )
    {
        lock ( Runnables )
        {
            Runnables.Add( runnable );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="dglWindow"></param>
    /// <param name="config"></param>
    /// <param name="sharedContext"></param>
    public DesktopGLWindow CreateWindow( DesktopGLWindow? dglWindow,
                                         DesktopGLApplicationConfiguration config,
                                         DotGLFW.Window? sharedContext )
    {
        Guard.Against.Null( dglWindow );

        DotGLFW.Window windowHandle = CreateGlfwWindow( config, sharedContext );

        dglWindow.Create( windowHandle );
        dglWindow.SetVisible( config.InitialVisibility );

        // Clear the display buffers
        for ( var i = 0; i < 2; i++ )
        {
            Engine.GL.BindFramebuffer( IGL.GLFramebuffer, 0 );
            Engine.GL.ClearColor
                (
                 config.InitialBackgroundColor.R,
                 config.InitialBackgroundColor.G,
                 config.InitialBackgroundColor.B,
                 config.InitialBackgroundColor.A
                );

            Engine.GL.Clear( IGL.GLColorBufferBit );
            DotGLFW.Glfw.SwapBuffers( windowHandle );
        }

        // The call above to CreateGlfwWindow switches the OpenGL context to the
        // newly created window, so ensure that the invariant "currentWindow is the
        // window with the current active OpenGL context" holds.
        CurrentWindow?.MakeCurrent();

        return dglWindow;
    }

    /// <summary>
    /// </summary>
    /// <param name="config"></param>
    /// <param name="sharedContextWindow">
    /// The window whose OpenGL context the new window should share, or <c>null</c>
    /// to share no context.
    /// </param>
    /// <returns></returns>
    /// <exception cref="LughRuntimeException"></exception>
    private DotGLFW.Window CreateGlfwWindow( DesktopGLApplicationConfiguration config, DotGLFW.Window? sharedContextWindow )
    {
        SetWindowHints( config );

        DotGLFW.Window? windowHandle;

        // A null shared context means "share nothing" (the primary window).
        DotGLFW.Window shareWindow = sharedContextWindow ?? DotGLFW.Window.NULL;

        if ( config.FullscreenMode != null )
        {
            // Create a fullscreen window
            DotGLFW.Glfw.WindowHint( DotGLFW.WindowHint.RefreshRate, config.FullscreenMode.RefreshRate );

            windowHandle = DotGLFW.Glfw.CreateWindow
                (
                 config.FullscreenMode.Width,
                 config.FullscreenMode.Height,
                 config.Title ?? string.Empty,
                 config.FullscreenMode.MonitorHandle,
                 shareWindow
                );
        }
        else
        {
            // Create a 'windowed' window
            windowHandle = DotGLFW.Glfw.CreateWindow
                (
                 config.WindowWidth,
                 config.WindowHeight,
                 config.Title ?? string.Empty,
                 Monitor.NULL,
                 shareWindow
                );
        }

        if ( windowHandle.Equals( null ) )
        {
            throw new LughRuntimeException( "Failed to create window!" );
        }

        DesktopGLWindow.SetSizeLimits
            (
             windowHandle,
             config.WindowMinWidth,
             config.WindowMinHeight,
             config.WindowMaxWidth,
             config.WindowMaxHeight
            );

        if ( config.FullscreenMode == null )
        {
            if ( config is { WindowX: -1, WindowY: -1 } )
            {
                int windowWidth  = Math.Max( config.WindowWidth, config.WindowMinWidth );
                int windowHeight = Math.Max( config.WindowHeight, config.WindowMinHeight );

                if ( config.WindowMaxWidth > -1 )
                {
                    windowWidth = Math.Min( windowWidth, config.WindowMaxWidth );
                }

                if ( config.WindowMaxHeight > -1 )
                {
                    windowHeight = Math.Min( windowHeight, config.WindowMaxHeight );
                }

                Monitor? monitorHandle = DotGLFW.Glfw.GetPrimaryMonitor();

                if ( config is { WindowMaximized: true, MaximizedMonitor: not null } )
                {
                    monitorHandle = config.MaximizedMonitor.MonitorHandle;
                }

                DotGLFW.Glfw.GetMonitorWorkarea
                    (
                     monitorHandle,
                     out int areaX,
                     out int areaY,
                     out int areaW,
                     out int areaH
                    );

                DotGLFW.Glfw.SetWindowPos
                    (
                     windowHandle,
                     areaX + ( areaW / 2 ) - ( windowWidth / 2 ),
                     areaY + ( areaH / 2 ) - ( windowHeight / 2 )
                    );
            }
            else
            {
                DotGLFW.Glfw.SetWindowPos( windowHandle, config.WindowX, config.WindowY );
            }

            if ( config.WindowMaximized )
            {
                DotGLFW.Glfw.MaximizeWindow( windowHandle );
            }
        }

        if ( config.WindowIconPaths != null )
        {
            DesktopGLWindow.SetIcon( windowHandle, config.WindowIconPaths, config.WindowIconPathType );
        }

        DotGLFW.Glfw.MakeContextCurrent( windowHandle );
        DotGLFW.Glfw.SwapInterval( config.VSyncEnabled ? 1 : 0 );
        GLUtils.CreateCapabilities();

        if ( config.Debug )
        {
            GLDebugControl.EnableGLDebugOutput();
        }

        return windowHandle;
    }

    #endregion window creation handlers
}

// ============================================================================
// ============================================================================
