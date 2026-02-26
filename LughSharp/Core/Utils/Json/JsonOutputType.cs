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

using System.Text;

using JetBrains.Annotations;

using LughSharp.Core.Graphics.Text;
using LughSharp.Core.Maths;

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

[PublicAPI]
public static class JsonOutput
{
    public static string? QuoteValue( object? value, JsonOutputType outputType )
    {
        if ( value == null )
        {
            return "null";
        }

        string? stringValue = value.ToString();

        if ( NumberUtils.IsNumeric( value ) || value is bool )
        {
            return stringValue;
        }

        bool quote = false;

        for ( int i = 0; i < stringValue?.Length; i++ )
        {
            switch ( stringValue[ i ] )
            {
                case '\\':
                case '\r':
                case '\n':
                case '\t':
                    stringValue = Escape( stringValue, i );
                    quote       = true;
                    i           = stringValue.Length;

                    break;

                case '"':
                    quote = true;

                    break;
            }
        }

        if ( stringValue == null )
        {
            return "null";
        }

        if ( outputType == JsonOutputType.Minimal
          && !stringValue.Equals( "true" )
          && !stringValue.Equals( "false" )
          && !stringValue.Equals( "null" )
          && !stringValue.Contains( "//" )
          && !stringValue.Contains( "/*" ) )
        {
            int length = stringValue.Length;

            if ( length > 0
              && stringValue[ length - 1 ] != ' '
              && RegexUtils.MinimalValuePatternRegex().Matches( stringValue ).Count > 0 )
            {
                return stringValue;
            }
        }

        return quote ? EscapeQuote( stringValue ) : '"' + stringValue + '"';
    }

    public static string QuoteName( string? value, JsonOutputType outputType )
    {
        bool quote = false;

        for ( int i = 0; i < value.Length; i++ )
        {
            switch ( value[ i ] )
            {
                case '\\':
                case '\r':
                case '\n':
                case '\t':
                    value = Escape( value, i );
                    quote = true;
                    i     = value.Length;

                    break;

                case '"':
                    quote = true;

                    break;
            }
        }

        switch ( outputType )
        {
            case JsonOutputType.Minimal:
                if ( !value.Contains( "//" )
                  && !value.Contains( "/*" )
                  && RegexUtils.MinimalNamePatternRegex().Matches( value ).Count > 0 )
                {
                    return value;
                }

                break;

            case JsonOutputType.Javascript:
                if ( RegexUtils.JavascriptPatternRegex().Matches( value ).Count > 0 )
                {
                    return value;
                }

                break;
        }

        return quote ? EscapeQuote( value ) : '"' + value + '"';
    }

    private static string Escape( string value, int i )
    {
        StringBuilder buffer = new StringBuilder( value.Length + 6 );

        buffer.Append( value, 0, i );

        for ( ; i < value.Length; i++ )
        {
            char c = value[ i ];

            switch ( c )
            {
                case '\\':
                    buffer.Append( "\\\\" );

                    break;

                case '\r':
                    buffer.Append( "\\r" );

                    break;

                case '\n':
                    buffer.Append( "\\n" );

                    break;

                case '\t':
                    buffer.Append( "\\t" );

                    break;

                default:
                    buffer.Append( c );

                    break;
            }
        }

        return buffer.ToString();
    }

    private static string EscapeQuote( string value )
    {
        StringBuilder buffer = new StringBuilder( value.Length + 6 );

        buffer.Append( '"' );

        foreach ( var c in value )
        {
            if ( c == '"' )
            {
                buffer.Append( "\\\"" );
            }
            else
            {
                buffer.Append( c );
            }
        }

        buffer.Append( '"' );

        return buffer.ToString();
    }
}

// ============================================================================
// ============================================================================