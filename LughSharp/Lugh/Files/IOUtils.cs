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

using LughSharp.Lugh.Utils;

namespace LughSharp.Lugh.Files;

[PublicAPI]
public class IOUtils
{
    // ========================================================================

    public static string ExternalPath => NormalizePath( $"{Environment.GetFolderPath( Environment.SpecialFolder.UserProfile )}/" );
    public static string InternalPath => NormalizePath( $"{Directory.GetCurrentDirectory()}/" );
    public static string LocalPath    => NormalizePath( "/" );

    // ========================================================================

    public static string AssemblyPath      => NormalizePath( Assembly.GetExecutingAssembly().Location );
    public static string AssemblyDirectory => NormalizePath( Path.GetDirectoryName( AssemblyPath ) + "/" );

    // ========================================================================

    public static string AssetsRoot           => AssemblyDirectory;
    public static string AssetsObjectsPath    => NormalizePath( $"{AssetsRoot}/PackedImages/Objects/" );
    public static string AssetsAnimationsPath => NormalizePath( $"{AssetsRoot}/PackedImages/Animations/" );
    public static string AssetsInputsPath     => NormalizePath( $"{AssetsRoot}/PackedImages/Inputs/" );
    public static string AssetsUIPath         => NormalizePath( $"{AssetsRoot}/PackedImages/UI/" );
    public static string AssetsOutputPath     => NormalizePath( $"{AssetsRoot}/PackedImages/Output/" );
    
    // ========================================================================

    /// <summary>
    /// Uses the Replace() method to replace all occurrences of both backslashes (\) and
    /// forward slashes (/) with the platform-specific <see cref="Path.DirectorySeparatorChar"/>.
    /// </summary>
    /// <param name="path"> The path to normalize. </param>
    /// <returns> The normalized path with consistent separators. </returns>
    public static string NormalizePath( string? path )
    {
        if ( string.IsNullOrEmpty( path ) )
        {
            return $"{Path.DirectorySeparatorChar}";
        }

        return path.Replace( @"\\", Path.DirectorySeparatorChar.ToString() )
                   .Replace( '\\', Path.DirectorySeparatorChar )
                   .Replace( '/', Path.DirectorySeparatorChar );
    }

    /// <summary>
    /// Validates the provided asset path by ensuring it is relative to the assembly
    /// directory and normalizing it to use consistent directory separators.
    /// </summary>
    /// <param name="path">The asset path to validate and normalize.</param>
    /// <returns>The validated and normalized asset path.</returns>
    public static string ValidateAssetPath( string path )
    {
        Logger.Checkpoint();
        
        if ( !path.Contains( AssemblyDirectory ) )
        {
            path = AssemblyDirectory + path;
        }
        
        Logger.Debug( $"Normalized asset path: '{NormalizePath( path )}'" );

        return NormalizePath( path );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string ValidateAssetsOutputPath( string path )
    {
        if ( !path.Contains( "/PackedImages/Output" ) )
        {
            path = AssetsOutputPath + path;
        }
        
        return NormalizePath( path );
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

    public static bool IsFile( FileSystemInfo inputFileOrDir )
    {
        return ( inputFileOrDir.Attributes & FileAttributes.Directory ) == 0;
    }

    public static PathTypes GetPathType( string path )
    {
        if ( string.IsNullOrEmpty( path ) )
        {
            return PathTypes.Unknown;
        }

        try
        {
            if ( Directory.Exists( path ) )
            {
                return PathTypes.Directory;
            }

            return File.Exists( path ) ? PathTypes.File : PathTypes.DoesNotExist;
        }

        // Handle potential exceptions like PathTooLongException, SecurityException, etc.
        catch ( Exception e )
        {
            Logger.Warning( $"Exception ignored: {e.Message}" );

            return PathTypes.Invalid;
        }
    }

    public static string StripExtension( string fileName, string extension )
    {
        if ( fileName.ToLower().EndsWith( extension.ToLower() ) )
        {
            fileName = fileName.Substring( 0, fileName.Length - extension.Length );
        }

        return fileName;
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
        var position = path.IndexOf( "Assets", StringComparison.Ordinal );
        
        return path.Substring( position, path.Length - position );
    }
    
    // ========================================================================

    #if DEBUG
    public static void DebugFileList( string directoryPath )
    {
        var filesList = Directory.GetFiles( directoryPath );

        foreach ( var f in filesList )
        {
            Logger.Debug( f );
        }
    }

    public static void DebugPaths()
    {
        Logger.Debug( $"ExternalPath        : {ExternalPath}" );
        Logger.Debug( $"InternalPath        : {InternalPath}" );
        Logger.Debug( $"LocalPath           : {LocalPath}" );

        // --------------------------------------------------------------------
        Logger.Debug( $"AssemblyPath        : {AssemblyPath}" );
        Logger.Debug( $"AssemblyDirectory   : {AssemblyDirectory}" );
        Logger.Debug( $"AssetsPath          : {AssetsRoot}" );

        // --------------------------------------------------------------------
        Logger.Debug( $"AssetsObjectsPath   : {AssetsObjectsPath}" );
        Logger.Debug( $"AssetsAnimationsPath: {AssetsAnimationsPath}" );
        Logger.Debug( $"AssetsInputsPath    : {AssetsInputsPath}" );
        Logger.Debug( $"AssetsUIPath        : {AssetsUIPath}" );
    }
    #endif
}