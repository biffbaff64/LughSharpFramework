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

using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.BitmapFonts;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.SceneGraph2D.Utils;

using Newtonsoft.Json;

namespace LughSharp.Core.SceneGraph2D.UI.Styles;

/// <summary>
/// The style for a window, see <see cref="Window"/>.
/// </summary>
[PublicAPI]
public class WindowStyle : ISceneStyle
{
    public ISceneDrawable? Background      { get; set; }
    public BitmapFont?     TitleFont       { get; set; }
    public Color?          TitleFontColor  { get; set; } = new( 1, 1, 1, 1 );
    public ISceneDrawable? StageBackground { get; set; }

    // ====================================================================

    /// <summary>
    /// Creates a new empty WindowStyle instance. Before using this style it will be
    /// necessary to set the <see cref="Background"/>, <see cref="TitleFont"/> properties.
    /// The <see cref="TitleFontColor"/>defaults to white, but can be changed to any color.
    /// </summary>
    public WindowStyle()
    {
    }

    /// <summary>
    /// Creates a new WindowStyle instance with the specified parameters for
    /// <see cref="TitleFont"/>, <see cref="TitleFontColor"/>, and <see cref="Background"/>.
    /// </summary>
    /// <param name="titleFont"></param>
    /// <param name="titleFontColor"></param>
    /// <param name="background"></param>
    [JsonConstructor]
    public WindowStyle( BitmapFont titleFont, Color titleFontColor, ISceneDrawable? background )
    {
        TitleFont  = titleFont;
        Background = background;

        TitleFontColor?.Set( titleFontColor );
    }

    /// <summary>
    /// Copies the specified style into this style.
    /// </summary>
    /// <param name="style"> The style to copy from. </param>
    public WindowStyle( WindowStyle style )
    {
        Background = style.Background;
        TitleFont  = style.TitleFont;

        if ( style.TitleFontColor != null )
        {
            TitleFontColor = new Color( style.TitleFontColor );
        }

        Background = style.Background;
    }
}

// ============================================================================
// ============================================================================

