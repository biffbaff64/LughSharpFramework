// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Circa64 Software Projects / Richard Ikin.
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

using System;

using JetBrains.Annotations;

using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Maths;
using LughSharp.Core.SceneGraph2D.Utils;
using LughSharp.Core.Utils;

namespace LughSharp.Core.SceneGraph2D.UI;

/// <summary>
/// A button with a child <see cref="Scene2DImage"/> to display an image. This is useful when
/// the button must be larger than the image and the image centered on the button. If
/// the image is the size of the button, a <see cref="Button"/> without any children can be used,
/// where the <see cref="ButtonStyle.Up"/>, <see cref="ButtonStyle.Down"/>,
/// and <see cref="ButtonStyle.Checked"/> nine patches define the image.
/// </summary>
[PublicAPI]
public class ImageButton : Button
{
    public     Scene2DImage     Scene2DImage { get; }
    public new ImageButtonStyle Style        { get; set; } = null!;

    // ========================================================================

    /// <summary>
    /// Creates a new ImageButton using the supplied <see cref="Skin"/>. The
    /// skin should contain an <see cref="ImageButtonStyle"/>.
    /// </summary>
    /// <param name="skin"></param>
    public ImageButton( Skin skin )
        : this( skin.Get< ImageButtonStyle >() )
    {
        Skin = skin;
    }

    /// <summary>
    /// </summary>
    /// <param name="skin"></param>
    /// <param name="styleName"></param>
    public ImageButton( Skin skin, string styleName )
        : this( skin.Get< ImageButtonStyle >( styleName ) )
    {
        Skin = skin;
    }

    /// <summary>
    /// </summary>
    /// <param name="style"></param>
    public ImageButton( ImageButtonStyle style ) : base( style )
    {
        Scene2DImage = new Scene2DImage();
        Scene2DImage.SetScaling( Scaling.Fit );

        Add( Scene2DImage );

        SetStyle( style );
        SetSize( GetPrefWidthSafe(), GetPrefHeightSafe() );
    }

    /// <summary>
    /// </summary>
    /// <param name="imageUp"></param>
    /// <param name="imageDown"></param>
    /// <param name="imageChecked"></param>
    public ImageButton( ISceneDrawable? imageUp, ISceneDrawable? imageDown, ISceneDrawable? imageChecked )
        : this( new ImageButtonStyle( imageUp, imageDown, imageChecked ) )
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="style"></param>
    /// <exception cref="ArgumentException"></exception>
    public void SetStyle( ButtonStyle style )
    {
        Style      = new ImageButtonStyle( style );
        base.SetStyle< ButtonStyle >( style );

        UpdateImage();
    }

    /// <summary>
    /// Returns the appropriate image drawable from the style based on the current button state.
    /// </summary>
    protected ISceneDrawable? GetImageDrawable()
    {
        if ( IsDisabled && ( Style.ImageDisabled != null ) )
        {
            return Style.ImageDisabled;
        }

        if ( IsPressed() )
        {
            if ( IsChecked && ( Style.ImageCheckedDown != null ) )
            {
                return Style.ImageCheckedDown;
            }

            if ( Style.ImageDown != null )
            {
                return Style.ImageDown;
            }
        }

        if ( IsOver() )
        {
            if ( IsChecked )
            {
                if ( Style.ImageCheckedOver != null )
                {
                    return Style.ImageCheckedOver;
                }
            }
            else
            {
                if ( Style.ImageOver != null )
                {
                    return Style.ImageOver;
                }
            }
        }

        if ( IsChecked )
        {
            if ( Style.ImageChecked != null )
            {
                return Style.ImageChecked;
            }

            if ( IsOver() && ( Style.ImageOver != null ) )
            {
                return Style.ImageOver;
            }
        }

        return Style.ImageUp;
    }

    protected Scene2DImage? NewImage()
    {
        return new Scene2DImage( null, Scaling.Fit );
    }

    /// <summary>
    /// Sets the image drawable based on the current button state. The default implementation
    /// sets the image drawable using <see cref="GetImageDrawable()"/>.
    /// </summary>
    protected void UpdateImage()
    {
        ISceneDrawable? drawable = GetImageDrawable();

        if ( drawable != null )
        {
            Scene2DImage.SetDrawable( drawable );
        }
    }

    /// <summary>
    /// Draws the group and its children. The default implementation calls
    /// <see cref="Group.ApplyTransform(LughSharp.Core.Graphics.G2D.IBatch,Matrix4)"/> if
    ///  needed, then <see cref="Button.DrawChildren(IBatch, float)"/>, followed by
    /// <see cref="Button.ResetTransform(IBatch)"/> if needed.
    /// </summary>
    /// <param name="batch"> The <see cref="IBatch"/> </param>
    /// <param name="parentAlpha"></param>
    public override void Draw( IBatch batch, float parentAlpha )
    {
        UpdateImage();
        base.Draw( batch, parentAlpha );
    }

    public override string Name => GetType().Name;

    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }
}

// ============================================================================
// ============================================================================