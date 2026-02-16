// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Red 7 Projects
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// /////////////////////////////////////////////////////////////////////////////

using JetBrains.Annotations;

namespace LughSharp.Core.Graphics.OpenGL.Enums;

/// <summary>
/// Status of a joystick hat.
/// </summary>
[PublicAPI]
public enum JoystickHat : byte
{
    /// <summary>
    /// Hat is centered.
    /// </summary>
    Centered = 0,

    /// <summary>
    /// Hat is pointing up.
    /// </summary>
    Up = 1,

    /// <summary>
    /// Hat is pointing right.
    /// </summary>
    Right = 2,

    /// <summary>
    /// Hat is pointing down.
    /// </summary>
    Down = 4,

    /// <summary>
    /// Hat is pointing left.
    /// </summary>
    Left = 8,

    /// <summary>
    /// Hat is pointing up and to the right.
    /// </summary>
    RightUp = Right | Up,

    /// <summary>
    /// Hat is pointing down and to the right.
    /// </summary>
    RightDown = Right | Down,

    /// <summary>
    /// Hat is pointing up and to the left.
    /// </summary>
    LeftUp = Left | Up,

    /// <summary>
    /// Hat is pointing down and to the left.
    /// </summary>
    LeftDown = Left | Down,
}

// ========================================================================
// ========================================================================