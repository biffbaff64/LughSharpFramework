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
/// The style for a <see cref="ScrollPane"/>.
/// </summary>
[PublicAPI]
public class ScrollPaneStyle : ISceneStyle
{
    /// <summary>
    /// Defines the background drawable of the scroll pane's UI. This property determines
    /// the visual styling, appearance, and boundaries of the scroll pane's background,
    /// providing a base layer that complements other UI elements within the scrollable
    /// area.
    /// </summary>
    public ISceneDrawable? Background { get; set; }

    /// <summary>
    /// Represents the drawable element for the corner area where horizontal and vertical
    /// scroll bars intersect in a scroll pane's UI. This property defines the visual appearance
    /// or style of that intersecting region, enhancing the overall design and seamlessness of the
    /// scroll pane interface.
    /// </summary>
    public ISceneDrawable? Corner { get; set; }

    /// <summary>
    /// Represents the drawable element for the horizontal scroll track in a scroll pane's
    /// UI. This property defines the appearance or style of the track, which provides
    /// the visual context for the horizontal scrolling area where the scroll knob operates.
    /// </summary>
    public ISceneDrawable? HScroll { get; set; }

    /// <summary>
    /// Represents the drawable element for the horizontal scroll knob in a scroll pane's
    /// UI. This property defines the appearance or style of the knob, which allows users
    /// to interact with and navigate horizontally scrollable content.
    /// </summary>
    public ISceneDrawable? HScrollKnob { get; set; }

    /// <summary>
    /// Represents the drawable element for the vertical scroll track in a scroll pane's
    /// UI. This property defines the appearance or style for the background of the vertical
    /// scrollbar, enabling users to visually navigate through vertically scrollable content.
    /// </summary>
    public ISceneDrawable? VScroll { get; set; }

    /// <summary>
    /// Represents the drawable element for the vertical scroll knob in a scroll pane's
    /// UI. This property defines the appearance or style for the knob used to navigate
    /// a vertical scrollbar.
    /// </summary>
    public ISceneDrawable? VScrollKnob { get; set; }

    // ====================================================================

    /// <summary>
    /// Represents the visual styling for a <see cref="ScrollPane"/> component,
    /// allowing customization of its appearance, including background, scroll bars,
    /// and other drawable elements.
    /// </summary>
    public ScrollPaneStyle()
    {
    }

    /// <summary>
    /// Defines the visual style for a <see cref="ScrollPane"/> by specifying customizable
    /// drawable elements such as background, scroll bars, and scroll knobs.
    /// </summary>
    /// <param name="background"> The drawable element representing the background of the scroll pane. </param>
    /// <param name="hScroll"> The drawable element representing the horizontal scroll track. </param>
    /// <param name="hScrollKnob"> The drawable element representing the horizontal scroll knob. </param>
    /// <param name="vScroll"> The drawable element representing the vertical scroll track. </param>
    /// <param name="vScrollKnob"> The drawable element representing the vertical scroll knob. </param>
    public ScrollPaneStyle( ISceneDrawable background,
                            ISceneDrawable hScroll,
                            ISceneDrawable hScrollKnob,
                            ISceneDrawable vScroll,
                            ISceneDrawable vScrollKnob )
    {
        Background  = background;
        HScroll     = hScroll;
        HScrollKnob = hScrollKnob;
        VScroll     = vScroll;
        VScrollKnob = vScrollKnob;
    }

    /// <summary>
    /// Creates a new ScrollPaneStyle from the provided ScrollPaneStyle.
    /// </summary>
    /// <param name="style">The ScrollPaneStyle to copy.</param>
    public ScrollPaneStyle( ScrollPaneStyle style )
    {
        Background = style.Background;
        Corner     = style.Corner;

        HScroll     = style.HScroll;
        HScrollKnob = style.HScrollKnob;

        VScroll     = style.VScroll;
        VScrollKnob = style.VScrollKnob;
    }
}

// ============================================================================
// ============================================================================