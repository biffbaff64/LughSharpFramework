// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Circa64 Software Projects
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

using DotGLFW;

using LughSharp.Source.Graphics.OpenGL.Enums;

namespace LughSharp.Source.Graphics.OpenGL;

[PublicAPI]
public class LughGL
{
    // ========================================================================
    // GL Defaults

    public const int           DefaultGLMajor             = 3;
    public const int           DefaultGLMinor             = 2;
    public const ClientAPI     DefaultClientApi           = ClientAPI.OpenGLAPI;
    public const OpenGLProfile DefaultOpenglProfile       = OpenGLProfile.CoreProfile;
    public const bool          DefaultOpenglForwardcompat = true;

    /// <summary>
    /// As named, this is the currently bound FBO.
    /// </summary>
    public static uint CurrentBoundFbo { get; set; }

    // ========================================================================

    /// <summary>
    /// Checks for OpenGL errors after a given stage of rendering or processing.
    /// Throws an <see cref="InvalidOperationException"/> if an OpenGL error is detected.
    /// </summary>
    /// <param name="stage">The description of the stage at which the error occurred.</param>
    /// <exception cref="InvalidOperationException">Thrown when an OpenGL error is detected.</exception>
    public static void GLErrorCheck( string stage )
    {
        int error = Engine.GL.GetError();

        if ( error != ( int )DotGLFW.ErrorCode.NoError )
        {
            throw new InvalidOperationException( $"OpenGL error at {stage}: {error}" );
        }
    }

    /// <summary>
    /// Converts a specified OpenGL error code into a corresponding descriptive error string.
    /// </summary>
    /// <param name="errorCode">
    /// The error code to be translated, typically returned from OpenGL functions.
    /// </param>
    /// <returns>
    /// A string that describes the given OpenGL error code. If the error code is unrecognized,
    /// a generic message including the code is returned.
    /// </returns>
    public static string GetErrorString( int errorCode )
    {
        return errorCode switch
               {
                   IGL.GLInvalidEnum                 => "Invalid Enum",
                   IGL.GLInvalidOperation            => "Invalid Operation",
                   IGL.GLInvalidValue                => "Invalid Value",
                   IGL.GLOutOfMemory                 => "Out of memory",
                   IGL.GLInvalidFramebufferOperation => "Invalid Framebuffer operation",
                   IGL.GLStackOverflow               => "Stack Overflow",
                   IGL.GLStackUnderflow              => "Stack Underflow",
                   IGL.GLContextLost                 => "Context Lost",

                   // ----------------------------------

                   IGL.GLNoError => "No Error",

                   // ----------------------------------

                   var _ => $"Unknown GL Error Code: {errorCode}"
               };
    }

    /// <summary>
    /// Checks if there is a current OpenGL context bound to the calling thread.
    /// </summary>
    /// <exception cref="LughRuntimeException">
    /// Thrown when no OpenGL context is currently bound to the thread.
    /// </exception>
    public static bool CheckOpenGLContext()
    {
        return DotGLFW.Glfw.GetCurrentContext() == null
            ? throw new LughRuntimeException( "No OpenGL context is current on this thread!" )
            : true;
    }

