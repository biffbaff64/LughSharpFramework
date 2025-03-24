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

using System.Text;

using LughSharp.Lugh.Utils.Exceptions;
using LughSharp.Lugh.Utils.Guarding;

namespace LughSharp.Lugh.Utils.Collections.DeleteCandidates;

/// <summary>
/// An <see cref="ObjectMap{TK,TV}"/> that also stores Keys in an <see cref="List{T}"/> using the
/// insertion order. Null Keys are not allowed. No allocation is done except when growing the
/// table size.
/// <p>
/// Iteration over the <see cref="ObjectMap{TK,TV}.Entries"/>, <see cref="Keys()"/>, and <see cref="ObjectMap{TK,TV}.Values"/> is
/// ordered and faster than an unordered map. Keys can also be accessed and the order changed using
/// <see cref="OrderedKeys()"/>. There is some additional overhead for put and remove operations.
/// </p>
/// <p>
/// This class performs fast contains (typically O(1), worst case O(n) but that is rare in practice).
/// Remove is somewhat slower due to <see cref="OrderedKeys()"/>. Add may be slightly slower, depending
/// on hash collisions. Hashcodes are rehashed to reduce collisions and the need to resize. Load
/// factors greater than 0.91 greatly increase the chances to resize to the next higher POT size.
/// </p>
/// <p>
/// Unordered sets and maps are not designed to provide especially fast iteration. Iteration is faster
/// with OrderedSet and OrderedMap.
/// </p>
/// <p>
/// This implementation uses linear probing with the backward shift algorithm for removal. Hashcodes
/// are rehashed using Fibonacci hashing, instead of the more common power-of-two mask, to better
/// distribute poor hashCodes (see <a href= "https://probablydance.com/2018/06/16/fibonacci-hashing-the-optimization-that-the-world-forgot-or-a-better-alternative-to-integer-modulo/">
/// Malte Skarupke's blog post</a>). Linear probing continues to work even when all hashCodes collide,
/// just more slowly.
/// </p>
/// </summary>
[PublicAPI]
public class OrderedMap< TK, TV > : ObjectMap< TK, TV >
{
    private readonly List< TK > _keys;

    // ========================================================================

    /// <summary>
    /// Creates a new map with an initial capacity of 51 and a load factor of 0.8.
    /// </summary>
    public OrderedMap()
    {
        _keys = [ ];
    }

    /// <summary>
    /// Creates a new map with a load factor of 0.8.
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
        int i = LocateKey( key );

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
        if ( containsKey( after ) )
        {
            return false;
        }

        int index = keys.indexOf( before, false );

        if ( index == -1 )
        {
            return false;
        }

        base.put( after, base.remove( before ) );
        keys.set( index, after );

