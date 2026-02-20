// ///////////////////////////////////////////////////////////////////////////////
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

using System;
using System.Collections;
using System.Collections.Generic;

using JetBrains.Annotations;

using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;

namespace LughSharp.Core.Utils.Collections;

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
public class SnapshotArrayList< T > : IEnumerable< T >
{
    public int  Size    { get; private set; }
    public bool Ordered { get; private set; }

    // ========================================================================

    private T[]  _items;
    private T[]? _recycled;
    private T[]? _snapshot;
    private int  _snapshots;

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
    public SnapshotArrayList( T[] array, bool ordered = true )
        : this( ordered, array, 0, array.Length )
    {
    }

    /// <summary>
    /// Creates a new SnapshotArray, with <see cref="Ordered" /> and
    /// array capacity set to the supplied values.
    /// </summary>
    /// <param name="ordered"> Default value is TRUE. </param>
    /// <param name="capacity"> Default value is 16. </param>
    public SnapshotArrayList( bool ordered = true, int capacity = 16 )
    {
        Ordered = ordered;
        _items  = new T[ capacity ];
    }

    /// <summary>
    /// Creates a new SnapshotArray from the supplied <paramref name="array" />,
    /// copying <paramref name="size" /> elements from <paramref name="startIndex" />
    /// onwards. <see cref="Ordered" /> will be set to the supplied value.
    /// </summary>
    /// <param name="ordered"> Whether this array is ordered or not. </param>
    /// <param name="array"> The array to copy from. </param>
    /// <param name="startIndex"> The index to start copying data from. </param>
    /// <param name="size"> The number of elements to copy. </param>
    public SnapshotArrayList( bool ordered, T[] array, int startIndex, int size )
    {
        Ordered = ordered;
        Size    = size;
        _items  = new T[ size ];

        Array.Copy( array, startIndex, _items, 0, size );
    }

    /// <summary>
    /// Sets or Gets the element at the given index.
    /// </summary>
    public T this[ int index ]
    {
        get => _items[ index ];
        set => _items[ index ] = value;
    }

    /// <summary>
    /// Takes a snapshot of the current array state and then returns the array.
    /// </summary>
    /// <returns>
    /// The backing array, which is guaranteed to not be modified before <see cref="End()" />
    /// </returns>
    public T[] Begin()
    {
        Modified();

        _snapshot = _items;

        _snapshots++;

        return _items;
    }

    /// <summary>
    /// Releases the guarantee that the array returned by <see cref="Begin()" />
    /// won't be modified.
    /// </summary>
    public void End()
    {
        _snapshots = Math.Max( 0, _snapshots - 1 );

        if ( _snapshot == null )
        {
            return;
        }

        if ( ( _snapshot != _items ) && ( _snapshots == 0 ) )
        {
            // The backing array was copied, keep around the old array.
            _recycled = _snapshot;

            if ( _recycled != null )
            {
                Array.Fill( _recycled, default( T )! );
            }
        }

        _snapshot = null!;
    }

    /// <summary>
    /// Marks the current state of the underlying array as modified, ensuring
    /// that if a snapshot is in use, the changes do not affect the snapshot.
    /// If the snapshot and backing array are the same, the method copies the
    /// current data into a recycled array or resizes the backing array to
    /// maintain data consistency.
    /// </summary>
    public void Modified()
    {
        if ( ( _snapshot == null ) || ( _snapshot != _items ) )
        {
            return;
        }

        // Snapshot is in use, copy backing array to recycled
        // array or create new backing array.
        if ( ( _recycled != null ) && ( _recycled.Length >= Size ) )
        {
            // Copy the contents of items[] to recycled
            for ( var i = 0; i < Size; i++ )
            {
                _recycled[ i ] = _items[ i ];
            }

            _items    = _recycled;
            _recycled = null;
        }
        else
        {
            Resize( Size );
        }
    }

    /// <summary>
    /// Add the supplied value to the end of the array.
    /// </summary>
    public void Add( T value )
    {
        Modified();

        if ( Size == _items.Length )
        {
            _items = Resize( Math.Max( 8, ( int )( Size * 1.75f ) ) );
        }

        _items[ Size++ ] = value;
    }

    /// <summary>
    /// Copy <c>count</c> items from the supplied array to this array,
    /// starting from position <c>start</c>.
    /// </summary>
    /// <param name="arrayList">The array of items to add.</param>
    /// <param name="start">The start index.</param>
    /// <param name="count">The number of items to copy.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void AddAll( SnapshotArrayList< T > arrayList, int start, int count )
    {
        if ( ( start + count ) > arrayList.Size )
        {
            throw new ArgumentOutOfRangeException
                ( $"start + count must be <= size - {start} + {count} <= {arrayList.Size}" );
        }

        AddAll( arrayList._items, start, count );
    }

