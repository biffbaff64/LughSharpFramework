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

using LughSharp.Source.Graphics.G2D;
using LughSharp.Source.Graphics.Images;
using LughSharp.Source.Scene2D.Listeners;
using LughSharp.Source.Scene2D.UI.Styles;
using LughSharp.Source.Scene2D.Utils;
using LughSharp.Source.Utils.Pooling;

namespace LughSharp.Source.Scene2D.UI;

/// <summary>
/// A progress bar is a widget that visually displays the progress of some activity
/// or a value within given range. The progress bar has a range (min, max) and a
/// stepping between each value it represents. The percentage of completeness typically
/// starts out as an empty progress bar and gradually becomes filled in as the task
/// or variable value progresses.
/// <para>
/// A <see cref="ChangeListener.ChangeEvent"/> is fired when the progress bar knob is
/// moved. Cancelling the event will move the knob to where it was previously.
/// </para>
/// <para>
/// For a horizontal progress bar, its preferred height is determined by the larger of
/// the knob and background, and the preferred width is 140, a relatively arbitrary size.
/// These parameters are reversed for a vertical progress bar.
/// </para>
/// </summary>
/// <remarks>
/// A ProgressBar does not contain any support for user interaction. The Knob Position
/// can only be set programmatically and cannot be interacted with by the user. For
/// a progress bar that can be interacted with, see <see cref="Slider"/>.
/// </remarks>
[PublicAPI]
[ActorDefinition( Role = "UI" )]
public class ProgressBar : Widget, IDisableable
{
    public float KnobPosition { get; set; }
    public float MinValue     { get; set; }
    public float MaxValue     { get; set; }
    public float Value        { get; set; }
    public bool  IsVertical   { get; set; }
    public bool  IsRound      { get; set; } = true;
    public bool  IsDisabled   { get; set; }

    public Interpolator AnimateInterpolation { get; set; } = Interpolation.Linear;
    public Interpolator VisualInterpolation  { get; set; } = Interpolation.Linear;

    // ========================================================================

    private const float DefaultPrefWidth  = 140f;
    private const float DefaultPrefHeight = 140f;

    private readonly bool _programmaticChangeEvents = true;

    private float _animateDuration;
    private float _animateFromValue;
    private float _animateTime;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="stepSize"></param>
    /// <param name="vertical"></param>
    /// <param name="skin"></param>
    public ProgressBar( float min, float max, float stepSize, bool vertical, Skin skin )
        : this( min,
                max,
                stepSize,
                vertical,
                skin.Get< ProgressBarStyle >( $"default{( vertical ? "-vertical" : "-horizontal" )}" ) )
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="stepSize"></param>
    /// <param name="vertical"></param>
    /// <param name="skin"></param>
    /// <param name="styleName"></param>
    public ProgressBar( float min, float max, float stepSize, bool vertical, Skin skin, string styleName )
        : this( min, max, stepSize, vertical, skin.Get< ProgressBarStyle >( styleName ) )
    {
    }

