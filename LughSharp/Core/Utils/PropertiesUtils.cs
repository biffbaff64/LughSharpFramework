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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using JetBrains.Annotations;

using LughSharp.Core.Utils.Logging;

namespace LughSharp.Core.Utils;

/// <summary>
/// A helper class that allows you to load and store key/value pairs of an
/// <see cref="Dictionary{TKey,TValue}" /> with the same line-oriented syntax supported
/// by <see cref="IPreferences" />
/// </summary>
[PublicAPI]
public static class PropertiesUtils
{
    private const int    None           = 0;
    private const int    Slash          = 1;
    private const int    Unicode        = 2;
    private const int    Continue       = 3;
    private const int    KeyDone       = 4;
    private const int    Ignore         = 5;
    private const string LineSeparator = "\n";

    // ========================================================================

    /// <summary>
    /// Loads properties from the specified <see cref="StreamReader" /> into the provided dictionary.
    /// </summary>
    /// <param name="properties">The dictionary to load properties into.</param>
    /// <param name="reader">The reader to read the properties from.</param>
    /// <exception cref="ArgumentNullException">Thrown if properties or reader is null.</exception>
    /// <exception cref="ArgumentException">Thrown if an invalid Unicode sequence is encountered.</exception>
    public static void Load( Dictionary< string, string > properties, StreamReader reader )
    {
        ArgumentNullException.ThrowIfNull( properties );
        ArgumentNullException.ThrowIfNull( reader );

        int mode      = None;
        var unicode   = 0;
        var count     = 0;
        var buf       = new char[ 40 ];
        var offset    = 0;
        int keyLength = -1;
        var firstChar = true;

        while ( true )
        {
            int intVal = reader.Read();

            if ( intVal == -1 )
            {
                break;
            }

            var nextChar = ( char )intVal;

            if ( offset == buf.Length )
            {
                var newBuf = new char[ buf.Length * 2 ];
                Array.Copy( buf, 0, newBuf, 0, offset );
                buf = newBuf;
            }

            switch ( mode )
            {
                case Unicode:
                    double num   = char.GetNumericValue( nextChar.ToString(), 16 );
                    var    digit = ( int )num;

                    if ( digit >= 0 )
                    {
                        unicode = ( unicode << 4 ) + digit;

                        if ( ++count < 4 )
                        {
                            continue;
                        }
                    }
                    else if ( count <= 4 )
                    {
                        throw new ArgumentException( "Invalid Unicode sequence: illegal character" );
                    }

                    mode            = None;
                    buf[ offset++ ] = ( char )unicode;

                    if ( nextChar != '\n' )
                    {
                        continue;
                    }

                    break;
            }

            switch ( mode )
            {
                case Slash:
                    mode = None;

                    switch ( nextChar )
                    {
                        case '\r':
                            mode = Continue;

                            continue;

                        case '\n':
                            mode = Ignore;

                            continue;

                        case 'b':
                            nextChar = '\b';

                            break;

                        case 'f':
                            nextChar = '\f';

                            break;

                        case 'n':
                            nextChar = '\n';

                            break;

                        case 'r':
                            nextChar = '\r';

                            break;

                        case 't':
                            nextChar = '\t';

                            break;

                        case 'u':
                            mode    = Unicode;
                            unicode = count = 0;

                            continue;
                    }

                    break;

                default:
                    switch ( nextChar )
                    {
                        case '#':
                        case '!':
                            if ( firstChar )
                            {
                                while ( true )
                                {
                                    intVal = reader.Read();

                                    if ( intVal == -1 )
                                    {
                                        break;
                                    }

                                    nextChar = ( char )intVal;

                                    if ( nextChar is '\r' or '\n' )
                                    {
                                        break;
                                    }
                                }

                                continue;
                            }

                            break;

                        case '\n':
                        case '\r':
                            if ( ( nextChar == '\n' ) && ( mode == Continue ) )
                            {
                                mode = Ignore;

                                continue;
                            }

                            mode      = None;
                            firstChar = true;

                            if ( ( offset > 0 ) || ( keyLength == 0 ) )
                            {
                                keyLength = keyLength == -1 ? offset : keyLength;
                                var temp = new string( buf, 0, offset );
                                properties[ temp.Substring( 0, keyLength ) ] = temp.Substring( keyLength );
                            }

                            keyLength = -1;
                            offset    = 0;

                            continue;

                        case '\\':
                            keyLength = mode == KeyDone ? offset : keyLength;
                            mode      = Slash;

                            continue;

                        case ':':
                        case '=':
                            if ( keyLength == -1 )
                            {
                                mode      = None;
                                keyLength = offset;

                                continue;
                            }

                            break;
                    }

                    if ( char.IsWhiteSpace( nextChar ) )
                    {
                        mode = mode == Continue ? Ignore : mode;

                        if ( ( offset == 0 ) || ( offset == keyLength ) || ( mode == Ignore ) )
                        {
                            continue;
                        }

                        if ( keyLength == -1 )
                        {
                            mode = KeyDone;

                            continue;
                        }
                    }

                    mode = mode is Ignore or Continue ? None : mode;

                    break;
            }

            firstChar = false;

            if ( mode == KeyDone )
            {
                keyLength = offset;
                mode      = None;
            }

            buf[ offset++ ] = nextChar;
        }

        if ( ( mode == Unicode ) && ( count <= 4 ) )
        {
            throw new ArgumentException( "Invalid Unicode sequence: expected format \\uxxxx" );
        }

        keyLength = ( keyLength == -1 ) && ( offset > 0 ) ? offset : keyLength;

        if ( keyLength >= 0 )
        {
            var    temp  = new string( buf, 0, offset );
            string key   = temp.Substring( 0, keyLength );
            string value = temp.Substring( keyLength );

            if ( mode == Slash )
            {
                value += "\0";
            }

            properties[ key ] = value;
        }
    }

