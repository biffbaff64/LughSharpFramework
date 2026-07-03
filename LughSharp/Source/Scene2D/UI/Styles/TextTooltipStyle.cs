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
/// The style for a <see cref="TextTooltip"/>.
/// </summary>
[PublicAPI]
public class TextTooltipStyle : ISceneStyle
{
    /// <summary>
    /// Gets or sets the <see cref="LabelStyle"/> that defines the font, font color,
    /// and optional background style applied to the label displayed within the
    /// <see cref="TextTooltip"/>.
    /// </summary>
    public LabelStyle LabelStyle { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ISceneDrawable"/> used to define the background
    /// appearance and styling for the <see cref="TextTooltip"/>.
    /// </summary>
    public ISceneDrawable? Background { get; set; }

    /// <summary>
    /// Gets or sets the maximum width at which the content text will wrap for a
    /// <see cref="TextTooltip"/>. This property determines the width constraint
    /// applied to the tooltip's content, ensuring text wraps appropriately within
    /// the UI layout.
    /// </summary>
    public float WrapWidth  { get; set; }

    // ========================================================================

    /// <summary>
    /// Defines a style for a <see cref="TextTooltip"/> UI element. This style includes
    /// properties for configuring label appearance, tooltip background, and text wrapping
    /// behavior. Implements <see cref="ISceneStyle"/>.
    /// </summary>
    public TextTooltipStyle()
    {
        LabelStyle = new LabelStyle();
    }

    /// <summary>
    /// Defines a style for a <see cref="TextTooltip"/> UI element. This style specifies the
    /// appearance of tooltips, including label styling, background customization, and text
    /// wrapping behavior. Implements <see cref="ISceneStyle"/>.
    /// </summary>
    /// <param name="label"> The label to display withing the TextTooltip. </param>
    /// <param name="background"> The background drawable for the TextTooltip. </param>
    public TextTooltipStyle( LabelStyle label, ISceneDrawable background )
    {
        LabelStyle = label;
        Background = background;
    }

    /// <summary>
    /// Creates a copy of the provided <see cref="TextTooltipStyle"/>.
    /// </summary>
    /// <param name="style"> The style to copy. </param>
    public TextTooltipStyle( TextTooltipStyle style )
    {
        LabelStyle = new LabelStyle( style.LabelStyle );
        Background = style.Background;
        WrapWidth  = style.WrapWidth;
    }
}

// ============================================================================
// ============================================================================