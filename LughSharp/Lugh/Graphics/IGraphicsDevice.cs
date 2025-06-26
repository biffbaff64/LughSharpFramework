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

using LughSharp.Lugh.Graphics.Images;
using LughSharp.Lugh.Graphics.Utils;

namespace LughSharp.Lugh.Graphics;

/// <summary>
/// An interface representing the graphics context of the application.
/// This interface provides access to various graphics-related functionalities,
/// including OpenGL version information, buffer formats, window dimensions,
/// display modes, and cursor management.
/// <para>
/// Implementations of this interface are responsible for managing the
/// underlying graphics backend (e.g., OpenGL via GLFW) and providing
/// a consistent API for graphics operations within the LughSharp framework.
/// </para>
/// </summary>
[PublicAPI]
public partial interface IGraphicsDevice
{
    /// <summary>
    /// Gets or sets the OpenGL version information.
    /// </summary>
    GLVersion? GLVersion { get; set; }

    /// <summary>
    /// Gets the type of graphics backend being used.
    /// </summary>
    GraphicsBackend.BackendType GraphicsType { get; }

    /// <summary>
    /// Gets or sets the buffer config data (bits per pixel, depth, stencil, samples).
    /// </summary>
    FramebufferConfig BufferConfig { get; set; }

    /// <summary>
    /// Gets or sets the time span between the current frame and the last frame in seconds,
    /// with smoothing applied for more consistent updates.
    /// <para>
    /// This value is typically used for frame-rate independent animation and game logic.
    /// </para>
    /// </summary>
    float DeltaTime { get; set; }

    /// <summary>
    /// Gets the current width of the application window in pixels.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the current height of the application window in pixels.
    /// </summary>
    int Height { get; }

    /// <summary>
    /// Gets or sets the width of the back buffer in pixels.
    /// The back buffer is the off-screen buffer where rendering is performed
    /// before being presented to the screen.
    /// </summary>
    int BackBufferWidth { get; set; }

    /// <summary>
    /// Gets or sets the height of the back buffer in pixels.
    /// The back buffer is the off-screen buffer where rendering is performed
    /// before being presented to the screen.
    /// </summary>
    int BackBufferHeight { get; set; }

    /// <summary>
    /// Gets a value indicating whether the application is currently in fullscreen mode.
    /// </summary>
    bool IsFullscreen { get; }

    /// <summary>
    /// Gets or sets the current GLFW window context.
    /// <para>
    /// This property provides access to the underlying GLFW window handle,
    /// which might be needed for advanced GLFW operations.
    /// </para>
    /// </summary>
    Window CurrentContext { get; set; }

    /// <summary>
    /// Gets a value indicating whether continuous rendering is enabled.
    /// When enabled, the application will render frames continuously, typically
    /// driven by the system's refresh rate.
    /// <para>
    /// Disabling continuous rendering can be useful for saving power when
    /// the application doesn't need to be constantly redrawing (e.g., in
    /// non-interactive scenes). Rendering can then be triggered manually
    /// using <see cref="RequestRendering" />.
    /// </para>
    /// </summary>
    bool ContinuousRendering { get; }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Updates the current graphics state by calculating the frame delta time,
    /// updating frames per second (FPS), and incrementing the frame identifier.
    /// This method is typically called on each render loop iteration.
    /// </summary>
    void Update();

    /// <summary>
    /// Returns the amount of pixels per logical pixel (point).
    /// This is used for scaling UI elements and text to maintain visual consistency
    /// across different screen densities.
    /// </summary>
    /// <returns>The back buffer scale factor.</returns>
    float GetBackBufferScale();

    /// <summary>
    /// Returns the time span between the current frame and the last frame in seconds,
    /// without smoothing. This provides the raw delta time value as measured directly
    /// from the system timer, before any smoothing or averaging is applied.
    /// </summary>
    /// <returns>The raw delta time in seconds.</returns>
    float GetRawDeltaTime();

    /// <summary>
    /// This is a scaling factor for the Density Independent Pixel unit, following the
    /// convention where one DIP is one pixel on an approximately 160 dpi screen.
    /// <para>
    /// Thus on a 160dpi screen this density value will be 1; on a 120 dpi screen it would
    /// be 0.75; etc. This value is useful for converting DIP units to screen pixels
    /// and vice versa, ensuring UI elements are displayed at similar physical sizes
    /// across devices with different screen densities.
    /// </para>
    /// </summary>
    /// <returns>The Density Independent Pixel factor of the display.</returns>
    float GetDensity();

    /// <summary>
    /// Returns the left safe area inset in pixels.
    /// Safe area insets represent regions of the screen that might be obscured by
    /// system UI elements or device bezels (e.g., notches, rounded corners).
    /// </summary>
    /// <returns>The left safe area inset in pixels.</returns>
    int GetSafeInsetLeft();

