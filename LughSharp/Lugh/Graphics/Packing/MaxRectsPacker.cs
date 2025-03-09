// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin
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
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Collections;
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.Packing;

[PublicAPI]
public class MaxRectsPacker : TexturePacker.IPacker
{
    private static   TexturePacker.Settings    _settings = null!;
    private readonly FreeRectChoiceHeuristic[] _methods;
    private readonly MaxRects                  _maxRects = new();
    private readonly SortUtils                 _sort     = new();

    // ========================================================================
    
    private readonly Comparison< TexturePacker.Rect > rectComparator = ( o1, o2 ) =>
        string.Compare( TexturePacker.Rect.GetAtlasName( o1.Name, _settings.flattenPaths ),
                        TexturePacker.Rect.GetAtlasName( o2.Name, _settings.flattenPaths ),
                        StringComparison.Ordinal );

    // ========================================================================

    public MaxRectsPacker( TexturePacker.Settings settings )
    {
        _settings = settings;
        _methods  = Enum.GetValues< FreeRectChoiceHeuristic >();

        if ( settings.minWidth > settings.maxWidth )
        {
            throw new GdxRuntimeException( "Page min width cannot be higher than max width." );
        }

        if ( settings.minHeight > settings.maxHeight )
        {
            throw new GdxRuntimeException( "Page min height cannot be higher than max height." );
        }
    }

    // ========================================================================

    public List< TexturePacker.Page > Pack( List< TexturePacker.Rect > inputRects )
    {
        return Pack( null, inputRects );
    }

    public List< TexturePacker.Page > Pack( TexturePacker.ProgressListener? progress, List< TexturePacker.Rect > inputRects )
    {
        var n = inputRects.Count;

        for ( var i = 0; i < n; i++ )
        {
            var rect = inputRects[ i ];
            rect.Width  += _settings.paddingX;
            rect.Height += _settings.paddingY;
        }

        if ( _settings.fast )
        {
            if ( _settings.rotation )
            {
                // Sort by longest side if rotation is enabled.
                inputRects.Sort( CompareRectsByMaxDimensionDescending );

                static int CompareRectsByMaxDimensionDescending( TexturePacker.Rect o1, TexturePacker.Rect o2 )
                {
                    var n1 = Math.Max( o1.Width, o1.Height );
                    var n2 = Math.Max( o2.Width, o2.Height );

                    return n2 - n1;
                }
            }
            else
            {
                // Sort only by width (largest to smallest) if rotation is disabled.
                inputRects.Sort( CompareRectsByWidthIfRotationDisabled );

                static int CompareRectsByWidthIfRotationDisabled( TexturePacker.Rect o1, TexturePacker.Rect o2 )
                {
                    return o2.Width - o1.Width;
                }
            }
        }

        List< TexturePacker.Page > pages = [ ];

        while ( inputRects?.Count > 0 )
        {
            progress.Count = ( n - inputRects.Count ) + 1;

            if ( progress.Update( progress.Count, n ) ) break;

            var result = PackPage( inputRects );
            pages.Add( result );
            inputRects = result.RemainingRects;
        }

        return pages;
    }

