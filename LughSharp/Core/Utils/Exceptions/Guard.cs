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
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace LughSharp.Core.Utils.Exceptions;

[PublicAPI]
public class Guard
{
    #region Collection checks

    // ========================================================================

    /// <summary>
    /// Throws ArgumentException if array is null or empty.
    /// Provides a clear error message with the argumentName.
    /// </summary>
    /// <param name="argument"></param>
    /// <param name="argumentName"></param>
    public static void ThrowIfNullOrEmpty( string[]? argument,
                                           [CallerArgumentExpression( nameof( argument ) )]
                                           string argumentName = "" )
    {
        if ( ( argument == null ) || ( argument.Length == 0 ) )
        {
            throw new ArgumentNullException( argumentName );
        }
    }

    /// <summary>
    /// Throws ArgumentException if collection is null or empty.
    /// Provides a clear error message with the argumentName.
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="argumentName"></param>
    public static void ThrowIfNullOrEmpty( ICollection collection,
                                           [CallerArgumentExpression( nameof( collection ) )]
                                           string argumentName = "" )
    {
        if ( ( collection == null ) || ( collection.Count == 0 ) )
        {
            throw new ArgumentNullException( argumentName );
        }
    }

    /// <summary>
    /// Throws ArgumentNullException if argumentValue is null.
    /// Throws ArgumentException if argumentValue is empty.
    /// </summary>
    public static void ThrowIfNullOrEmpty< T >( IEnumerable< T >? enumerable,
                                                [CallerArgumentExpression( nameof( enumerable ) )]
                                                string argumentName = "" )
    {
        if ( enumerable == null )
        {
            throw new ArgumentNullException( argumentName );
        }

        var enumerable1 = enumerable as T[] ?? enumerable.ToArray();

        if ( enumerable1.Length == 0 )
        {
            throw new ArgumentException( $"The Enumerable {enumerable1} should not be empty." );
        }
    }

    #endregion

    // ========================================================================

    #region File Validation checks

    // ========================================================================

    /// <summary>
    /// Throws an ArgumentNullException if the specified FileInfo object is null.
    /// Throws an ArgumentException if the file specified by the provided FileInfo object
    /// does not exist.
    /// </summary>
    public static void ThrowIfFileNullOrNotExist(
        [System.Diagnostics.CodeAnalysis.NotNull] FileSystemInfo? argumentValue,
        [CallerArgumentExpression( nameof( argumentValue ) )]
        string argumentName = "" )
    {
        switch ( argumentValue )
        {
            case { Exists: true }:
                return;

            case null:
                throw new ArgumentNullException( $"The File {argumentName} cannot be null." );

            default:
                throw new ArgumentException( $"The file {argumentName} does not exist" );
        }
    }

    /// <summary>
    /// Throws an ArgumentException if the provided FileSystemInfo object is not
    /// a valid file or directory.
    /// Throws an ArgumentNullException if the specified FileSystemInfo object is null.
    /// Throws an ArgumentException if the file specified by the provided FileSystemInfo object
    /// does not exist.
    /// </summary>
    public static void ThrowIfNotFileOrDirectory(
        [System.Diagnostics.CodeAnalysis.NotNull] FileSystemInfo? argumentValue,
        [CallerArgumentExpression( nameof( argumentValue ) )]
        string argumentName = "" )
    {
        switch ( argumentValue )
        {
            case { Exists: true }:

                if ( argumentValue is not DirectoryInfo or FileInfo )
                {
                    throw new ArgumentException( $"{argumentName} must be a valid  directory or file." );
                }

                return;

            case null:
                throw new ArgumentNullException( $"The File {argumentName} cannot be null." );

            default:
                throw new ArgumentException( $"The file {argumentName} does not exist" );
        }
    }

    #endregion

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public static class Against
    {
        /// <summary>
        /// Throws ArgumentNullException if obj is null.
        /// Provides a clear error message with the argumentName.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void Null( [System.Diagnostics.CodeAnalysis.NotNull] object? obj,
                                 [CallerArgumentExpression( nameof( obj ) )]
                                 string argumentName = "" )
        {
            ArgumentNullException.ThrowIfNull( obj, argumentName );
        }

