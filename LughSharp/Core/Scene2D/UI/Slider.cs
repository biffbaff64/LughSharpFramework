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

using LughSharp.Core.Graphics.Images;
using LughSharp.Core.Scene2D.Listeners;
using LughSharp.Core.Scene2D.UI.Styles;
using LughSharp.Core.Scene2D.Utils;
using LughSharp.Core.Utils.Pooling;

namespace LughSharp.Core.Scene2D.UI;

/// <summary>
/// A slider is a horizontal indicator that allows a user to set a value. The slider has
/// a range (min, max) and a stepping between each value the slider represents.
/// <para>
/// <see cref="ChangeListener.ChangeEvent"/> is fired when the slider knob is moved.
/// Canceling the event will move the knob to where it was previously.
/// </para>
/// <para>
/// For a horizontal progress bar, its preferred height is determined by the larger of the
/// knob and background, and the preferred width is 140, a relatively arbitrary size. These
/// parameters are reversed for a vertical progress bar.
/// </para>
/// </summary>
/// <remarks>
/// A Slider is the interactive equivalent of a <see cref="ProgressBar"/>. Use this over a
/// ProgressBar if you want to allow the user to set a value.
/// </remarks>
[PublicAPI]
[ActorDefinition( Role = "UI" )]
public class Slider : ProgressBar
{
    private int      _draggingPointer = -1;
    private float[]? _snapValues;
    private float    _threshold;

    // ========================================================================

    /// <summary>
    /// True if the mouse is currently over the slider.
    /// </summary>
    public bool MouseOver { get; set; }

    /// <summary>
    /// Sets the mouse button, which can trigger a change of the slider.
    /// Is set to -1, so every button, by default.
    /// </summary>
    public int MouseButton { get; set; } = -1;

    /// <summary>
    /// Sets the inverse interpolation to use for display. This should perform the
    /// inverse of setting <see cref="ProgressBar.VisualInterpolation"/>.
    /// </summary>
    public Interpolator VisualInterpolationInverse { get; set; } = Interpolation.Linear;

    // ========================================================================

    /// <summary>
    /// Creates a new slider with the default style obtained from the supplied <see cref="Skin"/>.
    /// </summary>
    /// <param name="min"> the minimum value </param>
    /// <param name="max"> the maximum value </param>
    /// <param name="stepSize"> the step size between values </param>
    /// <param name="vertical"> True if the slider is to be drawn vertically. otherwise false. </param>
    /// <param name="skin"> The Skin holding the style. </param>
    public Slider( float min, float max, float stepSize, bool vertical, Skin skin )
        : this( min,
                max,
                stepSize,
                vertical,
                skin.Get< SliderStyle >( "default" + ( vertical ? "-vertical" : "" ) ) )
    {
    }

    /// <summary>
    /// Creates a new slider with the named style obtained from the supplied <see cref="Skin"/>.
    /// </summary>
    /// <param name="min"> the minimum value </param>
    /// <param name="max"> the maximum value </param>
    /// <param name="stepSize"> the step size between values </param>
    /// <param name="vertical"> True if the slider is to be drawn vertically. otherwise false. </param>
    /// <param name="skin"> The Skin holding the style. </param>
    /// <param name="styleName">
    /// The Style name from the style held in the Skin. Usuallly either <c>default</c>
    /// or <c>default-vertical</c>, but can be any style name registered in the Skin.
    /// </param>
    public Slider( float min, float max, float stepSize, bool vertical, Skin skin, string styleName )
        : this( min, max, stepSize, vertical, skin.Get< SliderStyle >( styleName ) )
    {
    }

    /// <summary>
    /// Creates a new slider. If horizontal, its width is determined by the prefWidth
    /// parameter, its height is determined by the maximum of the height of either the
    /// slider <see cref="NinePatch"/> or slider handle <see cref="TextureRegion"/>.
    /// The min and max values determine the range the values of this slider can take on,
    /// the stepSize parameter specifies the distance between individual values. E.g. min
    /// could be 4, max could be 10 and stepSize could be 0.2, giving you a total of 30
    /// values, 4.0 4.2, 4.4 and so on.
    /// </summary>
    /// <param name="min"> the minimum value </param>
    /// <param name="max"> the maximum value </param>
    /// <param name="stepSize"> the step size between values </param>
    /// <param name="vertical"> True if the slider is to be drawn vertically. otherwise false. </param>
    /// <param name="style"> the <see cref="SliderStyle"/> </param>
    public Slider( float min, float max, float stepSize, bool vertical, SliderStyle style )
        : base( min, max, stepSize, vertical, style )
    {
        if ( !AddListener( new SliderInputListener( this ) ) )
        {
            throw new ListenerFailureException( "Failed to add slider input listener" );
        }
    }