    private TexturePacker.Page PackPage( List< TexturePacker.Rect > inputRects )
    {
        var   paddingX  = _settings.paddingX;
        var   paddingY  = _settings.paddingY;
        float maxWidth  = _settings.maxWidth;
        float maxHeight = _settings.maxHeight;
        var   edgePadX  = false;
        var   edgePadY  = false;

        if ( _settings.edgePadding )
        {
            if ( _settings.duplicatePadding )
            {
                maxWidth  -= paddingX;
                maxHeight -= paddingY;
            }
            else
            {
                maxWidth  -= paddingX * 2;
                maxHeight -= paddingY * 2;
            }

            edgePadX = paddingX > 0;
            edgePadY = paddingY > 0;
        }

        // Find min size.
        var minWidth  = int.MaxValue;
        var minHeight = int.MaxValue;

        for ( int i = 0, nn = inputRects.Count; i < nn; i++ )
        {
            var rect   = inputRects[ i ];
            var width  = rect.Width - paddingX;
            var height = rect.Height - paddingY;
            minWidth  = Math.Min( minWidth, width );
            minHeight = Math.Min( minHeight, height );

            if ( _settings.rotation )
            {
                if ( ( ( width > maxWidth ) || ( height > maxHeight ) ) && ( ( width > maxHeight ) || ( height > maxWidth ) ) )
                {
                    var paddingMessage = ( edgePadX || edgePadY ) ? ( " and edge padding " + paddingX + "*2," + paddingY + "*2" ) : "";

                    throw new GdxRuntimeException( "Image does not fit within max page size " + _settings.maxWidth + "x"
                                                   + _settings.maxHeight + paddingMessage + ": " + rect.Name + " " + width + "x" + height );
                }
            }
            else
            {
                if ( width > maxWidth )
                {
                    var paddingMessage = edgePadX ? ( " and X edge padding " + paddingX + "*2" ) : "";

                    throw new GdxRuntimeException( "Image does not fit within max page width " + _settings.maxWidth + paddingMessage + ": "
                                                   + rect.Name + " " + width + "x" + height );
                }

                if ( ( height > maxHeight ) && ( !_settings.rotation || ( width > maxHeight ) ) )
                {
                    var paddingMessage = edgePadY ? ( " and Y edge padding " + paddingY + "*2" ) : "";

                    throw new GdxRuntimeException( "Image does not fit within max page height " + _settings.maxHeight + paddingMessage
                                                   + ": " + rect.Name + " " + width + "x" + height );
                }
            }
        }

        minWidth  = Math.Max( minWidth, _settings.minWidth );
        minHeight = Math.Max( minHeight, _settings.minHeight );

        // BinarySearch uses the max size. Rects are packed with right and top padding, so the max size is increased to match.
        // After packing the padding is subtracted from the page size.
        int adjustX = paddingX, adjustY = paddingY;

        if ( _settings.edgePadding )
        {
            if ( _settings.duplicatePadding )
            {
                adjustX -= paddingX;
                adjustY -= paddingY;
            }
            else
            {
                adjustX -= paddingX * 2;
                adjustY -= paddingY * 2;
            }
        }

        if ( !_settings.silent ) Console.WriteLine( "Packing" );

        // Find the minimal page size that fits all rects.
        TexturePacker.Page bestResult = null;

        if ( _settings.square )
        {
            int minSize = Math.Max( minWidth, minHeight );
            int maxSize = Math.Min( _settings.maxWidth, _settings.maxHeight );
            var sizeSearch = new BinarySearch( minSize, maxSize, _settings.fast ? 25 : 15, _settings.pot,
                                               _settings.multipleOfFour );
            int size = sizeSearch.Reset(), i = 0;

            while ( size != -1 )
            {
                TexturePacker.Page result = PackAtSize( true, size + adjustX, size + adjustY, inputRects );

                if ( !_settings.silent )
                {
                    if ( ( ++i % 70 ) == 0 ) Console.WriteLine();
                    Console.WriteLine( "." );
                }

                bestResult = GetBest( bestResult, result );
                size       = sizeSearch.Next( result == null );
            }

            if ( !_settings.silent ) Console.WriteLine();

            // Rects don't fit on one page. Fill a whole page and return.
            if ( bestResult == null )
            {
                bestResult = PackAtSize( false, maxSize + adjustX, maxSize + adjustY, inputRects );
            }

            _sort.Sort( bestResult.OutputRects, rectComparator );
            bestResult.Width  = Math.Max( bestResult.Width, bestResult.Height ) - paddingX;
            bestResult.Height = Math.Max( bestResult.Width, bestResult.Height ) - paddingY;

            return bestResult;
        }
        else
        {
            var widthSearch = new BinarySearch( minWidth, _settings.maxWidth, _settings.fast ? 25 : 15, _settings.pot,
                                                _settings.multipleOfFour );
            var heightSearch = new BinarySearch( minHeight, _settings.maxHeight, _settings.fast ? 25 : 15, _settings.pot,
                                                 _settings.multipleOfFour );
            int width  = widthSearch.Reset(), i = 0;
            var height = _settings.square ? width : heightSearch.Reset();

            while ( true )
            {
                TexturePacker.Page bestWidthResult = null;

                while ( width != -1 )
                {
                    TexturePacker.Page result = PackAtSize( true, width + adjustX, height + adjustY, inputRects );

                    if ( !_settings.silent )
                    {
                        if ( ( ++i % 70 ) == 0 ) Console.WriteLine();
                        Console.WriteLine( "." );
                    }

                    bestWidthResult = GetBest( bestWidthResult, result );
                    width           = widthSearch.Next( result == null );

                    if ( _settings.square )
                    {
                        height = width;
                    }
                }

                bestResult = GetBest( bestResult, bestWidthResult );

                if ( _settings.square ) break;

                height = heightSearch.Next( bestWidthResult == null );

                if ( height == -1 ) break;

                width = widthSearch.Reset();
            }

            if ( !_settings.silent ) Console.WriteLine();

            // Rects don't fit on one page. Fill a whole page and return.
            if ( bestResult == null )
            {
                bestResult = PackAtSize( false, _settings.maxWidth + adjustX, _settings.maxHeight + adjustY, inputRects );
            }

            _sort.Sort( bestResult.OutputRects, rectComparator );
            bestResult.Width  -= paddingX;
            bestResult.Height -= paddingY;

            return bestResult;
        }
    }

