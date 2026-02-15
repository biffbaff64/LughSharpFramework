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

using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Graphics.Utils;
using LughSharp.Core.Main;
using LughSharp.Core.Maths;
using LughSharp.Core.Scenes.Scene2D.Listeners;
using LughSharp.Core.Scenes.Scene2D.Utils;
using LughSharp.Core.Utils.Exceptions;

using Rectangle = LughSharp.Core.Maths.Rectangle;

namespace LughSharp.Core.Scenes.Scene2D.UI;

[PublicAPI]
public class ScrollPane : WidgetGroup
{
    public readonly Rectangle HKnobBounds   = new();
    public readonly Rectangle HScrollBounds = new();
    public readonly Vector2   LastPoint     = new();
    public readonly Rectangle VKnobBounds   = new();
    public readonly Rectangle VScrollBounds = new();
    public readonly Rectangle WidgetArea    = new();

    // ========================================================================

    public int    DraggingPointer    { get; set; } = -1;
    public float  FadeAlpha          { get; set; }
    public float  FadeAlphaSeconds   { get; set; } = 1;
    public float  FadeDelay          { get; set; }
    public float  FadeDelaySeconds   { get; set; } = 1;
    public bool   FlickScroll        { get; set; } = true;
    public float  FlingTimer         { get; set; }
    public bool   HScrollOnBottom    { get; set; } = true;
    public float  OverscrollSpeedMax { get; set; } = 200;
    public float  OverscrollSpeedMin { get; set; } = 30;
    public bool   OverscrollX        { get; set; } = true;
    public bool   OverscrollY        { get; set; } = true;
    public bool   ScrollbarsOnTop    { get; set; }
    public bool   TouchScrollH       { get; set; }
    public bool   TouchScrollV       { get; set; }
    public float  VisualAmountX      { get; set; }
    public float  VisualAmountY      { get; set; }
    public bool   VScrollOnRight     { get; set; } = true;
    public Actor? Widget             { get; set; }
    public float  AmountX            { get; set; }
    public float  AmountY            { get; set; }
    public bool   ForceScrollX       { get; set; }
    public bool   ForceScrollY       { get; set; }
    public bool   DisableXScroll     { get; set; }
    public bool   DisableYScroll     { get; set; }
    public float  OverscrollDistance { get; set; } = 50;
    public bool   FadeScrollBars     { get; set; } = true;
    public bool   SmoothScrolling    { get; set; } = true;

    /// <summary>
    /// Returns the maximum scroll value in the x direction.
    /// </summary>
    public float MaxScrollX { get; set; }

    /// <summary>
    /// Returns the maximum scroll value in the y direction.
    /// </summary>
    public float MaxScrollY { get; set; }

    /// <summary>
    /// Indicates whether horizontal scrolling is enabled within the scroll pane.
    /// </summary>
    public bool IsScrollX { get; set; }

    /// <summary>
    /// Indicates whether vertical scrolling is enabled within the scroll pane.
    /// </summary>
    public bool IsScrollY { get; set; }

    /// <summary>
    /// Flick scroll X velocity.
    /// </summary>
    public float VelocityX { get; set; }

    /// <summary>
    /// Flick scroll Y velocity.
    /// </summary>
    public float VelocityY { get; set; }

    /// <summary>
    /// When false, the scroll bars don't respond to touch or mouse events.
    /// Default is true.
    /// </summary>
    public bool ScrollBarTouch { get; set; } = true;

    /// <summary>
    /// For flick scroll, sets the amount of time in seconds that a fling
    /// will continue to scroll. Default is 1.
    /// </summary>
    public float FlingTime { get; set; } = 1f;

    /// <summary>
    /// For flick scroll, prevents scrolling out of the widget's bounds.
    /// Default is true.
    /// </summary>
    public bool Clamp { get; set; } = true;

    public ScrollPaneStyle Style
    {
        get;
        set
        {
            Guard.Against.Null( value, "style cannot be null." );

            field = value;
            InvalidateHierarchy();
        }
    }
    
    /// If true, the scroll knobs are sized based on
    /// <see cref="MaxScrollX"/>
    /// " or
    /// <see cref="MaxScrollY"/>
    /// . If false, the scroll knobs are sized based on
    /// <see cref="ISceneDrawable.MinWidth"/>
    /// or
    /// <see cref="ISceneDrawable.MinHeight"/>
    /// ".
    /// Default is true.
    public bool VariableSizeKnobs { get; set; } = true;

    /// <summary>
    /// When true (default) and flick scrolling begins, <see cref="TouchFocusCancel"/>
    /// is called. This causes any widgets inside the scrollpane that have received
    /// touchDown to receive touchUp when flick scrolling begins.
    /// </summary>
    public bool CancelTouchFocus { get; set; } = true;

    // ========================================================================

    private readonly ActorGestureListener _flickScrollListener;
    private readonly Rectangle            _widgetCullingArea = new();

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates a scroll pane with the specified widget and default <see cref="ScrollPaneStyle"/>.
    /// </summary>
    public ScrollPane( Actor? widget ) : this( widget, new ScrollPaneStyle() )
    {
    }

