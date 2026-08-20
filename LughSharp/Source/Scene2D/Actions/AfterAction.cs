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
/// The AfterAction class is used to ensure that a specific action executes
/// only after all previously running actions on the actor have completed.
/// <para>
/// This action is designed to work with actors and their associated actions in
/// a scene. It waits for existing actions on a specified target actor to either
/// complete or be removed before delegating execution to a specific inner action.
/// </para>
/// </summary>
[PublicAPI]
public class AfterAction : DelegateAction
{
    private readonly List< SceneAction > _waitForActions = new( 4 );
    
    // ========================================================================

    /// <summary>
    /// Sets the target actor for this action and ensures that
    /// all current actions of the target are queued to be waited on.
    /// </summary>
    /// <param name="target">
    /// The actor for which this action will execute. If the target is not null,
    /// its existing actions are added to the waiting list.
    /// </param>
    public void SetTarget( Actor? target )
    {
        if ( target != null )
        {
            _waitForActions.AddRange( target.Actions );
        }

        base.Target = target;
    }

    /// <summary>
    /// Sets the state of the action so it can be run again.
    /// Default implementation does nothing.
    /// </summary>
    public override void Restart()
    {
        base.Restart();

        _waitForActions.Clear();
    }

    /// <summary>
    /// Executes the delegate action and evaluates whether all waiting actions
    /// in the target actor have completed.
    /// </summary>
    /// <param name="delta">
    /// The time in seconds since the last frame. This is used to update the
    /// action's progress.
    /// </param>
    /// <returns>
    /// A bool indicating whether the delegate action has finished executing.
    /// Returns true if all waiting actions have been completed and the delegate
    /// action has successfully executed, otherwise returns false.
    /// </returns>
    protected override bool ActionDelegate( float delta )
    {
        List< SceneAction >? currentActions = Target?.Actions;

        if ( currentActions?.Count == 1 )
        {
            _waitForActions.Clear();
        }

        for ( int i = _waitForActions.Count - 1; i >= 0; i-- )
        {
            SceneAction sceneAction = _waitForActions[ i ];

            if ( currentActions?.IndexOf( sceneAction ) == -1 )
            {
                _waitForActions.RemoveAt( i );
            }
        }

        return ( _waitForActions.Count <= 0 ) && Action!.Act( delta );
    }
}

// ============================================================================
// ============================================================================

