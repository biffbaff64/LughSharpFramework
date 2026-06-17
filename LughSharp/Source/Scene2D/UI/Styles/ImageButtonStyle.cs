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
/// The style for an image button.
/// </summary>
[PublicAPI]
public class ImageButtonStyle : ButtonStyle, ISceneStyle
{
    /// <summary>
    /// The button image for when the button is in its normal, unpressed, state.
    /// </summary>
    public ISceneDrawable? ImageUp;
    
    /// <summary>
    /// The button image for when the button is in its pressed, held down, state.
    /// </summary>
    public ISceneDrawable? ImageDown;
    
    /// <summary>
    /// The button image for when the mouse or touch is over the button.
    /// </summary>
    public ISceneDrawable? ImageOver;
    
    /// <summary>
    /// The button image for when the button is disabled.
    /// </summary>
    public ISceneDrawable? ImageDisabled;
    
    /// <summary>
    /// The button image for when the button is in its checked, toggled, state.
    /// </summary>
    public ISceneDrawable? ImageChecked;
    
    /// <summary>
    /// The button image for when the button is in its checked, toggled, state, and is
    /// pressed down.
    /// </summary>
    public ISceneDrawable? ImageCheckedDown;
    
    /// <summary>
    /// The button image for when the mouse or touch is over the button, and the button
    /// is in its checked, toggled, state.
    /// </summary>
    public ISceneDrawable? ImageCheckedOver;

    // ========================================================================

    /// <summary>
    /// Creates a new, unitialised, ImageButtonStyle instance.
    /// </summary>
    public ImageButtonStyle()
    {
    }

    /// <summary>
    /// Creates a new ImageButtonStyle instance, using the supplied <see cref="ISceneDrawable"/>
    /// images for <see cref="ImageUp"/>, <see cref="ImageDown"/> and <see cref="ImageChecked"/>.
    /// The provided ISceneDrawables cannot be <c>null</c>.
    /// </summary>
    public ImageButtonStyle( ISceneDrawable? imageUp, ISceneDrawable? imageDown, ISceneDrawable? imageChecked )
        : base( imageUp, imageDown, imageChecked )
    {
        ImageUp      = imageUp;
        ImageDown    = imageDown;
        ImageChecked = imageChecked;
    }

    /// <summary>
    /// Creates a new ImageButtonStyle instance, using the supplied <see cref="ISceneDrawable"/>
    /// images for <see cref="ImageUp"/>, <see cref="ImageDown"/> and <see cref="ImageChecked"/>.
    /// </summary>
    public ImageButtonStyle( ISceneDrawable up,
                             ISceneDrawable down,
                             ISceneDrawable chcked,
                             ISceneDrawable imageUp,
                             ISceneDrawable imageDown,
                             ISceneDrawable imageChecked ) : base( up, down, chcked )
    {
        ImageUp      = imageUp;
        ImageDown    = imageDown;
        ImageChecked = imageChecked;
    }

    /// <summary>
    /// Creates a new ImageButtonStyle instance, using the given <see cref="ImageButtonStyle"/>
    /// </summary>
    public ImageButtonStyle( ImageButtonStyle style ) : base( style )
    {
        ImageUp          = style.ImageUp;
        ImageDown        = style.ImageDown;
        ImageOver        = style.ImageOver;
        ImageDisabled    = style.ImageDisabled;
        ImageChecked     = style.ImageChecked;
        ImageCheckedDown = style.ImageCheckedDown;
        ImageCheckedOver = style.ImageCheckedOver;
    }

    /// <summary>
    /// Creates a new ImageButtonStyle instance, using the supplied <see cref="ButtonStyle"/>.
    /// </summary>
    public ImageButtonStyle( ButtonStyle style )
        : base( style )
    {
    }
}

// ============================================================================
// ============================================================================