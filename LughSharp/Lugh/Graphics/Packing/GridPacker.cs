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

using LughSharp.Lugh.Utils.Collections;

namespace LughSharp.Lugh.Graphics.Packing;

[PublicAPI]
public class GridPacker : TexturePacker.IPacker
{
    private readonly TexturePacker.Settings settings;

    public GridPacker( TexturePacker.Settings settings )
    {
        this.settings = settings;
    }

    public List< TexturePacker.Page > Pack( List< TexturePacker.Rect > inputRects )
    {
        return Pack( null, inputRects );
    }

    public List< TexturePacker.Page > Pack( TexturePacker.ProgressListener? progress, List< TexturePacker.Rect > inputRects )
    {
        ArgumentNullException.ThrowIfNull( progress );

        if ( !settings.Silent )
        {
            Console.WriteLine( "Packing" );
        }

        // Rects are packed with right and top padding, so the max size is increased to match.
        // After packing the padding is subtracted from the page size.
        var paddingX = settings.PaddingX;
        var paddingY = settings.PaddingY;
        var adjustX  = paddingX;
        var adjustY  = paddingY;

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
        var n          = inputRects.Count;
        var cellWidth  = 0;
        var cellHeight = 0;

        for ( var i = 0; i < n; i++ )
        {
            var rect = inputRects[ i ];

            cellWidth  = Math.Max( cellWidth, rect.Width );
            cellHeight = Math.Max( cellHeight, rect.Height );
        }

        cellWidth  += paddingX;
        cellHeight += paddingY;

        inputRects.Reverse();

        List< TexturePacker.Page > pages = [ ];

        while ( inputRects.Count > 0 )
        {
            progress.Count = ( n - inputRects.Count ) + 1;

            if ( progress.Update( progress.Count, n ) )
            {
                break;
            }

            var page = PackPage( inputRects, cellWidth, cellHeight, maxWidth, maxHeight );
            page.Width  -= paddingX;
            page.Height -= paddingY;

            pages.Add( page );
        }

        return pages;
    }

    private TexturePacker.Page PackPage( List< TexturePacker.Rect > inputRects, int cellWidth, int cellHeight, int maxWidth, int maxHeight )
    {
        TexturePacker.Page page = new()
        {
            OutputRects = [ ],
        };

        var n = inputRects.Count;
        var x = 0;
        var y = 0;

        for ( var i = n - 1; i >= 0; i-- )
        {
            if ( ( x + cellWidth ) > maxWidth )
            {
                y += cellHeight;

                if ( y > ( maxHeight - cellHeight ) )
                {
                    break;
                }

                x = 0;
            }

            var rect = inputRects.RemoveIndex( i );

            rect.X      =  x;
            rect.Y      =  y;
            rect.Width  += settings.PaddingX;
            rect.Height += settings.PaddingY;

            page.OutputRects.Add( rect );

            x           += cellWidth;
            page.Width  =  Math.Max( page.Width, x );
            page.Height =  Math.Max( page.Height, y + cellHeight );
        }

        // Flip so rows start at top.
        for ( var i = page.OutputRects.Count - 1; i >= 0; i-- )
        {
            var rect = page.OutputRects[ i ];
            rect.Y = page.Height - rect.Y - rect.Height;
        }

        return page;
    }
}