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

using LughSharp.Source.Graphics.G2D;
using LughSharp.Source.Scene2D.UI.Styles;
using LughSharp.Source.Scene2D.Utils;

namespace LughSharp.Source.Scene2D.UI;

/// <summary>
/// A button with a child <see cref="Scene2DImage"/> to display an image. This is
/// useful when the button must be larger than the image and the image centered on
/// the button. If the image is the size of the button, a <see cref="Button"/>
/// without any children can be used, where the <see cref="ButtonStyle.Up"/>,
/// <see cref="ButtonStyle.Down"/>, and <see cref="ButtonStyle.Checked"/> nine
/// patches define the image.
/// </summary>
[PublicAPI]
[ActorDefinition( Role = "UI" )]
public class ImageButton : Button, IStyleable< ImageButtonStyle >
{
    public Scene2DImage Scene2DImage { get; }

    // ========================================================================

    private ImageButtonStyle _style = null!;

    // ========================================================================

    /// <summary>
    /// Creates a new ImageButton using the supplied <see cref="Skin"/>. The skin
    /// should contain an <see cref="ImageButtonStyle"/>.
    /// </summary>
    /// <param name="skin"> The skin holding the <see cref="ImageButtonStyle"/>. </param>
    public ImageButton( Skin skin ) : this( skin.Get< ImageButtonStyle >() )
    {
        Skin = skin;
    }

    /// <summary>
    /// Creates a new ImageButton using the supplied <see cref="Skin"/>. The skin
    /// should contain an <see cref="ImageButtonStyle"/> with the specified name.
    /// </summary>
    /// <param name="skin"> The skin holding the <see cref="ImageButtonStyle"/>. </param>
    /// <param name="styleName"> The name of the style to use. </param>
    public ImageButton( Skin skin, string styleName )
        : this( skin.Get< ImageButtonStyle >( styleName ) )
    {
        Skin = skin;
    }

    /// <summary>
    /// Creates a new ImageButton using the supplied <see cref="ImageButtonStyle"/>.
    /// It does so by creating a new <see cref="Scene2DImage"/> drawable instance, and
    /// adding that to this button. This drawable will be updated, according to
    /// the button's state, with the correct image in the call to <see cref="SetStyle"/>.
    /// </summary>
    /// <param name="style"> The style to use. </param>
    public ImageButton( ImageButtonStyle style ) : base( style )
    {
        Scene2DImage = new Scene2DImage();
        Scene2DImage.SetScaling( Scaling.Fit );

        AddCell( Scene2DImage );

        // The Scene2DImage drawable will be updated by the style.
        SetStyle( style );
        SetSize( GetPrefWidthUnchecked(), GetPrefHeightUnchecked() );
    }

    /// <summary>
    /// Creates a new ImageButton using the supplied <see cref="ISceneDrawable"/> instances
    /// for the image up, down, and checked states. These images will be used to create a new
    /// <see cref="ImageButtonStyle"/> instance, and the button will be created from that.
    /// </summary>
    /// <param name="imageUp"> The drawable to use for the up state. </param>
    /// <param name="imageDown"> The drawable to use for the down state. </param>
    /// <param name="imageChecked"> The drawable to use for the checked state. </param>
    public ImageButton( ISceneDrawable? imageUp,
                        ISceneDrawable? imageDown,
                        ISceneDrawable? imageChecked )
        : this( new ImageButtonStyle( imageUp, imageDown, imageChecked ) )
    {
    }

    /// <summary>
    /// Returns the buttons style.
    /// </summary>
    /// <returns> The buttons style. </returns>
    public override ImageButtonStyle GetStyle() => _style;

    /// <summary>
    /// Sets the style for the <see cref="ImageButton"/> by updating its appearance
    /// and the associated drawable based on the supplied <see cref="ImageButtonStyle"/>.
    /// </summary>
    /// <param name="style">The style to apply to the image button.</param>
    /// <exception cref="ArgumentException">Thrown if the provided style is invalid or null.</exception>
    public void SetStyle( ImageButtonStyle style )
    {
        _style = style;

        base.SetStyle< ButtonStyle >( style );

        UpdateImage();
    }

    /// <summary>
    /// Updates the image drawable based on the current button state. The default implementation
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
    /// Returns the appropriate image drawable from the style based on the current button state.
    /// </summary>
    protected ISceneDrawable? GetImageDrawable()
    {
        if ( IsDisabled && ( _style.ImageDisabled != null ) )
        {
            return _style.ImageDisabled;
        }

        if ( IsPressed )
        {
            if ( IsChecked && ( _style.ImageCheckedDown != null ) )
            {
                return _style.ImageCheckedDown;
            }

            if ( _style.ImageDown != null )
            {
                return _style.ImageDown;
            }
        }

        if ( IsOver )
        {
            if ( IsChecked )
            {
                if ( _style.ImageCheckedOver != null )
                {
                    return _style.ImageCheckedOver;
                }
            }
            else
            {
                if ( _style.ImageOver != null )
                {
                    return _style.ImageOver;
                }
            }
        }

        if ( IsChecked )
        {
            if ( _style.ImageChecked != null )
            {
                return _style.ImageChecked;
            }

            if ( IsOver && ( _style.ImageOver != null ) )
            {
                return _style.ImageOver;
            }
        }

        return _style.ImageUp;
    }

    /// <summary>
    /// Creates a new, empty, <see cref="Scene2DImage"/> drawable instance, with
    /// scaling set to <see cref="Scaling.Fit"/>.
    /// </summary>
    /// <returns> A new <see cref="Scene2DImage"/> drawable instance. </returns>
    protected Scene2DImage NewImage()
    {
        return new Scene2DImage( null, Scaling.Fit );
    }

    /// <summary>
    /// Draws the group and its children. The default implementation calls
    /// <see cref="Group.ApplyTransform(LughSharp.Source.Graphics.G2D.IBatch,Matrix4)"/> if
    ///  needed, then <see cref="Button.DrawChildren(IBatch, float)"/>, followed by
    /// <see cref="Button.ResetTransform(IBatch)"/> if needed.
    /// </summary>
    /// <param name="batch"> The <see cref="IBatch"/> </param>
    /// <param name="parentAlpha"> The alpha value of the parent. </param>
    public override void Draw( IBatch batch, float parentAlpha )
    {
        UpdateImage();
        base.Draw( batch, parentAlpha );
    }
}

// ============================================================================
// ============================================================================