    /// <summary>
    /// Creates a scroll pane with the specified widget and <see cref="ScrollPaneStyle"/>
    /// from the specified skin.
    /// </summary>
    /// <param name="widget"></param>
    /// <param name="skin"></param>
    public ScrollPane( Actor? widget, Skin skin )
        : this( widget, skin.Get< ScrollPaneStyle >() )
    {
    }

    /// <summary>
    /// Creates a scroll pane with the specified <see cref="Widget"/>, <see cref="Skin"/>
    /// and <see cref="ScrollPaneStyle"/>
    /// </summary>
    /// <param name="widget"></param>
    /// <param name="skin"></param>
    /// <param name="styleName"></param>
    public ScrollPane( Actor? widget, Skin skin, string styleName )
        : this( widget, skin.Get< ScrollPaneStyle >( styleName ) )
    {
    }

    /// <summary>
    /// Creates a scroll pane with the specified widget and <see cref="ScrollPaneStyle"/>
    /// </summary>
    /// <param name="widget"></param>
    /// <param name="style"></param>
    public ScrollPane( Actor? widget, ScrollPaneStyle style )
    {
        Guard.Against.Null( style );

        Style = style;

        _flickScrollListener = new ScrollPaneGestureListener( this );

        SetActor( widget );
        SetSize( 150, 150 );

        AddCaptureListener( new ScrollPaneCaptureListener( this ) );
        AddListener( _flickScrollListener );
        AddListener( new ScrollPaneScrollListener( this ) );
    }

    /// <summary>
    /// Shows or hides the scrollbars for when using <see cref="SetFadeScrollBars(bool)"/>
    /// </summary>
    public void SetScrollbarsVisible( bool visible )
    {
        if ( visible )
        {
            FadeAlpha = FadeAlphaSeconds;
            FadeDelay = FadeDelaySeconds;
        }
        else
        {
            FadeAlpha = 0;
            FadeDelay = 0;
        }
    }

    /// <summary>
    /// Cancels the stage's touch focus for all listeners except this scroll
    /// pane's flick scroll listener. This causes any widgets inside the
    /// scrollpane that have received touchDown to receive touchUp.
    /// </summary>
    public void TouchFocusCancel()
    {
        Stage?.CancelTouchFocusExcept( _flickScrollListener, this );
    }

    /// <summary>
    /// If currently scrolling by tracking a touch down, stop scrolling.
    /// </summary>
    public void Cancel()
    {
        DraggingPointer = -1;
        TouchScrollH    = false;
        TouchScrollV    = false;
        _flickScrollListener.Detector.Cancel();
    }

    /// <summary>
    /// 
    /// </summary>
    public void ClampPane()
    {
        if ( !Clamp )
        {
            return;
        }

        AmountX = OverscrollX
            ? MathUtils.Clamp( AmountX, -OverscrollDistance, MaxScrollX + OverscrollDistance )
            : MathUtils.Clamp( AmountX, 0, MaxScrollX );

        AmountY = OverscrollY
            ? MathUtils.Clamp( AmountY, -OverscrollDistance, MaxScrollY + OverscrollDistance )
            : MathUtils.Clamp( AmountY, 0, MaxScrollY );
    }

