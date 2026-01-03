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

using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;
using Platform = LughSharp.Core.Main.Platform;
using Capabilities = LughSharp.Core.Graphics.OpenGL.OpenGL.Capabilities;

namespace LughSharp.Core.Graphics.Utils;

/// <summary>
/// Wrapper for the current OpenGL Version used by this library.
/// </summary>
/// <remarks>
/// It is HIGHLY likely that this class can be removed, with some minor work elsewhere.
/// </remarks>
[DebuggerDisplay( "DebugVersionString" )]
[PublicAPI]
public class AppVersion
{
    public GraphicsBackend.BackendType BackendType { get; set; }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="appType"></param>
    /// <param name="profile"></param>
    public AppVersion( Platform.ApplicationType appType, DotGLFW.OpenGLProfile profile )
    {
        BackendType = appType switch
        {
            Platform.ApplicationType.Android     => GraphicsBackend.BackendType.AndroidGLES,
            Platform.ApplicationType.WindowsGles => GraphicsBackend.BackendType.OpenGLES,
            Platform.ApplicationType.WindowsGL   => GraphicsBackend.BackendType.OpenGL,
            Platform.ApplicationType.WebGL       => GraphicsBackend.BackendType.WebGL,
            Platform.ApplicationType.IOS         => GraphicsBackend.BackendType.IOSGLES,

            var _ => throw new GdxRuntimeException( $"Unknown Platform ApplicationType: {appType}" ),
        };

        OpenGL.OpenGL.Initialisation.LoadVersion();
        Capabilities.OpenGLProfile = profile;

        DebugPrint();
    }

    /// <summary>
    /// Checks to see if the current GL connection version is higher, or
    /// equal to the provided test versions.
    /// </summary>
    /// <param name="testMajorVersion"> the major version to test against </param>
    /// <param name="testMinorVersion"> the minor version to test against </param>
    /// <returns> true if the current version is higher or equal to the test version </returns>
    public bool IsVersionEqualToOrHigher( int testMajorVersion, int testMinorVersion )
    {
        return ( Capabilities.MajorVersion > testMajorVersion )
               || ( ( Capabilities.MajorVersion == testMajorVersion )
                    && ( Capabilities.MinorVersion >= testMinorVersion ) );
    }

    /// <summary>
    /// </summary>
    public void DebugPrint()
    {
        Logger.Divider();
        Logger.Debug( $"BackendType : {BackendType}" );
        Logger.Debug( $"Version     : {Capabilities.MajorVersion}.{Capabilities.MinorVersion}.{Capabilities.RevisionVersion}" );
        Logger.Debug( $"Profile     : {Capabilities.OpenGLProfile}" );
        Logger.Debug( $"Vendor      : {Capabilities.VendorString}" );
        Logger.Debug( $"Renderer    : {Capabilities.RendererString}" );
        Logger.Divider();
    }

    /// <summary>
    /// Returns a string with the current GL connection data.
    /// </summary>
    public string DebugVersionString()
    {
        return $"Type: {BackendType} :: " +
               $"Version: {Capabilities.MajorVersion}.{Capabilities.MinorVersion}.{Capabilities.RevisionVersion} :: " +
               $"Profile: {Capabilities.OpenGLProfile} :: " +
               $"Vendor: {Capabilities.VendorString} :: " +
               $"Renderer: {Capabilities.RendererString}";
    }
}

// ============================================================================
// ============================================================================
