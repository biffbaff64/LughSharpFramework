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

namespace LughSharp.Lugh.Graphics;

[PublicAPI]
public abstract class GraphicsDevice : IGraphicsDevice
{
    public FramebufferConfig BufferConfig { get; set; } = null!;

    public int   LogicalWidth          { get; set; } = 0;
    public int   LogicalHeight         { get; set; } = 0;
    public Color WindowBackgroundColor { get; set; } = Color.Blue;
    public int   BackBufferWidth       { get; set; } = 0;
    public int   BackBufferHeight      { get; set; } = 0;

    // ========================================================================

    public virtual int   Width               { get; }
    public virtual int   Height              { get; }
    public virtual float DeltaTime           { get; set; }
    public virtual bool  ContinuousRendering { get; set; } = true;
    public virtual bool  IsFullscreen        { get; }

    public virtual AppVersion?                 GLVersion      { get; set; } = null!;
    public virtual GraphicsBackend.BackendType GraphicsType   { get; set; }
    public virtual GraphicsCapabilities        Capabilities   { get; set; } = null!;
    public virtual GLFormatChooser             FormatChooser  { get; set; } = null!;
    public virtual Window                      CurrentContext { get; set; } = null!;

    // ========================================================================

    /// <summary>
    /// Returns the time span between the current frame and the last frame
    /// in seconds, without smoothing.
    /// </summary>
    public virtual float GetRawDeltaTime()
    {
        return DeltaTime;
    }

    /// <summary>
    /// This is a scaling factor for the Density Independent Pixel unit, following the convention
    /// where one DIP is one pixel on an approximately 160 dpi screen. Thus on a 160dpi screen this
    /// density value will be 1; on a 120 dpi screen it would be .75; etc.
    /// </summary>
    /// <returns>the Density Independent Pixel factor of the display.</returns>
    public virtual float GetDensity()
    {
        return GetPpiXY().X / 160f;
    }

    /// <summary>
    /// Updates the current graphics state by calculating the frame delta time,
    /// updating frames per second (FPS), and incrementing the frame identifier.
    /// This method is typically called on each render loop iteration.
    /// </summary>
    public virtual void Update()
    {
    }

    /// <summary>
    /// Returns the amount of pixels per logical pixel (point).
    /// </summary>
    float IGraphicsDevice.GetBackBufferScale()
    {
        return BackBufferWidth / ( float )Width;
    }

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="windowHandle"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public abstract void RenderWindow( Window windowHandle, int width, int height );

    /// <summary>
    /// Updates the viewport with the specified dimensions and coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate of the viewport.</param>
    /// <param name="y">The y-coordinate of the viewport.</param>
    /// <param name="width">The width of the viewport.</param>
    /// <param name="height">The height of the viewport.</param>
    /// <param name="source"></param>
    public abstract void UpdateViewport( int x, int y, int width, int height, int source = 0 );

    /// <summary>
    /// Gets the current display mode of the primary monitor.
    /// </summary>
    /// <returns>The current <see cref="IGraphicsDevice.DisplayMode"/> of the primary monitor.</returns>
    public abstract IGraphicsDevice.DisplayMode GetDisplayMode();

    /// <summary>
    /// Gets the current display mode of the specified monitor.
    /// </summary>
    /// <param name="monitor">The <see cref="GLFW.Monitor"/> to query for the display mode.</param>
    /// <returns>The current <see cref="IGraphicsDevice.DisplayMode"/> of the specified monitor.</returns>
    public abstract IGraphicsDevice.DisplayMode GetDisplayMode( GLFW.Monitor monitor );

    /// <summary>
    /// Returns an array of all supported display modes for the current monitor.
    /// Display modes describe available screen resolutions, refresh rates, and bit depths.
    /// </summary>
    /// <returns>An array of <see cref="IGraphicsDevice.DisplayMode"/> objects representing supported display modes.</returns>
    public abstract IGraphicsDevice.DisplayMode[] GetDisplayModes();

    /// <summary>
    /// Returns an array of all supported display modes for the specified monitor.
    /// Display modes describe available screen resolutions, refresh rates, and bit depths.
    /// </summary>
    /// <param name="monitor">The <see cref="GLFW.Monitor"/> to query for display modes.</param>
    /// <returns>An array of <see cref="IGraphicsDevice.DisplayMode"/> objects representing supported display modes for the specified monitor.</returns>
    public abstract IGraphicsDevice.DisplayMode[] GetDisplayModes( GLFW.Monitor monitor );

