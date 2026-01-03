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

namespace LughSharp.Core.Utils;

/// <summary>
/// Provides a base class for factory implementations that require initialization logic.
/// </summary>
[PublicAPI]
public abstract class BaseClassFactory
{
    /// <summary>
    /// When implemented in a derived class, performs custom initialization logic.
    /// </summary>
    protected abstract void InitializeInternal();

    /// <summary>
    /// Creates an instance of the specified derived class, calls its initialization logic,
    /// and returns the initialized instance.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the derived class to instantiate. Must inherit from <see cref="BaseClassFactory"/>
    /// and have a parameterless constructor.
    /// </typeparam>
    /// <returns>
    /// An initialized instance of the specified derived class.
    /// </returns>
    public static T Create<T>() where T : BaseClassFactory, new()
    {
        var instance = new T();
        instance.InitializeInternal();
        return instance;
    }
}

// ========================================================================
// ========================================================================