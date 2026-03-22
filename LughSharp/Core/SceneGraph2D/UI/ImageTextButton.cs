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

using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.SceneGraph2D.Utils;
using LughSharp.Core.Utils;

namespace LughSharp.Core.SceneGraph2D.UI;

[PublicAPI]
public class ImageTextButton : Button
{
    private readonly Scene2DImage?         _image;
    private          Label?                _label;
    private          ImageTextButtonStyle? _style;

    // ========================================================================
    
    public ImageTextButton( string text, Skin skin )
        : this( text, skin.Get< ImageTextButtonStyle >() )
    {
        Skin = skin;
    }

    public ImageTextButton( string text, Skin skin, string styleName )
        : this( text, skin.Get< ImageTextButtonStyle >( styleName ) )
    {
        Skin = skin;
    }

    /// <summary>
    /// </summary>
    /// <param name="text"></param>
    /// <param name="style"></param>
    public ImageTextButton( string text, ImageTextButtonStyle style )
        : base( style )
    {
        _style = style;

        CellDefaults.Space( 3 );

        _image = new Scene2DImage();
        _image.SetScaling( Scaling.Fit );
        Add( _image );

        _label = new Label( text, new LabelStyle( style.Font!, style.FontColor! ) );
        _label.SetAlignment( Align.Center );
        Add( _label );

        SetStyleAndSize( style );
    }

    private void SetStyleAndSize( ImageTextButtonStyle style )
    {
        SetStyle( style );
        SetSize( GetPrefWidth(), GetPrefHeight() );
    }

    public void SetStyle( ButtonStyle style )
    {
        if ( !( style is ImageTextButtonStyle textButtonStyle ) )
        {
            throw new ArgumentException( "style must be a ImageTextButtonStyle." );
        }

        _style = textButtonStyle;
        base.SetStyle< ImageTextButtonStyle >( textButtonStyle );

        if ( _image != null )
        {
            UpdateImage();
        }

        if ( _label != null )
        {
            LabelStyle labelStyle = _label.Style;

            labelStyle.Font      = textButtonStyle.Font!;
            labelStyle.FontColor = GetFontColor();
            _label.Style         = labelStyle;
        }
    }

    /// <summary>
    /// Returns the appropriate image drawable from the style based on the
    /// current button state.
    /// </summary>
    public virtual ISceneDrawable? GetImageDrawable()
    {
        if ( IsDisabled && ( _style?.ImageDisabled != null ) )
        {
            return _style.ImageDisabled;
        }

        if ( IsPressed() )
        {
            if ( IsChecked && ( _style?.ImageCheckedDown != null ) )
            {
                return _style.ImageCheckedDown;
            }

            if ( _style?.ImageDown != null )
            {
                return _style.ImageDown;
            }
        }

        if ( IsOver() )
        {
            if ( IsChecked )
            {
                if ( _style?.ImageCheckedOver != null )
                {
                    return _style.ImageCheckedOver;
                }
            }
            else
            {
                if ( _style?.ImageOver != null )
                {
                    return _style.ImageOver;
                }
            }
        }

        if ( IsChecked )
        {
            if ( _style?.ImageChecked != null )
            {
                return _style.ImageChecked;
            }

            if ( IsOver() && ( _style?.ImageOver != null ) )
            {
                return _style.ImageOver;
            }
        }

        return _style?.ImageUp;
    }

    /// <summary>
    /// Sets the image drawable based on the current button state. The default
    /// implementation sets the image drawable using <see cref="GetImageDrawable()"/>.
    /// </summary>
    public virtual void UpdateImage()
    {
        _image?.SetDrawable( GetImageDrawable() );
    }

    /// <summary>
    /// Returns the appropriate label font color from the style based on the current button state.
    /// </summary>
    protected Color GetFontColor()
    {
        if ( IsDisabled && ( _style?.DisabledFontColor != null ) )
        {
            return _style.DisabledFontColor;
        }

        if ( IsPressed() )
        {
            if ( IsChecked && ( _style?.CheckedDownFontColor != null ) )
            {
                return _style.CheckedDownFontColor;
            }

            if ( _style?.DownFontColor != null )
            {
                return _style.DownFontColor;
            }
        }

        if ( IsOver() )
        {
            if ( IsChecked )
            {
                if ( _style?.CheckedOverFontColor != null )
                {
                    return _style.CheckedOverFontColor;
                }
            }
            else
            {
                if ( _style?.OverFontColor != null )
                {
                    return _style.OverFontColor;
                }
            }
        }

        bool focused = HasKeyboardFocus();

        if ( IsChecked )
        {
            if ( focused && ( _style?.CheckedFocusedFontColor != null ) )
            {
                return _style.CheckedFocusedFontColor;
            }

            if ( _style?.CheckedFontColor != null )
            {
                return _style.CheckedFontColor;
            }

            if ( IsOver() && ( _style?.OverFontColor != null ) )
            {
                return _style.OverFontColor;
            }
        }

        if ( focused && ( _style?.FocusedFontColor != null ) )
        {
            return _style.FocusedFontColor;
        }

        return _style?.FontColor ?? Color.White;
    }

    /// <inheritdoc />
    public override void Draw( IBatch batch, float parentAlpha )
    {
        UpdateImage();

        if ( _label != null )
        {
            _label.Style.FontColor = GetFontColor();
        }

        base.Draw( batch, parentAlpha );
    }

    public Scene2DImage? GetImage()
    {
        return _image;
    }

    public Label? GetLabel()
    {
        return _label;
    }

    public Cell? GetImageCell()
    {
        return GetCell( _image! );
    }

    public Cell? GetLabelCell()
    {
        return GetCell( _label! );
    }

    public void SetLabel( Label label )
    {
        GetLabelCell()!.Actor = label;
        _label                = label;
    }

    public string GetText()
    {
        return _label?.GetText().ToString() ?? "";
    }

    public void SetText( string text )
    {
        if ( _label != null )
        {
            _label.GetText().Clear();
            _label.GetText().Append( text );
        }
    }
}

// ============================================================================
// ============================================================================

