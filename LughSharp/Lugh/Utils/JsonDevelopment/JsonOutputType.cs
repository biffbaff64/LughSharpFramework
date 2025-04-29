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

using System.Text;
using System.Text.RegularExpressions;

using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Utils.JsonDevelopment;

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
    /// <li>Names only require double quotes if they start with <code>space</code> or any of <code>":,}/</code> or
    /// they contain any of:- <code>// or /* or :</code>.
    /// </li>
    /// <li>
    /// Values only require double quotes if they start with <code>space</code> or any of <code>":,{[]/</code> or
    /// they contain <code>// or /* </code> or any of <code>}],</code> or they are equal to <code>true, false, or null </code>.
    /// </li>
    /// <li> Newlines are treated as commas, making commas optional in many cases. </li>
    /// <li> C style comments may be used: <code>//...</code> or <code>/*...*<b></b>/</code> </li>
    /// <br/>`
    /// </summary>
    Minimal,
}

[PublicAPI]
public static partial class OutputTypeExtensions
{
    private static readonly Regex _javascriptPattern   = MyRegex();
    private static readonly Regex _minimalValuePattern = MyRegex1();
    private static readonly Regex _minimalNamePattern  = MyRegex2();

    /// <summary>
    /// </summary>
    /// <param name="outputType"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string QuoteValue( this JsonOutputType outputType, object? value )
    {
        if ( value == null )
        {
            return "null";
        }

        var stringValue = value.ToString();

        Guard.ThrowIfNull( stringValue );

        if ( value is int or long or float or double or bool )
        {
            return stringValue;
        }

        var buffer = new StringBuilder( stringValue )
                     .Replace( "\\", @"\\" )
                     .Replace( "\r", "\\r" )
                     .Replace( "\n", "\\n" )
                     .Replace( "\t", "\\t" );

        if ( ( outputType == JsonOutputType.Minimal )
             && !stringValue!.Equals( "true", StringComparison.Ordinal )
             && !stringValue.Equals( "false", StringComparison.Ordinal )
             && !stringValue.Equals( "null", StringComparison.Ordinal )
             && !stringValue.Contains( "//", StringComparison.Ordinal )
             && !stringValue.Contains( "/*", StringComparison.Ordinal ) )
        {
            var length = buffer.Length;

            if ( ( length > 0 ) && ( buffer[ length - 1 ] != ' ' ) && _minimalValuePattern.IsMatch( buffer.ToString() ) )
            {
                return buffer.ToString();
            }
        }

        return "\"" + buffer.Replace( '"'.ToString(), "\\\"" ) + "\"";
    }

    /// <summary>
    /// </summary>
    /// <param name="outputType"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string QuoteName( this JsonOutputType outputType, string? value )
    {
        Guard.ThrowIfNull( value );

        var buffer = new StringBuilder( value );
        buffer.Replace( "\\", @"\\" ).Replace( "\r", "\\r" ).Replace( "\n", "\\n" ).Replace( "\t", "\\t" );

        switch ( outputType )
        {
            case JsonOutputType.Minimal:
                if ( !value.Contains( "//", StringComparison.Ordinal )
                     && !value.Contains( "/*", StringComparison.Ordinal )
                     && _minimalNamePattern.IsMatch( buffer.ToString() ) )
                {
                    return buffer.ToString();
                }

                goto case JsonOutputType.Javascript; // C# allows fall-through with goto

            case JsonOutputType.Javascript:
                if ( _javascriptPattern.IsMatch( buffer.ToString() ) )
                {
                    return buffer.ToString();
                }

                break;

            case JsonOutputType.Json:
            default:
                break;
        }

        return "\"" + buffer.Replace( '"'.ToString(), "\\\"" ) + "\"";
    }

    // ====================================================================

//    [GeneratedRegex( "^[a-zA-Z_$][a-zA-Z_$0-9]*$" )]
//    private static partial Regex MyRegex();

//    [GeneratedRegex( "^[^\":,{\\[\\]/ ][^}\\],]*$" )]
//    private static partial Regex MyRegex1();

//    [GeneratedRegex( "^[^\":,}/ ][^:]*$" )]
//    private static partial Regex MyRegex2();
}