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

using LughSharp.Core.Scenes.Scene2D.Utils;

namespace LughSharp.Core.Scenes.Scene2D.UI;

/// <summary>
/// The style for a <see cref="Tree{TN,TV}"/>.
/// </summary>
[PublicAPI]
public class TreeStyle< TNode, TValue > where TNode : Tree< TNode, TValue >.Node
{
    public ISceneDrawable  Plus       { get; set; }
    public ISceneDrawable  Minus      { get; set; }
    public ISceneDrawable? PlusOver   { get; set; }
    public ISceneDrawable? MinusOver  { get; set; }
    public ISceneDrawable? Over       { get; set; }
    public ISceneDrawable? Selection  { get; set; }
    public ISceneDrawable? Background { get; set; }

    // ====================================================================

    public TreeStyle( ISceneDrawable plus, ISceneDrawable minus, ISceneDrawable? selection )
    {
        Plus      = plus;
        Minus     = minus;
        Selection = selection;
    }

    public TreeStyle( TreeStyle< TNode, TValue > style )
    {
        Plus  = style.Plus;
        Minus = style.Minus;

        PlusOver  = style.PlusOver;
        MinusOver = style.MinusOver;

        Over       = style.Over;
        Selection  = style.Selection;
        Background = style.Background;
    }
}

// ============================================================================
// ============================================================================

