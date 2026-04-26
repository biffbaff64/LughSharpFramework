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

using System.Collections.Concurrent;

using LughSharp.Source.Scene2D.Listeners;
using LughSharp.Source.Scene2D.Utils;

namespace LughSharp.Source.Scene2D.Actions;

[PublicAPI]
public class SceneActions
{
    /// <summary>
    /// The default pool for scene actions.
    /// </summary>
    private static readonly ConcurrentDictionary< Type, IScenePool > _pools = new();

    private static IScenePool GetPool< T >() where T : SceneAction, new()
    {
        return _pools.GetOrAdd( typeof( T ), _ => new ScenePoolAdapter< T >() );
    }

    /// <summary>
    /// Returns a new or pooled action of the specified type.
    /// </summary>
    public static T ObtainAction< T >() where T : SceneAction, new()
    {
        var pool   = GetPool< T >();
        var action = pool.Obtain() as T;

        Guard.Against.Null( action );

        action.Pool = pool;

        return action;
    }

    /// <summary>
    /// Obtains a new or pooled action of the type <see cref="AddSceneAction"/>, and sets
    /// its Action property to the provided SceneAction.
    /// </summary>
    /// <param name="sceneAction"> The action to set. </param>
    /// <returns> The new or pooled action. </returns>
    /// <exception cref="ArgumentException"></exception>
    public static AddSceneAction AddAction( SceneAction sceneAction )
    {
        Guard.Against.Null( sceneAction );

        AddSceneAction addAction = ObtainAction< AddSceneAction >();

        addAction.Action = sceneAction;

        return addAction;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sceneAction"></param>
    /// <param name="targetActor"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static AddSceneAction AddAction( SceneAction sceneAction, Actor targetActor )
    {
        Guard.Against.Null( sceneAction );
        Guard.Against.Null( targetActor );

        AddSceneAction addAction = ObtainAction< AddSceneAction >();

        addAction.Target = targetActor;
        addAction.Action = sceneAction;

        return addAction;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sceneAction"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static RemoveSceneAction RemoveAction( SceneAction sceneAction )
    {
        Guard.Against.Null( sceneAction );

        RemoveSceneAction removeAction = ObtainAction< RemoveSceneAction >();

        removeAction.Action = sceneAction;

        return removeAction;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sceneAction"></param>
    /// <param name="targetActor"></param>
    /// <returns></returns>
    public static RemoveSceneAction RemoveAction( SceneAction sceneAction, Actor targetActor )
    {
        Guard.Against.Null( sceneAction );
        Guard.Against.Null( targetActor );

        RemoveSceneAction removeAction = ObtainAction< RemoveSceneAction >();

        removeAction.Target = targetActor;
        removeAction.Action = sceneAction;

        return removeAction;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="duration"></param>
    /// <param name="interpolation"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static MoveToSceneAction MoveTo( float x,
                                            float y,
                                            float duration = 0,
                                            IInterpolation? interpolation = null )
    {
        MoveToSceneAction action = ObtainAction< MoveToSceneAction >();

        action.SetPosition( x, y );
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="alignment"></param>
    /// <param name="duration"></param>
    /// <param name="interpolation"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static MoveToSceneAction MoveToAligned( float x,
                                                   float y,
                                                   Align alignment,
                                                   float duration = 0,
                                                   IInterpolation? interpolation = null )
    {
        MoveToSceneAction action = ObtainAction< MoveToSceneAction >();

        action.SetPosition( x, y, alignment );
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="amountX"></param>
    /// <param name="amountY"></param>
    /// <param name="duration"></param>
    /// <param name="interpolation"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static MoveBySceneAction MoveBy( float amountX,
                                            float amountY,
                                            float duration = 0,
                                            IInterpolation? interpolation = null )
    {
        MoveBySceneAction action = ObtainAction< MoveBySceneAction >();

        action.SetAmount( amountX, amountY );
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="duration"></param>
    /// <param name="interpolation"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static SizeToSceneAction SizeTo( float x,
                                            float y,
                                            float duration = 0,
                                            IInterpolation? interpolation = null )
    {
        SizeToSceneAction action = ObtainAction< SizeToSceneAction >();

        action.SetSize( x, y );
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="amountX"></param>
    /// <param name="amountY"></param>
    /// <param name="duration"></param>
    /// <param name="interpolation"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static SizeBySceneAction SizeBy( float amountX,
                                            float amountY,
                                            float duration = 0,
                                            IInterpolation? interpolation = null )
    {
        SizeBySceneAction action = ObtainAction< SizeBySceneAction >();

        action.SetAmount( amountX, amountY );
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="duration"></param>
    /// <param name="interpolation"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static ScaleToSceneAction ScaleTo( float x,
                                              float y,
                                              float duration = 0,
                                              IInterpolation? interpolation = null )
    {
        ScaleToSceneAction action = ObtainAction< ScaleToSceneAction >();

        action.SetScale( x, y );
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="amountX"></param>
    /// <param name="amountY"></param>
    /// <param name="duration"></param>
    /// <param name="interpolation"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static ScaleBySceneAction ScaleBy( float amountX,
                                              float amountY,
                                              float duration = 0,
                                              IInterpolation? interpolation = null )
    {
        ScaleBySceneAction action = ObtainAction< ScaleBySceneAction >();

        action.SetAmount( amountX, amountY );
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rotation"></param>
    /// <param name="duration"></param>
    /// <param name="interpolation"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static RotateToSceneAction RotateTo( float rotation,
                                                float duration = 0,
                                                IInterpolation? interpolation = null )
    {
        RotateToSceneAction action = ObtainAction< RotateToSceneAction >();

        action.Rotation      = rotation;
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rotationAmount"></param>
    /// <param name="duration"></param>
    /// <param name="interpolation"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static RotateBySceneAction RotateBy( float rotationAmount,
                                                float duration = 0,
                                                IInterpolation? interpolation = null )
    {
        RotateBySceneAction action = ObtainAction< RotateBySceneAction >();

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
        ColorSceneAction action = ObtainAction< ColorSceneAction >();

        action.EndColor      = color;
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    /// <summary>
    /// Transitions from the alpha at the time this action starts to the specified alpha.
    /// </summary>
    public static AlphaSceneAction Alpha( float alpha, float duration = 0, IInterpolation? interpolation = null )
    {
        AlphaSceneAction action = ObtainAction< AlphaSceneAction >();

        action.Alpha         = alpha;
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
        Guard.Against.Null( interpolation );

        AlphaSceneAction action = ObtainAction< AlphaSceneAction >();

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
        Guard.Against.Null( interpolation );

        AlphaSceneAction action = ObtainAction< AlphaSceneAction >();

        action.Alpha         = 1;
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static VisibleSceneAction Show()
    {
        return Visible( true );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static VisibleSceneAction Hide()
    {
        return Visible( false );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="visible"></param>
    /// <returns></returns>
    public static VisibleSceneAction Visible( bool visible )
    {
        VisibleSceneAction action = ObtainAction< VisibleSceneAction >();

        action.Visible = visible;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="touchable"> A value from the <see cref="Touchable"/> enum. </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static TouchableSceneAction Touchable( Touchable touchable )
    {
        Guard.Against.EnumOutOfRange( typeof( Touchable ), touchable );

        TouchableSceneAction action = ObtainAction< TouchableSceneAction >();

        action.Touchable = touchable;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static RemoveActorSceneAction RemoveActor()
    {
        return ObtainAction< RemoveActorSceneAction >();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="removeActor"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static RemoveActorSceneAction RemoveActor( Actor removeActor )
    {
        Guard.Against.Null( removeActor );

        RemoveActorSceneAction action = ObtainAction< RemoveActorSceneAction >();

        action.Target = removeActor;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static DelaySceneAction Delay( float duration )
    {
        DelaySceneAction action = ObtainAction< DelaySceneAction >();

        action.Duration = duration;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="delayedSceneAction"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static DelaySceneAction Delay( float duration, SceneAction delayedSceneAction )
    {
        Guard.Against.Null( delayedSceneAction );

        DelaySceneAction action = ObtainAction< DelaySceneAction >();

        action.Duration = duration;
        action.Action   = delayedSceneAction;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="scale"></param>
    /// <param name="scaledSceneAction"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static TimeScaleSceneAction TimeScale( float scale, SceneAction scaledSceneAction )
    {
        Guard.Against.Null( scaledSceneAction );

        TimeScaleSceneAction action = ObtainAction< TimeScaleSceneAction >();

        action.Scale  = scale;
        action.Action = scaledSceneAction;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="actions"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static SequenceSceneAction Sequence( params SceneAction[] actions )
    {
        if ( actions.Length == 0 )
        {
            throw new ArgumentException( "No SceneActions provided for sequencing." );
        }

        if ( actions.Contains( null ) )
        {
            throw new ArgumentException( "Actions list contains null. This is not allowed." );
        }

        SequenceSceneAction action = ObtainAction< SequenceSceneAction >();

        for ( int i = 0, n = actions.Length; i < n; i++ )
        {
            action.AddAction( actions[ i ] );
        }

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static SequenceSceneAction Sequence()
    {
        return ObtainAction< SequenceSceneAction >();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="actions"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static ParallelSceneAction Parallel( params SceneAction[] actions )
    {
        if ( actions.Length == 0 )
        {
            throw new ArgumentException( "No SceneActions provided. At least "
                                       + "one is required for ParallelSceneAction." );
        }

        if ( actions.Contains( null ) )
        {
            throw new ArgumentException( "Actions list contains null. This is not allowed." );
        }

        ParallelSceneAction action = ObtainAction< ParallelSceneAction >();

        for ( int i = 0, n = actions.Length; i < n; i++ )
        {
            action.AddAction( actions[ i ] );
        }

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static ParallelSceneAction Parallel()
    {
        return ObtainAction< ParallelSceneAction >();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="count"></param>
    /// <param name="repeatedSceneAction"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static RepeatSceneAction Repeat( int count, SceneAction repeatedSceneAction )
    {
        Guard.Against.Null( repeatedSceneAction );

        RepeatSceneAction action = ObtainAction< RepeatSceneAction >();

        action.RepeatCount = count;
        action.Action      = repeatedSceneAction;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="repeatedSceneAction"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static RepeatSceneAction Forever( SceneAction repeatedSceneAction )
    {
        Guard.Against.Null( repeatedSceneAction );

        RepeatSceneAction action = ObtainAction< RepeatSceneAction >();

        action.RepeatCount = RepeatSceneAction.Forever;
        action.Action      = repeatedSceneAction;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="runnable"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static RunnableSceneAction Run( Action runnable )
    {
        Guard.Against.Null( runnable );

        RunnableSceneAction action = ObtainAction< RunnableSceneAction >();

        action.RunnableTask = runnable;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="enabled"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static LayoutSceneAction Layout( bool enabled )
    {
        LayoutSceneAction action = ObtainAction< LayoutSceneAction >();

        action.Enabled = enabled;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sceneAction"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static AfterSceneAction After( SceneAction sceneAction )
    {
        Guard.Against.Null( sceneAction );

        AfterSceneAction action = ObtainAction< AfterSceneAction >();

        action.Action = sceneAction;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="listener"></param>
    /// <param name="capture"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static AddListenerSceneAction AddListener( IEventListener listener, bool capture )
    {
        Guard.Against.Null( listener );

        AddListenerSceneAction action = ObtainAction< AddListenerSceneAction >();

        action.Listener  = listener;
        action.IsCapture = capture;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="listener"></param>
    /// <param name="capture"></param>
    /// <param name="targetActor"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static AddListenerSceneAction AddListener( IEventListener listener, bool capture, Actor targetActor )
    {
        Guard.Against.Null( listener );
        Guard.Against.Null( targetActor );

        AddListenerSceneAction action = ObtainAction< AddListenerSceneAction >();

        action.Target    = targetActor;
        action.Listener  = listener;
        action.IsCapture = capture;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="listener"></param>
    /// <param name="capture"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static RemoveListenerSceneAction RemoveListener( IEventListener listener, bool capture )
    {
        Guard.Against.Null( listener );

        RemoveListenerSceneAction action = ObtainAction< RemoveListenerSceneAction >();

        action.Listener = listener;
        action.Capture  = capture;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="listener"></param>
    /// <param name="capture"></param>
    /// <param name="targetActor"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static RemoveListenerSceneAction RemoveListener( IEventListener listener, bool capture, Actor targetActor )
    {
        Guard.Against.Null( listener );
        Guard.Against.Null( targetActor );

        RemoveListenerSceneAction action = ObtainAction< RemoveListenerSceneAction >();

        action.Target   = targetActor;
        action.Listener = listener;
        action.Capture  = capture;

        return action;
    }

    /// <summary>
    /// Sets the target of an action and returns the action
    /// </summary>
    /// <param name="target"> the desired target of the action </param>
    /// <param name="sceneAction"> the action on which to set the target </param>
    /// <returns> the action with its target set </returns>
    public static SceneAction Targeting( Actor target, SceneAction sceneAction )
    {
        Guard.Against.Null( target );
        Guard.Against.Null( sceneAction );

        sceneAction.Target = target;

        return sceneAction;
    }
}

// ============================================================================
// ============================================================================