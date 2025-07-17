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

using DesktopGLBackend.Core;
using DesktopGLBackend.Graphics;

using LughSharp.Lugh.Core;
using LughSharp.Lugh.Files;

namespace DesktopGLBackend.Window;

[PublicAPI]
public class DesktopGLWindowConfiguration : WindowConfiguration
{
    public DesktopGLGraphics.DesktopGLMonitor? MaximizedMonitor { get; set; }

    /// <summary>
    /// Sets the <see cref="IDesktopGLWindowListener" /> which will be informed about
    /// iconficiation, focus loss and window close events.
    /// </summary>
    public IDesktopGLWindowListener? WindowListener { get; set; }

    /// <summary>
    /// Sets the app to use fullscreen mode.
    /// <para>
    /// Use the static methods like <see cref="DesktopGLApplicationConfiguration.GetDisplayMode()" />
    /// on this class to enumerate connected monitors and their fullscreen display modes.
    /// </para>
    /// </summary>
    public DesktopGLGraphics.DesktopGLDisplayMode? FullscreenMode { get; set; }

    public bool IsFullscreenMode => FullscreenMode != null;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Sets this windows configuration settings.
    /// </summary>
    /// <param name="config">
    /// The window configuration data from which to initialise this window config.
    /// </param>
    public void SetWindowConfiguration( DesktopGLWindowConfiguration config )
    {
        base.SetWindowConfiguration( config );
        
        MaximizedMonitor = config.MaximizedMonitor;
        WindowListener   = config.WindowListener;
        FullscreenMode   = config.FullscreenMode;
    }
}