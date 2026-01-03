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

namespace LughSharp.Core.Utils.Pooling;

/// <summary>
/// A pool of objects that can be reused to avoid allocation.
/// </summary>
[PublicAPI]
public class Pool< T > where T : class
{
    public const int DEFAULT_INITIAL_CAPACITY = 16;

    public int MaxFreeObjects  { get; private set; }
    public int PeakFreeObjects { get; set; }

    public delegate T? PoolObjectFactory();

    public required PoolObjectFactory NewObjectFactory;
    
    // ========================================================================

    private readonly Stack< T? > _freeObjects;

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="initialCapacity">
    /// The initial size of the array supporting the pool. No objects are created/pre-allocated.
    /// Use <see cref="Fill(int)"/> after instantiation if needed.
    /// </param>
    /// <param name="max">The maximum number of free objects to store in this pool.</param>
    public Pool( int initialCapacity = DEFAULT_INITIAL_CAPACITY, int max = int.MaxValue )
    {
        _freeObjects = new Stack< T? >( initialCapacity );
        MaxFreeObjects     = max;
    }

    /// <summary>
    /// Returns an object from this pool. The object may be new (from <see cref="NewObjectFactory"/>)
    /// or reused (previously
    /// </summary>
    public virtual T? Obtain()
    {
        return _freeObjects.Count == 0 ? NewObjectFactory() : _freeObjects.Pop();
    }

    /// <summary>
    /// Puts the specified object in the pool, making it eligible to be returned
    /// by <see cref="Obtain()"/>. If the pool already contains the maximum free
    /// objects, the specified object is discarded, it is not reset and not added
    /// to the pool.
    /// <para>
    /// The pool does not check if an object is already freed, so the same object
    /// must not be freed multiple times.
    /// </para>
    /// </summary>
    public virtual void Free( T obj )
    {
        if ( _freeObjects.Count < MaxFreeObjects )
        {
            _freeObjects.Push( obj );
            PeakFreeObjects = Math.Max( PeakFreeObjects, _freeObjects.Count );

            Reset( obj );
        }
        else
        {
            Discard( obj );
        }
    }

    /// <summary>
    /// Adds the specified number of new free objects to the pool. Usually called early
    /// on as a pre-allocation mechanism but can be used at any time.
    /// </summary>
    /// <param name="size"> the number of objects to be added. </param>
    public void Fill( int size )
    {
        for ( var i = 0; i < size; i++ )
        {
            if ( _freeObjects.Count < MaxFreeObjects )
            {
                _freeObjects.Push( NewObjectFactory() );
            }
        }

        PeakFreeObjects = Math.Max( PeakFreeObjects, _freeObjects.Count );
    }

    /// <summary>
    /// Called when an object is freed to clear the state of the object for possible
    /// later reuse. The default implementation calls <see cref="IPoolable.Reset()"/>
    /// if the object is Poolable.
    /// </summary>
    protected virtual void Reset( T? obj )
    {
        if ( ( obj != null ) && ( obj is IPoolable poolable ) )
        {
            poolable.Reset();
        }
    }

    /// <summary>
    /// Called when an object is discarded. This is the case when an object is freed,
    /// but the maximum capacity of the pool is reached, and when the pool is cleared.
    /// </summary>
    protected void Discard( T? obj )
    {
        Reset( obj );
    }

    /// <summary>
    /// Puts the specified objects in the pool. Null objects within the array are
    /// silently ignored. The pool does not check if an object is already freed, so
    /// the same object must not be freed multiple times.
    /// </summary>
    public virtual void FreeAll( List< T? > objects )
    {
        for ( int i = 0, n = objects.Count; i < n; i++ )
        {
            var obj = objects[ i ];

            if ( obj == null )
            {
                continue;
            }

            if ( _freeObjects.Count < MaxFreeObjects )
            {
                _freeObjects.Push( obj );
                Reset( obj );
            }
            else
            {
                Discard( obj );
            }
        }

        PeakFreeObjects = Math.Max( PeakFreeObjects, _freeObjects.Count );
    }

    /// <summary>
    /// Removes and discards all free objects from this pool.
    /// </summary>
    public void Clear()
    {
        foreach ( var obj in _freeObjects )
        {
            Discard( obj );
        }

        _freeObjects.Clear();
    }

    /// <summary>
    /// The number of objects available to be obtained.
    /// </summary>
    public int GetFree() => _freeObjects.Count;
}

// ============================================================================
// ============================================================================