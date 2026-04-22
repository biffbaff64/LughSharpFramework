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

using System.Collections.Concurrent;

namespace LughSharp.Core.Utils.Pooling;

/// <summary>
/// Stores a map of <see cref="Pool{T}" />s by type for convenient static access.
/// </summary>
[PublicAPI]
public static class Pools
{
    // Use a ConcurrentDictionary for thread-safe access to the pools themselves.
    // Store object as base Pool<object> or dynamic, cast when retrieving.
    private static readonly ConcurrentDictionary< Type, object > _typePools = new();

    // ========================================================================

    /// <summary>
    /// Registers a new <see cref="Pool{T}"/>. Will throw an exception, if a pool
    /// for the same Type is already registered
    /// </summary>
    /// <param name="pool"> The pool to register. </param>
    /// <typeparam name="T"> The Type of the pool. </typeparam>
    public static void AddPool< T >( Pool< T > pool ) where T : class
    {
        if ( !_typePools.TryAdd( typeof( T ), pool ) )
        {
            throw new InvalidOperationException( $"Pool for type {typeof( T ).Name} already exists." );
        }
    }

    /// <summary>
    /// Removes a registered <see cref="Pool{T}"/>
    /// </summary>
    /// <param name="pool"></param>
    /// <typeparam name="T"></typeparam>
    public static void RemovePool< T >( Pool< T > pool ) where T : class
    {
        if ( !_typePools.TryRemove( typeof( T ), out _ ) )
        {
            throw new InvalidOperationException( $"Unable to remove pool of type {typeof( T ).Name}." );
        }
    }

    /// <summary>
    /// Obtains an object of the specified type from its corresponding pool.
    /// The pool must have been registered via Add, Get or Set methods.
    /// </summary>
    /// <typeparam name="T">The type of object to obtain.</typeparam>
    /// <returns> An object from the pool. </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no pool is registered for the specified type.
    /// </exception>
    public static T Obtain< T >() where T : class, new()
    {
        Pool< T > pool = GetRegisteredPool< T >();

        return pool.Obtain();
    }

    /// <summary>
    /// Obtains an object of the specified type from its corresponding pool.
    /// The pool must have been registered via Add, Get or Set methods.
    /// </summary>
    /// <typeparam name="T">The type of object to obtain.</typeparam>
    /// <returns>
    /// An object from the pool, or default(T) if the pool is exhausted or creation failed.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no pool is registered for the specified type.
    /// </exception>
    public static T? ObtainOrNull< T >() where T : class, new()
    {
        return TryGetRegisteredPool< T >( out Pool< T > pool ) ? pool.Obtain() : null;
    }

    /// <summary>
    /// Returns a new or existing pool for the specified type. If a pool for the type
    /// already exists, its existing instance is returned, and 'newObject', 'max' and
    /// 'initialCapacity' parameters are ignored.
    /// </summary>
    /// <remarks>
    /// Thread-safety note: If multiple threads call this method concurrently for the same type
    /// with different parameters, only one pool will be created and stored. The factory and
    /// parameters used for that stored pool are non-deterministic in such scenarios.
    /// To ensure specific pool configuration, pre-register the pool using <see cref="AddPool{T}"/>
    /// or <see cref="Set{T}"/> before concurrent access.
    /// </remarks>
    /// <typeparam name="T">The type of object to pool.</typeparam>
    /// <param name="newObjectFactory">A function to create new objects when the pool needs them.</param>
    /// <param name="initialCapacity">Initial capacity of the pool if a new one is created.</param>
    /// <param name="max">Maximum number of objects to store in the pool if a new one is created.</param>
    /// <returns>The Pool instance for the specified type.</returns>
    public static Pool< T > Get< T >( Pool< T >.PoolObjectFactory newObjectFactory,
                                      int initialCapacity = Pool< T >.DefaultInitialCapacity,
                                      int max = int.MaxValue ) where T : class
    {
        Guard.Against.Null( newObjectFactory );
        
        // If pool exists, return it; otherwise create new one
        if ( _typePools.TryGetValue( typeof( T ), out object? existingPool ) )
        {
            return ( Pool< T > )existingPool;
        }
    
        // Create and add new pool
        var newPool = new Pool< T >( initialCapacity, max )
        {
            NewObjectFactory = newObjectFactory
        };
    
        return ( Pool< T > )_typePools.GetOrAdd( typeof( T ), newPool );
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

        _typePools[ typeof( T ) ] = pool; // Overwrites if exists, adds if not
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
        Guard.Against.Null( obj );

        Pool< T > pool = GetRegisteredPool< T >();
        pool.Free( obj );
    }

    /// <summary>
    /// Frees a collection of objects of the specified type back to their corresponding pool.
    /// The pool must have been registered. Null objects in the list are ignored.
    /// </summary>
    /// <remarks>
    /// If an exception occurs while freeing objects (e.g., pool not registered), the method
    /// will stop processing and throw immediately. Objects processed before the exception
    /// will have been freed successfully, but remaining objects will not be freed.
    /// Consider calling <see cref="TryGetRegisteredPool{T}"/> before calling this method
    /// if you need to verify pool existence without throwing.
    /// </remarks>
    /// <typeparam name="T">The type of the objects to free.</typeparam>
    /// <param name="objects">The collection of objects to free.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no pool is registered for the specified type.
    /// </exception>
    public static void FreeAll< T >( IEnumerable< T? > objects ) where T : class
    {
        Guard.Against.Null( objects );

        // Materialize the enumerable to get accurate count
        // and avoid multiple enumeration issues
        List< T? > objectsList = objects.Where( obj => obj != null ).ToList();
    
        if ( objectsList.Count == 0 )
        {
            return;
        }

        // Validate pool exists before processing any objects
        Pool< T > pool = GetRegisteredPool< T >();

        // null objects have been filtered out by this stage
        foreach ( T? obj in objectsList )
        {
            pool.Free( obj! );
        }
    }

    /// <summary>
    /// Clears the pool for a specific type, discarding all free objects.
    /// </summary>
    /// <typeparam name="T">The type of objects in the pool to clear.</typeparam>
    public static void Clear< T >() where T : class
    {
        if ( TryGetRegisteredPool< T >( out Pool< T > pool ) )
        {
            pool.Clear();
        }
    }

    /// <summary>
    /// Clears all registered pools.
    /// </summary>
    public static void ClearAllPools()
    {
        foreach ( object entry in _typePools.Values )
        {
            if ( entry is IClearablePool clearablePool )
            {
                clearablePool.Clear();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static Pool< T > GetRegisteredPool< T >() where T : class
    {
        return !TryGetRegisteredPool< T >( out Pool< T > pool )
            ? throw new InvalidOperationException( $"No pool registered for type {typeof( T ).Name}." )
            : pool;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pool"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private static bool TryGetRegisteredPool< T >( out Pool< T > pool ) where T : class
    {
        if ( _typePools.TryGetValue( typeof( T ), out object? poolObject ) )
        {
            pool = ( Pool< T > )poolObject;

            return true;
        }

        pool = null!;

        return false;
    }
}

// ============================================================================
// ============================================================================

