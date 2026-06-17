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

using LughSharp.Source.Scene2D.Utils;

namespace LughSharp.Source.Scene2D.UI.Styles;

/// <summary>
/// The style for a <see cref="Tree{TN,TV}"/>.
/// </summary>
[PublicAPI]
public class TreeStyle : ISceneStyle
{
    /// <summary>
    /// The <see cref="ISceneDrawable"/>s used for the Plus sign.
    /// </summary>
    public ISceneDrawable? Plus { get; set; }

    /// <summary>
    /// The <see cref="ISceneDrawable"/>s used for the Minus sign.
    /// </summary>
    public ISceneDrawable? Minus { get; set; }

    /// <summary>
    /// The <see cref="ISceneDrawable"/> used to represent the hovered state of the Plus sign,
    /// typically displayed when the user moves the cursor over the expand icon in a tree view.
    /// </summary>
    public ISceneDrawable? PlusOver { get; set; }

    /// <summary>
    /// The <see cref="ISceneDrawable"/> used to represent the hovered state of the Minus sign,
    /// typically displayed when the user moves the cursor over the collapse icon in a tree view.
    /// </summary>
    public ISceneDrawable? MinusOver { get; set; }

    /// <summary>
    /// The <see cref="ISceneDrawable"/> used to represent the hovered state of the tree view.
    /// </summary>
    public ISceneDrawable? Over { get; set; }

    /// <summary>
    /// The <see cref="ISceneDrawable"/> used to represent the selected state of the tree view.
    /// </summary>
    public ISceneDrawable? Selection { get; set; }

    /// <summary>
    /// The <see cref="ISceneDrawable"/> used to represent the background of the tree view.
    /// </summary>
    public ISceneDrawable? Background { get; set; }

    // ====================================================================

    /// <summary>
    /// Creates a new, uninitialized <see cref="TreeStyle"/>.
    /// </summary>
    public TreeStyle()
    {
    }

    /// <summary>
    /// Creates a new <see cref="TreeStyle"/> with the given <see cref="ISceneDrawable"/>s. 
    /// </summary>
    /// <param name="plus"> The drawable to use for the plus sign. </param>
    /// <param name="minus"> The drawable to use for the minus sign. </param>
    /// <param name="selection"> The drawable to use for the selection. </param>
    public TreeStyle( ISceneDrawable plus, ISceneDrawable minus, ISceneDrawable? selection )
    {
        Plus      = plus;
        Minus     = minus;
        Selection = selection;
    }

    /// <summary>
    /// Creates a new <see cref="TreeStyle"/> using the given <see cref="TreeStyle"/> as a template.
    /// </summary>
    /// <param name="style"> The <see cref="TreeStyle"/> to use as a template. </param>
    public TreeStyle( TreeStyle style )
    {
        Plus       = style.Plus;
        Minus      = style.Minus;
        PlusOver   = style.PlusOver;
        MinusOver  = style.MinusOver;
        Over       = style.Over;
        Selection  = style.Selection;
        Background = style.Background;
    }
}

// ============================================================================
// ============================================================================