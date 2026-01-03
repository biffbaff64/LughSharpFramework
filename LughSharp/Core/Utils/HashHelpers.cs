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

namespace LughSharp.Core.Utils;

/// <summary>
/// Provides helper methods and constants for efficient hash table index
/// calculations, including support for Fibonacci hashing and bitmask/shift
/// computations for power-of-two table sizes.
/// </summary>
[PublicAPI]
public class HashHelpers
{
    // The golden ratio constant used in Fibonacci hashing
    public const ulong GOLDEN_RATIO_MULTIPLIER_64_BIT = 0x9E3779B97F4A7C15UL; // U for ulong literal
    public const uint  GOLDEN_RATIO_MULTIPLIER_32_BIT = 0x9E3779B9;           // For 32-bit if needed

    // ========================================================================

    /// <summary>
    /// A bitmask used to confine hashcodes to the size of the table. Must be all
    /// 1 bits in its low positions, ie a power of two minus 1.
    /// Example: for a table size of 16, Mask would be 15 (0b00001111).
    /// </summary>
    protected int Mask { get; set; } // Must be power of 2 minus 1

    /// <summary>
    /// The number of bits to shift right to get the desired index.
    /// This is typically calculated as 64 - log2(TableSize).
    /// </summary>
    public int Shift { get; set; }

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tableSize"></param>
    /// <exception cref="ArgumentException"></exception>
    public void Setup( int tableSize ) // Or you can set Mask directly
    {
        // Ensure tableSize is a power of 2
        if ( ( tableSize <= 0 ) || ( ( tableSize & ( tableSize - 1 ) ) != 0 ) )
        {
            throw new ArgumentException( "Table size must be a power of two.", nameof( tableSize ) );
        }

        Mask = tableSize - 1;

        // Calculate shift based on table size
        // This calculates log2(tableSize)
        var bitsForTableSize = 0;
        var tempTableSize    = tableSize;

        while ( tempTableSize > 1 )
        {
            tempTableSize >>= 1;
            bitsForTableSize++;
        }

        Shift = 64 - bitsForTableSize;
    }

    /// <summary>
    /// Helper to calculate shift from mask, assuming mask is power-of-2 minus 1
    /// </summary>
    protected static int CalculateShiftFromMask( int mask )
    {
        if ( ( mask <= 0 ) || ( ( mask & ( mask + 1 ) ) != 0 ) )
        {
            throw new ArgumentException( "Mask must be a power of two minus one (e.g., 7, 15, 31).", nameof( mask ) );
        }

        // Calculate the number of leading zeros to find the most significant bit's position,
        // then subtract from total bits (32 for int, assuming Mask is used with 32-bit hashes implicitly)
        // Or, more simply, it's 32 - (log2(mask + 1)) which is 32 - table_size_power_of_2_exponent
        // Example: Mask = 15 (0b1111). Table size = 16 (2^4). Shift = 32 - 4 = 28.
        // Example: Mask = 7 (0b111). Table size = 8 (2^3). Shift = 32 - 3 = 29.
        // This assumes the Mask determines the *lower* bits to keep.
        // The original Java code's Shift implies taking *upper* bits.
        // Let's assume Shift is intended to be for the *table size* itself.
        // If table size is power of 2, then tableSize = 1 << tableSizePower
        // Then Shift = 64 - tableSizePower.

        // A safer way to get the power of 2 from a mask (if mask is (1 << n) - 1):
        var tableSize      = mask + 1;
        var tableSizePower = 0;

        while ( ( 1 << tableSizePower ) < tableSize )
        {
            tableSizePower++;
        }

        return 64 - tableSizePower; // For 64-bit hash
    }
}

// ========================================================================
// ========================================================================