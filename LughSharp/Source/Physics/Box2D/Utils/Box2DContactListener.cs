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

namespace LughSharp.Source.Physics.Box2D.Utils;

[PublicAPI]
public class Box2DContactListener : Multiplexer< IContactListener >, IContactListener
{
    /// <summary>
    ///  Called when two fixtures begin to touch.
    /// </summary>
    public void BeginContact( Contact contact )
    {
        if ( Receivers.Count > 0 )
        {
            foreach ( IContactListener listener in Receivers )
            {
                listener.BeginContact( contact );
            }
        }
    }

    /// <summary>
    ///  Called when two fixtures cease to touch.
    /// </summary>
    public void EndContact( Contact contact )
    {
        if ( Receivers.Count > 0 )
        {
            foreach ( IContactListener listener in Receivers )
            {
                listener.EndContact( contact );
            }
        }
    }

    /// <summary>
    ///  This is called after a contact is updated. This allows you to inspect
    ///  a contact before it goes to the solver. If you are careful, you can modify
    ///  the contact manifold (e.g. disable contact). A copy of the old manifold is
    ///  provided so that you can detect changes.
    /// 
    ///  Note: this is called only for awake bodies.
    ///  Note: this is called even when the number of contact points is zero.
    ///  Note: this is not called for sensors.
    ///  Note: if you set the number of contact points to zero, you will not get an
    ///  EndContact callback. However, you may get a BeginContact callback the next step.
    /// </summary>
    public void PreSolve( Contact contact, Manifold oldManifold )
    {
        if ( Receivers.Count > 0 )
        {
            foreach ( IContactListener listener in Receivers )
            {
                listener.PreSolve( contact, oldManifold );
            }
        }
    }

    /// <summary>
    ///  This lets you inspect a contact after the solver is finished. This
    ///  is useful for inspecting impulses.
    /// 
    ///  Note: the contact manifold does not include time of impact impulses,
    ///  which can be arbitrarily large if the sub-step is small. Hence the
    ///  impulse is provided explicitly in a separate data structure.
    /// = Note: this is only called for contacts that are touching, solid, and awake.
    /// </summary>
    public void PostSolve( Contact contact, ContactImpulse impulse )
    {
        if ( Receivers.Count > 0 )
        {
            foreach ( IContactListener listener in Receivers )
            {
                listener.PostSolve( contact, impulse );
            }
        }
    }

    /// <summary>
    /// Adds a new contact listener to receive contact-related events.
    /// <param name="listener">The contact listener to be added.</param>
    /// </summary>
    public void AddListener( IContactListener listener )
    {
        Receivers.Add( listener );
    }
}

// ============================================================================
// ============================================================================
