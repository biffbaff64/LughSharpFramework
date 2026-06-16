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

namespace LughSharp.Source.Utils;

/// <summary>
/// Marks a type as part of an unstable API that is still in active development.
/// Applying this attribute alongside <see cref="System.Diagnostics.CodeAnalysis.ExperimentalAttribute"/>
/// provides both human-readable context and IDE/compiler diagnostics at usage sites.
/// <br/><br/>
/// Recommended usage:
/// <code>
/// [UnstableApi("Style system is being redesigned")]
/// [Experimental("LUGH_UI_001")]
/// public class StyleRegistry { ... }
/// </code>
/// Callers must suppress the compiler warning to use the type:
/// <code>
/// #pragma warning disable LUGH_UI_001
///     var registry = new StyleRegistry();
/// #pragma warning restore LUGH_UI_001
/// </code>
/// </summary>
[PublicAPI]
[AttributeUsage( AttributeTargets.Class
               | AttributeTargets.Enum
               | AttributeTargets.Interface
               | AttributeTargets.Struct )]
public class UnstableApiAttribute : Attribute
{
    /// <param name="reason">A brief description of why this type is considered unstable.</param>
    public UnstableApiAttribute( string? reason = null )
    {
        Reason = reason;
    }

    /// <summary>
    /// A brief description of why this type is considered unstable or incomplete.
    /// </summary>
    public string? Reason { get; }
}

// ============================================================================
// ============================================================================