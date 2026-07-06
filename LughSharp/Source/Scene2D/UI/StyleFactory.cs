// ///////////////////////////////////////////////////////////////////////////////
// MIT License
// 
// Copyright (c) 2024, 2025, 2026 Circa64 Software Projects / Richard Ikin.
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

using System.Text.Json;

using LughSharp.Source.Graphics.Atlases;
using LughSharp.Source.Graphics.Fonts;
using LughSharp.Source.Scene2D.Utils;

namespace LughSharp.Source.Scene2D.UI;

[PublicAPI]
[UnstableApi( "Style system is still in active development and the API may change." )]
[Experimental( "LUGH_UI_001" )]
public class StyleFactory
{
    /// <summary>
    /// Creates a style object based on the given type name and JSON description.
    /// This method allows for the dynamic creation of style objects, applying inherited
    /// properties when a parent style is specified and updating properties based on the
    /// provided JSON metadata.
    /// </summary>
    /// <param name="typeName">
    /// The name of the type to create. This should match the type name of a class representing
    /// the style.
    /// </param>
    /// <param name="json">
    /// A JSON object containing the style properties and metadata. The JSON can also include a
    /// "parent" property to enable inheritance of properties from a parent style.
    /// </param>
    /// <param name="atlas">
    /// A <see cref="TextureAtlas"/> instance to assist in resolving relevant graphics details
    /// required during style creation.
    /// </param>
    /// <param name="registry">
    /// A <see cref="StyleRegistry"/> instance used to fetch parent styles and resolve dependencies
    /// during style creation.
    /// </param>
    /// <returns>
    /// Returns the instantiated and initialized style object if the type and properties were
    /// resolved successfully; otherwise, returns null if the specified type is not found.
    /// </returns>
    public static object? CreateStyle( string typeName, JsonElement json, TextureAtlas atlas, StyleRegistry registry )
    {
        Type? type = AppDomain.CurrentDomain.GetAssemblies()
                              .SelectMany( a => a.GetTypes() )
                              .FirstOrDefault( t => t.Name == typeName );

        if ( type == null ) return null;

        object style;

        // --- Handle Inheritance ---
        if ( json.TryGetProperty( "parent", out JsonElement parentNameProp ) )
        {
            string? parentName = parentNameProp.GetString();

            // Use reflection to call the generic Get<T> method on the registry
            MethodInfo? method      = registry.GetType().GetMethod( "Get" )?.MakeGenericMethod( type );
            object?     parentStyle = method?.Invoke( registry, new object[] { parentName! } );

            // Clone the parent style (requires styles to be records or have a clone mechanism)
            // For standard classes, we can use a simple memberwise clone or re-instantiate
            style = CloneObject( parentStyle ) ?? Activator.CreateInstance( type )!;
        }
        else
        {
            style = Activator.CreateInstance( type )!;
        }

        // --- Apply Properties ---
        foreach ( JsonProperty prop in json.EnumerateObject() )
        {
            if ( prop.NameEquals( "parent" ) ) continue; // Skip the metadata

            PropertyInfo? propertyInfo = type.GetProperty( prop.Name,
                                                           System.Reflection.BindingFlags.Public |
                                                           System.Reflection.BindingFlags.Instance |
                                                           System.Reflection.BindingFlags.IgnoreCase );

            if ( propertyInfo == null || !propertyInfo.CanWrite ) continue;

            ApplyPropertyValue( registry, style, propertyInfo, prop.Value, atlas );
        }

        return style;
    }

