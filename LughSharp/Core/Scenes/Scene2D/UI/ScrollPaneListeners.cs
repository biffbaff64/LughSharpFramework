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

using LughSharp.Core.Scenes.Scene2D.Listeners;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Scenes.Scene2D.UI;

public class ScrollPaneScrollListener( ScrollPane parent ) : InputListener
{
    private readonly ScrollPane? _parent = parent;

    /// <inheritdoc />
    public override bool Scrolled( InputEvent? inputEvent, float x, float y, float scrollAmountX, float scrollAmountY )
    {
        Guard.Against.Null( _parent );

        _parent!.SetScrollbarsVisible( true );

        if ( _parent!.ScrollY || _parent!.ScrollX )
        {
            if ( _parent!.ScrollY )
            {
                if ( !_parent!.ScrollX && ( scrollAmountY == 0 ) )
                {
                    scrollAmountY = scrollAmountX;
                }
            }
            else
            {
                if ( _parent!.ScrollX && ( scrollAmountX == 0 ) )
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

public class ScrollPaneCaptureListener( ScrollPane parent ) : InputListener
{
    private readonly ScrollPane? _parent = parent;
    private          float       _handlePosition;

    /// <inheritdoc />
    public override bool TouchDown( InputEvent? inputEvent, float x, float y, int pointer, int button )
    {
        Guard.Against.Null( _parent );

        if ( _parent!.DraggingPointer != -1 )
        {
            return false;
        }

        if ( ( pointer == 0 ) && ( button != 0 ) )
        {
            return false;
        }

        if ( _parent!.Stage != null )
        {
            _parent!.Stage.ScrollFocus = _parent;
        }

        if ( !_parent!.FlickScroll )
        {
            _parent!.SetScrollbarsVisible( true );
        }

        if ( _parent!.FadeAlpha == 0 )
        {
            return false;
        }

        if ( _parent!.ScrollBarTouch
          && _parent!.ScrollX
          && _parent!.HScrollBounds.Contains( x, y ) )
        {
            inputEvent?.Stop();
            _parent!.SetScrollbarsVisible( true );

            if ( _parent!.HKnobBounds.Contains( x, y ) )
            {
                _parent!.LastPoint.Set( x, y );
                _handlePosition           = _parent!.HKnobBounds.X;
                _parent!.TouchScrollH    = true;
                _parent!.DraggingPointer = pointer;

                return true;
            }

            _parent!.AmountX += _parent!.WidgetArea.Width * ( x < _parent!.HKnobBounds.X ? -1 : 1 );

            return true;
        }

        if ( _parent!.ScrollBarTouch
          && _parent!.ScrollY
          && _parent!.VScrollBounds.Contains( x, y ) )
        {
            inputEvent?.Stop();
            _parent!.SetScrollbarsVisible( true );

            if ( _parent!.VKnobBounds.Contains( x, y ) )
            {
                _parent!.LastPoint.Set( x, y );
                _handlePosition           = _parent!.VKnobBounds.Y;
                _parent!.TouchScrollV    = true;
                _parent!.DraggingPointer = pointer;

                return true;
            }

            _parent!.AmountY += _parent!.WidgetArea.Height * ( y < _parent!.VKnobBounds.Y ? 1 : -1 );

            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override void TouchUp( InputEvent? inputEvent, float x, float y, int pointer, int button )
    {
        Guard.Against.Null( _parent );

        if ( pointer != _parent!.DraggingPointer )
        {
            return;
        }

        _parent!.Cancel();
    }

    /// <inheritdoc />
    public override void TouchDragged( InputEvent? inputEvent, float x, float y, int pointer )
    {
        Guard.Against.Null( _parent );

        if ( pointer != _parent!.DraggingPointer )
        {
            return;
        }

        if ( _parent!.TouchScrollH )
        {
            var delta   = x - _parent!.LastPoint.X;
            var scrollH = _handlePosition + delta;

            _handlePosition = scrollH;
            scrollH         = Math.Max( _parent!.HScrollBounds.X, scrollH );

            scrollH = Math.Min( ( _parent!.HScrollBounds.X + _parent!.HScrollBounds.Width )
                              - _parent!.HKnobBounds.Width,
                                scrollH );

            var total = _parent!.HScrollBounds.Width - _parent!.HKnobBounds.Width;

            if ( total != 0 )
            {
                _parent!.SetScrollPercentX( ( scrollH - _parent!.HScrollBounds.X ) / total );
            }

            _parent!.LastPoint.Set( x, y );
        }
        else if ( _parent!.TouchScrollV )
        {
            var delta   = y - _parent!.LastPoint.Y;
            var scrollV = _handlePosition + delta;

            _handlePosition = scrollV;
            scrollV         = Math.Max( _parent!.VScrollBounds.Y, scrollV );

            scrollV = Math.Min( ( _parent!.VScrollBounds.Y + _parent!.VScrollBounds.Height )
                              - _parent!.VKnobBounds.Height,
                                scrollV );

            var total = _parent!.VScrollBounds.Height - _parent!.VKnobBounds.Height;

            if ( total != 0 )
            {
                _parent!.SetScrollPercentY( 1 - ( ( scrollV - _parent!.VScrollBounds.Y ) / total ) );
            }

            _parent!.LastPoint.Set( x, y );
        }
    }

    /// <inheritdoc />
    public override bool MouseMoved( InputEvent? inputEvent, float x, float y )
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

public class ScrollPaneGestureListener : ActorGestureListener
{
    private readonly ScrollPane? _parent;

    public ScrollPaneGestureListener( ScrollPane parent )
    {
        _parent = parent;
    }

    /// <inheritdoc />
    public override void Pan( InputEvent inputEvent, float x, float y, float deltaX, float deltaY )
    {
        Guard.Against.Null( _parent );

        _parent!.SetScrollbarsVisible( true );

        _parent!.AmountX -= deltaX;
        _parent!.AmountY += deltaY;

        _parent!.ClampPane();

        if ( _parent!.CancelTouchFocus &&
             ( ( _parent!.ScrollX && ( deltaX != 0 ) ) || ( _parent!.ScrollY && ( deltaY != 0 ) ) ) )
        {
            _parent!.TouchFocusCancel();
        }
    }

    /// <inheritdoc />
    public override void Fling( InputEvent inputEvent, float x, float y, int button )
    {
        Guard.Against.Null( _parent );

        if ( ( Math.Abs( x ) > 150 ) && _parent!.ScrollX )
        {
            _parent!.FlingTimer = _parent!.FlingTime;
            _parent!.VelocityX   = x;

            if ( _parent!.CancelTouchFocus )
            {
                _parent!.TouchFocusCancel();
            }
        }

        if ( ( Math.Abs( y ) > 150 ) && _parent!.ScrollY )
        {
            _parent!.FlingTimer = _parent!.FlingTime;
            _parent!.VelocityY   = -y;

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