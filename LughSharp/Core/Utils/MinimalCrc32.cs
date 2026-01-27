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

using System;
using System.Linq;
using System.Security.Cryptography;
using JetBrains.Annotations;

namespace LughSharp.Core.Utils;

/// <summary>
/// CRC32 implementation (minimal)
/// </summary>
[PublicAPI]
public class MinimalCrc32 : HashAlgorithm
{
    private uint _crc = 0xFFFFFFFF;

    private static readonly uint[] _table = Enumerable.Range( 0, 256 ).Select( i =>
    {
        var c = ( uint )i;

        for ( var k = 0; k < 8; k++ )
        {
            c = ( c & 1 ) != 0 ? 0xEDB88320 ^ ( c >> 1 ) : c >> 1;
        }

        return c;
    } ).ToArray();

    public override void Initialize()
    {
        _crc = 0xFFFFFFFF;
    }

    public override int HashSize => 32;

    protected override void HashCore( byte[] array, int ibStart, int cbSize )
    {
        for ( var i = ibStart; i < ( ibStart + cbSize ); i++ )
        {
            _crc = _table[ ( byte )( _crc ^ array[ i ] ) ] ^ ( _crc >> 8 );
        }
    }

    protected override byte[] HashFinal()
    {
        var hash = BitConverter.GetBytes( ~_crc );

        if ( BitConverter.IsLittleEndian )
        {
            Array.Reverse( hash );
        }

        return hash;
    }
}

// ============================================================================
// ============================================================================
