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

using LughSharp.Lugh.Graphics.OpenGL;

namespace LughSharp.Lugh.Graphics;

[PublicAPI]
public sealed class GraphicsCapabilities
{
    public int  Major                  { get; init; }
    public int  Minor                  { get; init; }
    public bool HasS3TC                { get; init; } // BC1–BC3
    public bool HasRGTC                { get; init; } // BC4–BC5
    public bool HasBPTC                { get; init; } // BC6H–BC7
    public bool HasETC2                { get; init; }
    public bool HasInternalFormatQuery { get; init; }

    // ========================================================================
    
    public static unsafe GraphicsCapabilities Detect()
    {
        GL.GetIntegerv( GLConsts.MAJOR_VERSION, out var major );
        GL.GetIntegerv( GLConsts.MINOR_VERSION, out var minor );

        return new GraphicsCapabilities
        {
            Major = major,
            Minor = minor,
            HasS3TC = HasExt( "GL_EXT_texture_compression_s3tc" )
                      || HasExt( "GL_ANGLE_texture_compression_dxt1" )
                      || HasExt( "GL_EXT_texture_compression_dxt1" ),
            HasRGTC                = VerAtLeast( 3, 0 ), // Core in 3.0
            HasBPTC                = VerAtLeast( 4, 2 ) || HasExt( "GL_ARB_texture_compression_bptc" ),
            HasETC2                = VerAtLeast( 4, 3 ) || HasExt( "GL_ARB_ES3_compatibility" ),
            HasInternalFormatQuery = VerAtLeast( 4, 2 ) || HasExt( "GL_ARB_internalformat_query2" )
        };

        // ------------------------------------------------

        static bool HasExt( string s )
        {
            GL.GetIntegerv( GLConsts.NUM_EXTENSIONS, out var n );

            for ( uint i = 0; i < n; i++ )
            {
                if ( string.Equals( GL.GetStringi( GLConsts.EXTENSIONS, i ) -> ToString(), s, StringComparison.Ordinal ) )
                {
                    return true;
                }
            }

            return false;
        }

        bool VerAtLeast( int mj, int mn ) => ( major > mj ) || ( ( major == mj ) && ( minor >= mn ) );
    }
}

// ========================================================================
// ========================================================================