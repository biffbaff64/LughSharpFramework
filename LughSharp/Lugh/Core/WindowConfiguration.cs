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

using LughSharp.Lugh.Files;
using LughSharp.Lugh.Graphics;

using Color = LughSharp.Lugh.Graphics.Color;

namespace LughSharp.Lugh.Core;

[PublicAPI]
public class WindowConfiguration
{
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
    /// whether the window will be visible on creation. (default true)
    /// </summary>
    public bool InitialVisibility { get; set; } = true;

    /// <summary>
    /// Sets whether to use vsync.
    /// <para>
    /// This setting can be changed anytime at runtime via <see cref="IGraphicsDevice.SetVSync(bool)" />.
    /// </para>
    /// <para>
    /// For multi-window applications, only one (the main) window should enable vsync. Otherwise,
    /// every window will wait for the vertical blank on swap individually, effectively cutting
    /// the frame rate to (refreshRate / numberOfWindows).
    /// </para>
    /// </summary>
    public bool VSyncEnabled { get; set; } = true;

    /// <summary>
    /// whether the windowed mode window is resizable (default true)
    /// </summary>
    public bool WindowResizable { get; set; } = true;

    /// <summary>
    /// whether the windowed mode window is decorated, i.e. displaying the title bars.
    /// (default true)
    /// </summary>
    public bool WindowDecorated { get; set; } = true;

    /// <summary>
    /// whether the window starts maximized. Ignored if the window is full screen.
    /// (default false)
    /// </summary>
    public bool WindowMaximized { get; set; } = false;

    /// <summary>
    /// whether the window should automatically iconify and restore previous video mode
    /// on input focus loss. (default false). Does nothing in windowed mode.
    /// (default false)
    /// </summary>
    public bool AutoIconify { get; set; } = false;

    /// <summary>
    /// Sets the initial background color. Defaults to black.
    /// </summary>
    public Color InitialBackgroundColor { get; set; } = Color.Black;

    /// <summary>
    /// Sets the window title. Defaults to empty string.
    /// </summary>
    public string? Title { get; set; } = "";
    
    // ========================================================================

    /// <summary>
    /// Configures the window settings based on the provided configuration.
    /// </summary>
    /// <param name="config">The window configuration to apply.</param>
    public void SetWindowConfiguration( WindowConfiguration config )
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
    /// One or more image paths, relative to the given <see cref="PathTypes" />. Must be JPEG,
    /// PNG, or BMP format. The one closest to the system's desired size will be scaled.
    /// Good sizes include 16x16, 32x32 and 48x48.
    /// </param>
    public void SetWindowIcon( PathTypes fileType, params string[] filePaths )
    {
        WindowIconFileType = fileType;
        WindowIconPaths    = filePaths;
    }
}

// ========================================================================
// ========================================================================