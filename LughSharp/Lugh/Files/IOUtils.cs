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
    [PublicAPI]
    public enum PathType
    {
        Unknown,
        File,
        Directory,
        DoesNotExist,
        Invalid,
    }

    // ========================================================================

    public static string ExternalPath => NormalizePath( Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ) );
    public static string InternalPath => NormalizePath( Directory.GetCurrentDirectory() );
    public static string LocalPath    => NormalizePath( $"{Path.PathSeparator}" );

    // ========================================================================

    public static string AssemblyPath      => NormalizePath( Assembly.GetExecutingAssembly().Location );
    public static string AssemblyDirectory => NormalizePath( Path.GetDirectoryName( AssemblyPath ) );
    public static string AssetsPath        => NormalizePath( Path.Combine( AssemblyDirectory ?? "", "Assets" ) );

    // ========================================================================

    public static string AssetsObjectsPath    => NormalizePath( $"{AssetsPath}/PackedImages/Objects/" );
    public static string AssetsAnimationsPath => NormalizePath( $"{AssetsPath}/PackedImages/Animations/" );
    public static string AssetsInputsPath     => NormalizePath( $"{AssetsPath}/PackedImages/Inputs/" );
    public static string AssetsUIPath         => NormalizePath( $"{AssetsPath}/PackedImages/UI/" );

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
            return "";
        }

        return path.Replace( '\\', Path.DirectorySeparatorChar )
                   .Replace( '/', Path.DirectorySeparatorChar );
    }

    public static bool IsDirectory( FileSystemInfo inputFileOrDir )
    {
        return ( inputFileOrDir.Attributes & FileAttributes.Directory ) != 0;
    }

    public static bool IsFile( FileSystemInfo inputFileOrDir )
    {
        return ( inputFileOrDir.Attributes & FileAttributes.Directory ) == 0;
    }

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
            Logger.Warning( $"Exception ignored: {e.Message}" );

            return PathType.Invalid;
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
        Logger.Debug( $"AssetsPath          : {AssetsPath}" );

        // --------------------------------------------------------------------
        Logger.Debug( $"AssetsObjectsPath   : {AssetsObjectsPath}" );
        Logger.Debug( $"AssetsAnimationsPath: {AssetsAnimationsPath}" );
        Logger.Debug( $"AssetsInputsPath    : {AssetsInputsPath}" );
        Logger.Debug( $"AssetsUIPath        : {AssetsUIPath}" );
    }
}