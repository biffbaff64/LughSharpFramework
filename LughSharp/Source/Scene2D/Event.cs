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

namespace LughSharp.Source.Scene2D;

/// <summary>
/// The base class for all events.
/// By default an event will "bubble" up through an actor's parent's handlers
/// (see <see cref="Bubbles"/>).
/// <para>
/// An actor's capture listeners can stop() an event to prevent child actors
/// from seeing it.
/// </para>
/// <para>
/// An Event may be marked as "handled" which will end its propagation outside
/// of the Stage (see <see cref="IsHandled"/>). The default Actor.fire(Event)
/// will mark events handled if an EventListener returns true.
/// A cancelled event will be stopped and handled. Additionally, many actors
/// will undo the side-effects of a canceled event. (See <see cref="IsCancelled"/>)
/// </para>
/// </summary>
[PublicAPI]
public class Event : IResetable
{
    /// <summary>
    /// The Stage for the Actor the event was fired on.
    /// </summary>
    public Stage? Stage { get; set; }

    /// <summary>
    /// The Actor this event originated from.
    /// </summary>
    public Actor? TargetActor { get; set; }

    /// <summary>
    /// The Actor this listener is attached to.
    /// </summary>
    public Actor? ListenerActor { get; set; }

    /// <summary>
    /// true means event occurred during the capture phase
    /// </summary>
    public bool Capture { get; set; }

    /// <summary>
    /// If true, after the event is fired on the target actor, it will also
    /// be fired on each of the parent actors, all the way to the root.
    /// </summary>
    public bool Bubbles { get; set; } = true;

    /// <summary>
    /// true means the event was handled (the stage will eat the input)
    /// </summary>
    public bool IsHandled { get; set; }

    /// <summary>
    /// true means event propagation was stopped
    /// </summary>
    public bool IsStopped { get; set; }

    /// <summary>
    /// true means propagation was stopped and any action that this event
    /// would cause should not happen
    /// </summary>
    public bool IsCancelled { get; set; }

    // ========================================================================
    
    /// <summary>
    /// Resets this event.
    /// </summary>
    public virtual void Reset()
    {
        Stage         = null;
        TargetActor   = null;
        ListenerActor = null;
        Capture       = false;
        Bubbles       = true;
        IsHandled     = false;
        IsStopped     = false;
        IsCancelled   = false;
    }

    /// <summary>
    /// Marks this event as handled. This does not affect event propagation inside
    /// scene2d, but causes the <see cref="Stage"/> <see cref="IInputProcessor"/>
    /// methods to return true, which will consume the event so it is not passed
    /// on to the application under the stage.
    /// </summary>
    public virtual void SetHandled()
    {
        IsHandled = true;
    }

    /// <summary>
    /// Marks this event cancelled. This handles the event and stops the event
    /// propagation. It also cancels any default action that would have been taken
    /// by the code that fired the event.
    /// <para>
    /// Eg, if the event is for a checkbox being checked, cancelling
    /// the event could uncheck the checkbox.
    /// </para>
    /// </summary>
    public virtual void Cancel()
    {
        IsCancelled = true;
        IsStopped   = true;
        IsHandled   = true;
    }

    /// <summary>
    /// Marks this event as being stopped. This halts event propagation. Any other
    /// listeners on the <see cref="ListenerActor"/> are notified, but
    /// after that no other listeners are notified.
    /// </summary>
    public virtual void Stop()
    {
        IsStopped = true;
    }
}

// ============================================================================
// ============================================================================

