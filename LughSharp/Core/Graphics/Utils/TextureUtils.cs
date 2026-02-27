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

using JetBrains.Annotations;

using LughSharp.Core.Graphics.OpenGL.Enums;
using LughSharp.Core.Main;
using LughSharp.Core.Utils.Logging;

namespace LughSharp.Core.Graphics.Utils;

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
        Engine.GL.GetIntegerv( ( int )GLParameter.ActiveTexture, out int activeTex );
        Engine.GL.GetIntegerv( ( int )GLParameter.TextureBinding2D, out int prevBinding );

        // Ensure we are inspecting the right object on the active unit
        Engine.GL.BindTexture( TextureTarget.Texture2D, textureId );

        Engine.GL.GetTexLevelParameteriv( TextureTarget.Texture2D, 0, TextureParameter.TextureWidth, out int w );
        Engine.GL.GetTexLevelParameteriv( TextureTarget.Texture2D, 0, TextureParameter.TextureHeight, out int h );
        Engine.GL.GetTexLevelParameteriv( TextureTarget.Texture2D,
                                          0,
                                          TextureParameter.TextureInternalFormat,
                                          out int internalFmt );
        Engine.GL.GetTexParameteriv( ( int )TextureTarget.Texture2D,
                                     ( int )TextureParameter.TextureMaxLevel,
                                     out int maxLevel );
        Engine.GL.GetTexParameteriv( ( int )TextureTarget.Texture2D,
                                     ( int )TextureParameter.MinFilter,
                                     out int minFilter );
        Engine.GL.GetTexParameteriv( ( int )TextureTarget.Texture2D,
                                     ( int )TextureParameter.MagFilter,
                                     out int magFilter );

        Logger.Debug( $"[Tex {textureId}] L0 Size: {w}x{h}, InternalFormat: 0x{internalFmt:X}" );
        Logger.Debug( $"[Tex {textureId}] MinFilter: 0x{minFilter:X}, MagFilter: 0x{magFilter:X}, MaxLevel: {maxLevel}" );
        Logger.Divider();

        // Restore state
        Engine.GL.BindTexture( TextureTarget.Texture2D, ( uint )prevBinding );
        Engine.GL.ActiveTexture( ( TextureUnit )activeTex );
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
        uint textureHandle = Engine.GL.GenTexture();

        Engine.GL.BindTexture( TextureTarget.Texture2D, textureHandle );

        // Use immutable texture storage if supported
        if ( Engine.GL.GetOpenGLVersion().major >= 4 )
        {
            Engine.GL.TexStorage2D( ( int )TextureTarget.Texture2D, 1, ( int )internalFormat, width, height );
        }
        else
        {
            Engine.GL.TexImage2D( ( int )TextureTarget.Texture2D,
                                  0,
                                  ( int )internalFormat,
                                  width,
                                  height,
                                  0,
                                  ( int )GLPixelFormat.Rgba,
                                  ( int )PixelType.UnsignedByte );
        }

        return textureHandle;
    }
}

// ========================================================================
// ========================================================================