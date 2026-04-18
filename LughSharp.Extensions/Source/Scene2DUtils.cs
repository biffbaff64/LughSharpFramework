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

using LughSharp.Core;
using LughSharp.Core.Assets;
using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Atlases;
using LughSharp.Core.Graphics.Fonts;
using LughSharp.Core.Graphics.Images;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.Maths;
using LughSharp.Core.Scene2D;
using LughSharp.Core.Scene2D.UI;
using LughSharp.Core.Scene2D.UI.Styles;
using LughSharp.Core.Scene2D.Utils;
using LughSharp.Core.Utils;

namespace Extensions.Source;

/// <summary>
/// A collection of convenience methods for creating UI elements in a Scene2D context.
/// </summary>
[PublicAPI]
public class Scene2DUtils
{
    /// <summary>
    /// Updates the stage, if it is not null, and isDrawingStage is true.
    /// </summary>
    /// <param name="stage"> The stage. </param>
    /// <param name="isDrawingStage"> Indicates if the stage should be updated. </param>
    public static void Update( Stage? stage, bool isDrawingStage )
    {
        if ( stage != null && isDrawingStage )
        {
            stage.Act( Math.Min( Engine.DeltaTime, 1.0f / 60.0f ) );
            stage.Draw();
        }
    }
    
    /// <summary>
    /// Creates a <see cref="Table"/>, without adding it to the stage.
    /// </summary>
    /// <param name="pos"> X, Y Display coordinates. </param>
    /// <param name="width"> The table Width in pixels.</param>
    /// <param name="height"> The table Height in pixels.</param>
    /// <param name="skin"> The <see cref="Skin"/> to use.</param>
    /// <returns> Tne Table. </returns>
    public static Table CreateTable( Vector2 pos, int width, int height, Skin skin )
    {
        var table = new Table( skin );

        table.SetSize( width, height );
        table.SetPosition( pos.X, pos.Y );

        return table;
    }

    /// <summary>
    /// Convenience method for creating a <see cref="Scene2DImage"/>.
    /// </summary>
    /// <param name="imageName"> The name of the image to use. </param>
    /// <param name="atlasLoader"> The <see cref="TextureAtlas"/> loader to use. </param>
    /// <returns> The Image. </returns>
    public static Scene2DImage CreateScene2DImage( string imageName, TextureAtlas atlasLoader )
    {
        AtlasRegion? region   = atlasLoader.FindRegion( imageName );
        var          drawable = new TextureRegionDrawable( region );

        return new Scene2DImage( drawable );
    }

    /// <summary>
    /// Convenience method for creating a <see cref="LughSharp.Core.Utils.IDrawable"/>.
    /// </summary>
    /// <param name="imageName"> The name of the image to use. </param>
    /// <param name="atlasLoader"> The <see cref="TextureAtlas"/> loader to use. </param>
    /// <returns> The ISceneDrawable. </returns>
    public static ISceneDrawable CreateDrawable( string imageName, TextureAtlas atlasLoader )
    {
        AtlasRegion? region = atlasLoader.FindRegion( imageName );

        return new TextureRegionDrawable( region );
    }

    /// <summary>
    /// Creates a <see cref="ScrollPane"/>, without adding it to the stage.
    /// </summary>
    /// <param name="table"> The associated <see cref="Table"/>. </param>
    /// <param name="skin">  The <see cref="Skin"/> to use. </param>
    /// <param name="name">  The name of this pane. </param>
    /// <returns> The ScrollPane. </returns>
    public static ScrollPane CreateScrollPane( Table table, Skin skin, string name )
    {
        var scrollPane = new ScrollPane( table, skin );

        return scrollPane;
    }

    /// <summary>
    /// Makes a text <see cref="Label"/>, and adds it to the stage.
    /// </summary>
    /// <param name="labelText"> The label text to display. </param>
    /// <param name="pos"> X, Y Display coordinates. </param>
    /// <param name="size"></param>
    /// <param name="color"> The Tint. </param>
    /// <param name="fontName"> The <see cref="BitmapFont"/> to use. </param>
    /// <returns> The label. </returns>
    public static Label AddLabel( string labelText, Vector2 pos, int size, Color color, string fontName )
    {
        var label1Style = new LabelStyle
        {
            Font      = FontUtils.CreateFont( fontName, size, Color.White ),
            FontColor = color
        };

        var label = new Label( labelText, label1Style )
        {
            LabelAlign = Align.Center,
            Style      = label1Style
        };
        label.SetPosition( pos.X, pos.Y );

        return label;
    }

    /// <summary>
    /// Makes a text <see cref="Label"/>, amd adds it to the stage.
    /// </summary>
    /// <param name="labelText"> The label text to display. </param>
    /// <param name="pos"> X, Y Display coordinates. </param>
    /// <param name="color"> The Tint. </param>
    /// <param name="skin"> The <see cref="Skin"/> to use. </param>
    /// <param name="stage"> The <see cref="Stage"/>. </param>
    /// <returns> The label. </returns>
    public static Label AddLabel( string labelText, Vector2 pos, Color color, Skin skin, Stage stage )
    {
        Label label = MakeLabel( labelText, ( int )pos.X, ( int )pos.Y, color, skin );

        stage.AddActor( label );

        return label;
    }

