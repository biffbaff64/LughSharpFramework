﻿// /////////////////////////////////////////////////////////////////////////////
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

namespace LughSharp.Lugh.Graphics.Images;

[PublicAPI]
public class BufferedImage
{
    public int              Width       { get; set; }
    public int              Height      { get; set; }
    public PixelType.Format PixelFormat { get; set; }
    public Image            Image       { get; private set; }

    // ========================================================================

    public BufferedImage()
    {
    }

    public BufferedImage( Image.ColorModel getColorModel, object createWritableChild, object isAlphaPremultiplied, object o )
    {
    }

    public BufferedImage( int colorModel, int writableChild, PixelType.Format format )
    {
    }

    public Image.ColorModel GetColorModel()
    {
        throw new NotImplementedException();
    }

    public Image.WritableRaster GetRaster()
    {
        throw new NotImplementedException();
    }

    public static BufferedImage Read( FileInfo file )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the packed color RGBA at the provided X and Y coordinates
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public uint GetPixel( int x, int y )
    {
        throw new NotImplementedException();
    }

    public void SetPixel( int x, int y, uint color )
    {
    }
}