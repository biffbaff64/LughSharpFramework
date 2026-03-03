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

namespace LughSharp.Core.Input;

/// <summary>
/// An adapter class for <see cref="IInputProcessor"/>.
/// You can derive from this and only override what you are interested in.
/// </summary>
[PublicAPI]
public class InputAdapter : IInputProcessor
{
    /// <inheritdoc />
    public virtual bool OnKeyDown( int keycode )
    {
        return false;
    }

    /// <inheritdoc />
    public virtual bool OnKeyUp( int keycode )
    {
        return false;
    }

    /// <inheritdoc />
    public virtual bool OnKeyTyped( char character )
    {
        return false;
    }

    /// <inheritdoc />
    public virtual bool OnTouchDown( int screenX, int screenY, int pointer, int button )
    {
        return false;
    }

    /// <inheritdoc />
    public virtual bool OnTouchUp( int screenX, int screenY, int pointer, int button )
    {
        return false;
    }

    /// <inheritdoc />
    public virtual bool OnTouchDragged( int screenX, int screenY, int pointer )
    {
        return false;
    }

    /// <inheritdoc />
    public virtual bool OnMouseMoved( int screenX, int screenY )
    {
        return false;
    }

    /// <inheritdoc />
    public virtual bool OnScrolled( float amountX, float amountY )
    {
        return false;
    }
}

// ============================================================================
// ============================================================================