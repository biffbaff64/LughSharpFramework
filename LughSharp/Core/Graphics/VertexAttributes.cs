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

using System.Text;

using JetBrains.Annotations;

namespace LughSharp.Core.Graphics;

[PublicAPI]
public class VertexAttributes
{
    /// <summary>
    /// the size of a single vertex in bytes
    /// </summary>
    public int VertexSize { get; private set; }

    // ========================================================================

    /// <summary>
    /// the attributes in the order they were specified
    /// </summary>
    private readonly VertexAttribute[] _attributes;

    /// <summary>
    /// cache of the value calculated by <see cref="Mask"/>.
    /// </summary>
    private long _mask = -1;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Constructor, sets the vertex attributes in a specific order.
    /// </summary>
    public VertexAttributes( params VertexAttribute[] attributes )
    {
        if ( attributes.Length == 0 )
        {
            throw new ArgumentException( "attributes must be >= 1" );
        }

        var list = new VertexAttribute[ attributes.Length ];

        for ( var i = 0; i < attributes.Length; i++ )
        {
            list[ i ] = attributes[ i ];
        }

        _attributes = list;
        VertexSize  = CalculateOffsets();
    }

    // ========================================================================

    /// <summary>
    /// Gets the number of attributes.
    /// </summary>
    public int Size => _attributes.Length;

    /// <summary>
    /// Calculates a mask based on the contained <see cref="VertexAttribute"/>
    /// instances. The mask is a bit-wise or of each attributes <see cref="VertexAttribute.Usage"/>.
    /// </summary>
    /// <returns> the mask  </returns>
    protected long Mask
    {
        get
        {
            if ( _mask == -1 )
            {
                long result = 0;

                foreach ( VertexAttribute t in _attributes )
                {
                    result |= ( uint )t.Usage;
                }

                _mask = result;
            }

            return _mask;
        }
    }

    /// <summary>
    /// Calculates the mask based on <see cref="Mask"/> and packs
    /// the attributes count into the last 32 bits.
    /// </summary>
    /// <returns>
    /// the mask with attributes count packed into the last 32 bits.
    /// </returns>
    public virtual long MaskWithSizePacked => Mask | ( ( long )_attributes.Length << 32 );

    /// <summary>
    /// Returns the offset for the first VertexAttribute with the specified usage.
    /// </summary>
    /// <param name="usage"> The usage of the VertexAttribute.</param>
    /// <param name="defaultIfNotFound"></param>
    protected int GetOffset( int usage, int defaultIfNotFound )
    {
        VertexAttribute? vertexAttribute = FindByUsage( usage );

        if ( vertexAttribute == null )
        {
            return defaultIfNotFound;
        }

        return vertexAttribute.Offset / 4;
    }

    /// <summary>
    /// Returns the offset for the first VertexAttribute with the specified usage.
    /// </summary>
    /// <param name="usage"> The usage of the VertexAttribute.  </param>
    public int GetOffset( int usage )
    {
        return GetOffset( usage, 0 );
    }

    /// <summary>
    /// Returns the first VertexAttribute for the given usage.
    /// </summary>
    /// <param name="usage"> The usage of the VertexAttribute to find.  </param>
    public VertexAttribute? FindByUsage( int usage )
    {
        int len = Size;

        for ( var i = 0; i < len; i++ )
        {
            if ( Get( i ).Usage == usage )
            {
                return Get( i );
            }
        }

        return null;
    }

    /// <summary>
    /// Calculates and sets the offsets for the vertex attributes and determines the
    /// total vertex size in bytes.
    /// </summary>
    /// <returns>
    /// The total size of the vertex structure in bytes.
    /// </returns>
    private int CalculateOffsets()
    {
        var count = 0;

        foreach ( VertexAttribute attribute in _attributes )
        {
            attribute.Offset =  count;
            count            += attribute.GetSizeInBytes();
        }

        return count;
    }

    /// <summary>
    /// Gets the VertexAttribute at the given index.
    /// </summary>
    /// <param name="index"> the index </param>
    public VertexAttribute Get( int index )
    {
        return _attributes[ index ];
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append( '[' );

        foreach ( VertexAttribute t in _attributes )
        {
            builder.Append( '(' );
            builder.Append( t.Alias );
            builder.Append( ", " );
            builder.Append( t.Usage );
            builder.Append( ", " );
            builder.Append( t.NumComponents );
            builder.Append( ", " );
            builder.Append( t.Offset );
            builder.Append( ')' );
            builder.Append( '\n' );
        }

        builder.Append( ']' );

        return builder.ToString();
    }

    /// <inheritdoc />
    public override bool Equals( object? obj )
    {
        if ( obj == this )
        {
            return true;
        }

        if ( obj is not VertexAttributes other )
        {
            return false;
        }

        if ( _attributes.Length != other._attributes.Length )
        {
            return false;
        }

        for ( var i = 0; i < _attributes.Length; i++ )
        {
            if ( !_attributes[ i ].Equals( other._attributes[ i ] ) )
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        long result = 61 * _attributes.Length;

        foreach ( VertexAttribute t in _attributes )
        {
            result = ( result * 61 ) + t.GetHashCode();
        }

        return ( int )( result ^ ( result >> 32 ) );
    }

    /// <summary>
    /// Compares the current <see cref="VertexAttributes"/> instance to another <see cref="VertexAttributes"/>
    /// instance to determine their relative order based on the number of attributes, mask values, and individual
    /// attribute properties.
    /// </summary>
    /// <param name="o">The <see cref="VertexAttributes"/> instance to compare with the current instance.</param>
    /// <returns>
    /// A signed integer that indicates the relative order of the objects being compared. Returns a value less
    /// than zero if this instance precedes <paramref name="o"/> in the sort order, zero if they are equivalent,
    /// and a value greater than zero if this instance follows <paramref name="o"/> in the sort order.
    /// </returns>
    public int CompareTo( VertexAttributes o )
    {
        if ( _attributes.Length != o._attributes.Length )
        {
            return _attributes.Length - o._attributes.Length;
        }

        long m1 = Mask;
        long m2 = o.Mask;

        if ( m1 != m2 )
        {
            return m1 < m2 ? -1 : 1;
        }

        for ( int i = _attributes.Length - 1; i >= 0; --i )
        {
            VertexAttribute va0 = _attributes[ i ];
            VertexAttribute va1 = o._attributes[ i ];

            if ( va0.Usage != va1.Usage )
            {
                return va0.Usage - va1.Usage;
            }

            if ( va0.Unit != va1.Unit )
            {
                return va0.Unit - va1.Unit;
            }

            if ( va0.NumComponents != va1.NumComponents )
            {
                return va0.NumComponents - va1.NumComponents;
            }

            if ( va0.Normalized != va1.Normalized )
            {
                return va0.Normalized ? 1 : -1;
            }

            if ( va0.ComponentType != va1.ComponentType )
            {
                return va0.ComponentType - va1.ComponentType;
            }
        }

        return 0;
    }
}