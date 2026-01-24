// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Red 7 Projects
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// /////////////////////////////////////////////////////////////////////////////

using JetBrains.Annotations;
using LughSharp.Core.Files;
using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Utils;
using LughSharp.Core.Utils.Logging;
using static LughSharp.Core.Graphics.GraphicsBackend;
using Color = LughSharp.Core.Graphics.Color;

namespace LughSharp.Core.Main;

[PublicAPI]
public class ApplicationConfiguration
{
    // ========================================================================
    // General Application Configuration
    // ========================================================================

    public BackendType     GraphicsBackend                { get; set; } = BackendType.OpenGL;
    public HdpiMode        HdpiMode                       { get; set; } = HdpiMode.Logical;
    public GLEmulationType GLEmulation                    { get; set; } = GLEmulationType.GL20;
    public string          PreferencesDirectory           { get; set; } = ".prefs/";
    public PathTypes       PreferencesFileType            { get; set; } = PathTypes.External;
    public bool            DisableAudio                   { get; set; }
    public int             AudioDeviceSimultaneousSources { get; set; } = 16;
    public int             AudioDeviceBufferSize          { get; set; } = 512;
    public int             AudioDeviceBufferCount         { get; set; } = 9;
    public bool            Debug                          { get; set; }
    public bool            TransparentFramebuffer         { get; set; }
    public int             Depth                          { get; set; } = 16;
    public int             Stencil                        { get; set; }
    public int             Samples                        { get; set; }
    public int             IdleFPS                        { get; set; } = 60;
    public int             ForegroundFPS                  { get; set; }
    public int             GLContextMajorVersion          { get; set; }
    public int             GLContextMinorVersion          { get; set; }
    public int             GLContextRevision              { get; set; }
    public int             Red                            { get; set; } = 8;
    public int             Green                          { get; set; } = 8;
    public int             Blue                           { get; set; } = 8;
    public int             Alpha                          { get; set; } = 8;
    public bool            PauseWhenLostFocus             { get; set; } = true;
    public bool            PauseWhenMinimized             { get; set; } = true;
    public bool            GLProfilingEnabled             { get; set; } = true;

    /// <summary>
    /// The maximum number of threads to use for network requests.
    /// Default is <see cref="int.MaxValue"/>.
    /// </summary>
    public int MaxNetThreads { get; set; } = int.MaxValue;

    // ========================================================================
    // Window Specific Configuration
    // ========================================================================

    protected const int DEFAULT_WINDOW_WIDTH      = 640;
    protected const int DEFAULT_WINDOW_HEIGHT     = 480;
    protected const int DEFAULT_WINDOW_X          = 80;
    protected const int DEFAULT_WINDOW_Y          = 80;
    protected const int DEFAULT_WINDOW_MIN_WIDTH  = 320;
    protected const int DEFAULT_WINDOW_MIN_HEIGHT = 240;
    protected const int DEFAULT_WINDOW_MAX_WIDTH  = 1280;
    protected const int DEFAULT_WINDOW_MAX_HEIGHT = 960;

    // ========================================================================

    public int       WindowX            { get; set; } = DEFAULT_WINDOW_X;
    public int       WindowY            { get; set; } = DEFAULT_WINDOW_Y;
    public int       WindowWidth        { get; set; } = DEFAULT_WINDOW_WIDTH;
    public int       WindowHeight       { get; set; } = DEFAULT_WINDOW_HEIGHT;
    public int       WindowMinWidth     { get; set; } = DEFAULT_WINDOW_MIN_WIDTH;
    public int       WindowMinHeight    { get; set; } = DEFAULT_WINDOW_MIN_HEIGHT;
    public int       WindowMaxWidth     { get; set; } = DEFAULT_WINDOW_MAX_WIDTH;
    public int       WindowMaxHeight    { get; set; } = DEFAULT_WINDOW_MAX_HEIGHT;
    public PathTypes WindowIconFileType { get; set; }
    public string[]? WindowIconPaths    { get; set; }

    /// <summary>
    /// Whether the window will be visible on creation. (default true)
    /// </summary>
    public bool InitialVisibility { get; set; } = true;