    /** @param fully If true, the only results that pack all rects will be considered. If false, all results are considered, not
     *           all rects may be packed. */
    private TexturePacker.Page? PackAtSize( bool fully, int width, int height, List< TexturePacker.Rect > inputRects )
    {
        TexturePacker.Page? bestResult = null;

        for ( int i = 0, n = _methods.Length; i < n; i++ )
        {
            _maxRects.Init( width, height );
            TexturePacker.Page result;

            if ( !_settings.fast )
            {
                result = _maxRects.Pack( inputRects, _methods[ i ] );
            }
            else
            {
                List< TexturePacker.Rect > remaining = [ ];

                for ( int ii = 0, nn = inputRects.Count; ii < nn; ii++ )
                {
                    TexturePacker.Rect rect = inputRects[ ii ];

                    if ( _maxRects.Insert( rect, _methods[ i ] ) == null )
                    {
                        while ( ii < nn )
                        {
                            remaining.Add( inputRects[ ii++ ] );
                        }
                    }
                }

                result                = _maxRects.GetResult();
                result.RemainingRects = remaining;
            }

            if ( ( fully && ( result.RemainingRects?.Count > 0 ) )
                || ( result.OutputRects?.Count == 0 ) )
            {
                continue;
            }

            bestResult = GetBest( bestResult, result );
        }

        return bestResult;
    }

    private TexturePacker.Page? GetBest( TexturePacker.Page? result1, TexturePacker.Page? result2 )
    {
        if ( result1 == null ) return result2;
        if ( result2 == null ) return result1;

        return result1.Occupancy > result2.Occupancy ? result1 : result2;
    }

    public class BinarySearch
    {
        private readonly bool _pot;
        private readonly bool _mod4;
        private readonly int  _min;
        private readonly int  _max;
        private readonly int  _fuzziness;
        private          int  _low;
        private          int  _high;
        private          int  _current;

        public BinarySearch( int min, int max, int fuzziness, bool pot, bool mod4 )
        {
            if ( pot )
            {
                this._min = ( int )( Math.Log( MathUtils.NextPowerOfTwo( min ) ) / Math.Log( 2 ) );
                this._max = ( int )( Math.Log( MathUtils.NextPowerOfTwo( max ) ) / Math.Log( 2 ) );
            }
            else if ( mod4 )
            {
                this._min = ( min % 4 ) == 0 ? min : ( min + 4 ) - ( min % 4 );
                this._max = ( max % 4 ) == 0 ? max : ( max + 4 ) - ( max % 4 );
            }
            else
            {
                this._min = min;
                this._max = max;
            }

            this._fuzziness = pot ? 0 : fuzziness;
            this._pot       = pot;
            this._mod4      = mod4;
        }

        public int Reset()
        {
            _low     = _min;
            _high    = _max;
            _current = ( _low + _high ) >>> 1;

            if ( _pot ) return ( int )Math.Pow( 2, _current );
            if ( _mod4 ) return ( _current % 4 ) == 0 ? _current : ( _current + 4 ) - ( _current % 4 );

            return _current;
        }

        public int Next( bool result )
        {
            if ( _low >= _high ) return -1;

            if ( result )
                _low = _current + 1;
            else
                _high = _current - 1;
            _current = ( _low + _high ) >>> 1;

            if ( Math.Abs( _low - _high ) < _fuzziness ) return -1;
            if ( _pot ) return ( int )Math.Pow( 2, _current );
            if ( _mod4 ) return ( _current % 4 ) == 0 ? _current : ( _current + 4 ) - ( _current % 4 );

            return _current;
        }
    }

    /// <summary>
    /// Maximal rectangles bin packing algorithm. Adapted from this C++ public domain source:
    /// http://clb.demon.fi/projects/even-more-rectangle-bin-packing
    /// </summary>
    private class MaxRects
    {
        private          int                        _binWidth;
        private          int                        _binHeight;
        private readonly List< TexturePacker.Rect > _usedRectangles = [ ];
        private readonly List< TexturePacker.Rect > _freeRectangles = [ ];

        public void Init( int width, int height )
        {
            _binWidth  = width;
            _binHeight = height;

            _usedRectangles.Clear();
            _freeRectangles.Clear();

            var n = new TexturePacker.Rect
            {
                X      = 0,
                Y      = 0,
                Width  = width,
                Height = height,
            };

            _freeRectangles.Add( n );
        }

