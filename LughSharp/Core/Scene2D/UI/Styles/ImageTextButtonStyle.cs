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

using LughSharp.Core.Graphics.Fonts;
using LughSharp.Core.Scene2D.Utils;

namespace LughSharp.Core.Scene2D.UI.Styles;

/// <summary>
/// The style for an image text button, see <see cref="ImageTextButton"/>.
/// </summary>
[PublicAPI]
public class ImageTextButtonStyle : TextButtonStyle, ISceneStyle
{
    public ISceneDrawable? ImageUp          { get; set; }
    public ISceneDrawable? ImageDown        { get; set; }
    public ISceneDrawable? ImageOver        { get; set; }
    public ISceneDrawable? ImageDisabled    { get; set; }
    public ISceneDrawable? ImageChecked     { get; set; }
    public ISceneDrawable? ImageCheckedDown { get; set; }
    public ISceneDrawable? ImageCheckedOver { get; set; }
    
    // ========================================================================
    
    public ImageTextButtonStyle()
    {
    }

    public ImageTextButtonStyle( ISceneDrawable up, ISceneDrawable down, ISceneDrawable chcked, BitmapFont font )
        : base( up, down, chcked, font )
    {
    }

    public ImageTextButtonStyle( ImageTextButtonStyle style )
        : base( style )

    {
        ImageUp          = style.ImageUp;
        ImageDown        = style.ImageDown;
        ImageOver        = style.ImageOver;
        ImageDisabled    = style.ImageDisabled;
        ImageChecked     = style.ImageChecked;
        ImageCheckedDown = style.ImageCheckedDown;
        ImageCheckedOver = style.ImageCheckedOver;
    }

    public ImageTextButtonStyle( TextButtonStyle style )
        : base( style )
    {
    }
}

// ============================================================================
// ============================================================================

