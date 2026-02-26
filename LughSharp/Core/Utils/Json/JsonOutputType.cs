using JetBrains.Annotations;

namespace LughSharp.Core.Utils.Json;

/// <summary>
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
    /// </summary>
    Minimal,
}

// ============================================================================
// ============================================================================