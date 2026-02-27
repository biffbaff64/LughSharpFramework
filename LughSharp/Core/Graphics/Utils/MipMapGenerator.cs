// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Circa64 Software Projects / Richard Ikin.
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

using JetBrains.Annotations;

using LughSharp.Core.Graphics.OpenGL;
using LughSharp.Core.Main;
using LughSharp.Core.Utils.Exceptions;

using Platform = LughSharp.Core.Main.Platform;

namespace LughSharp.Core.Graphics.Utils;

[PublicAPI]
public class MipMapGenerator
{
    // ========================================================================

    private MipMapGenerator()
    {
    }

    public static bool UseHwMipMap { get; set; } = true;

    /// <summary>
    /// Sets the image data of the <see cref="Texture"/> based on the <see cref="Pixmap"/>.
    /// The texture must be bound for this to work. If <code>disposePixmap</code> is true,
    /// the pixmap will be disposed at the end of the method.
    /// </summary>
    /// <param name="pixmap"> the Pixmap </param>
    /// <param name="textureWidth"></param>
    /// <param name="textureHeight"></param>
    public static void GenerateMipMap( Pixmap pixmap, int textureWidth, int textureHeight )
    {
        GenerateMipMap( IGL.GL_TEXTURE_2D, pixmap, textureWidth, textureHeight );
    }

    /// <summary>
    /// Sets the image data of the <see cref="Texture"/> based on the <see cref="Pixmap"/>.
    /// The texture must be bound for this to work. If <code>disposePixmap</code> is true,
    /// the pixmap will be disposed at the end of the method.
    /// </summary>
    public static void GenerateMipMap( int target, Pixmap pixmap, int textureWidth, int textureHeight )
    {
        if ( !UseHwMipMap )
        {
            GenerateMipMapCpu( target, pixmap, textureWidth, textureHeight );

            return;
        }

        if ( ( Engine.Api.App.AppType == Platform.ApplicationType.Android )
          || ( Engine.Api.App.AppType == Platform.ApplicationType.WebGL )
          || ( Engine.Api.App.AppType == Platform.ApplicationType.IOS ) )
        {
            GenerateMipMapGLES20( target, pixmap );
        }
        else
        {
            GenerateMipMapDesktop( target, pixmap, textureWidth, textureHeight );
        }
    }

    private static void GenerateMipMapGLES20( int target, Pixmap pixmap )
    {
        unsafe
        {
            fixed ( void* ptr = &pixmap.PixelData[ 0 ] )
            {
                Engine.GL.TexImage2D( target,
                                      0,
                                      pixmap.GLInternalPixelFormat,
                                      pixmap.Width,
                                      pixmap.Height,
                                      0,
                                      pixmap.GLPixelFormat,
                                      pixmap.GLDataType,
                                      ( IntPtr )ptr );
            }
        }

        Engine.GL.GenerateMipmap( target );
    }

    private static unsafe void GenerateMipMapDesktop( int target, Pixmap pixmap, int textureWidth, int textureHeight )
    {
        if ( Engine.Api.Graphics.SupportsExtension( "GL_ARB_framebuffer_object" )
          || Engine.Api.Graphics.SupportsExtension( "GL_EXT_framebuffer_object" ) )
        {
            fixed ( void* ptr = &pixmap.PixelData[ 0 ] )
            {
                Engine.GL.TexImage2D( target,
                                      0,
                                      pixmap.GLInternalPixelFormat,
                                      pixmap.Width,
                                      pixmap.Height,
                                      0,
                                      pixmap.GLPixelFormat,
                                      pixmap.GLDataType,
                                      ( IntPtr )ptr );
            }

            Engine.GL.GenerateMipmap( target );
        }
        else
        {
            GenerateMipMapCpu( target, pixmap, textureWidth, textureHeight );
        }
    }

    private static unsafe void GenerateMipMapCpu( int target, Pixmap pixmap, int textureWidth, int textureHeight )
    {
        fixed ( void* ptr = &pixmap.PixelData[ 0 ] )
        {
            Engine.GL.TexImage2D( target,
                                  0,
                                  pixmap.GLInternalPixelFormat,
                                  pixmap.Width,
                                  pixmap.Height,
                                  0,
                                  pixmap.GLPixelFormat,
                                  pixmap.GLDataType,
                                  ( IntPtr )ptr );
        }

        if ( textureWidth != textureHeight )
        {
            throw new RuntimeException( "texture width and height must be square when using mipmapping." );
        }

        int width  = pixmap.Width / 2;
        int height = pixmap.Height / 2;
        var level  = 1;

        while ( ( width > 0 ) && ( height > 0 ) )
        {
            var tmp = new Pixmap( width, height, pixmap.GetColorFormat() );

            tmp.Blending = Pixmap.BlendType.None;
            tmp.DrawPixmap( pixmap, 0, 0, pixmap.Width, pixmap.Height, 0, 0, width, height );

            if ( level > 1 )
            {
                pixmap.Dispose();
            }

            pixmap = tmp;

            fixed ( void* ptr = &pixmap.PixelData[ 0 ] )
            {
                Engine.GL.TexImage2D( target,
                                      level,
                                      pixmap.GLInternalPixelFormat,
                                      pixmap.Width,
                                      pixmap.Height,
                                      0,
                                      pixmap.GLPixelFormat,
                                      pixmap.GLDataType,
                                      ( IntPtr )ptr );
            }

            width  = pixmap.Width / 2;
            height = pixmap.Height / 2;
            level++;
        }
    }
}