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

using LughSharp.Core.Assets;
using LughSharp.Core.Assets.Loaders;
using LughSharp.Core.Assets.Loaders.Resolvers;
using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Atlases;
using LughSharp.Core.Graphics.OpenGL.Enums;
using LughSharp.Core.Maps.Tiled.Objects;
using LughSharp.Core.Utils.Exceptions;

using XmlReader = LughSharp.Core.Utils.XML.XmlReader;

namespace LughSharp.Core.Maps.Tiled.Loaders;

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
    protected readonly List< Texture > TrackedTextures = [ ];
    protected          IAtlasResolver? AtlasResolver;

    // ========================================================================
    // ========================================================================

    public AtlasTmxMapLoader() : this( new InternalFileHandleResolver() )
    {
    }

    public TiledMap Load( string filename )
    {
        return Load( filename, new AtlasTiledMapLoaderParameters() );
    }

    public TiledMap Load( string filename, AtlasTiledMapLoaderParameters parameter )
    {
        var tmxFile = Resolve( filename );

        // ----------------------------------------

        XmlRoot = XmlReader.Parse( tmxFile );

        // ----------------------------------------

        var atlasFileHandle = GetAtlasFileHandle( tmxFile );
        var atlas           = new TextureAtlas( atlasFileHandle );

        AtlasResolver = new IAtlasResolver.DirectAtlasResolver( atlas );

        var map = LoadTiledMap( tmxFile, parameter, AtlasResolver );

        map.OwnedResources = [ new List< TextureAtlas >( new[] { atlas } ) ];

        SetTextureFilters( parameter.TextureMinFilter, parameter.TextureMagFilter );

        return map;
    }

    /// <inheritdoc />
    public override void LoadAsync< TP >( AssetManager? manager,
                                          string filename,
                                          FileInfo? tmxFile,
                                          TP? parameter ) where TP : class
    {
        var atlasHandle = GetAtlasFileHandle( tmxFile );

        AtlasResolver = new IAtlasResolver.AssetManagerAtlasResolver( manager, atlasHandle.Name );

        Map = LoadTiledMap( tmxFile, parameter as AtlasTiledMapLoaderParameters, AtlasResolver );
    }

    /// <inheritdoc />
    public override TiledMap? LoadSync< TP >( AssetManager manager,
                                              FileInfo file,
                                              TP? parameter ) where TP : class
    {
        if ( parameter is AtlasTiledMapLoaderParameters p )
        {
            SetTextureFilters( p.TextureMinFilter, p.TextureMagFilter );
        }

        return Map;
    }

    /// <inheritdoc />
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
            new( GetAtlasFileHandle( tmxFile ), typeof( TextureAtlas ), textureLoaderParameters ),
        };

        return descriptors;
    }

    /// <inheritdoc />
    protected override void AddStaticTiles( XmlReader.Element? element,
                                            TileContext tileContext,
                                            List< XmlReader.Element? >? tileElements,
                                            TileMetrics tileMetrics,
                                            string? source,
                                            int offsetX,
                                            int offsetY,
                                            ImageDetails imageDetails )
    {
        var atlas = AtlasResolver?.GetAtlas();

        foreach ( var texture in atlas!.Textures )
        {
            TrackedTextures.Add( texture );
        }

        var props   = tileContext.Tileset.Properties;

        var imageSource = imageDetails.ImageSource;
        var imageWidth  = imageDetails.Width;
        var imageHeight = imageDetails.Height;
        var regionsName = tileMetrics.Name;
        var tilewidth   = tileMetrics.Tilewidth;
        var tileheight  = tileMetrics.Tileheight;
        var margin      = tileMetrics.Margin;
        var spacing     = tileMetrics.Spacing;
        var firstgid    = tileMetrics.Firstgid;

        props.Put( "imagesource", imageSource );
        props.Put( "imagewidth", imageWidth );
        props.Put( "imageheight", imageHeight );
        props.Put( "tilewidth", tilewidth );
        props.Put( "tileheight", tileheight );
        props.Put( "margin", margin );
        props.Put( "spacing", spacing );

        if ( imageSource?.Length > 0 )
        {
            var lastgid = ( firstgid + ( ( imageWidth / tilewidth ) * ( imageHeight / tileheight ) ) ) - 1;

            foreach ( var region in atlas.FindRegions( regionsName! ) )
            {
                // Handle unused tileIds
                if ( region != null )
                {
                    var tileId = firstgid + region.Index;

                    if ( ( tileId >= firstgid ) && ( tileId <= lastgid ) )
                    {
                        AddStaticTiledMapTile( tileContext.Tileset, region, tileId, offsetX, offsetY );
                    }
                }
            }
        }

        if ( tileElements != null )
        {
            // Add tiles with individual image sources
            foreach ( XmlReader.Element? tileElement in tileElements )
            {
                var tileId = ( firstgid + tileElement?.GetIntAttribute( "id" ) ) ?? 0;
                var tile   = tileContext.Tileset.GetTile( tileId );

                if ( tile == null )
                {
                    var imageElement = tileElement?.GetChildByName( "image" );

                    if ( imageElement != null )
                    {
                        var regionName = imageElement.GetAttribute( "source" );

                        regionName = regionName?.Substring( 0, regionName.LastIndexOf( '.' ) );

                        var region = atlas.FindRegion( regionName );

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

    protected FileInfo GetAtlasFileHandle( FileInfo? tmxFile )
    {
        var properties = XmlRoot?.GetChildByName( "properties" );

        if ( properties == null )
        {
            throw new RuntimeException( "The map is missing a properties node." );
        }

        string? atlasFilePath = null;

        var propertyList = properties.GetChildrenByName( "property" );

        if ( propertyList != null )
        {
            foreach ( XmlReader.Element? property in propertyList )
            {
                if ( property != null )
                {
                    var name = property.GetAttribute( "name" );

                    if ( name!.StartsWith( "atlas" ) )
                    {
                        atlasFilePath = property.GetAttribute( "value" );

                        break;
                    }
                }
            }
        }

        if ( atlasFilePath == null )
        {
            throw new RuntimeException( "The map is missing the 'atlas' property" );
        }

        var fileHandle = GetRelativeFileHandle( tmxFile, atlasFilePath );

        if ( fileHandle!.Exists )
        {
            return fileHandle;
        }

        throw new RuntimeException( $"The 'atlas' file could not be found: '{atlasFilePath}'" );
    }

    protected void SetTextureFilters( TextureFilterMode min, TextureFilterMode mag )
    {
        foreach ( var texture in TrackedTextures )
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

        public class AssetManagerAtlasResolver( AssetManager? assetManager, string atlasName )
            : IAtlasResolver
        {
            public TextureAtlas? GetAtlas()
            {
                return assetManager?.Get< TextureAtlas >( atlasName );
            }

            public TextureRegion? GetImage( string name )
            {
                return GetAtlas()?.FindRegion( name );
            }
        }
    }
}

// ============================================================================
// ============================================================================