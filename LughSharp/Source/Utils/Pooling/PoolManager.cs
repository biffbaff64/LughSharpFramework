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

namespace LughSharp.Source.Utils.Pooling;

[PublicAPI]
public class PoolManager
{
//    private readonly Dictionary< Type, Pool< Type > > _typePools = [ ];
//    
//    // ========================================================================
//
//    /** Registers a new pool with the given supplier. Will throw an exception, if a pool for the same class is already registered.
//     * This can be used like `PoolManager#addPoll(MyClass::new);` */
//    public void AddPool<T>( Type poolClass, IPoolSupplier< T > poolSupplier )
//    {
//        addPool( poolClass, new DefaultPool<>( poolSupplier ) );
//    }
//
//    /** Registers the new pool. Will throw an exception, if a pool for the same class is already registered */
//    public <T> void addPool( Type poolClass, Pool< T > pool )
//    {
//        Pool < ?> oldPool = typePools.put( poolClass, pool );
//
//        if ( oldPool != null )
//        {
//            throw new GdxRuntimeException( "Attempt to add pool with already existing class: " + poolClass
//                                         + ", register using PoolManager#addPool(" + poolClass.getSimpleName() + ", "
//                                         + poolClass.getSimpleName() + "::new)" );
//        }
//    }
//
//    /** Returns the pool registered for the class. Will throw an exception, if no pool for this class is registered */
//    public <T> Pool< T > getPool( Type clazz )
//    {
//        Pool< T > pool = ( Pool< T > )typePools.get( clazz );
//
//        if ( pool == null )
//        {
//            throw new GdxRuntimeException( "Attempt to get pool with unknown class: " + clazz
//                                         + ", register using PoolManager#addPool(" + clazz.getSimpleName() + "::new)" );
//        }
//
//        return pool;
//    }
//
//    /** Returns the pool registered for the class. Will return null, if no pool for this class is registered */
//    public <T> Pool< T > getPoolOrNull( Type clazz )
//    {
//        return ( Pool< T > )typePools.get( clazz );
//    }
//
//    /** Whether a pool for this class is already registered */
//    public boolean hasPool( Class<?> clazz )
//    {
//        return typePools.containsKey( clazz );
//    }
//
//    /** Returns a new pooled object for the class. Will throw an exception, if no pool for this class is registered. Free with
//     * {@link PoolManager#free} */
//    public <T> T obtain( Type clazz )
//    {
//        Pool< T > pool = ( Pool< T > )typePools.get( clazz );
//
//        if ( pool == null )
//        {
//            throw new GdxRuntimeException( "Attempt to get pooled object with unknown class: " + clazz
//                                         + ", register using PoolManager#addPool(" + clazz.getSimpleName() + "::new)" );
//        }
//
//        return pool.obtain();
//    }
//
//    /** Returns a new pooled object for the class. Will return null, if no pool for this class is registered. Free with
//     * {@link PoolManager#free} */
//    public <T> T obtainOrNull( Type clazz )
//    {
//        Pool< T > pool = ( Pool< T > )typePools.get( clazz );
//
//        if ( pool == null )
//        {
//            return null;
//        }
//
//        return pool.obtain();
//    }
//
//    /** Frees a pooled object. Will throw an exception, if no pool for this class is registered. It is unchecked, whether the
//     * object was obtained by the registered pool. */
//    public <T> void free( T object) {
//        Pool< T > pool = ( Pool< T > )typePools.get( object.getClass() );
//
//        if ( pool == null )
//        {
//            throw new GdxRuntimeException( "Attempt to free pooled object with unknown class: " + object.getClass()
//                                         + ", register using PoolManager#addPool(" + object.getClass().getSimpleName()
//                                         + "::new)" );
//        }
//
//        pool.free( object );
//    }
//
//    /** Clears all contents of the managed pools */
//    public void clear()
//    {
//        for ( Pool < ?> pool :
//        typePools.values()) {
//            pool.clear();
//        }
//    }
}

// ============================================================================
// ============================================================================