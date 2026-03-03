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
public enum PixelType
{
    UnsignedShort4444 = IGL.GLUnsignedShort4444,
    UnsignedShort5551 = IGL.GLUnsignedShort5551,
    UnsignedShort565  = IGL.GLUnsignedShort565,
    UnsignedByte      = IGL.GLUnsignedByte
}

[PublicAPI]
public enum GLPixelFormat
{
    Alpha          = IGL.GLAlpha,
    Luminance      = IGL.GLLuminance,
    LuminanceAlpha = IGL.GLLuminanceAlpha,
    Rgb565         = IGL.GLRGB565,
    Rgba4444       = IGL.GLRGBA4,
    Rgb            = IGL.GLRGB,
    Rgba           = IGL.GLRGBA,
    Red            = IGL.GLRed,
    Green          = IGL.GLGreen,
    Blue           = IGL.GLBlue
}

[PublicAPI]
public enum PixelInternalFormat
{
    Alpha          = IGL.GLAlpha,
    Rgb            = IGL.GLRGB,
    Rgba           = IGL.GLRGBA,
    Rgba8          = IGL.GLRGBA8,
    Luminance      = IGL.GLLuminance,
    LuminanceAlpha = IGL.GLLuminanceAlpha
}

[PublicAPI]
public enum PixelStoreParameter
{
    UnpackAlignment  = IGL.GLUnpackAlignment,
    PackAlignment    = IGL.GLPackAlignment,
    UnpackRowLength  = IGL.GLUnpackRowLength,
    PackRowLength    = IGL.GLPackRowLength,
    UnpackSkipPixels = IGL.GLUnpackSkipPixels,
    PackSkipPixels   = IGL.GLPackSkipPixels
}

// ========================================================================
// ========================================================================