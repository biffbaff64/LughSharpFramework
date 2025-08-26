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

namespace Extensions.Source.Drawing;

[PublicAPI]
public interface IWriteableRenderedImage
{
    /**
     * Adds an observer.  If the observer is already present,
     * it will receive multiple notifications.
     * @param to the specified <code>TileObserver</code>
     */
    public void AddTileObserver( TileObserver to );

    /**
     * Removes an observer.  If the observer was not registered,
     * nothing happens.  If the observer was registered for multiple
     * notifications, it will now be registered for one fewer.
     * @param to the specified <code>TileObserver</code>
     */
    public void RemoveTileObserver( TileObserver to );

    /**
     * Checks out a tile for writing.
     *
     * The WritableRenderedImage is responsible for notifying all
     * of its TileObservers when a tile goes from having
     * no writers to having one writer.
     *
     * @param tileX the X index of the tile.
     * @param tileY the Y index of the tile.
     * @return a writable tile.
     */
    public WritableRaster? GetWritableTile( int tileX, int tileY );

    /**
     * Relinquishes the right to write to a tile.  If the caller
     * continues to write to the tile, the results are undefined.
     * Calls to this method should only appear in matching pairs
     * with calls to getWritableTile; any other use will lead
     * to undefined results.
     *
     * The WritableRenderedImage is responsible for notifying all of
     * its TileObservers when a tile goes from having one writer
     * to having no writers.
     *
     * @param tileX the X index of the tile.
     * @param tileY the Y index of the tile.
     */
    public void ReleaseWritableTile( int tileX, int tileY );

    /**
     * Returns whether a tile is currently checked out for writing.
     *
     * @param tileX the X index of the tile.
     * @param tileY the Y index of the tile.
     * @return <code>true</code> if specified tile is checked out
     *         for writing; <code>false</code> otherwise.
     */
    public bool IsTileWritable( int tileX, int tileY );

    /**
     * Returns an array of Point objects indicating which tiles
     * are checked out for writing.  Returns null if none are
     * checked out.
     * @return an array containing the locations of tiles that are
     *         checked out for writing.
     */
    public Point[] GetWritableTileIndices();

    /**
     * Returns whether any tile is checked out for writing.
     * Semantically equivalent to (getWritableTileIndices() != null).
     * @return <code>true</code> if any tiles are checked out for
     *         writing; <code>false</code> otherwise.
     */
    public bool HasTileWriters();

    /**
     * Sets a rect of the image to the contents of the Raster r, which is
     * assumed to be in the same coordinate space as the WritableRenderedImage.
     * The operation is clipped to the bounds of the WritableRenderedImage.
     * @param r the specified <code>Raster</code>
     */
    public void SetData( Raster r );
}

// ========================================================================
// ========================================================================