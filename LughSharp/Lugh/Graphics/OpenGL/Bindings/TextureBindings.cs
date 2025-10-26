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
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACTh, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// /////////////////////////////////////////////////////////////////////////////

// ============================================================================

using GLenum = int;
using GLfloat = float;
using GLint = int;
using GLsizei = int;
using GLuint = uint;
using GLboolean = bool;

// ============================================================================

namespace LughSharp.Lugh.Graphics.OpenGL.Bindings;

// ============================================================================
// ============================================================================

/// <summary>
/// Bindings for OpenGL, for core profile. Blazing fast, low level,
/// direct access to the OpenGL API for all versions of OpenGL, using the unmanaged
/// delegates feature in C# 9.0,
/// <para>
/// Also includes a few overloads of many functions to make them a bit more C# friendly
/// (e.g. passing arrays of bytes or floats instead of passing pointers to fixed memory
/// locations). Significant effort has been made to make sure that the overloads are as
/// efficient as possible, in terms of both performance and memory usage.
/// </para>
/// </summary>
public partial class GLBindings
{
    /// <inheritdoc />
    public void TexImage1D( GLenum target,
                            GLint level,
                            GLenum internalFormat,
                            GLsizei width,
                            GLint border,
                            GLenum format,
                            GLenum type,
                            IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLTEXIMAGE1DPROC >( "glTexImage1D", out _glTexImage1D );

        _glTexImage1D( target, level, internalFormat, width, border, format, type, pixels );
    }

