// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Circa64 Software Projects
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

namespace LughSharp.Source.Collections;

/// <summary>
/// An <see cref="ObjectMap{TK,TV}"/> that also stores KeysIterator in an <see cref="List{T}"/> using the
/// insertion order. Null KeysIterator are not allowed. No allocation is done except when growing the
/// table size.
/// <p>
/// Iteration over the <see cref="ObjectMap{TK,TV}.EntriesIterator"/>, <see cref="GetKeys"/>, and <see cref="ObjectMap{TK,TV}.ValuesIterator"/> is
/// ordered and faster than an unordered map. KeysIterator can also be accessed and the order changed using
/// <see cref="OrderedKeys()"/>. There is some additional overhead for put and remove operations.
/// </p>
/// <p>
/// This class performs fast contains (typically O(1), worst case O(n) but that is rare in practice).
/// Remove is somewhat slower due to <see cref="OrderedKeys()"/>. Add may be slightly slower, depending
/// on hash collisions. Hash codes are rehashed to reduce collisions and the need to resize. Load
/// factors greater than 0.91 greatly increase the chances to resize to the next higher POT size.
/// </p>
/// <p>
/// Unordered sets and maps are not designed to provide especially fast iteration. Iteration is faster
/// with OrderedSet and OrderedMap.
/// </p>
/// <p>
/// This implementation uses linear probing with the backward shift algorithm for removal. Hash codes
/// are rehashed using Fibonacci hashing, instead of the more common power-of-two mask, to better
/// distribute poor hashCodes . Linear probing continues to work even when all hashCodes collide,
/// just more slowly.
/// </p>
/// </summary>
/// <remarks>
/// See <a href= "https://probablydance.com/2018/06/16/fibonacci-hashing-the-optimization-that-the-world-forgot-or-a-better-alternative-to-integer-modulo/">
/// Malte Skarupke's blog post</a> for further details.
/// </remarks>
[PublicAPI]
public class OrderedMap< TK, TV > : ObjectMap< TK, TV > where TK : notnull
{
    private readonly List< TK > _keys;

    // ========================================================================

    /// <summary>
    /// Creates a new map with default values for initial capacity and load factor.
    /// These values are inherited from <see cref="ObjectMap{T,V}"/>.
    /// The default initial capacity is 51 and the default load factor is 0.8
    /// </summary>
    public OrderedMap()
    {
        _keys = [ ];
    }

    /// <summary>
    /// Creates a new map with a default load factor, and an initial capacity sufficient to hold
    /// initialCapacity items. This map will hold initialCapacity items before growing the backing
    /// table.
    /// </summary>
    /// <param name="initialCapacity">
    /// If not a power of two, it is increased to the next nearest power of two.
    /// </param>
    public OrderedMap( int initialCapacity ) : base( initialCapacity )
    {
        _keys = new List< TK >( initialCapacity );
    }

    /// <summary>
    /// Creates a new map with the specified initial capacity and load factor. This map
    /// will hold initialCapacity items before growing the backing table.
    /// </summary>
    /// <param name="initialCapacity">
    /// If not a power of two, it is increased to the next nearest power of two.
    /// </param>
    /// <param name="loadFactor"></param>
    public OrderedMap( int initialCapacity, float loadFactor ) : base( initialCapacity, loadFactor )
    {
        _keys = new List< TK >( initialCapacity );
    }

    /// <summary>
    /// Creates a new map containing the items in the specified map.
    /// </summary>
    public OrderedMap( OrderedMap< TK, TV > map ) : base( map )
    {
        _keys = [ ];
        _keys.AddRange( map._keys );
    }

    // ========================================================================

    /// <summary>
    /// Replaces the value associated with the specified key, and returns the old value.
    /// If the key is not found, the value is added at the end of the map and null is returned.
    /// </summary>
    /// <param name="key">The key whose value is to be replaced.</param>
    /// <param name="value">The new value to associate with the key.</param>
    /// <returns>The old value associated with the key, or null if the key was not found.</returns>
    public override TV? Put( TK key, TV? value )
    {
        int i = LocateKey( key );

        if ( i >= 0 )
        {
            // Existing key was found.
            TV? oldValue = ValueTable[ i ];
            ValueTable[ i ] = value;

            return oldValue;
        }

        i               = -( i + 1 ); // Empty space was found.
        KeyTable[ i ]   = key;
        ValueTable[ i ] = value;
        _keys.Add( key );

        if ( ++Size >= Threshold )
        {
            Resize( KeyTable.Length << 1 );
        }

        return default( TV );
    }

