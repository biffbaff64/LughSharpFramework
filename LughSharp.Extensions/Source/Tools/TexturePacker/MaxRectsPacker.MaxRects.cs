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

using LughUtils.source.Collections;

namespace Extensions.Source.Tools.TexturePacker;

public partial class MaxRectsPacker
{
    /// <summary>
    /// Implements the **Maximal Rectangles (MaxRects) bin packing algorithm**. 
    /// This algorithm efficiently finds space for smaller rectangles (textures) within a large 
    /// container (texture atlas) by tracking and splitting the remaining free space.
    /// Adapted from the public domain source by Jukka Jylänki.
    /// </summary>
    private class MaxRects
    {
        private readonly List< TexturePacker.Rect > _usedRectangles = [ ];
        private readonly List< TexturePacker.Rect > _freeRectangles = [ ];

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

            var n = new TexturePacker.Rect
                    {
                        X      = 0,
                        Y      = 0,
                        Width  = width,
                        Height = height,
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
        public TexturePacker.Rect? Insert( TexturePacker.Rect rect, FreeRectChoiceHeuristic method )
        {
            // 1. Find the best position using the heuristic
            var newNode = ScoreRect( rect, method );

            if ( ( newNode == null ) || ( newNode.Height == 0 ) )
            {
                return null; // Cannot fit
            }

            // 2. Split the free space that contained the new rectangle
            var numRectanglesToProcess = _freeRectangles.Count;

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

        /// <summary>
        /// Packs a list of rectangles by iteratively choosing the one that best fits 
        /// the remaining space according to the given heuristic. This is generally slower
        /// than pre-sorting but may yield better results.
        /// </summary>
        /// <param name="rects">The list of rectangles to be packed.</param>
        /// <param name="method">The heuristic used to choose the best free space.</param>
        /// <returns>A <see cref="TexturePacker.Page"/> object containing the packed results and any remaining rectangles.</returns>
        public TexturePacker.Page Pack( List< TexturePacker.Rect > rects, FreeRectChoiceHeuristic method )
        {
            rects = new List< TexturePacker.Rect >( rects );

            while ( rects.Count > 0 )
            {
                var bestRectIndex = -1;
                var bestNode = new TexturePacker.Rect
                               {
                                   Score1 = int.MaxValue,
                                   Score2 = int.MaxValue,
                               };

                // Find the next rectangle that packs best.
                for ( var i = 0; i < rects.Count; i++ )
                {
                    // Score the current rectangle against all free nodes
                    var newNode = ScoreRect( rects[ i ], method );

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

            var result = GetResult();
            result.RemainingRects = rects;

            return result;
        }

        /// <summary>
        /// Compiles the final results of the packing process.
        /// </summary>
        /// <returns>A <see cref="TexturePacker.Page"/> object with the final dimensions and occupancy.</returns>
        public TexturePacker.Page GetResult()
        {
            var width  = 0;
            var height = 0;

            // Find the maximum utilized width and height (not necessarily the bin size).
            foreach ( var rect in _usedRectangles )
            {
                width  = Math.Max( width, rect.X + rect.Width );
                height = Math.Max( height, rect.Y + rect.Height );
            }

            return new TexturePacker.Page
                   {
                       OutputRects = [ .._usedRectangles ],
                       Occupancy   = GetOccupancyRatio(),
                       Width       = width,
                       Height      = height,
                   };
        }

        /// <summary>
        /// Finalizes the placement of a rectangle after its position has been determined by a scoring function.
        /// This updates the free list by splitting the space and pruning redundant nodes.
        /// </summary>
        /// <param name="node">The rectangle node that has been placed.</param>
        private void PlaceRect( TexturePacker.Rect node )
        {
            var numRectanglesToProcess = _freeRectangles.Count;

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
        /// A <see cref="TexturePacker.Rect"/> object containing the best found position (X, Y) 
        /// and the heuristic scores (<c>Score1</c>, <c>Score2</c>).
        /// </returns>
        private TexturePacker.Rect? ScoreRect( TexturePacker.Rect rect, FreeRectChoiceHeuristic method )
        {
            var width         = rect.Width;
            var height        = rect.Height;
            var rotatedWidth  = rect.Height;
            var rotatedHeight = rect.Width;
            var rotate        = rect.CanRotate && _settings.Rotation;

            TexturePacker.Rect? newNode;

            switch ( method )
            {
                case FreeRectChoiceHeuristic.BestShortSideFit:
                    // Minimize the short side of the wasted space.
                    newNode = FindPositionForNewNodeBestShortSideFit( width, height, rotatedWidth, rotatedHeight,
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
                    newNode = FindPositionForNewNodeBestLongSideFit( width, height, rotatedWidth, rotatedHeight,
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

            foreach ( var rect in _usedRectangles )
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
        /// <returns>The <see cref="TexturePacker.Rect"/> with the best found position and scores.</returns>
        private TexturePacker.Rect FindPositionForNewNodeBottomLeft( int width,
                                                                     int height,
                                                                     int rotatedWidth,
                                                                     int rotatedHeight,
                                                                     bool rotate )
        {
            var bestNode = new TexturePacker.Rect
                           {
                               Score1 = int.MaxValue, // Best Y-coordinate of the top edge (Y + Height)
                               Score2 = int.MaxValue, // Best X-coordinate (tie-breaker)
                           };

            foreach ( var rect in _freeRectangles )
            {
                // Try non-rotated
                if ( ( rect.Width >= width ) && ( rect.Height >= height ) )
                {
                    var topSideY = rect.Y + height;

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
                    var topSideY = rect.Y + rotatedHeight;

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
        /// <returns>The <see cref="TexturePacker.Rect"/> with the best found position and scores.</returns>
        private TexturePacker.Rect FindPositionForNewNodeBestShortSideFit( int width,
                                                                           int height,
                                                                           int rotatedWidth,
                                                                           int rotatedHeight,
                                                                           bool rotate )
        {
            var bestNode = new TexturePacker.Rect
                           {
                               Score1 = int.MaxValue, // Best Short Side Fit
                           };

            foreach ( var rect in _freeRectangles )
            {
                // Try non-rotated
                if ( ( rect.Width >= width ) && ( rect.Height >= height ) )
                {
                    var leftoverHoriz = Math.Abs( rect.Width - width );
                    var leftoverVert  = Math.Abs( rect.Height - height );
                    var shortSideFit  = Math.Min( leftoverHoriz, leftoverVert );
                    var longSideFit   = Math.Max( leftoverHoriz, leftoverVert ); // Tie-breaker

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
                    var flippedLeftoverHoriz = Math.Abs( rect.Width - rotatedWidth );
                    var flippedLeftoverVert  = Math.Abs( rect.Height - rotatedHeight );
                    var flippedShortSideFit  = Math.Min( flippedLeftoverHoriz, flippedLeftoverVert );
                    var flippedLongSideFit   = Math.Max( flippedLeftoverHoriz, flippedLeftoverVert ); // Tie-breaker

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
        /// <returns>The <see cref="TexturePacker.Rect"/> with the best found position and scores.</returns>
        private TexturePacker.Rect FindPositionForNewNodeBestLongSideFit( int width,
                                                                          int height,
                                                                          int rotatedWidth,
                                                                          int rotatedHeight,
                                                                          bool rotate )
        {
            var bestNode = new TexturePacker.Rect
                           {
                               Score2 = int.MaxValue, // Best Long Side Fit
                           };

            foreach ( var rect in _freeRectangles )
            {
                // Try non-rotated
                if ( ( rect.Width >= width ) && ( rect.Height >= height ) )
                {
                    var leftoverHoriz = Math.Abs( rect.Width - width );
                    var leftoverVert  = Math.Abs( rect.Height - height );
                    var shortSideFit  = Math.Min( leftoverHoriz, leftoverVert ); // Tie-breaker
                    var longSideFit   = Math.Max( leftoverHoriz, leftoverVert );

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
                    var leftoverHoriz = Math.Abs( rect.Width - rotatedWidth );
                    var leftoverVert  = Math.Abs( rect.Height - rotatedHeight );
                    var shortSideFit  = Math.Min( leftoverHoriz, leftoverVert ); // Tie-breaker
                    var longSideFit   = Math.Max( leftoverHoriz, leftoverVert );

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
        /// <returns>The <see cref="TexturePacker.Rect"/> with the best found position and scores.</returns>
        private TexturePacker.Rect FindPositionForNewNodeBestAreaFit( int width,
                                                                      int height,
                                                                      int rotatedWidth,
                                                                      int rotatedHeight,
                                                                      bool rotate )
        {
            var bestNode = new TexturePacker.Rect
                           {
                               Score1 = int.MaxValue, // Best Area Fit
                           };

            foreach ( var rect in _freeRectangles )
            {
                var areaFit = ( rect.Width * rect.Height ) - ( width * height );

                // Try non-rotated
                if ( ( rect.Width >= width ) && ( rect.Height >= height ) )
                {
                    var leftoverHoriz = Math.Abs( rect.Width - width );
                    var leftoverVert  = Math.Abs( rect.Height - height );
                    var shortSideFit  = Math.Min( leftoverHoriz, leftoverVert ); // Tie-breaker (often BSSF)

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
                    var leftoverHoriz = Math.Abs( rect.Width - rotatedWidth );
                    var leftoverVert  = Math.Abs( rect.Height - rotatedHeight );
                    var shortSideFit  = Math.Min( leftoverHoriz, leftoverVert ); // Tie-breaker

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
                var rect = _usedRectangles[ i ];

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
        /// <returns>The <see cref="TexturePacker.Rect"/> with the best found position and scores.</returns>
        private TexturePacker.Rect FindPositionForNewNodeContactPoint( int width,
                                                                       int height,
                                                                       int rotatedWidth,
                                                                       int rotatedHeight,
                                                                       bool rotate )
        {
            var bestNode = new TexturePacker.Rect
                           {
                               Score1 = -1, // Best contact score (maximizing)
                           };

            for ( int i = 0, n = _freeRectangles.Count; i < n; i++ )
            {
                var free = _freeRectangles[ i ];

                // Try non-rotated
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

                // Try rotated
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

        /// <summary>
        /// Splits a single free rectangle into up to four new, smaller free rectangles 
        /// based on the placement of a new used rectangle within it.
        /// This is the core geometric operation of the MaxRects algorithm.
        /// </summary>
        /// <param name="freeNode">The existing free rectangle being considered.</param>
        /// <param name="usedNode">The new rectangle that has been placed.</param>
        /// <returns><c>true</c> if the <paramref name="freeNode"/> was split (i.e., it overlaps and should be removed from the list), otherwise <c>false</c>.</returns>
        private bool SplitFreeNode( TexturePacker.Rect freeNode, TexturePacker.Rect? usedNode )
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
                    var newNode = new TexturePacker.Rect( freeNode );
                    newNode.Height = usedNode.Y - newNode.Y;
                    _freeRectangles.Add( newNode );
                }

                // New node at the bottom side of the used node.
                if ( ( usedNode.Y + usedNode.Height ) < ( freeNode.Y + freeNode.Height ) )
                {
                    var newNode = new TexturePacker.Rect( freeNode )
                                  {
                                      Y      = usedNode.Y + usedNode.Height,
                                      Height = ( freeNode.Y + freeNode.Height ) - ( usedNode.Y + usedNode.Height ),
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
                    var newNode = new TexturePacker.Rect( freeNode );
                    newNode.Width = usedNode.X - newNode.X;

                    _freeRectangles.Add( newNode );
                }

                // New node at the right side of the used node.
                if ( ( usedNode.X + usedNode.Width ) < ( freeNode.X + freeNode.Width ) )
                {
                    var newNode = new TexturePacker.Rect( freeNode )
                                  {
                                      X     = usedNode.X + usedNode.Width,
                                      Width = ( freeNode.X + freeNode.Width ) - ( usedNode.X + usedNode.Width ),
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
                for ( var j = i + 1; j < n; ++j )
                {
                    var rect1 = _freeRectangles[ i ];
                    var rect2 = _freeRectangles[ j ];

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
        private static bool IsContainedIn( TexturePacker.Rect a, TexturePacker.Rect b )
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