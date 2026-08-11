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
/// Multiplies the delta of an action.
/// </summary>
[PublicAPI]
public class TimeScaleAction : DelegateAction
{
    /// <summary>
    /// Gets or sets the scaling factor used to modify the time delta for an action.
    /// </summary>
    /// <remarks>
    /// The <c>Scale</c> property determines how the time progression of an action is adjusted.
    /// A value greater than 1 accelerates the action, while a value between 0 and 1 slows it down.
    /// </remarks>
    public float Scale { get; set; }
    
    // ========================================================================

    /// <summary>
    /// Executes a delegated action with a modified time scale.
    /// </summary>
    /// <param name="delta">
    /// The time delta used to determine the progression of the action, scaled by the
    /// current value of <c>Scale</c>.
    /// </param>
    /// <returns>
    /// Returns <c>true</c> if the action is complete, or <c>false</c> if it is still running.
    /// </returns>
    protected override bool ActionDelegate( float delta )
    {
        return ( Action == null ) || Action.Act( delta * Scale );
    }
}

// ============================================================================
// ============================================================================

