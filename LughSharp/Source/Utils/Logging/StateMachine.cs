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

namespace LughSharp.Source.Utils.Logging;

/// <summary>
/// The manager acts as a central registry for <see cref="StateID"/> states. It
/// handles safe transitions, guards against illegal jumps, and drives the active
/// state's update loop.
/// </summary>
[PublicAPI]
public class StateMachine
{
    public IState?  CurrentState   => _currentState;
    public StateID? CurrentStateID => _currentState?.ID;

    /// <summary>
    /// Optional: Triggered when a transition completes successfully
    /// </summary>
    public event Action< StateID, StateID >? OnStateTransitioned;

    private Dictionary< StateID, IState > _states = new();
    private IState?                       _currentState;

    // ========================================================================

    /// <summary>
    /// Registers a state instance to the manager.
    /// </summary>
    public void RegisterState( IState? state )
    {
        if ( state == null ) return;

        _states[ state.ID ] = state;
    }

    /// <summary>
    /// Safely changes the active state, handling exit/enter cycles and passing payloads.
    /// </summary>
    public void ChangeState( StateID nextStateID, object? dataPayload = null )
    {
        if ( !_states.TryGetValue( nextStateID, out IState? nextState ) )
        {
            throw new ArgumentException( $"State {nextStateID} is not registered in this manager." );
        }

        if ( _currentState != null )
        {
            // Guard check: Is this transition allowed?
            if ( !ValidateTransition( _currentState.ID, nextStateID ) )
            {
                Logging.Logger.Error
                    (
                     $"[Warning] Illegal transition attempted from "
                   + $"{_currentState.ID} to {nextStateID}"
                    );

                return;
            }

            // Tear down the old state
            _currentState.OnExit();
        }

        StateID? previousID = _currentState?.ID;

        // Swap and Initialize the new state
        _currentState = nextState;
        _currentState.OnEnter( dataPayload );

        if ( previousID.HasValue )
        {
            OnStateTransitioned?.Invoke( previousID.Value, _currentState.ID );
        }
    }

    /// <summary>
    /// Must be called every frame inside your main system/game loop.
    /// </summary>
    public void Update()
    {
        _currentState?.OnUpdate();
    }

    /// <summary>
    /// Hard rules dictating which states can legally flow into others.
    /// </summary>
    private bool ValidateTransition( StateID current, StateID target )
    {
        return ( current, target ) switch
               {
                   // Cannot pause if the system is completely closed
                   (StateID.StateClosed, StateID.StatePaused) => false,

                   // Cannot open something that is already zooming out
                   (StateID.StateZoomOut, StateID.StateOpen) => false,

                   // Allow everything else by default
                   _ => true
               };
    }
}

// ============================================================================
// ============================================================================
