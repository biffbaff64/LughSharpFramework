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

using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.SysDraw;

/// <summary>
///  Capabilities and properties of images.
/// </summary>
[PublicAPI]
public class ImageCapabilities : ICloneable
{
    /// <summary>
    /// Returns <c>true</c> if the object whose capabilities are
    /// encapsulated in this <c>ImageCapabilities</c> can be or is
    /// accelerated.
    /// </summary>
    /// <returns>
    /// whether or not an image can be, or is, accelerated.  There are
    /// various platform-specific ways to accelerate an image, including
    /// pixmaps, VRAM, AGP.  This is the general acceleration method (as
    /// opposed to residing in system memory).
    /// </returns>
    public bool IsAccelerated { get; init; } = false;

    /// <summary>
    /// Returns <c>true</c> if the <c>VolatileImage</c>
    /// described by this <c>ImageCapabilities</c> can lose
    /// its surfaces.
    /// </summary>
    /// <returns>
    /// whether or not a volatile image is subject to losing its surfaces
    /// at the whim of the operating system.
    /// </returns>
    public bool IsTrueVolatile { get; init; } = false;

    // ========================================================================

    /// <summary>
    /// Creates a new object for specifying image capabilities.
    /// </summary>
    /// <param name="accelerated"> whether or not an accelerated image is desired </param>
    public ImageCapabilities( bool accelerated )
    {
        IsAccelerated = accelerated;
    }

    /// <summary>
    /// Returns a copy of this ImageCapabilities object.
    /// </summary>
    public object Clone()
    {
        try
        {
            return MemberwiseClone();
        }
        catch ( Exception e )
        {
            // Since we implement ICloneable, this should never happen
            throw new GdxRuntimeException( e );
        }
    }
}

// ========================================================================
// ========================================================================