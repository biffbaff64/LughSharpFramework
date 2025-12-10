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

namespace LughSharp.Core.Graphics.G2D;

[PublicAPI]
public class Animation< T >
{
    /// <summary>
    /// Defines possible playback modes for an <see cref="Animation{T}"/>.
    /// </summary>
    public enum AnimMode
    {
        Normal,
        Reversed,
        Loop,
        LoopReversed,
        LoopPingpong,
        LoopRandom,
    }

    // ========================================================================

    private float _animationDuration;
    private float _frameDuration;
    private int   _lastFrameNumber;
    private float _lastStateTime;

    // ========================================================================

    /// <summary>
    /// Constructor, storing the frame duration and key frames.
    /// </summary>
    /// <param name="frameDuration">the time between frames in seconds.</param>
    /// <param name="keyFrames">
    /// The objects representing the frames.
    /// If this Array is type-aware, <see cref="KeyFrames"/> can return the
    /// correct type of array. Otherwise, it returns an object[].
    /// </param>
    public Animation( float frameDuration, List< T > keyFrames )
    {
        _frameDuration = frameDuration;

        KeyFrames = keyFrames.ToArray();
    }

    /// <summary>
    /// Constructor, storing the frame duration and key frames.
    /// </summary>
    /// <param name="frameDuration"> the time between frames in seconds.</param>
    /// <param name="keyFrames">
    /// The objects representing the frames. If this Array is type-aware,
    /// <see cref="KeyFrames"/> can return the correct type of array.
    /// Otherwise, it returns an object[].
    /// </param>
    /// <param name="playMode"> The required animation playback mode. </param>
    public Animation( float frameDuration, List< T > keyFrames, AnimMode playMode )
        : this( frameDuration, keyFrames )
    {
        PlayMode = playMode;
    }

    /// <summary>
    /// Constructor, storing the frame duration and key frames.
    /// </summary>
    /// <param name="frameDuration"> the time between frames in seconds.</param>
    /// <param name="keyFrames"> the objects representing the frames.</param>
    public Animation( float frameDuration, T[] keyFrames )
    {
        _frameDuration = frameDuration;
        KeyFrames      = keyFrames;
    }

    /// <summary>
    /// The animation play mode.
    /// </summary>
    public AnimMode PlayMode { get; set; } = AnimMode.Normal;

    /// <summary>
    /// The keyframes[] array where all the frames of the
    /// animation are stored.
    /// </summary>
    public T[] KeyFrames
    {
        get => field;
        set
        {
            field              = value;
            _animationDuration = field.Length * _frameDuration;
        }
    }

    /// <summary>
    /// Returns a frame based on the so called state time. This is the amount of
    /// seconds an object has spent in the state this Animation instance represents,
    /// e.g. running, jumping and so on. The mode specifies whether the animation is
    /// looping or not.
    /// </summary>
    /// <param name="stateTime">the time spent in the state represented by this animation.</param>
    /// <param name="looping"> whether the animation is looping or not.</param>
    /// <returns> the frame of animation for the given state time.</returns>
    public T GetKeyFrame( float stateTime, bool looping )
    {
        // we set the play mode by overriding the previous mode
        // based on looping parameter value
        var oldPlayMode = PlayMode;

        if ( looping
          && ( ( PlayMode == AnimMode.Normal )
            || ( PlayMode == AnimMode.Reversed ) ) )
        {
            PlayMode = PlayMode == AnimMode.Normal ? AnimMode.Loop : AnimMode.LoopReversed;
        }
        else if ( !looping
               && !( ( PlayMode == AnimMode.Normal )
                  || ( PlayMode == AnimMode.Reversed ) ) )
        {
            PlayMode = PlayMode == AnimMode.LoopReversed
                ? AnimMode.Reversed
                : AnimMode.Loop;
        }

        var frame = GetKeyFrame( stateTime );
        PlayMode = oldPlayMode;

        return frame;
    }

    /// <summary>
    /// Returns a frame based on the so called state time. This is the amount
    /// of seconds an object has spent in the state this Animation instance
    /// represents, e.g. running, jumping and so on using the mode specified by
    /// <see cref="PlayMode"/> property.
    /// </summary>
    /// <param name="stateTime"> </param>
    /// <returns> the frame of animation for the given state time.</returns>
    public T GetKeyFrame( float stateTime )
    {
        var frameNumber = GetKeyFrameIndex( stateTime );

        return KeyFrames[ frameNumber ];
    }

    /// <summary>
    /// Returns the current frame number.
    /// </summary>
    public int GetKeyFrameIndex( float stateTime )
    {
        if ( KeyFrames.Length == 1 )
        {
            return 0;
        }

        var frameNumber = ( int )( stateTime / _frameDuration );

        switch ( PlayMode )
        {
            case AnimMode.Normal:
            {
                frameNumber = Math.Min( KeyFrames.Length - 1, frameNumber );

                break;
            }

            case AnimMode.Loop:
            {
                frameNumber %= KeyFrames.Length;

                break;
            }

            case AnimMode.LoopPingpong:
            {
                frameNumber %= ( KeyFrames.Length * 2 ) - 2;

                if ( frameNumber >= KeyFrames.Length )
                {
                    frameNumber = KeyFrames.Length - 2 - ( frameNumber - KeyFrames.Length );
                }

                break;
            }

            case AnimMode.LoopRandom:
            {
                var lastFrameNumber = ( int )( _lastStateTime / _frameDuration );

                frameNumber = lastFrameNumber != frameNumber
                    ? MathUtils.Random( KeyFrames.Length - 1 )
                    : _lastFrameNumber;

                break;
            }

            case AnimMode.Reversed:
            {
                frameNumber = Math.Max( KeyFrames.Length - frameNumber - 1, 0 );

                break;
            }

            case AnimMode.LoopReversed:
            {
                frameNumber %= KeyFrames.Length;
                frameNumber =  KeyFrames.Length - frameNumber - 1;

                break;
            }
        }

        _lastFrameNumber = frameNumber;
        _lastStateTime   = stateTime;

        return frameNumber;
    }

    /// <summary>
    /// Whether the animation would be finished if played without looping
    /// (PlayMode#NORMAL), given the state time.
    /// </summary>
    /// <param name="stateTime"> </param>
    /// <returns> whether the animation is finished.</returns>
    public bool IsAnimationFinished( float stateTime )
    {
        var frameNumber = ( int )( stateTime / _frameDuration );

        return ( KeyFrames.Length - 1 ) < frameNumber;
    }

    /// <summary>
    /// Sets duration a frame will be displayed.
    /// </summary>
    /// <param name="frameDuration">The animation frame duration in seconds</param>
    public void SetFrameDuration( float frameDuration )
    {
        _frameDuration     = frameDuration;
        _animationDuration = KeyFrames.Length * frameDuration;
    }

    /// <summary>
    /// the duration of a frame in seconds.
    /// </summary>
    public float FrameDuration()
    {
        return _frameDuration;
    }

    /// <summary>
    /// the duration of the entire animation, (number of frames x frame duration),
    /// in seconds.
    /// </summary>
    public float GetAnimationDuration()
    {
        return _animationDuration;
    }
}