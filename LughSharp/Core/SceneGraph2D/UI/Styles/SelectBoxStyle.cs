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

using JetBrains.Annotations;

using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Fonts;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.SceneGraph2D.Utils;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.SceneGraph2D.UI.Styles;

/// <summary>
/// The Style for a <see cref="SelectBox{T}"/>.
/// </summary>
[PublicAPI]
public class SelectBoxStyle : ISceneStyle
{
    public BitmapFont      Font               { get; } = null!;
    public ScrollPaneStyle ScrollStyle        { get; } = null!;
    public ListBoxStyle    ListBoxStyle       { get; } = null!;
    public Color           FontColor          { get; } = new( 1, 1, 1, 1 );
    public Color?          OverFontColor      { get; }
    public Color?          DisabledFontColor  { get; }
    public ISceneDrawable? Background         { get; }
    public ISceneDrawable? BackgroundOver     { get; }
    public ISceneDrawable? BackgroundOpen     { get; }
    public ISceneDrawable? BackgroundDisabled { get; }

    // ====================================================================

    public SelectBoxStyle()
    {
    }

    public SelectBoxStyle( BitmapFont font,
                           Color fontColor,
                           ISceneDrawable background,
                           ScrollPaneStyle scrollStyle,
                           ListBoxStyle listBoxStyle )
    {
        Font         = font;
        Background   = background;
        ScrollStyle  = scrollStyle;
        ListBoxStyle = listBoxStyle;

        FontColor.Set( fontColor );
    }

    public SelectBoxStyle( SelectBoxStyle? style )
    {
        Guard.Against.Null( style );

        Font = style.Font;
        FontColor.Set( style.FontColor );

        if ( style.OverFontColor != null )
        {
            OverFontColor = new Color( style.OverFontColor );
        }

        if ( style.DisabledFontColor != null )
        {
            DisabledFontColor = new Color( style.DisabledFontColor );
        }

        Background   = style.Background;
        ScrollStyle  = new ScrollPaneStyle( style.ScrollStyle );
        ListBoxStyle = new ListBoxStyle( style.ListBoxStyle );

        BackgroundOver     = style.BackgroundOver;
        BackgroundOpen     = style.BackgroundOpen;
        BackgroundDisabled = style.BackgroundDisabled;
    }
}

// ============================================================================
// ============================================================================