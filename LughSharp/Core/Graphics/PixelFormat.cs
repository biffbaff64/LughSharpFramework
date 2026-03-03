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

using System.Runtime.Versioning;

using JetBrains.Annotations;

using LughSharp.Core.Graphics.OpenGL;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Graphics;

/// <summary>
/// 
/// </summary>
[PublicAPI]
public class PixelFormat
{
    /// <summary>
    /// Returns the Internal Format (how GPU stores the data).
    /// Modern GL requires sized formats for consistency.
    /// </summary>
    public static int ToGLInternalFormat( int format )
    {
        return format switch
               {
                   // Color data should use SRGB for automatic gamma correction
                   LughFormat.RGB888   => IGL.GLSrgb8,
                   LughFormat.RGBA8888 => IGL.GLSrgb8Alpha8,

                   // Modern GL Core uses R8 for single channel. 
                   // We use Swizzle on the texture unit to treat Red as Alpha.
                   LughFormat.Alpha           => IGL.GLR8,
                   LughFormat.LuminanceAlpha => IGL.GLRg8, // R=Luminance, G=Alpha

                   // Legacy packed formats (Optional, but better as RGBA8)
                   LughFormat.RGB565   => IGL.GLRGB565,
                   LughFormat.RGBA4444 => IGL.GLRGBA4,

                   // ---------------------------

                   var _ => throw new RuntimeException( $"Unsupported internal format: {format}" )
               };
    }

    /// <summary>
    /// Returns the External Format (how the C# data is laid out).
    /// </summary>
    public static int ToGLFormat( int format )
    {
        return format switch
               {
                   LughFormat.RGB888          => IGL.GLRGB,
                   LughFormat.RGBA8888        => IGL.GLRGBA,
                   LughFormat.Alpha           => IGL.GLRed, // Data comes in as 1 channel
                   LughFormat.LuminanceAlpha => IGL.GLRg,  // Data comes in as 2 channels
                   LughFormat.RGB565          => IGL.GLRGB,
                   LughFormat.RGBA4444        => IGL.GLRGBA,

                   // ---------------------------

                   var _ => throw new RuntimeException( $"Unsupported data format: {format}" )
               };
    }

    /// <summary>
    /// Returns the OpenGL Data Type (the "bucket" size for each channel).
    /// </summary>
    public static int ToGLDataType( int format )
    {
        return format switch
               {
                   LughFormat.RGB565   => IGL.GLUnsignedShort565,
                   LughFormat.RGBA4444 => IGL.GLUnsignedShort4444,

                   // ---------------------------

                   // Standard 8-bit per channel
                   var _ => IGL.GLUnsignedByte
               };
    }

    /// <summary>
    /// Gets the number of bytes required for 1 pixel.
    /// Used for buffer allocation and stride calculations.
    /// </summary>
    public static int BytesPerPixel( int format )
    {
        return format switch
               {
                   LughFormat.Alpha           => 1,
                   LughFormat.LuminanceAlpha => 2,
                   LughFormat.RGB565          => 2,
                   LughFormat.RGBA4444        => 2,
                   LughFormat.RGB888          => 3,
                   LughFormat.RGBA8888        => 4,

                   // ---------------------------

                   var _ => throw new RuntimeException( $"Invalid format: {format}" )
               };
    }

    public static int GetAlignment( Pixmap pixmap )
    {
        //TODO:
        return GetAlignment( pixmap.Width * BytesPerPixel( pixmap.GetColorFormat() ) );
    }

    /// <summary>
    /// Computes the GL_UNPACK_ALIGNMENT. 
    /// Very important for textures with 3 bytes per pixel (RGB888).
    /// </summary>
    public static int GetAlignment( int rowStrideBytes )
    {
        if ( ( rowStrideBytes % 8 ) == 0 )
        {
            return 8;
        }

        if ( ( rowStrideBytes % 4 ) == 0 )
        {
            return 4;
        }

        return ( rowStrideBytes % 2 ) == 0 ? 2 : 1;
    }

    /// <summary>
    /// Retrieves the OpenGL data type name corresponding to the given format.
    /// </summary>
    /// <param name="format">The OpenGL format represented as an integer.</param>
    /// <returns>
    /// A string representing the OpenGL data type name. Returns "Invalid format: {format}"
    /// if the format is unrecognized.
    /// </returns>
    public static string GetGLTypeName( int format )
    {
        return format switch
               {
                   IGL.GLUnsignedByte          => "IGL.GL_UNSIGNED_BYTE",
                   IGL.GLUnsignedShort565   => "IGL.GL_UNSIGNED_SHORT_5_6_5",
                   IGL.GLUnsignedShort4444 => "IGL.GL_UNSIGNED_SHORT_4_4_4_4",

                   // ----------------------------------

                   var _ => $"Invalid format: {format}"
               };
    }

