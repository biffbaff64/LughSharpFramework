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

using LughSharp.Core.Scene2D.UI;

namespace LughSharp.Core.Scene2D.Listeners;

/// <summary>
/// 
/// </summary>
/// <param name="parent"></param>
public sealed class ScrollPaneScrollListener( ScrollPane parent ) : InputListener
{
    private readonly ScrollPane? _parent = parent;
    
    // ========================================================================

    /// <summary>
    /// Called when the mouse wheel has been scrolled. When true is returned,
    /// the event is handled in <see cref="Event.SetHandled"/>.
    /// </summary>
    public override bool OnScrolled( InputEvent? inputEvent, float x, float y, float scrollAmountX, float scrollAmountY )
    {
        Guard.Against.Null( _parent );

        _parent!.SetScrollbarsVisible( true );

        if ( _parent!.IsScrollY || _parent!.IsScrollX )
        {
            if ( _parent!.IsScrollY )
            {
                if ( !_parent!.IsScrollX && ( scrollAmountY == 0 ) )
                {
                    scrollAmountY = scrollAmountX;
                }
            }
            else
            {
                if ( _parent!.IsScrollX && ( scrollAmountX == 0 ) )
                {
                    scrollAmountX = scrollAmountY;
                }
            }

            _parent!.AmountY += _parent!.GetMouseWheelY() * scrollAmountY;
            _parent!.AmountX += _parent!.GetMouseWheelX() * scrollAmountX;
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

public sealed class ScrollPaneCaptureListener( ScrollPane parent ) : InputListener
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
    public override bool OnTouchDown( InputEvent? inputEvent, float x, float y, int pointer, int button )
    {
        Guard.Against.Null( _parent );

        if ( _parent.DraggingPointer != -1 )
        {
            return false;
        }

        if ( ( pointer == 0 ) && ( button != 0 ) )
        {
            return false;
        }

        if ( _parent.Stage != null )
        {
            _parent.Stage.ScrollFocus = _parent;
        }

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
            inputEvent?.Stop();
            _parent.SetScrollbarsVisible( true );

            if ( _parent.HKnobBounds.Contains( x, y ) )
            {
                _parent.LastPoint.Set( x, y );
                _handlePosition          = _parent.HKnobBounds.X;
                _parent.TouchScrollH    = true;
                _parent.DraggingPointer = pointer;

                return true;
            }

            _parent.AmountX += _parent.WidgetArea.Width * ( x < _parent.HKnobBounds.X ? -1 : 1 );

            return true;
        }

        if ( _parent.ScrollBarTouch
          && _parent.IsScrollY
          && _parent.VScrollBounds.Contains( x, y ) )
        {
            inputEvent?.Stop();
            _parent.SetScrollbarsVisible( true );

            if ( _parent.VKnobBounds.Contains( x, y ) )
            {
                _parent.LastPoint.Set( x, y );
                _handlePosition          = _parent.VKnobBounds.Y;
                _parent.TouchScrollV    = true;
                _parent.DraggingPointer = pointer;

                return true;
            }

            _parent.AmountY += _parent.WidgetArea.Height * ( y < _parent.VKnobBounds.Y ? 1 : -1 );

            return true;
        }

        return false;
    }

    /// <summary>
    /// Called when a mouse button or a finger touch goes up anywhere, but only
    /// if touchDown previously returned true for the mouse button or touch.
    /// The touchUp event is always handled by <see cref="Event.SetHandled"/>.
    /// </summary>
    public override void OnTouchUp( InputEvent? inputEvent, float x, float y, int pointer, int button )
    {
        Guard.Against.Null( _parent );

        if ( pointer != _parent!.DraggingPointer )
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

public sealed class ScrollPaneGestureListener : ActorGestureListener
{
    private readonly ScrollPane? _parent;

    public ScrollPaneGestureListener( ScrollPane parent )
    {
        _parent = parent;
    }

    /// <inheritdoc />
    public override void OnPan( InputEvent inputEvent, float x, float y, float deltaX, float deltaY )
    {
        Guard.Against.Null( _parent );

        _parent!.SetScrollbarsVisible( true );

        _parent!.AmountX -= deltaX;
        _parent!.AmountY += deltaY;

        _parent!.ClampPane();

        if ( _parent!.CancelTouchFocus &&
             ( ( _parent!.IsScrollX && ( deltaX != 0 ) ) || ( _parent!.IsScrollY && ( deltaY != 0 ) ) ) )
        {
            _parent!.TouchFocusCancel();
        }
    }

    /// <inheritdoc />
    public override void OnFling( InputEvent inputEvent, float x, float y, int button )
    {
        Guard.Against.Null( _parent );

        if ( ( Math.Abs( x ) > 150 ) && _parent!.IsScrollX )
        {
            _parent!.FlingTimer = _parent!.FlingTime;
            _parent!.VelocityX  = x;

            if ( _parent!.CancelTouchFocus )
            {
                _parent!.TouchFocusCancel();
            }
        }

        if ( ( Math.Abs( y ) > 150 ) && _parent!.IsScrollY )
        {
            _parent!.FlingTimer = _parent!.FlingTime;
            _parent!.VelocityY  = -y;

            if ( _parent!.CancelTouchFocus )
            {
                _parent!.TouchFocusCancel();
            }
        }
    }

    /// <inheritdoc />
    public override bool Handle( Event inputEvent )
    {
        Guard.Against.Null( _parent );

        if ( base.Handle( inputEvent ) )
        {
            if ( ( ( InputEvent )inputEvent ).Type == InputEvent.EventType.TouchDown )
            {
                _parent!.FlingTimer = 0;
            }

            return true;
        }

        if ( inputEvent is InputEvent { TouchFocusCancel: true } )
        {
            _parent!.Cancel();
        }

        return false;
    }
}

// ============================================================================
// ============================================================================