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

using System.Runtime.Versioning;

using LughSharp.Lugh.Utils.Collections;
using LughSharp.Lugh.Utils.Exceptions;

namespace Extensions.Source.Tools.ImagePacker;

public partial class MaxRectsPacker
{
    /// <summary>
    /// Maximal rectangles bin packing algorithm. Adapted from this C++ public
    /// domain source:
    /// http://clb.demon.fi/projects/even-more-rectangle-bin-packing
    /// </summary>
    private class MaxRects
    {
        private readonly List< TexturePacker.Rect > _usedRectangles = [ ];
        private readonly List< TexturePacker.Rect > _freeRectangles = [ ];

        private int _binWidth;
        private int _binHeight;

        // ====================================================================

        /// <summary>
        /// Initialise this MaxRects object
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
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

        /// <summary>
        /// Packs a single image. Order is defined externally.
        /// </summary>
        public TexturePacker.Rect? Insert( TexturePacker.Rect rect, FreeRectChoiceHeuristic method )
        {
            var newNode = ScoreRect( rect, method );

            Guard.ThrowIfNull( newNode );

            if ( newNode.Height == 0 )
            {
                return null;
            }

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

        /// <summary>
        /// For each rectangle, packs each one then chooses the best and packs that. Slow!
        /// </summary>
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
                    var newNode = ScoreRect( rects[ i ], method );

                    if ( ( newNode?.Score1 < bestNode.Score1 )
                         || ( ( newNode?.Score1 == bestNode.Score1 ) && ( newNode.Score2 < bestNode.Score2 ) ) )
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

                if ( bestRectIndex == -1 )
                {
                    break;
                }

                PlaceRect( bestNode );
                rects.RemoveIndex( bestRectIndex );
            }

            var result = GetResult();
            result.RemainingRects = rects;

            return result;
        }

        public TexturePacker.Page GetResult()
        {
            int w = 0, h = 0;

            foreach ( var rect in _usedRectangles )
            {
                w = Math.Max( w, rect.X + rect.Width );
                h = Math.Max( h, rect.Y + rect.Height );
            }

            var result = new TexturePacker.Page
            {
                OutputRects = [ ],
                Occupancy   = GetOccupancy(),
                Width       = w,
                Height      = h,
            };

            return result;
        }

