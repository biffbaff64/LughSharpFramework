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
using System.Text;

using JetBrains.Annotations;

using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.Maths;
using LughSharp.Core.SceneGraph2D.Styles;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;

namespace LughSharp.Core.SceneGraph2D.UI;

/// <summary>
/// A text label, with optional word wrapping.
/// <para>
/// The preferred size of the label is determined by the actual text bounds,
/// unless <see cref="Wrap"/> is enabled.
/// </para>
/// </summary>
[PublicAPI]
public class Label : Widget, IDisposable
{
    public Align       LabelAlign  { get; set; } = Align.Left;
    public Align       LineAlign   { get; set; } = Align.Left;
    public GlyphLayout GlyphLayout { get; set; } = new();

    // ========================================================================

    private readonly Color       _tempColor      = new();
    private readonly GlyphLayout _prefSizeLayout = new();
    private readonly Vector2     _prefSize       = new();

    private StringBuilder _text            = new();
    private int           _intValue        = int.MinValue;
    private bool          _prefSizeInvalid = true;

    private BitmapFontCache _fontCache;
    private float           _lastPrefHeight;
    private string?         _ellipsis;
    private bool            _fontScaleChanged;

    // ========================================================================

    /// <summary>
    /// Creates a label using the supplied <see cref="Skin"/>, and with the specified text.
    /// </summary>
    /// <param name="text"> The label text. </param>
    /// <param name="skin"> The Skin, which should hold a <see cref="TextButtonStyle"/> </param>
    public Label( string text, Skin skin )
        : this( text, skin.Get< LabelStyle >() )
    {
    }

    /// <summary>
    /// Creates a label using the supplied <see cref="Skin"/>, identified by the
    /// supplied 'styleName', and with the specified text.
    /// </summary>
    /// <param name="text"> The label text. </param>
    /// <param name="skin"> The Skin, which should hold a <see cref="TextButtonStyle"/> </param>
    /// <param name="styleName">
    /// The name of the section within the style, i.e. "default" or "toggle".
    /// </param>
    public Label( string text, Skin skin, string styleName )
        : this( text, skin.Get< LabelStyle >( styleName ) )
    {
    }

    /// <summary>
    /// Creates a label, using a <see cref="LabelStyle"/> that has a BitmapFont with
    /// the specified name from the skin and the specified color.
    /// </summary>
    public Label( string text, Skin skin, string fontName, Color color )
        : this( text, new LabelStyle( skin.GetFont( fontName ), color ) )
    {
    }

    /// <summary>
    /// Creates a label, using a <see cref="LabelStyle"/> that has a BitmapFont
    /// with the specified name and the specified color from the
    /// skin.
    /// </summary>
    public Label( string text, Skin skin, string fontName, string colorName )
        : this( text, new LabelStyle( skin.GetFont( fontName ), skin.GetColor( colorName ) ) )
    {
    }

    /// <summary>
    /// Creates a label with the specified text and style.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="style"></param>
    public Label( string? text, LabelStyle style )
    {
        if ( text != null )
        {
            _text.Append( text );
        }

        _fontCache = style.Font.NewFontCache();

        Style = style;

        if ( text is { Length: > 0 } )
        {
            SetSize( GetPrefWidthSafe(), GetPrefHeightSafe() );
        }
    }

    /// <summary>
    /// If false, the text will only wrap where it contains newlines (\n). The preferred
    /// size of the label will be the text bounds.
    /// <para>
    /// If true, the text will word wrap using the width of the label. The preferred width
    /// of the label will be 0, it is expected that something external will set the width
    /// of the label. Wrapping will not occur when ellipsis is enabled.
    /// </para>
    /// <para>
    /// Default is false.
    /// </para>
    /// <para>
    /// When wrap is enabled, the label's preferred height depends on the width of the
    /// label. In some cases the parent of the label will need to layout twice: once to
    /// set the width of the label and a second time to adjust to the label's new preferred
    /// height.
    /// </para>
    /// </summary>
    public bool Wrap
    {
        get;
        set
        {
            field = value;

            InvalidateHierarchy();
        }
    }

