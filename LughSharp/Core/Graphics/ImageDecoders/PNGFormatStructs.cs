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

using JetBrains.Annotations; namespace LughSharp.Core.Graphics.Utils;

[PublicAPI]
public class PNGFormatStructs
{
    /// <summary>
    /// The PNG signature is eight bytes in length and contains information
    /// used to identify a file or data stream as conforming to the PNG
    /// specification.
    /// </summary>
    [PublicAPI]
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public struct PNGSignature
    {
        public byte[] Signature { get; set; } // Identifier (always 0x89504E470D0A1A0A)
    }

    /// <summary>
    /// PNG File IHDR Structure. The header chunk contains information on the image data
    /// stored in the PNG file. This chunk must be the first chunk in a PNG data stream
    /// and immediately follows the PNG signature.
    /// </summary>
    [PublicAPI]
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public struct IHDRChunk
    {
        public byte[] Ihdr        { get; set; } // 13 bytes - 0x00, 0x00, 0x00, 0x0D
        public byte[] IhdrType    { get; set; } //  4 bytes - 'I', 'H', 'D', 'R' - 0x49, 0x48, 0x44, 0x52
        public uint   Width       { get; set; } //  4 bytes - Width of image in pixels
        public uint   Height      { get; set; } //  4 bytes - Height of image in pixels
        public byte   BitDepth    { get; set; } //  1 byte  - Bits per pixel or per sample
        public byte   ColorType   { get; set; } //  1 byte  - Color interpretation indicator
        public byte   Compression { get; set; } //  1 byte  - Compression type indicator
        public byte   Filter      { get; set; } //  1 byte  - Filter type indicator
        public byte   Interlace   { get; set; } //  1 byte  - Type of interlacing scheme used
        public byte[] Crc         { get; set; } //  1 byte  - The CRC for IHDR
    }

    /// <summary>
    /// PNG File IDAT Chunk Structure.
    /// </summary>
    [PublicAPI]
    [StructLayout( LayoutKind.Sequential, Pack = 1 )] // Important for correct byte alignment
    public struct IDATChunk
    {
        public uint   ChunkSize { get; set; } // 4 bytes (unsigned int)
        public byte[] ChunkType { get; set; } // 4 bytes 'I', 'D', 'A', 'T'
    }
}