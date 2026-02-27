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

using LughSharp.Core.Assets;
using LughSharp.Core.Assets.Loaders.Resolvers;
using LughSharp.Core.Graphics;
using LughSharp.Core.Maps.Tiled.Objects;
using LughSharp.Core.Utils.Collections;
using LughSharp.Core.Utils.Exceptions;

using XmlReader = LughSharp.Core.Utils.XML.XmlReader;

namespace LughSharp.Core.Maps.Tiled.Loaders;

[PublicAPI]
public class TmxMapLoader : BaseTmxMapLoader< TmxMapLoader.LoaderParameters >
{
    /// <summary>
    /// Creates a new TmxMapLoader using a new <see cref="InternalFileHandleResolver"/>.
    /// </summary>
    public TmxMapLoader() : this( new InternalFileHandleResolver() )
    {
    }

    /// <summary>
    /// Creates a new TmxMapLoader using a supplied <see cref="IFileHandleResolver"/>.
    /// </summary>
    /// <param name="resolver"></param>
    public TmxMapLoader( IFileHandleResolver resolver ) : base( resolver )
    {
    }

    /// <summary>
    /// Loads the <see cref="TiledMap"/> from the given file. The file is resolved via
    /// the <see cref="IFileHandleResolver"/> set in the constructor of this class.
    /// By default it will resolve to an internal file. The map will be loaded for a
    /// y-up coordinate system.
    /// </summary>
    /// <param name="filename"> the filename </param>
    /// <returns> the TiledMap </returns>
    public TiledMap Load( string filename )
    {
        return Load( filename, new LoaderParameters() );
    }

    /// <summary>
    /// Loads the <see cref="TiledMap"/> from the given file. The file is resolved
    /// via the <see cref="IFileHandleResolver"/> set in the constructor of this class.
    /// By default it will resolve to an internal file.
    /// </summary>
    /// <param name="filename"> the filename </param>
    /// <param name="parameter"> specifies whether to use y-up, generate mip maps etc. </param>
    /// <returns> the TiledMap </returns>
    public TiledMap Load( string filename, LoaderParameters parameter )
    {
        FileInfo tmxFile = Resolve( filename );

        // ----------------------------------------

        XmlRoot = XmlReader.Parse( tmxFile );

        // ----------------------------------------

        var              textures     = new Dictionary< string, Texture >();
        List< FileInfo > textureFiles = GetDependencyFileHandles( tmxFile );

        foreach ( FileInfo textureFile in textureFiles )
        {
            var texture = new Texture( textureFile, parameter.GenerateMipMaps );

            texture.SetFilter( parameter.TextureMinFilter, parameter.TextureMagFilter );
            textures.Put( Path.GetFullPath( textureFile.Name ), texture );
        }

        // ----------------------------------------
        // Load the TiledMap

        TiledMap map = LoadTiledMap( tmxFile, parameter, new IImageResolver.DirectImageResolver( textures ) );

        map.OwnedResources = [ ..textures.Values.ToList() ];

        return map;
    }

    /// <summary>
    /// Loads the non-OpenGL part of the asset and injects any dependencies of
    /// the asset into the <paramref name="manager"/>.
    /// </summary>
    /// <param name="manager">The asset manager responsible for loading the asset.</param>
    /// <param name="filename"> The name of the asset to load. </param>
    /// <param name="file">The file information of the asset to load.</param>
    /// <param name="parameter">The parameters for loading the asset.</param>
    public override void LoadAsync< TP >( AssetManager manager,
                                          string filename,
                                          FileInfo? file,
                                          TP? parameter ) where TP : class
    {
        Map = LoadTiledMap( file,
                            parameter as LoaderParameters,
                            new IImageResolver.AssetManagerImageResolver( manager ) );
    }

