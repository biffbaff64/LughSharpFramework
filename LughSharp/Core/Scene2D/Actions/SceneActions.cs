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

using LughSharp.Core.Scene2D.Listeners;
using LughSharp.Core.Utils.Pooling;

namespace LughSharp.Core.Scene2D.Actions;

[PublicAPI]
public class SceneActions
{
    /// <summary>
    /// Returns a new or pooled action of the specified type.
    /// </summary>
    public static SceneAction Action( Type a )
    {
        Pool< SceneAction > actionPool = new()
        {
            NewObjectFactory = ( ) => Activator.CreateInstance( a ) as SceneAction
                                   ?? throw new ArgumentException( $"Unable to create action type: {a}" ),
        };

        SceneAction sceneAction = actionPool.Obtain();

        sceneAction!.Pool = actionPool;

        return sceneAction;
    }

    public static AddSceneAction AddAction( SceneAction sceneAction )
    {
        var addAction = ( AddSceneAction )Action( typeof( AddSceneAction ) );
        addAction.Action = sceneAction;

        return addAction;
    }

    public static AddSceneAction AddAction( SceneAction sceneAction, Actor targetActor )
    {
        var addAction = ( AddSceneAction )Action( typeof( AddSceneAction ) );
        addAction.Target = targetActor;
        addAction.Action = sceneAction;

        return addAction;
    }

    public static RemoveSceneAction RemoveAction( SceneAction sceneAction )
    {
        var removeAction = ( RemoveSceneAction )Action( typeof( RemoveSceneAction ) );
        removeAction.Action = sceneAction;

        return removeAction;
    }

    public static RemoveSceneAction RemoveAction( SceneAction sceneAction, Actor targetActor )
    {
        var removeAction = ( RemoveSceneAction )Action( typeof( RemoveSceneAction ) );
        removeAction.Target = targetActor;
        removeAction.Action = sceneAction;

        return removeAction;
    }

