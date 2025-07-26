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

global using System.Drawing;
global using System.Text.Json;
global using System.Numerics;
global using System.Runtime.CompilerServices;
global using System.Runtime.InteropServices;
global using System.Runtime.Serialization;
global using System.Threading;
global using System.Threading.Tasks;

// ============================================================================

global using JetBrains.Annotations;

// ============================================================================

global using DotGLFW;
global using GLFW = DotGLFW;

// ============================================================================

global using LughSharp.Lugh.Core;
global using LughSharp.Lugh.Graphics.OpenGL.Enums;
global using LughSharp.Lugh.Maths;

// ============================================================================

global using static LughSharp.Lugh.Core.Engine;

// ============================================================================

global using Vector2 = LughSharp.Lugh.Maths.Vector2;
global using Vector3 = LughSharp.Lugh.Maths.Vector3;
global using Vector4 = LughSharp.Lugh.Maths.Vector4;

// ============================================================================

global using SerializationException = LughSharp.Lugh.Utils.Exceptions.SerializationException;

// ============================================================================