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
using LughSharp.Lugh.Graphics;

namespace DesktopGLBackend;

/// <summary>
/// Configuration data and methods for the Desktop OpenGL backend.
/// </summary>
[PublicAPI]
public class DesktopGLApplicationConfiguration : ApplicationConfiguration
{
    public DesktopGLGraphics.DesktopGLMonitor? MaximizedMonitor { get; set; }

    /// <summary>
    /// Sets the <see cref="IDesktopGLWindowListener"/> which will be informed about
    /// iconficiation, focus loss and window close events.
    /// </summary>
    public IDesktopGLWindowListener? WindowListener { get; set; }

    /// <summary>
    /// Sets the app to use fullscreen mode.
    /// <para>
    /// Use the static methods like <see cref="GetDisplayMode"/>
    /// on this class to enumerate connected monitors and their fullscreen display modes.
    /// </para>
    /// </summary>
    public DesktopGLGraphics.DesktopGLDisplayMode? FullscreenMode { get; set; }

    public bool IsFullscreenMode => FullscreenMode != null;

    // ========================================================================

    /// <summary>
    /// Creates, and returns, a new DesktopApplicationConfiguration, using settings
    /// from the supplied DesktopApplicationConfiguratrion object.
    /// </summary>
    public static DesktopGLApplicationConfiguration Copy( DesktopGLApplicationConfiguration config )
    {
        var copy = new DesktopGLApplicationConfiguration();

        copy.Set( config );

        return copy;
    }

    /// <inheritdoc />
    public override IGraphicsDevice.DisplayMode GetDisplayMode( GLFW.Monitor monitor )
    {
        var videoMode = Glfw.GetVideoMode( monitor );

        return new DesktopGLGraphics.DesktopGLDisplayMode( monitor,
                                                           videoMode.Width,
                                                           videoMode.Height,
                                                           videoMode.RefreshRate,
                                                           videoMode.RedBits + videoMode.GreenBits + videoMode.BlueBits );
    }

    /// <inheritdoc />
    public override IGraphicsDevice.DisplayMode[] GetDisplayModes()
    {
        var videoModes = Glfw.GetVideoModes( Glfw.GetPrimaryMonitor() );

        var result = new IGraphicsDevice.DisplayMode[ videoModes.Length ];

        for ( var i = 0; i < result.Length; i++ )
        {
            var videoMode = videoModes[ i ];

            result[ i ] = new DesktopGLGraphics.DesktopGLDisplayMode( Glfw.GetPrimaryMonitor(),
                                                                      videoMode.Width,
                                                                      videoMode.Height,
                                                                      videoMode.RefreshRate,
                                                                      videoMode.RedBits + videoMode.GreenBits + videoMode.BlueBits );
        }

        return result;
    }

    /// <inheritdoc />
    public override IGraphicsDevice.DisplayMode[] GetDisplayModes( GLFW.Monitor monitor )
    {
        var videoModes = Glfw.GetVideoModes( monitor );

        var vmode = new IGraphicsDevice.DisplayMode[ videoModes.Length ];

        for ( var i = 0; i < vmode.Length; i++ )
        {
            var videoMode = videoModes[ i ];

            vmode[ i ] = new DesktopGLGraphics.DesktopGLDisplayMode( monitor,
                                                                     videoMode.Width,
                                                                     videoMode.Height,
                                                                     videoMode.RefreshRate,
                                                                     videoMode.RedBits + videoMode.GreenBits + videoMode.BlueBits );
        }

        return vmode;
    }

    /// <summary>
    /// Sets this windows configuration settings.
    /// </summary>
    /// <param name="config">
    /// The window configuration data from which to initialise this window config.
    /// </param>
    public void SetWindowConfiguration( DesktopGLApplicationConfiguration config )
    {
        base.SetWindowConfiguration( config );

        MaximizedMonitor = config.MaximizedMonitor;
        WindowListener   = config.WindowListener;
        FullscreenMode   = config.FullscreenMode;
    }
}

// ============================================================================
// ============================================================================
