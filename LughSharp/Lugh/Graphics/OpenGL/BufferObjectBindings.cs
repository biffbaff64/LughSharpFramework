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


// ============================================================================

using GLenum = int;
using GLfloat = float;
using GLint = int;
using GLsizei = int;
using GLbitfield = uint;
using GLdouble = double;
using GLuint = uint;
using GLboolean = bool;
using GLubyte = byte;
using GLsizeiptr = int;
using GLintptr = int;
using GLshort = short;
using GLbyte = sbyte;
using GLushort = ushort;
using GLchar = byte;
using GLuint64 = ulong;
using GLint64 = long;

// ============================================================================

namespace LughSharp.Lugh.Graphics.OpenGL;

public unsafe partial class GLBindings
{
    /// <inheritdoc />
    public void BindBuffer( GLenum target, GLuint buffer )
    {
        GetDelegateForFunction< PFNGLBINDBUFFERPROC >( "glBindBuffer", out _glBindBuffer );

        _glBindBuffer( target, buffer );
    }

    // ========================================================================

    /// <inheritdoc />
    public void DeleteBuffers( GLsizei n, GLuint* buffers )
    {
        GetDelegateForFunction< PFNGLDELETEBUFFERSPROC >( "glDeleteBuffers", out _glDeleteBuffers );

        _glDeleteBuffers( n, buffers );
    }