    /// <summary>
    /// Puts all the key-value pairs from the specified map into this map.
    /// </summary>
    /// <param name="map"> The map whose key-value pairs to put into this map. </param>
    /// <exception cref="NullReferenceException">
    /// Thrown if the specified map is null.
    /// </exception>
    public void PutAll( OrderedMap< TK, TV > map )
    {
        EnsureCapacity( map.Size );

        TK?[] keys   = map.KeyTable.ToArray() ?? throw new NullReferenceException();
        TV?[] values = map.ValueTable.ToArray() ?? throw new NullReferenceException();

        for ( int i = 0, n = keys.Length; i < n; i++ )
        {
            TK? key = keys[ i ];

            if ( key != null )
            {
                Put( key, values[ i ] );
            }
        }
    }

    /// <summary>
    /// Removes the entry for the specified key from the map, if present.
    /// </summary>
    /// <param name="key">The key whose mapping is to be removed from the map.</param>
    /// <returns>
    /// The previous value associated with the specified key, or the default
    /// value if the key was not found.
    /// </returns>
    public override TV? Remove( TK key )
    {
        _keys.Remove( key );

        return base.Remove( key );
    }

    /// <inheritdoc cref="ListExtensions.RemoveIndex{T}(List{T},int)"/>
    public TV? RemoveIndex( int index )
    {
        return base.Remove( _keys.RemoveIndex( index ) );
    }

    /// <summary>
    /// Changes the key <c>before</c> to <c>after</c> without changing its position in the
    /// order or its value.
    /// Returns true if <c>after</c> has been added to the OrderedMap and <c>before</c> has
    /// been removed.
    /// Returns false if <c>after</c> is already present or <c>before</c> is not present.
    /// If you are iterating over an OrderedMap and have an index, you should prefer
    /// <see cref="AlterIndex(int,TK)"/>, which doesn't need to search for an index like
    /// this method and so can be faster.
    /// </summary>
    /// <param name="before"> a key that must be present for this to succeed. </param>
    /// <param name="after"> a key that must not be in this map for this to succeed. </param>
    /// <returns> true if <c>before</c> was removed and <c>after</c> was added, false otherwise. </returns>
    public bool Alter( TK before, TK after )
    {
        if ( ContainsKey( after ) )
        {
            return false;
        }

        int index = _keys.IndexOf( before );

        if ( index == -1 )
        {
            return false;
        }

        base.Put( after, base.Remove( before ) );

        _keys[ index ] = after;

        return true;
    }

    /// <summary>
    /// Changes the key at the given {@code index} in the order to <c>after</c>, without
    /// changing the ordering of other entries or any values. If <c>after</c> is already
    /// present, this returns false; it will also return false if <c>index</c> is invalid
    /// for the size of this map. Otherwise, it returns true. Unlike <see cref="Alter(TK,TK)"/>,
    /// this operates in constant time.
    /// </summary>
    /// <param name="index">
    /// the index in the order of the key to change; must be non-negative and less than
    /// <see cref="ObjectMap{T,V}.Size"/>
    /// </param>
    /// <param name="after">
    /// the key that will replace the contents at <c>index</c>; this key must not be present
    /// for this to succeed
    /// </param>
    /// <returns>
    /// true if <c>after</c> successfully replaced the key at <c>index</c>, false otherwise
    /// </returns>
    public bool AlterIndex( int index, TK after )
    {
        if ( ( index < 0 ) || ( index >= Size ) || ContainsKey( after ) )
        {
            return false;
        }

        base.Put( after, base.Remove( _keys[ index ] ) );

        _keys[ index ] = after;

        return true;
    }

    /// <summary>
    /// Clears the map and reduces the size of the backing arrays to be the
    /// specified capacity / loadFactor, if they are larger.
    /// </summary>
    /// <param name="maximumCapacity"></param>
    public override void Clear( int maximumCapacity )
    {
        _keys.Clear();
        base.Clear( maximumCapacity );
    }

