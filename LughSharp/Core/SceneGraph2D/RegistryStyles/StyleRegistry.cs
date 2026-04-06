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

using System.Text.Json;

using JetBrains.Annotations;

using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Atlases;
using LughSharp.Core.Graphics.BitmapFonts;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.SceneGraph2D.UI;
using LughSharp.Core.SceneGraph2D.UI.Styles;
using LughSharp.Core.SceneGraph2D.Utils;
using LughSharp.Core.Utils.Logging;

namespace LughSharp.Core.SceneGraph2D.RegistryStyles;

/// <summary>
/// 
/// </summary>
[PublicAPI]
public class StyleRegistry
{
    private Dictionary< string, Dictionary< string, object > > _data = [ ];

    // ========================================================================

    /// <summary>
    /// Adds a style to the registry. If the section for type <typeparamref name="T"/> 
    /// does not exist, it is created automatically.
    /// </summary>
    public StyleRegistry Add< T >( string name, T style ) where T : class
    {
        string typeKey = typeof( T ).Name;

        // Ensure the dictionary for this type exists
        if ( !_data.TryGetValue( typeKey, out Dictionary< string, object >? section ) )
        {
            section          = new Dictionary< string, object >();
            _data[ typeKey ] = section;
        }

        section[ name ] = style;

        return this;
    }