    /// <summary>
    /// Retrieves the OpenGL target name corresponding to the given target integer value.
    /// </summary>
    /// <param name="target">The integer value representing the OpenGL target.</param>
    /// <returns>
    /// A string representing the name of the OpenGL target, or "UNKNOWN TARGET: {target}" if not recognized.
    /// </returns>
    public static string GetGLTargetName( int target )
    {
        return target switch
               {
                   IGL.GLTexture1D                   => "GL_TEXTURE_1D",
                   IGL.GLTexture2D                   => "GL_TEXTURE_2D",
                   IGL.GLTexture3D                   => "GL_TEXTURE_3D",
                   IGL.GLTextureCubeMap             => "GL_TEXTURE_CUBE_MAP",
                   IGL.GLTextureRectangle            => "GL_TEXTURE_RECTANGLE",
                   IGL.GL_TEXTURE_BUFFER               => "GL_TEXTURE_BUFFER",
                   IGL.GLTexture2DArray             => "GL_TEXTURE_2D_ARRAY",
                   IGL.GL_TEXTURE_CUBE_MAP_ARRAY       => "GL_TEXTURE_CUBE_MAP_ARRAY",
                   IGL.GLTexture2DMultisample       => "GL_TEXTURE_2D_MULTISAMPLE",
                   IGL.GL_TEXTURE_2D_MULTISAMPLE_ARRAY => "GL_TEXTURE_2D_MULTISAMPLE_ARRAY",

                   // ----------------------------------

                   var _ => $"UNKNOWN TARGET: {target}"
               };
    }

    /// <summary>
    /// Retrieves the name of the PixelFormat format based on the provided LughFormat identifier.
    /// </summary>
    public static string GetFormatString( int format )
    {
        return format switch
               {
                   LughFormat.Alpha           => "Alpha",
                   LughFormat.LuminanceAlpha => "LuminanceAlpha",
                   LughFormat.RGB565          => "RGB565",
                   LughFormat.RGBA4444        => "RGBA4444",
                   LughFormat.RGB888          => "RGB888",
                   LughFormat.RGBA8888        => "RGBA8888 ( DEFAULT )",
                   LughFormat.IndexedColor   => "IndexedColor",

                   // ----------------------------------

                   LughFormat.Invalid   => "Invalid",
                   LughFormat.Undefined => "Undefined",

                   // ----------------------------------

                   var _ => throw new RuntimeException( $"Unknown Format: {format}" )
               };
    }

    /// <summary>
    /// Retrieves the name of the OpenGL pixel format based on the provided format identifier.
    /// </summary>
    /// <param name="format">The integer identifier of the OpenGL pixel format.</param>
    /// <returns>A string representing the name of the corresponding OpenGL pixel format.</returns>
    public static string GLFormatAsString( int format )
    {
        return format switch
               {
                   // Unsigned normalized ( filterable, colour )
                   IGL.GLR8           => "IGL.GL_R8",
                   IGL.GLRg8          => "IGL.GL_RG8",
                   IGL.GLRGB8         => "IGL.GL_RGB8",
                   IGL.GLRGBA8        => "IGL.GL_RGBA8",
                   IGL.GLSrgb8        => "IGL.GL_SRGB8",
                   IGL.GLSrgb8Alpha8 => "IGL.GL_SRGB8_ALPHA8",

                   // Signed normalized
                   IGL.GLR8Snorm     => "IGL.GL_R8_SNORM",
                   IGL.GLRg8Snorm    => "IGL.GL_RG8_SNORM",
                   IGL.GLRGB8Snorm   => "IGL.GL_RGB8_SNORM",
                   IGL.GLRGBA8Snorm  => "GL_RGBA8_SNORM",
                   IGL.GLR16Snorm    => "IGL.GL_R16_SNORM",
                   IGL.GLRg16Snorm   => "IGL.GL_RG16_SNORM",
                   IGL.GLRGB16Snorm  => "IGL.GL_RGB16_SNORM",
                   IGL.GLRGBA16Snorm => "GL_RGBA16_SNORM",

                   IGL.GLR16F               => "IGL.GL_R16F",
                   IGL.GLRg16F              => "IGL.GL_RG16F",
                   IGL.GLRGBA16F            => "IGL.GL_RGBA16F",
                   IGL.GLR32F               => "IGL.GL_R32F",
                   IGL.GLRg32F              => "IGL.GL_RG32F",
                   IGL.GLRGBA32F            => "IGL.GL_RGBA32F",
                   IGL.GLR11FG11FB10F   => "IGL.GL_R11F_G11F_B10F",
                   IGL.GLRGB9E5             => "IGL.GL_RGB9_E5 ",
                   IGL.GLR8I                => "IGL.GL_R8I",
                   IGL.GLR8UI               => "IGL.GL_R8UI",
                   IGL.GLRg8I               => "IGL.GL_RG8I",
                   IGL.GLRg8UI              => "IGL.GL_RG8UI",
                   IGL.GLRGBA8I             => "IGL.GL_RGBA8I",
                   IGL.GLRGBA8UI            => "IGL.GL_RGBA8UI",
                   IGL.GLR16I               => "IGL.GL_R16I",
                   IGL.GLR16UI              => "IGL.GL_R16UI",
                   IGL.GLRg16I              => "IGL.GL_RG16I",
                   IGL.GLRg16UI             => "IGL.GL_RG16UI",
                   IGL.GLRGBA16I            => "IGL.GL_RGBA16I",
                   IGL.GLRGBA16UI           => "IGL.GL_RGBA16UI",
                   IGL.GLR32I               => "IGL.GL_R32I",
                   IGL.GLR32UI              => "IGL.GL_R32UI",
                   IGL.GLRg32I              => "IGL.GL_RG32I",
                   IGL.GLRg32UI             => "IGL.GL_RG32UI",
                   IGL.GLRGBA32I            => "IGL.GL_RGBA32I",
                   IGL.GLRGBA32UI           => "IGL.GL_RGBA32UI",
                   IGL.GLDepthComponent24   => "IGL.GL_DEPTH_COMPONENT24 ",
                   IGL.GLDepthComponent32F => "IGL.GL_DEPTH_COMPONENT32F",
                   IGL.GLStencilIndex8      => "IGL.GL_STENCIL_INDEX8",
                   IGL.GLDepth24Stencil8    => "IGL.GL_DEPTH24_STENCIL8",
                   IGL.GLDepth32FStencil8  => "IGL.GL_DEPTH32F_STENCIL8",
                   IGL.GLColorIndex         => "IGL.GL_COLOR_INDEX",

                   // ----------------------------------

                   var _ => $"Invalid format: {format}"
               };
    }