    public override void Act( float delta )
    {
        base.Act( delta );

        var panning   = _flickScrollListener.Detector.IsPanning();
        var animating = false;

        if ( ( FadeAlpha > 0 ) && FadeScrollBars && !panning && !TouchScrollH && !TouchScrollV )
        {
            FadeDelay -= delta;

            if ( FadeDelay <= 0 )
            {
                FadeAlpha = Math.Max( 0, FadeAlpha - delta );
            }

            animating = true;
        }

        if ( FlingTimer > 0 )
        {
            SetScrollbarsVisible( true );

            var alpha = FlingTimer / FlingTime;

            AmountX -= VelocityX * alpha * delta;
            AmountY -= VelocityY * alpha * delta;

            ClampPane();

            // Stop fling if hit overscroll distance.
            if ( AmountX.Equals( -OverscrollDistance ) )
            {
                VelocityX = 0;
            }

            if ( AmountX >= ( MaxScrollX + OverscrollDistance ) )
            {
                VelocityX = 0;
            }

            if ( AmountY.Equals( -OverscrollDistance ) )
            {
                VelocityY = 0;
            }

            if ( AmountY >= ( MaxScrollY + OverscrollDistance ) )
            {
                VelocityY = 0;
            }

            FlingTimer -= delta;

            if ( FlingTimer <= 0 )
            {
                VelocityX = 0;
                VelocityY = 0;
            }

            animating = true;
        }

        // Scroll smoothly when grabbing the scrollbar if one pixel of
        // scrollbar movement is > 10% of the scroll area.

        if ( SmoothScrolling
          && ( FlingTimer <= 0 )
          && !panning
          && ( !TouchScrollH ||
               ( IsScrollX && ( ( MaxScrollX / ( HScrollBounds.Width - HKnobBounds.Width ) ) >
                              ( WidgetArea.Width * 0.1f ) ) ) )
          && ( !TouchScrollV ||
               ( IsScrollY &&
                 ( ( MaxScrollY / ( VScrollBounds.Height - VKnobBounds.Height ) ) > ( WidgetArea.Height * 0.1f ) ) ) ) )
        {
            if ( !VisualAmountX.Equals( AmountX ) )
            {
                VisualScrollX( VisualAmountX < AmountX
                                   ? Math.Min( AmountX,
                                               VisualAmountX +
                                               Math.Max( 200 * delta, ( AmountX - VisualAmountX ) * 7 * delta ) )
                                   : Math.Max( AmountX,
                                               VisualAmountX -
                                               Math.Max( 200 * delta, ( VisualAmountX - AmountX ) * 7 * delta ) ) );

                animating = true;
            }

            if ( !VisualAmountY.Equals( AmountY ) )
            {
                VisualScrollY( VisualAmountY < AmountY
                                   ? Math.Min( AmountY,
                                               VisualAmountY +
                                               Math.Max( 200 * delta, ( AmountY - VisualAmountY ) * 7 * delta ) )
                                   : Math.Max( AmountY,
                                               VisualAmountY -
                                               Math.Max( 200 * delta, ( VisualAmountY - AmountY ) * 7 * delta ) ) );

                animating = true;
            }
        }
        else
        {
            if ( !VisualAmountX.Equals( AmountX ) )
            {
                VisualScrollX( AmountX );
            }

            if ( !VisualAmountY.Equals( AmountY ) )
            {
                VisualScrollY( AmountY );
            }
        }

        if ( !panning )
        {
            if ( OverscrollX && IsScrollX )
            {
                if ( AmountX < 0 )
                {
                    SetScrollbarsVisible( true );

                    AmountX += ( OverscrollSpeedMin
                               + ( ( ( OverscrollSpeedMax - OverscrollSpeedMin )
                                   * -AmountX )
                                 / OverscrollDistance ) )
                             * delta;

                    if ( AmountX > 0 )
                    {
                        AmountX = 0;
                    }

                    animating = true;
                }
                else if ( AmountX > MaxScrollX )
                {
                    SetScrollbarsVisible( true );

                    AmountX -= ( OverscrollSpeedMin
                               + ( ( ( OverscrollSpeedMax - OverscrollSpeedMin )
                                   * -( MaxScrollX - AmountX ) )
                                 / OverscrollDistance ) )
                             * delta;

                    if ( AmountX < MaxScrollX )
                    {
                        AmountX = MaxScrollX;
                    }

                    animating = true;
                }
            }

            if ( OverscrollY && IsScrollY )
            {
                if ( AmountY < 0 )
                {
                    SetScrollbarsVisible( true );

                    AmountY += ( OverscrollSpeedMin
                               + ( ( ( OverscrollSpeedMax - OverscrollSpeedMin )
                                   * -AmountY )
                                 / OverscrollDistance ) )
                             * delta;

                    if ( AmountY > 0 )
                    {
                        AmountY = 0;
                    }

                    animating = true;
                }
                else if ( AmountY > MaxScrollY )
                {
                    SetScrollbarsVisible( true );

                    AmountY -= ( OverscrollSpeedMin
                               + ( ( ( OverscrollSpeedMax - OverscrollSpeedMin )
                                   * -( MaxScrollY - AmountY ) )
                                 / OverscrollDistance ) )
                             * delta;

                    if ( AmountY < MaxScrollY )
                    {
                        AmountY = MaxScrollY;
                    }

                    animating = true;
                }
            }
        }

        if ( animating )
        {
            if ( Stage is { ActionsRequestRendering: true } )
            {
                Engine.Api.Graphics.RequestRendering();
            }
        }
    }

