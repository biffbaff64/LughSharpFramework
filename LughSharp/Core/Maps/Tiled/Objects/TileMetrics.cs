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

using JetBrains.Annotations;

namespace LughSharp.Core.Maps.Tiled.Objects;

/// <summary>
/// Source Info: Data about the tileset structure
/// </summary>
[PublicAPI]
public record TileMetrics
{
    public required string? Name       { get; init; }
    public required uint    Firstgid   { get; init; }
    public required int     Tilewidth  { get; init; }
    public required int     Tileheight { get; init; }
    public          int     Spacing    { get; init; }
    public          int     Margin     { get; init; }

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
    /// Source Info: Data about the tileset structure
    /// </summary>
    /// <param name="name"></param>
    /// <param name="firstGid"></param>
    /// <param name="tileWidth"></param>
    /// <param name="tileHeight"></param>
    /// <param name="spacing"></param>
    /// <param name="margin"></param>
    public TileMetrics( string? name,
                           uint firstGid,
                           int tileWidth,
                           int tileHeight,
                           int spacing = 0,
                           int margin = 0 )
    {
        this.Name       = name;
        this.Firstgid   = firstGid;
        this.Tilewidth  = tileWidth;
        this.Tileheight = tileHeight;
        this.Spacing    = spacing;
        this.Margin     = margin;
    }

    public void Deconstruct( out string? name,
                             out uint firstGid,
                             out int tileWidth,
                             out int tileHeight,
                             out int spacing,
                             out int margin )
    {
        name       = this.Name;
        firstGid   = this.Firstgid;
        tileWidth  = this.Tilewidth;
        tileHeight = this.Tileheight;
        spacing    = this.Spacing;
        margin     = this.Margin;
    }
}

[PublicAPI]
public record Tile( int Id, int Width, int Height );

// ============================================================================
// ============================================================================