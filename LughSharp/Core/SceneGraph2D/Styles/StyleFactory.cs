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

using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;

using JetBrains.Annotations;

using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Atlases;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.SceneGraph2D.UI;
using LughSharp.Core.SceneGraph2D.Utils;
using LughSharp.Core.Utils;

namespace LughSharp.Core.SceneGraph2D.Styles;

[PublicAPI]
public class StyleFactory
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="json"></param>
    /// <param name="atlas"></param>
    /// <param name="registry"></param>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <param name="registry"></param>
    /// <param name="target"></param>
    /// <param name="prop"></param>
    /// <param name="value"></param>
    /// <param name="atlas"></param>
    private static void ApplyPropertyValue( StyleRegistry registry, object target, PropertyInfo prop, JsonElement value, TextureAtlas atlas )
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

    private static object? CloneObject( object? source )
    {
        // Simple shallow clone for UI Styles (usually enough for Drawables/Colors)
        return source?.GetType().GetMethod( "MemberwiseClone",
                                            BindingFlags.Instance | BindingFlags.NonPublic )?.Invoke( source, null );
    }
}

// ============================================================================
// ============================================================================