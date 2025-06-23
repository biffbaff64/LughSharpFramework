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

public partial interface IGraphicsDevice
{
    /// <summary>
    /// Describes a fullscreen display mode, having the properties <see cref="Width" />,
    /// <see cref="Height" />, <see cref="RefreshRate" />, and <see cref="BitsPerPixel" />.
    /// <para>
    /// Display modes represent different configurations for fullscreen rendering,
    /// allowing you to choose between resolutions, refresh rates, and color depths
    /// supported by the monitor.
    /// </para>
    /// </summary>
    [PublicAPI]
    public class DisplayMode
    {
        /// <summary>
        /// Width of this display mode in pixels.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Height of this display mode in pixels.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The refresh rate of this display mode in Hertz (Hz).
        /// Refresh rate indicates how many times per second the monitor redraws the image.
        /// Common values are 60Hz, 75Hz, 144Hz, etc. Higher refresh rates can result in
        /// smoother visuals.
        /// </summary>
        public int RefreshRate { get; set; }

        /// <summary>
        /// Bits per Pixel for this display mode, indicating the color depth.
        /// Common values are 16-bit, 24-bit, and 32-bit. Higher bit depths allow for
        /// more colors to be displayed.
        /// </summary>
        public int BitsPerPixel { get; set; }

        /// <summary>
        /// Creates a new DisplayMode object, using the specified width, height, refresh rate and
        /// bits per pixel values.
        /// </summary>
        /// <param name="width"> Width of this display mode in pixels. </param>
        /// <param name="height"> Height of this display mode in pixels. </param>
        /// <param name="refreshRate"> The refresh rate. </param>
        /// <param name="bitsPerPixel"> Bits per Pixel. </param>
        public DisplayMode( int width, int height, int refreshRate, int bitsPerPixel )
        {
            Width        = width;
            Height       = height;
            RefreshRate  = refreshRate;
            BitsPerPixel = bitsPerPixel;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Width}x{Height}, bpp: {BitsPerPixel}, hz: {RefreshRate}";
        }
    }
}

// ========================================================================
// ========================================================================