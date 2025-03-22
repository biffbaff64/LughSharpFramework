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
using System.IO;
using System.Text;

namespace LughSharp.Lugh.Utils.Json;

public partial class Json
{
    public string ToJson( object? obj )
    {
        return ToJson( obj, obj?.GetType(), null );
    }

    public string ToJson( object? obj, Type knownType )
    {
        return ToJson( obj, knownType, null );
    }

    public string ToJson( object? obj, Type knownType, Type elementType )
    {
        using ( var buffer = new StringWriter() )
        {
            ToJson( obj, knownType, elementType, buffer );

            return buffer.ToString();
        }
    }

    public void ToJson( object? obj, FileInfo file )
    {
        ToJson( obj, obj?.GetType(), null, file );
    }

    public void ToJson( object? obj, Type knownType, FileInfo file )
    {
        ToJson( obj, knownType, null, file );
    }

    public void ToJson( object? obj, Type knownType, Type elementType, FileInfo file )
    {
        try
        {
            using ( var writer = new StreamWriter( file.FullName, false, Encoding.UTF8 ) )
            {
                ToJson( obj, knownType, elementType, writer );
            }
        }
        catch ( Exception ex )
        {
            throw new Exception( $"Error writing file: {file.FullName}", ex );
        }
    }

    public void ToJson( object? obj, TextWriter writer )
    {
        ToJson( obj, obj?.GetType(), null, writer );
    }

    public void ToJson( object? obj, Type knownType, TextWriter writer )
    {
        ToJson( obj, knownType, null, writer );
    }

    public void ToJson( object? obj, Type knownType, Type elementType, TextWriter writer )
    {
        SetWriter( writer );

        try
        {
            WriteValue( obj, knownType, elementType );
        }
        finally
        {
            CloseQuietly( this.writer );
            this.writer = null;
        }
    }
}