///////////////////////////////////////////////////////////////////////////////
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

using LughSharp.Source.Collections;
using LughSharp.Source.Graphics.Atlases;
using LughSharp.Source.Graphics.Fonts;
using LughSharp.Source.Graphics.G2D;
using LughSharp.Source.Graphics.Images;
using LughSharp.Source.IO;
using LughSharp.Source.Scene2D.UI.Styles;
using LughSharp.Source.Scene2D.Utils;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Exception = System.Exception;
using JsonReader = Newtonsoft.Json.JsonReader;
using JsonWriter = Newtonsoft.Json.JsonWriter;

namespace LughSharp.Source.Scene2D.UI;

/// <summary>
/// A skin stores resources for UI widgets to use (texture regions, ninepatches,
/// fonts, colors, etc). Resources are named and can be looked up by name and type.
/// <para>
/// Resources can be described in JSON.
/// </para>
/// <para>
/// Skin provides useful conversions, such as allowing access to regions in the
/// atlas as ninepatches, sprites, drawables, etc. The get* methods return an
/// instance of the object in the skin.
/// </para>
/// <para>
/// The new methods return a copy of an instance in the skin.
/// </para>
/// </summary>
[PublicAPI]
public class Skin : IDisposable
{
    /// <summary>
    /// Stores a collection of resources grouped by their associated <see cref="Type"/>.
    /// Each <see cref="Type"/> key is associated with a dictionary mapping resource names
    /// to their corresponding objects. This property facilitates efficient organization
    /// and retrieval of resources based on their types and names.
    /// </summary>
    public Dictionary< Type, Dictionary< string, object > > Resources { get; set; } = [ ];

    /// <summary>
    /// Maintains a mapping between string identifiers and their corresponding <see cref="Type"/>
    /// definitions. This property is used for associating class type names in JSON data with
    /// their actual .NET types, facilitating seamless deserialization within the context of
    /// a <see cref="Skin"/> instance.
    /// </summary>
    public Dictionary< string, Type > JsonClassTags { get; set; } = [ ];

    /// <summary>
    /// Returns the <see cref="TextureAtlas"/> passed to this skin constructor, or null.
    /// </summary>
    public TextureAtlas? Atlas { get; set; }

    /// <summary>
    /// The scale used to size drawables created by this skin.
    /// <para>
    /// This can be useful when scaling an entire UI (eg with a stage's viewport) then using an
    /// atlas with images whose resolution matches the UI scale. The skin can then be scaled the
    /// opposite amount so that the larger or smaller images are drawn at the original size. For
    /// example, if the UI is scaled 2x, the atlas would have images that are twice the size, then
    /// the skin's scale would be set to 0.5.
    /// </para>
    /// </summary>
    public float Scale { get; set; } = 1.0f;

    // ========================================================================

    /// <summary>
    /// Represents a combination of a name and a type, used to define and tag resources
    /// within the skin.
    /// <para>
    /// This struct is primarily used internally by the skin to manage and look up resources,
    /// providing a mapping between string identifiers and their associated types.
    /// </para>
    /// </summary>
    protected struct Tag
    {
        public readonly string Name;
        public readonly Type   Type;

        public Tag( string name, Type type )
        {
            Name = name;
            Type = type;
        }
    }

    /// <summary>
    /// A table of default tag classes. These are the classes that can be used
    /// in the JSON skin file, and are automatically added to the working
    /// dictionary.
    /// </summary>
    //@formatter:off
    protected static readonly Tag[] DefaultTagClasses =
    [
        // --------------------------------------
        new( "BitmapFont",              typeof( BitmapFont ) ),
        new( "Color",                   typeof( Color ) ),
        // --------------------------------------
        new( "TintedDrawable",          typeof( TintedDrawable ) ),
        new( "NinePatchDrawable",       typeof( NinePatchDrawable ) ),
        new( "SpriteDrawable",          typeof( SpriteDrawable ) ),
        new( "TextureRegionDrawable",   typeof( TextureRegionDrawable ) ),
        new( "TiledDrawable",           typeof( TiledDrawable ) ),
        // --------------------------------------
        new( "ButtonStyle",             typeof( ButtonStyle ) ),
        new( "TextButtonStyle",         typeof( TextButtonStyle ) ),
        new( "ImageButtonStyle",        typeof( ImageButtonStyle ) ),
        new( "ImageTextButtonStyle",    typeof( ImageTextButtonStyle ) ),
        new( "CheckBoxStyle",           typeof( CheckBoxStyle ) ),
        // --------------------------------------
        new( "LabelStyle",              typeof( LabelStyle ) ),
        new( "ProgressBarStyle",        typeof( ProgressBarStyle ) ),
        new( "TextFieldStyle",          typeof( TextFieldStyle ) ),
        new( "ListBoxStyle",            typeof( ListBoxStyle ) ),
        new( "ScrollPaneStyle",         typeof( ScrollPaneStyle ) ),
        new( "SelectBoxStyle",          typeof( SelectBoxStyle ) ),
        new( "SliderStyle",             typeof( SliderStyle ) ),
        new( "SplitPaneStyle",          typeof( SplitPaneStyle ) ),
        new( "TextTooltipStyle",        typeof( TextTooltipStyle ) ),
        new( "TouchpadStyle",           typeof( TouchpadStyle ) ),
        new( "TreeStyle",               typeof( TreeStyle ) ),
        new( "WindowStyle",             typeof( WindowStyle ) )
        // --------------------------------------
    ];
    //@formatter:on

    // ========================================================================

    private readonly string _skinHome;
    private          bool   _disposed;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates an empty Skin.
    /// </summary>
    public Skin() : this( null, null )
    {
    }

