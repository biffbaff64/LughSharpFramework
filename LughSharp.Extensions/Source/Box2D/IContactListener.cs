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

namespace Extensions.Source.Box2D;

[PublicAPI]
public interface IContactListener
{
    /// <summary>
    /// Called when two fixtures begin to touch.
    /// </summary>
    void BeginContact( Contact contact );

    /// <summary>
    /// Called when two fixtures cease to touch.
    /// </summary>
    void EndContact( Contact contact );

    /// <summary>
    /// This is called after a contact is updated. This allows you to inspect a contact before
    /// it goes to the solver. If you are careful, you can modify the contact manifold (e.g.
    /// disable contact). A copy of the old manifold is provided so that you can detect changes.
    /// <br></br>
    /// <para>
    /// Note: this is called only for awake bodies.
    /// </para>
    /// <para>
    /// Note: this is called even when the number of contact points is zero.
    /// </para>
    /// <para>
    /// Note: this is not called for sensors. Note: if you set the number of contact points to
    /// zero, you will not get an EndContact callback. However, you may get a BeginContact
    /// callback the next step.
    /// </para>
    /// </summary>
    void PreSolve( Contact contact, Manifold oldManifold );

    /// <summary>
    /// This lets you inspect a contact after the solver is finished. This is useful for
    /// inspecting impulses.
    /// <br></br>
    /// <para>
    /// Note: the contact manifold does not include time of impact impulses, which can be
    /// arbitrarily large if the sub-step is small. Hence the impulse is provided explicitly
    /// in a separate data structure.
    /// </para>
    /// <para>
    /// Note: this is only called for contacts that are touching, solid, and awake.
    /// </para>
    /// </summary>
    void PostSolve( Contact contact, ContactImpulse impulse );
}

// ============================================================================
// ============================================================================