    /// <summary>
    /// Applies a property value to the specified target object by resolving and converting
    /// the input value to the appropriate type and assigning it to the provided property.
    /// This method supports various property types, including primitives, <see cref="Color"/>,
    /// <see cref="BitmapFont"/>, and drawable types, utilizing a registry or texture atlas
    /// as needed for resolution.
    /// </summary>
    /// <param name="registry">
    /// A <see cref="StyleRegistry"/> instance used for managing named resources, such as fonts,
    /// that are required to resolve and assign specific property values.
    /// </param>
    /// <param name="target">
    /// The object on which the property value is to be applied. This is the style object
    /// being configured.
    /// </param>
    /// <param name="prop">
    /// A <see cref="PropertyInfo"/> object representing the property of the target for which
    /// the value should be resolved and assigned.
    /// </param>
    /// <param name="value">
    /// A <see cref="JsonElement"/> representing the raw value to be parsed, converted, and
    /// assigned to the specified property. The actual type and content of the value depend
    /// on the property type.
    /// </param>
    /// <param name="atlas">
    /// A <see cref="TextureAtlas"/> instance used for resolving graphical resources, such as
    /// texture regions, that may be required to assign drawable property values.
    /// </param>
    private static void ApplyPropertyValue( StyleRegistry registry, object target, PropertyInfo prop, JsonElement value,
                                            TextureAtlas atlas )
    {
        // --- Handle Color ---
        if ( prop.PropertyType == typeof( Color ) )
        {
            if ( value.ValueKind == JsonValueKind.String )
            {
                string? colorName = value.GetString();

                // Try to find the named color in the registry first
                try
                {
                    var namedColor = registry.Get< Color >( colorName! );
                    prop.SetValue( target, namedColor );
                }
                catch
                {
                    // Fallback: If not in registry, try to parse as Hex (e.g., "#FF0000")
                    prop.SetValue( target, Color.ParseColor( colorName!, registry ) );
                }
            }
        }
        // --- Handle BitmapFont ---
        else if ( prop.PropertyType == typeof( BitmapFont ) )
        {
            string? fontName = value.GetString();
            
            // Fonts are almost always retrieved by name from the registry
            var font = registry.Get< BitmapFont >( fontName! );
            
            prop.SetValue( target, font );
        }
        // --- Handle IDrawable (Existing logic) ---
        else if ( typeof( IDrawable ).IsAssignableFrom( prop.PropertyType ) )
        {
            string?      regionName = value.GetString();
            AtlasRegion? region     = atlas.FindRegion( regionName! );
            
            if ( region != null )
            {
                prop.SetValue( target, new TextureRegionDrawable( region ) );
            }
        }

        // Handle ISceneDrawable (TextureRegionDrawable)
        if ( typeof( IDrawable ).IsAssignableFrom( prop.PropertyType ) )
        {
            AtlasRegion? region = atlas.FindRegion( value.GetString()! );

            if ( region != null )
            {
                prop.SetValue( target, new TextureRegionDrawable( region ) );
            }
        }
        // Handle BitmapFont
        else if ( prop.PropertyType == typeof( BitmapFont ) )
        {
            // Implementation depends on how fonts are stored (Registry or File)
        }
        // Handle Color
        else if ( prop.PropertyType == typeof( Color ) )
        {
            //TODO: Add hex string parsing logic here
        }
        // Primitives
        else
        {
            object? convertedValue = JsonSerializer.Deserialize( value.GetRawText(), prop.PropertyType );
            prop.SetValue( target, convertedValue );
        }
    }

    /// <summary>
    /// Creates a shallow copy of the specified object using its MemberwiseClone method.
    /// This method allows for duplicating an object instance, primarily for use with
    /// classes that do not have a built-in cloning mechanism.
    /// </summary>
    /// <param name="source">
    /// The object to be cloned. This can be any reference type that supports MemberwiseClone.
    /// If the source is null, the method will return null.
    /// </param>
    /// <returns>
    /// Returns a new object that is a shallow copy of the source object. If the source object
    /// is null or the MemberwiseClone method is not accessible, the method will return null.
    /// </returns>
    private static object? CloneObject( object? source )
    {
        return source?.GetType().GetMethod( "MemberwiseClone",
                                            BindingFlags.Instance | BindingFlags.NonPublic )?.Invoke( source, null );
    }
}

// ============================================================================
// ============================================================================