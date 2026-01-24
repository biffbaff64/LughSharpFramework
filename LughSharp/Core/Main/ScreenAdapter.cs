// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Richard Ikin.
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

namespace LughSharp.Core.Main;

/// <summary>
/// Convenience implementation of <see cref="IScreen"/>. Derive from this and
/// only override what you need.
/// <para>
/// Using this means that any app screens use only what they need and don't
/// duplicate unnecessary code.
/// </para>
/// </summary>
[PublicAPI]
public class ScreenAdapter : IScreen
{
    /// <summary>
    /// Called when this screen becomes the current screen for
    /// a <see cref="Game"/>.
    /// </summary>
    public virtual void Show()
    {
    }

    /// <summary>
    /// Called when the screen should update itself.
    /// </summary>
    /// <param name="delta"> The time in seconds since the last update. </param>
    public virtual void Update( float delta )
    {
    }

    /// <summary>
    /// Called when the screen should render itself.
    /// </summary>
    /// <param name="delta"> The time in seconds since the last render. </param>
    public virtual void Render( float delta )
    {
    }

    /// <summary>
    /// Called when the Application is resized. This can happen at any point during
    /// a non-paused state but will never happen before a call to create().
    /// </summary>
    public virtual void Resize( int width, int height )
    {
    }

    /// <summary>
    /// Called when the Application is paused, usually when it's not active or visible
    /// on-screen. An Application is also paused before it is destroyed.
    /// </summary>
    public virtual void Pause()
    {
    }

    /// <summary>
    /// Called when the Application is resumed from a paused state, usually when
    /// it regains focus.
    /// </summary>
    public virtual void Resume()
    {
    }

    /// <summary>
    /// Called when this screen is no longer the current screen for a <see cref="Game"/>.
    /// </summary>
    public virtual void Hide()
    {
    }

    /// <summary>
    /// Called when this screen should release all resources.
    /// </summary>
    public virtual void Close()
    {
    }
}

// ============================================================================
// ============================================================================