    public override void SetLayout()
    {
        var bg          = Style.Background;
        var hScrollKnob = Style.HScrollKnob;
        var vScrollKnob = Style.VScrollKnob;

        if ( bg == null )
        {
            return;
        }

        WidgetArea.Set( bg.LeftWidth,
                        bg.BottomHeight,
                        Width - bg.LeftWidth - bg.RightWidth,
                        Height - bg.TopHeight - bg.BottomHeight );

        if ( Widget == null )
        {
            return;
        }

        float scrollbarHeight = 0;
        float scrollbarWidth  = 0;

        if ( hScrollKnob != null )
        {
            scrollbarHeight = hScrollKnob.MinHeight;
        }

        if ( Style.HScroll != null )
        {
            scrollbarHeight = Math.Max( scrollbarHeight, Style.HScroll.MinHeight );
        }

        if ( vScrollKnob != null )
        {
            scrollbarWidth = vScrollKnob.MinWidth;
        }

        if ( Style.VScroll != null )
        {
            scrollbarWidth = Math.Max( scrollbarWidth, Style.VScroll.MinWidth );
        }

        // Get widget's desired width.
        float widgetWidth;
        float widgetHeight;

        if ( Widget is ILayout layout )
        {
            widgetWidth  = layout.GetPrefWidth();
            widgetHeight = layout.GetPrefHeight();
        }
        else
        {
            widgetWidth  = Widget.Width;
            widgetHeight = Widget.Height;
        }

        // Determine if horizontal/vertical scrollbars are needed.
        IsScrollX = ForceScrollX || ( ( widgetWidth > WidgetArea.Width ) && !DisableXScroll );
        IsScrollY = ForceScrollY || ( ( widgetHeight > WidgetArea.Height ) && !DisableYScroll );

        // Adjust widget area for scrollbar sizes and check if it causes the other scrollbar to show.
        if ( !ScrollbarsOnTop )
        {
            if ( IsScrollY )
            {
                WidgetArea.Width -= scrollbarWidth;

                if ( !VScrollOnRight )
                {
                    WidgetArea.X += scrollbarWidth;
                }

                // Horizontal scrollbar may cause vertical scrollbar to show.
                if ( !IsScrollX && ( widgetWidth > WidgetArea.Width ) && !DisableXScroll )
                {
                    IsScrollX = true;
                }
            }

            if ( IsScrollX )
            {
                WidgetArea.Height -= scrollbarHeight;

                if ( HScrollOnBottom )
                {
                    WidgetArea.Y += scrollbarHeight;
                }

                // Vertical scrollbar may cause horizontal scrollbar to show.
                if ( !IsScrollY && ( widgetHeight > WidgetArea.Height ) && !DisableYScroll )
                {
                    IsScrollY          =  true;
                    WidgetArea.Width -= scrollbarWidth;

                    if ( !VScrollOnRight )
                    {
                        WidgetArea.X += scrollbarWidth;
                    }
                }
            }
        }

        // If the widget is smaller than the available space, make it take up the available space.
        widgetWidth  = DisableXScroll ? WidgetArea.Width : Math.Max( WidgetArea.Width, widgetWidth );
        widgetHeight = DisableYScroll ? WidgetArea.Height : Math.Max( WidgetArea.Height, widgetHeight );

        MaxScrollX = widgetWidth - WidgetArea.Width;
        MaxScrollY = widgetHeight - WidgetArea.Height;

        AmountX = MathUtils.Clamp( AmountX, 0, MaxScrollX );
        AmountY = MathUtils.Clamp( AmountY, 0, MaxScrollY );

        // Set the scrollbar and knob bounds.
        if ( IsScrollX )
        {
            if ( hScrollKnob != null )
            {
                var x = ScrollbarsOnTop ? bg.LeftWidth : WidgetArea.X;
                var y = HScrollOnBottom ? bg.BottomHeight : Height - bg.TopHeight - scrollbarHeight;

                HScrollBounds.Set( x, y, WidgetArea.Width, scrollbarHeight );

                if ( IsScrollY && ScrollbarsOnTop )
                {
                    HScrollBounds.Width -= scrollbarWidth;

                    if ( !VScrollOnRight )
                    {
                        HScrollBounds.X += scrollbarWidth;
                    }
                }

                if ( VariableSizeKnobs )
                {
                    HKnobBounds.Width = Math.Max( hScrollKnob.MinWidth,
                                                  ( int )( ( HScrollBounds.Width * WidgetArea.Width ) /
                                                           widgetWidth ) );
                }
                else
                {
                    HKnobBounds.Width = hScrollKnob.MinWidth;
                }

                if ( HKnobBounds.Width > widgetWidth )
                {
                    HKnobBounds.Width = 0;
                }

                HKnobBounds.Height = hScrollKnob.MinHeight;
                HKnobBounds.X = HScrollBounds.X +
                                ( int )( ( HScrollBounds.Width - HKnobBounds.Width ) * GetScrollPercentX() );
                HKnobBounds.Y = HScrollBounds.Y;
            }
            else
            {
                HScrollBounds.Set( 0, 0, 0, 0 );
                HKnobBounds.Set( 0, 0, 0, 0 );
            }
        }

        if ( IsScrollY )
        {
            if ( vScrollKnob != null )
            {
                var x = VScrollOnRight ? Width - bg.RightWidth - scrollbarWidth : bg.LeftWidth;
                var y = ScrollbarsOnTop ? bg.BottomHeight : WidgetArea.Y;

                VScrollBounds.Set( x, y, scrollbarWidth, WidgetArea.Height );

                if ( IsScrollX && ScrollbarsOnTop )
                {
                    VScrollBounds.Height -= scrollbarHeight;

                    if ( HScrollOnBottom )
                    {
                        VScrollBounds.Y += scrollbarHeight;
                    }
                }

                VKnobBounds.Width = vScrollKnob.MinWidth;

                if ( VariableSizeKnobs )
                {
                    VKnobBounds.Height = Math.Max( vScrollKnob.MinHeight,
                                                   ( int )( ( VScrollBounds.Height * WidgetArea.Height ) /
                                                            widgetHeight ) );
                }
                else
                {
                    VKnobBounds.Height = vScrollKnob.MinHeight;
                }

                if ( VKnobBounds.Height > widgetHeight )
                {
                    VKnobBounds.Height = 0;
                }

                VKnobBounds.X = VScrollOnRight ? Width - bg.RightWidth - vScrollKnob.MinWidth : bg.LeftWidth;
                VKnobBounds.Y = VScrollBounds.Y +
                                ( int )( ( VScrollBounds.Height - VKnobBounds.Height ) *
                                         ( 1 - GetScrollPercentY() ) );
            }
            else
            {
                VScrollBounds.Set( 0, 0, 0, 0 );
                VKnobBounds.Set( 0, 0, 0, 0 );
            }
        }

        UpdateWidgetPosition();

        if ( Widget is ILayout widgetLayout )
        {
            Widget.SetSize( widgetWidth, widgetHeight );
            widgetLayout.Validate();
        }
    }

