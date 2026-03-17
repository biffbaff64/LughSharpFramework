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
using LughSharp.Core.Main;
using LughSharp.Core.SceneGraph2D.Listeners;
using LughSharp.Core.SceneGraph2D.Styles;
using LughSharp.Core.SceneGraph2D.Utils;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Pooling;

namespace LughSharp.Core.SceneGraph2D.UI;

/// <summary>
/// A button is a <see cref="Table"/> with a checked state and additional
/// <see cref="ButtonStyle"/> style fields for pressed, unpressed, and
/// checked. Each time a button is clicked, the checked state is toggled.
/// Being a table, a button can contain any other actors.
/// <para>
/// The button's padding is set to the background drawable's padding when
/// the background changes, overwriting any padding set manually. Padding
/// can still be set on the button's table cells.
/// </para>
/// <para>
/// A <see cref="ChangeListener.ChangeEvent"/> is fired when the button is
/// clicked. Cancelling the event will restore the checked button state to
/// what it was previously.
/// </para>
/// <para>
/// The preferred size of the button is determined by the background and the
/// button contents.
/// </para>
/// </summary>
[PublicAPI]
public class Button : Table, IDisableable
{
    public ButtonClickListener?   ClickListener { get; set; }
    public bool                   IsChecked     { get; private set; }
    public bool                   IsDisabled    { get; set; }
    public ButtonGroup< Button >? ButtonGroup   { get; set; }

    private bool        _programmaticChangeEvents = true;
    private ButtonStyle _style                     = null!;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates a button without setting the style or size.
    /// At least a style must be set before using this button.
    /// </summary>
    public Button()
    {
        Initialise();
    }

    public Button( Skin skin ) : base( skin )
    {
        Initialise();

        SetStyle( skin.Get< ButtonStyle >() );
        SetSize( GetPrefWidthSafe(), GetPrefHeightSafe() );
    }

    public Button( Skin skin, string styleName )
        : base( skin )
    {
        Initialise();

        SetStyle( skin.Get< ButtonStyle >( styleName ) );
        SetSize( GetPrefWidthSafe(), GetPrefHeightSafe() );
    }

    public Button( Actor child, Skin skin, string styleName )
        : this( child, skin.Get< ButtonStyle >( styleName ) )
    {
        Skin = skin;
    }

    public Button( Actor child, ButtonStyle style )
    {
        Initialise();
        Add( child );

        SetStyle( style );
        SetSize( GetPrefWidthSafe(), GetPrefHeightSafe() );
    }

    public Button( ButtonStyle style )
    {
        Initialise();

        _style = style;
        SetSize( GetPrefWidthSafe(), GetPrefHeightSafe() );
    }

    public Button( ISceneDrawable? up )
        : this( new ButtonStyle( up, null, null ) )
    {
    }

    public Button( ISceneDrawable? up, ISceneDrawable? down )
        : this( new ButtonStyle( up, down, null ) )
    {
    }

    public Button( ISceneDrawable? upImage, ISceneDrawable? downImage, ISceneDrawable? checkedImage )
        : this( new ButtonStyle( upImage, downImage, checkedImage ) )
    {
    }

    public Button( Actor child, Skin skin )
        : this( child, skin.Get< ButtonStyle >() )
    {
    }

    private void Initialise()
    {
        Touchable = Touchable.Enabled;

        ClickListener = new ButtonClickListener( this );

        AddListener( ClickListener! );
    }

    public void SetStyle< TStyle >( TStyle? style )
    {
        if ( style == null )
        {
            return;
        }

        _style = ( style as ButtonStyle )!;
        
        SetBackground( GetBackgroundDrawable() );
    }

    public ButtonStyle GetStyle()
    {
        return _style;
    }
    
//    /// <summary>
//    /// Returns the button's style. Modifying the returned style may not have an
//    /// effect until <see cref="Style"/> set() is called.
//    /// </summary>
//    public ButtonStyle Style
//    {
//        get => _style;
//        set
//        {
//            _style = value ?? throw new ArgumentException( "style cannot be null." );
//
//            SetBackground( GetBackgroundDrawable() );
//        }
//    }

    public void SetChecked( bool isChecked )
    {
        SetChecked( isChecked, _programmaticChangeEvents );
    }

    public void SetChecked( bool isChecked, bool fireEvent )
    {
        if ( IsChecked == isChecked )
        {
            return;
        }

        if ( ( ButtonGroup != null ) && !ButtonGroup.CanCheck( this, isChecked ) )
        {
            return;
        }

        IsChecked = isChecked;

        if ( fireEvent )
        {
            var changeEvent = Pools.Obtain< ChangeListener.ChangeEvent >();

            if ( changeEvent is not null )
            {
                if ( Fire( changeEvent ) )
                {
                    IsChecked = !isChecked;
                }

                Pools.Free< ChangeListener.ChangeEvent >( changeEvent );
            }
        }
    }