    /// <inheritdoc />
    public void TexImage1D< T >( GLenum target,
                                 GLint level,
                                 GLenum internalFormat,
                                 GLsizei width,
                                 GLint border,
                                 GLenum format,
                                 GLenum type,
                                 T[] pixels ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLTEXIMAGE1DPROC >( "glTexImage1D", out _glTexImage1D );

        unsafe
        {
            fixed ( void* p = &pixels[ 0 ] )
            {
                _glTexImage1D( target, level, internalFormat, width, border, format, type, ( IntPtr )p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void TexImage2D( GLenum target,
                            GLint level,
                            GLenum internalFormat,
                            GLsizei width,
                            GLsizei height,
                            GLint border,
                            GLenum format,
                            GLenum type,
                            IntPtr pixels = 0,
                            GLboolean enabled = true )
    {
        GetDelegateForFunction< PFNGLTEXIMAGE2DPROC >( "glTexImage2D", out _glTexImage2D );

        if ( enabled )
        {
            _glTexImage2D( target, level, internalFormat, width, height, border, format, type, pixels );
        }
    }

    /// <inheritdoc />
    public void TexImage2D< T >( GLenum target,
                                 GLint level,
                                 GLenum internalpixelformat,
                                 GLsizei width,
                                 GLsizei height,
                                 GLint border,
                                 GLenum pixelFormat,
                                 GLenum dataType,
                                 T[]? pixels,
                                 GLboolean enabled = true ) where T : unmanaged
    {
        if ( pixels == null )
        {
            return;
        }

        GetDelegateForFunction< PFNGLTEXIMAGE2DPROC >( "glTexImage2D", out _glTexImage2D );

        unsafe
        {
            fixed ( T* p = &pixels[ 0 ] )
            {
                if ( enabled )
                {
                    _glTexImage2D( target,
                                   level,
                                   internalpixelformat,
                                   width,
                                   height,
                                   border,
                                   pixelFormat,
                                   dataType,
                                   ( IntPtr )p );
                }
            }
        }
    }

    /// <inheritdoc />
    public void TexImage2D( GLenum target, GLint level, GLint border, Pixmap pixmap, GLboolean enabled = true )
    {
        Logger.Checkpoint();

        GetDelegateForFunction< PFNGLTEXIMAGE2DPROC >( "glTexImage2D", out _glTexImage2D );

        unsafe
        {
            fixed ( byte* p = &pixmap.PixelData[ 0 ] )
            {
                GL.PixelStorei( PixelStoreParameter.UnpackAlignment, 1 );

                if ( enabled )
                {
                    _glTexImage2D( target,                       // int
                                   level,                        // int
                                   pixmap.GLInternalPixelFormat, // int
                                   pixmap.Width,                 // int
                                   pixmap.Height,                // int
                                   border,                       // int
                                   pixmap.GLPixelFormat,         // int
                                   pixmap.GLDataType,            // int
                                   *p );                         // nint
                }
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void CopyTexImage1D( GLenum target, GLint level, GLenum internalFormat, GLint x, GLint y, GLsizei width, GLint border )
    {
        GetDelegateForFunction< PFNGLCOPYTEXIMAGE1DPROC >( "glCopyTexImage1D", out _glCopyTexImage1D );

        _glCopyTexImage1D( target, level, internalFormat, x, y, width, border );
    }

    // ========================================================================

    /// <inheritdoc />
    public void CopyTexImage2D( GLenum target, GLint level, GLenum internalFormat, GLint x, GLint y, GLsizei width, GLsizei height,
                                GLint border )
    {
        GetDelegateForFunction< PFNGLCOPYTEXIMAGE2DPROC >( "glCopyTexImage2D", out _glCopyTexImage2D );

        _glCopyTexImage2D( target, level, internalFormat, x, y, width, height, border );
    }

    // ========================================================================

    /// <inheritdoc />
    public void CopyTexSubImage1D( GLenum target, GLint level, GLint xoffset, GLint x, GLint y, GLsizei width )
    {
        GetDelegateForFunction< PFNGLCOPYTEXSUBIMAGE1DPROC >( "glCopyTexSubImage1D", out _glCopyTexSubImage1D );

        _glCopyTexSubImage1D( target, level, xoffset, x, y, width );
    }

    // ========================================================================

    /// <inheritdoc />
    public void CopyTexSubImage2D( GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint x, GLint y, GLsizei width,
                                   GLsizei height )
    {
        GetDelegateForFunction< PFNGLCOPYTEXSUBIMAGE2DPROC >( "glCopyTexSubImage2D", out _glCopyTexSubImage2D );

        _glCopyTexSubImage2D( target, level, xoffset, yoffset, x, y, width, height );
    }

    // ========================================================================

    /// <inheritdoc />
    public void TexSubImage1D( GLenum target, GLint level, GLint xoffset, GLsizei width, GLenum format, GLenum type, IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLTEXSUBIMAGE1DPROC >( "glTexSubImage1D", out _glTexSubImage1D );

        _glTexSubImage1D( target, level, xoffset, width, format, type, pixels );
    }

    /// <inheritdoc />
    public void TexSubImage1D< T >( GLenum target, GLint level, GLint xoffset, GLsizei width, GLenum format, GLenum type, T[] pixels )
        where T : unmanaged
    {
        GetDelegateForFunction< PFNGLTEXSUBIMAGE1DPROC >( "glTexSubImage1D", out _glTexSubImage1D );

        unsafe
        {
            fixed ( void* p = &pixels[ 0 ] )
            {
                var dataptr = ( IntPtr )p;

                _glTexSubImage1D( target, level, xoffset, width, format, type, dataptr );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void TexSubImage2D( GLenum target,
                               GLint level,
                               GLint xoffset,
                               GLint yoffset,
                               GLsizei width,
                               GLsizei height,
                               GLenum format,
                               GLenum type,
                               IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLTEXSUBIMAGE2DPROC >( "glTexSubImage2D", out _glTexSubImage2D );

        _glTexSubImage2D( target, level, xoffset, yoffset, width, height, format, type, pixels );
    }

    /// <inheritdoc />
    public void TexSubImage2D< T >( GLenum target,
                                    GLint level,
                                    GLint xoffset,
                                    GLint yoffset,
                                    GLsizei width,
                                    GLsizei height,
                                    GLenum format,
                                    GLenum type,
                                    T[] pixels ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLTEXSUBIMAGE2DPROC >( "glTexSubImage2D", out _glTexSubImage2D );

        unsafe
        {
            fixed ( void* p = &pixels[ 0 ] )
            {
                var dataptr = ( IntPtr )p;

                _glTexSubImage2D( target, level, xoffset, yoffset, width, height, format, type, dataptr );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void BindTexture( GLenum target, GLuint texture )
    {
        GetDelegateForFunction< PFNGLBINDTEXTUREPROC >( "glBindTexture", out _glBindTexture );

        _glBindTexture( target, texture );
    }

    public void BindTexture( TextureTarget target, GLuint texture )
    {
        BindTexture( ( int )target, texture );
    }

    // ========================================================================

    /// <inheritdoc />
    public unsafe void DeleteTextures( GLsizei n, GLuint* textures )
    {
        GetDelegateForFunction< PFNGLDELETETEXTURESPROC >( "glDeleteTextures", out _glDeleteTextures );

        _glDeleteTextures( n, textures );
    }

    /// <inheritdoc />
    public void DeleteTextures( params GLuint[] textures )
    {
        GetDelegateForFunction< PFNGLDELETETEXTURESPROC >( "glDeleteTextures", out _glDeleteTextures );

        unsafe
        {
            fixed ( void* p = &textures[ 0 ] )
            {
                _glDeleteTextures( textures.Length, ( GLuint* )p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public unsafe void GenTextures( GLsizei n, GLuint* textures )
    {
        GetDelegateForFunction< PFNGLGENTEXTURESPROC >( "glGenTextures", out _glGenTextures );

        _glGenTextures( n, textures );
    }

    /// <inheritdoc />
    public GLuint[] GenTextures( GLsizei n )
    {
        GetDelegateForFunction< PFNGLGENTEXTURESPROC >( "glGenTextures", out _glGenTextures );

        var textures = new GLuint[ n ];

        unsafe
        {
            fixed ( void* p = &textures[ 0 ] )
            {
                _glGenTextures( n, ( GLuint* )p );
            }
        }

        return textures;
    }

    /// <inheritdoc />
    public GLuint GenTexture()
    {
        return GenTextures( 1 )[ 0 ];
    }

    // ========================================================================

    /// <inheritdoc />
    public GLboolean IsGLTexture( GLuint texture )
    {
        GetDelegateForFunction< PFNGLISTEXTUREPROC >( "glIsTexture", out _glIsTexture );

        return _glIsTexture( texture );
    }

    // ========================================================================

    /// <inheritdoc />
    public void TexImage3D( GLenum target,
                            GLint level,
                            GLint internalFormat,
                            GLsizei width,
                            GLsizei height,
                            GLsizei depth,
                            GLint border,
                            GLenum format,
                            GLenum type,
                            IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLTEXIMAGE3DPROC >( "glTexImage3D", out _glTexImage3D );

        _glTexImage3D( target, level, internalFormat, width, height, depth, border, format, type, pixels );
    }

    /// <inheritdoc />
    public void TexImage3D< T >( GLenum target,
                                 GLint level,
                                 GLint internalFormat,
                                 GLsizei width,
                                 GLsizei height,
                                 GLsizei depth,
                                 GLint border,
                                 GLenum format,
                                 GLenum type,
                                 T[] pixels ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLTEXIMAGE3DPROC >( "glTexImage3D", out _glTexImage3D );

        unsafe
        {
            fixed ( T* pPixels = &pixels[ 0 ] )
            {
                var dataptr = ( IntPtr )pPixels;

                _glTexImage3D( target, level, internalFormat, width, height, depth, border, format, type, dataptr );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void TexSubImage3D( GLenum target,
                               GLint level,
                               GLint xoffset,
                               GLint yoffset,
                               GLint zoffset,
                               GLsizei width,
                               GLsizei height,
                               GLsizei depth,
                               GLenum format,
                               GLenum type,
                               IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLTEXSUBIMAGE3DPROC >( "glTexSubImage3D", out _glTexSubImage3D );

        _glTexSubImage3D( target, level, xoffset, yoffset, zoffset, width, height, depth, format, type, pixels );
    }

    /// <inheritdoc />
    public void TexSubImage3D< T >( GLenum target,
                                    GLint level,
                                    GLint xoffset,
                                    GLint yoffset,
                                    GLint zoffset,
                                    GLsizei width,
                                    GLsizei height,
                                    GLsizei depth,
                                    GLenum format,
                                    GLenum type,
                                    T[] pixels ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLTEXSUBIMAGE3DPROC >( "glTexSubImage3D", out _glTexSubImage3D );

        unsafe
        {
            fixed ( T* pPixels = &pixels[ 0 ] )
            {
                var dataptr = ( IntPtr )pPixels;

                _glTexSubImage3D( target, level, xoffset, yoffset, zoffset, width, height, depth, format, type, dataptr );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void CopyTexSubImage3D( GLenum target,
                                   GLint level,
                                   GLint xoffset,
                                   GLint yoffset,
                                   GLint zoffset,
                                   GLint x,
                                   GLint y,
                                   GLsizei width,
                                   GLsizei height )
    {
        GetDelegateForFunction< PFNGLCOPYTEXSUBIMAGE3DPROC >( "glCopyTexSubImage3D", out _glCopyTexSubImage3D );

        _glCopyTexSubImage3D( target, level, xoffset, yoffset, zoffset, x, y, width, height );
    }

    // ========================================================================

    /// <inheritdoc />
    public void ActiveTexture( TextureUnit texture )
    {
        ActiveTexture( ( int )texture );
    }

    /// <inheritdoc />
    public void ActiveTexture( GLenum texture )
    {
        GetDelegateForFunction< PFNGLACTIVETEXTUREPROC >( "glActiveTexture", out _glActiveTexture );

        _glActiveTexture( texture );
    }

    // ========================================================================

    /// <inheritdoc />
    public void CompressedTexImage3D( GLenum target,
                                      GLint level,
                                      GLenum internalFormat,
                                      GLsizei width,
                                      GLsizei height,
                                      GLsizei depth,
                                      GLint border,
                                      GLsizei imageSize,
                                      IntPtr data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXIMAGE3DPROC >( "glCompressedTexImage3D", out _glCompressedTexImage3D );

        _glCompressedTexImage3D( target, level, internalFormat, width, height, depth, border, imageSize, data );
    }

    /// <inheritdoc />
    public void CompressedTexImage3D( GLenum target,
                                      GLint level,
                                      GLenum internalFormat,
                                      GLsizei width,
                                      GLsizei height,
                                      GLsizei depth,
                                      GLint border,
                                      byte[] data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXIMAGE3DPROC >( "glCompressedTexImage3D", out _glCompressedTexImage3D );

        unsafe
        {
            fixed ( byte* p = &data[ 0 ] )
            {
                _glCompressedTexImage3D( target, level, internalFormat, width, height, depth, border, data.Length, ( IntPtr )p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void CompressedTexImage2D( GLenum target,
                                      GLint level,
                                      GLenum internalFormat,
                                      GLsizei width,
                                      GLsizei height,
                                      GLint border,
                                      GLsizei imageSize,
                                      IntPtr data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXIMAGE2DPROC >( "glCompressedTexImage2D", out _glCompressedTexImage2D );

        _glCompressedTexImage2D( target, level, internalFormat, width, height, border, imageSize, data );
    }

    /// <inheritdoc />
    public void CompressedTexImage2D( GLenum target, GLint level, GLenum internalFormat, GLsizei width, GLsizei height, GLint border,
                                      byte[] data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXIMAGE2DPROC >( "glCompressedTexImage2D", out _glCompressedTexImage2D );

        unsafe
        {
            fixed ( byte* p = &data[ 0 ] )
            {
                _glCompressedTexImage2D( target, level, internalFormat, width, height, border, data.Length, ( IntPtr )p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void CompressedTexImage1D( GLenum target, GLint level, GLenum internalFormat, GLsizei width, GLint border, GLsizei imageSize,
                                      IntPtr data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXIMAGE1DPROC >( "glCompressedTexImage1D", out _glCompressedTexImage1D );

        _glCompressedTexImage1D( target, level, internalFormat, width, border, imageSize, data );
    }

    /// <inheritdoc />
    public void CompressedTexImage1D( GLenum target, GLint level, GLenum internalFormat, GLsizei width, GLint border, byte[] data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXIMAGE1DPROC >( "glCompressedTexImage1D", out _glCompressedTexImage1D );

        unsafe
        {
            fixed ( byte* p = &data[ 0 ] )
            {
                _glCompressedTexImage1D( target, level, internalFormat, width, border, data.Length, ( IntPtr )p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void CompressedTexSubImage3D( GLenum target,
                                         GLint level,
                                         GLint xoffset,
                                         GLint yoffset,
                                         GLint zoffset,
                                         GLsizei width,
                                         GLsizei height,
                                         GLsizei depth,
                                         GLenum format,
                                         GLsizei imageSize,
                                         IntPtr data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXSUBIMAGE3DPROC >( "glCompressedTexSubImage3D", out _glCompressedTexSubImage3D );

        _glCompressedTexSubImage3D( target, level, xoffset, yoffset, zoffset, width, height, depth, format, imageSize, data );
    }

    /// <inheritdoc />
    public void CompressedTexSubImage3D( GLenum target,
                                         GLint level,
                                         GLint xoffset,
                                         GLint yoffset,
                                         GLint zoffset,
                                         GLsizei width,
                                         GLsizei height,
                                         GLsizei depth,
                                         GLenum format,
                                         byte[] data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXSUBIMAGE3DPROC >( "glCompressedTexSubImage3D", out _glCompressedTexSubImage3D );

        unsafe
        {
            fixed ( byte* p = &data[ 0 ] )
            {
                _glCompressedTexSubImage3D( target, level, xoffset, yoffset, zoffset, width, height, depth, format, data.Length,
                                            ( IntPtr )p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void CompressedTexSubImage2D( GLenum target,
                                         GLint level,
                                         GLint xoffset,
                                         GLint yoffset,
                                         GLsizei width,
                                         GLsizei height,
                                         GLenum format,
                                         GLsizei imageSize,
                                         IntPtr data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXSUBIMAGE2DPROC >( "glCompressedTexSubImage2D", out _glCompressedTexSubImage2D );

        _glCompressedTexSubImage2D( target, level, xoffset, yoffset, width, height, format, imageSize, data );
    }

    /// <inheritdoc />
    public void CompressedTexSubImage2D( GLenum target,
                                         GLint level,
                                         GLint xoffset,
                                         GLint yoffset,
                                         GLsizei width,
                                         GLsizei height,
                                         GLenum format,
                                         byte[] data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXSUBIMAGE2DPROC >( "glCompressedTexSubImage2D", out _glCompressedTexSubImage2D );

        unsafe
        {
            fixed ( byte* p = &data[ 0 ] )
            {
                _glCompressedTexSubImage2D( target, level, xoffset, yoffset, width, height, format, data.Length, ( IntPtr )p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void CompressedTexSubImage1D( GLenum target, GLint level, GLint xoffset, GLsizei width, GLenum format, GLsizei imageSize,
                                         IntPtr data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXSUBIMAGE1DPROC >( "glCompressedTexSubImage1D", out _glCompressedTexSubImage1D );

        _glCompressedTexSubImage1D( target, level, xoffset, width, format, imageSize, data );
    }

    /// <inheritdoc />
    public void CompressedTexSubImage1D( GLenum target, GLint level, GLint xoffset, GLsizei width, GLenum format, byte[] data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXSUBIMAGE1DPROC >( "glCompressedTexSubImage1D", out _glCompressedTexSubImage1D );

        unsafe
        {
            fixed ( byte* p = &data[ 0 ] )
            {
                _glCompressedTexSubImage1D( target, level, xoffset, width, format, data.Length, ( IntPtr )p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetCompressedTexImage( GLenum target, GLint level, IntPtr img )
    {
        GetDelegateForFunction< PFNGLGETCOMPRESSEDTEXIMAGEPROC >( "glGetCompressedTexImage", out _glGetCompressedTexImage );

        _glGetCompressedTexImage( target, level, img );
    }

    /// <inheritdoc />
    public void GetCompressedTexImage( GLenum target, GLint level, ref byte[] img )
    {
        GetDelegateForFunction< PFNGLGETCOMPRESSEDTEXIMAGEPROC >( "glGetCompressedTexImage", out _glGetCompressedTexImage );

        unsafe
        {
            fixed ( byte* p = &img[ 0 ] )
            {
                _glGetCompressedTexImage( target, level, ( IntPtr )p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void TexParameterf( GLenum target, GLenum pname, GLfloat param )
    {
        GetDelegateForFunction< PFNGLTEXPARAMETERFPROC >( "glTexParameterf", out _glTexParameterf );

        _glTexParameterf( target, pname, param );
    }

    // ========================================================================

    /// <inheritdoc />
    public unsafe void TexParameterfv( GLenum target, GLenum pname, GLfloat* parameters )
    {
        GetDelegateForFunction< PFNGLTEXPARAMETERFVPROC >( "glTexParameterfv", out _glTexParameterfv );

        _glTexParameterfv( target, pname, parameters );
    }

    /// <inheritdoc />
    public void TexParameterfv( GLenum target, GLenum pname, GLfloat[] parameters )
    {
        GetDelegateForFunction< PFNGLTEXPARAMETERFVPROC >( "glTexParameterfv", out _glTexParameterfv );

        unsafe
        {
            fixed ( GLfloat* p = &parameters[ 0 ] )
            {
                _glTexParameterfv( target, pname, p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void TexParameteri( GLenum target, GLenum pname, GLint param )
    {
        GetDelegateForFunction< PFNGLTEXPARAMETERIPROC >( "glTexParameteri", out _glTexParameteri );

        _glTexParameteri( target, pname, param );
    }

    // ========================================================================

    /// <inheritdoc />
    public unsafe void TexParameteriv( GLenum target, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLTEXPARAMETERIVPROC >( "glTexParameteriv", out _glTexParameteriv );

        _glTexParameteriv( target, pname, parameters );
    }

    /// <inheritdoc />
    public void TexParameteriv( GLenum target, GLenum pname, GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLTEXPARAMETERIVPROC >( "glTexParameteriv", out _glTexParameteriv );

        unsafe
        {
            fixed ( GLint* p = &parameters[ 0 ] )
            {
                _glTexParameteriv( target, pname, p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetTexImage( GLenum target, GLint level, GLenum format, GLenum type, IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLGETTEXIMAGEPROC >( "glGetTexImage", out _glGetTexImage );

        _glGetTexImage( target, level, format, type, pixels );
    }

    /// <inheritdoc />
    public void GetTexImage< T >( GLenum target, GLint level, GLenum format, GLenum type, ref T[] pixels ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLGETTEXIMAGEPROC >( "glGetTexImage", out _glGetTexImage );

        unsafe
        {
            fixed ( void* p = &pixels[ 0 ] )
            {
                _glGetTexImage( target, level, format, type, ( IntPtr )p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public unsafe void GetTexParameterfv( GLenum target, GLenum pname, GLfloat* parameters )
    {
        GetDelegateForFunction< PFNGLGETTEXPARAMETERFVPROC >( "glGetTexParameterfv", out _glGetTexParameterfv );

        _glGetTexParameterfv( target, pname, parameters );
    }

    /// <inheritdoc />
    public void GetTexParameterfv( GLenum target, GLenum pname, ref GLfloat[] parameters )
    {
        GetDelegateForFunction< PFNGLGETTEXPARAMETERFVPROC >( "glGetTexParameterfv", out _glGetTexParameterfv );

        unsafe
        {
            fixed ( GLfloat* p = &parameters[ 0 ] )
            {
                _glGetTexParameterfv( target, pname, p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public unsafe void GetTexParameteriv( GLenum target, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETTEXPARAMETERIVPROC >( "glGetTexParameteriv", out _glGetTexParameteriv );

        _glGetTexParameteriv( target, pname, parameters );
    }

    /// <inheritdoc />
    public void GetTexParameteriv( GLenum target, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETTEXPARAMETERIVPROC >( "glGetTexParameteriv", out _glGetTexParameteriv );

        unsafe
        {
            fixed ( GLint* p = &parameters[ 0 ] )
            {
                _glGetTexParameteriv( target, pname, p );
            }
        }
    }

    public void GetTexParameteriv( int target, int pname, out int data )
    {
        var tempArray = new int[ 1 ];

        GetTexParameteriv( ( GLenum )target, ( GLenum )pname, ref tempArray );

        data = tempArray[ 0 ];
    }

    // ========================================================================

    /// <inheritdoc />
    public unsafe void GetTexLevelParameterfv( GLenum target, GLint level, GLenum pname, GLfloat* parameters )
    {
        GetDelegateForFunction< PFNGLGETTEXLEVELPARAMETERFVPROC >( "glGetTexLevelParameterfv", out _glGetTexLevelParameterfv );

        _glGetTexLevelParameterfv( target, level, pname, parameters );
    }

    /// <inheritdoc />
    public void GetTexLevelParameterfv( GLenum target, GLint level, GLenum pname, ref GLfloat[] parameters )
    {
        GetDelegateForFunction< PFNGLGETTEXLEVELPARAMETERFVPROC >( "glGetTexLevelParameterfv", out _glGetTexLevelParameterfv );

        unsafe
        {
            fixed ( GLfloat* p = &parameters[ 0 ] )
            {
                _glGetTexLevelParameterfv( target, level, pname, p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public unsafe void GetTexLevelParameteriv( GLenum target, GLint level, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETTEXLEVELPARAMETERIVPROC >( "glGetTexLevelParameteriv", out _glGetTexLevelParameteriv );

        _glGetTexLevelParameteriv( target, level, pname, parameters );
    }

    /// <inheritdoc />
    public void GetTexLevelParameteriv( GLenum target, GLint level, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETTEXLEVELPARAMETERIVPROC >( "glGetTexLevelParameteriv", out _glGetTexLevelParameteriv );

        unsafe
        {
            fixed ( GLint* p = &parameters[ 0 ] )
            {
                _glGetTexLevelParameteriv( target, level, pname, p );
            }
        }
    }

    public void GetTexLevelParameteriv( TextureTarget target, int level, TextureParameter pname, out int data )
    {
        // Create a temporary array to hold the result from the underlying OpenGL method.
        var tempArray = new int[ 1 ];

        // Call the original method with the temporary array.
        GetTexLevelParameteriv( ( int )target, level, ( int )pname, ref tempArray );

        // Assign the value from the temporary array to the out parameter.
        data = tempArray[ 0 ];
    }

    // ========================================================================

    /// <inheritdoc />
    public void TexImage2DMultisample( GLenum target,
                                       GLsizei samples,
                                       GLenum internalFormat,
                                       GLsizei width,
                                       GLsizei height,
                                       GLboolean fixedsamplelocations )
    {
        GetDelegateForFunction< PFNGLTEXIMAGE2DMULTISAMPLEPROC >( "glTexImage2DMultisample", out _glTexImage2DMultisample );

        _glTexImage2DMultisample( target, samples, internalFormat, width, height, fixedsamplelocations );
    }

    // ========================================================================

    /// <inheritdoc />
    public void TexImage3DMultisample( GLenum target,
                                       GLsizei samples,
                                       GLenum internalFormat,
                                       GLsizei width,
                                       GLsizei height,
                                       GLsizei depth,
                                       GLboolean fixedsamplelocations )
    {
        GetDelegateForFunction< PFNGLTEXIMAGE3DMULTISAMPLEPROC >( "glTexImage3DMultisample", out _glTexImage3DMultisample );

        _glTexImage3DMultisample( target, samples, internalFormat, width, height, depth, fixedsamplelocations );
    }
}

// ============================================================================
// ============================================================================
