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

using LughSharp.Core.Utils.Collections;

namespace LughSharp.Core.Input;

[PublicAPI]
public class InputMultiplexer : IInputProcessor
{
    public SnapshotArrayList< IInputProcessor > Processors { get; set; } = new( 4 );

    // ========================================================================

    /// <summary>
    /// Constructor.
    /// Creates a new InputMultiplexer. It will not contain any Input Processors,
    /// these will need adding seperately.
    /// </summary>
    public InputMultiplexer()
    {
    }

    /// <summary>
    /// Constructor.
    /// Creats a new InputMultiplexer. The supplied <see cref="IInputProcessor"/>(s)
    /// will be added to the multiplexer.
    /// </summary>
    public InputMultiplexer( params IInputProcessor[] processors )
    {
        foreach ( IInputProcessor inputProcessor in processors )
        {
            Processors.Add( inputProcessor );
        }
    }

    /// <summary>
    /// Called when a key is pressed.
    /// </summary>
    /// <param name="keycode">One of the constants in <see cref="IInput.Keys"/></param>
    /// <returns>TRUE if the input was processed.</returns>
    public bool OnKeyDown( int keycode )
    {
        IInputProcessor[] items = Processors.Begin();

        try
        {
            for ( int i = 0, n = Processors.Size; i < n; i++ )
            {
                if ( items[ i ].OnKeyDown( keycode ) )
                {
                    return true;
                }
            }
        }
        finally
        {
            Processors.End();
        }

        return false;
    }

    /// <summary>
    /// Called when a key is released.
    /// </summary>
    /// <param name="keycode">One of the constants in <see cref="IInput.Keys"/></param>
    /// <returns>TRUE if the input was processed.</returns>
    public bool OnKeyUp( int keycode )
    {
        IInputProcessor[] items = Processors.Begin();

        try
        {
            for ( int i = 0, n = Processors.Size; i < n; i++ )
            {
                if ( items[ i ].OnKeyUp( keycode ) )
                {
                    return true;
                }
            }
        }
        finally
        {
            Processors.End();
        }

        return false;
    }

    /// <summary>
    /// Called when a key was typed
    /// </summary>
    /// <param name="character"></param>
    /// <returns>TRUE if the input was processed.</returns>
    public bool OnKeyTyped( char character )
    {
        IInputProcessor[] items = Processors.Begin();

        try
        {
            for ( int i = 0, n = Processors.Size; i < n; i++ )
            {
                if ( items[ i ].OnKeyTyped( character ) )
                {
                    return true;
                }
            }
        }
        finally
        {
            Processors.End();
        }

        return false;
    }

    /// <summary>
    /// Called when the screen was touched or a mouse button was pressed.
    /// </summary>
    /// <param name="screenX"> Screen touch X coordinate. </param>
    /// <param name="screenY"> Screen touch Y coordinate. </param>
    /// <param name="pointer"></param>
    /// <param name="button"></param>
    /// <returns>TRUE if the input was processed.</returns>
    public bool OnTouchDown( int screenX, int screenY, int pointer, int button )
    {
        IInputProcessor[] items = Processors.Begin();

        try
        {
            for ( int i = 0, n = Processors.Size; i < n; i++ )
            {
                if ( items[ i ].OnTouchDown( screenX, screenY, pointer, button ) )
                {
                    return true;
                }
            }
        }
        finally
        {
            Processors.End();
        }

        return false;
    }

    /// <summary>
    /// Called when a screen touch is lifted or mouse button is released.
    /// </summary>
    /// <param name="screenX"></param>
    /// <param name="screenY"></param>
    /// <param name="pointer"></param>
    /// <param name="button"></param>
    /// <returns>TRUE if the input was processed.</returns>
    public bool OnTouchUp( int screenX, int screenY, int pointer, int button )
    {
        IInputProcessor[] items = Processors.Begin();

        try
        {
            for ( int i = 0, n = Processors.Size; i < n; i++ )
            {
                if ( items[ i ].OnTouchUp( screenX, screenY, pointer, button ) )
                {
                    return true;
                }
            }
        }
        finally
        {
            Processors.End();
        }

        return false;
    }

