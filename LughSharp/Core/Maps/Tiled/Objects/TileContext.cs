// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Circa64 Software Projects / Richard Ikin.
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
/// Context: The "Big" objects required for processing
/// </summary>
[PublicAPI]
public record TileContext
{
    public required FileInfo        TmxFile       { get; init; }
    public required IImageResolver  ImageResolver { get; init; }
    public required TiledMapTileSet Tileset       { get; init; }

    // ========================================================================

    /// <summary>
    /// Default Constructor.
    /// </summary>
    public TileContext()
    {
    }

    /// <summary>
    /// Context: The "Big" objects required for processing
    /// </summary>
    /// <param name="TmxFile"></param>
    /// <param name="ImageResolver"></param>
    /// <param name="TileSet"></param>
    public TileContext( FileInfo TmxFile, IImageResolver ImageResolver, TiledMapTileSet TileSet )
    {
        this.TmxFile       = TmxFile;
        this.ImageResolver = ImageResolver;
        Tileset            = TileSet;
    }

    public void Deconstruct( out FileInfo tmxFile,
                             out IImageResolver imageResolver,
                             out TiledMapTileSet tileSet )
    {
        tmxFile       = TmxFile;
        imageResolver = ImageResolver;
        tileSet       = Tileset;
    }
}

// ============================================================================
// ============================================================================