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
using System.Collections.Generic;
using System.IO;
using System.Text;

using LughSharp.Lugh.Files;
using LughUtils.source.Exceptions;

namespace Extensions.Source.Json;

public partial class JsonReader
{
    private char[]               _parseData     = null!;
    private int[]                _parseStack    = new int[ 4 ];
    private int                  _stackPointer  = 0;
    private List< string >       _parseNameList = new( 8 );
    private int                  _parsePosition;
    private int                  _parseLength;
    private GdxRuntimeException? _parseException;
    private int                  _stringValueStartPos; // Start position for string values

    // ========================================================================

    /// <summary>
    /// Parses a JSON string and converts it to a <see cref="JsonValue"/> object.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <returns>A <see cref="JsonValue"/>
    /// object representing the JSON data, or null if parsing fails.
    /// </returns>
    public JsonValue? Parse( string json )
    {
        var data = json.ToCharArray();

        return Parse( data, 0, data.Length );
    }

    /// <summary>
    /// Parses the JSON data from the provided <see cref="TextReader"/> and returns
    /// a <see cref="JsonValue"/> representation of the data.
    /// </summary>
    /// <param name="reader">The <see cref="TextReader"/> to read the JSON data from.</param>
    /// <returns>A <see cref="JsonValue"/>
    /// representing the parsed JSON data, or null if parsing fails.
    /// </returns>
    /// <exception cref="SerializationException">
    /// Thrown when there is an error reading the input data.
    /// </exception>
    public JsonValue? Parse( TextReader reader )
    {
        var data   = new char[ 1024 ];
        var offset = 0;

        try
        {
            while ( true )
            {
                var length = reader.Read( data, offset, data.Length - offset );

                if ( length == -1 )
                {
                    break;
                }

                if ( length == 0 )
                {
                    var newData = new char[ data.Length * 2 ];

                    Array.Copy( data, 0, newData, 0, data.Length );
                    data = newData;
                }
                else
                {
                    offset += length;
                }
            }
        }
        catch ( IOException ex )
        {
            throw new SerializationException( "Error reading input.", ex );
        }
        finally
        {
            reader.Close();
        }

        return Parse( data, 0, offset );
    }

    /// <summary>
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="SerializationException"></exception>
    public JsonValue? Parse( InputStream input )
    {
        StreamReader reader;

        try
        {
            reader = new StreamReader( input, Encoding.UTF8 );
        }
        catch ( Exception ex )
        {
            throw new SerializationException( "Error reading stream.", ex );
        }

        return Parse( reader );
    }

    public JsonValue? Parse( FileInfo file )
    {
        StreamReader reader;

        try
        {
            reader = new StreamReader( file.Name, Encoding.UTF8 );
        }
        catch ( Exception ex )
        {
            throw new SerializationException( "Error reading file: " + file, ex );
        }

        try
        {
            return Parse( reader );
        }
        catch ( Exception ex )
        {
            throw new SerializationException( "Error parsing file: " + file, ex );
        }
    }
}