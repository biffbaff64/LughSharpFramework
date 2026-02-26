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

using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Atlases;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.Scenes.Scene2D.UI;
using LughSharp.Core.Scenes.Scene2D.Utils;

namespace LughSharp.Core.Scenes.Scene2D.Styles;

// W.I.P class intended, eventually, to be used for loading and saving styles.
// Ultimately this will replace the use of Skins.

/// <summary>
/// 
/// </summary>
[PublicAPI]
public class StyleRegistry
{
    private Dictionary< Type, Dictionary< string, object > > _data = new();

    // ========================================================================

    /// <summary>
    /// Registers a new section for styles of type <typeparamref name="T"/>.
    /// </summary>
    /// <returns> This StyleRegistry instance for chaining. </returns>
    public StyleRegistry RegisterSection< T >() where T : class
    {
        var type = typeof( T );

        if ( !_data.ContainsKey( type ) )
        {
            _data[ type ] = new Dictionary< string, object >();
        }

        return this;
    }

    /// <summary>
    /// Adds a style to the registry.
    /// The code:-
    /// <code>
    /// var style = new ButtonStyle()
    /// {
    ///     Up       = new TextureRegionDrawable( atlas.FindRegion( "default-round" ) ),
    ///     Down     = new TextureRegionDrawable( atlas.FindRegion( "default-round-down" ) ),
    ///     Disabled = new TextureRegionDrawable( atlas.FindRegion( "default-round" ) )
    /// };
    ///
    /// registry.Add( "default", style );
    /// </code>
    /// is equivalent to the 'default' element in the Json:<br/>
    /// "ButtonStyle": {<br/>
    ///     "default": {<br/>
    ///         "down": "default-round-down",<br/>
    ///         "up": "default-round",<br/>
    ///         "disabled": "default-round"<br/>
    ///     },<br/>
    ///     "toggle": {<br/>
    ///         "parent": "default",<br/>
    ///         "checked": "default-round-down"<br/>
    ///     }<br/>
    /// },<br/>
    /// </summary>
    /// <param name="name"> The style name. </param>
    /// <param name="style"> The style properties. </param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public StyleRegistry Add< T >( string name, T style ) where T : class
    {
        RegisterSection< T >();

        _data[ typeof( T ) ][ name ] = style;

        return this;
    }

