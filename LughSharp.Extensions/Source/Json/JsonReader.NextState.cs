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

using System;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace Extensions.Source.Json;

[PublicAPI]
public enum JsonParseState : int
{
    Start,

    // ----------
    String,
    ObjectStart,
    ObjectEnd,
    ArrayStart,
    ArrayEnd,
    SkipComment,
    UnquotedChars,
    QuotedChars,

    // ----------
    ObjectKeyStart,
    ObjectKey,
    Colon,
    Number,
    ValueStart,
    ArrayElement,
    Eof,

    // ----------
    End,
}

// ============================================================================

public partial class JsonReader
{
    [SuppressMessage( "ReSharper", "EnforceIfStatementBraces" )]
    [SuppressMessage( "ReSharper", "ConvertIfStatementToSwitchStatement" )]
    private JsonParseState GetNextState( JsonParseState currentState, char inputChar )
    {
        switch ( currentState )
        {
            case JsonParseState.Start:
                if ( inputChar == '{' ) return JsonParseState.ObjectStart;
                if ( inputChar == '[' ) return JsonParseState.ArrayStart;
                if ( inputChar == '"' ) return JsonParseState.String;

                break;

            case JsonParseState.String:
                break;

            case JsonParseState.ObjectStart:
                break;

            case JsonParseState.ObjectEnd:
                break;

            case JsonParseState.ArrayStart:
                break;

            case JsonParseState.ArrayEnd:
                break;

            case JsonParseState.SkipComment:
                break;

            case JsonParseState.UnquotedChars:
                break;

            case JsonParseState.QuotedChars:
                break;

            case JsonParseState.ObjectKeyStart:
                break;

            case JsonParseState.ObjectKey:
                break;

            case JsonParseState.Colon:
                break;

            case JsonParseState.Number:
                break;

            case JsonParseState.ValueStart:
                break;

            case JsonParseState.ArrayElement:
                break;

            case JsonParseState.Eof:
                break;

            case JsonParseState.End:
                break;

            default:
                throw new ArgumentOutOfRangeException( nameof( currentState ), currentState, null );
        }

        return currentState; // Or throw an error for unexpected input
    }
}