        /// <summary>
        /// Throws ArgumentNullException if argumentValue is null.
        /// Throws ArgumentException if argumentValue is string.Empty.
        /// Provides a clear error message with the argumentName.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
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
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
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
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
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
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
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
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void GreaterThan( int argumentValue, int maximum,
                                        [CallerArgumentExpression( nameof( argumentValue ) )]
                                        string argumentName = "" )
        {
            if ( argumentValue >= maximum )
            {
                throw new ArgumentOutOfRangeException( argumentName,
                                                       $"Value {argumentValue} must be less than maximum {maximum}" );
            }
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public static class Ensure
    {
        /// <summary>
        /// Verifies that the specified value is less than a maximum value and throws
        /// an exception if it is not.
        /// </summary>
        /// <param name="value">The target value, which should be validated.</param>
        /// <param name="limit">The maximum value.</param>
        /// <param name="argumentName">The name of the parameter that is to be checked.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="value"/> is greater than, or equal to, the maximum value.
        /// </exception>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void LessThan< T >( T value, T limit, string argumentName = "" )
            where T : IComparable< T >
        {
            if ( value.CompareTo( limit ) < 0 )
            {
                return;
            }

            throw new ArgumentOutOfRangeException( argumentName, $"Value {value} must be less than {limit}" );
        }

        /// <summary>
        /// Verifies that the specified value is less than or equal to a maximum value
        /// and throws an exception if it is not.
        /// </summary>
        /// <param name="value">The target value, which should be validated.</param>
        /// <param name="limit">The maximum value.</param>
        /// <param name="argumentName">The name of the parameter that is to be checked.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="value"/> is greater than the maximum value.
        /// </exception>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void LessThanOrEqualTo< T >( T value, T limit, string argumentName = "" )
            where T : IComparable< T >
        {
            if ( value.CompareTo( limit ) <= 0 )
            {
                return;
            }

            throw new ArgumentOutOfRangeException( argumentName,
                                                   $"Value {value} must be less than or equal to {limit}" );
        }

        /// <summary>
        /// Verifies that the specified value is greater than a minimum value and throws
        /// an exception if it is not.
        /// </summary>
        /// <param name="value">The target value, which should be validated.</param>
        /// <param name="minimum">The minimum value.</param>
        /// <param name="argumentName">The name of the parameter that is to be checked.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="value"/> is less than, or equal to, the minimum value.
        /// </exception>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void GreaterThan< T >( T value, T minimum, string argumentName = "" )
            where T : IComparable< T >
        {
            if ( value.CompareTo( minimum ) > 0 )
            {
                return;
            }

            throw new ArgumentOutOfRangeException( argumentName, $"Value {value} must be greater than {minimum}" );
        }

        /// <summary>
        /// Verifies that the specified value is greater than, or equal to, a minimum
        /// value and throws an exception if it is not.
        /// </summary>
        /// <param name="value">The target value, which should be validated.</param>
        /// <param name="minimum">The minimum value.</param>
        /// <param name="argumentName">The name of the parameter that is to be checked.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="value"/> is less than the minimum value.
        /// </exception>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void GreaterThanOrEqualTo< T >( T value, T minimum, string argumentName = "" )
            where T : IComparable< T >
        {
            if ( value.CompareTo( minimum ) >= 0 )
            {
                return;
            }

            throw new ArgumentOutOfRangeException( argumentName,
                                                   $"Value {value} must be greater, or equal to, than {minimum}" );
        }

        /// <summary>
        /// Verifies that the specified value is greater than or equal to a minimum value and less than
        /// or equal to a maximum value and throws an exception if it is not.
        /// </summary>
        /// <param name="value">The target value, which should be validated.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="argumentName">The name of the parameter that is to be checked.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <exception cref="ArgumentException">
        /// <paramref name="value"/> is less than the minimum value or greater than the maximum value.
        /// </exception>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void BetweenOrEqualTo< T >( T value, T min, T max, string argumentName )
            where T : IComparable< T >
        {
            if ( ( value.CompareTo( min ) >= 0 ) && ( value.CompareTo( max ) <= 0 ) )
            {
                return;
            }

            throw new ArgumentOutOfRangeException( argumentName,
                                                   $"Value {value} must be between {min} and {max}," +
                                                   $" or equal to one of those values." );
        }

        /// <summary>
        /// Throws ArgumentException if argumentValue is not of type T.
        /// Provides a clear error message with the argumentName and the expected type.
        /// </summary>
        public static void IsOfType< T >( object argumentValue,
                                          [CallerArgumentExpression( nameof( argumentValue ) )]
                                          string argumentName = "" )
        {
            if ( argumentValue.GetType() == typeof( T ) )
            {
                return;
            }

            throw new ArgumentException( $"Object {argumentValue} must be of type {typeof( T )}" );
        }

        /// <summary>
        /// Throws ArgumentException if argumentValue is not assignable to type T.
        /// </summary>
        public static void IsAssignableTo< T >( object argumentValue,
                                                [CallerArgumentExpression( nameof( argumentValue ) )]
                                                string argumentName = "" )
        {
            if ( argumentValue is T )
            {
                return;
            }

            throw new ArgumentException( $"Type {argumentValue} must be assignable to {typeof( T )}" );
        }
    }
}

// ============================================================================
// ============================================================================