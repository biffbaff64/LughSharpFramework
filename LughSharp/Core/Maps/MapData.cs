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

using LughSharp.Core.Utils.XML;

namespace LughSharp.Core.Maps;

[PublicAPI]
public sealed class MapData
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
    public int     MapWidth           { get; set; }
    public int     MapHeight          { get; set; }
    public int     TileWidth          { get; set; }
    public int     TileHeight         { get; set; }
    public bool    IsInfinite         { get; set; }

    // ========================================================================

    public MapData( XmlReader.Element? element )
    {
        if ( element == null )
        {
            return;
        }

        MapVersion         = element.GetAttribute( "version", null );
        TiledVersion       = element.GetAttribute( "tiledversion", null );
        MapOrientation     = element.GetAttribute( "orientation", null );
        RenderOrder        = element.GetAttribute( "renderorder", null );
        NextLayerID        = element.GetAttribute( "nextlayerid", null );
        NextObjectID       = element.GetAttribute( "nextobjectid", null );
        HexSideLength      = element.GetAttribute( "hexsidelength", null );
        StaggerAxis        = element.GetAttribute( "staggeraxis", null );
        StaggerIndex       = element.GetAttribute( "staggerindex", null );
        MapBackgroundColor = element.GetAttribute( "backgroundcolor", null );
        IsInfinite         = element.GetAttribute( "infinite", null ) == "1";
        MapWidth           = element.GetIntAttribute( "width" );
        MapHeight          = element.GetIntAttribute( "height" );
        TileWidth          = element.GetIntAttribute( "tilewidth" );
        TileHeight         = element.GetIntAttribute( "tileheight" );
    }
}

// ============================================================================
// ============================================================================