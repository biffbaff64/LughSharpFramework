﻿// ///////////////////////////////////////////////////////////////////////////////
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

namespace LughSharp.Lugh.Graphics.Utils;

[PublicAPI]
public enum HdpiMode
{
    /// <summary>
    /// Mouse coordinates, <see cref="IGraphicsDevice.Width" /> and <see cref="IGraphicsDevice.Height" />
    /// will return logical coordinates according to the system defined HDPI scaling.
    /// Rendering will be performed to a backbuffer at raw resolution. Use <see cref="HdpiUtils" />
    /// when calling <see cref="OpenGL.GLBindings.glScissor" /> or <see cref="OpenGL.GLBindings.glViewport" />
    /// which expect raw coordinates.
    /// </summary>
    Logical,

    /// <summary>
    /// Mouse coordinates, <see cref="IGraphicsDevice.Width" /> and <see cref="IGraphicsDevice.Height" />
    /// will return raw pixel coordinates irrespective of the system defined HDPI scaling.
    /// </summary>
    Pixels,
}

/// <summary>
/// To deal with HDPI monitors properly, use the glViewport and glScissor functions
/// of this class instead of directly calling OpenGL yourself. The logical coordinate
/// system provided by the operating system may not have the same resolution as the
/// actual drawing surface to which OpenGL draws, also known as the backbuffer. This
/// class will ensure, that you pass the correct values to OpenGL for any function
/// that expects backbuffer coordinates instead of logical coordinates.
/// </summary>
[PublicAPI]
public class HdpiUtils
{
    private static HdpiMode _mode = HdpiMode.Logical;

    /// <summary>
    /// Allows applications to override HDPI coordinate conversion for glViewport and
    /// glScissor calls. This function can be used to ignore the default behavior, for
    /// example when rendering a UI stage to an off-screen framebuffer:
    /// <code>
    ///     HdpiUtils.SetMode(HdpiMode.Pixels);
    ///     fb.Begin();
    ///     stage.Draw();
    ///     fb.End();
    ///     HdpiUtils.SetMode(HdpiMode.Logical);
    /// </code>
    /// </summary>
    /// <param name="mode">
    /// set to HdpiMode.Pixels to ignore HDPI conversion for glViewport and glScissor functions
    /// </param>
    public static void SetMode( HdpiMode mode )
    {
        _mode = mode;
    }

    /// <summary>
    /// Calls <see cref="OpenGL.GLBindings.Scissor(int, int, int, int)" />, expecting the
    /// coordinates and sizes given in logical coordinates and automatically
    /// converts them to backbuffer coordinates, which may be bigger on HDPI screens.
    /// </summary>
    public static void GLScissor( int x, int y, int width, int height )
    {
        if ( ( _mode == HdpiMode.Logical )
             && ( ( Api.Graphics.Width != Api.Graphics.BackBufferWidth )
                  || ( Api.Graphics.Height != Api.Graphics.BackBufferHeight ) ) )
        {
            GL.Scissor( ToBackBufferX( x ),
                        ToBackBufferY( y ),
                        ToBackBufferX( width ),
                        ToBackBufferY( height ) );
        }
        else
        {
            GL.Scissor( x, y, width, height );
        }
    }

    /// <summary>
    /// Calls <see cref="OpenGL.GLBindings.Viewport(int, int, int, int)" />, expecting
    /// the coordinates and sizes given in logical coordinates and automatically
    /// converts them to backbuffer coordinates, which may be bigger on HDPI screens.
    /// </summary>
    public static void GLViewport( int x, int y, int width, int height )
    {
        if ( ( _mode == HdpiMode.Logical )
             && ( ( Api.Graphics.Width != Api.Graphics.BackBufferWidth )
                  || ( Api.Graphics.Height != Api.Graphics.BackBufferHeight ) ) )
        {
            Api.Graphics.UpdateViewport( ToBackBufferX( x ),
                                         ToBackBufferY( y ),
                                         ToBackBufferX( width ),
                                         ToBackBufferY( height ),
                                         3 );
        }
        else
        {
            Api.Graphics.UpdateViewport( x, y, width, height, 4 );
        }
    }

    /// <summary>
    /// Converts an x-coordinate given in backbuffer coordinates to
    /// logical screen coordinates.
    /// </summary>
    public static int ToLogicalX( int backBufferX )
    {
        return ( int )( ( backBufferX * Api.Graphics.Width ) / ( float )Api.Graphics.BackBufferWidth );
    }

    /// <summary>
    /// Converts a y-coordinate given in backbuffer coordinates to
    /// logical screen coordinates
    /// </summary>
    public static int ToLogicalY( int backBufferY )
    {
        return ( int )( ( backBufferY * Api.Graphics.Height ) / ( float )Api.Graphics.BackBufferHeight );
    }

    /// <summary>
    /// Converts an x-coordinate given in logical screen coordinates to
    /// backbuffer coordinates.
    /// </summary>
    public static int ToBackBufferX( int logicalX )
    {
        return ( int )( ( logicalX * Api.Graphics.BackBufferWidth ) / ( float )Api.Graphics.Width );
    }

    /// <summary>
    /// Convers a y-coordinate given in backbuffer coordinates to
    /// logical screen coordinates
    /// </summary>
    public static int ToBackBufferY( int logicalY )
    {
        return ( int )( ( logicalY * Api.Graphics.BackBufferHeight ) / ( float )Api.Graphics.Height );
    }
}