    /// <summary>
    /// Returns the top safe area inset in pixels.
    /// Safe area insets represent regions of the screen that might be obscured by
    /// system UI elements or device bezels (e.g., notches, rounded corners).
    /// </summary>
    /// <returns>The top safe area inset in pixels.</returns>
    int GetSafeInsetTop();

    /// <summary>
    /// Returns the bottom safe area inset in pixels.
    /// Safe area insets represent regions of the screen that might be obscured by
    /// system UI elements or device bezels (e.g., notches, rounded corners).
    /// </summary>
    /// <returns>The bottom safe area inset in pixels.</returns>
    int GetSafeInsetBottom();

    /// <summary>
    /// Returns the right safe area inset in pixels.
    /// Safe area insets represent regions of the screen that might be obscured by
    /// system UI elements or device bezels (e.g., notches, rounded corners).
    /// </summary>
    /// <returns>The right safe area inset in pixels.</returns>
    int GetSafeInsetRight();

    /// <summary>
    /// Returns the current frame ID. The frame ID is typically incremented with each rendered frame.
    /// </summary>
    /// <returns>The current frame ID.</returns>
    long GetFrameID();

    /// <summary>
    /// Returns the current frames per second (FPS). This value is usually calculated as a moving
    /// average over a short period to provide a smooth FPS reading.
    /// </summary>
    /// <returns>The current frames per second.</returns>
    int GetFramesPerSecond();

    /// <summary>
    /// Gets the pixels per centimeter (PPC) in the X and Y directions for the display.
    /// PPC describes the physical pixel density of the screen.
    /// </summary>
    /// <returns>A tuple containing PPC in the X and Y directions (PpcXY).</returns>
    (float X, float Y) GetPpcXY();

    /// <summary>
    /// Gets the pixels per inch (PPI) in the X and Y directions for the display.
    /// PPI describes the physical pixel density of the screen, commonly used in display specifications.
    /// </summary>
    /// <returns>A tuple containing PPI in the X and Y directions (PpiXY).</returns>
    (float X, float Y) GetPpiXY();

    /// <summary>
    /// Whether an IGraphics implementation supports display mode changing or not.
    /// Display mode changing refers to the ability to switch between different
    /// screen resolutions, refresh rates, and fullscreen/windowed modes.
    /// </summary>
    /// <returns><c>true</c> if display mode changing is supported, <c>false</c> otherwise.</returns>
    bool SupportsDisplayModeChange();

    /// <summary>
    /// Checks if an OpenGL extension is supported by the current graphics context.
    /// OpenGL extensions provide access to advanced or vendor-specific graphics features.
    /// </summary>
    /// <param name="extension">The name of the OpenGL extension to check (e.g., "GL_ARB_texture_non_power_of_two").</param>
    /// <returns><c>true</c> if the extension is supported, <c>false</c> otherwise.</returns>
    bool SupportsExtension( string extension );

    /// <summary>
    /// Returns whether cubemap seamless feature is supported.
    /// </summary>
    bool SupportsCubeMapSeamless();

    /// <summary>
    /// Enable or disable cubemap seamless feature. Default is true if supported.
    /// Should only be called if this feature is supported.
    /// </summary>
    /// <param name="enable"></param>
    void EnableCubeMapSeamless( bool enable );

    /// <summary>
    /// Returns an array of all supported display modes for the current monitor.
    /// Display modes describe available screen resolutions, refresh rates, and bit depths.
    /// </summary>
    /// <returns>An array of <see cref="DisplayMode" /> objects representing supported display modes.</returns>
    DisplayMode[] GetDisplayModes();

    /// <summary>
    /// Gets the current display mode of the primary monitor.
    /// </summary>
    /// <returns>The current <see cref="DisplayMode" /> of the primary monitor.</returns>
    DisplayMode GetDisplayMode();

    /// <summary>
    /// Returns an array of all supported display modes for the specified monitor.
    /// Display modes describe available screen resolutions, refresh rates, and bit depths.
    /// </summary>
    /// <param name="monitor">The <see cref="GLFW.Monitor" /> to query for display modes.</param>
    /// <returns>An array of <see cref="DisplayMode" /> objects representing supported display modes for the specified monitor.</returns>
    DisplayMode[] GetDisplayModes( GLFW.Monitor monitor );

    /// <summary>
    /// Gets the current display mode of the specified monitor.
    /// </summary>
    /// <param name="monitor">The <see cref="GLFW.Monitor" /> to query for the display mode.</param>
    /// <returns>The current <see cref="DisplayMode" /> of the specified monitor.</returns>
    DisplayMode GetDisplayMode( GLFW.Monitor monitor );

