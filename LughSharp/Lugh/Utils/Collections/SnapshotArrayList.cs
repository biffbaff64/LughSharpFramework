﻿// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Richard Ikin.
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

using System.Collections;

using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Utils.Collections;

/// <summary>
/// An array that allows modification during iteration. Guarantees that array entries provided
/// by begin() between indexes 0 and size at the time begin was called will not be modified
/// until end() is called. If modification of the SnapshotArray occurs between begin/end, the
/// backing array is copied prior to the modification, ensuring that the backing array that was
/// returned by begin() is unaffected. To avoid allocation, an attempt is made to reuse any extra
/// array created as a result of this copy on subsequent copies.
/// <para>
/// Note that SnapshotArray is not for thread safety, only for modification
/// during iteration.
/// </para>
/// </summary>
[PublicAPI]
public class SnapshotArrayList< T > : ArrayList< T >, IEnumerable< T >
{
    private T[]? _recycled;
    private T[]? _snapshot;
    private int  _snapshotCount;

    // ========================================================================

    /// <summary>
    /// Creates a new SnapshotArray with the specified initial capacity.
    /// The created array will be Ordered.
    /// </summary>
    /// <param name="capacity"> Initial capacity, default is 0. </param>
    public SnapshotArrayList( int capacity = 0 )
        : this( true, capacity )
    {
    }

    /// <summary>
    /// Creates a new SnapshotArray from the supplied <see cref="ArrayList{T}" />
    /// </summary>
    public SnapshotArrayList( ArrayList< T > arrayList )
    {
        ArgumentNullException.ThrowIfNull( arrayList );
        ArgumentNullException.ThrowIfNull( arrayList.Items );

        Ordered = arrayList.Ordered;
        Count   = arrayList.Count;
        Items   = new T[ Count ];

        Array.Copy( arrayList.Items, 0, Items, 0, Count );
    }

    /// <summary>
    /// Creates a new SnapshotArray, with <see cref="ArrayList{T}.Ordered" /> and
    /// array capacity set to the supplied values.
    /// </summary>
    /// <param name="ordered"> Default value is TRUE. </param>
    /// <param name="capacity"> Default value is 16. </param>
    public SnapshotArrayList( bool ordered = true, int capacity = 16 )
    {
        Ordered = ordered;
        Items   = new T[ capacity ];
    }

    /// <summary>
    /// Creates a new SnapshotArray from the supplied <paramref name="array" />,
    /// copying <paramref name="count" /> elements from <paramref name="startIndex" />
    /// onwards. <see cref="ArrayList{T}.Ordered" /> will be set to the supplied value.
    /// </summary>
    /// <param name="ordered"> Whether this array is ordered or not. </param>
    /// <param name="array"> The array to copy from. </param>
    /// <param name="startIndex"> The index to start copying data from. </param>
    /// <param name="count"> The number of elements to copy. </param>
    public SnapshotArrayList( bool ordered, T[] array, int startIndex, int count )
    {
        Ordered = ordered;
        Count   = count;
        Items   = new T[ count ];

        Array.Copy( array, startIndex, Items, 0, count );
    }

