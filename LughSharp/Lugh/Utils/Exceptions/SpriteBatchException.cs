// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Richard Ikin.
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

using System.Runtime.CompilerServices;

using LughSharp.Lugh.Graphics.G2D;

using Exception = System.Exception;
using NotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;

namespace LughSharp.Lugh.Utils.Exceptions;

/// <summary>
/// Typed runtime exception used in SpriteBatch.
/// </summary>
[PublicAPI]
public class SpriteBatchException : ApplicationException
{
    /// <summary>
    /// Initializes a new SpriteBatchException with a specified error message.
    /// </summary>
    /// <param name="message"> The message that describes the error. </param>
    public SpriteBatchException( string? message = "" )
        : base( message )
    {
    }

    /// <summary>
    /// Initializes a new SpriteBatchException with a reference to the inner exception
    /// that is the cause of this exception.
    /// </summary>
    /// <param name="e">
    /// The exception that is the cause of the current exception, or a null
    /// reference if no inner exception is specified.
    /// </param>
    public SpriteBatchException( Exception? e )
        : this( "", e )
    {
    }

    /// <summary>
    /// Initializes a new SpriteBatchException with a specified error message and a
    /// reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="exception">
    /// The exception that is the cause of the current exception, or a null
    /// reference if no inner exception is specified.
    /// </param>
    public SpriteBatchException( string message, Exception? exception )
        : base( message, exception )
    {
    }

    /// <summary>
    /// Throws an SpriteBatchException if End, or Draw, is called before Begin.
    /// </summary>
    /// <param name="batch"> </param>
    [InTesting]
    public static void ThrowIfEndBeforeBegin( IBatch batch )
    {
        if ( batch.IsDrawing )
        {
            Throw( "Draw Error: Begin() must be called before Draw()" );
        }
    }

    /// <summary>
    /// Throws an SpriteBatchException if Begin is called before End and the batch is currently drawing.
    /// </summary>
    /// <param name="batch"> </param>
    [InTesting]
    public static void ThrowIfBeginBeforeEnd( IBatch batch )
    {
        if ( !batch.IsDrawing )
        {
            Throw( "Draw Error: End() must be called before Begin()" );
        }
    }

    // ========================================================================
    
    [DoesNotReturn]
    internal static void Throw( string? message )
    {
        throw new GdxRuntimeException( message );
    }
}