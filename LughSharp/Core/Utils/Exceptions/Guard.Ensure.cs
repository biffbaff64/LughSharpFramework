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

namespace LughSharp.Core.Utils.Exceptions;

public partial class Guard
{
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

            throw new ArgumentOutOfRangeException( argumentName, $"Value {value} must be less than or equal to {limit}" );
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

            throw new ArgumentOutOfRangeException( argumentName, $"Value {value} must be greater, or equal to, than {minimum}" );
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
                                                  $" or equal to one of those values.");
        }
    }
}

// ============================================================================
// ============================================================================