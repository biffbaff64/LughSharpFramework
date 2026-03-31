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

using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;

namespace LughSharp.Core.SceneGraph2D.UI;

[PublicAPI]
[ActorDefinition( Role = "UI" )]
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
    /// A <see cref="TextButtonStyle"/> identified by <c>styleName</c> will be used.
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

        _label = NewLabel( text, new LabelStyle( style.Font, style.FontColor ) );
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
    public TextButtonStyle? Style
    {
        get;
        set
        {
            Guard.Against.Null( value );

            if ( value is not { } textButtonStyle )
            {
                throw new ArgumentException( "style must be a TextButtonStyle." );
            }

            field = textButtonStyle;
            base.SetStyle< TextButtonStyle >( textButtonStyle );

            if ( Label != null )
            {
                LabelStyle labelStyle = Label.Style;

                labelStyle.Font      = textButtonStyle.Font ?? new BitmapFont();
                labelStyle.FontColor = textButtonStyle.FontColor ?? Color.White;
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
    /// Creates a new Label with the supplied text and style.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="style"></param>
    /// <returns></returns>
    protected Label NewLabel( string? text, LabelStyle style )
    {
        return new Label( text, style );
    }

    /// <summary>
    /// Returns the appropriate label font color from the style based on
    /// the current button state.
    /// </summary>
    public Color? GetFontColor()
    {
        if ( Style == null ) return Color.White;

        if ( IsDisabled && ( Style.DisabledFontColor != null ) )
        {
            return Style.DisabledFontColor;
        }

        if ( IsPressed() )
        {
            if ( IsChecked && ( Style.CheckedDownFontColor != null ) )
            {
                return Style.CheckedDownFontColor;
            }

            if ( Style.DownFontColor != null )
            {
                return Style.DownFontColor;
            }
        }

        if ( IsOver() )
        {
            if ( IsChecked )
            {
                if ( Style.CheckedOverFontColor != null )
                {
                    return Style.CheckedOverFontColor;
                }
            }
            else
            {
                if ( Style.OverFontColor != null )
                {
                    return Style.OverFontColor;
                }
            }
        }

        bool focused = HasKeyboardFocus();

        if ( IsChecked )
        {
            if ( focused && ( Style.CheckedFocusedFontColor != null ) )
            {
                return Style.CheckedFocusedFontColor;
            }

            if ( Style.CheckedFontColor != null )
            {
                return Style.CheckedFontColor;
            }

            if ( IsOver() && ( Style.OverFontColor != null ) )
            {
                return Style.OverFontColor;
            }
        }

        if ( focused && ( Style.FocusedFontColor != null ) )
        {
            return Style.FocusedFontColor;
        }

        return Style.FontColor;
    }

    /// <inheritdoc />
    public override void Draw( IBatch batch, float parentAlpha )
    {
        if ( Label != null )
        {
            Label.Style.FontColor = GetFontColor() ?? Color.White;
            base.Draw( batch, parentAlpha );
        }
    }

    /// <summary>
    /// Returns the <see cref="Cell"/> for this button's <see cref="Label"/>.
    /// </summary>
    /// <returns> The requested Cell, or null if the button has no label. </returns>
    public Cell? GetLabelCell()
    {
        return _label != null ? GetCell( _label ) : null;
    }

    /// <summary>
    /// Sets this buttons <see cref="Label"/> text.
    /// </summary>
    /// <param name="text"> A string holding the Text. If <c>null</c> is passed,
    /// this will clear the Text Label.
    /// </param>
    public void SetText( string? text ) => _label?.SetText( text );

    /// <summary>
    /// Returns this buttons <see cref="Label"/> text.
    /// </summary>
    public string? GetText() => _label?.GetText().ToString();

    // ========================================================================

    /// <inheritdoc />
    public override string ToString()
    {
        string className = GetType().Name;
        int    dotIndex  = className.LastIndexOf( '.' );

        if ( dotIndex != -1 )
        {
            className = className.Substring( dotIndex + 1 );
        }

        return $"{( className.IndexOf( '$' ) != -1 ? "TextButton " : "" )}{className}: {_label?.GetText()}";
    }
}

// ============================================================================
// ============================================================================