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

using System;
using System.IO;
using System.Text;

using JetBrains.Annotations;

namespace LughSharp.Core.Files;

[PublicAPI]
public class FileHandle
{
    public PathType Type     { get; }

    // ========================================================================
    
    private readonly string _rawPath;
    
    // ========================================================================

    public FileHandle( string? path, PathType type = PathType.Internal )
    {
        Type     = type;
        _rawPath = path ?? "";
    }

    public StreamWriter Writer( bool append = false, Encoding? encoding = null )
    {
        encoding ??= Encoding.UTF8;

        //TODO: resolve the Path based on PathType (Internal vs External)
        return new StreamWriter( FullPath(), append, encoding );
    }

    public StreamReader Reader()
    {
        return new StreamReader( FullPath(), Encoding.UTF8 );
    }

    public StreamReader Reader( Encoding? encoding = null )
    {
        encoding ??= Encoding.UTF8;

        return new StreamReader( FullPath(), encoding );
    }

    public string FullPath()
    {
        return ResolvePath( _rawPath, Type );
    }

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

    public bool Exists()
    {
        return File.Exists( FullPath() ) || Directory.Exists( FullPath() );
    }

    public FileHandle Child( string name )
    {
        return new FileHandle( System.IO.Path.Combine( FullPath(), name ), Type );
    }

    public FileHandle Parent()
    {
        return new FileHandle( System.IO.Path.GetDirectoryName( FullPath() ), Type );
    }

    public string NameWithoutExtension()
    {
        return NameWithoutExtension( FullPath() );
    }

    public string NameWithoutExtension( string path )
    {
        return System.IO.Path.GetFileNameWithoutExtension( path );
    }
}

// ============================================================================
// ============================================================================