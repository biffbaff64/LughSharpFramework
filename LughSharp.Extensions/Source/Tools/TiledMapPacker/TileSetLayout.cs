// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin
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
using LughSharp.Core.Maps.Tiled;
using LughSharp.Core.Maths;
using LughSharp.Core.Utils.Collections;
using LughSharp.Core.Utils.Exceptions;
using SixLabors.ImageSharp;

namespace Extensions.Source.Tools.TiledMapPacker;

/// <summary>
/// Contains extra information that can only be calculated after a Tiled
/// Map's tile set images are loaded.
/// </summary>
[PublicAPI]
public class TileSetLayout
{
    public Image Image    { get; private set; }
    public int   NumTiles { get; private set; }
    public int   Firstgid { get; private set; }
    public int   NumRows  { get; private set; }
    public int   NumCols  { get; private set; }

    // ========================================================================

    private Dictionary< int, Vector2 > _imageTilePositions;

    // ========================================================================

    /// <summary>
    /// Constructs a Tile Set layout. The tile set image contained in the baseDir should
    /// be the original tile set images before being processed by <see cref="TiledMapPacker"/>
    /// (the ones actually read by Tiled).
    /// </summary>
    /// <param name="firstgid"></param>
    /// <param name="tileset"> the tile set to process </param>
    /// <param name="baseDir"> the directory in which the tile set image is stored </param>
    protected TileSetLayout( int firstgid, TiledMapTileSet tileset, FileInfo baseDir )
    {
        var tileWidth  = tileset.Properties.Get< int >( "tilewidth" );
        var tileHeight = tileset.Properties.Get< int >( "tileheight" );
        var margin     = tileset.Properties.Get< int >( "margin" );
        var spacing    = tileset.Properties.Get< int >( "spacing" );

        this.Firstgid = firstgid;

        var imagePath = tileset.Properties.Get< string >( "imagesource" );

        Guard.Against.NullOrEmpty( imagePath );

        Image = Image.Load( imagePath );

        _imageTilePositions = new Dictionary< int, Vector2 >();

        // fill the tile regions
        int y;
        var tile = 0;

        NumRows = 0;
        NumCols = 0;

        var stopWidth  = Image.Width - tileWidth;
        var stopHeight = Image.Height - tileHeight;

        for ( y = margin; y <= stopHeight; y += tileHeight + spacing )
        {
            for ( int x = margin; x <= stopWidth; x += tileWidth + spacing )
            {
                if ( y == margin )
                {
                    NumCols++;
                }

                _imageTilePositions[ tile ] = new Vector2( x, y );
                tile++;
            }

            NumRows++;
        }

        NumTiles = NumRows * NumCols;
    }

    /// <summary>
    /// Returns the location of the tile in <see cref="TileSetLayout.Image"/>
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public Vector2 GetLocation( int tile )
    {
        return _imageTilePositions[ tile - Firstgid ];
    }
}

// ============================================================================
// ============================================================================