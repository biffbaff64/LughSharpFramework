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

using System.Runtime.InteropServices;

using JetBrains.Annotations;

namespace LughSharp.Core.Graphics.ImageDecoders;

// ========================================================================
/// <summary>
/// BMP Header Structure.
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

// ========================================================================
/// <summary>
/// BMP Info Header Structure. Do not change this layout order.
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

    public void Init()
    {
        HeaderSize = ( uint )Marshal.SizeOf( this );
    }
}

[PublicAPI]
public enum BitmapCompressionMode : uint
{
    BI_RGB       = 0,
    BI_RLE8      = 1,
    BI_RLE4      = 2,
    BI_BITFIELDS = 3,
    BI_JPEG      = 4,
    BI_PNG       = 5
}

// ========================================================================
/// <summary>
/// BMP Pixel Array Structure.
/// </summary>
[PublicAPI]
[StructLayout( LayoutKind.Sequential )]
public struct BMPPixelArray
{
    public byte[] PixelData { get; set; } // Array of color data for each pixel in the image.  
}