    /// <summary>
    /// Removes all keys and values from the map, resetting it to its initial state.
    /// This implementation clears the key and value tables and sets the size to zero.
    /// </summary>
    public override void Clear()
    {
        _keys.Clear();
        base.Clear();
    }

    /// <summary>
    /// Returns the <see cref="_keys"/> list.
    /// </summary>
    public List< TK > OrderedKeys()
    {
        return _keys;
    }

    /// <summary>
    /// Returns the iterator for the entries in the map. Remove is supported.
    /// </summary>
    /// <returns></returns>
    public EntriesIterator Iterator()
    {
        return GetEntries();
    }

    /// <summary>
    /// Returns an iterator for the entries in the map. Remove is supported.
    /// <para>
    /// If <see cref="ObjectMap{T,V}.AllocateIterators"/> is false, the same iterator instance is
    /// returned each time this method is called. Use the <see cref="OrderedMapEntries"/> constructor
    /// for nested or multithreaded iteration.
    /// </para>
    /// </summary>
    public override EntriesIterator GetEntries()
    {
        if ( Collections.AllocateIterators )
        {
            return new OrderedMapEntries( this );
        }

        if ( Entries1 == null )
        {
            Entries1 = new OrderedMapEntries( this );
            Entries2 = new OrderedMapEntries( this );
        }

        Guard.Against.Null( Entries2 );

        if ( !Entries1.Valid )
        {
            Entries1.Reset();
            Entries1.Valid = true;
            Entries2.Valid = false;

            return Entries1;
        }

        Entries2.Reset();
        Entries2.Valid = true;
        Entries1.Valid = false;

        return Entries2;
    }

    /// <summary>
    /// Returns an iterator for the values in the map. Remove is supported.
    /// <para>
    /// If <see cref="ObjectMap{T,V}.AllocateIterators"/> is false, the same iterator instance is
    /// returned each time this method is called. Use the <see cref="OrderedMapValues"/> constructor
    /// for nested or multithreaded iteration.
    /// </para>
    /// </summary>
    public override ValuesIterator GetValues()
    {
        if ( Collections.AllocateIterators )
        {
            return new OrderedMapValues( this );
        }

        if ( Values1 == null )
        {
            Values1 = new OrderedMapValues( this );
            Values2 = new OrderedMapValues( this );
        }

        Guard.Against.Null( Values2 );

        if ( !Values1.Valid )
        {
            Values1.Reset();
            Values1.Valid = true;
            Values2.Valid = false;

            return Values1;
        }

        Values2.Reset();
        Values2.Valid = true;
        Values1.Valid = false;

        return Values2;
    }

    /// <summary>
    /// Returns an iterator for the keys in the map. Remove is supported.
    /// <para>
    /// If <see cref="Collections.AllocateIterators"/> is false, the same iterator instance is
    /// returned each time this method is called. Use the <see cref="OrderedMapValues"/> constructor
    /// for nested or multithreaded iteration.
    /// </para>
    /// </summary>
    public override KeysIterator GetKeys()
    {
        if ( Collections.AllocateIterators )
        {
            return new OrderedMapKeys( this );
        }

        Guard.Against.Null( Keys2 );

        if ( Keys1 == null )
        {
            Keys1 = new OrderedMapKeys( this );
            Keys2 = new OrderedMapKeys( this );
        }

        if ( !Keys1.Valid )
        {
            Keys1.Reset();
            Keys1.Valid = true;
            Keys2.Valid = false;

            return Keys1;
        }

        Keys2.Reset();
        Keys2.Valid = true;
        Keys1.Valid = false;

        return Keys2;
    }

    // ========================================================================

