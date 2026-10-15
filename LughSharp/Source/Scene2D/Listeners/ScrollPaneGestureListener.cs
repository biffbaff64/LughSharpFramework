// // /////////////////////////////////////////////////////////////////////////////
// //  MIT License
// //
// //  Copyright (c) 2024 Richard Ikin
// //
// //  Permission is hereby granted, free of charge, to any person obtaining a copy
// //  of this software and associated documentation files (the "Software"), to deal
// //  in the Software without restriction, including without limitation the rights
// //  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// //  copies of the Software, and to permit persons to whom the Software is
// //  furnished to do so, subject to the following conditions:
// //
// //  The above copyright notice and this permission notice shall be included in all
// //  copies or substantial portions of the Software.
// //
// //  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// //  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// //  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// //  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// //  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// //  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// //  SOFTWARE.
// // /////////////////////////////////////////////////////////////////////////////

using LughSharp.Source.Scene2D.UI;

namespace LughSharp.Source.Scene2D.Listeners;

/// <summary>
/// A listener for handling gesture-based interactions with a <see cref="ScrollPane"/>.
/// This listener provides support for user gestures such as panning and flinging,
/// enabling intuitive navigation and interaction with the content of the associated
/// <see cref="ScrollPane"/>.
/// </summary>
[PublicAPI]
public class ScrollPaneGestureListener( ScrollPane parent ) : ActorGestureListener
{
    private readonly ScrollPane? _parent = parent;

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