    /// <summary>
    /// Creates a skin containing the resources in the specified skin JSON
    /// file. If a file in the same directory with a ".atlas" extension exists,
    /// it is loaded as a <see cref="TextureAtlas"/> and the texture regions
    /// added to the skin. The atlas is automatically disposed when the skin is
    /// disposed.
    /// </summary>
    /// <param name="skinFile">The skin JSON file to load.</param>
    public Skin( FileInfo skinFile ) : this( skinFile, LoadAtlasIfExists( skinFile ) )
    {
    }

    /// <summary>
    /// Creates a skin containing the texture regions from the specified
    /// atlas. The atlas is automatically disposed when the skin is disposed.
    /// </summary>
    /// <param name="atlas"> The atlas to load texture regions from. </param>
    public Skin( TextureAtlas atlas ) : this( null, atlas )
    {
    }

    /// <summary>
    /// Creates a skin containing the resources in the specified skin JSON
    /// file and the texture regions from the specified atlas.
    /// <para>
    /// The atlas is automatically disposed when the skin is disposed.
    /// </para>
    /// </summary>
    /// <param name="skinFile">The skin JSON file to load.</param>
    /// <param name="atlas"> The atlas to load texture regions from. </param>
    public Skin( FileInfo? skinFile, TextureAtlas? atlas )
    {
        Guard.Against.Null( atlas );
        Guard.Against.Null( skinFile );

        InitialiseJsonClassTags();

        _skinHome = skinFile.DirectoryName ?? Files.ContentRoot;
        Atlas     = atlas;

        AddRegions( atlas );
        Load( skinFile );
    }

    /// <summary>
    /// Initialises the table of default tag classes into the working dictionary.
    /// Further tag classes can be added to the working dictionary if required.
    /// </summary>
    private void InitialiseJsonClassTags()
    {
        // Start afresh
        JsonClassTags.Clear();

        foreach ( Tag tag in DefaultTagClasses )
        {
            JsonClassTags.Add( tag.Name, tag.Type );
        }
    }

    /// <summary>
    /// Attempts to load a texture atlas if an associated atlas file exists in the same
    /// directory as the provided skin file.
    /// </summary>
    /// <param name="skinFile">
    /// The file representing the skin, which is used to determine the atlas file's
    /// location and name.
    /// </param>
    /// <returns>
    /// A <see cref="TextureAtlas"/> instance if the associated atlas file exists;
    /// otherwise, null.
    /// </returns>
    private static TextureAtlas? LoadAtlasIfExists( FileInfo skinFile )
    {
        string skinHome  = skinFile.DirectoryName ?? string.Empty;
        string name      = Path.GetFileNameWithoutExtension( skinFile.Name );
        var    atlasFile = new FileInfo( Path.Combine( skinHome, $"{name}.atlas" ) );

        return atlasFile.Exists ? new TextureAtlas( atlasFile ) : null;
    }

    /// <summary>
    /// Adds all resources in the specified skin JSON file.
    /// </summary>
    /// <param name="skinFile">The file representing the skin to load.</param>
    public void Load( FileInfo skinFile )
    {
        try
        {
            string jsonText = File.ReadAllText( skinFile.FullName );
            var    settings = new JsonSerializerSettings();

            // SkinConverter handles the root { "Type": { "Name": { ... } } }
            settings.Converters.Add( new SkinConverter( this, skinFile ) );

            // Handles explicit Color object definitions/hex strings.
            settings.Converters.Add( new ColorConverter( this ) );

            // SkinReferenceConverter resolves string names back to skin resources.
            settings.Converters.Add( new SkinReferenceConverter( this ) );

            JsonConvert.DeserializeObject< Skin >( jsonText, settings );
        }
        catch ( Exception ex )
        {
            throw new Exception( $"Error reading skin file: {skinFile.FullName}", ex );
        }
    }

    /// <summary>
    /// Adds all named texture regions from the atlas. The atlas will not be
    /// automatically disposed when the skin is disposed.
    /// </summary>
    /// <param name="atlas">The atlas containing the texture regions to add.</param>
    public void AddRegions( TextureAtlas atlas )
    {
        for ( int i = 0, n = atlas.Regions.Count; i < n; i++ )
        {
            AtlasRegion? region = atlas.Regions[ i ];

            if ( region != null )
            {
                string? name = region.Name;

                if ( region.Index != -1 )
                {
                    name += $"_{region.Index}";
                }

                Add< TextureRegion >( name, region );
            }
        }
    }

    /// <summary>
    /// Adds the spcified resource to the skin.
    /// </summary>
    /// <param name="name"> The resource name. </param>
    /// <param name="resource"> The resource to add. </param>
    public void Add( string name, object resource )
    {
        Add( name, resource, resource.GetType() );
    }

    /// <summary>
    /// Adds a resource to the skin with the specified name and type.
    /// </summary>
    /// <typeparam name="T">The type of the resource to add.
    /// <b>The type must be a reference type.</b></typeparam>
    /// <param name="name">The name associated with the resource. Can be null.</param>
    /// <param name="resource">The resource to add. Can be null.</param>
    public void Add< T >( string? name, T? resource ) where T : class
    {
        Add( name, resource, typeof( T ) );
    }

