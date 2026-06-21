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

namespace LughSharp.Source.Utils;

/// <summary>
/// Provides utility methods for working with buffers.
/// </summary>
[PublicAPI]
public class BufferUtils
{
    //TODO: the byte-by-byte loops in these methods are a performance liability for large
    // vertex/index uploads — replace them with MemoryMarshal.Cast + Span<T>.CopyTo eventually. 
    
    /// <summary>
    /// Copies the content of the source buffer to the destination buffer.
    /// </summary>
    /// <param name="source">The source buffer to copy from.</param>
    /// <param name="destination">The destination buffer to copy to.</param>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="source"/> or <paramref name="destination"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// If buffers are incompatible or destination is too small.
    /// </exception>
    public static void Copy< T >( Buffer< T > source, Buffer< T > destination ) where T : unmanaged
    {
        Guard.Against.Null( source );
        Guard.Against.Null( destination );

        if ( source == destination )
        {
            throw new ArgumentException( "Source and destination buffers cannot be the same instance." );
        }

        var bytesToCopy = Math.Max( 0, Math.Min( source.Remaining(), destination.Remaining() ) );

        for ( var i = 0; i < bytesToCopy; i++ )
        {
            destination.PutByte( source.GetByte() );
        }
    }

    /// <summary>
    /// Copies a range of bytes from the source buffer to the destination buffer.
    /// </summary>
    /// <param name="source">The source buffer.</param>
    /// <param name="sourceOffset">The starting position in the source buffer to copy from.</param>
    /// <param name="destination">The destination buffer.</param>
    /// <param name="destinationOffset">The starting position in the destination buffer to copy to.</param>
    /// <param name="length">The number of bytes to copy.</param>
    public static void Copy< T >( Buffer< T > source,
                                  int sourceOffset,
                                  Buffer< T > destination,
                                  int destinationOffset,
                                  int length ) where T : unmanaged
    {
        Guard.Against.Null( source );
        Guard.Against.Null( destination );

        if ( source == destination )
        {
            if ( sourceOffset == destinationOffset )
            {
                return;
            }

            throw new ArgumentException( "In-place copy within the same buffer with different offsets is not supported." );
        }

        if ( ( sourceOffset < 0 ) || ( destinationOffset < 0 ) || ( length < 0 ) )
        {
            throw new RuntimeException( "Offsets and length must be non-negative." );
        }

        destination.EnsureCapacity( destinationOffset + length );

        if ( ( sourceOffset + length ) > source.Capacity )
        {
            throw new RuntimeException( "Cannot copy more than source contents." );
        }

        for ( var i = 0; i < length; i++ )
        {
            destination.PutByte( destinationOffset + i, source.GetByte( sourceOffset + i ) );
        }
    }

    /// <summary>
    /// Copies data from a source buffer to a destination buffer.
    /// </summary>
    /// <typeparam name="T">The type of the elements contained in the buffers. Must be unmanaged.</typeparam>
    /// <param name="source">The source buffer to copy data from.</param>
    /// <param name="destination">The destination buffer to copy data to.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="source"/> or <paramref name="destination"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the buffers are incompatible or if the destination buffer does not have sufficient capacity.
    /// </exception>
    public static void Copy( byte[] source, int sourceOffset, int length, Buffer< byte > destination )
    {
        ValidateArrayCopyArgs( source, destination, sourceOffset, length, sizeof( byte ) );

        for ( var i = 0; i < length; i++ )
        {
            destination.PutByte( i, source[ sourceOffset + i ] );
        }
    }

    /// <summary>
    /// Copies elements from a source array of type <see cref="short"/> into a destination buffer.
    /// </summary>
    /// <param name="source">The source array containing the short elements to copy.</param>
    /// <param name="sourceOffset">The offset in the source array at which to begin copying.</param>
    /// <param name="length">The number of elements to copy from the source array.</param>
    /// <param name="destination">The destination buffer where the elements will be copied.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="source"/> or <paramref name="destination"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="sourceOffset"/> or <paramref name="length"/> is negative.
    /// </exception>
    /// <exception cref="RuntimeException">
    /// Thrown if the destination buffer cannot accommodate the required capacity.
    /// </exception>
    public static void Copy( short[] source, int sourceOffset, int length, Buffer< byte > destination )
    {
        ValidateArrayCopyArgs( source, destination, sourceOffset, length, sizeof( short ) );

        for ( var i = 0; i < length; i++ )
        {
            destination.PutShort( i, source[ sourceOffset + i ] );
        }
    }

    /// <summary>
    /// Copies a specified range of elements from the source array to the destination buffer.
    /// </summary>
    /// <param name="source">The source array containing the data to copy.</param>
    /// <param name="sourceOffset">The starting index in the source array from where data will be copied.</param>
    /// <param name="length">The number of elements to copy from the source array.</param>
    /// <param name="destination">The destination buffer to which the data will be copied.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="source"/> or <paramref name="destination"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="sourceOffset"/> or <paramref name="length"/> is negative,
    /// or if the destination buffer does not have sufficient capacity.
    /// </exception>
    public static void Copy( float[] source, int sourceOffset, int length, Buffer< byte > destination )
    {
        ValidateArrayCopyArgs( source, destination, sourceOffset, length, sizeof( float ) );

        for ( var i = 0; i < length; i++ )
        {
            destination.PutFloat( i, source[ sourceOffset + i ] );
        }
    }

    /// <summary>
    /// Copies a range of integers from the source array to the destination buffer.
    /// </summary>
    /// <param name="source">The source array to copy data from.</param>
    /// <param name="sourceOffset">The starting position in the source array.</param>
    /// <param name="length">The number of integers to copy.</param>
    /// <param name="destination">The destination buffer where the data will be copied to.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if either <paramref name="source"/> or <paramref name="destination"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="sourceOffset"/> or <paramref name="length"/> is negative, or if
    /// the destination buffer does not have enough capacity.
    /// </exception>
    public static void Copy( int[] source, int sourceOffset, int length, Buffer< byte > destination )
    {
        ValidateArrayCopyArgs( source, destination, sourceOffset, length, sizeof( int ) );

        for ( var i = 0; i < length; i++ )
        {
            destination.PutInt( i, source[ sourceOffset + i ] );
        }
    }

    /// <summary>
    /// Validates the arguments for an array copy operation to ensure they meet the
    /// necessary conditions.
    /// </summary>
    /// <param name="source">The source array from which data will be copied.</param>
    /// <param name="destination">The destination buffer to which data will be copied.</param>
    /// <param name="sourceOffset">The starting index in the source array.</param>
    /// <param name="length">The number of elements to copy.</param>
    /// <param name="elementSize">The size of each element in bytes.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="source"/> or <paramref name="destination"/> is null.
    /// </exception>
    /// <exception cref="RuntimeException">
    /// Thrown if <paramref name="sourceOffset"/> or <paramref name="length"/> is negative.
    /// </exception>
    private static void ValidateArrayCopyArgs( Array source, Buffer< byte > destination, int sourceOffset, int length,
                                               int elementSize )
    {
        Guard.Against.Null( source );
        Guard.Against.Null( destination );

        if ( ( sourceOffset < 0 ) || ( length < 0 ) )
        {
            throw new RuntimeException( "Offset and length must be non-negative." );
        }

        destination.EnsureCapacity( length * elementSize );
    }
}

// ============================================================================
// ============================================================================