    /// <summary>
    /// Sets or Gets the element at the given index.
    /// </summary>
    public override T this[ int index ]
    {
        get => Items[ index ];
        set => Items[ index ] = value;
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc />
    public IEnumerator< T > GetEnumerator()
    {
        return new SnapshotEnumerator< T >( Items );
    }

    /// <summary>
    /// Takes a snapshot of the current array state and then
    /// returns the array.
    /// </summary>
    /// <returns>
    /// Returns the backing array, which is guaranteed to not be modified
    /// before <see cref="End()" />
    /// </returns>
    public T[] Begin()
    {
        Modified();

        _snapshot = Items;

        _snapshotCount++;

        return Items;
    }

    /// <summary>
    /// Releases the guarantee that the array returned by <see cref="Begin()" />
    /// won't be modified.
    /// </summary>
    public void End()
    {
        _snapshotCount = Math.Max( 0, _snapshotCount - 1 );

        if ( ( _snapshot != Items ) && ( _snapshotCount == 0 ) )
        {
            // The backing array was copied, keep around the old array.
            _recycled = _snapshot;

            if ( _recycled != null )
            {
                for ( int i = 0, n = _recycled.Length; i < n; i++ )
                {
                    _recycled[ i ] = default( T )!;
                }
            }
        }

        _snapshot = null!;
    }

    /// <summary>
    /// </summary>
    public void Modified()
    {
        if ( ( _snapshot == null ) || ( _snapshot != Items ) )
        {
            return;
        }

        // Snapshot is in use, copy backing array to recycled
        // array or create new backing array.
        if ( ( _recycled != null ) && ( _recycled.Length >= Count ) )
        {
            // Copy the contents of items[] to recycled
            for ( var i = 0; i < Count; i++ )
            {
                _recycled[ i ] = Items[ i ];
            }

            // '_recycled' now references nothing 
            _recycled = null;
        }
        else
        {
            Resize( Count );
        }
    }

    /// <summary>
    /// Add the supplied value to the end of the array.
    /// </summary>
    public override void Add( T value )
    {
        Modified();

        if ( Count == Items.Length )
        {
            Items = Resize( Math.Max( 8, ( int )( Count * 1.75f ) ) );
        }

        Items[ Count++ ] = value;
    }

    /// <summary>
    /// Copy 'count' items from the supplied array to this array,
    /// starting from position 'start'.
    /// </summary>
    /// <param name="arrayList">The array of items to add.</param>
    /// <param name="start">The start index.</param>
    /// <param name="count">The number of items to copy.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void AddAll( SnapshotArrayList< T > arrayList, int start, int count )
    {
        if ( ( start + count ) > arrayList.Count )
        {
            throw new ArgumentOutOfRangeException
                ( $"start + count must be <= size - {start} + {count} <= {arrayList.Count}" );
        }

        Modified();

        AddAll( arrayList.Items, start, count );
    }

    /// <summary>
    /// Copy 'count' items from the supplied array to this array,
    /// starting from position 'start'.
    /// </summary>
    /// <param name="array">The array of items to add.</param>
    /// <param name="start">The start index.</param>
    /// <param name="count">The number of items to copy.</param>
    public override void AddAll( T?[] array, int start, int count )
    {
        Modified();

        var sizeNeeded = Count + count;

        if ( sizeNeeded > Items.Length )
        {
            Items = Resize( Math.Max( 8, ( int )( sizeNeeded * 1.75f ) ) );
        }

        Array.Copy( array, start, Items, Count, count );

        Count += count;
    }

    /// <summary>
    /// Returns the item at the specified <paramref name="index" />.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the specified index is &gt;= the array size.
    /// </exception>
    public override T GetAt( int index )
    {
        if ( index >= Count )
        {
            throw new ArgumentOutOfRangeException( $"index can't be >= size - {index} >= {Count}" );
        }

        return Items[ index ];
    }

    /// <summary>
    /// Sets the array element at position index to the supplied value.
    /// </summary>
    /// <param name="index"> The index. </param>
    /// <param name="value"> The value. </param>
    public override void Set( int index, T value )
    {
        if ( index >= Count )
        {
            throw new ArgumentOutOfRangeException( $"index can't be >= size - {index} >= {Count}" );
        }

        Modified();

        Items[ index ] = value;
    }

    /// <summary>
    /// Insert the supplied value into the array at position 'index'.
    /// </summary>
    public override void Insert( int index, T value )
    {
        if ( index > Count )
        {
            throw new ArgumentOutOfRangeException( $"index can't be >= size - {index} >= {Count}" );
        }

        if ( Items == null )
        {
            throw new GdxRuntimeException( "Items cannot be null!" );
        }

        Modified();

        if ( Count == Items.Length )
        {
            Items = Resize( Math.Max( 8, ( int )( Count * 1.75f ) ) );
        }

        if ( Ordered )
        {
            Array.Copy( Items, index, Items, index + 1, Count - index );
        }
        else
        {
            Items[ Count ] = Items[ index ];
        }

        Count++;
        Items[ index ] = value;
    }

    /// <summary>
    /// Swap the array elements at <paramref name="firstIndex" />
    /// and <paramref name="secondIndex" />.
    /// </summary>
    /// <param name="firstIndex"> The position of element 1. </param>
    /// <param name="secondIndex"> The position of element 2. </param>
    public override void Swap( int firstIndex, int secondIndex )
    {
        Modified();

        ( Items[ firstIndex ], Items[ secondIndex ] ) = ( Items[ secondIndex ], Items[ firstIndex ] );
    }

    /// <summary>
    /// Removes the first occurance of <paramref name="value" /> from the array.
    /// </summary>
    /// <param name="value"> The value to remove. </param>
    /// <returns> TRUE if successful. </returns>
    public bool Remove( T value )
    {
        Modified();

        for ( int i = 0, n = Count; i < n; i++ )
        {
            if ( value!.Equals( Items[ i ] ) )
            {
                RemoveAt( i );

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Removes, and returns, the element at the specified index.
    /// If the array is ordered, all elements above index will be moved down
    /// 1 position. If the array is not ordered, the element at the end of
    /// the array will be moved into the position at index.
    /// </summary>
    /// <param name="index"> The index. </param>
    /// <returns> The removed element. </returns>
    public T RemoveAt( int index )
    {
        Modified();

        if ( index >= Count )
        {
            throw new ArgumentOutOfRangeException( $"index can't be >= size - {index} >= {Count}" );
        }

        var value = Items[ index ];

        Count--;

        if ( Ordered )
        {
            Array.Copy( Items, index + 1, Items, index, Count - index );
        }
        else
        {
            Items[ index ] = Items[ Count ];
        }

        Items[ Count ] = default( T )!;

        return value;
    }

    /// <summary>
    /// Removes a range of elements from the array.
    /// </summary>
    /// <param name="start">
    /// The zero-based starting index of the range of elements to remove.
    /// </param>
    /// <param name="end">The ending index of the range.</param>
    public override void RemoveRange( int start, int end )
    {
        Modified();

        if ( end >= Count )
        {
            throw new ArgumentOutOfRangeException( $"end can't be >= size - {end} >= {Count}" );
        }

        if ( start > end )
        {
            throw new ArgumentOutOfRangeException( $"start can't be > end - {start} > {end}" );
        }

        var count = ( end - start ) + 1;

        if ( Ordered )
        {
            Array.Copy( Items, start + count, Items, start, Count - ( start + count ) );
        }
        else
        {
            var lastIndex = Count - 1;

            for ( var i = 0; i < count; i++ )
            {
                Items[ start + i ] = Items[ lastIndex - i ];
            }
        }

        Count -= count;
    }

    /// <summary>
    /// Removes all the elements that match the items in the supplied array.
    /// </summary>
    /// <param name="arrayList"></param>
    /// <returns> TRUE if items have been removed. </returns>
    public bool RemoveAll( SnapshotArrayList< T > arrayList )
    {
        Modified();

        var size      = Count;
        var startSize = size;

        for ( int i = 0, n = arrayList.Count; i < n; i++ )
        {
            var item = arrayList.GetAt( i );

            for ( var ii = 0; ii < size; ii++ )
            {
                if ( item!.Equals( Items[ ii ] ) )
                {
                    RemoveAt( ii );
                    size--;

                    break;
                }
            }
        }

        return size != startSize;
    }

    /// <summary>
    /// Returns the index of first occurrence of value in the array,
    /// or -1 if no such value exists.
    /// </summary>
    /// <param name="value"> May be null. </param>
    /// <returns>
    /// An index of first occurrence of value in array or -1 if no such value exists
    /// </returns>
    public override int IndexOf( T? value )
    {
        for ( int i = 0, n = Count; i < n; i++ )
        {
            if ( ( value != null ) && value.Equals( Items[ i ] ) )
            {
                return i;
            }
        }

        return -1;
    }

    /// <inheritdoc />
    public override bool Contains( T? value )
    {
        var i = Count - 1;

        while ( i >= 0 )
        {
            if ( Items[ i-- ]!.Equals( value ) )
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    public override T Peek()
    {
        if ( Count == 0 )
        {
            throw new NullReferenceException( "Array is empty." );
        }

        return Items[ Count - 1 ];
    }

    /// <inheritdoc />
    public override T Pop()
    {
        Modified();

        if ( Count == 0 )
        {
            throw new IndexOutOfRangeException( "Array is empty." );
        }

        --Count;

        var item = Items[ Count ];

        Items[ Count ] = default( T )!;

        return item;
    }

    /// <inheritdoc />
    public override void Clear()
    {
        Array.Clear( Items );

        Count = 0;
    }

    /// <inheritdoc />
    protected override T[] Resize( int newSize )
    {
        var newItems = new T[ newSize ];

        Array.Copy( Items, 0, newItems, 0, Math.Min( Count, newItems.Length ) );

        Items = newItems;

        return newItems;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        const int PRIME = 31;

        var result = PRIME + 43;
        result = ( PRIME * result ) + 34;

        return result;
    }

    /// <inheritdoc />
    public override bool Equals( object? obj )
    {
        if ( obj == this )
        {
            return true;
        }

        if ( !Ordered )
        {
            return false;
        }

        var array = ( SnapshotArrayList< T >? )obj;

        if ( array is not { Ordered: true } )
        {
            return false;
        }

        var n = Count;

        if ( n != array.Count )
        {
            return false;
        }

        for ( var i = 0; i < n; i++ )
        {
            if ( Items[ i ]!.Equals( array.Items[ i ] ) )
            {
                return false;
            }
        }

        return true;
    }
}

// ====================================================================--------

[PublicAPI]
public class SnapshotEnumerator< T > : IEnumerator< T >
{
    private readonly T[] _array;
    private          int _position = -1;

    public SnapshotEnumerator( T[] array )
    {
        _array = array;
    }

    /// <inheritdoc />
    public bool MoveNext()
    {
        _position++;

        return _position < _array.Length;
    }

    /// <inheritdoc />
    public void Reset()
    {
    }

    /// <inheritdoc />
    public T Current
    {
        get
        {
            try
            {
                return _array[ _position ];
            }
            catch ( IndexOutOfRangeException )
            {
                throw new InvalidOperationException();
            }
            catch ( NullReferenceException )
            {
                Logger.Warning( "NullReference encountered!" );

                throw;
            }
        }
    }

    /// <inheritdoc />
    object IEnumerator.Current => Current!;

    /// <inheritdoc />
    public void Dispose()
    {
    }
}