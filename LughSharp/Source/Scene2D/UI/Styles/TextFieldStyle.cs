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
    public BitmapFont     Font               { get; set; }
    public BitmapFont?    MessageFont        { get; set; }
    public Color          FontColor          { get; set; } = new( 1, 1, 1, 1 );
    public Color          FocusedFontColor   { get; set; } = new( 1, 1, 1, 1 );
    public Color          DisabledFontColor  { get; set; } = new( 0.5f, 0.5f, 0.5f, 1 );
    public Color          MessageFontColor   { get; set; } = new( 1, 1, 1, 1 );
    public ISceneDrawable Background         { get; set; } = new BaseDrawable();
    public ISceneDrawable FocusedBackground  { get; set; } = new BaseDrawable();
    public ISceneDrawable DisabledBackground { get; set; } = new BaseDrawable();
    public ISceneDrawable Cursor             { get; set; } = new BaseDrawable();
    public ISceneDrawable Selection          { get; set; } = new BaseDrawable();

    // ====================================================================

    public TextFieldStyle()
    {
        Font = new BitmapFont();
    }

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

    public TextFieldStyle( TextFieldStyle style )
    {
        Font        = style.Font;
        MessageFont = style.MessageFont;

        Background         = style.Background;
        FocusedBackground  = style.FocusedBackground;
        DisabledBackground = style.DisabledBackground;
        Cursor             = style.Cursor;
        Selection          = style.Selection;

        FontColor         = new Color( style.FontColor );
        FocusedFontColor  = new Color( style.FocusedFontColor );
        DisabledFontColor = new Color( style.DisabledFontColor );
        MessageFontColor  = new Color( style.MessageFontColor );
    }
}

// ============================================================================
// ============================================================================