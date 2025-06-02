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

using LughSharp.Lugh.Graphics.OpenGL.Enums;

using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace LughSharp.Lugh.Graphics.Utils;

[PublicAPI]
public class TextureUtils
{
    public static uint CreateTexture2D( int width, int height, PixelInternalFormat internalFormat )
    {
        var textureHandle = GL.GenTexture();

        GL.BindTexture( TextureTarget.Texture2D, textureHandle );

        // Use immutable texture storage if supported
        if ( GL.GetOpenGLVersion().major >= 4 )
        {
            GL.TexStorage2D( ( int )TextureTarget.Texture2D, 1, ( int )internalFormat, width, height );
        }
        else
        {
            GL.TexImage2D( ( int )TextureTarget.Texture2D,
                           0,
                           ( int )internalFormat,
                           width,
                           height,
                           0,
                           ( int )OpenGL.Enums.PixelFormat.Rgba,
                           ( int )OpenGL.Enums.PixelType.UnsignedByte,
                           0 );
        }

        return textureHandle;
    }
}

// ========================================================================
// ========================================================================