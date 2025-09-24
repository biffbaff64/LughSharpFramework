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

using Platform = LughSharp.Lugh.Core.Platform;

namespace LughSharp.Lugh.Graphics.Utils;

/// <summary>
/// Wrapper for the current OpenGL Version used by this library.
/// </summary>
/// <remarks>
/// It is HIGHLY likely that this class can be removed, with some minor work elsewhere.
/// </remarks>
[PublicAPI]
[DebuggerDisplay( "DebugVersionString" )]
public class GLVersion
{
    public GraphicsBackend.BackendType BackendType { get; set; }

    // ========================================================================

    private int           _majorVersion    = 0;
    private int           _minorVersion    = 0;
    private string        _renderer        = "";
    private int           _revisionVersion = 0;
    private OpenGLProfile _openGLProfile;
    private string        _vendor = "";

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="appType"></param>
    /// <param name="profile"></param>
    public unsafe GLVersion( Platform.ApplicationType appType, OpenGLProfile profile )
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

        var version = BytePointerToString.Convert( GL.GetString( ( int )StringName.Version ) );

        _majorVersion    = ( int )char.GetNumericValue( version[ 0 ] );
        _minorVersion    = ( int )char.GetNumericValue( version[ 2 ] );
        _revisionVersion = ( int )char.GetNumericValue( version[ 4 ] );
        _openGLProfile   = profile;

        Logger.Divider();
        DebugPrint();
        Logger.Divider();
    }

    public unsafe string VendorString
    {
        get
        {
            _vendor = BytePointerToString.Convert( GL.GetString( ( int )StringName.Vendor ) );

            return _vendor;
        }
    }

    public unsafe string RendererString
    {
        get
        {
            _renderer = BytePointerToString.Convert( GL.GetString( ( int )StringName.Renderer ) );

            return _renderer;
        }
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public (int major, int minor, int revision) Get()
    {
        return ( _majorVersion, _minorVersion, _revisionVersion );
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
        return ( _majorVersion > testMajorVersion )
               || ( ( _majorVersion == testMajorVersion ) && ( _minorVersion >= testMinorVersion ) );
    }

    /// <summary>
    /// </summary>
    public void DebugPrint()
    {
        Logger.Debug( $"BackendType : {BackendType}" );
        Logger.Debug( $"Version     : {_majorVersion}.{_minorVersion}.{_revisionVersion}" );
        Logger.Debug( $"Profile     : {_openGLProfile}" );
        Logger.Debug( $"Vendor      : {VendorString}" );
        Logger.Debug( $"Renderer    : {RendererString}" );
    }

    /// <summary>
    /// Returns a string with the current GL connection data.
    /// </summary>
    public string DebugVersionString()
    {
        return $"Type: {BackendType} :: "
               + $"Version: {_majorVersion}.{_minorVersion}.{_revisionVersion} :: "
               + $"Profile: {_openGLProfile} :: "
               + $"Vendor: {VendorString} :: "
               + $"Renderer: {RendererString}";
    }

    // ========================================================================
}