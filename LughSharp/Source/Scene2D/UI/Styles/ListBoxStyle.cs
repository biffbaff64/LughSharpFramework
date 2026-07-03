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
/// The style for a list, see <see cref="ListBox{T}"/>.
/// </summary>
[PublicAPI]
public class ListBoxStyle : ISceneStyle
{
    /// <summary>
    /// The <see cref="BitmapFont"/> to use for the listbox.
    /// </summary>
    public BitmapFont Font { get; set; }

    /// <summary>
    /// The font color to use for the selected item in the listbox
    /// </summary>
    public Color FontColorSelected { get; set; } = Color.White;

    /// <summary>
    /// The font color to use for the unselected items in the listbox
    /// </summary>
    public Color FontColorUnselected { get; set; } = Color.White;

    /// <summary>
    /// The color to use for the highlight of the listbox selected item.
    /// </summary>
    public ISceneDrawable Selection { get; set; }

    /// <summary>
    /// The <see cref="ISceneDrawable"/> to render when a list item is pressed.
    /// </summary>
    public ISceneDrawable? Down { get; set; }

    /// <summary>
    /// The <see cref="ISceneDrawable"/> used to visually represent an item
    /// when the mouse pointer is hovering over it in a listbox.
    /// </summary>
    public ISceneDrawable? Over { get; set; }

    /// <summary>
    /// The background, if any, to use for the list dropdown.
    /// </summary>
    public ISceneDrawable? Background { get; set; }

    // ========================================================================

    /// <summary>
    /// Creates a new <see cref="ListBoxStyle"/> with default values.
    /// <see cref="Font"/> is set to a new <see cref="BitmapFont"/>.
    /// <see cref="Selection"/> is set to a new <see cref="BaseDrawable"/>.
    /// </summary>
    public ListBoxStyle()
    {
        Font      = new BitmapFont();
        Selection = new BaseDrawable();
    }

    /// <summary>
    /// Creates a new <see cref="ListBoxStyle"/> with the specified values for
    /// <see cref="Font"/>, <see cref="FontColorSelected"/>, <see cref="FontColorUnselected"/>,
    /// and <see cref="Selection"/>.
    /// </summary>
    /// <param name="font"> The <see cref="BitmapFont"/> to use for rendering list items. </param>
    /// <param name="fontColorSelected"> The color to use for rendering selected list items. </param>
    /// <param name="fontColorUnselected"> The color to use for rendering unselected list items. </param>
    /// <param name="selection"> The <see cref="ISceneDrawable"/> to render when a list item is selected. </param>
    public ListBoxStyle( BitmapFont font, Color fontColorSelected, Color fontColorUnselected,
                         ISceneDrawable selection )
    {
        Font      = font;
        Selection = selection;

        FontColorSelected.Set( fontColorSelected );
        FontColorUnselected.Set( fontColorUnselected );
    }

    /// <summary>
    /// Creates a new <see cref="ListBoxStyle"/> using the specified <see cref="ListBoxStyle"/>.
    /// </summary>
    /// <param name="boxStyle"> The <see cref="ListBoxStyle"/> to copy. </param>
    public ListBoxStyle( ListBoxStyle boxStyle )
    {
        Font       = boxStyle.Font;
        Selection  = boxStyle.Selection;
        Down       = boxStyle.Down;
        Over       = boxStyle.Over;
        Background = boxStyle.Background;

        FontColorSelected.Set( boxStyle.FontColorSelected );
        FontColorUnselected.Set( boxStyle.FontColorUnselected );
    }
}

// ============================================================================
// ============================================================================