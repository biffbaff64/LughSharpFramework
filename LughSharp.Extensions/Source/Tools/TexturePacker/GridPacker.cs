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

namespace Extensions.Source.Tools.TexturePacker;

[PublicAPI]
[SupportedOSPlatform( "windows" )]
public class GridPacker( TexturePackerSettings settings ) : TexturePacker.IPacker
{
    /// <summary>
    /// Performs a grid packing of the input rects without a progress listener.
    /// </summary>
    /// <param name="inputRects">The list of rectangles to pack.</param>
    /// <returns>A list of packed pages.</returns>
    public List< TexturePacker.Page > Pack( List< TexturePacker.Rect > inputRects )
    {
        // Pass null safely to the main Pack method.
        return Pack( null, inputRects );
    }

    /// <summary>
    /// Performs a grid packing of the input rects.
    /// </summary>
    /// <param name="progress">The progress listener to report packing status. Can be null.</param>
    /// <param name="inputRects">The list of rectangles to pack.</param>
    /// <returns>A list of packed pages.</returns>
    public List< TexturePacker.Page > Pack( TexturePacker.TexturePackerProgressListener? progress,
                                            List< TexturePacker.Rect > inputRects )
    {
        // Check for null only if the progress listener is actually used.
        // We'll trust the user to handle the null checks inside the loop or caller if needed.

        // Rects are packed with right and top padding, so the max size is increased
        // to match. After packing the padding is subtracted from the page size.
        var paddingX = settings.PaddingX;
        var paddingY = settings.PaddingY;

        // Resetting adjustX/Y to a sane default of padding before applying edge rules.
        var adjustX = paddingX;
        var adjustY = paddingY;

        // NOTE: The padding adjustment logic here is preserved from the original code,
        // but it is mathematically complex and may be incorrect for generic grid padding.
        if ( settings.EdgePadding )
        {
            if ( settings.DuplicatePadding )
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

        var maxWidth   = settings.MaxWidth + adjustX;
        var maxHeight  = settings.MaxHeight + adjustY;
        var numRects   = inputRects.Count;
        var cellWidth  = 0;
        var cellHeight = 0;

        // 1. Determine the maximum required cell size.
        for ( var i = 0; i < numRects; i++ )
        {
            var rect = inputRects[ i ];

            cellWidth  = Math.Max( cellWidth, rect.Width );
            cellHeight = Math.Max( cellHeight, rect.Height );
        }

        // 2. Add padding to define the final cell size.
        cellWidth  += paddingX;
        cellHeight += paddingY;

        List< TexturePacker.Page > pages          = [ ];
        List< TexturePacker.Rect > remainingRects = new( inputRects );

        while ( remainingRects.Count > 0 )
        {
            if ( progress != null )
            {
                progress.Count = ( numRects - remainingRects.Count ) + 1;

                if ( progress.Update( progress.Count, numRects ) )
                {
                    break;
                }
            }

            // Pack the page and get the list of rects that were successfully placed.
            var (page, packedRects) = PackPage( remainingRects, cellWidth, cellHeight, maxWidth, maxHeight );

            // Update the list of remaining rectangles by excluding those that were packed.
            remainingRects = remainingRects.Except( packedRects ).ToList();

            // Subtract the padding from the final page size.
            page.Width  -= paddingX;
            page.Height -= paddingY;

            pages.Add( page );
        }

        return pages;
    }

    /// <summary>
    /// Packs one page (atlas) using the calculated cell dimensions.
    /// </summary>
    /// <param name="inputRects">The list of rectangles that need to be packed.</param>
    /// <param name="cellWidth">The fixed width of each grid cell (includes padding).</param>
    /// <param name="cellHeight">The fixed height of each grid cell (includes padding).</param>
    /// <param name="maxWidth">The maximum allowed width for the page (adjusted for padding).</param>
    /// <param name="maxHeight">The maximum allowed height for the page (adjusted for padding).</param>
    /// <returns>A tuple containing the packed page object and the list of rectangles that were successfully packed.</returns>
    private (TexturePacker.Page page, List< TexturePacker.Rect > packedRects) PackPage(
        List< TexturePacker.Rect > inputRects,
        int cellWidth,
        int cellHeight,
        int maxWidth,
        int maxHeight )
    {
        TexturePacker.Page page = new()
        {
            OutputRects = [ ],
        };
        List< TexturePacker.Rect > packedRects = [ ];

        var n = inputRects.Count;
        var x = 0;
        var y = 0;

        // Iterate forward through the list for O(1) tracking.
        for ( var i = 0; i < n; i++ )
        {
            var rect = inputRects[ i ];

            // Check if the next cell position exceeds the max width.
            if ( ( x + cellWidth ) > maxWidth )
            {
                // Move to the next row.
                y += cellHeight;

                // Check if the current row exceeds the max height.
                if ( y > ( maxHeight - cellHeight ) )
                {
                    break; // Page is full.
                }

                // Reset X for the new row.
                x = 0;
            }

            // Assign position and size (with padding).
            rect.X = x;
            rect.Y = y;
            
            // NOTE: The rect's own Width/Height properties are being changed to include padding.
            rect.Width  += settings.PaddingX;
            rect.Height += settings.PaddingY;

            page.OutputRects.Add( rect );
            packedRects.Add( rect ); // Keep track of successfully packed rects

            // Advance to the next column.
            x += cellWidth;

            // Update the page's utilized size.
            page.Width  = Math.Max( page.Width, x );
            page.Height = Math.Max( page.Height, y + cellHeight );
        }

        // Flip Y coordinates so the first row starts at the top (Y=0).
        for ( var i = page.OutputRects.Count - 1; i >= 0; i-- )
        {
            var rect = page.OutputRects[ i ];
            rect.Y = page.Height - rect.Y - rect.Height;
        }

        return ( page, packedRects );
    }
}

// ============================================================================
// ============================================================================