    /// <summary>
    /// Adds a resource to the Skin instance under the specified name and type.
    /// </summary>
    /// <param name="name">The name to associate with the resource. Must not be null.</param>
    /// <param name="resource">The resource to add. Must not be null.</param>
    /// <param name="type">The type of the resource being added. Must not be null.</param>
    public void Add( string? name, object? resource, Type type )
    {
        Guard.Against.Null( name );
        Guard.Against.Null( resource );

        Dictionary< string, object >? typeResources = Resources.Get( type );

        if ( typeResources == null )
        {
            typeResources = new Dictionary< string, object >
                ( ( type == typeof( TextureRegion ) )
               || ( type == typeof( ISceneDrawable ) )
               || ( type == typeof( Sprite2D ) )
                      ? 256
                      : 64 );

            Resources.Put( type, typeResources );
        }

        typeResources.Put( name, resource );
    }

    /// <summary>
    /// Removes the specified resource of the given type from the skin.
    /// </summary>
    /// <param name="name">The name of the resource to remove.</param>
    /// <param name="type">The type of the resource to remove.</param>
    public void Remove( string name, Type type )
    {
        Guard.Against.Null( name );

        Resources.Get( type )?.Remove( name );
    }

    /// <summary>
    /// Returns a resource named "default" for the specified type.
    /// </summary>
    /// <exception cref="RuntimeException">if the resource was not found.</exception>
    public T Get< T >()
    {
        return ( T )Get( "default", typeof( T ) );
    }

    /// <summary>
    /// Returns a named resource of the specified type.
    /// </summary>
    /// <param name="name">The name of the resource to retrieve.</param>
    /// <exception cref="RuntimeException">if the resource was not found.</exception>
    public T Get< T >( string? name )
    {
        return ( T )Get( name, typeof( T ) );
    }

    /// <summary>
    /// Returns a named resource of the specified type.
    /// </summary>
    /// <param name="name">The name of the resource to retrieve.</param>
    /// <param name="type">The type of the resource to retrieve.</param>
    /// <exception cref="RuntimeException">if the resource was not found.</exception>
    public object Get( string? name, Type? type )
    {
        Guard.Against.Null( name );
        Guard.Against.Null( type );

        // Redirect specialized types to their specific getter methods
        // This bypasses the Resources[type] dictionary for interfaces/complex types
        if ( type == typeof( ISceneDrawable ) ) return GetDrawable( name );
        if ( type == typeof( TextureRegion ) ) return GetRegion( name );
        if ( type == typeof( NinePatch ) ) return GetPatch( name );
        if ( type == typeof( Sprite2D ) ) return GetSprite( name );

        // Use TryGetValue to avoid KeyNotFoundException
        if ( !Resources.TryGetValue( type, out Dictionary< string, object >? typeResources ) )
        {
            throw new RuntimeException( $"No {type.FullName} registered with name: {name}" );
        }

        return !typeResources.TryGetValue( name, out object? resource )
                   ? throw new RuntimeException( $"No {type.FullName} registered with name: {name}" )
                   : resource;
    }

    /// <summary>
    /// Returns a named resource of the specified type.
    /// </summary>
    /// <param name="name">The name of the resource to retrieve.</param>
    /// <returns> null if not found. </returns>
    public T? Optional< T >( string name )
    {
        if ( Resources.TryGetValue( typeof( T ), out Dictionary< string, object >? typeResources ) )
        {
            return ( T? )typeResources.Get( name );
        }

        return default;
    }

    /// <summary>
    /// Checks if a resource with the specified name and type exists in the skin.
    /// </summary>
    /// <param name="name">The name of the resource to check for.</param>
    /// <param name="type">The type of the resource to check for.</param>
    /// <returns>True if the resource exists; otherwise, false.</returns>
    public bool Has( string name, Type type )
    {
        return Resources.TryGetValue( type, out Dictionary< string, object >? resource )
            && resource.ContainsKey( name );
    }

    /// <summary>
    /// Retrieves all resources of the specified type from the skin.
    /// </summary>
    /// <param name="type">The type of the resources to retrieve.</param>
    /// <returns>
    /// A dictionary containing the resources of the specified type, or null if no
    /// resources of that type are found.
    /// </returns>
    public Dictionary< string, object >? GetAll( Type type )
    {
        return Resources.GetValueOrDefault( type );
    }

    /// <summary>
    /// Retrieves a color from the skin by the specified name.
    /// </summary>
    /// <param name="name">The name of the color to retrieve.</param>
    /// <returns>The color object associated with the specified name.</returns>
    public Color GetColor( string name )
    {
        return Get< Color >( name );
    }

    /// <summary>
    /// Returns the named <see cref="BitmapFont"/> from the skin, or null if not found.
    /// </summary>
    /// <param name="name"> The name of the font.</param>
    public BitmapFont GetFont( string name )
    {
        return Get< BitmapFont >( name );
    }

    /// <summary>
    /// Returns a registered texture region. If no region is found but a
    /// texture exists with the name, a region is created from the texture
    /// and stored in the skin.
    /// </summary>
    /// <param name="name"> The name of the texture region to retrieve.</param>
    public TextureRegion GetRegion( string name )
    {
        var region = Optional< TextureRegion? >( name );

        if ( region != null )
        {
            return region;
        }

        var texture = Optional< Texture2D >( name );

        if ( texture == null )
        {
            throw new RuntimeException( $"No TextureRegion or Texture registered with name: {name}" );
        }

        region = new TextureRegion( texture );

        Add< TextureRegion >( name, region );

        return region;
    }

    /// <summary>
    /// Retrieves a list of texture regions that match the specified region name.
    /// </summary>
    /// <param name="regionName">
    /// The name of the texture region to retrieve. The method will attempt to find
    /// regions with names following the pattern "&lt;regionName&gt;_&lt;index&gt;".
    /// </param>
    /// <returns>
    /// A list of texture regions matching the specified name pattern, or null if no
    /// regions are found.
    /// </returns>
    public List< TextureRegion >? GetRegions( string regionName )
    {
        var i = 0;

        List< TextureRegion >? regions = null;
        var                    region  = Optional< TextureRegion? >( $"{regionName}_{i++}" );

        if ( region != null )
        {
            regions = [ ];

            while ( region != null )
            {
                regions.Add( region );
                region = Optional< TextureRegion? >( $"{regionName}_{i++}" );
            }
        }

        return regions;
    }

