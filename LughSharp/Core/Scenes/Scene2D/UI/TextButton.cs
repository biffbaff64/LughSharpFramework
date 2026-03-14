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
using System.Diagnostics;

using JetBrains.Annotations;

using LughSharp.Core.Graphics.Colors;
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Scenes.Scene2D.UI;

[PublicAPI]
public class TextButton : Button
{
    private Label? _label;

    // ========================================================================

    /// <summary>
    /// Creates a new TextButton using the supplied <see cref="Skin"/>, and
    /// setting its text property to the supplied text.
    /// The default <see cref="TextButtonStyle"/> will be used.
    /// </summary>
    public TextButton( string? text, Skin skin )
        : this( text, skin.Get< TextButtonStyle >() )
    {
        Skin = skin;
    }

    /// <summary>
    /// Creates a new TextButton using the supplied <see cref="Skin"/>, and
    /// setting its text property to the supplied text.
    /// A <see cref="TextButtonStyle"/> identified by <tt>styleName</tt> will be used.
    /// </summary>
    public TextButton( string? text, Skin skin, string styleName )
        : this( text, skin.Get< TextButtonStyle >( styleName ) )
    {
        Skin = skin;
    }

    /// <summary>
    /// Creates a new TextButton, setting its text property to the supplied text.
    /// The supplied <see cref="TextButtonStyle"/> will be used.
    /// </summary>
    public TextButton( string? text, TextButtonStyle style )
    {
        Style = style;

        _label = new Label( text, new LabelStyle( style.Font ?? new BitmapFont(),
                                                  style.FontColor ?? Color4.White ) );
        _label.SetAlignment( Align.Center );

        Add( _label ).Expand().SetFill();

        SetSize( GetPrefWidthSafe(), GetPrefHeightSafe() );
    }
    
    /// <summary>
    /// Property: The <see cref="TextButtonStyle"/> for this TextButton.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown if an attempt to set Style to null is made.
    /// </exception>
    public new TextButtonStyle? Style
    {
        get;
        set
        {
            Guard.Against.Null( value );

            if ( value is not { } textButtonStyle )
            {
                throw new ArgumentException( "style must be a TextButtonStyle." );
            }

            field      = textButtonStyle;
            base.Style = textButtonStyle;

            if ( Label != null )
            {
                LabelStyle labelStyle = Label.Style;

                labelStyle.Font      = textButtonStyle.Font ?? new BitmapFont();
                labelStyle.FontColor = textButtonStyle.FontColor ?? Color4.White;
                Label.Style          = labelStyle;
            }
        }
    }

    /// <summary>
    /// A Text<see cref="Label"/> which is used to store the text for this button.
    /// </summary>
    public Label? Label
    {
        get => _label;
        set
        {
            Guard.Against.Null( value );

            GetLabelCell()!.Actor = value;
            _label                = value;
        }
    }

    /// <summary>
    /// Returns the appropriate label font color from the style based on
    /// the current button state.
    /// </summary>
    public Color4? GetFontColor()
    {
        Debug.Assert( Style != null, "Style is null" );

        if ( IsDisabled && ( Style?.DisabledFontColor != null ) )
        {
            return Style?.DisabledFontColor;
        }

        if ( IsPressed() )
        {
            if ( IsChecked && ( Style?.CheckedDownFontColor != null ) )
            {
                return Style?.CheckedDownFontColor;
            }

            if ( Style?.DownFontColor != null )
            {
                return Style?.DownFontColor;
            }
        }

        if ( IsOver() )
        {
            if ( IsChecked )
            {
                if ( Style?.CheckedOverFontColor != null )
                {
                    return Style?.CheckedOverFontColor;
                }
            }
            else
            {
                if ( Style?.OverFontColor != null )
                {
                    return Style?.OverFontColor;
                }
            }
        }

        bool focused = HasKeyboardFocus();

        if ( IsChecked )
        {
            if ( focused && ( Style?.CheckedFocusedFontColor != null ) )
            {
                return Style?.CheckedFocusedFontColor;
            }

            if ( Style?.CheckedFontColor != null )
            {
                return Style?.CheckedFontColor;
            }

            if ( IsOver() && ( Style?.OverFontColor != null ) )
            {
                return Style?.OverFontColor;
            }
        }

        if ( focused && ( Style?.FocusedFontColor != null ) )
        {
            return Style?.FocusedFontColor;
        }

        return Style?.FontColor;
    }

    /// <inheritdoc />
    public override void Draw( IBatch batch, float parentAlpha )
    {
        if ( Label != null )
        {
            Label.Style.FontColor = GetFontColor() ?? Color4.White;
            base.Draw( batch, parentAlpha );
        }
    }

    public Cell? GetLabelCell()
    {
        return GetCell( _label! );
    }

    public void SetText( string? text )
    {
        _label?.SetText( text );
    }

    public string? GetText()
    {
        return _label?.Text.ToString();
    }

    // ========================================================================

    /// <inheritdoc />
    public override string ToString()
    {
        if ( Name != null )
        {
            return Name;
        }

        string className = GetType().Name;
        int    dotIndex  = className.LastIndexOf( '.' );

        if ( dotIndex != -1 )
        {
            className = className.Substring( dotIndex + 1 );
        }

        return $"{( className.IndexOf( '$' ) != -1 ? "TextButton " : "" )}{className}: {_label?.Text}";
    }

    // ========================================================================
    // ========================================================================
}

// ============================================================================
// ============================================================================

