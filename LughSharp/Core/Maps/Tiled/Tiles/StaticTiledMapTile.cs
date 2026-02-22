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

using JetBrains.Annotations;

using LughSharp.Core.Graphics;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Maps.Tiled.Tiles;

/// <summary>
/// Represents a non changing <see cref="ITiledMapTile"/> (can be cached)
/// </summary>
[PublicAPI]
public class StaticTiledMapTile : ITiledMapTile
{
    public uint                    ID            { get; set; }
    public float                   OffsetX       { get; set; }
    public float                   OffsetY       { get; set; }
    public TextureRegion           TextureRegion { get; set; }
    public ITiledMapTile.Blendmode BlendMode     { get; set; } = ITiledMapTile.Blendmode.Alpha;

    // ========================================================================

    /// <summary>
    /// Creates a static tile with the given region
    /// </summary>
    /// <param name="texture"> The <see cref="TextureRegion"/> to use. </param>
    public StaticTiledMapTile( TextureRegion texture )
    {
        TextureRegion = texture;
    }

    /// <summary>
    /// Copy Constructor
    /// </summary>
    /// <param name="copy"> The StaticTiledMapTile to copy. </param>
    public StaticTiledMapTile( StaticTiledMapTile copy )
    {
        Guard.Against.Null( copy );

        Properties    = copy.Properties;
        MapObjects    = copy.MapObjects;
        TextureRegion = copy.TextureRegion;
        ID            = copy.ID;
    }

    /// <summary>
    /// Returns the <see cref="MapProperties"/> instance for this tile. If the
    /// current properties instance is null, a new instance will be created first.
    /// </summary>
    public MapProperties? Properties
    {
        get => field ??= new MapProperties();
        set;
    } = new();

    /// <summary>
    /// Returns the <see cref="MapObjects"/> instance for this tile. If the
    /// current mapobjects instance is null, a new instance will be created first.
    /// </summary>
    public MapObjects? MapObjects
    {
        get => field ??= new MapObjects();
        set;
    } = new();
}

// ============================================================================
// ============================================================================