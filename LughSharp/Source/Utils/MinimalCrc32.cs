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

using System.Security.Cryptography;

namespace LughSharp.Source.Utils;

/// <summary>
/// CRC32 implementation (minimal)
/// </summary>
[PublicAPI]
public class MinimalCrc32 : HashAlgorithm
{
    private uint _crc = 0xFFFFFFFF;
    
    // ========================================================================

    /// <summary>
    /// CRC32 lookup table
    /// </summary>
    private static readonly uint[] _table = Enumerable.Range( 0, 256 ).Select( i =>
    {
        var c = ( uint )i;

        for ( var k = 0; k < 8; k++ )
        {
            c = ( c & 1 ) != 0 ? 0xEDB88320 ^ ( c >> 1 ) : c >> 1;
        }

        return c;
    } ).ToArray();

    // ========================================================================

    /// <summary>
    /// Resets the hash algorithm to its initial state.
    /// </summary>
    public override void Initialize()
    {
        _crc = 0xFFFFFFFF;
    }

    /// <summary>
    /// Gets the size, in bits, of the computed hash code.
    /// </summary>
    /// <returns>The size, in bits, of the computed hash code.</returns>
    public override int HashSize => 32;

    /// <summary>
    /// When overridden in a derived class, routes data written to the object into the
    /// hash algorithm for computing the hash.
    /// </summary>
    /// <param name="array">The input to compute the hash code for.</param>
    /// <param name="ibStart">The offset into the byte array from which to begin using data.</param>
    /// <param name="cbSize">The number of bytes in the byte array to use as data.</param>
    protected override void HashCore( byte[] array, int ibStart, int cbSize )
    {
        for ( int i = ibStart; i < ( ibStart + cbSize ); i++ )
        {
            _crc = _table[ ( byte )( _crc ^ array[ i ] ) ] ^ ( _crc >> 8 );
        }
    }

    /// <summary>
    /// When overridden in a derived class, finalizes the hash computation after the last
    /// data is processed by the cryptographic hash algorithm.
    /// </summary>
    /// <returns>The computed hash code.</returns>
    protected override byte[] HashFinal()
    {
        byte[] hash = BitConverter.GetBytes( ~_crc );

        if ( BitConverter.IsLittleEndian )
        {
            Array.Reverse( hash );
        }

        return hash;
    }
}

// ============================================================================
// ============================================================================