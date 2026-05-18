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

namespace LughSharp.Source.Scene2D.UI;

[PublicAPI]
[ActorDefinition( Role = "UI" )]
public class TextButton : Button, IStyleable< TextButtonStyle >
{
    private TextButtonStyle _style = null!;

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
    public TextButton( string? text, TextButtonStyle style ) : base( style )
    {
        SetStyle( style );

        Label = new Label( text, new LabelStyle( style.Font, style.FontColor ) );
        Label.SetAlignment( Align.Center );

        AddCell( Label ).Expand().Fill();
        SetSize( GetPrefWidthUnchecked(), GetPrefHeightUnchecked() );
    }

    /// <summary>
    /// Property: The <see cref="TextButtonStyle"/> for this TextButton.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown if an attempt to set Style to null is made.
    /// </exception>
    public override TextButtonStyle GetStyle() => _style;

    /// <summary>
    /// Property: The <see cref="TextButtonStyle"/> for this TextButton.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown if an attempt to set Style to null is made.
    /// </exception>
    public void SetStyle( TextButtonStyle value )
    {
        Guard.Against.Null( value );

        _style = value;

        this.SetStyle< TextButtonStyle >( value );
        base.SetStyle( value );

        if ( Label != null )
        {
            var style = Label.GetStyle();

            style.Font      = value.Font;
            style.FontColor = value.FontColor ?? Color.White;

            Label?.SetStyle( new LabelStyle( style ) );
        }
    }

    /// <summary>
    /// A Text<see cref="Label"/> which is used to store the text for this button.
    /// </summary>
    public Label? Label
    {
        get;
        set
        {
            Guard.Against.Null( value );

            GetLabelCell()?.Actor = value;
            field                 = value;
        }
    }

    /// <summary>
    /// Returns the appropriate label font color from the style based on
    /// the current button state. If the color is not set in the style, this
    /// method will return <see cref="Color.White"/>.
    /// </summary>
    public Color GetFontColor()
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

    /// <inheritdoc />
    public override void Draw( IBatch batch, float parentAlpha )
    {
        if ( Label != null )
        {
            Label.GetStyle().FontColor = GetFontColor();
            base.Draw( batch, parentAlpha );
        }
    }

    /// <summary>
    /// Returns the <see cref="Cell"/> for this button's <see cref="Label"/>.
    /// </summary>
    public Cell? GetLabelCell()
    {
        return Label != null ? GetCell( Label ) : null;
    }

    /// <summary>
    /// Sets this buttons <see cref="Label"/> text.
    /// </summary>
    /// <param name="text"> A string holding the Text. If <c>null</c> is passed,
    /// this will clear the Text Label.
    /// </param>
    public void SetText( string? text ) => Label?.SetText( text );

    /// <summary>
    /// Returns this buttons <see cref="Label"/> text.
    /// </summary>
    public string? GetText() => Label?.GetText().ToString();

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

        return $"{( className.IndexOf( '$' ) != -1 ? "TextButton " : string.Empty )}{className}: {Label?.GetText()}";
    }
}

// ============================================================================
// ============================================================================