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

namespace LughSharp.Lugh.Graphics.OpenGL;

public static partial class OpenGL
{
    public class Initialisation
    {
        /// <summary>
        /// Loads and sets the OpenGL version and determines the capabilities of the environment,
        /// including whether it is OpenGL ES and if the reported version is emulated.
        /// Retrieves the major and minor version numbers and considers potential discrepancies
        /// between reported values in certain cases such as emulation.
        /// </summary>
        public static unsafe void LoadVersion()
        {
            var majorVersion = 0;
            var minorVersion = 0;

            var version = GL.GetString( IGL.GL_VERSION ) -> ToString();

            Capabilities.IsGLES     = version.StartsWith( "OpenGL ES" );
            Capabilities.IsEmulated = false;

            GL.GetIntegerv( IGL.GL_MAJOR_VERSION, &majorVersion );

            if ( GL.GetError() == IGL.GL_INVALID_ENUM )
            {
                if ( !TryParseOpenGLVersionFromString( version, out majorVersion, out minorVersion ) )
                {
                    // Something is horribly wrong with our version string; be optimistic!
                    Capabilities.MajorVersion = 4;
                    Capabilities.MinorVersion = 0;
                }
            }
            else
            {
                GL.GetIntegerv( IGL.GL_MINOR_VERSION, &minorVersion );

                Capabilities.MajorVersion = majorVersion;
                Capabilities.MinorVersion = minorVersion;
            }

            // In the case of GLES, it's possible for the value reported by glGetIntegerv() to
            // differ from the value specified in GL_VERSION if we're running inside of an emulator
            // with native GPU enabled - so try to account for that case.
            if ( Capabilities.IsGLES )
            {
                if ( TryParseOpenGLVersionFromString( GL.GetString( IGL.GL_VERSION )->ToString(),
                                                      out var glesMajorVersion,
                                                      out var glesMinorVersion ) )
                {
                    if ( glesMajorVersion != Capabilities.MajorVersion
                         || glesMinorVersion != Capabilities.MinorVersion )
                    {
                        Capabilities.IsEmulated   = true;
                        Capabilities.MajorVersion = glesMajorVersion;
                        Capabilities.MinorVersion = glesMinorVersion;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Attempts to parse the version of the loaded OpenGL implementation from the string
    /// returned by Api.GL.GetString( IGL.GL_VERSION ).
    /// </summary>
    private static bool TryParseOpenGLVersionFromString( string str, out int major, out int minor )
    {
        str = Capabilities.IsGLES
            ? str.Substring( "OpenGL ES ".Length )
            : str.Substring( "OpenGL ".Length );

        var components = str.Split( [ ' ', '.' ], StringSplitOptions.RemoveEmptyEntries );

        if ( components.Length < 2
             || !int.TryParse( components[ 0 ], out major )
             || !int.TryParse( components[ 1 ], out minor ) )
        {
            major = 0;
            minor = 0;

            return false;
        }

        return true;
    }
}

// ========================================================================
// ========================================================================