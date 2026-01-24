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

namespace LughSharp.Core.Graphics.OpenGL.Enums;

[PublicAPI]
public enum PixelType : int
{
    UnsignedShort4444 = IGL.GL_UNSIGNED_SHORT_4_4_4_4,
    UnsignedShort5551 = IGL.GL_UNSIGNED_SHORT_5_5_5_1,
    UnsignedShort565  = IGL.GL_UNSIGNED_SHORT_5_6_5,
    UnsignedByte      = IGL.GL_UNSIGNED_BYTE,
}

[PublicAPI]
public enum GLPixelFormat : int
{
    Alpha          = IGL.GL_ALPHA,
    Luminance      = IGL.GL_LUMINANCE,
    LuminanceAlpha = IGL.GL_LUMINANCE_ALPHA,
    Rgb565         = IGL.GL_RGB565,
    Rgba4444       = IGL.GL_RGBA4,
    Rgb            = IGL.GL_RGB,
    Rgba           = IGL.GL_RGBA,
    Red            = IGL.GL_RED,
    Green          = IGL.GL_GREEN,
    Blue           = IGL.GL_BLUE,
}

[PublicAPI]
public enum PixelInternalFormat : int
{
    Alpha          = IGL.GL_ALPHA,
    Rgb            = IGL.GL_RGB,
    Rgba           = IGL.GL_RGBA,
    Rgba8          = IGL.GL_RGBA8,
    Luminance      = IGL.GL_LUMINANCE,
    LuminanceAlpha = IGL.GL_LUMINANCE_ALPHA,
}

[PublicAPI]
public enum PixelStoreParameter : int
{
    UnpackAlignment  = IGL.GL_UNPACK_ALIGNMENT,
    PackAlignment    = IGL.GL_PACK_ALIGNMENT,
    UnpackRowLength  = IGL.GL_UNPACK_ROW_LENGTH,
    PackRowLength    = IGL.GL_PACK_ROW_LENGTH,
    UnpackSkipPixels = IGL.GL_UNPACK_SKIP_PIXELS,
    PackSkipPixels   = IGL.GL_PACK_SKIP_PIXELS,
}

// ========================================================================
// ========================================================================