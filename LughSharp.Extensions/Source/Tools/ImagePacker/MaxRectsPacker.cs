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

namespace Extensions.Source.Tools.ImagePacker;

[PublicAPI]
[SupportedOSPlatform( "windows" )]
public partial class MaxRectsPacker : TexturePacker.IPacker
{
    private static   TexturePacker.Settings    _settings = null!;
    private readonly FreeRectChoiceHeuristic[] _methods;
    private readonly MaxRects                  _maxRects = new();

    // ========================================================================

    private readonly Comparison< TexturePacker.Rect > _rectComparator = ( o1, o2 ) =>
        string.Compare( TexturePacker.Rect.GetAtlasName( o1.Name, _settings.FlattenPaths ),
                        TexturePacker.Rect.GetAtlasName( o2.Name, _settings.FlattenPaths ),
                        StringComparison.Ordinal );

    // ========================================================================

    public MaxRectsPacker( TexturePacker.Settings settings )
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
    public List< TexturePacker.Page > Pack( List< TexturePacker.Rect > inputRects )
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
    public List< TexturePacker.Page > Pack( TexturePacker.AbstractProgressListener? progress, List< TexturePacker.Rect > inputRects )
    {
        var n = inputRects.Count;

        for ( var i = 0; i < n; i++ )
        {
            var rect = inputRects[ i ];
            rect.Width  += _settings.PaddingX;
            rect.Height += _settings.PaddingY;
        }

        if ( _settings.Fast )
        {
            if ( _settings.Rotation )
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

        while ( inputRects.Count > 0 )
        {
            if ( progress != null )
            {
                progress.Count = ( n - inputRects.Count ) + 1;

                if ( ( bool )progress.Update( progress.Count, n ) )
                {
                    break;
                }
            }

            var result = PackPage( inputRects );

            pages.Add( result );

            Logger.Debug( "Page added" );
            Logger.Debug( $"pages.Count: {pages.Count}" );
            
            inputRects = result.RemainingRects ?? throw new NullReferenceException();
        }

        return pages;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="inputRects"></param>
    /// <returns></returns>
    /// <exception cref="GdxRuntimeException"></exception>
    private TexturePacker.Page PackPage( List< TexturePacker.Rect > inputRects )
    {
        float maxWidth  = _settings.MaxWidth;
        float maxHeight = _settings.MaxHeight;
        var   edgePadX  = false;
        var   edgePadY  = false;

        if ( _settings.EdgePadding )
        {
            if ( _settings.DuplicatePadding )
            {
                maxWidth  -= _settings.PaddingX;
                maxHeight -= _settings.PaddingY;
            }
            else
            {
                maxWidth  -= _settings.PaddingX * 2;
                maxHeight -= _settings.PaddingY * 2;
            }

            edgePadX = _settings.PaddingX > 0;
            edgePadY = _settings.PaddingY > 0;
        }

        // Find min size.
        var minWidth  = int.MaxValue;
        var minHeight = int.MaxValue;

        for ( int i = 0, nn = inputRects.Count; i < nn; i++ )
        {
            var rect   = inputRects[ i ];
            var width  = rect.Width - _settings.PaddingX;
            var height = rect.Height - _settings.PaddingY;

            minWidth  = Math.Min( minWidth, width );
            minHeight = Math.Min( minHeight, height );

            if ( _settings.Rotation )
            {
                if ( ( ( width > maxWidth ) || ( height > maxHeight ) ) && ( ( width > maxHeight ) || ( height > maxWidth ) ) )
                {
                    var paddingMessage = edgePadX || edgePadY
                        ? $" and edge padding {_settings.PaddingX} *2, {_settings.PaddingY} *2"
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

        // BinarySearch uses the max size. Rects are packed with right and top padding, so the max
        // size is increased to match. After packing the padding is subtracted from the page size.
        var adjustX = _settings.PaddingX;
        var adjustY = _settings.PaddingY;

        if ( _settings.EdgePadding )
        {
            if ( _settings.DuplicatePadding )
            {
                adjustX -= _settings.PaddingX;
                adjustY -= _settings.PaddingY;
            }
            else
            {
                adjustX -= _settings.PaddingX * 2;
                adjustY -= _settings.PaddingY * 2;
            }
        }

        if ( !_settings.Silent )
        {
            Logger.Debug( "Packing" );
        }

        var bestResult = GetMinimalPageSize( minWidth, minHeight, adjustX, adjustY, inputRects );

        Logger.Debug( $"Best result: {bestResult.ImageName}" );
        Logger.Debug( $"Best result: {bestResult.Width}x{bestResult.Height}" );
        Logger.Debug( $"Best result: {bestResult.OutputRects?.Count} rects" );
        
        return bestResult;
    }

    /// <summary>
    /// Determines the minimal page size required to accommodate all given rectangles,
    /// considering the specified minimum dimensions and adjustments.
    /// </summary>
    /// <param name="minWidth">The minimum allowable width for the page.</param>
    /// <param name="minHeight">The minimum allowable height for the page.</param>
    /// <param name="adjustX">The horizontal adjustment to apply to the page dimensions.</param>
    /// <param name="adjustY">The vertical adjustment to apply to the page dimensions.</param>
    /// <param name="inputRects">A list of rectangles that need to fit within the page.</param>
    /// <returns>
    /// A TexturePacker.Page representing the minimal dimensions that can contain all rectangles.
    /// If no suitable configuration is found, returns null.
    /// </returns>
    private TexturePacker.Page GetMinimalPageSize( int minWidth, int minHeight,
                                                   int adjustX, int adjustY,
                                                   List< TexturePacker.Rect > inputRects )
    {
        Logger.Debug( $"minWidth: {minWidth}, minHeight: {minHeight}" );
        Logger.Debug( $"adjustX: {adjustX}, adjustY: {adjustY}" );
        Logger.Debug( $"inputRects.Count: {inputRects.Count}" );

        foreach ( var rect in inputRects )
        {
            Logger.Debug( $"rect: {rect.Name} {rect.Width}x{rect.Height}" );
        }
        
        // Find the minimal page size that fits all rects.
        TexturePacker.Page? bestResult = null;

        Logger.Debug( $"_settings.Square: {_settings.Square}" );

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

            Logger.Debug( $"minSize: {minSize}, maxSize: {maxSize}" );
            Logger.Debug( $"size: {size}" );
            
            while ( size != -1 )
            {
                var result = PackAtSize( true, size + adjustX, size + adjustY, inputRects );

                bestResult = GetBest( bestResult, result );
                size       = sizeSearch.Next( result == null );
            }

            if ( !_settings.Silent )
            {
                Logger.NewLine();
            }

            // Rects don't fit on one page. Fill a whole page and return.
            if ( bestResult == null )
            {
                bestResult = PackAtSize( false, maxSize + adjustX, maxSize + adjustY, inputRects );
            }

            SortUtils.Sort( bestResult?.OutputRects, _rectComparator );

            Guard.WarnIfNull( bestResult );

            bestResult!.Width = Math.Max( bestResult.Width, bestResult.Height ) - _settings.PaddingX;
            bestResult.Height = Math.Max( bestResult.Width, bestResult.Height ) - _settings.PaddingY;
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

            Logger.Debug( $"width: {width}x{height}" );
            
            while ( true )
            {
                var bestWidthResult = new TexturePacker.Page();

                while ( width != -1 )
                {
                    var result = PackAtSize( true, width + adjustX, height + adjustY, inputRects );
                    
                    result?.Debug();
                    
                    bestWidthResult = GetBest( bestWidthResult, result );
                    width           = widthSearch.Next( result == null );

                    if ( _settings.Square )
                    {
                        height = width;
                    }
                }

                Logger.Debug( $"bestResult: {bestResult}" );
                bestResult?.Debug(); 
                Logger.Debug( $"bestWidthResult: {bestWidthResult}" );
                bestWidthResult?.Debug();
                
                bestResult = GetBest( bestResult, bestWidthResult );

                if ( _settings.Square )
                {
                    break;
                }

                height = heightSearch.Next( bestWidthResult == null );

                if ( height == -1 )
                {
                    break;
                }

                width = widthSearch.Reset();
            }

            if ( !_settings.Silent )
            {
                Logger.NewLine();
            }

            if ( bestResult == null )
            {
                Logger.Debug( "Rects don't fit on one page. Fill a whole page and return." );

                bestResult = PackAtSize( false, _settings.MaxWidth + adjustX, _settings.MaxHeight + adjustY, inputRects );

                if ( bestResult == null )
                {
                    Logger.Warning( "PackAtSize() FAILED to initialise bestResult!" );
                }
            }

            SortUtils.Sort( bestResult!.OutputRects, _rectComparator );

            bestResult.Width  -= _settings.PaddingX;
            bestResult.Height -= _settings.PaddingY;
        }

        Logger.NewLine();

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

        for ( int i = 0, n = _methods.Length; i < n; i++ )
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

                for ( int ii = 0, nn = inputRects.Count; ii < nn; ii++ )
                {
                    var rect = inputRects[ ii ];

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
//        Logger.Debug( $"result1: {result1?.ImageName}" );
//        Logger.Debug( $"result2: {result2?.ImageName}" );
//        Logger.Debug( $"result1.OutputRects.Count: {result1?.OutputRects?.Count}" );
//        Logger.Debug( $"result2.OutputRects.Count: {result2?.OutputRects?.Count}" );
//        Logger.Debug( $"result1.RemainingRects.Count: {result1?.RemainingRects?.Count}" );
//        Logger.Debug( $"result2.RemainingRects.Count: {result2?.RemainingRects?.Count}" );
//        Logger.Debug( $"result1.Width: {result1?.Width}" );
//        Logger.Debug( $"result2.Width: {result2?.Width}" );
//        Logger.Debug( $"result1.Height: {result1?.Height}" );
//        Logger.Debug( $"result2.Height: {result2?.Height}" );
//        Logger.Debug( $"result1.Occupancy: {result1?.Occupancy}" );
//        Logger.Debug( $"result2.Occupancy: {result2?.Occupancy}" );
//        Logger.Divider();

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
}