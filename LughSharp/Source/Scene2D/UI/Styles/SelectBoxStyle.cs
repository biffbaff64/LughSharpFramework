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
/// The Style for a <see cref="SelectBox{T}"/>.
/// </summary>
[PublicAPI]
public class SelectBoxStyle : ISceneStyle
{
    public BitmapFont      Font               { get; }
    public ScrollPaneStyle ScrollPaneStyle    { get; }
    public ListBoxStyle    ListBoxStyle       { get; }
    public Color           FontColor          { get; } = new Color( 1, 1, 1, 1 );
    public Color           OverFontColor      { get; } = new Color( 1, 1, 1, 1 );
    public Color           DisabledFontColor  { get; } = new Color( 0.5f, 0.5f, 0.5f, 1 );
    public ISceneDrawable  Background         { get; } = new BaseDrawable();
    public ISceneDrawable  BackgroundOver     { get; } = new BaseDrawable();
    public ISceneDrawable  BackgroundOpen     { get; } = new BaseDrawable();
    public ISceneDrawable  BackgroundDisabled { get; } = new BaseDrawable();

    // ====================================================================

    public SelectBoxStyle()
    {
        Font            = new BitmapFont();
        ScrollPaneStyle = new ScrollPaneStyle();
        ListBoxStyle    = new ListBoxStyle();
    }

    public SelectBoxStyle( BitmapFont font,
                           Color fontColor,
                           ISceneDrawable background,
                           ScrollPaneStyle scrollStyle,
                           ListBoxStyle listBoxStyle )
    {
        Font            = font;
        ScrollPaneStyle = scrollStyle;
        ListBoxStyle    = listBoxStyle;
        Background      = background;

        FontColor         = fontColor;
        OverFontColor     = fontColor;
        DisabledFontColor = new Color( 0.5f, 0.5f, 0.5f, 1 );
    }

    /// <summary>
    /// Copy Constructor
    /// </summary>
    /// <param name="style"></param>
    /// <exception cref="NullReferenceException"></exception>
    public SelectBoxStyle( SelectBoxStyle? style )
    {
        Guard.Against.Null( style );

        // Font and Styles
        Font            = style.Font;
        ScrollPaneStyle = new ScrollPaneStyle( style.ScrollPaneStyle ?? throw new NullReferenceException() );
        ListBoxStyle    = new ListBoxStyle( style.ListBoxStyle ?? throw new NullReferenceException() );

        // Colors
        FontColor         = style.FontColor;
        OverFontColor     = style.OverFontColor;
        DisabledFontColor = style.DisabledFontColor;

        // Drawables
        Background         = style.Background;
        BackgroundOver     = style.BackgroundOver;
        BackgroundOpen     = style.BackgroundOpen;
        BackgroundDisabled = style.BackgroundDisabled;
    }
}

// ============================================================================
// ============================================================================