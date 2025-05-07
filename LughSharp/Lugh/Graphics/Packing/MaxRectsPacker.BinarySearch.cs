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

using LughSharp.Lugh.Maths;

namespace LughSharp.Lugh.Graphics.Packing;

public partial class MaxRectsPacker
{
    [PublicAPI]
    public class BinarySearch
    {
        private readonly bool _pot;
        private readonly bool _mod4;
        private readonly int  _min;
        private readonly int  _max;
        private readonly int  _fuzziness;

        private int _low;
        private int _high;
        private int _current;

        // ====================================================================
        
        public BinarySearch( int min, int max, int fuzziness, bool pot, bool mod4 )
        {
            if ( pot )
            {
                _min = ( int )( Math.Log( MathUtils.NextPowerOfTwo( min ) ) / Math.Log( 2 ) );
                _max = ( int )( Math.Log( MathUtils.NextPowerOfTwo( max ) ) / Math.Log( 2 ) );
            }
            else if ( mod4 )
            {
                _min = ( min % 4 ) == 0 ? min : ( min + 4 ) - ( min % 4 );
                _max = ( max % 4 ) == 0 ? max : ( max + 4 ) - ( max % 4 );
            }
            else
            {
                _min = min;
                _max = max;
            }

            _fuzziness = pot ? 0 : fuzziness;
            _pot       = pot;
            _mod4      = mod4;
        }

        public int Reset()
        {
            _low     = _min;
            _high    = _max;
            _current = ( _low + _high ) >>> 1;

            if ( _pot )
            {
                return ( int )Math.Pow( 2, _current );
            }

            if ( _mod4 )
            {
                return ( _current % 4 ) == 0 ? _current : ( _current + 4 ) - ( _current % 4 );
            }

            return _current;
        }

        public int Next( bool result )
        {
            if ( _low >= _high )
            {
                return -1;
            }

            if ( result )
            {
                _low = _current + 1;
            }
            else
            {
                _high = _current - 1;
            }

            _current = ( _low + _high ) >>> 1;

            if ( Math.Abs( _low - _high ) < _fuzziness )
            {
                return -1;
            }

            if ( _pot )
            {
                return ( int )Math.Pow( 2, _current );
            }

            if ( _mod4 )
            {
                return ( _current % 4 ) == 0 ? _current : ( _current + 4 ) - ( _current % 4 );
            }

            return _current;
        }
    }
}