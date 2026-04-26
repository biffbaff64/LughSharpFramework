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

using RegexUtils = LughSharp.Source.Graphics.Utils.RegexUtils;

namespace LughSharp.Source.Utils.Json;

[PublicAPI]
public static class JsonOutput
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="outputType"></param>
    /// <returns></returns>
    public static string? QuoteValue( object? value, JsonOutputType outputType )
    {
        if ( value == null )
        {
            return "null";
        }

        var stringValue = value.ToString();

        if ( NumberUtils.IsNumeric( value ) || value is bool )
        {
            return stringValue;
        }

        var quote = false;

        for ( var i = 0; i < stringValue?.Length; i++ )
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="outputType"></param>
    /// <returns></returns>
    public static string QuoteName( string name, JsonOutputType outputType )
    {
        var quote = false;

        for ( var i = 0; i < name.Length; i++ )
        {
            switch ( name[ i ] )
            {
                case '\\':
                case '\r':
                case '\n':
                case '\t':
                    name = Escape( name, i );
                    quote = true;
                    i     = name.Length;

                    break;

                case '"':
                    quote = true;

                    break;
            }
        }

        switch ( outputType )
        {
            case JsonOutputType.Minimal:
                if ( !name.Contains( "//" )
                  && !name.Contains( "/*" )
                  && RegexUtils.MinimalNamePatternRegex().Matches( name ).Count > 0 )
                {
                    return name;
                }

                break;

            case JsonOutputType.Javascript:
                if ( RegexUtils.JavascriptPatternRegex().Matches( name ).Count > 0 )
                {
                    return name;
                }

                break;
            
            case JsonOutputType.Json:
            default:
                break;
        }

        return quote ? EscapeQuote( name ) : $"\"{name}\"";
    }

    /// <summary>
    /// Escapes the specified string value by replacing special characters with
    /// their escape sequences.
    /// </summary>
    /// <param name="value">The string to be escaped.</param>
    /// <param name="i">The index from which escaping begins in the string.</param>
    /// <returns>
    /// A new string with special characters replaced by their escape sequences.
    /// </returns>
    private static string Escape( string value, int i )
    {
        var buffer = new StringBuilder( value.Length + 6 );

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

    /// <summary>
    /// Escapes the specified value with double quotes.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static string EscapeQuote( string value )
    {
        var buffer = new StringBuilder( value.Length + 6 );

        buffer.Append( '"' );

        foreach ( char c in value )
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