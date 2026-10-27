// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin
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

namespace LughSharp.Source.Graphics.Images.Decoders;

/// <summary>
/// BMP Header Structure.
/// <br/>Bytes 0 - 1: File signature/identifier; must be 0x4D42 in hex or "BM" in ASCII.
/// <br/>Bytes 2 - 5: Size of the file in bytes.
/// <br/>Bytes 6 - 7: Reserved, must be 0.
/// <br/>Bytes 8 - 9: Reserved, must be 0.
/// <br/>Bytes 10 - 13: Offset to the pixel array from the beginning of the file.
/// </summary>
[PublicAPI]
[StructLayout( LayoutKind.Sequential )]
public struct BitmapFileHeader
{
    public ushort FileType           { get; set; } // 2 bytes, 'BM'
    public uint   FileSize           { get; set; } // 4 bytes, size of the file in bytes
    public ushort Reserved1          { get; set; } // 2 bytes, always 0
    public ushort Reserved2          { get; set; } // 2 bytes, always 0
    public uint   OffsetToPixelArray { get; set; } // 4 bytes, offset to the pixel array from the beginning of the file
}

// ============================================================================
// ============================================================================

/// <summary>
/// BMP Info Header Structure. Do not change this layout order.
/// <br/>Bytes 0 - 3: Size of the header in bytes.
/// <br/>Bytes 4 - 7: Width of the image in pixels.
/// <br/>Bytes 8 - 11: Height of the image in pixels.
/// <br/>Bytes 12 - 13: Number of color planes.
/// <br/>Bytes 14 - 15: Number of bits per pixel.
/// <br/>Bytes 16 - 19: Compression method.
/// <br/>Bytes 20 - 23: Image size in bytes.
/// <br/>Bytes 24 - 27: Horizontal resolution in pixels per meter.
/// <br/>Bytes 28 - 31: Vertical resolution in pixels per meter.
/// <br/>Bytes 32 - 35: Number of colors used.
/// <br/>Bytes 36 - 39: Number of important colors.
/// </summary>
[PublicAPI]
[StructLayout( LayoutKind.Sequential )]
public struct BitmapFileInfoHeader
{
    public uint                  HeaderSize      { get; set; } // 4 bytes, size of the header in bytes.
    public int                   Width           { get; set; } // 4 bytes, width of the image in pixels.
    public int                   Height          { get; set; } // 4 bytes, height of the image in pixels.
    public ushort                Planes          { get; set; } // 2 bytes, number of color planes.
    public ushort                BitCount        { get; set; } // 2 bytes, number of bits per pixel.
    public BitmapCompressionMode Compression     { get; set; } // 4 bytes, compression method.
    public uint                  ImageSize       { get; set; } // 4 bytes, image size in bytes.
    public int                   XPixelsPerMeter { get; set; } // 4 bytes, horizontal resolution in pixels per meter.
    public int                   YPixelsPerMeter { get; set; } // 4 bytes, vertical resolution in pixels per meter.
    public uint                  ColorsUsed      { get; set; } // 4 bytes, number of colors used.
    public uint                  ImportantColors { get; set; } // 4 bytes, number of important colors. 

    public BitmapFileInfoHeader()
    {
        HeaderSize = ( uint )Marshal.SizeOf( this );
    }
}

// ============================================================================
// ============================================================================

/// <summary>
/// 
/// </summary>
[PublicAPI]
public enum BitmapCompressionMode : uint
{
    /// <summary>
    ///  The standard uncompressed mode used for most BMP files. It stores raw
    /// pixel data sequentially without any compression.
    /// </summary>
    BiRGB = 0,

    /// <summary>
    /// Run-Length Encoding (RLE) used exclusively for 8-bit (256-color) paletted
    /// bitmaps. It compresses consecutive matching color indices into pairs of
    /// count and color data.
    /// </summary>
    BiRle8 = 1,

    /// <summary>
    /// Run-Length Encoding used exclusively for 4-bit (16-color) paletted bitmaps,
    /// working on the same repetition-counting principle for 4-bit nibbles.
    /// </summary>
    BiRle4 = 2,

    /// <summary>
    /// Used for 16-bit or 32-bit high-color and true-color images. Instead of
    /// compression, it uses explicit red, green, and blue bit masks in the header
    /// to define how color channels are packed into each pixel.
    /// </summary>
    BiBitfields = 3,

    /// <summary>
    /// Later extension in certain device/driver specs allowing embedded JPEG streams
    /// directly inside a BMP wrapper, though rarely used in standard file creation.
    /// </summary>
    BiJpeg = 4,

    /// <summary>
    /// Later extension in certain device/driver specs allowing embedded PNG streams
    /// directly inside a BMP wrapper, though rarely used in standard file creation.
    /// </summary>
    BiPNG = 5
}

// ============================================================================
// ============================================================================

/// <summary>
/// BMP Pixel Array Structure.
/// </summary>
[PublicAPI]
[StructLayout( LayoutKind.Sequential )]
public struct BmpPixelArray
{
    public byte[] PixelData { get; set; } // Array of color data for each pixel in the image.  
}

// ============================================================================
// ============================================================================
