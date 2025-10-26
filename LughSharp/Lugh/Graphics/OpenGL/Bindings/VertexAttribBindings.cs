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
using GLint = int;
using GLsizei = int;
using GLdouble = double;
using GLuint = uint;
using GLboolean = bool;
using GLubyte = byte;
using GLshort = short;
using GLbyte = sbyte;
using GLushort = ushort;

// ============================================================================

namespace LughSharp.Lugh.Graphics.OpenGL.Bindings;

public unsafe partial class GLBindings
{
    /// <inheritdoc />
    public void VertexAttribDivisor( GLuint index, GLuint divisor )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBDIVISORPROC >( "glVertexAttribDivisor", out _glVertexAttribDivisor );

        _glVertexAttribDivisor( index, divisor );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttribP1ui( GLuint index, GLenum type, GLboolean normalized, GLuint value )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBP1UIPROC >( "glVertexAttribP1ui", out _glVertexAttribP1ui );

        _glVertexAttribP1ui( index, type, normalized, value );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttribP1uiv( GLuint index, GLenum type, GLboolean normalized, GLuint* value )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBP1UIVPROC >( "glVertexAttribP1uiv", out _glVertexAttribP1uiv );

        _glVertexAttribP1uiv( index, type, normalized, value );
    }

    /// <inheritdoc />
    public void VertexAttribP1uiv( GLuint index, GLenum type, GLboolean normalized, GLuint[] value )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBP1UIVPROC >( "glVertexAttribP1uiv", out _glVertexAttribP1uiv );

        fixed ( GLuint* dp = &value[ 0 ] )
        {
            _glVertexAttribP1uiv( index, type, normalized, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttribP2ui( GLuint index, GLenum type, GLboolean normalized, GLuint value )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBP2UIPROC >( "glVertexAttribP2ui", out _glVertexAttribP2ui );

        _glVertexAttribP2ui( index, type, normalized, value );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttribP2uiv( GLuint index, GLenum type, GLboolean normalized, GLuint* value )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBP2UIVPROC >( "glVertexAttribP2uiv", out _glVertexAttribP2uiv );

        _glVertexAttribP2uiv( index, type, normalized, value );
    }

    /// <inheritdoc />
    public void VertexAttribP2uiv( GLuint index, GLenum type, GLboolean normalized, GLuint[] value )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBP2UIVPROC >( "glVertexAttribP2uiv", out _glVertexAttribP2uiv );

        fixed ( GLuint* dp = &value[ 0 ] )
        {
            _glVertexAttribP2uiv( index, type, normalized, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttribP3ui( GLuint index, GLenum type, GLboolean normalized, GLuint value )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBP3UIPROC >( "glVertexAttribP3ui", out _glVertexAttribP3ui );

        _glVertexAttribP3ui( index, type, normalized, value );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttribP3uiv( GLuint index, GLenum type, GLboolean normalized, GLuint* value )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBP3UIVPROC >( "glVertexAttribP3uiv", out _glVertexAttribP3uiv );

        _glVertexAttribP3uiv( index, type, normalized, value );
    }

    /// <inheritdoc />
    public void VertexAttribP3uiv( GLuint index, GLenum type, GLboolean normalized, GLuint[] value )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBP3UIVPROC >( "glVertexAttribP3uiv", out _glVertexAttribP3uiv );

        fixed ( GLuint* dp = &value[ 0 ] )
        {
            _glVertexAttribP3uiv( index, type, normalized, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttribP4ui( GLuint index, GLenum type, GLboolean normalized, GLuint value )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBP4UIPROC >( "glVertexAttribP4ui", out _glVertexAttribP4ui );

        _glVertexAttribP4ui( index, type, normalized, value );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttribP4uiv( GLuint index, GLenum type, GLboolean normalized, GLuint* value )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBP4UIVPROC >( "glVertexAttribP4uiv", out _glVertexAttribP4uiv );

        _glVertexAttribP4uiv( index, type, normalized, value );
    }

    /// <inheritdoc />
    public void VertexAttribP4uiv( GLuint index, GLenum type, GLboolean normalized, GLuint[] value )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBP4UIVPROC >( "glVertexAttribP4uiv", out _glVertexAttribP4uiv );

        fixed ( GLuint* dp = &value[ 0 ] )
        {
            _glVertexAttribP4uiv( index, type, normalized, dp );
        }
    }

    // ========================================================================

    public void VertexAttribL1d( GLuint index, GLdouble x )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBL1DPROC >( "glVertexAttribL1d", out _glVertexAttribL1d );

        _glVertexAttribL1d( index, x );
    }

    // ========================================================================

    public void VertexAttribL2d( GLuint index, GLdouble x, GLdouble y )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBL2DPROC >( "glVertexAttribL2d", out _glVertexAttribL2d );

        _glVertexAttribL2d( index, x, y );
    }

    // ========================================================================

    public void VertexAttribL3d( GLuint index, GLdouble x, GLdouble y, GLdouble z )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBL3DPROC >( "glVertexAttribL3d", out _glVertexAttribL3d );

        _glVertexAttribL3d( index, x, y, z );
    }

    // ========================================================================

    public void VertexAttribL4d( GLuint index, GLdouble x, GLdouble y, GLdouble z, GLdouble w )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBL4DPROC >( "glVertexAttribL4d", out _glVertexAttribL4d );

        _glVertexAttribL4d( index, x, y, z, w );
    }

    // ========================================================================

    public void VertexAttribL1dv( GLuint index, GLdouble* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBL1DVPROC >( "glVertexAttribL1dv", out _glVertexAttribL1dv );

        _glVertexAttribL1dv( index, v );
    }

    public void VertexAttribL1dv( GLuint index, GLdouble[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBL1DVPROC >( "glVertexAttribL1dv", out _glVertexAttribL1dv );

        fixed ( GLdouble* pV = &v[ 0 ] )
        {
            _glVertexAttribL1dv( index, pV );
        }
    }

    // ========================================================================

    public void VertexAttribL2dv( GLuint index, GLdouble* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBL2DVPROC >( "glVertexAttribL2dv", out _glVertexAttribL2dv );

        _glVertexAttribL2dv( index, v );
    }

    public void VertexAttribL2dv( GLuint index, GLdouble[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBL2DVPROC >( "glVertexAttribL2dv", out _glVertexAttribL2dv );

        fixed ( GLdouble* pV = &v[ 0 ] )
        {
            _glVertexAttribL2dv( index, pV );
        }
    }

    // ========================================================================

    public void VertexAttribL3dv( GLuint index, GLdouble* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBL3DVPROC >( "glVertexAttribL3dv", out _glVertexAttribL3dv );

        _glVertexAttribL3dv( index, v );
    }

    public void VertexAttribL3dv( GLuint index, GLdouble[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBL3DVPROC >( "glVertexAttribL3dv", out _glVertexAttribL3dv );

        fixed ( GLdouble* pV = &v[ 0 ] )
        {
            _glVertexAttribL3dv( index, pV );
        }
    }

    // ========================================================================

    public void VertexAttribL4dv( GLuint index, GLdouble* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBL4DVPROC >( "glVertexAttribL4dv", out _glVertexAttribL4dv );

        _glVertexAttribL4dv( index, v );
    }

    public void VertexAttribL4dv( GLuint index, GLdouble[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBL4DVPROC >( "glVertexAttribL4dv", out _glVertexAttribL4dv );

        fixed ( GLdouble* pV = &v[ 0 ] )
        {
            _glVertexAttribL4dv( index, pV );
        }
    }

    // ========================================================================

    public void VertexAttribLPointer( GLuint index, GLint size, GLenum type, GLsizei stride, IntPtr pointer )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBLPOINTERPROC >( "glVertexAttribLPointer", out _glVertexAttribLPointer );

        _glVertexAttribLPointer( index, size, type, stride, pointer );
    }

    public void VertexAttribLPointer( GLuint index, GLint size, GLenum type, GLsizei stride, GLsizei pointer )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBLPOINTERPROC >( "glVertexAttribLPointer", out _glVertexAttribLPointer );

        _glVertexAttribLPointer( index, size, type, stride, ( IntPtr )pointer );
    }

    // ========================================================================

    public void GetVertexAttribLdv( GLuint index, GLenum pname, GLdouble* parameters )
    {
        GetDelegateForFunction< PFNGLGETVERTEXATTRIBLDVPROC >( "glGetVertexAttribLdv", out _glGetVertexAttribLdv );

        _glGetVertexAttribLdv( index, pname, parameters );
    }

    public void GetVertexAttribLdv( GLuint index, GLenum pname, ref GLdouble[] parameters )
    {
        GetDelegateForFunction< PFNGLGETVERTEXATTRIBLDVPROC >( "glGetVertexAttribLdv", out _glGetVertexAttribLdv );

        fixed ( GLdouble* pP = &parameters[ 0 ] )
        {
            _glGetVertexAttribLdv( index, pname, pP );
        }
    }

    // ========================================================================

    public void VertexAttribFormat( GLuint attribindex, GLint size, GLenum type, GLboolean normalized, GLuint relativeoffset )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBFORMATPROC >( "glVertexAttribFormat", out _glVertexAttribFormat );

        _glVertexAttribFormat( attribindex, size, type, normalized, relativeoffset );
    }

    // ========================================================================

    public void VertexAttribIFormat( GLuint attribindex, GLint size, GLenum type, GLuint relativeoffset )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBIFORMATPROC >( "glVertexAttribIFormat", out _glVertexAttribIFormat );

        _glVertexAttribIFormat( attribindex, size, type, relativeoffset );
    }

    // ========================================================================

    public void VertexAttribLFormat( GLuint attribindex, GLint size, GLenum type, GLuint relativeoffset )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBLFORMATPROC >( "glVertexAttribLFormat", out _glVertexAttribLFormat );

        _glVertexAttribLFormat( attribindex, size, type, relativeoffset );
    }

    // ========================================================================

    public void VertexAttribBinding( GLuint attribindex, GLuint bindingindex )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBBINDINGPROC >( "glVertexAttribBinding", out _glVertexAttribBinding );

        _glVertexAttribBinding( attribindex, bindingindex );
    }

    // ========================================================================

    public void VertexBindingDivisor( GLuint bindingindex, GLuint divisor )
    {
        GetDelegateForFunction< PFNGLVERTEXBINDINGDIVISORPROC >( "glVertexBindingDivisor", out _glVertexBindingDivisor );

        _glVertexBindingDivisor( bindingindex, divisor );
    }

    // ========================================================================

    public void VertexAttribIPointer( GLuint index, GLint size, GLenum type, GLsizei stride, IntPtr pointer )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBIPOINTERPROC >( "glVertexAttribIPointer", out _glVertexAttribIPointer );

        _glVertexAttribIPointer( index, size, type, stride, pointer );
    }

    // ========================================================================

    public void VertexAttribIPointer( GLuint index, GLint size, GLenum type, GLsizei stride, uint pointer )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBIPOINTERPROC >( "glVertexAttribIPointer", out _glVertexAttribIPointer );

        _glVertexAttribIPointer( index, size, type, stride, ( IntPtr )pointer );
    }

    // ========================================================================

    public void GetVertexAttribIiv( GLuint index, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETVERTEXATTRIBIIVPROC >( "glGetVertexAttribIiv", out _glGetVertexAttribIiv );

        _glGetVertexAttribIiv( index, pname, parameters );
    }

    // ========================================================================

    public void GetVertexAttribIiv( GLuint index, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETVERTEXATTRIBIIVPROC >( "glGetVertexAttribIiv", out _glGetVertexAttribIiv );

        fixed ( GLint* p = &parameters[ 0 ] )
        {
            _glGetVertexAttribIiv( index, pname, p );
        }
    }

    // ========================================================================

    public void GetVertexAttribIuiv( GLuint index, GLenum pname, GLuint* parameters )
    {
        GetDelegateForFunction< PFNGLGETVERTEXATTRIBIUIVPROC >( "glGetVertexAttribIuiv", out _glGetVertexAttribIuiv );

        _glGetVertexAttribIuiv( index, pname, parameters );
    }

    // ========================================================================

    public void GetVertexAttribIuiv( GLuint index, GLenum pname, ref GLuint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETVERTEXATTRIBIUIVPROC >( "glGetVertexAttribIuiv", out _glGetVertexAttribIuiv );

        fixed ( GLuint* p = &parameters[ 0 ] )
        {
            _glGetVertexAttribIuiv( index, pname, p );
        }
    }

    // ========================================================================

    public void VertexAttribI1i( GLuint index, GLint x )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI1IPROC >( "glVertexAttribI1i", out _glVertexAttribI1i );

        _glVertexAttribI1i( index, x );
    }

    // ========================================================================

    public void VertexAttribI2i( GLuint index, GLint x, GLint y )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI2IPROC >( "glVertexAttribI2i", out _glVertexAttribI2i );

        _glVertexAttribI2i( index, x, y );
    }

    // ========================================================================

    public void VertexAttribI3i( GLuint index, GLint x, GLint y, GLint z )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI3IPROC >( "glVertexAttribI3i", out _glVertexAttribI3i );

        _glVertexAttribI3i( index, x, y, z );
    }

    // ========================================================================

    public void VertexAttribI4i( GLuint index, GLint x, GLint y, GLint z, GLint w )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4IPROC >( "glVertexAttribI4i", out _glVertexAttribI4i );

        _glVertexAttribI4i( index, x, y, z, w );
    }

    // ========================================================================

    public void VertexAttribI1ui( GLuint index, GLuint x )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI1UIPROC >( "glVertexAttribI1ui", out _glVertexAttribI1ui );

        _glVertexAttribI1ui( index, x );
    }

    // ========================================================================

    public void VertexAttribI2ui( GLuint index, GLuint x, GLuint y )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI2UIPROC >( "glVertexAttribI2ui", out _glVertexAttribI2ui );

        _glVertexAttribI2ui( index, x, y );
    }

    // ========================================================================

    public void VertexAttribI3ui( GLuint index, GLuint x, GLuint y, GLuint z )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI3UIPROC >( "glVertexAttribI3ui", out _glVertexAttribI3ui );

        _glVertexAttribI3ui( index, x, y, z );
    }

    // ========================================================================

    public void VertexAttribI4ui( GLuint index, GLuint x, GLuint y, GLuint z, GLuint w )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4UIPROC >( "glVertexAttribI4ui", out _glVertexAttribI4ui );

        _glVertexAttribI4ui( index, x, y, z, w );
    }

    // ========================================================================

    public void VertexAttribI1iv( GLuint index, GLint* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI1IVPROC >( "glVertexAttribI1iv", out _glVertexAttribI1iv );

        _glVertexAttribI1iv( index, v );
    }

    public void VertexAttribI1iv( GLuint index, GLint[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI1IVPROC >( "glVertexAttribI1iv", out _glVertexAttribI1iv );

        fixed ( GLint* p = &v[ 0 ] )
        {
            _glVertexAttribI1iv( index, p );
        }
    }

    // ========================================================================

    public void VertexAttribI2iv( GLuint index, GLint* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI2IVPROC >( "glVertexAttribI2iv", out _glVertexAttribI2iv );

        _glVertexAttribI2iv( index, v );
    }

    public void VertexAttribI2iv( GLuint index, GLint[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI2IVPROC >( "glVertexAttribI2iv", out _glVertexAttribI2iv );

        fixed ( GLint* p = &v[ 0 ] )
        {
            _glVertexAttribI2iv( index, p );
        }
    }

    // ========================================================================

    public void VertexAttribI3iv( GLuint index, GLint* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI3IVPROC >( "glVertexAttribI3iv", out _glVertexAttribI3iv );

        _glVertexAttribI3iv( index, v );
    }

    public void VertexAttribI3iv( GLuint index, GLint[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI3IVPROC >( "glVertexAttribI3iv", out _glVertexAttribI3iv );

        fixed ( GLint* p = &v[ 0 ] )
        {
            _glVertexAttribI3iv( index, p );
        }
    }

    // ========================================================================

    public void VertexAttribI4iv( GLuint index, GLint* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4IVPROC >( "glVertexAttribI4iv", out _glVertexAttribI4iv );

        _glVertexAttribI4iv( index, v );
    }

    public void VertexAttribI4iv( GLuint index, GLint[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4IVPROC >( "glVertexAttribI4iv", out _glVertexAttribI4iv );

        fixed ( GLint* p = &v[ 0 ] )
        {
            _glVertexAttribI4iv( index, p );
        }
    }

    // ========================================================================

    public void VertexAttribI1uiv( GLuint index, GLuint* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI1UIVPROC >( "glVertexAttribI1uiv", out _glVertexAttribI1uiv );

        _glVertexAttribI1uiv( index, v );
    }

    public void VertexAttribI1uiv( GLuint index, GLuint[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI1UIVPROC >( "glVertexAttribI1uiv", out _glVertexAttribI1uiv );

        fixed ( GLuint* p = &v[ 0 ] )
        {
            _glVertexAttribI1uiv( index, p );
        }
    }

    // ========================================================================

    public void VertexAttribI2uiv( GLuint index, GLuint* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI2UIVPROC >( "glVertexAttribI2uiv", out _glVertexAttribI2uiv );

        _glVertexAttribI2uiv( index, v );
    }

    public void VertexAttribI2uiv( GLuint index, GLuint[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI2UIVPROC >( "glVertexAttribI2uiv", out _glVertexAttribI2uiv );

        fixed ( GLuint* p = &v[ 0 ] )
        {
            _glVertexAttribI2uiv( index, p );
        }
    }

    // ========================================================================

    public void VertexAttribI3uiv( GLuint index, GLuint* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI3UIVPROC >( "glVertexAttribI3uiv", out _glVertexAttribI3uiv );

        _glVertexAttribI3uiv( index, v );
    }

    public void VertexAttribI3uiv( GLuint index, GLuint[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI3UIVPROC >( "glVertexAttribI3uiv", out _glVertexAttribI3uiv );

        fixed ( GLuint* p = &v[ 0 ] )
        {
            _glVertexAttribI3uiv( index, p );
        }
    }

    // ========================================================================

    public void VertexAttribI4uiv( GLuint index, GLuint* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4UIVPROC >( "glVertexAttribI4uiv", out _glVertexAttribI4uiv );

        _glVertexAttribI4uiv( index, v );
    }

    public void VertexAttribI4uiv( GLuint index, GLuint[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4UIVPROC >( "glVertexAttribI4uiv", out _glVertexAttribI4uiv );

        fixed ( GLuint* p = &v[ 0 ] )
        {
            _glVertexAttribI4uiv( index, p );
        }
    }

    // ========================================================================

    public void VertexAttribI4bv( GLuint index, GLbyte* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4BVPROC >( "glVertexAttribI4bv", out _glVertexAttribI4bv );

        _glVertexAttribI4bv( index, v );
    }

    public void VertexAttribI4bv( GLuint index, GLbyte[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4BVPROC >( "glVertexAttribI4bv", out _glVertexAttribI4bv );

        fixed ( GLbyte* p = &v[ 0 ] )
        {
            _glVertexAttribI4bv( index, p );
        }
    }

    // ========================================================================

    public void VertexAttribI4sv( GLuint index, GLshort* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4SVPROC >( "glVertexAttribI4sv", out _glVertexAttribI4sv );

        _glVertexAttribI4sv( index, v );
    }

    public void VertexAttribI4sv( GLuint index, GLshort[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4SVPROC >( "glVertexAttribI4sv", out _glVertexAttribI4sv );

        fixed ( GLshort* p = &v[ 0 ] )
        {
            _glVertexAttribI4sv( index, p );
        }
    }

    // ========================================================================

    public void VertexAttribI4ubv( GLuint index, GLubyte* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4UBVPROC >( "glVertexAttribI4ubv", out _glVertexAttribI4ubv );

        _glVertexAttribI4ubv( index, v );
    }

    public void VertexAttribI4ubv( GLuint index, GLubyte[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4UBVPROC >( "glVertexAttribI4ubv", out _glVertexAttribI4ubv );

        fixed ( GLubyte* p = &v[ 0 ] )
        {
            _glVertexAttribI4ubv( index, p );
        }
    }

    // ========================================================================

    public void VertexAttribI4usv( GLuint index, GLushort* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4USVPROC >( "glVertexAttribI4usv", out _glVertexAttribI4usv );

        _glVertexAttribI4usv( index, v );
    }

    public void VertexAttribI4usv( GLuint index, GLushort[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4USVPROC >( "glVertexAttribI4usv", out _glVertexAttribI4usv );

        fixed ( GLushort* p = &v[ 0 ] )
        {
            _glVertexAttribI4usv( index, p );
        }
    }
}

// ========================================================================
// ========================================================================