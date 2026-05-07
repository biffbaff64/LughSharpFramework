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
using LughSharp.Source.IO;
using LughSharp.Source.Scene2D.Utils;

namespace LughSharp.Source.Scene2D.UI.Styles;

/// <summary>
/// The style for a select box, see <see cref="CheckBox"/>.
/// </summary>
[PublicAPI]
public class CheckBoxStyle : TextButtonStyle
{
    public ISceneDrawable CheckboxOn          { get; set; } = new BaseDrawable();
    public ISceneDrawable CheckboxOff         { get; set; } = new BaseDrawable();
    public ISceneDrawable CheckboxOnOver      { get; set; } = new BaseDrawable();
    public ISceneDrawable CheckboxOver        { get; set; } = new BaseDrawable();
    public ISceneDrawable CheckboxOnDisabled  { get; set; } = new BaseDrawable();
    public ISceneDrawable CheckboxOffDisabled { get; set; } = new BaseDrawable();

    // ========================================================================

    /// <summary>
    /// Default constructor. All properties are initialised to default values.
    /// </summary>
    public CheckBoxStyle()
    {
    }

    /// <summary>
    /// Creates a new CheckBoxStyle with the given ISceneDrawables for <see cref="CheckboxOn"/>,
    /// <see cref="CheckboxOff"/>, and base properties <see cref="TextButtonStyle.Font"/> and
    /// <see cref="TextButtonStyle.FontColor"/>.
    /// </summary>
    /// <param name="checkboxOff"> The OFF-state SceneDrawable. </param>
    /// <param name="checkboxOn"> The ON-state SceneDrawable. </param>
    /// <param name="font"> The <see cref="BitmapFont"/> to use for labels. </param>
    /// <param name="fontColor"> The font color to use. </param>
    public CheckBoxStyle( ISceneDrawable checkboxOff, ISceneDrawable checkboxOn, BitmapFont font, Color fontColor )
    {
        CheckboxOn  = checkboxOn;
        CheckboxOff = checkboxOff;

        // Properties from base
        Font      = font.Copy();
        FontColor = fontColor.Copy();
    }

    /// <summary>
    /// Creates a new CheckBoxStyle with the same properties as the supplied style.
    /// </summary>
    /// <param name="style"></param>
    public CheckBoxStyle( CheckBoxStyle style ) : base( style )
    {
        CheckboxOn          = style.CheckboxOn;
        CheckboxOff         = style.CheckboxOff;
        CheckboxOnOver      = style.CheckboxOnOver;
        CheckboxOver        = style.CheckboxOver;
        CheckboxOnDisabled  = style.CheckboxOnDisabled;
        CheckboxOffDisabled = style.CheckboxOffDisabled;
    }
}

// ============================================================================
// ============================================================================