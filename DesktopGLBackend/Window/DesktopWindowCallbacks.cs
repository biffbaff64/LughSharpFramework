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

namespace DesktopGLBackend.Window;

public partial class DesktopGLWindow
{
    /// <summary>
    /// Callback method invoked when the focus state of the associated window changes.
    /// </summary>
    /// <param name="windowHandle">The handle to the GLFW window whose focus state has changed.</param>
    /// <param name="focused">
    /// A boolean indicating the new focus state; true if the window gained focus,
    /// otherwise false.
    /// </param>
    public void GdxFocusCallback( GLFW.Window windowHandle, bool focused )
    {
        if ( WindowListener != null )
        {
            if ( focused )
            {
                WindowListener.FocusGained();
            }
            else
            {
                WindowListener.FocusLost();
            }

            _focused = focused;
        }
    }

    /// <summary>
    /// Callback method invoked when the iconification state of the associated window changes.
    /// </summary>
    /// <param name="windowHandle">The handle to the GLFW window whose iconification state has changed.</param>
    /// <param name="iconified">
    /// A boolean indicating the new iconification state; true if the window was minimized,
    /// otherwise false.
    /// </param>
    public void GdxIconifyCallback( GLFW.Window windowHandle, bool iconified )
    {
        WindowListener?.Iconified( iconified );

        _iconified = iconified;

        if ( iconified )
        {
            ApplicationListener.Pause();
        }
        else
        {
            ApplicationListener.Resume();
        }
    }

    /// <summary>
    /// Callback method invoked when the maximization state of the associated window changes.
    /// </summary>
    /// <param name="windowHandle">The handle to the GLFW window whose maximization state has changed.</param>
    /// <param name="maximized">
    /// A boolean indicating the new maximization state; true if the window is maximized,
    /// otherwise false.
    /// </param>
    public void GdxMaximizeCallback( GLFW.Window windowHandle, bool maximized )
    {
        WindowListener?.Maximized( maximized );
    }

    /// <summary>
    /// Callback method invoked when a close request is made for the associated GLFW window.
    /// </summary>
    /// <param name="windowHandle">
    /// The handle to the GLFW window for which the close request is received.
    /// </param>
    public void GdxWindowCloseCallback( GLFW.Window windowHandle )
    {
        if ( WindowListener != null )
        {
            if ( !WindowListener.CloseRequested() )
            {
                Glfw.SetWindowShouldClose( windowHandle, false );
            }
        }
    }

    /// <summary>
    /// Callback method invoked when files are dropped onto the associated window.
    /// </summary>
    /// <param name="window">The handle to the GLFW window on which files were dropped.</param>
    /// <param name="paths">An array of file paths representing the dropped files.</param>
    public void GdxDropCallback( GLFW.Window window, string[] paths )
    {
        var files = new string[ paths.Length ];

        Array.Copy( paths, 0, files, 0, paths.Length );

        WindowListener?.FilesDropped( files );
    }

    /// <summary>
    /// Callback method invoked when the associated window requires a refresh,
    /// typically used to redraw or re-render the window's content.
    /// </summary>
    /// <param name="window">The handle to the GLFW window that requires a refresh.</param>
    public void GdxRefreshCallback( GLFW.Window window )
    {
        WindowListener?.RefreshRequested();
    }
}