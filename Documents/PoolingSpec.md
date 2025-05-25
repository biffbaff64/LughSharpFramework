Key Problems with Your Current Pools<T> and Pool<T> Implementations
Pools<T> (Static Class)
Incorrect T in Pools<T>: The biggest issue is that Pools<T> is a generic class itself. When you do Pools<T>.Get(), typeof(T) refers to the generic parameter of the static Pools class, not the type you're trying to pool.

Example: Pools<MyObject>.Obtain() will always try to get or create a Pool<MyObject> instance, but Pools<AnotherObject>.Obtain() will operate on a different static instance of Pools<T> which also uses Pool<MyObject>. This is a type system misuse for static pools.
Result: You'll only ever manage pools for one specific type (MyObject in the example above) across your entire application, determined by the first type you instantiate Pools<T> with.
Fix: Pools should likely not be generic, or it should be a non-generic static class that manages Dictionary<Type, object> where the objects are Pool<SomeType>. This is the most common pattern for static pool managers.
Dictionary<Type, Pool<T>?> _typePools Issues:

_typePools[typeof(T)] will throw a KeyNotFoundException if the key doesn't exist. You need TryGetValue or ContainsKey checks.
Put extension method: While not shown, this implies custom extension. Standard Dictionary access would be _typePools[typeof(T)] = pool;.
Setting _typePools[typeof(T)] = null; in FreeAll(..., !samePool) is extremely dangerous. It removes the entire pool for that type, potentially leaving active objects without a pool to return to, or causing subsequent Obtain() calls to create a new pool, leading to memory leaks and confusion.
FreeAll(List<T> objects, bool samePool = false) Logic:

The !samePool logic is deeply flawed. If samePool is false, it essentially says "after freeing the first object, discard the pool completely for this type." This is almost certainly not intended behavior and will lead to numerous bugs.
It also ignores the Discard method of the individual Pool<T> instances.
No Constructor Constraint for T: Your Pool<T> relies on NewObject(), but your Pools<T> class doesn't enforce that T must have a parameterless constructor (where T : new()) or provide a factory. This is handled partially by the NewObjectHandler delegate, but the static Pools<T> class doesn't enforce or provide this.

Pool<T> (Instance Class)
NewObject Delegate is null!:

NewObject = null!; means it's uninitialized and will result in a NullReferenceException when Obtain() or Fill() is called before NewObject is assigned.
There's no public setter for NewObject on Pool<T>, meaning it can only be set via the Pools<T>.Set method, which is itself flawed. A pool should ideally be created with its NewObjectHandler in its constructor.
Obtain() Behavior:

_freeObjects[^1] = default(T)!; This line sets the last element in the _freeObjects list to its default value (null for reference types). This is unnecessary if you immediately RemoveAt(Count - 1) or Pop() it. If you don't remove it, you'll have null entries in your _freeObjects list.
The Logger.Warning("Pool is empty, creating new object.") is good, but consider if this should be an error or if the pool should have options to grow or wait.
Free() Method:

"The pool does not check if an object is already freed, so the same object must not be freed multiple times." This is a critical flaw. If an object is freed multiple times, it will be added to _freeObjects multiple times, leading to:
The same object being Obtained multiple times.
The pool size growing indefinitely with "phantom" objects.
Memory leaks.
A robust pool must detect and prevent double-freeing. This usually involves tracking active objects.
FreeAll() Method:

Similar to Free(), it doesn't check for double-freeing.
The Reset(obj) call within Free() and FreeAll() is generally good, ensuring objects are clean before being reused.
Clear() Method:

_freeObjects.RemoveAt(i) inside a for loop that iterates with i++ while simultaneously removing elements is incorrect and will skip elements. You need to iterate backward or use a while loop with an enumerator or clear and then discard.
No Thread Safety: Neither class appears to have any thread synchronization. If Obtain() and Free() are called from multiple threads, you'll have race conditions, corrupted internal state, and crashes.

How to Rewrite: A Modern & Robust Approach
I'll propose a structure that addresses these issues. We'll separate the static manager from the individual pool.

