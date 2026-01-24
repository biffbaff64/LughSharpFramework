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

using JetBrains.Annotations;

namespace LughSharp.Core.Main;

[PublicAPI]
public class GameTime
{
    public TimeSpan TotalGameTime   { get; set; }
    public TimeSpan ElapsedGameTime { get; set; }
    public bool     IsRunningSlowly { get; set; }

    // ========================================================================

    /// <summary>
    /// Represents game timing information, including total elapsed game time,
    /// time elapsed during the current update, and whether the game is running slowly.
    /// </summary>
    public GameTime()
    {
        TotalGameTime   = TimeSpan.Zero;
        ElapsedGameTime = TimeSpan.Zero;
        IsRunningSlowly = false;
    }

    /// <summary>
    /// Represents game timing information, including elapsed game time, total game time,
    /// and the state indicating whether the game is running slower than expected.
    /// </summary>
    public GameTime( TimeSpan totalGameTime, TimeSpan elapsedGameTime )
    {
        TotalGameTime   = totalGameTime;
        ElapsedGameTime = elapsedGameTime;
        IsRunningSlowly = false;
    }

    /// <summary>
    /// Provides game timing information, including the total elapsed game time,
    /// time elapsed since the last update, and whether the game performance is slower than expected.
    /// </summary>
    public GameTime( TimeSpan totalRealTime, TimeSpan elapsedRealTime, bool isRunningSlowly )
    {
        TotalGameTime   = totalRealTime;
        ElapsedGameTime = elapsedRealTime;
        IsRunningSlowly = isRunningSlowly;
    }
}

// ============================================================================
// ============================================================================