    /// <summary>
    /// Sets this actor as the event target and propagates the event to this actor and
    /// ascendants as necessary. If this actor is not in the stage, the stage must be
    /// set before calling this method.
    /// <para>
    /// Events are fired in 2 phases:
    /// <li>
    /// The first phase (the "capture" phase) notifies listeners on each actor starting
    /// at the root and propagating down the hierarchy to (and including) this actor.
    /// </li>
    /// <li>
    /// The second phase notifies listeners on each actor starting at this actor and, if
    /// <see cref="Event.Bubbles()"/> is true, propagating upward to the root.
    /// </li>
    /// </para>
    /// <para>
    /// If the event is stopped at any time, it will not propagate to the next actor.
    /// </para>
    /// </summary>
    /// <param name="ev"> The <see cref="Event"/> to fire. </param>
    /// <returns> True if the event was cancelled. </returns>
    public override bool Fire( Event? ev )
    {
        Logger.Checkpoint();

        return base.Fire( ev );
    }

    /// <summary>
    /// Responsible for notifying event listeners of an event.
    /// <para>
    /// This method first verifies that the event has a valid target actor. Depending on
    /// whether the event is in the capture phase, it selects the appropriate listener list.
    /// It then iterates through these listeners and notifies them of the event. If any
    /// listener handles the event, the event is marked as handled.
    /// </para>
    /// <para>
    /// If an exception occurs during this process, a new exception is thrown with additional
    /// context.
    /// </para>
    /// </summary>
    /// <param name="ev"> The event. </param>
    /// <param name="capture">
    /// true for <see cref="Actor.CaptureListeners"/>, false for <see cref="Actor.Listeners"/>.
    /// </param>
    /// <returns></returns>
    public override bool Notify( Event ev, bool capture )
    {
        Logger.Checkpoint();

        return base.Notify( ev, capture );
    }

    /// <summary>
    /// Returns the appropriate <see cref="ISceneDrawable"/> for the slider background,
    /// based on the current state of the slider.
    /// </summary>
    protected ISceneDrawable? GetBackgroundDrawable()
    {
        var style = ( SliderStyle )Style;

        if ( IsDisabled && ( style.DisabledBackground != null ) )
        {
            return style.DisabledBackground;
        }

        if ( IsDragging() && ( style.BackgroundDown != null ) )
        {
            return style.BackgroundDown;
        }

        if ( MouseOver && ( style.BackgroundOver != null ) )
        {
            return style.BackgroundOver;
        }

        return style.Background;
    }

    /// <summary>
    /// Returns the appropriate <see cref="ISceneDrawable"/> for the slider knob,
    /// based on the current state of the slider.
    /// </summary>
    protected ISceneDrawable? GetKnobDrawable()
    {
        var style = ( SliderStyle )Style;

        if ( IsDisabled && ( style.DisabledKnob != null ) )
        {
            return style.DisabledKnob;
        }

        if ( IsDragging() && ( style.KnobDown != null ) )
        {
            return style.KnobDown;
        }

        if ( MouseOver && ( style.KnobOver != null ) )
        {
            return style.KnobOver;
        }

        return style.Knob;
    }

    /// <summary>
    /// Retrieves the drawable for the section of the slider before the knob, based
    /// on the current state of the slider.
    /// </summary>
    /// <returns>
    /// The <see cref="ISceneDrawable"/> representing the knob-before drawable. Returns
    /// a disabled, down, hovered, or default drawable, depending on the slider's state
    /// and the presence of corresponding assets in the style.
    /// </returns>
    protected ISceneDrawable? GetKnobBeforeDrawable()
    {
        var style = ( SliderStyle )Style;

        if ( IsDisabled && ( style.DisabledKnobBefore != null ) )
        {
            return style.DisabledKnobBefore;
        }

        if ( IsDragging() && ( style.KnobBeforeDown != null ) )
        {
            return style.KnobBeforeDown;
        }

        if ( MouseOver && ( style.KnobBeforeOver != null ) )
        {
            return style.KnobBeforeOver;
        }

        return style.KnobBefore;
    }