1. IPoolable Interface (for objects that can be reset)
   This is already good.

C#

public interface IResetable
{
void Reset();
}
2. Pool<T> Class (The actual object pool)
   This needs to be responsible for creating, holding, and managing objects.

C#

using System;
using System.Collections.Concurrent; // For thread-safe alternatives if needed
using System.Collections.Generic;
using System.Linq; // For debugging/linq if needed

// Interface for types that can be managed by the pool and need resetting
public interface IPoolable
{
void Reset();
}

public class Pool<T>
{
public const int DefaultInitialCapacity = 16;

    // Delegate for creating new instances of T
    public delegate T NewObjectHandler();

    // --- Public Properties ---
    public int Max { get; } // The maximum number of objects that will be pooled.
    public int Peak { get; private set; } // The highest number of objects ever active (obtained + free).
                                          // Or, highest number of free objects, depending on what Peak means to you.
                                          // The current definition (highest number of free objects) is less common,
                                          // but if that's what you need, it's fine.

    // --- Private Fields ---
    private readonly ConcurrentBag<T> _freeObjects; // ConcurrentBag is thread-safe for add/take operations
                                                    // if you anticipate multi-threaded access.
                                                    // If single-threaded, a List<T> is fine, but needs to be managed carefully.
    private readonly NewObjectHandler _newObjectFactory; // The factory to create new objects.

    // Using a HashSet to track active objects allows checking for double-freeing.
    // This adds overhead, but is crucial for correctness.
    private readonly ConcurrentDictionary<T, bool> _activeObjects; // For thread-safe tracking
                                                                    // If single-threaded, a HashSet<T> is fine.

    // --- Constructor ---
    public Pool(NewObjectHandler newObjectFactory, int initialCapacity = DefaultInitialCapacity, int max = int.MaxValue)
    {
        _newObjectFactory = newObjectFactory ?? throw new ArgumentNullException(nameof(newObjectFactory), "NewObjectHandler cannot be null.");
        Max = max;

        // Choose between ConcurrentBag (thread-safe) or List<T> (single-threaded, better perf)
        _freeObjects = new ConcurrentBag<T>(); // Or new List<T>(initialCapacity);
        _activeObjects = new ConcurrentDictionary<T, bool>(); // Or new HashSet<T>();

        // Pre-fill the pool if initialCapacity > 0
        if (initialCapacity > 0)
        {
            Fill(initialCapacity);
        }
    }

    // --- Core Methods ---