    /// <summary>
    /// Sets the application to fullscreen mode using the specified display mode.
    /// </summary>
    /// <param name="displayMode">The <see cref="DisplayMode" /> to use for fullscreen mode.</param>
    /// <returns><c>true</c> if the display mode was successfully set, <c>false</c> otherwise.</returns>
    bool SetFullscreenMode( DisplayMode displayMode );

    /// <summary>
    /// Sets the application to windowed mode with the specified width and height.
    /// </summary>
    /// <param name="width">The desired width of the window in pixels.</param>
    /// <param name="height">The desired height of the window in pixels.</param>
    /// <returns><c>true</c> if the window mode was successfully set, <c>false</c> otherwise.</returns>
    bool SetWindowedMode( int width, int height );

    /// <summary>
    /// Sets whether the application window should be undecorated (i.e., borderless, no title bar).
    /// </summary>
    /// <param name="undecorated"><c>true</c> to make the window undecorated, <c>false</c> otherwise.</param>
    void SetUndecorated( bool undecorated );

    /// <summary>
    /// Sets whether the application window should be resizable by the user.
    /// </summary>
    /// <param name="resizable"><c>true</c> to make the window resizable, <c>false</c> otherwise.</param>
    void SetResizable( bool resizable );

    /// <summary>
    /// Updates the viewport with the specified dimensions and coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate of the viewport.</param>
    /// <param name="y">The y-coordinate of the viewport.</param>
    /// <param name="width">The width of the viewport.</param>
    /// <param name="height">The height of the viewport.</param>
    /// <param name="source"></param>
    void UpdateViewport( int x, int y, int width, int height, int source = 0 );

    /// <summary>
    /// Sets whether VSync (vertical synchronization) should be enabled.
    /// VSync synchronizes the application's frame rate with the monitor's refresh rate,
    /// preventing screen tearing but potentially limiting FPS.
    /// </summary>
    /// <param name="vsync"><c>true</c> to enable VSync, <c>false</c> to disable it.</param>
    void SetVSync( bool vsync );

    /// <summary>
    /// Sets the desired foreground FPS (frames per second) when continuous rendering is enabled.
    /// <para>
    /// Note that the actual FPS might be lower if the system cannot achieve the desired rate.
    /// Setting this value to 0 or a negative value disables foreground FPS limiting,
    /// allowing the application to render as fast as possible (limited only by VSync if enabled).
    /// </para>
    /// </summary>
    /// <param name="fps">The desired foreground FPS.</param>
    void SetForegroundFps( int fps );

    /// <summary>
    /// Requests a rendering frame to be performed.
    /// <para>
    /// This method is typically used when continuous rendering is disabled
    /// (<see cref="ContinuousRendering" /> is <c>false</c>) to manually trigger
    /// rendering updates on demand (e.g., in response to user input or game events).
    /// </para>
    /// </summary>
    void RequestRendering();

    /// <summary>
    /// Create a new cursor represented by the <see cref="Pixmap" />. The Pixmap must be
    /// in RGBA8888 format, Width &amp; height must be powers-of-two greater than zero (not
    /// necessarily equal) and of a certain minimum size (32x32 is a safe bet), and alpha
    /// transparency must be single-bit (i.e., 0x00 or 0xFF only).
    /// <para>
    /// This function returns a Cursor object that can be set as the system cursor
    /// by calling <see cref="SetCursor(ICursor)" />.
    /// </para>
    /// </summary>
    /// <param name="pixmap"> the mouse cursor image as a <see cref="Pixmap" />. </param>
    /// <param name="xHotspot">
    /// The x location of the hotspot pixel within the cursor image (origin top-left corner)
    /// </param>
    /// <param name="yHotspot">
    /// The y location of the hotspot pixel within the cursor image (origin top-left corner)
    /// </param>
    /// <returns>
    /// a cursor object that can be used by calling <see cref="SetCursor(ICursor)" /> or null
    /// if not supported
    /// </returns>
    ICursor NewCursor( Pixmap pixmap, int xHotspot, int yHotspot );

    /// <summary>
    /// Only viable on the lwjgl-backend and on the gwt-backend. Browsers that support
    /// cursor:url() and support the png format (the pixmap is converted to a data-url of
    /// type image/png) should also support custom cursors.
    /// <para>
    /// Will set the mouse cursor image to the image represented by the Cursor.
    /// It is recommended to call this function in the main render thread, and maximum one time per frame.
    /// </para>
    /// </summary>
    /// <param name="cursor">The mouse cursor as a <see cref="ICursor" /></param>
    void SetCursor( ICursor cursor );

    /// <summary>
    /// Sets one of the predefined <see cref="ICursor.SystemCursor" />s.
    /// This allows you to set standard system cursors like arrow, crosshair, hand, etc.
    /// </summary>
    /// <param name="systemCursor"> The system cursor to use. </param>
    void SetSystemCursor( ICursor.SystemCursor systemCursor );
}

// ============================================================================
// ============================================================================