    /// <summary>
    /// Retrieves the drawable associated with the slider's "knob after" visual representation
    /// based on the current state of the slider, such as being disabled, dragged, or hovered over.
    /// </summary>
    /// <returns>
    /// The appropriate <see cref="ISceneDrawable"/> for the "knob after" based on the slider's state.
    /// Returns <c>DisabledKnobAfter</c> if the slider is disabled and it is available,
    /// <c>KnobAfterDown</c> if the slider is being dragged and it is available,
    /// <c>KnobAfterOver</c> if the mouse is over the slider and it is available,
    /// or <c>KnobAfter</c> otherwise.
    /// </returns>
    protected ISceneDrawable? GetKnobAfterDrawable()
    {
        var style = ( SliderStyle )Style;

        if ( IsDisabled && ( style.DisabledKnobAfter != null ) )
        {
            return style.DisabledKnobAfter;
        }

        if ( IsDragging() && ( style.KnobAfterDown != null ) )
        {
            return style.KnobAfterDown;
        }

        if ( MouseOver && ( style.KnobAfterOver != null ) )
        {
            return style.KnobAfterOver;
        }

        return style.KnobAfter;
    }

    /// <summary>
    /// Calculates the slider's position and value based on the given input coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate of the input event.</param>
    /// <param name="y">The y-coordinate of the input event.</param>
    /// <returns>True if the value was successfully set, otherwise false.</returns>
    private bool CalculatePositionAndValue( float x, float y )
    {
        SliderStyle     style = ( SliderStyle )Style;
        ISceneDrawable? knob  = style.Knob;
        ISceneDrawable? bg    = GetBackgroundDrawable();

        float value;
        float oldPosition = KnobPosition;

        float min = MinValue;
        float max = MaxValue;

        if ( IsVertical )
        {
            float height     = Height - bg!.TopHeight - bg.BottomHeight;
            float knobHeight = knob?.MinHeight ?? 0;

            KnobPosition = y - bg.BottomHeight - ( knobHeight * 0.5f );
            value = min + ( ( max - min )
                          * VisualInterpolationInverse.Apply( KnobPosition / ( height - knobHeight ) ) );
            KnobPosition = Math.Max( Math.Min( 0, bg.BottomHeight ), KnobPosition );
            KnobPosition = Math.Min( height - knobHeight, KnobPosition );
        }
        else
        {
            float width     = Width - bg!.LeftWidth - bg.RightWidth;
            float knobWidth = knob?.MinWidth ?? 0;

            KnobPosition = x - bg.LeftWidth - ( knobWidth * 0.5f );
            value = min + ( ( max - min ) * VisualInterpolationInverse.Apply( KnobPosition / ( width - knobWidth ) ) );
            KnobPosition = Math.Max( Math.Min( 0, bg.LeftWidth ), KnobPosition );
            KnobPosition = Math.Min( width - knobWidth, KnobPosition );
        }

        float oldValue = value;

        if ( !Engine.Input.IsKeyPressed( IInput.Keys.ShiftLeft )
          && !Engine.Input.IsKeyPressed( IInput.Keys.ShiftRight ) )
        {
            value = GetSnapped( value );
        }

        bool valueSet = SetBarPosition( value );

        if ( value.Equals( oldValue ) )
        {
            KnobPosition = oldPosition;
        }

        return valueSet;
    }

    /// <summary>
    /// Adjusts the provided value to the nearest snap value based on the defined threshold,
    /// or returns the original value if no snap values are set or within range.
    /// </summary>
    /// <param name="value">The value to modify based on snap values.</param>
    /// <returns>
    /// The value adjusted to the nearest snap value or the original value if snapping does
    /// not apply.
    /// </returns>
    protected float GetSnapped( float value )
    {
        if ( _snapValues == null )
        {
            return value;
        }

        float bestDiff  = -1;
        float bestValue = 0;

        foreach ( float snapValue in _snapValues )
        {
            float diff = Math.Abs( value - snapValue );

            if ( diff <= _threshold )
            {
                if ( ( Math.Abs( bestDiff - ( -1 ) ) < NumberUtils.FloatTolerance )
                  || ( diff < bestDiff ) )
                {
                    bestDiff  = diff;
                    bestValue = snapValue;
                }
            }
        }

        return Math.Abs( bestDiff - ( -1f ) ) < NumberUtils.FloatTolerance
            ? value
            : bestValue;
    }

    /// <summary>
    /// Snaps the knob to the specified values, if the knob is within the threshold.
    /// </summary>
    /// <param name="values"> May be null. </param>
    /// <param name="threshld"> The snap threshold. </param>
    public void SetSnapToValues( float[]? values, float threshld )
    {
        _snapValues = values;
        _threshold  = threshld;
    }

    /// <summary>
    /// Returns true if the slider is being dragged.
    /// </summary>
    public bool IsDragging()
    {
        return _draggingPointer != -1;
    }

