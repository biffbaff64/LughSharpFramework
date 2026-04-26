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

namespace LughSharp.Source.Utils;

/// <summary>
/// Class to keep track of a group of <see cref="PerformanceCounter" /> instances.
/// </summary>
[PublicAPI]
public class PerformanceCounters
{
    /// <summary>
    /// The list of <see cref="PerformanceCounter" />s to track.
    /// </summary>
    public List< PerformanceCounter > Counters { get; set; } = [ ];

    // ========================================================================

    private const float Nano2Seconds = 1f / 1000000000.0f;

    private long _lastTick;

    // ========================================================================
    
    /// <summary>
    /// Adds a new <see cref="PerformanceCounter" /> to the <see cref="Counters" /> list.
    /// </summary>
    /// <param name="name"> The identifying name. </param>
    /// <param name="windowSize"></param>
    /// <returns> The newly created PerformanceCounter. </returns>
    public PerformanceCounter Add( in string name, in int windowSize )
    {
        var result = new PerformanceCounter( name, windowSize );

        Counters.Add( result );

        return result;
    }

    /// <summary>
    /// Adds a new <see cref="PerformanceCounter" /> to the <see cref="Counters" /> list.
    /// </summary>
    /// <param name="name"> The identifying name. </param>
    /// <returns> The newly created PerformanceCounter. </returns>
    public PerformanceCounter Add( in string name )
    {
        var result = new PerformanceCounter( name );

        Counters.Add( result );

        return result;
    }

    /// <summary>
    /// Updates all <see cref="PerformanceCounter" /> instances by calculating the time
    /// elapsed since the last tick and invoking the corresponding Tick method with the
    /// calculated delta time.
    /// </summary>
    public void Tick()
    {
        long t = TimeUtils.NanoTime();

        if ( _lastTick > 0L )
        {
            Tick( ( t - _lastTick ) * Nano2Seconds );
        }

        _lastTick = t;
    }

    /// <summary>
    /// Updates all <see cref="PerformanceCounter" /> instances in the <see cref="Counters" />
    /// list with the time elapsed since the last tick.
    /// </summary>
    public void Tick( in float deltaTime )
    {
        foreach ( PerformanceCounter t in Counters )
        {
            t.Tick( deltaTime );
        }
    }
}

// ============================================================================
// ============================================================================