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

using System.Drawing;
using JetBrains.Annotations;

namespace Extensions.Source.Drawing;

[PublicAPI]
public interface IWriteableRenderedImage
{
    /// <summary>
    /// Adds an observer. If the observer is already present, it will receive
    /// multiple notifications.
    /// </summary>
    /// <param name="to"> the specified <c>TileObserver</c>. </param>
    public void AddTileObserver( TileObserver to );

    /// <summary>
    /// Removes an observer. If the observer was not registered, nothing happens.
    /// If the observer was registered for multiple notifications, it will now be
    /// registered for one fewer.
    /// </summary>
    /// <param name="to"> the specified <c>TileObserver</c>. </param>
    public void RemoveTileObserver( TileObserver to );

    /// <summary>
    /// Checks out a tile for writing.
    /// <para>
    /// The WritableRenderedImage is responsible for notifying all of its TileObservers
    /// when a tile goes from having no writers to having one writer.
    /// </para>
    /// </summary>
    /// <param name="tileX"> the X index of the tile. </param>
    /// <param name="tileY"> the Y index of the tile. </param>
    /// <returns> a writable tile. </returns>
    public WritableRaster? GetWritableTile( int tileX, int tileY );

    /// <summary>
    /// Relinquishes the right to write to a tile.  If the caller continues to write
    /// to the tile, the results are undefined. Calls to this method should only appear
    /// in matching pairs with calls to getWritableTile; any other use will lead
    /// to undefined results.
    /// <para>
    /// The WritableRenderedImage is responsible for notifying all of its TileObservers
    /// when a tile goes from having one writer to having no writers.
    /// </para>
    /// </summary>
    /// <param name="tileX"> the X index of the tile. </param>
    /// <param name="tileY"> the Y index of the tile. </param>
    public void ReleaseWritableTile( int tileX, int tileY );

    /// <summary>
    /// Returns whether a tile is currently checked out for writing.
    /// </summary>
    /// <param name="tileX"> the X index of the tile. </param>
    /// <param name="tileY"> the Y index of the tile. </param>
    /// <returns>
    /// <c>true</c> if specified tile is checked out for writing; <c>false</c> otherwise.
    /// </returns>
    public bool IsTileWritable( int tileX, int tileY );

    /// <summary>
    /// Returns an array of Point objects indicating which tiles
    /// are checked out for writing.  Returns null if none are
    /// checked out.
    /// </summary>
    /// <returns>
    /// an array containing the locations of tiles that are checked out for writing.
    /// </returns>
    public Point[] GetWritableTileIndices();

    /// <summary>
    /// Returns whether any tile is checked out for writing.
    /// Semantically equivalent to ( GetWritableTileIndices() != null ).
    /// </summary>
    /// <returns>
    /// <c>true</c> if any tiles are checked out for writing; <c>false</c> otherwise.
    /// </returns>
    public bool HasTileWriters();

    /// <summary>
    /// Sets a rect of the image to the contents of the supplied Raster, which is
    /// assumed to be in the same coordinate space as the WritableRenderedImage.
    /// The operation is clipped to the bounds of the WritableRenderedImage.
    /// </summary>
    /// <param name="raster"> the specified <c>Raster</c>. </param>
    public void SetData( Raster raster );
}

// ========================================================================
// ========================================================================