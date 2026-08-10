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

using LughSharp.Source.Scene2D.UI;

namespace LughSharp.Source.Scene2D.Listeners;

/// <summary>
/// A listener for handling scrolling events within a <see cref="ScrollPane"/>.
/// This listener responds to mouse wheel scroll inputs and adjusts the
/// scroll position of the associated <see cref="ScrollPane"/>.
/// </summary>
public sealed class ScrollPaneScrollListener( ScrollPane parent ) : InputListener
{
    private readonly ScrollPane? _parent = parent;
    
    // ========================================================================

    /// <summary>
    /// Called when the mouse wheel has been scrolled. When true is returned,
    /// the event is handled in <see cref="Event.SetHandled"/>.
    /// </summary>
    public override bool OnScrolled( InputEvent? ev, float x, float y, float amountX, float amountY )
    {
        Guard.Against.Null( _parent );

        _parent!.SetScrollbarsVisible( true );

        if ( _parent!.IsScrollY || _parent!.IsScrollX )
        {
            if ( _parent!.IsScrollY )
            {
                if ( !_parent!.IsScrollX && ( amountY == 0 ) )
                {
                    amountY = amountX;
                }
            }
            else
            {
                if ( _parent!.IsScrollX && ( amountX == 0 ) )
                {
                    amountX = amountY;
                }
            }

            _parent!.ScrollAmountY += _parent!.GetMouseWheelY() * amountY;
            _parent!.ScrollAmountX += _parent!.GetMouseWheelX() * amountX;
        }
        else
        {
            return false;
        }

        return true;
    }
}

// ============================================================================
// ============================================================================

/// <summary>
/// A listener that captures touch and mouse events for a <see cref="ScrollPane"/>
/// and processes them to enable scrolling behaviors, including dragging and scrollbar interactions.
/// Handles user interactions such as touch down, touch up, dragging, and mouse movement.
/// </summary>
[PublicAPI]
public class ScrollPaneCaptureListener( ScrollPane parent ) : InputListener
{
    private readonly ScrollPane? _parent = parent;
    private          float       _handlePosition;

    // ========================================================================
    
    /// <summary>
    /// Called when a mouse button or a finger touch goes down on the actor.
    /// If true is returned, this listener will have
    /// <see cref="Stage.AddTouchFocus(IEventListener, Actor, Actor, int, int)"/>,
    /// so it will receive all touchDragged and touchUp events, even those not
    /// over this actor, until touchUp is received. Also when true is returned,
    /// the event is handled by <see cref="Event.SetHandled"/>.
    /// </summary>
    public override bool OnTouchDown( InputEvent? ev, float x, float y, int ptr, int button )
    {
        Guard.Against.Null( _parent );

        Logger.Checkpoint();
        
        if ( ( _parent.DraggingPointer != -1 )
            || ( ( ptr == 0 ) && ( button != 0 ) ) )
        {
            return false;
        }

        _parent.GetStage()?.ScrollFocus = _parent;

        if ( !_parent.FlickScroll )
        {
            _parent.SetScrollbarsVisible( true );
        }

        if ( _parent.FadeAlpha == 0 )
        {
            return false;
        }

        if ( _parent.ScrollBarTouch
          && _parent.IsScrollX
          && _parent.HScrollBounds.Contains( x, y ) )
        {
            ev?.Stop();
            _parent.SetScrollbarsVisible( true );

            if ( _parent.HKnobBounds.Contains( x, y ) )
            {
                _parent.LastPoint.Set( x, y );
                _handlePosition          = _parent.HKnobBounds.X;
                _parent.TouchScrollH    = true;
                _parent.DraggingPointer = ptr;

                return true;
            }

            _parent.ScrollAmountX += _parent.WidgetArea.Width * ( x < _parent.HKnobBounds.X ? -1 : 1 );

            return true;
        }

        if ( _parent.ScrollBarTouch
          && _parent.IsScrollY
          && _parent.VScrollBounds.Contains( x, y ) )
        {
            ev?.Stop();
            _parent.SetScrollbarsVisible( true );

            if ( _parent.VKnobBounds.Contains( x, y ) )
            {
                _parent.LastPoint.Set( x, y );
                _handlePosition          = _parent.VKnobBounds.Y;
                _parent.TouchScrollV    = true;
                _parent.DraggingPointer = ptr;

                return true;
            }

            _parent.ScrollAmountY += _parent.WidgetArea.Height * ( y < _parent.VKnobBounds.Y ? 1 : -1 );

            return true;
        }

        return false;
    }

