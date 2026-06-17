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
/// The style for a <see cref="Slider"/>.
/// </summary>
[PublicAPI]
public class SliderStyle : ProgressBarStyle
{
    /// <summary>
    /// The <see cref="ISceneDrawable"/> used for the Slider's background when the
    /// mouse or touch is over the slider knob.
    /// </summary>
    public ISceneDrawable? BackgroundOver { get; set; }

    /// <summary>
    /// The <see cref="ISceneDrawable"/> used for the Slider's background when
    /// the slider knob is pressed down and/or being dragged.
    /// </summary>
    public ISceneDrawable? BackgroundDown { get; set; }

    /// <summary>
    /// The <see cref="ISceneDrawable"/> used for the Slider's knob portion before the
    /// current value when the mouse or touch is over the slider knob.
    /// </summary>
    public ISceneDrawable? KnobBeforeOver { get; set; }

    /// <summary>
    /// The <see cref="ISceneDrawable"/> used for the Slider's knob portion after the
    /// current value when the mouse or touch is over the slider knob.
    /// </summary>
    public ISceneDrawable? KnobOver { get; set; }

    /// <summary>
    /// The <see cref="ISceneDrawable"/> used for the slider's knob portion after the knob
    /// when the mouse or touch is over this area.
    /// </summary>
    public ISceneDrawable? KnobAfterOver { get; set; }

    /// <summary>
    /// The <see cref="ISceneDrawable"/> used for the Slider's knob portion before the
    /// current value when the slider knob is pressed down and/or being dragged.
    /// </summary>
    public ISceneDrawable? KnobBeforeDown { get; set; }

    /// <summary>
    /// The <see cref="ISceneDrawable"/> used for the Slider's knob when it is actively
    /// being pressed or dragged by the user.
    /// </summary>
    public ISceneDrawable? KnobDown { get; set; }

    /// <summary>
    /// The <see cref="ISceneDrawable"/> used for the slider's knob portion after the
    /// knob when the slider knob is pressed down and/or being dragged.
    /// </summary>
    public ISceneDrawable? KnobAfterDown { get; set; }

    // ====================================================================

    /// <summary>
    /// Creates a new, uninitialized SliderStyle.
    /// </summary>
    public SliderStyle()
    {
    }

    /// <summary>
    /// Creates a new SliderStyle with the given ISceneDrawables.
    /// </summary>
    /// <param name="background"> The background <see cref="ISceneDrawable"/>. </param>
    /// <param name="knob"> The <see cref="ISceneDrawable"/> to use for the default slider knob. </param>
    public SliderStyle( ISceneDrawable background, ISceneDrawable knob )
        : base( background, knob )
    {
    }

    /// <summary>
    /// Creates a new SliderStyle using the <see cref="ISceneDrawable"/>s from the given SliderStyle.
    /// </summary>
    /// <param name="style"> The SliderStyle to copy the <see cref="ISceneDrawable"/>s from. </param>
    public SliderStyle( SliderStyle style ) : base( style )
    {
        BackgroundOver = style.BackgroundOver;
        BackgroundDown = style.BackgroundDown;

        KnobOver = style.KnobOver;
        KnobDown = style.KnobDown;

        KnobBeforeOver = style.KnobBeforeOver;
        KnobBeforeDown = style.KnobBeforeDown;

        KnobAfterOver = style.KnobAfterOver;
        KnobAfterDown = style.KnobAfterDown;
    }
}

// ============================================================================
// ============================================================================