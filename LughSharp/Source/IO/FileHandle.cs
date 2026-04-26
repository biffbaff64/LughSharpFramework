// ///////////////////////////////////////////////////////////////////////////////
// MIT License
// 
// Copyright (c) 2024, 2025, 2026 Circa64 Software Projects / Richard Ikin.
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

namespace LughSharp.Source.IO;

/// <summary>
/// Represents a handle to manage file and directory-related operations. Provides methods
/// to interact with file paths, read and write streams, and determine file or directory
/// existence.
/// </summary>
[PublicAPI]
public class FileHandle
{
    /// <summary>
    /// Represents the type of a file system path used to define the nature or origin
    /// of the path, such as whether it is an absolute path, a local directory, or a
    /// path within the assembly.
    /// Useful for distinguishing between various file handling mechanisms and scopes.
    /// </summary>
    public PathType Type { get; }

    // ========================================================================

    private readonly string _rawPath;

    // ========================================================================

    /// <summary>
    /// Creates a new <see cref="FileHandle"/> instance for the specified
    /// file path and <see cref="PathType"/>.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="type"></param>
    public FileHandle( string? path, PathType type = PathType.Internal )
    {
        Type     = type;
        _rawPath = path ?? "";
    }

    /// <summary>
    /// Creates a new <see cref="StreamWriter"/> for writing.
    /// </summary>
    /// <param name="append">
    /// true to append data to the file; false to overwrite the file. If the specified
    /// file does not exist, this parameter has no effect, and a new file is created.</param>
    /// <param name="encoding"> The character encoding to use. </param>
    /// <returns></returns>
    public StreamWriter Writer( bool append = false, Encoding? encoding = null )
    {
        encoding ??= Encoding.UTF8;

        return new StreamWriter( FullPath(), append, encoding );
    }

    /// <summary>
    /// Creates a new <see cref="StreamReader"/> for writing.
    /// </summary>
    /// <param name="encoding"> The character encoding to use. </param>
    /// <returns></returns>
    public StreamReader Reader( Encoding? encoding = null )
    {
        encoding ??= Encoding.UTF8;

        return new StreamReader( FullPath(), encoding );
    }

    /// <summary>
    /// Returns the full path of the file or directory represented by this handle.
    /// </summary>
    /// <returns></returns>
    public string FullPath()
    {
        return ResolvePath( _rawPath, Type );
    }

    /// <summary>
    /// Resolves the given path based on the specified <see cref="PathType"/>.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private string ResolvePath( string path, PathType type )
    {
        switch ( type )
        {
            case PathType.Internal:
                // Usually the 'assets' folder in the app's installation directory
                return Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "assets", path );

            case PathType.Local:
                // Private app storage (Data/Data or AppData) - Persistence
                return Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
                                     path );

            case PathType.External:
                // User's documents or shared storage
                return Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), path );

            case PathType.Absolute:
                return path;

            default:
                throw new ArgumentOutOfRangeException( nameof( type ), type, null );
        }
    }

    /// <summary>
    /// Determines whether the file or directory represented by this handle exists.
    /// </summary>
    public bool Exists()
    {
        return File.Exists( FullPath() ) || Directory.Exists( FullPath() );
    }

    /// <summary>
    /// Returns a new file handle representing a child file or directory with the
    /// specified name, relative to the current file handle's path.
    /// </summary>
    /// <param name="name">
    /// The name of the child file or directory to append to the current path. Cannot
    /// be null or contain invalid path characters.
    /// </param>
    /// <returns>
    /// A new FileHandle instance representing the child file or directory at the combined path.
    /// </returns>
    public FileHandle Child( string name )
    {
        return new FileHandle( Path.Combine( FullPath(), name ), Type );
    }

    /// <summary>
    /// Gets a new file handle representing the parent directory of the current file or directory.
    /// </summary>
    /// <remarks>
    /// Use this method to navigate to the parent directory of the current file or directory.
    /// If the current path does not have a parent (for example, if it is a root directory),
    /// the returned FileHandle may not represent a valid directory.
    /// </remarks>
    /// <returns>
    /// A new FileHandle instance for the parent directory. If the current file or directory
    /// is at the root, the returned handle may reference a null or empty path.
    /// </returns>
    public FileHandle Parent()
    {
        return new FileHandle( Path.GetDirectoryName( FullPath() ), Type );
    }

    /// <summary>
    /// Gets the file or directory name without its extension from the full path.
    /// </summary>
    /// <returns>
    /// A string containing the name of the file or directory without its extension. Returns
    /// an empty string if the path does not contain a name component.
    /// </returns>
    public string NameWithoutExtension()
    {
        return NameWithoutExtension( FullPath() );
    }

    /// <summary>
    /// Returns the file name without the extension from the specified path string.
    /// </summary>
    /// <param name="path">The path to a file. The path can be absolute or relative.</param>
    /// <returns>
    /// A string containing the file name without its extension, or an empty string if
    /// the path does not contain file information.
    /// </returns>
    public string NameWithoutExtension( string path )
    {
        return Path.GetFileNameWithoutExtension( path );
    }
}

// ============================================================================
// ============================================================================