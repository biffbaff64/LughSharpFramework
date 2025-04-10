﻿// ///////////////////////////////////////////////////////////////////////////////
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

using LughSharp.Lugh.Files;
using LughSharp.Lugh.Utils.Exceptions;

using FileInfo = System.IO.FileInfo;

namespace DesktopGLBackend.Files;

/// <summary>
/// </summary>
[PublicAPI]
public class DesktopGLFileHandle : FileHandle
{
    /// <summary>
    /// Creates a new DesktopGLFileHandle instance, using the supplied filename
    /// and <see cref="PathTypes" />.
    /// </summary>
    public DesktopGLFileHandle( string fileName, PathTypes type )
        : base( fileName, type )
    {
    }

    /// <summary>
    /// Creates a new DesktopGLFileHandle instance, using the file name obtained
    /// from the supplied <see cref="FileInfo" /> and <see cref="PathTypes" />.
    /// </summary>
    public DesktopGLFileHandle( FileInfo file, PathTypes type )
        : base( file.Name, type )
    {
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Returns the abstract pathname of this abstract pathname's parent, or null if this pathname
    /// does not name a parent directory.
    /// <para>
    /// The parent of an abstract pathname consists of the pathname's prefix, if any, and each name
    /// in the pathname's name sequence except for the last. If the name sequence is empty then the
    /// pathname does not name a parent directory.
    /// </para>
    /// </summary>
    public DirectoryInfo ParentFolder()
    {
        var directoryInfo = !Directory.Exists( FilePath )
            ? new DirectoryInfo( PathType == PathTypes.Absolute ? "/" : "" )
            : new DirectoryInfo( Path.GetFullPath( FileName ) );

        return directoryInfo;
    }

    /// <summary>
    /// Returns a handle to the child with the specified name.
    /// </summary>
    public FileHandle Child( string name )
    {
        return Path.GetFullPath( name ).Length == 0
            ? new DesktopGLFileHandle( new FileInfo( name ), PathType )
            : new DesktopGLFileHandle( new FileInfo( Path.GetDirectoryName( name )! ), PathType );
    }

    /// <summary>
    /// Returns a handle to the sibling with the specified name.
    /// </summary>
    public FileHandle Sibling( string name )
    {
        return Path.GetFullPath( name ).Length == 0
            ? throw new GdxRuntimeException( "Cannot get the sibling of the root." )
            : new DesktopGLFileHandle( new FileInfo( name ), PathType );
    }

    /// <summary>
    /// Returns a File that represents this file handle. Note the returned file will only
    /// be usable for <see cref="PathTypes.Absolute" />, <see cref="PathTypes.Internal" />
    /// and <see cref="PathTypes.External" /> file handles.
    /// </summary>
    public FileInfo GetFile()
    {
        return PathType switch
        {
            PathTypes.External => new FileInfo( IOData.ExternalPath + FileName ),
            PathTypes.Internal => new FileInfo( IOData.InternalPath + FileName ),

            var _ => PathType == PathTypes.Local
                ? new FileInfo( IOData.LocalPath + FileName )
                : File,
        };
    }
}