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


using JetBrains.Annotations;

using LughSharp.Core.Files;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Mock.Files;

[PublicAPI]
public class MockFiles : IFiles
{
    /// <summary>
    /// Returns a handle representing a file or directory.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="type">Determines how the path is resolved.</param>
    /// <exception cref="RuntimeException">
    /// if the type is classpath or internal and the file does not exist.
    /// </exception>
    public FileInfo GetFileHandle( string path, PathType type )
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="PathType.Classpath"/>
    public FileInfo Classpath( string path )
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="PathType.Absolute"/>
    public FileInfo Absolute( string path )
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="PathType.Assembly"/>
    public FileInfo Assembly( string path )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns the internal storage path directory. This is the app asset directory
    /// on Android and the Applications root directory on the desktop.
    /// </summary>
    public string GetAssemblyStoragePath()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns true if the internal storage is ready for file IO. The return value
    /// is true by default, but backends may override this to return false if the
    /// internal storage is not available.
    /// </summary>
    public bool IsAssemblyStorageAvailable()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="PathType.Internal"/>
    public FileInfo Internal( string path )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns the internal storage path directory. This is the app asset directory
    /// on Android and the Applications root directory on the desktop.
    /// </summary>
    public string GetInternalStoragePath()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns true if the internal storage is ready for file IO. The return value
    /// is true by default, but backends may override this to return false if the
    /// internal storage is not available.
    /// </summary>
    public bool IsInternalStorageAvailable()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="PathType.External"/>
    public FileInfo External( string path )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns the external storage path directory. This is the app external storage
    /// on Android and the home directory of the current user on the desktop.
    /// </summary>
    public string GetExternalStoragePath()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns true if the external storage is ready for file IO.
    /// For desktops builds, this would default to true if the folder exists.
    /// </summary>
    public bool IsExternalStorageAvailable()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="PathType.Assets"/>
    public FileInfo Assets( string path )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns the assets storage path directory. This is the app asset directory
    /// on Android and the Applications root directory on the desktop.
    /// </summary>
    public string GetAssetsStoragePath()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="PathType.Local"/>
    public FileInfo Local( string path )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns the local storage path directory. This is the private
    /// files directory on Android and the directory of the exe on
    /// the desktop.
    /// </summary>
    public string GetLocalStoragePath()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns true if the local storage is ready for file IO.
    /// For desktops builds, this would default to true if the folder exists.
    /// </summary>
    public bool IsLocalStorageAvailable()
    {
        throw new NotImplementedException();
    }
}

// ============================================================================
// ============================================================================