    /// <summary>
    /// Makes a selectable <see cref="ImageButton"/> and adds it to the stage.
    /// </summary>
    /// <param name="upButton"> The <see cref="Scene2DImage"/> to display when NOT pressed. </param>
    /// <param name="downButton"> The <see cref="Scene2DImage"/> to display when pressed. </param>
    /// <param name="checkedButton"> The <see cref="Scene2DImage"/> to display when checked. </param>
    /// <param name="x"> X Display coordinate. </param>
    /// <param name="y"> Y Display coordinate. </param>
    /// <param name="stage"> The <see cref="Stage"/>. </param>
    /// <returns> The ImageButton. </returns>
    public static ImageButton AddButton( Scene2DImage upButton,
                                  Scene2DImage downButton,
                                  Scene2DImage checkedButton,
                                  int x,
                                  int y,
                                  Stage stage )
    {
        var imageButton = new ImageButton( upButton.Drawable, downButton.Drawable, checkedButton.Drawable );

        imageButton.SetPosition( x, y );
        imageButton.IsVisible = true;
        imageButton.SetZIndex( 1 );

        stage.AddActor( imageButton );

        return imageButton;
    }

    /// <summary>
    /// Makes a selectable <see cref="TextButton"/> and adds it to the stage.
    /// </summary>
    /// <param name="text"> The text to display on the button. </param>
    /// <param name="x"> X Display coordinate. </param>
    /// <param name="y"> Y Display coordinate. </param>
    /// <param name="skin"> The <see cref="Skin"/> to use. </param>
    /// <param name="stage"> The <see cref="Stage"/>. </param>
    /// <returns> The TextButton. </returns>
    public static TextButton AddButton( string text, int x, int y, Skin skin, Stage stage )
    {
        var button = new TextButton( text, skin );

        button.SetPosition( x, y );
        button.IsVisible = true;
        button.SetZIndex( 1 );

        stage.AddActor( button );

        return button;
    }

    /// <summary>
    /// Makes a selectable <see cref="CheckBox"/> and adds it to the stage.
    /// </summary>
    /// <param name="imageOn"> The image to display when selected. </param>
    /// <param name="imageOff"> The image to display when deselected. </param>
    /// <param name="x"> X Display coordinate. </param>
    /// <param name="y"> Y Display coordinate. </param>
    /// <param name="color"> The Tint. </param>
    /// <param name="skin"> The <see cref="Skin"/> to use. </param>
    /// <param name="stage"> The <see cref="Stage"/>. </param>
    /// <returns> The Checkbox. </returns>
    public static CheckBox AddCheckBox( TextureRegion imageOn,
                                        TextureRegion imageOff,
                                        int x,
                                        int y,
                                        Color color,
                                        Skin skin,
                                        Stage stage )
    {
        CheckBox checkBox = MakeCheckBox( imageOn, imageOff, x, y, color, skin );

        stage.AddActor( checkBox );

        return checkBox;
    }

    /// <summary>
    /// Makes a text <see cref="Label"/>, without adding it to the stage.
    /// </summary>
    /// <param name="str"> The label text to display. </param>
    /// <param name="x">" X Display coordinate. </param>
    /// <param name="y">" Y Display coordinate. </param>
    /// <param name="color"> The Tint. </param>
    /// <param name="skin"> The <see cref="Skin"/> to use. </param>
    /// <returns> The label. </returns>
    public static Label MakeLabel( string str, int x, int y, Color color, Skin skin )
    {
        var        label = new Label( str, skin );
        LabelStyle style = label.Style;

        style.FontColor = color;

        label.Style = style;
        label.SetAlignment( Align.Center );
        label.SetPosition( x, y );

        return label;
    }

    /// <summary>
    /// Make a <see cref="Slider"/> bar with a sliding indicator,
    /// without adding it to the stage.
    /// </summary>
    /// <param name="min"> The minimum value. </param>
    /// <param name="max"> The maximum value. </param>
    /// <param name="stepSize"> The step size between values. </param>
    /// <param name="vertical">
    /// True if the slider should be vertical, false if it should be horizontal.
    /// </param>
    /// <param name="pos"> X, Y Display coordinates. </param>
    /// <param name="size"> The slider Width and Height in pixels. </param>
    /// <param name="skin"> The <see cref="Skin"/> to use. </param>
    /// <returns> The Slider. </returns>
    public static Slider MakeSlider( float min,
                                     float max,
                                     float stepSize,
                                     bool vertical,
                                     Vector2 pos,
                                     Vector2 size,
                                     Skin skin )
    {
        var slider = new Slider( min, max, stepSize, vertical, skin );

        slider.SetPosition( pos.X, pos.Y );
        slider.SetSize( size.X, size.Y );

        return slider;
    }

    /// <summary>
    /// Create a <see cref="CheckBox"/> without adding it to stage.
    /// </summary>
    /// <param name="imageOn"> The image to display when box is selected. </param>
    /// <param name="imageOff"> The image to display when box is deselected. </param>
    /// <param name="x"> X Display coordinate. </param>
    /// <param name="y"> Y Display coordinate. </param>
    /// <param name="color"> The Tint. </param>
    /// <param name="skin"> The <see cref="Skin"/> to use. </param>
    /// <returns> The Checkbox. </returns>
    public static CheckBox MakeCheckBox( TextureRegion imageOn, TextureRegion imageOff, int x, int y, Color color, Skin skin )
    {
        var            checkBox = new CheckBox( "", skin );
        CheckBoxStyle? style    = checkBox.Style;

        style?.FontColor   = color;
        style?.CheckboxOn  = new TextureRegionDrawable( imageOn );
        style?.CheckboxOff = new TextureRegionDrawable( imageOff );

        checkBox.SetSize( imageOn.GetRegionWidth(), imageOn.GetRegionHeight() );
        checkBox.Style = style;
        checkBox.SetPosition( x, y );

        return checkBox;
    }
}

// ============================================================================
// ============================================================================