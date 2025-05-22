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

using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Files;

[PublicAPI]
public class AbstractFiles : IFiles
{
    // ========================================================================

    /// <summary>
    /// </summary>
    public virtual FileInfo GetFileHandle( string path, PathTypes type )
    {
        return type switch
        {
            PathTypes.Classpath => Classpath( path ),
            PathTypes.Internal  => Internal( path ),
            PathTypes.Absolute  => Absolute( path ),
            PathTypes.External  => External( path ),
            PathTypes.Local     => Local( path ),
            var _               => throw new GdxRuntimeException( $"Invalid path type: {nameof( type )}" ),
        };
    }

    // ========================================================================
    // ========================================================================
    // Class path

    /// <summary>
    /// Convenience method that returns a <see cref="PathTypes.Classpath" /> file handle.
    /// </summary>
    //TODO: Make obsolete?
    public virtual FileInfo Classpath( string path )
    {
        return new FileInfo( IOUtils.NormalizePath( path ) );
    }

    // ========================================================================
    // ========================================================================
    // Absolute path

    /// <summary>
    /// Convenience method that returns a <see cref="PathTypes.Absolute" /> file handle.
    /// </summary>
    //TODO: Make obsolete?
    public virtual FileInfo Absolute( string path )
    {
        return new FileInfo( IOUtils.NormalizePath( path ) );
    }

    // ========================================================================
    // ========================================================================
    // Assembly path

    /// <summary>
    /// Convenience method that returns a <see cref="PathTypes.Assembly" /> file handle.
    /// </summary>
    public virtual FileInfo Assembly( string path )
    {
        path = IOUtils.NormalizePath( path );
        
        var assemblyPath = GetAssemblyStoragePath();
        var prefix       = path.Contains( assemblyPath ) ? "" : assemblyPath;

        return new FileInfo( $"{prefix}{path}" );
    }

    /// <summary>
    /// Returns the internal storage path directory. This is the app asset directory
    /// on Android and the Applications root directory on the desktop.
    /// </summary>
    public virtual string GetAssemblyStoragePath()
    {
        return IOUtils.AssemblyDirectory;
    }

    /// <summary>
    /// Returns true if the internal storage is ready for file IO. The return value
    /// is true by default, but backends may override this to return false if the
    /// internal storage is not available.
    /// </summary>
    public virtual bool IsAssemblyStorageAvailable()
    {
        return true;
    }

    // ========================================================================
    // ========================================================================
    // Internal storage

    /// <summary>
    /// Convenience method that returns a <see cref="PathTypes.Internal" /> file handle.
    /// </summary>
    public virtual FileInfo Internal( string path )
    {
        path = IOUtils.NormalizePath( path );

        var internalPath = GetInternalStoragePath();
        var prefix       = path.Contains( internalPath ) ? "" : internalPath;

        return new FileInfo( $"{prefix}{path}" );
    }

    /// <summary>
    /// Returns the internal storage path directory. This is the app asset directory
    /// on Android and the Applications root directory on the desktop.
    /// </summary>
    public virtual string GetInternalStoragePath()
    {
        return IOUtils.InternalPath;
    }

    /// <summary>
    /// Returns true if the internal storage is ready for file IO. The return value
    /// is true by default, but backends may override this to return false if the
    /// internal storage is not available.
    /// </summary>
    public virtual bool IsInternalStorageAvailable()
    {
        return true;
    }

    // ========================================================================
    // ========================================================================
    // External storage

    /// <summary>
    /// Convenience method that returns a <see cref="PathTypes.External" /> file handle.
    /// </summary>
    public virtual FileInfo External( string path )
    {
        path = IOUtils.NormalizePath( path );

        var externalPath = GetExternalStoragePath();
        var prefix       = path.Contains( externalPath ) ? "" : externalPath;

        return new FileInfo( $"{prefix}{path}" );
    }

    /// <summary>
    /// Returns the external storage path directory. This is the app external storage
    /// on Android and the home directory of the current user on the desktop.
    /// </summary>
    public virtual string GetExternalStoragePath()
    {
        return IOUtils.ExternalPath;
    }

    /// <summary>
    /// Returns true if the external storage is ready for file IO. The return value
    /// is true by default, but backends may override this to return false if the
    /// external storage is not available.
    /// </summary>
    public virtual bool IsExternalStorageAvailable()
    {
        return true;
    }

    // ========================================================================
    // ========================================================================
    // Local storage

    /// <summary>
    /// Convenience method that returns a <see cref="PathTypes.Local" /> file handle.
    /// </summary>
    public virtual FileInfo Local( string path )
    {
        path = IOUtils.NormalizePath( path );

        var localPath = GetLocalStoragePath();
        var prefix    = path.Contains( localPath ) ? "" : localPath;

        return new FileInfo( $"{prefix}{path}" );
    }

    /// <summary>
    /// Returns the local storage path directory. This is the private files directory
    /// on Android and the directory of the jar on the desktop.
    /// </summary>
    public virtual string GetLocalStoragePath()
    {
        return IOUtils.LocalPath;
    }

    /// <summary>
    /// Returns true if the local storage is ready for file IO. The return value
    /// is true by default, but backends may override this to return false if the
    /// local storage is not available.
    /// </summary>
    public virtual bool IsLocalStorageAvailable()
    {
        return true;
    }
}

// ========================================================================
// ========================================================================