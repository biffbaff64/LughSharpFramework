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

using System.Runtime.Versioning;

using JetBrains.Annotations;

using LughSharp.Core.Maths;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Collections;
using LughSharp.Core.Utils.Exceptions;

namespace Extensions.Source.Tools.TexturePacker;

[PublicAPI]
[SupportedOSPlatform( "windows" )]
public class MaxRectsPacker : IPacker
{
    private static   TexturePackerSettings     _settings = null!;
    private readonly FreeRectChoiceHeuristic[] _methods;
    private readonly MaxRects                  _maxRects = new();

    // ========================================================================

    private readonly Comparison< TexturePackerRect > _rectComparator = ( o1, o2 ) =>
        string.Compare( TexturePackerRect.GetAtlasName( o1.Name, _settings.FlattenPaths ),
                        TexturePackerRect.GetAtlasName( o2.Name, _settings.FlattenPaths ),
                        StringComparison.Ordinal );

    // ========================================================================

    /// <summary>
    /// Implements a packing algorithm for rectangles based on the MaxRects approach,
    /// which is effective at minimizing wasted space in texture packing. This class
    /// handles the logic for arranging texture regions into rectangular areas, adhering
    /// to the provided packing settings.
    /// </summary>
    public MaxRectsPacker( TexturePackerSettings settings )
    {
        _settings = settings;
        _methods  = Enum.GetValues< FreeRectChoiceHeuristic >();

        if ( settings.MinWidth >= settings.MaxWidth )
        {
            throw new RuntimeException( "Page min width MUST be less than max width." );
        }

        if ( settings.MinHeight >= settings.MaxHeight )
        {
            throw new RuntimeException( "Page min height MUST be less than max height." );
        }
    }

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="inputRects"></param>
    /// <returns></returns>
    public List< TexturePackerPage > Pack( List< TexturePackerRect > inputRects )
    {
        return Pack( null, inputRects );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="inputRects"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public List< TexturePackerPage > Pack( TexturePackerProgressListener? progress,
                                           List< TexturePackerRect > inputRects )
    {
        int n = inputRects.Count;

        for ( var i = 0; i < n; i++ )
        {
            TexturePackerRect rect = inputRects[ i ];

            rect.Width  += _settings.PaddingX;
            rect.Height += _settings.PaddingY;
        }

        if ( _settings.Fast )
        {
            if ( _settings.Rotation )
            {
                // Sort by longest side if rotation is enabled.
                SortUtils.Sort( inputRects,
                                ( o1, o2 ) =>
                                {
                                    int n1 = o1.Width > o1.Height ? o1.Width : o1.Height;
                                    int n2 = o2.Width > o2.Height ? o2.Width : o2.Height;

                                    return n2 - n1;
                                } );
            }
            else
            {
                // Sort only by width (largest to smallest) if rotation is disabled.
                inputRects.Sort( CompareRectsByWidthIfRotationDisabled );

                static int CompareRectsByWidthIfRotationDisabled( TexturePackerRect o1, TexturePackerRect o2 )
                {
                    return o2.Width - o1.Width;
                }
            }
        }

        List< TexturePackerPage > pages = [ ];

        while ( inputRects.Count > 0 )
        {
            if ( progress != null )
            {
                progress.Count = n - inputRects.Count + 1;

                if ( progress.Update( progress.Count, n ) )
                {
                    break;
                }
            }

            TexturePackerPage? result = PackPage( inputRects );

            if ( result != null )
            {
                pages.Add( result );
            }

            inputRects = result?.RemainingRects ?? throw new NullReferenceException();
        }

        return pages;
    }

    /// <summary>
    /// Packs a list of rectangles into a new TexturePackerPage instance while considering
    /// the specified packing settings.
    /// </summary>
    /// <param name="inputRects">The list of rectangles to be packed into a page.</param>
    /// <returns>
    /// Returns a TexturePackerPage instance containing the packed rectangles.
    /// If the packing fails, a fallback empty page may be created and returned.
    /// </returns>
    /// <exception cref="RuntimeException">
    /// Thrown when an error occurs during the packing process, such as invalid settings
    /// or input data.
    /// </exception>
    private TexturePackerPage? PackPage( List< TexturePackerRect > inputRects )
    {
        int paddingX  = _settings.PaddingX;
        int paddingY  = _settings.PaddingY;
        int maxWidth  = _settings.MaxWidth;
        int maxHeight = _settings.MaxHeight;
        var edgePadX  = false;
        var edgePadY  = false;

        if ( _settings.EdgePadding )
        {
            if ( _settings.DuplicatePadding )
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

        // Find the minimum width and height values
        var minWidth  = int.MaxValue;
        var minHeight = int.MaxValue;

        if ( !_settings.Silent )
        {
            Console.Write( "Packing" );
        }

        foreach ( TexturePackerRect rect in inputRects )
        {
            int width  = rect.Width - paddingX;
            int height = rect.Height - paddingY;

            minWidth  = Math.Min( minWidth, width );
            minHeight = Math.Min( minHeight, height );

            if ( _settings.Rotation )
            {
                if ( ( ( width > maxWidth ) || ( height > maxHeight ) )
                  && ( ( width > maxHeight ) || ( height > maxWidth ) ) )
                {
                    string paddingMessage = edgePadX || edgePadY
                        ? $" and edge padding {paddingX} *2, {paddingY} *2"
                        : "";

                    throw new RuntimeException( $"Image does not fit within max page size " +
                                                $"{_settings.MaxWidth}x{_settings.MaxHeight}" +
                                                $"{paddingMessage}: {rect.Name} {width}x{height}" );
                }
            }
            else
            {
                if ( width > maxWidth )
                {
                    string paddingMessage = edgePadX ? $" and X edge padding {_settings.PaddingX} *2" : "";

                    throw new RuntimeException( $"Image does not fit within max page width " +
                                                $"{_settings.MaxWidth}{paddingMessage}: {rect.Name} {width}x{height}" );
                }

                if ( ( height > maxHeight ) && ( !_settings.Rotation || ( width > maxHeight ) ) )
                {
                    string paddingMessage = edgePadY ? $" and Y edge padding {_settings.PaddingY} *2" : "";

                    throw new RuntimeException( $"Image does not fit within max page height " +
                                                $"{_settings.MaxHeight}{paddingMessage}: {rect.Name} {width}x{height}" );
                }
            }
        }

        minWidth  = Math.Max( minWidth, _settings.MinWidth );
        minHeight = Math.Max( minHeight, _settings.MinHeight );

        // BinarySearch uses the max size. Rects are packed with right
        // and top padding, so the max size is increased to match. After
        // packing the padding is subtracted from the page size.

        int adjustX = paddingX;
        int adjustY = paddingY;

        if ( _settings.EdgePadding )
        {
            if ( _settings.DuplicatePadding )
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

        // --------------------------------------------------------------------
        // Find the minimal page size that fits all rects.
        TexturePackerPage? bestResult = null;

        if ( _settings.Square )
        {
            int minSize = Math.Max( minWidth, minHeight );
            int maxSize = Math.Min( _settings.MaxWidth, _settings.MaxHeight );
            var sizeSearch = new BinarySearch( minSize,
                                               maxSize,
                                               _settings.Fast ? 25 : 15,
                                               _settings.PowerOfTwo,
                                               _settings.MultipleOfFour );

            int size = sizeSearch.Reset();
            var i    = 0;

            while ( size != -1 )
            {
                TexturePackerPage? result = PackAtSize( true, size + adjustX, size + adjustY, inputRects );

                if ( !_settings.Silent )
                {
                    if ( ++i % 70 == 0 )
                    {
                        Console.WriteLine();
                    }

                    Console.Write( "." );
                }

                bestResult = GetBest( bestResult, result );
                size       = sizeSearch.Next( result == null );
            }

            if ( !_settings.Silent )
            {
                Console.WriteLine();
            }

            // Rects don't fit on one page. Fill a whole page and return.
            bestResult ??= PackAtSize( false, maxSize + adjustX, maxSize + adjustY, inputRects );

            bestResult?.OutputRects.Sort( _rectComparator );
            bestResult?.Width  = Math.Max( bestResult.Width, bestResult.Height ) - _settings.PaddingX;
            bestResult?.Height = Math.Max( bestResult.Width, bestResult.Height ) - _settings.PaddingY;
        }
        else
        {
            var widthSearch = new BinarySearch( minWidth,
                                                _settings.MaxWidth,
                                                _settings.Fast ? 25 : 15,
                                                _settings.PowerOfTwo,
                                                _settings.MultipleOfFour );

            var heightSearch = new BinarySearch( minHeight,
                                                 _settings.MaxHeight,
                                                 _settings.Fast ? 25 : 15,
                                                 _settings.PowerOfTwo,
                                                 _settings.MultipleOfFour );

            int width  = widthSearch.Reset();
            int height = _settings.Square ? width : heightSearch.Reset();
            var i      = 0;

            while ( true )
            {
                TexturePackerPage? bestWidthResult = null;

                while ( width != -1 )
                {
                    TexturePackerPage? result = PackAtSize( true, width + adjustX, height + adjustY, inputRects );

                    if ( !_settings.Silent )
                    {
                        if ( ++i % 70 == 0 )
                        {
                            Console.WriteLine();
                        }

                        Console.Write( "." );
                    }

                    bestWidthResult = GetBest( bestWidthResult, result );
                    width           = widthSearch.Next( result == null );

                    if ( _settings.Square )
                    {
                        height = width;
                    }
                }

                bestResult = GetBest( bestResult, bestWidthResult );

                if ( _settings.Square )
                {
                    break;
                }

                if ( ( height = heightSearch.Next( bestWidthResult == null ) ) == -1 )
                {
                    break;
                }

                width = widthSearch.Reset();
            }

            if ( !_settings.Silent )
            {
                Console.WriteLine();
            }

            bestResult ??= PackAtSize( false,
                                       _settings.MaxWidth + adjustX,
                                       _settings.MaxHeight + adjustY,
                                       inputRects );

            if ( bestResult != null )
            {
                bestResult.OutputRects.Sort( _rectComparator );
                bestResult.Width  -= paddingX;
                bestResult.Height -= paddingY;
            }
        }

        return bestResult;
    }

    /// <summary>
    /// </summary>
    /// <param name="fully">
    /// If true, the only results that pack all rects will be considered. If false,
    /// all results are considered, not all rects may be packed.
    /// </param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="inputRects"></param>
    private TexturePackerPage? PackAtSize( bool fully, int width, int height, List< TexturePackerRect > inputRects )
    {
        TexturePackerPage? bestResult = null;

        for ( int i = 0, numMethods = _methods.Length; i < numMethods; i++ )
        {
            _maxRects.Init( width, height );

            TexturePackerPage result;

            if ( !_settings.Fast )
            {
                result = _maxRects.Pack( inputRects, _methods[ i ] );
            }
            else
            {
                List< TexturePackerRect > remaining = [ ];

                for ( int ii = 0, nn = inputRects.Count; ii < nn; ii++ )
                {
                    TexturePackerRect rect = inputRects[ ii ];

                    if ( _maxRects.Insert( rect, _methods[ i ] ) == null )
                    {
                        while ( ii < numMethods )
                        {
                            remaining.Add( inputRects[ ii++ ] );
                        }
                    }
                }

                result                = _maxRects.GetResult();
                result.RemainingRects = remaining;
            }

            if ( ( fully && ( result.RemainingRects.Count > 0 ) )
              || ( result.OutputRects.Count == 0 ) )
            {
                continue;
            }

            bestResult = GetBest( bestResult, result );
        }

        return bestResult;
    }

    /// <summary>
    /// Compares two TexturePackerPage objects and returns the one with the higher occupancy value.
    /// </summary>
    /// <param name="result1">The first TexturePackerPage instance to compare.</param>
    /// <param name="result2">The second TexturePackerPage instance to compare.</param>
    /// <returns>
    /// Returns the TexturePackerPage with the higher occupancy value.
    /// If one of the inputs is null, the other non-null instance is returned.
    /// If both are null, returns null.
    /// </returns>
    private static TexturePackerPage? GetBest( TexturePackerPage? result1, TexturePackerPage? result2 )
    {
        // return null if both are null, or returns a non-null result2 if
        // result1 is null and result2 is not null.
        if ( result1 == null )
        {
            return result2;
        }

        // return null if both are null, or returns a non-null result1 if
        // result2 is null and result1 is not null.
        if ( result2 == null )
        {
            return result1;
        }

        // return the result with the higher occupancy value.
        return result1.Occupancy > result2.Occupancy ? result1 : result2;
    }

    // ========================================================================
    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Determines the minimal page size required to accommodate all given rectangles,
    /// considering the specified minimum dimensions and adjustments.
    /// </summary>
    /// <param name="minWidth">The minimum allowable width for the page.</param>
    /// <param name="minHeight">The minimum allowable height for the page.</param>
    /// <param name="adjustX">The horizontal adjustment to apply to the page dimensions.</param>
    /// <param name="adjustY">The vertical adjustment to apply to the page dimensions.</param>
    /// <param name="inputRects">A list of rectangles that need to fit within the page.</param>
    /// <param name="paddingX"></param>
    /// <param name="paddingY"></param>
    /// <returns>
    /// A TexturePackerPage representing the minimal dimensions that can contain all rectangles.
    /// If no suitable configuration is found, returns null.
    /// </returns>
    [Obsolete]
    private TexturePackerPage GetMinimalPageSize( int minWidth, int minHeight,
                                                  int adjustX, int adjustY,
                                                  int paddingX, int paddingY,
                                                  List< TexturePackerRect > inputRects )
    {
        // Find the minimal page size that fits all rects.
        TexturePackerPage? bestResult = null;

        if ( _settings.Square )
        {
            int minSize = Math.Max( minWidth, minHeight );
            int maxSize = Math.Min( _settings.MaxWidth, _settings.MaxHeight );
            var sizeSearch = new BinarySearch( minSize,
                                               maxSize,
                                               _settings.Fast ? 25 : 15,
                                               _settings.PowerOfTwo,
                                               _settings.MultipleOfFour );

            int size = sizeSearch.Reset();

            TexturePackerPage? result;

            for ( var i = 0; size != -1; size = sizeSearch.Next( result == null ) )
            {
                result = PackAtSize( true, size + i, size + adjustY, inputRects );

                bestResult = GetBest( bestResult, result );
            }

            // Rects don't fit on one page. Fill a whole page and return.
            bestResult ??= PackAtSize( false, maxSize + adjustX, maxSize + adjustY, inputRects );

            if ( bestResult != null )
            {
                bestResult.OutputRects.Sort( _rectComparator );
                bestResult.Width  = Math.Max( bestResult.Width, bestResult.Height ) - _settings.PaddingX;
                bestResult.Height = Math.Max( bestResult.Width, bestResult.Height ) - _settings.PaddingY;
            }
        }
        else
        {
            var widthSearch = new BinarySearch( minWidth,
                                                _settings.MaxWidth,
                                                _settings.Fast ? 25 : 15,
                                                _settings.PowerOfTwo,
                                                _settings.MultipleOfFour );

            var heightSearch = new BinarySearch( minHeight,
                                                 _settings.MaxHeight,
                                                 _settings.Fast ? 25 : 15,
                                                 _settings.PowerOfTwo,
                                                 _settings.MultipleOfFour );

            int width  = widthSearch.Reset();
            int height = _settings.Square ? width : heightSearch.Reset();
            var i      = 0;

            while ( true )
            {
                TexturePackerPage? bestWidthResult = null;

                while ( width != -1 )
                {
                    TexturePackerPage? result = PackAtSize( true, width + i, height + adjustY, inputRects );

                    if ( !_settings.Silent )
                    {
                        ++i;

                        if ( ( i % 70 ) == 0 )
                        {
                            Console.WriteLine();
                        }

                        Console.Write( "." );
                    }

                    bestWidthResult = GetBest( bestWidthResult, result );
                    width           = widthSearch.Next( result == null );

                    if ( _settings.Square )
                    {
                        height = width;
                    }
                }

                bestResult = GetBest( bestResult, bestWidthResult );

                if ( _settings.Square )
                {
                    break;
                }

                if ( ( height = heightSearch.Next( bestWidthResult == null ) ) == -1 )
                {
                    break;
                }

                width = widthSearch.Reset();
            }

            if ( !_settings.Silent )
            {
                Console.WriteLine();
            }

            bestResult ??= PackAtSize( false,
                                       _settings.MaxWidth + adjustX,
                                       _settings.MaxHeight + adjustY,
                                       inputRects );

            if ( bestResult != null )
            {
                bestResult.OutputRects.Sort( _rectComparator );
                bestResult.Width  -= paddingX;
                bestResult.Height -= paddingY;
            }
        }

        if ( bestResult == null )
        {
            if ( ( bestResult = PackAtSize( false,
                                            _settings.MaxWidth + adjustX,
                                            _settings.MaxHeight + adjustY,
                                            inputRects ) ) == null )
            {
                // Create a fallback empty page
                bestResult = new TexturePackerPage
                {
                    Width          = _settings.MinWidth,
                    Height         = _settings.MinHeight,
                    OutputRects    = [ ],
                    RemainingRects = [ ..inputRects ]
                };
            }
        }

        bestResult.OutputRects.Sort( _rectComparator );

        // Don't subtract padding if the result is invalid
        if ( ( bestResult.Width > paddingX ) && ( bestResult.Height > paddingY ) )
        {
            bestResult.Width  -= paddingX;
            bestResult.Height -= paddingY;
        }
        else
        {
            bestResult.Width  = Math.Max( _settings.MinWidth, bestResult.Width );
            bestResult.Height = Math.Max( _settings.MinHeight, bestResult.Height );
        }

        return bestResult;
    }

    // ========================================================================
    // ========================================================================

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
        private readonly int _min;

        /// <summary>
        /// The maximum value (or exponent) for the search range.
        /// </summary>
        private readonly int _max;

        /// <summary>
        /// The tolerance for ending the search before 'low' equals 'high'. Only used if not POT.
        /// </summary>
        private readonly int _fuzziness;

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
                _min = ( min % 4 ) == 0 ? min : min + 4 - ( min % 4 );
                _max = ( max % 4 ) == 0 ? max : max + 4 - ( max % 4 );
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
            _low  = _min;
            _high = _max;

            // Correction: Use '>> 1' for a standard integer right shift (equivalent to division by 2)
            _current = ( _low + _high ) >> 1;

            if ( _pot )
            {
                return ( int )Math.Pow( 2, _current );
            }

            if ( _mod4 )
            {
                // Ensure the initial value is a multiple of 4
                return ( _current % 4 ) == 0 ? _current : _current + 4 - ( _current % 4 );
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
                return ( _current % 4 ) == 0 ? _current : _current + 4 - ( _current % 4 );
            }

            return _current;
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Implements the **Maximal Rectangles (MaxRects) bin packing algorithm**. 
    /// This algorithm efficiently finds space for smaller rectangles (textures) within a large 
    /// container (texture atlas) by tracking and splitting the remaining free space.
    /// Adapted from the public domain source by Jukka Jylänki.
    /// </summary>
    private class MaxRects
    {
        private readonly List< TexturePackerRect > _usedRectangles = [ ];
        private readonly List< TexturePackerRect > _freeRectangles = [ ];

        private int _binWidth;
        private int _binHeight;

        // ====================================================================

        /// <summary>
        /// Initializes the MaxRects object by setting the size of the bin (atlas) 
        /// and creating a single free rectangle representing the entire space.
        /// </summary>
        /// <param name="width">The width of the packing bin.</param>
        /// <param name="height">The height of the packing bin.</param>
        public void Init( int width, int height )
        {
            _binWidth  = width;
            _binHeight = height;

            _usedRectangles.Clear();
            _freeRectangles.Clear();

            var n = new TexturePackerRect
            {
                X      = 0,
                Y      = 0,
                Width  = width,
                Height = height
            };

            // Start with one free rectangle covering the entire bin.
            _freeRectangles.Add( n );
        }

        /// <summary>
        /// Inserts a single rectangle into the bin using a specified packing heuristic.
        /// This is the primary method for sequential or external ordering.
        /// </summary>
        /// <param name="rect">The rectangle to be inserted (must contain original width/height).</param>
        /// <param name="method">The heuristic used to determine the best free space to place the rectangle.</param>
        /// <returns>
        /// The placed and scored rectangle with its final coordinates, or <c>null</c> if it cannot be fit.
        /// </returns>
        public TexturePackerRect? Insert( TexturePackerRect rect, FreeRectChoiceHeuristic method )
        {
            // 1. Find the best position using the heuristic
            TexturePackerRect? newNode = ScoreRect( rect, method );

            if ( ( newNode == null ) || ( newNode.Height == 0 ) )
            {
                return null; // Cannot fit
            }

            // 2. Split the free space that contained the new rectangle
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

            // 3. Remove redundant (contained) free rectangles
            PruneFreeList();

            // 4. Finalize and record the placed rectangle
            var bestNode = new TexturePackerRect();
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

        /// <summary>
        /// Packs a list of rectangles by iteratively choosing the one that best fits 
        /// the remaining space according to the given heuristic. This is generally slower
        /// than pre-sorting but may yield better results.
        /// </summary>
        /// <param name="rects">The list of rectangles to be packed.</param>
        /// <param name="method">The heuristic used to choose the best free space.</param>
        /// <returns>A <see cref="TexturePackerPage"/> object containing the packed results and any remaining rectangles.</returns>
        public TexturePackerPage Pack( List< TexturePackerRect > rects, FreeRectChoiceHeuristic method )
        {
            rects = new List< TexturePackerRect >( rects );

            while ( rects.Count > 0 )
            {
                int bestRectIndex = -1;
                var bestNode = new TexturePackerRect
                {
                    Score1 = int.MaxValue,
                    Score2 = int.MaxValue
                };

                // Find the next rectangle that packs best.
                for ( var i = 0; i < rects.Count; i++ )
                {
                    // Score the current rectangle against all free nodes
                    TexturePackerRect? newNode = ScoreRect( rects[ i ], method );

                    // Check if this rectangle/position is better than the current best-found node
                    if ( ( newNode?.Score1 < bestNode.Score1 )
                      || ( ( newNode?.Score1 == bestNode.Score1 ) && ( newNode.Score2 < bestNode.Score2 ) ) )
                    {
                        // Found a new best fit. Copy its data and position.
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

                if ( bestRectIndex == -1 )
                {
                    // No more rectangles fit in the remaining space.
                    break;
                }

                // Place the best-fitting rectangle and update the free list.
                PlaceRect( bestNode );
                rects.RemoveIndex( bestRectIndex );
            }

            TexturePackerPage result = GetResult();
            result.RemainingRects = rects;

            return result;
        }

        /// <summary>
        /// Compiles the final results of the packing process.
        /// </summary>
        /// <returns>A <see cref="TexturePackerPage"/> object with the final dimensions and occupancy.</returns>
        public TexturePackerPage GetResult()
        {
            var width  = 0;
            var height = 0;

            // Find the maximum utilised width and height (not necessarily the bin size).
            foreach ( TexturePackerRect rect in _usedRectangles )
            {
                width  = Math.Max( width, rect.X + rect.Width );
                height = Math.Max( height, rect.Y + rect.Height );
            }

            return new TexturePackerPage
            {
                OutputRects = new List< TexturePackerRect >( _usedRectangles ),
                Occupancy   = GetOccupancyRatio(),
                Width       = width,
                Height      = height
            };
        }

        /// <summary>
        /// Finalises the placement of a rectangle after its position has been determined
        /// by a scoring function. This updates the free list by splitting the space and
        /// pruning redundant nodes.
        /// </summary>
        /// <param name="node">The rectangle node that has been placed.</param>
        private void PlaceRect( TexturePackerRect node )
        {
            int numRectanglesToProcess = _freeRectangles.Count;

            // Split all free nodes that overlap with the new node.
            for ( var i = 0; i < numRectanglesToProcess; i++ )
            {
                if ( SplitFreeNode( _freeRectangles[ i ], node ) )
                {
                    _freeRectangles.RemoveIndex( i );
                    --i;
                    --numRectanglesToProcess;
                }
            }

            // Remove any newly created free nodes that are redundant.
            PruneFreeList();

            // Record the placed rectangle.
            _usedRectangles.Add( node );
        }

        /// <summary>
        /// Finds the best position and orientation for a rectangle using the specified heuristic.
        /// </summary>
        /// <param name="rect">The rectangle to be scored (contains original dimensions).</param>
        /// <param name="method">The free rectangle choice heuristic to use.</param>
        /// <returns>
        /// A <see cref="TexturePackerRect"/> object containing the best found position (X, Y) 
        /// and the heuristic scores (<c>Score1</c>, <c>Score2</c>).
        /// </returns>
        private TexturePackerRect? ScoreRect( TexturePackerRect rect, FreeRectChoiceHeuristic method )
        {
            int  width         = rect.Width;
            int  height        = rect.Height;
            int  rotatedWidth  = rect.Height;
            int  rotatedHeight = rect.Width;
            bool rotate        = rect.CanRotate && _settings.Rotation;

            TexturePackerRect? newNode;

            switch ( method )
            {
                case FreeRectChoiceHeuristic.BestShortSideFit:
                    // Minimize the short side of the wasted space.
                    newNode = FindPositionForNewNodeBestShortSideFit( width,
                                                                      height,
                                                                      rotatedWidth,
                                                                      rotatedHeight,
                                                                      rotate );

                    break;

                case FreeRectChoiceHeuristic.BottomLeftRule:
                    // Minimize Y, then minimize X (the lowest, leftmost position).
                    newNode = FindPositionForNewNodeBottomLeft( width, height, rotatedWidth, rotatedHeight, rotate );

                    break;

                case FreeRectChoiceHeuristic.ContactPointRule:
                    // Maximize the perimeter touch points with used rectangles and bin edges.
                    newNode = FindPositionForNewNodeContactPoint( width, height, rotatedWidth, rotatedHeight, rotate );
                    // Contact Point Rule maximizes the score, while other rules minimize it.
                    newNode.Score1 = -newNode.Score1;

                    break;

                case FreeRectChoiceHeuristic.BestLongSideFit:
                    // Minimize the long side of the wasted space.
                    newNode = FindPositionForNewNodeBestLongSideFit( width,
                                                                     height,
                                                                     rotatedWidth,
                                                                     rotatedHeight,
                                                                     rotate );

                    break;

                case FreeRectChoiceHeuristic.BestAreaFit:
                    // Minimize the area of the wasted free rectangle.
                    newNode = FindPositionForNewNodeBestAreaFit( width, height, rotatedWidth, rotatedHeight, rotate );

                    break;

                default:
                    newNode = null;

                    break;
            }

            // Cannot fit the current rectangle.
            if ( newNode?.Height == 0 )
            {
                newNode.Score1 = int.MaxValue;
                newNode.Score2 = int.MaxValue;
            }

            return newNode;
        }

        /// <summary>
        /// Computes the ratio of the total surface area of all packed rectangles 
        /// to the total area of the bin.
        /// </summary>
        /// <returns>A float between 0 and 1 representing the occupancy ratio.</returns>
        private float GetOccupancyRatio()
        {
            var usedSurfaceArea = 0;

            foreach ( TexturePackerRect rect in _usedRectangles )
            {
                usedSurfaceArea += rect.Width * rect.Height;
            }

            return ( float )usedSurfaceArea / ( _binWidth * _binHeight );
        }

        /// <summary>
        /// Finds the best position using the **Bottom-Left Rule (BL)** heuristic.
        /// This tries to minimize the Y-coordinate of the placed rectangle, and then 
        /// minimize the X-coordinate as a tie-breaker.
        /// </summary>
        /// <param name="width">The width of the rectangle to place.</param>
        /// <param name="height">The height of the rectangle to place.</param>
        /// <param name="rotatedWidth">The rotated width.</param>
        /// <param name="rotatedHeight">The rotated height.</param>
        /// <param name="rotate">If rotation is allowed.</param>
        /// <returns>The <see cref="TexturePackerRect"/> with the best found position and scores.</returns>
        private TexturePackerRect FindPositionForNewNodeBottomLeft( int width,
                                                                    int height,
                                                                    int rotatedWidth,
                                                                    int rotatedHeight,
                                                                    bool rotate )
        {
            var bestNode = new TexturePackerRect
            {
                Score1 = int.MaxValue, // Best Y-coordinate of the top edge (Y + Height)
                Score2 = int.MaxValue  // Best X-coordinate (tie-breaker)
            };

            foreach ( TexturePackerRect rect in _freeRectangles )
            {
                // Try non-rotated
                if ( ( rect.Width >= width ) && ( rect.Height >= height ) )
                {
                    int topSideY = rect.Y + height;

                    if ( ( topSideY < bestNode.Score1 ) ||
                         ( ( topSideY == bestNode.Score1 ) && ( rect.X < bestNode.Score2 ) ) )
                    {
                        bestNode.X       = rect.X;
                        bestNode.Y       = rect.Y;
                        bestNode.Width   = width;
                        bestNode.Height  = height;
                        bestNode.Score1  = topSideY;
                        bestNode.Score2  = rect.X;
                        bestNode.Rotated = false;
                    }
                }

                // Try rotated
                if ( rotate && ( rect.Width >= rotatedWidth ) && ( rect.Height >= rotatedHeight ) )
                {
                    int topSideY = rect.Y + rotatedHeight;

                    if ( ( topSideY < bestNode.Score1 ) ||
                         ( ( topSideY == bestNode.Score1 ) && ( rect.X < bestNode.Score2 ) ) )
                    {
                        bestNode.X       = rect.X;
                        bestNode.Y       = rect.Y;
                        bestNode.Width   = rotatedWidth;
                        bestNode.Height  = rotatedHeight;
                        bestNode.Score1  = topSideY;
                        bestNode.Score2  = rect.X;
                        bestNode.Rotated = true;
                    }
                }
            }

            return bestNode;
        }

        /// <summary>
        /// Finds the best position using the **Best Short-Side Fit (BSSF)** heuristic.
        /// This minimizes the shorter leftover side of the free rectangle after placing the new rectangle.
        /// </summary>
        /// <param name="width">The width of the rectangle to place.</param>
        /// <param name="height">The height of the rectangle to place.</param>
        /// <param name="rotatedWidth">The rotated width.</param>
        /// <param name="rotatedHeight">The rotated height.</param>
        /// <param name="rotate">If rotation is allowed.</param>
        /// <returns>The <see cref="TexturePackerRect"/> with the best found position and scores.</returns>
        private TexturePackerRect FindPositionForNewNodeBestShortSideFit( int width,
                                                                          int height,
                                                                          int rotatedWidth,
                                                                          int rotatedHeight,
                                                                          bool rotate )
        {
            var bestNode = new TexturePackerRect
            {
                Score1 = int.MaxValue // Best Short Side Fit
            };

            foreach ( TexturePackerRect rect in _freeRectangles )
            {
                // Try non-rotated
                if ( ( rect.Width >= width ) && ( rect.Height >= height ) )
                {
                    int leftoverHoriz = Math.Abs( rect.Width - width );
                    int leftoverVert  = Math.Abs( rect.Height - height );
                    int shortSideFit  = Math.Min( leftoverHoriz, leftoverVert );
                    int longSideFit   = Math.Max( leftoverHoriz, leftoverVert ); // Tie-breaker

                    if ( ( shortSideFit < bestNode.Score1 ) ||
                         ( ( shortSideFit == bestNode.Score1 ) && ( longSideFit < bestNode.Score2 ) ) )
                    {
                        bestNode.X       = rect.X;
                        bestNode.Y       = rect.Y;
                        bestNode.Width   = width;
                        bestNode.Height  = height;
                        bestNode.Score1  = shortSideFit;
                        bestNode.Score2  = longSideFit;
                        bestNode.Rotated = false;
                    }
                }

                // Try rotated
                if ( rotate && ( rect.Width >= rotatedWidth ) && ( rect.Height >= rotatedHeight ) )
                {
                    int flippedLeftoverHoriz = Math.Abs( rect.Width - rotatedWidth );
                    int flippedLeftoverVert  = Math.Abs( rect.Height - rotatedHeight );
                    int flippedShortSideFit  = Math.Min( flippedLeftoverHoriz, flippedLeftoverVert );
                    int flippedLongSideFit   = Math.Max( flippedLeftoverHoriz, flippedLeftoverVert ); // Tie-breaker

                    if ( ( flippedShortSideFit < bestNode.Score1 )
                      || ( ( flippedShortSideFit == bestNode.Score1 ) && ( flippedLongSideFit < bestNode.Score2 ) ) )
                    {
                        bestNode.X       = rect.X;
                        bestNode.Y       = rect.Y;
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

        /// <summary>
        /// Finds the best position using the **Best Long-Side Fit (BLSF)** heuristic.
        /// This minimizes the longer leftover side of the free rectangle after placing the new rectangle.
        /// </summary>
        /// <param name="width">The width of the rectangle to place.</param>
        /// <param name="height">The height of the rectangle to place.</param>
        /// <param name="rotatedWidth">The rotated width.</param>
        /// <param name="rotatedHeight">The rotated height.</param>
        /// <param name="rotate">If rotation is allowed.</param>
        /// <returns>The <see cref="TexturePackerRect"/> with the best found position and scores.</returns>
        private TexturePackerRect FindPositionForNewNodeBestLongSideFit( int width,
                                                                         int height,
                                                                         int rotatedWidth,
                                                                         int rotatedHeight,
                                                                         bool rotate )
        {
            var bestNode = new TexturePackerRect
            {
                Score2 = int.MaxValue // Best Long Side Fit
            };

            foreach ( TexturePackerRect rect in _freeRectangles )
            {
                // Try non-rotated
                if ( ( rect.Width >= width ) && ( rect.Height >= height ) )
                {
                    int leftoverHoriz = Math.Abs( rect.Width - width );
                    int leftoverVert  = Math.Abs( rect.Height - height );
                    int shortSideFit  = Math.Min( leftoverHoriz, leftoverVert ); // Tie-breaker
                    int longSideFit   = Math.Max( leftoverHoriz, leftoverVert );

                    if ( ( longSideFit < bestNode.Score2 ) ||
                         ( ( longSideFit == bestNode.Score2 ) && ( shortSideFit < bestNode.Score1 ) ) )
                    {
                        bestNode.X       = rect.X;
                        bestNode.Y       = rect.Y;
                        bestNode.Width   = width;
                        bestNode.Height  = height;
                        bestNode.Score1  = shortSideFit;
                        bestNode.Score2  = longSideFit;
                        bestNode.Rotated = false;
                    }
                }

                // Try rotated
                if ( rotate && ( rect.Width >= rotatedWidth ) && ( rect.Height >= rotatedHeight ) )
                {
                    int leftoverHoriz = Math.Abs( rect.Width - rotatedWidth );
                    int leftoverVert  = Math.Abs( rect.Height - rotatedHeight );
                    int shortSideFit  = Math.Min( leftoverHoriz, leftoverVert ); // Tie-breaker
                    int longSideFit   = Math.Max( leftoverHoriz, leftoverVert );

                    if ( ( longSideFit < bestNode.Score2 ) ||
                         ( ( longSideFit == bestNode.Score2 ) && ( shortSideFit < bestNode.Score1 ) ) )
                    {
                        bestNode.X       = rect.X;
                        bestNode.Y       = rect.Y;
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

        /// <summary>
        /// Finds the best position using the **Best Area Fit (BAF)** heuristic.
        /// This minimizes the wasted area (the area of the free rectangle minus the area of the new rectangle).
        /// </summary>
        /// <param name="width">The width of the rectangle to place.</param>
        /// <param name="height">The height of the rectangle to place.</param>
        /// <param name="rotatedWidth">The rotated width.</param>
        /// <param name="rotatedHeight">The rotated height.</param>
        /// <param name="rotate">If rotation is allowed.</param>
        /// <returns>The <see cref="TexturePackerRect"/> with the best found position and scores.</returns>
        private TexturePackerRect FindPositionForNewNodeBestAreaFit( int width,
                                                                     int height,
                                                                     int rotatedWidth,
                                                                     int rotatedHeight,
                                                                     bool rotate )
        {
            var bestNode = new TexturePackerRect
            {
                Score1 = int.MaxValue // Best Area Fit
            };

            foreach ( TexturePackerRect rect in _freeRectangles )
            {
                int areaFit = ( rect.Width * rect.Height ) - ( width * height );

                // Try non-rotated
                if ( ( rect.Width >= width ) && ( rect.Height >= height ) )
                {
                    int leftoverHoriz = Math.Abs( rect.Width - width );
                    int leftoverVert  = Math.Abs( rect.Height - height );
                    int shortSideFit  = Math.Min( leftoverHoriz, leftoverVert ); // Tie-breaker (often BSSF)

                    if ( ( areaFit < bestNode.Score1 )
                      || ( ( areaFit == bestNode.Score1 ) && ( shortSideFit < bestNode.Score2 ) ) )
                    {
                        bestNode.X       = rect.X;
                        bestNode.Y       = rect.Y;
                        bestNode.Width   = width;
                        bestNode.Height  = height;
                        bestNode.Score2  = shortSideFit;
                        bestNode.Score1  = areaFit;
                        bestNode.Rotated = false;
                    }
                }

                // Try rotated
                if ( rotate && ( rect.Width >= rotatedWidth ) && ( rect.Height >= rotatedHeight ) )
                {
                    int leftoverHoriz = Math.Abs( rect.Width - rotatedWidth );
                    int leftoverVert  = Math.Abs( rect.Height - rotatedHeight );
                    int shortSideFit  = Math.Min( leftoverHoriz, leftoverVert ); // Tie-breaker

                    if ( ( areaFit < bestNode.Score1 )
                      || ( ( areaFit == bestNode.Score1 ) && ( shortSideFit < bestNode.Score2 ) ) )
                    {
                        bestNode.X       = rect.X;
                        bestNode.Y       = rect.Y;
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

        /// <summary>
        /// Calculates the length of the intersection (overlap) between two 1D intervals.
        /// </summary>
        /// <param name="i1Start">Start of interval 1.</param>
        /// <param name="i1End">End of interval 1.</param>
        /// <param name="i2Start">Start of interval 2.</param>
        /// <param name="i2End">End of interval 2.</param>
        /// <returns>The length of the overlap, or 0 if they are disjoint.</returns>
        private static int CommonIntervalLength( int i1Start, int i1End, int i2Start, int i2End )
        {
            if ( ( i1End < i2Start ) || ( i2End < i1Start ) )
            {
                return 0;
            }

            return Math.Min( i1End, i2End ) - Math.Max( i1Start, i2Start );
        }

        /// <summary>
        /// Calculates the **Contact Point Score** for a potential placement of a rectangle.
        /// The score is the total length of the perimeter that touches either the bin edge 
        /// or an already used rectangle.
        /// </summary>
        /// <param name="x">X-coordinate of the proposed placement.</param>
        /// <param name="y">Y-coordinate of the proposed placement.</param>
        /// <param name="width">Width of the rectangle.</param>
        /// <param name="height">Height of the rectangle.</param>
        /// <returns>The contact score (higher is better).</returns>
        private int ContactPointScoreNode( int x, int y, int width, int height )
        {
            var score = 0;

            // 1. Score for touching bin edges
            if ( ( x == 0 ) || ( ( x + width ) == _binWidth ) )
            {
                score += height; // Whole height contributes to the score
            }

            if ( ( y == 0 ) || ( ( y + height ) == _binHeight ) )
            {
                score += width; // Whole width contributes to the score
            }

            // 2. Score for touching used rectangles
            for ( int i = 0, n = _usedRectangles.Count; i < n; i++ )
            {
                TexturePackerRect rect = _usedRectangles[ i ];

                // Check for horizontal contact (left/right sides)
                if ( ( rect.X == ( x + width ) ) || ( ( rect.X + rect.Width ) == x ) )
                {
                    // Add the length of the vertical overlap (common interval)
                    score += CommonIntervalLength( rect.Y, rect.Y + rect.Height, y, y + height );
                }

                // Check for vertical contact (top/bottom sides)
                if ( ( rect.Y == ( y + height ) ) || ( ( rect.Y + rect.Height ) == y ) )
                {
                    // Add the length of the horizontal overlap (common interval)
                    score += CommonIntervalLength( rect.X, rect.X + rect.Width, x, x + width );
                }
            }

            return score;
        }

        /// <summary>
        /// Finds the best position using the **Contact Point Rule (CP)** heuristic.
        /// This maximizes the number of perimeter touch points with used rectangles and bin edges.
        /// </summary>
        /// <param name="width">The width of the rectangle to place.</param>
        /// <param name="height">The height of the rectangle to place.</param>
        /// <param name="rotatedWidth">The rotated width.</param>
        /// <param name="rotatedHeight">The rotated height.</param>
        /// <param name="rotate">If rotation is allowed.</param>
        /// <returns>The <see cref="TexturePackerRect"/> with the best found position and scores.</returns>
        private TexturePackerRect FindPositionForNewNodeContactPoint( int width,
                                                                      int height,
                                                                      int rotatedWidth,
                                                                      int rotatedHeight,
                                                                      bool rotate )
        {
            var bestNode = new TexturePackerRect
            {
                Score1 = -1 // Best contact score (maximizing)
            };

            for ( int i = 0, n = _freeRectangles.Count; i < n; i++ )
            {
                TexturePackerRect free = _freeRectangles[ i ];

                // Try non-rotated
                if ( ( free.Width >= width ) && ( free.Height >= height ) )
                {
                    int score = ContactPointScoreNode( free.X, free.Y, width, height );

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

                // Try rotated
                if ( rotate && ( free.Width >= rotatedWidth ) && ( free.Height >= rotatedHeight ) )
                {
                    int score = ContactPointScoreNode( free.X, free.Y, rotatedWidth, rotatedHeight );

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

        /// <summary>
        /// Splits a single free rectangle into up to four new, smaller free rectangles 
        /// based on the placement of a new used rectangle within it.
        /// This is the core geometric operation of the MaxRects algorithm.
        /// </summary>
        /// <param name="freeNode">The existing free rectangle being considered.</param>
        /// <param name="usedNode">The new rectangle that has been placed.</param>
        /// <returns><c>true</c> if the <paramref name="freeNode"/> was split (i.e., it overlaps and should be removed from the list), otherwise <c>false</c>.</returns>
        private bool SplitFreeNode( TexturePackerRect freeNode, TexturePackerRect? usedNode )
        {
            if ( usedNode == null )
            {
                return false;
            }

            // Test with Separating Axis Theorem (SAT) if the rectangles intersect.
            // If they don't intersect, there's nothing to split.
            if ( ( usedNode.X >= ( freeNode.X + freeNode.Width ) )
              || ( ( usedNode.X + usedNode.Width ) <= freeNode.X )
              || ( usedNode.Y >= ( freeNode.Y + freeNode.Height ) ) ||
                 ( ( usedNode.Y + usedNode.Height ) <= freeNode.Y ) )
            {
                return false;
            }

            // -------------------- Split vertically (creates top/bottom fragments) --------------------
            if ( ( usedNode.X < ( freeNode.X + freeNode.Width ) ) && ( ( usedNode.X + usedNode.Width ) > freeNode.X ) )
            {
                // New node at the top side of the used node.
                if ( ( usedNode.Y > freeNode.Y ) && ( usedNode.Y < ( freeNode.Y + freeNode.Height ) ) )
                {
                    var newNode = new TexturePackerRect( freeNode );
                    newNode.Height = usedNode.Y - newNode.Y;
                    _freeRectangles.Add( newNode );
                }

                // New node at the bottom side of the used node.
                if ( ( usedNode.Y + usedNode.Height ) < ( freeNode.Y + freeNode.Height ) )
                {
                    var newNode = new TexturePackerRect( freeNode )
                    {
                        Y      = usedNode.Y + usedNode.Height,
                        Height = freeNode.Y + freeNode.Height - ( usedNode.Y + usedNode.Height )
                    };

                    _freeRectangles.Add( newNode );
                }
            }

            // -------------------- Split horizontally (creates left/right fragments) --------------------
            if ( ( usedNode.Y < ( freeNode.Y + freeNode.Height ) )
              && ( ( usedNode.Y + usedNode.Height ) > freeNode.Y ) )
            {
                // New node at the left side of the used node.
                if ( ( usedNode.X > freeNode.X ) && ( usedNode.X < ( freeNode.X + freeNode.Width ) ) )
                {
                    var newNode = new TexturePackerRect( freeNode );
                    newNode.Width = usedNode.X - newNode.X;

                    _freeRectangles.Add( newNode );
                }

                // New node at the right side of the used node.
                if ( ( usedNode.X + usedNode.Width ) < ( freeNode.X + freeNode.Width ) )
                {
                    var newNode = new TexturePackerRect( freeNode )
                    {
                        X     = usedNode.X + usedNode.Width,
                        Width = freeNode.X + freeNode.Width - ( usedNode.X + usedNode.Width )
                    };

                    _freeRectangles.Add( newNode );
                }
            }

            return true;
        }

        /// <summary>
        /// Cleans up the free rectangle list by removing any redundant free rectangles 
        /// that are completely contained within another, larger free rectangle.
        /// </summary>
        private void PruneFreeList()
        {
            // O(N^2) cleanup: Go through each pair and remove any redundant rectangle.
            for ( int i = 0, n = _freeRectangles.Count; i < n; i++ )
            {
                for ( int j = i + 1; j < n; ++j )
                {
                    TexturePackerRect rect1 = _freeRectangles[ i ];
                    TexturePackerRect rect2 = _freeRectangles[ j ];

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

        /// <summary>
        /// Returns <c>true</c> if rectangle <c>a</c> is completely contained within 
        /// rectangle <c>b</c>.
        /// </summary>
        /// <param name="a">The potentially contained rectangle.</param>
        /// <param name="b">The container rectangle.</param>
        private static bool IsContainedIn( TexturePackerRect a, TexturePackerRect b )
        {
            return ( a.X >= b.X )
                && ( a.Y >= b.Y )
                && ( ( a.X + a.Width ) <= ( b.X + b.Width ) )
                && ( ( a.Y + a.Height ) <= ( b.Y + b.Height ) );
        }
    }
}

// ============================================================================
// ============================================================================