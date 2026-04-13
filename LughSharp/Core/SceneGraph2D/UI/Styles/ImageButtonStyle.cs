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

using JetBrains.Annotations;

using LughSharp.Core.SceneGraph2D.Utils;
using LughSharp.Core.Utils.Logging;

namespace LughSharp.Core.SceneGraph2D.UI.Styles;

/// <summary>
/// The style for an image button.
/// </summary>
[PublicAPI]
public class ImageButtonStyle : ButtonStyle, ISceneStyle
{
    public ISceneDrawable? ImageChecked;
    public ISceneDrawable? ImageCheckedDown;
    public ISceneDrawable? ImageCheckedOver;
    public ISceneDrawable? ImageDisabled;
    public ISceneDrawable? ImageDown;
    public ISceneDrawable? ImageOver;
    public ISceneDrawable? ImageUp;

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
    public ImageButtonStyle( ISceneDrawable? up,
                             ISceneDrawable? down,
                             ISceneDrawable? chcked,
                             ISceneDrawable? imageUp,
                             ISceneDrawable? imageDown,
                             ISceneDrawable? imageChecked )
        : base( up, down, chcked )
    {
        ImageUp      = imageUp;
        ImageDown    = imageDown;
        ImageChecked = imageChecked;
    }

    /// <summary>
    /// Creates a new ImageButtonStyle instance, using the given <see cref="ImageButtonStyle"/>
    /// </summary>
    public ImageButtonStyle( ImageButtonStyle style )
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

    /// <summary>
    /// Creates a new ImageButtonStyle instance, using the supplied
    /// <see cref="ButtonStyle"/>.
    /// </summary>
    public ImageButtonStyle( ButtonStyle style )
        : base( style )
    {
    }

    public void Debug()
    {
        Logger.Debug( "ImageButtonStyle" );
        Logger.Debug( $"  ImageUp: {ImageUp}" );
        Logger.Debug( $"  ImageDown: {ImageDown}" );
        Logger.Debug( $"  ImageChecked: {ImageChecked}" );
        Logger.Debug( $"  ImageDisabled: {ImageDisabled}" );
        Logger.Debug( $"  ImageCheckedDown: {ImageCheckedDown}" );
        Logger.Debug( $"  ImageCheckedOver: {ImageCheckedOver}" );
        Logger.Debug( $"  ImageOver: {ImageOver}" );
        Logger.Debug( $"  Up: {Up}" );
        Logger.Debug( $"  Down: {Down}" );
        Logger.Debug( $"  Over: {Over}" );
        Logger.Debug( $"  Focused: {Focused}" );
        Logger.Debug( $"  Disabled: {Disabled}" );
        Logger.Debug( $"  Checked: {Checked}" );
        Logger.Debug( $"  CheckedDown: {CheckedDown}" );
        Logger.Debug( $"  CheckedOver: {CheckedOver}" );
        Logger.Debug( $"  CheckedFocused: {CheckedFocused}" );
        Logger.Debug( $"  CheckedOffsetX: {CheckedOffsetX}" );
        Logger.Debug( $"  CheckedOffsetY: {CheckedOffsetY}" );
        Logger.Debug( $"  PressedOffsetX: {PressedOffsetX}" );
        Logger.Debug( $"  PressedOffsetY: {PressedOffsetY}" );
        Logger.Debug( $"  UnpressedOffsetX: {UnpressedOffsetX}" );
        Logger.Debug( $"  UnpressedOffsetY: {UnpressedOffsetY}" );
    }
}

// ============================================================================
// ============================================================================