    /// <summary>
    /// Returns a string representation of the map, using the specified separator for each entry,
    /// and optionally including the braces in the output.
    /// </summary>
    /// <param name="separator"> The separator to use between each entry. </param>
    /// <param name="braces"> Whether to include braces in the output. </param>
    /// <returns> The string representation of the map. </returns>
    protected override string ToString( string separator, bool braces )
    {
        if ( Size == 0 )
        {
            return braces ? "{}" : string.Empty;
        }

        var buffer = new StringBuilder( 32 );

        if ( braces )
        {
            buffer.Append( '{' );
        }

        for ( int i = 0, n = _keys.Count; i < n; i++ )
        {
            if ( i > 0 )
            {
                buffer.Append( separator );
            }

            //TODO:
            buffer.Append( /*_keys[ i ] == this ? "(this)" :*/ _keys[ i ] );
            buffer.Append( '=' );

            TV? value = Get( _keys[ i ] );
            buffer.Append( /*value == this ? "(this)" :*/ value );
        }

        if ( braces )
        {
            buffer.Append( '}' );
        }

        return buffer.ToString();
    }

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Provides an iterator implementation for traversing the entries of an <see cref="OrderedMap{TK, TV}"/>.
    /// This iterator respects the insertion order of the keys within the map and allows for sequential access,
    /// removal, and reset operations. The iteration order is guaranteed to match the order in which the keys
    /// were added to the map, ensuring predictability in traversal.
    /// </summary>
    /// <remarks>
    /// This iterator is specifically tailored for use with <see cref="OrderedMap{TK, TV}"/> and differs from a
    /// standard unordered map iterator in that it preserves insertion order. It utilizes the underlying entries
    /// of its parent map for iteration and supports key removal during traversal.
    /// </remarks>
    [PublicAPI]
    public class OrderedMapEntries : EntriesIterator
    {
        private List< TK > _keys;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedMapEntries"/> class for the specified map.
        /// </summary>
        /// <param name="map"> The map to iterate over. </param>
        public OrderedMapEntries( OrderedMap< TK, TV > map ) : base( map )
        {
            _keys = map._keys;
        }

        /// <summary>
        /// Resets the iterator to the start of the map.
        /// </summary>
        public override void Reset()
        {
            CurrentIndex = -1;
            NextIndex    = 0;
            HasNext      = Map.Size > 0;
        }

        /// <summary>
        /// Returns the next entry in the iteration.
        /// </summary>
        /// <returns>The next entry in the map.</returns>
        /// <exception cref="LughRuntimeException">
        /// Thrown if there are no more entries to iterate over, or if the iterator is nested.
        /// </exception>
        public override Entry Next()
        {
            if ( !HasNext )
            {
                throw new LughRuntimeException( "No Such Element" );
            }

            if ( !Valid )
            {
                throw new LughRuntimeException( "#iterator() cannot be used nested." );
            }

            CurrentIndex = NextIndex;
            Entry.Key    = _keys[ NextIndex ];
            Entry.Value  = Map.Get( Entry.Key );

            NextIndex++;

            HasNext = NextIndex < Map.Size;

            return Entry;
        }

