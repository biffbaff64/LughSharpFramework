// // /////////////////////////////////////////////////////////////////////////////
// //  MIT License
// //
// //  Copyright (c) 2024 Richard Ikin
// //
// //  Permission is hereby granted, free of charge, to any person obtaining a copy
// //  of this software and associated documentation files (the "Software"), to deal
// //  in the Software without restriction, including without limitation the rights
// //  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// //  copies of the Software, and to permit persons to whom the Software is
// //  furnished to do so, subject to the following conditions:
// //
// //  The above copyright notice and this permission notice shall be included in all
// //  copies or substantial portions of the Software.
// //
// //  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// //  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// //  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// //  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// //  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// //  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// //  SOFTWARE.
// // /////////////////////////////////////////////////////////////////////////////

using System.Diagnostics;

namespace LughSharp.Source.Utils;

[PublicAPI]
[UnstableApi( "GameTime class is not ready for use." )]
public class GameTime
{
    /// <summary>
    /// The total game time since the app first started.
    /// </summary>
    public TimeSpan TotalGameTime { get; set; }

    /// <summary>
    /// The total time elapsed since the last call to <see cref="UpdateGameTime"/>
    /// </summary>
    public TimeSpan ElapsedGameTime { get; set; }

    // ========================================================================

    private int  _fps;
    private long _frameId;
    private int  _frames;
    private long _frameCounterStart;
    private long _lastFrameTime = -1;

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    public GameTime()
    {
        TotalGameTime   = TimeSpan.Zero;
        ElapsedGameTime = TimeSpan.Zero;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="totalGameTime"></param>
    /// <param name="elapsedGameTime"></param>
    public GameTime( TimeSpan totalGameTime, TimeSpan elapsedGameTime )
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public float UpdateGameTime()
    {
        long time = Stopwatch.GetTimestamp() * 1_000_000_000 / Stopwatch.Frequency;

        if ( _lastFrameTime == -1 )
        {
            _lastFrameTime = time;
        }

        float deltaTime = ( time - _lastFrameTime ) / ( float )1_000_000_000;
        _lastFrameTime = time;

        if ( ( time - _frameCounterStart ) >= ( float )1_000_000_000 )
        {
            _fps               = _frames;
            _frames            = 0;
            _frameCounterStart = time;
        }

        _frames++;
        _frameId++;
        
        return deltaTime;
    }
}

// ============================================================================
// ============================================================================
