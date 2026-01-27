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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Utils.Collections;

/// <summary>
/// Represents a dictionary-like data structure that maintains the order of elements based
/// on their insertion sequence.
/// </summary>
/// <typeparam name="TK">The type of the keys in the dictionary. Keys must be non-null.</typeparam>
/// <typeparam name="TV">The type of the values in the dictionary.</typeparam>
/// <remarks>
/// This class combines the characteristics of a dictionary for key-value pair mapping and
/// a linked list for maintaining insertion order. It ensures that keys are unique while
/// preserving the order in which they were inserted.
/// </remarks>
[PublicAPI]
public class LinkedHashMap< TK, TV > : IDictionary< TK, TV > where TK : notnull
{
    private LinkedList< TK >                _items;
    private Dictionary< TK, ValueNodePair > _valueByKey;

    // ========================================================================

    /// <summary>
    /// Represents a dictionary-like data structure that maintains the order of elements
    /// according to their insertion sequence.
    /// </summary>
    /// <typeparam name="TK">The type of keys in the collection. Keys must be non-null.</typeparam>
    /// <typeparam name="TV">The type of values in the collection.</typeparam>
    /// <remarks>
    /// This class combines the properties of a dictionary and a linked list. It ensures unique keys
    /// while preserving the order in which keys and values are added.
    /// </remarks>
    public LinkedHashMap() : this( EqualityComparer< TK >.Default )
    {
    }

    /// <summary>
    /// Represents a dictionary-like data structure that maintains the order of its elements
    /// based on their insertion sequence.
    /// </summary>
    /// <typeparam name="TK">The type of the keys in the dictionary. Keys must be non-null.</typeparam>
    /// <typeparam name="TV">The type of the values in the dictionary.</typeparam>
    /// <remarks>
    /// Combines the functionality of a dictionary for key-value pair mapping and a linked list
    /// for preserving the insertion order of elements. Ensures that each key is unique in the collection.
    /// </remarks>
    public LinkedHashMap( IEqualityComparer< TK > comparer )
    {
        _items = [ ];

        // Use the provided comparer for the underlying dictionary
        _valueByKey = new Dictionary< TK, ValueNodePair >( comparer );
    }

