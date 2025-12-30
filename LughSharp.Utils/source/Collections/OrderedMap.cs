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

namespace LughSharp.Utils.source.Collections;

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
public class OrderedMap< TK, TV > : ObjectMap< TK, TV >
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

    /// <inheritdoc />
    public override TV? Put( TK key, TV? value )
    {
        var i = LocateKey( key );

        if ( i >= 0 )
        {
            // Existing key was found.
            var oldValue = ValueTable[ i ];
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
    /// </summary>
    /// <param name="map"></param>
    /// <exception cref="NullReferenceException"></exception>
    public void PutAll( OrderedMap< TK, TV > map )
    {
        EnsureCapacity( map.Size );

        var keys   = map.KeyTable ?? throw new NullReferenceException();
        var values = map.ValueTable ?? throw new NullReferenceException();

        for ( int i = 0, n = keys.Length; i < n; i++ )
        {
            var key = keys[ i ];

            if ( key != null )
            {
                Put( key, values[ i ] );
            }
        }
    }

    /// <inheritdoc />
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

        var index = _keys.IndexOf( before );

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

    /// <inheritdoc />
    public override void Clear( int maximumCapacity )
    {
        _keys.Clear();
        base.Clear( maximumCapacity );
    }

    /// <inheritdoc />
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

        Guard.ThrowIfNull( Entries2 );

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

        Guard.ThrowIfNull( Values2 );

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
    /// If <see cref="ObjectMap{T,V}.AllocateIterators"/> is false, the same iterator instance is
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

        Guard.ThrowIfNull( Keys2 );

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

    /// <inheritdoc />
    protected override string ToString( string separator, bool braces )
    {
        if ( Size == 0 )
        {
            return braces ? "{}" : "";
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

            var value = Get( _keys[ i ] );
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

    [PublicAPI]
    public class OrderedMapEntries : EntriesIterator
    {
        private List< TK > _keys;

        /// <inheritdoc />
        public OrderedMapEntries( OrderedMap< TK, TV > map ) : base( map )
        {
            _keys = map._keys;
        }

        /// <inheritdoc />
        public override void Reset()
        {
            CurrentIndex = -1;
            NextIndex    = 0;
            HasNext      = Map.Size > 0;
        }

        /// <inheritdoc />
        public override Entry Next()
        {
            if ( !HasNext )
            {
                throw new GdxRuntimeException( "No Such Element" );
            }

            if ( !Valid )
            {
                throw new GdxRuntimeException( "#iterator() cannot be used nested." );
            }

            CurrentIndex = NextIndex;
            Entry.Key    = _keys[ NextIndex ];
            Entry.Value  = Map.Get( Entry.Key );

            NextIndex++;

            HasNext = NextIndex < Map.Size;

            return Entry;
        }

        /// <inheritdoc />
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

    [PublicAPI]
    public class OrderedMapKeys : KeysIterator
    {
        private List< TK > _keys;

        /// <inheritdoc />
        public OrderedMapKeys( OrderedMap< TK, TV > map ) : base( map )
        {
            _keys = map._keys;
        }

        /// <inheritdoc />
        public override void Reset()
        {
            CurrentIndex = -1;
            NextIndex    = 0;
            HasNext      = Map.Size > 0;
        }

        /// <inheritdoc />
        public override TK Next()
        {
            if ( !HasNext )
            {
                throw new GdxRuntimeException( "No Such Element." );
            }

            if ( !Valid )
            {
                throw new GdxRuntimeException( "#iterator() cannot be used nested." );
            }

            var key = _keys[ NextIndex ];

            CurrentIndex = NextIndex;
            NextIndex++;
            HasNext = NextIndex < Map.Size;

            return key;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override List< TK > ToArray( List< TK > array )
        {
            array.AddAll( _keys, NextIndex, _keys.Count - NextIndex );

            NextIndex = _keys.Count;
            HasNext   = false;

            return array;
        }

        /// <inheritdoc />
        public override List< TK > ToArray()
        {
            return ToArray( new List< TK >( _keys.Count - NextIndex ) );
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class OrderedMapValues : ValuesIterator
    {
        private List< TK > _keys;

        /// <inheritdoc />
        public OrderedMapValues( OrderedMap< TK, TV > map ) : base( map )
        {
            _keys = map._keys;
        }

        /// <inheritdoc />
        public override void Reset()
        {
            CurrentIndex = -1;
            NextIndex    = 0;
            HasNext      = Map.Size > 0;
        }

        /// <inheritdoc />
        public override TV? Next()
        {
            if ( !HasNext )
            {
                throw new GdxRuntimeException( "No Such Element" );
            }

            if ( !Valid )
            {
                throw new GdxRuntimeException( "#iterator() cannot be used nested." );
            }

            var value = Map.Get( _keys[ NextIndex ] );

            CurrentIndex = NextIndex;
            NextIndex++;
            HasNext = NextIndex < Map.Size;

            return value;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override List< TV > ToArray( List< TV > array )
        {
            var n = _keys.Count;

            array.EnsureCapacity( n - NextIndex );

            var keys = _keys;

            for ( var i = NextIndex; i < n; i++ )
            {
                array.Add( Map.Get( keys[ i ] ) ?? throw new NullReferenceException() );
            }

            CurrentIndex = n - 1;
            NextIndex    = n;
            HasNext      = false;

            return array;
        }

        /// <inheritdoc />
        public override List< TV > ToArray()
        {
            return ToArray( new List< TV >( _keys.Count - NextIndex ) );
        }
    }
}