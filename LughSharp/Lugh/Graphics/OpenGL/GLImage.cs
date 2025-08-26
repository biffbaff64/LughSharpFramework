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

namespace LughSharp.Lugh.Graphics.OpenGL;

/// <summary>
/// Represents an image with specified dimensions and GLFW pixel data.
/// </summary>
[PublicAPI]
[StructLayout( LayoutKind.Sequential, Pack = 1 )]
public unsafe struct GLImage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GLImage"/> struct.
    /// </summary>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="height">The height of the image in pixels.</param>
    /// <param name="pixels"><see cref="IntPtr"/> pointing to the RGBA pixel data of the image.</param>
    public GLImage( int width, int height, byte* pixels )
    {
        Width  = width;
        Height = height;
        Pixels = pixels;
    }

    /// <summary>
    /// The width, in pixels, of this <see cref="GLImage"/>.
    /// </summary>
    public int Width;

    /// <summary>
    /// The height, in pixels, of this <see cref="GLImage"/>.
    /// </summary>
    public int Height;

    /// <summary>
    /// A <see cref="byte"/> pointer pointing to the RGBA pixel data.
    /// </summary>
    public byte* Pixels;
}

// ========================================================================
// ========================================================================