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

using LughSharp.Source.Graphics.G2D;
using LughSharp.Source.Scene2D.UI.Styles;
using LughSharp.Source.Scene2D.Utils;

namespace LughSharp.Source.Scene2D.UI;

/// <summary>
/// Represents a UI button that combines an image and text, styled with <see cref="ImageTextButtonStyle"/>.
/// </summary>
/// <remarks>
/// Designed for use in UI scenes where both an icon (image) and a textual label are required.
/// Provides flexibility in styling and customization through a predefined or user-specified style.
/// </remarks>
[PublicAPI]
[ActorDefinition( Role = "UI" )]
public class ImageTextButton : Button, IStyleable< ImageTextButtonStyle >
{
    public Scene2DImage? Image { get; private set; }

    // ========================================================================

    private ImageTextButtonStyle _style;

    // ========================================================================

    /// <summary>
    /// Creates a new ImageTextButton with the specified text and <see cref="Skin"/>.
    /// </summary>
    /// <param name="text">
    /// Text to display on this button. A Label will be created to display the text.
    /// </param>
    /// <param name="skin">
    /// The <see cref="Skin"/> holding the <see cref="ImageTextButtonStyle"/> for this button.
    /// </param>
    public ImageTextButton( string text, Skin skin )
        : this( text, skin.Get< ImageTextButtonStyle >() )
    {
        Skin = skin;
    }

    /// <summary>
    /// Creates a new ImageTextButton with the specified text, <see cref="Skin"/>, and
    /// <see cref="ImageTextButtonStyle"/> name.
    /// </summary>
    /// <param name="text">
    /// Text to display on this button. A Label will be created to display the text.
    /// </param>
    /// <param name="skin">
    /// The <see cref="Skin"/> holding the <see cref="ImageTextButtonStyle"/> for this button.
    /// </param>
    /// <param name="styleName">
    /// The name of the <see cref="ImageTextButtonStyle"/> to use. The style name must be
    /// defined in the skin.
    /// </param>
    public ImageTextButton( string text, Skin skin, string styleName )
        : this( text, skin.Get< ImageTextButtonStyle >( styleName ) )
    {
        Skin = skin;
    }

    /// <summary>
    /// Creates a new ImageTextButton with the specified text and <see cref="ImageTextButtonStyle"/>.
    /// </summary>
    /// <param name="text">
    /// Text to display on this button. A Label will be created to display the text.
    /// </param>
    /// <param name="style"> The <see cref="ImageTextButtonStyle"/> to use. </param>
    public ImageTextButton( string text, ImageTextButtonStyle style )
        : base( style )
    {
        _style = style;

        CellDefaults.Space( 3 );

        Image = new Scene2DImage();
        Image.SetScaling( Scaling.Fit );
        AddCell( Image );

        Label = new Label( text, new LabelStyle( style.Font, style.FontColor ) );
        Label.SetAlignment( Align.Center );
        AddCell( Label );

        SetStyleAndSize( style );
    }

    /// <summary>
    /// Returns the buttons style.
    /// </summary>
    public override ImageTextButtonStyle GetStyle() => _style;

    /// <summary>
    /// Set the current style of the actor
    /// </summary>
    public void SetStyle( ImageTextButtonStyle style )
    {
        SetStyle< ImageTextButtonStyle >( style );
    }

    /// <summary>
    /// Sets the style and size of this button, according to the preferred width and height as
    /// defined in the provided style.
    /// </summary>
    /// <param name="style"> The style to use, which must be <see cref="ImageTextButtonStyle"/> </param>
    /// <exception cref="ArgumentException">
    /// Thrown if the style is not an <see cref="ImageTextButtonStyle"/>.
    /// </exception>
    public override void SetStyle< T >( T? style ) where T : class
    {
        if ( style is not ImageTextButtonStyle textButtonStyle )
        {
            throw new ArgumentException( "style must be a ImageTextButtonStyle." );
        }

        _style = textButtonStyle;
        base.SetStyle< ImageTextButtonStyle >( textButtonStyle );

        if ( Image != null )
        {
            UpdateImage();
        }

        LabelStyle labelStyle = Label.GetStyle();

        labelStyle.Font      = textButtonStyle.Font;
        labelStyle.FontColor = GetFontColor();

        Label.SetStyle( labelStyle );
    }