    /// <summary>
    /// Converts a PNG Color Type value to a LughFormat value.
    /// </summary>
    public static int PNGColorToLughFormat( int colorType )
    {
        return colorType switch
               {
                   1 => LughFormat.Alpha,
                   2 => LughFormat.LuminanceAlpha,
                   3 => LughFormat.RGB888,
                   4 => LughFormat.RGBA8888,
                   5 => LughFormat.RGB565,
                   6 => LughFormat.RGBA4444,

                   // ----------------------------------

                   var _ => throw new RuntimeException( $"Unknown PNG Color Type: {colorType}" )
               };
    }

    /// <summary>
    /// Converts a <see cref="LughFormat"/> value to the corresponding
    /// <see cref="System.Drawing.Imaging.PixelFormat"/>.
    /// </summary>
    /// <param name="format">The pixel format of type <c>LughFormat</c>.</param>
    /// <returns>The corresponding <see cref="System.Drawing.Imaging.PixelFormat"/>.</returns>
    /// <exception cref="RuntimeException">
    /// Thrown if the provided format is invalid or unsupported.
    /// </exception>
    [SupportedOSPlatform( "windows" )]
    public static System.Drawing.Imaging.PixelFormat ToSystemPixelFormat( int format )
    {
        return format switch
               {
                   LughFormat.Alpha           => System.Drawing.Imaging.PixelFormat.Alpha,
                   LughFormat.LuminanceAlpha => System.Drawing.Imaging.PixelFormat.Alpha,
                   LughFormat.RGB565          => System.Drawing.Imaging.PixelFormat.Format16bppRgb565,
                   LughFormat.RGB888          => System.Drawing.Imaging.PixelFormat.Format32bppRgb,
                   LughFormat.RGBA8888        => System.Drawing.Imaging.PixelFormat.Format32bppArgb,
                   LughFormat.RGBA4444        => System.Drawing.Imaging.PixelFormat.Format32bppArgb,

                   // ----------------------------------

                   var _ => throw new RuntimeException( $"Invalid format: {format}" )
               };
    }

    /// <summary>
    /// Converts a PNG color type and bit depth to a PixelFormat value. If invalid
    /// color type or bit depth is provided, the function returns <see cref="LughFormat.Invalid"/>,
    /// and this must be handled by the caller.
    /// </summary>
    public static int FromPNGColorAndBitDepth( byte colorType, byte bitDepth )
    {
        // Map PNG color type and bit depth to the format
        int format = ( colorType, bitDepth ) switch
                     {
                         (6, 8) => LughFormat.RGBA8888,        // Truecolor with alpha, 8 bits
                         (6, 4) => LughFormat.RGBA4444,        // Truecolor with alpha, 4 bits
                         (2, 8) => LughFormat.RGB888,          // Truecolor, 8 bits
                         (0, 8) => LughFormat.Alpha,           // Grayscale, 8 bits
                         (4, 8) => LughFormat.LuminanceAlpha, // Grayscale with alpha, 8 bits
                         (2, 5) => LughFormat.RGB565,          // Truecolor, 5 bits
                         (3, 8) => LughFormat.IndexedColor,   // Indexed color, 8 bits

                         // ----------------------------------

                         var _ => LughFormat.Invalid // Invalid format, handled by caller
                     };

        return format;
    }
}

// ========================================================================
// ========================================================================