        /** Packs a single image. Order is defined externally. */
        public TexturePacker.Rect? Insert( TexturePacker.Rect rect, FreeRectChoiceHeuristic method )
        {
            var newNode = ScoreRect( rect, method );

            if ( newNode.Height == 0 ) return null;

            int numRectanglesToProcess = _freeRectangles.Count;

            for ( var i = 0; i < numRectanglesToProcess; ++i )
            {
                if ( SplitFreeNode( _freeRectangles[ i ], newNode ) )
                {
                    _freeRectangles.RemoveIndex( i );
                    --i;
                    --numRectanglesToProcess;
                }
            }

            PruneFreeList();

            var bestNode = new TexturePacker.Rect();
            bestNode.Set( rect );
            bestNode.Score1  = newNode.Score1;
            bestNode.Score2  = newNode.Score2;
            bestNode.X       = newNode.X;
            bestNode.Y       = newNode.Y;
            bestNode.Width   = newNode.Width;
            bestNode.Height  = newNode.Height;
            bestNode.Rotated = newNode.Rotated;

            _usedRectangles.Add( bestNode );

            return bestNode;
        }

        /** For each rectangle, packs each one then chooses the best and packs that. Slow! */
        public TexturePacker.Page Pack( List< TexturePacker.Rect > rects, FreeRectChoiceHeuristic method )
        {
            rects = new List< TexturePacker.Rect >( rects ); //TODO: ??

            while ( rects.Count > 0 )
            {
                var bestRectIndex = -1;
                var bestNode      = new TexturePacker.Rect();
                bestNode.Score1 = int.MaxValue;
                bestNode.Score2 = int.MaxValue;

                // Find the next rectangle that packs best.
                for ( var i = 0; i < rects.Count; i++ )
                {
                    var newNode = ScoreRect( rects[ i ], method );

                    if ( ( newNode.Score1 < bestNode.Score1 )
                         || ( ( newNode.Score1 == bestNode.Score1 ) && ( newNode.Score2 < bestNode.Score2 ) ) )
                    {
                        bestNode.Set( rects[ i ] );
                        bestNode.Score1  = newNode.Score1;
                        bestNode.Score2  = newNode.Score2;
                        bestNode.X       = newNode.X;
                        bestNode.Y       = newNode.Y;
                        bestNode.Width   = newNode.Width;
                        bestNode.Height  = newNode.Height;
                        bestNode.Rotated = newNode.Rotated;
                        bestRectIndex    = i;
                    }
                }

                if ( bestRectIndex == -1 ) break;

                PlaceRect( bestNode );
                rects.RemoveIndex( bestRectIndex );
            }

            TexturePacker.Page result = GetResult();
            result.RemainingRects = rects;

            return result;
        }

        public TexturePacker.Page GetResult()
        {
            int w = 0, h = 0;

            for ( var i = 0; i < _usedRectangles.Count; i++ )
            {
                TexturePacker.Rect rect = _usedRectangles[ i ];
                w = Math.Max( w, rect.X + rect.Width );
                h = Math.Max( h, rect.Y + rect.Height );
            }

            TexturePacker.Page result = new TexturePacker.Page();
            result.OutputRects = [ ];
            result.Occupancy   = GetOccupancy();
            result.Width       = w;
            result.Height      = h;

            return result;
        }

        private void PlaceRect( TexturePacker.Rect node )
        {
            int numRectanglesToProcess = _freeRectangles.Count;

            for ( var i = 0; i < numRectanglesToProcess; i++ )
            {
                if ( SplitFreeNode( _freeRectangles[ i ], node ) )
                {
                    _freeRectangles.RemoveIndex( i );
                    --i;
                    --numRectanglesToProcess;
                }
            }

            PruneFreeList();

            _usedRectangles.Add( node );
        }

        private TexturePacker.Rect ScoreRect( TexturePacker.Rect rect, FreeRectChoiceHeuristic method )
        {
            int width         = rect.Width;
            int height        = rect.Height;
            var rotatedWidth  = ( height - _settings.paddingY ) + _settings.paddingX;
            var rotatedHeight = ( width - _settings.paddingX ) + _settings.paddingY;
            var rotate        = rect.CanRotate && _settings.rotation;

            TexturePacker.Rect newNode = null;

            switch ( method )
            {
                case FreeRectChoiceHeuristic.BestShortSideFit:
                    newNode = FindPositionForNewNodeBestShortSideFit( width, height, rotatedWidth, rotatedHeight, rotate );

                    break;

                case FreeRectChoiceHeuristic.BottomLeftRule:
                    newNode = FindPositionForNewNodeBottomLeft( width, height, rotatedWidth, rotatedHeight, rotate );

                    break;

                case FreeRectChoiceHeuristic.ContactPointRule:
                    newNode        = FindPositionForNewNodeContactPoint( width, height, rotatedWidth, rotatedHeight, rotate );
                    newNode.Score1 = -newNode.Score1; // Reverse since we are minimizing, but for contact point score bigger is better.

                    break;

                case FreeRectChoiceHeuristic.BestLongSideFit:
                    newNode = FindPositionForNewNodeBestLongSideFit( width, height, rotatedWidth, rotatedHeight, rotate );

                    break;

                case FreeRectChoiceHeuristic.BestAreaFit:
                    newNode = FindPositionForNewNodeBestAreaFit( width, height, rotatedWidth, rotatedHeight, rotate );

                    break;
            }

            // Cannot fit the current rectangle.
            if ( newNode.Height == 0 )
            {
                newNode.Score1 = int.MaxValue;
                newNode.Score2 = int.MaxValue;
            }

            return newNode;
        }