    /// <summary>
    /// Adds all elements of the given array to the current SnapshotArrayList.
    /// </summary>
    /// <param name="arrayList"></param>
    public void AddAll( List< T > arrayList )
    {
        AddAll( arrayList.ToArray() );
    }

    /// <summary>
    /// Adds all elements of the given array to the current SnapshotArrayList.
    /// New elements are appended at the end of the existing collection.
    /// </summary>
    /// <param name="array">The array containing elements to be added to the SnapshotArrayList.</param>
    public void AddAll( T?[] array )
    {
        AddAll( array, 0, array.Length );
    }

    /// <summary>
    /// Copy <c>count</c> items from the supplied array to this array,
    /// starting from position <c>start</c>.
    /// </summary>
    /// <param name="array">The array of items to add.</param>
    /// <param name="start">The start index.</param>
    /// <param name="count">The number of items to copy.</param>
    public void AddAll( T?[] array, int start, int count )
    {
        Modified();

        var sizeNeeded = Size + count;

        if ( sizeNeeded > _items.Length )
        {
            _items = Resize( Math.Max( 8, ( int )( sizeNeeded * 1.75f ) ) );
        }

        Array.Copy( array, start, _items, Size, count );

        Size += count;
    }

    /// <summary>
    /// Returns the item at the specified <paramref name="index" />.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the specified index is &gt;= the array size.
    /// </exception>
    public T GetAt( int index )
    {
        if ( index >= Size )
        {
            throw new ArgumentOutOfRangeException( $"index can't be >= size - {index} >= {Size}" );
        }

        return _items[ index ];
    }

    /// <summary>
    /// Sets the array element at position index to the supplied value.
    /// </summary>
    /// <param name="index"> The index. </param>
    /// <param name="value"> The value. </param>
    public void Set( int index, T value )
    {
        if ( index >= Size )
        {
            throw new ArgumentOutOfRangeException( $"index can't be >= size - {index} >= {Size}" );
        }

        Modified();

        _items[ index ] = value;
    }

    /// <summary>
    /// Insert the supplied value into the array at position 'index'.
    /// </summary>
    public void Insert( int index, T value )
    {
        if ( index > Size )
        {
            throw new ArgumentOutOfRangeException( $"index can't be >= size - {index} >= {Size}" );
        }

        if ( _items == null )
        {
            throw new RuntimeException( "_items cannot be null!" );
        }

        Modified();

        if ( Size == _items.Length )
        {
            _items = Resize( Math.Max( 8, ( int )( Size * 1.75f ) ) );
        }

        if ( Ordered )
        {
            Array.Copy( _items, index, _items, index + 1, Size - index );
        }
        else
        {
            _items[ Size ] = _items[ index ];
        }

        Size++;
        _items[ index ] = value;
    }

    /// <summary>
    /// Swap the array elements at <paramref name="firstIndex" />
    /// and <paramref name="secondIndex" />.
    /// </summary>
    /// <param name="firstIndex"> The position of element 1. </param>
    /// <param name="secondIndex"> The position of element 2. </param>
    public void Swap( int firstIndex, int secondIndex )
    {
        Modified();

        ( _items[ firstIndex ], _items[ secondIndex ] ) = ( _items[ secondIndex ], _items[ firstIndex ] );
    }

