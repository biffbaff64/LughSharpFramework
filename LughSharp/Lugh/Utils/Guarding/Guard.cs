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

using System.Runtime.CompilerServices;

namespace LughSharp.Lugh.Utils.Guarding;

[PublicAPI]
public class Guard
{
    // ========================================================================
    // Null checks
    // ========================================================================

    /// <summary>
    /// Throws ArgumentNullException if argumentValue is null.
    /// Provides a clear error message with the argumentName.
    /// </summary>
    public static void ThrowIfNull( object? obj,
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
    public static void ThrowIfNullOrEmpty( string? argumentValue,
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
    /// Throws ArgumentNullException if argumentValue is null.
    /// Throws ArgumentException if argumentValue is string.Empty or consists only of whitespace characters.
    /// Provides a clear error message with the argumentName.
    /// </summary>
    public static void ThrowIfNullOrWhiteSpace( string? argumentValue,
                                                [CallerArgumentExpression( nameof( argumentValue ) )]
                                                string argumentName = "" )
    {
        ArgumentNullException.ThrowIfNull( argumentValue, argumentName );
    }

    // ========================================================================
    // Range checks
    // ========================================================================

    /// <summary>
    /// Throws ArgumentOutOfRangeException if argumentValue is less than minimum or greater than maximum.
    /// Provides a clear error message with the argumentName, minimum, and maximum.
    /// </summary>
    public static void InRange( int argumentValue, int minimum, int maximum,
                                [CallerArgumentExpression( nameof( argumentValue ) )]
                                string argumentName = "" )
    {
        if ( ( argumentValue < minimum ) || ( argumentValue > maximum ) )
        {
            throw new ArgumentOutOfRangeException( argumentName, $"Value {argumentValue} must be between {minimum} and {maximum}" );
        }
    }

    /// <summary>
    /// Throws ArgumentOutOfRangeException if argumentValue is less than minimum or greater than maximum.
    /// Provides a clear error message with the argumentName, minimum, and maximum.
    /// </summary>
    public static void InRange( double argumentValue, double minimum, double maximum,
                                [CallerArgumentExpression( nameof( argumentValue ) )]
                                string argumentName = "" )
    {
        if ( ( argumentValue < minimum ) || ( argumentValue > maximum ) )
        {
            throw new ArgumentOutOfRangeException( argumentName, $"Value {argumentValue} must be between {minimum} and {maximum}" );
        }
    }

    /// <summary>
    /// Throws ArgumentOutOfRangeException if argumentValue is less than or equal to minimum.
    /// </summary>
    public static void GreaterThan( int argumentValue, int minimum,
                                    [CallerArgumentExpression( nameof( argumentValue ) )]
                                    string argumentName = "" )
    {
        if ( argumentValue <= minimum )
        {
            throw new ArgumentOutOfRangeException( argumentName, $"Value {argumentValue} must be greater than minimum {minimum}" );
        }
    }

    /// <summary>
    /// Throws ArgumentOutOfRangeException if argumentValue is greater than or equal to maximum.
    /// </summary>
    public static void LessThan( int argumentValue, int maximum,
                                 [CallerArgumentExpression( nameof( argumentValue ) )]
                                 string argumentName = "" )
    {
        if ( argumentValue >= maximum )
        {
            throw new ArgumentOutOfRangeException( argumentName, $"Value {argumentValue} must be less than maximum {maximum}" );
        }
    }

    // ========================================================================
    // Type checks
    // ========================================================================

    /// <summary>
    /// Throws ArgumentException if argumentValue is not of type T.
    /// Provides a clear error message with the argumentName and the expected type.
    /// </summary>
    public static void OfType< T >( object argumentValue,
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
    public static void AssignableTo< T >( object argumentValue,
                                          [CallerArgumentExpression( nameof( argumentValue ) )]
                                          string argumentName = "" )
    {
        if ( argumentValue is T )
        {
            return;
        }

        throw new ArgumentException( $"Type {argumentValue} must be assignable to {typeof( T )}" );
    }

    // ========================================================================
    // Boolean checks
    // ========================================================================

    /// <summary>
    /// Throws ArgumentException if argumentValue is false.
    /// Provides a clear error message with the argumentName.
    /// </summary>
    public static void IsTrue( [DoesNotReturnIf( false )] bool argumentValue,
                               [CallerArgumentExpression( nameof( argumentValue ) )]
                               string argumentName = "" )
    {
        if ( argumentValue )
        {
            return;
        }

        throw new ArgumentException( $"The result of expression {argumentValue} should be TRUE." );
    }

    /// <summary>
    /// Throws ArgumentException if argumentValue is true.
    /// Provides a clear error message with the argumentName.
    /// </summary>
    public static void IsFalse( [DoesNotReturnIf( true )] bool argumentValue,
                                [CallerArgumentExpression( nameof( argumentValue ) )]
                                string argumentName = "" )
    {
        if ( !argumentValue )
        {
            return;
        }

        throw new ArgumentException( $"The result of expression {argumentValue} should be FALSE." );
    }

    // ========================================================================
    // Collection checks
    // ========================================================================

    /// <summary>
    /// Throws ArgumentException if collection is null or empty.
    /// Provides a clear error message with the argumentName.
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="argumentName"></param>
    public static void NotEmpty( ICollection collection,
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
    public static void NotNullOrEmpty< T >( IEnumerable< T >? enumerable,
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

    // ========================================================================
    // Custom Validation checks
    // ========================================================================

    // ========================================================================
    // Number / Maths checks
    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    public static void ValidPositiveInteger( int value,
                                             [CallerArgumentExpression( nameof( value ) )]
                                             string argumentName = "" )
    {
        if ( value >= 0 )
        {
            return;
        }

        throw new ArithmeticException( $"Value {value} must be positive." );
    }

    // ========================================================================
    // String checks
    // ========================================================================

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the supplied string is either Null, empty or
    /// consists only of whitespace characters.
    /// </summary>
    public static void ValidateString( string input )
    {
        if ( string.IsNullOrWhiteSpace( input ) )
        {
            throw new ArgumentException( "Input string cannot be null or empty." );
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the supplied string's length is
    /// greater than the specified maximum length.
    /// </summary>
    public static void ValidateStringLength( string input, int maxLength )
    {
        if ( input.Length <= maxLength )
        {
            return;
        }

        throw new ArgumentException( $"Supplied string length must be less than or equal to {maxLength}" );
    }
}