// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Circa64 Software Projects / Richard Ikin.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ///////////////////////////////////////////////////////////////////////////////

using LughSharp.Source.Utils.Pooling;

namespace LughSharp.Source.Utils;

/// <summary>
/// A quad tree that stores a float for each point.
/// </summary>
[PublicAPI]
public class QuadTreeFloat : IResetable
{
    public const int Value   = 0;
    public const int Xpos    = 1;
    public const int Ypos    = 2;
    public const int Distsqr = 3;

    public const int DefaultInitialCapacity = 128;
    public const int DefaultMaxCapacity     = 4096;
    public const int DefaultMaxDepth        = 8;
    public const int DefaultMaxValues       = 16;
    
    // ========================================================================

    public int   MaxValues { get; }
    public int   MaxDepth  { get; }
    public float X         { get; set; }
    public float Y         { get; set; }
    public float Width     { get; set; }
    public float Height    { get; set; }
    public int   Depth     { get; set; }

    public QuadTreeFloat? Nw { get; set; }
    public QuadTreeFloat? Ne { get; set; }
    public QuadTreeFloat? Sw { get; set; }
    public QuadTreeFloat? Se { get; set; }

    // For each entry, stores the value, x, and y.
    public List< float > Values { get; set; }

    // The number of elements stored in 'values' (3 values per quad tree entry).
    public int Count { get; set; }

    // ========================================================================

    private readonly Pool< QuadTreeFloat > _pool = new( DefaultInitialCapacity, DefaultMaxCapacity )
    {
        NewObjectFactory = GetNewObject
    };

    // ========================================================================

    /// <summary>
    /// Creates a quad tree with 16 for maxValues and 8 for maxDepth.
    /// </summary>
    public QuadTreeFloat() : this( DefaultMaxValues, DefaultMaxDepth )
    {
    }

    /// <summary>
    /// Creates a quad tree with provided values for maxValues and maxDepth.
    /// </summary>
    /// <param name="maxValues">
    /// The maximum number of values stored in each quad tree node. When exceeded,
    /// the node is split into 4 child nodes. If the maxDepth has been reached,
    /// more than maxValues may be stored.
    /// </param>
    /// <param name="maxDepth">
    /// The maximum depth of the tree nodes. Nodes at the maxDepth will not be
    /// split and may store more than maxValues number of entries.
    /// </param>
    public QuadTreeFloat( int maxValues, int maxDepth )
    {
        MaxValues = maxValues * 3;
        MaxDepth  = maxDepth;
        Values    = new List< float >( MaxValues );

//        _pool.NewObject = GetNewObject;
    }

    /// <summary>
    /// Resets this quad tree and all child nodes, freeing them to the pool. After
    /// resetting, the quad tree can be reused.
    /// </summary>
    public void Reset()
    {
        if ( Count == -1 )
        {
            if ( Nw != null )
            {
                _pool.Free( Nw );
                Nw = null;
            }

            if ( Sw != null )
            {
                _pool.Free( Sw );
                Sw = null;
            }

            if ( Ne != null )
            {
                _pool.Free( Ne );
                Ne = null;
            }

            if ( Se != null )
            {
                _pool.Free( Se );
                Se = null;
            }
        }

        Count = 0;

        if ( Values.Count > MaxValues )
        {
            Values = new List< float >( MaxValues );
        }
    }

    /// <summary>
    /// Sets the bounds of the quad tree node by specifying its origin and dimensions.
    /// </summary>
    /// <param name="x"> The x-coordinate of the origin of the bounds. </param>
    /// <param name="y"> The y-coordinate of the origin of the bounds. </param>
    /// <param name="width"> The width of the bounds. </param>
    /// <param name="height"> The height of the bounds. </param>
    public void SetBounds( float x, float y, float width, float height )
    {
        X      = x;
        Y      = y;
        Width  = width;
        Height = height;
    }

    /// <summary>
    /// Adds a new entry to the quad tree with the specified value, x-coordinate, and y-coordinate.
    /// </summary>
    /// <param name="value">The value to associate with the point being added.</param>
    /// <param name="valueX">The x-coordinate of the point being added.</param>
    /// <param name="valueY">The y-coordinate of the point being added.</param>
    public void Add( float value, float valueX, float valueY )
    {
        int count = Count;

        if ( count == -1 )
        {
            AddToChild( value, valueX, valueY );

            return;
        }

        if ( Depth < MaxDepth )
        {
            if ( count == MaxValues )
            {
                Split( value, valueX, valueY );

                return;
            }
        }
        else if ( count == Values.Count )
        {
            Values.EnsureCapacity( GrowValues() );
        }

        Values[ count ]     = value;
        Values[ count + 1 ] = valueX;
        Values[ count + 2 ] = valueY;

        Count += 3;
    }