    private void UpdateWidgetPosition()
    {
        // Calculate the widget's position depending on the scroll state and available widget area.
        var x = WidgetArea.X - ( IsScrollX ? ( int )VisualAmountX : 0 );
        var y = WidgetArea.Y - ( int )( IsScrollY ? MaxScrollY - VisualAmountY : MaxScrollY );

        Widget?.SetPosition( x, y );

        if ( Widget is ICullable cullable )
        {
            _widgetCullingArea.X      = WidgetArea.X - x;
            _widgetCullingArea.Y      = WidgetArea.Y - y;
            _widgetCullingArea.Width  = WidgetArea.Width;
            _widgetCullingArea.Height = WidgetArea.Height;
            cullable.CullingArea      = _widgetCullingArea;
        }
    }

    public override void Draw( IBatch batch, float parentAlpha )
    {
        if ( Widget == null )
        {
            return;
        }

        Validate();

        // Setup transform for this group.
        ApplyTransform( batch, ComputeTransform() );

        if ( IsScrollX )
        {
            HKnobBounds.X = HScrollBounds.X
                          + ( int )( ( HScrollBounds.Width - HKnobBounds.Width )
                                   * GetVisualScrollPercentX() );
        }

        if ( IsScrollY )
        {
            VKnobBounds.Y = VScrollBounds.Y
                          + ( int )( ( VScrollBounds.Height - VKnobBounds.Height )
                                   * ( 1 - GetVisualScrollPercentY() ) );
        }

        UpdateWidgetPosition();

        // Draw the background ninepatch.
        var color = Color;
        var alpha = color.A * parentAlpha;

        if ( Style.Background != null )
        {
            batch.SetColor( color.R, color.G, color.B, alpha );
            Style.Background.Draw( batch, 0, 0, Width, Height );
        }

        batch.Flush();

        if ( ClipBegin( WidgetArea.X, WidgetArea.Y, WidgetArea.Width, WidgetArea.Height ) )
        {
            DrawChildren( batch, parentAlpha );
            batch.Flush();
            ClipEnd();
        }

        // Render scrollbars and knobs on top if they will be visible.
        batch.SetColor( color.R, color.G, color.B, alpha );

        if ( FadeScrollBars )
        {
            alpha *= Interpolation.Fade.Apply( FadeAlpha / FadeAlphaSeconds );
        }

        DrawScrollBars( batch, color.R, color.G, color.B, alpha );

        ResetTransform( batch );
    }

    /// <summary>
    /// Renders the scrollbars after the children have been drawn. If the
    /// scrollbars faded out, a is zero and rendering can be skipped.
    /// </summary>
    protected void DrawScrollBars( IBatch batch, float r, float g, float b, float a )
    {
        if ( a <= 0 )
        {
            return;
        }

        batch.SetColor( r, g, b, a );

        var x = IsScrollX && ( HKnobBounds.Width > 0 );
        var y = IsScrollY && ( VKnobBounds.Height > 0 );

        if ( x && y )
        {
            if ( Style.Corner != null )
            {
                Style.Corner.Draw( batch,
                                   HScrollBounds.X + HScrollBounds.Width,
                                   HScrollBounds.Y,
                                   VScrollBounds.Width,
                                   VScrollBounds.Y );
            }
        }

        if ( x )
        {
            if ( Style.HScroll != null )
            {
                Style.HScroll.Draw( batch,
                                    HScrollBounds.X,
                                    HScrollBounds.Y,
                                    HScrollBounds.Width,
                                    HScrollBounds.Height );
            }

            if ( Style.HScrollKnob != null )
            {
                Style.HScrollKnob.Draw( batch,
                                        HKnobBounds.X,
                                        HKnobBounds.Y,
                                        HKnobBounds.Width,
                                        HKnobBounds.Height );
            }
        }

        if ( y )
        {
            if ( Style.VScroll != null )
            {
                Style.VScroll.Draw( batch,
                                    VScrollBounds.X,
                                    VScrollBounds.Y,
                                    VScrollBounds.Width,
                                    VScrollBounds.Height );
            }

            if ( Style.VScrollKnob != null )
            {
                Style.VScrollKnob.Draw( batch,
                                        VKnobBounds.X,
                                        VKnobBounds.Y,
                                        VKnobBounds.Width,
                                        VKnobBounds.Height );
            }
        }
    }

    /// <summary>
    /// Generate fling gesture.
    /// </summary>
    /// <param name="flingTime"> Time in seconds for which you want to fling last. </param>
    /// <param name="velocityX"> Velocity for horizontal direction. </param>
    /// <param name="velocityY"> Velocity for vertical direction. </param>
    public void Fling( float flingTime, float velocityX, float velocityY )
    {
        FlingTimer = flingTime;
        VelocityX  = velocityX;
        VelocityY  = velocityY;
    }

