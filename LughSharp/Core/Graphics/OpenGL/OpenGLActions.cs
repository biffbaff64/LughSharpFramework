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

using System;

using DotGLFW;

using JetBrains.Annotations;

using LughSharp.Core.Graphics.OpenGL.Enums;
using LughSharp.Core.Main;
using LughSharp.Core.Utils;

namespace LughSharp.Core.Graphics.OpenGL;

[PublicAPI]
public class OpenGL
{
    public class Initialisation
    {
        public static Capabilities Capabilities { get; } = new();

        // ====================================================================

        /// <summary>
        /// Loads and sets the OpenGL version and determines the capabilities of the environment,
        /// including whether it is OpenGL ES and if the reported version is emulated.
        /// Retrieves the major and minor version numbers and considers potential discrepancies
        /// between reported values in certain cases such as emulation.
        /// </summary>
        public static unsafe void LoadVersion()
        {
            var majorVersion    = 0;
            var minorVersion    = 0;
            var revisionVersion = 0;

            string version = Engine.GL.GetString( IGL.GL_VERSION )->ToString();

            Capabilities.IsGLES     = version.StartsWith( "OpenGL ES" );
            Capabilities.IsEmulated = false;

            Engine.GL.GetIntegerv( IGL.GL_MAJOR_VERSION, &majorVersion );

            if ( Engine.GL.GetError() == IGL.GL_INVALID_ENUM )
            {
                if ( !TryParseOpenGLVersionFromString( version,
                                                       out majorVersion,
                                                       out minorVersion,
                                                       out revisionVersion ) )
                {
                    // Something is horribly wrong with our version string; be optimistic!
                    Capabilities.MajorVersion    = 4;
                    Capabilities.MinorVersion    = 0;
                    Capabilities.RevisionVersion = 0;
                }
            }
            else
            {
                Engine.GL.GetIntegerv( IGL.GL_MINOR_VERSION, &minorVersion );

                Capabilities.MajorVersion    = majorVersion;
                Capabilities.MinorVersion    = minorVersion;
                Capabilities.RevisionVersion = revisionVersion;
            }

            // In the case of GLES, it's possible for the value reported by glGetIntegerv() to
            // differ from the value specified in GL_VERSION if we're running inside of an emulator
            // with native GPU enabled - so try to account for that case.
            if ( Capabilities.IsGLES )
            {
                if ( TryParseOpenGLVersionFromString( Engine.GL.GetString( IGL.GL_VERSION )->ToString(),
                                                      out int glesMajorVersion,
                                                      out int glesMinorVersion,
                                                      out int glesRevisionVersion ) )
                {
                    if ( ( glesMajorVersion != Capabilities.MajorVersion )
                      || ( glesMinorVersion != Capabilities.MinorVersion ) )
                    {
                        Capabilities.IsEmulated      = true;
                        Capabilities.MajorVersion    = glesMajorVersion;
                        Capabilities.MinorVersion    = glesMinorVersion;
                        Capabilities.RevisionVersion = glesRevisionVersion;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Attempts to parse the version of the loaded OpenGL implementation from the string
    /// returned by Api.Engine.GL.GetString( IGL.GL_VERSION ).
    /// </summary>
    private static bool TryParseOpenGLVersionFromString( string str, out int major, out int minor, out int revision )
    {
        str = Capabilities.IsGLES
            ? str.Substring( "OpenGL ES ".Length )
            : str.Substring( "OpenGL ".Length );

        string[] components = str.Split( [ ' ', '.' ], StringSplitOptions.RemoveEmptyEntries );

        if ( ( components.Length < 2 )
          || !int.TryParse( components[ 0 ], out major )
          || !int.TryParse( components[ 1 ], out minor )
          || !int.TryParse( components[ 2 ], out revision ) )
        {
            major    = 0;
            minor    = 0;
            revision = 0;

            return false;
        }

        return true;
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class Capabilities
    {
        public static bool IsDebugContext      { get; set; }
        public static bool IsGLES              { get; set; }
        public static bool IsEmulated          { get; set; }
        public static bool IsCoreProfile       { get; set; }
        public static bool IsForwardCompatible { get; set; }

        // --------------------------------------

        public static int MajorVersion    { get; set; }
        public static int MinorVersion    { get; set; }
        public static int RevisionVersion { get; set; }

        // --------------------------------------

        public static OpenGLProfile OpenGLProfile { get; set; } = OpenGLProfile.CoreProfile;

        // ====================================================================

        /// <summary>
        /// A string describing the graphics vendor, i.e. "Intel Corporation"
        /// </summary>
        public static unsafe string VendorString
        {
            get
            {
                field = BytePointerToString.Convert( Engine.GL.GetString( ( int )StringName.Vendor ) );

                return field;
            }
        } = "";

        /// <summary>
        /// A string describing the graphics hardware, i.e. "Intel(R) UHD Graphics 620"
        /// </summary>
        public static unsafe string RendererString
        {
            get
            {
                field = BytePointerToString.Convert( Engine.GL.GetString( ( int )StringName.Renderer ) );

                return field;
            }
        } = "";

        /// <summary>
        /// A string describing the OpenGL version, i.e. "4.6.0"
        /// </summary>
        public static unsafe string VersionString
        {
            get
            {
                field = BytePointerToString.Convert( Engine.GL.GetString( ( int )StringName.Version ) );

                return field;
            }
        } = "";

        /// <summary>
        /// A string describing the shading language version, i.e. "4.60"
        /// </summary>
        public static unsafe string ShadingLanguageVersionString
        {
            get
            {
                field = BytePointerToString.Convert( Engine.GL.GetString( ( int )StringName.ShadingLanguageVersion ) );

                return field;
            }
        } = "";
    }
}

// ========================================================================
// ========================================================================