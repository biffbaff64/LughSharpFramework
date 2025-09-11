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

using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;
using LughSharp.Lugh.Utils.Logging;

namespace Extensions.Source.Tools.TexturePacker;

[PublicAPI]
[SupportedOSPlatform( "windows" )]
public partial class MaxRectsPacker : TexturePacker.IPacker
{
    private static   TexturePackerSettings     _settings = null!;
    private readonly FreeRectChoiceHeuristic[] _methods;
    private readonly MaxRects                  _maxRects = new();

    // ========================================================================

    private readonly Comparison< TexturePacker.Rect > _rectComparator = ( o1, o2 ) =>
        string.Compare( TexturePacker.Rect.GetAtlasName( o1.Name, _settings.FlattenPaths ),
                        TexturePacker.Rect.GetAtlasName( o2.Name, _settings.FlattenPaths ),
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

        if ( settings.MinWidth > settings.MaxWidth )
        {
            throw new GdxRuntimeException( "Page min width cannot be higher than max width." );
        }

        if ( settings.MinHeight > settings.MaxHeight )
        {
            throw new GdxRuntimeException( "Page min height cannot be higher than max height." );
        }
    }

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="inputRects"></param>
    /// <returns></returns>
    public List< TexturePacker.Page? > Pack( List< TexturePacker.Rect > inputRects )
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
    public List< TexturePacker.Page? > Pack( TexturePacker.TexturePackerProgressListener? progress,
                                             List< TexturePacker.Rect > inputRects )
    {
        var n = inputRects.Count;

        for ( var i = 0; i < n; i++ )
        {
            var rect = inputRects[ i ];
            
            if ( !rect.IsPadded )
            {
                rect.Width  += _settings.PaddingX;
                rect.Height += _settings.PaddingY;
                
                rect.IsPadded = true;
            }
        }

        if ( _settings.Fast )
        {
            if ( _settings.Rotation )
            {
                // Sort by longest side if rotation is enabled.
                SortUtils.Sort( inputRects, ( o1, o2 ) =>
                {
                    var n1 = o1.Width > o1.Height ? o1.Width : o1.Height;
                    var n2 = o2.Width > o2.Height ? o2.Width : o2.Height;

                    return n2 - n1;
                } );
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

        List< TexturePacker.Page? > pages = [ ];

        while ( inputRects.Count > 0 )
        {
            if ( progress != null )
            {
                progress.Count = ( n - inputRects.Count ) + 1;

                if ( progress.Update( progress.Count, n ) )
                {
                    break;
                }
            }

            var result = PackPage( inputRects );

//            if ( result != null )
            {
                pages.Add( result );
            }

            inputRects = result?.RemainingRects ?? throw new NullReferenceException();
        }

        return pages;
    }

    /// <summary>
    /// Packs a list of rectangles into a new TexturePacker.Page instance while considering
    /// the specified packing settings.
    /// </summary>
    /// <param name="inputRects">The list of rectangles to be packed into a page.</param>
    /// <returns>
    /// Returns a TexturePacker.Page instance containing the packed rectangles.
    /// If the packing fails, a fallback empty page may be created and returned.
    /// </returns>
    /// <exception cref="GdxRuntimeException">
    /// Thrown when an error occurs during the packing process, such as invalid settings
    /// or input data.
    /// </exception>
    private TexturePacker.Page? PackPage( List< TexturePacker.Rect > inputRects )
    {
        var paddingX  = _settings.PaddingX;
        var paddingY  = _settings.PaddingY;
        var maxWidth  = ( float )_settings.MaxWidth;
        var maxHeight = ( float )_settings.MaxHeight;
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

        for ( int i = 0, numRects = inputRects.Count; i < numRects; ++i )
        {
            var rect   = inputRects[ i ];
            var width  = rect.Width - paddingX;
            var height = rect.Height - paddingY;

            minWidth  = Math.Min( minWidth, width );
            minHeight = Math.Min( minHeight, height );

            if ( _settings.Rotation )
            {
                if ( ( ( width > maxWidth ) || ( height > maxHeight ) )
                     && ( ( width > maxHeight ) || ( height > maxWidth ) ) )
                {
                    var paddingMessage = edgePadX || edgePadY
                        ? $" and edge padding {paddingX} *2, {paddingY} *2"
                        : "";

                    throw new GdxRuntimeException( $"Image does not fit within max page size " +
                                                   $"{_settings.MaxWidth}x{_settings.MaxHeight}" +
                                                   $"{paddingMessage}: {rect.Name} {width}x{height}" );
                }
            }
            else
            {
                if ( width > maxWidth )
                {
                    var paddingMessage = edgePadX ? $" and X edge padding {_settings.PaddingX} *2" : "";

                    throw new GdxRuntimeException( $"Image does not fit within max page width " +
                                                   $"{_settings.MaxWidth}{paddingMessage}: {rect.Name} {width}x{height}" );
                }

                if ( ( height > maxHeight ) && ( !_settings.Rotation || ( width > maxHeight ) ) )
                {
                    var paddingMessage = edgePadY ? $" and Y edge padding {_settings.PaddingY} *2" : "";

                    throw new GdxRuntimeException( $"Image does not fit within max page height " +
                                                   $"{_settings.MaxHeight}{paddingMessage}: {rect.Name} {width}x{height}" );
                }
            }
        }

        minWidth  = Math.Max( minWidth, _settings.MinWidth );
        minHeight = Math.Max( minHeight, _settings.MinHeight );

        var adjustX = paddingX;
        var adjustY = paddingY;

        // BinarySearch uses the max size. Rects are packed with right
        // and top padding, so the max size is increased to match. After
        // packing the padding is subtracted from the page size.

        if ( _settings.EdgePadding )
        {
            if ( _settings.DuplicatePadding )
            {
                adjustX = paddingX - paddingX; // ????
                adjustY = paddingY - paddingY; // ????
            }
            else
            {
                adjustX = paddingX - paddingX * 2; // ????
                adjustY = paddingY - paddingY * 2; // ????
            }
        }

        // --------------------------------------------------------------------
        // Find the minimal page size that fits all rects.
        TexturePacker.Page? bestResult = null;

        if ( _settings.Square )
        {
            var minSize = Math.Max( minWidth, minHeight );
            var maxSize = Math.Min( _settings.MaxWidth, _settings.MaxHeight );
            var sizeSearch = new BinarySearch( minSize,
                                               maxSize,
                                               _settings.Fast ? 25 : 15,
                                               _settings.PowerOfTwo,
                                               _settings.MultipleOfFour );

            var size = sizeSearch.Reset();

            TexturePacker.Page? result;

            for ( var i = 0; size != -1; size = sizeSearch.Next( result == null ) )
            {
                result = PackAtSize( true, size + i, size + adjustY, inputRects );

                bestResult = GetBest( bestResult, result );
            }

            // Rects don't fit on one page. Fill a whole page and return.
            if ( bestResult == null )
            {
                bestResult = PackAtSize( false, maxSize + adjustX, maxSize + adjustY, inputRects );
            }

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

            var width  = widthSearch.Reset();
            var height = _settings.Square ? width : heightSearch.Reset();
            
            while ( true )
            {
                var bestWidthResult = new TexturePacker.Page();

                while ( width != -1 )
                {
                    var result = PackAtSize( true, width + adjustX, height + adjustY, inputRects );

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

            if ( bestResult == null )
            {
                bestResult = PackAtSize( false,
                                         _settings.MaxWidth + adjustX,
                                         _settings.MaxHeight + adjustY,
                                         inputRects );
            }

            if ( bestResult != null )
            {
                bestResult.OutputRects.Sort( _rectComparator );
                bestResult.Width  -= paddingX;
                bestResult.Height -= paddingY;
            }
        }

        // --------------------------------------------------------------------

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
    private TexturePacker.Page? PackAtSize( bool fully, int width, int height, List< TexturePacker.Rect > inputRects )
    {
        var bestResult = new TexturePacker.Page();

        for ( int i = 0, numMethods = _methods.Length; i < numMethods; i++ )
        {
            _maxRects.Init( width, height );

            TexturePacker.Page result;

            if ( !_settings.Fast )
            {
                result = _maxRects.Pack( inputRects, _methods[ i ] );
            }
            else
            {
                List< TexturePacker.Rect > remaining = [ ];

                for ( int j = 0, nn = inputRects.Count; j < nn; j++ )
                {
                    var rect = inputRects[ j ];

                    if ( _maxRects.Insert( rect, _methods[ i ] ) == null )
                    {
                        remaining.Add( rect );
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
    /// Compares two TexturePacker.Page objects and returns the one with the higher occupancy value.
    /// </summary>
    /// <param name="result1">The first TexturePacker.Page instance to compare.</param>
    /// <param name="result2">The second TexturePacker.Page instance to compare.</param>
    /// <returns>
    /// Returns the TexturePacker.Page with the higher occupancy value.
    /// If one of the inputs is null, the other non-null instance is returned.
    /// If both are null, returns null.
    /// </returns>
    private static TexturePacker.Page? GetBest( TexturePacker.Page? result1, TexturePacker.Page? result2 )
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
    /// A TexturePacker.Page representing the minimal dimensions that can contain all rectangles.
    /// If no suitable configuration is found, returns null.
    /// </returns>
    [Obsolete]
    private TexturePacker.Page GetMinimalPageSize( int minWidth, int minHeight,
                                                   int adjustX, int adjustY,
                                                   int paddingX, int paddingY,
                                                   List< TexturePacker.Rect > inputRects )
    {
        // Find the minimal page size that fits all rects.
        TexturePacker.Page? bestResult = null;

        if ( _settings.Square )
        {
            var minSize = Math.Max( minWidth, minHeight );
            var maxSize = Math.Min( _settings.MaxWidth, _settings.MaxHeight );
            var sizeSearch = new BinarySearch( minSize,
                                               maxSize,
                                               _settings.Fast ? 25 : 15,
                                               _settings.PowerOfTwo,
                                               _settings.MultipleOfFour );

            var size = sizeSearch.Reset();

            TexturePacker.Page? result;

            for ( var i = 0; size != -1; size = sizeSearch.Next( result == null ) )
            {
                result = PackAtSize( true, size + i, size + adjustY, inputRects );

                bestResult = GetBest( bestResult, result );
            }

            // Rects don't fit on one page. Fill a whole page and return.
            if ( bestResult == null )
            {
                bestResult = PackAtSize( false, maxSize + adjustX, maxSize + adjustY, inputRects );
            }

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

            var width  = widthSearch.Reset();
            var height = _settings.Square ? width : heightSearch.Reset();
            var i      = 0;

            while ( true )
            {
                TexturePacker.Page? bestWidthResult = null;

                while ( width != -1 )
                {
                    var result = PackAtSize( true, width + i, height + adjustY, inputRects );

                    if ( !_settings.Silent )
                    {
                        ++i;

                        if ( i % 70 == 0 )
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

            if ( bestResult == null )
            {
                bestResult = PackAtSize( false,
                                         _settings.MaxWidth + adjustX,
                                         _settings.MaxHeight + adjustY,
                                         inputRects );
            }

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
                bestResult = new TexturePacker.Page
                {
                    Width          = _settings.MinWidth,
                    Height         = _settings.MinHeight,
                    OutputRects    = [ ],
                    RemainingRects = [ ..inputRects ],
                };
            }
        }

        bestResult.OutputRects.Sort( _rectComparator );

        // Don't subtract padding if the result is invalid
        if ( bestResult.Width > paddingX && bestResult.Height > paddingY )
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
}