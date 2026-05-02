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

using LughSharp.Source.Maths;

namespace LughSharp.Source.Graphics.G2D;

/// <summary>
/// Defines possible playback modes for an <see cref="Animation{T}"/>.
/// </summary>
[PublicAPI]
public enum AnimationMode
{
    /// <summary>
    /// Runs through the animation from frame 0 to the last frame, then stops.
    /// </summary>
    Normal,

    /// <summary>
    /// Runs through the animation from the last frame to frame 0, then stops.
    /// </summary>
    Reversed,

    /// <summary>
    /// As <see cref="Normal"/> but looped.
    /// </summary>
    Loop,

    /// <summary>
    /// As <see cref="Reversed"/> but looped.
    /// </summary>
    LoopReversed,

    /// <summary>
    /// Looped animation sequence that runs from frame 0 to the last frame, and then
    /// back down the frames to frame 0 again.
    /// </summary>
    LoopPingpong,

    /// <summary>
    /// Looped animation sequence that displays random frames from the animation#
    /// sequence.
    /// </summary>
    LoopRandom
}

// ============================================================================
// ============================================================================

[PublicAPI]
public class Animation< T >
{
    /// <summary>
    /// The animation play mode.
    /// </summary>
    public AnimationMode PlayMode { get; set; } = AnimationMode.Normal;

    /// <summary>
    /// The duration of a frame in seconds.
    /// </summary>
    public float FrameDuration
    {
        get;
        set
        {
            field             = value;
            AnimationDuration = KeyFrames.Length * value;
        }
    }

    /// <summary>
    /// the duration of the entire animation, (number of frames x frame duration),
    /// in seconds.
    /// </summary>
    public float AnimationDuration { get; private set; }

    /// <summary>
    /// The keyframes[] array where all the frames of the animation are stored.
    /// </summary>
    public T[] KeyFrames
    {
        get;
        set
        {
            field             = value;
            AnimationDuration = field.Length * FrameDuration;
        }
    }

    // ========================================================================

    private int   _lastFrameNumber;
    private float _lastStateTime;

    // ========================================================================

    /// <summary>
    /// Constructor, storing the frame duration and animation frames.
    /// </summary>
    /// <param name="frameDuration"> The desired time between frames in seconds. </param>
    /// <param name="keyFrames"> A List holding the animation frames. </param>
    public Animation( float frameDuration, List< T > keyFrames )
    {
        FrameDuration = frameDuration;
        KeyFrames     = keyFrames.ToArray();
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
    public Animation( float frameDuration, List< T > keyFrames, AnimationMode playMode )
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
        FrameDuration = frameDuration;
        KeyFrames     = keyFrames;
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
        AnimationMode oldPlayMode = PlayMode;

        if ( looping && ( PlayMode is AnimationMode.Normal or AnimationMode.Reversed ) )
        {
            PlayMode = PlayMode == AnimationMode.Normal
                ? AnimationMode.Loop
                : AnimationMode.LoopReversed;
        }
        else if ( !looping && !( PlayMode is AnimationMode.Normal or AnimationMode.Reversed ) )
        {
            PlayMode = PlayMode == AnimationMode.LoopReversed
                ? AnimationMode.Reversed
                : AnimationMode.Loop;
        }

        T frame = GetKeyFrame( stateTime );
        PlayMode = oldPlayMode;

        return frame;
    }

    /// <summary>
    /// Returns a frame based on the state time.
    /// This is the amount of seconds an object has spent in the state this
    /// Animation instance represents, e.g. running, jumping and so on using
    /// the mode specified by <see cref="PlayMode"/> property.
    /// </summary>
    /// <param name="stateTime">the time spent in the state represented by this animation.</param>
    /// <returns> the frame of animation for the given state time.</returns>
    public T GetKeyFrame( float stateTime )
    {
        int frameNumber = GetKeyFrameIndex( stateTime );

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

        var frameNumber = ( int )( stateTime / FrameDuration );

        switch ( PlayMode )
        {
            case AnimationMode.Normal:
            {
                frameNumber = Math.Min( KeyFrames.Length - 1, frameNumber );

                break;
            }

            case AnimationMode.Loop:
            {
                frameNumber %= KeyFrames.Length;

                break;
            }

            case AnimationMode.LoopPingpong:
            {
                frameNumber %= ( KeyFrames.Length * 2 ) - 2;

                if ( frameNumber >= KeyFrames.Length )
                {
                    frameNumber = KeyFrames.Length - 2 - ( frameNumber - KeyFrames.Length );
                }

                break;
            }

            case AnimationMode.LoopRandom:
            {
                var lastFrameNumber = ( int )( _lastStateTime / FrameDuration );

                frameNumber = lastFrameNumber != frameNumber
                    ? MathUtils.Random( KeyFrames.Length - 1 )
                    : _lastFrameNumber;

                break;
            }

            case AnimationMode.Reversed:
            {
                frameNumber = Math.Max( KeyFrames.Length - frameNumber - 1, 0 );

                break;
            }

            case AnimationMode.LoopReversed:
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
    /// Whether the animation would be finished if played without looping (PlayMode#NORMAL),
    /// given the state time.
    /// </summary>
    /// <param name="stateTime"> </param>
    /// <returns> whether the animation is finished.</returns>
    public bool IsAnimationFinished( float stateTime )
    {
        var frameNumber = ( int )( stateTime / FrameDuration );

        return ( KeyFrames.Length - 1 ) < frameNumber;
    }
}

// ============================================================================
// ============================================================================