    /// <summary>
    /// Gets a named style from the registry.
    /// </summary>
    /// <param name="name"> The name of the style to retrieve (e.g., "default", "toggle").</param>
    /// <typeparam name="T"> The type of style to retrieve (e.g., ButtonStyle).</typeparam>
    public T Get< T >( string name ) where T : class
    {
        string typeKey = typeof( T ).Name;

        // Check if the Type is registered at all
        if ( !_data.TryGetValue( typeKey, out Dictionary< string, object >? section ) )
        {
            // This error will now tell you EXACTLY what string key it was looking for
            throw new Exception( $"StyleRegistry: No styles registered for type key '{typeKey}'." );
        }

        // Check if the specific name exists in that Type's dictionary
        if ( section.TryGetValue( name, out object? style ) )
        {
            return ( T )style;
        }

        // Specific error for missing name
        throw new Exception( $"StyleRegistry: Section '{typeKey}' exists, but style '{name}' is missing." );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Dictionary< string, Dictionary< string, object > > GetRegistry()
    {
        return _data;
    }

    /// <summary>
    /// Populate the registry with default styles.
    /// </summary>
    /// <param name="atlas"> The <see cref="TextureAtlas"/> containing default UI elements.</param>
    public void CreateStyleDefaults( TextureAtlas atlas )
    {
        var btnUp       = new TextureRegionDrawable( atlas.FindRegion( "default-round" ) );
        var btnDown     = new TextureRegionDrawable( atlas.FindRegion( "default-round-down" ) );
        var btnDisabled = new TextureRegionDrawable( atlas.FindRegion( "default-round" ) );
        var btnChecked  = new TextureRegionDrawable( atlas.FindRegion( "default-round-down" ) );
        var defaultFont = new BitmapFont();

        // ----- ButtonStyle -----
        var btnStyleDefault = new ButtonStyle
        {
            Up       = btnUp,
            Down     = btnDown,
            Disabled = btnDisabled
        };

        var btnStyleToggle = new ButtonStyle
        {
            Up       = btnUp,
            Down     = btnDown,
            Disabled = btnDisabled,
            Checked  = btnStyleDefault.Down
        };

        Add( "default", btnStyleDefault ).Add( "toggle", btnStyleToggle );

        // ----- TextButtonStyle -----
        var tbStyle = new TextButtonStyle( btnUp, btnDown, btnChecked, defaultFont );

        Add( "default", tbStyle );

        // ----- ScrollPaneStyle -----
        var spStyle = new ScrollPaneStyle
        {
            Background  = new TextureRegionDrawable( atlas.FindRegion( "default-rect" ) ),
            Corner      = new TextureRegionDrawable( atlas.FindRegion( "default-rect-corner" ) ),
            VScroll     = new TextureRegionDrawable( atlas.FindRegion( "default-scroll" ) ),
            HScroll     = new TextureRegionDrawable( atlas.FindRegion( "default-scroll" ) ),
            VScrollKnob = new TextureRegionDrawable( atlas.FindRegion( "default-round-large" ) ),
            HScrollKnob = new TextureRegionDrawable( atlas.FindRegion( "default-round-large" ) )
        };

        Add( "default", spStyle );

        // ----- SplitPaneStyle -----
        var horizHandle = new TextureRegionDrawable( atlas.FindRegion( "default-splitpane" ) );
        var vertHandle  = new TextureRegionDrawable( atlas.FindRegion( "default-splitpane-vertical" ) );

        Add( "default-vertical", new SplitPaneStyle { Handle        = vertHandle } )
            .Add( "default-horizontal", new SplitPaneStyle { Handle = horizHandle } );

        // ----- WindowStyle -----
        var winStyle = new WindowStyle
        {
            Background      = new TextureRegionDrawable( atlas.FindRegion( "default-window" ) ),
            TitleFont       = defaultFont,
            TitleFontColor  = Color.White,
            StageBackground = new TextureRegionDrawable( atlas.FindRegion( "dialogDim" ) )
        };

        Add( "default", winStyle ).Add( "dialog", winStyle );

        // ----- ProgressBarStyle -----
        Add( "default-horizontal",
             new ProgressBarStyle
             {
                 Background = new TextureRegionDrawable( atlas.FindRegion( "default-slider" ) ),
                 Knob       = new TextureRegionDrawable( atlas.FindRegion( "default-slider-knob" ) )
             } )
            .Add( "default-vertical",
                  new ProgressBarStyle
                  {
                      Background = new TextureRegionDrawable( atlas.FindRegion( "default-slider" ) ),
                      Knob       = new TextureRegionDrawable( atlas.FindRegion( "default-round-large" ) )
                  } );

        // ----- SliderStyle -----
        var sliderStyle = new SliderStyle();

        Add( "default-horizontal", sliderStyle ).Add( "default-vertical", sliderStyle );

        // ----- LabelStyle -----
        var labelStyle = new LabelStyle
        {
            Font      = defaultFont,
            FontColor = Color.White
        };

        Add( "default", labelStyle );

        // ----- TextFieldStyle -----
        var tfStyle = new TextFieldStyle
        {
            Selection  = new TextureRegionDrawable( atlas.FindRegion( "selection" ) ),
            Background = new TextureRegionDrawable( atlas.FindRegion( "textfield" ) ),
            Font       = defaultFont,
            FontColor  = Color.White,
            Cursor     = new TextureRegionDrawable( atlas.FindRegion( "cursor" ) )
        };

        Add( "default", tfStyle );

        // ----- CheckBoxStyle -----
        var cbStyle = new CheckBoxStyle
        {
            CheckboxOn  = new TextureRegionDrawable( atlas.FindRegion( "check-on" ) ),
            CheckboxOff = new TextureRegionDrawable( atlas.FindRegion( "check-off" ) ),
            Font        = defaultFont,
            FontColor   = Color.White
        };

        Add( "default", cbStyle );

        // ----- ListStyle -----
//        var listStyle = new ListBoxStyle();

        // ----- TouchpadStyle -----
        var tpStyle = new TouchpadStyle
        {
            Background = new TextureRegionDrawable( atlas.FindRegion( "default-pane" ) ),
            Knob       = new TextureRegionDrawable( atlas.FindRegion( "default-round-large" ) )
        };

        Add( "default", tpStyle );

        // ----- TreeStyle -----
//        var treeStyle = new TreeStyle();

        // ----- TextTooltipStyle -----
//        var ttStyle = new TextTooltipStyle();

        // ----- ImageButtonStyle -----
        var ibStyle = new ImageButtonStyle
        {
            ImageUp       = new TextureRegionDrawable( atlas.FindRegion( "default-round" ) ),
            ImageDown     = new TextureRegionDrawable( atlas.FindRegion( "default-round-down" ) ),
            ImageDisabled = new TextureRegionDrawable( atlas.FindRegion( "default-round" ) ),
        };

//        Add( "default", ibStyle ).Add( "toggle", ibStyle with { ImageChecked = ibStyle.ImageDown } );

        Add( "default", ibStyle );
        ibStyle.ImageChecked = ibStyle.ImageDown;
        Add( "toggle", ibStyle );

        // ----- ImageTextButtonStyle -----
        var itbStyle = new ImageTextButtonStyle
        {
            ImageUp       = new TextureRegionDrawable( atlas.FindRegion( "default-round" ) ),
            ImageDown     = new TextureRegionDrawable( atlas.FindRegion( "default-round-down" ) ),
            ImageDisabled = new TextureRegionDrawable( atlas.FindRegion( "default-round" ) ),
        };

//        Add( "default", itbStyle ).Add( "toggle", itbStyle with { ImageChecked = itbStyle.ImageDown } );

        Add( "default", itbStyle );
        itbStyle.ImageChecked = itbStyle.ImageDown;
        Add( "toggle", itbStyle );

        // ----- SelectBoxStyle -----
//        var sbStyle = new SelectBoxStyle();
    }

    /// <summary>
    /// Loads styles from a JSON file.
    /// </summary>
    /// <param name="jsonFile"></param>
    /// <param name="atlas"></param>
    /// <exception cref="FileNotFoundException"></exception>
    public void LoadFromJson( FileInfo jsonFile, TextureAtlas atlas )
    {
        if ( !jsonFile.Exists )
        {
            throw new FileNotFoundException( jsonFile.FullName );
        }

        string jsonString = File.ReadAllText( jsonFile.FullName );

        using JsonDocument document = JsonDocument.Parse( jsonString );

        JsonElement root = document.RootElement;

        // Load Colors first so they are available for fonts and styles
        if ( root.TryGetProperty( "Color", out JsonElement colors ) )
        {
            foreach ( JsonProperty col in colors.EnumerateObject() )
            {
                // Assuming a helper to parse JSON to Color object
                Add( col.Name, Color.ParseColor( col.Value.GetString()!, this ) );
            }
        }

        // Load Fonts next
        if ( root.TryGetProperty( "BitmapFont", out JsonElement fonts ) )
        {
            foreach ( JsonProperty font in fonts.EnumerateObject() )
            {
                // Typically fonts need a .fnt file path from the JSON
                string? fontPath = font.Value.GetProperty( "file" ).GetString();
                Add( font.Name, new BitmapFont( new FileInfo( fontPath! ) ) );
            }
        }

        // 3. Finally, load all other Styles (LabelStyle, ButtonStyle, etc.)
        // Use the StyleFactory logic here...
        foreach ( JsonProperty property in document.RootElement.EnumerateObject() )
        {
            string      styleTypeName = property.Name;  // e.g., "ButtonStyle"
            JsonElement styles        = property.Value; // The object containing "default", "toggle", etc.

            foreach ( JsonProperty styleEntry in styles.EnumerateObject() )
            {
                string styleName = styleEntry.Name;

                // Delegate creation to the Factory
                object? style = StyleFactory.CreateStyle( styleTypeName, styleEntry.Value, atlas, this );

                if ( style != null )
                {
                    // Internal method to add by type name string
                    AddByTypeName( styleTypeName, styleName, style );
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="styleName"></param>
    /// <param name="style"></param>
    private void AddByTypeName( string typeName, string styleName, object style )
    {
        string typeKey = style.GetType().FullName ?? style.GetType().Name;

        if ( !_data.TryGetValue( typeKey, out Dictionary< string, object >? section ) )
        {
            section          = new Dictionary< string, object >();
            _data[ typeKey ] = section;
        }

        section[ styleName ] = style;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="jsonFile"></param>
    public void SaveToJson( FileInfo jsonFile )
    {
    }

    /// <summary>
    /// Output debug information about the registry, specifically the sections and
    /// members within each section.
    /// </summary>
    public void DebugRegistry()
    {
        foreach ( KeyValuePair< string, Dictionary< string, object > > entry in _data )
        {
            Logger.Debug( $"Registered Key: {entry.Key} | Count: {entry.Value.Count}" );

            foreach ( KeyValuePair< string, object > style in entry.Value )
            {
                Logger.Debug( $"    - {style.Key}" );
            }
        }
    }
}

// ============================================================================
// ============================================================================