    /// <summary>
    /// The font X scale of the label.
    /// </summary>
    public float FontScaleX
    {
        get;
        set
        {
            _fontScaleChanged = true;
            field             = value;

            InvalidateHierarchy();
        }
    }

    /// <summary>
    /// The font Y scale of the label.
    /// </summary>
    public float FontScaleY
    {
        get;
        set
        {
            _fontScaleChanged = true;
            field             = value;

            InvalidateHierarchy();
        }
    }

    /// <summary>
    /// The <see cref="LabelStyle"/> used with this label. It is used to
    /// specify the Font, FontColor, and Background
    /// </summary>
    public LabelStyle Style
    {
        get;
        set
        {
            if ( value == null ) throw new ArgumentException( "style cannot be null." );
            if ( value.Font == null ) throw new ArgumentException( "Missing LabelStyle font." );

            field      = value;
            _fontCache = value.Font.NewFontCache();

            InvalidateHierarchy();
        }
    }

    /// <summary>
    /// Returns the text of the label.
    /// </summary>
    public StringBuilder GetText() => _text;

    /// <summary>
    /// Sets the text to the specified integer value. If the text is already equivalent
    /// to the specified value, a string is not allocated.
    /// </summary>
    /// <returns> true if the text was changed. </returns>
    public bool SetText( int value )
    {
        if ( _intValue == value )
        {
            return false;
        }

        _text.Clear();
        _text.Append( value );
        _intValue = value;

        InvalidateHierarchy();

        return true;
    }