    /// <summary>
    /// Divides the current quad tree node into four child nodes and redistributes existing values among them.
    /// </summary>
    /// <param name="value">The value to be added to the appropriate child node after splitting.</param>
    /// <param name="valueX">The X coordinate associated with the value being added.</param>
    /// <param name="valueY">The Y coordinate associated with the value being added.</param>
    private void Split( float value, float valueX, float valueY )
    {
        for ( var i = 0; i < MaxValues; i += 3 )
        {
            AddToChild( Values[ i ], Values[ i + 1 ], Values[ i + 2 ] );
        }

        // values isn't nulled because the trees are pooled.
        Count = -1;

        AddToChild( value, valueX, valueY );
    }

    /// <summary>
    /// Adds a value along with its X and Y coordinates to the appropriate child node of the quad tree.
    /// </summary>
    /// <param name="value">The value to be added.</param>
    /// <param name="valueX">The X coordinate associated with the value.</param>
    /// <param name="valueY">The Y coordinate associated with the value.</param>
    private void AddToChild( float value, float valueX, float valueY )
    {
        QuadTreeFloat? child;
        float          halfWidth  = Width / 2;
        float          halfHeight = Height / 2;

        if ( valueX < ( X + halfWidth ) )
        {
            if ( valueY < ( Y + halfHeight ) )
            {
                child = Sw ??= ObtainChild( X, Y, halfWidth, halfHeight, Depth + 1 );
            }
            else
            {
                child = Nw ??= ObtainChild( X, Y + halfHeight, halfWidth, halfHeight, Depth + 1 );
            }
        }
        else
        {
            if ( valueY < ( Y + halfHeight ) )
            {
                child = Se ??= ObtainChild( X + halfWidth, Y, halfWidth, halfHeight, Depth + 1 );
            }
            else
            {
                child = Ne ??= ObtainChild( X + halfWidth, Y + halfHeight, halfWidth, halfHeight, Depth + 1 );
            }
        }

        child?.Add( value, valueX, valueY );
    }

    /// <summary>  
    /// Obtains a quad tree child node with the specified bounds and depth.
    /// </summary>
    /// <param name="x">The x-coordinate of the child node's top-left corner.</param>
    /// <param name="y">The y-coordinate of the child node's top-left corner.</param>
    /// <param name="width">The width of the child node.</param>
    /// <param name="height">The height of the child node.</param>
    /// <param name="depth">The depth of the child node within the quad tree.</param>
    /// <returns>
    /// A quad tree child node configured with the specified properties.
    /// </returns>
    private QuadTreeFloat? ObtainChild( float x, float y, float width, float height, int depth )
    {
        QuadTreeFloat child = _pool.Obtain();

        child.X      = x;
        child.Y      = y;
        child.Width  = width;
        child.Height = height;
        child.Depth  = depth;

        return child;
    }

    /// <summary>
    /// Returns a new length for <see cref="Values" /> when it is not enough to
    /// hold all the entries after <see cref="MaxDepth" /> has been reached.
    /// </summary>
    protected int GrowValues()
    {
        return Count + ( 10 * 3 ); //TODO: No magic numbers!
    }

    /// <summary>
    /// Queries the quad tree for all points within a circular area defined by a center point and radius.
    /// </summary>
    /// <param name="centerX">The x-coordinate of the center of the search area.</param>
    /// <param name="centerY">The y-coordinate of the center of the search area.</param>
    /// <param name="radius">The radius of the circular search area.</param>
    /// <param name="results">A list to store the results found within the search area.
    /// Each result consists of the value, x, y, and the square of the distance to the entry.</param>
    public void Query( float centerX, float centerY, float radius, List< float > results )
    {
        Query( centerX, centerY, radius * radius, centerX - radius, centerY - radius, radius * 2, results );
    }

    /// <summary>
    /// Queries the quad tree to find all entries within a circular area defined by the
    /// given center and radius.
    /// </summary>
    /// <param name="centerX">The x-coordinate of the center of the circular area.</param>
    /// <param name="centerY">The y-coordinate of the center of the circular area.</param>
    /// <param name="radiusSqr">The square of the radius of the circular area.</param>
    /// <param name="rectX">
    /// The x-coordinate of the top-left corner of the bounding rectangle for the circular area.
    /// </param>
    /// <param name="rectY">
    /// The y-coordinate of the top-left corner of the bounding rectangle for the circular area.
    /// </param>
    /// <param name="rectSize">
    /// The size (width and height) of the bounding rectangle for the circular area.
    /// </param>
    /// <param name="results">The list to store entries found within the circular area.</param>
    private void Query( float centerX,
                        float centerY,
                        float radiusSqr,
                        float rectX,
                        float rectY,
                        float rectSize,
                        List< float > results )
    {
        if ( !( ( X < ( rectX + rectSize ) )
             && ( ( X + Width ) > rectX )
             && ( Y < ( rectY + rectSize ) )
             && ( ( Y + Height ) > rectY ) ) )
        {
            return;
        }

        int count = Count;

        if ( count != -1 )
        {
            List< float > values = Values;

            for ( var i = 1; i < count; i += 3 )
            {
                float px = values[ i ];
                float py = values[ i + 1 ];
                float dx = px - centerX;
                float dy = py - centerY;
                float d  = ( dx * dx ) + ( dy * dy );

                if ( d <= radiusSqr )
                {
                    results.Add( values[ i - 1 ] );
                    results.Add( px );
                    results.Add( py );
                    results.Add( d );
                }
            }
        }
        else
        {
            Nw?.Query( centerX, centerY, radiusSqr, rectX, rectY, rectSize, results );
            Sw?.Query( centerX, centerY, radiusSqr, rectX, rectY, rectSize, results );
            Ne?.Query( centerX, centerY, radiusSqr, rectX, rectY, rectSize, results );
            Se?.Query( centerX, centerY, radiusSqr, rectX, rectY, rectSize, results );
        }
    }

