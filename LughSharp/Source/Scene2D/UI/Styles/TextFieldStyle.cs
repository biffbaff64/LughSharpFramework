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
/// The style for a <see cref="TextField"/>.
/// </summary>
[PublicAPI]
public class TextFieldStyle : ISceneStyle
{
    public BitmapFont      Font               { get; set; }
    public BitmapFont?     MessageFont        { get; set; }
    public Color           FontColor          { get; set; }
    public Color?          FocusedFontColor   { get; set; }
    public Color?          DisabledFontColor  { get; set; }
    public Color?          MessageFontColor   { get; set; }
    public ISceneDrawable? Background         { get; set; }
    public ISceneDrawable? FocusedBackground  { get; set; }
    public ISceneDrawable? DisabledBackground { get; set; }
    public ISceneDrawable? Cursor             { get; set; }
    public ISceneDrawable? Selection          { get; set; }

    // ====================================================================

    /// <summary>
    /// Creates a new <see cref="TextFieldStyle"/> with default values.
    /// </summary>
    public TextFieldStyle()
    {
        Font      = new BitmapFont();
        FontColor = Color.White;
    }

    /// <summary>
    /// Creates a new <see cref="TextFieldStyle"/> UI element. This style specifies the
    /// appearance of text fields, including font, font color, cursor, selection, and background.
    /// Implements <see cref="ISceneStyle"/>.
    /// </summary>
    /// <param name="font"> The <see cref="BitmapFont"/> to use for the TextField. </param>
    /// <param name="fontColor"> The color to use for the font. </param>
    /// <param name="cursor"> The cursor drawable for the TextFieldStyle. </param>
    /// <param name="selection"> The selection drawable for the TextFieldStyle. </param>
    /// <param name="background"> The background drawable for the TextFieldStyle. </param>
    public TextFieldStyle( BitmapFont font,
                           Color fontColor,
                           ISceneDrawable cursor,
                           ISceneDrawable selection,
                           ISceneDrawable background )
    {
        Font       = font;
        FontColor  = fontColor;
        Cursor     = cursor;
        Selection  = selection;
        Background = background;
    }

    /// <summary>
    /// Creates a new <see cref="TextFieldStyle"/> by copying the properties from the
    /// provided <see cref="TextFieldStyle"/>.
    /// </summary>
    /// <param name="style"> The <see cref="TextFieldStyle"/> to copy. </param>
    public TextFieldStyle( TextFieldStyle style )
    {
        Font               = style.Font;
        MessageFont        = style.MessageFont;
        Background         = style.Background;
        FocusedBackground  = style.FocusedBackground;
        DisabledBackground = style.DisabledBackground;
        Cursor             = style.Cursor;
        Selection          = style.Selection;
        FontColor          = new Color( style.FontColor );

        if ( style.FocusedFontColor != null )
        {
            FocusedFontColor = new Color( style.FocusedFontColor );
        }

        if ( style.DisabledFontColor != null )
        {
            DisabledFontColor = new Color( style.DisabledFontColor );
        }

        if ( style.MessageFontColor != null )
        {
            MessageFontColor = new Color( style.MessageFontColor );
        }
    }
}

// ============================================================================
// ============================================================================