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

using LughSharp.Source.Assets.Loaders;
using LughSharp.Source.Assets.Loaders.Resolvers;
using LughSharp.Source.Graphics.Atlases;
using LughSharp.Source.Graphics.Images;
using LughSharp.Source.Graphics.OpenGL.Enums;
using LughSharp.Source.Maps.Tiled.Objects;

using XmlReader = LughSharp.Source.Utils.XML.XmlReader;

namespace LughSharp.Source.Maps.Tiled.Loaders;

/// <summary>
/// A TiledMap Loader which loads tiles from a TextureAtlas instead of separate images.
/// It requires a map-level property called 'atlas' with its value being the relative
/// path to the TextureAtlas.
/// <para>
/// The atlas must have in it indexed regions named after the tilesets used in the map.
/// The indexes shall be local to the tileset (not the global id). Strip whitespace and
/// rotation should not be used when creating the atlas.
/// </para>
/// </summary>
[PublicAPI]
public class AtlasTmxMapLoader( IFileHandleResolver resolver )
    : BaseTmxMapLoader< AtlasTmxMapLoader.AtlasTiledMapLoaderParameters >( resolver )
{
    protected readonly List< Texture2D > TrackedTextures = [ ];
    protected          IAtlasResolver?   AtlasResolver;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates a new instance of the <see cref="AtlasTmxMapLoader"/> class, using
    /// the specified <see cref="IFileHandleResolver"/>.
    /// </summary>
    public AtlasTmxMapLoader() : this( new InternalFileHandleResolver() )
    {
    }

    /// <summary>
    /// Loads a TiledMap from the specified file, using default <see cref="AtlasTiledMapLoaderParameters"/>.
    /// </summary>
    /// <param name="filename"> The path of the TiledMap. </param>
    /// <returns> The <see cref="TiledMap"/>. </returns>
    public TiledMap Load( string filename )
    {
        return Load( filename, new AtlasTiledMapLoaderParameters() );
    }

    /// <summary>
    /// Loads a TiledMap from the specified file, using the specified <see cref="AtlasTiledMapLoaderParameters"/>.
    /// </summary>
    /// <param name="filename"> The path of the TiledMap. </param>
    /// <param name="parameter"> The loader parameters to use. </param>
    /// <returns> The <see cref="TiledMap"/>. </returns>
    public TiledMap Load( string filename, AtlasTiledMapLoaderParameters parameter )
    {
        FileInfo tmxFile = Resolve( filename );

        // ----------------------------------------

        XmlRoot = XmlReader.Parse( tmxFile );

        // ----------------------------------------

        FileInfo atlasFileHandle = GetAtlasFileHandle( tmxFile );
        var      atlas           = new TextureAtlas( atlasFileHandle );

        AtlasResolver = new IAtlasResolver.DirectAtlasResolver( atlas );

        TiledMap map = LoadTiledMap( tmxFile, parameter, AtlasResolver );

        map.OwnedResources = [ new List< TextureAtlas >( new[] { atlas } ) ];

        SetTextureFilters( parameter.TextureMinFilter, parameter.TextureMagFilter );

        return map;
    }

    /// <summary>
    /// Asynchronously loads an asset using the specified parameters.
    /// </summary>
    /// <typeparam name="TP">The type of the parameters for loading the asset.</typeparam>
    /// <param name="manager">The asset manager responsible for managing the asset loading process.</param>
    /// <param name="filename">The name of the asset to be loaded.</param>
    /// <param name="file">The file information of the asset, if available.</param>
    /// <param name="parameter">An optional set of parameters used for loading the asset.</param>
    public override void LoadAsync< TP >( AssetManager? manager,
                                          string filename,
                                          FileInfo? tmxFile,
                                          TP? parameter ) where TP : class
    {
        FileInfo atlasHandle = GetAtlasFileHandle( tmxFile );

        AtlasResolver = new IAtlasResolver.AssetManagerAtlasResolver( manager, atlasHandle.Name );

        Map = LoadTiledMap( tmxFile, parameter as AtlasTiledMapLoaderParameters, AtlasResolver );
    }

    /// <summary>
    /// Synchronously loads an asset from the specified file using the provided asset
    /// manager and parameters.
    /// </summary>
    /// <param name="manager">The asset manager responsible for managing the asset loading process.</param>
    /// <param name="file">The file information of the asset to be loaded.</param>
    /// <param name="parameter">The optional parameters used for customizing the asset loading process.</param>
    /// <typeparam name="TP">The type of the asset loader parameters.</typeparam>
    /// <return>Returns the loaded asset object or null if the loading process fails.</return>
    public override TiledMap LoadSync< TP >( AssetManager manager,
                                             FileInfo file,
                                             TP? parameter ) where TP : class
    {
        if ( parameter is AtlasTiledMapLoaderParameters p )
        {
            SetTextureFilters( p.TextureMinFilter, p.TextureMagFilter );
        }

        return Map;
    }

    /// <summary>
    /// Returns the assets this asset requires to be loaded first. This method may be
    /// called on a thread other than the GL thread.
    /// </summary>
    /// <param name="filename">name of the asset to load</param>
    /// <param name="file">the resolved file to load</param>
    /// <param name="p">parameters for loading the asset</param>
    public override List< AssetDescriptor > GetDependencies< TP >( string filename,
                                                                   FileInfo file,
                                                                   TP? p ) where TP : class
    {
        return null!;
    }

    /// <inheritdoc />
    public override List< AssetDescriptor > GetDependencyAssetDescriptors( FileInfo tmxFile,
                                                                           TextureLoader.TextureLoaderParameters
                                                                               textureLoaderParameters )
    {
        var descriptors = new List< AssetDescriptor >
        {
            new( GetAtlasFileHandle( tmxFile ), typeof( TextureAtlas ), textureLoaderParameters )
        };

        return descriptors;
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
    protected override void AddStaticTiles( XmlReader.Element? element,
                                            TileContext tileContext,
                                            List< XmlReader.Element? >? tileElements,
                                            TileMetrics tileMetrics,
                                            string? source,
                                            int offsetX,
                                            int offsetY,
                                            ImageDetails imageDetails )
    {
        TextureAtlas? atlas = AtlasResolver?.GetAtlas();

        foreach ( Texture2D texture in atlas!.Textures )
        {
            TrackedTextures.Add( texture );
        }

        MapProperties props = tileContext.Tileset.Properties;

        string? imageSource = imageDetails.ImageSource;
        int     imageWidth  = imageDetails.Width;
        int     imageHeight = imageDetails.Height;
        string? regionsName = tileMetrics.Name;
        int     tilewidth   = tileMetrics.Tilewidth;
        int     tileheight  = tileMetrics.Tileheight;
        int     margin      = tileMetrics.Margin;
        int     spacing     = tileMetrics.Spacing;
        uint    firstgid    = tileMetrics.Firstgid;

        props.Put( "imagesource", imageSource );
        props.Put( "imagewidth", imageWidth );
        props.Put( "imageheight", imageHeight );
        props.Put( "tilewidth", tilewidth );
        props.Put( "tileheight", tileheight );
        props.Put( "margin", margin );
        props.Put( "spacing", spacing );

        if ( imageSource?.Length > 0 )
        {
            long lastgid = firstgid + ( imageWidth / tilewidth * ( imageHeight / tileheight ) ) - 1;

            foreach ( AtlasRegion? region in atlas.FindRegions( regionsName! ) )
            {
                // Handle unused tileIds
                if ( region != null )
                {
                    long tileId = firstgid + region.Index;

                    if ( ( tileId >= firstgid ) && ( tileId <= lastgid ) )
                    {
                        AddStaticTiledMapTile( tileContext.Tileset, region, ( uint )tileId, offsetX, offsetY );
                    }
                }
            }
        }

        if ( tileElements != null )
        {
            // Add tiles with individual image sources
            foreach ( XmlReader.Element? tileElement in tileElements )
            {
                uint           tileId = ( firstgid + tileElement?.GetUIntAttribute( "id" ) ) ?? 0;
                ITiledMapTile? tile   = tileContext.Tileset.GetTile( tileId );

                if ( tile == null )
                {
                    XmlReader.Element? imageElement = tileElement?.GetChildByName( "image" );

                    if ( imageElement != null )
                    {
                        string? regionName = imageElement.GetAttribute( "source" );

                        regionName = regionName?.Substring( 0, regionName.LastIndexOf( '.' ) );

                        AtlasRegion? region = atlas.FindRegion( regionName );

                        if ( region == null )
                        {
                            throw new RuntimeException( $"Tileset atlasRegion not found: {regionName}" );
                        }

                        AddStaticTiledMapTile( tileContext.Tileset, region, tileId, offsetX, offsetY );
                    }
                }
            }
        }
    }

    /// <summary>
    /// Resolves and retrieves the file handle for the associated texture atlas based on
    /// the provided TMX file.
    /// </summary>
    /// <param name="tmxFile">The TMX file from which the atlas file path is determined.</param>
    /// <returns>The file handle of the resolved texture atlas.</returns>
    /// <exception cref="RuntimeException">
    /// Thrown when the map is missing a properties node, the 'atlas' property is not
    /// specified, or the atlas file cannot be found.
    /// </exception>
    protected FileInfo GetAtlasFileHandle( FileInfo? tmxFile )
    {
        XmlReader.Element? properties = XmlRoot?.GetChildByName( "properties" );

        if ( properties == null )
        {
            throw new RuntimeException( "The map is missing a properties node." );
        }

        string? atlasFilePath = null;

        List< XmlReader.Element? > propertyList = properties.GetChildrenByName( "property" );

        foreach ( XmlReader.Element? property in propertyList )
        {
            if ( property != null )
            {
                string? name = property.GetAttribute( "name" );

                if ( name!.StartsWith( "atlas" ) )
                {
                    atlasFilePath = property.GetAttribute( "value" );

                    break;
                }
            }
        }

        if ( atlasFilePath == null )
        {
            throw new RuntimeException( "The map is missing the 'atlas' property" );
        }

        FileInfo? fileHandle = GetRelativeFileHandle( tmxFile, atlasFilePath );

        if ( fileHandle!.Exists )
        {
            return fileHandle;
        }

        throw new RuntimeException( $"The 'atlas' file could not be found: '{atlasFilePath}'" );
    }

    /// <summary>
    /// Sets the minification and magnification filters for all textures tracked by this loader.
    /// </summary>
    /// <param name="min"> The minification filter mode to apply to all tracked textures. </param>
    /// <param name="mag"> The magnification filter mode to apply to all tracked textures. </param>
    protected void SetTextureFilters( TextureFilterMode min, TextureFilterMode mag )
    {
        foreach ( Texture2D texture in TrackedTextures )
        {
            texture.SetFilter( min, mag );
        }

        TrackedTextures.Clear();
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class AtlasTiledMapLoaderParameters : BaseTmxLoaderParameters
    {
        public bool ForceTextureFilters { get; set; }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    protected interface IAtlasResolver : IImageResolver
    {
        public TextureAtlas? GetAtlas();

        // ====================================================================

        public class DirectAtlasResolver( TextureAtlas atlas )
            : IAtlasResolver
        {
            public TextureAtlas GetAtlas()
            {
                return atlas;
            }

            public TextureRegion? GetImage( string name )
            {
                return atlas.FindRegion( name );
            }
        }

        // ====================================================================

        public class AssetManagerAtlasResolver( AssetManager? assetManager, string atlasName )
            : IAtlasResolver
        {
            /// <summary>
            /// Gets the <see cref="TextureAtlas"/> from the <see cref="AssetManager"/>
            /// </summary>
            public TextureAtlas? GetAtlas()
            {
                return assetManager?.Get< TextureAtlas >( atlasName );
            }

            /// <summary>
            /// Gets the image, identified by the given name, from the <see cref="TextureAtlas"/>
            /// </summary>
            /// <param name="name"> The name of the atlas region. </param>
            public TextureRegion? GetImage( string name )
            {
                return GetAtlas()?.FindRegion( name );
            }
        }
    }
}

// ============================================================================
// ============================================================================