    /// <summary>
    /// Loads the OpenGL part of the asset.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="tmxFile"> the resolved file to load </param>
    /// <param name="parameter"></param>
    public override TiledMap LoadSync< TP >( AssetManager manager,
                                             FileInfo tmxFile,
                                             TP? parameter ) where TP : class
    {
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

    /// <summary>
    /// </summary>
    /// <param name="tmxFile"></param>
    /// <param name="textureParameter"></param>
    /// <returns></returns>
    protected List< AssetDescriptor > GetDependencyAssetDescriptors( FileInfo tmxFile,
                                                                     AssetLoaderParameters textureParameter )
    {
        var descriptors = new List< AssetDescriptor >();

        List< FileInfo > fileHandles = GetDependencyFileHandles( tmxFile );

        foreach ( FileInfo handle in fileHandles )
        {
            descriptors.Add( new AssetDescriptor( handle, typeof( Texture ), textureParameter ) );
        }

        return descriptors;
    }

    /// <summary>
    /// Retrieves a list of file handles for the external dependencies of a TiledMap.
    /// These include tileset files and images referenced within the map or tilesets.
    /// </summary>
    /// <param name="tmxFile">
    /// The TiledMap (.tmx) file for which the dependency file handles are to be extracted.
    /// </param>
    /// <returns>
    /// A list of file handles corresponding to dependency files, such as tileset sources
    /// and associated images.
    /// </returns>
    /// <exception cref="RuntimeException">
    /// Thrown if the TiledMap file does not contain any tileset nodes, which are required
    /// to determine dependencies.
    /// </exception>
    protected List< FileInfo > GetDependencyFileHandles( FileInfo tmxFile )
    {
        var fileHandles = new List< FileInfo >();

        // TileSet descriptors
        List< XmlReader.Element? >? children;

        if ( ( children = XmlRoot?.GetChildrenByName( "tileset" ) ) == null )
        {
            throw new RuntimeException( "Error: Map does not contain tileset nodes." );
        }

        foreach ( XmlReader.Element? tileset in children )
        {
            string? source = tileset?.GetAttribute( "source", null );

            if ( source != null )
            {
                FileInfo?                   tsxFile      = GetRelativeFileHandle( tmxFile, source );
                XmlReader.Element?          tset         = XmlReader.Parse( tsxFile );
                List< XmlReader.Element? >? imageElement = tset?.GetChildrenByName( "image" );

                if ( imageElement != null )
                {
                    string?   imageSource = tset?.GetChildByName( "image" )?.GetAttribute( "source" );
                    FileInfo? image       = GetRelativeFileHandle( tsxFile, imageSource );

                    fileHandles.Add( image! );
                }
                else
                {
                    List< XmlReader.Element? >? tilelist = tset?.GetChildrenByName( "tile" );

                    if ( tilelist != null )
                    {
                        foreach ( XmlReader.Element? tile in tilelist )
                        {
                            string?   imageSource = tile?.GetChildByName( "image" )?.GetAttribute( "source" );
                            FileInfo? image       = GetRelativeFileHandle( tsxFile, imageSource );

                            fileHandles.Add( image! );
                        }
                    }
                }
            }
            else
            {
                XmlReader.Element? imageElement = tileset?.GetChildByName( "image" );

                if ( imageElement != null )
                {
                    string?   imageSource = imageElement.GetAttribute( "source" );
                    FileInfo? image       = GetRelativeFileHandle( tmxFile, imageSource );

                    fileHandles.Add( image! );
                }
                else
                {
                    List< XmlReader.Element? >? tileList = tileset?.GetChildrenByName( "tile" );

                    if ( tileList != null )
                    {
                        foreach ( XmlReader.Element? tile in tileList )
                        {
                            string?   imageSource = tile?.GetChildByName( "image" )?.GetAttribute( "source" );
                            FileInfo? image       = GetRelativeFileHandle( tmxFile, imageSource );

                            fileHandles.Add( image! );
                        }
                    }
                }
            }
        }

        // ImageLayer descriptors
        List< XmlReader.Element? >? imageLayerList = XmlRoot?.GetChildrenByName( "imagelayer" );

        if ( imageLayerList != null )
        {
            foreach ( XmlReader.Element? imageLayer in imageLayerList )
            {
                XmlReader.Element? image  = imageLayer?.GetChildByName( "image" );
                string?            source = image?.GetAttribute( "source", null );

                if ( source != null )
                {
                    FileInfo? handle = GetRelativeFileHandle( tmxFile, source );

                    fileHandles.Add( handle! );
                }
            }
        }

        return fileHandles;
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
        MapProperties props   = tileContext.Tileset.Properties;
        FileInfo?     image   = imageDetails.Image;
        FileInfo      tmxFile = tileContext.TmxFile;

        string? imageSource = imageDetails.ImageSource;
        int     imageWidth  = imageDetails.Width;
        int     imageHeight = imageDetails.Height;
        int     tilewidth   = tileMetrics.Tilewidth;
        int     tileheight  = tileMetrics.Tileheight;
        int     margin      = tileMetrics.Margin;
        int     spacing     = tileMetrics.Spacing;
        uint    firstgid    = tileMetrics.Firstgid;

        if ( image != null )
        {
            // One image for the whole tileSet
            TextureRegion? texture = tileContext.ImageResolver.GetImage( Path.GetFullPath( image.Name ) );

            if ( texture == null )
            {
                throw new RuntimeException( $"Tileset image not found: {image.Name}" );
            }

            props.Put( "imagesource", imageSource );
            props.Put( "imagewidth", imageWidth );
            props.Put( "imageheight", imageHeight );
            props.Put( "tilewidth", tilewidth );
            props.Put( "tileheight", tileheight );
            props.Put( "margin", margin );
            props.Put( "spacing", spacing );

            int stopWidth  = texture.GetRegionWidth() - tilewidth;
            int stopHeight = texture.GetRegionHeight() - tileheight;

            uint id = firstgid;

            for ( int y = margin; y <= stopHeight; y += tileheight + spacing )
            {
                for ( int x = margin; x <= stopWidth; x += tilewidth + spacing )
                {
                    var  tileRegion = new TextureRegion( texture, x, y, tilewidth, tileheight );
                    uint tileId     = id++;

                    AddStaticTiledMapTile( tileContext.Tileset, tileRegion, tileId, offsetX, offsetY );
                }
            }
        }
        else
        {
            if ( tileElements == null )
            {
                throw new RuntimeException( "Error: Tile Elements List is null!" );
            }

            // Every tile has its own image source
            foreach ( XmlReader.Element? tileElement in tileElements )
            {
                XmlReader.Element? imageElement = tileElement?.GetChildByName( "image" );

                if ( imageElement != null )
                {
                    imageSource = imageElement.GetAttribute( "source", null );

                    image = GetRelativeFileHandle( source != null
                                                       ? GetRelativeFileHandle( tmxFile, source )
                                                       : tmxFile,
                                                   imageSource );
                }

                TextureRegion? texture = tileContext.ImageResolver.GetImage( Path.GetFullPath( image?.Name! ) );
                uint           tileId  = firstgid + tileElement!.GetUIntAttribute( "id" );

                AddStaticTiledMapTile( tileContext.Tileset, texture, tileId, offsetX, offsetY );
            }
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// The Loader parameters to use. Simply extends <see cref="BaseTmxLoaderParameters"/>
    /// and doesn't add anything new.
    /// </summary>
    [PublicAPI]
    public class LoaderParameters : BaseTmxLoaderParameters;
}

// ============================================================================
// ============================================================================