    /// <summary>
    /// Returns an object from this pool. The object may be new or reused.
    /// Returns null if the pool is exhausted and cannot create new objects (e.g., if Max is reached and no objects are free).
    /// </summary>
    public virtual T Obtain()
    {
        T obj;

        // Try to get from free objects first
        if (!_freeObjects.TryTake(out obj)) // Use TryTake for ConcurrentBag
        {
            // Pool is empty, create a new object
            if (GetTotalPooledAndActiveCount() >= Max)
            {
                // Pool is at max capacity and no free objects.
                // Depending on policy: throw, return null, or block.
                // For now, returning null as per original question.
                Console.WriteLine($"[WARNING] Pool for type {typeof(T).Name} exhausted. Cannot obtain object.");
                return default(T); // Returning default(T) (null for reference types)
            }

            try
            {
                obj = _newObjectFactory();
                if (obj == null) // Factory itself returned null (e.g., due to creation failure)
                {
                    Console.WriteLine($"[ERROR] NewObjectHandler for type {typeof(T).Name} returned null.");
                    return default(T);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to create new object for type {typeof(T).Name}: {ex.Message}");
                return default(T);
            }
        }

        // Add to active objects tracking for double-free check
        if (!_activeObjects.TryAdd(obj, true)) // For ConcurrentDictionary
        {
            // This should ideally not happen if pool is correctly managed, but indicates an issue.
            // Means an object is being obtained but already marked as active.
            Console.WriteLine($"[WARNING] Obtained object {obj?.GetType().Name} was already marked as active. Potential pool misuse.");
            // You might want to re-add to free and try again, or throw. For now, proceeding.
        }

        // Reset the object before returning
        ResetObject(obj);

        UpdatePeak(); // Update peak after obtaining an object if needed

        return obj;
    }

    /// <summary>
    /// Adds the specified number of new free objects to the pool.
    /// Usually called early on as a pre-allocation mechanism.
    /// </summary>
    /// <param name="size">The number of objects to be added.</param>
    public void Fill(int size)
    {
        for (var i = 0; i < size; i++)
        {
            if (GetFree() >= Max) break; // Don't exceed max capacity

            try
            {
                var obj = _newObjectFactory();
                if (obj == null)
                {
                    Console.WriteLine($"[ERROR] NewObjectHandler for type {typeof(T).Name} returned null during Fill.");
                    break; // Stop filling if factory returns null
                }
                ResetObject(obj); // Reset new objects before adding to free list
                _freeObjects.Add(obj); // For ConcurrentBag
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to create object during Fill for type {typeof(T).Name}: {ex.Message}");
                break; // Stop filling if creation fails
            }
        }
        UpdatePeak();
    }

    /// <summary>
    /// Puts the specified object in the pool, making it eligible to be returned by Obtain.
    /// If the pool already contains Max free objects, the specified object is discarded.
    /// Throws ArgumentException if the object was not obtained from this pool or is being double-freed.
    /// </summary>
    public virtual void Free(T obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        // Crucial: Check if the object was actually obtained from this pool and is not being double-freed.
        if (!_activeObjects.TryRemove(obj, out _)) // For ConcurrentDictionary
        {
            Console.WriteLine($"[WARNING] Attempted to free object {obj.GetType().Name} that was not obtained from this pool or was already freed.");
            // Depending on strictness, you might throw an exception here:
            // throw new InvalidOperationException($"Attempted to free object {obj.GetType().Name} that was not obtained from this pool or was already freed.");
            return; // Don't add to free list if it wasn't active
        }

        if (GetFree() < Max)
        {
            ResetObject(obj); // Reset before adding back to pool
            _freeObjects.Add(obj); // For ConcurrentBag
        }
        else
        {
            DiscardObject(obj);
        }
    }

    /// <summary>
    /// Puts the specified objects in the pool. Null objects within the array are silently ignored.
    /// Objects must have been obtained from this pool and not be double-freed.
    /// </summary>
    public virtual void FreeAll(IEnumerable<T> objects)
    {
        ArgumentNullException.ThrowIfNull(objects);

        foreach (var obj in objects)
        {
            if (obj == null) continue;
            Free(obj); // Use the Free method to ensure checks and discarding logic
        }
    }

    /// <summary>
    /// Removes and discards all free objects from this pool.
    /// </summary>
    public virtual void Clear()
    {
        while (_freeObjects.TryTake(out T obj)) // For ConcurrentBag
        {
            DiscardObject(obj);
        }
        // _activeObjects should only contain truly active objects
        // and shouldn't be cleared here unless the pool is being shut down completely.
        // If pool is being fully reset (all objects returned to pool), then also clear active objects.
        // For a typical Clear, only free objects are removed.
    }

    /// <summary>
    /// The number of objects currently available to be obtained.
    /// </summary>
    public virtual int GetFree()
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

    // --- Internal/Protected Helper Methods ---

    /// <summary>
    /// Resets the state of an object before it's returned to the pool or reused.
    /// </summary>
    protected virtual void ResetObject(T obj)
    {
        if (obj is IPoolable poolable)
        {
            poolable.Reset();
        }
    }

    /// <summary>
    /// Called when an object is discarded (e.g., pool is full, or cleared).
    /// Override this to dispose of resources held by the object.
    /// </summary>
    protected virtual void DiscardObject(T obj)
    {
        if (obj is IDisposable disposable)
        {
            disposable.Dispose(); // Important for resource management
        }
        // Log if needed
        Console.WriteLine($"[INFO] Discarding object of type {obj?.GetType().Name}");
    }

    private void UpdatePeak()
    {
        Peak = Math.Max(Peak, GetTotalPooledAndActiveCount()); // Or GetFree() depending on your definition
    }
}
3. Pools (Static Pool Manager - NON-GENERIC)
   This static class will manage pools for different types.

C#

using System;
using System.Collections.Concurrent; // For thread-safe dictionary
using System.Collections.Generic;

// PublicAPI attribute can be kept if you wish
[PublicAPI]
public static class Pools
{
// Use a ConcurrentDictionary for thread-safe access to the pools themselves.
// Store object as base Pool<object> or dynamic, cast when retrieving.
private static readonly ConcurrentDictionary<Type, object> _typePools = new ConcurrentDictionary<Type, object>();

