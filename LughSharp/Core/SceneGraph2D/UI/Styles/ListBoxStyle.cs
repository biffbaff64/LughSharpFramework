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

namespace LughSharp.Core.SceneGraph2D.UI.Styles;

/// <summary>
/// The style for a list, see <see cref="ListBox{T}"/>.
/// </summary>
[PublicAPI]
public class ListBoxStyle : ISceneStyle
{
    public BitmapFont      Font                { get; set; }
    public Color           FontColorSelected   { get; set; } = new( 1, 1, 1, 1 );
    public Color           FontColorUnselected { get; set; } = new( 1, 1, 1, 1 );
    public ISceneDrawable? Selection           { get; set; }
    public ISceneDrawable? Down                { get; set; }
    public ISceneDrawable? Over                { get; set; }
    public ISceneDrawable? Background          { get; set; }
    
    // ========================================================================

    public ListBoxStyle()
    {
        Font = new BitmapFont();
    }

    public ListBoxStyle( BitmapFont font, Color fontColorSelected, Color fontColorUnselected,
                         ISceneDrawable selection )
    {
        Font = font;
        FontColorSelected.Set( fontColorSelected );
        FontColorUnselected.Set( fontColorUnselected );
        Selection = selection;
    }

    public ListBoxStyle( ListBoxStyle boxStyle )
    {
        Font = boxStyle.Font;
        FontColorSelected.Set( boxStyle.FontColorSelected );
        FontColorUnselected.Set( boxStyle.FontColorUnselected );
        Selection = boxStyle.Selection;

        Down       = boxStyle.Down;
        Over       = boxStyle.Over;
        Background = boxStyle.Background;
    }
}

// ============================================================================
// ============================================================================

