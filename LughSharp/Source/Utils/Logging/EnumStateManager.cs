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
/// Manages states that are driven by simple Enum values, as defined in <see cref="StateID"/>.
/// </summary>
[PublicAPI]
public class EnumStateManager
{
    // Property to read the current state publicly
    public StateID CurrentState => _currentState;

    // Optional: Event triggered whenever the state changes
    public event Action<StateID, StateID>? OnStateChanged;

    // ========================================================================
    
    // Backing field to hold the current state
    private StateID _currentState;

    // ========================================================================
    
    /// <summary>
    /// Initializes the StateManager with a default starting state.
    /// </summary>
    public EnumStateManager(StateID initialState = StateID.StateSetup)
    {
        _currentState = initialState;
    }

    /// <summary>
    /// Sets a new state.
    /// </summary>
    public void SetState(StateID newState)
    {
        if (_currentState == newState) return;

        StateID previousState = _currentState;
        _currentState = newState;

        // Trigger event if anyone is listening
        OnStateChanged?.Invoke(previousState, _currentState);
    }

    /// <summary>
    /// Compares a supplied state against the current state.
    /// Returns 0 if equal, 1 if supplied is greater, -1 if supplied is less.
    /// </summary>
    public int CompareToCurrent(StateID suppliedState)
    {
        // Enums implement IComparable natively
        return suppliedState.CompareTo(_currentState);
    }

    /// <summary>
    /// Helper method to quickly check if a supplied state is greater than the current state.
    /// </summary>
    public bool IsSuppliedStateGreater(StateID suppliedState)
    {
        return (int)suppliedState > (int)_currentState;
    }

    /// <summary>
    /// Helper method to quickly check if a supplied state is less than the current state.
    /// </summary>
    public bool IsSuppliedStateLess(StateID suppliedState)
    {
        return (int)suppliedState < (int)_currentState;
    }

    /// <summary>
    /// Helper method to quickly check if a supplied state is equal to the current state.
    /// </summary>
    public bool IsSuppliedStateEqual(StateID suppliedState)
    {
        return _currentState == suppliedState;
    }
}

// ============================================================================
// ============================================================================