    /// <inheritdoc />
    public void DeleteBuffers( params GLuint[] buffers )
    {
        GetDelegateForFunction< PFNGLDELETEBUFFERSPROC >( "glDeleteBuffers", out _glDeleteBuffers );

        {
            fixed ( GLuint* p = &buffers[ 0 ] )
            {
                _glDeleteBuffers( buffers.Length, p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GenBuffers( GLsizei n, GLuint* buffers )
    {
        GetDelegateForFunction< PFNGLGENBUFFERSPROC >( "glGenBuffers", out _glGenBuffers );

        _glGenBuffers( n, buffers );
    }

    /// <inheritdoc />
    public GLuint[] GenBuffers( GLsizei n )
    {
        GetDelegateForFunction< PFNGLGENBUFFERSPROC >( "glGenBuffers", out _glGenBuffers );

        {
            var ret = new GLuint[ n ];

            fixed ( GLuint* p = &ret[ 0 ] )
            {
                _glGenBuffers( n, p );
            }

            return ret;
        }
    }

    /// <inheritdoc />
    public GLuint GenBuffer()
    {
        return GenBuffers( 1 )[ 0 ];
    }

    // ========================================================================

    /// <inheritdoc />
    public GLboolean IsBuffer( GLuint buffer )
    {
        GetDelegateForFunction< PFNGLISBUFFERPROC >( "glIsBuffer", out _glIsBuffer );

        return _glIsBuffer( buffer );
    }

    // ========================================================================

    /// <inheritdoc />
    public void BufferData( GLenum target, GLsizeiptr size, IntPtr data, GLenum usage )
    {
        GetDelegateForFunction< PFNGLBUFFERDATAPROC >( "glBufferData", out _glBufferData );

        _glBufferData( target, size, data, usage );

        CheckErrors();
    }

    /// <inheritdoc />
    public void BufferData< T >( GLenum target, T[] data, GLenum usage ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLBUFFERDATAPROC >( "glBufferData", out _glBufferData );

        fixed ( T* p = &data[ 0 ] )
        {
            _glBufferData( target, sizeof( T ) * data.Length, ( IntPtr )p, usage );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void BufferSubData( GLenum target, GLintptr offset, GLsizeiptr size, IntPtr data )
    {
        GetDelegateForFunction< PFNGLBUFFERSUBDATAPROC >( "glBufferSubData", out _glBufferSubData );

        _glBufferSubData( target, offset, size, data );
    }

    /// <inheritdoc />
    public void BufferSubData< T >( GLenum target, GLintptr offsetCount, T[] data ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLBUFFERSUBDATAPROC >( "glBufferSubData", out _glBufferSubData );

        fixed ( T* p = &data[ 0 ] )
        {
            _glBufferSubData( target, offsetCount * sizeof( T ), sizeof( T ) * data.Length, ( IntPtr )p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetBufferSubData( GLenum target, GLintptr offset, GLsizeiptr size, IntPtr data )
    {
        GetDelegateForFunction< PFNGLGETBUFFERSUBDATAPROC >( "glGetBufferSubData", out _glGetBufferSubData );

        _glGetBufferSubData( target, offset, size, data );
    }

    /// <inheritdoc />
    public void GetBufferSubData< T >( GLenum target, GLintptr offsetCount, GLsizei count, ref T[] data ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLGETBUFFERSUBDATAPROC >( "glGetBufferSubData", out _glGetBufferSubData );

        fixed ( T* p = &data[ 0 ] )
        {
            _glGetBufferSubData( target, offsetCount * sizeof( T ), sizeof( T ) * count, ( IntPtr )p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public IntPtr MapBuffer( GLenum target, GLenum access )
    {
        GetDelegateForFunction< PFNGLMAPBUFFERPROC >( "glMapBuffer", out _glMapBuffer );

        return _glMapBuffer( target, access );
    }

    /// <inheritdoc />
    public Span< T > MapBuffer< T >( GLenum target, GLenum access ) where T : unmanaged
    {
        GLint size;

        GetDelegateForFunction< PFNGLGETBUFFERPARAMETERIVPROC >( "glGetBufferParameteriv", out _glGetBufferParameteriv );

        _glGetBufferParameteriv( target, IGL.GL_BUFFER_SIZE, &size );

        GetDelegateForFunction< PFNGLMAPBUFFERPROC >( "glMapBuffer", out _glMapBuffer );

        var ret = _glMapBuffer( target, access );

        return new Span< T >( ( void* )ret, size / sizeof( T ) );
    }

    // ========================================================================

    /// <inheritdoc />
    public GLboolean UnmapBuffer( GLenum target )
    {
        GetDelegateForFunction< PFNGLUNMAPBUFFERPROC >( "glUnmapBuffer", out _glUnmapBuffer );

        return _glUnmapBuffer( target );
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetBufferParameteriv( GLenum target, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETBUFFERPARAMETERIVPROC >( "glGetBufferParameteriv", out _glGetBufferParameteriv );

        _glGetBufferParameteriv( target, pname, parameters );
    }

    /// <inheritdoc />
    public void GetBufferParameteriv( GLenum target, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETBUFFERPARAMETERIVPROC >( "glGetBufferParameteriv", out _glGetBufferParameteriv );

        fixed ( GLint* p = &parameters[ 0 ] )
        {
            _glGetBufferParameteriv( target, pname, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetBufferPointerv( GLenum target, GLenum pname, IntPtr* parameters )
    {
        GetDelegateForFunction< PFNGLGETBUFFERPOINTERVPROC >( "glGetBufferPointerv", out _glGetBufferPointerv );

        _glGetBufferPointerv( target, pname, parameters );
    }

    /// <inheritdoc />
    public void GetBufferPointerv( GLenum target, GLenum pname, ref IntPtr[] parameters )
    {
        GetDelegateForFunction< PFNGLGETBUFFERPOINTERVPROC >( "glGetBufferPointerv", out _glGetBufferPointerv );

        fixed ( IntPtr* p = &parameters[ 0 ] )
        {
            _glGetBufferPointerv( target, pname, ( IntPtr* )p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void DrawBuffers( GLsizei n, GLenum* bufs )
    {
        GetDelegateForFunction< PFNGLDRAWBUFFERSPROC >( "glDrawBuffers", out _glDrawBuffers );

        _glDrawBuffers( n, bufs );
    }

    /// <inheritdoc />
    public void DrawBuffers( params GLenum[] bufs )
    {
        GetDelegateForFunction< PFNGLDRAWBUFFERSPROC >( "glDrawBuffers", out _glDrawBuffers );

        fixed ( GLenum* pbufs = &bufs[ 0 ] )
        {
            _glDrawBuffers( bufs.Length, pbufs );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void BindBufferRange( GLenum target, GLuint index, GLuint buffer, GLintptr offset, GLsizeiptr size )
    {
        GetDelegateForFunction< PFNGLBINDBUFFERRANGEPROC >( "glBindBufferRange", out _glBindBufferRange );

        _glBindBufferRange( target, index, buffer, offset, size );
    }

    // ========================================================================

    /// <inheritdoc />
    public void BindBufferBase( GLenum target, GLuint index, GLuint buffer )
    {
        GetDelegateForFunction< PFNGLBINDBUFFERBASEPROC >( "glBindBufferBase", out _glBindBufferBase );

        _glBindBufferBase( target, index, buffer );
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetBufferParameteri64v( GLenum target, GLenum pname, GLint64* parameters )
    {
        GetDelegateForFunction< PFNGLGETBUFFERPARAMETERI64VPROC >( "glGetBufferParameteri64v", out _glGetBufferParameteri64v );

        _glGetBufferParameteri64v( target, pname, parameters );
    }

    /// <inheritdoc />
    public void GetBufferParameteri64v( GLenum target, GLenum pname, ref GLint64[] parameters )
    {
        GetDelegateForFunction< PFNGLGETBUFFERPARAMETERI64VPROC >( "glGetBufferParameteri64v", out _glGetBufferParameteri64v );

        {
            fixed ( GLint64* dp = &parameters[ 0 ] )
            {
                _glGetBufferParameteri64v( target, pname, dp );
            }
        }
    }

    // ========================================================================

    public void ClearBufferData( GLenum target, GLenum internalFormat, GLenum format, GLenum type, IntPtr data )
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERDATAPROC >( "glClearBufferData", out _glClearBufferData );

        _glClearBufferData( target, internalFormat, format, type, data );
    }

    public void ClearBufferData< T >( GLenum target, GLenum internalFormat, GLenum format, GLenum type, T[] data ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERDATAPROC >( "glClearBufferData", out _glClearBufferData );

        fixed ( T* t = &data[ 0 ] )
        {
            _glClearBufferData( target, internalFormat, format, type, ( IntPtr )t );
        }
    }

    // ========================================================================

    public void ClearBufferSubData( GLenum target, GLenum internalFormat, GLintptr offset, GLsizeiptr size, GLenum format, GLenum type,
                                    IntPtr data )
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERSUBDATAPROC >( "glClearBufferSubData", out _glClearBufferSubData );

        _glClearBufferSubData( target, internalFormat, offset, size, format, type, data );
    }

    public void ClearBufferSubData< T >( GLenum target, GLenum internalFormat, GLintptr offset, GLsizeiptr size, GLenum format, GLenum type,
                                         T[] data ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERSUBDATAPROC >( "glClearBufferSubData", out _glClearBufferSubData );

        fixed ( T* t = &data[ 0 ] )
        {
            _glClearBufferSubData( target, internalFormat, offset, size, format, type, ( IntPtr )t );
        }
    }

    // ========================================================================

    public void InvalidateBufferSubData( GLuint buffer, GLintptr offset, GLsizeiptr length )
    {
        GetDelegateForFunction< PFNGLINVALIDATEBUFFERSUBDATAPROC >( "glInvalidateBufferSubData", out _glInvalidateBufferSubData );

        _glInvalidateBufferSubData( buffer, offset, length );
    }

    // ========================================================================

    public void InvalidateBufferData( GLuint buffer )
    {
        GetDelegateForFunction< PFNGLINVALIDATEBUFFERDATAPROC >( "glInvalidateBufferData", out _glInvalidateBufferData );

        _glInvalidateBufferData( buffer );
    }

    // ========================================================================

    public void ClearBufferiv( GLenum buffer, GLint drawbuffer, GLint* value )
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERIVPROC >( "glClearBufferiv", out _glClearBufferiv );

        _glClearBufferiv( buffer, drawbuffer, value );
    }

    public void ClearBufferiv( GLenum buffer, GLint drawbuffer, GLint[] value )
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERIVPROC >( "glClearBufferiv", out _glClearBufferiv );

        fixed ( GLint* p = &value[ 0 ] )
        {
            _glClearBufferiv( buffer, drawbuffer, p );
        }
    }

    // ========================================================================

    public void ClearBufferuiv( GLenum buffer, GLint drawbuffer, GLuint* value )
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERUIVPROC >( "glClearBufferuiv", out _glClearBufferuiv );

        _glClearBufferuiv( buffer, drawbuffer, value );
    }

    public void ClearBufferuiv( GLenum buffer, GLint drawbuffer, GLuint[] value )
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERUIVPROC >( "glClearBufferuiv", out _glClearBufferuiv );

        fixed ( GLuint* p = &value[ 0 ] )
        {
            _glClearBufferuiv( buffer, drawbuffer, p );
        }
    }

    // ========================================================================

    public void ClearBufferfv( GLenum buffer, GLint drawbuffer, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERFVPROC >( "glClearBufferfv", out _glClearBufferfv );

        _glClearBufferfv( buffer, drawbuffer, value );
    }

    public void ClearBufferfv( GLenum buffer, GLint drawbuffer, GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERFVPROC >( "glClearBufferfv", out _glClearBufferfv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glClearBufferfv( buffer, drawbuffer, p );
        }
    }

    // ========================================================================

    public void ClearBufferfi( GLenum buffer, GLint drawbuffer, GLfloat depth, GLint stencil )
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERFIPROC >( "glClearBufferfi", out _glClearBufferfi );

        _glClearBufferfi( buffer, drawbuffer, depth, stencil );
    }

    // ========================================================================

}