    /// <summary>
    /// Retrieves the current High-DPI mode, which determines whether rendering coordinates
    /// are defined in logical units or in physical pixels.
    /// </summary>
    /// <returns>
    /// An <see cref="HdpiMode"/> value that specifies the High-DPI mode: either logical units or pixels.
    /// </returns>
    public abstract HdpiMode? GetHdpiMode();

    // ========================================================================
    // Window properties

    /// <summary>
    /// Sets the application to windowed mode with the specified width and height.
    /// </summary>
    /// <param name="width">The desired width of the window in pixels.</param>
    /// <param name="height">The desired height of the window in pixels.</param>
    /// <returns><c>true</c> if the window mode was successfully set, <c>false</c> otherwise.</returns>
    public abstract bool SetWindowedMode( int width, int height );

    /// <summary>
    /// Sets whether the application window should be undecorated (i.e., borderless, no title bar).
    /// </summary>
    /// <param name="undecorated"><c>true</c> to make the window undecorated, <c>false</c> otherwise.</param>
    public abstract void SetUndecorated( bool undecorated );

    /// <summary>
    /// Sets whether the application window should be resizable by the user.
    /// </summary>
    /// <param name="resizable"><c>true</c> to make the window resizable, <c>false</c> otherwise.</param>
    public abstract void SetResizable( bool resizable );

    /// <summary>
    /// Sets whether VSync (vertical synchronization) should be enabled.
    /// VSync synchronizes the application's frame rate with the monitor's refresh rate,
    /// preventing screen tearing but potentially limiting FPS.
    /// </summary>
    /// <param name="vsync"><c>true</c> to enable VSync, <c>false</c> to disable it.</param>
    public abstract void SetVSync( bool vsync );

    /// <summary>
    /// Sets the desired foreground FPS (frames per second) when continuous rendering is enabled.
    /// <para>
    /// Note that the actual FPS might be lower if the system cannot achieve the desired rate.
    /// Setting this value to 0 or a negative value disables foreground FPS limiting,
    /// allowing the application to render as fast as possible (limited only by VSync if enabled).
    /// </para>
    /// </summary>
    /// <param name="fps">The desired foreground FPS.</param>
    public abstract void SetForegroundFps( int fps );

    /// <summary>
    /// Sets the application to fullscreen mode using the specified display mode.
    /// </summary>
    /// <param name="displayMode">The <see cref="IGraphicsDevice.DisplayMode"/> to use for fullscreen mode.</param>
    /// <returns><c>true</c> if the display mode was successfully set, <c>false</c> otherwise.</returns>
    public abstract bool SetFullscreenMode( IGraphicsDevice.DisplayMode displayMode );

    // ========================================================================

    /// <summary>
    /// Checks if an OpenGL extension is supported by the current graphics context.
    /// OpenGL extensions provide access to advanced or vendor-specific graphics features.
    /// </summary>
    /// <param name="extension">The name of the OpenGL extension to check (e.g., "GL_ARB_texture_non_power_of_two").</param>
    /// <returns><c>true</c> if the extension is supported, <c>false</c> otherwise.</returns>
    public abstract bool SupportsExtension( string extension );

    /// <summary>
    /// Returns whether cubemap seamless feature is supported.
    /// </summary>
    public abstract bool SupportsCubeMapSeamless();

    /// <summary>
    /// Enable or disable cubemap seamless feature. Default is true if supported.
    /// Should only be called if this feature is supported.
    /// </summary>
    /// <param name="enable"></param>
    public abstract void EnableCubeMapSeamless( bool enable );

    /// <summary>
    /// Whether an IGraphics implementation supports display mode changing or not.
    /// Display mode changing refers to the ability to switch between different
    /// screen resolutions, refresh rates, and fullscreen/windowed modes.
    /// </summary>
    /// <returns><c>true</c> if display mode changing is supported, <c>false</c> otherwise.</returns>
    public abstract bool SupportsDisplayModeChange();

    /// <summary>
    /// Requests a rendering frame to be performed.
    /// <para>
    /// This method is typically used when continuous rendering is disabled
    /// (<see cref="IGraphicsDevice.ContinuousRendering"/> is <c>false</c>) to manually trigger
    /// rendering updates on demand (e.g., in response to user input or game events).
    /// </para>
    /// </summary>
    public abstract void RequestRendering();

    /// <summary>
    /// Returns the left safe area inset in pixels.
    /// Safe area insets represent regions of the screen that might be obscured by
    /// system UI elements or device bezels (e.g., notches, rounded corners).
    /// </summary>
    /// <returns>The left safe area inset in pixels.</returns>
    public abstract int GetSafeInsetLeft();

