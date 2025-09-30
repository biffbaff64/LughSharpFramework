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

namespace LughSharp.Lugh.Graphics.Utils;

[PublicAPI]
public class TextureUtils
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="textureId"></param>
    public static void DebugTexture2D( int textureId )
    {
        DebugTexture2D( ( uint )textureId );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="textureId"></param>
    public static void DebugTexture2D( uint textureId )
    {
        // Save state
        GL.GetIntegerv( ( int )GLParameter.ActiveTexture, out var activeTex );
        GL.GetIntegerv( ( int )GLParameter.TextureBinding2D, out var prevBinding );

        // Ensure we are inspecting the right object on the active unit
        GL.BindTexture( TextureTarget.Texture2D, textureId );

        GL.GetTexLevelParameteriv( TextureTarget.Texture2D, 0, TextureParameter.TextureWidth, out var w );
        GL.GetTexLevelParameteriv( TextureTarget.Texture2D, 0, TextureParameter.TextureHeight, out var h );
        GL.GetTexLevelParameteriv( TextureTarget.Texture2D, 0, TextureParameter.TextureInternalFormat, out var internalFmt );
        GL.GetTexParameteriv( ( int )TextureTarget.Texture2D, ( int )TextureParameter.TextureMaxLevel, out var maxLevel );
        GL.GetTexParameteriv( ( int )TextureTarget.Texture2D, ( int )TextureParameter.MinFilter, out var minFilter );
        GL.GetTexParameteriv( ( int )TextureTarget.Texture2D, ( int )TextureParameter.MagFilter, out var magFilter );

        Logger.Debug( $"[Tex {textureId}] L0 Size: {w}x{h}, InternalFormat: 0x{internalFmt:X}" );
        Logger.Debug( $"[Tex {textureId}] MinFilter: 0x{minFilter:X}, MagFilter: 0x{magFilter:X}, MaxLevel: {maxLevel}" );
        Logger.Divider();

        // Restore state
        GL.BindTexture( TextureTarget.Texture2D, ( uint )prevBinding );
        GL.ActiveTexture( ( TextureUnit )activeTex );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="internalFormat"></param>
    /// <returns></returns>
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
                           ( int )PixelType.UnsignedByte,
                           0 );
        }

        return textureHandle;
    }
}

// ========================================================================
// ========================================================================