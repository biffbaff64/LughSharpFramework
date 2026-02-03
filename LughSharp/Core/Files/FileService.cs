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

using JetBrains.Annotations;

namespace LughSharp.Core.Files;

/// <summary>
/// A concrete implementation of <see cref="IFileService"/> providing local file system path resolution.
/// </summary>
[PublicAPI]
public class FileService : IFileService
{
    /// <inheritdoc />
    public FileInfo? GetRelativeFileHandle( FileInfo? baseFile, string? relativePath, string? rootLimit = null )
    {
        // Guard against null or empty inputs
        if ( baseFile == null || string.IsNullOrWhiteSpace( relativePath ) )
        {
            return null;
        }

        // Identify the parent directory of the provided file
        var baseDirectory = baseFile.DirectoryName;

        if ( baseDirectory == null )
        {
            return null;
        }

        // Path.Combine joins the parts; Path.GetFullPath resolves the ".." and "." segments
        var combinedPath = Path.Combine( baseDirectory, relativePath );
        var resolvedPath = Path.GetFullPath( combinedPath );

        // Security check: only perform if a root limit is provided
        if ( !string.IsNullOrEmpty( rootLimit ) )
        {
            if ( !IsPathSafe( resolvedPath, rootLimit ) )
            {
                throw new UnauthorizedAccessException( $"Security Breach: The resolved path '{resolvedPath}' " +
                                                       $"is outside the allowed root directory '{rootLimit}'." );
            }
        }

        return new FileInfo( resolvedPath );
    }

    /// <summary>
    /// Validates that a resolved path sits within a specific root boundary.
    /// </summary>
    /// <param name="fullPath">The fully resolved absolute path to check.</param>
    /// <param name="rootPath">The absolute path of the allowed root directory.</param>
    /// <returns><c>true</c> if the path is within the root; otherwise, <c>false</c>.</returns>
    private bool IsPathSafe( string fullPath, string rootPath )
    {
        // Normalize the root path to ensure it is absolute
        var normalizedRoot = Path.GetFullPath( rootPath );

        // Ensure the root ends with a separator to prevent 'partial name' bypasses
        // e.g., prevents "C:\Data" from incorrectly allowing "C:\DataArchive"
        if ( !normalizedRoot.EndsWith( Path.DirectorySeparatorChar.ToString() ) )
        {
            normalizedRoot += Path.DirectorySeparatorChar;
        }

        // Compare the start of the resolved path with the normalized root
        return fullPath.StartsWith( normalizedRoot, StringComparison.OrdinalIgnoreCase );
    }
}

// ============================================================================
// ============================================================================