    /// <summary>
    /// Returns the appropriate image drawable from the style based on the
    /// current button state.
    /// </summary>
    /// <returns> The drawable. </returns>
    public virtual ISceneDrawable? GetImageDrawable()
    {
        // The button is disabled.
        if ( IsDisabled && ( _style.ImageDisabled != null ) )
        {
            return _style.ImageDisabled;
        }

        // The button is being pressed.
        if ( IsPressed )
        {
            if ( IsChecked && ( _style.ImageCheckedDown != null ) )
            {
                return _style.ImageCheckedDown;
            }

            if ( _style.ImageDown != null )
            {
                return _style.ImageDown;
            }
        }

        // The button is being hovered over.
        if ( IsOver )
        {
            if ( IsChecked )
            {
                if ( _style.ImageCheckedOver != null )
                {
                    return _style.ImageCheckedOver;
                }
            }
            else
            {
                if ( _style.ImageOver != null )
                {
                    return _style.ImageOver;
                }
            }
        }

        // The button is being focused.
        if ( IsChecked )
        {
            if ( _style.ImageChecked != null )
            {
                return _style.ImageChecked;
            }

            if ( IsOver && ( _style.ImageOver != null ) )
            {
                return _style.ImageOver;
            }
        }

        return _style.ImageUp;
    }

    /// <summary>
    /// Sets the image drawable based on the current button state. The default
    /// implementation sets the image drawable using <see cref="GetImageDrawable()"/>.
    /// </summary>
    public virtual void UpdateImage()
    {
        Image?.SetDrawable( GetImageDrawable() );
    }

    /// <summary>
    /// Returns the appropriate label font color from the style based on the current button state.
    /// </summary>
    protected Color GetFontColor()
    {
        if ( IsDisabled && ( _style.DisabledFontColor != null ) )
        {
            return _style.DisabledFontColor;
        }

        if ( IsPressed )
        {
            if ( IsChecked && ( _style.CheckedDownFontColor != null ) )
            {
                return _style.CheckedDownFontColor;
            }

            if ( _style.DownFontColor != null )
            {
                return _style.DownFontColor;
            }
        }

        if ( IsOver )
        {
            if ( IsChecked )
            {
                if ( _style.CheckedOverFontColor != null )
                {
                    return _style.CheckedOverFontColor;
                }
            }
            else
            {
                if ( _style.OverFontColor != null )
                {
                    return _style.OverFontColor;
                }
            }
        }

        bool focused = HasKeyboardFocus();

        if ( IsChecked )
        {
            if ( focused && ( _style.CheckedFocusedFontColor != null ) )
            {
                return _style.CheckedFocusedFontColor;
            }

            if ( _style.CheckedFontColor != null )
            {
                return _style.CheckedFontColor;
            }

            if ( IsOver && ( _style.OverFontColor != null ) )
            {
                return _style.OverFontColor;
            }
        }

        if ( focused && ( _style.FocusedFontColor != null ) )
        {
            return _style.FocusedFontColor;
        }

        return _style.FontColor ?? Color.White;
    }

    /// <summary>
    /// Renders the ImageTextButton using the specified drawing batch and alpha transparency.
    /// </summary>
    /// <param name="batch">
    /// The <see cref="IBatch"/> used to draw the button and associated child elements.
    /// </param>
    /// <param name="parentAlpha">
    /// The alpha value inherited from the parent, affecting the overall transparency of
    /// the button during rendering.
    /// </param>
    public override void Draw( IBatch batch, float parentAlpha )
    {
        UpdateImage();

        Label.GetStyle().FontColor = GetFontColor();

        base.Draw( batch, parentAlpha );
    }

    /// <summary>
    /// Retrieves the <see cref="Cell"/> associated with the button's image.
    /// </summary>
    /// <returns>
    /// The <see cref="Cell"/> containing the image, or null if no image is set.
    /// </returns>
    public Cell? GetImageCell()
    {
        return GetCell( Image! );
    }

    /// <summary>
    /// Retrieves the <see cref="Cell"/> associated with the label in this button.
    /// </summary>
    /// <returns>
    /// The <see cref="Cell"/> that holds the <see cref="Label"/> in the button,
    /// or null if no such cell exists.
    /// </returns>
    public Cell? GetLabelCell()
    {
        return GetCell( Label );
    }

    /// <summary>
    /// The label for this button. Cannot be null.
    /// </summary>
    public Label Label
    {
        get;
        set
        {
            GetLabelCell()?.Actor = value;
            field                 = value;
        }
    }

    /// <summary>
    /// Returns this buttons label text. If no label is set, an empty string is returned.
    /// </summary>
    public string GetText()
    {
        return Label.GetText().ToString();
    }

    /// <summary>
    /// Sets the label's text.
    /// </summary>
    /// <param name="text"> The new text for the label. </param>
    public void SetText( string text )
    {
        Label.GetText().Clear();
        Label.GetText().Append( text );
    }
}

// ============================================================================
// ============================================================================