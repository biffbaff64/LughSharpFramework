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

using LughSharp.Core.Scene2D.Utils;

namespace LughSharp.Core.Scene2D.RegistryStyles;

[PublicAPI]
public record ProgressBarStyleRecord
{
    /// <summary>
    /// The progress bar background, stretched only in one direction.
    /// </summary>
    public ISceneDrawable? Background         { get; set; }
    public ISceneDrawable? DisabledBackground { get; set; }
    
    /// <summary>
    /// This is the visual indicator that marks the current value. Even if the progress
    /// bar is non-interactive (not a Slider), the "Knob" image is often used as the
    /// "head" of the progress line.
    /// </summary>
    public ISceneDrawable? Knob               { get; set; }
    public ISceneDrawable? DisabledKnob       { get; set; }

    /// <summary>
    /// This is the drawable used to fill the area before the knob (the "completed" or
    /// "filled" side). For a horizontal bar, this is the left side; for a vertical bar,
    /// it is the bottom.
    /// </summary>
    public ISceneDrawable? KnobBefore         { get; set; }
    public ISceneDrawable? DisabledKnobBefore { get; set; }

    /// <summary>
    /// This is the drawable used to fill the area after the knob (the "unfilled" or
    /// "remaining" side).
    /// </summary>
    public ISceneDrawable? KnobAfter          { get; set; }
    public ISceneDrawable? DisabledKnobAfter  { get; set; }
}

// ============================================================================
// ============================================================================
