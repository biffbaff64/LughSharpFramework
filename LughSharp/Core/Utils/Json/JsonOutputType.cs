// ///////////////////////////////////////////////////////////////////////////////
// MIT License
// 
// Copyright (c) 2024, 2025, 2026 Circa64 Software Projects / Richard Ikin.
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

namespace LughSharp.Core.Utils.Json;

/// <summary>
/// Specifies the output format for JSON serialization.
/// </summary>
[PublicAPI]
public enum JsonOutputType
{
    /// <summary>
    /// Normal JSON, with all its double quotes.
    /// </summary>
    Json,

    /// <summary>
    /// Like JSON, but names are only double quoted if necessary.
    /// </summary>
    Javascript,

    /// <summary>
    /// Like JSON, but:
    /// <li>
    /// Names only require double quotes if they start with <c>space</c> or any of
    /// <c>":,}/</c> or they contain <c>//</c> or <c>/*</c> or <c>:</c>.
    /// </li>
    /// <li>
    /// Values only require double quotes if they start with <c>space</c> or any of
    /// <c>":,{[]/</c> or they contain <c>//</c> or <c>/*</c> or any of <c>}],</c>
    /// or they are equal to <c>true</c>, <c>false</c>, or <c>null</c>.
    /// </li>
    /// <li>
    /// Newlines are treated as commas, making commas optional in many cases.
    /// </li>
    /// <li>
    /// C style comments may be used: <c>//...</c> or <c>/*...*<b></b>/</c>
    /// </li>
    /// <br/>
    /// </summary>
    Minimal
}

// ============================================================================
// ============================================================================

