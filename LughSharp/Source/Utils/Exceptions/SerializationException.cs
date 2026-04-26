// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Circa64 Software Projects
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

using JetBrains.Annotations;

namespace LughSharp.Source.Utils.Exceptions;

/// <summary>
/// Represents an exception that occurs during the serialization or deserialization process.
/// </summary>
/// <remarks>
/// This exception provides additional methods for adding and retrieving trace information
/// during the serialization or deserialization of objects. It extends the standard
/// <see cref="System.Runtime.Serialization.SerializationException"/> class.
/// </remarks>
[PublicAPI]
[Serializable]
public class SerializationException : System.Runtime.Serialization.SerializationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SerializationException"/> class.
    /// </summary>
    public SerializationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SerializationException"/> class with
    /// a specified error message.
    /// </summary>
    /// <param name="message"></param>
    public SerializationException( string message )
        : base( message )
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SerializationException"/> class with
    /// a specified error message and a reference to the inner exception that is the cause
    /// of this exception.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public SerializationException( string message, Exception? inner )
        : base( message, inner )
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SerializationException"/> class with
    /// a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="inner"></param>
    public SerializationException( Exception inner )
        : base( "", inner )
    {
    }

    /// <summary>
    /// Adds a trace message to the exception's trace information.
    /// </summary>
    /// <param name="traceMessage"></param>
    public void AddTrace( string traceMessage )
    {
        if ( Data.Contains( "Trace" ) )
        {
            Data[ "Trace" ] = Data[ "Trace" ] + Environment.NewLine + traceMessage;
        }
        else
        {
            Data[ "Trace" ] = traceMessage;
        }
    }

    /// <summary>
    /// Gets the trace information associated with the exception.
    /// </summary>
    /// <returns></returns>
    public string? GetTrace()
    {
        return Data.Contains( "Trace" ) ? Data[ "Trace" ]!.ToString() : null;
    }
}

// ========================================================================
// ========================================================================