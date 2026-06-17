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
/// The style for a label, see <see cref="Label"/>.
/// </summary>
[PublicAPI]
public class LabelStyle : ISceneStyle
{
    /// <summary>
    /// The <see cref="BitmapFont"/> used for the label.
    /// </summary>
    public BitmapFont Font { get; set; }

    /// <summary>
    /// The color of the font used in the label. If set to null, the default color is used.
    /// </summary>
    public Color? FontColor { get; set; }

    /// <summary>
    /// The <see cref="ISceneDrawable"/> used to render the background of the label.
    /// It defines the visual look and padding of the label's background.
    /// </summary>
    public ISceneDrawable? Background { get; set; }

    // ====================================================================

    /// <summary>
    /// Creates a new LabelStyle with a default font and white color. The
    /// style also specifies no background, but this can be changed later.
    /// </summary>
    public LabelStyle()
    {
        Font = new BitmapFont();
    }

    /// <summary>
    /// Creates a new LabelStyle with the supplied font and color. The style
    /// also specifies no background, but this can be changed later.
    /// </summary>
    /// <param name="font"></param>
    /// <param name="fontColor"></param>
    public LabelStyle( BitmapFont font, Color? fontColor )
    {
        Font       = font;
        FontColor  = fontColor;
        Background = new BaseDrawable();
    }

    /// <summary>
    /// Creates a new LabelStyle with the same properties as the supplied style.
    /// </summary>
    public LabelStyle( LabelStyle? style )
    {
        Guard.Against.Null( style );

        Font       = style.Font;
        Background = style.Background;

        if ( style.FontColor != null )
        {
            FontColor = new Color( style.FontColor );
        }
    }
}

// ============================================================================
// ============================================================================