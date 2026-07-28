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

using LughSharp.Source.Scene2D.Actions;

namespace LughSharp.Source.Scene2D.UI;

/// <summary>
/// Keeps track of an application's tooltips.
/// </summary>
[PublicAPI]
public class TooltipManager< T > where T : Actor
{
    /// <summary>
    /// The X distance from the mouse position to offset the tooltip actor. Default is 15.
    /// </summary>
    public float OffsetX { get; set; } = 15;

    /// <summary>
    /// The Y distance from the mouse position to offset the tooltip actor. Default is 19.
    /// </summary>
    public float OffsetY { get; set; } = 19;

    /// <summary>
    /// The maximum width of a <see cref="TextTooltip"/>. The label will wrap if needed.
    /// Default is <see cref="int.MaxValue"/>.
    /// </summary>
    public float MaxWidth { get; set; } = int.MaxValue;

    /// <summary>
    /// The distance from the tooltip actor position to the edge of the screen where the
    /// actor will be shown on the other side of the mouse cursor. Default is 7.
    /// </summary>
    public float EdgeDistance { get; set; } = 7;

    /// <summary>
    /// Seconds from when an actor is hovered to when the tooltip is shown. Default is 2.
    /// Call <see cref="HideAll()"/> after changing to reset internal state.
    /// </summary>
    public float InitialTime { get; set; } = 2;

    /// <summary>
    /// Once a tooltip is shown, this is used instead of <see cref="InitialTime"/>.
    /// Default is 0.
    /// </summary>
    public float SubsequentTime { get; set; }

    /// <summary>
    /// Seconds to use <see cref="SubsequentTime"/>. Default is 1.5f.
    /// </summary>
    public float ResetTime { get; set; } = 1.5f;

    /// <summary>
    /// If false, tooltips will not be shown. Default is true.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// If false, tooltips will be shown without animations. Default is true.
    /// </summary>
    public bool Animations { get; set; } = true;

    // ========================================================================

    private readonly List< Tooltip< T > > _activeTooltips = [ ];

    private readonly CancellationToken       _showTaskCancellationToken;
    private readonly CancellationTokenSource _showTaskCancellationTokenSource;
    private          Task                    _showTask = null!;

    private Tooltip< T > _showTooltip = null!;
    private float        _time;

    // ========================================================================

    /// <summary>
    /// Initializes a new instance of the <see cref="TooltipManager{T}"/> class.
    /// </summary>
    public TooltipManager()
    {
        _time                            = InitialTime;
        _showTaskCancellationTokenSource = new CancellationTokenSource();
        _showTaskCancellationToken       = _showTaskCancellationTokenSource.Token;

        Create();
    }

    /// <summary>
    /// Creates and initializes a new task to manage tooltip display behavior.
    /// Ensures that the tooltip is added to the stage, brought to the front,
    /// and associated actions are performed. Respects cancellation tokens to
    /// handle task termination cleanly.
    /// </summary>
    private void Create()
    {
        //@formatter:off
        _showTask = new Task( () =>
        {
            Stage? stage = _showTooltip.TargetActor?.GetStage();

            if ( stage == null )
            {
                return;
            }

            stage.AddActor( _showTooltip.Container );

            _showTooltip.Container.BringToFront();
            _activeTooltips.Add( _showTooltip );
            _showTooltip.Container.ClearActions();

            ShowAction( _showTooltip );

            if ( !_showTooltip.Instant )
            {
                _time = SubsequentTime;
            }

            if ( _showTaskCancellationToken.IsCancellationRequested )
            {
                _showTaskCancellationToken.ThrowIfCancellationRequested();
            }
        },_showTaskCancellationToken );
        //@formatter:on
    }

    /// <summary>
    /// Handles the TouchDown event for the specified tooltip. This method initializes
    /// tooltip removal and prepares it for display based on the current settings.
    /// </summary>
    /// <param name="tooltip">The tooltip to handle the TouchDown event for.</param>
    public void TouchDown( Tooltip< T > tooltip )
    {
        CancelTask();

        tooltip.Container.Remove();

        _time = InitialTime;

        if ( Enabled || tooltip.Always )
        {
            _showTooltip = tooltip;

//            Timer.schedule( showTask, time );
        }
    }

