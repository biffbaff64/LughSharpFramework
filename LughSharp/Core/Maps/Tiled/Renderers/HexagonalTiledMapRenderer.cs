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
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Maps.Tiled.Tiles;
using LughSharp.Core.Utils.Exceptions;

using Color = LughSharp.Core.Graphics.Color;

namespace LughSharp.Core.Maps.Tiled.Renderers;

[PublicAPI]
public class HexagonalTiledMapRenderer : BatchTileMapRenderer
{
    /// <summary>
    /// the parameter defining the shape of the hexagon from tiled. more
    /// specifically it represents the length of the sides that are parallel
    /// to the stagger axis. e.g. with respect to the stagger axis a value
    /// of 0 results in a rhombus shape, while a value equal to the tile
    /// length/height represents a square shape and a value of 0.5 represents
    /// a regular hexagon if tile length equals tile height
    /// </summary>
    private float _hexSideLength;

    /// <summary>
    /// true for X-Axis, false for Y-Axis
    /// </summary>
    private bool _staggerAxisX = true;

    /// <summary>
    /// true for even StaggerIndex, false for odd
    /// </summary>
    private bool _staggerIndexEven;

    // ========================================================================

    public HexagonalTiledMapRenderer( TiledMap map ) : base( map )
    {
        Init( map );
    }

    public HexagonalTiledMapRenderer( TiledMap map, float unitScale )
        : base( map, unitScale )
    {
        Init( map );
    }

    public HexagonalTiledMapRenderer( TiledMap map, IBatch batch )
        : base( map, batch )
    {
        Init( map );
    }

    public HexagonalTiledMapRenderer( TiledMap map, float unitScale, IBatch batch )
        : base( map, unitScale, batch )
    {
        Init( map );
    }

    // ========================================================================

    private void Init( TiledMap map )
    {
        var axis = map.Properties.Get< string >( "staggeraxis" );

        if ( axis != null )
        {
            _staggerAxisX = axis.Equals( "x" );
        }

        var index = map.Properties.Get< string >( "staggerindex" );

        if ( index != null )
        {
            _staggerIndexEven = index.Equals( "even" );
        }

        int? length = map.Properties.Get< int >( "hexsidelength" );

        if ( length != null )
        {
            _hexSideLength = length.Value;
        }
        else
        {
            if ( _staggerAxisX )
            {
                length = map.Properties.Get< int >( "tilewidth" );

                if ( length != null )
                {
                    _hexSideLength = 0.5f * length.Value;
                }
                else
                {
                    var tmtl = ( TiledMapTileLayer )map.Layers.Get( 0 );
                    _hexSideLength = 0.5f * tmtl.TileWidth;
                }
            }
            else
            {
                length = map.Properties.Get< int >( "tileheight" );

                if ( length != null )
                {
                    _hexSideLength = 0.5f * length.Value;
                }
                else
                {
                    var tmtl = ( TiledMapTileLayer )map.Layers.Get( 0 );
                    _hexSideLength = 0.5f * tmtl.TileHeight;
                }
            }
        }
    }

    /// <inheritdoc />
    public override void RenderTileLayer( TiledMapTileLayer layer )
    {
        float color =
            Color.ToFloatBitsAbgr( Batch.Color.R, Batch.Color.G, Batch.Color.B, Batch.Color.A * layer.Opacity );

        int   layerWidth      = layer.Width;
        int   layerHeight     = layer.Height;
        float layerTileWidth  = layer.TileWidth * UnitScale;
        float layerTileHeight = layer.TileHeight * UnitScale;
        float layerOffsetX    = layer.RenderOffsetX * UnitScale;
        float layerOffsetY    = -layer.RenderOffsetY * UnitScale; // offset in tiled is y down, so we flip it
        float layerHexLength  = _hexSideLength * UnitScale;

        if ( _staggerAxisX )
        {
            float tileWidthLowerCorner = ( layerTileWidth - layerHexLength ) / 2;
            float tileWidthUpperCorner = ( layerTileWidth + layerHexLength ) / 2;
            float layerTileHeight50    = layerTileHeight * 0.50f;

            int row1 = Math.Max( 0, ( int )( ( ViewBounds.Y - layerTileHeight50 - layerOffsetX ) / layerTileHeight ) );

            int row2 = Math.Min( layerHeight,
                                 ( int )( ( ViewBounds.Y + ViewBounds.Height + layerTileHeight - layerOffsetX )
                                        / layerTileHeight ) );

            int col1 = Math.Max( 0,
                                 ( int )( ( ViewBounds.X - tileWidthLowerCorner - layerOffsetY )
                                        / tileWidthUpperCorner ) );

            int col2 = Math.Min( layerWidth,
                                 ( int )( ( ViewBounds.X + ViewBounds.Width + tileWidthUpperCorner - layerOffsetY )
                                        / tileWidthUpperCorner ) );

            // depending on the stagger index either draw all even before the odd or vice versa
            int colA = _staggerIndexEven == ( ( col1 % 2 ) == 0 ) ? col1 + 1 : col1;
            int colB = _staggerIndexEven == ( ( col1 % 2 ) == 0 ) ? col1 : col1 + 1;

            for ( int row = row2 - 1; row >= row1; row-- )
            {
                for ( int col = colA; col < col2; col += 2 )
                {
                    RenderCell( layer.GetCell( col, row ),
                                ( tileWidthUpperCorner * col ) + layerOffsetX,
                                layerTileHeight50 + ( layerTileHeight * row ) + layerOffsetY,
                                color );
                }

                for ( int col = colB; col < col2; col += 2 )
                {
                    RenderCell( layer.GetCell( col, row ),
                                ( tileWidthUpperCorner * col ) + layerOffsetX,
                                ( layerTileHeight * row ) + layerOffsetY,
                                color );
                }
            }
        }
        else
        {
            float tileHeightLowerCorner = ( layerTileHeight - layerHexLength ) / 2;
            float tileHeightUpperCorner = ( layerTileHeight + layerHexLength ) / 2;
            float layerTileWidth50      = layerTileWidth * 0.50f;

            int row1 = Math.Max( 0,
                                 ( int )( ( ViewBounds.Y - tileHeightLowerCorner - layerOffsetX )
                                        / tileHeightUpperCorner ) );

            int row2 = Math.Min( layerHeight,
                                 ( int )( ( ViewBounds.Y + ViewBounds.Height + tileHeightUpperCorner - layerOffsetX )
                                        / tileHeightUpperCorner ) );

            int col1 = Math.Max( 0, ( int )( ( ViewBounds.X - layerTileWidth50 - layerOffsetY ) / layerTileWidth ) );

            int col2 = Math.Min( layerWidth,
                                 ( int )( ( ViewBounds.X + ViewBounds.Width + layerTileWidth - layerOffsetY )
                                        / layerTileWidth ) );

            for ( int row = row2 - 1; row >= row1; row-- )
            {
                // depending on the stagger index either shift for even or uneven indexes
                float shiftX = ( row % 2 ) == 0 == _staggerIndexEven ? layerTileWidth50 : 0;

                for ( int col = col1; col < col2; col++ )
                {
                    RenderCell( layer.GetCell( col, row ),
                                ( layerTileWidth * col ) + shiftX + layerOffsetX,
                                ( tileHeightUpperCorner * row ) + layerOffsetY,
                                color );
                }
            }
        }
    }