    /// <summary>
    /// Called when a mouse button or a finger touch goes up anywhere, but only
    /// if touchDown previously returned true for the mouse button or touch.
    /// The touchUp event is always handled by <see cref="Event.SetHandled"/>.
    /// </summary>
    public override void OnTouchUp( InputEvent? ev, float x, float y, int ptr, int button )
    {
        Guard.Against.Null( _parent );

        if ( ptr != _parent!.DraggingPointer )
        {
            return;
        }

        _parent!.Cancel();
    }

    /// <summary>
    /// Called when a mouse button or a finger touch is moved anywhere, but only
    /// if touchDown previously returned true for the mouse button or touch.
    /// The touchDragged event is always handled by <see cref="Event.SetHandled"/>.
    /// </summary>
    public override void OnTouchDragged( InputEvent? inputEvent, float x, float y, int pointer )
    {
        Guard.Against.Null( _parent );

        if ( pointer != _parent!.DraggingPointer )
        {
            return;
        }

        if ( _parent!.TouchScrollH )
        {
            float delta   = x - _parent!.LastPoint.X;
            float scrollH = _handlePosition + delta;

            _handlePosition = scrollH;
            scrollH         = Math.Max( _parent!.HScrollBounds.X, scrollH );

            scrollH = Math.Min( _parent!.HScrollBounds.X + _parent!.HScrollBounds.Width
                              - _parent!.HKnobBounds.Width,
                                scrollH );

            float total = _parent!.HScrollBounds.Width - _parent!.HKnobBounds.Width;

            if ( total != 0 )
            {
                _parent!.SetScrollPercentX( ( scrollH - _parent!.HScrollBounds.X ) / total );
            }

            _parent!.LastPoint.Set( x, y );
        }
        else if ( _parent!.TouchScrollV )
        {
            float delta   = y - _parent!.LastPoint.Y;
            float scrollV = _handlePosition + delta;

            _handlePosition = scrollV;
            scrollV         = Math.Max( _parent!.VScrollBounds.Y, scrollV );

            scrollV = Math.Min( _parent!.VScrollBounds.Y + _parent!.VScrollBounds.Height
                              - _parent!.VKnobBounds.Height,
                                scrollV );

            float total = _parent!.VScrollBounds.Height - _parent!.VKnobBounds.Height;

            if ( total != 0 )
            {
                _parent!.SetScrollPercentY( 1 - ( ( scrollV - _parent!.VScrollBounds.Y ) / total ) );
            }

            _parent!.LastPoint.Set( x, y );
        }
    }

    /// <summary>
    /// Called any time the mouse is moved when a button is not down. This event
    /// only occurs on the desktop. When true is returned, the event is handled
    /// by <see cref="Event.SetHandled"/>.
    /// </summary>
    public override bool OnMouseMoved( InputEvent? inputEvent, float x, float y )
    {
        Guard.Against.Null( _parent );

        if ( !_parent!.FlickScroll )
        {
            _parent!.SetScrollbarsVisible( true );
        }

        return false;
    }
}

// ============================================================================
// ============================================================================

