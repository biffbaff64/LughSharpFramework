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
                   LughFormat.RGB888   => IGL.GL_SRGB8,
                   LughFormat.RGBA8888 => IGL.GL_SRGB8_ALPHA8,

                   // Modern GL Core uses R8 for single channel. 
                   // We use Swizzle on the texture unit to treat Red as Alpha.
                   LughFormat.ALPHA           => IGL.GL_R8,
                   LughFormat.LUMINANCE_ALPHA => IGL.GL_RG8, // R=Luminance, G=Alpha

                   // Legacy packed formats (Optional, but better as RGBA8)
                   LughFormat.RGB565   => IGL.GL_RGB565,
                   LughFormat.RGBA4444 => IGL.GL_RGBA4,

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
                   LughFormat.RGB888          => IGL.GL_RGB,
                   LughFormat.RGBA8888        => IGL.GL_RGBA,
                   LughFormat.ALPHA           => IGL.GL_RED, // Data comes in as 1 channel
                   LughFormat.LUMINANCE_ALPHA => IGL.GL_RG,  // Data comes in as 2 channels
                   LughFormat.RGB565          => IGL.GL_RGB,
                   LughFormat.RGBA4444        => IGL.GL_RGBA,

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
                   LughFormat.RGB565   => IGL.GL_UNSIGNED_SHORT_5_6_5,
                   LughFormat.RGBA4444 => IGL.GL_UNSIGNED_SHORT_4_4_4_4,

                   // ---------------------------

                   // Standard 8-bit per channel
                   var _ => IGL.GL_UNSIGNED_BYTE
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
                   LughFormat.ALPHA           => 1,
                   LughFormat.LUMINANCE_ALPHA => 2,
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
        return GetAlignment( pixmap.Width * BytesPerPixel( pixmap.GetColorFormat()) );
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

        if ( ( rowStrideBytes % 2 ) == 0 )
        {
            return 2;
        }

        return 1;
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
                   IGL.GL_UNSIGNED_BYTE          => "IGL.GL_UNSIGNED_BYTE",
                   IGL.GL_UNSIGNED_SHORT_5_6_5   => "IGL.GL_UNSIGNED_SHORT_5_6_5",
                   IGL.GL_UNSIGNED_SHORT_4_4_4_4 => "IGL.GL_UNSIGNED_SHORT_4_4_4_4",

                   // ----------------------------------

                   var _ => $"Invalid format: {format}",
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
                   IGL.GL_TEXTURE_1D                   => "GL_TEXTURE_1D",
                   IGL.GL_TEXTURE_2D                   => "GL_TEXTURE_2D",
                   IGL.GL_TEXTURE_3D                   => "GL_TEXTURE_3D",
                   IGL.GL_TEXTURE_CUBE_MAP             => "GL_TEXTURE_CUBE_MAP",
                   IGL.GL_TEXTURE_RECTANGLE            => "GL_TEXTURE_RECTANGLE",
                   IGL.GL_TEXTURE_BUFFER               => "GL_TEXTURE_BUFFER",
                   IGL.GL_TEXTURE_2D_ARRAY             => "GL_TEXTURE_2D_ARRAY",
                   IGL.GL_TEXTURE_CUBE_MAP_ARRAY       => "GL_TEXTURE_CUBE_MAP_ARRAY",
                   IGL.GL_TEXTURE_2D_MULTISAMPLE       => "GL_TEXTURE_2D_MULTISAMPLE",
                   IGL.GL_TEXTURE_2D_MULTISAMPLE_ARRAY => "GL_TEXTURE_2D_MULTISAMPLE_ARRAY",

                   // ----------------------------------

                   var _ => $"UNKNOWN TARGET: {target}",
               };
    }

    /// <summary>
    /// Retrieves the name of the PixelFormat format based on the provided format identifier.
    /// </summary>
    public static string GetFormatString( int format )
    {
        return format switch
               {
                   LughFormat.ALPHA           => "Alpha",
                   LughFormat.LUMINANCE_ALPHA => "LuminanceAlpha",
                   LughFormat.RGB565          => "RGB565",
                   LughFormat.RGBA4444        => "RGBA4444",
                   LughFormat.RGB888          => "RGB888",
                   LughFormat.RGBA8888        => "RGBA8888 ( DEFAULT )",
                   LughFormat.INDEXED_COLOR   => "IndexedColor",

                   // ----------------------------------

                   LughFormat.INVALID   => "Invalid",
                   LughFormat.UNDEFINED => "Undefined",

                   // ----------------------------------

                   var _ => throw new RuntimeException( $"Unknown Format: {format}" ),
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
                   IGL.GL_R8           => "IGL.GL_R8",
                   IGL.GL_RG8          => "IGL.GL_RG8",
                   IGL.GL_RGB8         => "IGL.GL_RGB8",
                   IGL.GL_RGBA8        => "IGL.GL_RGBA8",
                   IGL.GL_SRGB8        => "IGL.GL_SRGB8",
                   IGL.GL_SRGB8_ALPHA8 => "IGL.GL_SRGB8_ALPHA8",

                   // Signed normalized
                   IGL.GL_R8_SNORM     => "IGL.GL_R8_SNORM",
                   IGL.GL_RG8_SNORM    => "IGL.GL_RG8_SNORM",
                   IGL.GL_RGB8_SNORM   => "IGL.GL_RGB8_SNORM",
                   IGL.GL_RGBA8_SNORM  => "GL_RGBA8_SNORM",
                   IGL.GL_R16_SNORM    => "IGL.GL_R16_SNORM",
                   IGL.GL_RG16_SNORM   => "IGL.GL_RG16_SNORM",
                   IGL.GL_RGB16_SNORM  => "IGL.GL_RGB16_SNORM",
                   IGL.GL_RGBA16_SNORM => "GL_RGBA16_SNORM",

                   IGL.GL_R16_F               => "IGL.GL_R16F",
                   IGL.GL_RG16_F              => "IGL.GL_RG16F",
                   IGL.GL_RGBA16_F            => "IGL.GL_RGBA16F",
                   IGL.GL_R32_F               => "IGL.GL_R32F",
                   IGL.GL_RG32_F              => "IGL.GL_RG32F",
                   IGL.GL_RGBA32_F            => "IGL.GL_RGBA32F",
                   IGL.GL_R11_F_G11_F_B10_F   => "IGL.GL_R11F_G11F_B10F",
                   IGL.GL_RGB9_E5             => "IGL.GL_RGB9_E5 ",
                   IGL.GL_R8_I                => "IGL.GL_R8I",
                   IGL.GL_R8_UI               => "IGL.GL_R8UI",
                   IGL.GL_RG8_I               => "IGL.GL_RG8I",
                   IGL.GL_RG8_UI              => "IGL.GL_RG8UI",
                   IGL.GL_RGBA8_I             => "IGL.GL_RGBA8I",
                   IGL.GL_RGBA8_UI            => "IGL.GL_RGBA8UI",
                   IGL.GL_R16_I               => "IGL.GL_R16I",
                   IGL.GL_R16_UI              => "IGL.GL_R16UI",
                   IGL.GL_RG16_I              => "IGL.GL_RG16I",
                   IGL.GL_RG16_UI             => "IGL.GL_RG16UI",
                   IGL.GL_RGBA16_I            => "IGL.GL_RGBA16I",
                   IGL.GL_RGBA16_UI           => "IGL.GL_RGBA16UI",
                   IGL.GL_R32_I               => "IGL.GL_R32I",
                   IGL.GL_R32_UI              => "IGL.GL_R32UI",
                   IGL.GL_RG32_I              => "IGL.GL_RG32I",
                   IGL.GL_RG32_UI             => "IGL.GL_RG32UI",
                   IGL.GL_RGBA32_I            => "IGL.GL_RGBA32I",
                   IGL.GL_RGBA32_UI           => "IGL.GL_RGBA32UI",
                   IGL.GL_DEPTH_COMPONENT24   => "IGL.GL_DEPTH_COMPONENT24 ",
                   IGL.GL_DEPTH_COMPONENT32_F => "IGL.GL_DEPTH_COMPONENT32F",
                   IGL.GL_STENCIL_INDEX8      => "IGL.GL_STENCIL_INDEX8",
                   IGL.GL_DEPTH24_STENCIL8    => "IGL.GL_DEPTH24_STENCIL8",
                   IGL.GL_DEPTH32_F_STENCIL8  => "IGL.GL_DEPTH32F_STENCIL8",
                   IGL.GL_COLOR_INDEX         => "IGL.GL_COLOR_INDEX",

                   // ----------------------------------

                   var _ => $"Invalid format: {format}",
               };
    }

    /// <summary>
    /// Converts a PNG Color Type value to a PixelFormat value.
    /// </summary>
    public static int PNGColorToLughFormat( int colorType )
    {
        return colorType switch
               {
                   1 => LughFormat.ALPHA,
                   2 => LughFormat.LUMINANCE_ALPHA,
                   3 => LughFormat.RGB888,
                   4 => LughFormat.RGBA8888,
                   5 => LughFormat.RGB565,
                   6 => LughFormat.RGBA4444,

                   // ----------------------------------

                   var _ => throw new RuntimeException( $"Unknown PNG Color Type: {colorType}" ),
               };
    }

    /// <summary>
    /// Converts a <c>Gdx2DPixmap.GDX_2D_FORMAT_XXX</c> to the corresponding System.Drawing.Imaging.PixelFormat.
    /// </summary>
    /// <param name="format">The pixel format of type <c>Gdx2DPixmap.GDX_2D_FORMAT_XXX</c>.</param>
    /// <returns>The corresponding <see cref="System.Drawing.Imaging.PixelFormat"/>.</returns>
    /// <exception cref="RuntimeException">
    /// Thrown if the provided format is invalid or unsupported.
    /// </exception>
    [SupportedOSPlatform( "windows" )]
    public static System.Drawing.Imaging.PixelFormat ToSystemPixelFormat( int format )
    {
        return format switch
               {
                   LughFormat.ALPHA           => System.Drawing.Imaging.PixelFormat.Alpha,
                   LughFormat.LUMINANCE_ALPHA => System.Drawing.Imaging.PixelFormat.Alpha,
                   LughFormat.RGB565          => System.Drawing.Imaging.PixelFormat.Format16bppRgb565,
                   LughFormat.RGB888          => System.Drawing.Imaging.PixelFormat.Format32bppRgb,
                   LughFormat.RGBA8888        => System.Drawing.Imaging.PixelFormat.Format32bppArgb,
                   LughFormat.RGBA4444        => System.Drawing.Imaging.PixelFormat.Format32bppArgb,

                   // ----------------------------------

                   var _ => throw new RuntimeException( $"Invalid format: {format}" ),
               };
    }

    /// <summary>
    /// Converts a PNG color type and bit depth to a PixelFormat value.
    /// </summary>
    public static int FromPNGColorAndBitDepth( byte colorType, byte bitDepth )
    {
        // Map PNG color type and bit depth to the format
        var format = ( colorType, bitDepth ) switch
                     {
                         (6, 8) => LughFormat.RGBA8888,        // Truecolor with alpha, 8 bits
                         (6, 4) => LughFormat.RGBA4444,        // Truecolor with alpha, 4 bits
                         (2, 8) => LughFormat.RGB888,          // Truecolor, 8 bits
                         (0, 8) => LughFormat.ALPHA,           // Grayscale, 8 bits
                         (4, 8) => LughFormat.LUMINANCE_ALPHA, // Grayscale with alpha, 8 bits
                         (2, 5) => LughFormat.RGB565,          // Truecolor, 5 bits
                         (3, 8) => LughFormat.INDEXED_COLOR,   // Indexed color, 8 bits

                         // ----------------------------------

                         var _ => LughFormat.INVALID, // Invalid format, handled by caller
                     };

        return format;
    }
}

// ========================================================================
// ========================================================================