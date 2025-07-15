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
public class Files : IFiles
{
    // ========================================================================

    /// <summary>
    /// Retrieves a file handle based on the specified path and path type.
    /// </summary>
    /// <param name="path">The file path to be used for creating the file handle.</param>
    /// <param name="type">
    /// The type of the path, indicating how the file should be resolved (e.g.,
    /// Classpath, Internal, Absolute, External, or Local).
    /// </param>
    /// <returns>
    /// A <see cref="FileInfo"/> object that represents the file handle for the
    /// specified path and path type.
    /// </returns>
    public virtual FileInfo GetFileHandle( string path, PathTypes type )
    {
        if ( string.IsNullOrEmpty( path ) )
        {
            throw new ArgumentNullException( nameof( path ),
                                             "Path cannot be null or empty.\n" +
                                             "Example: 'Assets/Textures/sprite.png'" );
        }

        return type switch
        {
            PathTypes.Classpath => Classpath( path ),
            PathTypes.Internal  => Internal( path ),
            PathTypes.Absolute  => Absolute( path ),
            PathTypes.Assembly  => Assembly( path ),
            PathTypes.External  => External( path ),
            PathTypes.Local     => Local( path ),

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"Invalid path type: {type}\nValid types are: " +
                                                    $"Classpath, Internal, Absolute, " +
                                                    $"Assembly, External, Local" ),
        };
    }

    // ========================================================================
    // ========================================================================
    // Class path

    /// <summary>
    /// Convenience method that returns a <see cref="PathTypes.Classpath" /> file handle.
    /// </summary>
    public virtual FileInfo Classpath( string path )
    {
        if ( string.IsNullOrEmpty( path ) )
        {
            throw new ArgumentNullException( nameof( path ),
                                             "Classpath path cannot be null or empty.\n" +
                                             "Example: 'Resources/Textures/sprite.png'" );
        }

        path = IOUtils.NormalizePath( path );

        // Prevent path traversal attacks
        if ( path.Contains( ".." ) )
        {
            throw new GdxRuntimeException( $"Path traversal is not allowed in classpath resources: {path}\n" +
                                           "Example valid path: 'Resources/Textures/sprite.png'\n" +
                                           "Note: '../' is not allowed for security reasons. Use absolute " +
                                           "paths or paths relative to the classpath root." );
        }

        try
        {
            // First try to find the resource in the executing assembly
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            // Convert path separators to dots for resource name format
            var resourcePath = path.TrimStart( '/' ).Replace( '/', '.' ).Replace( '\\', '.' );

            // Try to find the resource in the assembly's manifest
            var manifestResourceNames = assembly.GetManifestResourceNames();
            var matchingResource = manifestResourceNames
                .FirstOrDefault( r => r.EndsWith( resourcePath, StringComparison.OrdinalIgnoreCase ) );

            if ( matchingResource != null )
            {
                // If found in assembly resources, create a FileInfo pointing to the assembly location
                var assemblyLocation = Path.GetDirectoryName( assembly.Location )
                                       ?? throw new GdxRuntimeException( "Unable to determine assembly location" );

                return new FileInfo( Path.Combine( assemblyLocation, path ) );
            }

            // If not found in assembly resources, try to find it in the application's classpath
            var classpathFile = new FileInfo( Path.Combine( AppDomain.CurrentDomain.BaseDirectory, path ) );

            if ( classpathFile.Exists )
            {
                return classpathFile;
            }

            throw new GdxRuntimeException( $"Classpath resource not found: {path}\n" +
                                           "Example valid path: 'Resources/Textures/sprite.png'\n" +
                                           "Note: Make sure the file exists and is included " +
                                           "as an embedded resource." );
        }
        catch ( Exception ex ) when ( ex is not GdxRuntimeException )
        {
            throw new GdxRuntimeException( $"Error accessing classpath resource: {path}\n" +
                                           "Example valid path: 'Resources/Textures/sprite.png'", ex );
        }
    }

    // ========================================================================
    // ========================================================================
    // Internal storage

    /// <summary>
    /// Convenience method that returns a <see cref="PathTypes.Internal" /> file handle.
    /// </summary>
    public virtual FileInfo Internal( string path )
    {
        if ( string.IsNullOrEmpty( path ) )
        {
            throw new ArgumentNullException( nameof( path ),
                                             "Internal path cannot be null or empty.\n" +
                                             "Example: 'Assets/Config/settings.json'" );
        }

        if ( !IsInternalStorageAvailable() )
        {
            throw new GdxRuntimeException( $"Internal storage is not available for path: {path}\n" +
                                           "Example valid path: 'Assets/Config/settings.json'\n" +
                                           "Note: Check if the application has proper permissions " +
                                           "and storage is mounted." );
        }

        path = IOUtils.NormalizePath( path );

        if ( path.Contains( ".." ) )
        {
            throw new GdxRuntimeException( $"Path traversal is not allowed in internal storage: {path}\n" +
                                           "Example valid path: 'Assets/Config/settings.json'\n" +
                                           "Note: '../' is not allowed for security reasons. " +
                                           "Use paths relative to internal storage root." );
        }

        var internalPath = GetInternalStoragePath();

        return new FileInfo( Path.Combine( internalPath, path ) );
    }

    /// <summary>
    /// Returns the internal storage path directory. This is the app asset directory
    /// on Android and the Applications root directory on the desktop.
    /// </summary>
    public virtual string GetInternalStoragePath()
    {
        var path = IOUtils.InternalPath;

        if ( string.IsNullOrEmpty( path ) )
        {
            throw new GdxRuntimeException( "Could not determine internal storage path" );
        }

        return path;
    }

    /// <summary>
    /// Returns true if the internal storage is ready for file IO. The return value
    /// is true by default, but backends may override this to return false if the
    /// internal storage is not available.
    /// </summary>
    public virtual bool IsInternalStorageAvailable()
    {
        try
        {
            var path = GetInternalStoragePath();

            return Directory.Exists( path );
        }
        catch
        {
            return false;
        }
    }

    // ========================================================================
    // ========================================================================
    // Absolute path

    /// <summary>
    /// Convenience method that returns a <see cref="PathTypes.Absolute" /> file handle.
    /// </summary>
    public virtual FileInfo Absolute( string path )
    {
        if ( string.IsNullOrEmpty( path ) )
        {
            throw new ArgumentNullException( nameof( path ),
                                             "Absolute path cannot be null or empty.\n" +
                                             "Example Windows: 'C:/Games/MyGame/config.json'\n" +
                                             "Example Unix: '/usr/local/share/game/config.json'" );
        }

        path = IOUtils.NormalizePath( path );

        if ( !Path.IsPathRooted( path ) )
        {
            throw new GdxRuntimeException( $"Invalid absolute path: {path}\n" +
                                           "Example Windows: 'C:/Games/MyGame/config.json'\n" +
                                           "Example Unix: '/usr/local/share/game/config.json'\n" +
                                           "Note: Absolute paths must be fully qualified from the root directory." );
        }

        return new FileInfo( path );
    }

    // ========================================================================
    // ========================================================================
    // External storage

    /// <summary>
    /// Convenience method that returns a <see cref="PathTypes.External" /> file handle.
    /// </summary>
    public virtual FileInfo External( string path )
    {
        if ( string.IsNullOrEmpty( path ) )
        {
            throw new ArgumentNullException( nameof( path ),
                                             "External path cannot be null or empty.\n" +
                                             "Example: 'Documents/GameData/saves/save1.dat'" );
        }

        if ( !IsExternalStorageAvailable() )
        {
            throw new GdxRuntimeException( $"External storage is not available for path: {path}\n" +
                                           "Example valid path: 'Documents/GameData/saves/save1.dat'\n" +
                                           "Note: Check if external storage is mounted and accessible." );
        }

        path = IOUtils.NormalizePath( path );

        if ( path.Contains( ".." ) )
        {
            throw new GdxRuntimeException( $"Path traversal is not allowed in external storage: {path}\n" +
                                           "Example valid path: 'Documents/GameData/saves/save1.dat'\n" +
                                           "Note: '../' is not allowed for security reasons. " +
                                           "Use paths relative to external storage root." );
        }

        var externalPath = GetExternalStoragePath();

        return new FileInfo( Path.Combine( externalPath, path ) );
    }

    /// <summary>
    /// Returns the external storage path directory. This is the app external storage
    /// on Android and the home directory of the current user on the desktop.
    /// </summary>
    public virtual string GetExternalStoragePath()
    {
        var path = IOUtils.ExternalPath;

        if ( string.IsNullOrEmpty( path ) )
        {
            throw new GdxRuntimeException( "Could not determine external storage path" );
        }

        return path;
    }

    /// <summary>
    /// Returns true if the external storage is ready for file IO. The return value
    /// is true by default, but backends may override this to return false if the
    /// external storage is not available.
    /// </summary>
    public virtual bool IsExternalStorageAvailable()
    {
        try
        {
            var path = GetExternalStoragePath();

            return Directory.Exists( path );
        }
        catch
        {
            return false;
        }
    }

    // ========================================================================
    // ========================================================================
    // Assembly path

    /// <summary>
    /// Convenience method that returns a <see cref="PathTypes.Assembly" /> file handle.
    /// </summary>
    public virtual FileInfo Assembly( string path )
    {
        if ( string.IsNullOrEmpty( path ) )
        {
            throw new ArgumentNullException( nameof( path ),
                                             "Assembly path cannot be null or empty.\n" +
                                             "Example: 'Resources/Shaders/default.glsl'" );
        }

        path = IOUtils.NormalizePath( path );

        if ( path.Contains( ".." ) )
        {
            throw new GdxRuntimeException( $"Path traversal is not allowed in assembly resources: {path}\n" +
                                           "Example valid path: 'Resources/Shaders/default.glsl'\n" +
                                           "Note: '../' is not allowed for security reasons. " +
                                           "Use paths relative to assembly location." );
        }

        var assemblyPath = GetAssemblyStoragePath();

        return new FileInfo( Path.Combine( assemblyPath, path ) );
    }

    /// <summary>
    /// Returns the internal storage path directory. This is the app asset directory
    /// on Android and the Applications root directory on the desktop.
    /// </summary>
    public virtual string GetAssemblyStoragePath()
    {
        var path = IOUtils.AssemblyDirectory;

        if ( string.IsNullOrEmpty( path ) )
        {
            throw new GdxRuntimeException( "Could not determine assembly storage path" );
        }

        return path;
    }

    /// <summary>
    /// Returns true if the internal storage is ready for file IO. The return value
    /// is true by default, but backends may override this to return false if the
    /// internal storage is not available.
    /// </summary>
    public virtual bool IsAssemblyStorageAvailable()
    {
        try
        {
            var path = GetAssemblyStoragePath();

            return Directory.Exists( path );
        }
        catch
        {
            return false;
        }
    }

    // ========================================================================
    // ========================================================================
    // Local storage

    /// <summary>
    /// Convenience method that returns a <see cref="PathTypes.Local" /> file handle.
    /// </summary>
    public virtual FileInfo Local( string path )
    {
        if ( string.IsNullOrEmpty( path ) )
        {
            throw new ArgumentNullException( nameof( path ),
                                             "Local path cannot be null or empty.\n" +
                                             "Example: 'UserData/preferences.xml'" );
        }

        if ( !IsLocalStorageAvailable() )
        {
            throw new GdxRuntimeException( $"Local storage is not available for path: {path}\n" +
                                           "Example valid path: 'UserData/preferences.xml'\n" +
                                           "Note: Check if local storage directory exists and is accessible." );
        }

        path = IOUtils.NormalizePath( path );

        if ( path.Contains( ".." ) )
        {
            throw new GdxRuntimeException( $"Path traversal is not allowed in local storage: {path}\n" +
                                           "Example valid path: 'UserData/preferences.xml'\n" +
                                           "Note: '../' is not allowed for security reasons. " +
                                           "Use paths relative to local storage root." );
        }

        var localPath = GetLocalStoragePath();

        return new FileInfo( Path.Combine( localPath, path ) );
    }

    /// <summary>
    /// Returns the local storage path directory. This is the private files directory
    /// on Android and the directory of the jar on the desktop.
    /// </summary>
    public virtual string GetLocalStoragePath()
    {
        var path = IOUtils.LocalPath;

        if ( string.IsNullOrEmpty( path ) )
        {
            throw new GdxRuntimeException( "Could not determine local storage path" );
        }

        return path;
    }

    /// <summary>
    /// Returns true if the local storage is ready for file IO. The return value
    /// is true by default, but backends may override this to return false if the
    /// local storage is not available.
    /// </summary>
    public virtual bool IsLocalStorageAvailable()
    {
        try
        {
            var path = GetLocalStoragePath();

            return Directory.Exists( path );
        }
        catch
        {
            return false;
        }
    }
}

// ========================================================================
// ========================================================================