    private void RenderCell( in TiledMapTileLayer.Cell? cell, in float x, in float y, in float color )
    {
        Guard.Against.Null( cell );

        ITiledMapTile? tile = cell.GetTile();

        if ( tile != null )
        {
            if ( tile is AnimatedTiledMapTile )
            {
                return;
            }

            bool flipX     = cell.GetFlipHorizontally();
            bool flipY     = cell.GetFlipVertically();
            int  rotations = cell.GetRotation();

            TextureRegion region = tile.TextureRegion;

            Guard.Against.Null( region.Texture );

            float x1 = x + ( tile.OffsetX * UnitScale );
            float y1 = y + ( tile.OffsetY * UnitScale );
            float x2 = x1 + ( region.GetRegionWidth() * UnitScale );
            float y2 = y1 + ( region.GetRegionHeight() * UnitScale );

            float u1 = region.U;
            float v1 = region.V2;
            float u2 = region.U2;
            float v2 = region.V;

            Vertices[ IBatch.X1 ] = x1;
            Vertices[ IBatch.Y1 ] = y1;
            Vertices[ IBatch.C1 ] = color;
            Vertices[ IBatch.U1 ] = u1;
            Vertices[ IBatch.V1 ] = v1;

            Vertices[ IBatch.X2 ] = x1;
            Vertices[ IBatch.Y2 ] = y2;
            Vertices[ IBatch.C2 ] = color;
            Vertices[ IBatch.U2 ] = u1;
            Vertices[ IBatch.V2 ] = v2;

            Vertices[ IBatch.X3 ] = x2;
            Vertices[ IBatch.Y3 ] = y2;
            Vertices[ IBatch.C3 ] = color;
            Vertices[ IBatch.U3 ] = u2;
            Vertices[ IBatch.V3 ] = v2;

            Vertices[ IBatch.X4 ] = x2;
            Vertices[ IBatch.Y4 ] = y1;
            Vertices[ IBatch.C4 ] = color;
            Vertices[ IBatch.U4 ] = u2;
            Vertices[ IBatch.V4 ] = v1;

            if ( flipX )
            {
                ( Vertices[ IBatch.U1 ], Vertices[ IBatch.U3 ] )
                    = ( Vertices[ IBatch.U3 ], Vertices[ IBatch.U1 ] );

                ( Vertices[ IBatch.U2 ], Vertices[ IBatch.U4 ] )
                    = ( Vertices[ IBatch.U4 ], Vertices[ IBatch.U2 ] );
            }

            if ( flipY )
            {
                ( Vertices[ IBatch.V1 ], Vertices[ IBatch.V3 ] )
                    = ( Vertices[ IBatch.V3 ], Vertices[ IBatch.V1 ] );

                ( Vertices[ IBatch.V2 ], Vertices[ IBatch.V4 ] )
                    = ( Vertices[ IBatch.V4 ], Vertices[ IBatch.V2 ] );
            }

            if ( rotations == 2 )
            {
                ( Vertices[ IBatch.U1 ], Vertices[ IBatch.U3 ] )
                    = ( Vertices[ IBatch.U3 ], Vertices[ IBatch.U1 ] );

                ( Vertices[ IBatch.U2 ], Vertices[ IBatch.U4 ] )
                    = ( Vertices[ IBatch.U4 ], Vertices[ IBatch.U2 ] );

                ( Vertices[ IBatch.V1 ], Vertices[ IBatch.V3 ] )
                    = ( Vertices[ IBatch.V3 ], Vertices[ IBatch.V1 ] );

                ( Vertices[ IBatch.V2 ], Vertices[ IBatch.V4 ] )
                    = ( Vertices[ IBatch.V4 ], Vertices[ IBatch.V2 ] );
            }

            Batch.Draw( region.Texture, Vertices, 0, NUM_VERTICES );
        }
    }
}

// ============================================================================
// ============================================================================