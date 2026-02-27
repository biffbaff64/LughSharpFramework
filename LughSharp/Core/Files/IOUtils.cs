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

using System.Reflection;

using JetBrains.Annotations;

using LughSharp.Core.Utils.Logging;

namespace LughSharp.Core.Files;

/// <summary>
/// Utility methods for various Input/Output (I/O) operations, including file and
/// directory management, and path handling.
/// </summary>
[PublicAPI]
public class IOUtils
{
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
    /// C:\Development\Projects\CSharp\Template\bin\Debug\net8.0\
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

    // ========================================================================

    /// <summary>
    /// Gets the full path of the currently executing assembly. This value is derived
    /// from the location of the assembly file on disk, provided as a normalized path
    /// with consistent separators.
    /// </summary>
    public static string AssemblyPath => Assembly.GetExecutingAssembly().Location;

    /// <summary>
    /// Gets the directory path of the executing assembly, normalized to include a
    /// trailing directory separator. This value is derived from the location of the
    /// assembly currently executing.
    /// </summary>
    public static string AssemblyDirectory => Path.GetDirectoryName( AssemblyPath ) + "\\";

    // ========================================================================

    /// <summary>
    /// The full path to the applicationsbase assets folder.
    /// a valid example path is:-
    /// <code>
    /// C:\Development\Projects\CSharp\Template\bin\Debug\net8.0\Assets\
    /// </code>
    /// </summary>
    public static string AssetsRoot => $"{AssemblyDirectory}Assets\\";

    // ========================================================================

    /// <summary>
    /// Validates the provided asset path by ensuring it is relative to the assets
    /// directory and normalizing it to use consistent directory separators.
    /// </summary>
    /// <param name="path">The asset path to validate and normalize.</param>
    /// <returns>The validated asset path.</returns>
    public static string AssetPath( string path )
    {
        if ( !path.Contains( AssemblyDirectory ) )
        {
            return AssemblyDirectory + path;
        }

        return path;
    }

    /// <summary>
    /// Creates a new file at the specified path and ensures that all necessary directories
    /// in the path are created beforehand.
    /// </summary>
    /// <param name="filePath">The full path, including file name, where the file should be created.</param>
    /// <returns>A <see cref="FileStream"/> object for the created file.</returns>
    public static FileStream CreateFileWithDirectories( string filePath )
    {
        string? directoryPath = Path.GetDirectoryName( filePath );

        if ( !string.IsNullOrEmpty( directoryPath ) )
        {
            Directory.CreateDirectory( directoryPath );
        }

        return File.Create( filePath );
    }

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
        int position = path.IndexOf( "Assets", StringComparison.Ordinal );

        return path.Substring( position, path.Length - position );
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
        Logger.Debug( $"ExternalPath        : {ExternalPath}" );
        Logger.Debug( $"InternalPath        : {InternalPath}" );
        Logger.Debug( $"LocalPath           : {LocalPath}" );

        // --------------------------------------------------------------------
        Logger.Debug( $"AssemblyPath        : {AssemblyPath}" );
        Logger.Debug( $"AssemblyDirectory   : {AssemblyDirectory}" );
        Logger.Debug( $"AssetsPath          : {AssetsRoot}" );
    }
    #endif
}

// ============================================================================
// ============================================================================