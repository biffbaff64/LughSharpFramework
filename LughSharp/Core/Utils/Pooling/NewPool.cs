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

using System.Collections.Concurrent;
using LughSharp.Core.Utils.Logging;

namespace LughSharp.Core.Utils.Pooling;

/// <summary>
/// A pool of objects that can be reused to avoid allocation.
/// </summary>
[PublicAPI]
public class NewPool< T > where T : notnull
{
    public const int DEFAULT_INITIAL_CAPACITY = 16;

    // Delegate for creating new instances of T
    public delegate T? NewObjectHandler();

    // The maximum number of objects that will be pooled.
    public int Max { get; }

    // The highest number of objects ever free.
    public int Peak { get; private set; }

    // ========================================================================

    // ConcurrentBag is thread-safe for add/take operations
    private readonly ConcurrentBag< T > _freeObjects;

    // The factory to create new objects.
    private readonly NewObjectHandler _newObjectFactory;

    // Using a HashSet to track active objects allows checking for double-freeing.
    // This adds overhead, but is crucial for correctness.
    // For thread-safe tracking
    private readonly ConcurrentDictionary< T, bool > _activeObjects;

    // ========================================================================

    public NewPool( NewObjectHandler newObjectFactory,
                    int initialCapacity = DEFAULT_INITIAL_CAPACITY,
                    int max = int.MaxValue )
    {
        _newObjectFactory = newObjectFactory ??
                            throw new ArgumentNullException( nameof( newObjectFactory ),
                                                             "NewObjectHandler cannot be null." );
        Max = max;

        _freeObjects   = [ ];
        _activeObjects = new ConcurrentDictionary< T, bool >();

        // Pre-fill the pool if initialCapacity > 0
        if ( initialCapacity > 0 )
        {
            Fill( initialCapacity );
        }
    }

    // ========================================================================

    /// <summary>
    /// Returns an object from this pool. The object may be new or reused.
    /// Returns null if the pool is exhausted and cannot create new objects (e.g.,
    /// if Max is reached and no objects are free).
    /// </summary>
    public virtual T? Obtain()
    {
        // Try to get from free objects first
        // Use TryTake for ConcurrentBag
        if ( !_freeObjects.TryTake( out var obj ) )
        {
            // Pool is empty, create a new object
            if ( GetTotalPooledAndActiveCount() >= Max )
            {
                // Pool is at max capacity and no free objects.
                // Depending on policy: throw, return null, or block.
                // For now, returning null.
                Logger.Error( $"Pool for type {typeof( T ).Name} exhausted. Cannot obtain object." );

                return default( T ); // Returning default(T) (null for reference types)
            }

            try
            {
                obj = _newObjectFactory();

                if ( obj == null )
                {
                    // Factory itself returned null (e.g., due to creation failure)
                    Logger.Error( $"NewObjectHandler for type {typeof( T ).Name} returned null." );

                    return default( T );
                }
            }
            catch ( Exception ex )
            {
                Logger.Error( $"Failed to create new object for type {typeof( T ).Name}: {ex.Message}" );

                return default( T );
            }
        }

        // Add to active objects tracking for double-free check
        if ( !_activeObjects.TryAdd( obj, true ) ) // For ConcurrentDictionary
        {
            // This should ideally not happen if pool is correctly managed, but indicates an issue.
            // Means an object is being obtained but already marked as active.
            Logger.Error( $"Obtained object {obj.GetType().Name} was already marked as " +
                            $"active. Potential pool misuse." );

            //TODO: We might want to re-add to free and try again, or throw. For now, proceeding.
        }

        // Reset the object before returning
        ResetObject( obj );

        // Update peak after obtaining an object if needed
        UpdatePeak();

        return obj;
    }

