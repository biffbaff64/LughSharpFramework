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

using LughSharp.Source.Scene2D.Utils;

namespace LughSharp.Source.Scene2D.Actions;

/// <summary>
/// Base class for actions that transition over time using percent complete.
/// </summary>
[PublicAPI]
public abstract class TemporalAction( float duration, IInterpolation? interpolation )
    : SceneAction
{
    /// <summary>
    /// Indicates whether the action's progression is reversed, causing it to complete by
    /// going from 1 to 0 instead of 0 to 1 over its duration.
    /// </summary>
    public bool Reverse { get; set; }

    /// <summary>
    /// Specifies the total duration for the action to complete, measured in seconds.
    /// Determines the amount of time the action takes to transition from start to finish.
    /// </summary>
    public float Duration { get; set; } = duration;

    /// <summary>
    /// Represents the elapsed time in seconds since the start of the action.
    /// Used to calculate the progression of the action based on the total duration.
    /// </summary>
    public float Time { get; set; }

    /// <summary>
    /// Controls how the progression of the action is interpolated over time, enabling
    /// non-linear transitions. This property modifies the percentage completion value,
    /// allowing for easing effects such as acceleration and deceleration. If set to null,
    /// the action progresses linearly.
    /// </summary>
    public IInterpolation? Interpolation { get; set; } = interpolation;

    /// <summary>
    /// Returns true after <see cref="Act(float)"/> has been called where time >= duration.
    /// </summary>
    public bool IsComplete { get; private set; }

    // ========================================================================
    
    private bool _began;

    // ========================================================================

    /// <summary>
    /// Default constructor for <see cref="TemporalAction"/>. Initializes the action
    /// with a duration of 0.
    /// </summary>
    protected TemporalAction() : this( 0 )
    {
    }
    
    /// <summary>
    /// Base class for actions that transition over time using percent completion.
    /// </summary>
    protected TemporalAction( float duration ) : this( duration, null )
    {
    }

    /// <summary>
    /// Updates the action based on time.
    /// Typically this is called each frame by <see cref="SceneAction.Actor"/>.
    /// </summary>
    /// <param name="delta">Time in seconds since the last frame.</param>
    /// <returns>
    /// true if the action is done. This method may continue to be called after
    /// the action is done.
    /// </returns>
    public override bool Act( float delta )
    {
        if ( IsComplete )
        {
            return true;
        }

        IScenePool? pool = Pool;

        // Ensure this action can't be returned to the pool while executing.
        Pool = null;

        try
        {
            if ( !_began )
            {
                BeginAction();
                _began = true;
            }

            Time       += delta;
            IsComplete =  Time >= Duration;

            float percent = IsComplete ? 1 : Time / Duration;

            if ( Interpolation != null )
            {
                percent = Interpolation.Apply( percent );
            }

            Update( Reverse ? 1 - percent : percent );

            if ( IsComplete )
            {
                EndAction();
            }

            return IsComplete;
        }
        finally
        {
            Pool = pool;
        }
    }

    /// <summary>
    /// Called the first time <see cref="Act(float)"/> is called. This is a good place
    /// to query the <see cref="Actor"/>'s starting state.
    /// </summary>
    /// <remarks>
    /// This default implementation does nothing. To add functionality, override this method.
    /// </remarks>
    protected virtual void BeginAction()
    {
    }

    /// <summary>
    /// Called the last time <see cref="Act(float)"/> is called.
    /// </summary>
    /// <remarks>
    /// This default implementation does nothing. To add functionality, override this method.
    /// </remarks>
    protected virtual void EndAction()
    {
    }

    /// <summary>
    /// Called each frame.
    /// </summary>
    /// <param name="percent">
    /// The percentage of completion for this action, growing from 0 to 1 over the
    /// duration. If <see cref="Reverse"/> is true, this will shrink from 1 to 0.
    /// </param>
    protected abstract void Update( float percent );

    /// <summary>
    /// Skips to the end of the transition.
    /// </summary>
    public virtual void Finish()
    {
        Time = Duration;
    }

    /// <summary>
    /// Sets the state of the action so it can be run again.
    /// Default implementation does nothing.
    /// </summary>
    public override void Restart()
    {
        Time       = 0;
        _began     = false;
        IsComplete = false;
    }

    /// <summary>
    /// Resets the optional state of this action as if it were newly created, allowing the
    /// action to be pooled and reused. State required to be set for every usage of this action
    /// or computed during the action does not need to be reset.
    /// <para>
    /// The default implementation should call <see cref="SceneAction.Restart"/>
    /// </para>
    /// <para>
    /// If a subclass has optional state, it must override this method, call super, and reset
    /// the optional state.
    /// </para>
    /// </summary>
    public override void Reset()
    {
        base.Reset();

        Reverse       = false;
        Interpolation = null;
    }
}

// ============================================================================
// ============================================================================