        return true;
    }

    /** Changes the key at the given {@code index} in the order to {@code after}, without changing the ordering of other entries or
     * any values. If {@code after} is already present, this returns false; it will also return false if {@code index} is invalid
     * for the size of this map. Otherwise, it returns true. Unlike {@link #alter(Object, Object)}, this operates in constant time.
     * @param index the index in the order of the key to change; must be non-negative and less than {@link #size}
     * @param after the key that will replace the contents at {@code index}; this key must not be present for this to succeed
     * @return true if {@code after} successfully replaced the key at {@code index}, false otherwise */
    public bool AlterIndex( int index, TK after )
    {
        if ( index < 0 || index >= size || containsKey( after ) )
        {
            return false;
        }

        base.put( after, base.remove( keys.get( index ) ) );
        keys.set( index, after );

        return true;
    }

    public void clear( int maximumCapacity )
    {
        keys.clear();
        base.clear( maximumCapacity );
    }

    public void clear()
    {
        keys.clear();
        base.clear();
    }

    public List< TK > orderedKeys()
    {
        return keys;
    }

    public Entries< TK, TV > iterator()
    {
        return entries();
    }

    /** Returns an iterator for the entries in the map. Remove is supported.
     * <p>
     * If {@link Collections#allocateIterators} is false, the same iterator instance is returned each time this method is called.
     * Use the {@link OrderedMapEntries} constructor for nested or multithreaded iteration. */
    public Entries< TK, TV > entries()
    {
        if ( Collections.allocateIterators )
        {
            return new OrderedMapEntries( this );
        }

        if ( entries1 == null )
        {
            entries1 = new OrderedMapEntries( this );
            entries2 = new OrderedMapEntries( this );
        }

        if ( !entries1.valid )
        {
            entries1.reset();
            entries1.valid = true;
            entries2.valid = false;

            return entries1;
        }

        entries2.reset();
        entries2.valid = true;
        entries1.valid = false;

        return entries2;
    }

    /** Returns an iterator for the values in the map. Remove is supported.
     * <p>
     * If {@link Collections#allocateIterators} is false, the same iterator instance is returned each time this method is called.
     * Use the {@link OrderedMapValues} constructor for nested or multithreaded iteration. */
    public Values< TV > values()
    {
        if ( Collections.allocateIterators )
        {
            return new OrderedMapValues( this );
        }

        if ( values1 == null )
        {
            values1 = new OrderedMapValues( this );
            values2 = new OrderedMapValues( this );
        }

        if ( !values1.valid )
        {
            values1.reset();
            values1.valid = true;
            values2.valid = false;

            return values1;
        }

        values2.reset();
        values2.valid = true;
        values1.valid = false;

        return values2;
    }

    /** Returns an iterator for the keys in the map. Remove is supported.
     * <p>
     * If {@link Collections#allocateIterators} is false, the same iterator instance is returned each time this method is called.
     * Use the {@link OrderedMapKeys} constructor for nested or multithreaded iteration. */
    public Keys< TK > keys()
    {
        if ( Collections.allocateIterators )
        {
            return new OrderedMapKeys( this );
        }

        if ( keys1 == null )
        {
            keys1 = new OrderedMapKeys( this );
            keys2 = new OrderedMapKeys( this );
        }

        if ( !keys1.valid )
        {
            keys1.reset();
            keys1.valid = true;
            keys2.valid = false;

            return keys1;
        }

        keys2.reset();
        keys2.valid = true;
        keys1.valid = false;

        return keys2;
    }

    // ========================================================================
    
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

        var keys = this.keys;

        for ( int i = 0, n = keys.size; i < n; i++ )
        {
            TK key = keys.get( i );
            if ( i > 0 )
            {
                buffer.Append( separator );
            }

            buffer.Append( key == this ? "(this)" : key );
            buffer.Append( '=' );
            TV value = get( key );
            buffer.Append( value == this ? "(this)" : value );
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
    public class OrderedMapEntries : Entries
    {
        private List< TK > _keys;

        public OrderedMapEntries( OrderedMap< TK, TV > map ) : base( map )
        {
            _keys = map._keys;
        }

        public override void Reset()
        {
            CurrentIndex = -1;
            NextIndex    = 0;
            HasNext      = Map.Size > 0;
        }

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
    public class OrderedMapKeys : Keys
    {
        private List< TK > _keys;

        public OrderedMapKeys( OrderedMap< TK,TV > map ) : base( map )
        {
            _keys = map._keys;
        }

        public override void Reset()
        {
            CurrentIndex = -1;
            NextIndex    = 0;
            HasNext      = Map.Size > 0;
        }

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

        public override List< TK > ToArray( List< TK > array )
        {
            array.AddAll( _keys, NextIndex, _keys.Count - NextIndex );

            NextIndex = _keys.Count;
            HasNext   = false;

            return array;
        }

        public override List< TK > ToArray()
        {
            return ToArray( new List< TK >( _keys.Count - NextIndex ) );
        }
    }

    // ========================================================================
    // ========================================================================
    
    [PublicAPI]
    public class OrderedMapValues : Values
    {
        private List< TK > _keys;

        public OrderedMapValues( OrderedMap< TK, TV > map ) : base( map )
        {
            _keys = map._keys;
        }

        public override void Reset()
        {
            CurrentIndex = -1;
            NextIndex    = 0;
            HasNext      = Map.Size > 0;
        }

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

        public override List< TV > ToArray( List< TV > array )
        {
            var n = _keys.Count;
            
            array.EnsureCapacity( n - NextIndex );

            var keys = this._keys;

            for ( var i = NextIndex; i < n; i++ )
            {
                array.Add( Map.Get( keys[ i ] ) ?? throw new NullReferenceException() );
            }
            
            CurrentIndex = n - 1;
            NextIndex    = n;
            HasNext      = false;

            return array;
        }

        public new List< TV > ToArray()
        {
            return ToArray( new List< TV >( _keys.Count - NextIndex ) );
        }
    }
}