        private void PlaceRect( TexturePacker.Rect node )
        {
            var numRectanglesToProcess = _freeRectangles.Count;

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

        private TexturePacker.Rect? ScoreRect( TexturePacker.Rect rect, FreeRectChoiceHeuristic method )
        {
            var width         = rect.Width;
            var height        = rect.Height;
            var rotatedWidth  = ( height - _settings.PaddingY ) + _settings.PaddingX;
            var rotatedHeight = ( width - _settings.PaddingX ) + _settings.PaddingY;
            var rotate        = rect.CanRotate && _settings.Rotation;

            TexturePacker.Rect? newNode;

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

        // / Computes the ratio of used surface area.
        private float GetOccupancy()
        {
            var usedSurfaceArea = 0;

            foreach ( var rect in _usedRectangles )
            {
                usedSurfaceArea += rect.Width * rect.Height;
            }

            return ( float )usedSurfaceArea / ( _binWidth * _binHeight );
        }

        private TexturePacker.Rect FindPositionForNewNodeBottomLeft( int width,
                                                                     int height,
                                                                     int rotatedWidth,
                                                                     int rotatedHeight,
                                                                     bool rotate )
        {
            var bestNode = new TexturePacker.Rect
            {
                Score1 = int.MaxValue, // best y, _score2 is best x
            };

            foreach ( var rect in _freeRectangles )
            {
                // Try to place the rectangle in upright (non-rotated) orientation.
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

        private TexturePacker.Rect FindPositionForNewNodeBestShortSideFit( int width,
                                                                           int height,
                                                                           int rotatedWidth,
                                                                           int rotatedHeight,
                                                                           bool rotate )
        {
            var bestNode = new TexturePacker.Rect
            {
                Score1 = int.MaxValue,
            };

            foreach ( var rect in _freeRectangles )
            {
                // Try to place the rectangle in upright (non-rotated) orientation.
                if ( ( rect.Width >= width ) && ( rect.Height >= height ) )
                {
                    var leftoverHoriz = Math.Abs( rect.Width - width );
                    var leftoverVert  = Math.Abs( rect.Height - height );
                    var shortSideFit  = Math.Min( leftoverHoriz, leftoverVert );
                    var longSideFit   = Math.Max( leftoverHoriz, leftoverVert );

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

                if ( rotate && ( rect.Width >= rotatedWidth ) && ( rect.Height >= rotatedHeight ) )
                {
                    var flippedLeftoverHoriz = Math.Abs( rect.Width - rotatedWidth );
                    var flippedLeftoverVert  = Math.Abs( rect.Height - rotatedHeight );
                    var flippedShortSideFit  = Math.Min( flippedLeftoverHoriz, flippedLeftoverVert );
                    var flippedLongSideFit   = Math.Max( flippedLeftoverHoriz, flippedLeftoverVert );

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
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="rotatedWidth"></param>
        /// <param name="rotatedHeight"></param>
        /// <param name="rotate"></param>
        /// <returns></returns>
        private TexturePacker.Rect FindPositionForNewNodeBestLongSideFit( int width,
                                                                          int height,
                                                                          int rotatedWidth,
                                                                          int rotatedHeight,
                                                                          bool rotate )
        {
            var bestNode = new TexturePacker.Rect
            {
                Score2 = int.MaxValue,
            };

            foreach ( var rect in _freeRectangles )
            {
                // Try to place the rectangle in upright (non-rotated) orientation.
                if ( ( rect.Width >= width ) && ( rect.Height >= height ) )
                {
                    var leftoverHoriz = Math.Abs( rect.Width - width );
                    var leftoverVert  = Math.Abs( rect.Height - height );
                    var shortSideFit  = Math.Min( leftoverHoriz, leftoverVert );
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

                if ( rotate && ( rect.Width >= rotatedWidth ) && ( rect.Height >= rotatedHeight ) )
                {
                    var leftoverHoriz = Math.Abs( rect.Width - rotatedWidth );
                    var leftoverVert  = Math.Abs( rect.Height - rotatedHeight );
                    var shortSideFit  = Math.Min( leftoverHoriz, leftoverVert );
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
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="rotatedWidth"></param>
        /// <param name="rotatedHeight"></param>
        /// <param name="rotate"></param>
        /// <returns></returns>
        private TexturePacker.Rect FindPositionForNewNodeBestAreaFit( int width,
                                                                      int height,
                                                                      int rotatedWidth,
                                                                      int rotatedHeight,
                                                                      bool rotate )
        {
            var bestNode = new TexturePacker.Rect
            {
                Score1 = int.MaxValue, // best area fit, _score2 is best short side fit
            };

            foreach ( var rect in _freeRectangles )
            {
                var areaFit = ( rect.Width * rect.Height ) - ( width * height );

                // Try to place the rectangle in upright (non-rotated) orientation.
                if ( ( rect.Width >= width ) && ( rect.Height >= height ) )
                {
                    var leftoverHoriz = Math.Abs( rect.Width - width );
                    var leftoverVert  = Math.Abs( rect.Height - height );
                    var shortSideFit  = Math.Min( leftoverHoriz, leftoverVert );

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

                if ( rotate && ( rect.Width >= rotatedWidth ) && ( rect.Height >= rotatedHeight ) )
                {
                    var leftoverHoriz = Math.Abs( rect.Width - rotatedWidth );
                    var leftoverVert  = Math.Abs( rect.Height - rotatedHeight );
                    var shortSideFit  = Math.Min( leftoverHoriz, leftoverVert );

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
        /// Returns 0 if the two intervals i1 and i2 are disjoint, or the length of their overlap otherwise.
        /// </summary>
        private static int CommonIntervalLength( int i1Start, int i1End, int i2Start, int i2End )
        {
            if ( ( i1End < i2Start ) || ( i2End < i1Start ) )
            {
                return 0;
            }

            return Math.Min( i1End, i2End ) - Math.Max( i1Start, i2Start );
        }

        private int ContactPointScoreNode( int x, int y, int width, int height )
        {
            var score = 0;

            if ( ( x == 0 ) || ( ( x + width ) == _binWidth ) )
            {
                score += height;
            }

            if ( ( y == 0 ) || ( ( y + height ) == _binHeight ) )
            {
                score += width;
            }

            for ( int i = 0, n = _usedRectangles.Count; i < n; i++ )
            {
                var rect = _usedRectangles[ i ];

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
            var bestNode = new TexturePacker.Rect
            {
                Score1 = -1, // best contact score
            };

            for ( int i = 0, n = _freeRectangles.Count; i < n; i++ )
            {
                // Try to place the rectangle in upright (non-rotated) orientation.
                var free = _freeRectangles[ i ];

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

        private bool SplitFreeNode( TexturePacker.Rect freeNode, TexturePacker.Rect? usedNode )
        {
            // Test with SAT if the rectangles even intersect.
            if ( ( usedNode?.X >= ( freeNode.X + freeNode.Width ) )
                 || ( ( usedNode?.X + usedNode?.Width ) <= freeNode.X )
                 || ( usedNode?.Y >= ( freeNode.Y + freeNode.Height ) ) ||
                 ( ( usedNode?.Y + usedNode?.Height ) <= freeNode.Y ) )
            {
                return false;
            }

            if ( ( usedNode?.X < ( freeNode.X + freeNode.Width ) ) && ( ( usedNode.X + usedNode.Width ) > freeNode.X ) )
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

            if ( ( usedNode?.Y < ( freeNode.Y + freeNode.Height ) )
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

        private void PruneFreeList()
        {
            // Go through each pair and remove any rectangle that is redundant.
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
        /// Rweturns TRUE if <see cref="TexturePacker.Rect"/> <c>b</c> is contained within
        /// <see cref="TexturePacker.Rect"/> <c>a</c>.
        /// </summary>
        private static bool IsContainedIn( TexturePacker.Rect a, TexturePacker.Rect b )
        {
            return ( a.X >= b.X )
                   && ( a.Y >= b.Y )
                   && ( ( a.X + a.Width ) <= ( b.X + b.Width ) )
                   && ( ( a.Y + a.Height ) <= ( b.Y + b.Height ) );
        }
    }
}