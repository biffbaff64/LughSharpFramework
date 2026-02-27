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

using System.Diagnostics;

using JetBrains.Annotations;

namespace DesktopGLBackend;

/// <summary>
/// Provides synchronization mechanisms to ensure accurate frame rate capping,
/// utilizing high-resolution timing techniques for precise frame scheduling.
/// </summary>
[PublicAPI]
public class Sync
{
    private const long NANOS_IN_SECOND = 1000L * 1000L * 1000L;

    private readonly Stopwatch _stopwatch = new();

    private bool _initialised;
    private long _nextFrame;

    // ========================================================================

    private void Initialise()
    {
        _initialised = true;
        _stopwatch.Start();
        _nextFrame = _stopwatch.ElapsedTicks * ( NANOS_IN_SECOND / Stopwatch.Frequency );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fps"></param>
    public void SyncFrameRate( int fps )
    {
        if ( fps <= 0 )
        {
            return;
        }

        if ( !_initialised )
        {
            Initialise();
        }

        long targetTime  = _nextFrame;
        long currentTime = _stopwatch.ElapsedTicks * ( NANOS_IN_SECOND / Stopwatch.Frequency );

        long sleepTime = targetTime - currentTime;

        switch ( sleepTime )
        {
            // Sleep for longer periods (1ms)
            case > 1000000:
                Thread.Sleep( ( int )( sleepTime / 1000000 ) );

                break;

            //Yield if sleep time is less than 1ms
            case > 0:
                Thread.Yield();

                break;
        }

        currentTime = _stopwatch.ElapsedTicks * ( NANOS_IN_SECOND / Stopwatch.Frequency );
        _nextFrame  = Math.Max( targetTime + ( NANOS_IN_SECOND / fps ), currentTime );
    }
}

// ============================================================================
// ============================================================================