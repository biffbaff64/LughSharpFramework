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
/// A <c>Pool</c> which keeps track of the obtained items (see <see cref="Obtain"/>),
/// which can be freed all at once using the <see cref="Flush()"/> method.
/// </summary>
[PublicAPI]
public class FlushablePool< T > : Pool< T > where T : class
{
    protected List< T > Obtained = new();

    private readonly Func< T >? _factory;

    // ========================================================================

    /// <summary>
    /// Creates a new pool with the specified maximum pool size and initial capacity.
    /// </summary>
    /// <param name="initialCapacity"></param>
    public FlushablePool( int initialCapacity )
        : base( initialCapacity )
    {
    }

    /// <summary>
    /// Creates a new pool with the specified maximum pool size, initial capacity and maximum size.
    /// </summary>
    /// <param name="initialCapacity"></param>
    /// <param name="max"></param>
    public FlushablePool( int initialCapacity, int max )
        : base( initialCapacity, max )
    {
    }

    /// <summary>
    /// Obtains an instance from the pool.
    /// </summary>
    /// <returns> The instance, or null if it was not possible to obtain one. </returns>
    public override T? Obtain()
    {
        var result = base.Obtain();

        if ( result != null )
        {
            Obtained.Add( result );
        }

        return result;
    }

    /// <summary>
    /// Frees all obtained instances. */
    /// </summary>
    public void Flush()
    {
        base.FreeAll( Obtained! );

        Obtained.Clear();
    }

    /// <summary>
    /// Frees the specified object obtained from the pool. Removes the freed
    /// object from the list of currently obtained items.
    /// </summary>
    /// <param name="obj"></param>
    public override void Free( T obj )
    {
        Obtained.Remove( obj );
        base.Free( obj );
    }

    /// <summary>
    /// Frees all the specified objects obtained from the pool. Null objects
    /// and objects not obtained from this pool are ignored. Removes the freed
    /// objects from the list of currently obtained items.
    /// </summary>
    /// <param name="objects">The list of objects to be freed.</param>
    public override void FreeAll( List< T? > objects )
    {
        // If we are freeing everything, just clear the list!
        if ( objects.Count == Obtained.Count )
        {
            Obtained.Clear();
        }
        else
        {
            var hashSet = new HashSet< T? >( objects );

            // Iterate backwards when removing to keep indices stable
            for ( var i = Obtained.Count - 1; i >= 0; i-- )
            {
                if ( hashSet.Contains( Obtained[ i ] ) )
                {
                    Obtained.RemoveAt( i );
                }
            }
        }

        base.FreeAll( objects );
    }
}

// ============================================================================
// ============================================================================