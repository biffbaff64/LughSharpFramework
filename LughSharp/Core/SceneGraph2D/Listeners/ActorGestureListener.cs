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

using LughSharp.Core.Input;
using LughSharp.Core.Maths;

namespace LughSharp.Core.SceneGraph2D.Listeners;

/// <summary>
/// Detects tap, long press, fling, pan, zoom, and pinch gestures on an actor.
/// If there is only a need to detect tap, use <see cref="ClickListener"/>.
/// </summary>
[PublicAPI]
public class ActorGestureListener : IEventListener
{
    public ActorGestureDetector Detector        { get; set; }
    public Actor?               TouchDownTarget { get; set; }

    // ========================================================================
    
    private const float DefaultHalfTapSquareSize = 20;
    private const float DefaultTapCountInterval   = 0.4f;
    private const float DefaultLongPressDuration  = 1.1f;
    private const float DefaultMaxFlingDelay      = int.MaxValue;

    private static readonly Vector2 _tmpCoords  = new();
    private static readonly Vector2 _tmpCoords2 = new();

    private Actor?      _actor;
    private InputEvent? _inputEvent;

    // ========================================================================

    /// <summary>
    /// Constructs a new GestureListener for Actors.
    /// </summary>
    /// <param name="halfTapSquareSize">
    /// Half width in pixels of the square around an initial touch event, see
    /// <see cref="ActorGestureDetector.OnTap(float, float, int, int)"/>.
    /// </param>
    /// <param name="tapCountInterval">
    /// Time in seconds that must pass for two touch down/up sequences to be detected
    /// as consecutive taps.
    /// </param>
    /// <param name="longPressDuration">
    /// Time in seconds that must pass for the detector to fire a
    /// <see cref="ActorGestureDetector.OnLongPress(float, float)"/> event.
    /// </param>
    /// <param name="maxFlingDelay">
    /// No fling event is fired when the time in seconds the finger was dragged is larger
    /// than this, see <see cref="ActorGestureDetector.OnFling(float, float, int)"/>.
    /// </param>
    public ActorGestureListener( float halfTapSquareSize = DefaultHalfTapSquareSize,
                                 float tapCountInterval = DefaultTapCountInterval,
                                 float longPressDuration = DefaultLongPressDuration,
                                 float maxFlingDelay = DefaultMaxFlingDelay )
    {
        Detector = new ActorGestureDetector( halfTapSquareSize,
                                             tapCountInterval,
                                             longPressDuration,
                                             maxFlingDelay,
                                             this );
    }