/// <summary>
/// A listener for handling gesture-based interactions with a <see cref="ScrollPane"/>.
/// This listener provides support for user gestures such as panning and flinging,
/// enabling intuitive navigation and interaction with the content of the associated
/// <see cref="ScrollPane"/>.
/// </summary>
[PublicAPI]
public class ScrollPaneGestureListener : ActorGestureListener
{
    private readonly ScrollPane? _parent;

    public ScrollPaneGestureListener( ScrollPane parent )
    {
        _parent = parent;
    }

    /// <summary>
    /// Called when a pan gesture is detected by the user on the scroll pane.
    /// Handles scrolling of the content within the scroll pane, updates scroll values,
    /// enforces bounds, and optionally cancels touch focus if applicable.
    /// </summary>
    /// <param name="ev">The input event associated with the pan gesture.</param>
    /// <param name="x">The current x-coordinate of the gesture's drag.</param>
    /// <param name="y">The current y-coordinate of the gesture's drag.</param>
    /// <param name="deltaX">The change in the x-coordinate since the last update.</param>
    /// <param name="deltaY">The change in the y-coordinate since the last update.</param>
    public override void OnPan( InputEvent ev, float x, float y, float deltaX, float deltaY )
    {
        Guard.Against.Null( _parent );

        _parent!.SetScrollbarsVisible( true );

        _parent!.ScrollAmountX -= deltaX;
        _parent!.ScrollAmountY += deltaY;

        _parent!.ClampPane();

        if ( _parent!.CancelTouchFocus &&
             ( ( _parent!.IsScrollX && ( deltaX != 0 ) ) || ( _parent!.IsScrollY && ( deltaY != 0 ) ) ) )
        {
            _parent!.TouchFocusCancel();
        }
    }

    /// <summary>
    /// Invoked when a fling gesture is detected. Manages the fling behavior of the scroll pane,
    /// applying velocity to the scroll position based on the fling gesture parameters.
    /// </summary>
    /// <param name="ev">The event associated with the fling gesture.</param>
    /// <param name="velocityX">The horizontal velocity of the fling. A positive value represents rightward motion.</param>
    /// <param name="velocityY">The vertical velocity of the fling. A positive value represents downward motion.</param>
    /// <param name="button">The button involved in the fling gesture, if applicable.</param>
    public override void OnFling( InputEvent ev, float velocityX, float velocityY, int button )
    {
        Guard.Against.Null( _parent );

        if ( ( Math.Abs( velocityX ) > 150 ) && _parent!.IsScrollX )
        {
            _parent!.FlingTimer = _parent!.FlingTime;
            _parent!.VelocityX  = velocityX;

            if ( _parent!.CancelTouchFocus )
            {
                _parent!.TouchFocusCancel();
            }
        }

        if ( ( Math.Abs( velocityY ) > 150 ) && _parent!.IsScrollY )
        {
            _parent!.FlingTimer = _parent!.FlingTime;
            _parent!.VelocityY  = -velocityY;

            if ( _parent!.CancelTouchFocus )
            {
                _parent!.TouchFocusCancel();
            }
        }
    }

    /// <summary>
    /// Handles the event passed into the method. If the event is a touch down input event,
    /// it resets the fling timer. If a touch focus cancel event is detected, it triggers a
    /// cancellation of the scroll pane. Returns true if the event is processed, otherwise false.
    /// </summary>
    /// <param name="e">
    /// The event to be processed, provided as an instance of <see cref="Event"/>.
    /// </param>
    /// <returns>
    /// True if the event has been handled; otherwise, false.
    /// </returns>
    public override bool Handle( Event e )
    {
        Guard.Against.Null( _parent );

        if ( base.Handle( e ) )
        {
            if ( ( ( InputEvent )e ).Type == InputEvent.EventType.TouchDown )
            {
                _parent!.FlingTimer = 0;
            }

            return true;
        }

        if ( e is InputEvent { TouchFocusCancel: true } )
        {
            _parent!.Cancel();
        }

        return false;
    }
}

// ============================================================================
// ============================================================================

