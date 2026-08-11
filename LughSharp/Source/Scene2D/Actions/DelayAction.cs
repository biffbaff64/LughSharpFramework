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

namespace LughSharp.Source.Scene2D.Actions;

/// <summary>
/// Represents an action that introduces a delay of a specific duration before executing
/// the next action in a sequence.
/// </summary>
/// <remarks>
/// The <c>DelayAction</c> class allows defining a delay interval during which no other
/// actions are executed. It is typically used in action sequences to create timed gaps
/// between other actions. The action completes after the specified duration.
/// </remarks>
[PublicAPI]
public class DelayAction( float duration ) : DelegateAction
{
    public float Duration { get; set; } = duration;
    public float Time     { get; set; } = 0;

    // ========================================================================

    /// <summary>
    /// Represents an action that delays execution for a specified duration of time.
    /// This action is often used to introduce pauses between consecutive actions
    /// in a sequence or to delay the execution of an action.
    /// </summary>
    /// <remarks>
    /// The <see cref="DelayAction"/> class is derived from <see cref="DelegateAction"/>
    /// and uses a floating-point duration value to control the delay timing. During the delay,
    /// the associated delegate action, if any, will only execute once the delay completes.
    /// </remarks>
    public DelayAction() : this( 0 )
    {
    }

    /// <summary>
    /// Represents an action that delegates its execution to another action.
    /// This provides a mechanism to wrap or modify the behavior of another action.
    /// </summary>
    /// <remarks>
    /// The <see cref="ActionDelegate"/> method forms the core execution logic for actions
    /// derived from <see cref="DelegateAction"/>. These derived actions typically manipulate,
    /// delay, or combine the execution of the underlying action set on the <see cref="Action"/>
    /// property.
    /// </remarks>
    /// <param name="delta">
    /// The elapsed time in seconds since the last frame. This value is used to drive time-dependent
    /// behavior.
    /// </param>
    /// <returns>
    /// Returns <c>true</c> if the action, including any delegated action, has completed execution;
    /// otherwise, <c>false</c>.
    /// </returns>
    protected override bool ActionDelegate( float delta )
    {
        if ( Time < Duration )
        {
            Time += delta;

            if ( Time < Duration )
            {
                return false;
            }

            delta = Time - Duration;
        }

        return ( Action == null ) || Action.Act( delta );
    }

    /// <summary>
    /// Forces the action to complete immediately by setting the current elapsed time
    /// to the total duration. Any remaining time-dependent behavior is skipped, and
    /// the action is effectively marked as finished upon the next execution cycle.
    /// </summary>
    /// <remarks>
    /// This method is typically used to abruptly terminate an ongoing delay or to
    /// synchronize the completion state of the action with other logic in a scene. After
    /// calling this method, the <see cref="DelayAction"/> will complete on its next update.
    /// </remarks>
    public void Finish()
    {
        Time = Duration;
    }

    /// <summary>
    /// Resets the state of the associated action, allowing it to be restarted from its
    /// initial configuration. This method is typically called when reusing an instance of
    /// an action or preparing it for repeated execution.
    /// </summary>
    /// <remarks>
    /// When <see cref="Restart"/> is invoked, all internal state related to the progress
    /// or timing of the action is cleared. For a <see cref="DelayAction"/>, this includes
    /// resetting the elapsed time to zero.
    /// </remarks>
    public override void Restart()
    {
        base.Restart();

        Time = 0;
    }
}

// ============================================================================
// ============================================================================

