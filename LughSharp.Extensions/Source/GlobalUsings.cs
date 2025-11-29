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

// ====================================================================--------

global using System.Drawing;
global using System.Drawing.Drawing2D;
global using System.Drawing.Imaging;
global using System.Collections.Generic;

global using PixelFormat = System.Drawing.Imaging.PixelFormat;

// ============================================================================

global using System.Text.Json;

// ============================================================================

global using System.Runtime.Versioning;

// ============================================================================

global using JetBrains.Annotations;

// ============================================================================

// ============================================================================

global using DotGLFW;

global using GLFW = DotGLFW;

// ============================================================================

global using LughSharp.Lugh;
global using LughSharp.Lugh.Core;
global using LughSharp.Lugh.Graphics.OpenGL.Enums;

// ============================================================================

global using LughUtils.source.Logging;

// ============================================================================

global using static LughSharp.Lugh.Core.Engine;

// ============================================================================

global using Vector2 = LughUtils.source.Maths.Vector2;
global using Vector3 = LughUtils.source.Maths.Vector3;

// ============================================================================

global using SerializationException = LughSharp.Lugh.Utils.SerializationException;

// ============================================================================