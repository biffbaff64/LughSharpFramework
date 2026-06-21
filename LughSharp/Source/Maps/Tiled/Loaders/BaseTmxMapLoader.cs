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

using System.IO.Compression;
using System.Xml;

using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

using LughSharp.Source.Assets.Loaders;
using LughSharp.Source.Assets.Loaders.Resolvers;
using LughSharp.Source.Graphics.Images;
using LughSharp.Source.Graphics.OpenGL.Enums;
using LughSharp.Source.Maps.Objects;
using LughSharp.Source.Maps.Tiled.Objects;
using LughSharp.Source.Maps.Tiled.Tiles;

using XmlReader = LughSharp.Source.Utils.XML.XmlReader;

namespace LughSharp.Source.Maps.Tiled.Loaders;

/// <summary>
/// Base class for loading Tiled TMX maps into a usable internal representation.
/// This abstract class provides a framework for handling various components of a TMX map,
/// such as tilesets, layers, and objects, in a structured and asynchronous manner.
/// </summary>
/// <typeparam name="TP">
/// The parameter type used for map-loading configurations. The type must inherit from
/// <see cref="BaseTmxMapLoader{TP}.BaseTmxLoaderParameters" />.
/// </typeparam>
[PublicAPI]
public abstract class BaseTmxMapLoader< TP > : AsynchronousAssetLoader
    where TP : BaseTmxMapLoader< TP >.BaseTmxLoaderParameters
{
    protected const uint FlipHorizontally    = 0x80000000;
    protected const uint FlipVertically      = 0x40000000;
    protected const uint FlipDiagonally      = 0x20000000;
    protected const uint RotatedHexagonal120 = 0x10000000;
    protected const uint MaskClear           = 0xF0000000;

    // ========================================================================

    /// <summary>
    /// The width, in pixels, of a single tile in the map, as specified in the TMX file.
    /// </summary>
    public int MapTileWidth { get; set; }

    /// <summary>
    /// The height, in pixels, of a single tile in the map, as specified in the TMX file.
    /// </summary>
    public int MapTileHeight { get; set; }

    /// <summary>
    /// The width, in pixels, of the entire map, as specified in the TMX file.
    /// </summary>
    public int MapWidthInPixels { get; set; }

    /// <summary>
    /// The height, in pixels, of the entire map, as specified in the TMX file.
    /// </summary>
    public int MapHeightInPixels { get; set; }

    /// <summary>
    /// The <see cref="TiledMap"/> object representing the loaded map data.
    /// </summary>
    public TiledMap Map { get; set; } = null!;

    /// <summary>
    /// A dictionary that maps unique identifiers to corresponding <see cref="MapObject"/>
    /// instances. This property is used to associate map objects with their identifiers,
    /// facilitating retrieval and manipulation of map objects during map loading or processing.
    /// </summary>
    public Dictionary< uint, MapObject >? IdToObject { get; set; }

    /// <summary>
    /// A collection of actions that are executed after the tiled map has finished loading.
    /// This allows for deferred or post-processing operations, such as modifying map properties
    /// or performing other cleanup tasks after the map data and associated resources have
    /// been fully loaded.
    /// </summary>
    public List< ( uint id, string propName, MapProperties? targetProps ) >? RunOnEndOfLoadTiled { get; set; }

    // ========================================================================

    protected bool ConvertObjectToTileSpace;
    protected bool FlipY = true;

    protected XmlDocument        XmlDocument = new();
    protected XmlReader          XmlReader   = new();
    protected XmlReader.Element? XmlRoot;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates a new instance of the <see cref="BaseTmxMapLoader{TP}"/> class, using
    /// the specified <see cref="IFileHandleResolver"/>.
    /// </summary>
    /// <param name="resolver"></param>
    protected BaseTmxMapLoader( IFileHandleResolver resolver )
        : base( resolver )
    {
    }

    /// <summary>
    /// Loads the map data, given the XML root element.
    /// </summary>
    /// <param name="tmxFile"> The Filehandle of the tmx file </param>
    /// <param name="loadParams"> The parameters for loading the map. </param>
    /// <param name="imageResolver"> The image resolver to use for loading images. </param>
    /// <returns>The <see cref="TiledMap"/> The final loaded TiledMap. </returns>
    protected TiledMap LoadTiledMap( FileInfo? tmxFile, TP? loadParams, IImageResolver imageResolver )
    {
        if ( XmlRoot == null )
        {
            throw new RuntimeException( "XmlRoot must be set before loading the map!" );
        }

        Map                 = new TiledMap();
        IdToObject          = new Dictionary< uint, MapObject >();
        RunOnEndOfLoadTiled = new List< (uint, string, MapProperties?) >();

        if ( loadParams != null )
        {
            ConvertObjectToTileSpace = loadParams.ConvertObjectToTileSpace;
            FlipY                    = loadParams.FlipY;
        }
        else
        {
            ConvertObjectToTileSpace = false;
            FlipY                    = true;
        }

        FetchMapAttributes();

        // Set map and tile dimensions
        MapTileWidth      = XmlRoot.GetIntAttribute( "tilewidth" );
        MapTileHeight     = XmlRoot.GetIntAttribute( "tileheight" );
        MapWidthInPixels  = XmlRoot.GetIntAttribute( "width" ) * MapTileWidth;
        MapHeightInPixels = XmlRoot.GetIntAttribute( "height" ) * MapTileHeight;

        // --------------------------------------

        string? mapOrientation = XmlRoot.GetAttribute( "orientation", null );

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

        // --------------------------------------

        XmlReader.Element? properties = XmlRoot.GetChildByName( "properties" );

        LoadProperties( Map.Properties, properties );

        // --------------------------------------

        // Load tilesets used in this map
        List< XmlReader.Element? > tilesetList = XmlRoot.GetChildrenByName( "tileset" );

        if ( tmxFile != null )
        {
            foreach ( XmlReader.Element? tset in tilesetList )
            {
                LoadTileSet( tset, tmxFile, imageResolver );

                XmlRoot?.RemoveChild( tset );
            }
        }

        // --------------------------------------

        // Load all this maps component layers
        int childCount = XmlRoot?.GetChildCount() ?? 0;

        if ( childCount == 0 )
        {
            throw new RuntimeException( "No layers found in map!" );
        }

        for ( int i = 0, j = childCount; i < j; i++ )
        {
            LoadLayer( Map, Map.Layers, XmlRoot?.GetChild( i ), tmxFile, imageResolver );
        }

        // --------------------------------------

        // Opdate hierarchical parallax scrolling factors.
        // In Tiled the final parallax scrolling factor of a layer is the
        // multiplication of its factor with all its parents.
        List< MapGroupLayer > groups = Map.Layers.GetByType< MapGroupLayer >();

        while ( groups.Count > 0 )
        {
            MapGroupLayer group = groups.First();
            groups.RemoveAt( 0 );

            foreach ( MapLayer child in group.Layers )
            {
                child.ParallaxX *= group.ParallaxX;
                child.ParallaxY *= group.ParallaxY;

                if ( child is MapGroupLayer groupChild )
                {
                    groups.Add( groupChild );
                }
            }
        }

        // --------------------------------------
        // Process any post-load actions.

        foreach ( var (id, propName, targetProps) in RunOnEndOfLoadTiled )
        {
            ExecutePropertyAssignment( id, propName, targetProps );
        }

        RunOnEndOfLoadTiled.Clear();
        RunOnEndOfLoadTiled = null;

        return Map;
    }

    /// <summary>
    /// Parses and populates the map metadata from the XML root element. Extracts attributes
    /// such as version, dimensions, tile configurations, render settings, and additional
    /// properties, storing them into the map's property collection.
    /// </summary>
    /// <exception cref="RuntimeException">
    /// Thrown if the map object is null during the loading process.
    /// </exception>
    private void FetchMapAttributes()
    {
        if ( Map == null )
        {
            throw new RuntimeException( "Map cannot be null!" );
        }

        if ( XmlRoot == null )
        {
            throw new RuntimeException( "XmlRoot must be set before loading the map!" );
        }

        Map.Properties.Put( "version", XmlRoot.GetAttribute( "version" ) );
        Map.Properties.Put( "tiledversion", XmlRoot.GetAttribute( "tiledversion" ) );
        Map.Properties.Put( "orientation", XmlRoot.GetAttribute( "orientation", null ) );
        Map.Properties.Put( "width", XmlRoot.GetIntAttribute( "width" ) );
        Map.Properties.Put( "height", XmlRoot.GetIntAttribute( "height" ) );
        Map.Properties.Put( "tilewidth", XmlRoot.GetIntAttribute( "tilewidth" ) );
        Map.Properties.Put( "tileheight", XmlRoot.GetIntAttribute( "tileheight" ) );
        Map.Properties.Put( "hexsidelength", XmlRoot.GetIntAttribute( "hexsidelength" ) );

        string? staggerAxis = XmlRoot.GetAttribute( "staggeraxis", null );

        if ( !string.IsNullOrEmpty( staggerAxis ) )
        {
            Map.Properties.Put( "staggeraxis", staggerAxis );
        }

        string? staggerIndex = XmlRoot.GetAttribute( "staggerindex", null );

        if ( !string.IsNullOrEmpty( staggerIndex ) )
        {
            Map.Properties.Put( "staggerindex", staggerIndex );
        }

        string? backgroundColor = XmlRoot.GetAttribute( "backgroundcolor", null );

        if ( !string.IsNullOrEmpty( backgroundColor ) )
        {
            Map.Properties.Put( "backgroundcolor", backgroundColor );
        }

        Map.Properties.Put( "infinite", XmlRoot.GetAttribute( "infinite" ) );
        Map.Properties.Put( "nextLayerID", XmlRoot.GetAttribute( "nextlayerid" ) );
        Map.Properties.Put( "nextObjectID", XmlRoot.GetAttribute( "nextobjectid" ) );
        Map.Properties.Put( "renderorder", XmlRoot.GetAttribute( "renderorder" ) );
    }

    /// <summary>
    /// Returns a List{} holding dependencies for the given map.
    /// </summary>
    /// <param name="filename"> The filename of the map. </param>
    /// <param name="tmxFile"> The file of the map. </param>
    /// <param name="loadParams"> The LoaderParameters for the map. </param>
    /// <returns></returns>
    public List< AssetDescriptor >? GetDependencies( string filename, FileInfo tmxFile, TP? loadParams )
    {
        XmlRoot = XmlReader.Parse( tmxFile.FullName );

        var textureParameter = new TextureLoader.TextureLoaderParameters();

        if ( loadParams != null )
        {
            textureParameter.GenMipMaps = loadParams.GenerateMipMaps;
            textureParameter.MinFilter  = loadParams.TextureMinFilter;
            textureParameter.MagFilter  = loadParams.TextureMagFilter;
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

    /// <summary>
    /// Loads a tiledmap layer from the provided XML element and adds it to the parent
    /// layers of the specified map
    /// </summary>
    /// <param name="map">The map instance to which the layer group will be added.</param>
    /// <param name="parentLayers">
    /// The group of layers to which the newly loaded layers will be added.
    /// </param>
    /// <param name="element">The XML element representing the layer group to be processed.</param>
    /// <param name="tmxFile">The TMX file associated with the map, if available.</param>
    /// <param name="imageResolver">
    /// The resolver used to handle image references within the layer group.
    /// </param>
    /// <exception cref="RuntimeException">
    /// Thrown if the specified XML element is not recognized as a valid layer type.
    /// </exception>
    protected void LoadLayer( TiledMap map, MapLayers parentLayers,
                              XmlReader.Element? element,
                              FileInfo? tmxFile,
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

            case "editorsettings":
                //@formatter:off
                #if DEBUG
                Logger.Divider();
                Logger.Divider();
                Logger.Error( "This TiledMap contains an <editorsettings/> element. "
                            + "This is not currently supported. This support is "
                            + "planned for a future release. The element will be ignored." );

                Logger.Divider();
                Logger.Divider();
                break;
                #else
                        throw new NotSupportedException( "TiledMap editorsettings element is not "
                                                       + "currently supported. This support is "
                                                       + "planned for a future release." );
                #endif
            //@formatter:on

            default:
                throw new RuntimeException( $"Unknown layer type: {element?.Name}" );
        }
    }

    /// <summary>
    /// Processes and loads a layer group from the provided XML element and adds it to the
    /// parent layers of the specified map.
    /// </summary>
    /// <param name="map">The map instance to which the layer group will be added.</param>
    /// <param name="parentLayers">
    /// The group of layers to which the newly loaded layers will be added.
    /// </param>
    /// <param name="element">The XML element representing the layer group to be processed.</param>
    /// <param name="tmxFile">The TMX file associated with the map, if available.</param>
    /// <param name="imageResolver">
    /// The resolver used to handle image references within the layer group.
    /// </param>
    protected void LoadLayerGroup( TiledMap map,
                                   MapLayers parentLayers,
                                   XmlReader.Element element,
                                   FileInfo? tmxFile,
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
                XmlReader.Element? child = element.GetChild( i );

                LoadLayer( map, groupLayer.Layers, child, tmxFile, imageResolver );
            }

            foreach ( MapLayer layer in groupLayer.Layers )
            {
                layer.Parent = groupLayer;
            }

            parentLayers.Add( groupLayer );
        }
    }

    /// <summary>
    /// Loads a tile layer into the provided <see cref="TiledMap"/> from the specified XML element.
    /// </summary>
    /// <param name="map">The <see cref="TiledMap"/> into which the tile layer will be loaded.</param>
    /// <param name="parentLayers">The layer group within the map that the tile layer belongs to.</param>
    /// <param name="element">The XML element representing the tile layer to be processed.</param>
    /// <exception cref="ArgumentException">Thrown when the element's name is null.</exception>
    protected void LoadTileLayer( TiledMap map, MapLayers parentLayers, XmlReader.Element element )
    {
        if ( element.Name == null )
        {
            throw new ArgumentException( "node.Name cannot by null!" );
        }

        if ( element.Name.Equals( "layer" ) )
        {
            int width  = element.GetIntAttribute( "width" );
            int height = element.GetIntAttribute( "height" );

            var tileWidth  = map.Properties.Get< int >( "tilewidth" );
            var tileHeight = map.Properties.Get< int >( "tileheight" );

            var layer = new TiledMapTileLayer( width, height, tileWidth, tileHeight );

            LoadBasicLayerInfo( layer, element );

            uint[] ids = GetTileIDs( element, width, height );

            TiledMapTileSets tilesets = map.Tilesets;

            for ( var y = 0; y < height; y++ )
            {
                for ( var x = 0; x < width; x++ )
                {
                    uint id               = ids[ ( y * width ) + x ];
                    bool flipHorizontally = ( id & FlipHorizontally ) != 0;
                    bool flipVertically   = ( id & FlipVertically ) != 0;
                    bool flipDiagonally   = ( id & FlipDiagonally ) != 0;

                    ITiledMapTile? tile = tilesets.GetTile( id & ~MaskClear );

                    if ( tile != null )
                    {
                        TiledMapTileLayer.Cell cell =
                            CreateTileLayerCell( flipHorizontally, flipVertically, flipDiagonally );

                        cell.SetTile( tile );
                        layer.SetCell( x, FlipY ? height - 1 - y : y, cell );
                    }
                }
            }

            XmlReader.Element? properties = element.GetChildByName( "properties" );

            if ( properties != null )
            {
                LoadProperties( layer.Properties, properties );
            }

            parentLayers.Add( layer );
        }
    }

    /// <summary>
    /// Processes and loads an object group layer from the provided XML element
    /// into the given parent map layers within the specified tiled map.
    /// </summary>
    /// <param name="map">The tiled map instance to which the object group layer is being added.</param>
    /// <param name="parentLayers">
    /// The collection of existing map layers that will include the newly loaded object group layer.
    /// </param>
    /// <param name="element">
    /// The XML element representing the object group layer to be processed and added.
    /// </param>
    /// <exception cref="ArgumentException">Thrown when the provided XML element has a null name.</exception>
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
                foreach ( XmlReader.Element? objectElement in objectNode )
                {
                    LoadObject( map, layer, objectElement );
                }
            }

            parentLayers.Add( layer );
        }
    }

    /// <summary>
    /// From the official Tiled website:
    /// "Image layers provide a way to quickly include a single image as foreground or background
    /// of your map. They currently have limited functionality and you may consider adding the
    /// image as a Tileset instead and place it as a Tile Object. This way, you gain the ability
    /// to freely scale and rotate the image."
    /// <para>
    /// See https://doc.mapeditor.org/en/stable/manual/layers/
    /// </para>
    /// </summary>
    /// <param name="map"> The parent <see cref="TiledMap"/>. </param>
    /// <param name="parentLayers"> The actual layer group belonging to the map. </param>
    /// <param name="element"> The xml node being processed. </param>
    /// <param name="tmxFile"> The parent TMX map file. </param>
    /// <param name="imageResolver"> The asset loader interface. </param>
    protected void LoadImageLayer( TiledMap map,
                                   MapLayers parentLayers,
                                   XmlReader.Element element,
                                   FileInfo? tmxFile,
                                   IImageResolver imageResolver )
    {
        if ( element.Name == null )
        {
            throw new ArgumentException( "element cannot by null!" );
        }

        if ( element.Name.Equals( "imagelayer" ) )
        {
            float x = float.Parse( element.GetAttribute( "offsetx" )
                                ?? element.GetAttribute( "x" )
                                ?? "0" );

            float y = float.Parse( element.GetAttribute( "offsety" )
                                ?? element.GetAttribute( "y" )
                                ?? "0" );

            if ( FlipY )
            {
                y = MapHeightInPixels - y;
            }

            TextureRegion? texture = null;

            XmlReader.Element? image = element.GetChildByName( "image" );

            if ( image != null )
            {
                string?   source = image.GetAttribute( "source" );
                FileInfo? handle = GetRelativeFileHandle( tmxFile, source );

                texture = imageResolver.GetImage( handle!.FullName );

                if ( texture == null )
                {
                    throw new RuntimeException( "Image Texture cannot be null!" );
                }

                y -= texture.GetRegionHeight();
            }

            var imageLayer = new TiledMapImageLayer( texture, x, y );

            LoadBasicLayerInfo( imageLayer, element );

            XmlReader.Element? properties = element.GetChildByName( "properties" );

            if ( properties != null )
            {
                LoadProperties( imageLayer.Properties, properties );
            }

            parentLayers.Add( imageLayer );
        }
    }

    /// <summary>
    /// Initializes the basic properties of a map layer using the data from the specified XML element.
    /// </summary>
    /// <param name="layer">The target layer to populate with information.</param>
    /// <param name="element">The XML element containing the layer data.</param>
    protected void LoadBasicLayerInfo( MapLayer layer, XmlReader.Element element )
    {
        layer.Name      = element.GetAttribute( "name", null );
        layer.Opacity   = float.Parse( element.GetAttribute( "opacity ", "1.0" ) ?? "1.0" );
        layer.Visible   = element.GetIntAttribute( "visible", 1 ) == 1;
        layer.OffsetX   = element.GetFloatAttribute( "offsetx" );
        layer.OffsetY   = element.GetFloatAttribute( "offsety" );
        layer.ParallaxX = element.GetFloatAttribute( "parallaxx", 1f );
        layer.ParallaxY = element.GetFloatAttribute( "parallaxy", 1f );
    }

    /// <summary>
    /// Processes an individual object element from the XML node and associates it with
    /// the specified tiled map and layer.
    /// </summary>
    /// <param name="map">The tiled map to which the object belongs.</param>
    /// <param name="layer">The map layer containing the objects.</param>
    /// <param name="node">The XML element representing the object data.</param>
    protected void LoadObject( TiledMap map, MapLayer layer, XmlReader.Element? node )
    {
        LoadObject( map, layer.Objects, node, MapTileHeight );
    }

    /// <summary>
    /// Processes an individual object element from the XML node and associates it with
    /// the specified tiled map and tile.
    /// </summary>
    /// <param name="map"> The tiled map to which the object belongs. </param>
    /// <param name="tile"> The tile to which the object belongs. </param>
    /// <param name="node">The XML element representing the object data.</param>
    protected void LoadObject( TiledMap? map, ITiledMapTile tile, XmlReader.Element? node )
    {
        LoadObject( map, tile.MapObjects, node, tile.TextureRegion.GetRegionHeight() );
    }

    /// <summary>
    /// Processes an individual object element from the XML node and associates it with
    /// the specified tiled map and tile. 
    /// </summary>
    /// <param name="map"> The tiled map to which the object belongs. </param>
    /// <param name="objects"> The map objects collection to which the object belongs. </param>
    /// <param name="node">The XML element representing the object data.</param>
    /// <param name="heightInPixels"> THe height in pixels of the tile to which the object belongs. </param>
    /// <exception cref="ArgumentException">
    /// Thrown if the provided XML element, node, or map are null. 
    /// </exception>
    protected void LoadObject( TiledMap? map, MapObjects? objects, XmlReader.Element? node, float heightInPixels )
    {
        Guard.Against.Null( map );
        Guard.Against.Null( objects );
        Guard.Against.Null( node );

        if ( node.Name.Equals( "object" ) )
        {
            MapObject? mapObject = null;

            float scaleX = ConvertObjectToTileSpace ? 1.0f / MapTileWidth : 1.0f;
            float scaleY = ConvertObjectToTileSpace ? 1.0f / MapTileHeight : 1.0f;

            float x = node.GetFloatAttribute( "x" ) * scaleX;
            float y = ( FlipY ? heightInPixels - node.GetFloatAttribute( "y" ) : node.GetFloatAttribute( "y" ) )
                    * scaleY;

            float width  = node.GetFloatAttribute( "width" ) * scaleX;
            float height = node.GetFloatAttribute( "height" ) * scaleY;

            if ( node.GetChildCount() > 0 )
            {
                XmlReader.Element? child;

                if ( ( child = node.GetChildByName( "polygon" ) ) != null )
                {
                    string[]? points   = child.GetAttribute( "points" )?.Split( " " );
                    var       vertices = new float[ points!.Length * 2 ];

                    for ( var i = 0; i < points.Length; i++ )
                    {
                        string[] point = points[ i ].Split( "," );
                        vertices[ i * 2 ]         = float.Parse( point[ 0 ] ) * scaleX;
                        vertices[ ( i * 2 ) + 1 ] = float.Parse( point[ 1 ] ) * scaleY * ( FlipY ? -1 : 1 );
                    }

                    var polygon = new Polygon( vertices );
                    polygon.SetPosition( x, y );
                    mapObject = new PolygonMapObject( polygon );
                }
                else if ( ( child = node.GetChildByName( "polyline" ) ) != null )
                {
                    string[]? points   = child.GetAttribute( "points" )?.Split( " " );
                    var       vertices = new float[ points!.Length * 2 ];

                    for ( var i = 0; i < points.Length; i++ )
                    {
                        string[] point = points[ i ].Split( "," );
                        vertices[ i * 2 ]         = float.Parse( point[ 0 ] ) * scaleX;
                        vertices[ ( i * 2 ) + 1 ] = float.Parse( point[ 1 ] ) * scaleY * ( FlipY ? -1 : 1 );
                    }

                    var polyline = new Polyline( vertices );
                    polyline.SetPosition( x, y );
                    mapObject = new PolylineMapObject( polyline );
                }
                else if ( node.GetChildByName( "ellipse" ) != null )
                {
                    mapObject = new EllipseMapObject( x, FlipY ? y - height : y, width, height );
                }
            }

            uint id;

            if ( mapObject == null )
            {
                string? gid;

                if ( ( gid = node.GetAttribute( "gid", null ) ) != null )
                {
                    id = ( uint )long.Parse( gid );

                    bool flipHorizontally = ( id & FlipHorizontally ) != 0;
                    bool flipVertically = ( id & FlipVertically ) != 0;
                    ITiledMapTile? tile = map.Tilesets.GetTile( id & ~MaskClear );
                    var tiledMapTileMapObject = new TiledMapTileMapObject( tile, flipHorizontally, flipVertically );
                    TextureRegion textureRegion = tiledMapTileMapObject.TextureRegion!;

                    tiledMapTileMapObject.Properties.Put( "gid", id );
                    tiledMapTileMapObject.X = x;
                    tiledMapTileMapObject.Y = FlipY ? y : y - height;

                    float objectWidth  = node.GetFloatAttribute( "width", textureRegion.GetRegionWidth() );
                    float objectHeight = node.GetFloatAttribute( "height", textureRegion.GetRegionHeight() );

                    tiledMapTileMapObject.ScaleX   = scaleX * ( objectWidth / textureRegion.GetRegionWidth() );
                    tiledMapTileMapObject.ScaleY   = scaleY * ( objectHeight / textureRegion.GetRegionHeight() );
                    tiledMapTileMapObject.Rotation = node.GetFloatAttribute( "rotation" );

                    mapObject = tiledMapTileMapObject;
                }
                else
                {
                    mapObject = new RectangleMapObject( x, FlipY ? y - height : y, width, height );
                }
            }

            mapObject.Name = node.GetAttribute( "name", string.Empty ) ?? string.Empty;

            string? rotation;

            if ( ( rotation = node.GetAttribute( "rotation", null ) ) != null )
            {
                mapObject.Properties.Put( "rotation", float.Parse( rotation ) );
            }

            string? type;

            if ( ( type = node.GetAttribute( "type", null ) ) != null )
            {
                mapObject.Properties.Put( "type", type );
            }

            if ( ( id = node.GetUIntAttribute( "id" ) ) != 0 )
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
            mapObject.IsVisible = node.GetIntAttribute( "visible", 1 ) == 1;

            XmlReader.Element? properties = node.GetChildByName( "properties" );

            if ( properties != null )
            {
                LoadProperties( mapObject.Properties, properties );
            }

            IdToObject?[ id ] = mapObject;

            objects.Add( mapObject );
        }
    }

    /// <summary>
    /// Loads properties from the specified XML element and assigns them to the given
    /// map properties. If the element is null, or has no name, no properties are loaded.
    /// </summary>
    /// <param name="properties">
    /// The target <see cref="MapProperties"/> object in which to store the loaded properties.
    /// </param>
    /// <param name="element">The XML element containing the properties data.</param>
    /// <exception cref="RuntimeException">
    /// Thrown when an error occurs during the property loading process.
    /// </exception>
    protected void LoadProperties( MapProperties? properties, XmlReader.Element? element )
    {
        if ( element?.Name == null )
        {
            return;
        }

        if ( element.Name.Equals( "properties" ) )
        {
            List< XmlReader.Element? > props = element.GetChildrenByName( "property" );

            foreach ( XmlReader.Element? property in props )
            {
                string? name  = property?.GetAttribute( "name", null );
                string? value = property?.GetAttribute( "value", null );
                string? type  = property?.GetAttribute( "type", null );

                value ??= property?.Text;

                if ( type is "object" && properties != null )
                {
                    // Wait until the end of [LoadTiledMap] to fetch the object
                    try
                    {
                        uint          id          = uint.Parse( value! );
                        string        propName    = name!;
                        MapProperties targetProps = properties;

                        RunOnEndOfLoadTiled?.Add( ( id, propName, targetProps ) );
                    }
                    catch ( Exception )
                    {
                        throw new RuntimeException( $"Error parsing property {name} "
                                                  + $"of type object with value: [{value}]" );
                    }
                }
                else
                {
                    object? castValue = ParseProperty( name, value, type );
                    properties?.Put< object? >( name!, castValue );
                }
            }
        }
    }

    /// <summary>
    /// Assigns a <see cref="MapObject"/> retrieved by its unique identifier to a specified property
    /// within the provided <see cref="MapProperties"/>. If the object does not exist or the target
    /// properties are null, no operation will be performed.
    /// </summary>
    /// <param name="id">The unique identifier of the <see cref="MapObject"/> to assign.</param>
    /// <param name="name">The name of the property to which the object will be assigned.</param>
    /// <param name="mapProps">
    /// The <see cref="MapProperties"/> instance that will store the assigned property.
    /// </param>
    private void ExecutePropertyAssignment( uint id, string name, MapProperties? mapProps )
    {
        MapObject? obj = IdToObject?[ id ];
        mapProps?.Put( name, obj );
    }

    /// <summary>
    /// Converts a property value from a string representation to the specified type.
    /// Supported types include string, int, float, bool, and color (or colour).
    /// </summary>
    /// <param name="name">The name of the property being converted.</param>
    /// <param name="value">The string representation of the property value to convert.</param>
    /// <param name="type">The target type to which the value should be converted.</param>
    /// <returns>
    /// The converted property value as an object of the specified type, or null if the value is null.
    /// </returns>
    /// <exception cref="RuntimeException">
    /// Thrown when the specified type is neither null nor one of the supported types
    /// (string, int, float, bool, color, or colour).
    /// </exception>
    protected object? ParseProperty( string? name, string? value, string? type )
    {
        if ( value is null )
        {
            Logger.Error( "Property value is null!" );

            return null;
        }

        switch ( type )
        {
            case null:
                return value;

            case "int":
                return int.Parse( value );

            case "float":
                return float.Parse( value );

            case "bool":
                return bool.Parse( value );

            case "color":
            case "colour":
            {
                // Tiled uses the format #AARRGGBB
                string? opaqueColor = value?.Substring( 3 );
                string? alpha       = value?.Substring( 1, 3 );

                return Color.FromHexString( opaqueColor + alpha );
            }

            default:
                throw new RuntimeException( $"Wrong type given for property {name}, "
                                          + $"given : {type}, supported : string, bool, "
                                          + $"int, float, color ( or colour )" );
        }
    }

    /// <summary>
    /// Creates and configures a new instance of a <see cref="TiledMapTileLayer.Cell"/>
    /// based on the provided flipping and rotation parameters.
    /// </summary>
    /// <param name="flipHorizontally">Indicates whether the cell's content should be flipped horizontally.</param>
    /// <param name="flipVertically">Indicates whether the cell's content should be flipped vertically.</param>
    /// <param name="flipDiagonally">Indicates whether the cell's content should be flipped diagonally with rotation applied.</param>
    /// <returns>A configured instance of <see cref="TiledMapTileLayer.Cell"/>.</returns>
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
                    cell.SetRotation( TiledMapTileLayer.Cell.Rotate270 );

                    break;

                case true:
                    cell.SetRotation( TiledMapTileLayer.Cell.Rotate270 );

                    break;

                default:
                {
                    if ( flipVertically )
                    {
                        cell.SetRotation( TiledMapTileLayer.Cell.Rotate90 );
                    }
                    else
                    {
                        cell.SetFlipVertically( true );
                        cell.SetRotation( TiledMapTileLayer.Cell.Rotate270 );
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
    /// Extracts tile IDs from the specified XML element of a TMX map layer
    /// and returns them as an array of uint values.
    /// </summary>
    /// <param name="element">The XML element containing the data for the TMX map layer.</param>
    /// <param name="width">The width of the tile layer, used to determine the number of tile IDs.</param>
    /// <param name="height">The height of the tile layer, used to determine the number of tile IDs.</param>
    /// <returns>An array of uint values representing the tile IDs in the layer.</returns>
    /// <exception cref="RuntimeException">
    /// Thrown if the required "data" element is missing or if the tile encoding type
    /// is unsupported (e.g., "XML") within the provided TMX map layer.
    /// </exception>
    public uint[] GetTileIDs( XmlReader.Element element, int width, int height )
    {
        XmlReader.Element? data = element.GetChildByName( "data" );

        if ( data == null )
        {
            throw new RuntimeException( "data is missing" );
        }

        string? encoding = data.GetAttribute( "encoding", null );

        if ( encoding == null )
        {
            // no 'encoding' attribute means that the encoding is XML
            throw new RuntimeException( "Unsupported encoding (XML) for TMX Layer Data" );
        }

        var ids = new uint[ width * height ];

        if ( encoding.Equals( "csv" ) )
        {
            string[]? array = data.Text?.Split( "," );

            for ( var i = 0; i < array?.Length; i++ )
            {
                ids[ i ] = ( uint )long.Parse( array[ i ].Trim() );
            }
        }
        else
        {
            if ( encoding.Equals( "base64" ) )
            {
                Stream? inputStream = null;

                try
                {
                    string? compression = data.GetAttribute( "compression", null );
                    byte[]  bytes       = Convert.FromBase64String( data.Text ?? data.ToString() );

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
                            int read = inputStream.Read( temp );

                            while ( read < temp.Length )
                            {
                                int curr = inputStream.Read( temp, read, temp.Length - read );

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

                            ids[ ( y * width ) + x ] = ( uint )( MathUtils.UnsignedByteToInt( temp[ 0 ] )
                                                               | ( MathUtils.UnsignedByteToInt( temp[ 1 ] ) << 8 )
                                                               | ( MathUtils.UnsignedByteToInt( temp[ 2 ] ) << 16 )
                                                               | ( MathUtils.UnsignedByteToInt( temp[ 3 ] ) << 24 ) );
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
    /// Resolves the relative file path based on the directory of the specified reference file
    /// and returns a <see cref="FileInfo"/> object for the resolved file path.
    /// Optionally, the method ensures that the resolved path is within the specified root limit.
    /// </summary>
    /// <param name="file">
    /// The reference <see cref="FileInfo"/> object representing the base file path to resolve from.
    /// </param>
    /// <param name="path">
    /// The relative path to resolve against the directory of the specified reference file.
    /// </param>
    /// <param name="rootLimit">
    /// An optional root directory path to act as a boundary. If provided,
    /// the resolved path must be within this directory; otherwise, the method returns <c>null</c>.
    /// </param>
    /// <returns>
    /// A <see cref="FileInfo"/> object representing the resolved file path, or <c>null</c> if the
    /// input file is <c>null</c>, the relative path is invalid, or the resolved path is outside the root limit.
    /// </returns>
    protected static FileInfo? GetRelativeFileHandle( FileInfo? file, string? path, string? rootLimit = null )
    {
        if ( file == null || string.IsNullOrEmpty( path ) )
        {
            return null;
        }

        // Get the starting point (the directory of the reference file)
        string? baseDirectory = file.DirectoryName;

        if ( baseDirectory == null )
        {
            return null;
        }

        // Resolve the full destination path
        string combinedPath = Path.Combine( baseDirectory, path );
        string resolvedPath = Path.GetFullPath( combinedPath );

        // Security Check: If a root limit is provided, ensure resolvedPath is inside it
        if ( !string.IsNullOrEmpty( rootLimit ) )
        {
            string fullRoot = Path.GetFullPath( rootLimit );

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
    /// The Node is laid out as follows:-
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
            var firstgid    = ( uint )element.GetIntAttribute( "firstgid", 1 );
            var imageSource = string.Empty;
            var imageWidth  = 0;
            var imageHeight = 0;

            FileInfo? imageFileHandle = null;

            // Fetch the .tsx file which contains the image name etc.
            string? source = element.GetAttribute( "source", null );

            if ( source != null )
            {
                FileInfo? tsx = GetRelativeFileHandle( tmxFile, source );

                if ( tsx == null )
                {
                    throw new RuntimeException( $"Unable to find tileset source file: {source}" );
                }

                try
                {
                    element = XmlReader.Parse( tsx );

                    XmlReader.Element? imageElement;

                    if ( ( imageElement = element?.GetChildByName( "image" ) ) != null )
                    {
                        imageSource     = imageElement.GetAttribute( "source" );
                        imageWidth      = imageElement.GetIntAttribute( "width" );
                        imageHeight     = imageElement.GetIntAttribute( "height" );
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
                XmlReader.Element? imageElement;

                if ( ( imageElement = element.GetChildByName( "image" ) ) != null )
                {
                    imageSource     = imageElement.GetAttribute( "source" );
                    imageWidth      = imageElement.GetIntAttribute( "width" );
                    imageHeight     = imageElement.GetIntAttribute( "height" );
                    imageFileHandle = GetRelativeFileHandle( tmxFile, imageSource );
                }
            }

            string?            name       = element?.Get( "name", null );
            int                tilewidth  = element?.GetIntAttribute( "tilewidth" ) ?? 0;
            int                tileheight = element?.GetIntAttribute( "tileheight" ) ?? 0;
            int                spacing    = element?.GetIntAttribute( "spacing" ) ?? 0;
            int                margin     = element?.GetIntAttribute( "margin" ) ?? 0;
            XmlReader.Element? offset     = element?.GetChildByName( "tileoffset" );
            var                offsetX    = 0;
            var                offsetY    = 0;

            if ( offset != null )
            {
                offsetX = offset.GetIntAttribute( "x" );
                offsetY = offset.GetIntAttribute( "y" );
            }

            var tileSet = new TiledMapTileSet
            {
                Name = name ?? string.Empty
            };

            MapProperties      tileSetProperties = tileSet.Properties;
            XmlReader.Element? properties        = element?.GetChildByName( "properties" );

            if ( properties != null )
            {
                LoadProperties( tileSetProperties, properties );
            }

            tileSetProperties.Put( "firstgid", firstgid );

            List< XmlReader.Element? >? tileElements;

            if ( ( tileElements = element?.GetChildrenByName( "tile" ) ) == null )
            {
                throw new RuntimeException( "Error: No tile elements found!" );
            }

            var tileContext = new TileContext
            {
                ImageResolver = imageResolver,
                Tileset       = tileSet,
                TmxFile       = tmxFile
            };

            var imageDetails = new ImageDetails
            {
                ImageSource = imageSource,
                Width       = imageWidth,
                Height      = imageHeight,
                Image       = imageFileHandle
            };

            var tileMetrics = new TileMetrics
            {
                Name       = name,
                Firstgid   = firstgid,
                Tilewidth  = tilewidth,
                Tileheight = tileheight,
                Spacing    = spacing,
                Margin     = margin
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

            foreach ( XmlReader.Element? tileElement in tileElements )
            {
                uint                  localtid     = tileElement?.GetUIntAttribute( "id" ) ?? 0;
                ITiledMapTile?        tile         = tileSet.GetTile( firstgid + localtid );
                AnimatedTiledMapTile? animatedTile = CreateAnimatedTile( tileSet, tile, tileElement, firstgid );

                if ( animatedTile != null )
                {
                    animatedTiles.Add( animatedTile );
                    tile = animatedTile;
                }

                AddTileProperties( tile, tileElement );
                AddTileObjectGroup( tile, tileElement );
            }

            // replace original static tiles by animated tiles
            foreach ( AnimatedTiledMapTile animatedTile in animatedTiles )
            {
                tileSet.PutTile( animatedTile.ID, animatedTile );
            }

            Map.Tilesets.AddTileSet( tileSet );
        }
    }

    /// <summary>
    /// Adds static tiles to the map using the specified parameters. This implemengtation is
    /// abstract and must be overridden by subclasses.
    /// </summary>
    /// <param name="element">The XML element representing the current tile or node to process.</param>
    /// <param name="tileContext">The context associated with the tile layer being processed.</param>
    /// <param name="tileElements">A list of XML elements representing tiles to be added.</param>
    /// <param name="tileMetrics">Metrics that define the dimensions and properties of the tiles.</param>
    /// <param name="source">The source path or identifier of the tileset being used.</param>
    /// <param name="offsetX">The horizontal offset for tile placement, in pixels.</param>
    /// <param name="offsetY">The vertical offset for tile placement, in pixels.</param>
    /// <param name="imageDetails">Details about the image or texture associated with the tiles.</param>
    protected abstract void AddStaticTiles( XmlReader.Element? element,
                                            TileContext tileContext,
                                            List< XmlReader.Element? >? tileElements,
                                            TileMetrics tileMetrics,
                                            string? source,
                                            int offsetX,
                                            int offsetY,
                                            ImageDetails imageDetails );

    /// <summary>
    /// Adds properties from an XML element to the specified tiled map tile.
    /// </summary>
    /// <param name="tile">The tile to which the properties will be added.</param>
    /// <param name="element">The XML element containing the property definitions.</param>
    protected void AddTileProperties( ITiledMapTile? tile, XmlReader.Element? element )
    {
        Guard.Against.Null( tile );
        Guard.Against.Null( element );

        string? terrain;

        if ( ( terrain = element.GetAttribute( "terrain", null ) ) != null )
        {
            tile.Properties?.Put( "terrain", terrain );
        }

        string? probability;

        if ( ( probability = element.GetAttribute( "probability", null ) ) != null )
        {
            tile.Properties?.Put( "probability", probability );
        }

        string? type;

        if ( ( type = element.GetAttribute( "type", null ) ) != null )
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
    /// Adds object group information to a given tile by parsing XML element data.
    /// </summary>
    /// <param name="tile">The tile to which the object group will be added. Cannot be null.</param>
    /// <param name="element">
    /// The XML element from which the object group data will be parsed. Cannot be null.
    /// </param>
    protected void AddTileObjectGroup( ITiledMapTile? tile, XmlReader.Element? element )
    {
        Guard.Against.Null( tile );
        Guard.Against.Null( element );

        XmlReader.Element?          objectgroupElement = element.GetChildByName( "objectgroup" );
        List< XmlReader.Element? >? children           = objectgroupElement?.GetChildrenByName( "object" );

        if ( children != null )
        {
            foreach ( XmlReader.Element? objectElement in children )
            {
                LoadObject( Map, tile, objectElement );
            }
        }
    }

    /// <summary>
    /// Creates an animated tile using the provided tile set, base tile, XML element, and
    /// the specified first global ID.
    /// </summary>
    /// <param name="tileSet">The tile set containing the tile data.</param>
    /// <param name="tile">The base tile from which the animation will be created.</param>
    /// <param name="element">The XML element containing animation data for the tile.</param>
    /// <param name="firstgid">The first global ID associated with the tile set.</param>
    /// <returns>
    /// An instance of <see cref="AnimatedTiledMapTile"/> representing the animated tile, or
    /// null if the animation data is invalid.
    /// </returns>
    protected AnimatedTiledMapTile? CreateAnimatedTile( TiledMapTileSet tileSet,
                                                        ITiledMapTile? tile,
                                                        XmlReader.Element? element,
                                                        uint firstgid )
    {
        XmlReader.Element? animationElement = element?.GetChildByName( "animation" );

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

        foreach ( XmlReader.Element? frameElement in frames )
        {
            if ( frameElement != null )
            {
                uint id = firstgid + frameElement.GetUIntAttribute( "tileid" );

                staticTiles.Add( ( StaticTiledMapTile? )tileSet.GetTile( id ) );

                intervals.Add( frameElement.GetIntAttribute( "duration" ) );
            }
        }

        if ( tile == null )
        {
            return null;
        }

        var animatedTile = new AnimatedTiledMapTile( intervals, staticTiles! )
        {
            ID = tile.ID
        };

        return animatedTile;
    }

    /// <summary>
    /// Add a standard, non-animating, static tile to the map.
    /// </summary>
    /// <param name="tileSet"> The tileset containing the tile </param>
    /// <param name="textureRegion">
    /// The textureRegion within the tileset image which contains the tile.
    /// </param>
    /// <param name="tileId"> The tile ID </param>
    /// <param name="offsetX"> The X offset of the tile </param>
    /// <param name="offsetY"> The Y offset of the tile </param>
    protected void AddStaticTiledMapTile( TiledMapTileSet tileSet,
                                          TextureRegion? textureRegion,
                                          uint tileId,
                                          float offsetX,
                                          float offsetY )
    {
        Guard.Against.Null( tileSet );
        Guard.Against.Null( textureRegion );

        var tile = new StaticTiledMapTile( textureRegion )
        {
            ID      = tileId,
            OffsetX = offsetX,
            OffsetY = FlipY ? -offsetY : offsetY
        };

        tileSet.PutTile( tileId, tile );
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class BaseTmxLoaderParameters : AssetLoaderParameters
    {
        /// <summary>
        /// Specifies whether mipmaps should be generated for textures when loading assets.
        /// </summary>
        public bool GenerateMipMaps { get; set; }

        /// <summary>
        /// Specifies the texture minifying filter mode used when scaling down textures.
        /// Determines how texture samples are calculated when a texture is displayed at
        /// a smaller size than its original resolution.
        /// </summary>
        public TextureFilterMode TextureMinFilter { get; set; } = TextureFilterMode.Nearest;

        /// <summary>
        /// Specifies the texture magnification filter mode to be used when scaling textures up in size.
        /// Determines how the graphics hardware should interpolate or sample texture data
        /// when a texture is magnified to cover more pixels than its original resolution.
        /// </summary>
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

// ============================================================================
// ============================================================================