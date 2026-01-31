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

using System.Xml.Linq;
using JetBrains.Annotations;

namespace LughSharp.Core.Maps.Tiled.Loaders;

[PublicAPI]
public class TiledMapProperties
{
    public string? MapVersion         { get; set; }
    public string? TiledVersion       { get; set; }
    public string? MapOrientation     { get; set; }
    public string? RenderOrder        { get; set; }
    public string? NextLayerID        { get; set; }
    public string? NextObjectID       { get; set; }
    public string? HexSideLength      { get; set; }
    public string? StaggerAxis        { get; set; }
    public string? StaggerIndex       { get; set; }
    public string? MapBackgroundColor { get; set; }

    public int MapWidth   { get; set; }
    public int MapHeight  { get; set; }
    public int TileWidth  { get; set; }
    public int TileHeight { get; set; }

    public bool IsInfinite { get; set; }

    // ========================================================================

    public TiledMapProperties( XDocument? doc )
    {
        if ( doc == null )
        {
            return;
        }

        // Find the <map/> root in the TMX file
        var map =  doc.Element( "map" );
        
        MapVersion         = map?.Element( "version" )?.Value;
        TiledVersion       = map?.Element( "tiledversion" )?.Value;
        MapOrientation     = map?.Element( "orientation" )?.Value;
        RenderOrder        = map?.Element( "renderorder" )?.Value;
        NextLayerID        = map?.Element( "nextlayerid" )?.Value;
        NextObjectID       = map?.Element( "nextobjectid" )?.Value;
        HexSideLength      = map?.Element( "hexsidelength" )?.Value;
        StaggerAxis        = map?.Element( "staggeraxis" )?.Value;
        StaggerIndex       = map?.Element( "staggerindex" )?.Value;
        MapBackgroundColor = map?.Element( "backgroundcolor" )?.Value;
        
        MapWidth           = int.Parse( map?.Element( "width" )?.Value! );
        MapHeight          = int.Parse( map?.Element( "height" )?.Value! );
        TileWidth          = int.Parse( map?.Element( "tilewidth" )?.Value! );
        TileHeight         = int.Parse( map?.Element( "tileheight" )?.Value! );
        
        // Is the map 'infinite'? i.e wrap-around scrolling?
        IsInfinite         = map?.Element( "infinite" )?.Value == "1";
    }
}

// ============================================================================
// ============================================================================

