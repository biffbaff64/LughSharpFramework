// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Circa64 Software Projects / Richard Ikin.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ///////////////////////////////////////////////////////////////////////////////

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using JetBrains.Annotations;
using Rectangle = System.Drawing.Rectangle;

namespace Extensions.Source.Tools.TexturePacker;

[PublicAPI]
[SupportedOSPlatform( "windows" )]
public class ColorBleedEffect
{
    private static readonly int[] Offsets = [ -1, -1, 0, -1, 1, -1, -1, 0, 1, 0, -1, 1, 0, 1, 1, 1 ];

    public Bitmap ProcessImage( Bitmap image, int maxIterations )
    {
        var width  = image.Width;
        var height = image.Height;

        var processedImage = new Bitmap( width, height, PixelFormat.Format32bppArgb );

        var imageData = image.LockBits( new Rectangle( 0, 0, width, height ),
                                        ImageLockMode.ReadOnly,
                                        PixelFormat.Format32bppArgb );

        var processedImageData = processedImage.LockBits( new Rectangle( 0, 0, width, height ),
                                                          ImageLockMode.WriteOnly,
                                                          PixelFormat.Format32bppArgb );

        try
        {
            var rgb = new int[ width * height ];

            Marshal.Copy( imageData.Scan0, rgb, 0, rgb.Length );

            var mask        = new Mask( rgb );
            var iterations  = 0;
            var lastPending = -1;

            while ( ( mask.PendingSize > 0 ) && ( mask.PendingSize != lastPending ) && ( iterations < maxIterations ) )
            {
                lastPending = mask.PendingSize;
                ExecuteIteration( rgb, mask, width, height );
                iterations++;
            }

            Marshal.Copy( rgb, 0, processedImageData.Scan0, rgb.Length );
        }
        finally
        {
            image.UnlockBits( imageData );
            processedImage.UnlockBits( processedImageData );
        }

        return processedImage;
    }

    private static void ExecuteIteration( int[] rgb, Mask mask, int width, int height )
    {
        var iterator = mask.NewMaskIterator();

        while ( iterator.HasNext() )
        {
            var pixelIndex = iterator.Next();
            var x          = pixelIndex % width;
            var y          = pixelIndex / width;
            int r          = 0, g = 0, b = 0;
            var count      = 0;

            for ( int i = 0, n = Offsets.Length; i < n; i += 2 )
            {
                var column = x + Offsets[ i ];
                var row    = y + Offsets[ i + 1 ];

                if ( ( column < 0 ) || ( column >= width ) || ( row < 0 ) || ( row >= height ) )
                {
                    continue;
                }

                var currentPixelIndex = GetPixelIndex( width, column, row );

                if ( !mask.IsBlank( currentPixelIndex ) )
                {
                    var argb = rgb[ currentPixelIndex ];

                    r += Red( argb );
                    g += Green( argb );
                    b += Blue( argb );
                    count++;
                }
            }

            if ( count != 0 )
            {
                rgb[ pixelIndex ] = Argb( 0, r / count, g / count, b / count );
                iterator.MarkAsInProgress();
            }
        }

        iterator.Reset();
    }

    private static int GetPixelIndex( int width, int x, int y )
    {
        return ( y * width ) + x;
    }

    private static int Red( int argb )
    {
        return ( argb >> 16 ) & 0xFF;
    }

    private static int Green( int argb )
    {
        return ( argb >> 8 ) & 0xFF;
    }

    private static int Blue( int argb )
    {
        return ( argb >> 0 ) & 0xFF;
    }

    private static int Argb( int a, int r, int g, int b )
    {
        if ( ( a < 0 ) || ( a > 255 ) || ( r < 0 ) || ( r > 255 ) || ( g < 0 ) || ( g > 255 ) || ( b < 0 ) || ( b > 255 ) )
        {
            throw new ArgumentException( "Invalid RGBA: " + r + ", " + g + "," + b + "," + a );
        }

        return ( ( a & 0xFF ) << 24 ) | ( ( r & 0xFF ) << 16 ) | ( ( g & 0xFF ) << 8 ) | ( ( b & 0xFF ) << 0 );
    }

    // ========================================================================

    private class Mask
    {
        public int PendingSize { get; private set; }

        private readonly bool[] _blank;
        private readonly int[]  _pending;
        private readonly int[]  _changing;
        private          int    _changingSize;

        public Mask( int[] rgb )
        {
            var n = rgb.Length;

            _blank    = new bool[ n ];
            _pending  = new int[ n ];
            _changing = new int[ n ];

            for ( var i = 0; i < n; i++ )
            {
                if ( Alpha( rgb[ i ] ) == 0 )
                {
                    _blank[ i ]             = true;
                    _pending[ PendingSize ] = i;
                    PendingSize++;
                }
            }
        }

        public bool IsBlank( int index )
        {
            return _blank[ index ];
        }

        private int RemoveIndex( int index )
        {
            if ( index >= PendingSize )
            {
                throw new IndexOutOfRangeException( index.ToString() );
            }

            var value = _pending[ index ];

            PendingSize--;
            _pending[ index ] = _pending[ PendingSize ];

            return value;
        }

        public MaskIterator NewMaskIterator()
        {
            return new MaskIterator( this );
        }

        private static int Alpha( int argb )
        {
            return ( argb >> 24 ) & 0xff;
        }

        // ====================================================================

        [PublicAPI]
        public class MaskIterator
        {
            private readonly Mask _mask;
            private          int  _index;

            public MaskIterator( Mask mask )
            {
                _mask = mask;
            }

            public bool HasNext()
            {
                return _index < _mask.PendingSize;
            }

            public int Next()
            {
                if ( _index >= _mask.PendingSize )
                {
                    throw new IndexOutOfRangeException( _index.ToString() );
                }

                return _mask._pending[ _index++ ];
            }

            public void MarkAsInProgress()
            {
                _index--;
                _mask._changing[ _mask._changingSize ] = _mask.RemoveIndex( _index );
                _mask._changingSize++;
            }

            public void Reset()
            {
                _index = 0;

                for ( int i = 0, n = _mask._changingSize; i < n; i++ )
                {
                    _mask._blank[ _mask._changing[ i ] ] = false;
                }

                _mask._changingSize = 0;
            }
        }
    }
}