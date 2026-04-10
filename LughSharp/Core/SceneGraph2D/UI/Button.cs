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
using LughSharp.Core.SceneGraph2D.Listeners;
using LughSharp.Core.SceneGraph2D.UI.Styles;
using LughSharp.Core.SceneGraph2D.Utils;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;
using LughSharp.Core.Utils.Pooling;

namespace LughSharp.Core.SceneGraph2D.UI;

/// <summary>
/// A button is a <see cref="Table"/> with a checked state and additional
/// <see cref="ButtonStyle"/> style fields for pressed, unpressed, and checked.
/// Each time a button is clicked, the checked state is toggled. Being a table,
/// a button can contain any other actors.
/// <para>
/// The button's padding is set to the background drawable's padding when the
/// 0background changes, overwriting any padding set manually. Padding can still
/// be set on the button's table cells.
/// </para>
/// <para>
/// A <see cref="ChangeListener.ChangeEvent"/> is fired when the button is clicked.
/// Cancelling the event will restore the checked button state to what it was previously.
/// </para>
/// <para>
/// The preferred size of the button is determined by the background and the
/// button contents.
/// </para>
/// </summary>
[PublicAPI]
[ActorDefinition( Role = "UI" )]
public class Button : Table, IDisableable
{
    public ButtonClickListener?   ClickListener { get; set; }
    public bool                   IsChecked     { get; private set; }
    public bool                   IsDisabled    { get; set; }
    public ButtonGroup< Button >? ButtonGroup   { get; set; }

    // ========================================================================

    private bool        _programmaticChangeEvents = true;
    private ButtonStyle _style                    = new();

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates a button without setting the style or size. Before this button can
    /// be used, it is important that at least a <see cref="ButtonStyle"/> is created
    /// and set.
    /// </summary>
    public Button()
    {
        SetClickListener();
    }

    /// <summary>
    /// Creates a button with using the <see cref="ButtonStyle"/> from the
    /// supplied <see cref="Skin"/>.
    /// </summary>
    public Button( Skin skin ) : base( skin )
    {
        SetStyle( skin.Get< ButtonStyle >() );
        SetSize( GetPrefWidthSafe(), GetPrefHeightSafe() );

        SetClickListener();
    }

    /// <summary>
    /// Creates a button with using the named <see cref="ButtonStyle"/> from the
    /// supplied <see cref="Skin"/>.
    /// </summary>
    public Button( Skin skin, string styleName ) : base( skin )
    {
        var style = skin.Get< ButtonStyle >( styleName );

        Guard.Against.Null( style, $"Skin does not contain a ButtonStyle named {styleName}" );

        SetStyle( style );
        SetSize( GetPrefWidthSafe(), GetPrefHeightSafe() );

        SetClickListener();
    }

    /// <summary>
    /// Creates a button with using the named <see cref="ButtonStyle"/> from the
    /// supplied <see cref="Skin"/>, and also adding the supplied child.
    /// </summary>
    /// <param name="child"></param>
    /// <param name="skin"></param>
    /// <param name="styleName"></param>
    public Button( Actor child, Skin skin, string styleName )
        : this( child, skin.Get< ButtonStyle >( styleName ) )
    {
        Skin = skin;
    }

    /// <summary>
    /// Creates a button with using the supplied <see cref="ButtonStyle"/>, and
    /// also adding the supplied child.
    /// </summary>
    public Button( Actor child, ButtonStyle style )
    {
        SetStyle( style );
        SetSize( GetPrefWidthSafe(), GetPrefHeightSafe() );

        SetClickListener();
        Add( child );
    }

    /// <summary>
    /// Creates a button with using the supplied <see cref="ButtonStyle"/>.
    /// </summary>
    /// <param name="style"></param>
    public Button( ButtonStyle style )
    {
        _style = style;
        SetSize( GetPrefWidthSafe(), GetPrefHeightSafe() );

        SetClickListener();
    }

    /// <summary>
    /// Creates a button with using the supplied <see cref="ISceneDrawable"/>
    /// images for the button's up state. The down and checked states will be
    /// set to <c>null</c>, and can be updated later. These images will be used
    /// to create a new <see cref="ButtonStyle"/> instance, and the button will
    /// be created from that.
    /// </summary>
    public Button( ISceneDrawable? up )
        : this( new ButtonStyle( up, null, null ) )
    {
    }

    /// <summary>
    /// Creates a button with using the supplied <see cref="ISceneDrawable"/>
    /// images for the button's up and down states. The checked state will be
    /// set to <c>null</c>, and can be updated later. These images will be used
    /// to create a new <see cref="ButtonStyle"/> instance, and the button will
    /// be created from that.
    /// </summary>
    public Button( ISceneDrawable? up, ISceneDrawable? down )
        : this( new ButtonStyle( up, down, null ) )
    {
    }

