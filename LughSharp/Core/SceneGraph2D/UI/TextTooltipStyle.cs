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

using LughSharp.Core.SceneGraph2D.Utils;

namespace LughSharp.Core.SceneGraph2D.UI;

/// <summary>
/// The style for a <see cref="TextTooltip"/>.
/// </summary>
[PublicAPI]
public class TextTooltipStyle : ISceneStyle
{
    public LabelStyle     Label      { get; set; } = null!;
    public ISceneDrawable Background { get; set; } = null!;
    public float          WrapWidth  { get; set; }

    // ========================================================================

    public TextTooltipStyle()
    {
    }

    public TextTooltipStyle( LabelStyle label, ISceneDrawable background )
    {
        Label      = label;
        Background = background;
    }

    public TextTooltipStyle( TextTooltipStyle style )
    {
        Label      = new LabelStyle( style.Label );
        Background = style.Background;
        WrapWidth  = style.WrapWidth;
    }
}

// ============================================================================
// ============================================================================