        // / Computes the ratio of used surface area.
        private float GetOccupancy()
        {
            var usedSurfaceArea = 0;

            for ( var i = 0; i < _usedRectangles.Count; i++ )
            {
                usedSurfaceArea += _usedRectangles[ i ].Width * _usedRectangles[ i ].Height;
            }

            return ( float )usedSurfaceArea / ( _binWidth * _binHeight );
        }

        private TexturePacker.Rect FindPositionForNewNodeBottomLeft( int width, int height, int rotatedWidth, int rotatedHeight,
                                                                     bool rotate )
        {
            var bestNode = new TexturePacker.Rect();

            bestNode.Score1 = int.MaxValue; // best y, _score2 is best x

            for ( var i = 0; i < _freeRectangles.Count; i++ )
            {
                // Try to place the rectangle in upright (non-rotated) orientation.
                if ( ( _freeRectangles[ i ].Width >= width ) && ( _freeRectangles[ i ].Height >= height ) )
                {
                    var topSideY = _freeRectangles[ i ].Y + height;

                    if ( ( topSideY < bestNode.Score1 ) ||
                         ( ( topSideY == bestNode.Score1 ) && ( _freeRectangles[ i ].X < bestNode.Score2 ) ) )
                    {
                        bestNode.X       = _freeRectangles[ i ].X;
                        bestNode.Y       = _freeRectangles[ i ].Y;
                        bestNode.Width   = width;
                        bestNode.Height  = height;
                        bestNode.Score1  = topSideY;
                        bestNode.Score2  = _freeRectangles[ i ].X;
                        bestNode.Rotated = false;
                    }
                }

                if ( rotate && ( _freeRectangles[ i ].Width >= rotatedWidth ) && ( _freeRectangles[ i ].Height >= rotatedHeight ) )
                {
                    var topSideY = _freeRectangles[ i ].Y + rotatedHeight;

                    if ( ( topSideY < bestNode.Score1 ) ||
                         ( ( topSideY == bestNode.Score1 ) && ( _freeRectangles[ i ].X < bestNode.Score2 ) ) )
                    {
                        bestNode.X       = _freeRectangles[ i ].X;
                        bestNode.Y       = _freeRectangles[ i ].Y;
                        bestNode.Width   = rotatedWidth;
                        bestNode.Height  = rotatedHeight;
                        bestNode.Score1  = topSideY;
                        bestNode.Score2  = _freeRectangles[ i ].X;
                        bestNode.Rotated = true;
                    }
                }
            }

            return bestNode;
        }

