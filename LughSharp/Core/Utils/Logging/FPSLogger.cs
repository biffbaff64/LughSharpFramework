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

using JetBrains.Annotations;

namespace LughSharp.Core.Utils.Logging;

/// <summary>
/// A simple helper class to log the frames per seconds achieved. Just invoke the
/// Log() method in your rendering method. The output will be logged once per second.
/// </summary>
[PublicAPI]
public class FPSLogger
{
    private readonly int  _bound;
    private          long _lastLogTime;
    private          int  _fps;

    // ========================================================================

    /// <summary>
    /// Constructs a new FPSLogger instance.
    /// </summary>
    /// <param name="bound"></param>
    public FPSLogger( int bound = int.MaxValue )
    {
        _bound       = bound;
        _lastLogTime = TimeHelpers.NanoTime();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="frames"></param>
    public void Update( int frames )
    {
        _fps = frames;
    }

    /// <summary>
    /// Logs the current frames per second to the console.
    /// </summary>
    public void LogFPS()
    {
        long     currentTime = TimeHelpers.NanoTime();
        TimeSpan elapsedTime = TimeSpan.FromTicks( currentTime - _lastLogTime );

        // Log FPS if at least one second has passed
        if ( elapsedTime.TotalSeconds >= 1 )
        {
            if ( _fps <= _bound )
            {
                Logger.Debug( $"FPS: {_fps}" );
            }

            _lastLogTime = currentTime;
        }
    }
}