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

using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Utils;

[PublicAPI]
public class BufferUtils
{
    /// <summary>
    /// Copies the content of the source buffer to the destination buffer.
    /// </summary>
    /// <param name="source">The source buffer to copy from.</param>
    /// <param name="destination">The destination buffer to copy to.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="source" /> or <paramref name="destination" /> is null.</exception>
    /// <exception cref="ArgumentException">If buffers are incompatible or destination is too small.</exception>
    public static void Copy< T >( Buffer< T > source, Buffer< T > destination ) where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull( source );
        ArgumentNullException.ThrowIfNull( destination );

        if ( source == destination )
        {
            throw new ArgumentException( "Source and destination buffers cannot be the same instance." );
        }

        var bytesToCopy = Math.Min( source.Remaining(), destination.Remaining() ); // Copy up to the smaller remaining size

        if ( bytesToCopy < 0 )
        {
            bytesToCopy = 0; // Ensure non-negative if somehow Remaining is negative
        }

        for ( var i = 0; i < bytesToCopy; i++ )
        {
            destination.PutByte( source.GetByte() ); // Sequential byte-by-byte copy - optimize for bulk copy if needed.
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

    // ... (Overload with offsets and length for more control) ...
    public static void Copy< T >( Buffer< T > source,
                                  int sourceOffset,
                                  Buffer< T > destination,
                                  int destinationOffset,
                                  int length ) where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull( source );
        ArgumentNullException.ThrowIfNull( destination );

        if ( source == destination )
        {
            if ( sourceOffset == destinationOffset )
            {
                return; // No copy needed if same buffer and offsets are same
            }

            // Handle overlapping copy within the same buffer carefully if needed (more complex)
            // For now, throw exception if source and destination are same instance with different offsets:
            throw new ArgumentException( "In-place copy within the same buffer with different offsets is " +
                                         "not yet supported. Use different buffer instances or implement " +
                                         "in-place copy logic." );
        }

        if ( ( sourceOffset < 0 ) || ( destinationOffset < 0 ) || ( length < 0 ) )
        {
            throw new GdxRuntimeException( "Offsets and length must be non-negative." );
        }

        destination.EnsureCapacity( destinationOffset + length );

        if ( ( sourceOffset + length ) > source.Capacity )
        {
            throw new GdxRuntimeException( "Cannot copy more than source contents." );
        }

        for ( var i = 0; i < length; i++ )
        {
            // Indexed byte-by-byte copy - optimize for bulk copy if needed.
            destination.PutByte( destinationOffset + i, source.GetByte( sourceOffset + i ) );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="source"></param>
    /// <param name="sourceOffset"></param>
    /// <param name="length"></param>
    /// <param name="destination"></param>
    /// <exception cref="NotImplementedException"></exception>
    public static void Copy( byte[] source, int sourceOffset, int length, Buffer< byte > destination )
    {
        ArgumentNullException.ThrowIfNull( source );
        ArgumentNullException.ThrowIfNull( destination );

        if ( ( sourceOffset < 0 ) || ( length < 0 ) )
        {
            throw new GdxRuntimeException( "Offset and length must be non-negative." );
        }

        destination.EnsureCapacity( length * sizeof( byte ) );

        for ( var i = 0; i < length; i++ )
        {
            // Indexed byte-by-byte copy - optimize for bulk copy if needed.
            destination.PutByte( i, source[ sourceOffset + i ] );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="source"></param>
    /// <param name="sourceOffset"></param>
    /// <param name="length"></param>
    /// <param name="destination"></param>
    /// <exception cref="NotImplementedException"></exception>
    public static void Copy( short[] source, int sourceOffset, int length, Buffer< byte > destination )
    {
        ArgumentNullException.ThrowIfNull( source );
        ArgumentNullException.ThrowIfNull( destination );

        if ( ( sourceOffset < 0 ) || ( length < 0 ) )
        {
            throw new GdxRuntimeException( "Offset and length must be non-negative." );
        }

        destination.EnsureCapacity( length * sizeof( short ) );

        for ( var i = 0; i < length; i++ )
        {
            // Indexed byte-by-byte copy - optimize for bulk copy if needed.
            destination.PutShort( i, source[ sourceOffset + i ] );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="source"></param>
    /// <param name="sourceOffset"></param>
    /// <param name="length"></param>
    /// <param name="destination"></param>
    /// <exception cref="NotImplementedException"></exception>
    public static void Copy( float[] source, int sourceOffset, int length, Buffer< byte > destination )
    {
        ArgumentNullException.ThrowIfNull( source );
        ArgumentNullException.ThrowIfNull( destination );

        if ( ( sourceOffset < 0 ) || ( length < 0 ) )
        {
            throw new GdxRuntimeException( "Offset and length must be non-negative." );
        }

        destination.EnsureCapacity( length * sizeof( float ) );

        for ( var i = 0; i < length; i++ )
        {
            // Indexed byte-by-byte copy - optimize for bulk copy if needed.
            destination.PutFloat( i, source[ sourceOffset + i ] );
        }
    }
}