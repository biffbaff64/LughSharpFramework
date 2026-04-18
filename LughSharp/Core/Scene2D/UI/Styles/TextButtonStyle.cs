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

using LughSharp.Core.Graphics.Fonts;
using LughSharp.Core.Scene2D.Utils;

namespace LughSharp.Core.Scene2D.UI.Styles;

/// <summary>
/// The style for a text button, see <see cref="TextButton"/>.
/// </summary>
[PublicAPI]
public class TextButtonStyle : ButtonStyle
{
    public BitmapFont Font                    { get; set; }
    public Color?     FontColor               { get; set; }
    public Color?     DownFontColor           { get; set; }
    public Color?     OverFontColor           { get; set; }
    public Color?     FocusedFontColor        { get; set; }
    public Color?     DisabledFontColor       { get; set; }
    public Color?     CheckedFontColor        { get; set; }
    public Color?     CheckedDownFontColor    { get; set; }
    public Color?     CheckedOverFontColor    { get; set; }
    public Color?     CheckedFocusedFontColor { get; set; }

    // ========================================================================

    public TextButtonStyle()
    {
        Font = new BitmapFont();
    }

    public TextButtonStyle( ISceneDrawable upImage,
                            ISceneDrawable downImage,
                            ISceneDrawable checkedImage,
                            BitmapFont font )
        : base( upImage, downImage, checkedImage )
    {
        Font = font;
    }

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