    /// <summary>
    /// Creates a button with using the supplied <see cref="ISceneDrawable"/>
    /// images for the button's up, down, and checked states. These images will
    /// be used to create a new <see cref="ButtonStyle"/> instance, and the
    /// button will be created from that.
    /// </summary>
    public Button( ISceneDrawable? upImage, ISceneDrawable? downImage, ISceneDrawable? checkedImage )
        : this( new ButtonStyle( upImage, downImage, checkedImage ) )
    {
    }

    /// <summary>
    /// Creates a button with using the <see cref="ButtonStyle"/> from the
    /// supplied <see cref="Skin"/>, and also adding the supplied child.
    /// </summary>
    public Button( Actor child, Skin skin )
        : this( child, skin.Get< ButtonStyle >() )
    {
    }

    /// <summary>
    /// Enables the button to be clicked, setting the touchable to <see cref="Touchable.Enabled"/>,
    /// and adding a <see cref="ButtonClickListener"/> to the button.
    /// </summary>
    private void SetClickListener()
    {
        Touchable = Touchable.Enabled;

        ClickListener = new ButtonClickListener( this );

        AddListener( ClickListener! );
    }

    /// <summary>
    /// Sets the button's style.
    /// </summary>
    /// <param name="style">
    /// The style, which could be <see cref="ButtonStyle"/>, or any style that
    /// inherits from ButtonStyle.
    /// </param>
    /// <typeparam name="TStyle"></typeparam>
    public void SetStyle< TStyle >( TStyle? style ) where TStyle : ButtonStyle
    {
        if ( style == null )
        {
            return;
        }

        _style.Set< TStyle >( style );

        SetBackground( GetBackgroundDrawable() );
    }

    /// <summary>
    /// Returns the buttons style.
    /// </summary>
    public ButtonStyle GetStyle()
    {
        return _style;
    }

    /// <summary>
    /// Convenience method which toggles the button's checked state.
    /// </summary>
    public void ToggleChecked( bool fireEvent = false )
    {
        SetChecked( !IsChecked, fireEvent );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isChecked"></param>
    public void SetChecked( bool isChecked )
    {
        SetChecked( isChecked, _programmaticChangeEvents );
    }

    /// <summary>
    /// Sets the button's checked state, and optionally fires a
    /// <see cref="ChangeListener.ChangeEvent"/> if the checked state changes.
    /// </summary>
    /// <param name="isChecked"></param>
    /// <param name="fireEvent"></param>
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

            if ( Fire( changeEvent ) )
            {
                IsChecked = !isChecked;
            }

            Pools.Free< ChangeListener.ChangeEvent >( changeEvent );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override float GetPrefWidth()
    {
        return GetPrefWidthSafe();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override float GetPrefHeight()
    {
        return GetPrefHeightSafe();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
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
        if ( IsDisabled && ( _style.Disabled != null ) )
        {
            return _style.Disabled;
        }

        if ( IsPressed )
        {
            if ( IsChecked && ( _style.CheckedDown != null ) )
            {
                return _style.CheckedDown;
            }

            if ( _style.Down != null )
            {
                return _style.Down;
            }
        }

        if ( IsOver() )
        {
            if ( IsChecked )
            {
                if ( _style.CheckedOver != null )
                {
                    return _style.CheckedOver;
                }
            }
            else
            {
                if ( _style.Over != null )
                {
                    return _style.Over;
                }
            }
        }

        bool focused = HasKeyboardFocus();

        if ( IsChecked )
        {
            if ( focused && ( _style.CheckedFocused != null ) )
            {
                return _style.CheckedFocused;
            }

            if ( _style.Checked != null )
            {
                return _style.Checked;
            }

            if ( IsOver() && ( _style.Over != null ) )
            {
                return _style.Over;
            }
        }

        if ( focused && ( _style.Focused != null ) )
        {
            return _style.Focused;
        }

        return _style.Up;
    }

    /// <inheritdoc />
    public override void Draw( IBatch batch, float parentAlpha )
    {
        Validate();

        SetBackground( GetBackgroundDrawable() );

        float offsetX;
        float offsetY;

        Guard.Against.Null( _style );

        // Establish drawing offsets for any children of this button.
        if ( IsPressed && !IsDisabled )
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
          && ( IsPressed != ClickListener?.Pressed ) )
        {
            Engine.Graphics.RequestRendering();
        }
    }

    /// <summary>
    /// Convenience method which returns <c>true</c> if the button is pressed,
    /// otherwise <c>false</c>.
    /// </summary>
    public bool IsPressed => ClickListener?.VisualPressed ?? false;

    /// <summary>
    /// Convenience method which returns <c>true</c> if the mouse or touch is
    /// over the button or pressed and within the tap square.
    /// </summary>
    public bool IsOver()
    {
        return ClickListener?.Over ?? false;
    }
}

// ============================================================================
// ============================================================================