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

using LughSharp.Core.Graphics.OpenGL.Bindings;
using LughSharp.Core.Graphics.OpenGL.Enums;

namespace LughSharp.Core.Graphics.OpenGL;

[PublicAPI]
[SuppressMessage( "ReSharper", "InconsistentNaming" )]
public class GLFormatChooser
{
    private readonly GraphicsCapabilities _caps;
    private readonly IGLBindings          _gl;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="caps"></param>
    /// <param name="gl"></param>
    public GLFormatChooser( GraphicsCapabilities caps, IGLBindings gl )
    {
        _caps = caps;
        _gl   = gl;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="usage"></param>
    /// <param name="preferCompressed"></param>
    /// <param name="requireFramebufferRenderable"></param>
    /// <param name="preferSmallHdr"></param>
    /// <returns></returns>
    public int Choose( TextureUsage usage,
                       bool preferCompressed = true,
                       bool requireFramebufferRenderable = false,
                       bool preferSmallHdr = true ) // uses R11F_G11F_B10F for HDR when alpha not required
    {
        var candidates = new List< int >();

        switch ( usage )
        {
            case TextureUsage.ColorSrgb:
                if ( preferCompressed )
                {
                    if ( _caps.HasBPTC )
                    {
                        candidates.Add( GLIF.COMPRESSED_SRGB_ALPHA_BPTC_UNORM ); // BC7 sRGB
                    }

                    if ( _caps.HasS3TC )
                    {
                        candidates.Add( GLIF.COMPRESSED_RGBA_S3TC_DXT5_EXT ); // BC3
                    }
                }

                candidates.Add( GLIF.SRGB8_ALPHA8 );

                break;

            case TextureUsage.ColorLdr:
                if ( preferCompressed )
                {
                    if ( _caps.HasBPTC )
                    {
                        candidates.Add( GLIF.COMPRESSED_RGBA_BPTC_UNORM ); // BC7
                    }

                    if ( _caps.HasS3TC )
                    {
                        candidates.Add( GLIF.COMPRESSED_RGBA_S3TC_DXT5_EXT ); // BC3
                    }
                }

                candidates.Add( GLIF.RGBA8 );

                break;

            case TextureUsage.NormalMap:
                if ( _caps.HasRGTC )
                {
                    candidates.Add( GLIF.COMPRESSED_RG_RGTC2 ); // BC5
                }

                candidates.Add( GLIF.RG8 );

                break;

            case TextureUsage.MaskR8:
                candidates.Add( GLIF.R8 );

                break;

            case TextureUsage.HdrColor:
                if ( preferCompressed && _caps.HasBPTC )
                {
                    // BC6H; great for RGB HDR textures (no alpha). Not renderable.
                    candidates.Add( GLIF.COMPRESSED_RGB_BPTC_UNSIGNED_FLOAT );
                }

                if ( preferSmallHdr )
                {
                    candidates.Add( GLIF.R11F_G11F_B10F ); // Filterable HDR-ish (no alpha), widely supported
                }

                candidates.Add( GLIF.RGBA16F ); // Safe HDR with alpha, renderable on most drivers

                break;

            case TextureUsage.DepthOnly:
                candidates.Add( GLIF.DEPTH_COMPONENT24 );

                // Consider DEPTH_COMPONENT32F as a fallback
                break;

            case TextureUsage.DepthStencil:
                candidates.Add( GLIF.DEPTH24_STENCIL8 );

                // Consider DEPTH32F_STENCIL8 as a fallback
                break;

            default:
                candidates.Add( GLIF.RGBA8 );

                break;
        }

        foreach ( var fmt in candidates )
        {
            if ( !IsSupported( fmt ) )
            {
                continue;
            }

            if ( requireFramebufferRenderable && !IsFramebufferRenderable( fmt ) )
            {
                continue;
            }

            return fmt;
        }

        // Last-resort fallbacks
        if ( usage == TextureUsage.DepthStencil )
        {
            return GLIF.DEPTH24_STENCIL8;
        }

        if ( usage == TextureUsage.DepthOnly )
        {
            return GLIF.DEPTH_COMPONENT24;
        }

        return usage == TextureUsage.MaskR8 ? GLIF.R8 : GLIF.RGBA8;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="internalFormat"></param>
    /// <returns></returns>
    private bool IsSupported( int internalFormat )
    {
        if ( _caps.HasInternalFormatQuery )
        {
            _gl.GetInternalformativ( GLData.TEXTURE_2D, internalFormat, GLIFQ.INTERNALFORMAT_SUPPORTED, 1, out var v );

            return v != 0;
        }

        // Conservative fallback: assume core formats are supported; gate compressed by caps.
        if ( IsCoreFormat( internalFormat ) )
        {
            return true;
        }

        if ( IsS3TC( internalFormat ) )
        {
            return _caps.HasS3TC;
        }

        if ( IsRGTC( internalFormat ) )
        {
            return _caps.HasRGTC;
        }

        if ( IsBPTC( internalFormat ) )
        {
            return _caps.HasBPTC;
        }

        return IsETC2( internalFormat ) && _caps.HasETC2;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="internalFormat"></param>
    /// <returns></returns>
    private bool IsFramebufferRenderable( int internalFormat )
    {
        if ( internalFormat == GLIF.COMPRESSED_RGB_BPTC_UNSIGNED_FLOAT )
        {
            return false; // compressed HDR textures aren't renderable
        }

        if ( !_caps.HasInternalFormatQuery )
        {
            // Heuristic: known good color/depth formats
            return internalFormat is GLIF.RGBA8 or GLIF.SRGB8_ALPHA8 or GLIF.R11F_G11F_B10F or GLIF.RGBA16F
                                     or GLIF.DEPTH_COMPONENT24 or GLIF.DEPTH24_STENCIL8;
        }

        _gl.GetInternalformativ( GLData.TEXTURE_2D, internalFormat, GLIFQ.FRAMEBUFFER_RENDERABLE, 1, out var v );

        return v != 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    private static bool IsCoreFormat( int f )
    {
        return f is GLIF.R8 or GLIF.RG8 or GLIF.RGBA8 or GLIF.SRGB8_ALPHA8
                    or GLIF.R11F_G11F_B10F or GLIF.RGBA16F
                    or GLIF.DEPTH_COMPONENT24 or GLIF.DEPTH24_STENCIL8;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    private static bool IsS3TC( int f )
    {
        return f is GLIF.COMPRESSED_RGB_S3TC_DXT1_EXT or GLIF.COMPRESSED_RGBA_S3TC_DXT1_EXT
                                                      or GLIF.COMPRESSED_RGBA_S3TC_DXT3_EXT
                                                      or GLIF.COMPRESSED_RGBA_S3TC_DXT5_EXT;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    private static bool IsRGTC( int f )
    {
        return f is GLIF.COMPRESSED_RED_RGTC1 or GLIF.COMPRESSED_RG_RGTC2;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    private static bool IsBPTC( int f )
    {
        return f is GLIF.COMPRESSED_RGBA_BPTC_UNORM or GLIF.COMPRESSED_SRGB_ALPHA_BPTC_UNORM
                                                    or GLIF.COMPRESSED_RGB_BPTC_UNSIGNED_FLOAT
                                                    or GLIF.COMPRESSED_RGB_BPTC_SIGNED_FLOAT;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    private static bool IsETC2( int f )
    {
        return f is GLIF.COMPRESSED_SRGB8_ALPHA8_ETC2
                    or GLIF.COMPRESSED_RGBA8_ETC2_EAC;
    }
}

// ============================================================================

[PublicAPI]
[SuppressMessage( "ReSharper", "InconsistentNaming" )]
public static class GLIFQ
{
    public const int INTERNALFORMAT_SUPPORTED = 0x826F;
    public const int FRAMEBUFFER_RENDERABLE   = 0x8289;
}

// ============================================================================

// Subset of internal formats used by the chooser.
// Values match OpenGL enums.
[PublicAPI]
[SuppressMessage( "ReSharper", "InconsistentNaming" )]
public static class GLIF
{
    // Uncompressed color
    public const int R8           = 0x8229;
    public const int RG8          = 0x822B;
    public const int RGBA8        = 0x8058;
    public const int SRGB8_ALPHA8 = 0x8C43;

    // Float color
    public const int R11F_G11F_B10F = 0x8C3A;
    public const int RGBA16F        = 0x881A;

    // Depth/Stencil
    public const int DEPTH_COMPONENT24 = 0x81A6;
    public const int DEPTH24_STENCIL8  = 0x88F0;

    // S3TC (BC1–BC3)
    public const int COMPRESSED_RGB_S3TC_DXT1_EXT  = 0x83F0;
    public const int COMPRESSED_RGBA_S3TC_DXT1_EXT = 0x83F1;
    public const int COMPRESSED_RGBA_S3TC_DXT3_EXT = 0x83F2;
    public const int COMPRESSED_RGBA_S3TC_DXT5_EXT = 0x83F3;

    // RGTC (BC4–BC5)
    public const int COMPRESSED_RED_RGTC1 = 0x8DBB;
    public const int COMPRESSED_RG_RGTC2  = 0x8DBD;

    // BPTC (BC6H–BC7)
    public const int COMPRESSED_RGBA_BPTC_UNORM         = 0x8E8C;
    public const int COMPRESSED_SRGB_ALPHA_BPTC_UNORM   = 0x8E8D;
    public const int COMPRESSED_RGB_BPTC_SIGNED_FLOAT   = 0x8E8E;
    public const int COMPRESSED_RGB_BPTC_UNSIGNED_FLOAT = 0x8E8F;

    // ETC2 (optional on desktop)
    public const int COMPRESSED_SRGB8_ALPHA8_ETC2 = 0x9279;
    public const int COMPRESSED_RGBA8_ETC2_EAC    = 0x9278;
}

// ========================================================================
// ========================================================================
