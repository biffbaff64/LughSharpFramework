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
using LughSharp.Core.Maths;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;

using Color = LughSharp.Core.Graphics.Color;

namespace LughSharp.Core.Maps.Tiled.Renderers;

[PublicAPI]
public class IsometricTiledMapRenderer : BatchTileMapRenderer
{
    private readonly Vector2 _bottomLeft  = new();
    private readonly Vector2 _bottomRight = new();
    private readonly Vector3 _screenPos   = new();
    private readonly Vector2 _topLeft     = new();
    private readonly Vector2 _topRight    = new();

    private Matrix4? _invIsotransform;
    private Matrix4? _isoTransform;

    public IsometricTiledMapRenderer( TiledMap map )
        : base( map )
    {
        Init();
    }

    public IsometricTiledMapRenderer( TiledMap map, IBatch batch )
        : base( map, batch )
    {
        Init();
    }

    public IsometricTiledMapRenderer( TiledMap map, float unitScale )
        : base( map, unitScale )
    {
        Init();
    }

    public IsometricTiledMapRenderer( TiledMap map, float unitScale, IBatch batch )
        : base( map, unitScale, batch )
    {
        Init();
    }

    private void Init()
    {
        // create the isometric transform
        _isoTransform = new Matrix4();
        _isoTransform.ToIdentity();

        // isoTransform.translate(0, 32, 0);
        _isoTransform.Scale(
                            ( float )( Math.Sqrt( 2.0 ) / 2.0 ),
                            ( float )( Math.Sqrt( 2.0 ) / 4.0 ),
                            1.0f
                           );

        _isoTransform.Rotate( 0.0f, 0.0f, 1.0f, -45 );

        // ... and the inverse matrix
        _invIsotransform = new Matrix4( _isoTransform );
        _invIsotransform.Invert();
    }

    private Vector3 TranslateScreenToIsometric( Vector2 vec )
    {
        _screenPos.Set( vec.X, vec.Y, 0 );

        if ( _invIsotransform == null )
        {
            Logger.Debug( "_invIsotransform is null!" );
        }
        else
        {
            _screenPos.Mul( _invIsotransform );
        }

        return _screenPos;
    }