    /// <summary>
    /// Adds the specified object to the collection.
    /// </summary>
    /// <param name="item">The object to add to the collection.</param>
    public void Add( KeyValuePair< TK, TV > item )
    {
        Add( item.Key, item.Value );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <exception cref="ArgumentException"></exception>
    public void Add( TK key, TV value )
    {
        Guard.Against.Null( key );

        if ( ContainsKey( key ) )
        {
            throw new ArgumentException( "An element with the same key already exists in the LinkedHashMap." );
        }

        var node = _items.AddLast( key );
        _valueByKey.Add( key, new ValueNodePair( value, node ) );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Remove( KeyValuePair< TK, TV > item )
    {
        return Remove( item.Key );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Remove( TK key )
    {
        Guard.Against.Null( key );

        if ( !_valueByKey.TryGetValue( key, out var tempValue ) )
        {
            return false;
        }

        var node = tempValue.Node;
        _valueByKey.Remove( key );
        _items.Remove( node );

        return true;
    }

    /// <summary>
    /// Retrieves the value associated with the specified key, if the key exists.
    /// </summary>
    /// <param name="key">The key whose associated value is to be retrieved.</param>
    /// <param name="value">
    /// When this method returns, contains the value associated with the specified key, if the
    /// key is found; otherwise, the default value for the type of the value parameter.
    /// </param>
    /// <returns>True if the key is found; otherwise, false.</returns>
    public bool TryGetValue( TK key, out TV value )
    {
        if ( !_valueByKey.TryGetValue( key, out var tempValue ) )
        {
            value = default!;

            return false;
        }

        value = tempValue.Value;

        return true;
    }

    /// <summary>
    /// Determines whether the collection contains a specific key-value pair.
    /// </summary>
    /// <param name="item">The key-value pair to locate in the collection.</param>
    /// <returns>True if the key-value pair is found in the collection; otherwise, false.</returns>
    public bool Contains( KeyValuePair< TK, TV > item )
    {
        return _valueByKey.TryGetValue( item.Key, out var value )
               && EqualityComparer< TV >.Default.Equals( value.Value, item.Value );
    }

    /// <summary>
    /// Determines whether the dictionary contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <returns>True if the dictionary contains an element with the specified key; otherwise, false.</returns>
    public bool ContainsKey( TK key )
    {
        return _valueByKey.ContainsKey( key );
    }

    /// <summary>
    /// Copies the elements of the collection to an array, starting at the specified array index.
    /// </summary>
    /// <param name="array">The array that is the destination of the elements copied from the collection.
    /// The array must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in the array at which copying begins.</param>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the arrayIndex is less than 0.</exception>
    /// <exception cref="ArgumentException">Thrown when the number of elements in the source collection
    /// is greater than the available space from arrayIndex to the end of the destination array.</exception>
    public void CopyTo( KeyValuePair< TK, TV >[] array, int arrayIndex )
    {
        ArgumentNullException.ThrowIfNull( array );
        ArgumentOutOfRangeException.ThrowIfNegative( arrayIndex );

        if ( ( array.Length - arrayIndex ) < Count )
        {
            throw new ArgumentException( "The number of elements is greater than the available space." );
        }

        foreach ( var item in this )
        {
            array[ arrayIndex++ ] = item;
        }
    }

    /// <summary>
    /// Represents a reference to the current instance of the class or struct.
    /// Used to access members of the current object or to pass the current object as a parameter.
    /// </summary>
    public TV this[ TK key ]
    {
        get => _valueByKey[ key ].Value;
        set
        {
            if ( !_valueByKey.TryGetValue( key, out var value1 ) )
            {
                Add( key, value );
            }
            else
            {
                value1.Value = value;
                UpdateKey( key );
            }
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator< KeyValuePair< TK, TV > > GetEnumerator()
    {
        var node = _items.First;

        while ( node != null )
        {
            yield return KeyValuePair.Create( node.Value, _valueByKey[ node.Value ].Value );

            node = node.Next;
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to
    /// iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Updates the specified key's position in the internal order to reflect it
    /// as the most recently accessed.
    /// </summary>
    /// <param name="key">The key to be updated in the internal order.</param>
    public void UpdateKey( TK key )
    {
        if ( !_valueByKey.TryGetValue( key, out var value ) )
        {
            return;
        }

        var node = value.Node;

        if ( node.Next == null )
        {
            return;
        }

        _items.Remove( node );
        _items.AddLast( node );
    }

    /// <summary>
    /// Gets a collection containing the keys in the LinkedHashMap, in the order they were inserted.
    /// </summary>
    /// <value>
    /// The collection of keys stored in the LinkedHashMap, preserving their insertion order.
    /// </value>
    public ICollection< TK > Keys => _items; // LinkedList<TK> implements ICollection<TK>

    /// <summary>
    /// Gets a collection containing the values in the LinkedHashMap, in the order they were inserted.
    /// </summary>
    /// <value>
    /// The collection of values stored in the LinkedHashMap, preserving their insertion order.
    /// </value>
    public ICollection< TV > Values
    {
        get
        {
            return new List< TV >( this.Select( kvp => kvp.Value ) ); // Creates a list/collection from the ordered enumerator
        }
    }

    /// <summary>
    /// Gets the number of key-value pairs contained in the LinkedHashMap.
    /// </summary>
    /// <value>
    /// The total number of elements currently stored in the collection.
    /// </value>
    public int Count => _valueByKey.Count;

    /// <summary>
    /// Indicates whether the LinkedHashMap is read-only.
    /// </summary>
    /// <value>
    /// Always returns false as this implementation of LinkedHashMap does not support a read-only mode.
    /// </value>
    public bool IsReadOnly => false;

    /// <summary>
    /// Removes all keys and values from the LinkedHashMap, clearing its contents.
    /// </summary>
    public void Clear()
    {
        _items      = [ ];
        _valueByKey = [ ];
    }

    // ========================================================================

    /// <summary>
    /// Represents a pair that associates a value with its corresponding node in a linked list.
    /// </summary>
    private record ValueNodePair
    {
        public TV                   Value { get; set; }
        public LinkedListNode< TK > Node  { get; }

        public ValueNodePair( TV value, LinkedListNode< TK > node )
        {
            Value = value;
            Node  = node;
        }
    }
}

// ========================================================================
// ========================================================================