    /// <summary>
    /// Returns the top safe area inset in pixels.
    /// Safe area insets represent regions of the screen that might be obscured by
    /// system UI elements or device bezels (e.g., notches, rounded corners).
    /// </summary>
    /// <returns>The top safe area inset in pixels.</returns>
    public abstract int GetSafeInsetTop();

    /// <summary>
    /// Returns the bottom safe area inset in pixels.
    /// Safe area insets represent regions of the screen that might be obscured by
    /// system UI elements or device bezels (e.g., notches, rounded corners).
    /// </summary>
    /// <returns>The bottom safe area inset in pixels.</returns>
    public abstract int GetSafeInsetBottom();

    /// <summary>
    /// Returns the right safe area inset in pixels.
    /// Safe area insets represent regions of the screen that might be obscured by
    /// system UI elements or device bezels (e.g., notches, rounded corners).
    /// </summary>
    /// <returns>The right safe area inset in pixels.</returns>
    public abstract int GetSafeInsetRight();

    /// <summary>
    /// Returns the current frame ID. The frame ID is typically incremented with each rendered frame.
    /// </summary>
    /// <returns>The current frame ID.</returns>
    public abstract long GetFrameID();

    /// <summary>
    /// Returns the current frames per second (FPS). This value is usually calculated as a moving
    /// average over a short period to provide a smooth FPS reading.
    /// </summary>
    /// <returns>The current frames per second.</returns>
    public abstract int GetFramesPerSecond();

    /// <summary>
    /// Gets the pixels per centimeter (PPC) in the X and Y directions for the display.
    /// PPC describes the physical pixel density of the screen.
    /// </summary>
    /// <returns>A tuple containing PPC in the X and Y directions (PpcXY).</returns>
    public abstract (float X, float Y) GetPpcXY();

    /// <summary>
    /// Gets the pixels per inch (PPI) in the X and Y directions for the display.
    /// PPI describes the physical pixel density of the screen, commonly used in display specifications.
    /// </summary>
    /// <returns>A tuple containing PPI in the X and Y directions (PpiXY).</returns>
    public abstract (float X, float Y) GetPpiXY();

    // ========================================================================
    // Cursor / SystemCursor

    /// <summary>
    /// Create a new cursor represented by the <see cref="Pixmap"/>. The Pixmap must be
    /// in RGBA8888 format, Width &amp; height must be powers-of-two greater than zero (not
    /// necessarily equal) and of a certain minimum size (32x32 is a safe bet), and alpha
    /// transparency must be single-bit (i.e., 0x00 or 0xFF only).
    /// <para>
    /// This function returns a Cursor object that can be set as the system cursor
    /// by calling <see cref="IGraphicsDevice.SetCursor"/>.
    /// </para>
    /// </summary>
    /// <param name="pixmap"> the mouse cursor image as a <see cref="Pixmap"/>. </param>
    /// <param name="xHotspot">
    /// The x location of the hotspot pixel within the cursor image (origin top-left corner)
    /// </param>
    /// <param name="yHotspot">
    /// The y location of the hotspot pixel within the cursor image (origin top-left corner)
    /// </param>
    /// <returns>
    /// a cursor object that can be used by calling <see cref="IGraphicsDevice.SetCursor"/> or null
    /// if not supported
    /// </returns>
    public abstract ICursor NewCursor( Pixmap pixmap, int xHotspot, int yHotspot );

    /// <summary>
    /// Only viable on the lwjgl-backend and on the gwt-backend. Browsers that support
    /// cursor:url() and support the png format (the pixmap is converted to a data-url of
    /// type image/png) should also support custom cursors.
    /// <para>
    /// Will set the mouse cursor image to the image represented by the Cursor.
    /// It is recommended to call this function in the main render thread, and maximum one time per frame.
    /// </para>
    /// </summary>
    /// <param name="cursor">The mouse cursor as a <see cref="ICursor"/></param>
    public abstract void SetCursor( ICursor cursor );

    /// <summary>
    /// Sets one of the predefined <see cref="ICursor.SystemCursor"/>s.
    /// This allows you to set standard system cursors like arrow, crosshair, hand, etc.
    /// </summary>
    /// <param name="systemCursor"> The system cursor to use. </param>
    public abstract void SetSystemCursor( ICursor.SystemCursor systemCursor );
}

// ========================================================================
// ========================================================================