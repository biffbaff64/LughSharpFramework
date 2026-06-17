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

using Newtonsoft.Json;

namespace LughSharp.Source.Scene2D.UI.Styles;

/// <summary>
/// The style for a window, see <see cref="Window"/>.
/// </summary>
[PublicAPI]
public class WindowStyle : ISceneStyle
{
    /// <summary>
    /// The <see cref="BitmapFont"/> used for the window title.
    /// </summary>
    public BitmapFont? TitleFont { get; set; }

    /// <summary>
    /// The color of the font used in the Title.
    /// </summary>
    public Color TitleFontColor { get; set; }

    /// <summary>
    /// The <see cref="ISceneDrawable"/> used to render the background of the window.
    /// It defines the visual look and padding of the window's background.
    /// </summary>
    public ISceneDrawable? Background { get; set; }

    /// <summary>
    /// The <see cref="ISceneDrawable"/> representing the background for the stage area in the window.
    /// </summary>
    public ISceneDrawable? StageBackground { get; set; }

    // ====================================================================

    /// <summary>
    /// Creates a new empty WindowStyle instance. Before using this style it will be
    /// necessary to set the <see cref="Background"/>, <see cref="TitleFont"/> properties.
    /// The <see cref="TitleFontColor"/>defaults to white, but can be changed to any color.
    /// </summary>
    public WindowStyle()
    {
        TitleFontColor = Color.White;
    }

    /// <summary>
    /// Creates a new WindowStyle instance with the specified parameters for
    /// <see cref="TitleFont"/>, <see cref="TitleFontColor"/>, and <see cref="Background"/>.
    /// </summary>
    /// <param name="titleFont"></param>
    /// <param name="titleFontColor"></param>
    /// <param name="background"></param>
    [JsonConstructor]
    public WindowStyle( BitmapFont titleFont, Color titleFontColor, ISceneDrawable background )
    {
        TitleFont       = titleFont;
        TitleFontColor  = titleFontColor;
        Background      = background;
        StageBackground = new BaseDrawable();
    }

    /// <summary>
    /// Copies the specified style into this style.
    /// </summary>
    /// <param name="style"> The style to copy from. </param>
    public WindowStyle( WindowStyle style )
    {
        Background      = style.Background;
        TitleFont       = style.TitleFont;
        TitleFontColor  = style.TitleFontColor;
        StageBackground = style.StageBackground;
    }
}

// ============================================================================
// ============================================================================