    /// <summary>
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public virtual bool Handle( Event e )
    {
        if ( e is not InputEvent ev )
        {
            return false;
        }

        switch ( ev.Type )
        {
            case InputEvent.EventType.TouchDown:
            {
                _actor          = ev.ListenerActor;
                TouchDownTarget = ev.TargetActor;

                Detector.OnTouchDown( ev.StageX, ev.StageY, ev.Pointer, ev.Button );
                _actor?.StageToLocalCoordinates( _tmpCoords.Set( ev.StageX, ev.StageY ) );

                OnTouchDown( ev, _tmpCoords.X, _tmpCoords.Y, ev.Pointer, ev.Button );

                if ( ev.TouchFocus )
                {
                    ev.Stage?.AddTouchFocus( this,
                                             ev.ListenerActor,
                                             ev.TargetActor,
                                             ev.Pointer,
                                             ev.Button );
                }

                return true;
            }

            case InputEvent.EventType.TouchUp:
            {
                if ( ev.TouchFocusCancel )
                {
                    Detector.Reset();

                    return false;
                }

                _inputEvent = ev;
                _actor      = ev.ListenerActor;

                Detector.OnTouchUp( ev.StageX, ev.StageY, ev.Pointer, ev.Button );
                _actor?.StageToLocalCoordinates( _tmpCoords.Set( ev.StageX, ev.StageY ) );
                OnTouchUp( ev, _tmpCoords.X, _tmpCoords.Y, ev.Pointer, ev.Button );

                return true;
            }

            case InputEvent.EventType.TouchDragged:
            {
                _inputEvent = ev;
                _actor      = ev.ListenerActor;
                Detector.OnTouchDragged( ev.StageX, ev.StageY, ev.Pointer );

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Called when a touch down event is detected.
    /// </summary>
    /// <param name="ev"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="pointer"></param>
    /// <param name="button"></param>
    public virtual void OnTouchDown( InputEvent ev, float x, float y, int pointer, int button )
    {
    }

    /// <summary>
    /// Called when a touch up event is detected.
    /// </summary>
    /// <param name="ev"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="pointer"></param>
    /// <param name="button"></param>
    public virtual void OnTouchUp( InputEvent ev, float x, float y, int pointer, int button )
    {
    }

    /// <summary>
    /// Called when a touch dragged event is detected.
    /// </summary>
    /// <param name="ev"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="count"></param>
    /// <param name="button"></param>
    public virtual void OnTap( InputEvent ev, float x, float y, int count, int button )
    {
    }

    /// <summary>
    /// If true is returned, additional gestures will not be triggered. No ev is
    /// provided because this ev is triggered by time passing, not by an InputEvent.
    /// </summary>
    public virtual bool OnLongPress( Actor actor, float x, float y )
    {
        return false;
    }

    /// <summary>
    /// </summary>
    /// <param name="ev"></param>
    /// <param name="velocityX"></param>
    /// <param name="velocityY"></param>
    /// <param name="button"></param>
    public virtual void OnFling( InputEvent ev, float velocityX, float velocityY, int button )
    {
    }

    /// <summary>
    /// The delta is the difference in stage coordinates since the last pan.
    /// </summary>
    public virtual void OnPan( InputEvent ev, float x, float y, float deltaX, float deltaY )
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ev"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="pointer"></param>
    /// <param name="button"></param>
    public virtual void OnPanStop( InputEvent ev, float x, float y, int pointer, int button )
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ev"></param>
    /// <param name="initialDistance"></param>
    /// <param name="distance"></param>
    public virtual void OnZoom( InputEvent ev, float initialDistance, float distance )
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ev"></param>
    /// <param name="initialPointer1"></param>
    /// <param name="initialPointer2"></param>
    /// <param name="pointer1"></param>
    /// <param name="pointer2"></param>
    public virtual void OnPinch( InputEvent ev,
                               Vector2 initialPointer1,
                               Vector2 initialPointer2,
                               Vector2 pointer1,
                               Vector2 pointer2 )
    {
    }

    // ========================================================================

    [PublicAPI]
    public class ActorGestureDetector : GestureDetector
    {
        private readonly Vector2              _initialPointer1 = new();
        private readonly Vector2              _initialPointer2 = new();
        private readonly ActorGestureListener _parent;
        private readonly Vector2              _pointer1 = new();
        private readonly Vector2              _pointer2 = new();

        public ActorGestureDetector( float halfTapSquareSize,
                                     float tapCountInterval,
                                     float longPressDuration,
                                     float maxFlingDelay,
                                     ActorGestureListener parent )
            : base( halfTapSquareSize,
                    tapCountInterval,
                    longPressDuration,
                    maxFlingDelay,
                    new GestureAdapter() )
        {
            _parent = parent;
        }

        public bool OnTap( float stageX, float stageY, int count, int button )
        {
            _parent._actor?.StageToLocalCoordinates( _tmpCoords.Set( stageX, stageY ) );

            _parent.OnTap( _parent._inputEvent!, _tmpCoords.X, _tmpCoords.Y, count, button );

            return true;
        }

        public bool OnLongPress( float stageX, float stageY )
        {
            _parent._actor?.StageToLocalCoordinates( _tmpCoords.Set( stageX, stageY ) );

            return _parent.OnLongPress( _parent._actor!, _tmpCoords.X, _tmpCoords.Y );
        }

        public bool OnFling( float velocityX, float velocityY, int button )
        {
            StageToLocalAmount( _tmpCoords.Set( velocityX, velocityY ) );
            _parent.OnFling( _parent._inputEvent!, _tmpCoords.X, _tmpCoords.Y, button );

            return true;
        }

        public bool OnPan( float stageX, float stageY, float deltaX, float deltaY )
        {
            StageToLocalAmount( _tmpCoords.Set( deltaX, deltaY ) );

            deltaX = _tmpCoords.X;
            deltaY = _tmpCoords.Y;

            _parent._actor!.StageToLocalCoordinates( _tmpCoords.Set( stageX, stageY ) );
            _parent.OnPan( _parent._inputEvent!, _tmpCoords.X, _tmpCoords.Y, deltaX, deltaY );

            return true;
        }

        public bool PanStop( float stageX, float stageY, int pointer, int button )
        {
            _parent._actor!.StageToLocalCoordinates( _tmpCoords.Set( stageX, stageY ) );
            _parent.OnPanStop( _parent._inputEvent!, _tmpCoords.X, _tmpCoords.Y, pointer, button );

            return true;
        }

        public bool OnZoom( float initialDistance, float distance )
        {
            _parent.OnZoom( _parent._inputEvent!, initialDistance, distance );

            return true;
        }

        public bool OnPinch( Vector2 stageInitialPointer1,
                           Vector2 stageInitialPointer2,
                           Vector2 stagePointer1,
                           Vector2 stagePointer2 )
        {
            _parent._actor!.StageToLocalCoordinates( _initialPointer1.Set( stageInitialPointer1 ) );
            _parent._actor!.StageToLocalCoordinates( _initialPointer2.Set( stageInitialPointer2 ) );
            _parent._actor!.StageToLocalCoordinates( _pointer1.Set( stagePointer1 ) );
            _parent._actor!.StageToLocalCoordinates( _pointer2.Set( stagePointer2 ) );

            _parent.OnPinch( _parent._inputEvent!, _initialPointer1, _initialPointer2, _pointer1, _pointer2 );

            return true;
        }

        private void StageToLocalAmount( Vector2 amount )
        {
            _parent._actor!.StageToLocalCoordinates( amount );
            amount.Sub( _parent._actor.StageToLocalCoordinates( _tmpCoords2.Set( 0, 0 ) ) );
        }
    }
}

// ============================================================================
// ============================================================================

