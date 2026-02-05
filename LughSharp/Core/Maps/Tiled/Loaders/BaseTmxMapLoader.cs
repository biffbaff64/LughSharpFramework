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

using System.IO.Compression;
using System.Xml;

using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

using JetBrains.Annotations;

using LughSharp.Core.Assets;
using LughSharp.Core.Assets.Loaders;
using LughSharp.Core.Assets.Loaders.Resolvers;
using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.OpenGL.Enums;
using LughSharp.Core.Maps.Objects;
using LughSharp.Core.Maps.Tiled.Objects;
using LughSharp.Core.Maps.Tiled.Tiles;
using LughSharp.Core.Maths;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;

using Color = LughSharp.Core.Graphics.Color;
using XmlReader = LughSharp.Core.Utils.XML.XmlReader;

namespace LughSharp.Core.Maps.Tiled.Loaders;

[PublicAPI]
public abstract class BaseTmxMapLoader< TP >( IFileHandleResolver resolver )
    : AsynchronousAssetLoader( resolver ) where TP : BaseTmxMapLoader< TP >.BaseTmxLoaderParameters
{
    // ========================================================================
    // ========================================================================

    protected const uint FLAG_FLIP_HORIZONTALLY = 0x80000000;
    protected const uint FLAG_FLIP_VERTICALLY   = 0x40000000;
    protected const uint FLAG_FLIP_DIAGONALLY   = 0x20000000;
    protected const uint MASK_CLEAR             = 0xE0000000;

    // ========================================================================
    // ========================================================================

    protected bool               ConvertObjectToTileSpace;
    protected bool               FlipY       = true;
    protected XmlDocument        XmlDocument = new();
    protected XmlReader          XmlReader   = new();
    protected XmlReader.Element? XmlRoot;

    public int                           MapTileWidth        { get; set; }
    public int                           MapTileHeight       { get; set; }
    public int                           MapWidthInPixels    { get; set; }
    public int                           MapHeightInPixels   { get; set; }
    public TiledMap?                     Map                 { get; set; }
    public Dictionary< int, MapObject >? IdToObject          { get; set; }
    public List< Action >?               RunOnEndOfLoadTiled { get; set; }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Loads the map data, given the XML root element.
    /// </summary>
    /// <param name="tmxFile">The Filehandle of the tmx file </param>
    /// <param name="parameter"></param>
    /// <param name="imageResolver"></param>
    /// <returns>The <see cref="TiledMap"/>.</returns>
    protected TiledMap LoadTiledMap( FileInfo? tmxFile, TP? parameter, IImageResolver imageResolver )
    {
        Logger.Checkpoint();

        Map                 = new TiledMap();
        IdToObject          = new Dictionary< int, MapObject >();
        RunOnEndOfLoadTiled = new List< Action >();

        if ( parameter != null )
        {
            ConvertObjectToTileSpace = parameter.ConvertObjectToTileSpace;
            FlipY                    = parameter.FlipY;
        }
        else
        {
            ConvertObjectToTileSpace = false;
            FlipY                    = true;
        }

        UpdateMapProperties();

        MapTileWidth      = XmlRoot?.GetIntAttribute( "tilewidth" ) ?? 0;
        MapTileHeight     = XmlRoot?.GetIntAttribute( "tileheight" ) ?? 0;
        MapWidthInPixels  = XmlRoot?.GetIntAttribute( "width" ) * MapTileWidth ?? 0;
        MapHeightInPixels = XmlRoot?.GetIntAttribute( "height" ) * MapTileHeight ?? 0;

        var mapOrientation = XmlRoot?.GetAttribute( "orientation", null );

        if ( !string.IsNullOrEmpty( mapOrientation ) )
        {
            if ( "staggered".Equals( mapOrientation ) )
            {
                if ( MapHeightInPixels > 1 )
                {
                    MapWidthInPixels  += MapTileWidth / 2;
                    MapHeightInPixels =  ( MapHeightInPixels / 2 ) + ( MapTileHeight / 2 );
                }
            }
        }

        var properties = XmlRoot?.GetChildByName( "properties" );

        if ( properties != null )
        {
            LoadProperties( Map.Properties, properties );
        }

        var tilesetList = XmlRoot?.GetChildrenByName( "tileset" );

        if ( tilesetList != null && tmxFile != null )
        {
            foreach ( var tset in tilesetList )
            {
                LoadTileSet( tset, tmxFile, imageResolver );

                XmlRoot?.RemoveChild( tset );
            }
        }

        var groups = Map.Layers.GetByType< MapGroupLayer >();

        while ( groups.Count > 0 )
        {
            var group = groups.First();
            groups.RemoveAt( 0 );

            foreach ( var child in group.Layers )
            {
                child.ParallaxX *= group.ParallaxX;
                child.ParallaxY *= group.ParallaxY;

                if ( child is MapGroupLayer groupChild )
                {
                    groups.Add( groupChild );
                }
            }
        }

        foreach ( var action in RunOnEndOfLoadTiled )
        {
            action();
        }

        RunOnEndOfLoadTiled.Clear();
        RunOnEndOfLoadTiled = null;

        return Map;
    }

    private void UpdateMapProperties()
    {
        if ( Map == null )
        {
            throw new RuntimeException( "Map cannot be null!" );
        }
        
        Map.Properties.Put( "version", XmlRoot?.GetAttribute( "version" ) );
        Map.Properties.Put( "tiledversion", XmlRoot?.GetAttribute( "tiledversion" ) );

        var mapOrientation = XmlRoot?.GetAttribute( "orientation", null );
        Map.Properties.Put( "orientation", mapOrientation );
        
        Map.Properties.Put( "width", XmlRoot?.GetIntAttribute( "width" ) );
        Map.Properties.Put( "height", XmlRoot?.GetIntAttribute( "height" ) );
        Map.Properties.Put( "tilewidth", XmlRoot?.GetIntAttribute( "tilewidth" ) );
        Map.Properties.Put( "tileheight", XmlRoot?.GetIntAttribute( "tileheight" ) );
        Map.Properties.Put( "hexsidelength", XmlRoot?.GetIntAttribute( "hexsidelength" ) );

        var staggerAxis = XmlRoot?.GetAttribute( "staggeraxis", null );

        if ( !string.IsNullOrEmpty( staggerAxis ) )
        {
            Map.Properties.Put( "staggeraxis", staggerAxis );
        }

        var staggerIndex = XmlRoot?.GetAttribute( "staggerindex", null );

        if ( !string.IsNullOrEmpty( staggerIndex ) )
        {
            Map.Properties.Put( "staggerindex", staggerIndex );
        }

        var backgroundColor = XmlRoot?.GetAttribute( "backgroundcolor", null );

        if ( !string.IsNullOrEmpty( backgroundColor ) )
        {
            Map.Properties.Put( "backgroundcolor", backgroundColor );
        }
        
        Map.Properties.Put( "infinite", XmlRoot?.GetAttribute( "infinite" ) );
        Map.Properties.Put( "nextLayerID", XmlRoot?.GetAttribute( "nextlayerid" ) );
        Map.Properties.Put( "nextObjectID", XmlRoot?.GetAttribute( "nextobjectid" ) );
        Map.Properties.Put( "renderorder", XmlRoot?.GetAttribute( "renderorder" ) );
    }
    
    public List< AssetDescriptor >? GetDependencies( string filename, FileInfo tmxFile, TP? parameter )
    {
        XmlRoot = XmlReader.Parse( tmxFile.FullName );

        var textureParameter = new TextureLoader.TextureLoaderParameters();

        if ( parameter != null )
        {
            textureParameter.GenMipMaps = parameter.GenerateMipMaps;
            textureParameter.MinFilter  = parameter.TextureMinFilter;
            textureParameter.MagFilter  = parameter.TextureMagFilter;
        }

        return GetDependencyAssetDescriptors( tmxFile, textureParameter );
    }

    public virtual List< AssetDescriptor >? GetDependencyAssetDescriptors( FileInfo tmxFile,
                                                                           TextureLoader.TextureLoaderParameters
                                                                               textureLoaderParameters )
    {
        return null;
    }

    // ========================================================================
    // ========================================================================
    // Load map components - layers, tilesets, groups etc

    protected void LoadLayer( TiledMap map, MapLayers parentLayers,
                              XmlReader.Element? element,
                              FileInfo tmxFile,
                              IImageResolver imageResolver )
    {
        switch ( element?.Name )
        {
            case "group":
                LoadLayerGroup( map, parentLayers, element, tmxFile, imageResolver );

                break;

            case "layer":
                LoadTileLayer( map, parentLayers, element );

                break;

            case "objectgroup":
                LoadObjectGroup( map, parentLayers, element );

                break;

            case "imagelayer":
                LoadImageLayer( map, parentLayers, element, tmxFile, imageResolver );

                break;

            default:
                throw new RuntimeException( $"Unknown layer type: {element?.Name}" );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="map"></param>
    /// <param name="parentLayers"> The actual layer group belonging to the map. </param>
    /// <param name="element"> The xml node being processed. </param>
    /// <param name="tmxFile"></param>
    /// <param name="imageResolver"></param>
    protected void LoadLayerGroup( TiledMap map,
                                   MapLayers parentLayers,
                                   XmlReader.Element element,
                                   FileInfo tmxFile,
                                   IImageResolver imageResolver )
    {
        Guard.Against.Null( element );

        if ( element.Name.Equals( "group" ) )
        {
            var groupLayer = new MapGroupLayer();

            LoadBasicLayerInfo( groupLayer, element );

            XmlReader.Element? properties;

            if ( ( properties = element.GetChildByName( "properties" ) ) != null )
            {
                LoadProperties( groupLayer.Properties, properties );
            }

            for ( int i = 0, j = element.GetChildCount(); i < j; i++ )
            {
                var child = element.GetChild( i );

                LoadLayer( map, groupLayer.Layers, child, tmxFile, imageResolver );
            }

            foreach ( var layer in groupLayer.Layers )
            {
                layer.Parent = groupLayer;
            }

            parentLayers.Add( groupLayer );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="map"></param>
    /// <param name="parentLayers"> The actual layer group belonging to the map. </param>
    /// <param name="element"> The xml node being processed. </param>
    /// <exception cref="ArgumentException"></exception>
    protected void LoadTileLayer( TiledMap map, MapLayers parentLayers, XmlReader.Element element )
    {
        if ( element.Name == null )
        {
            throw new ArgumentException( "node.Name cannot by null!" );
        }

        if ( element.Name.Equals( "layer" ) )
        {
            var width  = element.GetIntAttribute( "width" );
            var height = element.GetIntAttribute( "height" );

            var tileWidth  = map.Properties.Get< int >( "tilewidth" );
            var tileHeight = map.Properties.Get< int >( "tileheight" );

            var layer = new TiledMapTileLayer( width, height, tileWidth, tileHeight );

            LoadBasicLayerInfo( layer, element );

            var ids = GetTileIDs( element, width, height );

            var tilesets = map.Tilesets;

            for ( var y = 0; y < height; y++ )
            {
                for ( var x = 0; x < width; x++ )
                {
                    var id               = ids[ ( y * width ) + x ];
                    var flipHorizontally = ( id & FLAG_FLIP_HORIZONTALLY ) != 0;
                    var flipVertically   = ( id & FLAG_FLIP_VERTICALLY ) != 0;
                    var flipDiagonally   = ( id & FLAG_FLIP_DIAGONALLY ) != 0;

                    var tile = tilesets.GetTile( ( int )( id & ~MASK_CLEAR ) );

                    if ( tile != null )
                    {
                        var cell = CreateTileLayerCell( flipHorizontally, flipVertically, flipDiagonally );

                        cell.SetTile( tile );
                        layer.SetCell( x, FlipY ? height - 1 - y : y, cell );
                    }
                }
            }

            var properties = element.GetChildByName( "properties" );

            if ( properties != null )
            {
                LoadProperties( layer.Properties, properties );
            }

            parentLayers.Add( layer );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="map"></param>
    /// <param name="parentLayers"> The actual layer group belonging to the map. </param>
    /// <param name="element"></param>
    /// <exception cref="ArgumentException"></exception>
    protected void LoadObjectGroup( TiledMap map, MapLayers parentLayers, XmlReader.Element element )
    {
        if ( element.Name == null )
        {
            throw new ArgumentException( "element cannot by null!" );
        }

        if ( element.Name.Equals( "objectgroup" ) )
        {
            var layer = new MapLayer();

            LoadBasicLayerInfo( layer, element );

            XmlReader.Element? properties;

            if ( ( properties = element.GetChildByName( "properties" ) ) != null )
            {
                LoadProperties( layer.Properties, properties );
            }

            List< XmlReader.Element? >? objectNode;

            if ( ( objectNode = element.GetChildrenByName( "object" ) ) != null )
            {
                foreach ( var objectElement in objectNode )
                {
                    LoadObject( map, layer, objectElement );
                }
            }

            parentLayers.Add( layer );
        }
    }

    /// <summary>
    /// From the official Tiled website:
    /// "Image layers provide a way to quickly include a single image as foreground
    /// or background of your map. They currently have limited functionality and you
    /// may consider adding the image as a Tileset instead and place it as a Tile
    /// Object. This way, you gain the ability to freely scale and rotate the image."
    /// See https://doc.mapeditor.org/en/stable/manual/layers/
    /// </summary>
    /// <param name="map"> The parent <see cref="TiledMap"/>. </param>
    /// <param name="parentLayers"> The actual layer group belonging to the map. </param>
    /// <param name="element"> The xml node being processed. </param>
    /// <param name="tmxFile"> The parent TMX map file. </param>
    /// <param name="imageResolver"> The asset loader interface. </param>
    protected void LoadImageLayer( TiledMap map,
                                   MapLayers parentLayers,
                                   XmlReader.Element element,
                                   FileInfo tmxFile,
                                   IImageResolver imageResolver )
    {
        if ( element.Name == null )
        {
            throw new ArgumentException( "element cannot by null!" );
        }

        if ( element.Name.Equals( "imagelayer" ) )
        {
            var x = float.Parse( element.GetAttribute( "offsetx" )
                              ?? element.GetAttribute( "x" )
                              ?? "0" );

            var y = float.Parse( element.GetAttribute( "offsety" )
                              ?? element.GetAttribute( "y" )
                              ?? "0" );

            if ( FlipY )
            {
                y = MapHeightInPixels - y;
            }

            TextureRegion? texture = null;

            var image = element.GetChildByName( "image" );

            if ( image != null )
            {
                var source = image.GetAttribute( "source" );
                var handle = GetRelativeFileHandle( tmxFile, source );

                texture = imageResolver.GetImage( handle!.FullName );

                if ( texture == null )
                {
                    throw new RuntimeException( "Image Texture cannot be null!" );
                }

                y -= texture.RegionHeight;
            }

            var imageLayer = new TiledMapImageLayer( texture, x, y );

            LoadBasicLayerInfo( imageLayer, element );

            var properties = element.GetChildByName( "properties" );

            if ( properties != null )
            {
                LoadProperties( imageLayer.Properties, properties );
            }

            parentLayers.Add( imageLayer );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="element"></param>
    protected void LoadBasicLayerInfo( MapLayer layer, XmlReader.Element element )
    {
        layer.Name      = element.GetAttribute( "name", null );
        layer.Opacity   = float.Parse( element.GetAttribute( "opacity ", "1.0" ) ?? "1.0" );
        layer.Visible   = ( element.GetIntAttribute( "visible", 1 ) == 1 );
        layer.OffsetX   = element.GetFloatAttribute( "offsetx" );
        layer.OffsetY   = element.GetFloatAttribute( "offsety" );
        layer.ParallaxX = element.GetFloatAttribute( "parallaxx", 1f );
        layer.ParallaxY = element.GetFloatAttribute( "parallaxy", 1f );
    }

    /// <summary>
    /// </summary>
    /// <param name="map"></param>
    /// <param name="layer"></param>
    /// <param name="node"></param>
    protected void LoadObject( TiledMap map, MapLayer layer, XmlReader.Element? node )
    {
        LoadObject( map, layer.Objects, node, MapHeightInPixels );
    }

    /// <summary>
    /// </summary>
    /// <param name="map"></param>
    /// <param name="tile"></param>
    /// <param name="node"></param>
    protected void LoadObject( TiledMap? map, ITiledMapTile tile, XmlReader.Element? node )
    {
        LoadObject( map, tile.MapObjects, node, tile.TextureRegion.RegionHeight );
    }

    /// <summary>
    /// </summary>
    /// <param name="map"></param>
    /// <param name="objects"></param>
    /// <param name="element"></param>
    /// <param name="heightInPixels"></param>
    /// <exception cref="ArgumentException"></exception>
    protected void LoadObject( TiledMap? map, MapObjects? objects, XmlReader.Element? element, float heightInPixels )
    {
        if ( element?.Name == null )
        {
            return;
        }

        if ( element.Name.Equals( "object" ) )
        {
            MapObject? mapObject = null;

            var scaleX = ConvertObjectToTileSpace ? 1.0f / MapTileWidth : 1.0f;
            var scaleY = ConvertObjectToTileSpace ? 1.0f / MapTileHeight : 1.0f;

            var x = element.GetFloatAttribute( "x" ) * scaleX;
            var y = ( FlipY ? heightInPixels - element.GetFloatAttribute( "y" ) : element.GetFloatAttribute( "y" ) )
                  * scaleY;

            var width  = element.GetFloatAttribute( "width" ) * scaleX;
            var height = element.GetFloatAttribute( "height" ) * scaleY;

            if ( element.GetChildCount() > 0 )
            {
                XmlReader.Element? child;

                if ( ( child = element.GetChildByName( "polygon" ) ) != null )
                {
                    var points   = child.GetAttribute( "points" )?.Split( " " );
                    var vertices = new float[ points!.Length * 2 ];

                    for ( var i = 0; i < points.Length; i++ )
                    {
                        var point = points[ i ].Split( "," );
                        vertices[ i * 2 ]         = float.Parse( point[ 0 ] ) * scaleX;
                        vertices[ ( i * 2 ) + 1 ] = float.Parse( point[ 1 ] ) * scaleY * ( FlipY ? -1 : 1 );
                    }

                    var polygon = new Polygon( vertices );
                    polygon.SetPosition( x, y );
                    mapObject = new PolygonMapObject( polygon );
                }
                else if ( ( child = element.GetChildByName( "polyline" ) ) != null )
                {
                    var points   = child.GetAttribute( "points" )?.Split( " " );
                    var vertices = new float[ points!.Length * 2 ];

                    for ( var i = 0; i < points.Length; i++ )
                    {
                        var point = points[ i ].Split( "," );
                        vertices[ i * 2 ]         = float.Parse( point[ 0 ] ) * scaleX;
                        vertices[ ( i * 2 ) + 1 ] = float.Parse( point[ 1 ] ) * scaleY * ( FlipY ? -1 : 1 );
                    }

                    var polyline = new Polyline( vertices );
                    polyline.SetPosition( x, y );
                    mapObject = new PolylineMapObject( polyline );
                }
                else if ( element.GetChildByName( "ellipse" ) != null )
                {
                    mapObject = new EllipseMapObject( x, FlipY ? y - height : y, width, height );
                }
            }

            int id;

            if ( mapObject == null )
            {
                string? gid;

                if ( ( gid = element.GetAttribute( "gid" ) ) != null )
                {
                    id = ( int )long.Parse( gid );

                    var flipHorizontally = ( id & FLAG_FLIP_HORIZONTALLY ) != 0;
                    var flipVertically   = ( id & FLAG_FLIP_VERTICALLY ) != 0;

                    var tile = map?.Tilesets.GetTile( ( int )( id & ~MASK_CLEAR ) );

                    var tiledMapTileMapObject = new TiledMapTileMapObject( tile!, flipHorizontally, flipVertically );

                    var textureRegion = tiledMapTileMapObject.TextureRegion!;

                    tiledMapTileMapObject.Properties.Put( "gid", id );
                    tiledMapTileMapObject.X = x;
                    tiledMapTileMapObject.Y = FlipY ? y : y - height;

                    var objectWidth  = element.GetFloatAttribute( "width", textureRegion.RegionWidth );
                    var objectHeight = element.GetFloatAttribute( "height", textureRegion.RegionHeight );

                    tiledMapTileMapObject.ScaleX   = scaleX * ( objectWidth / textureRegion.RegionWidth );
                    tiledMapTileMapObject.ScaleY   = scaleY * ( objectHeight / textureRegion.RegionHeight );
                    tiledMapTileMapObject.Rotation = element.GetFloatAttribute( "rotation" );

                    mapObject = tiledMapTileMapObject;
                }
                else
                {
                    mapObject = new RectangleMapObject( x, FlipY ? y - height : y, width, height );
                }
            }

            mapObject.Name = element.GetAttribute( "name", "" ) ?? "";
            var rotation = element.GetAttribute( "rotation" );

            if ( rotation != null )
            {
                mapObject.Properties.Put( "rotation", float.Parse( rotation ) );
            }

            var type = element.GetAttribute( "type" );

            if ( type != null )
            {
                mapObject.Properties.Put( "type", type );
            }

            id = element.GetIntAttribute( "id" );

            if ( id != 0 )
            {
                mapObject.Properties.Put( "id", id );
            }

            mapObject.Properties.Put( "x", x );

            if ( mapObject is TiledMapTileMapObject )
            {
                mapObject.Properties.Put( "y", y );
            }
            else
            {
                mapObject.Properties.Put( "y", FlipY ? y - height : y );
            }

            mapObject.Properties.Put( "width", width );
            mapObject.Properties.Put( "height", height );
            mapObject.IsVisible = element.GetIntAttribute( "visible", 1 ) == 1;

            var properties = element.GetChildByName( "properties" );

            if ( properties != null )
            {
                LoadProperties( mapObject.Properties, properties );
            }

            IdToObject?[ id ] = mapObject;

            objects?.Add( mapObject );
        }
    }

    protected void LoadProperties( MapProperties? properties, XmlReader.Element? element )
    {
        if ( element?.Name == null )
        {
            return;
        }

        if ( element.Name.Equals( "properties" ) )
        {
            var props = element.GetChildrenByName( "property" );

            foreach ( var property in props )
            {
                var name  = property?.GetAttribute( "name", null );
                var value = property?.GetAttribute( "value", null );
                var type  = property?.GetAttribute( "type", null );

                value ??= property?.Text;

                if ( type is "object" && properties != null )
                {
                    // Wait until the end of [LoadTiledMap] to fetch the object
                    try
                    {
                        var id          = int.Parse( value! );
                        var propName    = name!;
                        var targetProps = properties;

                        //TODO: Refactor to remove capture of variables and 'this'
                        RunOnEndOfLoadTiled?.Add( () => { ExecutePropertyAssignment( id, propName, targetProps ); } );
                    }
                    catch ( Exception )
                    {
                        throw new RuntimeException( $"Error parsing property {name} "
                                                  + $"of type object with value: [{value}]" );
                    }
                }
                else
                {
                    var castValue = CastProperty( name, value, type );
                    properties?.Put< object? >( name!, castValue );
                }
            }
        }
    }

    private void ExecutePropertyAssignment( int id, string name, MapProperties? mapProps )
    {
        var obj = IdToObject?[ id ];
        mapProps?.Put( name, obj );
    }

    /// <summary>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="RuntimeException"></exception>
    protected object? CastProperty( string? name, string? value, string? type )
    {
        switch ( type )
        {
            case null:
                return value;

            case "int":
                return int.Parse( value! );

            case "float":
                return float.Parse( value! );

            case "bool":
                return bool.Parse( value! );

            case "color":
            case "colour":
            {
                // Tiled uses the format #AARRGGBB
                var opaqueColor = value?.Substring( 3 );
                var alpha       = value?.Substring( 1, 3 );

                return Color.FromHexString( opaqueColor + alpha );
            }

            default:
                throw new RuntimeException( $"Wrong type given for property {name}, "
                                          + $"given : {type}, supported : string, bool, "
                                          + $"int, float, color ( or colour )" );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="flipHorizontally"></param>
    /// <param name="flipVertically"></param>
    /// <param name="flipDiagonally"></param>
    /// <returns></returns>
    protected TiledMapTileLayer.Cell CreateTileLayerCell( bool flipHorizontally,
                                                          bool flipVertically,
                                                          bool flipDiagonally )
    {
        var cell = new TiledMapTileLayer.Cell();

        if ( flipDiagonally )
        {
            switch ( flipHorizontally )
            {
                case true when flipVertically:
                    cell.SetFlipHorizontally( true );
                    cell.SetRotation( TiledMapTileLayer.Cell.ROTATE270 );

                    break;

                case true:
                    cell.SetRotation( TiledMapTileLayer.Cell.ROTATE270 );

                    break;

                default:
                {
                    if ( flipVertically )
                    {
                        cell.SetRotation( TiledMapTileLayer.Cell.ROTATE90 );
                    }
                    else
                    {
                        cell.SetFlipVertically( true );
                        cell.SetRotation( TiledMapTileLayer.Cell.ROTATE270 );
                    }

                    break;
                }
            }
        }
        else
        {
            cell.SetFlipHorizontally( flipHorizontally );
            cell.SetFlipVertically( flipVertically );
        }

        return cell;
    }

    /// <summary>
    /// </summary>
    /// <param name="element"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    /// <exception cref="RuntimeException"></exception>
    public int[] GetTileIDs( XmlReader.Element element, int width, int height )
    {
        var data = element.GetChildByName( "data" );

        if ( data == null )
        {
            throw new RuntimeException( "data is missing" );
        }

        var encoding = data.GetAttribute( "encoding", null );

        if ( encoding == null )
        {
            // no 'encoding' attribute means that the encoding is XML
            throw new RuntimeException( "Unsupported encoding (XML) for TMX Layer Data" );
        }

        var ids = new int[ width * height ];

        if ( encoding.Equals( "csv" ) )
        {
            var array = data.Text?.Split( "," );

            for ( var i = 0; i < array?.Length; i++ )
            {
                ids[ i ] = ( int )long.Parse( array[ i ].Trim() );
            }
        }
        else
        {
            if ( encoding.Equals( "base64" ) )
            {
                Stream? inputStream = null;

                try
                {
                    var compression = data.GetAttribute( "compression", null );
                    var bytes       = Convert.FromBase64String( data.Text ?? data.ToString() );

                    switch ( compression )
                    {
                        case null:
                            inputStream = new MemoryStream( bytes );

                            break;

                        case "gzip":
                            inputStream = new BufferedStream( new GZipStream( new MemoryStream( bytes ),
                                                                              CompressionMode.Decompress ) );

                            break;

                        case "zlib":
                            inputStream = new BufferedStream( new InflaterInputStream( new MemoryStream( bytes ) ) );

                            break;

                        default:
                            throw
                                new RuntimeException( $"Unrecognised compression ({compression}) for TMX Layer Data" );
                    }

                    var temp = new byte[ 4 ];

                    for ( var y = 0; y < height; y++ )
                    {
                        for ( var x = 0; x < width; x++ )
                        {
                            var read = inputStream.Read( temp );

                            while ( read < temp.Length )
                            {
                                var curr = inputStream.Read( temp, read, temp.Length - read );

                                if ( curr == -1 )
                                {
                                    break;
                                }

                                read += curr;
                            }

                            if ( read != temp.Length )
                            {
                                throw
                                    new RuntimeException( "Error Reading TMX Layer Data: Premature end of tile data" );
                            }

                            ids[ ( y * width ) + x ] = MathUtils.UnsignedByteToInt( temp[ 0 ] )
                                                     | ( MathUtils.UnsignedByteToInt( temp[ 1 ] ) << 8 )
                                                     | ( MathUtils.UnsignedByteToInt( temp[ 2 ] ) << 16 )
                                                     | ( MathUtils.UnsignedByteToInt( temp[ 3 ] ) << 24 );
                        }
                    }
                }
                catch ( IOException e )
                {
                    throw new RuntimeException( $"Error Reading TMX Layer Data - IOException: {e.Message}" );
                }
                finally
                {
                    inputStream?.Close();
                }
            }
            else
            {
                // any other value of 'encoding' is one we're not aware of, probably
                // a feature of a future version of Tiled or another editor
                throw new RuntimeException( $"Unrecognised encoding ({encoding}) for TMX Layer Data" );
            }
        }

        return ids;
    }

    /// <summary>
    /// </summary>
    /// <param name="file"></param>
    /// <param name="path"></param>
    /// <param name="rootLimit"></param>
    /// <returns></returns>
    protected static FileInfo? GetRelativeFileHandle( FileInfo? file, string? path, string? rootLimit = null )
    {
        if ( file == null || string.IsNullOrEmpty( path ) )
        {
            return null;
        }

        // 1. Get the starting point (the directory of the reference file)
        var baseDirectory = file.DirectoryName;

        if ( baseDirectory == null )
        {
            return null;
        }

        // 2. Resolve the full destination path
        var combinedPath = Path.Combine( baseDirectory, path );
        var resolvedPath = Path.GetFullPath( combinedPath );

        // 3. Security Check: If a root limit is provided, ensure resolvedPath is inside it
        if ( !string.IsNullOrEmpty( rootLimit ) )
        {
            var fullRoot = Path.GetFullPath( rootLimit );

            // Ensure the path starts with the root folder string
            // StringComparison.OrdinalIgnoreCase is used because Windows paths are case-insensitive
            if ( !resolvedPath.StartsWith( fullRoot, StringComparison.OrdinalIgnoreCase ) )
            {
                throw new UnauthorizedAccessException( "Attempted to access a path "
                                                     + "outside of the allowed directory." );
            }
        }

        return new FileInfo( resolvedPath );
    }

    /// <summary>
    /// Loads a Tileset as described in <paramref name="element"/>.
    /// The Node is laid ouit as follows:-
    /// <code>
    /// &lt;tileset firstgid="x" source="filename.tsx"/&gt;
    /// </code>
    /// where 'x' is the id of the first tile in the tileset.
    /// The width and height dimensions of the image used for the tiles are held in the TSX file, as are
    /// the tile width/height, number of columns in the tile image, and total tile count.
    /// </summary>
    /// <param name="element"> The node referencing the TSX tileset file. </param>
    /// <param name="tmxFile"> The current TMX file being processed. </param>
    /// <param name="imageResolver"></param>
    /// <exception cref="RuntimeException"></exception>
    protected void LoadTileSet( XmlReader.Element? element, FileInfo tmxFile, IImageResolver imageResolver )
    {
        if ( element is { Name: "tileset" } )
        {
            var firstgid    = element.GetIntAttribute( "firstgid", 1 );
            var imageSource = "";
            var imageWidth  = 0;
            var imageHeight = 0;

            FileInfo? imageFileHandle = null;

            // Fetch the .tsx file which contains the image name etc.
            var source = element.GetAttribute( "source", null );

            if ( source != null )
            {
                var tsx = GetRelativeFileHandle( tmxFile, source );

                if ( tsx == null )
                {
                    throw new RuntimeException( $"Unable to find tileset source file: {source}" );
                }

                try
                {
                    element = XmlReader.Parse( tsx );

                    var imageElement = element?.GetChildByName( "image" );

                    if ( imageElement != null )
                    {
                        // Image filename
                        imageSource = imageElement.GetAttribute( "source" );

                        // Image width, in pixels
                        imageWidth = imageElement.GetIntAttribute( "width" );

                        // Image height, in pixels
                        imageHeight = imageElement.GetIntAttribute( "height" );

                        if ( imageSource is null )
                        {
                            throw new RuntimeException( $"Error: Image source for tileset {tmxFile.Name} is null!" );
                        }

                        imageFileHandle = GetRelativeFileHandle( tsx, imageSource );
                    }
                }
                catch ( SerializationException )
                {
                    throw new RuntimeException( "Error parsing external tileset." );
                }
            }
            else
            {
                var imageElement = element.GetChildByName( "image" );

                if ( imageElement != null )
                {
                    // Image filename
                    imageSource = imageElement.GetAttribute( "source" );

                    // Image width, in pixels
                    imageWidth = imageElement.GetIntAttribute( "width" );

                    // Image height, in pixels
                    imageHeight = imageElement.GetIntAttribute( "height" );

                    imageFileHandle = GetRelativeFileHandle( tmxFile, imageSource );
                }
            }

            var name       = element?.Get( "name", null );
            var tilewidth  = element?.GetIntAttribute( "tilewidth" ) ?? 0;
            var tileheight = element?.GetIntAttribute( "tileheight" ) ?? 0;
            var spacing    = element?.GetIntAttribute( "spacing" ) ?? 0;
            var margin     = element?.GetIntAttribute( "margin" ) ?? 0;
            var offset     = element?.GetChildByName( "tileoffset" );
            var offsetX    = 0;
            var offsetY    = 0;

            if ( offset != null )
            {
                offsetX = offset.GetIntAttribute( "x" );
                offsetY = offset.GetIntAttribute( "y" );
            }

            var tileSet = new TiledMapTileSet
            {
                Name = name ?? "",
            };

            var tileSetProperties = tileSet.Properties;
            var properties        = element?.GetChildByName( "properties" );

            if ( properties != null )
            {
                LoadProperties( tileSetProperties, properties );
            }

            tileSetProperties.Put( "firstgid", firstgid );

            // Tiles
            var tileElements = element?.GetChildrenByName( "tile" );

            if ( tileElements == null )
            {
                throw new RuntimeException( "Error: No tile elements found!" );
            }

            var tileContext = new TileContext
            {
                ImageResolver = imageResolver,
                Tileset       = tileSet,
                TmxFile       = tmxFile,
            };

            var imageDetails = new ImageDetails
            {
                ImageSource = imageSource,
                Width       = imageWidth,
                Height      = imageHeight,
                Image       = imageFileHandle,
            };

            var tileMetrics = new TileMetrics
            {
                Name       = name,
                Firstgid   = firstgid,
                Tilewidth  = tilewidth,
                Tileheight = tileheight,
                Spacing    = spacing,
                Margin     = margin,
            };

            AddStaticTiles( element,
                            tileContext,
                            tileElements,
                            tileMetrics,
                            source,
                            offsetX,
                            offsetY,
                            imageDetails );

            var animatedTiles = new List< AnimatedTiledMapTile >();

            foreach ( var tileElement in tileElements )
            {
                var localtid = tileElement?.GetIntAttribute( "id" ) ?? 0;
                var tile     = tileSet.GetTile( firstgid + localtid );

                var animatedTile = CreateAnimatedTile( tileSet, tile, tileElement, firstgid );

                if ( animatedTile != null )
                {
                    animatedTiles.Add( animatedTile );
                    tile = animatedTile;
                }

                AddTileProperties( tile, tileElement );
                AddTileObjectGroup( tile, tileElement );
            }

            // replace original static tiles by animated tiles
            foreach ( var animatedTile in animatedTiles )
            {
                tileSet.PutTile( animatedTile.ID, animatedTile );
            }

            Map?.Tilesets.AddTileSet( tileSet );
        }
    }

    protected abstract void AddStaticTiles( XmlReader.Element? element,
                                            TileContext tileContext,
                                            List< XmlReader.Element? > tileElements,
                                            TileMetrics tileMetrics,
                                            string? source,
                                            int offsetX,
                                            int offsetY,
                                            ImageDetails imageDetails );

    /// <summary>
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="element"></param>
    protected void AddTileProperties( ITiledMapTile tile, XmlReader.Element? element )
    {
        Guard.Against.Null( element );

        string? terrain;

        if ( ( terrain = element.GetAttribute( "terrain" ) ) != null )
        {
            tile.Properties?.Put( "terrain", terrain );
        }

        string? probability;

        if ( ( probability = element.GetAttribute( "probability" ) ) != null )
        {
            tile.Properties?.Put( "probability", probability );
        }

        string? type;

        if ( ( type = element.GetAttribute( "type" ) ) != null )
        {
            tile.Properties?.Put( "type", type );
        }

        XmlReader.Element? properties;

        if ( ( properties = element.GetChildByName( "properties" ) ) != null )
        {
            LoadProperties( tile.Properties, properties );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="element"></param>
    protected void AddTileObjectGroup( ITiledMapTile tile, XmlReader.Element? element )
    {
        var objectgroupElement = element?.GetChildByName( "objectgroup" );

        if ( objectgroupElement != null )
        {
            var children = objectgroupElement.GetChildrenByName( "object" );

            foreach ( var objectElement in children )
            {
                LoadObject( Map, tile, objectElement );
            }
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="tileSet"></param>
    /// <param name="tile"></param>
    /// <param name="element"></param>
    /// <param name="firstgid"></param>
    /// <returns></returns>
    protected AnimatedTiledMapTile? CreateAnimatedTile( TiledMapTileSet tileSet,
                                                        ITiledMapTile tile,
                                                        XmlReader.Element? element,
                                                        int firstgid )
    {
        var animationElement = element?.GetChildByName( "animation" );

        if ( animationElement == null )
        {
            return null;
        }

        var staticTiles = new List< StaticTiledMapTile? >();
        var intervals   = new List< int >();

        List< XmlReader.Element? >? frames;

        if ( ( frames = animationElement.GetChildrenByName( "frame" ) ) == null )
        {
            return null;
        }

        foreach ( var frameElement in frames )
        {
            staticTiles.Add( ( StaticTiledMapTile? )tileSet
                                 .GetTile( firstgid + ( frameElement?.GetIntAttribute( "tileid" ) ?? 0 ) ) );

            intervals.Add( frameElement?.GetIntAttribute( "duration" ) ?? 0 );
        }

        var animatedTile = new AnimatedTiledMapTile( intervals, staticTiles! )
        {
            ID = tile.ID,
        };

        return animatedTile;
    }

    /// <summary>
    /// Add a standard, non-animating, static tile to the map.
    /// </summary>
    /// <param name="tileSet"></param>
    /// <param name="textureRegion"> The tile image </param>
    /// <param name="tileId"></param>
    /// <param name="offsetX"></param>
    /// <param name="offsetY"></param>
    protected void AddStaticTiledMapTile( TiledMapTileSet tileSet,
                                          TextureRegion? textureRegion,
                                          int tileId,
                                          float offsetX,
                                          float offsetY )
    {
        Guard.Against.Null( tileSet );
        Guard.Against.Null( textureRegion );

        ITiledMapTile tile = new StaticTiledMapTile( textureRegion );

        tile.ID      = tileId;
        tile.OffsetX = offsetX;
        tile.OffsetY = FlipY ? -offsetY : offsetY;

        tileSet.PutTile( tileId, tile );
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class BaseTmxLoaderParameters : AssetLoaderParameters
    {
        // generate mipmaps?
        public bool GenerateMipMaps { get; set; }

        // The TextureFilter to use for minification
        public TextureFilterMode TextureMinFilter { get; set; } = TextureFilterMode.Nearest;

        // The TextureFilter to use for magnification
        public TextureFilterMode TextureMagFilter { get; set; } = TextureFilterMode.Nearest;

        /// <summary>
        /// Whether to convert the objects' pixel position and size to the equivalent in tile space.
        /// </summary>
        public bool ConvertObjectToTileSpace { get; set; }

        /// <summary>
        /// Whether to flip all Y coordinates so that Y positive is up. All LibGDX renderers
        /// require flipped Y coordinates, and thus flipY set to true. This parameter is included
        /// for non-rendering related purposes of TMX files, or custom renderers.
        /// </summary>
        public bool FlipY { get; set; } = true;
    }
}

// ========================================================================
// ========================================================================

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

        MapVersion         = element.GetAttribute( "version" );
        TiledVersion       = element.GetAttribute( "tiledversion" );
        MapOrientation     = element.GetAttribute( "orientation" );
        RenderOrder        = element.GetAttribute( "renderorder" );
        NextLayerID        = element.GetAttribute( "nextlayerid" );
        NextObjectID       = element.GetAttribute( "nextobjectid" );
        HexSideLength      = element.GetAttribute( "hexsidelength" );
        StaggerAxis        = element.GetAttribute( "staggeraxis" );
        StaggerIndex       = element.GetAttribute( "staggerindex" );
        MapBackgroundColor = element.GetAttribute( "backgroundcolor" );
        IsInfinite         = element.GetAttribute( "infinite" ) == "1";
        MapWidth           = element.GetIntAttribute( "width" );
        MapHeight          = element.GetIntAttribute( "height" );
        TileWidth          = element.GetIntAttribute( "tilewidth" );
        TileHeight         = element.GetIntAttribute( "tileheight" );
    }
}

// ============================================================================
// ============================================================================