        private TexturePacker.Rect FindPositionForNewNodeBestShortSideFit( int width,
                                                                           int height,
                                                                           int rotatedWidth,
                                                                           int rotatedHeight,
                                                                           bool rotate )
        {
            var bestNode = new TexturePacker.Rect();
            bestNode.Score1 = int.MaxValue;

            for ( var i = 0; i < _freeRectangles.Count; i++ )
            {
                // Try to place the rectangle in upright (non-rotated) orientation.
                if ( ( _freeRectangles[ i ].Width >= width ) && ( _freeRectangles[ i ].Height >= height ) )
                {
                    int leftoverHoriz = Math.Abs( _freeRectangles[ i ].Width - width );
                    int leftoverVert  = Math.Abs( _freeRectangles[ i ].Height - height );
                    int shortSideFit  = Math.Min( leftoverHoriz, leftoverVert );
                    int longSideFit   = Math.Max( leftoverHoriz, leftoverVert );

                    if ( ( shortSideFit < bestNode.Score1 ) ||
                         ( ( shortSideFit == bestNode.Score1 ) && ( longSideFit < bestNode.Score2 ) ) )
                    {
                        bestNode.X       = _freeRectangles[ i ].X;
                        bestNode.Y       = _freeRectangles[ i ].Y;
                        bestNode.Width   = width;
                        bestNode.Height  = height;
                        bestNode.Score1  = shortSideFit;
                        bestNode.Score2  = longSideFit;
                        bestNode.Rotated = false;
                    }
                }

                if ( rotate && ( _freeRectangles[ i ].Width >= rotatedWidth ) && ( _freeRectangles[ i ].Height >= rotatedHeight ) )
                {
                    int flippedLeftoverHoriz = Math.Abs( _freeRectangles[ i ].Width - rotatedWidth );
                    int flippedLeftoverVert  = Math.Abs( _freeRectangles[ i ].Height - rotatedHeight );
                    int flippedShortSideFit  = Math.Min( flippedLeftoverHoriz, flippedLeftoverVert );
                    int flippedLongSideFit   = Math.Max( flippedLeftoverHoriz, flippedLeftoverVert );

                    if ( ( flippedShortSideFit < bestNode.Score1 )
                         || ( ( flippedShortSideFit == bestNode.Score1 ) && ( flippedLongSideFit < bestNode.Score2 ) ) )
                    {
                        bestNode.X       = _freeRectangles[ i ].X;
                        bestNode.Y       = _freeRectangles[ i ].Y;
                        bestNode.Width   = rotatedWidth;
                        bestNode.Height  = rotatedHeight;
                        bestNode.Score1  = flippedShortSideFit;
                        bestNode.Score2  = flippedLongSideFit;
                        bestNode.Rotated = true;
                    }
                }
            }

            return bestNode;
        }

        private TexturePacker.Rect FindPositionForNewNodeBestLongSideFit( int width,
                                                                          int height,
                                                                          int rotatedWidth,
                                                                          int rotatedHeight,
                                                                          bool rotate )
        {
            var bestNode = new TexturePacker.Rect();

            bestNode.Score2 = int.MaxValue;

            for ( var i = 0; i < _freeRectangles.Count; i++ )
            {
                // Try to place the rectangle in upright (non-rotated) orientation.
                if ( ( _freeRectangles[ i ].Width >= width ) && ( _freeRectangles[ i ].Height >= height ) )
                {
                    int leftoverHoriz = Math.Abs( _freeRectangles[ i ].Width - width );
                    int leftoverVert  = Math.Abs( _freeRectangles[ i ].Height - height );
                    int shortSideFit  = Math.Min( leftoverHoriz, leftoverVert );
                    int longSideFit   = Math.Max( leftoverHoriz, leftoverVert );

                    if ( ( longSideFit < bestNode.Score2 ) ||
                         ( ( longSideFit == bestNode.Score2 ) && ( shortSideFit < bestNode.Score1 ) ) )
                    {
                        bestNode.X       = _freeRectangles[ i ].X;
                        bestNode.Y       = _freeRectangles[ i ].Y;
                        bestNode.Width   = width;
                        bestNode.Height  = height;
                        bestNode.Score1  = shortSideFit;
                        bestNode.Score2  = longSideFit;
                        bestNode.Rotated = false;
                    }
                }

                if ( rotate && ( _freeRectangles[ i ].Width >= rotatedWidth ) && ( _freeRectangles[ i ].Height >= rotatedHeight ) )
                {
                    int leftoverHoriz = Math.Abs( _freeRectangles[ i ].Width - rotatedWidth );
                    int leftoverVert  = Math.Abs( _freeRectangles[ i ].Height - rotatedHeight );
                    int shortSideFit  = Math.Min( leftoverHoriz, leftoverVert );
                    int longSideFit   = Math.Max( leftoverHoriz, leftoverVert );

                    if ( ( longSideFit < bestNode.Score2 ) ||
                         ( ( longSideFit == bestNode.Score2 ) && ( shortSideFit < bestNode.Score1 ) ) )
                    {
                        bestNode.X       = _freeRectangles[ i ].X;
                        bestNode.Y       = _freeRectangles[ i ].Y;
                        bestNode.Width   = rotatedWidth;
                        bestNode.Height  = rotatedHeight;
                        bestNode.Score1  = shortSideFit;
                        bestNode.Score2  = longSideFit;
                        bestNode.Rotated = true;
                    }
                }
            }

            return bestNode;
        }

