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

namespace LughSharp.Lugh.Files;

[PublicAPI]
public interface IFiles
{
    /// <summary>
    /// Returns a handle representing a file or directory.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="type">Determines how the path is resolved.</param>
    /// <exception cref="GdxRuntimeException">
    /// if the type is classpath or internal and the file does not exist.
    /// </exception>
    FileInfo GetFileHandle( string path, PathTypes type );

    /// <inheritdoc cref="PathTypes.Classpath" />
    FileInfo Classpath( string path );

    /// <inheritdoc cref="PathTypes.Absolute" />
    FileInfo Absolute( string path );

    // ========================================================================

    /// <inheritdoc cref="PathTypes.Assembly" />
    FileInfo Assembly( string path );

    /// <summary>
    /// Returns the internal storage path directory. This is the app asset directory
    /// on Android and the Applications root directory on the desktop.
    /// </summary>
    string GetAssemblyStoragePath();

    /// <summary>
    /// Returns true if the internal storage is ready for file IO. The return value
    /// is true by default, but backends may override this to return false if the
    /// internal storage is not available.
    /// </summary>
    bool IsAssemblyStorageAvailable();

    // ========================================================================

    /// <inheritdoc cref="PathTypes.Internal" />
    FileInfo Internal( string path );

    /// <summary>
    /// Returns the internal storage path directory. This is the app asset directory
    /// on Android and the Applications root directory on the desktop.
    /// </summary>
    string GetInternalStoragePath();

    /// <summary>
    /// Returns true if the internal storage is ready for file IO. The return value
    /// is true by default, but backends may override this to return false if the
    /// internal storage is not available.
    /// </summary>
    bool IsInternalStorageAvailable();

    // ========================================================================

    /// <inheritdoc cref="PathTypes.External" />
    FileInfo External( string path );

    /// <summary>
    /// Returns the external storage path directory. This is the app external storage
    /// on Android and the home directory of the current user on the desktop.
    /// </summary>
    string GetExternalStoragePath();

    /// <summary>
    /// Returns true if the external storage is ready for file IO.
    /// For desktops builds, this would default to true if the folder exists.
    /// </summary>
    bool IsExternalStorageAvailable();

    // ========================================================================

    /// <inheritdoc cref="PathTypes.Local" />
    FileInfo Local( string path );

    /// <summary>
    /// Returns the local storage path directory. This is the private
    /// files directory on Android and the directory of the exe on
    /// the desktop.
    /// </summary>
    string GetLocalStoragePath();

    /// <summary>
    /// Returns true if the local storage is ready for file IO.
    /// For desktops builds, this would default to true if the folder exists.
    /// </summary>
    bool IsLocalStorageAvailable();
}

// ============================================================================
// ============================================================================