    /// <summary>
    /// Finds the nearest point to the specified coordinates (x, y) within the quad tree,
    /// returning the result in the provided list.
    /// </summary>
    /// <param name="x">The x-coordinate of the target point.</param>
    /// <param name="y">The y-coordinate of the target point.</param>
    /// <param name="result">
    /// A list to store the details of the nearest point. The list will be cleared and populated with:
    /// - Value (index 0): The value of the nearest point.
    /// - X (index 1): The x-coordinate of the nearest point.
    /// - Y (index 2): The y-coordinate of the nearest point.
    /// - DistanceSquared (index 3): The squared distance of the nearest point from (x, y).
    /// </param>
    /// <returns>
    /// True if a near point is found; otherwise, false if no contents are available.
    /// </returns>
    public bool Nearest( float x, float y, List< float > result )
    {
        // Find nearest value in a cell that contains the point.
        result.Clear();
        result.Add( 0 );
        result.Add( 0 );
        result.Add( 0 );
        result.Add( float.PositiveInfinity );

        FindNearestInternal( x, y, result );

        float nearValue = result.First();
        float nearX     = result[ 1 ];
        float nearY     = result[ 2 ];
        float nearDist  = result[ 3 ];

        bool found = !float.IsPositiveInfinity( nearDist );

        if ( !found )
        {
            nearDist =  Math.Max( Width, Height );
            nearDist *= nearDist;
        }

        // Check for a nearer value in a neighboring cell.
        result.Clear();
        Query( x, y, ( float )Math.Sqrt( nearDist ), result );

        for ( int i = 3, n = result.Count; i < n; i += 4 )
        {
            float dist = result[ i ];

            if ( dist < nearDist )
            {
                nearDist  = dist;
                nearValue = result[ i - 3 ];
                nearX     = result[ i - 2 ];
                nearY     = result[ i - 1 ];
            }
        }

        if ( !found && ( result.Count == 0 ) )
        {
            return false;
        }

        result.Clear();
        result.Add( nearValue );
        result.Add( nearX );
        result.Add( nearY );
        result.Add( nearDist );

        return true;
    }

    /// <summary>
    /// Finds the nearest value to the given coordinates within the current quad tree
    /// node and updates the result.
    /// </summary>
    /// <param name="x">The x-coordinate of the point to search for.</param>
    /// <param name="y">The y-coordinate of the point to search for.</param>
    /// <param name="result">
    /// A list containing the current nearest value, x-coordinate, y-coordinate, and squared distance.
    /// This list will be updated with the nearest value found during the search.
    /// </param>
    private void FindNearestInternal( float x, float y, List< float > result )
    {
        if ( !( ( X < x )
             && ( ( X + Width ) > x )
             && ( Y < y )
             && ( ( Y + Height ) > y ) ) )
        {
            return;
        }

        int count = Count;

        if ( count != -1 )
        {
            float nearValue = result.First();
            float nearX     = result[ 1 ];
            float nearY     = result[ 2 ];
            float nearDist  = result[ 3 ];

            List< float > values = Values;

            for ( var i = 1; i < count; i += 3 )
            {
                float px   = values[ i ], py = values[ i + 1 ];
                float dx   = px - x,      dy = py - y;
                float dist = ( dx * dx ) + ( dy * dy );

                if ( dist < nearDist )
                {
                    nearDist  = dist;
                    nearValue = values[ i - 1 ];
                    nearX     = px;
                    nearY     = py;
                }
            }

            result[ 0 ] = nearValue;
            result[ 1 ] = nearX;
            result[ 2 ] = nearY;
            result[ 3 ] = nearDist;
        }
        else
        {
            Nw?.FindNearestInternal( x, y, result );
            Sw?.FindNearestInternal( x, y, result );
            Ne?.FindNearestInternal( x, y, result );
            Se?.FindNearestInternal( x, y, result );
        }
    }

    /// <summary>
    /// Creates a new <see cref="QuadTreeFloat" /> object. This method is called by
    /// the pool when a pooled object is being reused.
    /// </summary>
    /// <returns></returns>
    public static QuadTreeFloat GetNewObject()
    {
        return new QuadTreeFloat();
    }
}

// ============================================================================
// ============================================================================