    // --- Public Methods ---

    /// <summary>
    /// Returns a new or existing pool for the specified type.
    /// If a pool for the type already exists, its existing instance is returned,
    /// and 'max' and 'initialCapacity' parameters are ignored.
    /// </summary>
    /// <typeparam name="T">The type of object to pool.</typeparam>
    /// <param name="newObjectFactory">A function to create new objects when the pool needs them.</param>
    /// <param name="initialCapacity">Initial capacity of the pool if a new one is created.</param>
    /// <param name="max">Maximum number of objects to store in the pool if a new one is created.</param>
    /// <returns>The Pool instance for the specified type.</returns>
    /// <exception cref="ArgumentNullException">Thrown if newObjectFactory is null when creating a new pool.</exception>
    public static Pool<T> Get<T>(Pool<T>.NewObjectHandler newObjectFactory, int initialCapacity = Pool<T>.DefaultInitialCapacity, int max = int.MaxValue)
    {
        // GetOrAdd is thread-safe for creating or retrieving.
        return (Pool<T>)_typePools.GetOrAdd(typeof(T), type =>
        {
            // This factory method is called only if the key (type) is not already present.
            // It creates a new Pool<T> instance.
            return new Pool<T>(newObjectFactory, initialCapacity, max);
        });
    }

    /// <summary>
    /// Sets an existing pool for the specified type. Useful for custom pool implementations.
    /// Overwrites any existing pool for this type.
    /// </summary>
    /// <typeparam name="T">The type of object the pool manages.</typeparam>
    /// <param name="pool">The Pool instance to set.</param>
    /// <exception cref="ArgumentNullException">Thrown if pool is null.</exception>
    public static void Set<T>(Pool<T> pool)
    {
        ArgumentNullException.ThrowIfNull(pool);
        _typePools[typeof(T)] = pool; // Overwrites if exists, adds if not
    }

    /// <summary>
    /// Obtains an object of the specified type from its corresponding pool.
    /// The pool must have been registered via Get or Set methods.
    /// </summary>
    /// <typeparam name="T">The type of object to obtain.</typeparam>
    /// <returns>An object from the pool, or default(T) if the pool is exhausted or creation failed.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no pool is registered for the specified type.</exception>
    public static T Obtain<T>()
    {
        if (!_typePools.TryGetValue(typeof(T), out var poolObject))
        {
            throw new InvalidOperationException($"No pool registered for type {typeof(T).Name}. Call Pools.Get<{typeof(T).Name}>() first with a NewObjectHandler.");
        }
        var pool = (Pool<T>)poolObject;
        return pool.Obtain();
    }

    /// <summary>
    /// Frees an object of the specified type back to its corresponding pool.
    /// The pool must have been registered.
    /// </summary>
    /// <typeparam name="T">The type of the object to free.</typeparam>
    /// <param name="obj">The object to free.</param>
    /// <exception cref="InvalidOperationException">Thrown if no pool is registered for the specified type.</exception>
    public static void Free<T>(T obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        if (!_typePools.TryGetValue(typeof(T), out var poolObject))
        {
            throw new InvalidOperationException($"No pool registered for type {typeof(T).Name}. Cannot free object.");
        }
        var pool = (Pool<T>)poolObject;
        pool.Free(obj);
    }

