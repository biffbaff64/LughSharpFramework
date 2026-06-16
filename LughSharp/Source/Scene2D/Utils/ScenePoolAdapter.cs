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


using LughSharp.Source.Utils.Pooling;

namespace LughSharp.Source.Scene2D.Utils;

/// <summary>
/// Provides an adapter for managing pooled instances of <see cref="SceneAction"/> objects.
/// </summary>
/// <typeparam name="T">
/// Specifies the type of <see cref="SceneAction"/> managed by the pool. Must be a class
/// inheriting <see cref="SceneAction"/> and have a parameterless constructor.
/// </typeparam>
[PublicAPI]
public sealed class ScenePoolAdapter< T > : IScenePool where T : SceneAction, new()
{
    private readonly Pool< T > _inner = new()
    {
        NewObjectFactory = () => new T()
    };

    /// <summary>
    /// Retrieves an instance of the <see cref="SceneAction"/> object from the underlying pool.
    /// </summary>
    /// <returns>
    /// An instance of <see cref="SceneAction"/> of the specified type, ready for use.
    /// </returns>
    public SceneAction Obtain()
    {
        return _inner.Obtain();
    }

    /// <summary>
    /// Releases the specified <see cref="SceneAction"/> instance back to the underlying pool.
    /// </summary>
    /// <param name="action">
    /// The <see cref="SceneAction"/> instance to be released. If the provided action is of
    /// the appropriate type, it will be returned to the pool; otherwise, no action is taken.
    /// </param>
    public void Free( SceneAction action )
    {
        if ( action is T typed )
        {
            _inner.Free( typed );
        }
    }
}

// ============================================================================
// ============================================================================
