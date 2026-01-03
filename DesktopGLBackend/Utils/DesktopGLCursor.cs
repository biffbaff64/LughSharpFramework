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

using LughSharp.Core.Graphics;
using LughSharp.Core.Utils.Exceptions;

namespace DesktopGLBackend.Utils;

[PublicAPI]
public class DesktopGLCursor : ICursor, IDisposable
{
    public DesktopGLWindow Window     { get; set; }
    public Pixmap          PixmapCopy { get; set; }
    public DotGLFW.Image   GlfwImage  { get; set; }
    public DotGLFW.Cursor  GlfwCursor { get; set; }

    // ========================================================================

    public static readonly List< DesktopGLCursor > Cursors = [ ];

    public static readonly Dictionary< ICursor.SystemCursor, DotGLFW.Cursor > SystemCursors = [ ];

    // ========================================================================
    // ========================================================================

    public DesktopGLCursor( DesktopGLWindow window, Pixmap pixmap, int xHotspot, int yHotspot )
    {
        Window = window;

        if ( pixmap.GetColorFormat() != LughFormat.RGBA8888 )
        {
            throw new GdxRuntimeException( "Cursor image pixmap should be in RGBA8888 format." );
        }

        if ( ( pixmap.Width & ( pixmap.Width - 1 ) ) != 0 )
        {
            throw new GdxRuntimeException( $"Cursor image pixmap width of {pixmap.Width} is "
                                         + $"not a power-of-two greater than zero." );
        }

        if ( ( pixmap.Height & ( pixmap.Height - 1 ) ) != 0 )
        {
            throw new GdxRuntimeException( $"Cursor image pixmap height of {pixmap.Height} "
                                         + $"is not a power-of-two greater than zero." );
        }

        if ( ( xHotspot < 0 ) || ( xHotspot >= pixmap.Width ) )
        {
            throw new GdxRuntimeException( $"xHotspot coordinate of {xHotspot} is not within "
                                         + $"image width bounds: [0, {pixmap.Width})." );
        }

        if ( ( yHotspot < 0 ) || ( yHotspot >= pixmap.Height ) )
        {
            throw new GdxRuntimeException( $"yHotspot coordinate of {yHotspot} is not within "
                                         + $"image height bounds: [0, {pixmap.Height})." );
        }

        PixmapCopy          = new Pixmap( pixmap.Width, pixmap.Height, LughFormat.RGBA8888 );
        PixmapCopy.Blending = Pixmap.BlendTypes.None;
        PixmapCopy.DrawPixmap( pixmap, 0, 0 );

        GlfwImage = new DotGLFW.Image
        {
            Pixels = PixmapCopy.PixelData,
            Width  = PixmapCopy.Width,
            Height = PixmapCopy.Height,
        };

        GlfwCursor = DotGLFW.Glfw.CreateCursor( GlfwImage, xHotspot, yHotspot );

        Cursors.Add( this );
    }

    /// <summary>
    /// Sets the system cursor for the given <see cref="DotGLFW.Window"/> to the
    /// cursor specified by the parameter <paramref name="systemCursor"/>.
    /// </summary>
    /// <param name="window"></param>
    /// <param name="systemCursor"></param>
    public static void SetSystemCursor( DotGLFW.Window window, ICursor.SystemCursor systemCursor )
    {
        //@formatter:off
        var glCursor = systemCursor switch
        {
            ICursor.SystemCursor.Ibeam            => DotGLFW.Glfw.CreateStandardCursor( DotGLFW.CursorShape.Ibeam ),
            ICursor.SystemCursor.Crosshair        => DotGLFW.Glfw.CreateStandardCursor( DotGLFW.CursorShape.Crosshair ),
            ICursor.SystemCursor.Hand             => DotGLFW.Glfw.CreateStandardCursor( DotGLFW.CursorShape.PointingHand ),
            ICursor.SystemCursor.HorizontalResize => DotGLFW.Glfw.CreateStandardCursor( DotGLFW.CursorShape.Hresize ),
            ICursor.SystemCursor.VerticalResize   => DotGLFW.Glfw.CreateStandardCursor( DotGLFW.CursorShape.Vresize ),
            var _                                 => DotGLFW.Glfw.CreateStandardCursor( DotGLFW.CursorShape.Arrow ),
        };
        //@formatter:on

        SystemCursors[ systemCursor ] = glCursor;

        DotGLFW.Glfw.SetCursor( window, glCursor );
    }

    // ========================================================================

    #region Dispose methods

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose( true );
        GC.SuppressFinalize( this );
    }

    protected virtual void Dispose( bool disposing )
    {
        if ( disposing )
        {
            if ( PixmapCopy == null )
            {
                throw new GdxRuntimeException( "Cursor already disposed" );
            }

            Cursors.Remove( this );
            PixmapCopy.Dispose();
            PixmapCopy = null!;
            GlfwImage  = null!;
            DotGLFW.Glfw.DestroyCursor( GlfwCursor );
        }
    }

    public static void DisposeGLCursor( DesktopGLWindow glWindow )
    {
        for ( var i = Cursors.Count - 1; i >= 0; i-- )
        {
            var cursor = Cursors[ i ];

            if ( cursor.Window.Equals( glWindow ) )
            {
                Cursors.RemoveAt( i );
            }
        }
    }

    public static void DisposeSystemCursors()
    {
        foreach ( var systemCursor in SystemCursors.Values )
        {
            DotGLFW.Glfw.DestroyCursor( systemCursor );
        }

        SystemCursors.Clear();
    }

    #endregion Dispose methods
}

// ============================================================================
// ============================================================================