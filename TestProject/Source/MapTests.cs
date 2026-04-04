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

using LughSharp.Core.Graphics.Cameras;
using LughSharp.Core.Maps.Tiled;
using LughSharp.Core.Maps.Tiled.Loaders;
using LughSharp.Core.Maps.Tiled.Renderers;
using LughSharp.Core.Utils.Logging;

namespace TestProject.Source;

[PublicAPI]
public class MapTests
{
    public TmxMapLoader?               TmxMapLoader { get; private set; }
    public OrthogonalTiledMapRenderer? MapRenderer  { get; private set; }
    public OrthographicGameCamera?     TiledMapCam  { get; private set; }
    public TiledMap?                   TiledMap     { get; private set; }

    // ========================================================================

    private int _mapWidth;
    private int _mapHeight;
    private int _mapPosX;
    private int _mapPosY;
    private int _mapDirX = 1;
    private int _mapDirY = 1;

    // ========================================================================

    public void CreateMap()
    {
        TmxMapLoader = new TmxMapLoader();
        TiledMap     = TmxMapLoader.Load( Assets.Room1Map );
        MapRenderer  = new OrthogonalTiledMapRenderer( TiledMap );

        var width      = TiledMap.Properties.Get< int >( "width" );
        var height     = TiledMap.Properties.Get< int >( "height" );
        var tileWidth  = TiledMap.Properties.Get< int >( "tilewidth" );
        var tileHeight = TiledMap.Properties.Get< int >( "tileheight" );

        Logger.Divider( lineCount: 2 );
        Logger.Debug( $"Map Width  : {width}" );
        Logger.Debug( $"Map Height : {height}" );
        Logger.Debug( $"Tile Width : {tileWidth}" );
        Logger.Debug( $"Tile Height: {tileHeight}" );
        Logger.Divider( lineCount: 2 );

        _mapWidth  = width * tileWidth;
        _mapHeight = height * tileHeight;
    }

    public void ScrollMap( ref readonly OrthographicGameCamera? tiledMapCam )
    {
        if ( tiledMapCam != null && TiledMap != null )
        {
            _mapPosX += _mapDirX;

            if ( _mapPosX > _mapWidth && _mapDirX == 1 )
            {
                _mapDirX = -1;
            }
            else if ( _mapPosX <= 0 && _mapDirX == -1 )
            {
                _mapDirX = 1;
            }

            _mapPosY += _mapDirY;

            if ( _mapPosY >= _mapHeight && _mapDirY == 1 )
            {
                _mapDirY = -1;
            }
            else if ( _mapPosY <= 0 && _mapDirY == -1 )
            {
                _mapDirY = 1;
            }

            tiledMapCam.Camera.Translate( _mapDirX, _mapDirY );
        }
    }
}

// ============================================================================
// ============================================================================