    /// <summary>
    /// Verifies whether the currently bound buffer matches the expected buffer ID.
    /// Logs an error if there is a mismatch.
    /// </summary>
    /// <param name="expectedBuffer">The identifier of the buffer that is expected to be currently bound.</param>
    public static unsafe void CheckBufferBinding( uint expectedBuffer )
    {
        var currentBuffer = new int[ 1 ];

        fixed ( int* ptr = &currentBuffer[ 0 ] )
        {
            // Fetch the currently bound buffer
            Engine.GL.GetIntegerv( ( int )BufferBinding.ArrayBufferBinding, ptr );
        }

        if ( currentBuffer[ 0 ] != expectedBuffer )
        {
            Logger.Error( $"Buffer not bound correctly! Expected {expectedBuffer}, got {currentBuffer[ 0 ]}" );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vbo"></param>
    /// <param name="sizeInBytes"></param>
    /// <param name="vertexSizeInFloats"></param>
    /// <returns></returns>
    public static float[]? GetVboData( uint vbo, int sizeInBytes, int vertexSizeInFloats )
    {
        Engine.GL.BindBuffer( BufferTarget.ArrayBuffer, vbo );

        IntPtr dataPtr = Engine.GL.MapBuffer( ( int )BufferTarget.ArrayBuffer, ( int )BufferAccess.ReadOnly );

        if ( dataPtr == IntPtr.Zero )
        {
            Logger.Debug( "Error: Failed to map VBO." );

            return null;
        }

        var data = new float[ sizeInBytes / sizeof( float ) ];
        Marshal.Copy( dataPtr, data, 0, data.Length );

        Engine.GL.UnmapBuffer( ( int )BufferTarget.ArrayBuffer );
        Engine.GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );

        return data;
    }

    /// <summary>
    /// Logs the data contained in a specified Vertex Buffer Object (VBO) for debugging purposes.
    /// </summary>
    /// <param name="vbo">
    /// The ID of the Vertex Buffer Object to retrieve and print data from.
    /// </param>
    /// <param name="sizeInBytes"> The size of the data in the VBO, in bytes. </param>
    /// <param name="vertexSizeInFloats">
    /// The size of each vertex in the VBO, in terms of the number of floats it contains.
    /// </param>
    public static void PrintVboData( uint vbo, int sizeInBytes, int vertexSizeInFloats )
    {
        float[]? data = GetVboData( vbo, sizeInBytes, vertexSizeInFloats );

        if ( data == null )
        {
            return;
        }

        Logger.Debug( $"sizeInBytes: {sizeInBytes}" );
        Logger.Debug( $"vertexSizeInFloats: {vertexSizeInFloats}" );
        Logger.Debug( $"data.Length: {data.Length}" );

        for ( var i = 0; i < data.Length; i += vertexSizeInFloats )
        {
            Logger.Debug( $"Vertex {i / vertexSizeInFloats}: " );

            for ( var j = 0; j < vertexSizeInFloats; j++ )
            {
                Logger.Debug( $"{data[ i + j ]} " );
            }
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// OpenGL initialisation and capabilities.
    /// </summary>
    [PublicAPI]
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

            string version = Engine.GL.GetString( IGL.GLVersion )->ToString();

            Capabilities.IsGLES     = version.StartsWith( "OpenGL ES" );
            Capabilities.IsEmulated = false;

            Engine.GL.GetIntegerv( IGL.GLMajorVersion, &majorVersion );

            if ( Engine.GL.GetError() == IGL.GLInvalidEnum )
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
                Engine.GL.GetIntegerv( IGL.GLMinorVersion, &minorVersion );

                Capabilities.MajorVersion    = majorVersion;
                Capabilities.MinorVersion    = minorVersion;
                Capabilities.RevisionVersion = revisionVersion;
            }

            // In the case of GLES, it's possible for the value reported by glGetIntegerv() to
            // differ from the value specified in GL_VERSION if we're running inside of an emulator
            // with native GPU enabled - so try to account for that case.
            if ( Capabilities.IsGLES )
            {
                if ( TryParseOpenGLVersionFromString( Engine.GL.GetString( IGL.GLVersion )->ToString(),
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
    /// returned by <see cref="Engine.GL.GetString( IGL.GLVersion )"/>.
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

        /// <summary>
        /// Indicates the OpenGL context. Can be one of <see cref="OpenGLProfile.AnyProfile"/>,
        /// <see cref="OpenGLProfile.CoreProfile"/>or <see cref="OpenGLProfile.CompatProfile"/>.
        /// </summary>
        public static OpenGLProfile OpenGLProfile
        {
            get;
            set
            {
                field = value;

                IsForwardCompatible = ( value == OpenGLProfile.CompatProfile );
                IsCoreProfile       = ( value == OpenGLProfile.CoreProfile );
            }
        } = OpenGLProfile.CoreProfile;

        // ====================================================================

        /// <summary>
        /// A string describing the graphics vendor, i.e. "Intel Corporation"
        /// </summary>
        /// <remarks>
        /// <see cref="Marshal.PtrToStringUTF8(IntPtr)"/> reads the null-terminated UTF-8
        /// byte pointer returned by OpenGL from unmanaged memory into a managed string.
        /// </remarks>
        public static unsafe string VendorString
        {
            get
            {
                field = Marshal.PtrToStringUTF8( ( IntPtr )Engine.GL.GetString( ( int )StringName.Vendor ) )
                     ?? string.Empty;

                return field;
            }
        } = string.Empty;

        /// <summary>
        /// A string describing the graphics hardware, i.e. "Intel(R) UHD Graphics 620"
        /// </summary>
        /// <remarks>
        /// <see cref="Marshal.PtrToStringUTF8(IntPtr)"/> reads the null-terminated UTF-8
        /// byte pointer returned by OpenGL from unmanaged memory into a managed string.
        /// </remarks>
        public static unsafe string RendererString
        {
            get
            {
                field = Marshal.PtrToStringUTF8( ( IntPtr )Engine.GL.GetString( ( int )StringName.Renderer ) )
                     ?? string.Empty;

                return field;
            }
        } = string.Empty;

        /// <summary>
        /// A string describing the OpenGL version, i.e. "4.6.0"
        /// </summary>
        /// <remarks>
        /// <see cref="Marshal.PtrToStringUTF8(IntPtr)"/> reads the null-terminated UTF-8
        /// byte pointer returned by OpenGL from unmanaged memory into a managed string.
        /// </remarks>
        public static unsafe string VersionString
        {
            get
            {
                field = Marshal.PtrToStringUTF8( ( IntPtr )Engine.GL.GetString( ( int )StringName.Version ) )
                     ?? string.Empty;

                return field;
            }
        } = string.Empty;

        /// <summary>
        /// A string describing the shading language version, i.e. "4.60"
        /// </summary>
        /// <remarks>
        /// <see cref="Marshal.PtrToStringUTF8(IntPtr)"/> reads the null-terminated UTF-8
        /// byte pointer returned by OpenGL from unmanaged memory into a managed string.
        /// </remarks>
        public static unsafe string ShadingLanguageVersionString
        {
            get
            {
                field =
                    Marshal.PtrToStringUTF8( ( IntPtr )Engine.GL.GetString( ( int )StringName.ShadingLanguageVersion ) )
                 ?? string.Empty;

                return field;
            }
        } = string.Empty;
    }
}

// ========================================================================
// ========================================================================
