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

using JetBrains.Annotations;

namespace LughSharp.Core.Files;

[PublicAPI]
public enum PathType
{
    /// <summary>
    /// Path relative to the root of the classpath. Classpath files are always readonly.
    /// Note that classpath files are not compatible with some functionality on Android,
    /// such as Audio#NewSound(FileInfo) and Audio#NewMusic(FileInfo).
    /// </summary>
    Classpath,

    /// <summary>
    /// Path that is a fully qualified, absolute filesystem path. <b>To ensure
    /// portability across platforms use absolute files only when absolutely
    /// necessary.</b>
    /// </summary>
    Absolute,

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
    Internal,

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
    External,

    /// <summary>
    /// Path relative to the private files directory on Android and to the
    /// application's root directory on the desktop.
    /// </summary>
    Local,

    /// <summary>
    /// Path relative to the location of the assembly itself. Primarily used to
    /// access files that are distributed alongside the application's binaries.
    /// These files are typically read-only and are intended to be bundled with
    /// the application during deployment.
    /// </summary>
    Assembly,

    // ========================================================================

    /// <summary>
    /// Represents an unknown or unspecified path type. This value is used when
    /// the path type cannot be determined or does not match any known category.
    /// </summary>
    Unknown,

    /// <summary>
    /// Represents a file path that is explicitly designated as a file rather
    /// than a directory or other path type. This is used to clarify file-specific
    /// operations and enforce file-related constraints where necessary.
    /// </summary>
    File,

    /// <summary>
    /// Represents a path to the frameworks default assets directory. This is
    /// typically the "assets" folder in the application's root directory.
    /// An example Assets path would be:-
    /// <code>
    /// C:\Development\Projects\CSharp\Template\bin\Debug\net8.0\assets\
    /// </code>
    /// </summary>
    Assets,

    /// <summary>
    /// Represents a path to a directory. Used to specify or resolve paths that
    /// refer to directories, enabling organization or manipulation of grouped
    /// files and subdirectories.
    /// </summary>
    Directory,

    /// <summary>
    /// Represents a path type indicating that the specified file or directory
    /// does not exist. This can be used as a placeholder or error indicator for
    /// non-existent paths.
    /// </summary>
    DoesNotExist,

    /// <summary>
    /// Represents an invalid or unsupported path type. This value may be used as
    /// a fallback or error state, indicating that the provided path cannot be
    /// resolved within the context of the available path types.
    /// </summary>
    Invalid,
}

// ============================================================================
// ============================================================================

