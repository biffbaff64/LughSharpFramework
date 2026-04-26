// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Circa64 Software Projects
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

using JetBrains.Annotations;

namespace LughSharp.Source.Assets;

/// <summary>
/// Interface representing a task to be performed on an asset.
/// </summary>
[PublicAPI]
public interface IAssetTask
{
    /// <summary>
    /// Executes the asset loading task, handling dependencies and managing the
    /// asynchronous loading process.
    /// <para>
    /// If the task is marked for cancellation, it exits early. If dependencies are
    /// not yet loaded, it calculates the dependencies for the asset and initiates
    /// dependency injection. If no dependencies are found, the asynchronous loading
    /// of the asset begins. If dependencies have already been loaded, the asynchronous
    /// loading process proceeds.
    /// </para>
    /// </summary>
    void Call();
}

// ============================================================================
// ============================================================================