    /// <summary>
    /// Adds the specified number of new free objects to the pool.
    /// Usually called early on as a pre-allocation mechanism.
    /// </summary>
    /// <param name="size">The number of objects to be added.</param>
    public void Fill( int size )
    {
        for ( var i = 0; i < size; i++ )
        {
            if ( GetFreeObjects() >= Max )
            {
                break; // Don't exceed max capacity
            }

            try
            {
                var obj = _newObjectFactory();

                if ( obj == null )
                {
                    Logger.Error( $"NewObjectHandler for type {typeof( T ).Name} returned null during Fill." );

                    break; // Stop filling if factory returns null
                }

                ResetObject( obj );      // Reset new objects before adding to free list
                _freeObjects.Add( obj ); // For ConcurrentBag
            }
            catch ( Exception ex )
            {
                Logger.Error( $"Failed to create object during Fill for " +
                                $"type {typeof( T ).Name}: {ex.Message}" );

                break; // Stop filling if creation fails
            }
        }

        UpdatePeak();
    }

    /// <summary>
    /// Puts the specified object in the pool, making it eligible to be returned by Obtain.
    /// If the pool already contains Max free objects, the specified object is discarded.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// If the object was not obtained from this pool or is being double-freed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Attempted to free object that was not obtained from this pool or was already freed.
    /// </exception>
    public virtual void Free( T obj )
    {
        ArgumentNullException.ThrowIfNull( obj );

        // Check if the object was actually obtained from this pool and is not being double-freed.
        if ( !_activeObjects.TryRemove( obj, out var _ ) )
        {
            throw new InvalidOperationException( $"Attempted to free object {obj.GetType().Name} that was " +
                                                 $"not obtained from this pool or was already freed." );
        }

        if ( GetFreeObjects() < Max )
        {
            ResetObject( obj );      // Reset before adding back to pool
            _freeObjects.Add( obj ); // For ConcurrentBag
        }
        else
        {
            DiscardObject( obj );
        }
    }

    /// <summary>
    /// Puts the specified objects in the pool. Null objects within the array
    /// are silently ignored. Objects must have been obtained from this pool and
    /// not be double-freed.
    /// </summary>
    public virtual void FreeAll( IEnumerable< T? > objects )
    {
        ArgumentNullException.ThrowIfNull( objects );

        foreach ( var obj in objects )
        {
            if ( obj == null )
            {
                continue;
            }

            Free( obj );
        }
    }

    /// <summary>
    /// Removes and discards all free objects from this pool.
    /// </summary>
    public virtual void Clear()
    {
        while ( _freeObjects.TryTake( out var obj ) ) // For ConcurrentBag
        {
            DiscardObject( obj );
        }

        // _activeObjects should only contain truly active objects and shouldn't
        // be cleared here unless the pool is being shut down completely. If pool
        // is being fully reset (all objects returned to pool), then also clear
        // active objects. For a typical Clear, only free objects are removed.
    }

    /// <summary>
    /// The number of objects currently available to be obtained.
    /// </summary>
    public virtual int GetFreeObjects()
    {
        return _freeObjects.Count;
    }

    /// <summary>
    /// The number of objects currently obtained (active) from the pool.
    /// </summary>
    public virtual int GetActive()
    {
        return _activeObjects.Count;
    }

    /// <summary>
    /// Total number of objects currently managed by the pool (free + active).
    /// </summary>
    public virtual int GetTotalPooledAndActiveCount()
    {
        return _freeObjects.Count + _activeObjects.Count;
    }

    /// <summary>
    /// Resets the state of an object before it's returned to the pool or reused.
    /// </summary>
    protected virtual void ResetObject( T? obj )
    {
        if ( obj is IPoolable poolable )
        {
            poolable.Reset();
        }
    }

    /// <summary>
    /// Called when an object is discarded (e.g., pool is full, or cleared).
    /// Override this to dispose of resources held by the object.
    /// </summary>
    protected virtual void DiscardObject( T obj )
    {
        if ( obj is IDisposable disposable )
        {
            disposable.Dispose(); // Important for resource management
        }

        // Log if needed
        Logger.Debug( $"Discarding object of type {obj.GetType().Name}" );
    }

    /// <summary>
    /// Updates the peak count of pooled objects to reflect the highest number of
    /// total objects (free and active) managed by the pool at any given time.
    /// </summary>
    private void UpdatePeak()
    {
        Peak = Math.Max( Peak, GetTotalPooledAndActiveCount() ); // Or GetFree() ??
    }
}

// ============================================================================
// ============================================================================