    /// <summary>
    /// Sets whether to use VSync.
    /// <para>
    /// This setting can be changed anytime at runtime via <see cref="IGraphicsDevice.SetVSync(bool)"/>.
    /// </para>
    /// <para>
    /// For multi-window applications, only one (the main) window should enable vsync. Otherwise,
    /// every window will wait for the vertical blank on swap individually, effectively cutting
    /// the frame rate to (refreshRate / numberOfWindows).
    /// </para>
    /// </summary>
    public bool VSyncEnabled { get; set; } = true;

    /// <summary>
    /// Whether the windowed mode window is resizable (default true)
    /// </summary>
    public bool WindowResizable { get; set; } = true;

    /// <summary>
    /// Whether the windowed mode window is decorated, i.e. displaying the title bars.
    /// (default true)
    /// </summary>
    public bool WindowDecorated { get; set; } = true;

    /// <summary>
    /// Whether the window starts maximized. Ignored if the window is full screen.
    /// (default false)
    /// </summary>
    public bool WindowMaximized { get; set; }

    /// <summary>
    /// Whether the window should automatically iconify and restore previous video mode
    /// on input focus loss. (default false). Does nothing in windowed mode.
    /// (default false)
    /// </summary>
    public bool AutoIconify { get; set; }

    /// <summary>
    /// The initial window background color. Defaults to blue.
    /// </summary>
    public Color InitialBackgroundColor { get; set; } = Color.Blue;

    /// <summary>
    /// Sets the window title. Defaults to empty string.
    /// </summary>
    public string? Title { get; set; } = "";

    // ========================================================================

    [PublicAPI]
    public enum GLEmulationType
    {
        AngleGles20,
        GL20,
        GL30,
        GL31,
        GL32,
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates, and returns, a new ApplicationConfiguration, using settings
    /// from the supplied ApplicationConfiguration object.
    /// </summary>
    public static ApplicationConfiguration Copy( ApplicationConfiguration config )
    {
        var copy = new ApplicationConfiguration();

        copy.Set( config );

        return copy;
    }

    /// <summary>
    /// Sets this ApplicationConfiguration settings, using settings from
    /// the supplied ApplicationConfiguration object.
    /// </summary>
    public void Set( ApplicationConfiguration config )
    {
        DisableAudio                   = config.DisableAudio;
        AudioDeviceSimultaneousSources = config.AudioDeviceSimultaneousSources;
        AudioDeviceBufferSize          = config.AudioDeviceBufferSize;
        AudioDeviceBufferCount         = config.AudioDeviceBufferCount;
        Debug                          = config.Debug;
        TransparentFramebuffer         = config.TransparentFramebuffer;
        HdpiMode                       = config.HdpiMode;
        Depth                          = config.Depth;
        Stencil                        = config.Stencil;
        Samples                        = config.Samples;
        IdleFPS                        = config.IdleFPS;
        ForegroundFPS                  = config.ForegroundFPS;
        GLContextMajorVersion          = config.GLContextMajorVersion;
        GLContextMinorVersion          = config.GLContextMinorVersion;
        GLContextRevision              = config.GLContextRevision;
        Red                            = config.Red;
        Green                          = config.Green;
        Blue                           = config.Blue;
        Alpha                          = config.Alpha;
        PreferencesDirectory           = config.PreferencesDirectory;
        PreferencesFileType            = config.PreferencesFileType;
        GLEmulation                    = config.GLEmulation;
        WindowX                        = config.WindowX;
        WindowY                        = config.WindowY;
        WindowWidth                    = config.WindowWidth;
        WindowHeight                   = config.WindowHeight;
        WindowMinWidth                 = config.WindowMinWidth;
        WindowMinHeight                = config.WindowMinHeight;
        WindowMaxWidth                 = config.WindowMaxWidth;
        WindowMaxHeight                = config.WindowMaxHeight;
        WindowIconFileType             = config.WindowIconFileType;
        WindowIconPaths                = config.WindowIconPaths;
        InitialVisibility              = config.InitialVisibility;
        VSyncEnabled                   = config.VSyncEnabled;
        WindowResizable                = config.WindowResizable;
        WindowDecorated                = config.WindowDecorated;
        WindowMaximized                = config.WindowMaximized;
        AutoIconify                    = config.AutoIconify;
        InitialBackgroundColor         = config.InitialBackgroundColor;
        Title                          = config.Title;
        GLProfilingEnabled             = config.GLProfilingEnabled;
        MaxNetThreads                  = config.MaxNetThreads;
    }

