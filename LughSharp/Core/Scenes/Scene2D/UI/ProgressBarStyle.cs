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
/// The style for a progress bar, see <see cref="ProgressBar"/>.
/// </summary>
[PublicAPI]
public class ProgressBarStyle
{
    // The progress bar background, stretched only in one direction.
    public ISceneDrawable? Background         { get; set; }
    public ISceneDrawable? DisabledBackground { get; set; }
    public ISceneDrawable? Knob               { get; set; }
    public ISceneDrawable? DisabledKnob       { get; set; }
    public ISceneDrawable? KnobBefore         { get; set; }
    public ISceneDrawable? DisabledKnobBefore { get; set; }
    public ISceneDrawable? KnobAfter          { get; set; }
    public ISceneDrawable? DisabledKnobAfter  { get; set; }

    // ====================================================================

    public ProgressBarStyle()
    {
    }

    public ProgressBarStyle( ISceneDrawable background, ISceneDrawable knob )
    {
        Background = background;
        Knob       = knob;
    }

    public ProgressBarStyle( ProgressBarStyle style )
    {
        Background         = style.Background;
        DisabledBackground = style.DisabledBackground;
        Knob               = style.Knob;
        DisabledKnob       = style.DisabledKnob;
        KnobBefore         = style.KnobBefore;
        DisabledKnobBefore = style.DisabledKnobBefore;
        KnobAfter          = style.KnobAfter;
        DisabledKnobAfter  = style.DisabledKnobAfter;
    }
}

// ============================================================================
// ============================================================================

