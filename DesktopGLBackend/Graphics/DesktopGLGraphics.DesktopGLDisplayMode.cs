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

using LughSharp.Core.Graphics;

namespace DesktopGLBackend.Graphics;

public partial class DesktopGLGraphics
{
    /// <summary>
    /// Describes a Display Mode for a <see cref="DotGLFW.Monitor"/>
    /// </summary>
    [PublicAPI]
    public class DesktopGLDisplayMode : IGraphicsDevice.DisplayMode
    {
        /// <summary>
        /// The <see cref="DotGLFW.Monitor"/> this <see cref="IGraphicsDevice.DisplayMode"/> applies to.
        /// </summary>
        public DotGLFW.Monitor MonitorHandle { get; set; }

        /// <summary>
        /// Creates a new Display Mode and its properties.
        /// </summary>
        /// <param name="monitor"> The target monitor. </param>
        /// <param name="width"> Monitor display width. </param>
        /// <param name="height"> Monior display height. </param>
        /// <param name="refreshRate"> The refresh rate. </param>
        /// <param name="bitsPerPixel"> The bits per pixel. </param>
        public DesktopGLDisplayMode( DotGLFW.Monitor monitor,
                                     int width,
                                     int height,
                                     int refreshRate,
                                     int bitsPerPixel )
            : base( width, height, refreshRate, bitsPerPixel )
        {
            MonitorHandle = monitor;
        }
    }
}

// ============================================================================
// ============================================================================