        private TexturePacker.Rect FindPositionForNewNodeBestAreaFit( int width,
                                                                      int height,
                                                                      int rotatedWidth,
                                                                      int rotatedHeight,
                                                                      bool rotate )
        {
            var bestNode = new TexturePacker.Rect();

            bestNode.Score1 = int.MaxValue; // best area fit, _score2 is best short side fit

            for ( var i = 0; i < _freeRectangles.Count; i++ )
            {
                var areaFit = ( _freeRectangles[ i ].Width * _freeRectangles[ i ].Height ) - ( width * height );

                // Try to place the rectangle in upright (non-rotated) orientation.
                if ( ( _freeRectangles[ i ].Width >= width ) && ( _freeRectangles[ i ].Height >= height ) )
                {
                    int leftoverHoriz = Math.Abs( _freeRectangles[ i ].Width - width );
                    int leftoverVert  = Math.Abs( _freeRectangles[ i ].Height - height );
                    int shortSideFit  = Math.Min( leftoverHoriz, leftoverVert );

                    if ( ( areaFit < bestNode.Score1 ) || ( ( areaFit == bestNode.Score1 ) && ( shortSideFit < bestNode.Score2 ) ) )
                    {
                        bestNode.X       = _freeRectangles[ i ].X;
                        bestNode.Y       = _freeRectangles[ i ].Y;
                        bestNode.Width   = width;
                        bestNode.Height  = height;
                        bestNode.Score2  = shortSideFit;
                        bestNode.Score1  = areaFit;
                        bestNode.Rotated = false;
                    }
                }

                if ( rotate && ( _freeRectangles[ i ].Width >= rotatedWidth ) && ( _freeRectangles[ i ].Height >= rotatedHeight ) )
                {
                    int leftoverHoriz = Math.Abs( _freeRectangles[ i ].Width - rotatedWidth );
                    int leftoverVert  = Math.Abs( _freeRectangles[ i ].Height - rotatedHeight );
                    int shortSideFit  = Math.Min( leftoverHoriz, leftoverVert );

                    if ( ( areaFit < bestNode.Score1 ) || ( ( areaFit == bestNode.Score1 ) && ( shortSideFit < bestNode.Score2 ) ) )
                    {
                        bestNode.X       = _freeRectangles[ i ].X;
                        bestNode.Y       = _freeRectangles[ i ].Y;
                        bestNode.Width   = rotatedWidth;
                        bestNode.Height  = rotatedHeight;
                        bestNode.Score2  = shortSideFit;
                        bestNode.Score1  = areaFit;
                        bestNode.Rotated = true;
                    }
                }
            }

            return bestNode;
        }

        // / Returns 0 if the two intervals i1 and i2 are disjoint, or the length of their overlap otherwise.
        private int CommonIntervalLength( int i1start, int i1end, int i2start, int i2end )
        {
            if ( ( i1end < i2start ) || ( i2end < i1start ) ) return 0;

            return Math.Min( i1end, i2end ) - Math.Max( i1start, i2start );
        }

        private int ContactPointScoreNode( int x, int y, int width, int height )
        {
            var score = 0;

            if ( ( x == 0 ) || ( ( x + width ) == _binWidth ) ) score   += height;
            if ( ( y == 0 ) || ( ( y + height ) == _binHeight ) ) score += width;

            for ( int i = 0, n = _usedRectangles.Count; i < n; i++ )
            {
                TexturePacker.Rect rect = _usedRectangles[ i ];

                if ( ( rect.X == ( x + width ) ) || ( ( rect.X + rect.Width ) == x ) )
                {
                    score += CommonIntervalLength( rect.Y, rect.Y + rect.Height, y, y + height );
                }

                if ( ( rect.Y == ( y + height ) ) || ( ( rect.Y + rect.Height ) == y ) )
                {
                    score += CommonIntervalLength( rect.X, rect.X + rect.Width, x, x + width );
                }
            }

            return score;
        }

        private TexturePacker.Rect FindPositionForNewNodeContactPoint( int width,
                                                                       int height,
                                                                       int rotatedWidth,
                                                                       int rotatedHeight,
                                                                       bool rotate )
        {
            var bestNode = new TexturePacker.Rect();
            bestNode.Score1 = -1; // best contact score

            for ( int i = 0, n = _freeRectangles.Count; i < n; i++ )
            {
                // Try to place the rectangle in upright (non-rotated) orientation.
                TexturePacker.Rect free = _freeRectangles[ i ];

                if ( ( free.Width >= width ) && ( free.Height >= height ) )
                {
                    var score = ContactPointScoreNode( free.X, free.Y, width, height );

                    if ( score > bestNode.Score1 )
                    {
                        bestNode.X       = free.X;
                        bestNode.Y       = free.Y;
                        bestNode.Width   = width;
                        bestNode.Height  = height;
                        bestNode.Score1  = score;
                        bestNode.Rotated = false;
                    }
                }

                if ( rotate && ( free.Width >= rotatedWidth ) && ( free.Height >= rotatedHeight ) )
                {
                    var score = ContactPointScoreNode( free.X, free.Y, rotatedWidth, rotatedHeight );

                    if ( score > bestNode.Score1 )
                    {
                        bestNode.X       = free.X;
                        bestNode.Y       = free.Y;
                        bestNode.Width   = rotatedWidth;
                        bestNode.Height  = rotatedHeight;
                        bestNode.Score1  = score;
                        bestNode.Rotated = true;
                    }
                }
            }

            return bestNode;
        }

