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

namespace LughSharp.Source.Maps.Tiled.Objects;

/// <summary>
/// Source Info: Data about the tileset structure
/// </summary>
[PublicAPI]
public record TileMetrics
{
    /// <summary>
    /// The name of the tileset this tile belongs to.
    /// </summary>
    public required string? Name { get; init; }

    /// <summary>
    /// The first tile ID in the tileset.
    /// </summary>
    public required uint Firstgid { get; init; }

    /// <summary>
    /// The width of a tile in the tileset.
    /// </summary>
    public required int Tilewidth { get; init; }

    /// <summary>
    /// The height of a tile in the tileset.
    /// </summary>
    public required int Tileheight { get; init; }

    /// <summary>
    /// The spacing between tiles in the tileset. 
    /// </summary>
    public int Spacing { get; init; }

    /// <summary>
    /// The margin around the edges of the tileset.
    /// </summary>
    public int Margin { get; init; }

    // ========================================================================

    /// <summary>
    /// Default Constructor.
    /// </summary>
    public TileMetrics()
    {
        Spacing = 0;
        Margin  = 0;
    }

    /// <summary>
    /// Represents details about a tileset structure, including its name, tile dimensions,
    /// and spacing/margin configuration.
    /// </summary>
    public TileMetrics( string? name,
                        uint firstGid,
                        int tileWidth,
                        int tileHeight,
                        int spacing = 0,
                        int margin = 0 )
    {
        Name       = name;
        Firstgid   = firstGid;
        Tilewidth  = tileWidth;
        Tileheight = tileHeight;
        Spacing    = spacing;
        Margin     = margin;
    }
}

// ============================================================================
// ============================================================================