    /// <summary>
    /// Returns a registered tiled drawable. If no tiled drawable is found but a
    /// region exists with the name, a tiled drawable is created from the region
    /// and stored in the skin.
    /// </summary>
    /// <param name="name">The name of the tiled drawable to retrieve.</param>
    /// <returns>The retrieved tiled drawable, or null if no tiled drawable is found.</returns>
    public TiledDrawable GetTiledDrawable( string name )
    {
        var tiled = Optional< TiledDrawable? >( name );

        if ( tiled != null )
        {
            return tiled;
        }

        tiled = new TiledDrawable( GetRegion( name ) )
        {
            Name = name
        };

        if ( Scale is not 1.0f )
        {
            tiled = ( TiledDrawable )ScaleDrawable( tiled );

            tiled.Scale = Scale;
        }

        Add< TiledDrawable >( name, tiled );

        return tiled;
    }

    /// <summary>
    /// Returns a registered ninepatch. If no ninepatch is found but a region exists with
    /// the name, a ninepatch is created from the region and stored in the skin. If the
    /// region is an <see cref="AtlasRegion"/> then its split AtlasRegion Values
    /// are used, otherwise the ninepatch will have the region as the center patch.
    /// </summary>
    /// <param name="name">The name of the ninepatch to retrieve.</param>
    /// <returns>The retrieved ninepatch, or null if no ninepatch is found.</returns>
    public NinePatch GetPatch( string name )
    {
        var patch = Optional< NinePatch? >( name );

        if ( patch != null )
        {
            return patch;
        }

        try
        {
            TextureRegion region = GetRegion( name );

            if ( region is AtlasRegion atlasRegion )
            {
                int[]? splits = atlasRegion.FindValue( "split" );

                if ( splits != null )
                {
                    patch = new NinePatch( atlasRegion, splits[ 0 ], splits[ 1 ], splits[ 2 ], splits[ 3 ] );

                    int[]? pads = atlasRegion.FindValue( "pad" );

                    if ( pads != null )
                    {
                        patch.SetPadding( pads[ 0 ], pads[ 1 ], pads[ 2 ], pads[ 3 ] );
                    }
                }
            }

            patch ??= new NinePatch( region );

            if ( Scale is not 1.0f )
            {
                patch.Scale( Scale, Scale );
            }

            Add< NinePatch >( name, patch );

            return patch;
        }
        catch ( RuntimeException )
        {
            throw new RuntimeException( $"No NinePatch, TextureRegion, or Texture registered with name: {name}" );
        }
    }

    /// <summary>
    /// Returns a registered sprite. If no sprite is found but a region exists
    /// with the name, a sprite is created from the region and stored in the skin.
    /// If the region is an <see cref="AtlasRegion"/> then an <see cref="AtlasSprite"/>
    /// is used if the region has been whitespace stripped or packed rotated 90 degrees.
    /// </summary>
    /// <param name="name">The name of the sprite to retrieve.</param>
    /// <returns>The retrieved sprite, or null if no sprite is found.</returns>
    public Sprite2D GetSprite( string name )
    {
        Sprite2D? sprite;

        if ( ( sprite = Optional< Sprite2D >( name ) ) != null )
        {
            return sprite;
        }

        try
        {
            TextureRegion textureRegion = GetRegion( name );

            if ( textureRegion is AtlasRegion region )
            {
                if ( region.Rotate
                  || ( region.PackedWidth != region.OriginalWidth )
                  || ( region.PackedHeight != region.OriginalHeight ) )
                {
                    sprite = new AtlasSprite( region );
                }
            }

            sprite ??= new Sprite2D( textureRegion );

            if ( Scale is not 1.0f )
            {
                sprite.SetSize( sprite.Width * Scale, sprite.Height * Scale );
            }

            Add< Sprite2D >( name, sprite );

            return sprite;
        }
        catch ( RuntimeException )
        {
            throw new RuntimeException( $"No NinePatch, TextureRegion, or Texture registered with name: {name}" );
        }
    }

    /// <summary>
    /// Returns a registered drawable. If no drawable is found but a region, ninepatch,
    /// or sprite exists with the name, then the appropriate drawable is created and
    /// stored in the skin.
    /// </summary>
    /// <param name="name"> The name of the drawable to retrieve. </param>
    /// <returns> The retrieved drawable. </returns>
    public ISceneDrawable GetDrawable( string name )
    {
        var drawable = Optional< ISceneDrawable >( name );

        if ( drawable != null )
        {
            return drawable;
        }

        // Use texture or texture region. If it has splits, use ninepatch.
        // If it has rotation or whitespace stripping, use sprite.
        try
        {
            TextureRegion textureRegion = GetRegion( name );

            if ( textureRegion is AtlasRegion region )
            {
                if ( region.FindValue( "split" ) != null )
                {
                    drawable = new NinePatchDrawable( GetPatch( name ) );
                }
                else if ( region.Rotate
                       || ( region.PackedWidth != region.OriginalWidth )
                       || ( region.PackedHeight != region.OriginalHeight ) )
                {
                    drawable = new SpriteDrawable( GetSprite( name ) );
                }
            }

            if ( drawable == null )
            {
                drawable = new TextureRegionDrawable( textureRegion );

                if ( Scale is not 1.0f )
                {
                    ScaleDrawable( drawable );
                }
            }
        }
        catch ( RuntimeException )
        {
            // Ignored
        }

        // Check for explicit registration of ninepatch, sprite, or tiled drawable.
        if ( drawable == null )
        {
            var patch = Optional< NinePatch >( name );

            if ( patch != null )
            {
                drawable = new NinePatchDrawable( patch );
            }
            else
            {
                var sprite = Optional< Sprite2D >( name );

                if ( sprite != null )
                {
                    drawable = new SpriteDrawable( sprite );
                }
                else
                {
                    throw new RuntimeException( $"No ISceneDrawable, NinePatch, TextureRegion,"
                                              + $" Texture, or Sprite registered with name: {name}" );
                }
            }
        }

        if ( drawable is BaseDrawable baseDrawable )
        {
            baseDrawable.Name = name;
        }

        Add< ISceneDrawable >( name, drawable );

        return drawable;
    }