    /// <summary>
    /// Frees a collection of objects of the specified type back to their corresponding pool.
    /// The pool must have been registered. Null objects in the list are ignored.
    /// </summary>
    /// <typeparam name="T">The type of the objects to free.</typeparam>
    /// <param name="objects">The collection of objects to free.</param>
    /// <exception cref="InvalidOperationException">Thrown if no pool is registered for the specified type.</exception>
    public static void FreeAll<T>(IEnumerable<T> objects)
    {
        ArgumentNullException.ThrowIfNull(objects);

        if (!_typePools.TryGetValue(typeof(T), out var poolObject))
        {
            throw new InvalidOperationException($"No pool registered for type {typeof(T).Name}. Cannot free objects.");
        }
        var pool = (Pool<T>)poolObject;
        pool.FreeAll(objects);
    }

    /// <summary>
    /// Clears the pool for a specific type, discarding all free objects.
    /// </summary>
    /// <typeparam name="T">The type of objects in the pool to clear.</typeparam>
    public static void Clear<T>()
    {
        if (_typePools.TryGetValue(typeof(T), out var poolObject))
        {
            ((Pool<T>)poolObject).Clear();
        }
    }

    /// <summary>
    /// Clears all registered pools.
    /// </summary>
    public static void ClearAllPools()
    {
        foreach (var entry in _typePools.Values)
        {
            if (entry is IClearablePool clearablePool) // You might need a non-generic interface for Clear()
            {
                clearablePool.Clear();
            }
            // Alternatively, cast to dynamic and call Clear() if you're sure it exists.
            // ((dynamic)entry).Clear();
        }
        _typePools.Clear(); // Remove all entries from the dictionary
    }

    // Helper interface for ClearAllPools if pools don't derive from a common non-generic base
    private interface IClearablePool
    {
        void Clear();
    }
    // Make Pool<T> implement this if you want ClearAllPools to work cleanly
    // public class Pool<T> : IClearablePool // ... and then in Pool<T>
    // {
    //     void IClearablePool.Clear() { Clear(); }
    // }
}
How to Use the Rewritten Pools
C#

public class MyGameObject : IPoolable // Implement the interface for objects that need resetting
{
public string Name { get; set; }
public bool IsActive { get; set; }

    public MyGameObject()
    {
        Console.WriteLine("MyGameObject created!");
    }

    public void Reset()
    {
        Name = "Default";
        IsActive = false;
        Console.WriteLine("MyGameObject reset!");
    }