    public override void RenderTileLayer( TiledMapTileLayer layer )
    {
        Color batchColor = Batch.Color;

        float color = Color.ToFloatBitsAbgr( batchColor.R,
                                             batchColor.G,
                                             batchColor.B,
                                             batchColor.A * layer.Opacity );

        float tileWidth    = layer.TileWidth * UnitScale;
        float tileHeight   = layer.TileHeight * UnitScale;
        float layerOffsetX = layer.RenderOffsetX * UnitScale;

        // offset in tiled is y down, so we flip it
        float layerOffsetY = -layer.RenderOffsetY * UnitScale;

        float halfTileWidth  = tileWidth * 0.5f;
        float halfTileHeight = tileHeight * 0.5f;

        // setting up the screen points
        // COL1
        _topRight.Set( ViewBounds.X + ViewBounds.Width - layerOffsetX, ViewBounds.Y - layerOffsetY );

        // COL2
        _bottomLeft.Set( ViewBounds.X - layerOffsetX, ViewBounds.Y + ViewBounds.Height - layerOffsetY );

        // ROW1
        _topLeft.Set( ViewBounds.X - layerOffsetX, ViewBounds.Y - layerOffsetY );

        // ROW2
        _bottomRight.Set( ViewBounds.X + ViewBounds.Width - layerOffsetX,
                          ViewBounds.Y + ViewBounds.Height - layerOffsetY );

        // transforming screen coordinates to iso coordinates
        int row1 = ( int )( TranslateScreenToIsometric( _topLeft ).Y / tileWidth ) - 2;
        int row2 = ( int )( TranslateScreenToIsometric( _bottomRight ).Y / tileWidth ) + 2;

        int col1 = ( int )( TranslateScreenToIsometric( _bottomLeft ).X / tileWidth ) - 2;
        int col2 = ( int )( TranslateScreenToIsometric( _topRight ).X / tileWidth ) + 2;

        for ( int row = row2; row >= row1; row-- )
        {
            for ( int col = col1; col <= col2; col++ )
            {
                float x = ( col * halfTileWidth ) + ( row * halfTileWidth );
                float y = ( row * halfTileHeight ) - ( col * halfTileHeight );

                TiledMapTileLayer.Cell? cell = layer.GetCell( col, row );

                if ( cell == null )
                {
                    return;
                }

                ITiledMapTile? tile = cell.GetTile();

                if ( tile != null )
                {
                    bool flipX     = cell.GetFlipHorizontally();
                    bool flipY     = cell.GetFlipVertically();
                    int  rotations = cell.GetRotation();

                    TextureRegion region = tile.TextureRegion;

                    Guard.Against.Null( region.Texture );

                    float x1 = x + ( tile.OffsetX * UnitScale ) + layerOffsetX;
                    float y1 = y + ( tile.OffsetY * UnitScale ) + layerOffsetY;
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

                    if ( rotations != 0 )
                    {
                        switch ( rotations )
                        {
                            case TiledMapTileLayer.Cell.ROTATE90:
                            {
                                float tempV = Vertices[ IBatch.V1 ];
                                Vertices[ IBatch.V1 ] = Vertices[ IBatch.V2 ];
                                Vertices[ IBatch.V2 ] = Vertices[ IBatch.V3 ];
                                Vertices[ IBatch.V3 ] = Vertices[ IBatch.V4 ];
                                Vertices[ IBatch.V4 ] = tempV;

                                float tempU = Vertices[ IBatch.U1 ];
                                Vertices[ IBatch.U1 ] = Vertices[ IBatch.U2 ];
                                Vertices[ IBatch.U2 ] = Vertices[ IBatch.U3 ];
                                Vertices[ IBatch.U3 ] = Vertices[ IBatch.U4 ];
                                Vertices[ IBatch.U4 ] = tempU;

                                break;
                            }

                            case TiledMapTileLayer.Cell.ROTATE180:
                            {
                                ( Vertices[ IBatch.U1 ], Vertices[ IBatch.U3 ] )
                                    = ( Vertices[ IBatch.U3 ], Vertices[ IBatch.U1 ] );

                                ( Vertices[ IBatch.U2 ], Vertices[ IBatch.U4 ] )
                                    = ( Vertices[ IBatch.U4 ], Vertices[ IBatch.U2 ] );

                                ( Vertices[ IBatch.V1 ], Vertices[ IBatch.V3 ] )
                                    = ( Vertices[ IBatch.V3 ], Vertices[ IBatch.V1 ] );

                                ( Vertices[ IBatch.V2 ], Vertices[ IBatch.V4 ] )
                                    = ( Vertices[ IBatch.V4 ], Vertices[ IBatch.V2 ] );

                                break;
                            }

                            case TiledMapTileLayer.Cell.ROTATE270:
                            {
                                float tempV = Vertices[ IBatch.V1 ];
                                Vertices[ IBatch.V1 ] = Vertices[ IBatch.V4 ];
                                Vertices[ IBatch.V4 ] = Vertices[ IBatch.V3 ];
                                Vertices[ IBatch.V3 ] = Vertices[ IBatch.V2 ];
                                Vertices[ IBatch.V2 ] = tempV;

                                float tempU = Vertices[ IBatch.U1 ];
                                Vertices[ IBatch.U1 ] = Vertices[ IBatch.U4 ];
                                Vertices[ IBatch.U4 ] = Vertices[ IBatch.U3 ];
                                Vertices[ IBatch.U3 ] = Vertices[ IBatch.U2 ];
                                Vertices[ IBatch.U2 ] = tempU;

                                break;
                            }
                        }
                    }

                    Batch.Draw( region.Texture, Vertices, 0, NUM_VERTICES );
                }
            }
        }
    }
}

// ============================================================================
// ============================================================================