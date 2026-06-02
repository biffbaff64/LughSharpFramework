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

[PublicAPI]
public abstract class DelegateAction : SceneAction
{
    public SceneAction? Action { get; set; }

    protected abstract bool Delegate( float delta );

    // ========================================================================
    
    /// <summary>
    /// The actor this action is attached to, or null if it is not attached.
    /// </summary>
    public override Actor? Actor
    {
        get => base.Actor;
        set
        {
            if ( Action != null )
            {
                Action.Actor = value;
            }

            base.Actor = value;
        }
    }

    /// <summary>
    /// The actor this action targets, or null if a target has not been set.
    /// </summary>
    public override Actor? Target
    {
        get => base.Target;
        set
        {
            if ( Action != null )
            {
                Action.Target = value;
            }

            base.Target = value;
        }
    }

    /// <summary>
    /// Updates the action based on time.
    /// Typically this is called each frame by <see cref="Actor"/>.
    /// </summary>
    /// <param name="delta">Time in seconds since the last frame.</param>
    /// <returns>
    /// true if the action is done. This method may continue to be called after
    /// the action is done.
    /// </returns>
    public override bool Act( float delta )
    {
        IScenePool? pool = Pool;

        // Ensure this action can't be returned to the pool inside the delegate action.
        Pool = null;

        try
        {
            return Delegate( delta );
        }
        finally
        {
            Pool = pool;
        }
    }

    /// <summary>
    /// Sets the state of the action so it can be run again.
    /// Default implementation does nothing.
    /// </summary>
    public override void Restart()
    {
        Action?.Restart();
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
        Action = null;
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        return base.ToString() + ( Action == null ? string.Empty : $"({Action})" );
    }
}

// ============================================================================
// ============================================================================

