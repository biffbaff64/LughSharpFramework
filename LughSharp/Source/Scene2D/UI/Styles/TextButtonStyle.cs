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
/// The style for a text button, see <see cref="TextButton"/>.
/// </summary>
[PublicAPI]
public class TextButtonStyle : ButtonStyle
{
    /// <summary>
    /// Gets or sets the <see cref="BitmapFont"/> used for rendering text in the text button.
    /// </summary>
    /// <remarks>
    /// The font defines the appearance and rendering of the text displayed in the button.
    /// It can be customized to use different styles or sizes of fonts, enabling more flexibility
    /// in designing the button's text appearance.
    /// </remarks>
    public BitmapFont Font { get; set; }

    /// <summary>
    /// Gets or sets the font color used for rendering text in the text button.
    /// This property defines the primary color applied to the text displayed in the button.
    /// Customizing the font color allows for adjusting the text's visual appearance to match
    /// the design requirements of the button.
    /// </summary>
    public Color? FontColor { get; set; }

    /// <summary>
    /// Gets or sets the color used for rendering the text when the button is in a pressed
    /// (down) state. This property defines the appearance of the text specifically when a
    /// button is being interacted with in its pressed state. Setting this property allows
    /// customization of the text appearance for better visual feedback during user interaction.
    /// </summary>
    public Color? DownFontColor { get; set; }

    /// <summary>
    /// Gets or sets the color used for rendering the text when the button is hovered over by
    /// the cursor. This property allows customization of the text color displayed in the button's
    /// hover state. It provides visual feedback to the user, indicating that the button is in
    /// the "hover" state, and can be customized to align with the design requirements or theme
    /// of the application.
    /// </summary>
    public Color? OverFontColor { get; set; }

    /// <summary>
    /// Gets or sets the font color used when the text button is focused.
    /// The focused font color is applied to the text of the button when it has keyboard focus.
    /// This allows for visual differentiation and enhances accessibility by providing
    /// a clear indicator of the button's focus state.
    /// </summary>
    public Color? FocusedFontColor { get; set; }

    /// <summary>
    /// Gets or sets the font color used when the text button is disabled.
    /// The disabled font color determines the appearance of the text displayed on a button
    /// when it is in a disabled state. This property allows customization of the text color
    /// to visually indicate that the button is not interactive.
    /// </summary>
    public Color? DisabledFontColor { get; set; }

    /// <summary>
    /// Gets or sets the font color used for a <see cref="TextButton"/> when it is in a
    /// checked state. This property determines the color of the text displayed on the button
    /// when the button is in a checked or toggled state. It provides a way to visually
    /// differentiate the button's state by altering its text appearance.
    /// </summary>
    public Color? CheckedFontColor { get; set; }

    /// <summary>
    /// Gets or sets the color of the text rendered when the button is in a pressed (down)
    /// state and is also in a checked state. This property allows customization of the font
    /// color for a text button when it is both pressed and checked. It can be used to
    /// differentiate the button's appearance and enhance visual feedback for user interactions.
    /// If this property is not set, the default behavior or other font color properties may
    /// be used instead.
    /// </summary>
    public Color? CheckedDownFontColor { get; set; }

    /// <summary>
    /// Gets or sets the color of the font displayed when the text button is both checked
    /// and hovered over (mouse-over state). This property allows customizing the font color
    /// to provide visual feedback when the button is in a checked state and is being hovered
    /// over. If not set, the default behavior may inherit a different font color from the
    /// style hierarchy.
    /// </summary>
    public Color? CheckedOverFontColor { get; set; }

    /// <summary>
    /// Gets or sets the color of the font when the text button is both checked and focused.
    /// This property allows customization of the font color to visually indicate the button's
    /// state as both checked and focused. It can be used to enhance the user interface by
    /// providing clear visual feedback based on the button's interaction state. If this
    /// property is not set, the fallback behavior will rely on other relevant font color
    /// properties, such as <see cref="CheckedFontColor"/> or <see cref="FocusedFontColor"/>,
    /// depending on the specific context.
    /// </summary>
    public Color? CheckedFocusedFontColor { get; set; }

    // ========================================================================

    /// <summary>
    /// Represents the style for a text button in the user interface.
    /// Inherits from <see cref="ButtonStyle"/> and introduces additional properties
    /// for font and font-related colors used for different button states.
    /// </summary>
    public TextButtonStyle()
    {
        Font = new BitmapFont();
    }

    /// <summary>
    /// Defines the style for a text button, including visual states and font properties.
    /// Inherits from <see cref="ButtonStyle"/> to provide drawable elements for different
    /// button states and extends functionality with font support for text rendering.
    /// </summary>
    public TextButtonStyle( ISceneDrawable upImage,
                            ISceneDrawable downImage,
                            ISceneDrawable checkedImage,
                            BitmapFont font ) : base( upImage, downImage, checkedImage )
    {
        Font = font;
    }

    /// <summary>
    /// Creates a new TextButtonStyle by copying the properties from the specified style.
    /// Defines the style for a text button within the user interface, inheriting from
    /// <see cref="ButtonStyle"/>. Allows customization of font and associated colors for
    /// various button states, such as default, focused, disabled, and checked.
    /// </summary>
    /// <param name="style">The style to copy properties from.</param>
    protected TextButtonStyle( TextButtonStyle style ) : base( style )
    {
        Font = style.Font;

        if ( style.FontColor != null )
        {
            FontColor = new Color( style.FontColor );
        }

        if ( style.DownFontColor != null )
        {
            DownFontColor = new Color( style.DownFontColor );
        }

        if ( style.OverFontColor != null )
        {
            OverFontColor = new Color( style.OverFontColor );
        }

        if ( style.FocusedFontColor != null )
        {
            FocusedFontColor = new Color( style.FocusedFontColor );
        }

        if ( style.DisabledFontColor != null )
        {
            DisabledFontColor = new Color( style.DisabledFontColor );
        }

        if ( style.CheckedFontColor != null )
        {
            CheckedFontColor = new Color( style.CheckedFontColor );
        }

        if ( style.CheckedDownFontColor != null )
        {
            CheckedDownFontColor = new Color( style.CheckedDownFontColor );
        }

        if ( style.CheckedOverFontColor != null )
        {
            CheckedOverFontColor = new Color( style.CheckedOverFontColor );
        }

        if ( style.CheckedFocusedFontColor != null )
        {
            CheckedFocusedFontColor = new Color( style.CheckedFocusedFontColor );
        }
    }
}

// ============================================================================
// ============================================================================