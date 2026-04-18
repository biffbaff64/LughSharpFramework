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

namespace LughSharp.Core.Scene2D.Listeners;

/// <summary>
/// Detects mouse over, mouse or finger touch presses, and clicks on an actor. A touch
/// must go down over the actor and is considered pressed as long as it is over the actor
/// or within the <see cref="TapSquareSize"/>.
/// <para>
/// This behavior makes it easier to press buttons on a touch interface when the initial
/// touch happens near the edge of the actor. Double clicks can be detected using
/// <see cref="TapCount"/>. Any touch (not just the first) will trigger this listener.
/// </para>
/// <para>
/// While pressed, other touch downs are ignored.
/// </para>
/// </summary>
[PublicAPI]
public class ClickListener : InputListener
{
    public float TouchDownX     { get; set; } = -1;
    public float TouchDownY     { get; set; } = -1;
    public float TapSquareSize  { get; set; } = 14;
    public int   PressedPointer { get; set; } = -1;
    public int   PressedButton  { get; set; } = -1;
    public int   Button         { get; set; }
    public int   TapCount       { get; set; }
    public bool  Pressed        { get; set; }

    /// <summary>
    /// Time in seconds <see cref="VisualPressed"/> reports true after
    /// a press resulting in a click is released.
    /// </summary>
    public const float VisualPressedDuration = 0.1f;

    // ========================================================================
    
    private readonly Action< InputEvent, float, float >? _action;
    
    private bool _cancelled;
    private bool _over;
    private long _lastTapTime;
    private long _visualPressedTime;

    // ========================================================================

    /// <summary>
    /// Creates a new ClickListener.
    /// Sets the button to listen for, all other buttons are ignored.
    /// Default is <see cref="IInput.Buttons.Left"/>. Use -1 for any button.
    /// </summary>
    public ClickListener( int button = IInput.Buttons.Left )
    {
        Button = button;
    }
    
    /// <summary>
    /// Creates a new ClickListener, setting the action to be called when a click occurs.
    /// For example:-
    /// <code>
    ///     Button.AddListener( new ClickListener( ( ev, x, y ) =>
    ///     {
    ///         Logger.Debug( "Button clicked!" );
    ///     } ) );
    /// </code>
    /// </summary>
    /// <param name="ev"> The action to call. </param>
    public ClickListener( Action< InputEvent, float, float > ev ) : this()
    {
        _action = ev;
    }
    
    /// <summary>
    /// Called when a mouse button or a finger touch goes down on the actor.
    /// If true is returned, this listener will have
    /// <see cref="Stage.AddTouchFocus(IEventListener, Actor, Actor, int, int)"/>,
    /// so it will receive all touchDragged and touchUp events, even those not
    /// over this actor, until touchUp is received. Also when true is returned,
    /// the event is handled by <see cref="Event.SetHandled"/>.
    /// </summary>
    public override bool OnTouchDown( InputEvent? ev, float x, float y, int pointer, int button )
    {
        if ( Pressed )
        {
            return false;
        }

        if ( ( pointer == 0 ) && ( Button != -1 ) && ( button != Button ) )
        {
            return false;
        }

        Pressed        = true;
        PressedPointer = pointer;
        PressedButton  = button;
        TouchDownX     = x;
        TouchDownY     = y;
        VisualPressed  = true;

        return true;
    }

    /// <summary>
    /// Called when a mouse button or a finger touch is moved anywhere, but only
    /// if touchDown previously returned true for the mouse button or touch.
    /// The touchDragged event is always handled by <see cref="Event.SetHandled"/>.
    /// </summary>
    public override void OnTouchDragged( InputEvent? ev, float x, float y, int pointer )
    {
        if ( ( pointer != PressedPointer ) || _cancelled )
        {
            return;
        }

        Pressed = IsOver( ev?.ListenerActor, x, y );

        if ( !Pressed )
        {
            // Once outside the tap square, don't use the tap square anymore.
            InvalidateTapSquare();
        }
    }

    /// <summary>
    /// Called when a mouse button or a finger touch goes up anywhere, but only
    /// if touchDown previously returned true for the mouse button or touch.
    /// The touchUp event is always handled by <see cref="Event.SetHandled"/>.
    /// </summary>
    public override void OnTouchUp( InputEvent? ev, float x, float y, int pointer, int button )
    {
        if ( pointer == PressedPointer )
        {
            // If the stage cancels the touch focus (e.g. scrolling started), flag it!
            if ( ev is { TouchFocusCancel: true } ) _cancelled = true;
            
            if ( !_cancelled )
            {
                bool touchUpOver = IsOver( ev?.ListenerActor, x, y );

                // Ignore touch up if the wrong mouse button.
                if ( touchUpOver && ( pointer == 0 ) && ( Button != -1 ) && ( button != Button ) )
                {
                    touchUpOver = false;
                }

                if ( touchUpOver )
                {
                    long time = TimeUtils.NanoTime();

                    if ( ( time - _lastTapTime ) > TapCountInterval )
                    {
                        TapCount = 0;
                    }

                    TapCount++;
                    _lastTapTime = time;

                    OnClicked( ev!, x, y );
                }
            }

            Pressed        = false;
            PressedPointer = -1;
            PressedButton  = -1;
            _cancelled     = false;
        }
    }