    public override float GetPrefWidth()
    {
        return GetPrefWidthSafe();
    }

    protected float GetPrefWidthSafe()
    {
        float width = base.GetPrefWidth();

        if ( _style.Up != null )
        {
            width = Math.Max( width, _style.Up.MinWidth );
        }

        if ( _style.Down != null )
        {
            width = Math.Max( width, _style.Down.MinWidth );
        }

        if ( _style.Checked != null )
        {
            width = Math.Max( width, _style.Checked.MinWidth );
        }

        return width;
    }

    public override float GetPrefHeight()
    {
        return GetPrefHeightSafe();
    }

    public float GetPrefHeightSafe()
    {
        float height = base.GetPrefHeight();

        if ( _style.Up != null )
        {
            height = Math.Max( height, _style.Up.MinHeight );
        }

        if ( _style.Down != null )
        {
            height = Math.Max( height, _style.Down.MinHeight );
        }

        if ( _style.Checked != null )
        {
            height = Math.Max( height, _style.Checked!.MinHeight );
        }

        return height;
    }

    /// <summary>
    /// If false, <see cref="SetChecked(bool)"/> and <see cref="ToggleChecked"/> will
    /// not fire <see cref="ChangeListener.ChangeEvent()"/>. The event will only be
    /// fired when the user clicks the button
    /// </summary>
    public void SetProgrammaticChangeEvents( bool programmaticChangeEvents )
    {
        _programmaticChangeEvents = programmaticChangeEvents;
    }

    /// <summary>
    /// Returns appropriate background drawable from the style based on the current button state.
    /// </summary>
    public virtual ISceneDrawable? GetBackgroundDrawable()
    {
        if ( IsDisabled && ( _style?.Disabled != null ) )
        {
            return _style.Disabled;
        }

        if ( IsPressed() )
        {
            if ( IsChecked && ( _style?.CheckedDown != null ) )
            {
                return _style.CheckedDown;
            }

            if ( _style?.Down != null )
            {
                return _style.Down;
            }
        }

        if ( IsOver() )
        {
            if ( IsChecked )
            {
                if ( _style?.CheckedOver != null )
                {
                    return _style.CheckedOver;
                }
            }
            else
            {
                if ( _style?.Over != null )
                {
                    return _style.Over;
                }
            }
        }

        bool focused = HasKeyboardFocus();

        if ( IsChecked )
        {
            if ( focused && ( _style?.CheckedFocused != null ) )
            {
                return _style.CheckedFocused;
            }

            if ( _style?.Checked != null )
            {
                return _style.Checked;
            }

            if ( IsOver() && ( _style?.Over != null ) )
            {
                return _style.Over;
            }
        }

        if ( focused && ( _style?.Focused != null ) )
        {
            return _style.Focused;
        }

        return _style?.Up;
    }

    /// <inheritdoc />
    public override void Draw( IBatch batch, float parentAlpha )
    {
        Validate();

        SetBackground( GetBackgroundDrawable() );

        float offsetX;
        float offsetY;

        Guard.Against.Null( _style );

        if ( IsPressed() && !IsDisabled )
        {
            offsetX = _style.PressedOffsetX;
            offsetY = _style.PressedOffsetY;
        }
        else if ( IsChecked && !IsDisabled )
        {
            offsetX = _style.CheckedOffsetX;
            offsetY = _style.CheckedOffsetY;
        }
        else
        {
            offsetX = _style.UnpressedOffsetX;
            offsetY = _style.UnpressedOffsetY;
        }

        bool offset = ( offsetX != 0 ) || ( offsetY != 0 );

        if ( offset )
        {
            foreach ( Actor? actor in Children )
            {
                actor?.MoveBy( offsetX, offsetY );
            }
        }

        base.Draw( batch, parentAlpha );

        if ( offset )
        {
            for ( var i = 0; i < Children.Size; i++ )
            {
                Children.GetAt( i )?.MoveBy( -offsetX, -offsetY );
            }
        }

        if ( Stage is { ActionsRequestRendering: true }
          && ( IsPressed() != ClickListener?.Pressed ) )
        {
            Engine.Graphics.RequestRendering();
        }
    }

    public void ToggleChecked()
    {
        SetChecked( !IsChecked );
    }

    public bool IsPressed()
    {
        return ClickListener!.VisualPressed;
    }

    public override string? Name => GetType().Name;
    
    public bool IsOver()
    {
        return ClickListener!.Over;
    }
}

// ============================================================================
// ============================================================================