    /// <summary>
    /// Handles the entry of a specified tooltip into the TooltipManager, updating its
    /// visibility and interaction state.
    /// </summary>
    /// <param name="tooltip">
    /// The tooltip instance to be registered and displayed in the manager.
    /// </param>
    public void Enter( Tooltip< T > tooltip )
    {
        _showTooltip = tooltip;

        CancelTask();

        if ( Enabled || tooltip.Always )
        {
            if ( ( _time == 0 ) || tooltip.Instant )
            {
                _showTask.Start();
            }
        }
    }

    /// <summary>
    /// Hides the specified tooltip by removing it from the active tooltips list
    /// and invoking the hide action if it has a parent container.
    /// </summary>
    /// <param name="tooltip">The tooltip to be hidden.</param>
    public void Hide( Tooltip< T > tooltip )
    {
        _showTooltip = null!;
        CancelTask();

        if ( tooltip.Container.HasParent() )
        {
            _activeTooltips.Remove( tooltip );
            HideAction( tooltip );
        }
    }

    /// <summary>
    /// Called when tooltip is shown. Default implementation sets actions to animate showing.
    /// </summary>
    /// <param name="tooltip">The tooltip instance being shown.</param>
    protected void ShowAction( Tooltip< T > tooltip )
    {
        float actionTime = Animations ? _time > 0 ? 0.5f : 0.15f : 0.1f;

        tooltip.Container.Transform    = true;
        tooltip.Container.ActorColor.A = 0.2f;
        tooltip.Container.SetScale( 0.05f );

        tooltip.Container.AddAction
            (
             Actions.SceneActions.Parallel
                 (
                  Actions.SceneActions.FadeIn( actionTime, Interpolation.Fade ),
                  Actions.SceneActions.ScaleTo( 1, 1, actionTime, Interpolation.Fade )
                 )
            );
    }

    /// <summary>
    /// Called when tooltip is hidden. Default implementation sets actions to animate hiding
    /// and to remove the actor from the stage when the actions are complete. A subclass must
    /// at least remove the actor.
    /// </summary>
    /// <param name="tooltip">The tooltip to be hidden.</param>
    protected static void HideAction( Tooltip< T > tooltip )
    {
        tooltip.Container.AddAction
            (
             Actions.SceneActions.Sequence
                 (
                  Actions.SceneActions.Parallel
                      (
                       Actions.SceneActions.Alpha( 0.2f, 0.2f, Interpolation.Fade ),
                       Actions.SceneActions.ScaleTo( 0.05f, 0.05f, 0.2f, Interpolation.Fade )
                      ),
                  Actions.SceneActions.RemoveActor()
                 )
            );
    }

    /// <summary>
    /// Hides all active tooltips managed by this instance and clears the internally
    /// tracked state of active tooltips. Resets the display timer and nullifies the
    /// currently showing tooltip reference.
    /// </summary>
    public void HideAll()
    {
        CancelTask();

        _time        = InitialTime;
        _showTooltip = null!;

        foreach ( Tooltip< T > tooltip in _activeTooltips )
        {
            tooltip.Hide();
        }

        _activeTooltips.Clear();
    }

    /// <summary>
    /// Shows all tooltips on hover without a delay for <see cref="ResetTime"/> seconds.
    /// </summary>
    public void ShowInstantly()
    {
        _time = 0;
        _showTask.Start();
        CancelTask();
    }

    /// <summary>
    /// Cancels the currently running task associated with showing a tooltip.
    /// </summary>
    /// <remarks>
    /// If a task is currently running, it will be interrupted to prevent further processing.
    /// This method ensures that no tooltip display logic continues when it's no longer needed.
    /// </remarks>
    private void CancelTask()
    {
        if ( _showTask is { Status: TaskStatus.Running } )
        {
            _showTaskCancellationTokenSource.Cancel();
        }
    }
}

// ============================================================================
// ============================================================================


