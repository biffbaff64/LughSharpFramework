// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Richard Ikin.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ///////////////////////////////////////////////////////////////////////////////

using System.IO;

using JetBrains.Annotations;

namespace LughSharp.Core.Maps.Tiled.Objects;

/// <summary>
/// Image Metadata: Details about the specific texture being used
/// </summary>
[PublicAPI]
public record ImageDetails
{
    public required string?   ImageSource { get; init; }
    public required int       Width       { get; init; }
    public required int       Height      { get; init; }
    public required FileInfo? Image       { get; init; }

    // ========================================================================

    /// <summary>
    /// Default Constructor.
    /// </summary>
    public ImageDetails()
    {
    }

    /// <summary>
    /// Image Metadata: Details about the specific texture being used
    /// </summary>
    /// <param name="ImageSource"> The TileSet source image. </param>
    /// <param name="Width"> Image width in pixels. </param>
    /// <param name="Height"> Image height in pixels. </param>
    /// <param name="Image"> FileInfo object pointing to the image. </param>
    public ImageDetails( string ImageSource, int Width, int Height, FileInfo? Image )
    {
        this.ImageSource = ImageSource;
        this.Width       = Width;
        this.Height      = Height;
        this.Image       = Image;
    }

    public void Deconstruct( out string? imageSource,
                             out int width,
                             out int height,
                             out FileInfo? image )
    {
        imageSource = this.ImageSource;
        width       = this.Width;
        height      = this.Height;
        image       = this.Image;
    }
}

// ============================================================================
// ============================================================================