        /// <summary>
        /// Removes the current key-value pair from the map.
        /// </summary>
        public override void Remove()
        {
            if ( CurrentIndex < 0 )
            {
                throw new InvalidOperationException( "next must be called before remove." );
            }

            Map.Remove( Entry.Key! );

            NextIndex--;
            CurrentIndex = -1;
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Represents an iterator for the keys of an <see cref="OrderedMap{TK, TV}"/>. This iterator provides
    /// traversal functionality for accessing keys in the insertion order of the map. It inherits from
    /// <see cref="KeysIterator"/> and includes additional methods for interacting with the ordered keys
    /// as an array or resetting the iterator state.
    /// </summary>
    /// <remarks>
    /// This class operates on the keys of its corresponding <see cref="OrderedMap{TK, TV}"/> to ensure
    /// they are iterated in the same order in which they were added. Modifications to the map during
    /// iteration (such as adding or removing elements) must be handled with care to avoid unintended
    /// behavior.
    /// </remarks>
    [PublicAPI]
    public class OrderedMapKeys : KeysIterator
    {
        private List< TK > _keys;

        /// <inheritdoc />
        public OrderedMapKeys( OrderedMap< TK, TV > map ) : base( map )
        {
            _keys = map._keys;
        }

        /// <summary>
        /// Resets the iterator to the start of the map.
        /// </summary>
        public override void Reset()
        {
            CurrentIndex = -1;
            NextIndex    = 0;
            HasNext      = Map.Size > 0;
        }

        /// <summary>
        /// Returns the next key in the iteration.
        /// </summary>
        /// <returns>The next key in the map.</returns>
        /// <exception cref="LughRuntimeException">
        /// Thrown if there are no more keys to iterate over, or if the iterator is nested.
        /// </exception>
        public override TK Next()
        {
            if ( !HasNext )
            {
                throw new LughRuntimeException( "No Such Element." );
            }

            if ( !Valid )
            {
                throw new LughRuntimeException( "#iterator() cannot be used nested." );
            }

            TK key = _keys[ NextIndex ];

            CurrentIndex = NextIndex;
            NextIndex++;
            HasNext = NextIndex < Map.Size;

            return key;
        }

        /// <summary>
        /// Removes the current key-value pair from the map.
        /// </summary>
        public override void Remove()
        {
            if ( CurrentIndex < 0 )
            {
                throw new InvalidOperationException( "next must be called before remove." );
            }

            ( ( OrderedMap< TK, TV > )Map ).RemoveIndex( CurrentIndex );

            NextIndex    = CurrentIndex;
            CurrentIndex = -1;
        }

        /// <summary>
        /// Adds the remaining keys to the specified list.
        /// </summary>
        /// <param name="array">The list to add the remaining keys to.</param>
        /// <returns>The list containing the remaining keys.</returns>
        public override List< TK > ToArray( List< TK > array )
        {
            for ( int i = NextIndex; i < _keys.Count; i++ )
            {
                array.Add( _keys[ i ] );
            }
            
            NextIndex = _keys.Count;
            HasNext   = false;

            return array;
        }

        /// <summary>
        /// Returns a new list containing the remaining keys.
        /// </summary>
        public override List< TK > ToArray()
        {
            return ToArray( new List< TK >( _keys.Count - NextIndex ) );
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Represents an iterator for the values of an <see cref="OrderedMap{TK, TV}"/>.
    /// Unlike a standard map values iterator, this implementation respects the order of key-value
    /// insertion as maintained by the <see cref="OrderedMap{TK, TV}"/>.
    /// </summary>
    /// <remarks>
    /// This class provides functionality to iterate over the values in their ordered sequence,
    /// as well as utility methods for resetting the iteration state, removing entries during
    /// iteration, and exporting the values to a list. Iteration order matches the insertion order
    /// defined by the encapsulating ordered map.
    /// </remarks>
    [PublicAPI]
    public class OrderedMapValues : ValuesIterator
    {
        private List< TK > _keys;

        /// <inheritdoc />
        public OrderedMapValues( OrderedMap< TK, TV > map ) : base( map )
        {
            _keys = map._keys;
        }

        /// <summary>
        /// Resets the iterator to the start of the map.
        /// </summary>
        public override void Reset()
        {
            CurrentIndex = -1;
            NextIndex    = 0;
            HasNext      = Map.Size > 0;
        }

        /// <summary>
        /// Returns the next key in the iteration.
        /// </summary>
        /// <returns>The next key in the map.</returns>
        /// <exception cref="LughRuntimeException">
        /// Thrown if there are no more values to iterate over, or if the iterator is nested.
        /// </exception>
        public override TV? Next()
        {
            if ( !HasNext )
            {
                throw new LughRuntimeException( "No Such Element" );
            }

            if ( !Valid )
            {
                throw new LughRuntimeException( "#iterator() cannot be used nested." );
            }

            TV? value = Map.Get( _keys[ NextIndex ] );

            CurrentIndex = NextIndex;
            NextIndex++;
            HasNext = NextIndex < Map.Size;

            return value;
        }

        /// <summary>
        /// Removes the current key-value pair from the map.
        /// </summary>
        public override void Remove()
        {
            if ( CurrentIndex < 0 )
            {
                throw new ArgumentException( "next must be called before remove." );
            }

            ( ( OrderedMap< TK, TV > )Map ).RemoveIndex( CurrentIndex );

            NextIndex    = CurrentIndex;
            CurrentIndex = -1;
        }

        /// <summary>
        /// Adds the remaining values to the array.
        /// </summary>
        public override List< TV > ToArray( List< TV > array )
        {
            int n = _keys.Count;

            array.EnsureCapacity( n - NextIndex );

            List< TK > keys = _keys;

            for ( int i = NextIndex; i < n; i++ )
            {
                array.Add( Map.Get( keys[ i ] ) ?? throw new NullReferenceException() );
            }

            CurrentIndex = n - 1;
            NextIndex    = n;
            HasNext      = false;

            return array;
        }

        /// <summary>
        /// Returns a new array containing the remaining values.
        /// </summary>
        public override List< TV > ToArray()
        {
            return ToArray( new List< TV >( _keys.Count - NextIndex ) );
        }
    }
}

// ============================================================================
// ============================================================================