        private bool SplitFreeNode( TexturePacker.Rect freeNode, TexturePacker.Rect usedNode )
        {
            // Test with SAT if the rectangles even intersect.
            if ( ( usedNode.X >= ( freeNode.X + freeNode.Width ) ) || ( ( usedNode.X + usedNode.Width ) <= freeNode.X )
                                                                   || ( usedNode.Y >= ( freeNode.Y + freeNode.Height ) ) ||
                                                                   ( ( usedNode.Y + usedNode.Height ) <= freeNode.Y ) ) return false;

            if ( ( usedNode.X < ( freeNode.X + freeNode.Width ) ) && ( ( usedNode.X + usedNode.Width ) > freeNode.X ) )
            {
                // New node at the top side of the used node.
                if ( ( usedNode.Y > freeNode.Y ) && ( usedNode.Y < ( freeNode.Y + freeNode.Height ) ) )
                {
                    var newNode = new TexturePacker.Rect( freeNode );
                    newNode.Height = usedNode.Y - newNode.Y;
                    _freeRectangles.Add( newNode );
                }

                // New node at the bottom side of the used node.
                if ( ( usedNode.Y + usedNode.Height ) < ( freeNode.Y + freeNode.Height ) )
                {
                    var newNode = new TexturePacker.Rect( freeNode );
                    newNode.Y      = usedNode.Y + usedNode.Height;
                    newNode.Height = ( freeNode.Y + freeNode.Height ) - ( usedNode.Y + usedNode.Height );
                    _freeRectangles.Add( newNode );
                }
            }

            if ( ( usedNode.Y < ( freeNode.Y + freeNode.Height ) ) && ( ( usedNode.Y + usedNode.Height ) > freeNode.Y ) )
            {
                // New node at the left side of the used node.
                if ( ( usedNode.X > freeNode.X ) && ( usedNode.X < ( freeNode.X + freeNode.Width ) ) )
                {
                    var newNode = new TexturePacker.Rect( freeNode );
                    newNode.Width = usedNode.X - newNode.X;
                    _freeRectangles.Add( newNode );
                }

                // New node at the right side of the used node.
                if ( ( usedNode.X + usedNode.Width ) < ( freeNode.X + freeNode.Width ) )
                {
                    var newNode = new TexturePacker.Rect( freeNode );
                    newNode.X     = usedNode.X + usedNode.Width;
                    newNode.Width = ( freeNode.X + freeNode.Width ) - ( usedNode.X + usedNode.Width );
                    _freeRectangles.Add( newNode );
                }
            }

            return true;
        }

        private void PruneFreeList()
        {
            // Go through each pair and remove any rectangle that is redundant.
            for ( int i = 0, n = _freeRectangles.Count; i < n; i++ )
            {
                for ( var j = i + 1; j < n; ++j )
                {
                    TexturePacker.Rect rect1 = _freeRectangles[ i ];
                    TexturePacker.Rect rect2 = _freeRectangles[ j ];

                    if ( IsContainedIn( rect1, rect2 ) )
                    {
                        _freeRectangles.RemoveIndex( i );
                        --i;
                        --n;

                        break;
                    }

                    if ( IsContainedIn( rect2, rect1 ) )
                    {
                        _freeRectangles.RemoveIndex( j );
                        --j;
                        --n;
                    }
                }
            }
        }

        private static bool IsContainedIn( TexturePacker.Rect a, TexturePacker.Rect b )
        {
            return ( a.X >= b.X )
                   && ( a.Y >= b.Y )
                   && ( ( a.X + a.Width ) <= ( b.X + b.Width ) )
                   && ( ( a.Y + a.Height ) <= ( b.Y + b.Height ) );
        }
    }

    // ========================================================================

    public enum FreeRectChoiceHeuristic
    {
        /** BSSF: Positions the rectangle against the short side of a free rectangle into which it fits the best. */
        BestShortSideFit,

        /** BLSF: Positions the rectangle against the long side of a free rectangle into which it fits the best. */
        BestLongSideFit,

        /** BAF: Positions the rectangle into the smallest free rect into which it fits. */
        BestAreaFit,

        /** BL: Does the Tetris placement. */
        BottomLeftRule,

        /** CP: Choosest the placement where the rectangle touches other rects as much as possible. */
        ContactPointRule
    };
}