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
    public void BindVertexArray( GLuint array )
    {
        GetDelegateForFunction< PFNGLBINDVERTEXARRAYPROC >( "glBindVertexArray", out _glBindVertexArray );

        _glBindVertexArray( array );
    }

    // ========================================================================

    public void DeleteVertexArrays( GLsizei n, GLuint* arrays )
    {
        GetDelegateForFunction< PFNGLDELETEVERTEXARRAYSPROC >( "glDeleteVertexArrays", out _glDeleteVertexArrays );

        _glDeleteVertexArrays( n, arrays );
    }

    public void DeleteVertexArrays( params GLuint[] arrays )
    {
        GetDelegateForFunction< PFNGLDELETEVERTEXARRAYSPROC >( "glDeleteVertexArrays", out _glDeleteVertexArrays );

        fixed ( GLuint* p = &arrays[ 0 ] )
        {
            _glDeleteVertexArrays( arrays.Length, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GenVertexArrays( GLsizei n, GLuint* arrays )
    {
        GetDelegateForFunction< PFNGLGENVERTEXARRAYSPROC >( "glGenVertexArrays", out _glGenVertexArrays );

        _glGenVertexArrays( n, arrays );
    }

    /// <inheritdoc />
    public GLuint[] GenVertexArrays( GLsizei n )
    {
        var arrays = new GLuint[ n ];

        GetDelegateForFunction< PFNGLGENVERTEXARRAYSPROC >( "glGenVertexArrays", out _glGenVertexArrays );

        fixed ( GLuint* p = &arrays[ 0 ] )
        {
            _glGenVertexArrays( n, p );
        }

        return arrays;
    }

    /// <inheritdoc />
    public GLuint GenVertexArray()
    {
        GLuint array = 0;

        GetDelegateForFunction< PFNGLGENVERTEXARRAYSPROC >( "glGenVertexArrays", out _glGenVertexArrays );

        _glGenVertexArrays( 1, &array );

        return array;
    }

    // ========================================================================

    /// <inheritdoc />
    public GLboolean IsVertexArray( GLuint array )
    {
        GetDelegateForFunction< PFNGLISVERTEXARRAYPROC >( "glIsVertexArray", out _glIsVertexArray );

        return _glIsVertexArray( array );
    }

    // ========================================================================

    public void CreateVertexArrays( GLsizei n, GLuint* arrays )
    {
        GetDelegateForFunction< PFNGLCREATEVERTEXARRAYSPROC >( "glCreateVertexArrays", out _glCreateVertexArrays );

        _glCreateVertexArrays( n, arrays );
    }

    public GLuint[] CreateVertexArrays( GLsizei n )
    {
        var arrays = new GLuint[ n ];

        GetDelegateForFunction< PFNGLCREATEVERTEXARRAYSPROC >( "glCreateVertexArrays", out _glCreateVertexArrays );

        fixed ( GLuint* ptrArrays = &arrays[ 0 ] )
        {
            _glCreateVertexArrays( n, ptrArrays );
        }

        return arrays;
    }

    public GLuint CreateVertexArray()
    {
        return CreateVertexArrays( 1 )[ 0 ];
    }

    // ========================================================================

    public void DisableVertexArrayAttrib( GLuint vaobj, GLuint index )
    {
        GetDelegateForFunction< PFNGLDISABLEVERTEXARRAYATTRIBPROC >( "glDisableVertexArrayAttrib", out _glDisableVertexArrayAttrib );

        _glDisableVertexArrayAttrib( vaobj, index );
    }

    // ========================================================================

    public void EnableVertexArrayAttrib( GLuint vaobj, GLuint index )
    {
        GetDelegateForFunction< PFNGLENABLEVERTEXARRAYATTRIBPROC >( "glEnableVertexArrayAttrib", out _glEnableVertexArrayAttrib );

        _glEnableVertexArrayAttrib( vaobj, index );
    }

    // ========================================================================

    public void VertexArrayElementBuffer( GLuint vaobj, GLuint buffer )
    {
        GetDelegateForFunction< PFNGLVERTEXARRAYELEMENTBUFFERPROC >( "glVertexArrayElementBuffer", out _glVertexArrayElementBuffer );

        _glVertexArrayElementBuffer( vaobj, buffer );
    }

    // ========================================================================

    public void VertexArrayVertexBuffer( GLuint vaobj, GLuint bindingindex, GLuint buffer, GLintptr offset, GLsizei stride )
    {
        GetDelegateForFunction< PFNGLVERTEXARRAYVERTEXBUFFERPROC >( "glVertexArrayVertexBuffer", out _glVertexArrayVertexBuffer );

        _glVertexArrayVertexBuffer( vaobj, bindingindex, buffer, offset, stride );
    }

    public void VertexArrayVertexBuffers( GLuint vaobj, GLuint first, GLsizei count, GLuint* buffers, GLintptr* offsets, GLsizei* strides )
    {
        GetDelegateForFunction< PFNGLVERTEXARRAYVERTEXBUFFERPROC >( "glVertexArrayVertexBuffer", out _glVertexArrayVertexBuffer );

        if ( _glVertexArrayVertexBuffers != null )
        {
            _glVertexArrayVertexBuffers( vaobj, first, count, buffers, offsets, strides );
        }
    }

    // ========================================================================

    public void VertexArrayVertexBuffers( GLuint vaobj, GLuint first, GLuint[] buffers, GLintptr[] offsets, GLsizei[] strides )
    {
        GetDelegateForFunction< PFNGLVERTEXARRAYVERTEXBUFFERSPROC >( "glVertexArrayVertexBuffers", out _glVertexArrayVertexBuffers );

        fixed ( GLuint* ptrBuffers = &buffers[ 0 ] )
        fixed ( GLintptr* ptrOffsets = &offsets[ 0 ] )
        fixed ( GLsizei* ptrStrides = &strides[ 0 ] )
        {
            _glVertexArrayVertexBuffers( vaobj, first, ( GLsizei )buffers.Length, ptrBuffers, ptrOffsets, ptrStrides );
        }
    }

    // ========================================================================

    public void VertexArrayAttribBinding( GLuint vaobj, GLuint attribindex, GLuint bindingindex )
    {
        GetDelegateForFunction< PFNGLVERTEXARRAYATTRIBBINDINGPROC >( "glVertexArrayAttribBinding", out _glVertexArrayAttribBinding );

        _glVertexArrayAttribBinding( vaobj, attribindex, bindingindex );
    }

    // ========================================================================

    public void VertexArrayAttribFormat( GLuint vaobj, GLuint attribindex, GLint size, GLenum type, GLboolean normalized,
                                         GLuint relativeoffset )
    {
        GetDelegateForFunction< PFNGLVERTEXARRAYATTRIBFORMATPROC >( "glVertexArrayAttribFormat", out _glVertexArrayAttribFormat );

        _glVertexArrayAttribFormat( vaobj, attribindex, size, type, normalized, relativeoffset );
    }

    // ========================================================================

    public void VertexArrayAttribIFormat( GLuint vaobj, GLuint attribindex, GLint size, GLenum type, GLuint relativeoffset )
    {
        GetDelegateForFunction< PFNGLVERTEXARRAYATTRIBIFORMATPROC >( "glVertexArrayAttribIFormat", out _glVertexArrayAttribIFormat );

        _glVertexArrayAttribIFormat( vaobj, attribindex, size, type, relativeoffset );
    }

    // ========================================================================

    public void VertexArrayAttribLFormat( GLuint vaobj, GLuint attribindex, GLint size, GLenum type, GLuint relativeoffset )
    {
        GetDelegateForFunction< PFNGLVERTEXARRAYATTRIBLFORMATPROC >( "glVertexArrayAttribLFormat", out _glVertexArrayAttribLFormat );

        _glVertexArrayAttribLFormat( vaobj, attribindex, size, type, relativeoffset );
    }

    // ========================================================================

    public void VertexArrayBindingDivisor( GLuint vaobj, GLuint bindingindex, GLuint divisor )
    {
        GetDelegateForFunction< PFNGLVERTEXARRAYBINDINGDIVISORPROC >( "glVertexArrayBindingDivisor", out _glVertexArrayBindingDivisor );

        _glVertexArrayBindingDivisor( vaobj, bindingindex, divisor );
    }

    // ========================================================================

    public void GetVertexArrayiv( GLuint vaobj, GLenum pname, GLint* param )
    {
        GetDelegateForFunction< PFNGLGETVERTEXARRAYIVPROC >( "glGetVertexArrayiv", out _glGetVertexArrayiv );

        _glGetVertexArrayiv( vaobj, pname, param );
    }

    public void GetVertexArrayiv( GLuint vaobj, GLenum pname, ref GLint[] param )
    {
        GetDelegateForFunction< PFNGLGETVERTEXARRAYIVPROC >( "glGetVertexArrayiv", out _glGetVertexArrayiv );

        fixed ( GLint* ptrParam = &param[ 0 ] )
        {
            _glGetVertexArrayiv( vaobj, pname, ptrParam );
        }
    }

    // ========================================================================

    public void GetVertexArrayIndexediv( GLuint vaobj, GLuint index, GLenum pname, GLint* param )
    {
        GetDelegateForFunction< PFNGLGETVERTEXARRAYINDEXEDIVPROC >( "glGetVertexArrayIndexediv", out _glGetVertexArrayIndexediv );

        _glGetVertexArrayIndexediv( vaobj, index, pname, param );
    }

    public void GetVertexArrayIndexediv( GLuint vaobj, GLuint index, GLenum pname, ref GLint[] param )
    {
        GetDelegateForFunction< PFNGLGETVERTEXARRAYINDEXEDIVPROC >( "glGetVertexArrayIndexediv", out _glGetVertexArrayIndexediv );

        fixed ( GLint* ptrParam = &param[ 0 ] )
        {
            _glGetVertexArrayIndexediv( vaobj, index, pname, ptrParam );
        }
    }

    // ========================================================================

    public void GetVertexArrayIndexed64iv( GLuint vaobj, GLuint index, GLenum pname, GLint64* param )
    {
        GetDelegateForFunction< PFNGLGETVERTEXARRAYINDEXED64IVPROC >( "glGetVertexArrayIndexed64iv", out _glGetVertexArrayIndexed64iv );

        _glGetVertexArrayIndexed64iv( vaobj, index, pname, param );
    }

    public void GetVertexArrayIndexed64iv( GLuint vaobj, GLuint index, GLenum pname, ref GLint64[] param )
    {
        GetDelegateForFunction< PFNGLGETVERTEXARRAYINDEXED64IVPROC >( "glGetVertexArrayIndexed64iv", out _glGetVertexArrayIndexed64iv );

        fixed ( GLint64* ptrParam = &param[ 0 ] )
        {
            _glGetVertexArrayIndexed64iv( vaobj, index, pname, ptrParam );
        }
    }
}

// ============================================================================
// ============================================================================