    public override float GetPrefWidth()
    {
        float width = 0;

        if ( Widget is ILayout layout )
        {
            width = layout.GetPrefWidth();
        }
        else if ( Widget != null )
        {
            width = Widget.Width;
        }

        var background = Style.Background;

        if ( background != null )
        {
            width = Math.Max( width + background.LeftWidth + background.RightWidth, background.MinWidth );
        }

        if ( IsScrollY )
        {
            float scrollbarWidth = 0;

            if ( Style.VScrollKnob != null )
            {
                scrollbarWidth = Style.VScrollKnob.MinWidth;
            }

            if ( Style.VScroll != null )
            {
                scrollbarWidth = Math.Max( scrollbarWidth, Style.VScroll.MinWidth );
            }

            width += scrollbarWidth;
        }

        return width;
    }

    public override float GetPrefHeight()
    {
        float height = 0;

        if ( Widget is ILayout layout )
        {
            height = layout.GetPrefHeight();
        }
        else if ( Widget != null )
        {
            height = Widget.Height;
        }

        var background = Style.Background;

        if ( background != null )
        {
            height = Math.Max( height + background.TopHeight + background.BottomHeight, background.MinHeight );
        }

        if ( IsScrollX )
        {
            float scrollbarHeight = 0;

            if ( Style.HScrollKnob != null )
            {
                scrollbarHeight = Style.HScrollKnob.MinHeight;
            }

            if ( Style.HScroll != null )
            {
                scrollbarHeight = Math.Max( scrollbarHeight, Style.HScroll.MinHeight );
            }

            height += scrollbarHeight;
        }

        return height;
    }

    /// <summary>
    /// Sets the <see cref="Actor"/> embedded in this scroll pane.
    /// </summary>
    /// <param name="actor"> May be null to remove zsany current actor. </param>
    public void SetActor( Actor? actor )
    {
        if ( Widget == this )
        {
            throw new ArgumentException( "widget cannot be the ScrollPane." );
        }

        if ( Widget != null )
        {
            base.RemoveActor( Widget, true );
        }

        Widget = actor;

        if ( Widget != null )
        {
            AddActor( Widget );
        }
    }

    public bool RemoveActor( Actor? actor )
    {
        Guard.Against.Null( actor, "actor cannot be null." );

        if ( actor != Widget )
        {
            return false;
        }

        SetActor( null );

        return true;
    }

    public override bool RemoveActor( Actor actor, bool unfocus )
    {
        if ( actor == null )
        {
            throw new ArgumentException( "actor cannot be null." );
        }

        if ( actor != Widget )
        {
            return false;
        }

        Widget = null;

        return base.RemoveActor( actor, unfocus );
    }

    public override Actor? RemoveActorAt( int index, bool unfocus )
    {
        var actor = base.RemoveActorAt( index, unfocus );

        if ( actor == Widget )
        {
            Widget = null;
        }

        return actor;
    }

    /// <summary>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="touchable"></param>
    /// <returns></returns>
    public override Actor? Hit( float x, float y, bool touchable )
    {
        if ( ( x < 0 ) || ( x >= Width ) || ( y < 0 ) || ( y >= Height ) )
        {
            return null;
        }

        if ( touchable && ( Touchable == Touchable.Enabled ) && IsVisible )
        {
            if ( ( IsScrollX && TouchScrollH && HScrollBounds.Contains( x, y ) )
              || ( IsScrollY && TouchScrollV && VScrollBounds.Contains( x, y ) ) )
            {
                return this;
            }
        }

        return base.Hit( x, y, touchable );
    }

    /// <summary>
    /// Returns the amount to scroll horizontally when the mouse wheel is scrolled.
    /// </summary>
    public float GetMouseWheelX()
    {
        return Math.Min( WidgetArea.Width, Math.Max( WidgetArea.Width * 0.9f, MaxScrollX * 0.1f ) / 4 );
    }

    /// <summary>
    /// Returns the amount to scroll vertically when the mouse wheel is scrolled.
    /// </summary>
    public float GetMouseWheelY()
    {
        return Math.Min( WidgetArea.Height, Math.Max( WidgetArea.Height * 0.9f, MaxScrollY * 0.1f ) / 4 );
    }

    /// <summary>
    /// Sets the visual scroll amount equal to the scroll amount. This can
    /// be used when setting the scroll amount without animating.
    /// </summary>
    public void UpdateVisualScroll()
    {
        VisualAmountX = AmountX;
        VisualAmountY = AmountY;
    }

    public float GetVisualScrollPercentX()
    {
        if ( MaxScrollX == 0 )
        {
            return 0;
        }

        return MathUtils.Clamp( VisualAmountX / MaxScrollX, 0, 1 );
    }

    public float GetVisualScrollPercentY()
    {
        if ( MaxScrollY == 0 )
        {
            return 0;
        }

        return MathUtils.Clamp( VisualAmountY / MaxScrollY, 0, 1 );
    }

    public float GetScrollPercentX()
    {
        if ( MaxScrollX == 0 )
        {
            return 0;
        }

        return MathUtils.Clamp( AmountX / MaxScrollX, 0, 1 );
    }

    public void SetScrollPercentX( float percentX )
    {
        AmountX = MaxScrollX * MathUtils.Clamp( percentX, 0, 1 );
    }

    public float GetScrollPercentY()
    {
        if ( MaxScrollY == 0 )
        {
            return 0;
        }

        return MathUtils.Clamp( AmountY / MaxScrollY, 0, 1 );
    }

