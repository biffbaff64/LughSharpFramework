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

namespace LughSharp.Lugh.Graphics.Text.Freetype;

public partial class FreeType
{
    public const int FT_PIXEL_MODE_NONE                  = 0;
    public const int FT_PIXEL_MODE_MONO                  = 1;
    public const int FT_PIXEL_MODE_GRAY                  = 2;
    public const int FT_PIXEL_MODE_GRAY2                 = 3;
    public const int FT_PIXEL_MODE_GRAY4                 = 4;
    public const int FT_PIXEL_MODE_LCD                   = 5;
    public const int FT_PIXEL_MODE_LCD_V                 = 6;
    public const int FT_FACE_FLAG_SCALABLE               = 1 << 0;
    public const int FT_FACE_FLAG_FIXED_SIZES            = 1 << 1;
    public const int FT_FACE_FLAG_FIXED_WIDTH            = 1 << 2;
    public const int FT_FACE_FLAG_SFNT                   = 1 << 3;
    public const int FT_FACE_FLAG_HORIZONTAL             = 1 << 4;
    public const int FT_FACE_FLAG_VERTICAL               = 1 << 5;
    public const int FT_FACE_FLAG_KERNING                = 1 << 6;
    public const int FT_FACE_FLAG_FAST_GLYPHS            = 1 << 7;
    public const int FT_FACE_FLAG_MULTIPLE_MASTERS       = 1 << 8;
    public const int FT_FACE_FLAG_GLYPH_NAMES            = 1 << 9;
    public const int FT_FACE_FLAG_EXTERNAL_STREAM        = 1 << 10;
    public const int FT_FACE_FLAG_HINTER                 = 1 << 11;
    public const int FT_FACE_FLAG_CID_KEYED              = 1 << 12;
    public const int FT_FACE_FLAG_TRICKY                 = 1 << 13;
    public const int FT_STYLE_FLAG_ITALIC                = 1 << 0;
    public const int FT_STYLE_FLAG_BOLD                  = 1 << 1;
    public const int FT_LOAD_DEFAULT                     = 0x0;
    public const int FT_LOAD_NO_SCALE                    = 0x1;
    public const int FT_LOAD_NO_HINTING                  = 0x2;
    public const int FT_LOAD_RENDER                      = 0x4;
    public const int FT_LOAD_NO_BITMAP                   = 0x8;
    public const int FT_LOAD_VERTICAL_LAYOUT             = 0x10;
    public const int FT_LOAD_FORCE_AUTOHINT              = 0x20;
    public const int FT_LOAD_CROP_BITMAP                 = 0x40;
    public const int FT_LOAD_PEDANTIC                    = 0x80;
    public const int FT_LOAD_IGNORE_GLOBAL_ADVANCE_WIDTH = 0x200;
    public const int FT_LOAD_NO_RECURSE                  = 0x400;
    public const int FT_LOAD_IGNORE_TRANSFORM            = 0x800;
    public const int FT_LOAD_MONOCHROME                  = 0x1000;
    public const int FT_LOAD_LINEAR_DESIGN               = 0x2000;
    public const int FT_LOAD_NO_AUTOHINT                 = 0x8000;
    public const int FT_LOAD_TARGET_NORMAL               = 0x0;
    public const int FT_LOAD_TARGET_LIGHT                = 0x10000;
    public const int FT_LOAD_TARGET_MONO                 = 0x20000;
    public const int FT_LOAD_TARGET_LCD                  = 0x30000;
    public const int FT_LOAD_TARGET_LCD_V                = 0x40000;
    public const int FT_RENDER_MODE_NORMAL               = 0;
    public const int FT_RENDER_MODE_LIGHT                = 1;
    public const int FT_RENDER_MODE_MONO                 = 2;
    public const int FT_RENDER_MODE_LCD                  = 3;
    public const int FT_RENDER_MODE_LCD_V                = 4;
    public const int FT_RENDER_MODE_MAX                  = 5;
    public const int FT_KERNING_DEFAULT                  = 0;
    public const int FT_KERNING_UNFITTED                 = 1;
    public const int FT_KERNING_UNSCALED                 = 2;
    public const int FT_STROKER_LINECAP_BUTT             = 0;
    public const int FT_STROKER_LINECAP_ROUND            = 1;
    public const int FT_STROKER_LINECAP_SQUARE           = 2;
    public const int FT_STROKER_LINEJOIN_ROUND           = 0;
    public const int FT_STROKER_LINEJOIN_BEVEL           = 1;
    public const int FT_STROKER_LINEJOIN_MITER_VARIABLE  = 2;
    public const int FT_STROKER_LINEJOIN_MITER           = FT_STROKER_LINEJOIN_MITER_VARIABLE;
    public const int FT_STROKER_LINEJOIN_MITER_FIXED     = 3;

    // ========================================================================

    public readonly int FtEncodingAdobeCustom   = Encode( 'A', 'D', 'B', 'C' );
    public readonly int FtEncodingAdobeExpert   = Encode( 'A', 'D', 'B', 'E' );
    public readonly int FtEncodingAdobeLatin1   = Encode( 'l', 'a', 't', '1' );
    public readonly int FtEncodingAdobeStandard = Encode( 'A', 'D', 'O', 'B' );
    public readonly int FtEncodingAppleRoman    = Encode( 'a', 'r', 'm', 'n' );
    public readonly int FtEncodingBig5          = Encode( 'b', 'i', 'g', '5' );
    public readonly int FtEncodingGb2312        = Encode( 'g', 'b', ' ', ' ' );
    public readonly int FtEncodingJohab         = Encode( 'j', 'o', 'h', 'a' );
    public readonly int FtEncodingMsSymbol      = Encode( 's', 'y', 'm', 'b' );
    public readonly int FtEncodingOldLatin2     = Encode( 'l', 'a', 't', '2' );
    public readonly int FtEncodingSjis          = Encode( 's', 'j', 'i', 's' );
    public readonly int FtEncodingUnicode       = Encode( 'u', 'n', 'i', 'c' );
    public readonly int FtEncodingWansung       = Encode( 'w', 'a', 'n', 's' );
    public readonly int FtEncodingNone          = 0;
}

