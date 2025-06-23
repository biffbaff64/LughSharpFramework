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

[PublicAPI]
public abstract class AbstractGraphics : IGraphicsDevice
{
    public FramebufferConfig BufferConfig { get; set; } = null!;

    public int    LogicalWidth          { get; set; } = 0;
    public int    LogicalHeight         { get; set; } = 0;
    public Color  WindowBackgroundColor { get; set; } = Color.Blue;
    public int    BackBufferWidth       { get; set; } = 0;
    public int    BackBufferHeight      { get; set; } = 0;
    public Window CurrentContext        { get; set; } = null!;

    // ========================================================================

    public virtual int                         Width               { get; }
    public virtual int                         Height              { get; }
    public virtual float                       DeltaTime           { get; set; }
    public virtual GLVersion?                  GLVersion           { get; set; } = null!;
    public virtual GraphicsBackend.BackendType GraphicsType        { get; set; }
    public virtual bool                        ContinuousRendering { get; set; } = true;
    public virtual bool                        IsFullscreen        { get; }

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
    /// Returns the amount of pixels per logical pixel (point).
    /// </summary>
    float IGraphicsDevice.GetBackBufferScale()
    {
        return BackBufferWidth / ( float )Width;
    }

    // ========================================================================
    // 

    /// <inheritdoc />
    public abstract void Update();

    /// <inheritdoc />
    public abstract void UpdateViewport( int x, int y, int width, int height, int source = 0 );

    /// <inheritdoc />
    public abstract IGraphicsDevice.DisplayMode GetDisplayMode();

    /// <inheritdoc />
    public abstract IGraphicsDevice.DisplayMode GetDisplayMode( GLFW.Monitor monitor );

    /// <inheritdoc />
    public abstract IGraphicsDevice.DisplayMode[] GetDisplayModes();

    /// <inheritdoc />
    public abstract IGraphicsDevice.DisplayMode[] GetDisplayModes( GLFW.Monitor monitor );

    // ========================================================================
    // Window properties

    /// <inheritdoc />
    public abstract bool SetWindowedMode( int width, int height );

    /// <inheritdoc />
    public abstract void SetUndecorated( bool undecorated );

    /// <inheritdoc />
    public abstract void SetResizable( bool resizable );

    /// <inheritdoc />
    public abstract void SetVSync( bool vsync );

    /// <inheritdoc />
    public abstract void SetForegroundFps( int fps );

    /// <inheritdoc />
    public abstract bool SetFullscreenMode( IGraphicsDevice.DisplayMode displayMode );

    // ========================================================================
    // 

    /// <inheritdoc />
    public abstract bool SupportsExtension( string extension );

    /// <inheritdoc />
    public abstract bool SupportsCubeMapSeamless();

    /// <inheritdoc />
    public abstract void EnableCubeMapSeamless( bool enable );

    /// <inheritdoc />
    public abstract bool SupportsDisplayModeChange();

    /// <inheritdoc />
    public abstract void RequestRendering();

    /// <inheritdoc />
    public abstract int GetSafeInsetLeft();

    /// <inheritdoc />
    public abstract int GetSafeInsetTop();

    /// <inheritdoc />
    public abstract int GetSafeInsetBottom();

    /// <inheritdoc />
    public abstract int GetSafeInsetRight();

    /// <inheritdoc />
    public abstract long GetFrameID();

    /// <inheritdoc />
    public abstract int GetFramesPerSecond();

    /// <inheritdoc />
    public abstract (float X, float Y) GetPpcXY();

    /// <inheritdoc />
    public abstract (float X, float Y) GetPpiXY();

    // ========================================================================
    // Cursor / SystemCursor

    /// <inheritdoc />
    public abstract ICursor NewCursor( Pixmap pixmap, int xHotspot, int yHotspot );

    /// <inheritdoc />
    public abstract void SetCursor( ICursor cursor );

    /// <inheritdoc />
    public abstract void SetSystemCursor( ICursor.SystemCursor systemCursor );
}