    public void SetScrollPercentY( float percentY )
    {
        AmountY = MaxScrollY * MathUtils.Clamp( percentY, 0, 1 );
    }

    public void SetFlickScroll( bool flickScroll )
    {
        if ( FlickScroll == flickScroll )
        {
            return;
        }

        FlickScroll = flickScroll;

        if ( flickScroll )
        {
            AddListener( _flickScrollListener );
        }
        else
        {
            RemoveListener( _flickScrollListener );
        }

        Invalidate();
    }

    public void SetFlickScrollTapSquareSize( float halfTapSquareSize )
    {
        _flickScrollListener.Detector.SetTapSquareSize( halfTapSquareSize );
    }

    /// <summary>
    /// Sets the scroll offset so the specified rectangle is fully in view,
    /// if possible. Coordinates are in the scroll pane widget's coordinate
    /// system.
    /// </summary>
    public void ScrollTo( float x, float y, float width, float height )
    {
        ScrollTo( x, y, width, height, false, false );
    }

    /// <summary>
    /// Sets the scroll offset so the specified rectangle is fully in view, and
    /// optionally centered vertically and/or horizontally, if possible.
    /// Coordinates are in the scroll pane widget's coordinate system.
    /// </summary>
    public void ScrollTo( float x, float y, float width, float height, bool centerHorizontal, bool centerVertical )
    {
        Validate();

        var amountX = AmountX;

        if ( centerHorizontal )
        {
            amountX = ( x - ( WidgetArea.Width / 2 ) ) + ( width / 2 );
        }
        else
        {
            if ( ( x + width ) > ( amountX + WidgetArea.Width ) )
            {
                amountX = ( x + width ) - WidgetArea.Width;
            }

            if ( x < amountX )
            {
                amountX = x;
            }
        }

        AmountX = MathUtils.Clamp( amountX, 0, MaxScrollX );

        var amountY = AmountY;

        if ( centerVertical )
        {
            amountY = ( ( MaxScrollY - y ) + ( WidgetArea.Height / 2 ) ) - ( height / 2 );
        }
        else
        {
            if ( amountY > ( ( MaxScrollY - y - height ) + WidgetArea.Height ) )
            {
                amountY = ( MaxScrollY - y - height ) + WidgetArea.Height;
            }

            if ( amountY < ( MaxScrollY - y ) )
            {
                amountY = MaxScrollY - y;
            }
        }

        AmountY = MathUtils.Clamp( amountY, 0f, MaxScrollY );
    }

    public float GetScrollBarHeight()
    {
        if ( !IsScrollX )
        {
            return 0;
        }

        float height = 0;

        if ( Style.HScrollKnob != null )
        {
            height = Style.HScrollKnob.MinHeight;
        }

        if ( Style.HScroll != null )
        {
            height = Math.Max( height, Style.HScroll.MinHeight );
        }

        return height;
    }

    public float GetScrollBarWidth()
    {
        if ( !IsScrollY )
        {
            return 0;
        }

        float width = 0;

        if ( Style.VScrollKnob != null )
        {
            width = Style.VScrollKnob.MinWidth;
        }

        if ( Style.VScroll != null )
        {
            width = Math.Max( width, Style.VScroll.MinWidth );
        }

        return width;
    }

    /// <summary>
    /// Disables scrolling in a direction. The widget will be sized to the
    /// FlickScrollPane in the disabled direction.
    /// </summary>
    public void SetScrollingDisabled( bool x, bool y )
    {
        DisableXScroll = x;
        DisableYScroll = y;

        Invalidate();
    }

    public bool IsPanning()
    {
        return _flickScrollListener.Detector.IsPanning();
    }

    /// <summary>
    /// For flick scroll, if true the widget can be scrolled slightly past its
    /// bounds and will animate back to its bounds when scrolling is stopped.
    /// Default is true.
    /// </summary>
    public void SetOverscroll( bool overscrollX, bool overscrollY )
    {
        OverscrollX = overscrollX;
        OverscrollY = overscrollY;
    }

    /// <summary>
    /// For flick scroll, sets the overscroll distance in pixels and the speed
    /// it returns to the widget's bounds in seconds. Default is 50, 30, 200.
    /// </summary>
    public void SetupOverscroll( float distance, float speedMin, float speedMax )
    {
        OverscrollDistance = distance;
        OverscrollSpeedMin = speedMin;
        OverscrollSpeedMax = speedMax;
    }

    /// <summary>
    /// Forces enabling scrollbars (for non-flick scroll) and overscrolling
    /// (for flick scroll) in a direction, even if the contents do not exceed
    /// the bounds in that direction.
    /// </summary>
    public void SetForceScroll( bool x, bool y )
    {
        ForceScrollX = x;
        ForceScrollY = y;
    }

    /// <summary>
    /// Set the position of the vertical and horizontal scroll bars.
    /// </summary>
    public void SetScrollBarPositions( bool bottom, bool right )
    {
        HScrollOnBottom = bottom;
        VScrollOnRight  = right;
    }

