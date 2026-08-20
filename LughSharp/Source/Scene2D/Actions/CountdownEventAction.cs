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
/// An EventAction that is complete once it receives X number of events.
/// </summary>
[PublicAPI]
public class CountdownEventAction< T >( T eventClass, int count )
    : EventAction< T >( eventClass )
    where T : Event
{
    private int _current;

    /// <summary>
    /// Handles the delegated event and evaluates whether the conditions for completion
    /// are met. Increments the internal event count upon handling the provided event.
    /// </summary>
    /// <param name="e">The event instance to handle.</param>
    /// <returns>
    /// A bool value indicating whether the required count of events has been reached.
    /// Returns <c>true</c> if the number of handled events is equal to or greater than
    /// the specified count; otherwise, <c>false</c>.
    /// </returns>
    public override bool HandleDelegate( Event e )
    {
        _current++;

        return _current >= count;
    }
}

// ============================================================================
// ============================================================================