    /// <summary>
    /// Removes the first occurance of <paramref name="value" /> from the array.
    /// </summary>
    /// <param name="value"> The value to remove. </param>
    /// <returns> TRUE if successful. </returns>
    public bool Remove( T value )
    {
        Modified();

        for ( int i = 0, n = Size; i < n; i++ )
        {
            if ( value!.Equals( _items[ i ] ) )
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

        if ( index >= Size )
        {
            throw new ArgumentOutOfRangeException( $"index can't be >= size - {index} >= {Size}" );
        }

        var value = _items[ index ];

        Size--;

        if ( Ordered )
        {
            Array.Copy( _items, index + 1, _items, index, Size - index );
        }
        else
        {
            _items[ index ] = _items[ Size ];
        }

        _items[ Size ] = default( T )!;

        return value;
    }

    /// <summary>
    /// Removes a range of elements from the array.
    /// </summary>
    /// <param name="start">
    /// The zero-based starting index of the range of elements to remove.
    /// </param>
    /// <param name="end">The ending index of the range.</param>
    public void RemoveRange( int start, int end )
    {
        Modified();

        if ( end >= Size )
        {
            throw new ArgumentOutOfRangeException( $"end can't be >= size - {end} >= {Size}" );
        }

        if ( start > end )
        {
            throw new ArgumentOutOfRangeException( $"start can't be > end - {start} > {end}" );
        }

        var count = ( end - start ) + 1;

        if ( Ordered )
        {
            Array.Copy( _items, start + count, _items, start, Size - ( start + count ) );
        }
        else
        {
            var lastIndex = Size - 1;

            for ( var i = 0; i < count; i++ )
            {
                _items[ start + i ] = _items[ lastIndex - i ];
            }
        }

        Size -= count;
    }

    /// <summary>
    /// Removes all the elements that match the items in the supplied array.
    /// </summary>
    /// <param name="arrayList"></param>
    /// <returns> TRUE if items have been removed. </returns>
    public bool RemoveAll( SnapshotArrayList< T > arrayList )
    {
        Modified();

        var size      = Size;
        var startSize = size;

        for ( int i = 0, n = arrayList.Size; i < n; i++ )
        {
            var item = arrayList.GetAt( i );

            for ( var ii = 0; ii < size; ii++ )
            {
                if ( item != null && item.Equals( _items[ ii ] ) )
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
    public int IndexOf( T? value )
    {
        for ( int i = 0, n = Size; i < n; i++ )
        {
            if ( ( value != null ) && value.Equals( _items[ i ] ) )
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Returns true if the array contains the specified value.
    /// </summary>
    /// <param name="value"> The value to search for. May be null. </param>
    public bool Contains( T? value )
    {
        var i = Size - 1;

        while ( i >= 0 )
        {
            if ( _items[ i-- ]!.Equals( value ) )
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns the element at the top of the array.
    /// </summary>
    /// <exception cref="NullReferenceException"> If the array is empty. </exception>
    public T Peek()
    {
        return Size == 0 ? throw new NullReferenceException( "Array is empty." ) : _items[ Size - 1 ];
    }

    /// <summary>
    /// Removes and returns the element at the top of the array.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public T Pop()
    {
        Modified();

        if ( Size == 0 )
        {
            throw new IndexOutOfRangeException( "Array is empty." );
        }

        --Size;

        var item = _items[ Size ];

        _items[ Size ] = default( T )!;

        return item;
    }

    /// <summary>
    /// Clears the array, removing all elements. Resets <c>Size</c> to zero.
    /// </summary>
    public void Clear()
    {
        Array.Clear( _items );

        Size = 0;
    }

    /// <summary>
    /// Resizes the backing array to the specified size.
    /// </summary>
    /// <param name="newSize"> The new size for the backing array.</param>
    /// <returns>The resized array.</returns>
    protected T[] Resize( int newSize )
    {
        var newItems = new T[ newSize ];

        Array.Copy( _items, 0, newItems, 0, Math.Min( Size, newItems.Length ) );

        _items = newItems;

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

        var n = Size;

        if ( n != array.Size )
        {
            return false;
        }

        for ( var i = 0; i < n; i++ )
        {
            if ( _items[ i ]!.Equals( array._items[ i ] ) )
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc />
    public IEnumerator< T > GetEnumerator()
    {
        return new SnapshotEnumerator< T >( _items );
    }
}

// ====================================================================--------
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

    /// <summary>
    /// Advances the enumerator to the next element of the collection.
    /// </summary>
    /// <exception cref="T:System.InvalidOperationException">
    /// The collection was modified after the enumerator was created.
    /// </exception>
    /// <returns>
    /// <b>true</b> if the enumerator was successfully advanced to the next element;
    /// <b>false</b> if the enumerator has passed the end of the collection.
    /// </returns>
    public bool MoveNext()
    {
        _position++;

        return _position < _array.Length;
    }

    /// <summary>
    /// Sets the enumerator to its initial position, which is before the first
    /// element in the collection.
    /// </summary>
    /// <exception cref="T:System.InvalidOperationException">
    /// The collection was modified after the enumerator was created.
    /// </exception>
    /// <exception cref="T:System.NotSupportedException">
    /// The enumerator does not support being reset.
    /// </exception>
    public void Reset()
    {
    }

    /// <summary>
    /// Gets the element in the collection at the current position of the enumerator.
    /// </summary>
    /// <returns>
    /// The element in the collection at the current position of the enumerator.
    /// </returns>
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
                Logger.Error( "NullReference encountered!" );

                throw;
            }
        }
    }

    /// <inheritdoc />
    object IEnumerator.Current => Current!;

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose( true );
        GC.SuppressFinalize( this );
    }

    protected virtual void Dispose( bool disposing )
    {
        if ( disposing )
        {
        }
    }
}

// ============================================================================
// ============================================================================