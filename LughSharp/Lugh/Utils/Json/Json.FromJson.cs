﻿// /////////////////////////////////////////////////////////////////////////////
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
using System.Runtime.Serialization;

using LughSharp.Lugh.Files;

namespace LughSharp.Lugh.Utils.Json;

public partial class Json
{
    public T? FromJson< T >( Type type, TextReader reader )
    {
        return ReadValue< T >( type, null, new JsonReader().Parse( reader ) );
    }

    public T? FromJson< T >( Type type, Type elementType, TextReader reader )
    {
        return ReadValue< T >( type, elementType, new JsonReader().Parse( reader ) );
    }

    public T? FromJson< T >( Type type, InputStream input )
    {
        return ReadValue< T >( type, null, new JsonReader().Parse( input ) );
    }

    public T? FromJson< T >( Type type, Type elementType, InputStream input )
    {
        return ReadValue< T >( type, elementType, new JsonReader().Parse( input ) );
    }

    public T? FromJson< T >( Type type, FileInfo file )
    {
        try
        {
            return ReadValue< T >( type, null, new JsonReader().Parse( file ) );
        }
        catch ( Exception ex )
        {
            throw new SerializationException( "Error reading file: " + file, ex );
        }
    }

    public T? FromJson< T >( Type type, Type elementType, FileInfo file )
    {
        try
        {
            return ReadValue< T >( type, elementType, new JsonReader().Parse( file ) );
        }
        catch ( Exception ex )
        {
            throw new SerializationException( "Error reading file: " + file, ex );
        }
    }

    public T? FromJson< T >( Type type, char[] data, int offset, int length )
    {
        return ReadValue< T >( type, null, new JsonReader().Parse( data, offset, length ) );
    }

    public T? FromJson< T >( Type type, Type elementType, char[] data, int offset, int length )
    {
        return ReadValue< T >( type, elementType, new JsonReader().Parse( data, offset, length ) );
    }

    public T? FromJson< T >( Type type, string json )
    {
        return ReadValue< T >( type, null, new JsonReader().Parse( json ) );
    }

    public T? FromJson< T >( Type type, Type elementType, string json )
    {
        return ReadValue< T >( type, elementType, new JsonReader().Parse( json ) );
    }
}