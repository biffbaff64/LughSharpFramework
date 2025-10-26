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

[PublicAPI]
public partial class OpenGL
{
    [PublicAPI]
    public class Capabilities
    {
        public static bool IsDebugContext      { get; set; } = false;
        public static bool IsGLES              { get; set; } = false;
        public static bool IsEmulated          { get; set; } = false;
        public static bool IsCoreProfile       { get; set; } = false;
        public static bool IsForwardCompatible { get; set; } = false;

        // --------------------------------------
        
        public static int MajorVersion    { get; set; } = 0;
        public static int MinorVersion    { get; set; } = 0;
        public static int RevisionVersion { get; set; } = 0;

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
                field = BytePointerToString.Convert( GL.GetString( ( int )StringName.Vendor ) );

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
                field = BytePointerToString.Convert( GL.GetString( ( int )StringName.Renderer ) );

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
                field = BytePointerToString.Convert( GL.GetString( ( int )StringName.Version ) );

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
                field = BytePointerToString.Convert( GL.GetString( ( int )StringName.ShadingLanguageVersion ) );

                return field;
            }
        } = "";
    }
}

// ========================================================================
// ========================================================================