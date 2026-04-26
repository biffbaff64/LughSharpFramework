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

namespace LughSharp.Source.IO;

/// <summary>
/// Implementation of <see cref="IFiles"/> providing convenience methods for resolving file paths.
/// </summary>
[PublicAPI]
public class Files : IFiles
{
    /// <summary>
    /// The root directory for all assets.
    /// Defaults to "Assets", but can be changed to suit your project's needs.
    /// All Asset Management classes will use this path as the root for all assets.
    /// </summary>
    /// <remarks>
    /// The ContentRoot property does not include a trailing slash. If one is included
    /// when setting the property, it will be removed.
    /// <para>
    /// Correct usage:
    /// <code>
    /// Files.ContentRoot = "Content";
    /// const string AssetPath = $"{Files.ContentRoot}/Textures/sprite.png";  
    /// </code>
    /// </para>
    /// </remarks>
    public static string ContentRoot
    {
        get;
        set
        {
            if ( value.EndsWith( Path.DirectorySeparatorChar.ToString() ) )
            {
                value = value.TrimEnd( Path.DirectorySeparatorChar );
            }

            field = value;
        }
    } = "Assets";

    /// <summary>
    /// Path relative to the root of the app external storage on Android and
    /// to the home directory of the current user on the desktop.
    /// <para>
    /// An example External path would be:-
    /// <code>
    /// C:\Users\joe_blogs\
    /// </code>
    /// </para>
    /// </summary>
    public static string ExternalPath => $"{Environment.GetFolderPath( Environment.SpecialFolder.UserProfile )}\\";

    /// <summary>
    /// Path relative to the asset directory on Android and to the application's root
    /// directory on the desktop. On the desktop, if the file is not found, then the
    /// classpath is checked.
    /// <para>
    /// This is not the root folder of this framework, it is the root folder of the
    /// application that is using this framework. An example Internal path would be:-
    /// <code>
    /// C:\Development\Projects\CSharp\TestProject\bin\Debug\net8.0\
    /// </code>
    /// </para>
    /// <para>
    /// <b>Internal files are always readonly.</b>
    /// </para>
    /// </summary>
    public static string InternalPath => $"{Directory.GetCurrentDirectory()}\\";

    /// <summary>
    /// Path relative to the private files directory on Android and to the
    /// application's root directory on the desktop.
    /// </summary>
    public static string LocalPath => Path.DirectorySeparatorChar.ToString();

    /// <summary>
    /// Gets the full path of the currently executing assembly. This value is derived
    /// from the location of the assembly file on disk, provided as a normalized path
    /// with consistent separators.
    /// </summary>
    public static string AssemblyPath => System.Reflection.Assembly.GetExecutingAssembly().Location;

    /// <summary>
    /// Gets the directory path of the executing assembly, normalized to include a
    /// trailing directory separator. This value is derived from the location of the
    /// assembly currently executing.
    /// </summary>
    public static string AssemblyDirectory => Path.GetDirectoryName( AssemblyPath ) + "\\";

    /// <summary>
    /// The full path to the applicationsbase assets folder.
    /// a valid example path is:-
    /// <code>
    /// C:\Development\Projects\CSharp\TestProject\bin\Debug\net8.0\Assets\
    /// </code>
    /// </summary>
    public static string AssetsRoot => $"{AssemblyDirectory}{ContentRoot}\\";

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
    public virtual FileInfo GetFileHandle( string path, PathType type )
    {
        if ( string.IsNullOrEmpty( path ) )
        {
            throw new ArgumentNullException( nameof( path ),
                                             "Path cannot be null or empty.\n" +
                                             "Example: 'Assets/Textures/sprite.png'" );
        }

        return type switch
               {
                   PathType.Classpath => Classpath( path ),
                   PathType.Internal  => Internal( path ),
                   PathType.Absolute  => Absolute( path ),
                   PathType.Assembly  => Assembly( path ),
                   PathType.External  => External( path ),
                   PathType.Local     => Local( path ),
                   PathType.Assets    => Assets( path ),

                   // ----------------------------------

                   var _ => throw new RuntimeException( $"Invalid path type: {type}\nValid types are: " +
                                                        $"Classpath, Internal, Absolute, " +
                                                        $"Assembly, External, Local" )
               };
    }

