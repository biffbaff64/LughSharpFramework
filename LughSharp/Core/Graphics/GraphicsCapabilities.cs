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

using LughSharp.Core.Graphics.OpenGL;
using LughSharp.Core.Main;

namespace LughSharp.Core.Graphics;

[PublicAPI]
public unsafe class GraphicsCapabilities
{
    public int  Major                  { get; init; }
    public int  Minor                  { get; init; }
    public bool HasS3TC                { get; init; } // BC1–BC3
    public bool HasRGTC                { get; init; } // BC4–BC5
    public bool HasBPTC                { get; init; } // BC6H–BC7
    public bool HasETC2                { get; init; }
    public bool HasInternalFormatQuery { get; init; }

    // ========================================================================

    /// <summary>
    /// Detects and retrieves the graphics capabilities supported by the current
    /// OpenGL context, including version information and support for specific
    /// extensions and features.
    /// </summary>
    /// <returns>
    /// A <see cref="GraphicsCapabilities"/> object containing the detected graphics
    /// capabilities, such as OpenGL version, and availability of texture compression
    /// formats and other advanced features.
    /// </returns>
    public static GraphicsCapabilities Detect()
    {
        Engine.GL.GetIntegerv( GLData.MAJOR_VERSION, out int major );
        Engine.GL.GetIntegerv( GLData.MINOR_VERSION, out int minor );

        return new GraphicsCapabilities
        {
            Major = major,
            Minor = minor,
            HasS3TC = HasExtension( "GL_EXT_texture_compression_s3tc" )
                   || HasExtension( "GL_ANGLE_texture_compression_dxt1" )
                   || HasExtension( "GL_EXT_texture_compression_dxt1" ),
            HasRGTC = VersionAtLeast( 3, 0, major, minor ), // Core in 3.0
            HasBPTC = VersionAtLeast( 4, 2, major, minor )
                   || HasExtension( "GL_ARB_texture_compression_bptc" ),
            HasETC2 = VersionAtLeast( 4, 3, major, minor )
                   || HasExtension( "GL_ARB_ES3_compatibility" ),

            HasInternalFormatQuery = VersionAtLeast( 4, 2, major, minor )
                                  || HasExtension( "GL_ARB_internalformat_query2" )
        };
    }

    private static bool HasExtension( string s )
    {
        Engine.GL.GetIntegerv( GLData.NUM_EXTENSIONS, out int n );

        for ( uint i = 0; i < n; i++ )
        {
            if ( string.Equals( Engine.GL.GetStringi( GLData.EXTENSIONS, i )->ToString(),
                                s,
                                StringComparison.Ordinal ) )
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns <c>true</c> if the provided (major, minor) version is at least the
    /// version specified by (mj, mn).
    /// </summary>
    private static bool VersionAtLeast( int mj, int mn, int major, int minor )
    {
        return ( major > mj ) || ( ( major == mj ) && ( minor >= mn ) );
    }
}

// ========================================================================
// ========================================================================