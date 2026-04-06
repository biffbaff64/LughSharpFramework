// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Circa64 Software Projects
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

namespace LughSharp.Core.Graphics;

/// <summary>
/// Pixel formats used in graphics rendering. These formats describe the
/// arrangement and size of color channels in an image.
/// </summary>
[PublicAPI]
public class LughFormat
{
    /// <summary>
    /// 
    /// </summary>
    public const int Alpha = 1;

    /// <summary>
    /// 
    /// </summary>
    public const int LuminanceAlpha = 2;

    /// <summary>
    /// 8 bits each for Red, Green, and Blue, with no Alpha.
    /// Stored as RRRR RRRR GGGG GGGG BBBB BBBB.
    /// </summary>
    public const int RGB888 = 3;

    /// <summary>
    /// The standard format, with 8 bits each for Red, Green, Blue, and Alpha.
    /// Stored as RRRR RRRR GGGG GGGG BBBB AAAA.
    /// </summary>
    public const int RGBA8888 = 4;

    /// <summary>
    /// 5 bits for Red, 6 bits for Green, and 5 bits for Blue, with no Alpha.
    /// Stored as RRRR RGGG GGGB BBBB.
    /// </summary>
    public const int RGB565 = 5;

    /// <summary>
    /// Similar to RGBA888 but with 4 bits each for Red, Green, Blue, and Alpha,
    /// resulting in a lower color resolution.
    /// Stored as RRRR GGGG BBBB AAAA.
    /// </summary>
    public const int RGBA4444 = 6;

    /// <summary>
    /// 
    /// </summary>
    public const int IndexedColor = 7;

    // ------------------------------------------

    public const int Default   = RGBA8888;
    public const int Invalid   = -1;
    public const int Undefined = 0;
}

// ============================================================================
// ============================================================================