    /// <summary>
    /// Returns the name of the specified style object, or null if it is not in the skin.
    /// This compares potentially every style object in the skin of the same type as the
    /// specified style, which may be a somewhat expensive operation.
    /// </summary>
    /// <param name="resource"> The resource to find. </param>
    /// <returns>
    /// The name of the specified style object, or null if it is not in the skin.
    /// </returns>
    public string? Find( object resource )
    {
        if ( resource == null )
        {
            throw new ArgumentException( "style cannot be null." );
        }

        Dictionary< string, object > typeResources = Resources[ resource.GetType() ];

        return typeResources.FindKey( resource );
    }

    /// <summary>
    /// Returns a copy of a drawable found in the skin via <see cref="GetDrawable(string)"/>.
    /// </summary>
    /// <param name="name"> The name of the drawable to retrieve. </param>
    /// <returns> The retrieved drawable. </returns>
    public ISceneDrawable NewDrawable( string name )
    {
        return NewDrawable( GetDrawable( name ) );
    }

    /// <summary>
    /// Returns a tinted copy of a drawable found in the skin via <see cref="GetDrawable(string)"/>.
    /// </summary>
    /// <param name="name"> The name of the drawable to retrieve. </param>
    /// <param name="r"> The red component of the tint. </param>
    /// <param name="g"> The green component of the tint. </param>
    /// <param name="b"> The blue component of the tint. </param>
    /// <param name="a"> The alpha component of the tint. </param>
    /// <returns> The retrieved drawable. </returns>
    public ISceneDrawable NewDrawable( string name, float r, float g, float b, float a )
    {
        return NewDrawable( GetDrawable( name ), new Color( r, g, b, a ) );
    }

    /// <summary>
    /// Returns a tinted copy of a drawable found in the skin via <see cref="GetDrawable(string)"/>.
    /// </summary>
    /// <param name="name"> The name of the drawable to retrieve. </param>
    /// <param name="tint"> The tint to apply to the drawable. </param>
    /// <returns> The retrieved drawable. </returns>
    public ISceneDrawable NewDrawable( string? name, Color tint )
    {
        return string.IsNullOrEmpty( name )
                   ? throw new ArgumentException( "name cannot be null or empty." )
                   : NewDrawable( GetDrawable( name ), tint );
    }

    /// <summary>
    /// Returns a copy of the specified drawable.
    /// </summary>
    /// <param name="drawable"> The drawable to copy. </param>
    /// <returns> The copied drawable. </returns>
    public static ISceneDrawable NewDrawable( ISceneDrawable drawable )
    {
        return drawable switch
               {
                   TiledDrawable tiledDrawable          => new TiledDrawable( tiledDrawable ),
                   TextureRegionDrawable regionDrawable => new TextureRegionDrawable( regionDrawable ),
                   NinePatchDrawable patchDrawable      => new NinePatchDrawable( patchDrawable ),
                   SpriteDrawable spriteDrawable        => new SpriteDrawable( spriteDrawable ),

                   // ---------------------------

                   var _ => throw new RuntimeException( $"Unable to copy, unknown "
                                                      + $"drawable type: {drawable.GetType()}" )
               };
    }

    /// <summary>
    /// Returns a tinted copy of a drawable found in the skin via <see cref="GetDrawable(string)"/>.
    /// </summary>
    /// <param name="drawable"> The drawable to copy. </param>
    /// <param name="r"> The red component of the tint. </param>
    /// <param name="g"> The green component of the tint. </param>
    /// <param name="b"> The blue component of the tint. </param>
    /// <param name="a"> The alpha component of the tint. </param>
    /// <returns> The copied drawable. </returns>
    public ISceneDrawable NewDrawable( ISceneDrawable drawable, float r, float g, float b, float a )
    {
        return NewDrawable( drawable, new Color( r, g, b, a ) );
    }

    /// <summary>
    /// Returns a tinted copy of a drawable found in the skin via <see cref="GetDrawable(string)"/>.
    /// </summary>
    /// <param name="drawable"> The drawable to copy. </param>
    /// <param name="tint"> The tint color. </param>
    /// <returns> The copied drawable. </returns>
    public ISceneDrawable NewDrawable( ISceneDrawable drawable, Color tint )
    {
        //@formatter:off
        ISceneDrawable newDrawable = drawable switch
         {
             TextureRegionDrawable regionDrawable => regionDrawable.Tint( tint ),
             NinePatchDrawable patchDrawable      => patchDrawable.Tint( tint ),
             SpriteDrawable spriteDrawable        => spriteDrawable.Tint( tint ),

             // ----------------

             _ => throw new RuntimeException( $"Unable to copy, unknown drawable type: {drawable.GetType()}" )
         };
        //@formatter:on

        if ( newDrawable is BaseDrawable named )
        {
            if ( drawable is BaseDrawable baseDrawable )
            {
                named.Name = $"{baseDrawable.Name} ({tint})";
            }
            else
            {
                named.Name = $" ({tint})";
            }
        }

        return newDrawable;
    }