    public T Get< T >( string name ) where T : class
    {
        if ( _data.TryGetValue( typeof( T ), out var map ) && map.TryGetValue( name, out var style ) )
        {
            return ( T )style;
        }

        // Fallback or detailed error
        throw new Exception( $"Missing {typeof( T ).Name}: {name}" );
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
        var btnStyle = new ButtonStyle()
        {
            Up       = btnUp,
            Down     = btnDown,
            Disabled = btnDisabled
        };

        this.Add( "default", btnStyle ).Add( "toggle", btnStyle with { Checked = btnStyle.Down } );

        // ----- TextButtonStyle -----
        var tbStyle = new TextButton.TextButtonStyle( btnUp, btnDown, btnChecked, defaultFont );

        this.Add( "default", tbStyle );

        // ----- ScrollPaneStyle -----
        var spStyle = new ScrollPane.ScrollPaneStyle()
        {
            Background  = new TextureRegionDrawable( atlas.FindRegion( "default-rect" ) ),
            Corner      = new TextureRegionDrawable( atlas.FindRegion( "default-rect-corner" ) ),
            VScroll     = new TextureRegionDrawable( atlas.FindRegion( "default-scroll" ) ),
            HScroll     = new TextureRegionDrawable( atlas.FindRegion( "default-scroll" ) ),
            VScrollKnob = new TextureRegionDrawable( atlas.FindRegion( "default-round-large" ) ),
            HScrollKnob = new TextureRegionDrawable( atlas.FindRegion( "default-round-large" ) )
        };

        this.Add( "default", spStyle );

        // ----- SplitPaneStyle -----
        var horizHandle = new TextureRegionDrawable( atlas.FindRegion( "default-splitpane" ) );
        var vertHandle  = new TextureRegionDrawable( atlas.FindRegion( "default-splitpane-vertical" ) );

        this.Add( "default-vertical", new SplitPane.SplitPaneStyle { Handle   = vertHandle } )
            .Add( "default-horizontal", new SplitPane.SplitPaneStyle { Handle = horizHandle } );

        // ----- WindowStyle -----
        var winStyle = new Window.WindowStyle()
        {
            Background      = new TextureRegionDrawable( atlas.FindRegion( "default-window" ) ),
            TitleFont       = defaultFont,
            TitleFontColor  = Color.White,
            StageBackground = new TextureRegionDrawable( atlas.FindRegion( "dialogDim" ) )
        };

        this.Add( "default", winStyle ).Add( "dialog", winStyle );

        // ----- ProgressBarStyle -----
        this.Add( "default-horizontal",
                  new ProgressBar.ProgressBarStyle()
                  {
                      Background = new TextureRegionDrawable( atlas.FindRegion( "default-slider" ) ),
                      Knob       = new TextureRegionDrawable( atlas.FindRegion( "default-slider-knob" ) )
                  } )
            .Add( "default-vertical",
                  new ProgressBar.ProgressBarStyle()
                  {
                      Background = new TextureRegionDrawable( atlas.FindRegion( "default-slider" ) ),
                      Knob       = new TextureRegionDrawable( atlas.FindRegion( "default-round-large" ) )
                  } );

        // ----- SliderStyle -----
        var sliderStyle = new Slider.SliderStyle();

        this.Add( "default-horizontal", sliderStyle ).Add( "default-vertical", sliderStyle );
        
        // ----- LabelStyle -----
        var labelStyle = new Label.LabelStyle()
        {
            Font      = defaultFont,
            FontColor = Color.White
        };

        this.Add( "default", labelStyle );
        
        // ----- TextFieldStyle -----
        var tfStyle = new TextField.TextFieldStyle()
        {
            Selection = new TextureRegionDrawable( atlas.FindRegion( "selection" ) ),
            Background = new TextureRegionDrawable( atlas.FindRegion( "textfield" ) ),
            Font       = defaultFont,
            FontColor  = Color.White,
            Cursor = new TextureRegionDrawable( atlas.FindRegion( "cursor" ) )
        };
        
        this.Add( "default", tfStyle );

        // ----- CheckBoxStyle -----
        var cbStyle = new CheckBox.CheckBoxStyle()
        {
            CheckboxOn = new TextureRegionDrawable( atlas.FindRegion( "check-on" ) ),
            CheckboxOff = new TextureRegionDrawable( atlas.FindRegion( "check-off" ) ),
            Font = defaultFont,
            FontColor = Color.White
        };

        this.Add( "default", cbStyle );
        
        // ----- ListStyle -----
//        var listStyle = new ListBox<>.ListBoxStyle();

        // ----- TouchpadStyle -----
        var tpStyle = new Touchpad.TouchpadStyle()
        {
            Background = new TextureRegionDrawable( atlas.FindRegion( "default-pane" ) ),
            Knob       = new TextureRegionDrawable( atlas.FindRegion( "default-round-large" ) )
        };
        
        this.Add( "default", tpStyle );

        // ----- TreeStyle -----
//        var treeStyle = new Tree< , >.TreeStyle();

        // ----- TextTooltipStyle -----
        var ttStyle = new TextTooltip.TextTooltipStyle();

        // ----- ImageTextButtonStyle -----
        var itbStyle = new ImageTextButton.ImageTextButtonStyle();

        // ----- ImageButtonStyle -----
        var ibStyle = new ImageButton.ImageButtonStyle();

        // ----- SelectBoxStyle -----
//        var sbStyle = new SelectBox<>.SelectBoxStyle();
    }

    public void LoadFromJson( FileInfo jsonFile )
    {
    }

    public void SaveToJson( FileInfo jsonFile )
    {
    }
}

// ============================================================================
// ============================================================================