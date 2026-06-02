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

    // ========================================================================

    /// <summary>
    /// Gets a pool for the specified type from the pool dictionary. If the pool does not exist,
    /// a new pool is created and added to the dictionary.
    /// </summary>
    /// <typeparam name="T"> The type of SceneAction required. </typeparam>
    private static IScenePool GetPool< T >() where T : SceneAction, new()
    {
        return _pools.GetOrAdd( typeof( T ), _ => new ScenePoolAdapter< T >() );
    }

    /// <summary>
    /// Returns a new or pooled action of the specified type.
    /// </summary>
    public static T ObtainAction< T >() where T : SceneAction, new()
    {
        Logger.Debug( $"Obtaining new action of type {typeof( T ).Name}" );

        IScenePool pool   = GetPool< T >();
        var        action = pool.Obtain() as T;

        Guard.Against.Null( action );

        action.Pool = pool;

        return action;
    }

    /// <summary>
    /// Obtains a new or pooled action of the type <see cref="Actions.AddAction"/>, and sets
    /// its Action property to the provided SceneAction.
    /// </summary>
    /// <param name="sceneAction"> The action to set. </param>
    /// <returns> The new or pooled action. </returns>
    /// <exception cref="ArgumentException"></exception>
    public static AddAction AddAction( SceneAction sceneAction )
    {
        Guard.Against.Null( sceneAction );

        var addAction = ObtainAction< AddAction >();

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
    public static AddAction AddAction( SceneAction sceneAction, Actor targetActor )
    {
        Guard.Against.Null( sceneAction );
        Guard.Against.Null( targetActor );

        var addAction = ObtainAction< AddAction >();

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
    public static RemoveAction RemoveAction( SceneAction sceneAction )
    {
        Guard.Against.Null( sceneAction );

        var removeAction = ObtainAction< RemoveAction >();

        removeAction.Action = sceneAction;

        return removeAction;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sceneAction"></param>
    /// <param name="targetActor"></param>
    /// <returns></returns>
    public static RemoveAction RemoveAction( SceneAction sceneAction, Actor targetActor )
    {
        Guard.Against.Null( sceneAction );
        Guard.Against.Null( targetActor );

        var removeAction = ObtainAction< RemoveAction >();

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
    public static MoveToAction MoveTo( float x,
                                            float y,
                                            float duration = 0,
                                            IInterpolation? interpolation = null )
    {
        var action = ObtainAction< MoveToAction >();

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
    public static MoveToAction MoveToAligned( float x,
                                                   float y,
                                                   Align alignment,
                                                   float duration = 0,
                                                   IInterpolation? interpolation = null )
    {
        var action = ObtainAction< MoveToAction >();

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
    public static MoveByAction MoveBy( float amountX,
                                            float amountY,
                                            float duration = 0,
                                            IInterpolation? interpolation = null )
    {
        var action = ObtainAction< MoveByAction >();

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
    public static SizeToAction SizeTo( float x,
                                            float y,
                                            float duration = 0,
                                            IInterpolation? interpolation = null )
    {
        var action = ObtainAction< SizeToAction >();

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
    public static SizeByAction SizeBy( float amountX,
                                            float amountY,
                                            float duration = 0,
                                            IInterpolation? interpolation = null )
    {
        var action = ObtainAction< SizeByAction >();

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
    public static ScaleToAction ScaleTo( float x,
                                              float y,
                                              float duration = 0,
                                              IInterpolation? interpolation = null )
    {
        var action = ObtainAction< ScaleToAction >();

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
    public static ScaleByAction ScaleBy( float amountX,
                                              float amountY,
                                              float duration = 0,
                                              IInterpolation? interpolation = null )
    {
        var action = ObtainAction< ScaleByAction >();

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
    public static RotateToAction RotateTo( float rotation,
                                                float duration = 0,
                                                IInterpolation? interpolation = null )
    {
        var action = ObtainAction< RotateToAction >();

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
    public static RotateByAction RotateBy( float rotationAmount,
                                                float duration = 0,
                                                IInterpolation? interpolation = null )
    {
        var action = ObtainAction< RotateByAction >();

        action.Amount        = rotationAmount;
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    /// <summary>
    /// Transitions from the color at the time this action starts to the specified color.
    /// </summary>
    public static ColorAction Color( Color color, float duration = 0, IInterpolation? interpolation = null )
    {
        var action = ObtainAction< ColorAction >();

        action.EndColor      = color;
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    /// <summary>
    /// Transitions from the alpha at the time this action starts to the specified alpha.
    /// </summary>
    public static AlphaAction Alpha( float alpha, float duration = 0, IInterpolation? interpolation = null )
    {
        var action = ObtainAction< AlphaAction >();

        action.EndAlpha      = alpha;
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    /// <summary>
    /// Transitions from the alpha at the time this action starts to an alpha of 0.
    /// </summary>
    public static AlphaAction FadeOut( float duration )
    {
        return Alpha( 0, duration );
    }

    /// <summary>
    /// Transitions from the alpha at the time this action starts to an alpha of 0.
    /// </summary>
    public static AlphaAction FadeOut( float duration, IInterpolation interpolation )
    {
        Guard.Against.Null( interpolation );

        var action = ObtainAction< AlphaAction >();

        action.EndAlpha      = 0.0f;
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    /// <summary>
    /// Transitions from the alpha at the time this action starts to an alpha of 1.
    /// </summary>
    public static AlphaAction FadeIn( float duration )
    {
        return Alpha( 1f, duration );
    }

    /// <summary>
    /// Transitions from the alpha at the time this action starts to an alpha of 1.
    /// </summary>
    public static AlphaAction FadeIn( float duration, IInterpolation interpolation )
    {
        Guard.Against.Null( interpolation );

        var action = ObtainAction< AlphaAction >();

        action.EndAlpha      = 1.0f;
        action.Duration      = duration;
        action.Interpolation = interpolation;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static VisibleAction Show()
    {
        return Visible( true );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static VisibleAction Hide()
    {
        return Visible( false );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="visible"></param>
    /// <returns></returns>
    public static VisibleAction Visible( bool visible )
    {
        var action = ObtainAction< VisibleAction >();

        action.Visible = visible;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="touchable"> A value from the <see cref="Touchable"/> enum. </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static TouchableAction Touchable( Touchable touchable )
    {
        Guard.Against.EnumOutOfRange( typeof( Touchable ), touchable );

        var action = ObtainAction< TouchableAction >();

        action.Touchable = touchable;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static RemoveActorAction RemoveActor()
    {
        return ObtainAction< RemoveActorAction >();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="removeActor"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static RemoveActorAction RemoveActor( Actor removeActor )
    {
        Guard.Against.Null( removeActor );

        var action = ObtainAction< RemoveActorAction >();

        action.Target = removeActor;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static DelayAction Delay( float duration )
    {
        var action = ObtainAction< DelayAction >();

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
    public static DelayAction Delay( float duration, SceneAction delayedSceneAction )
    {
        Guard.Against.Null( delayedSceneAction );

        var action = ObtainAction< DelayAction >();

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
    public static TimeScaleAction TimeScale( float scale, SceneAction scaledSceneAction )
    {
        Guard.Against.Null( scaledSceneAction );

        var action = ObtainAction< TimeScaleAction >();

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
    public static SequenceAction Sequence( params SceneAction[] actions )
    {
        if ( actions.Length == 0 )
        {
            throw new ArgumentException( "No SceneActions provided for sequencing." );
        }

        if ( actions.Contains( null ) )
        {
            throw new ArgumentException( "Actions list contains null. This is not allowed." );
        }

        var action = ObtainAction< SequenceAction >();

        for ( int i = 0, n = actions.Length; i < n; i++ )
        {
            action.AddAction( actions[ i ] );
        }

        Logger.Debug( $"Created SequenceAction with {actions.Length} actions." );

        foreach ( var act in actions )
        {
            Logger.Debug( $" - {act}" );
        }
        
        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static SequenceAction Sequence()
    {
        return ObtainAction< SequenceAction >();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="actions"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static ParallelAction Parallel( params SceneAction[] actions )
    {
        if ( actions.Length == 0 )
        {
            throw new ArgumentException( "No SceneActions provided. At least "
                                       + "one is required for ParallelAction." );
        }

        if ( actions.Contains( null ) )
        {
            throw new ArgumentException( "Actions list contains null. This is not allowed." );
        }

        var action = ObtainAction< ParallelAction >();

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
    public static ParallelAction Parallel()
    {
        return ObtainAction< ParallelAction >();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="count"></param>
    /// <param name="repeatedSceneAction"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static RepeatAction Repeat( int count, SceneAction repeatedSceneAction )
    {
        Guard.Against.Null( repeatedSceneAction );

        var action = ObtainAction< RepeatAction >();

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
    public static RepeatAction Forever( SceneAction repeatedSceneAction )
    {
        Guard.Against.Null( repeatedSceneAction );

        var action = ObtainAction< RepeatAction >();

        action.RepeatCount = RepeatAction.Forever;
        action.Action      = repeatedSceneAction;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="runnable"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static RunnableAction Run( Action runnable )
    {
        Guard.Against.Null( runnable );

        var action = ObtainAction< RunnableAction >();

        action.RunnableTask = runnable;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="enabled"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static LayoutAction Layout( bool enabled )
    {
        var action = ObtainAction< LayoutAction >();

        action.Enabled = enabled;

        return action;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sceneAction"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static AfterAction After( SceneAction sceneAction )
    {
        Guard.Against.Null( sceneAction );

        var action = ObtainAction< AfterAction >();

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
    public static AddListenerAction AddListener( IEventListener listener, bool capture )
    {
        Guard.Against.Null( listener );

        var action = ObtainAction< AddListenerAction >();

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
    public static AddListenerAction AddListener( IEventListener listener, bool capture, Actor targetActor )
    {
        Guard.Against.Null( listener );
        Guard.Against.Null( targetActor );

        var action = ObtainAction< AddListenerAction >();

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
    public static RemoveListenerAction RemoveListener( IEventListener listener, bool capture )
    {
        Guard.Against.Null( listener );

        var action = ObtainAction< RemoveListenerAction >();

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
    public static RemoveListenerAction RemoveListener( IEventListener listener, bool capture, Actor targetActor )
    {
        Guard.Against.Null( listener );
        Guard.Against.Null( targetActor );

        var action = ObtainAction< RemoveListenerAction >();

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