    /// <summary>
    /// Scales the drawable's properties by the specified scale factor. Those
    /// properties are:
    /// <li><see cref="ISceneDrawable.LeftWidth"/></li>
    /// <li><see cref="ISceneDrawable.RightWidth"/></li>
    /// <li><see cref="ISceneDrawable.BottomHeight"/></li>
    /// <li><see cref="ISceneDrawable.TopHeight"/></li>
    /// <li><see cref="ISceneDrawable.MinWidth"/></li>
    /// <li><see cref="ISceneDrawable.MinHeight"/></li>
    /// </summary>
    /// <param name="drawable"> The drawable to scale. </param>
    /// <returns> The scaled drawable. </returns>
    public ISceneDrawable ScaleDrawable( ISceneDrawable drawable )
    {
        drawable.LeftWidth    *= Scale;
        drawable.RightWidth   *= Scale;
        drawable.BottomHeight *= Scale;
        drawable.TopHeight    *= Scale;
        drawable.MinWidth     *= Scale;
        drawable.MinHeight    *= Scale;

        return drawable;
    }

    /// <summary>
    /// Sets the style on the actor to disabled or enabled. This is done by appending
    /// "-disabled" to the style name when enabled is false, and removing "-disabled"
    /// from the style name when enabled is true. A method named "GetStyle" is called
    /// the actor via reflection and the name of that style is found in the skin. If
    /// the actor doesn't have a "GetStyle" method or the style was not found in the
    /// skin, no exception is thrown and the actor is left unchanged.
    /// </summary>
    /// <param name="actor"> The actor to set the enabled state on. </param>
    /// <param name="enabled"> The enabled state to set. </param>
    public void SetEnabled( Actor actor, bool enabled )
    {
        // Get current style.
        MethodInfo? method = actor.GetType().GetMethod( "GetStyle" );

        if ( method == null )
        {
            return;
        }

        object style;

        try
        {
            style = method.Invoke( actor, null )!;
        }
        catch ( Exception )
        {
            return;
        }

        // Determine new style.
        string? name = Find( style );

        if ( name == null )
        {
            return;
        }

        name  = name.Replace( "-disabled", string.Empty ) + ( enabled ? string.Empty : "-disabled" );
        style = Get( name, style.GetType() );

        // Set new style.
        if ( ( method = FindMethod( actor.GetType(), "SetStyle" ) ) == null )
        {
            return;
        }

        try
        {
            method.Invoke( actor, ( object?[]? )style );
        }
        catch ( Exception )
        {
            // ignored
        }
    }

    /// <summary>
    /// Outputs a list of all loaded styles of the specified type to the console.
    /// </summary>
    /// <param name="style">The type representing the styles to be listed.</param>
    public void Debug( Type style )
    {
        // Debug: List all loaded TextButtonStyles
        Dictionary< string, object >? styles = GetAll( style );

        if ( styles?.Keys != null )
        {
            foreach ( string name in styles.Keys )
            {
                Console.WriteLine( $"Loaded Style: {name}" );
            }
        }
    }

    /// <summary>
    /// Searches for a method with the specified name in the provided type, including
    /// public, non-public, instance, and static methods.
    /// </summary>
    /// <param name="type">The type in which to search for the method. Can be null.</param>
    /// <param name="name">The name of the method to look for. Can be null.</param>
    /// <returns>
    /// A <see cref="MethodInfo"/> object representing the found method, or null
    /// if no method with the specified name is found.
    /// </returns>
    private static MethodInfo? FindMethod( Type? type, string? name )
    {
        MethodInfo[] methods = type?.GetMethods( BindingFlags.Public
                                               | BindingFlags.NonPublic
                                               | BindingFlags.Instance
                                               | BindingFlags.Static ) ?? Array.Empty< MethodInfo >();

        foreach ( MethodInfo method in methods )
        {
            if ( method.Name == name )
            {
                return method;
            }
        }

        return null;
    }

    // ========================================================================

    /// <summary>
    /// Disposes the <see cref="TextureAtlas"/> and all <see cref="IDisposable"/>
    /// resources in the skin.
    /// </summary>
    public void Dispose()
    {
        Dispose( true );
        GC.SuppressFinalize( this );
    }