    /// <summary>
    /// Creates a new progress bar. If horizontal, its width is determined by the
    /// prefWidth parameter, and its height is determined by the maximum of the height
    /// of either the progress bar <see cref="NinePatch"/> or progress bar handle
    /// <see cref="TextureRegion"/>. The min and max values determine the range the
    /// values of this progress bar can take on, the stepSize parameter specifies the
    /// distance between individual values.
    /// <para>
    /// E.g. min could be 4, max could be 10 and stepSize could be 0.2, giving you a
    /// total of 30 values, 4.0 4.2, 4.4 and so on.
    /// </para>
    /// </summary>
    /// <param name="min"> the minimum value </param>
    /// <param name="max"> the maximum value </param>
    /// <param name="stepSize"> the step size between values </param>
    /// <param name="vertical"></param>
    /// <param name="style"> the <see cref="ProgressBarStyle"/>  </param>
    public ProgressBar( float min, float max, float stepSize, bool vertical, ProgressBarStyle style )
    {
        if ( min > max )
        {
            throw new ArgumentException( $"max must be > min. min,max: {min}, {max}" );
        }

        if ( stepSize <= 0 )
        {
            throw new ArgumentException( $"stepSize must be > 0. stepSize: {stepSize}" );
        }

        Style = style;

        MinValue   = min;
        MaxValue   = max;
        StepSize   = stepSize;
        IsVertical = vertical;
        Value      = min;

        SetSize( GetPrefWidthSafe(), GetPrefHeightSafe() );
    }

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    public bool IsAnimating => _animateTime > 0;

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    public float StepSize
    {
        get;
        init
        {
            if ( value <= 0 )
            {
                throw new ArgumentException( $"steps must be > 0: {field}" );
            }

            field = value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public ProgressBarStyle Style
    {
        get;
        set
        {
            Guard.Against.Null( value );

            field = value;
            InvalidateHierarchy();
        }
    }

    /// <inheritdoc />
    public override void Act( float delta )
    {
        base.Act( delta );

        if ( _animateTime > 0 )
        {
            _animateTime -= delta;

            if ( Stage is { ActionsRequestRendering: true } )
            {
                Engine.Graphics.RequestRendering();
            }
        }
    }

    /// <inheritdoc />
    public override void Draw( IBatch batch, float parentAlpha )
    {
        ISceneDrawable? knob        = Style.Knob;
        ISceneDrawable? currentKnob = GetKnobDrawable();
        ISceneDrawable? bg          = GetBackgroundDrawable();
        ISceneDrawable? knobBefore  = GetKnobBeforeDrawable();
        ISceneDrawable? knobAfter   = GetKnobAfterDrawable();

        float x          = X;
        float y          = Y;
        float width      = Width;
        float height     = Height;
        float knobHeight = knob?.MinHeight ?? 0;
        float knobWidth  = knob?.MinWidth ?? 0;
        float percent    = GetVisualPercent();

        batch.SetColor( ActorColor.R, ActorColor.G, ActorColor.B, ActorColor.A * parentAlpha );

        if ( IsVertical )
        {
            DrawVerticalBar();
        }
        else
        {
            DrawHorizontalBar();
        }

        return;

        // --------------------------------------

        void DrawVerticalBar()
        {
            float bgTopHeight    = 0;
            float bgBottomHeight = 0;

            if ( bg != null )
            {
                DrawRound( batch,
                           bg,
                           x + ( ( width - bg.MinWidth ) * 0.5f ),
                           y,
                           bg.MinWidth,
                           height );

                bgTopHeight    =  bg.TopHeight;
                bgBottomHeight =  bg.BottomHeight;
                height         -= bgTopHeight + bgBottomHeight;
            }

            float total          = height - knobHeight;
            float beforeHeight   = MathUtils.Clamp( ( total * percent ), 0, total );
            float knobHeightHalf = knobHeight * 0.5f;

            KnobPosition = bgBottomHeight + beforeHeight;

            if ( knobBefore != null )
            {
                DrawRound( batch,
                           knobBefore,
                           x + ( ( width - knobBefore.MinWidth ) * 0.5f ),
                           y + bgBottomHeight,
                           knobBefore.MinWidth,
                           beforeHeight + knobHeightHalf );
            }

            if ( knobAfter != null )
            {
                DrawRound( batch,
                           knobAfter,
                           x + ( ( width - knobAfter.MinWidth ) * 0.5f ),
                           y + KnobPosition + knobHeightHalf,
                           knobAfter.MinWidth,
                           total - ( IsRound
                               ? ( float )Math.Ceiling( beforeHeight - knobHeightHalf )
                               : beforeHeight - knobHeightHalf ) );
            }

            if ( currentKnob != null )
            {
                float w = currentKnob.MinWidth, h = currentKnob.MinHeight;
                DrawRound( batch,
                           currentKnob,
                           x + ( ( width - w ) * 0.5f ),
                           y + KnobPosition + ( ( knobHeight - h ) * 0.5f ),
                           w,
                           h );
            }
        }

        // --------------------------------------

        void DrawHorizontalBar()
        {
            float bgLeftWidth  = 0;
            float bgRightWidth = 0;

            if ( bg != null )
            {
                DrawRound( batch,
                           bg,
                           x,
                           ( float )Math.Round( y + ( ( height - bg.MinHeight ) * 0.5f ) ),
                           width,
                           ( float )Math.Round( bg.MinHeight ) );
                bgLeftWidth  =  bg.LeftWidth;
                bgRightWidth =  bg.RightWidth;
                width        -= bgLeftWidth + bgRightWidth;
            }

            float total       = width - knobWidth;
            float beforeWidth = MathUtils.Clamp( total * percent, 0, total );

            KnobPosition = bgLeftWidth + beforeWidth;

            float knobWidthHalf = knobWidth * 0.5f;

            if ( knobBefore != null )
            {
                DrawRound( batch,
                           knobBefore,
                           x + bgLeftWidth,
                           y + ( ( height - knobBefore.MinHeight ) * 0.5f ),
                           beforeWidth + knobWidthHalf,
                           knobBefore.MinHeight );
            }

            if ( knobAfter != null )
            {
                DrawRound( batch,
                           knobAfter,
                           x + KnobPosition + knobWidthHalf,
                           y + ( ( height - knobAfter.MinHeight ) * 0.5f ),
                           total - ( IsRound
                               ? ( float )Math.Ceiling( beforeWidth - knobWidthHalf )
                               : beforeWidth - knobWidthHalf ),
                           knobAfter.MinHeight );
            }

            if ( currentKnob != null )
            {
                float w = currentKnob.MinWidth, h = currentKnob.MinHeight;
                DrawRound( batch,
                           currentKnob,
                           x + KnobPosition + ( ( knobWidth - w ) * 0.5f ),
                           y + ( ( height - h ) * 0.5f ),
                           w,
                           h );
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="drawable"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="w"></param>
    /// <param name="h"></param>
    private void DrawRound( IBatch batch, ISceneDrawable drawable, float x, float y, float w, float h )
    {
        if ( IsRound )
        {
            x = ( float )Math.Floor( x );
            y = ( float )Math.Floor( y );
            w = ( float )Math.Ceiling( w );
            h = ( float )Math.Ceiling( h );
        }

        drawable.Draw( batch, x, y, w, h );
    }

    /// <summary>
    /// Sets the progress bar position, rounded to the nearest step size and
    /// clamped to the minimum and maximum values. <see cref="Clamp(float)"/>
    /// can be overridden to allow values outside of the progress bar's min/max
    /// range.
    /// </summary>
    /// <returns>
    /// <tt>false</tt> if the value was not changed because the progress bar
    /// already had the value or it was canceled by a listener.
    /// </returns>
    public bool SetBarPosition( float value )
    {
        value = Clamp( Round( value ) );

        float oldValue = Value;

        if ( value.Equals( oldValue ) )
        {
            return false;
        }

        float oldVisualValue = GetVisualValue();

        Value = value;

        if ( _programmaticChangeEvents )
        {
            var  changeEvent = Pools.Obtain< ChangeListener.ChangeEvent >();
            bool cancelled   = Fire( changeEvent );

            // It is safe to suppress nullability warnings for 'changeEvent'
            // here because Fire() will throw an exception is it is null.
            Pools.Free< ChangeListener.ChangeEvent >( changeEvent );

            if ( cancelled )
            {
                Value = oldValue;

                return false;
            }
        }

        if ( _animateDuration > 0 )
        {
            _animateFromValue = oldVisualValue;
            _animateTime      = _animateDuration;
        }

        return true;
    }

    /// <summary>
    /// Sets the progress bar position, rounded to the nearest step size and clamped to the
    /// minimum and maximum values. <see cref="Clamp(float)"/> can be overridden to allow
    /// values outside of the progress bar's min/max range.
    /// </summary>
    /// <returns>
    /// <c>false</c> if the value was not changed because the progress bar already had the
    /// value or it was canceled by a listener.
    /// </returns>
    public bool SetValue( float value )
    {
        value = Clamp( Round( value ) );
        float oldValue = Value;

        if ( Math.Abs( Value - oldValue ) < NumberUtils.FloatTolerance ) return false;

        float oldVisualValue = GetVisualValue();
        Value = value;

        if ( _programmaticChangeEvents )
        {
            ChangeListener.ChangeEvent changeEvent = Pools.Obtain< ChangeListener.ChangeEvent > ();
            bool     cancelled   = Fire( changeEvent );

            Pools.Free( changeEvent );

            if ( cancelled )
            {
                Value = oldValue;

                return false;
            }
        }

        if ( _animateDuration > 0 )
        {
            _animateFromValue = oldVisualValue;
            _animateTime      = _animateDuration;
        }

        return true;
    }

    /// <summary>
    /// Rounds the value using the progress bar's step size.
    /// This can be overridden to customize or disable rounding.
    /// </summary>
    private float Round( float value )
    {
        return ( float )( Math.Round( value / StepSize ) * StepSize );
    }

    /// <summary>
    /// Clamps the value to the progress bar's min/max range. This can be overridden
    /// to allow a range different from the progress bar knob's range.
    /// </summary>
    private float Clamp( float value )
    {
        return MathUtils.Clamp( value, MinValue, MaxValue );
    }

    /// <summary>
    /// Sets the range of this progress bar. The progress bar's current value is clamped to the range.
    /// </summary>
    public void SetRange( float min, float max )
    {
        if ( min > max )
        {
            throw new ArgumentException( $"min must be <= max: {min} <= {max}" );
        }

        MinValue = min;
        MaxValue = max;

        if ( Value < min )
        {
            SetValue( min );
        }
        else if ( Value > max )
        {
            SetValue( max );
        }
    }

    /// <inheritdoc />
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
        if ( IsVertical )
        {
            ISceneDrawable? knob = Style.Knob;
            ISceneDrawable? bg   = GetBackgroundDrawable();

            return Math.Max( knob?.MinWidth ?? 0, bg?.MinWidth ?? 0 );
        }

        return DefaultPrefWidth;
    }

    /// <inheritdoc />
    public override float GetPrefHeight()
    {
        return GetPrefHeightSafe();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected float GetPrefHeightSafe()
    {
        if ( IsVertical )
        {
            return DefaultPrefHeight;
        }

        ISceneDrawable? knob = Style.Knob;
        ISceneDrawable? bg   = GetBackgroundDrawable();

        return Math.Max( knob?.MinHeight ?? 0, bg?.MinHeight ?? 0 );
    }

    /// <summary>
    /// If > 0, changes to the progress bar value via <see cref="Value"/>
    /// will happen over this duration in seconds.
    /// </summary>
    /// <param name="duration"></param>
    public void SetAnimateDuration( float duration )
    {
        _animateDuration = duration;
    }

    /// <summary>
    /// If <see cref="SetAnimateDuration(float)"/> animating the progress bar value,
    /// this returns the value current displayed.
    /// </summary>
    public float GetVisualValue()
    {
        if ( _animateTime > 0 )
        {
            return AnimateInterpolation.Apply( _animateFromValue,
                                               Value,
                                               1 - ( _animateTime / _animateDuration ) );
        }

        return Value;
    }

    /// <summary>
    /// Sets the visual value equal to the actual value. This can be used to set the
    /// value without animating.
    /// </summary>
    public virtual void UpdateVisualValue()
    {
        //TODO:
        _animateTime = 0;
    }

    /// <summary>
    /// Calculates the progress of the <see cref="ProgressBar"/> as a percentage. The
    /// percentage is determined by the current value relative to the minimum and
    /// maximum values of the progress bar.
    /// </summary>
    /// <returns>The progress represented as a float value between 0 and 1.</returns>
    public float GetPercent()
    {
        if ( MinValue.Equals( MaxValue ) )
        {
            return 0;
        }

        return ( Value - MinValue ) / ( MaxValue - MinValue );
    }

    /// <summary>
    /// Calculates the visual percentage position of the progress bar's value relative to
    /// its minimum and maximum values, using the configured interpolation method.
    /// </summary>
    /// <returns>
    /// A value between 0 and 1 representing the visual position of the progress bar's
    /// value as a percentage. Returns 0 if the minimum and maximum values are the same.
    /// </returns>
    public float GetVisualPercent()
    {
        if ( MinValue.Equals( MaxValue ) )
        {
            return 0;
        }

        return VisualInterpolation.Apply( ( GetVisualValue() - MinValue ) / ( MaxValue - MinValue ) );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ISceneDrawable? GetBackgroundDrawable()
    {
        if ( IsDisabled && ( Style.DisabledBackground != null ) )
        {
            return Style.DisabledBackground;
        }

        return Style.Background;
    }

    /// <summary>
    /// Retrieves the drawable used for the knob of the progress bar. If the progress bar
    /// is disabled and a specific disabled knob drawable is defined in the style, it
    /// returns the disabled knob drawable. Otherwise, it returns the default knob drawable
    /// defined in the style.
    /// </summary>
    /// <returns>
    /// The <see cref="ISceneDrawable"/> instance representing the knob drawable, or null
    /// if no drawable is defined.
    /// </returns>
    private ISceneDrawable? GetKnobDrawable()
    {
        if ( IsDisabled && ( Style.DisabledKnob != null ) )
        {
            return Style.DisabledKnob;
        }

        return Style.Knob;
    }

    /// <summary>
    /// Retrieves the drawable representation of the area before the progress bar knob. If
    /// the progress bar is disabled and a specific drawable for the disabled state is set,
    /// it will return the disabled drawable. Otherwise, it will return the default drawable
    /// for the knob-before region, as specified in the <see cref="ProgressBarStyle"/>.
    /// </summary>
    /// <returns>
    /// The <see cref="ISceneDrawable"/> representing the drawable before the knob
    /// or null if no drawable is set.
    /// </returns>
    private ISceneDrawable? GetKnobBeforeDrawable()
    {
        if ( IsDisabled && ( Style.DisabledKnobBefore != null ) )
        {
            return Style.DisabledKnobBefore;
        }

        return Style.KnobBefore;
    }

    /// <summary>
    /// Retrieves the drawable used to represent the portion of the progress bar
    /// after the knob. If the progress bar is disabled and a specific drawable for
    /// the disabled state is defined, it will return the drawable for the disabled
    /// state; otherwise, it returns the default drawable for the portion after the knob.
    /// </summary>
    /// <returns>
    /// The drawable for the portion of the progress bar after the knob, or the disabled
    /// version of it if the progress bar is disabled and a disabled drawable is defined.
    /// </returns>
    private ISceneDrawable? GetKnobAfterDrawable()
    {
        if ( IsDisabled && ( Style.DisabledKnobAfter != null ) )
        {
            return Style.DisabledKnobAfter;
        }

        return Style.KnobAfter;
    }
}

// ============================================================================
// ============================================================================