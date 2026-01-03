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

using System.Collections;

namespace LughSharp.Core.Utils.Exceptions;

public partial class Guard
{
    [PublicAPI]
    public static class Against
    {
        /// <summary>
        /// Throws ArgumentNullException if obj is null.
        /// Provides a clear error message with the argumentName.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Null( [System.Diagnostics.CodeAnalysis.NotNull] object? obj,
                                 [CallerArgumentExpression( nameof( obj ) )]
                                 string argumentName = "" )
        {
            ArgumentNullException.ThrowIfNull( obj, argumentName );
        }

        /// <summary>
        /// Throws ArgumentNullException if any of the objects in the 'objects[]' array is null.
        /// Provides a clear error message with the argumentName.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Null( [System.Diagnostics.CodeAnalysis.NotNull] params object?[]? objects )
        {
            ArgumentNullException.ThrowIfNull( objects );

            foreach ( var obj in objects )
            {
                ArgumentNullException.ThrowIfNull( obj, obj?.GetType().FullName );
            }
        }

        /// <summary>
        /// Throws ArgumentNullException if argumentValue is null.
        /// Throws ArgumentException if argumentValue is string.Empty.
        /// Provides a clear error message with the argumentName.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NullOrEmpty( [System.Diagnostics.CodeAnalysis.NotNull] string? argumentValue,
                                        [CallerArgumentExpression( nameof( argumentValue ) )]
                                        string argumentName = "" )
        {
            ArgumentNullException.ThrowIfNull( argumentValue, argumentName );

            if ( argumentValue.Length == 0 )
            {
                throw new ArgumentException( argumentName );
            }
        }

        /// <summary>
        /// Throws ArgumentException if <paramref name="collection"/> is null or empty.
        /// Provides a clear error message with the argumentName.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="argumentName"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NullOrEmpty( ICollection collection,
                                        [CallerArgumentExpression( nameof( collection ) )]
                                        string argumentName = "" )
        {
            if ( ( collection == null ) || ( collection.Count == 0 ) )
            {
                throw new ArgumentNullException( argumentName );
            }
        }

        /// <summary>
        /// Throws ArgumentException if <paramref name="value"/> is negative.
        /// Provides a clear error message with the argumentName.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="argumentName"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Negative( int value,
                                     [CallerArgumentExpression( nameof( value ) )]
                                     string argumentName = "" )
        {
            if ( value < 0 )
            {
                throw new ArgumentException( argumentName );
            }
        }

        /// <summary>
        /// Throws ArgumentException if <paramref name="value"/> is out of range.
        /// Provides a clear error message with the argumentName.
        /// </summary>
        /// <param name="value"> The value to range check. </param>
        /// <param name="minimum"> The minimum allowed value. </param>
        /// <param name="maximum"> The maximim allowed value. </param>
        /// <param name="argumentName"> The name of the passed argument ( optional ).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OutOfRange( int value, int minimum, int maximum,
                                       [CallerArgumentExpression( nameof( value ) )]
                                       string argumentName = "" )
        {
            if ( ( value < minimum ) || ( value > maximum ) )
            {
                throw new ArgumentException( argumentName );
            }
        }

        /// <summary>
        /// Throws ArgumentOutOfRangeException if argumentValue is greater than or equal to maximum.
        /// Provides a clear error message with the argumentName.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GreaterThan( int argumentValue, int maximum,
                                        [CallerArgumentExpression( nameof( argumentValue ) )]
                                        string argumentName = "" )
        {
            if ( argumentValue >= maximum )
            {
                throw new ArgumentOutOfRangeException( argumentName, $"Value {argumentValue} must be less than maximum {maximum}" );
            }
        }
    }
}

// ============================================================================
// ============================================================================