    /// <summary>
    /// Sets the audio device configuration.
    /// </summary>
    /// <param name="simultaniousSources">
    /// the maximum number of sources that can be played simultaniously (default 16)
    /// </param>
    /// <param name="bufferSize">the audio device buffer size in samples (default 512)</param>
    /// <param name="bufferCount">the audio device buffer count (default 9)</param>
    public void SetAudioConfig( int simultaniousSources, int bufferSize, int bufferCount )
    {
        AudioDeviceSimultaneousSources = simultaniousSources;
        AudioDeviceBufferSize          = bufferSize;
        AudioDeviceBufferCount         = bufferCount;
    }

    /// <summary>
    /// Sets which OpenGL version to use to emulate OpenGL ES. If the given major/minor version
    /// is not supported, the backend falls back to OpenGL ES 2.0 emulation through OpenGL 2.0.
    /// The default parameters for major and minor should be 3 and 2 respectively to be compatible
    /// with Mac OS X. Specifying major version 4 and minor version 2 will ensure that all OpenGL ES
    /// 3.0 features are supported. Note however that Mac OS X does only support 3.2.
    /// </summary>
    /// <param name="glVersion"> which OpenGL ES emulation version to use </param>
    /// <param name="glesMajorVersion"> OpenGL ES major version, use 3 as default </param>
    /// <param name="glesMinorVersion"> OpenGL ES minor version, use 2 as default </param>
    public void SetOpenGLEmulation( GLEmulationType glVersion, int glesMajorVersion, int glesMinorVersion )
    {
        GLEmulation           = glVersion;
        GLContextMajorVersion = glesMajorVersion;
        GLContextMinorVersion = glesMinorVersion;
    }

    /// <summary>
    /// Sets the bit depth of the color, depth and stencil buffer as well as multi-sampling.
    /// </summary>
    /// <param name="r"> red bits (default 8) </param>
    /// <param name="g"> green bits (default 8) </param>
    /// <param name="b"> blue bits (default 8) </param>
    /// <param name="a"> alpha bits (default 8) </param>
    /// <param name="depth"> depth bits (default 16) </param>
    /// <param name="stencil"> stencil bits (default 0) </param>
    /// <param name="samples"> MSAA samples (default 0) </param>
    public void SetBackBufferConfig( int r = 8,
                                     int g = 8,
                                     int b = 8,
                                     int a = 8,
                                     int depth = 16,
                                     int stencil = 0,
                                     int samples = 0 )
    {
        Red     = r;
        Green   = g;
        Blue    = b;
        Alpha   = a;
        Depth   = depth;
        Stencil = stencil;
        Samples = samples;
    }

    /// <summary>
    /// Sets the directory where <see cref="IPreferences"/> will be stored, as well as
    /// the file type to be used to store them. Defaults to "$USER_HOME/.prefs/"
    /// and <see cref="PathTypes"/>.
    /// </summary>
    public void SetPreferencesConfig( string preferencesDirectory, PathTypes preferencesFileType )
    {
        PreferencesDirectory = preferencesDirectory;
        PreferencesFileType  = preferencesFileType;
    }

    /// <summary>
    /// Sets the correct values for <see cref="GLContextMajorVersion"/> and
    /// <see cref="GLContextMinorVersion"/>.
    /// </summary>
    public void SetGLContextVersion( int major, int minor )
    {
        GLContextMajorVersion = major;
        GLContextMinorVersion = minor;
    }

    /// <summary>
    /// Gets the currently active display mode for the primary monitor.
    /// </summary>
    public virtual IGraphicsDevice.DisplayMode GetDisplayMode()
    {
        return GetDisplayMode( DotGLFW.Glfw.GetPrimaryMonitor() );
    }

