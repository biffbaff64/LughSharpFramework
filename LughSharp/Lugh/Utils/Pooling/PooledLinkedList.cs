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

namespace LughSharp.Lugh.Utils.Pooling;

/// <summary>
/// A simple linked list that pools its nodes.
/// </summary>
[PublicAPI]
public class PooledLinkedList< T >
{
    public int Size { get; set; } = 0;

    // ========================================================================

    private readonly Pool< Item< T > > _pool;

    private Item< T >? _current;
    private Item< T >? _head;
    private Item< T >? _iterator;
    private Item< T >? _tail;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates a new PooledLinkedList with an initial capacity of 16 and the
    /// supplied maximum pool size.
    /// </summary>
    /// <param name="maxPoolSize"></param>
    /// <param name="initialCapacity"></param>
    public PooledLinkedList( int maxPoolSize, int initialCapacity = 16 )
    {
        _pool = new Pool< Item< T > >( GetNewObject, initialCapacity, maxPoolSize );
    }

    /// <summary>
    /// Creates, and returns, a new <see cref="Item{T}" />
    /// </summary>
    public Item< T > GetNewObject()
    {
        return new Item< T >();
    }

    /// <summary>
    /// Adds the specified object to the end of the list regardless of iteration status
    /// </summary>
    public void Add( T obj )
    {
        Item< T >? item;

        if ( ( item = _pool.Obtain() ) != null )
        {
            item.Payload = obj;
            item.Next    = null;
            item.Prev    = null;

            if ( _head == null )
            {
                _head = item;
                _tail = item;

                Size++;

                return;
            }

            item.Prev = _tail;

            _tail      = item;
            _tail.Next = item;

            Size++;
        }
    }

    /// <summary>
    /// Adds the specified object to the head of the list regardless of iteration status
    /// </summary>
    public void AddFirst( T obj )
    {
        Item< T >? item;

        if ( ( item = _pool.Obtain() ) != null )
        {
            item.Payload = obj;
            item.Next    = _head;
            item.Prev    = null;

            if ( _head != null )
            {
                _head.Prev = item;
            }
            else
            {
                _tail = item;
            }

            _head = item;

            Size++;
        }
    }

    /// <summary>
    /// Starts iterating over the list's items from the head of the list
    /// </summary>
    protected void Iterate()
    {
        _iterator = _head;
    }

    /// <summary>
    /// Starts iterating over the list's items from the tail of the list
    /// </summary>
    public void IterateReverse()
    {
        _iterator = _tail;
    }

    /// <summary>
    /// Gets the next item in the list
    /// </summary>
    /// <returns> the next item in the list or null if there are no more items</returns>
    protected T? Next()
    {
        if ( _iterator == null )
        {
            return default( T? );
        }

        var payload = _iterator.Payload;

        _current  = _iterator;
        _iterator = _iterator.Next;

        return payload;
    }

    /// <summary>
    /// Gets the previous item in the list
    /// </summary>
    /// <returns> the previous item in the list or null if there are no more items </returns>
    public T? Previous()
    {
        if ( _iterator == null )
        {
            return default( T? );
        }

        var payload = _iterator.Payload;

        _current  = _iterator;
        _iterator = _iterator.Prev;

        return payload;
    }

    /// <summary>
    /// Removes the current list item based on the iterator position.
    /// </summary>
    protected void Remove()
    {
        if ( ( _current?.Prev == null ) || ( _current.Next == null ) )
        {
            return;
        }

        Size--;

        var c = _current;
        var n = _current.Next;
        var p = _current.Prev;

        _pool.Free( _current );
        _current = null;

        if ( Size == 0 )
        {
            _head = null;
            _tail = null;

            return;
        }

        if ( c == _head )
        {
            n.Prev = null;
            _head  = n;

            return;
        }

        if ( c == _tail )
        {
            p.Next = null;
            _tail  = p;

            return;
        }

        p.Next = n;
        n.Prev = p;
    }

    /// <summary>
    /// Removes the tail of the list regardless of iteration status
    /// </summary>
    public T? RemoveLast()
    {
        if ( _tail == null )
        {
            return default( T? );
        }

        var payload = _tail.Payload;

        Size--;

        var p = _tail.Prev;

        _pool.Free( _tail );

        if ( Size == 0 )
        {
            _head = null;
            _tail = null;
        }
        else
        {
            _tail = p;

            if ( _tail != null )
            {
                _tail.Next = null;
            }
        }

        return payload;
    }

    /// <summary>
    /// Clears this linked list.
    /// </summary>
    public void Clear()
    {
        Iterate();

        while ( Next() != null )
        {
            Remove();
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class Item< TT >
    {
        internal TT?         Payload { get; set; }
        internal Item< TT >? Next    { get; set; }
        internal Item< TT >? Prev    { get; set; }
    }
}