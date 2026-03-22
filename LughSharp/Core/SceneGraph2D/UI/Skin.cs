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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using JetBrains.Annotations;

using LughSharp.Core.Files;
using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Atlases;
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Graphics.Images;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.Main;
using LughSharp.Core.SceneGraph2D.Utils;
using LughSharp.Core.Utils.Collections;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Exception = System.Exception;
using JsonReader = Newtonsoft.Json.JsonReader;
using JsonWriter = Newtonsoft.Json.JsonWriter;

namespace LughSharp.Core.SceneGraph2D.UI;

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
/// The new* methods return a copy of an instance in the skin.
/// </para>
/// </summary>
[PublicAPI]
public class Skin : IDisposable
{
    public Dictionary< Type, Dictionary< string, object > > Resources     { get; set; } = [ ];
    public Dictionary< string, Type >                       JsonClassTags { get; set; } = [ ];

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
        new( "TiledSceneDrawable",      typeof( TiledSceneDrawable ) ),
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
    public Skin( FileInfo skinFile ) : this( skinFile, LoadAtlasIfExists( skinFile ) )
    {
    }

    /// <summary>
    /// Creates a skin containing the texture regions from the specified
    /// atlas. The atlas is automatically disposed when the skin is disposed.
    /// </summary>
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
    public Skin( FileInfo? skinFile, TextureAtlas? atlas )
    {
        InitialiseJsonClassTags();

        _skinHome = skinFile?.DirectoryName ?? Files.Files.ContentRoot;

        if ( atlas != null )
        {
            Atlas = atlas;
            AddRegions( atlas );
        }

        if ( skinFile != null )
        {
            Load( skinFile );
        }
    }

