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

namespace LughSharp.Lugh.Graphics;

/// <summary>
/// Simple pixmap struct holding the pixel data, the dimensions and the
/// format of the pixmap.
/// </summary>
[PublicAPI]
[StructLayout( LayoutKind.Sequential )]
public struct PixmapDescriptor()
{
    public int           Width         { get; set; } = 0;   // Width of the pixmap in pixels.
    public int           Height        { get; set; } = 0;   // Height of the pixmap in pixels.
    public Pixmap.Format ColorFormat   { get; set; } = 0;   // Color type of the pixmap.
    public byte          BitDepth      { get; set; } = 0;   // Number of bits per pixel.
    public int           BytesPerPixel { get; set; } = 0;   // Number of bytes per pixel.
    public uint          Blend         { get; set; } = 0;   // Blend mode.
    public uint          Scale         { get; set; } = 0;   // Scale mode.
    public byte[]        Pixels        { get; set; } = [ ]; // Pixel data.

    // ========================================================================

    public void DebugPrint()
    {
        Logger.Debug( $"Width        : {Width}" );
        Logger.Debug( $"Height       : {Height}" );
        Logger.Debug( $"ColorType    : {ColorFormat}" );
        Logger.Debug( $"Blend        : {Blend}" );
        Logger.Debug( $"Scale        : {Scale}" );
        Logger.Debug( $"BitDepth     : {BitDepth}" );
        Logger.Debug( $"BytesPerPixel: {BytesPerPixel}" );
    }
}

// ============================================================================
// ============================================================================