    /// <summary>
    /// Called when a touch input is dragged across the screen.
    /// </summary>
    /// <param name="screenX">The current x-coordinate of the touch input on the screen.</param>
    /// <param name="screenY">The current y-coordinate of the touch input on the screen.</param>
    /// <param name="pointer">The index of the touch pointer associated with this event.</param>
    /// <returns>TRUE if the input was processed.</returns>
    public bool OnTouchDragged( int screenX, int screenY, int pointer )
    {
        IInputProcessor[] items = Processors.Begin();

        try
        {
            for ( int i = 0, n = Processors.Size; i < n; i++ )
            {
                if ( items[ i ].OnTouchDragged( screenX, screenY, pointer ) )
                {
                    return true;
                }
            }
        }
        finally
        {
            Processors.End();
        }

        return false;
    }

    /// <summary>
    /// Called when the mouse pointer is moved.
    /// </summary>
    /// <param name="screenX">The X-coordinate of the mouse pointer on the screen.</param>
    /// <param name="screenY">The Y-coordinate of the mouse pointer on the screen.</param>
    /// <returns>TRUE if the input was processed.</returns>
    public bool OnMouseMoved( int screenX, int screenY )
    {
        IInputProcessor[] items = Processors.Begin();

        try
        {
            for ( int i = 0, n = Processors.Size; i < n; i++ )
            {
                if ( items[ i ].OnMouseMoved( screenX, screenY ) )
                {
                    return true;
                }
            }
        }
        finally
        {
            Processors.End();
        }

        return false;
    }

    /// <summary>
    /// Called when the mouse wheel is moved.
    /// </summary>
    /// <param name="amountX"> The amount of horizontal moverment. </param>
    /// <param name="amountY"> The amount of vertical movement. </param>
    /// <returns>TRUE if the input was processed.</returns>
    public bool OnScrolled( float amountX, float amountY )
    {
        IInputProcessor[] items = Processors.Begin();

        try
        {
            for ( int i = 0, n = Processors.Size; i < n; i++ )
            {
                if ( items[ i ].OnScrolled( amountX, amountY ) )
                {
                    return true;
                }
            }
        }
        finally
        {
            Processors.End();
        }

        return false;
    }

    /// <summary>
    /// Inserts an <see cref="IInputProcessor"/> into the list of processors. This
    /// processor will be inserted at the position specified by <paramref name="index"/>
    /// </summary>
    public void AddProcessor( int index, IInputProcessor processor )
    {
        if ( processor == null )
        {
            throw new NullReferenceException( "processor cannot be null" );
        }

        Processors.Insert( index, processor );
    }

    /// <summary>
    /// Adds the specified <see cref="IInputProcessor"/> to the list of processors.
    /// </summary>
    public void AddProcessor( IInputProcessor processor )
    {
        if ( processor == null )
        {
            throw new NullReferenceException( "processor cannot be null" );
        }

        Processors.Add( processor );
    }

    /// <summary>
    /// Remove the <see cref="IInputProcessor"/> at the given index from
    /// the multiplexer.
    /// </summary>
    public void RemoveProcessor( int index )
    {
        Processors.RemoveAt( index );
    }

    /// <summary>
    /// Remove the first occurance of the specified <see cref="IInputProcessor"/>
    /// from the InputMultiplexer.
    /// </summary>
    public void RemoveProcessor( IInputProcessor processor )
    {
        Processors.Remove( processor );
    }

    /// <summary>
    /// Returns the number of <see cref="IInputProcessor"/>s in the list.
    /// </summary>
    /// <returns></returns>
    public int Size()
    {
        return Processors.Size;
    }

    /// <summary>
    /// Clears the list of Input Processors.
    /// </summary>
    public void Clear()
    {
        Processors.Clear();
    }

    /// <summary>
    /// Adds the given list of <see cref="IInputProcessor"/>s, which can be a
    /// single processor or multiple processors, to the multiplexer.
    /// </summary>
    public void SetProcessors( IInputProcessor[] processorList )
    {
        Processors.Clear();
        Processors.AddAll( processorList );
    }

    /// <summary>
    /// Adds the given list of <see cref="IInputProcessor"/>s, which can be a
    /// single processor or multiple processors, to the multiplexer.
    /// </summary>
    public void SetProcessors( List< IInputProcessor > processorList )
    {
        Processors.Clear();
        Processors.AddAll( processorList );
    }
}

// ============================================================================
// ============================================================================