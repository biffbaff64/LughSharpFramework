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
using System.IO;

using JetBrains.Annotations;

namespace LughSharp.Core.Files;

/// <summary>
/// Defines a service for safely navigating and resolving file system paths.
/// </summary>
[PublicAPI]
public interface IFileService
{
    /// <summary>
    /// Resolves a relative path against a base file's parent directory and ensures 
    /// the result remains within an authorized root directory.
    /// </summary>
    /// <param name="baseFile">The <see cref="FileInfo"/> representing the starting point.</param>
    /// <param name="relativePath">The path string to resolve (supports navigation like ".." or "subfolder/file.txt").</param>
    /// <param name="rootLimit">Optional. An absolute path that acts as a security boundary. 
    /// Access outside this path will throw an exception.</param>
    /// <returns>A <see cref="FileInfo"/> object for the resolved path, or <c>null</c> if inputs are invalid.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the resolved path is outside the <paramref name="rootLimit"/>.</exception>
    FileInfo? GetRelativeFileHandle(FileInfo? baseFile, string? relativePath, string? rootLimit = null);
}

// ============================================================================
// ============================================================================