    /// <summary>
    /// Sets the value using the specified visual percent.
    /// </summary>
    public void SetVisualPercent( float percent )
    {
        Value = MinValue + ( ( MaxValue - MinValue ) * VisualInterpolationInverse.Apply( percent ) );
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Handles input events for the <see cref="Slider"/> widget and allows interaction
    /// via touch gestures to adjust the slider's value. This listener is responsible for
    /// processing user inputs such as touch down, touch up, and touch drag, which directly
    /// manipulate the slider's knob position and value.
    /// </summary>
    /// <remarks>
    /// The <see cref="SliderInputListener"/> is attached to a <see cref="Slider"/> to provide
    /// interactive functionality. It tracks pointer events, validates interactions based on
    /// the slider's properties (e.g., disabled state, specific mouse buttons), and delegates
    /// actions to the slider for calculating changes in value. This enables responsive and
    /// intuitive control for the end user.
    /// </remarks>
    [PublicAPI]
    public class SliderInputListener : InputListener
    {
        private readonly Slider _parent;

        public SliderInputListener( Slider parent )
        {
            _parent = parent;
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
            Logger.Checkpoint();

            if ( _parent.IsDisabled )
            {
                return false;
            }

            if ( ( _parent.MouseButton != -1 ) && ( _parent.MouseButton != button ) )
            {
                return false;
            }

            if ( _parent._draggingPointer != -1 )
            {
                return false;
            }

            _parent._draggingPointer = pointer;
            _parent.CalculatePositionAndValue( x, y );

            return true;
        }

        /// <summary>
        /// Called when a mouse button or a finger touch goes up anywhere, but only
        /// if touchDown previously returned true for the mouse button or touch.
        /// The touchUp event is always handled by <see cref="Event.SetHandled"/>.
        /// </summary>
        public override void OnTouchUp( InputEvent? ev, float x, float y, int pointer, int button )
        {
            if ( pointer != _parent._draggingPointer )
            {
                return;
            }

            // The position is invalid when focus is cancelled
            if ( ev!.TouchFocusCancel || !_parent.CalculatePositionAndValue( x, y ) )
            {
                // Fire an event on touchUp even if the value didn't change, so
                // listeners can see when a drag ends via isDragging.
                var changeEvent = Pools.Obtain< ChangeListener.ChangeEvent >();

                Guard.Against.Null( changeEvent );

                _parent.Fire( changeEvent );
                Pools.Free< ChangeListener.ChangeEvent >( changeEvent );
            }
        }

        /// <summary>
        /// Called when a mouse button or a finger touch is moved anywhere, but only
        /// if touchDown previously returned true for the mouse button or touch.
        /// The touchDragged event is always handled by <see cref="Event.SetHandled"/>.
        /// </summary>
        public override void OnTouchDragged( InputEvent? ev, float x, float y, int pointer )
        {
            _parent.CalculatePositionAndValue( x, y );
        }

        /// <summary>
        /// Called any time the mouse cursor or a finger touch is moved over an actor.
        /// On the desktop, this event occurs even when no mouse buttons are pressed
        /// (pointer will be -1).
        /// </summary>
        /// <param name="ev"> The input event. </param>
        /// <param name="x"> The x coordinate of the mouse cursor or touch. </param>
        /// <param name="y"> The y coordinate of the mouse cursor or touch. </param>
        /// <param name="pointer"> The pointer index of the mouse cursor or touch. </param>
        /// <param name="from"> The actor that the mouse cursor or touch is entering. </param>
        public override void Enter( InputEvent? ev, float x, float y, int pointer, Actor? from )
        {
            if ( pointer == -1 )
            {
                _parent.MouseOver = true;
            }
        }

        /// <summary>
        /// Called any time the mouse cursor or a finger touch is moved out of an actor.
        /// On the desktop, this event occurs even when no mouse buttons are pressed
        /// (pointer will be -1).
        /// </summary>
        /// <param name="ev"> The input event. </param>
        /// <param name="x"> The x coordinate of the mouse cursor or touch. </param>
        /// <param name="y"> The y coordinate of the mouse cursor or touch. </param>
        /// <param name="pointer"> The pointer index of the mouse cursor or touch. </param>
        /// <param name="to"> The actor that the mouse cursor or touch is exiting. </param>
        public override void Exit( InputEvent? ev, float x, float y, int pointer, Actor? to )
        {
            if ( pointer == -1 )
            {
                _parent.MouseOver = false;
            }
        }
    }
}

// ============================================================================
// ============================================================================