    /// <summary>
    /// Called any time the mouse cursor or a finger touch is moved over an actor.
    /// On the desktop, this event occurs even when no mouse buttons are pressed
    /// (pointer will be -1).
    /// </summary>
    /// <param name="ev"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="pointer"></param>
    /// <param name="fromActor"> May be null. </param>
    public override void Enter( InputEvent? ev, float x, float y, int pointer, Actor? fromActor )
    {
        if ( ( pointer == -1 ) && !_cancelled )
        {
            _over = true;
        }
    }

    /// <summary>
    /// Called any time the mouse cursor or a finger touch is moved out of an actor.
    /// On the desktop, this event occurs even when no mouse buttons are pressed
    /// (pointer will be -1).
    /// </summary>
    /// <param name="ev"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="pointer"></param>
    /// <param name="toActor"> May be null. </param>
    /// <see cref="InputEvent "/>
    public override void Exit( InputEvent? ev, float x, float y, int pointer, Actor? toActor )
    {
        if ( ( pointer == -1 ) && !_cancelled )
        {
            _over = false;
        }
    }

    /// <summary>
    /// If a touch down is being monitored, the drag and touch up events are
    /// ignored until the next touch up.
    /// </summary>
    public virtual void Cancel()
    {
        if ( PressedPointer == -1 )
        {
            return;
        }

        _cancelled = true;
        Pressed    = false;
    }

    /// <summary>
    /// </summary>
    /// <param name="ev"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public virtual void OnClicked( InputEvent ev, float x, float y )
    {
        _action?.Invoke( ev, x, y );
    }

    /// <summary>
    /// Returns true if the specified position is over the specified
    /// actor or within the tap square.
    /// </summary>
    public bool IsOver( Actor? actor, float x, float y )
    {
        Actor? hit = actor?.Hit( x, y, true );

        if ( ( hit == null ) || !hit.IsDescendantOf( actor ) )
        {
            return InTapSquare( x, y );
        }

        return true;
    }

    /// <summary>
    /// Returns true if the supplied x and y coordinates are within the tap square.
    /// </summary>
    public bool InTapSquare( float x, float y )
    {
        if ( Math.Abs( TouchDownX - ( -1f ) ) < NumberUtils.FloatTolerance
          && Math.Abs( TouchDownY - ( -1f ) ) < NumberUtils.FloatTolerance )
        {
            return false;
        }

        return ( Math.Abs( x - TouchDownX ) < TapSquareSize )
            && ( Math.Abs( y - TouchDownY ) < TapSquareSize );
    }

    /// <summary>
    /// Returns true if a touch is within the tap square.
    /// </summary>
    public bool InTapSquare()
    {
        return !TouchDownX.Equals( -1 );
    }

    /// <summary>
    /// The tap square will no longer be used for the current touch.
    /// </summary>
    public void InvalidateTapSquare()
    {
        TouchDownX = -1;
        TouchDownY = -1;
    }

    /// <summary>
    /// Returns true if a touch is over the actor or within the tap square or
    /// has been very recently. This allows the UI to show a press and release
    /// that was so fast it occurred within a single frame.
    /// </summary>
    public bool VisualPressed
    {
        get
        {
            if ( Pressed )
            {
                return true;
            }

            if ( _visualPressedTime <= 0 )
            {
                return false;
            }

            if ( _visualPressedTime > TimeUtils.Millis() )
            {
                return true;
            }

            _visualPressedTime = 0;

            return false;
        }
        set
        {
            if ( value )
            {
                _visualPressedTime = TimeUtils.Millis() + ( long )( VisualPressedDuration * 1000 );
            }
            else
            {
                _visualPressedTime = 0;
            }
        }
    }

    /// <summary>
    /// Returns true if the mouse or touch is over the actor or pressed
    /// and within the tap square.
    /// </summary>
    public bool Over => _over || Pressed;

    /// <summary>
    /// Sets the button to listen for, all other buttons are ignored.
    /// Default is <see cref="IInput.Buttons.Left"/>.
    /// Use -1 for any button.
    /// </summary>
    public long TapCountInterval
    {
        get;
        set;
    } = ( long )( 0.4f * 1000000000L );
}

// ============================================================================
// ============================================================================