    public override string ToString()
    {
        return $"MyGameObject: Name='{Name}', Active={IsActive}";
    }
}

public class ExampleUsage
{
public static void Run()
{
// 1. Get/Register the pool for MyGameObject.
// You MUST provide the factory function here for the static manager.
var myGameObjectPool = Pools.Get<MyGameObject>(() => new MyGameObject(), initialCapacity: 5, max: 10);

        Console.WriteLine($"\n--- Initial Pool State ---");
        Console.WriteLine($"Free: {myGameObjectPool.GetFree()}");
        Console.WriteLine($"Active: {myGameObjectPool.GetActive()}");
        Console.WriteLine($"Total: {myGameObjectPool.GetTotalPooledAndActiveCount()}");


        // 2. Obtain objects
        Console.WriteLine("\n--- Obtaining Objects ---");
        var obj1 = Pools.Obtain<MyGameObject>();
        obj1.Name = "Object 1";
        obj1.IsActive = true;
        Console.WriteLine(obj1);

        var obj2 = Pools.Obtain<MyGameObject>();
        obj2.Name = "Object 2";
        obj2.IsActive = true;
        Console.WriteLine(obj2);

        Console.WriteLine($"\n--- Pool State After Obtaining ---");
        Console.WriteLine($"Free: {myGameObjectPool.GetFree()}");
        Console.WriteLine($"Active: {myGameObjectPool.GetActive()}");
        Console.WriteLine($"Total: {myGameObjectPool.GetTotalPooledAndActiveCount()}");


        // 3. Free objects
        Console.WriteLine("\n--- Freeing Objects ---");
        Pools.Free(obj1);
        // Pools.Free(obj1); // Uncommenting this will trigger the double-free warning!

        Console.WriteLine($"\n--- Pool State After Freeing obj1 ---");
        Console.WriteLine($"Free: {myGameObjectPool.GetFree()}");
        Console.WriteLine($"Active: {myGameObjectPool.GetActive()}");
        Console.WriteLine($"Total: {myGameObjectPool.GetTotalPooledAndActiveCount()}");


        // 4. Test Pool Exhaustion (if Max is hit and no free objects)
        Console.WriteLine("\n--- Testing Exhaustion ---");
        // Get 8 more objects (total 10 with obj2 and obj1 now free)
        var objects = new List<MyGameObject>();
        for (int i = 0; i < 8; i++)
        {
            objects.Add(Pools.Obtain<MyGameObject>());
        }

        Console.WriteLine($"\n--- Pool State Near Max Capacity ---");
        Console.WriteLine($"Free: {myGameObjectPool.GetFree()}");
        Console.WriteLine($"Active: {myGameObjectPool.GetActive()}");
        Console.WriteLine($"Total: {myGameObjectPool.GetTotalPooledAndActiveCount()}");

        var objExhausted = Pools.Obtain<MyGameObject>(); // This should return null or log warning
        if (objExhausted == null)
        {
            Console.WriteLine("Successfully handled pool exhaustion: objExhausted is null.");
            // Here you'd decide to create a new object or handle the failure
        }

        // 5. Free all at once
        Console.WriteLine("\n--- Freeing All ---");
        objects.Add(obj2); // Add obj2 back to the list of things to free
        Pools.FreeAll(objects);

        Console.WriteLine($"\n--- Pool State After Freeing All ---");
        Console.WriteLine($"Free: {myGameObjectPool.GetFree()}");
        Console.WriteLine($"Active: {myGameObjectPool.GetActive()}");
        Console.WriteLine($"Total: {myGameObjectPool.GetTotalPooledAndActiveCount()}");

        // 6. Clear pool
        Console.WriteLine("\n--- Clearing Pool ---");
        Pools.Clear<MyGameObject>();
        Console.WriteLine($"Free: {myGameObjectPool.GetFree()}");
        Console.WriteLine($"Active: {myGameObjectPool.GetActive()}");
        Console.WriteLine($"Total: {myGameObjectPool.GetTotalPooledAndActiveCount()}");

        // Clear all pools (if you implement IClearablePool)
        // Pools.ClearAllPools();
    }
}
Testing Strategy
Once you've refactored, comprehensive unit tests are crucial. Here are some test cases:

Basic Obtain/Free: Get an object, ensure it's not null, free it, ensure Free() works.
Initial Capacity & Fill: Test Fill() to ensure it pre-populates correctly without exceeding Max.
Max Capacity:
Obtain objects up to Max.
Attempt to Obtain() one more time and verify it returns null (or whatever your exhaustion policy is).
Double-Freeing: Obtain() an object, Free() it, then try to Free() it again. Verify it logs a warning/throws an exception and doesn't add it to the free list twice.
Freeing Unobtained Objects: Try to Free() an object that was never obtained from the pool.
NewObjectHandler Failure: Provide a factory that returns null or throws an exception, and verify the pool handles it gracefully (e.g., returns null from Obtain()).
Reset Method: Ensure Reset() is called on IPoolable objects when obtained and when freed (if you choose to reset on free).
Discard Method: Verify Discard() is called when Max is reached during Free() or when Clear() is called.
FreeAll: Test FreeAll with a list containing nulls, duplicates, and objects from other sources.
Thread Safety (if using Concurrent collections): Run Obtain() and Free() from multiple threads concurrently and verify no data corruption or crashes. This is complex but vital for multi-threaded applications.
Pools Static Manager:
Verify Get<T>() returns the same pool instance for the same T.
Verify Obtain<T>() throws if no pool is registered for T.
Verify Set<T>() correctly replaces a pool.
Test ClearAllPools().
Rewriting these will give you a much more robust, predictable, and maintainable object pooling system. Good luck!

