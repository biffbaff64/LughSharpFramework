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

global using System;
global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.Drawing;
global using System.IO;
global using System.IO.Compression;
global using System.Linq;
global using System.Numerics;
global using System.Runtime.CompilerServices;
global using System.Runtime.InteropServices;
global using System.Runtime.Serialization;
global using System.Text;
global using System.Threading;
global using System.Threading.Tasks;

// ============================================================================

global using JetBrains.Annotations;

// ============================================================================
global using LughSharp.Core.Graphics;

// ============================================================================

global using static LughSharp.Core.Main.Engine;

// ============================================================================

global using Vector2 = LughUtils.source.Maths.Vector2;
global using Vector3 = LughUtils.source.Maths.Vector3;
global using Vector4 = LughUtils.source.Maths.Vector4;

// ============================================================================

global using LughUtils.source;
global using LughUtils.source.Collections;
global using LughUtils.source.Exceptions;
global using LughUtils.source.Logging;
global using LughUtils.source.Maths;
global using LughUtils.source.Maths.Collision;

global using SerializationException = LughSharp.Core.Utils.SerializationException;

// ============================================================================