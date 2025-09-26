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

namespace LughSharp.Lugh.Input;

[PublicAPI]
public interface IInputProcessor
{
    /// <summary>
    /// Called when a key is pressed.
    /// </summary>
    /// <param name="keycode">One of the constants in <see cref="IInput.Keys"/></param>
    /// <returns>TRUE if the input was processed.</returns>
    bool KeyDown( int keycode );

    /// <summary>
    /// Called when a key is released.
    /// </summary>
    /// <param name="keycode">One of the constants in <see cref="IInput.Keys"/></param>
    /// <returns>TRUE if the input was processed.</returns>
    bool KeyUp( int keycode );

    /// <summary>
    /// Called when a key was typed
    /// </summary>
    /// <param name="character"></param>
    /// <returns>TRUE if the input was processed.</returns>
    bool KeyTyped( char character );

    /// <summary>
    /// Called when the screen was touched or a mouse button was pressed.
    /// </summary>
    /// <param name="screenX"> Screen touch X coordinate. </param>
    /// <param name="screenY"> Screen touch Y coordinate. </param>
    /// <param name="pointer"></param>
    /// <param name="button"></param>
    /// <returns>TRUE if the input was processed.</returns>
    bool TouchDown( int screenX, int screenY, int pointer, int button );

    /// <summary>
    /// Called when a screen touch is lifted or mouse button is released.
    /// </summary>
    /// <param name="screenX"></param>
    /// <param name="screenY"></param>
    /// <param name="pointer"></param>
    /// <param name="button"></param>
    /// <returns>TRUE if the input was processed.</returns>
    bool TouchUp( int screenX, int screenY, int pointer, int button );

    /// <summary>
    /// Called when a touch input is dragged across the screen.
    /// </summary>
    /// <param name="screenX">The current x-coordinate of the touch input on the screen.</param>
    /// <param name="screenY">The current y-coordinate of the touch input on the screen.</param>
    /// <param name="pointer">The index of the touch pointer associated with this event.</param>
    /// <returns>TRUE if the input was processed.</returns>
    bool TouchDragged( int screenX, int screenY, int pointer );

    /// <summary>
    /// Called when the mouse pointer is moved.
    /// </summary>
    /// <param name="screenX">The X-coordinate of the mouse pointer on the screen.</param>
    /// <param name="screenY">The Y-coordinate of the mouse pointer on the screen.</param>
    /// <returns>TRUE if the input was processed.</returns>
    bool MouseMoved( int screenX, int screenY );

    /// <summary>
    /// Called when the mouse wheel is moved.
    /// </summary>
    /// <param name="amountX"> The amount of horizontal moverment. </param>
    /// <param name="amountY"> The amount of vertical movement. </param>
    /// <returns>TRUE if the input was processed.</returns>
    bool Scrolled( float amountX, float amountY );
}