    // ========================================================================
    // ========================================================================
    // Class path

    /// <summary>
    /// Convenience method that returns a <see cref="PathType.Classpath"/> file handle.
    /// </summary>
    public virtual FileInfo Classpath( string path )
    {
        if ( string.IsNullOrEmpty( path ) )
        {
            throw new ArgumentNullException( nameof( path ),
                                             "Classpath path cannot be null or empty.\n" +
                                             "Example: 'Resources/Textures/sprite.png'" );
        }

        // Prevent path traversal attacks
        if ( path.Contains( ".." ) )
        {
            throw new RuntimeException( $"Path traversal is not allowed in classpath resources: {path}\n" +
                                        "Example valid path: 'Resources/Textures/sprite.png'\n" +
                                        "Note: '../' is not allowed for security reasons. Use absolute " +
                                        "paths or paths relative to the classpath root." );
        }

        try
        {
            // First try to find the resource in the executing assembly
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            // Convert path separators to dots for resource name format
            string resourcePath = path.TrimStart( '/' ).Replace( '/', '.' ).Replace( '\\', '.' );

            // Try to find the resource in the assembly's manifest
            string[] manifestResourceNames = assembly.GetManifestResourceNames();
            string? matchingResource = manifestResourceNames
                .FirstOrDefault( r => r.EndsWith( resourcePath, StringComparison.OrdinalIgnoreCase ) );

            if ( matchingResource != null )
            {
                // If found in assembly resources, create a FileInfo pointing to the assembly location
                string assemblyLocation = Path.GetDirectoryName( assembly.Location )
                                       ?? throw new RuntimeException( "Unable to determine assembly location" );

                return new FileInfo( Path.Combine( assemblyLocation, path ) );
            }

            // If not found in assembly resources, try to find it in the application's classpath
            var classpathFile = new FileInfo( Path.Combine( AppDomain.CurrentDomain.BaseDirectory, path ) );

            if ( classpathFile.Exists )
            {
                return classpathFile;
            }

            throw new RuntimeException( $"Classpath resource not found: {path}\n" +
                                        "Example valid path: 'Resources/Textures/sprite.png'\n" +
                                        "Note: Make sure the file exists and is included " +
                                        "as an embedded resource." );
        }
        catch ( Exception ex ) when ( ex is not RuntimeException )
        {
            throw new RuntimeException( $"Error accessing classpath resource: {path}\n" +
                                        "Example valid path: 'Resources/Textures/sprite.png'",
                                        ex );
        }
    }

    // ========================================================================
    // ========================================================================
    // Internal storage

