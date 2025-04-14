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
public class IOData
{
    public static readonly string ExternalPath = Environment.GetFolderPath( Environment.SpecialFolder.UserProfile );
    public static readonly string InternalPath = Directory.GetCurrentDirectory();
    public static readonly string LocalPath    = $"{Path.PathSeparator}";

    // ========================================================================

    public static readonly string? AssemblyPath      = Assembly.GetExecutingAssembly().Location;
    public static readonly string? AssemblyDirectory = Path.GetDirectoryName( AssemblyPath );
    public static readonly string? AssetsPath        = Path.Combine( AssemblyDirectory ?? "", "Assets" );

    // ========================================================================

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
        catch ( Exception e ) // Handle potential exceptions like PathTooLongException, SecurityException, etc.
        {
            Logger.Error( $"Exception ignored: {e.Message}" );
            
            return PathType.Invalid;
        }
    }

    [PublicAPI]
    public enum PathType
    {
        Unknown,
        File,
        Directory,
        DoesNotExist,
        Invalid,
    }
}