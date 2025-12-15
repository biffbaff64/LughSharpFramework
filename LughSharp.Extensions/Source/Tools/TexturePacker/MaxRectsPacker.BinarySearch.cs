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

namespace Extensions.Source.Tools.TexturePacker;

public partial class MaxRectsPacker
{
    /// <summary>
    /// A specialized binary search utility class designed to find an optimal integer value 
    /// within a specified range. It supports optional constraints for Power-of-Two (POT)
    /// and Modulus 4 (mod4) sizing, primarily used in texture packing algorithms.
    /// </summary>
    [PublicAPI]
    public class BinarySearch
    {
        // ----------------- Configuration Fields -----------------

        private readonly bool _pot;
        private readonly bool _mod4;

        /// <summary>
        /// The minimum value (or exponent) for the search range.
        /// </summary>
        private readonly int  _min;

        /// <summary>
        /// The maximum value (or exponent) for the search range.
        /// </summary>
        private readonly int  _max;

        /// <summary>
        /// The tolerance for ending the search before 'low' equals 'high'. Only used if not POT.
        /// </summary>
        private readonly int  _fuzziness;

        // ----------------- State Fields -----------------

        /// <summary>
        /// The current lower bound of the search.
        /// </summary>
        private int _low;

        /// <summary>
        /// The current upper bound of the search.
        /// </summary>
        private int _high;

        /// <summary>
        /// The current midpoint being tested.
        /// </summary>
        private int _current;

        // ====================================================================

        /// <summary>
        /// Initializes a new instance of the BinarySearch class.
        /// </summary>
        /// <param name="min">The minimum allowable value.</param>
        /// <param name="max">The maximum allowable value.</param>
        /// <param name="fuzziness">
        /// The range tolerance for ending the search. Ignored if <paramref name="pot"/> is true.
        /// </param>
        /// <param name="pot">
        /// If true, the search iterates over exponents (log base 2) and returns powers of two.
        /// </param>
        /// <param name="mod4">
        /// If true, the returned values are always rounded up to the nearest multiple of 4.
        /// </param>
        public BinarySearch( int min, int max, int fuzziness, bool pot, bool mod4 )
        {
            if ( pot )
            {
                // When POT, the search operates on the exponents.
                // MathUtils.NextPowerOfTwo is assumed to be an external helper.
                _min = ( int )( Math.Log( MathUtils.NextPowerOfTwo( min ) ) / Math.Log( 2 ) );
                _max = ( int )( Math.Log( MathUtils.NextPowerOfTwo( max ) ) / Math.Log( 2 ) );
            }
            else if ( mod4 )
            {
                // When mod4, the search operates on the original values, 
                // adjusting the bounds to the next multiple of 4 if necessary.
                _min = ( min % 4 ) == 0 ? min : ( min + 4 ) - ( min % 4 );
                _max = ( max % 4 ) == 0 ? max : ( max + 4 ) - ( max % 4 );
            }
            else
            {
                // Standard search operates on raw values.
                _min = min;
                _max = max;
            }

            _fuzziness = pot ? 0 : fuzziness; // Fuzziness is usually irrelevant for discrete exponents
            _pot       = pot;
            _mod4      = mod4;
        }

        /// <summary>
        /// Resets the search bounds and calculates the initial value to be tested.
        /// </summary>
        /// <returns>The initial value to test, adjusted for POT or mod4 constraints.</returns>
        public int Reset()
        {
            _low     = _min;
            _high    = _max;
            
            // Correction: Use '>> 1' for a standard integer right shift (equivalent to division by 2)
            _current = ( _low + _high ) >> 1;

            if ( _pot )
            {
                return ( int )Math.Pow( 2, _current );
            }

            if ( _mod4 )
            {
                // Ensure the initial value is a multiple of 4
                return ( _current % 4 ) == 0 ? _current : ( _current + 4 ) - ( _current % 4 );
            }

            return _current;
        }

        /// <summary>
        /// Takes the result of the previous test and calculates the next value to test.
        /// </summary>
        /// <param name="result">
        /// The result of the previous test: <c>true</c> if the previous test value was
        /// acceptable/too low, <c>false</c> if the previous test value was too high.
        /// </param>
        /// <returns>
        /// The next value to test, or -1 if the search is complete or within fuzziness tolerance.
        /// </returns>
        public int Next( bool result )
        {
            // Check if the search is complete
            if ( _low > _high )
            {
                return -1;
            }

            if ( result )
            {
                // Previous value was acceptable/too low, so try higher.
                _low = _current + 1;
            }
            else
            {
                // Previous value was too high, so try lower.
                _high = _current - 1;
            }

            // Recalculate the midpoint
            // Correction: Use '>> 1' for a standard integer right shift (equivalent to division by 2)
            _current = ( _low + _high ) >> 1;
            
            // Check fuzziness tolerance
            if ( Math.Abs( _low - _high ) <= _fuzziness )
            {
                return -1;
            }

            // Return the value based on the constraint type
            if ( _pot )
            {
                return ( int )Math.Pow( 2, _current );
            }

            if ( _mod4 )
            {
                // Ensure the returned value is a multiple of 4
                return ( _current % 4 ) == 0 ? _current : ( _current + 4 ) - ( _current % 4 );
            }

            return _current;
        }
    }
}

// ============================================================================
// ============================================================================
