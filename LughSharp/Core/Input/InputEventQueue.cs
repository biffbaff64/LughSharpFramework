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

using LughSharp.Core.Maths;
using LughSharp.Core.Utils.Logging;

namespace LughSharp.Core.Input;

/// <summary>
/// Queues events that are later passed to an <see cref="IInputProcessor"/>.
/// </summary>
[PublicAPI]
public class InputEventQueue
{
    public long CurrentEventTime { get; set; }

    // ========================================================================

    private const int Skip          = -1;
    private const int KeyDown       = 0;
    private const int KeyUp         = 1;
    private const int KeyTyped      = 2;
    private const int TouchDown     = 3;
    private const int TouchUp       = 4;
    private const int TouchDragged  = 5;
    private const int MouseMoved    = 6;
    private const int MouseScrolled = 7;

    // ========================================================================

    private readonly List< int > _processingQueue = new();
    private readonly List< int > _queue           = new();

    // ========================================================================

    /// <summary>
    /// Processes and drains the events in the queue using the specified input processor.
    /// </summary>
    /// <param name="processor">
    /// The input processor to handle the events. If null, the queue will be cleared without processing.
    /// </param>
    /// <exception cref="SystemException"> Thrown if an unknown event type is encountered. </exception>
    public void ProcessInputEvents( IInputProcessor? processor )
    {
        int[] processingArray;

        lock ( this )
        {
            if ( processor == null )
            {
                _queue.Clear();

                return;
            }

            _processingQueue.AddRange( _queue );
            _queue.Clear();

            processingArray = _processingQueue.ToArray();
        }

        var index = 0;
        int count = processingArray.Length;

        while ( index < count )
        {
            int eventType = processingArray[ index++ ];
            CurrentEventTime = ( ( long )processingArray[ index++ ] << 32 )
                             | ( processingArray[ index++ ] & 0xFFFFFFFFL );

            switch ( eventType )
            {
                case Skip:
                    Logger.Debug( "Skip" );
                    index += processingArray[ index ];

                    break;

                case KeyDown:
                    Logger.Debug( "KeyDown" );
                    processor.OnKeyDown( processingArray[ index++ ] );

                    break;

                case KeyUp:
                    Logger.Debug( "KeyUp" );
                    processor.OnKeyUp( processingArray[ index++ ] );

                    break;

                case KeyTyped:
                    Logger.Debug( "KeyTyped" );
                    processor.OnKeyTyped( ( char )processingArray[ index++ ] );

                    break;

                case TouchDown:
                    Logger.Debug( "TouchDown" );
                    processor.OnTouchDown( processingArray[ index++ ],
                                           processingArray[ index++ ],
                                           processingArray[ index++ ],
                                           processingArray[ index++ ] );

                    break;

                case TouchUp:
                    Logger.Debug( "TouchUp" );
                    processor.OnTouchUp( processingArray[ index++ ],
                                         processingArray[ index++ ],
                                         processingArray[ index++ ],
                                         processingArray[ index++ ] );

                    break;

                case TouchDragged:
                    Logger.Debug( "TouchDragged" );
                    processor.OnTouchDragged( processingArray[ index++ ],
                                              processingArray[ index++ ],
                                              processingArray[ index++ ] );

                    break;

                case MouseMoved:
                    Logger.Debug( "MouseMoved" );
                    processor.OnMouseMoved( processingArray[ index++ ], processingArray[ index++ ] );

                    break;

                case MouseScrolled:
                    Logger.Debug( "MouseScrolled" );
                    processor.OnScrolled( NumberUtils.IntBitsToFloat( processingArray[ index++ ] ),
                                          NumberUtils.IntBitsToFloat( processingArray[ index++ ] ) );

                    break;

                default:
                    throw new SystemException( $"Unknown event type: {eventType}" );
            }
        }

        _processingQueue.Clear();
    }

    /// <summary>
    /// Finds the next index of the specified event type starting from the given index.
    /// </summary>
    /// <param name="nextType"> The event type to search for. </param>
    /// <param name="i"> The index to start searching from. </param>
    /// <returns>
    /// The index of the next occurrence of the specified event type, or -1 if not found.
    /// </returns>
    /// <exception cref="SystemException"> Thrown if an unknown event type is encountered. </exception>
    private int NextIndex( int nextType, int i )
    {
        lock ( this )
        {
            int[] q = _queue.ToArray();

            for ( int n = _queue.Count; i < n; )
            {
                int type = q[ i ];

                if ( type == nextType )
                {
                    return i;
                }

                i += 3;

                switch ( type )
                {
                    case Skip:
                    {
                        i += q[ i ];

                        break;
                    }

                    case KeyDown:
                    case KeyUp:
                    case KeyTyped:
                    {
                        i++;

                        break;
                    }

                    case TouchDown:
                    case TouchUp:
                    {
                        i += 4;

                        break;
                    }

                    case TouchDragged:
                    {
                        i += 3;

                        break;
                    }

                    case MouseMoved:
                    case MouseScrolled:
                    {
                        i += 2;

                        break;
                    }

                    default:
                    {
                        throw new SystemException();
                    }
                }
            }
        }

        return -1;
    }

    /// <summary>
    /// Adds the specified time to the queue.
    /// </summary>
    /// <param name="time">The time to add to the queue.</param>
    private void QueueTime( long time )
    {
        _queue.Add( ( int )( time >> 32 ) );
        _queue.Add( ( int )time );
    }