    /// <summary>
    /// Stores the properties from the dictionary to the specified <see cref="StreamWriter" />.
    /// </summary>
    /// <param name="properties">The dictionary containing properties to store.</param>
    /// <param name="writer">The writer to write the properties to.</param>
    /// <param name="comment">An optional comment to include at the top of the output.</param>
    /// <param name="escapeUnicode">Whether to escape non-ASCII Unicode characters.</param>
    public static void Store( Dictionary< string, string > properties, StreamWriter writer, string? comment,
                              bool escapeUnicode = false )
    {
        if ( comment != null )
        {
            WriteComment( writer, comment );
        }

        writer.Write( "#" );
        writer.Write( DateTime.Now.ToString( "ddd MM/dd/yyyy h:mm tt" ) );
        writer.Write( LineSeparator );

        var sb = new StringBuilder( 200 );

        foreach ( KeyValuePair< string, string > entry in properties )
        {
            DumpString( sb, entry.Key, true, escapeUnicode );
            sb.Append( '=' );
            DumpString( sb, entry.Value, false, escapeUnicode );
            writer.Write( LineSeparator );
            writer.Write( sb.ToString() );
            sb.Clear();
        }

        writer.Flush();
    }

    /// <summary>
    /// Converts a string to a form suitable for writing to a properties file, escaping necessary characters.
    /// </summary>
    /// <param name="outBuffer">The buffer to write the escaped string to.</param>
    /// <param name="str">The string to escape.</param>
    /// <param name="escapeSpace">Whether to escape spaces.</param>
    /// <param name="escapeUnicode">Whether to escape non-ASCII Unicode characters.</param>
    private static void DumpString( StringBuilder outBuffer, string str, bool escapeSpace, bool escapeUnicode )
    {
        int len = str.Length;

        for ( var i = 0; i < len; i++ )
        {
            char ch = str[ i ];

            if ( ( ch > 61 ) && ( ch < 127 ) )
            {
                outBuffer.Append( ch == '\\' ? @"\\" : ch );

                continue;
            }

            switch ( ch )
            {
                case ' ':
                    if ( ( i == 0 ) || escapeSpace )
                    {
                        outBuffer.Append( "\\ " );
                    }
                    else
                    {
                        outBuffer.Append( ch );
                    }

                    break;

                case '\n':
                    outBuffer.Append( "\\n" );

                    break;

                case '\r':
                    outBuffer.Append( "\\r" );

                    break;

                case '\t':
                    outBuffer.Append( "\\t" );

                    break;

                case '\f':
                    outBuffer.Append( "\\f" );

                    break;

                case '=':
                case ':':
                case '#':
                case '!':
                    outBuffer.Append( '\\' ).Append( ch );

                    break;

                default:
                    if ( escapeUnicode && ( ( ch < 0x0020 ) || ( ch > 0x007e ) ) )
                    {
                        var hex = ( ( int )ch ).ToString( "X" );
                        outBuffer.Append( "\\u" );

                        for ( var j = 0; j < ( 4 - hex.Length ); j++ )
                        {
                            outBuffer.Append( '0' );
                        }

                        outBuffer.Append( hex );
                    }
                    else
                    {
                        outBuffer.Append( ch );
                    }

                    break;
            }
        }
    }

    /// <summary>
    /// Writes a comment to the specified <see cref="StreamWriter" />.
    /// </summary>
    /// <param name="writer">The writer to write the comment to.</param>
    /// <param name="comment">The comment to write.</param>
    private static void WriteComment( StreamWriter writer, string comment )
    {
        writer.Write( "#" );

        int len       = comment.Length;
        var curIndex  = 0;
        var lastIndex = 0;

        while ( curIndex < len )
        {
            char c = comment[ curIndex ];

            if ( c is > '\u00ff' or '\n' or '\r' )
            {
                if ( lastIndex != curIndex )
                {
                    writer.Write( comment.Substring( lastIndex, curIndex ) );
                }

                if ( c > '\u00ff' )
                {
                    var hex = ( ( int )c ).ToString( "X" );
                    writer.Write( "\\u" );

                    for ( var j = 0; j < ( 4 - hex.Length ); j++ )
                    {
                        writer.Write( '0' );
                    }

                    writer.Write( hex );
                }
                else
                {
                    writer.Write( LineSeparator );

                    if ( ( c == '\r' ) && ( curIndex != ( len - 1 ) ) && ( comment[ curIndex + 1 ] == '\n' ) )
                    {
                        curIndex++;
                    }

                    if ( ( curIndex == ( len - 1 ) )
                      || ( ( comment[ curIndex + 1 ] != '#' ) && ( comment[ curIndex + 1 ] != '!' ) ) )
                    {
                        writer.Write( "#" );
                    }
                }

                lastIndex = curIndex + 1;
            }

            curIndex++;
        }

        if ( lastIndex != curIndex )
        {
            writer.Write( comment.Substring( lastIndex, curIndex ) );
        }

        writer.Write( LineSeparator );
    }
}