    /// <summary>
    /// Sets the Label's text to the specified float value. If the text is already
    /// equivalent to the specified value, a string is not allocated. If <c>null</c>
    /// is passed, the label's text will be cleared.'
    /// </summary>
    /// <param name="newText">
    /// A string holding the text to set, or null to clear the label.
    /// </param>
    public void SetText( string? newText )
    {
        if ( newText == null )
        {
            if ( _text.Length == 0 )
            {
                return;
            }

            _text.Clear();
        }
        else
        {
            if ( TextEquals( newText ) )
            {
                return;
            }

            _text.Clear();
            _text.Append( newText );
        }

        _intValue = int.MinValue;

        InvalidateHierarchy();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool TextEquals( string? other )
    {
        if ( other == null ) return false;

        if ( _text.Length != other.Length )
        {
            return false;
        }

        for ( var i = 0; i < _text.Length; i++ )
        {
            if ( _text[ i ] != other[ i ] )
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Marks the Label as invalid, triggering a recalculation of its preferred
    /// size or layout during the next rendering pass.
    /// </summary>
    public override void Invalidate()
    {
        base.Invalidate();
        _prefSizeInvalid = true;
    }

    /// <summary>
    /// 
    /// </summary>
    private void ScaleAndComputePrefSize()
    {
        Guard.Against.Null( _fontCache );

        BitmapFont font = _fontCache.Font;

        float oldScaleX = font.GetScaleX();
        float oldScaleY = font.GetScaleY();

        if ( _fontScaleChanged )
        {
            font.FontData.SetScale( FontScaleX, FontScaleY );
        }

        ComputePrefSize();

        if ( _fontScaleChanged )
        {
            font.FontData.SetScale( oldScaleX, oldScaleY );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void ComputePrefSize()
    {
        Guard.Against.Null( _fontCache );

        _prefSizeInvalid = false;

        if ( Wrap && ( _ellipsis == null ) )
        {
            float width = Width;

            if ( Style.Background != null )
            {
                width = Math.Max( width, Style.Background.MinWidth )
                      - Style.Background.LeftWidth
                      - Style.Background.RightWidth;
            }

            _prefSizeLayout.SetText( _fontCache.Font, _text.ToString(), Color.White, width, Align.Left, true );
        }
        else
        {
            _prefSizeLayout.SetText( _fontCache.Font, _text.ToString() );
        }

        _prefSize.Set( _prefSizeLayout.Width, _prefSizeLayout.Height );
    }

    /// <summary>
    /// 
    /// </summary>
    public void Layout()
    {
        Guard.Against.Null( _fontCache );

        BitmapFont font      = _fontCache.Font;
        float      oldScaleX = font.GetScaleX();
        float      oldScaleY = font.GetScaleY();

        if ( _fontScaleChanged )
        {
            // Update the font scale if the properties have changed.
            font.FontData.SetScale( FontScaleX, FontScaleY );
        }

        bool wrap = Wrap && ( _ellipsis == null );

        if ( wrap )
        {
            float prefHeight = GetPrefHeight();

            if ( !prefHeight.Equals( _lastPrefHeight ) )
            {
                _lastPrefHeight = prefHeight;
                InvalidateHierarchy();
            }
        }

        float width  = Width;
        float height = Height;
        float x      = 0;
        float y      = 0;

        if ( Style.Background != null )
        {
            x      =  Style.Background.LeftWidth;
            y      =  Style.Background.BottomHeight;
            width  -= Style.Background.LeftWidth + Style.Background.RightWidth;
            height -= Style.Background.BottomHeight + Style.Background.TopHeight;
        }

        GlyphLayout layout = GlyphLayout;
        float       textWidth, textHeight;

        if ( wrap || ( _text.ToString().Contains( '\n' ) ) )
        {
            // If the text can span multiple lines, determine the text's actual
            // size so it can be aligned within the label.
            layout.SetText( font,
                            _text.ToString(),
                            0,
                            _text.Length,
                            Color.White,
                            width,
                            LineAlign,
                            wrap,
                            _ellipsis );

            textWidth  = layout.Width;
            textHeight = layout.Height;

            if ( ( LabelAlign & Align.Left ) == 0 )
            {
                if ( ( LabelAlign & Align.Right ) != 0 )
                {
                    x += width - textWidth;
                }
                else
                {
                    x += ( width - textWidth ) / 2;
                }
            }
        }
        else
        {
            textWidth  = width;
            textHeight = font.FontData.CapHeight;
        }

        Guard.Against.Null( Style.Font );

        if ( ( LabelAlign & Align.Top ) != 0 )
        {
            y += _fontCache.Font.Flipped ? 0 : height - textHeight;
            y += Style.Font.GetDescent();
        }
        else if ( ( LabelAlign & Align.Bottom ) != 0 )
        {
            y += _fontCache.Font.Flipped ? height - textHeight : 0;
            y -= Style.Font.GetDescent();
        }
        else
        {
            y += ( height - textHeight ) / 2;
        }

        if ( !_fontCache.Font.Flipped )
        {
            y += textHeight;
        }

        layout.SetText( font,
                        _text.ToString(),
                        0,
                        _text.Length,
                        Color.White,
                        textWidth,
                        LineAlign,
                        wrap,
                        _ellipsis );

        _fontCache.SetText( layout, x, y );

        if ( _fontScaleChanged )
        {
            font.FontData.SetScale( oldScaleX, oldScaleY );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="parentAlpha"></param>
    public override void Draw( IBatch batch, float parentAlpha )
    {
        Validate();

        Color color = _tempColor.Set( ActorColor );
        color.A *= parentAlpha;

        if ( Style.Background != null )
        {
            Logger.Checkpoint();

            batch.SetColor( color.R, color.G, color.B, color.A );
            Style.Background?.Draw( batch, X, Y, Width, Height );
        }

        if ( Style.FontColor != null )
        {
            color.Mul( Style.FontColor );
        }

        _fontCache.SetText( _text.ToString(), X, Y );
        _fontCache.Tint( color );
        _fontCache.Draw( batch );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override float GetPrefWidth()
    {
        return GetPrefWidthSafe();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public float GetPrefWidthSafe()
    {
        if ( Wrap )
        {
            return 0;
        }

        if ( _prefSizeInvalid )
        {
            ScaleAndComputePrefSize();
        }

        float width = _prefSize.X;

        if ( Style.Background != null )
        {
            width = Math.Max( width + Style.Background!.LeftWidth + Style.Background!.RightWidth,
                              Style.Background!.MinWidth );
        }

        return width;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override float GetPrefHeight()
    {
        return GetPrefHeightSafe();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public float GetPrefHeightSafe()
    {
        if ( _prefSizeInvalid )
        {
            ScaleAndComputePrefSize();
        }

        float descentScaleCorrection = 1;

        if ( _fontScaleChanged )
        {
            descentScaleCorrection = FontScaleY / Style.Font.GetScaleY();
        }

        float height = _prefSize.Y - ( Style.Font.GetDescent() * descentScaleCorrection * 2 );

        if ( Style.Background != null )
        {
            height = Math.Max( height + Style.Background!.TopHeight + Style.Background!.BottomHeight,
                               Style.Background!.MinHeight );
        }

        return height;
    }

    /// <summary>
    /// Sets the alignmant of the label's text.
    /// </summary>
    /// <param name="alignment">
    /// Aligns all the text within the label (default left center) and each line
    /// of text horizontally (default is left).
    /// </param>
    /// <see cref="Align"/>
    public void SetAlignment( Align alignment )
    {
        SetAlignment( alignment, alignment );
    }

    /// <summary>
    /// Sets the alignment of the label's text.
    /// </summary>
    /// <param name="labelAlign"> Aligns all the text within the label (default left center). </param>
    /// <param name="lineAlign"> Aligns each line of text horizontally (default left). </param>
    /// See also <see cref="Align "/>
    public void SetAlignment( Align labelAlign, Align lineAlign )
    {
        LabelAlign = labelAlign;

        if ( ( lineAlign & Align.Left ) != 0 )
        {
            LineAlign = Align.Left;
        }
        else if ( ( lineAlign & Align.Right ) != 0 )
        {
            LineAlign = Align.Right;
        }
        else
        {
            LineAlign = Align.Center;
        }

        Invalidate();
    }

    /// <summary>
    /// Sets the scale of the label's font. The scale is applied to both the X and Y.
    /// </summary>
    /// <param name="fontScale"> The SCale. </param>
    public void SetFontScale( float fontScale )
    {
        SetFontScale( fontScale, fontScale );
    }

    /// <summary>
    /// Sets the X and Y scale of the label's font.
    /// </summary>
    /// <param name="fontScaleX"> The X Scale. </param>
    /// <param name="fontScaleY"> The Y Scale. </param>
    public void SetFontScale( float fontScaleX, float fontScaleY )
    {
        _fontScaleChanged = true;

        FontScaleX = fontScaleX;
        FontScaleY = fontScaleY;

        InvalidateHierarchy();
    }

    /// <summary>
    /// When non-null the text will be truncated "..." if it does not fit within
    /// the width of the label. Wrapping will not occur when ellipsis is enabled.
    /// Default is false.
    /// </summary>
    public void SetEllipsis( string? ellipsis )
    {
        _ellipsis = ellipsis;
    }

    /// <summary>
    /// When true the text will be truncated "..." if it does not fit within the
    /// width of the label. Wrapping will not occur when ellipsis is true. Default
    /// is false.
    /// </summary>
    public void SetEllipsis( bool ellipsis )
    {
        _ellipsis = ellipsis ? "..." : null;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        string className = GetType().Name;
        int    dotIndex  = className.LastIndexOf( '.' );

        if ( dotIndex != -1 )
        {
            className = className.Substring( dotIndex + 1 );
        }

        return ( className.IndexOf( '$' ) != -1 ? "Label " : "" ) + className + ": " + _text;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing,
    /// releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose( true );
        GC.SuppressFinalize( this );
    }

    protected virtual void Dispose( bool disposing )
    {
        if ( disposing )
        {
        }
    }
}

// ============================================================================
// ============================================================================