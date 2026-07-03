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

using LughSharp.Source.Graphics.Fonts;
using LughSharp.Source.Scene2D.Utils;

namespace LughSharp.Source.Scene2D.UI.Styles;

/// <summary>
/// The Style for a <see cref="SelectBox{T}"/>.
/// </summary>
[PublicAPI]
public class SelectBoxStyle : ISceneStyle
{
    /// <summary>
    /// The <see cref="BitmapFont"/> used to render the text.
    /// </summary>
    public BitmapFont Font { get; set; }

    /// <summary>
    /// The color used to render the text.
    /// </summary>
    public Color FontColor { get; set; }

    /// <summary>
    /// Defines the style for the ScrollPane., which is used to handle scrolling
    /// of the list of options.
    /// </summary>
    public ScrollPaneStyle ScrollPaneStyle { get; set; }

    /// <summary>
    /// Defines the style for the ListBox, which is used to display the list of options.
    /// </summary>
    public ListBoxStyle ListBoxStyle { get; set; }

    /// <summary>
    /// The background drawable for the ScrollPane.
    /// </summary>
    public ISceneDrawable? Background { get; set; }

    /// <summary>
    /// The color of the font used when the mouse cursor is hovering over the element.
    /// </summary>
    public Color? OverFontColor      { get; set; }

    /// <summary>
    /// The color of the font used for the list box item when it is in a disabled state.
    /// </summary>
    public Color? DisabledFontColor  { get; set; }
    
    public ISceneDrawable? BackgroundOver     { get; set; }
    public ISceneDrawable? BackgroundOpen     { get; set; }
    public ISceneDrawable? BackgroundDisabled { get; set; }

    // ====================================================================

    /// <summary>
    /// Creates a new instance of the SelectBoxStyle class, with default values for
    /// properties <see cref="Font"/>, <see cref="FontColor"/>, <see cref="Background"/>,
    /// <see cref="ScrollPaneStyle"/>, and <see cref="ListBoxStyle"/>.
    /// </summary>
    public SelectBoxStyle()
    {
        Font            = new BitmapFont();
        FontColor       = Color.White;
        ScrollPaneStyle = new ScrollPaneStyle();
        ListBoxStyle    = new ListBoxStyle();
        Background      = new BaseDrawable();
    }

    /// <summary>
    /// Represents a style for a SelectBox UI component, defining visual and behavioral
    /// properties such as font, font colors, background, and related styles for
    /// associated components like ScrollPane and ListBox.
    /// </summary>
    /// <param name="font"> The <see cref="BitmapFont"/> to use for rendering text. </param>
    /// <param name="fontColor"> The <see cref="Color"/> to use for rendering text. </param>
    /// <param name="background"> The <see cref="ISceneDrawable"/> to use for rendering the background. </param>
    /// <param name="scrollStyle"> The <see cref="ScrollPaneStyle"/> to use. </param>
    /// <param name="listBoxStyle"> The <see cref="ListBoxStyle"/> to use. </param>
    public SelectBoxStyle( BitmapFont font,
                           Color fontColor,
                           ISceneDrawable background,
                           ScrollPaneStyle scrollStyle,
                           ListBoxStyle listBoxStyle )
    {
        Font            = font;
        ScrollPaneStyle = scrollStyle;
        ListBoxStyle    = listBoxStyle;
        Background      = background;

        FontColor         = fontColor;
        OverFontColor     = fontColor;
        DisabledFontColor = Color.Gray;
    }

    /// <summary>
    /// Represents the style configuration for a <see cref="SelectBox{T}"/>.
    /// Provides customization options such as fonts, colors, backgrounds, and styles
    /// for various elements of the SelectBox.
    /// </summary>
    /// <param name="style"> The <see cref="SelectBoxStyle"/> to copy. </param>
    public SelectBoxStyle( SelectBoxStyle? style )
    {
        Guard.Against.Null( style );

        // Font and Styles
        Font            = style.Font;
        ScrollPaneStyle = new ScrollPaneStyle( style.ScrollPaneStyle );
        ListBoxStyle    = new ListBoxStyle( style.ListBoxStyle );

        // Colors
        FontColor = style.FontColor;

        if ( style.OverFontColor != null )
        {
            OverFontColor = style.OverFontColor;
        }

        if ( style.DisabledFontColor != null )
        {
            DisabledFontColor = style.DisabledFontColor;
        }

        // Drawables
        Background         = style.Background;
        BackgroundOver     = style.BackgroundOver;
        BackgroundOpen     = style.BackgroundOpen;
        BackgroundDisabled = style.BackgroundDisabled;
    }
}

// ============================================================================
// ============================================================================