    /// <summary>
    /// Convenience method that returns a <see cref="PathType.Internal"/> file handle.
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
            throw new RuntimeException( $"Internal storage is not available for path: {path}\n" +
                                        "Example valid path: 'Assets/Config/settings.json'\n" +
                                        "Note: Check if the application has proper permissions " +
                                        "and storage is mounted." );
        }

        if ( path.Contains( ".." ) )
        {
            throw new RuntimeException( $"Path traversal is not allowed in internal storage: {path}\n" +
                                        "Example valid path: 'Assets/Config/settings.json'\n" +
                                        "Note: '../' is not allowed for security reasons. " +
                                        "Use paths relative to internal storage root." );
        }

        string internalPath = GetInternalStoragePath();

        return new FileInfo( Path.Combine( internalPath, path ) );
    }

    /// <summary>
    /// Returns the internal storage path directory. This is the app asset directory
    /// on Android and the Applications root directory on the desktop.
    /// </summary>
    public virtual string GetInternalStoragePath()
    {
        string path = InternalPath;

        return string.IsNullOrEmpty( path )
            ? throw new RuntimeException( "Could not determine internal storage path" )
            : path;
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
            string path = GetInternalStoragePath();

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
    /// Convenience method that returns a <see cref="PathType.Absolute"/> file handle.
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

        if ( !Path.IsPathRooted( path ) )
        {
            throw new RuntimeException( $"Invalid absolute path: {path}\n" +
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
    /// Convenience method that returns a <see cref="PathType.External"/> file handle.
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
            throw new RuntimeException( $"External storage is not available for path: {path}\n" +
                                        "Example valid path: 'Documents/GameData/saves/save1.dat'\n" +
                                        "Note: Check if external storage is mounted and accessible." );
        }

        if ( path.Contains( ".." ) )
        {
            throw new RuntimeException( $"Path traversal is not allowed in external storage: {path}\n" +
                                        "Example valid path: 'Documents/GameData/saves/save1.dat'\n" +
                                        "Note: '../' is not allowed for security reasons. " +
                                        "Use paths relative to external storage root." );
        }

        string externalPath = GetExternalStoragePath();

        return new FileInfo( Path.Combine( externalPath, path ) );
    }

    /// <summary>
    /// Returns the external storage path directory. This is the app external storage
    /// on Android and the home directory of the current user on the desktop.
    /// </summary>
    public virtual string GetExternalStoragePath()
    {
        string path = ExternalPath;

        return string.IsNullOrEmpty( path )
            ? throw new RuntimeException( "Could not determine external storage path" )
            : path;
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
            string path = GetExternalStoragePath();

            return Directory.Exists( path );
        }
        catch
        {
            return false;
        }
    }

    // ========================================================================
    // ========================================================================
    // Default Assets path

    /// <summary>
    /// Convenience method that returns a <see cref="PathType.Assets"/> file handle.
    /// Example:-
    /// <code>
    ///      Assets( "Textures/sprite.png" );
    /// </code>
    /// will return a <see cref="FileInfo"/> pointing to the file "Textures/sprite.png"
    /// at:-
    /// <code>
    ///      {AssemblyDirectory}{ContentRoot}Textures/sprite.png
    /// </code>
    /// </summary>
    /// <param name="path">The path to the file relative to the Assets directory.</param>
    /// <returns>A <see cref="FileInfo"/> pointing to the specified file.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="path"/> is null or empty.</exception>
    /// <seealso cref="AssemblyDirectory"/>
    /// <seealso cref="ContentRoot"/>
    public virtual FileInfo Assets( string path )
    {
        if ( string.IsNullOrEmpty( path ) )
        {
            throw new ArgumentNullException( nameof( path ),
                                             "Assets path cannot be null or empty.\n" +
                                             "Example: 'Textures/sprite.png'" );
        }

        if ( path.Contains( ".." ) )
        {
            throw new RuntimeException( $"Path traversal is not allowed in assets storage: {path}\n" +
                                        "Example valid path: 'Textures/sprite.png'\n" +
                                        "Note: '../' is not allowed for security reasons. " +
                                        "Use paths relative to internal storage root." );
        }

        string assetsPath = GetAssetsStoragePath();

        return new FileInfo( Path.Combine( assetsPath, path ) );
    }

    /// <summary>
    /// Returns the default Assets storage path directory.
    /// </summary>
    public virtual string GetAssetsStoragePath()
    {
        string path = AssetsRoot;

        return string.IsNullOrEmpty( path )
            ? throw new RuntimeException( "Could not determine default assets storage path" )
            : path;
    }

    // ========================================================================
    // ========================================================================
    // Assembly path

    /// <summary>
    /// Convenience method that returns a <see cref="PathType.Assembly"/> file handle.
    /// </summary>
    public virtual FileInfo Assembly( string path )
    {
        if ( string.IsNullOrEmpty( path ) )
        {
            throw new ArgumentNullException( nameof( path ),
                                             "Assembly path cannot be null or empty.\n" +
                                             "Example: 'Resources/Shaders/default.glsl'" );
        }

        if ( path.Contains( ".." ) )
        {
            throw new RuntimeException( $"Path traversal is not allowed in assembly resources: {path}\n" +
                                        "Example valid path: 'Resources/Shaders/default.glsl'\n" +
                                        "Note: '../' is not allowed for security reasons. " +
                                        "Use paths relative to assembly location." );
        }

        string assemblyPath = GetAssemblyStoragePath();

        return new FileInfo( Path.Combine( assemblyPath, path ) );
    }

    /// <summary>
    /// Returns the internal storage path directory. This is the app asset directory
    /// on Android and the Applications root directory on the desktop.
    /// </summary>
    public virtual string GetAssemblyStoragePath()
    {
        string path = AssemblyDirectory;

        return string.IsNullOrEmpty( path )
            ? throw new RuntimeException( "Could not determine assembly storage path" )
            : path;
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
            string path = GetAssemblyStoragePath();

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
    /// Convenience method that returns a <see cref="PathType.Local"/> file handle.
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
            throw new RuntimeException( $"Local storage is not available for path: {path}\n" +
                                        "Example valid path: 'UserData/preferences.xml'\n" +
                                        "Note: Check if local storage directory exists and is accessible." );
        }

        if ( path.Contains( ".." ) )
        {
            throw new RuntimeException( $"Path traversal is not allowed in local storage: {path}\n" +
                                        "Example valid path: 'UserData/preferences.xml'\n" +
                                        "Note: '../' is not allowed for security reasons. " +
                                        "Use paths relative to local storage root." );
        }

        string localPath = GetLocalStoragePath();

        return new FileInfo( Path.Combine( localPath, path ) );
    }

    /// <summary>
    /// Returns the local storage path directory. This is the private files directory
    /// on Android and the directory of the jar on the desktop.
    /// </summary>
    public virtual string GetLocalStoragePath()
    {
        string path = LocalPath;

        return string.IsNullOrEmpty( path )
            ? throw new RuntimeException( "Could not determine local storage path" )
            : path;
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
            string path = GetLocalStoragePath();

            return Directory.Exists( path );
        }
        catch
        {
            return false;
        }
    }
    
    // ========================================================================
    // ========================================================================
    // General Utility Methods

    /// <summary>
    /// Determines whether the specified <see cref="FileSystemInfo"/> represents a directory.
    /// </summary>
    /// <param name="inputFileOrDir">The <see cref="FileSystemInfo"/> to evaluate.</param>
    /// <returns>
    /// True if the specified <see cref="FileSystemInfo"/> represents a directory;
    /// otherwise, false.
    /// </returns>
    public static bool IsDirectory( FileSystemInfo inputFileOrDir )
    {
        return ( inputFileOrDir.Attributes & FileAttributes.Directory ) != 0;
    }

    /// <summary>
    /// Determines if the specified file system entry is a file.
    /// </summary>
    /// <param name="inputFileOrDir">The file system entry to check.</param>
    /// <returns>True if the entry is a file; otherwise, false.</returns>
    public static bool IsFile( FileSystemInfo inputFileOrDir )
    {
        return ( inputFileOrDir.Attributes & FileAttributes.Directory ) == 0;
    }

    /// <summary>
    /// Removes the specified file extension from the given file name, if it exists.
    /// </summary>
    /// <param name="filename">
    /// The file name or path from which the extension should be removed.
    /// </param>
    /// <param name="extension">
    /// The extension to be removed, including the leading dot (e.g., ".txt").
    /// </param>
    /// <returns>
    /// The file name without the specified extension, or the original file name if the
    /// extension does not match.
    /// </returns>
    public static string StripExtension( string filename, string extension )
    {
        if ( filename.ToLower().EndsWith( extension.ToLower() ) )
        {
            filename = filename.Substring( 0, filename.Length - extension.Length );
        }

        return filename;
    }

    /// <summary>
    /// Removes the portion of the specified path before the "Assets" directory,
    /// leaving the path starting from the "Assets" directory.
    /// </summary>
    /// <param name="path">
    /// The full path from which the "Assets" directory path will be extracted.
    /// </param>
    /// <returns>
    /// The path stripped to start from the "Assets" directory. For example, If
    /// the supplied path is <c>"C:\Projects\MyProject\Assets\Animations\"</c>, the
    /// returned path will be <c>"Assets\Animations\"</c>.
    /// </returns>
    public static string StripAssetsPath( string path )
    {
        int position = path.IndexOf( Files.ContentRoot, StringComparison.Ordinal );

        return path.Substring( position, path.Length - position );
    }

    /// <summary>
    /// Determines the type of a given path, identifying whether it is a file, a directory,
    /// does not exist, or is invalid.
    /// </summary>
    /// <param name="path">The path to evaluate.</param>
    /// <returns>
    /// A value from the <see cref="PathType"/> enumeration indicating the type of the path:
    /// <list type="bullet">
    /// <item><term>PathTypes.Directory</term> - The path is a directory.</item>
    /// <item><term>PathTypes.File</term> - The path is a file.</item>
    /// <item><term>PathTypes.DoesNotExist</term> - The path does not exist.</item>
    /// <item><term>PathTypes.Invalid</term> - The path is invalid due to exceptions or other errors.</item>
    /// <item><term>PathTypes.Unknown</term> - The path is null or empty.</item>
    /// </list>
    /// </returns>
    public static PathType GetPathType( string path )
    {
        if ( string.IsNullOrEmpty( path ) )
        {
            return PathType.Unknown;
        }

        try
        {
            if ( Directory.Exists( path ) )
            {
                return PathType.Directory;
            }

            return File.Exists( path ) ? PathType.File : PathType.DoesNotExist;
        }

        // Handle potential exceptions like PathTooLongException, SecurityException, etc.
        catch ( Exception e )
        {
            Logger.Error( $"Exception ignored: {e.Message}" );

            return PathType.Invalid;
        }
    }

    /// <summary>
    /// Validates the provided asset path by ensuring it is relative to the assets
    /// directory and normalizing it to use consistent directory separators.
    /// </summary>
    /// <param name="path">The asset path to validate and normalize.</param>
    /// <returns>The validated asset path.</returns>
    public static string AssetPath( string path )
    {
        if ( !path.Contains( Files.AssemblyDirectory ) )
        {
            return Files.AssemblyDirectory + path;
        }

        return path;
    }

    /// <summary>
    /// Creates a new file at the specified path and ensures that all necessary directories
    /// in the path are created beforehand.
    /// </summary>
    /// <param name="filePath">The full path, including file name, where the file should be created.</param>
    /// <returns>A <see cref="FileStream"/> object for the created file.</returns>
    public static FileStream CreateFullPath( string filePath )
    {
        string? directoryPath = Path.GetDirectoryName( filePath );

        if ( !string.IsNullOrEmpty( directoryPath ) )
        {
            Directory.CreateDirectory( directoryPath );
        }

        return File.Create( filePath );
    }

    /// <summary>
    /// Creates a directory if it does not exist or clears all files if the
    /// directory already exists.
    /// </summary>
    /// <param name="dir"> The directory to create or empty. </param>
    public static void CreateOrEmptyFolder( DirectoryInfo dir )
    {
        // If the directory does not exist, create it.
        if ( !Directory.Exists( dir.FullName ) )
        {
            Directory.CreateDirectory( dir.FullName );
        }
        else
        {
            // If the directory exists, remove all files in it.
            FileInfo[] files = dir.GetFiles( "*" );

            foreach ( FileInfo file in files )
            {
                file.Delete();
            }
        }
    }
    
    // ========================================================================

    #if DEBUG
    /// <summary>
    /// Writes a list of all files in the supplied directory path to console.
    /// </summary>
    public static void ListFiles( string directoryPath )
    {
        Logger.Debug( $"Listing files in directory: {directoryPath}" );
        Logger.Debug( "--------------------------------" );

        Directory.GetFiles( directoryPath ).ToList().ForEach( f => Logger.Debug( f ) );
    }

    /// <summary>
    /// Logs debug information for various application paths, including external paths,
    /// internal paths, assembly paths, and asset paths. This method is primarily used
    /// for troubleshooting and verifying path configurations during development.
    /// </summary>
    public static void DebugPaths()
    {
        Logger.Debug( $"ExternalPath        : {Files.ExternalPath}" );
        Logger.Debug( $"InternalPath        : {Files.InternalPath}" );
        Logger.Debug( $"LocalPath           : {Files.LocalPath}" );
        // --------------------------------------------------------------------
        Logger.Debug( $"ContentRoot         : {Files.ContentRoot}" );
        // --------------------------------------------------------------------
        Logger.Debug( $"AssemblyPath        : {Files.AssemblyPath}" );
        Logger.Debug( $"AssemblyDirectory   : {Files.AssemblyDirectory}" );
        Logger.Debug( $"AssetsPath          : {Files.AssetsRoot}" );
    }
    #endif
}

// ========================================================================
// ========================================================================