    /// <summary>
    /// Releases all resources used by the AssetManager.
    /// </summary>
    /// <param name="disposing">
    /// Indicates whether the method call comes from a Dispose method (true) or from
    /// a finalizer (false).
    /// </param>
    protected void Dispose( bool disposing )
    {
        if ( !_disposed )
        {
            if ( disposing )
            {
                Atlas?.Dispose();

                foreach ( Dictionary< string, object > entry in Resources.Values )
                {
                    foreach ( object resource in entry.Values )
                    {
                        if ( resource is IDisposable disposable )
                        {
                            disposable.Dispose();
                        }
                    }
                }
            }

            _disposed = true;
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// A JSON converter responsible for serializing and deserializing instances of the
    /// <see cref="Color"/> class within the context of a <see cref="Skin"/>.
    /// <para>
    /// This converter handles the translation of colors represented in various formats,
    /// such as hexadecimal strings or component-wise values, into Color instances.
    /// </para>
    /// <para>
    /// When reading JSON, it determines the appropriate color representation based on the
    /// provided structure and resolves named colors via the associated <see cref="Skin"/>.
    /// </para>
    /// <para>
    /// When writing JSON, it provides a serialized representation of the Color object.
    /// </para>
    /// </summary>
    [PublicAPI]
    public class ColorConverter( Skin skin ) : JsonConverter< Color >
    {
        public override Color ReadJson( JsonReader reader, Type type, Color? existing, bool hasExt,
                                        JsonSerializer serializer )
        {
            JToken token = JToken.Load( reader );

            if ( token.Type == JTokenType.String )
            {
                return skin.Get< Color >( token.ToString() );
            }

            if ( token[ "hex" ] != null )
            {
                return Color.FromHexString( token[ "hex" ]!.ToString() );
            }

            return new Color( token[ "r" ]?.Value< float >() ?? 0,
                              token[ "g" ]?.Value< float >() ?? 0,
                              token[ "b" ]?.Value< float >() ?? 0,
                              token[ "a" ]?.Value< float >() ?? 1 );
        }

        /// <summary>
        /// Writes a JSON representation of a <see cref="Color"/> instance to the specified writer.
        /// </summary>
        /// <param name="writer">The JSON writer used to write the serialized JSON.</param>
        /// <param name="value">The <see cref="Color"/> instance to serialize. Can be null.</param>
        /// <param name="serializer">The serializer instance used for custom serialization behavior.</param>
        public override void WriteJson( JsonWriter writer, Color? value, JsonSerializer serializer )
        {
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// A custom JSON converter for the <see cref="Skin"/> class.
    /// <para>
    /// This converter is responsible for handling the deserialization of skin resources,
    /// specifically the root structure defined with types, names, and resource mappings.
    /// </para>
    /// <para>
    /// The converter integrates into the JSON serialization pipeline to resolve and
    /// map skin resources from a file into corresponding objects within the <see cref="Skin"/> instance.
    /// </para>
    /// <para>
    /// During the deserialization process, the converter processes the root structure of
    /// the JSON data and utilizes auxiliary converters for handling specific object types
    /// such as colors and references within the skin.
    /// </para>
    /// </summary>
    [PublicAPI]
    public class SkinConverter : JsonConverter< Skin >
    {
        private readonly Skin     _skin;
        private readonly FileInfo _skinFile;

        // ====================================================================

        /// <summary>
        /// Creates a new instance of the <see cref="SkinConverter"/> class.
        /// </summary>
        /// <param name="skin"></param>
        /// <param name="skinFile"></param>
        public SkinConverter( Skin skin, FileInfo skinFile )
        {
            _skin     = skin;
            _skinFile = skinFile;
        }

        /// <summary>
        /// Reads the JSON representation of the object. <b>This method is called externally
        /// from the deserialization process when reading JSON.</b>
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">
        /// The existing value of object being read. If there is no existing value then
        /// <c>null</c> will be used.
        /// </param>
        /// <param name="hasExistingValue">The existing value has a value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override Skin ReadJson( JsonReader reader,
                                       Type objectType,
                                       Skin? existingValue,
                                       bool hasExistingValue,
                                       JsonSerializer serializer )
        {
            JObject root = JObject.Load( reader );

            foreach ( JProperty typeProperty in root.Properties() )
            {
                Type targetType = ResolveType( typeProperty.Name );

                if ( typeProperty.Value is not JObject resources )
                {
                    continue;
                }

                foreach ( JProperty resProperty in resources.Properties() )
                {
                    object? finalObject = targetType == typeof( BitmapFont )
                                              ? ReadBitmapFont( resProperty.Value, _skinFile )
                                              : SerializeToObject( targetType, serializer, resProperty.Value );

                    if ( finalObject != null )
                    {
                        _skin.Add( resProperty.Name, finalObject, targetType );
                    }
                }
            }

            return _skin;
        }

        /// <summary>
        /// Serializes a JSON token into an object of the specified target type.
        /// </summary>
        /// <param name="targetType">The type to which the token should be deserialized.</param>
        /// <param name="serializer">The JSON serializer used for deserialization.</param>
        /// <param name="token">The JSON token to be deserialized.</param>
        /// <returns>
        /// An object of the specified target type deserialized from the provided token,
        /// or null if deserialization fails.
        /// </returns>
        private object? SerializeToObject( Type targetType, JsonSerializer serializer, JToken token )
        {
            if ( token.Type == JTokenType.String )
            {
                return _skin.Get( token.ToString(), targetType );
            }

            // Create a local serializer without the SkinConverter to avoid loops
            var localSettings = new JsonSerializerSettings
            {
                Converters = serializer.Converters.Where( c => c is not SkinConverter ).ToList()
            };
            var localSerializer = JsonSerializer.Create( localSettings );

            using JsonReader reader = token.CreateReader();

            return localSerializer.Deserialize( reader, targetType );
        }

        /// <summary>
        /// Resolves the type associated with the specified name from the skin's JSON
        /// class tags or default tag classes.
        /// </summary>
        /// <param name="name">The name of the tag to resolve.</param>
        /// <returns>
        /// The <see cref="Type"/> corresponding to the specified name, or <see cref="object"/>
        /// if no match is found.
        /// </returns>
        private Type ResolveType( string name )
        {
            Type type = _skin.JsonClassTags.GetValueOrDefault( name ) ??
                        DefaultTagClasses.FirstOrDefault( t => name.Contains( t.Name ) ).Type ??
                        typeof( object );

            return type;
        }

        /// <summary>
        /// Extracts a <see cref="BitmapFont"/> from the specified JSON token./>
        /// </summary>
        /// <param name="token">The JSON token containing font information.</param>
        /// <param name="file">The file associated with the JSON token.</param>
        /// <returns>The extracted <see cref="BitmapFont"/> instance.</returns>
        /// <exception cref="JsonException">Thrown when the font file is missing.</exception>
        private BitmapFont ReadBitmapFont( JToken token, FileInfo file )
        {
            string path   = token[ "file" ]?.ToString() ?? throw new JsonException( "Font file missing" );
            bool   markup = token[ "markupEnabled" ]?.Value< bool >() ?? false;

            path = Path.Combine( _skin._skinHome, path );

            var fontFile = new FileInfo( Path.Combine( file.DirectoryName ?? string.Empty, path ) );

            if ( !fontFile.Exists )
            {
                fontFile = Engine.Files.Internal( path );
            }

            string regionName = Path.GetFileNameWithoutExtension( fontFile.Name );

            List< TextureRegion >? regions = _skin.GetRegions( regionName );

            BitmapFont font = regions is { Count: > 0 }
                                  ? new BitmapFont( new BitmapFontData( fontFile ), regions )
                                  : new BitmapFont( fontFile );

            font.FontData.MarkupEnabled = markup;

            return font;
        }

        /// <summary>
        /// Extracts a <see cref="TintedDrawable"/> from the specified JSON token.
        /// </summary>
        /// <param name="token">The JSON token containing drawable information.</param>
        /// <param name="serializer">The JSON serializer used for deserialization.</param>
        /// <returns>The extracted <see cref="TintedDrawable"/> instance.</returns>
        private ISceneDrawable ReadTintedDrawable( JToken token, JsonSerializer serializer )
        {
            var   name  = token[ "name" ]?.ToString();
            Color color = token[ "color" ]?.ToObject< Color >( serializer ) ?? Color.White;

            return _skin.NewDrawable( name, color );
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson( JsonWriter writer, Skin? value, JsonSerializer serializer )
        {
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// A custom JSON converter for resolving references in a Skin instance.
    /// This converter is used to map string names in JSON back to skin resources such as colors,
    /// fonts, drawables, styles, or other related objects contained within the Skin.
    /// <para>
    /// The converter allows handling of complex object types, ensuring that references to
    /// resources stored in the Skin are correctly resolved during deserialization.
    /// </para>
    /// <para>
    /// It also manages exclusions to avoid infinite recursion issues when resolving nested objects
    /// that reference the same Skin, by temporarily replacing itself with a specialized variant
    /// during recursive deserialization.
    /// </para>
    /// </summary>
    [PublicAPI]
    public class SkinReferenceConverter : JsonConverter
    {
        private readonly Skin  _skin;
        private readonly Type? _excludedType;

        /// <summary>
        /// Provides a custom JSON converter for resolving string references to skin resources.
        /// </summary>
        /// <param name="skin">The Skin instance to which this converter is associated.</param>
        /// <param name="excludedType">An optional type to exclude from conversion.</param>
        public SkinReferenceConverter( Skin skin, Type? excludedType = null )
        {
            _skin         = skin;
            _excludedType = excludedType;
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert( Type objectType )
        {
            if ( _excludedType != null && objectType == _excludedType )
                return false;

            return objectType == typeof( Color ) ||
                   objectType == typeof( BitmapFont ) ||
                   typeof( ISceneDrawable ).IsAssignableFrom( objectType ) ||
                   typeof( ISceneStyle ).IsAssignableFrom( objectType );
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object? ReadJson( JsonReader reader, Type objectType, object? existingValue,
                                          JsonSerializer serializer )
        {
            // Handle string name lookup (e.g., "white", "button-down", "default")
            if ( reader.TokenType == JsonToken.String )
            {
                var name = ( string )reader.Value!;

                return _skin.Get( name, objectType );
            }

            // Handle inline object definition (e.g., { "r": 1, ... } or { "down": "button-down", ... })
            if ( reader.TokenType == JsonToken.StartObject )
            {
                if ( typeof( ISceneStyle ).IsAssignableFrom( objectType ) )
                {
                    // Style objects contain drawable/color/nested-style fields that need
                    // SkinReferenceConverter. Replace this converter with a variant that
                    // excludes the current type, preventing infinite re-entry while keeping
                    // resolution working for all field types.
                    var localSettings = new JsonSerializerSettings
                    {
                        ContractResolver = serializer.ContractResolver,
                        Converters = serializer.Converters
                                               .Where( c => c is not SkinReferenceConverter )
                                               .Append( new SkinReferenceConverter( _skin, objectType ) )
                                               .ToList()
                    };

                    return JsonSerializer.Create( localSettings ).Deserialize( reader, objectType );
                }

                // For non-style types (Color, ISceneDrawable) use a clean serializer to
                // avoid re-entry — their fields are primitives/textures, not skin references.
                var cleanSettings = new JsonSerializerSettings
                {
                    ContractResolver = serializer.ContractResolver,
                    Converters       = serializer.Converters.Where( c => c is not SkinReferenceConverter ).ToList()
                };

                return JsonSerializer.Create( cleanSettings ).Deserialize( reader, objectType );
            }

            return null;
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson( JsonWriter writer, object? value, JsonSerializer serializer )
        {
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Represents a drawable object that can be tinted with a specified color.
    /// <para>
    /// A TintedDrawable associates a name with a color, enabling customizable
    /// visual appearances by applying the specified color tint to the drawable resource.
    /// </para>
    /// <para>
    /// It is often used in UI designs where visual elements need to share the same
    /// base graphic but differ in color, providing a flexible and efficient way to
    /// apply common assets while varying their presentation.
    /// </para>
    /// </summary>
    [PublicAPI]
    public record TintedDrawable
    {
        public string Name  { get; set; } = "white";
        public Color  Color { get; set; } = Color.White;
    }
}

// ============================================================================
// ============================================================================