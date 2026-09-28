// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin
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

using System.Collections.Concurrent;

namespace LughSharp.Source.Utils.Pooling;

/// <summary>
/// A class that can be used to handle multiple pools together. Explicit pool
/// registration is needed via <see cref="AddPool{T}(Func{T})"/>
/// or <see cref="AddPool{T}(IPool{T})"/>.
/// </summary>
[PublicAPI]
[UnstableApi( "This class is not yet stable. Use PoolsMap instead." )]
[Experimental( "LUGH_POOLING_001" )]
public class PoolManager
{
    // ConcurrentDictionary handles safe concurrent reads and writes
    private readonly ConcurrentDictionary< Type, IPool > _typePools = new();

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="poolSupplier"></param>
    /// <typeparam name="T"></typeparam>
    public void AddPool< T >( Func< T > poolSupplier ) where T : class
    {
        AddPool( new ConcurrentPool< T >( poolSupplier ) );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pool"></param>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="InvalidOperationException"></exception>
    public void AddPool< T >( IPool< T > pool ) where T : class
    {
        Type type = typeof( T );

        if ( !_typePools.TryAdd( type, pool ) )
        {
            throw new InvalidOperationException
                (
                 $"Attempt to add pool with already "
               + $"existing type: {type.Name}"
                );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public IPool< T > GetPool< T >() where T : class
    {
        Type type = typeof( T );

        if ( !_typePools.TryGetValue( type, out IPool? pool ) )
        {
            throw new InvalidOperationException( $"Attempt to get pool with unknown type: {type.Name}" );
        }

        return ( IPool< T > )pool;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IPool< T >? GetPoolOrNull< T >() where T : class
    {
        return _typePools.TryGetValue( typeof( T ), out IPool? pool ) ? ( IPool< T > )pool : null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool HasPool< T >() where T : class => HasPool( typeof( T ) );

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool HasPool( Type type ) => _typePools.ContainsKey( type );

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public T Obtain< T >() where T : class
    {
        Type type = typeof( T );

        if ( !_typePools.TryGetValue( type, out IPool? pool ) )
        {
            throw new InvalidOperationException( $"Attempt to get pooled object with unknown type: {type.Name}" );
        }

        return ( ( IPool< T > )pool ).Obtain();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T? ObtainOrNull< T >() where T : class
    {
        Type type = typeof( T );

        return _typePools.TryGetValue( type, out IPool? pool ) ? ( ( IPool< T > )pool ).Obtain() : null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public void Free< T >( T obj ) where T : class
    {
        if ( obj == null ) throw new ArgumentNullException( nameof( obj ) );

        Type type = obj.GetType();

        if ( !_typePools.TryGetValue( type, out IPool? pool ) )
        {
            throw new InvalidOperationException( $"Attempt to free pooled object with unknown type: {type.Name}" );
        }

        ( ( IPool< T > )pool ).Free( obj );
    }

    /// <summary>
    /// 
    /// </summary>
    public void Clear()
    {
        foreach ( IPool pool in _typePools.Values )
        {
            pool.Clear();
        }
    }
}

// ============================================================================
// ============================================================================