    /// <summary>
    /// Moves the actor instantly.
    /// </summary>
    public static MoveToSceneAction MoveTo( float x,
                                       float y,
                                       float duration = 0,
                                       IInterpolation? interpolation = null )
    {
        var action = ( MoveToSceneAction )Action( typeof( MoveToSceneAction ) );
        action.SetPosition( x, y );
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    public static MoveToSceneAction MoveToAligned( float x,
                                              float y,
                                              Align alignment,
                                              float duration = 0,
                                              IInterpolation? interpolation = null )
    {
        var action = ( MoveToSceneAction )Action( typeof( MoveToSceneAction ) );
        action.SetPosition( x, y, alignment );
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    public static MoveBySceneAction MoveBy( float amountX,
                                       float amountY,
                                       float duration = 0,
                                       IInterpolation? interpolation = null )
    {
        var action = ( MoveBySceneAction )Action( typeof( MoveBySceneAction ) );
        action.SetAmount( amountX, amountY );
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    public static SizeToSceneAction SizeTo( float x,
                                       float y,
                                       float duration = 0,
                                       IInterpolation? interpolation = null )
    {
        var action = ( SizeToSceneAction )Action( typeof( SizeToSceneAction ) );
        action.SetSize( x, y );
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    public static SizeBySceneAction SizeBy( float amountX,
                                       float amountY,
                                       float duration = 0,
                                       IInterpolation? interpolation = null )
    {
        var action = ( SizeBySceneAction )Action( typeof( SizeBySceneAction ) );
        action.SetAmount( amountX, amountY );
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    public static ScaleToSceneAction ScaleTo( float x,
                                         float y,
                                         float duration = 0,
                                         IInterpolation? interpolation = null )
    {
        var action = ( ScaleToSceneAction )Action( typeof( ScaleToSceneAction ) );
        action.SetScale( x, y );
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    public static ScaleBySceneAction ScaleBy( float amountX,
                                         float amountY,
                                         float duration = 0,
                                         IInterpolation? interpolation = null )
    {
        var action = ( ScaleBySceneAction )Action( typeof( ScaleBySceneAction ) );
        action.SetAmount( amountX, amountY );
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    public static RotateToSceneAction RotateTo( float rotation,
                                           float duration = 0,
                                           IInterpolation? interpolation = null )
    {
        var action = ( RotateToSceneAction )Action( typeof( RotateToSceneAction ) );
        action.Rotation      = rotation;
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    public static RotateBySceneAction RotateBy( float rotationAmount,
                                           float duration = 0,
                                           IInterpolation? interpolation = null )
    {
        var action = ( RotateBySceneAction )Action( typeof( RotateBySceneAction ) );
        action.Amount        = rotationAmount;
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    /// <summary>
    /// Transitions from the color at the time this action starts to the specified color.
    /// </summary>
    public static ColorSceneAction Color( Color color, float duration = 0, IInterpolation? interpolation = null )
    {
        var action = ( ColorSceneAction )Action( typeof( ColorSceneAction ) );
        action.EndColor      = color;
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    /// <summary>
    /// Transitions from the alpha at the time this action starts to the specified alpha.
    /// </summary>
    public static AlphaSceneAction Alpha( float a,
                                     float duration = 0,
                                     IInterpolation? interpolation = null )
    {
        var action = ( AlphaSceneAction )Action( typeof( AlphaSceneAction ) );
        action.Alpha         = a;
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    /// <summary>
    /// Transitions from the alpha at the time this action starts to an alpha of 0.
    /// </summary>
    public static AlphaSceneAction FadeOut( float duration )
    {
        return Alpha( 0, duration );
    }

    /// <summary>
    /// Transitions from the alpha at the time this action starts to an alpha of 0.
    /// </summary>
    public static AlphaSceneAction FadeOut( float duration, IInterpolation interpolation )
    {
        var action = ( AlphaSceneAction )Action( typeof( AlphaSceneAction ) );
        action.Alpha         = 0;
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    /// <summary>
    /// Transitions from the alpha at the time this action starts to an alpha of 1.
    /// </summary>
    public static AlphaSceneAction FadeIn( float duration )
    {
        return Alpha( 1, duration );
    }

    /// <summary>
    /// Transitions from the alpha at the time this action starts to an alpha of 1.
    /// </summary>
    public static AlphaSceneAction FadeIn( float duration, IInterpolation interpolation )
    {
        var action = ( AlphaSceneAction )Action( typeof( AlphaSceneAction ) );
        action.Alpha         = 1;
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    public static VisibleSceneAction Show()
    {
        return Visible( true );
    }

    public static VisibleSceneAction Hide()
    {
        return Visible( false );
    }

    public static VisibleSceneAction Visible( bool visible )
    {
        var action = ( VisibleSceneAction )Action( typeof( VisibleSceneAction ) );
        action.Visible = visible;

        return action;
    }

    public static TouchableSceneAction Touchable( Touchable touchable )
    {
        var action = ( TouchableSceneAction )Action( typeof( TouchableSceneAction ) );
        action.Touchable = touchable;

        return action;
    }

    public static RemoveActorSceneAction RemoveActor()
    {
        return ( RemoveActorSceneAction )Action( typeof( RemoveActorSceneAction ) );
    }

    public static RemoveActorSceneAction RemoveActor( Actor removeActor )
    {
        var action = ( RemoveActorSceneAction )Action( typeof( RemoveActorSceneAction ) );
        action.Target = removeActor;

        return action;
    }

    public static DelaySceneAction Delay( float duration )
    {
        var action = ( DelaySceneAction )Action( typeof( DelaySceneAction ) );
        action.Duration = duration;

        return action;
    }

    public static DelaySceneAction Delay( float duration, SceneAction delayedSceneAction )
    {
        var action = ( DelaySceneAction )Action( typeof( DelaySceneAction ) );
        action.Duration = duration;
        action.Action   = delayedSceneAction;

        return action;
    }

    public static TimeScaleSceneAction TimeScale( float scale, SceneAction scaledSceneAction )
    {
        var action = ( TimeScaleSceneAction )Action( typeof( TimeScaleSceneAction ) );
        action.Scale  = scale;
        action.Action = scaledSceneAction;

        return action;
    }

    public static SequenceSceneAction Sequence( SceneAction action1 )
    {
        var action = ( SequenceSceneAction )Action( typeof( SequenceSceneAction ) );
        action.AddAction( action1 );

        return action;
    }

    public static SequenceSceneAction Sequence( SceneAction action1, SceneAction action2 )
    {
        var action = ( SequenceSceneAction )Action( typeof( SequenceSceneAction ) );
        action.AddAction( action1 );
        action.AddAction( action2 );

        return action;
    }

    public static SequenceSceneAction Sequence( SceneAction action1, SceneAction action2, SceneAction action3 )
    {
        var action = ( SequenceSceneAction )Action( typeof( SequenceSceneAction ) );
        action.AddAction( action1 );
        action.AddAction( action2 );
        action.AddAction( action3 );

        return action;
    }

    public static SequenceSceneAction Sequence( SceneAction action1, SceneAction action2, SceneAction action3, SceneAction action4 )
    {
        var action = ( SequenceSceneAction )Action( typeof( SequenceSceneAction ) );
        action.AddAction( action1 );
        action.AddAction( action2 );
        action.AddAction( action3 );
        action.AddAction( action4 );

        return action;
    }

    public static SequenceSceneAction Sequence( SceneAction action1, SceneAction action2, SceneAction action3, SceneAction action4,
                                           SceneAction action5 )
    {
        var action = ( SequenceSceneAction )Action( typeof( SequenceSceneAction ) );
        action.AddAction( action1 );
        action.AddAction( action2 );
        action.AddAction( action3 );
        action.AddAction( action4 );
        action.AddAction( action5 );

        return action;
    }

    public static SequenceSceneAction Sequence( params SceneAction[] actions )
    {
        var action = ( SequenceSceneAction )Action( typeof( SequenceSceneAction ) );

        for ( int i = 0, n = actions.Length; i < n; i++ )
        {
            action.AddAction( actions[ i ] );
        }

        return action;
    }

    public static SequenceSceneAction Sequence()
    {
        return ( SequenceSceneAction )Action( typeof( SequenceSceneAction ) );
    }

    public static ParallelSceneAction Parallel( SceneAction action1 )
    {
        var action = ( ParallelSceneAction )Action( typeof( ParallelSceneAction ) );
        action.AddAction( action1 );

        return action;
    }

    public static ParallelSceneAction Parallel( SceneAction action1, SceneAction action2 )
    {
        var action = ( ParallelSceneAction )Action( typeof( ParallelSceneAction ) );
        action.AddAction( action1 );
        action.AddAction( action2 );

        return action;
    }

    public static ParallelSceneAction Parallel( SceneAction action1, SceneAction action2, SceneAction action3 )
    {
        var action = ( ParallelSceneAction )Action( typeof( ParallelSceneAction ) );
        action.AddAction( action1 );
        action.AddAction( action2 );
        action.AddAction( action3 );

        return action;
    }

    public static ParallelSceneAction Parallel( SceneAction action1, SceneAction action2, SceneAction action3, SceneAction action4 )
    {
        var action = ( ParallelSceneAction )Action( typeof( ParallelSceneAction ) );
        action.AddAction( action1 );
        action.AddAction( action2 );
        action.AddAction( action3 );
        action.AddAction( action4 );

        return action;
    }

    public static ParallelSceneAction Parallel( SceneAction action1, SceneAction action2, SceneAction action3, SceneAction action4,
                                           SceneAction action5 )
    {
        var action = ( ParallelSceneAction )Action( typeof( ParallelSceneAction ) );
        action.AddAction( action1 );
        action.AddAction( action2 );
        action.AddAction( action3 );
        action.AddAction( action4 );
        action.AddAction( action5 );

        return action;
    }

    public static ParallelSceneAction Parallel( params SceneAction[] actions )
    {
        var action = ( ParallelSceneAction )Action( typeof( ParallelSceneAction ) );

        for ( int i = 0, n = actions.Length; i < n; i++ )
        {
            action.AddAction( actions[ i ] );
        }

        return action;
    }

    public static ParallelSceneAction Parallel()
    {
        return ( ParallelSceneAction )Action( typeof( ParallelSceneAction ) );
    }

    public static RepeatSceneAction Repeat( int count, SceneAction repeatedSceneAction )
    {
        var action = ( RepeatSceneAction )Action( typeof( RepeatSceneAction ) );
        action.RepeatCount = count;
        action.Action      = repeatedSceneAction;

        return action;
    }

    public static RepeatSceneAction Forever( SceneAction repeatedSceneAction )
    {
        var action = ( RepeatSceneAction )Action( typeof( RepeatSceneAction ) );
        action.RepeatCount = RepeatSceneAction.Forever;
        action.Action      = repeatedSceneAction;

        return action;
    }

    public static RunnableSceneAction Run( IRunnable.Runnable runnable )
    {
        var action = ( RunnableSceneAction )Action( typeof( RunnableSceneAction ) );
        action.RunnableTask = runnable;

        return action;
    }

    public static LayoutSceneAction Layout( bool enabled )
    {
        var action = ( LayoutSceneAction )Action( typeof( LayoutSceneAction ) );
        action.Enabled = enabled;

        return action;
    }

    public static AfterSceneAction After( SceneAction sceneAction )
    {
        var afterAction = ( AfterSceneAction )Action( typeof( AfterSceneAction ) );
        afterAction.Action = sceneAction;

        return afterAction;
    }

    public static AddListenerSceneAction AddListener( IEventListener listener, bool capture )
    {
        var addAction = ( AddListenerSceneAction )Action( typeof( AddListenerSceneAction ) );
        addAction.Listener  = listener;
        addAction.IsCapture = capture;

        return addAction;
    }

    public static AddListenerSceneAction AddListener( IEventListener listener, bool capture, Actor targetActor )
    {
        var addAction = ( AddListenerSceneAction )Action( typeof( AddListenerSceneAction ) );
        addAction.Target    = targetActor;
        addAction.Listener  = listener;
        addAction.IsCapture = capture;

        return addAction;
    }

    public static RemoveListenerSceneAction RemoveListener( IEventListener listener, bool capture )
    {
        var addAction = ( RemoveListenerSceneAction )Action( typeof( RemoveListenerSceneAction ) );
        addAction.Listener = listener;
        addAction.Capture  = capture;

        return addAction;
    }

    public static RemoveListenerSceneAction RemoveListener( IEventListener listener, bool capture, Actor targetActor )
    {
        var addAction = ( RemoveListenerSceneAction )Action( typeof( RemoveListenerSceneAction ) );
        addAction.Target   = targetActor;
        addAction.Listener = listener;
        addAction.Capture  = capture;

        return addAction;
    }

    /// <summary>
    /// Sets the target of an action and returns the action
    /// </summary>
    /// <param name="target"> the desired target of the action </param>
    /// <param name="sceneAction"> the action on which to set the target </param>
    /// <returns> the action with its target set </returns>
    public static SceneAction Targeting( Actor target, SceneAction sceneAction )
    {
        sceneAction.Target = target;

        return sceneAction;
    }
}