    /// <summary>
    /// Queues a key down event with the specified keycode and time.
    /// </summary>
    /// <param name="keycode"> The keycode of the key down event. </param>
    /// <param name="time"> The time the event occurred. </param>
    /// <returns> Always returns false. </returns>
    public bool OnKeyDown( int keycode, long time )
    {
        lock ( this )
        {
            _queue.Add( KeyDown );
            QueueTime( time );
            _queue.Add( keycode );
        }

        return false;
    }

    /// <summary>
    /// Queues a key up event with the specified keycode and time.
    /// </summary>
    /// <param name="keycode"> The keycode of the key up event. </param>
    /// <param name="time"> The time the event occurred. </param>
    /// <returns> Always returns false. </returns>
    public bool OnKeyUp( int keycode, long time )
    {
        lock ( this )
        {
            _queue.Add( KeyUp );
            QueueTime( time );
            _queue.Add( keycode );
        }

        return false;
    }

    /// <summary>
    /// Queues a key typed event with the specified character and time.
    /// </summary>
    /// <param name="character"> The character of the key typed event. </param>
    /// <param name="time"> The time the event occurred. </param>
    /// <returns> Always returns false. </returns>
    public bool OnKeyTyped( char character, long time )
    {
        lock ( this )
        {
            _queue.Add( KeyTyped );
            QueueTime( time );
            _queue.Add( character );
        }

        return false;
    }

    /// <summary>
    /// Queues a touch down event with the specified parameters.
    /// </summary>
    /// <param name="screenX"> The x-coordinate of the touch. </param>
    /// <param name="screenY"> The y-coordinate of the touch. </param>
    /// <param name="pointer"> The pointer index. </param>
    /// <param name="button"> The button pressed. </param>
    /// <param name="time"> The time the event occurred. </param>
    /// <returns> Always returns false. </returns>
    public bool OnTouchDown( int screenX, int screenY, int pointer, int button, long time )
    {
        lock ( this )
        {
            _queue.Add( TouchDown );
            QueueTime( time );
            _queue.Add( screenX );
            _queue.Add( screenY );
            _queue.Add( pointer );
            _queue.Add( button );
        }

        return false;
    }

    /// <summary>
    /// Queues a touch up event with the specified parameters.
    /// </summary>
    /// <param name="screenX"> The x-coordinate of the touch. </param>
    /// <param name="screenY"> The y-coordinate of the touch. </param>
    /// <param name="pointer"> The pointer index. </param>
    /// <param name="button"> The button released. </param>
    /// <param name="time"> The time the event occurred. </param>
    /// <returns> Always returns false. </returns>
    public bool OnTouchUp( int screenX, int screenY, int pointer, int button, long time )
    {
        lock ( this )
        {
            _queue.Add( TouchUp );

            QueueTime( time );

            _queue.Add( screenX );
            _queue.Add( screenY );
            _queue.Add( pointer );
            _queue.Add( button );
        }

        return false;
    }

    /// <summary>
    /// Queues a touch dragged event with the specified parameters, skipping any previously
    /// queued touch dragged events for the same pointer.
    /// </summary>
    /// <param name="screenX"> The x-coordinate of the drag. </param>
    /// <param name="screenY"> The y-coordinate of the drag. </param>
    /// <param name="pointer"> The pointer index. </param>
    /// <param name="time"> The time the event occurred. </param>
    /// <returns> Always returns false. </returns>
    public bool OnTouchDragged( int screenX, int screenY, int pointer, long time )
    {
        lock ( this )
        {
            // Skip any queued touch dragged events for the same pointer.
            for ( int i = NextIndex( TouchDragged, 0 ); i >= 0; i = NextIndex( TouchDragged, i + 6 ) )
            {
                if ( _queue[ i + 5 ] == pointer )
                {
                    _queue[ i ]     = Skip;
                    _queue[ i + 3 ] = 3;
                }
            }

            _queue.Add( TouchDragged );

            QueueTime( time );

            _queue.Add( screenX );
            _queue.Add( screenY );
            _queue.Add( pointer );
        }

        return false;
    }

    /// <summary>
    /// Queues a mouse moved event with the specified parameters, skipping any previously
    /// queued mouse moved events.
    /// </summary>
    /// <param name="screenX"> The x-coordinate of the mouse. </param>
    /// <param name="screenY"> The y-coordinate of the mouse. </param>
    /// <param name="time"> The time the event occurred. </param>
    /// <returns> Always returns false. </returns>
    public bool OnMouseMoved( int screenX, int screenY, long time )
    {
        lock ( this )
        {
            // Skip any queued mouse moved events.
            for ( int i = NextIndex( MouseMoved, 0 ); i >= 0; i = NextIndex( MouseMoved, i + 5 ) )
            {
                _queue[ i ]     = Skip;
                _queue[ i + 3 ] = 2;
            }

            _queue.Add( MouseMoved );

            QueueTime( time );

            _queue.Add( screenX );
            _queue.Add( screenY );
        }

        return false;
    }

    /// <summary>
    /// Queues a mouse scrolled event with the specified parameters.
    /// </summary>
    /// <param name="amountX">The horizontal scroll amount.</param>
    /// <param name="amountY">The vertical scroll amount.</param>
    /// <param name="time">The time the event occurred.</param>
    /// <returns>Always returns false.</returns>
    public bool OnScrolled( float amountX, float amountY, long time )
    {
        lock ( this )
        {
            _queue.Add( MouseScrolled );

            QueueTime( time );

            _queue.Add( NumberUtils.FloatToIntBits( amountX ) );
            _queue.Add( NumberUtils.FloatToIntBits( amountY ) );
        }

        return false;
    }
}

// ============================================================================
// ============================================================================