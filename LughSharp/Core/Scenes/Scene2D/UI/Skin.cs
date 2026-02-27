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

using System.Collections;
using System.Reflection;

using JetBrains.Annotations;

using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Atlases;
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.Main;
using LughSharp.Core.Maths;
using LughSharp.Core.Scenes.Scene2D.Utils;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Collections;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Json;
using LughSharp.Core.Utils.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Color = LughSharp.Core.Graphics.Color;
using Exception = System.Exception;
using JsonReader = Newtonsoft.Json.JsonReader;
using JsonWriter = Newtonsoft.Json.JsonWriter;

namespace LughSharp.Core.Scenes.Scene2D.UI;

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
    public Dictionary< Type, Dictionary< string, object >? > Resources     { get; set; } = [ ];
    public Dictionary< string, Type >                        JsonClassTags { get; set; } = [ ];

    /// <summary>
    /// Returns the <see cref="TextureAtlas"/> passed to this skin constructor, or null.
    /// </summary>
    public TextureAtlas? Atlas { get; set; }

    /// <summary>
    /// The scale used to size drawables created by this skin.
    /// <para>
    /// This can be useful when scaling an entire UI (eg with a stage's viewport)
    /// then using an atlas with images whose resolution matches the UI scale.
    /// The skin can then be scaled the opposite amount so that the larger or smaller
    /// images are drawn at the original size. For example, if the UI is scaled 2x,
    /// the atlas would have images that are twice the size, then the skin's scale
    /// would be set to 0.5.
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

    //@formatter:off
    protected static readonly Tag[] DefaultTagClasses =
    [
        new( "BitmapFont",              typeof( BitmapFont ) ),
        new( "Color",                   typeof( Color ) ),
        new( "TintedDrawable",          typeof( TintedDrawable ) ),
        new( "NinePatchDrawable",       typeof( NinePatchDrawable ) ),
        new( "SpriteDrawable",          typeof( SpriteDrawable ) ),
        new( "TextureRegionDrawable",   typeof( TextureRegionDrawable ) ),
        new( "TiledSceneDrawable",      typeof( TiledSceneDrawable ) ),
        new( "ButtonStyle",             typeof( Button.ButtonStyle ) ),
        new( "TextButtonStyle",         typeof( TextButton.TextButtonStyle ) ),
        new( "CheckBoxStyle",           typeof( CheckBox.CheckBoxStyle ) ),
        new( "LabelStyle",              typeof( Label.LabelStyle ) ),
        new( "ProgressBarStyle",        typeof( ProgressBar.ProgressBarStyle ) ),
        new( "TextFieldStyle",          typeof( TextField.TextFieldStyle ) ),
        new( "ImageButtonStyle",        typeof( ImageButton.ImageButtonStyle ) ),
        new( "ImageTextButtonStyle",    typeof( ImageTextButton.ImageTextButtonStyle ) ),
        new( "ListBoxStyle",            typeof( ListBox<>.ListBoxStyle ) ),
        new( "ScrollPaneStyle",         typeof( ScrollPane.ScrollPaneStyle ) ),
        new( "SelectBoxStyle",          typeof( SelectBox<>.SelectBoxStyle ) ),
        new( "SliderStyle",             typeof( Slider.SliderStyle ) ),
        new( "SplitPaneStyle",          typeof( SplitPane.SplitPaneStyle ) ),
        new( "TextTooltipStyle",        typeof( TextTooltip.TextTooltipStyle ) ),
        new( "TouchpadStyle",           typeof( Touchpad.TouchpadStyle ) ),
        new( "TreeStyle",               typeof( Tree< , >.TreeStyle ) ),
        new( "WindowStyle",             typeof( Window.WindowStyle ) )
    ];
    //@formatter:on

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates an empty Skin.
    /// </summary>
    public Skin()
    {
        Setup();
    }

    /// <summary>
    /// Creates a skin containing the resources in the specified skin JSON
    /// file. If a file in the same directory with a ".atlas" extension exists,
    /// it is loaded as a <see cref="TextureAtlas"/> and the texture regions
    /// added to the skin. The atlas is automatically disposed when the skin is
    /// disposed.
    /// </summary>
    public Skin( FileInfo skinFile )
    {
        Setup();

        string name      = Path.GetFileNameWithoutExtension( skinFile.Name );
        var    atlasFile = new FileInfo( $"{name}.atlas" );

        if ( atlasFile.Exists )
        {
            Atlas = new TextureAtlas( atlasFile );
            AddRegions( Atlas );
        }

        Load( skinFile );
    }

    /// <summary>
    /// Creates a skin containing the resources in the specified skin JSON
    /// file and the texture regions from the specified atlas.
    /// <para>
    /// The atlas is automatically disposed when the skin is disposed.
    /// </para>
    /// </summary>
    public Skin( FileInfo skinFile, TextureAtlas atlas )
    {
        Setup();

        Atlas = atlas;
        AddRegions( atlas );
        Load( skinFile );
    }

    /// <summary>
    /// Creates a skin containing the texture regions from the specified
    /// atlas. The atlas is automatically disposed when the skin is disposed.
    /// </summary>
    public Skin( TextureAtlas atlas )
    {
        Setup();

        Atlas = atlas;
        AddRegions( atlas );
    }

    /// <summary>
    /// 
    /// </summary>
    private void Setup()
    {
        foreach ( Tag c in DefaultTagClasses )
        {
            JsonClassTags.Add( c.Type.Name, c.Type );
        }
    }

    /// <summary>
    /// Adds all resources in the specified skin JSON file.
    /// </summary>
    public void Load( FileInfo skinFile )
    {
        try
        {
            string jsonText = File.ReadAllText( skinFile.FullName );

            var settings = new JsonSerializerSettings();

            settings.Converters.Add( new SkinConverter( this, skinFile ) );
            settings.Converters.Add( new ColorConverter( this ) );

            JsonConvert.DeserializeObject< Skin >( jsonText, settings );
        }
        catch ( JsonException ex )
        {
            throw new Exception( $"Error reading file: {skinFile}", ex );
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
            string?      name   = region?.Name;

            if ( region?.Index != -1 )
            {
                name += $"_{region?.Index}";
            }

            Add( name, region, typeof( TextureRegion ) );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="resource"></param>
    public void Add( string name, object resource )
    {
        Add( name, resource, resource.GetType() );
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

        if ( type == typeof( ISceneDrawable ) )
        {
            return GetDrawable( name );
        }

        if ( type == typeof( TextureRegion ) )
        {
            return GetRegion( name );
        }

        if ( type == typeof( NinePatch ) )
        {
            return GetPatch( name );
        }

        if ( type == typeof( Sprite ) )
        {
            return GetSprite( name );
        }

        Dictionary< string, object >? typeResources = Resources[ type ];

        if ( typeResources == null )
        {
            throw new RuntimeException( $"No {type.FullName} registered with name: {name}" );
        }

        object? resource = typeResources[ name ];

        return resource ?? throw new RuntimeException( $"No {type.FullName} registered with name: {name}" );
    }

    /// <summary>
    /// Returns a named resource of the specified type.
    /// </summary>
    /// <returns> null if not found. </returns>
    public T? Optional< T >( string name )
    {
        Guard.Against.Null( name );

        if ( typeof( T ) == null )
        {
            throw new ArgumentException( "type cannot be null." );
        }

        return ( T? )Resources[ typeof( T ) ]?.Get( name );
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
            Logger.Error( $"Resources Dictionary does not contain key: {type.Name}" );

            return false;
        }

        return resource!.ContainsKey( name );
    }

    /// <summary>
    /// Returns the name to resource mapping for the specified type, or
    /// null if no resources of that type exist.
    /// </summary>
    public Dictionary< string, object >? GetAll( Type type )
    {
        return Resources[ type ];
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

        Add( name, region, typeof( TextureRegion ) );

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

        Add( name, tiled, typeof( TiledSceneDrawable ) );

        return tiled;
    }

    /// <summary>
    /// Returns a registered ninepatch. If no ninepatch is found but a region exists with
    /// the name, a ninepatch is created from the region and stored in the skin. If the
    /// region is an <see cref="AtlasRegion"/> then its split <see cref="AtlasRegion.Values"/>
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

            Add( name, patch, typeof( NinePatch ) );

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

            Add( name, sprite, typeof( Sprite ) );

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
            // TODO:
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
                    throw new
                        RuntimeException( $"No ISceneDrawable, NinePatch, TextureRegion, Texture, or Sprite registered with name: {name}" );
                }
            }
        }

        if ( drawable is BaseDrawable baseDrawable )
        {
            baseDrawable.Name = name;
        }

        Add( name, drawable, typeof( ISceneDrawable ) );

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

        Dictionary< string, object >? typeResources = Resources[ resource.GetType() ];

        return typeResources?.FindKey( resource );
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
        if ( string.IsNullOrEmpty( name ) )
        {
            throw new ArgumentException( "name cannot be null or empty." );
        }

        return NewDrawable( GetDrawable( name ), tint );
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

                   var _ =>
                       throw new RuntimeException( $"Unable to copy, unknown drawable type: {drawable.GetType()}" )
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
        ISceneDrawable newDrawable = drawable switch
                                     {
                                         TextureRegionDrawable regionDrawable => regionDrawable.Tint( tint ),
                                         NinePatchDrawable patchDrawable      => patchDrawable.Tint( tint ),
                                         SpriteDrawable spriteDrawable        => spriteDrawable.Tint( tint ),

                                         // ----------------

                                         var _ => throw new
                                             RuntimeException( $"Unable to copy, unknown drawable type: {drawable.GetType()}" )
                                     };

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
    /// Scales the drawable's :-
    /// <see cref="ISceneDrawable.LeftWidth"/>,
    /// <see cref="ISceneDrawable.RightWidth"/>,
    /// <see cref="ISceneDrawable.BottomHeight"/>,
    /// <see cref="ISceneDrawable.TopHeight"/>,
    /// <see cref="ISceneDrawable.MinWidth"/>,
    /// <see cref="ISceneDrawable.MinHeight"/>.
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

    /// <summary>
    /// Disposes the <see cref="TextureAtlas"/> and all <see cref="IDisposable"/>
    /// resources in the skin.
    /// </summary>
    public void Dispose()
    {
        Atlas?.Dispose();

        foreach ( Dictionary< string, object >? entry in Resources.Values )
        {
            foreach ( object resource in entry!.Values )
            {
                if ( resource is IDisposable disposable )
                {
                    disposable.Dispose();
                }
            }
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="skinFile"></param>
    /// <returns></returns>
    protected Json GetJsonLoader( FileInfo skinFile )
    {
        var json = new SkinJson( this )
        {
            TypeName      = null,
            UsePrototypes = false
        };

        // Register the custom serializers
        json.SetSerializer< Color >( new ColorSerializer( this ) );
        json.SetSerializer< BitmapFont >( new BitmapFontSerializer( this, skinFile ) );
        json.SetSerializer< TintedDrawable >( new TintedDrawableSerializer( this ) );

        foreach ( KeyValuePair< string, Type > entry in JsonClassTags )
        {
            json.AddClassTag( entry.Key, entry.Value );
        }

        return json;
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    protected class SkinJson : Json
    {
        private const    string PARENT_FIELD_NAME = "parent";
        private readonly Skin   _skin;

        // ====================================================================

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skin"></param>
        public SkinJson( Skin skin )
        {
            _skin = skin;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="elementType"></param>
        /// <param name="jsonValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override T ReadValue< T >( Type? type, Type? elementType, JsonValue? jsonValue )
        {
            if ( jsonValue != null
              && jsonValue.IsString()
              && !typeof( IEnumerable ).IsAssignableFrom( typeof( T ) ) )
            {
                return _skin.Get< T >( jsonValue.AsString() );
            }

            return base.ReadValue< T >( type, elementType, jsonValue )
                ?? throw new RuntimeException( $"Unable to read value: {jsonValue}" );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="jsonMap"></param>
        /// <exception cref="SerializationException"></exception>
        public override void ReadFields( object obj, JsonValue jsonMap )
        {
            if ( jsonMap.Has( PARENT_FIELD_NAME ) )
            {
                var   parentName = ReadValue< string >( PARENT_FIELD_NAME, typeof( string ), jsonMap );
                Type? parentType = obj.GetType();

                while ( true )
                {
                    try
                    {
                        CopyFields( _skin.Get( parentName, parentType ), obj );

                        break;
                    }
                    catch ( RuntimeException )
                    {
                        // Parent resource doesn't exist.
                        parentType = parentType?.BaseType; // Try resource for base class.

                        if ( parentType == typeof( object ) )
                        {
                            var se = new SerializationException( $"Unable to find parent resource: {parentName}" );

                            if ( jsonMap.Child != null )
                            {
                                se.AddTrace( jsonMap.Child.Trace() );
                            }

                            throw se;
                        }
                    }
                }
            }

            base.ReadFields( obj, jsonMap );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public override bool IgnoreUnknownField( Type type, string fieldName )
        {
            return fieldName == PARENT_FIELD_NAME;
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    public class ColorSerializer : Json.ReadOnlySerializer< Color >
    {
        private readonly Skin _skin;

        public ColorSerializer( Skin skin )
        {
            _skin = skin;
        }

        // ====================================================================

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <param name="jsonData"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public override Color Read( Json json, JsonValue jsonData, Type type )
        {
            if ( jsonData.IsString() )
            {
                return _skin.Get< Color >( jsonData.AsString() );
            }

            var hex = json.ReadValue< string >( "hex", null, jsonData );

            if ( hex != null )
            {
                return Color.FromHexString( hex );
            }

            var r = json.ReadValue< float >( "r", typeof( float ), 0f, jsonData );
            var g = json.ReadValue< float >( "g", typeof( float ), 0f, jsonData );
            var b = json.ReadValue< float >( "b", typeof( float ), 0f, jsonData );
            var a = json.ReadValue< float >( "a", typeof( float ), 1f, jsonData );

            return new Color( r, g, b, a );
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    public class BitmapFontSerializer : Json.ReadOnlySerializer< BitmapFont >
    {
        private readonly Skin     _skin;
        private readonly FileInfo _skinFile;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skin"></param>
        /// <param name="skinFile"></param>
        public BitmapFontSerializer( Skin skin, FileInfo skinFile )
        {
            _skin     = skin;
            _skinFile = skinFile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <param name="jsonData"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="SerializationException"></exception>
        public override BitmapFont Read( Json json, JsonValue jsonData, Type type )
        {
            var path                = json.ReadValue< string >( "file", typeof( string ), jsonData );
            var scaledSize          = json.ReadValue< float >( "scaledSize", typeof( float ), -1f, jsonData );
            var flip                = json.ReadValue< bool >( "flip", typeof( bool ), false, jsonData );
            var markupEnabled       = json.ReadValue< bool >( "markupEnabled", typeof( bool ), false, jsonData );
            var useIntegerPositions = json.ReadValue< bool >( "useIntegerPositions", typeof( bool ), true, jsonData );

            Guard.Against.Null( path );

            string parentPath = _skinFile.DirectoryName ?? "\\.";
            var    fontFile   = new FileInfo( Path.Combine( parentPath, path ) );

            if ( !File.Exists( fontFile.FullName ) )
            {
                fontFile = Engine.Api.Files.Internal( path );
            }

            if ( !File.Exists( fontFile.FullName ) )
            {
                throw new SerializationException( $"Font file not found: {fontFile}" );
            }

            string regionName = Path.GetFileNameWithoutExtension( fontFile.Name );

            try
            {
                BitmapFont             font;
                List< TextureRegion >? regions = _skin.GetRegions( regionName );

                if ( regions is { Count: > 0 } )
                {
                    font = new BitmapFont( new BitmapFontData( fontFile, flip ), regions );
                }
                else
                {
                    var region = _skin.Optional< TextureRegion >( regionName );

                    if ( region != null )
                    {
                        font = new BitmapFont( fontFile, region, flip );
                    }
                    else
                    {
                        string fontFileParent = fontFile.DirectoryName ?? "\\.";
                        var    imageFile      = new FileInfo( Path.Combine( fontFileParent, $"{regionName}.png" ) );

                        font = File.Exists( imageFile.FullName )
                            ? new BitmapFont( fontFile, imageFile, flip )
                            : new BitmapFont( fontFile, flip );
                    }
                }

                font.FontData.MarkupEnabled = markupEnabled;
                font.UseIntegerPositions    = useIntegerPositions;

                if ( Math.Abs( scaledSize - -1 ) > NumberUtils.FLOAT_TOLERANCE )
                {
                    font.FontData.SetScale( scaledSize / font.GetCapHeight() );
                }

                return font;
            }
            catch ( Exception ex )
            {
                throw new SerializationException( $"Error loading bitmap font: {fontFile}", ex );
            }
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    public class TintedDrawableSerializer : Json.ReadOnlySerializer< TintedDrawable >

    {
        private readonly Skin _skin;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skin"></param>
        public TintedDrawableSerializer( Skin skin )
        {
            _skin = skin;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <param name="jsonData"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="SerializationException"></exception>
        public override object Read( Json json, JsonValue jsonData, Type type )
        {
            var name  = json.ReadValue< string >( "name", typeof( string ), jsonData );
            var color = json.ReadValue< Color >( "color", typeof( Color ), jsonData );

            if ( color == null )
            {
                throw new SerializationException( $"TintedDrawable missing color: {jsonData}" );
            }

            ISceneDrawable drawable = _skin.NewDrawable( name, color );

            if ( drawable is BaseDrawable baseDrawable )
            {
                baseDrawable.Name = $"{jsonData.Name} ({name}, {color})";
            }

            return drawable;
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    public class SkinSerializer : Json.ReadOnlySerializer< Skin >
    {
        private readonly Skin _skin;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skin"></param>
        public SkinSerializer( Skin skin )
        {
            _skin = skin;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <param name="typeToValueMap"></param>
        /// <param name="ignored"></param>
        /// <returns></returns>
        /// <exception cref="SerializationException"></exception>
        public override Skin Read( Json json, JsonValue typeToValueMap, Type ignored )
        {
            for ( JsonValue? valueMap = typeToValueMap.Child; valueMap != null; valueMap = valueMap.Next )
            {
                try
                {
                    if ( string.IsNullOrEmpty( valueMap.Name ) )
                    {
                        throw new SerializationException( "Skin missing name." );
                    }

                    // 1. Try to get type from Json's internal tag map
                    // 2. If not found, try to resolve the type string directly
                    Type? type = json.GetTagType( valueMap.Name ) ?? Type.GetType( valueMap.Name );

                    if ( type == null )
                    {
                        throw new SerializationException( $"Unknown type: {valueMap.Name}" );
                    }

                    ReadNamedObjects( json, type, valueMap );
                }
                catch ( Exception ex )
                {
                    throw new SerializationException( ex );
                }
            }

            return _skin;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <param name="valueMap"></param>
        /// <exception cref="SerializationException"></exception>
        private void ReadNamedObjects( Json json, Type type, JsonValue valueMap )
        {
            Type addType = ( type == typeof( TintedDrawable ) ) ? typeof( IDrawable ) : type;

            for ( JsonValue? valueEntry = valueMap.Child; valueEntry != null; valueEntry = valueEntry.Next )
            {
                object? obj = json.ReadValue( type, valueEntry );

                if ( obj == null )
                {
                    continue;
                }

                try
                {
                    // Add the resource to the skin
                    _skin.Add( valueEntry.Name, obj, addType );

                    // If it's a Drawable (but not explicitly added as one yet), add it to the Drawable bucket too
                    if ( addType != typeof( IDrawable ) && typeof( IDrawable ).IsAssignableFrom( addType ) )
                    {
                        _skin.Add( valueEntry.Name, obj, typeof( IDrawable ) );
                    }
                }
                catch ( Exception ex )
                {
                    throw new SerializationException( $"Error reading {type.Name}: {valueEntry.Name}", ex );
                }
            }
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class ColorConverter : JsonConverter< Color >
    {
        private readonly Skin _skin;

        // ====================================================================

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skin"></param>
        public ColorConverter( Skin skin )
        {
            _skin = skin;
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">
        /// The existing value of object being read. If there is no existing value then <c>null</c> will be used.
        /// </param>
        /// <param name="hasExistingValue">The existing value has a value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override Color ReadJson( JsonReader reader,
                                        Type objectType,
                                        Color? existingValue,
                                        bool hasExistingValue,
                                        JsonSerializer serializer )
        {
            JToken token = JToken.Load( reader );

            // Equivalent to: if (jsonData.isString()) return get(jsonData.asString(), Color.class);
            if ( token.Type == JTokenType.String )
            {
                return _skin.Get< Color >( token.ToString() );
            }

            // Handle hex: { "hex": "ff00ff" }
            if ( token[ "hex" ] != null )
            {
                return Color.FromHexString( token[ "hex" ]!.ToString() );
            }

            // Handle RGBA: { "r": 1, "g": 0, "b": 0, "a": 1 }
            float r = token[ "r" ]?.Value< float >() ?? 0;
            float g = token[ "g" ]?.Value< float >() ?? 0;
            float b = token[ "b" ]?.Value< float >() ?? 0;
            float a = token[ "a" ]?.Value< float >() ?? 1;

            return new Color( r, g, b, a );
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson( JsonWriter writer, Color? value, JsonSerializer serializer )
        {
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class SkinConverter : JsonConverter< Skin >
    {
        private const    string PARENT_FIELD_NAME = "parent";
        private readonly Skin   _skin;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skin"></param>
        /// <param name="file"></param>
        public SkinConverter( Skin skin, FileInfo file )
        {
            _skin = skin;
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">
        /// The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.
        /// </param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">
        /// The existing value of object being read. If there is no existing value
        /// then <c>null</c> will be used.
        /// </param>
        /// <param name="hasExistingValue">The value has an existing value.</param>
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
                string className = typeProperty.Name;
                // Get the actual C# Type from the JSON key (e.g., "Color")
                Type targetType = ResolveType( className );

                if ( typeProperty.Value is JObject resources )
                {
                    foreach ( JProperty resourceProperty in resources.Properties() )
                    {
                        string resourceName  = resourceProperty.Name;
                        JToken resourceToken = resourceProperty.Value;

                        if ( resourceToken is JObject resourceObj
                          && resourceObj[ PARENT_FIELD_NAME ] != null )
                        {
                            var parentName = resourceObj[ PARENT_FIELD_NAME ]?.ToString();

                            if ( string.IsNullOrWhiteSpace( parentName ) )
                            {
                                throw new JsonSerializationException
                                    ( $"Resource '{resourceName}' has an empty parent field." );
                            }

                            // Try to find the parent instance
                            object? parentInstance = _skin.Get( parentName, targetType );

                            if ( parentInstance == null )
                            {
                                throw new JsonSerializationException
                                    ( $"Unable to find parent resource named '{parentName}' for '{resourceName}'" );
                            }

                            // Convert parent back to a JObject so we can merge
                            JObject parentJson = JObject.FromObject( parentInstance, serializer );

                            // Merge: Current properties overwrite parent properties
                            parentJson.Merge( resourceObj,
                                              new JsonMergeSettings
                                              {
                                                  MergeArrayHandling = MergeArrayHandling.Union
                                              } );

                            resourceToken = parentJson;
                        }

                        // Deserialize the (potentially merged) object and add to skin
                        var finalObject = resourceToken.ToObject( targetType, serializer );
                        _skin.Add( resourceName, finalObject, targetType );
                    }
                }
            }

            return _skin;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private Type ResolveType( string name )
        {
            foreach ( Tag tag in DefaultTagClasses )
            {
                if ( name.Contains( tag.Name ) )
                {
                    return tag.Type;
                }
            }

            return typeof( object );
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