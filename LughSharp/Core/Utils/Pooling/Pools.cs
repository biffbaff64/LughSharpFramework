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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;

namespace LughSharp.Core.Utils.Pooling;

/// <summary>
/// Stores a map of <see cref="Pool{T}" />s by type for convenient static access.
/// </summary>
[PublicAPI]
public static class Pools
{
    // Use a ConcurrentDictionary for thread-safe access to the pools themselves.
    // Store object as base Pool<object> or dynamic, cast when retrieving.
    private static readonly ConcurrentDictionary< Type, object > TypePools = new();

    // ========================================================================

    /// <summary>
    /// Obtains an object of the specified type from its corresponding pool.
    /// The pool must have been registered via Get or Set methods.
    /// </summary>
    /// <typeparam name="T">The type of object to obtain.</typeparam>
    /// <returns>
    /// An object from the pool, or default(T) if the pool is exhausted or creation failed.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no pool is registered for the specified type.
    /// </exception>
    public static T? Obtain< T >() where T : class, new()
    {
        return Get< T >( () => new T() ).Obtain();

//        if ( !TypePools.TryGetValue( typeof( T ), out var poolObject ) )
//        {
//            throw new InvalidOperationException( $"No pool registered for type {typeof( T ).Name}. " +
//                                                 $"Call Pools.Get<{typeof( T ).Name}>() first " +
//                                                 $"with a NewObjectHandler." );
//        }
//
//        var pool = ( Pool< T > )poolObject;
//
//        return pool.Obtain();
    }

    /// <summary>
    /// Returns a new or existing pool for the specified type. If a pool for the type
    /// already exists, its existing instance is returned, and 'max' and 'initialCapacity'
    /// parameters are ignored.
    /// </summary>
    /// <typeparam name="T">The type of object to pool.</typeparam>
    /// <param name="newObjectFactory">A function to create new objects when the pool needs them.</param>
    /// <param name="initialCapacity">Initial capacity of the pool if a new one is created.</param>
    /// <param name="max">Maximum number of objects to store in the pool if a new one is created.</param>
    /// <returns>The Pool instance for the specified type.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if newObjectFactory is null when creating a new pool.
    /// </exception>
    public static Pool< T > Get< T >( Pool< T >.PoolObjectFactory newObjectFactory,
                                      int initialCapacity = Pool< T >.DEFAULT_INITIAL_CAPACITY,
                                      int max = int.MaxValue ) where T : class
    {
        // GetOrAdd is thread-safe for creating or retrieving.
        return ( Pool< T > )TypePools.GetOrAdd( typeof( T ),
                                                new Pool< T >( initialCapacity, max )
                                                {
                                                    NewObjectFactory = newObjectFactory
                                                } );
    }

    /// <summary>
    /// Sets an existing pool for the specified type. Useful for custom pool implementations.
    /// Overwrites any existing pool for this type.
    /// </summary>
    /// <typeparam name="T">The type of object the pool manages.</typeparam>
    /// <param name="pool">The Pool instance to set.</param>
    /// <exception cref="ArgumentNullException">Thrown if pool is null.</exception>
    public static void Set< T >( Pool< T > pool ) where T : class
    {
        Guard.Against.Null( pool );

        TypePools[ typeof( T ) ] = pool; // Overwrites if exists, adds if not
    }

    /// <summary>
    /// Frees an object of the specified type back to its corresponding pool.
    /// The pool must have been registered.
    /// </summary>
    /// <typeparam name="T">The type of the object to free.</typeparam>
    /// <param name="obj">The object to free.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no pool is registered for the specified type.
    /// </exception>
    public static void Free< T >( T obj ) where T : class
    {
        ArgumentNullException.ThrowIfNull( obj );

        if ( !TypePools.TryGetValue( typeof( T ), out var poolObject ) )
        {
            // Ignore freeing an object that was never retained.
            return;
        }

        var pool = ( Pool< T > )poolObject;
        pool.Free( obj );
    }

    /// <summary>
    /// Frees a collection of objects of the specified type back to their corresponding pool.
    /// The pool must have been registered. Null objects in the list are ignored.
    /// </summary>
    /// <typeparam name="T">The type of the objects to free.</typeparam>
    /// <param name="objects">The collection of objects to free.</param>
    /// <param name="samePool"></param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no pool is registered for the specified type.
    /// </exception>
    public static void FreeAll< T >( IEnumerable< T > objects, bool samePool = true ) where T : class
    {
        Guard.Against.Null( objects );

        Pool< T >? pool = null;

        var enumerable = objects as T?[] ?? objects.ToArray();
        
        for ( int i = 0, n = enumerable.Length; i < n; i++ )
        {
            var obj = enumerable.ElementAt( i );

            if ( obj == null )
            {
                continue;
            }

            if ( pool == null )
            {
                pool = TypePools[ typeof( T ) ] as Pool< T >;

                if ( pool == null )
                {
                    continue;
                }
            }
            
            pool.Free( obj );

            if ( !samePool )
            {
                pool = null;
            }
        }
    }

    /// <summary>
    /// Clears the pool for a specific type, discarding all free objects.
    /// </summary>
    /// <typeparam name="T">The type of objects in the pool to clear.</typeparam>
    public static void Clear< T >() where T : class
    {
        if ( TypePools.TryGetValue( typeof( T ), out var poolObject ) )
        {
            ( ( Pool< T > )poolObject ).Clear();
        }
    }

    /// <summary>
    /// Clears all registered pools.
    /// </summary>
    public static void ClearAllPools()
    {
        foreach ( var entry in TypePools.Values )
        {
            if ( entry is IClearablePool clearablePool )
            {
                clearablePool.Clear();
            }
        }

        // Remove all entries from the dictionary
        TypePools.Clear();
    }
}

// ============================================================================
// ============================================================================