    /// <summary>
    /// When true the scrollbars don't reduce the scrollable size and fade out
    /// after some time of not being used.
    /// </summary>
    public void SetFadeScrollBars( bool fadeScrollBars )
    {
        if ( FadeScrollBars == fadeScrollBars )
        {
            return;
        }

        FadeScrollBars = fadeScrollBars;

        if ( !fadeScrollBars )
        {
            FadeAlpha = FadeAlphaSeconds;
        }

        Invalidate();
    }

    public void SetupFadeScrollBars( float fadeAlphaSeconds, float fadeDelaySeconds )
    {
        FadeAlphaSeconds = fadeAlphaSeconds;
        FadeDelaySeconds = fadeDelaySeconds;
    }

    /// <summary>
    /// When false (the default), the widget is clipped so it is not drawn
    /// under the scrollbars. When true, the widget is clipped to the entire
    /// scroll pane bounds and the scrollbars are drawn on top of the widget.
    /// If <see cref="SetFadeScrollBars(bool)"/> is true, the scroll bars are
    /// always drawn on top.
    /// </summary>
    public void SetScrollbarsOnTop( bool scrollbarsOnTop )
    {
        ScrollbarsOnTop = scrollbarsOnTop;
        Invalidate();
    }

    // ========================================================================
    // ========================================================================

    #region debug

    public override void DrawDebug( ShapeRenderer shapes )
    {
        DrawDebugBounds( shapes );
        ApplyTransform( shapes, ComputeTransform() );

        if ( ClipBegin( WidgetArea.X, WidgetArea.Y, WidgetArea.Width, WidgetArea.Height ) )
        {
            DrawDebugChildren( shapes );
            shapes.Flush();
            ClipEnd();
        }

        ResetTransform( shapes );
    }

    #endregion debug

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class ScrollPaneStyle
    {
        // ====================================================================

        public ScrollPaneStyle()
        {
        }

        public ScrollPaneStyle( ISceneDrawable background,
                                ISceneDrawable hScroll,
                                ISceneDrawable hScrollKnob,
                                ISceneDrawable vScroll,
                                ISceneDrawable vScrollKnob )
        {
            Background  = background;
            HScroll     = hScroll;
            HScrollKnob = hScrollKnob;
            VScroll     = vScroll;
            VScrollKnob = vScrollKnob;
        }

        public ScrollPaneStyle( ScrollPaneStyle style )
        {
            Background = style.Background;
            Corner     = style.Corner;

            HScroll     = style.HScroll;
            HScrollKnob = style.HScrollKnob;

            VScroll     = style.VScroll;
            VScrollKnob = style.VScrollKnob;
        }

        public ISceneDrawable? Background  { get; set; }
        public ISceneDrawable? Corner      { get; set; }
        public ISceneDrawable? HScroll     { get; set; }
        public ISceneDrawable? HScrollKnob { get; set; }
        public ISceneDrawable? VScroll     { get; set; }
        public ISceneDrawable? VScrollKnob { get; set; }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Called whenever the visual x scroll amount is changed.
    /// </summary>
    protected void VisualScrollX( float pixelsX )
    {
        VisualAmountX = pixelsX;
    }

    /// <summary>
    /// Called whenever the visual y scroll amount is changed.
    /// </summary>
    protected void VisualScrollY( float pixelsY )
    {
        VisualAmountY = pixelsY;
    }

    /// <summary>
    /// Returns the actor embedded in this scroll pane, or null.
    /// </summary>
    public Actor? GetActor()
    {
        return Widget;
    }

    /// <summary>
    /// Returns the width of the scrolled viewport.
    /// </summary>
    public float GetScrollWidth()
    {
        return WidgetArea.Width;
    }

    /// <summary>
    /// Returns the height of the scrolled viewport.
    /// </summary>
    public float GetScrollHeight()
    {
        return WidgetArea.Height;
    }

    public bool IsScrollingDisabledX()
    {
        return DisableXScroll;
    }

    public bool IsScrollingDisabledY()
    {
        return DisableYScroll;
    }

    public bool IsLeftEdge()
    {
        return !IsScrollX || ( AmountX <= 0 );
    }

    public bool IsRightEdge()
    {
        return !IsScrollX || ( AmountX >= MaxScrollX );
    }

    public bool IsTopEdge()
    {
        return !IsScrollY || ( AmountY <= 0 );
    }

    public bool IsBottomEdge()
    {
        return !IsScrollY || ( AmountY >= MaxScrollY );
    }

    public bool IsDragging()
    {
        return DraggingPointer != -1;
    }

    public float GetVisualScrollX()
    {
        return !IsScrollX ? 0 : VisualAmountX;
    }

    public float GetVisualScrollY()
    {
        return !IsScrollY ? 0 : VisualAmountY;
    }

    public void SetScrollX( float pixels )
    {
        AmountX = MathUtils.Clamp( pixels, 0, MaxScrollX );
    }

    public void SetScrollY( float pixels )
    {
        AmountY = MathUtils.Clamp( pixels, 0, MaxScrollY );
    }

    public bool IsFlinging()
    {
        return FlingTimer > 0;
    }

    public override float GetMinWidth()
    {
        return 0;
    }

    public override float GetMinHeight()
    {
        return 0;
    }
}

// ============================================================================
// ============================================================================


