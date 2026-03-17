// ///////////////////////////////////////////////////////////////////////////////
// MIT License
// 
// Copyright (c) 2024 Richard Ikin.
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

using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.SceneGraph2D.Utils;

namespace LughSharp.Core.SceneGraph2D.UI;

/// <summary>
/// The style for a <see cref="TextField"/>.
/// </summary>
[PublicAPI]
public class TextFieldStyle : ISceneStyle
{
    public BitmapFont?     Font               { get; set; }
    public Color?          FontColor          { get; set; }
    public Color?          FocusedFontColor   { get; set; }
    public Color?          DisabledFontColor  { get; set; }
    public ISceneDrawable? Background         { get; set; }
    public ISceneDrawable? FocusedBackground  { get; set; }
    public ISceneDrawable? DisabledBackground { get; set; }
    public ISceneDrawable? Cursor             { get; set; }
    public ISceneDrawable? Selection          { get; set; }
    public BitmapFont?     MessageFont        { get; set; }
    public Color?          MessageFontColor   { get; set; }

    // ====================================================================

    public TextFieldStyle()
    {
    }

    public TextFieldStyle( BitmapFont font,
                           Color fontColor,
                           ISceneDrawable? cursor,
                           ISceneDrawable? selection,
                           ISceneDrawable? background )
    {
        Font       = font;
        FontColor  = fontColor;
        Cursor     = cursor;
        Selection  = selection;
        Background = background;
    }

    public TextFieldStyle( TextFieldStyle style )
    {
        Font = style.Font;

        if ( style.FontColor != null )
        {
            FontColor = new Color( style.FontColor );
        }

        if ( style.FocusedFontColor != null )
        {
            FocusedFontColor = new Color( style.FocusedFontColor );
        }

        if ( style.DisabledFontColor != null )
        {
            DisabledFontColor = new Color( style.DisabledFontColor );
        }

        Background         = style.Background;
        FocusedBackground  = style.FocusedBackground;
        DisabledBackground = style.DisabledBackground;
        Cursor             = style.Cursor;
        Selection          = style.Selection;

        MessageFont = style.MessageFont;

        if ( style.MessageFontColor != null )
        {
            MessageFontColor = new Color( style.MessageFontColor );
        }
    }
}

// ============================================================================
// ============================================================================