    /// <summary>
    /// Initialises the table of default tag classes into the working dictionary.
    /// Further tag classes can be added to the working dictionary if required.
    /// </summary>
    private void InitialiseJsonClassTags()
    {
        foreach ( Tag tag in DefaultTagClasses )
        {
            JsonClassTags.Add( tag.Name, tag.Type );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="skinFile"></param>
    /// <returns></returns>
    private static TextureAtlas? LoadAtlasIfExists( FileInfo skinFile )
    {
        string skinHome  = skinFile.DirectoryName ?? "";
        string name      = Path.GetFileNameWithoutExtension( skinFile.Name );
        var    atlasFile = new FileInfo( Path.Combine( skinHome, $"{name}.atlas" ) );

        return atlasFile.Exists ? new TextureAtlas( atlasFile ) : null;
    }

    /// <summary>
    /// Adds all resources in the specified skin JSON file.
    /// </summary>
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
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="resource"></param>
    /// <typeparam name="T"></typeparam>
    public void Add< T >( string? name, T? resource ) where T : class
    {
        Add( name, resource, typeof( T ) );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="resource"></param>
    /// <param name="type"></param>
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
               || ( type == typeof( Sprite ) )
                      ? 256
                      : 64 );

            Resources.Put( type, typeResources );
        }

        typeResources.Put( name, resource );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
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
    /// <exception cref="RuntimeException">if the resource was not found.</exception>
    public T Get< T >( string? name )
    {
        return ( T )Get( name, typeof( T ) );
    }

    /// <summary>
    /// Returns a named resource of the specified type.
    /// </summary>
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
        if ( type == typeof( Sprite ) ) return GetSprite( name );

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
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool Has( string name, Type type )
    {
        if ( !Resources.TryGetValue( type, out Dictionary< string, object >? resource ) )
        {
            return false;
        }

        return resource.ContainsKey( name );
    }

    /// <summary>
    /// Returns the name to resource mapping for the specified type, or
    /// null if no resources of that type exist.
    /// </summary>
    public Dictionary< string, object >? GetAll( Type type )
    {
        return Resources.GetValueOrDefault( type );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
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
    public TextureRegion GetRegion( string name )
    {
        var region = Optional< TextureRegion? >( name );

        if ( region != null )
        {
            return region;
        }

        var texture = Optional< Texture >( name );

        if ( texture == null )
        {
            throw new RuntimeException( $"No TextureRegion or Texture registered with name: {name}" );
        }

        region = new TextureRegion( texture );

        Add< TextureRegion >( name, region );

        return region;
    }

    /// <summary>
    /// Returns an array with the <see cref="TextureRegion"/> that have an index that
    /// is not equal to -1, or null if none are found.
    /// </summary>
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
    public TiledSceneDrawable GetTiledDrawable( string name )
    {
        var tiled = Optional< TiledSceneDrawable? >( name );

        if ( tiled != null )
        {
            return tiled;
        }

        tiled = new TiledSceneDrawable( GetRegion( name ) )
        {
            Name = name
        };

        if ( Scale is not 1.0f )
        {
            tiled = ( TiledSceneDrawable )ScaleDrawable( tiled );

            tiled.Scale = Scale;
        }

        Add< TiledSceneDrawable >( name, tiled );

        return tiled;
    }

    /// <summary>
    /// Returns a registered ninepatch. If no ninepatch is found but a region exists with
    /// the name, a ninepatch is created from the region and stored in the skin. If the
    /// region is an <see cref="AtlasRegion"/> then its split AtlasRegion Values
    /// are used, otherwise the ninepatch will have the region as the center patch.
    /// </summary>
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
    public Sprite GetSprite( string name )
    {
        Sprite? sprite;

        if ( ( sprite = Optional< Sprite >( name ) ) != null )
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

            sprite ??= new Sprite( textureRegion );

            if ( Scale is not 1.0f )
            {
                sprite.SetSize( sprite.Width * Scale, sprite.Height * Scale );
            }

            Add< Sprite >( name, sprite );

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
                var sprite = Optional< Sprite >( name );

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
    public ISceneDrawable NewDrawable( string name )
    {
        return NewDrawable( GetDrawable( name ) );
    }

    /// <summary>
    /// Returns a tinted copy of a drawable found in the skin via <see cref="GetDrawable(string)"/>.
    /// </summary>
    public ISceneDrawable NewDrawable( string name, float r, float g, float b, float a )
    {
        return NewDrawable( GetDrawable( name ), new Color( r, g, b, a ) );
    }

    /// <summary>
    /// Returns a tinted copy of a drawable found in the skin via <see cref="GetDrawable(string)"/>.
    /// </summary>
    public ISceneDrawable NewDrawable( string? name, Color tint )
    {
        return string.IsNullOrEmpty( name )
            ? throw new ArgumentException( "name cannot be null or empty." )
            : NewDrawable( GetDrawable( name ), tint );
    }

    /// <summary>
    /// Returns a copy of the specified drawable.
    /// </summary>
    public static ISceneDrawable NewDrawable( ISceneDrawable drawable )
    {
        return drawable switch
               {
                   TiledSceneDrawable tiledDrawable     => new TiledSceneDrawable( tiledDrawable ),
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
    public ISceneDrawable NewDrawable( ISceneDrawable drawable, float r, float g, float b, float a )
    {
        return NewDrawable( drawable, new Color( r, g, b, a ) );
    }

    /// <summary>
    /// Returns a tinted copy of a drawable found in the skin via <see cref="GetDrawable(string)"/>.
    /// </summary>
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

        name  = name.Replace( "-disabled", "" ) + ( enabled ? "" : "-disabled" );
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
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <returns></returns>
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

        public override void WriteJson( JsonWriter writer, Color? value, JsonSerializer serializer )
        {
        }
    }

    // ========================================================================
    // ========================================================================

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
        /// 
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="serializer"></param>
        /// <param name="token"></param>
        /// <returns></returns>
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
        /// <param name="token"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        private BitmapFont ReadBitmapFont( JToken token, FileInfo file )
        {
            string path   = token[ "file" ]?.ToString() ?? throw new JsonException( "Font file missing" );
            bool   markup = token[ "markupEnabled" ]?.Value< bool >() ?? false;

            path = Path.Combine( _skin._skinHome, path );

            var fontFile = new FileInfo( Path.Combine( file.DirectoryName ?? "", path ) );

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
        /// <param name="token"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
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

    [PublicAPI]
    public class SkinReferenceConverter( Skin skin ) : JsonConverter
    {
        public override bool CanConvert( Type objectType )
        {
            return objectType == typeof( Color ) ||
                   objectType == typeof( BitmapFont ) ||
                   typeof( ISceneDrawable ).IsAssignableFrom( objectType );
        }

        public override object? ReadJson( JsonReader reader, Type objectType, object? existingValue,
                                          JsonSerializer serializer )
        {
            // 1. Handle string name lookup (e.g., "white")
            if ( reader.TokenType == JsonToken.String )
            {
                var name = ( string )reader.Value!;

                return skin.Get( name, objectType );
            }

            // 2. Handle object definition (e.g., { "r": 1, ... })
            if ( reader.TokenType == JsonToken.StartObject )
            {
                // To avoid StackOverflow/Error Context issues, create a clean serializer
                // that DOES NOT have this SkinReferenceConverter in its list.
                var localSettings = new JsonSerializerSettings
                {
                    ContractResolver = serializer.ContractResolver,
                    // Keep the ColorConverter if we are dealing with a Color
                    Converters = serializer.Converters.Where( c => c is not SkinReferenceConverter ).ToList()
                };

                var localSerializer = JsonSerializer.Create( localSettings );

                // Deserialize using the clean serializer
                return localSerializer.Deserialize( reader, objectType );
            }

            return null;
        }

        public override void WriteJson( JsonWriter writer, object? value, JsonSerializer serializer )
        {
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public record TintedDrawable
    {
        public string Name  { get; set; } = "white";
        public Color  Color { get; set; } = Color.White;
    }
}

// ============================================================================
// ============================================================================