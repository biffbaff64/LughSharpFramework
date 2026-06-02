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
/// Sets the alpha for an actor's color (or a specified color), from the current alpha
/// to the new alpha. Note this action transitions from the alpha at the time the action
/// starts to the specified alpha.
/// </summary>
[PublicAPI]
public class AlphaAction : TemporalAction
{
    public  Color? Color    { get; set; }
    public  float  EndAlpha { get; set; }

    // ========================================================================

    private float  _start;

    // ========================================================================

    /// <summary>
    /// Called the first time <see cref="TemporalAction.Act"/> is called. This is a good place
    /// to query the <see cref="Actor"/>'s starting state.
    /// </summary>
    protected override void Begin()
    {
        if ( Target == null )
        {
            Logger.Error( "Cannot begin with a null Target!" );
        }

        if ( Color == null )
        {
            Color = Target?.ActorColor;
        }

        _start = Color?.A ?? 0;
    }

    /// <summary>
    /// Called each frame.
    /// </summary>
    /// <param name="percent">
    /// The percentage of completion for this action, growing from 0 to 1 over the duration.
    /// If <see cref="TemporalAction.Reverse"/> is true, this will shrink from 1 to 0.
    /// </param>
    protected override void Update( float percent )
    {
        if ( percent == 0 )
        {
            Color?.A = _start;
        }
        else if ( percent is 1.0f )
        {
            Color?.A = EndAlpha;
        }
        else
        {
            Color?.A = _start + ( ( EndAlpha - _start ) * percent );
        }
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
        Color = null;
    }
}

// ============================================================================
// ============================================================================