    /// <summary>
    /// Gets the currterntly active display mode for the given monitor.
    /// </summary>
    public virtual IGraphicsDevice.DisplayMode GetDisplayMode( DotGLFW.Monitor monitor )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return the available <see cref="IGraphicsDevice.DisplayMode"/>s of the primary monitor
    /// </summary>
    public virtual IGraphicsDevice.DisplayMode[] GetDisplayModes()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns a list of the available <see cref="IGraphicsDevice.DisplayMode"/>s of the given monitor.
    /// </summary>
    public virtual IGraphicsDevice.DisplayMode[] GetDisplayModes( DotGLFW.Monitor monitor )
    {
        throw new NotImplementedException();
    }

    // ========================================================================

    /// <summary>
    /// Configures the window settings based on the provided configuration.
    /// </summary>
    /// <param name="config">The window configuration to apply.</param>
    public void SetWindowConfiguration( ApplicationConfiguration config )
    {
        WindowX            = config.WindowX;
        WindowY            = config.WindowY;
        WindowWidth        = config.WindowWidth;
        WindowHeight       = config.WindowHeight;
        WindowMinWidth     = config.WindowMinWidth;
        WindowMinHeight    = config.WindowMinHeight;
        WindowMaxWidth     = config.WindowMaxWidth;
        WindowMaxHeight    = config.WindowMaxHeight;
        WindowResizable    = config.WindowResizable;
        WindowDecorated    = config.WindowDecorated;
        WindowMaximized    = config.WindowMaximized;
        AutoIconify        = config.AutoIconify;
        WindowIconFileType = config.WindowIconFileType;

        if ( config.WindowIconPaths != null )
        {
            WindowIconPaths = new string[ config.WindowIconPaths.Length ];

            Array.Copy( config.WindowIconPaths, WindowIconPaths, config.WindowIconPaths.Length );
        }

        Title                  = config.Title;
        InitialBackgroundColor = config.InitialBackgroundColor;
        InitialVisibility      = config.InitialVisibility;
        VSyncEnabled           = config.VSyncEnabled;
    }

    /// <summary>
    /// Sets the app to use windowed mode.
    /// </summary>
    /// <param name="width"> the width of the window (default 640) </param>
    /// <param name="height">the height of the window (default 480) </param>
    public void SetWindowedMode( int width, int height )
    {
        WindowWidth  = width;
        WindowHeight = height;
    }

    /// <summary>
    /// Sets the position of the window in windowed mode.
    /// Default -1 for both coordinates for centered on primary monitor.
    /// </summary>
    public void SetWindowPosition( int x, int y )
    {
        WindowX = x;
        WindowY = y;
    }

    /// <summary>
    /// Sets minimum and maximum size limits for the window. If the window is full
    /// screen or not resizable, these limits are ignored. The default for all four
    /// parameters is -1, which means unrestricted.
    /// </summary>
    public void SetWindowSizeLimits( int minWidth, int minHeight, int maxWidth, int maxHeight )
    {
        WindowMinWidth  = minWidth;
        WindowMinHeight = minHeight;
        WindowMaxWidth  = maxWidth;
        WindowMaxHeight = maxHeight;
    }

    /// <summary>
    /// Sets the icon that will be used in the window's title bar. Has no effect in
    /// macOS, which doesn't use window icons.
    /// </summary>
    /// <param name="filePaths">
    /// One or more internal image paths. Must be JPEG, PNG, or BMP format. The one
    /// closest to the system's desired size will be scaled. Good sizes include 16x16,
    /// 32x32 and 48x48.
    /// </param>
    public void SetWindowIcon( params string[] filePaths )
    {
        SetWindowIcon( PathTypes.Internal, filePaths );
    }

    /// <summary>
    /// Sets the icon that will be used in the window's title bar.Has no effect in macOS,
    /// which doesn't use window icons.
    /// </summary>
    /// <param name="fileType"> The type of file handle the paths are relative to. </param>
    /// <param name="filePaths">
    /// One or more image paths, relative to the given <see cref="PathTypes"/>. Must be JPEG,
    /// PNG, or BMP format. The one closest to the system's desired size will be scaled.
    /// Good sizes include 16x16, 32x32 and 48x48.
    /// </param>
    public void SetWindowIcon( PathTypes fileType, params string[] filePaths )
    {
        WindowIconFileType = fileType;
        WindowIconPaths    = filePaths;
    }
}

// ============================================================================
// ============================================================================