// ///////////////////////////////////////////////////////////////////////////////
// MIT License
// 
// Copyright (c) 2023 dcronqvist (Daniel Cronqvist <daniel@dcronqvist.se>)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ///////////////////////////////////////////////////////////////////////////////

using System.Numerics;

using LughSharp.Source.Graphics.OpenGL.Enums;
using LughSharp.Source.Graphics.Images;

using GLenum = int;
using GLfloat = float;
using GLint = int;
using GLsizei = int;
using GLbitfield = uint;
using GLdouble = double;
using GLuint = uint;
using GLbool = bool;
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

namespace LughSharp.Source.Graphics.OpenGL.Bindings;

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
[PublicAPI]
public unsafe partial class GLBindings : IGLBindings
{
    public const int InvalidShaderProgram = -1;
    public const int InvalidShader        = -1;

    // ========================================================================

    /// <summary>
    /// The null pointer, just like in C/C++.
    /// </summary>
    public const IntPtr Null = 0;

    // ========================================================================
    // ========================================================================

    /// <inheritdoc />
    public void ColorMaski( GLuint index, GLbool r, GLbool g, GLbool b, GLbool a )
    {
        GetDelegateForFunction< PFNGLCOLORMASKIPROC >( "glColorMaski", out _glColorMaski );

        _glColorMaski( index, r, g, b, a );
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetBooleani_v( GLenum target, GLuint index, GLbool* data )
    {
        GetDelegateForFunction< PFNGLGETBOOLEANI_VPROC >( "glGetBooleani_v", out _glGetBooleani_v );

        _glGetBooleani_v( target, index, data );
    }

    /// <inheritdoc />
    public void GetBooleani_v( GLenum target, GLuint index, ref GLbool[] data )
    {
        GetDelegateForFunction< PFNGLGETBOOLEANI_VPROC >( "glGetBooleani_v", out _glGetBooleani_v );

        fixed ( GLbool* p = &data[ 0 ] )
        {
            _glGetBooleani_v( target, index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetIntegeri_v( GLenum target, GLuint index, GLint* data )
    {
        GetDelegateForFunction< PFNGLGETINTEGERI_VPROC >( "glGetIntegeri_v", out _glGetIntegeri_v );

        _glGetIntegeri_v( target, index, data );
    }

    /// <inheritdoc />
    public void GetIntegeri_v( GLenum target, GLuint index, ref GLint[] data )
    {
        GetDelegateForFunction< PFNGLGETINTEGERI_VPROC >( "glGetIntegeri_v", out _glGetIntegeri_v );

        fixed ( GLint* p = &data[ 0 ] )
        {
            _glGetIntegeri_v( target, index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetIntegerv( GLenum pname, GLint* data )
    {
        GetDelegateForFunction< PFNGLGETINTEGERVPROC >( "glGetIntegerv", out _glGetIntegerv );

        _glGetIntegerv( pname, data );
    }

    /// <inheritdoc />
    public void GetIntegerv( GLenum pname, ref GLint[] data )
    {
        GetDelegateForFunction< PFNGLGETINTEGERVPROC >( "glGetIntegerv", out _glGetIntegerv );

        fixed ( GLint* p = &data[ 0 ] )
        {
            _glGetIntegerv( pname, p );
        }
    }

    /// <inheritdoc />
    public void GetIntegerv( GLenum pname, out int data )
    {
        // Create a temporary array to hold the result from the underlying OpenGL method.
        var tempArray = new int[ 1 ];

        // Call the original method with the temporary array.
        GetIntegerv( pname, ref tempArray );

        // Assign the value from the temporary array to the out parameter.
        data = tempArray[ 0 ];
    }

    // ========================================================================

    /// <inheritdoc />
    public (int major, int minor) GetOpenGLVersion()
    {
        GLubyte* version = GetString( IGL.GLVersion );

        return version == null
                   ? throw new LughRuntimeException( "NULL GL Version returned!" )
                   : ( version[ 0 ], version[ 2 ] );
    }

    // ========================================================================
    // ========================================================================

    // ========================================================================
    // ========================================================================

    /// <inheritdoc />
    public void Hint( GLenum target, GLenum mode )
    {
        GetDelegateForFunction< PFNGLHINTPROC >( "glHint", out _glHint );

        _glHint( target, mode );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Clear( GLbitfield mask )
    {
        GetDelegateForFunction< PFNGLCLEARPROC >( "glClear", out _glClear );

        _glClear( mask );
    }

    // ========================================================================

    /// <inheritdoc />
    public void ClearColor( GLfloat red, GLfloat green, GLfloat blue, GLfloat alpha )
    {
        GetDelegateForFunction< PFNGLCLEARCOLORPROC >( "glClearColor", out _glClearColor );

        _glClearColor( red, green, blue, alpha );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Disable( EnableCap cap )
    {
        GetDelegateForFunction< PFNGLDISABLEPROC >( "glDisable", out _glDisable );

        _glDisable( ( GLenum )cap );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Enable( EnableCap cap )
    {
        GetDelegateForFunction< PFNGLENABLEPROC >( "glEnable", out _glEnable );

        _glEnable( ( GLenum )cap );
    }

    // ========================================================================

    /// <inheritdoc />
    public void EnableOrDisable( EnableCap cap, GLbool enable )
    {
        if ( enable )
        {
            Enable( cap );
        }
        else
        {
            Disable( cap );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void LogicOp( GLenum opcode )
    {
        GetDelegateForFunction< PFNGLLOGICOPPROC >( "glLogicOp", out _glLogicOp );

        _glLogicOp( opcode );
    }

    // ========================================================================

    /// <inheritdoc />
    public GLbool IsEnabled( EnableCap cap )
    {
        GetDelegateForFunction< PFNGLISENABLEDPROC >( "glIsEnabled", out _glIsEnabled );

        return _glIsEnabled( ( GLenum )cap );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Enablei( GLenum target, GLuint index )
    {
        GetDelegateForFunction< PFNGLENABLEIPROC >( "glEnablei", out _glEnablei );

        _glEnablei( target, index );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Disablei( GLenum target, GLuint index )
    {
        GetDelegateForFunction< PFNGLDISABLEIPROC >( "glDisablei", out _glDisablei );

        _glDisablei( target, index );
    }

    // ========================================================================

    /// <inheritdoc />
    public GLbool IsEnabledi( GLenum target, GLuint index )
    {
        GetDelegateForFunction< PFNGLISENABLEDIPROC >( "glIsEnabledi", out _glIsEnabledi );

        return _glIsEnabledi( target, index );
    }

    // ========================================================================

    /// <inheritdoc />
    public void CullFace( CullFaceMode mode )
    {
        GetDelegateForFunction< PFNGLCULLFACEPROC >( "glCullFace", out _glCullFace );

        _glCullFace( ( int )mode );
    }

    // ========================================================================

    /// <inheritdoc />
    public void FrontFace( FrontFaceDirection mode )
    {
        GetDelegateForFunction< PFNGLFRONTFACEPROC >( "glFrontFace", out _glFrontFace );

        _glFrontFace( ( int )mode );
    }

    // ========================================================================

    /// <inheritdoc />
    public void LineWidth( GLfloat width )
    {
        GetDelegateForFunction< PFNGLLINEWIDTHPROC >( "glLineWidth", out _glLineWidth );

        _glLineWidth( width );
    }

    // ========================================================================

    /// <inheritdoc />
    public void PointSize( GLfloat size )
    {
        GetDelegateForFunction< PFNGLPOINTSIZEPROC >( "glPointSize", out _glPointSize );

        _glPointSize( size );
    }

    // ========================================================================

    /// <inheritdoc />
    public void PolygonMode( GLenum face, GLenum mode )
    {
        GetDelegateForFunction< PFNGLPOLYGONMODEPROC >( "glPolygonMode", out _glPolygonMode );

        _glPolygonMode( face, mode );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Scissor( GLint x, GLint y, GLsizei width, GLsizei height )
    {
        GetDelegateForFunction< PFNGLSCISSORPROC >( "glScissor", out _glScissor );

        _glScissor( x, y, width, height );
    }

    // ========================================================================

    /// <inheritdoc />
    public void ClearStencil( GLint s )
    {
        GetDelegateForFunction< PFNGLCLEARSTENCILPROC >( "glClearStencil", out _glClearStencil );

        _glClearStencil( s );
    }

    // ========================================================================

    /// <inheritdoc />
    public void ClearDepth( GLdouble depth )
    {
        GetDelegateForFunction< PFNGLCLEARDEPTHPROC >( "glClearDepth", out _glClearDepth );

        _glClearDepth( depth );
    }

    // ========================================================================

    /// <inheritdoc />
    public void StencilMask( GLuint mask )
    {
        GetDelegateForFunction< PFNGLSTENCILMASKPROC >( "glStencilMask", out _glStencilMask );

        _glStencilMask( mask );
    }

    // ========================================================================

    /// <inheritdoc />
    public void ColorMask( GLbool red, GLbool green, GLbool blue, GLbool alpha )
    {
        GetDelegateForFunction< PFNGLCOLORMASKPROC >( "glColorMask", out _glColorMask );

        _glColorMask( red, green, blue, alpha );
    }

    // ========================================================================

    /// <inheritdoc />
    public void DepthMask( GLbool flag )
    {
        GetDelegateForFunction< PFNGLDEPTHMASKPROC >( "glDepthMask", out _glDepthMask );

        _glDepthMask( flag );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Finish()
    {
        GetDelegateForFunction< PFNGLFINISHPROC >( "glFinish", out _glFinish );

        _glFinish();
    }

    // ========================================================================

    /// <inheritdoc />
    public void Flush()
    {
        GetDelegateForFunction< PFNGLFLUSHPROC >( "glFlush", out _glFlush );

        _glFlush();
    }

    // ========================================================================

    /// <inheritdoc />
    public void BlendFunc( GLenum sfactor, GLenum dfactor )
    {
        GetDelegateForFunction< PFNGLBLENDFUNCPROC >( "glBlendFunc", out _glBlendFunc );

        _glBlendFunc( sfactor, dfactor );
    }

    // ========================================================================

    /// <inheritdoc />
    public void StencilFunc( GLenum func, GLint @ref, GLuint mask )
    {
        GetDelegateForFunction< PFNGLSTENCILFUNCPROC >( "glStencilFunc", out _glStencilFunc );

        _glStencilFunc( func, @ref, mask );
    }

    // ========================================================================

    /// <inheritdoc />
    public void StencilOp( GLenum fail, GLenum zfail, GLenum zpass )
    {
        GetDelegateForFunction< PFNGLSTENCILOPPROC >( "glStencilOp", out _glStencilOp );

        _glStencilOp( fail, zfail, zpass );
    }

    // ========================================================================

    /// <inheritdoc />
    public void DepthFunc( GLenum func )
    {
        GetDelegateForFunction< PFNGLDEPTHFUNCPROC >( "glDepthFunc", out _glDepthFunc );

        _glDepthFunc( func );
    }

    // ========================================================================

    /// <inheritdoc />
    public void PixelStoref( GLenum pname, GLfloat param )
    {
        GetDelegateForFunction< PFNGLPIXELSTOREFPROC >( "glPixelStoref", out _glPixelStoref );

        _glPixelStoref( pname, param );
    }

    // ========================================================================

    /// <inheritdoc />
    public void PixelStorei( GLenum pname, GLint param )
    {
        GetDelegateForFunction< PFNGLPIXELSTOREIPROC >( "glPixelStorei", out _glPixelStorei );

        _glPixelStorei( pname, param );
    }

    /// <inheritdoc />
    public void PixelStorei( PixelStoreParameter pname, GLint param )
    {
        PixelStorei( ( int )pname, param );
    }

    // ========================================================================

    /// <inheritdoc />
    public void ReadPixels( GLint x, GLint y, GLsizei width, GLsizei height, GLenum format, GLenum type, IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLREADPIXELSPROC >( "glReadPixels", out _glReadPixels );

        _glReadPixels( x, y, width, height, format, type, pixels );
    }

    /// <inheritdoc />
    public void ReadPixels< T >( GLint x, GLint y, GLsizei width, GLsizei height, GLenum format, GLenum type,
                                 ref T[] pixels )
        where T : unmanaged
    {
        GetDelegateForFunction< PFNGLREADPIXELSPROC >( "glReadPixels", out _glReadPixels );

        fixed ( void* p = &pixels[ 0 ] )
        {
            _glReadPixels( x, y, width, height, format, type, ( IntPtr )p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetBooleanv( GLenum pname, GLbool* data )
    {
        GetDelegateForFunction< PFNGLGETBOOLEANVPROC >( "glGetBooleanv", out _glGetBooleanv );

        _glGetBooleanv( pname, data );
    }

    /// <inheritdoc />
    public void GetBooleanv( GLenum pname, ref GLbool[] data )
    {
        GetDelegateForFunction< PFNGLGETBOOLEANVPROC >( "glGetBooleanv", out _glGetBooleanv );

        fixed ( GLbool* p = &data[ 0 ] )
        {
            _glGetBooleanv( pname, p );
        }
    }

    /// <inheritdoc />
    public void GetBooleanv( GLenum pname, out GLbool data )
    {
        // Create a temporary array to hold the result from the underlying OpenGL method.
        var tempArray = new GLbool[ 1 ];

        // Call the original method with the temporary array.
        GetBooleanv( pname, ref tempArray );

        // Assign the value from the temporary array to the out parameter.
        data = tempArray[ 0 ];
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetDoublev( GLenum pname, GLdouble* data )
    {
        GetDelegateForFunction< PFNGLGETDOUBLEVPROC >( "glGetDoublev", out _glGetDoublev );

        _glGetDoublev( pname, data );
    }

    /// <inheritdoc />
    public void GetDoublev( GLenum pname, ref GLdouble[] data )
    {
        GetDelegateForFunction< PFNGLGETDOUBLEVPROC >( "glGetDoublev", out _glGetDoublev );

        fixed ( GLdouble* p = &data[ 0 ] )
        {
            _glGetDoublev( pname, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetFloatv( GLenum pname, float* data )
    {
        GetDelegateForFunction< PFNGLGETFLOATVPROC >( "glGetFloatv", out _glGetFloatv );

        _glGetFloatv( pname, data );
    }

    /// <inheritdoc />
    public void GetFloatv( GLenum pname, ref GLfloat[] data )
    {
        GetDelegateForFunction< PFNGLGETFLOATVPROC >( "glGetFloatv", out _glGetFloatv );

        fixed ( GLfloat* p = &data[ 0 ] )
        {
            _glGetFloatv( pname, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public GLubyte* GetString( GLenum name )
    {
        GetDelegateForFunction< PFNGLGETSTRINGPROC >( "glGetString", out _glGetString );

        return _glGetString( name );
    }

    // ========================================================================

    /// <inheritdoc />
    public string GetStringSafe( GLenum name )
    {
        GetDelegateForFunction< PFNGLGETSTRINGPROC >( "glGetString", out _glGetString );

        byte* p = _glGetString( name );

        if ( p == null )
        {
            return string.Empty;
        }

        var i = 0;

        while ( p[ i ] != 0 )
        {
            i++;
        }

        return new string( ( GLbyte* )p, 0, i, Encoding.UTF8 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void DepthRange( GLdouble near, GLdouble far )
    {
        GetDelegateForFunction< PFNGLDEPTHRANGEPROC >( "glDepthRange", out _glDepthRange );

        _glDepthRange( near, far );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Viewport( GLint x, GLint y, GLsizei width, GLsizei height )
    {
        GetDelegateForFunction< PFNGLVIEWPORTPROC >( "glViewport", out _glViewport );

        _glViewport( x, y, width, height );
    }

    // ========================================================================

    /// <inheritdoc />
    public void DrawArrays( GLenum mode, GLint first, GLsizei count )
    {
        GetDelegateForFunction< PFNGLDRAWARRAYSPROC >( "glDrawArrays", out _glDrawArrays );

        _glDrawArrays( mode, first, count );
    }

    // ========================================================================

    /// <inheritdoc />
    public void DrawElements( GLenum mode, GLsizei count, GLenum type, IntPtr indices )
    {
        GetDelegateForFunction< PFNGLDRAWELEMENTSPROC >( "glDrawElements", out _glDrawElements );

        _glDrawElements( mode, count, type, indices );
    }

    /// <inheritdoc />
    public void DrawElements< T >( GLenum mode, GLsizei count, GLenum type, T[] indices )
        where T : unmanaged, IUnsignedNumber< T >
    {
        GetDelegateForFunction< PFNGLDRAWELEMENTSPROC >( "glDrawElements", out _glDrawElements );

        {
            fixed ( T* p = &indices[ 0 ] )
            {
                _glDrawElements( mode, count, type, ( IntPtr )p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void PolygonOffset( GLfloat factor, GLfloat units )
    {
        GetDelegateForFunction< PFNGLPOLYGONOFFSETPROC >( "glPolygonOffset", out _glPolygonOffset );

        _glPolygonOffset( factor, units );
    }

    // ========================================================================

    /// <inheritdoc />
    public void DrawRangeElements( GLenum mode, GLuint start, GLuint end, GLsizei count, GLenum type, IntPtr indices )
    {
        GetDelegateForFunction< PFNGLDRAWRANGEELEMENTSPROC >( "glDrawRangeElements", out _glDrawRangeElements );

        _glDrawRangeElements( mode, start, end, count, type, indices );
    }

    /// <inheritdoc />
    public void DrawRangeElements< T >( GLenum mode, GLuint start, GLuint end, GLsizei count, GLenum type, T[] indices )
        where T : unmanaged, IUnsignedNumber< T >
    {
        GetDelegateForFunction< PFNGLDRAWRANGEELEMENTSPROC >( "glDrawRangeElements", out _glDrawRangeElements );

        {
            fixed ( T* pIndices = &indices[ 0 ] )
            {
                _glDrawRangeElements( mode, start, end, count, type, ( IntPtr )pIndices );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void BlendFuncSeparate( BlendMode sfactorRGB, BlendMode dfactorRGB, BlendMode sfactorAlpha,
                                   BlendMode dfactorAlpha )
    {
        GetDelegateForFunction< PFNGLBLENDFUNCSEPARATEPROC >( "glBlendFuncSeparate", out _glBlendFuncSeparate );

        _glBlendFuncSeparate
            (
             ( GLenum )sfactorRGB,
             ( GLenum )dfactorRGB,
             ( GLenum )sfactorAlpha,
             ( GLenum )dfactorAlpha
            );
    }

    // ========================================================================

    /// <inheritdoc />
    public void MultiDrawArrays( GLenum mode, GLint* first, GLsizei* count, GLsizei drawcount )
    {
        GetDelegateForFunction< PFNGLMULTIDRAWARRAYSPROC >( "glMultiDrawArrays", out _glMultiDrawArrays );

        _glMultiDrawArrays( mode, first, count, drawcount );
    }

    /// <inheritdoc />
    public void MultiDrawArrays( GLenum mode, GLint[] first, GLsizei[] count )
    {
        if ( first.Length != count.Length )
        {
            throw new ArgumentException( "first and count arrays must be of the same length" );
        }

        GetDelegateForFunction< PFNGLMULTIDRAWARRAYSPROC >( "glMultiDrawArrays", out _glMultiDrawArrays );

        {
            fixed ( GLint* p1 = &first[ 0 ] )
            {
                fixed ( GLsizei* p2 = &count[ 0 ] )
                {
                    _glMultiDrawArrays( mode, p1, p2, count.Length );
                }
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void MultiDrawElements( GLenum mode, GLsizei* count, GLenum type, IntPtr* indices, GLsizei drawcount )
    {
        GetDelegateForFunction< PFNGLMULTIDRAWELEMENTSPROC >( "glMultiDrawElements", out _glMultiDrawElements );

        _glMultiDrawElements( mode, count, type, indices, drawcount );
    }

    /// <inheritdoc />
    public void MultiDrawElements< T >( GLenum mode, GLsizei[] count, GLenum type, T[][] indices )
        where T : unmanaged, IUnsignedNumber< T >
    {
        var indexPtrs = new void*[ indices.Length ];

        for ( var i = 0; i < indices.Length; i++ )
        {
            fixed ( void* p = &indices[ i ][ 0 ] )
            {
                indexPtrs[ i ] = p;
            }
        }

        GetDelegateForFunction< PFNGLMULTIDRAWELEMENTSPROC >( "glMultiDrawElements", out _glMultiDrawElements );

        fixed ( GLsizei* c = &count[ 0 ] )
        {
            fixed ( void** p = &indexPtrs[ 0 ] )
            {
                _glMultiDrawElements( mode, c, type, ( IntPtr* )p, count.Length );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void PointParameterf( GLenum pname, GLfloat param )
    {
        GetDelegateForFunction< PFNGLPOINTPARAMETERFPROC >( "glPointParameterf", out _glPointParameterf );

        _glPointParameterf( pname, param );
    }

    // ========================================================================

    /// <inheritdoc />
    public void PointParameterfv( GLenum pname, GLfloat* parameters )
    {
        GetDelegateForFunction< PFNGLPOINTPARAMETERFVPROC >( "glPointParameterfv", out _glPointParameterfv );

        _glPointParameterfv( pname, parameters );
    }

    /// <inheritdoc />
    public void PointParameterfv( GLenum pname, GLfloat[] parameters )
    {
        GetDelegateForFunction< PFNGLPOINTPARAMETERFVPROC >( "glPointParameterfv", out _glPointParameterfv );

        {
            fixed ( GLfloat* p = &parameters[ 0 ] )
            {
                _glPointParameterfv( pname, p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void PointParameteri( GLenum pname, GLint param )
    {
        GetDelegateForFunction< PFNGLPOINTPARAMETERIPROC >( "glPointParameteri", out _glPointParameteri );

        _glPointParameteri( pname, param );
    }

    // ========================================================================

    /// <inheritdoc />
    public void PointParameteriv( GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLPOINTPARAMETERIVPROC >( "glPointParameteriv", out _glPointParameteriv );

        _glPointParameteriv( pname, parameters );
    }

    /// <inheritdoc />
    public void PointParameteriv( GLenum pname, GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLPOINTPARAMETERIVPROC >( "glPointParameteriv", out _glPointParameteriv );

        {
            fixed ( GLint* p = &parameters[ 0 ] )
            {
                _glPointParameteriv( pname, p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void BlendColor( GLfloat red, GLfloat green, GLfloat blue, GLfloat alpha )
    {
        GetDelegateForFunction< PFNGLBLENDCOLORPROC >( "glBlendColor", out _glBlendColor );

        _glBlendColor( red, green, blue, alpha );
    }

    // ========================================================================

    /// <inheritdoc />
    public void BlendEquation( GLenum mode )
    {
        GetDelegateForFunction< PFNGLBLENDEQUATIONPROC >( "glBlendEquation", out _glBlendEquation );

        _glBlendEquation( mode );
    }

    // ========================================================================

    /// <inheritdoc />
    public void BlendEquationSeparate( GLenum modeRGB, GLenum modeAlpha )
    {
        GetDelegateForFunction< PFNGLBLENDEQUATIONSEPARATEPROC >
            (
             "glBlendEquationSeparate",
             out _glBlendEquationSeparate
            );

        _glBlendEquationSeparate( modeRGB, modeAlpha );
    }

    // ========================================================================

    /// <inheritdoc />
    public void StencilOpSeparate( GLenum face, GLenum sfail, GLenum dpfail, GLenum dppass )
    {
        GetDelegateForFunction< PFNGLSTENCILOPSEPARATEPROC >( "glStencilOpSeparate", out _glStencilOpSeparate );

        _glStencilOpSeparate( face, sfail, dpfail, dppass );
    }

    // ========================================================================

    /// <inheritdoc />
    public void StencilFuncSeparate( GLenum face, GLenum func, GLint @ref, GLuint mask )
    {
        GetDelegateForFunction< PFNGLSTENCILFUNCSEPARATEPROC >( "glStencilFuncSeparate", out _glStencilFuncSeparate );

        _glStencilFuncSeparate( face, func, @ref, mask );
    }

    // ========================================================================

    /// <inheritdoc />
    public void StencilMaskSeparate( GLenum face, GLuint mask )
    {
        GetDelegateForFunction< PFNGLSTENCILMASKSEPARATEPROC >( "glStencilMaskSeparate", out _glStencilMaskSeparate );

        _glStencilMaskSeparate( face, mask );
    }

    // ========================================================================

    /// <inheritdoc />
    public void WaitSync( IntPtr sync, GLbitfield flags, GLuint64 timeout )
    {
        GetDelegateForFunction< PFNGLWAITSYNCPROC >( "glWaitSync", out _glWaitSync );

        _glWaitSync( sync, flags, timeout );
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetInteger64V( GLenum pname, GLint64* data )
    {
        GetDelegateForFunction< PFNGLGETINTEGER64VPROC >( "glGetInteger64v", out _glGetInteger64v );

        _glGetInteger64v( pname, data );
    }

    /// <inheritdoc />
    public void GetInteger64V( GLenum pname, ref GLint64[] data )
    {
        GetDelegateForFunction< PFNGLGETINTEGER64VPROC >( "glGetInteger64v", out _glGetInteger64v );

        {
            fixed ( GLint64* dp = &data[ 0 ] )
            {
                _glGetInteger64v( pname, dp );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetSynciv( IntPtr sync, GLenum pname, GLsizei bufSize, GLsizei* length, GLint* values )
    {
        GetDelegateForFunction< PFNGLGETSYNCIVPROC >( "glGetSynciv", out _glGetSynciv );

        _glGetSynciv( sync, pname, bufSize, length, values );
    }

    /// <inheritdoc />
    public GLint[] GetSynciv( IntPtr sync, GLenum pname, GLsizei bufSize )
    {
        var ret = new GLint[ bufSize ];

        GetDelegateForFunction< PFNGLGETSYNCIVPROC >( "glGetSynciv", out _glGetSynciv );

        {
            fixed ( GLint* dp = &ret[ 0 ] )
            {
                GLsizei len;
                _glGetSynciv( sync, pname, bufSize, &len, dp );
                Array.Resize( ref ret, len );
            }
        }

        return ret;
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetInteger64i_v( GLenum target, GLuint index, GLint64* data )
    {
        GetDelegateForFunction< PFNGLGETINTEGER64I_VPROC >( "glGetInteger64i_v", out _glGetInteger64i_v );

        _glGetInteger64i_v( target, index, data );
    }

    /// <inheritdoc />
    public void GetInteger64i_v( GLenum target, GLuint index, ref GLint64[] data )
    {
        GetDelegateForFunction< PFNGLGETINTEGER64I_VPROC >( "glGetInteger64i_v", out _glGetInteger64i_v );

        fixed ( GLint64* dp = &data[ 0 ] )
        {
            _glGetInteger64i_v( target, index, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void FramebufferTexture( GLenum target, GLenum attachment, GLuint texture, GLint level )
    {
        GetDelegateForFunction< PFNGLFRAMEBUFFERTEXTUREPROC >( "glFramebufferTexture", out _glFramebufferTexture );

        _glFramebufferTexture( target, attachment, texture, level );
    }

    // ========================================================================

    /// <inheritdoc />
    public void BindFragDataLocationIndexed( GLuint program, GLuint colorNumber, GLuint index, GLchar* name )
    {
        GetDelegateForFunction< PFNGLBINDFRAGDATALOCATIONINDEXEDPROC >
            (
             "glBindFragDataLocationIndexed",
             out _glBindFragDataLocationIndexed
            );

        _glBindFragDataLocationIndexed( program, colorNumber, index, name );
    }

    /// <inheritdoc />
    public void BindFragDataLocationIndexed( GLuint program, GLuint colorNumber, GLuint index, string name )
    {
        byte[] nameBytes = Encoding.UTF8.GetBytes( name );

        GetDelegateForFunction< PFNGLBINDFRAGDATALOCATIONINDEXEDPROC >
            (
             "glBindFragDataLocationIndexed",
             out _glBindFragDataLocationIndexed
            );

        {
            fixed ( GLchar* p = &nameBytes[ 0 ] )
            {
                _glBindFragDataLocationIndexed( program, colorNumber, index, p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public GLint GetFragDataIndex( GLuint program, GLchar* name )
    {
        GetDelegateForFunction< PFNGLGETFRAGDATAINDEXPROC >( "glGetFragDataIndex", out _glGetFragDataIndex );

        return _glGetFragDataIndex( program, name );
    }

    /// <inheritdoc />
    public GLint GetFragDataIndex( GLuint program, string name )
    {
        byte[] nameBytes = Encoding.UTF8.GetBytes( name );

        GetDelegateForFunction< PFNGLGETFRAGDATAINDEXPROC >( "glGetFragDataIndex", out _glGetFragDataIndex );

        fixed ( GLchar* p = &nameBytes[ 0 ] )
        {
            return _glGetFragDataIndex( program, p );
        }
    }

    // ========================================================================

    public void BlendEquationi( GLuint buf, GLenum mode )
    {
        GetDelegateForFunction< PFNGLBLENDEQUATIONIPROC >( "glBlendEquationi", out _glBlendEquationi );

        _glBlendEquationi( buf, mode );
    }

    // ========================================================================

    public void BlendEquationSeparatei( GLuint buf, GLenum modeRGB, GLenum modeAlpha )
    {
        GetDelegateForFunction< PFNGLBLENDEQUATIONSEPARATEIPROC >
            (
             "glBlendEquationSeparatei",
             out _glBlendEquationSeparatei
            );

        _glBlendEquationSeparatei( buf, modeRGB, modeAlpha );
    }

    // ========================================================================

    public void BlendFunci( GLuint buf, GLenum src, GLenum dst )
    {
        GetDelegateForFunction< PFNGLBLENDFUNCIPROC >( "glBlendFunci", out _glBlendFunci );

        _glBlendFunci( buf, src, dst );
    }

    // ========================================================================

    public void BlendFuncSeparatei( GLuint buf, GLenum srcRGB, GLenum dstRGB, GLenum srcAlpha, GLenum dstAlpha )
    {
        GetDelegateForFunction< PFNGLBLENDFUNCSEPARATEIPROC >( "glBlendFuncSeparatei", out _glBlendFuncSeparatei );

        _glBlendFuncSeparatei( buf, srcRGB, dstRGB, srcAlpha, dstAlpha );
    }

    // ========================================================================

    public void DrawArraysIndirect( GLenum mode, IntPtr indirect )
    {
        GetDelegateForFunction< PFNGLDRAWARRAYSINDIRECTPROC >( "glDrawArraysIndirect", out _glDrawArraysIndirect );

        _glDrawArraysIndirect( mode, indirect );
    }

    // ========================================================================

    public void DrawElementsIndirect( GLenum mode, GLenum type, IntPtr indirect )
    {
        GetDelegateForFunction< PFNGLDRAWELEMENTSINDIRECTPROC >
            (
             "glDrawElementsIndirect",
             out _glDrawElementsIndirect
            );

        _glDrawElementsIndirect( mode, type, indirect );
    }

    // ========================================================================

    public GLint GetSubroutineUniformLocation( GLuint program, GLenum shadertype, GLchar* name )
    {
        GetDelegateForFunction< PFNGLGETSUBROUTINEUNIFORMLOCATIONPROC >
            (
             "glGetSubroutineUniformLocation",
             out _glGetSubroutineUniformLocation
            );

        return _glGetSubroutineUniformLocation( program, shadertype, name );
    }

    public GLint GetSubroutineUniformLocation( GLuint program, GLenum shadertype, string name )
    {
        byte[] nameBytes = Encoding.UTF8.GetBytes( name );

        GetDelegateForFunction< PFNGLGETSUBROUTINEUNIFORMLOCATIONPROC >
            (
             "glGetSubroutineUniformLocation",
             out _glGetSubroutineUniformLocation
            );

        fixed ( GLchar* p = &nameBytes[ 0 ] )
        {
            return _glGetSubroutineUniformLocation( program, shadertype, p );
        }
    }

    // ========================================================================

    public GLuint GetSubroutineIndex( GLuint program, GLenum shadertype, GLchar* name )
    {
        GetDelegateForFunction< PFNGLGETSUBROUTINEINDEXPROC >( "glGetSubroutineIndex", out _glGetSubroutineIndex );

        return _glGetSubroutineIndex( program, shadertype, name );
    }

    public GLuint GetSubroutineIndex( GLuint program, GLenum shadertype, string name )
    {
        byte[] nameBytes = Encoding.UTF8.GetBytes( name );

        GetDelegateForFunction< PFNGLGETSUBROUTINEINDEXPROC >( "glGetSubroutineIndex", out _glGetSubroutineIndex );

        fixed ( GLchar* p = &nameBytes[ 0 ] )
        {
            return _glGetSubroutineIndex( program, shadertype, p );
        }
    }

    // ========================================================================

    public void GetActiveSubroutineUniformiv( GLuint program, GLenum shadertype, GLuint index, GLenum pname,
                                              GLint* values )
    {
        GetDelegateForFunction< PFNGLGETACTIVESUBROUTINEUNIFORMIVPROC >
            (
             "glGetActiveSubroutineUniformiv",
             out _glGetActiveSubroutineUniformiv
            );

        _glGetActiveSubroutineUniformiv( program, shadertype, index, pname, values );
    }

    public void GetActiveSubroutineUniformiv( GLuint program, GLenum shadertype, GLuint index, GLenum pname,
                                              ref GLint[] values )
    {
        GetDelegateForFunction< PFNGLGETACTIVESUBROUTINEUNIFORMIVPROC >
            (
             "glGetActiveSubroutineUniformiv",
             out _glGetActiveSubroutineUniformiv
            );

        fixed ( GLint* p = &values[ 0 ] )
        {
            _glGetActiveSubroutineUniformiv( program, shadertype, index, pname, p );
        }
    }

    // ========================================================================

    public void GetActiveSubroutineUniformName( GLuint program, GLenum shadertype, GLuint index, GLsizei bufsize,
                                                GLsizei* length,
                                                GLchar* name )
    {
        GetDelegateForFunction< PFNGLGETACTIVESUBROUTINEUNIFORMNAMEPROC >
            (
             "glGetActiveSubroutineUniformName",
             out _glGetActiveSubroutineUniformName
            );

        _glGetActiveSubroutineUniformName( program, shadertype, index, bufsize, length, name );
    }

    public string GetActiveSubroutineUniformName( GLuint program, GLenum shadertype, GLuint index, GLsizei bufsize )
    {
        var name = new GLchar[ bufsize ];

        GetDelegateForFunction< PFNGLGETACTIVESUBROUTINEUNIFORMNAMEPROC >
            (
             "glGetActiveSubroutineUniformName",
             out _glGetActiveSubroutineUniformName
            );

        fixed ( GLchar* p = &name[ 0 ] )
        {
            GLsizei length;

            _glGetActiveSubroutineUniformName( program, shadertype, index, bufsize, &length, p );

            return new string( ( GLbyte* )p, 0, length, Encoding.UTF8 );
        }
    }

    // ========================================================================

    public void GetActiveSubroutineName( GLuint program, GLenum shadertype, GLuint index, GLsizei bufsize,
                                         GLsizei* length, GLchar* name )
    {
        GetDelegateForFunction< PFNGLGETACTIVESUBROUTINENAMEPROC >
            (
             "glGetActiveSubroutineName",
             out _glGetActiveSubroutineName
            );

        _glGetActiveSubroutineName( program, shadertype, index, bufsize, length, name );
    }

    public string GetActiveSubroutineName( GLuint program, GLenum shadertype, GLuint index, GLsizei bufsize )
    {
        var name = new GLchar[ bufsize ];

        GetDelegateForFunction< PFNGLGETACTIVESUBROUTINENAMEPROC >
            (
             "glGetActiveSubroutineName",
             out _glGetActiveSubroutineName
            );

        fixed ( GLchar* p = &name[ 0 ] )
        {
            GLsizei length;

            _glGetActiveSubroutineName( program, shadertype, index, bufsize, &length, p );

            return new string( ( GLbyte* )p, 0, length, Encoding.UTF8 );
        }
    }

    // ========================================================================

    public void PatchParameterfv( GLenum pname, GLfloat* values )
    {
        GetDelegateForFunction< PFNGLPATCHPARAMETERFVPROC >( "glPatchParameterfv", out _glPatchParameterfv );

        _glPatchParameterfv( pname, values );
    }

    public void PatchParameterfv( GLenum pname, GLfloat[] values )
    {
        GetDelegateForFunction< PFNGLPATCHPARAMETERFVPROC >( "glPatchParameterfv", out _glPatchParameterfv );

        fixed ( GLfloat* p = &values[ 0 ] )
        {
            _glPatchParameterfv( pname, p );
        }
    }

    // ========================================================================

    public void BeginQueryIndexed( GLenum target, GLuint index, GLuint id )
    {
        GetDelegateForFunction< PFNGLBEGINQUERYINDEXEDPROC >( "glBeginQueryIndexed", out _glBeginQueryIndexed );

        _glBeginQueryIndexed( target, index, id );
    }

    // ========================================================================

    public void EndQueryIndexed( GLenum target, GLuint index )
    {
        GetDelegateForFunction< PFNGLENDQUERYINDEXEDPROC >( "glEndQueryIndexed", out _glEndQueryIndexed );

        _glEndQueryIndexed( target, index );
    }

    // ========================================================================

    public void GetQueryIndexediv( GLenum target, GLuint index, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETQUERYINDEXEDIVPROC >( "glGetQueryIndexediv", out _glGetQueryIndexediv );

        _glGetQueryIndexediv( target, index, pname, parameters );
    }

    public void GetQueryIndexediv( GLenum target, GLuint index, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETQUERYINDEXEDIVPROC >( "glGetQueryIndexediv", out _glGetQueryIndexediv );

        fixed ( GLint* p = &parameters[ 0 ] )
        {
            _glGetQueryIndexediv( target, index, pname, p );
        }
    }

    // ========================================================================

    public void ReleaseShaderCompiler()
    {
        GetDelegateForFunction< PFNGLRELEASESHADERCOMPILERPROC >
            (
             "glReleaseShaderCompiler",
             out _glReleaseShaderCompiler
            );

        _glReleaseShaderCompiler();
    }

    // ========================================================================

    public void ShaderBinary( GLsizei count, GLuint* shaders, GLenum binaryformat, IntPtr binary, GLsizei length )
    {
        GetDelegateForFunction< PFNGLSHADERBINARYPROC >( "glShaderBinary", out _glShaderBinary );

        _glShaderBinary( count, shaders, binaryformat, binary, length );
    }

    public void ShaderBinary( GLuint[] shaders, GLenum binaryformat, byte[] binary )
    {
        int count = shaders.Length;

        GetDelegateForFunction< PFNGLSHADERBINARYPROC >( "glShaderBinary", out _glShaderBinary );

        fixed ( GLuint* pShaders = &shaders[ 0 ] )
        {
            fixed ( byte* pBinary = &binary[ 0 ] )
            {
                _glShaderBinary( count, pShaders, binaryformat, ( IntPtr )pBinary, binary.Length );
            }
        }
    }

    // ========================================================================

    public void GetShaderPrecisionFormat( GLenum shaderType, GLenum precisionType, GLint* range, GLint* precision )
    {
        GetDelegateForFunction< PFNGLGETSHADERPRECISIONFORMATPROC >
            (
             "glGetShaderPrecisionFormat",
             out _glGetShaderPrecisionFormat
            );

        _glGetShaderPrecisionFormat( shaderType, precisionType, range, precision );
    }

    public void GetShaderPrecisionFormat( GLenum shaderType, GLenum precisionType, ref GLint[] range,
                                          ref GLint precision )
    {
        range = new GLint[ 2 ];

        GetDelegateForFunction< PFNGLGETSHADERPRECISIONFORMATPROC >
            (
             "glGetShaderPrecisionFormat",
             out _glGetShaderPrecisionFormat
            );

        fixed ( GLint* pRange = &range[ 0 ] )
        {
            fixed ( GLint* pPrecision = &precision )
            {
                _glGetShaderPrecisionFormat( shaderType, precisionType, pRange, pPrecision );
            }
        }
    }

    // ========================================================================

    public void DepthRangef( GLfloat n, GLfloat f )
    {
        GetDelegateForFunction< PFNGLDEPTHRANGEFPROC >( "glDepthRangef", out _glDepthRangef );

        _glDepthRangef( n, f );
    }

    // ========================================================================

    public void ClearDepthf( GLfloat d )
    {
        GetDelegateForFunction< PFNGLCLEARDEPTHFPROC >( "glClearDepthf", out _glClearDepthf );

        _glClearDepthf( d );
    }

    // ========================================================================

    public void ViewportArrayv( GLuint first, GLsizei count, GLfloat* v )
    {
        GetDelegateForFunction< PFNGLVIEWPORTARRAYVPROC >( "glViewportArrayv", out _glViewportArrayv );

        _glViewportArrayv( first, count, v );
    }

    public void ViewportArrayv( GLuint first, GLsizei count, params GLfloat[] v )
    {
        GetDelegateForFunction< PFNGLVIEWPORTARRAYVPROC >( "glViewportArrayv", out _glViewportArrayv );

        fixed ( GLfloat* pV = &v[ 0 ] )
        {
            _glViewportArrayv( first, count, pV );
        }
    }

    // ========================================================================

    public void ViewportIndexedf( GLuint index, GLfloat x, GLfloat y, GLfloat w, GLfloat h )
    {
        GetDelegateForFunction< PFNGLVIEWPORTINDEXEDFPROC >( "glViewportIndexedf", out _glViewportIndexedf );

        _glViewportIndexedf( index, x, y, w, h );
    }

    // ========================================================================

    public void ViewportIndexedfv( GLuint index, GLfloat* v )
    {
        GetDelegateForFunction< PFNGLVIEWPORTINDEXEDFVPROC >( "glViewportIndexedfv", out _glViewportIndexedfv );

        _glViewportIndexedfv( index, v );
    }

    public void ViewportIndexedfv( GLuint index, params GLfloat[] v )
    {
        GetDelegateForFunction< PFNGLVIEWPORTINDEXEDFVPROC >( "glViewportIndexedfv", out _glViewportIndexedfv );

        fixed ( GLfloat* pV = &v[ 0 ] )
        {
            _glViewportIndexedfv( index, pV );
        }
    }

    // ========================================================================

    public void ScissorArrayv( GLuint first, GLsizei count, GLint* v )
    {
        GetDelegateForFunction< PFNGLSCISSORARRAYVPROC >( "glScissorArrayv", out _glScissorArrayv );

        _glScissorArrayv( first, count, v );
    }

    public void ScissorArrayv( GLuint first, GLsizei count, params GLint[] v )
    {
        GetDelegateForFunction< PFNGLSCISSORARRAYVPROC >( "glScissorArrayv", out _glScissorArrayv );

        fixed ( GLint* pV = &v[ 0 ] )
        {
            _glScissorArrayv( first, count, pV );
        }
    }

    // ========================================================================

    public void ScissorIndexed( GLuint index, GLint left, GLint bottom, GLsizei width, GLsizei height )
    {
        GetDelegateForFunction< PFNGLSCISSORINDEXEDPROC >( "glScissorIndexed", out _glScissorIndexed );

        _glScissorIndexed( index, left, bottom, width, height );
    }

    // ========================================================================

    public void ScissorIndexedv( GLuint index, GLint* v )
    {
        GetDelegateForFunction< PFNGLSCISSORINDEXEDVPROC >( "glScissorIndexedv", out _glScissorIndexedv );

        _glScissorIndexedv( index, v );
    }

    public void ScissorIndexedv( GLuint index, params GLint[] v )
    {
        GetDelegateForFunction< PFNGLSCISSORINDEXEDVPROC >( "glScissorIndexedv", out _glScissorIndexedv );

        fixed ( GLint* pV = &v[ 0 ] )
        {
            _glScissorIndexedv( index, pV );
        }
    }

    // ========================================================================

    public void DepthRangeArrayv( GLuint first, GLsizei count, GLdouble* v )
    {
        GetDelegateForFunction< PFNGLDEPTHRANGEARRAYVPROC >( "glDepthRangeArrayv", out _glDepthRangeArrayv );

        _glDepthRangeArrayv( first, count, v );
    }

    public void DepthRangeArrayv( GLuint first, GLsizei count, params GLdouble[] v )
    {
        GetDelegateForFunction< PFNGLDEPTHRANGEARRAYVPROC >( "glDepthRangeArrayv", out _glDepthRangeArrayv );

        fixed ( GLdouble* pV = &v[ 0 ] )
        {
            _glDepthRangeArrayv( first, count, pV );
        }
    }

    // ========================================================================

    public void DepthRangeIndexed( GLuint index, GLdouble n, GLdouble f )
    {
        GetDelegateForFunction< PFNGLDEPTHRANGEINDEXEDPROC >( "glDepthRangeIndexed", out _glDepthRangeIndexed );

        _glDepthRangeIndexed( index, n, f );
    }

    // ========================================================================

    public void GetFloati_v( GLenum target, GLuint index, GLfloat* data )
    {
        GetDelegateForFunction< PFNGLGETFLOATI_VPROC >( "glGetFloati_v", out _glGetFloati_v );

        _glGetFloati_v( target, index, data );
    }

    public void GetFloati_v( GLenum target, GLuint index, ref GLfloat[] data )
    {
        GetDelegateForFunction< PFNGLGETFLOATI_VPROC >( "glGetFloati_v", out _glGetFloati_v );

        fixed ( GLfloat* pData = &data[ 0 ] )
        {
            _glGetFloati_v( target, index, pData );
        }
    }

    // ========================================================================

    public void GetDoublei_v( GLenum target, GLuint index, GLdouble* data )
    {
        GetDelegateForFunction< PFNGLGETDOUBLEI_VPROC >( "glGetDoublei_v", out _glGetDoublei_v );

        _glGetDoublei_v( target, index, data );
    }

    public void GetDoublei_v( GLenum target, GLuint index, ref GLdouble[] data )
    {
        GetDelegateForFunction< PFNGLGETDOUBLEI_VPROC >( "glGetDoublei_v", out _glGetDoublei_v );

        fixed ( GLdouble* pData = &data[ 0 ] )
        {
            _glGetDoublei_v( target, index, pData );
        }
    }

    // ========================================================================

    public void DrawArraysInstancedBaseInstance( GLenum mode, GLint first, GLsizei count, GLsizei instancecount,
                                                 GLuint baseinstance )
    {
        GetDelegateForFunction< PFNGLDRAWARRAYSINSTANCEDBASEINSTANCEPROC >
            (
             "glDrawArraysInstancedBaseInstance",
             out _glDrawArraysInstancedBaseInstance
            );

        _glDrawArraysInstancedBaseInstance( mode, first, count, instancecount, baseinstance );
    }

    // ========================================================================

    public void DrawElementsInstancedBaseInstance( GLenum mode, GLsizei count, GLenum type, IntPtr indices,
                                                   GLsizei instancecount,
                                                   GLuint baseinstance )
    {
        GetDelegateForFunction< PFNGLDRAWELEMENTSINSTANCEDBASEINSTANCEPROC >
            (
             "glDrawElementsInstancedBaseInstance",
             out
             _glDrawElementsInstancedBaseInstance
            );

        _glDrawElementsInstancedBaseInstance( mode, count, type, indices, instancecount, baseinstance );
    }

    public void DrawElementsInstancedBaseInstance< T >( GLenum mode, GLsizei count, GLenum type, T[] indices,
                                                        GLsizei instancecount,
                                                        GLuint baseinstance ) where T : unmanaged, IUnsignedNumber< T >
    {
        GetDelegateForFunction< PFNGLDRAWELEMENTSINSTANCEDBASEINSTANCEPROC >
            (
             "glDrawElementsInstancedBaseInstance",
             out
             _glDrawElementsInstancedBaseInstance
            );

        fixed ( void* p = &indices[ 0 ] )
        {
            _glDrawElementsInstancedBaseInstance( mode, count, type, ( IntPtr )p, instancecount, baseinstance );
        }
    }

    // ========================================================================

    public void DrawElementsInstancedBaseVertexBaseInstance( GLenum mode,
                                                             GLsizei count, GLenum type, IntPtr indices,
                                                             GLsizei instancecount, GLint basevertex,
                                                             GLuint baseinstance )
    {
        GetDelegateForFunction<
                PFNGLDRAWELEMENTSINSTANCEDBASEVERTEXBASEINSTANCEPROC >
            (
             "glDrawElementsInstancedBaseVertexBaseInstance",
             out
             _glDrawElementsInstancedBaseVertexBaseInstance
            );

        _glDrawElementsInstancedBaseVertexBaseInstance
            (
             mode,
             count,
             type,
             indices,
             instancecount,
             basevertex,
             baseinstance
            );
    }

    public void DrawElementsInstancedBaseVertexBaseInstance< T >( GLenum mode, GLsizei count, GLenum type, T[] indices,
                                                                  GLsizei instancecount, GLint basevertex,
                                                                  GLuint baseinstance )
        where T : unmanaged, IUnsignedNumber< T >
    {
        GetDelegateForFunction<
                PFNGLDRAWELEMENTSINSTANCEDBASEVERTEXBASEINSTANCEPROC >
            (
             "glDrawElementsInstancedBaseVertexBaseInstance",
             out
             _glDrawElementsInstancedBaseVertexBaseInstance
            );

        fixed ( void* p = &indices[ 0 ] )
        {
            _glDrawElementsInstancedBaseVertexBaseInstance
                (
                 mode,
                 count,
                 type,
                 ( IntPtr )p,
                 instancecount,
                 basevertex,
                 baseinstance
                );
        }
    }

    // ========================================================================

    public void GetInternalformativ( GLenum target, GLenum internalFormat, GLenum pname, GLsizei bufSize,
                                     GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETINTERNALFORMATIVPROC >( "glGetInternalformativ", out _glGetInternalformativ );

        _glGetInternalformativ( target, internalFormat, pname, bufSize, parameters );
    }

    public void GetInternalformativ( GLenum target, GLenum internalFormat, GLenum pname, GLsizei bufSize,
                                     ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETINTERNALFORMATIVPROC >( "glGetInternalformativ", out _glGetInternalformativ );

        fixed ( GLint* p = &parameters[ 0 ] )
        {
            _glGetInternalformativ( target, internalFormat, pname, bufSize, p );
        }
    }

    public void GetInternalformativ( int target, int internalFormat, int pname, int bufSize, out int parameter )
    {
        // Create a temporary array to hold the result from the underlying OpenGL method.
        var tempArray = new int[ 1 ];

        // Call the original method with the temporary array.
        GetInternalformativ( target, internalFormat, pname, bufSize, ref tempArray );

        // Assign the value from the temporary array to the out parameter.
        parameter = tempArray[ 0 ];
    }

    // ========================================================================

    public void GetActiveAtomicCounterBufferiv( GLuint program, GLuint bufferIndex, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETACTIVEATOMICCOUNTERBUFFERIVPROC >
            (
             "glGetActiveAtomicCounterBufferiv",
             out _glGetActiveAtomicCounterBufferiv
            );

        _glGetActiveAtomicCounterBufferiv( program, bufferIndex, pname, parameters );
    }

    public void GetActiveAtomicCounterBufferiv( GLuint program, GLuint bufferIndex, GLenum pname,
                                                ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETACTIVEATOMICCOUNTERBUFFERIVPROC >
            (
             "glGetActiveAtomicCounterBufferiv",
             out _glGetActiveAtomicCounterBufferiv
            );

        fixed ( GLint* p = &parameters[ 0 ] )
        {
            _glGetActiveAtomicCounterBufferiv( program, bufferIndex, pname, p );
        }
    }

    // ========================================================================

    public void BindImageTexture( GLuint unit, GLuint texture, GLint level, GLbool layered, GLint layer,
                                  GLenum access, GLenum format )
    {
        GetDelegateForFunction< PFNGLBINDIMAGETEXTUREPROC >( "glBindImageTexture", out _glBindImageTexture );

        _glBindImageTexture( unit, texture, level, layered, layer, access, format );
    }

    // ========================================================================

    public void MemoryBarrier( GLbitfield barriers )
    {
        GetDelegateForFunction< PFNGLMEMORYBARRIERPROC >( "glMemoryBarrier", out _glMemoryBarrier );

        _glMemoryBarrier( barriers );
    }

    // ========================================================================

    public void TexStorage1D( GLenum target, GLsizei levels, GLenum internalFormat, GLsizei width )
    {
        GetDelegateForFunction< PFNGLTEXSTORAGE1DPROC >( "glTexStorage1D", out _glTexStorage1D );

        _glTexStorage1D( target, levels, internalFormat, width );
    }

    // ========================================================================

    public void TexStorage2D( GLenum target, GLsizei levels, GLenum internalFormat, GLsizei width, GLsizei height )
    {
        GetDelegateForFunction< PFNGLTEXSTORAGE2DPROC >( "glTexStorage2D", out _glTexStorage2D );

        _glTexStorage2D( target, levels, internalFormat, width, height );
    }

    // ========================================================================

    public void TexStorage3D( GLenum target, GLsizei levels, GLenum internalFormat, GLsizei width, GLsizei height,
                              GLsizei depth )
    {
        GetDelegateForFunction< PFNGLTEXSTORAGE3DPROC >( "glTexStorage3D", out _glTexStorage3D );

        _glTexStorage3D( target, levels, internalFormat, width, height, depth );
    }

    // ========================================================================

    public void GetPointerv( GLenum pname, IntPtr* parameters )
    {
        GetDelegateForFunction< PFNGLGETPOINTERVPROC >( "glGetPointerv", out _glGetPointerv );

        _glGetPointerv( pname, parameters );
    }

    public void GetPointerv( GLenum pname, ref IntPtr[] parameters )
    {
        int length = parameters.Length;

        void*  ptr = parameters[ 0 ].ToPointer();
        void** p   = &ptr;

        GetDelegateForFunction< PFNGLGETPOINTERVPROC >( "glGetPointerv", out _glGetPointerv );

        _glGetPointerv( pname, ( IntPtr* )p );

        for ( var i = 0; i < length; i++ )
        {
            parameters[ i ] = new IntPtr( *p );
            p++;
        }
    }

    // ========================================================================

    public void DispatchCompute( GLuint numGroupsX, GLuint numGroupsY, GLuint numGroupsZ )
    {
        GetDelegateForFunction< PFNGLDISPATCHCOMPUTEPROC >( "glDispatchCompute", out _glDispatchCompute );

        _glDispatchCompute( numGroupsX, numGroupsY, numGroupsZ );
    }

    // ========================================================================

    public void DispatchComputeIndirect( IntPtr indirect )
    {
        GetDelegateForFunction< PFNGLDISPATCHCOMPUTEINDIRECTPROC >
            (
             "glDispatchComputeIndirect",
             out _glDispatchComputeIndirect
            );

        _glDispatchComputeIndirect( indirect );
    }

    // ========================================================================

    public void CopyImageSubData( GLuint srcName, GLenum srcTarget, GLint srcLevel, GLint srcX, GLint srcY, GLint srcZ,
                                  GLuint dstName, GLenum dstTarget, GLint dstLevel, GLint dstX, GLint dstY,
                                  GLint dstZ, GLsizei srcWidth, GLsizei srcHeight, GLsizei srcDepth )
    {
        GetDelegateForFunction< PFNGLCOPYIMAGESUBDATAPROC >( "glCopyImageSubData", out _glCopyImageSubData );

        _glCopyImageSubData
            (
             srcName,
             srcTarget,
             srcLevel,
             srcX,
             srcY,
             srcZ,
             dstName,
             dstTarget,
             dstLevel,
             dstX,
             dstY,
             dstZ,
             srcWidth,
             srcHeight,
             srcDepth
            );
    }

    // ========================================================================

    public void GetInternalformati64v( GLenum target, GLenum internalFormat, GLenum pname, GLsizei count,
                                       GLint64* parameters )
    {
        GetDelegateForFunction< PFNGLGETINTERNALFORMATI64VPROC >
            (
             "glGetInternalformati64v",
             out _glGetInternalformati64v
            );

        _glGetInternalformati64v( target, internalFormat, pname, count, parameters );
    }

    public void GetInternalformati64v( GLenum target, GLenum internalFormat, GLenum pname, GLsizei count,
                                       ref GLint64[] parameters )
    {
        GetDelegateForFunction< PFNGLGETINTERNALFORMATI64VPROC >
            (
             "glGetInternalformati64v",
             out _glGetInternalformati64v
            );

        fixed ( GLint64* pParams = &parameters[ 0 ] )
        {
            _glGetInternalformati64v( target, internalFormat, pname, count, pParams );
        }
    }

    // ========================================================================

    public void InvalidateTexSubImage( GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLint zoffset,
                                       GLsizei width,
                                       GLsizei height, GLsizei depth )
    {
        GetDelegateForFunction< PFNGLINVALIDATETEXSUBIMAGEPROC >
            (
             "glInvalidateTexSubImage",
             out _glInvalidateTexSubImage
            );

        _glInvalidateTexSubImage( texture, level, xoffset, yoffset, zoffset, width, height, depth );
    }

    // ========================================================================

    public void InvalidateTexImage( GLuint texture, GLint level )
    {
        GetDelegateForFunction< PFNGLINVALIDATETEXIMAGEPROC >( "glInvalidateTexImage", out _glInvalidateTexImage );

        _glInvalidateTexImage( texture, level );
    }

    // ========================================================================

    public void InvalidateFramebuffer( GLenum target, GLsizei numAttachments, GLenum* attachments )
    {
        GetDelegateForFunction< PFNGLINVALIDATEFRAMEBUFFERPROC >
            (
             "glInvalidateFramebuffer",
             out _glInvalidateFramebuffer
            );

        _glInvalidateFramebuffer( target, numAttachments, attachments );
    }

    public void InvalidateFramebuffer( GLenum target, GLsizei numAttachments, GLenum[] attachments )
    {
        GetDelegateForFunction< PFNGLINVALIDATEFRAMEBUFFERPROC >
            (
             "glInvalidateFramebuffer",
             out _glInvalidateFramebuffer
            );

        fixed ( GLenum* pAttachments = &attachments[ 0 ] )
        {
            _glInvalidateFramebuffer( target, numAttachments, pAttachments );
        }
    }

    // ========================================================================

    public void InvalidateSubFramebuffer( GLenum target, GLsizei numAttachments, GLenum* attachments, GLint x, GLint y,
                                          GLsizei width,
                                          GLsizei height )
    {
        GetDelegateForFunction< PFNGLINVALIDATESUBFRAMEBUFFERPROC >
            (
             "glInvalidateSubFramebuffer",
             out _glInvalidateSubFramebuffer
            );

        _glInvalidateSubFramebuffer( target, numAttachments, attachments, x, y, width, height );
    }

    public void InvalidateSubFramebuffer( GLenum target, GLsizei numAttachments, GLenum[] attachments, GLint x, GLint y,
                                          GLsizei width,
                                          GLsizei height )
    {
        GetDelegateForFunction< PFNGLINVALIDATESUBFRAMEBUFFERPROC >
            (
             "glInvalidateSubFramebuffer",
             out _glInvalidateSubFramebuffer
            );

        fixed ( GLenum* pAttachments = &attachments[ 0 ] )
        {
            _glInvalidateSubFramebuffer( target, numAttachments, pAttachments, x, y, width, height );
        }
    }

    // ========================================================================

    public void MultiDrawArraysIndirect( GLenum mode, IntPtr indirect, GLsizei drawcount, GLsizei stride )
    {
        GetDelegateForFunction< PFNGLMULTIDRAWARRAYSINDIRECTPROC >
            (
             "glMultiDrawArraysIndirect",
             out _glMultiDrawArraysIndirect
            );

        _glMultiDrawArraysIndirect( mode, indirect, drawcount, stride );
    }

    // ========================================================================

    public void MultiDrawElementsIndirect( GLenum mode, GLenum type, IntPtr indirect, GLsizei drawcount,
                                           GLsizei stride )
    {
        GetDelegateForFunction< PFNGLMULTIDRAWELEMENTSINDIRECTPROC >
            (
             "glMultiDrawElementsIndirect",
             out _glMultiDrawElementsIndirect
            );

        _glMultiDrawElementsIndirect( mode, type, indirect, drawcount, stride );
    }

    // ========================================================================

    public void ShaderStorageBlockBinding( GLint program, GLuint storageBlockIndex, GLuint storageBlockBinding )
    {
        if ( !IsProgram( program ) )
        {
            throw new LughRuntimeException( $"Invalid program ID: {program}" );
        }

        GetDelegateForFunction< PFNGLSHADERSTORAGEBLOCKBINDINGPROC >
            (
             "glShaderStorageBlockBinding",
             out _glShaderStorageBlockBinding
            );

        _glShaderStorageBlockBinding( ( uint )program, storageBlockIndex, storageBlockBinding );
    }

    // ========================================================================

    public void TexBufferRange( GLenum target, GLenum internalFormat, GLuint buffer, GLintptr offset, GLsizeiptr size )
    {
        GetDelegateForFunction< PFNGLTEXBUFFERRANGEPROC >( "glTexBufferRange", out _glTexBufferRange );

        _glTexBufferRange( target, internalFormat, buffer, offset, size );
    }

    // ========================================================================

    public void TexStorage2DMultisample( GLenum target, GLsizei samples, GLenum internalFormat, GLsizei width,
                                         GLsizei height,
                                         GLbool fixedsamplelocations )
    {
        GetDelegateForFunction< PFNGLTEXSTORAGE2DMULTISAMPLEPROC >
            (
             "glTexStorage2DMultisample",
             out _glTexStorage2DMultisample
            );

        _glTexStorage2DMultisample( target, samples, internalFormat, width, height, fixedsamplelocations );
    }

    // ========================================================================

    public void TexStorage3DMultisample( GLenum target, GLsizei samples, GLenum internalFormat, GLsizei width,
                                         GLsizei height, GLsizei depth, GLbool fixedsamplelocations )
    {
        GetDelegateForFunction< PFNGLTEXSTORAGE3DMULTISAMPLEPROC >
            (
             "glTexStorage3DMultisample",
             out _glTexStorage3DMultisample
            );

        _glTexStorage3DMultisample( target, samples, internalFormat, width, height, depth, fixedsamplelocations );
    }

    // ========================================================================

    public void TextureView( GLuint texture,
                             GLenum target,
                             GLuint origtexture,
                             GLenum internalFormat, GLuint minlevel, GLuint numlevels, GLuint minlayer,
                             GLuint numlayers )
    {
        GetDelegateForFunction< PFNGLTEXTUREVIEWPROC >( "glTextureView", out _glTextureView );

        _glTextureView( texture, target, origtexture, internalFormat, minlevel, numlevels, minlayer, numlayers );
    }

    // ========================================================================

    public void BindVertexBuffer( GLuint bindingindex, GLuint buffer, GLintptr offset, GLsizei stride )
    {
        GetDelegateForFunction< PFNGLBINDVERTEXBUFFERPROC >( "glBindVertexBuffer", out _glBindVertexBuffer );

        _glBindVertexBuffer( bindingindex, buffer, offset, stride );
    }

    // ========================================================================

    public void ObjectLabel( GLenum identifier, GLuint name, GLsizei length, GLchar* label )
    {
        if ( ObjectLabelAvailable() )
        {
            GetDelegateForFunction< PFNGLOBJECTLABELPROC >( "glObjectLabel", out _glObjectLabel );

            _glObjectLabel( identifier, name, length, label );
        }
    }

    public void ObjectLabel( GLenum identifier, GLuint name, string label )
    {
        if ( ObjectLabelAvailable() )
        {
            if ( name <= 0 )
            {
                Logger.Error( $"Object handle {name} for {label} cannot be <= 0" );

                // No need, in this case, to throw an exception. Just return without
                // adding a debug label. 
                return;
            }

            GetDelegateForFunction< PFNGLOBJECTLABELPROC >( "glObjectLabel", out _glObjectLabel );

            byte[] labelBytes = Encoding.ASCII.GetBytes( label );

            fixed ( GLchar* pLabelBytes = &labelBytes[ 0 ] )
            {
                _glObjectLabel( identifier, name, labelBytes.Length, pLabelBytes );
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private static GLbool ObjectLabelAvailable()
    {
        if ( ( ( Engine.GL.GetOpenGLVersion().major >= 4 ) && ( Engine.GL.GetOpenGLVersion().minor >= 3 ) )
          || Engine.Graphics.SupportsExtension( "GL_ARB_debug_output" ) )
        {
            // glObjectLabel is available through OpenGL or the extension
            Logger.Debug( "glObjectLabel IS available" );

            return true;
        }

        // glObjectLabel is not available
        Logger.Debug( "glObjectLabel IS NOT available" );

        return false;
    }

    // ========================================================================

    public void GetObjectLabel( GLenum identifier, GLuint name, GLsizei bufSize, GLsizei* length, GLchar* label )
    {
        GetDelegateForFunction< PFNGLGETOBJECTLABELPROC >( "glGetObjectLabel", out _glGetObjectLabel );

        _glGetObjectLabel( identifier, name, bufSize, length, label );
    }

    public string GetObjectLabel( GLenum identifier, GLuint name, GLsizei bufSize )
    {
        var labelBytes = new GLchar[ bufSize ];

        GetDelegateForFunction< PFNGLGETOBJECTLABELPROC >( "glGetObjectLabel", out _glGetObjectLabel );

        fixed ( GLchar* pLabelBytes = labelBytes )
        {
            GLsizei length;

            _glGetObjectLabel( identifier, name, bufSize, &length, pLabelBytes );

            return new string( ( GLbyte* )pLabelBytes, 0, length, Encoding.UTF8 );
        }
    }

    // ========================================================================

    public void ObjectPtrLabel( IntPtr ptr, GLsizei length, GLchar* label )
    {
        GetDelegateForFunction< PFNGLOBJECTPTRLABELPROC >( "glObjectPtrLabel", out _glObjectPtrLabel );

        _glObjectPtrLabel( ptr, length, label );
    }

    public void ObjectPtrLabel( IntPtr ptr, string label )
    {
        byte[] labelBytes = Encoding.UTF8.GetBytes( label );

        GetDelegateForFunction< PFNGLOBJECTPTRLABELPROC >( "glObjectPtrLabel", out _glObjectPtrLabel );

        fixed ( GLchar* pLabelBytes = labelBytes )
        {
            _glObjectPtrLabel( ptr, labelBytes.Length, pLabelBytes );
        }
    }

    // ========================================================================

    public void GetObjectPtrLabel( IntPtr ptr, GLsizei bufSize, GLsizei* length, GLchar* label )
    {
        GetDelegateForFunction< PFNGLGETOBJECTPTRLABELPROC >( "glGetObjectPtrLabel", out _glGetObjectPtrLabel );

        _glGetObjectPtrLabel( ptr, bufSize, length, label );
    }

    public string GetObjectPtrLabel( IntPtr ptr, GLsizei bufSize )
    {
        var labelBytes = new GLchar[ bufSize ];

        GetDelegateForFunction< PFNGLGETOBJECTPTRLABELPROC >( "glGetObjectPtrLabel", out _glGetObjectPtrLabel );

        fixed ( GLchar* pLabelBytes = labelBytes )
        {
            GLsizei length;

            _glGetObjectPtrLabel( ptr, bufSize, &length, pLabelBytes );

            return new string( ( GLbyte* )pLabelBytes, 0, length, Encoding.UTF8 );
        }
    }

    // ========================================================================

    public void ClearTexImage( GLuint texture, GLint level, GLenum format, GLenum type, IntPtr data )
    {
        GetDelegateForFunction< PFNGLCLEARTEXIMAGEPROC >( "glClearTexImage", out _glClearTexImage );

        _glClearTexImage( texture, level, format, type, data );
    }

    public void ClearTexImage< T >( GLuint texture, GLint level, GLenum format, GLenum type, T[] data )
        where T : unmanaged
    {
        GetDelegateForFunction< PFNGLCLEARTEXIMAGEPROC >( "glClearTexImage", out _glClearTexImage );

        fixed ( T* t = &data[ 0 ] )
        {
            _glClearTexImage( texture, level, format, type, ( IntPtr )t );
        }
    }

    // ========================================================================

    public void ClearTexSubImage( GLuint texture, GLint level, GLint xOffset, GLint yOffset, GLint zOffset,
                                  GLsizei width, GLsizei height,
                                  GLsizei depth, GLenum format, GLenum type, IntPtr data )
    {
        GetDelegateForFunction< PFNGLCLEARTEXSUBIMAGEPROC >( "glClearTexSubImage", out _glClearTexSubImage );

        _glClearTexSubImage( texture, level, xOffset, yOffset, zOffset, width, height, depth, format, type, data );
    }

    public void ClearTexSubImage< T >( GLuint texture, GLint level, GLint xOffset, GLint yOffset, GLint zOffset,
                                       GLsizei width,
                                       GLsizei height, GLsizei depth, GLenum format, GLenum type, T[] data )
        where T : unmanaged
    {
        GetDelegateForFunction< PFNGLCLEARTEXSUBIMAGEPROC >( "glClearTexSubImage", out _glClearTexSubImage );

        fixed ( T* t = &data[ 0 ] )
        {
            _glClearTexSubImage
                (
                 texture,
                 level,
                 xOffset,
                 yOffset,
                 zOffset,
                 width,
                 height,
                 depth,
                 format,
                 type,
                 ( IntPtr )t
                );
        }
    }

    // ========================================================================

    public void BindTextures( GLuint first, GLsizei count, GLuint* textures )
    {
        GetDelegateForFunction< PFNGLBINDTEXTURESPROC >( "glBindTextures", out _glBindTextures );

        _glBindTextures( first, count, textures );
    }

    public void BindTextures( GLuint first, GLuint[] textures )
    {
        GetDelegateForFunction< PFNGLBINDTEXTURESPROC >( "glBindTextures", out _glBindTextures );

        fixed ( GLuint* pTextures = &textures[ 0 ] )
        {
            _glBindTextures( first, textures.Length, pTextures );
        }
    }

    // ========================================================================

    public void BindImageTextures( GLuint first, GLsizei count, GLuint* textures )
    {
        GetDelegateForFunction< PFNGLBINDIMAGETEXTURESPROC >( "glBindImageTextures", out _glBindImageTextures );

        _glBindImageTextures( first, count, textures );
    }

    public void BindImageTextures( GLuint first, GLuint[] textures )
    {
        GetDelegateForFunction< PFNGLBINDIMAGETEXTURESPROC >( "glBindImageTextures", out _glBindImageTextures );

        fixed ( GLuint* pTextures = &textures[ 0 ] )
        {
            _glBindImageTextures( first, textures.Length, pTextures );
        }
    }

    // ========================================================================

    public void BindVertexBuffers( GLuint first, GLsizei count, GLuint* buffers, GLintptr* offsets, GLsizei* strides )
    {
        GetDelegateForFunction< PFNGLBINDVERTEXBUFFERSPROC >( "glBindVertexBuffers", out _glBindVertexBuffers );

        _glBindVertexBuffers( first, count, buffers, offsets, strides );
    }

    public void BindVertexBuffers( GLuint first, GLuint[] buffers, GLintptr[] offsets, GLsizei[] strides )
    {
        GetDelegateForFunction< PFNGLBINDVERTEXBUFFERSPROC >( "glBindVertexBuffers", out _glBindVertexBuffers );

        fixed ( GLuint* pBuffers = &buffers[ 0 ] )
        {
            fixed ( GLintptr* pOffsets = &offsets[ 0 ] )
            {
                fixed ( GLsizei* pStrides = &strides[ 0 ] )
                {
                    _glBindVertexBuffers( first, buffers.Length, pBuffers, pOffsets, pStrides );
                }
            }
        }
    }

    // ========================================================================

    public void ClipControl( GLenum origin, GLenum depth )
    {
        GetDelegateForFunction< PFNGLCLIPCONTROLPROC >( "glClipControl", out _glClipControl );

        _glClipControl( origin, depth );
    }

    // ========================================================================

    public void CreateBuffers( GLsizei n, GLuint* buffers )
    {
        GetDelegateForFunction< PFNGLCREATEBUFFERSPROC >( "glCreateBuffers", out _glCreateBuffers );

        _glCreateBuffers( n, buffers );
    }

    public GLuint[] CreateBuffers( GLsizei n )
    {
        var buffers = new GLuint[ n ];

        GetDelegateForFunction< PFNGLCREATEBUFFERSPROC >( "glCreateBuffers", out _glCreateBuffers );

        fixed ( GLuint* pBuffers = &buffers[ 0 ] )
        {
            _glCreateBuffers( n, pBuffers );
        }

        return buffers;
    }

    public GLuint CreateBuffer()
    {
        return CreateBuffers( 1 )[ 0 ];
    }

    // ========================================================================

    public void NamedBufferStorage( GLuint buffer, GLsizeiptr size, IntPtr data, GLbitfield flags )
    {
        GetDelegateForFunction< PFNGLNAMEDBUFFERSTORAGEPROC >( "glNamedBufferStorage", out _glNamedBufferStorage );

        _glNamedBufferStorage( buffer, size, data, flags );
    }

    public void NamedBufferStorage< T >( GLuint buffer, GLsizeiptr size, T[] data, GLbitfield flags )
        where T : unmanaged
    {
        GetDelegateForFunction< PFNGLNAMEDBUFFERSTORAGEPROC >( "glNamedBufferStorage", out _glNamedBufferStorage );

        fixed ( T* pData = &data[ 0 ] )
        {
            _glNamedBufferStorage( buffer, size, ( IntPtr )pData, flags );
        }
    }

    // ========================================================================

    public void NamedBufferData( GLuint buffer, GLsizeiptr size, IntPtr data, GLenum usage )
    {
        GetDelegateForFunction< PFNGLNAMEDBUFFERDATAPROC >( "glNamedBufferData", out _glNamedBufferData );

        _glNamedBufferData( buffer, size, data, usage );
    }

    public void NamedBufferData< T >( GLuint buffer, GLsizeiptr size, T[] data, GLenum usage ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLNAMEDBUFFERDATAPROC >( "glNamedBufferData", out _glNamedBufferData );

        fixed ( T* pData = &data[ 0 ] )
        {
            _glNamedBufferData( buffer, size, ( IntPtr )pData, usage );
        }
    }

    // ========================================================================

    public void NamedBufferSubData( GLuint buffer, GLintptr offset, GLsizeiptr size, IntPtr data )
    {
        GetDelegateForFunction< PFNGLNAMEDBUFFERSUBDATAPROC >( "glNamedBufferSubData", out _glNamedBufferSubData );

        _glNamedBufferSubData( buffer, offset, size, data );
    }

    public void NamedBufferSubData< T >( GLuint buffer, GLintptr offset, GLsizeiptr size, T[] data ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLNAMEDBUFFERSUBDATAPROC >( "glNamedBufferSubData", out _glNamedBufferSubData );

        fixed ( T* pData = &data[ 0 ] )
        {
            _glNamedBufferSubData( buffer, offset, size, ( IntPtr )pData );
        }
    }

    // ========================================================================

    public void CopyNamedBufferSubData( GLuint readBuffer, GLuint writeBuffer, GLintptr readOffset,
                                        GLintptr writeOffset, GLsizeiptr size )
    {
        GetDelegateForFunction< PFNGLCOPYNAMEDBUFFERSUBDATAPROC >
            (
             "glCopyNamedBufferSubData",
             out _glCopyNamedBufferSubData
            );

        _glCopyNamedBufferSubData( readBuffer, writeBuffer, readOffset, writeOffset, size );
    }

    // ========================================================================

    public void ClearNamedBufferData( GLuint buffer, GLenum internalFormat, GLenum format, GLenum type, IntPtr data )
    {
        GetDelegateForFunction< PFNGLCLEARNAMEDBUFFERDATAPROC >
            (
             "glClearNamedBufferData",
             out _glClearNamedBufferData
            );

        _glClearNamedBufferData( buffer, internalFormat, format, type, data );
    }

    public void ClearNamedBufferData< T >( GLuint buffer, GLenum internalFormat, GLenum format, GLenum type, T[] data )
        where T : unmanaged
    {
        GetDelegateForFunction< PFNGLCLEARNAMEDBUFFERDATAPROC >
            (
             "glClearNamedBufferData",
             out _glClearNamedBufferData
            );

        fixed ( T* t = &data[ 0 ] )
        {
            _glClearNamedBufferData( buffer, internalFormat, format, type, ( IntPtr )t );
        }
    }

    // ========================================================================

    public void ClearNamedBufferSubData( GLuint buffer, GLenum internalFormat, GLintptr offset, GLsizeiptr size,
                                         GLenum format, GLenum type,
                                         IntPtr data )
    {
        GetDelegateForFunction< PFNGLCLEARNAMEDBUFFERSUBDATAPROC >
            (
             "glClearNamedBufferSubData",
             out _glClearNamedBufferSubData
            );

        _glClearNamedBufferSubData( buffer, internalFormat, offset, size, format, type, data );
    }

    public void ClearNamedBufferSubData< T >( GLuint buffer, GLenum internalFormat, GLintptr offset, GLsizeiptr size,
                                              GLenum format,
                                              GLenum type, T[] data ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLCLEARNAMEDBUFFERSUBDATAPROC >
            (
             "glClearNamedBufferSubData",
             out _glClearNamedBufferSubData
            );

        fixed ( T* t = &data[ 0 ] )
        {
            _glClearNamedBufferSubData( buffer, internalFormat, offset, size, format, type, ( IntPtr )t );
        }
    }

    // ========================================================================

    public IntPtr MapNamedBuffer( GLuint buffer, GLenum access )
    {
        GetDelegateForFunction< PFNGLMAPNAMEDBUFFERPROC >( "glMapNamedBuffer", out _glMapNamedBuffer );

        return _glMapNamedBuffer( buffer, access );
    }

    public Span< T > MapNamedBuffer< T >( GLuint buffer, GLenum access ) where T : unmanaged
    {
        GLint* size = stackalloc GLint[ 1 ];

        GetDelegateForFunction< PFNGLGETNAMEDBUFFERPARAMETERIVPROC >
            (
             "glGetNamedBufferParameteriv",
             out _glGetNamedBufferParameteriv
            );

        _glGetNamedBufferParameteriv( buffer, IGL.GLBufferSize, size );

        GetDelegateForFunction< PFNGLMAPNAMEDBUFFERPROC >( "glMapNamedBuffer", out _glMapNamedBuffer );

        IntPtr ptr = _glMapNamedBuffer( buffer, access );

        return new Span< T >( ( void* )ptr, *size / sizeof( T ) );
    }

    // ========================================================================

    public IntPtr MapNamedBufferRange( GLuint buffer, GLintptr offset, GLsizeiptr length, GLbitfield access )
    {
        GetDelegateForFunction< PFNGLMAPNAMEDBUFFERRANGEPROC >( "glMapNamedBufferRange", out _glMapNamedBufferRange );

        return _glMapNamedBufferRange( buffer, offset, length, access );
    }

    public Span< T > MapNamedBufferRange< T >( GLuint buffer, GLintptr offset, GLsizeiptr length, GLbitfield access )
        where T : unmanaged
    {
        GetDelegateForFunction< PFNGLMAPNAMEDBUFFERRANGEPROC >( "glMapNamedBufferRange", out _glMapNamedBufferRange );

        IntPtr ptr = _glMapNamedBufferRange( buffer, offset, length, access );

        return new Span< T >( ( void* )ptr, length / sizeof( T ) );
    }

    // ========================================================================

    public GLbool UnmapNamedBuffer( GLuint buffer )
    {
        GetDelegateForFunction< PFNGLUNMAPNAMEDBUFFERPROC >( "glUnmapNamedBuffer", out _glUnmapNamedBuffer );

        return _glUnmapNamedBuffer( buffer );
    }

    // ========================================================================

    public void FlushMappedNamedBufferRange( GLuint buffer, GLintptr offset, GLsizeiptr length )
    {
        GetDelegateForFunction< PFNGLFLUSHMAPPEDNAMEDBUFFERRANGEPROC >
            (
             "glFlushMappedNamedBufferRange",
             out _glFlushMappedNamedBufferRange
            );

        _glFlushMappedNamedBufferRange( buffer, offset, length );
    }

    // ========================================================================

    public void GetNamedBufferParameteriv( GLuint buffer, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETNAMEDBUFFERPARAMETERIVPROC >
            (
             "glGetNamedBufferParameteriv",
             out _glGetNamedBufferParameteriv
            );

        _glGetNamedBufferParameteriv( buffer, pname, parameters );
    }

    public void GetNamedBufferParameteriv( GLuint buffer, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETNAMEDBUFFERPARAMETERIVPROC >
            (
             "glGetNamedBufferParameteriv",
             out _glGetNamedBufferParameteriv
            );

        fixed ( GLint* ptrParameters = &parameters[ 0 ] )
        {
            _glGetNamedBufferParameteriv( buffer, pname, ptrParameters );
        }
    }

    // ========================================================================

    public void GetNamedBufferParameteri64v( GLuint buffer, GLenum pname, GLint64* parameters )
    {
        GetDelegateForFunction< PFNGLGETNAMEDBUFFERPARAMETERI64VPROC >
            (
             "glGetNamedBufferParameteri64v",
             out _glGetNamedBufferParameteri64v
            );

        _glGetNamedBufferParameteri64v( buffer, pname, parameters );
    }

    public void GetNamedBufferParameteri64v( GLuint buffer, GLenum pname, ref GLint64[] parameters )
    {
        GetDelegateForFunction< PFNGLGETNAMEDBUFFERPARAMETERI64VPROC >
            (
             "glGetNamedBufferParameteri64v",
             out _glGetNamedBufferParameteri64v
            );

        fixed ( GLint64* ptrParameters = &parameters[ 0 ] )
        {
            _glGetNamedBufferParameteri64v( buffer, pname, ptrParameters );
        }
    }

    // ========================================================================

    public void GetNamedBufferPointerv( GLuint buffer, GLenum pname, IntPtr* parameters )
    {
        GetDelegateForFunction< PFNGLGETNAMEDBUFFERPOINTERVPROC >
            (
             "glGetNamedBufferPointerv",
             out _glGetNamedBufferPointerv
            );

        _glGetNamedBufferPointerv( buffer, pname, parameters );
    }

    public void GetNamedBufferPointerv( GLuint buffer, GLenum pname, ref IntPtr[] parameters )
    {
        var ptrParameters = new IntPtr[ parameters.Length ];

        for ( var i = 0; i < parameters.Length; i++ )
        {
            ptrParameters[ i ] = parameters[ i ];
        }

        GetDelegateForFunction< PFNGLGETNAMEDBUFFERPOINTERVPROC >
            (
             "glGetNamedBufferPointerv",
             out _glGetNamedBufferPointerv
            );

        fixed ( IntPtr* ptr = &ptrParameters[ 0 ] )
        {
            _glGetNamedBufferPointerv( buffer, pname, ptr );
        }

        for ( var i = 0; i < parameters.Length; i++ )
        {
            parameters[ i ] = ptrParameters[ i ];
        }
    }

    // ========================================================================

    public void GetNamedBufferSubData( GLuint buffer, GLintptr offset, GLsizeiptr size, IntPtr data )
    {
        GetDelegateForFunction< PFNGLGETNAMEDBUFFERSUBDATAPROC >
            (
             "glGetNamedBufferSubData",
             out _glGetNamedBufferSubData
            );

        _glGetNamedBufferSubData( buffer, offset, size, data );
    }

    public T[] GetNamedBufferSubData< T >( GLuint buffer, GLintptr offset, GLsizeiptr size ) where T : unmanaged
    {
        var data = new T[ size / sizeof( T ) ];

        GetDelegateForFunction< PFNGLGETNAMEDBUFFERSUBDATAPROC >
            (
             "glGetNamedBufferSubData",
             out _glGetNamedBufferSubData
            );

        fixed ( T* ptrData = &data[ 0 ] )
        {
            _glGetNamedBufferSubData( buffer, offset, size, ( IntPtr )ptrData );
        }

        return data;
    }

    // ========================================================================

    public void CreateFramebuffers( GLsizei n, GLuint* framebuffers )
    {
        GetDelegateForFunction< PFNGLCREATEFRAMEBUFFERSPROC >( "glCreateFrameBuffers", out _glCreateFramebuffers );

        _glCreateFramebuffers( n, framebuffers );
    }

    public GLuint[] CreateFramebuffers( GLsizei n )
    {
        var framebuffers = new GLuint[ n ];

        GetDelegateForFunction< PFNGLCREATEFRAMEBUFFERSPROC >( "glCreateFrameBuffers", out _glCreateFramebuffers );

        fixed ( GLuint* ptrFramebuffers = &framebuffers[ 0 ] )
        {
            _glCreateFramebuffers( n, ptrFramebuffers );
        }

        return framebuffers;
    }

    public GLuint CreateFramebuffer()
    {
        return CreateFramebuffers( 1 )[ 0 ];
    }

    // ========================================================================

    public void NamedFramebufferRenderbuffer( GLuint framebuffer, GLenum attachment, GLenum renderbuffertarget,
                                              GLuint renderbuffer )
    {
        GetDelegateForFunction< PFNGLNAMEDFRAMEBUFFERRENDERBUFFERPROC >
            (
             "glNamedFramebufferRenderbuffer",
             out _glNamedFramebufferRenderbuffer
            );

        _glNamedFramebufferRenderbuffer( framebuffer, attachment, renderbuffertarget, renderbuffer );
    }

    // ========================================================================

    public void NamedFramebufferParameteri( GLuint framebuffer, GLenum pname, GLint param )
    {
        GetDelegateForFunction< PFNGLNAMEDFRAMEBUFFERPARAMETERIPROC >
            (
             "glNamedFramebufferParameteri",
             out _glNamedFramebufferParameteri
            );

        _glNamedFramebufferParameteri( framebuffer, pname, param );
    }

    // ========================================================================

    public void NamedFramebufferTexture( GLuint framebuffer, GLenum attachment, GLuint texture, GLint level )
    {
        GetDelegateForFunction< PFNGLNAMEDFRAMEBUFFERTEXTUREPROC >
            (
             "glNamedFramebufferTexture",
             out _glNamedFramebufferTexture
            );

        _glNamedFramebufferTexture( framebuffer, attachment, texture, level );
    }

    // ========================================================================

    public void NamedFramebufferTextureLayer( GLuint framebuffer, GLenum attachment, GLuint texture, GLint level,
                                              GLint layer )
    {
        GetDelegateForFunction< PFNGLNAMEDFRAMEBUFFERTEXTURELAYERPROC >
            (
             "glNamedFramebufferTextureLayer",
             out _glNamedFramebufferTextureLayer
            );

        _glNamedFramebufferTextureLayer( framebuffer, attachment, texture, level, layer );
    }

    // ========================================================================

    public void NamedFramebufferDrawBuffer( GLuint framebuffer, GLenum buf )
    {
        GetDelegateForFunction< PFNGLNAMEDFRAMEBUFFERDRAWBUFFERPROC >
            (
             "glNamedFramebufferDrawBuffer",
             out _glNamedFramebufferDrawBuffer
            );

        _glNamedFramebufferDrawBuffer( framebuffer, buf );
    }

    // ========================================================================

    public void NamedFramebufferDrawBuffers( GLuint framebuffer, GLsizei n, GLenum* bufs )
    {
        GetDelegateForFunction< PFNGLNAMEDFRAMEBUFFERDRAWBUFFERSPROC >
            (
             "glNamedFramebufferDrawBuffers",
             out _glNamedFramebufferDrawBuffers
            );

        _glNamedFramebufferDrawBuffers( framebuffer, n, bufs );
    }

    public void NamedFramebufferDrawBuffers( GLuint framebuffer, GLenum[] bufs )
    {
        GetDelegateForFunction< PFNGLNAMEDFRAMEBUFFERDRAWBUFFERSPROC >
            (
             "glNamedFramebufferDrawBuffers",
             out _glNamedFramebufferDrawBuffers
            );

        fixed ( GLenum* ptrBufs = &bufs[ 0 ] )
        {
            _glNamedFramebufferDrawBuffers( framebuffer, bufs.Length, ptrBufs );
        }
    }

    // ========================================================================

    public void NamedFramebufferReadBuffer( GLuint framebuffer, GLenum src )
    {
        GetDelegateForFunction< PFNGLNAMEDFRAMEBUFFERREADBUFFERPROC >
            (
             "glNamedFramebufferReadBuffer",
             out _glNamedFramebufferReadBuffer
            );

        _glNamedFramebufferReadBuffer( framebuffer, src );
    }

    // ========================================================================

    public void InvalidateNamedFramebufferData( GLuint framebuffer, GLsizei numAttachments, GLenum* attachments )
    {
        GetDelegateForFunction< PFNGLINVALIDATENAMEDFRAMEBUFFERDATAPROC >
            (
             "glInvalidateNamedFramebufferData",
             out _glInvalidateNamedFramebufferData
            );

        _glInvalidateNamedFramebufferData( framebuffer, numAttachments, attachments );
    }

    public void InvalidateNamedFramebufferData( GLuint framebuffer, GLenum[] attachments )
    {
        GetDelegateForFunction< PFNGLINVALIDATENAMEDFRAMEBUFFERDATAPROC >
            (
             "glInvalidateNamedFramebufferData",
             out _glInvalidateNamedFramebufferData
            );

        fixed ( GLenum* ptrAttachments = &attachments[ 0 ] )
        {
            _glInvalidateNamedFramebufferData( framebuffer, attachments.Length, ptrAttachments );
        }
    }

    // ========================================================================

    public void InvalidateNamedFramebufferSubData( GLuint framebuffer, GLsizei numAttachments, GLenum* attachments,
                                                   GLint x, GLint y,
                                                   GLsizei width, GLsizei height )
    {
        GetDelegateForFunction< PFNGLINVALIDATENAMEDFRAMEBUFFERSUBDATAPROC >
            (
             "glInvalidateNamedFramebufferSubData",
             out
             _glInvalidateNamedFramebufferSubData
            );

        _glInvalidateNamedFramebufferSubData( framebuffer, numAttachments, attachments, x, y, width, height );
    }

    public void InvalidateNamedFramebufferSubData( GLuint framebuffer, GLenum[] attachments, GLint x, GLint y,
                                                   GLsizei width,
                                                   GLsizei height )
    {
        GetDelegateForFunction< PFNGLINVALIDATENAMEDFRAMEBUFFERSUBDATAPROC >
            (
             "glInvalidateNamedFramebufferSubData",
             out
             _glInvalidateNamedFramebufferSubData
            );

        fixed ( GLenum* ptrAttachments = &attachments[ 0 ] )
        {
            _glInvalidateNamedFramebufferSubData
                (
                 framebuffer,
                 attachments.Length,
                 ptrAttachments,
                 x,
                 y,
                 width,
                 height
                );
        }
    }

    // ========================================================================

    public void ClearNamedFramebufferiv( GLuint framebuffer, GLenum buffer, GLint drawbuffer, GLint* value )
    {
        GetDelegateForFunction< PFNGLCLEARNAMEDFRAMEBUFFERIVPROC >
            (
             "glClearNamedFramebufferiv",
             out _glClearNamedFramebufferiv
            );

        _glClearNamedFramebufferiv( framebuffer, buffer, drawbuffer, value );
    }

    public void ClearNamedFramebufferiv( GLuint framebuffer, GLenum buffer, GLint drawbuffer, GLint[] value )
    {
        GetDelegateForFunction< PFNGLCLEARNAMEDFRAMEBUFFERIVPROC >
            (
             "glClearNamedFramebufferiv",
             out _glClearNamedFramebufferiv
            );

        fixed ( GLint* ptrValue = &value[ 0 ] )
        {
            _glClearNamedFramebufferiv( framebuffer, buffer, drawbuffer, ptrValue );
        }
    }

    // ========================================================================

    public void ClearNamedFramebufferuiv( GLuint framebuffer, GLenum buffer, GLint drawbuffer, GLuint* value )
    {
        GetDelegateForFunction< PFNGLCLEARNAMEDFRAMEBUFFERUIVPROC >
            (
             "glClearNamedFramebufferuiv",
             out _glClearNamedFramebufferuiv
            );

        _glClearNamedFramebufferuiv( framebuffer, buffer, drawbuffer, value );
    }

    public void ClearNamedFramebufferuiv( GLuint framebuffer, GLenum buffer, GLint drawbuffer, GLuint[] value )
    {
        GetDelegateForFunction< PFNGLCLEARNAMEDFRAMEBUFFERUIVPROC >
            (
             "glClearNamedFramebufferuiv",
             out _glClearNamedFramebufferuiv
            );

        fixed ( GLuint* ptrValue = &value[ 0 ] )
        {
            _glClearNamedFramebufferuiv( framebuffer, buffer, drawbuffer, ptrValue );
        }
    }

    // ========================================================================

    public void ClearNamedFramebufferfv( GLuint framebuffer, GLenum buffer, GLint drawbuffer, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLCLEARNAMEDFRAMEBUFFERFVPROC >
            (
             "glClearNamedFramebufferfv",
             out _glClearNamedFramebufferfv
            );

        _glClearNamedFramebufferfv( framebuffer, buffer, drawbuffer, value );
    }

    public void ClearNamedFramebufferfv( GLuint framebuffer, GLenum buffer, GLint drawbuffer, GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLCLEARNAMEDFRAMEBUFFERFVPROC >
            (
             "glClearNamedFramebufferfv",
             out _glClearNamedFramebufferfv
            );

        fixed ( GLfloat* ptrValue = &value[ 0 ] )
        {
            _glClearNamedFramebufferfv( framebuffer, buffer, drawbuffer, ptrValue );
        }
    }

    // ========================================================================

    public void ClearNamedFramebufferfi( GLuint framebuffer, GLenum buffer, GLfloat depth, GLint stencil )
    {
        GetDelegateForFunction< PFNGLCLEARNAMEDFRAMEBUFFERFIPROC >
            (
             "glClearNamedFramebufferfi",
             out _glClearNamedFramebufferfi
            );

        _glClearNamedFramebufferfi( framebuffer, buffer, depth, stencil );
    }

    // ========================================================================

    public void BlitNamedFramebuffer( GLuint readFramebuffer, GLuint drawFramebuffer, GLint srcX0, GLint srcY0,
                                      GLint srcX1, GLint srcY1,
                                      GLint dstX0, GLint dstY0, GLint dstX1, GLint dstY1,
                                      GLbitfield mask, GLenum filter )
    {
        GetDelegateForFunction< PFNGLBLITNAMEDFRAMEBUFFERPROC >
            (
             "glBlitNamedFramebuffer",
             out _glBlitNamedFramebuffer
            );

        _glBlitNamedFramebuffer
            (
             readFramebuffer,
             drawFramebuffer,
             srcX0,
             srcY0,
             srcX1,
             srcY1,
             dstX0,
             dstY0,
             dstX1,
             dstY1,
             mask,
             filter
            );
    }

    // ========================================================================

    public GLenum CheckNamedFramebufferStatus( GLuint framebuffer, GLenum target )
    {
        GetDelegateForFunction< PFNGLCHECKNAMEDFRAMEBUFFERSTATUSPROC >
            (
             "glCheckNamedFramebufferStatus",
             out _glCheckNamedFramebufferStatus
            );

        return _glCheckNamedFramebufferStatus( framebuffer, target );
    }

    // ========================================================================

    public void GetNamedFramebufferParameteriv( GLuint framebuffer, GLenum pname, GLint* param )
    {
        GetDelegateForFunction< PFNGLGETNAMEDFRAMEBUFFERPARAMETERIVPROC >
            (
             "glGetNamedFramebufferParameteriv",
             out _glGetNamedFramebufferParameteriv
            );

        _glGetNamedFramebufferParameteriv( framebuffer, pname, param );
    }

    public void GetNamedFramebufferParameteriv( GLuint framebuffer, GLenum pname, ref GLint[] param )
    {
        GetDelegateForFunction< PFNGLGETNAMEDFRAMEBUFFERPARAMETERIVPROC >
            (
             "glGetNamedFramebufferParameteriv",
             out _glGetNamedFramebufferParameteriv
            );

        fixed ( GLint* ptrParam = &param[ 0 ] )
        {
            _glGetNamedFramebufferParameteriv( framebuffer, pname, ptrParam );
        }
    }

    // ========================================================================

    public void GetNamedFramebufferAttachmentParameteriv( GLuint framebuffer, GLenum attachment, GLenum pname,
                                                          GLint* parameters )
    {
        GetDelegateForFunction<
                PFNGLGETNAMEDFRAMEBUFFERATTACHMENTPARAMETERIVPROC >
            (
             "glGetNamedFramebufferAttachmentParameteriv",
             out _glGetNamedFramebufferAttachmentParameteriv
            );

        _glGetNamedFramebufferAttachmentParameteriv( framebuffer, attachment, pname, parameters );
    }

    public void GetNamedFramebufferAttachmentParameteriv( GLuint framebuffer, GLenum attachment, GLenum pname,
                                                          ref GLint[] parameters )
    {
        GetDelegateForFunction<
                PFNGLGETNAMEDFRAMEBUFFERATTACHMENTPARAMETERIVPROC >
            (
             "glGetNamedFramebufferAttachmentParameteriv",
             out _glGetNamedFramebufferAttachmentParameteriv
            );

        fixed ( GLint* ptrParams = &parameters[ 0 ] )
        {
            _glGetNamedFramebufferAttachmentParameteriv( framebuffer, attachment, pname, ptrParams );
        }
    }

    // ========================================================================

    public void CreateRenderbuffers( GLsizei n, GLuint* renderbuffers )
    {
        GetDelegateForFunction< PFNGLCREATERENDERBUFFERSPROC >( "glCreateRenderbuffers", out _glCreateRenderbuffers );

        _glCreateRenderbuffers( n, renderbuffers );
    }

    public GLuint[] CreateRenderbuffers( GLsizei n )
    {
        var renderbuffers = new GLuint[ n ];

        GetDelegateForFunction< PFNGLCREATERENDERBUFFERSPROC >( "glCreateRenderbuffers", out _glCreateRenderbuffers );

        fixed ( GLuint* ptrRenderbuffers = &renderbuffers[ 0 ] )
        {
            _glCreateRenderbuffers( n, ptrRenderbuffers );
        }

        return renderbuffers;
    }

    public GLuint CreateRenderbuffer()
    {
        return CreateRenderbuffers( 1 )[ 0 ];
    }

    // ========================================================================

    public void NamedRenderbufferStorage( GLuint renderbuffer, GLenum internalFormat, GLsizei width, GLsizei height )
    {
        GetDelegateForFunction< PFNGLNAMEDRENDERBUFFERSTORAGEPROC >
            (
             "glNamedRenderbufferStorage",
             out _glNamedRenderbufferStorage
            );

        _glNamedRenderbufferStorage( renderbuffer, internalFormat, width, height );
    }

    // ========================================================================

    public void NamedRenderbufferStorageMultisample( GLuint renderbuffer, GLsizei samples, GLenum internalFormat,
                                                     GLsizei width,
                                                     GLsizei height )
    {
        GetDelegateForFunction< PFNGLNAMEDRENDERBUFFERSTORAGEMULTISAMPLEPROC >
            (
             "glNamedRenderbufferStorageMultisample",
             out
             _glNamedRenderbufferStorageMultisample
            );

        _glNamedRenderbufferStorageMultisample( renderbuffer, samples, internalFormat, width, height );
    }

    // ========================================================================

    public void GetNamedRenderbufferParameteriv( GLuint renderbuffer, GLenum pname, GLint* param )
    {
        GetDelegateForFunction< PFNGLGETNAMEDRENDERBUFFERPARAMETERIVPROC >
            (
             "glGetNamedRenderbufferParameteriv",
             out _glGetNamedRenderbufferParameteriv
            );

        _glGetNamedRenderbufferParameteriv( renderbuffer, pname, param );
    }

    public void GetNamedRenderbufferParameteriv( GLuint renderbuffer, GLenum pname, ref GLint[] param )
    {
        GetDelegateForFunction< PFNGLGETNAMEDRENDERBUFFERPARAMETERIVPROC >
            (
             "glGetNamedRenderbufferParameteriv",
             out _glGetNamedRenderbufferParameteriv
            );

        fixed ( GLint* ptrParam = &param[ 0 ] )
        {
            _glGetNamedRenderbufferParameteriv( renderbuffer, pname, ptrParam );
        }
    }

    // ========================================================================

    public void CreateTextures( GLenum target, GLsizei n, GLuint* textures )
    {
        GetDelegateForFunction< PFNGLCREATETEXTURESPROC >( "glCreateTextures", out _glCreateTextures );

        _glCreateTextures( target, n, textures );
    }

    public GLuint[] CreateTextures( GLenum target, GLsizei n )
    {
        var textures = new GLuint[ n ];

        GetDelegateForFunction< PFNGLCREATETEXTURESPROC >( "glCreateTextures", out _glCreateTextures );

        fixed ( GLuint* ptrTextures = &textures[ 0 ] )
        {
            _glCreateTextures( target, n, ptrTextures );
        }

        return textures;
    }

    public GLuint CreateTexture( GLenum target )
    {
        return CreateTextures( target, 1 )[ 0 ];
    }

    public GLuint CreateTexture( TextureTarget target )
    {
        return CreateTextures( ( int )target, 1 )[ 0 ];
    }

    // ========================================================================

    public void TextureBuffer( GLuint texture, GLenum internalFormat, GLuint buffer )
    {
        GetDelegateForFunction< PFNGLTEXTUREBUFFERPROC >( "glTextureBuffer", out _glTextureBuffer );

        _glTextureBuffer( texture, internalFormat, buffer );
    }

    // ========================================================================

    public void TextureBufferRange( GLuint texture, GLenum internalFormat, GLuint buffer, GLintptr offset,
                                    GLsizeiptr size )
    {
        GetDelegateForFunction< PFNGLTEXTUREBUFFERRANGEPROC >( "glTextureBufferRange", out _glTextureBufferRange );

        _glTextureBufferRange( texture, internalFormat, buffer, offset, size );
    }

    // ========================================================================

    public void TextureStorage1D( GLuint texture, GLsizei levels, GLenum internalFormat, GLsizei width )
    {
        GetDelegateForFunction< PFNGLTEXTURESTORAGE1DPROC >( "glTextureStorage1D", out _glTextureStorage1D );

        _glTextureStorage1D( texture, levels, internalFormat, width );
    }

    // ========================================================================

    public void TextureStorage2D( GLuint texture, GLsizei levels, GLenum internalFormat, GLsizei width, GLsizei height )
    {
        GetDelegateForFunction< PFNGLTEXTURESTORAGE2DPROC >( "glTextureStorage2D", out _glTextureStorage2D );

        _glTextureStorage2D( texture, levels, internalFormat, width, height );
    }

    // ========================================================================

    public void TextureStorage3D( GLuint texture, GLsizei levels, GLenum internalFormat, GLsizei width, GLsizei height,
                                  GLsizei depth )
    {
        GetDelegateForFunction< PFNGLTEXTURESTORAGE3DPROC >( "glTextureStorage3D", out _glTextureStorage3D );

        _glTextureStorage3D( texture, levels, internalFormat, width, height, depth );
    }

    // ========================================================================

    public void TextureStorage2DMultisample( GLuint texture, GLsizei samples, GLenum internalFormat, GLsizei width,
                                             GLsizei height,
                                             GLbool fixedsamplelocations )
    {
        GetDelegateForFunction< PFNGLTEXTURESTORAGE2DMULTISAMPLEPROC >
            (
             "glTextureStorage2DMultisample",
             out _glTextureStorage2DMultisample
            );

        _glTextureStorage2DMultisample( texture, samples, internalFormat, width, height, fixedsamplelocations );
    }

    // ========================================================================

    public void TextureStorage3DMultisample( GLuint texture, GLsizei samples, GLenum internalFormat, GLsizei width,
                                             GLsizei height,
                                             GLsizei depth, GLbool fixedsamplelocations )
    {
        GetDelegateForFunction< PFNGLTEXTURESTORAGE3DMULTISAMPLEPROC >
            (
             "glTextureStorage3DMultisample",
             out _glTextureStorage3DMultisample
            );

        _glTextureStorage3DMultisample( texture, samples, internalFormat, width, height, depth, fixedsamplelocations );
    }

    // ========================================================================

    public void TextureSubImage1D( GLuint texture, GLint level, GLint xoffset, GLsizei width, GLenum format,
                                   GLenum type, IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLTEXTURESUBIMAGE1DPROC >( "glTextureSubImage1D", out _glTextureSubImage1D );

        _glTextureSubImage1D( texture, level, xoffset, width, format, type, pixels );
    }

    public void TextureSubImage1D< T >( GLuint texture, GLint level, GLint xoffset, GLsizei width, GLenum format,
                                        GLenum type, T[] pixels )
        where T : unmanaged
    {
        GetDelegateForFunction< PFNGLTEXTURESUBIMAGE1DPROC >( "glTextureSubImage1D", out _glTextureSubImage1D );

        fixed ( T* ptrPixels = &pixels[ 0 ] )
        {
            _glTextureSubImage1D( texture, level, xoffset, width, format, type, ( IntPtr )ptrPixels );
        }
    }

    // ========================================================================

    public void TextureSubImage2D( GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLsizei width,
                                   GLsizei height, GLenum format,
                                   GLenum type, IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLTEXTURESUBIMAGE2DPROC >( "glTextureSubImage2D", out _glTextureSubImage2D );

        _glTextureSubImage2D( texture, level, xoffset, yoffset, width, height, format, type, pixels );
    }

    public void TextureSubImage2D< T >( GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLsizei width,
                                        GLsizei height,
                                        GLenum format, GLenum type, T[] pixels ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLTEXTURESUBIMAGE2DPROC >( "glTextureSubImage2D", out _glTextureSubImage2D );

        fixed ( T* ptrPixels = &pixels[ 0 ] )
        {
            _glTextureSubImage2D( texture, level, xoffset, yoffset, width, height, format, type, ( IntPtr )ptrPixels );
        }
    }

    // ========================================================================

    public void TextureSubImage3D( GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLint zoffset,
                                   GLsizei width, GLsizei height,
                                   GLsizei depth, GLenum format, GLenum type, IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLTEXTURESUBIMAGE3DPROC >( "glTextureSubImage3D", out _glTextureSubImage3D );

        _glTextureSubImage3D( texture, level, xoffset, yoffset, zoffset, width, height, depth, format, type, pixels );
    }

    public void TextureSubImage3D< T >( GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLint zoffset,
                                        GLsizei width,
                                        GLsizei height, GLsizei depth, GLenum format, GLenum type,
                                        T[] pixels ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLTEXTURESUBIMAGE3DPROC >( "glTextureSubImage3D", out _glTextureSubImage3D );

        fixed ( T* ptrPixels = &pixels[ 0 ] )
        {
            _glTextureSubImage3D
                (
                 texture,
                 level,
                 xoffset,
                 yoffset,
                 zoffset,
                 width,
                 height,
                 depth,
                 format,
                 type,
                 ( IntPtr )ptrPixels
                );
        }
    }

    // ========================================================================

    public void CompressedTextureSubImage1D( GLuint texture, GLint level, GLint xoffset, GLsizei width,
                                             GLenum format, GLsizei imageSize, IntPtr data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXTURESUBIMAGE1DPROC >
            (
             "glCompressedTextureSubImage1D",
             out _glCompressedTextureSubImage1D
            );

        _glCompressedTextureSubImage1D( texture, level, xoffset, width, format, imageSize, data );
    }

    public void CompressedTextureSubImage1D( GLuint texture, GLint level, GLint xoffset, GLsizei width,
                                             GLenum format, GLsizei imageSize, byte[] data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXTURESUBIMAGE1DPROC >
            (
             "glCompressedTextureSubImage1D",
             out _glCompressedTextureSubImage1D
            );

        fixed ( byte* ptrData = &data[ 0 ] )
        {
            _glCompressedTextureSubImage1D( texture, level, xoffset, width, format, imageSize, ( IntPtr )ptrData );
        }
    }

    // ========================================================================

    public void CompressedTextureSubImage2D( GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLsizei width,
                                             GLsizei height,
                                             GLenum format, GLsizei imageSize, IntPtr data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXTURESUBIMAGE2DPROC >
            (
             "glCompressedTextureSubImage2D",
             out _glCompressedTextureSubImage2D
            );

        _glCompressedTextureSubImage2D( texture, level, xoffset, yoffset, width, height, format, imageSize, data );
    }

    public void CompressedTextureSubImage2D( GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLsizei width,
                                             GLsizei height,
                                             GLenum format, GLsizei imageSize, byte[] data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXTURESUBIMAGE2DPROC >
            (
             "glCompressedTextureSubImage2D",
             out _glCompressedTextureSubImage2D
            );

        fixed ( byte* ptrData = &data[ 0 ] )
        {
            _glCompressedTextureSubImage2D
                (
                 texture,
                 level,
                 xoffset,
                 yoffset,
                 width,
                 height,
                 format,
                 imageSize,
                 ( IntPtr )ptrData
                );
        }
    }

    // ========================================================================

    public void CompressedTextureSubImage3D( GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLint zoffset,
                                             GLsizei width,
                                             GLsizei height, GLsizei depth, GLenum format, GLsizei imageSize,
                                             IntPtr data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXTURESUBIMAGE3DPROC >
            (
             "glCompressedTextureSubImage3D",
             out _glCompressedTextureSubImage3D
            );

        _glCompressedTextureSubImage3D
            (
             texture,
             level,
             xoffset,
             yoffset,
             zoffset,
             width,
             height,
             depth,
             format,
             imageSize,
             data
            );
    }

    public void CompressedTextureSubImage3D( GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLint zoffset,
                                             GLsizei width,
                                             GLsizei height, GLsizei depth, GLenum format, GLsizei imageSize,
                                             byte[] data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXTURESUBIMAGE3DPROC >
            (
             "glCompressedTextureSubImage3D",
             out _glCompressedTextureSubImage3D
            );

        fixed ( void* ptrData = &data[ 0 ] )
        {
            var dataptr = ( IntPtr )ptrData;

            _glCompressedTextureSubImage3D
                (
                 texture,
                 level,
                 xoffset,
                 yoffset,
                 zoffset,
                 width,
                 height,
                 depth,
                 format,
                 imageSize,
                 dataptr
                );
        }
    }

    // ========================================================================

    public void CopyTextureSubImage1D( GLuint texture, GLint level, GLint xoffset, GLint x, GLint y, GLsizei width )
    {
        GetDelegateForFunction< PFNGLCOPYTEXTURESUBIMAGE1DPROC >
            (
             "glCopyTextureSubImage1D",
             out _glCopyTextureSubImage1D
            );

        _glCopyTextureSubImage1D( texture, level, xoffset, x, y, width );
    }

    // ========================================================================

    public void CopyTextureSubImage2D( GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLint x, GLint y,
                                       GLsizei width,
                                       GLsizei height )
    {
        GetDelegateForFunction< PFNGLCOPYTEXTURESUBIMAGE2DPROC >
            (
             "glCopyTextureSubImage2D",
             out _glCopyTextureSubImage2D
            );

        _glCopyTextureSubImage2D( texture, level, xoffset, yoffset, x, y, width, height );
    }

    // ========================================================================

    public void CopyTextureSubImage3D( GLuint texture, GLint level, GLint xoffset, GLint yoffset,
                                       GLint zoffset, GLint x, GLint y, GLsizei width, GLsizei height )
    {
        GetDelegateForFunction< PFNGLCOPYTEXTURESUBIMAGE3DPROC >
            (
             "glCopyTextureSubImage3D",
             out _glCopyTextureSubImage3D
            );

        _glCopyTextureSubImage3D( texture, level, xoffset, yoffset, zoffset, x, y, width, height );
    }

    // ========================================================================

    public void TextureParameterf( GLuint texture, GLenum pname, GLfloat param )
    {
        GetDelegateForFunction< PFNGLTEXTUREPARAMETERFPROC >( "glTextureParameterf", out _glTextureParameterf );

        _glTextureParameterf( texture, pname, param );
    }

    // ========================================================================

    public void TextureParameterfv( GLuint texture, GLenum pname, GLfloat* param )
    {
        GetDelegateForFunction< PFNGLTEXTUREPARAMETERFVPROC >( "glTextureParameterfv", out _glTextureParameterfv );

        _glTextureParameterfv( texture, pname, param );
    }

    public void TextureParameterfv( GLuint texture, GLenum pname, GLfloat[] param )
    {
        GetDelegateForFunction< PFNGLTEXTUREPARAMETERFVPROC >( "glTextureParameterfv", out _glTextureParameterfv );

        fixed ( GLfloat* ptrParam = &param[ 0 ] )
        {
            _glTextureParameterfv( texture, pname, ptrParam );
        }
    }

    // ========================================================================

    public void TextureParameteri( GLuint texture, GLenum pname, GLint param )
    {
        GetDelegateForFunction< PFNGLTEXTUREPARAMETERIPROC >( "glTextureParameteri", out _glTextureParameteri );

        _glTextureParameteri( texture, pname, param );
    }

    // ========================================================================

    public void TextureParameterIiv( GLuint texture, GLenum pname, GLint* param )
    {
        GetDelegateForFunction< PFNGLTEXTUREPARAMETERIIVPROC >( "glTextureParameterIiv", out _glTextureParameterIiv );

        _glTextureParameterIiv( texture, pname, param );
    }

    public void TextureParameterIiv( GLuint texture, GLenum pname, GLint[] param )
    {
        GetDelegateForFunction< PFNGLTEXTUREPARAMETERIIVPROC >( "glTextureParameterIiv", out _glTextureParameterIiv );

        fixed ( GLint* ptrParam = &param[ 0 ] )
        {
            _glTextureParameterIiv( texture, pname, ptrParam );
        }
    }

    // ========================================================================

    public void TextureParameterIuiv( GLuint texture, GLenum pname, GLuint* param )
    {
        GetDelegateForFunction< PFNGLTEXTUREPARAMETERIUIVPROC >
            (
             "glTextureParameterIuiv",
             out _glTextureParameterIuiv
            );

        _glTextureParameterIuiv( texture, pname, param );
    }

    public void TextureParameterIuiv( GLuint texture, GLenum pname, GLuint[] param )
    {
        GetDelegateForFunction< PFNGLTEXTUREPARAMETERIUIVPROC >
            (
             "glTextureParameterIuiv",
             out _glTextureParameterIuiv
            );

        fixed ( GLuint* ptrParam = &param[ 0 ] )
        {
            _glTextureParameterIuiv( texture, pname, ptrParam );
        }
    }

    // ========================================================================

    public void TextureParameteriv( GLuint texture, GLenum pname, GLint* param )
    {
        GetDelegateForFunction< PFNGLTEXTUREPARAMETERIVPROC >( "glTextureParameteriv", out _glTextureParameteriv );

        _glTextureParameteriv( texture, pname, param );
    }

    public void TextureParameteriv( GLuint texture, GLenum pname, GLint[] param )
    {
        GetDelegateForFunction< PFNGLTEXTUREPARAMETERIVPROC >( "glTextureParameteriv", out _glTextureParameteriv );

        fixed ( GLint* ptrParam = &param[ 0 ] )
        {
            _glTextureParameteriv( texture, pname, ptrParam );
        }
    }

    // ========================================================================

    public void GenerateTextureMipmap( GLuint texture )
    {
        GetDelegateForFunction< PFNGLGENERATETEXTUREMIPMAPPROC >
            (
             "glGenerateTextureMipmap",
             out _glGenerateTextureMipmap
            );

        _glGenerateTextureMipmap( texture );
    }

    // ========================================================================

    public void BindTextureUnit( GLuint unit, GLuint texture )
    {
        GetDelegateForFunction< PFNGLBINDTEXTUREUNITPROC >( "glBindTextureUnit", out _glBindTextureUnit );

        _glBindTextureUnit( unit, texture );
    }

    // ========================================================================

    public void GetTextureImage( GLuint texture, GLint level, GLenum format, GLenum type, GLsizei bufSize,
                                 IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLGETTEXTUREIMAGEPROC >( "glGetTextureImage", out _glGetTextureImage );

        _glGetTextureImage( texture, level, format, type, bufSize, pixels );
    }

    public T[] GetTextureImage< T >( GLuint texture, GLint level, GLenum format, GLenum type, GLsizei bufSize )
        where T : unmanaged
    {
        var pixels = new T[ bufSize ];

        GetDelegateForFunction< PFNGLGETTEXTUREIMAGEPROC >( "glGetTextureImage", out _glGetTextureImage );

        fixed ( T* ptrPixels = &pixels[ 0 ] )
        {
            var dataptr = new IntPtr( ptrPixels );

            _glGetTextureImage( texture, level, format, type, bufSize, dataptr );
        }

        return pixels;
    }

    // ========================================================================

    public void GetCompressedTextureImage( GLuint texture, GLint level, GLsizei bufSize, IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLGETCOMPRESSEDTEXTUREIMAGEPROC >
            (
             "glGetCompressedTextureImage",
             out _glGetCompressedTextureImage
            );

        _glGetCompressedTextureImage( texture, level, bufSize, pixels );
    }

    public byte[] GetCompressedTextureImage( GLuint texture, GLint level, GLsizei bufSize )
    {
        var pixels = new byte[ bufSize ];

        GetDelegateForFunction< PFNGLGETCOMPRESSEDTEXTUREIMAGEPROC >
            (
             "glGetCompressedTextureImage",
             out _glGetCompressedTextureImage
            );

        fixed ( byte* ptrPixels = &pixels[ 0 ] )
        {
            _glGetCompressedTextureImage( texture, level, bufSize, ( IntPtr )ptrPixels );
        }

        return pixels;
    }

    // ========================================================================

    public void GetTextureLevelParameterfv( GLuint texture, GLint level, GLenum pname, GLfloat* param )
    {
        GetDelegateForFunction< PFNGLGETTEXTURELEVELPARAMETERFVPROC >
            (
             "glGetTextureLevelParameterfv",
             out _glGetTextureLevelParameterfv
            );

        _glGetTextureLevelParameterfv( texture, level, pname, param );
    }

    public void GetTextureLevelParameterfv( GLuint texture, GLint level, GLenum pname, ref GLfloat[] param )
    {
        GetDelegateForFunction< PFNGLGETTEXTURELEVELPARAMETERFVPROC >
            (
             "glGetTextureLevelParameterfv",
             out _glGetTextureLevelParameterfv
            );

        fixed ( GLfloat* ptrParam = &param[ 0 ] )
        {
            _glGetTextureLevelParameterfv( texture, level, pname, ptrParam );
        }
    }

    // ========================================================================

    public void GetTextureLevelParameteriv( GLuint texture, GLint level, GLenum pname, GLint* param )
    {
        GetDelegateForFunction< PFNGLGETTEXTURELEVELPARAMETERIVPROC >
            (
             "glGetTextureLevelParameteriv",
             out _glGetTextureLevelParameteriv
            );

        _glGetTextureLevelParameteriv( texture, level, pname, param );
    }

    public void GetTextureLevelParameteriv( GLuint texture, GLint level, GLenum pname, ref GLint[] param )
    {
        GetDelegateForFunction< PFNGLGETTEXTURELEVELPARAMETERIVPROC >
            (
             "glGetTextureLevelParameteriv",
             out _glGetTextureLevelParameteriv
            );

        fixed ( GLint* ptrParam = &param[ 0 ] )
        {
            _glGetTextureLevelParameteriv( texture, level, pname, ptrParam );
        }
    }

    // ========================================================================

    public void GetTextureParameterfv( GLuint texture, GLenum pname, GLfloat* param )
    {
        GetDelegateForFunction< PFNGLGETTEXTUREPARAMETERFVPROC >
            (
             "glGetTextureParameterfv",
             out _glGetTextureParameterfv
            );

        _glGetTextureParameterfv( texture, pname, param );
    }

    public void GetTextureParameterfv( GLuint texture, GLenum pname, ref GLfloat[] param )
    {
        GetDelegateForFunction< PFNGLGETTEXTUREPARAMETERFVPROC >
            (
             "glGetTextureParameterfv",
             out _glGetTextureParameterfv
            );

        fixed ( GLfloat* ptrParam = &param[ 0 ] )
        {
            _glGetTextureParameterfv( texture, pname, ptrParam );
        }
    }

    // ========================================================================

    public void GetTextureParameterIiv( GLuint texture, GLenum pname, GLint* param )
    {
        GetDelegateForFunction< PFNGLGETTEXTUREPARAMETERIIVPROC >
            (
             "glGetTextureParameterIiv",
             out _glGetTextureParameterIiv
            );

        _glGetTextureParameterIiv( texture, pname, param );
    }

    public void GetTextureParameterIiv( GLuint texture, GLenum pname, ref GLint[] param )
    {
        GetDelegateForFunction< PFNGLGETTEXTUREPARAMETERIIVPROC >
            (
             "glGetTextureParameterIiv",
             out _glGetTextureParameterIiv
            );

        fixed ( GLint* ptrParam = &param[ 0 ] )
        {
            _glGetTextureParameterIiv( texture, pname, ptrParam );
        }
    }

    // ========================================================================

    public void GetTextureParameterIuiv( GLuint texture, GLenum pname, GLuint* param )
    {
        GetDelegateForFunction< PFNGLGETTEXTUREPARAMETERIUIVPROC >
            (
             "glGetTextureParameterIuiv",
             out _glGetTextureParameterIuiv
            );

        _glGetTextureParameterIuiv( texture, pname, param );
    }

    public void GetTextureParameterIuiv( GLuint texture, GLenum pname, ref GLuint[] param )
    {
        GetDelegateForFunction< PFNGLGETTEXTUREPARAMETERIUIVPROC >
            (
             "glGetTextureParameterIuiv",
             out _glGetTextureParameterIuiv
            );

        fixed ( GLuint* ptrParam = &param[ 0 ] )
        {
            _glGetTextureParameterIuiv( texture, pname, ptrParam );
        }
    }

    // ========================================================================

    public void GetTextureParameteriv( GLuint texture, GLenum pname, GLint* param )
    {
        GetDelegateForFunction< PFNGLGETTEXTUREPARAMETERIVPROC >
            (
             "glGetTextureParameteriv",
             out _glGetTextureParameteriv
            );

        _glGetTextureParameteriv( texture, pname, param );
    }

    public void GetTextureParameteriv( GLuint texture, GLenum pname, ref GLint[] param )
    {
        GetDelegateForFunction< PFNGLGETTEXTUREPARAMETERIVPROC >
            (
             "glGetTextureParameteriv",
             out _glGetTextureParameteriv
            );

        fixed ( GLint* ptrParam = &param[ 0 ] )
        {
            _glGetTextureParameteriv( texture, pname, ptrParam );
        }
    }

    // ========================================================================

    public void MemoryBarrierByRegion( GLbitfield barriers )
    {
        GetDelegateForFunction< PFNGLMEMORYBARRIERBYREGIONPROC >
            (
             "glMemoryBarrierByRegion",
             out _glMemoryBarrierByRegion
            );

        _glMemoryBarrierByRegion( barriers );
    }

    // ========================================================================

    public void GetTextureSubImage( GLuint texture, GLint level, GLint xoffset, GLint yoffset,
                                    GLint zoffset, GLsizei width, GLsizei height, GLsizei depth,
                                    GLenum format, GLenum type, GLsizei bufSize, IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLGETTEXTURESUBIMAGEPROC >( "glGetTextureSubImage", out _glGetTextureSubImage );

        _glGetTextureSubImage
            (
             texture,
             level,
             xoffset,
             yoffset,
             zoffset,
             width,
             height,
             depth,
             format,
             type,
             bufSize,
             pixels
            );
    }

    public byte[] GetTextureSubImage( GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLint zoffset,
                                      GLsizei width, GLsizei height, GLsizei depth, GLenum format, GLenum type,
                                      GLsizei bufSize )
    {
        var pixels = new byte[ bufSize ];

        GetDelegateForFunction< PFNGLGETTEXTURESUBIMAGEPROC >( "glGetTextureSubImage", out _glGetTextureSubImage );

        fixed ( void* ptrPixels = &pixels[ 0 ] )
        {
            var dataptr = new IntPtr( ptrPixels );

            _glGetTextureSubImage
                (
                 texture,
                 level,
                 xoffset,
                 yoffset,
                 zoffset,
                 width,
                 height,
                 depth,
                 format,
                 type,
                 bufSize,
                 dataptr
                );
        }

        return pixels;
    }

    // ========================================================================

    public void GetCompressedTextureSubImage( GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLint zoffset,
                                              GLsizei width,
                                              GLsizei height, GLsizei depth, GLsizei bufSize, IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLGETCOMPRESSEDTEXTURESUBIMAGEPROC >
            (
             "glGetCompressedTextureSubImage",
             out _glGetCompressedTextureSubImage
            );

        _glGetCompressedTextureSubImage
            (
             texture,
             level,
             xoffset,
             yoffset,
             zoffset,
             width,
             height,
             depth,
             bufSize,
             pixels
            );
    }

    public byte[] GetCompressedTextureSubImage( GLuint texture, GLint level, GLint xoffset, GLint yoffset,
                                                GLint zoffset, GLsizei width,
                                                GLsizei height, GLsizei depth, GLsizei bufSize )
    {
        var pixels = new byte[ bufSize ];

        GetDelegateForFunction< PFNGLGETCOMPRESSEDTEXTURESUBIMAGEPROC >
            (
             "glGetCompressedTextureSubImage",
             out _glGetCompressedTextureSubImage
            );

        fixed ( void* ptrPixels = &pixels[ 0 ] )
        {
            var dataptr = new IntPtr( ptrPixels );

            _glGetCompressedTextureSubImage
                (
                 texture,
                 level,
                 xoffset,
                 yoffset,
                 zoffset,
                 width,
                 height,
                 depth,
                 bufSize,
                 dataptr
                );
        }

        return pixels;
    }

    // ========================================================================

    public GLenum GetGraphicsResetStatus()
    {
        GetDelegateForFunction< PFNGLGETGRAPHICSRESETSTATUSPROC >
            (
             "glGetGraphicsResetStatus",
             out _glGetGraphicsResetStatus
            );

        return _glGetGraphicsResetStatus();
    }

    // ========================================================================

    public void GetnCompressedTexImage( GLenum target, GLint lod, GLsizei bufSize, IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLGETNCOMPRESSEDTEXIMAGEPROC >
            (
             "glGetnCompressedTexImage",
             out _glGetnCompressedTexImage
            );

        _glGetnCompressedTexImage( target, lod, bufSize, pixels );
    }

    public byte[] GetnCompressedTexImage( GLenum target, GLint lod, GLsizei bufSize )
    {
        var pixels = new byte[ bufSize ];

        GetDelegateForFunction< PFNGLGETNCOMPRESSEDTEXIMAGEPROC >
            (
             "glGetnCompressedTexImage",
             out _glGetnCompressedTexImage
            );

        fixed ( void* ptrPixels = &pixels[ 0 ] )
        {
            var dataptr = new IntPtr( ptrPixels );

            _glGetnCompressedTexImage( target, lod, bufSize, dataptr );
        }

        return pixels;
    }

    // ========================================================================

    public void GetnTexImage( GLenum target, GLint level, GLenum format, GLenum type, GLsizei bufSize, IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLGETNTEXIMAGEPROC >( "glGetnTexImage", out _glGetnTexImage );

        _glGetnTexImage( target, level, format, type, bufSize, pixels );
    }

    public byte[] GetnTexImage( GLenum target, GLint level, GLenum format, GLenum type, GLsizei bufSize )
    {
        var pixels = new byte[ bufSize ];

        GetDelegateForFunction< PFNGLGETNTEXIMAGEPROC >( "glGetnTexImage", out _glGetnTexImage );

        fixed ( void* ptrPixels = &pixels[ 0 ] )
        {
            var dataptr = new IntPtr( ptrPixels );

            _glGetnTexImage( target, level, format, type, bufSize, dataptr );
        }

        return pixels;
    }

    // ========================================================================

    public void ReadnPixels( GLint x, GLint y, GLsizei width, GLsizei height, GLenum format, GLenum type,
                             GLsizei bufSize, IntPtr data )
    {
        GetDelegateForFunction< PFNGLREADNPIXELSPROC >( "glReadnPixels", out _glReadnPixels );

        _glReadnPixels( x, y, width, height, format, type, bufSize, data );
    }

    public byte[] ReadnPixels( GLint x, GLint y, GLsizei width, GLsizei height, GLenum format, GLenum type,
                               GLsizei bufSize )
    {
        var data = new byte[ bufSize ];

        GetDelegateForFunction< PFNGLREADNPIXELSPROC >( "glReadnPixels", out _glReadnPixels );

        fixed ( void* ptrData = &data[ 0 ] )
        {
            var dataptr = new IntPtr( ptrData );

            _glReadnPixels( x, y, width, height, format, type, bufSize, dataptr );
        }

        return data;
    }

    // ========================================================================

    /// <inheritdoc />
    public void TextureBarrier()
    {
        GetDelegateForFunction< PFNGLTEXTUREBARRIERPROC >( "glTextureBarrier", out _glTextureBarrier );

        _glTextureBarrier();
    }

    // ========================================================================

    public void SpecializeShader( GLuint shader, GLchar* pEntryPoint, GLuint numSpecializationConstants,
                                  GLuint* pConstantIndex,
                                  GLuint* pConstantValue )
    {
        GetDelegateForFunction< PFNGLSPECIALIZESHADERPROC >( "glSpecializeShader", out _glSpecializeShader );

        _glSpecializeShader( shader, pEntryPoint, numSpecializationConstants, pConstantIndex, pConstantValue );
    }

    public void SpecializeShader( GLuint shader, string pEntryPoint, GLuint numSpecializationConstants,
                                  GLuint[] pConstantIndex,
                                  GLuint[] pConstantValue )
    {
        byte[] pEntryPointBytes = Encoding.UTF8.GetBytes( pEntryPoint );

        GetDelegateForFunction< PFNGLSPECIALIZESHADERPROC >( "glSpecializeShader", out _glSpecializeShader );

        fixed ( GLchar* ptrPEntryPoint = &pEntryPointBytes[ 0 ] )
        {
            fixed ( GLuint* ptrPConstantIndex = &pConstantIndex[ 0 ] )
            {
                fixed ( GLuint* ptrPConstantValue = &pConstantValue[ 0 ] )
                {
                    _glSpecializeShader
                        (
                         shader,
                         ptrPEntryPoint,
                         numSpecializationConstants,
                         ptrPConstantIndex,
                         ptrPConstantValue
                        );
                }
            }
        }
    }

    // ========================================================================

    public void MultiDrawArraysIndirectCount( GLenum mode, IntPtr indirect, GLintptr drawcount, GLsizei maxdrawcount,
                                              GLsizei stride )
    {
        GetDelegateForFunction< PFNGLMULTIDRAWARRAYSINDIRECTCOUNTPROC >
            (
             "glMultiDrawArraysIndirectCount",
             out _glMultiDrawArraysIndirectCount
            );

        _glMultiDrawArraysIndirectCount( mode, indirect, drawcount, maxdrawcount, stride );
    }

    // ========================================================================

    public void MultiDrawElementsIndirectCount( GLenum mode, GLenum type, IntPtr indirect, GLintptr drawcount,
                                                GLsizei maxdrawcount,
                                                GLsizei stride )
    {
        GetDelegateForFunction< PFNGLMULTIDRAWELEMENTSINDIRECTCOUNTPROC >
            (
             "glMultiDrawElementsIndirectCount",
             out _glMultiDrawElementsIndirectCount
            );

        _glMultiDrawElementsIndirectCount( mode, type, indirect, drawcount, maxdrawcount, stride );
    }

    // ========================================================================

    public void PolygonOffsetClamp( GLfloat factor, GLfloat units, GLfloat clamp )
    {
        GetDelegateForFunction< PFNGLPOLYGONOFFSETCLAMPPROC >( "glPolygonOffsetClamp", out _glPolygonOffsetClamp );

        _glPolygonOffsetClamp( factor, units, clamp );
    }

    // ========================================================================

    public void ClampColor( GLenum target, GLenum clamp )
    {
        GetDelegateForFunction< PFNGLCLAMPCOLORPROC >( "glClampColor", out _glClampColor );

        _glClampColor( target, clamp );
    }

    // ========================================================================

    public void BeginConditionalRender( GLuint id, GLenum mode )
    {
        GetDelegateForFunction< PFNGLBEGINCONDITIONALRENDERPROC >
            (
             "glBeginConditionalRender",
             out _glBeginConditionalRender
            );

        _glBeginConditionalRender( id, mode );
    }

    // ========================================================================

    public void EndConditionalRender()
    {
        GetDelegateForFunction< PFNGLENDCONDITIONALRENDERPROC >
            (
             "glEndConditionalRender",
             out _glEndConditionalRender
            );

        _glEndConditionalRender();
    }

    // ========================================================================

    public void GetUniformuiv( GLuint program, GLint location, GLuint* parameters )
    {
        GetDelegateForFunction< PFNGLGETUNIFORMUIVPROC >( "glGetUniformuiv", out _glGetUniformuiv );

        _glGetUniformuiv( program, location, parameters );
    }

    public void GetUniformuiv( GLuint program, GLint location, ref GLuint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETUNIFORMUIVPROC >( "glGetUniformuiv", out _glGetUniformuiv );

        fixed ( GLuint* p = &parameters[ 0 ] )
        {
            _glGetUniformuiv( program, location, p );
        }
    }

    // ========================================================================

    public void BindFragDataLocation( GLuint program, GLuint color, GLchar* name )
    {
        GetDelegateForFunction< PFNGLBINDFRAGDATALOCATIONPROC >
            (
             "glBindFragDataLocation",
             out _glBindFragDataLocation
            );

        _glBindFragDataLocation( program, color, name );
    }

    public void BindFragDataLocation( GLuint program, GLuint color, string name )
    {
        byte[] narr = Encoding.UTF8.GetBytes( name );

        GetDelegateForFunction< PFNGLBINDFRAGDATALOCATIONPROC >
            (
             "glBindFragDataLocation",
             out _glBindFragDataLocation
            );

        fixed ( GLchar* p = &narr[ 0 ] )
        {
            _glBindFragDataLocation( program, color, p );
        }
    }

    // ========================================================================

    public GLint GetFragDataLocation( GLuint program, GLchar* name )
    {
        GetDelegateForFunction< PFNGLGETFRAGDATALOCATIONPROC >( "glGetFragDataLocation", out _glGetFragDataLocation );

        return _glGetFragDataLocation( program, name );
    }

    public GLint GetFragDataLocation( GLuint program, string name )
    {
        byte[] narr = Encoding.UTF8.GetBytes( name );

        GetDelegateForFunction< PFNGLGETFRAGDATALOCATIONPROC >( "glGetFragDataLocation", out _glGetFragDataLocation );

        fixed ( GLchar* p = &narr[ 0 ] )
        {
            return _glGetFragDataLocation( program, p );
        }
    }

    // ========================================================================

    public void Uniform1UI( GLint location, GLuint v0 )
    {
        GetDelegateForFunction< PFNGLUNIFORM1UIPROC >( "glUniform1ui", out _glUniform1ui );

        _glUniform1ui( location, v0 );
    }

    // ========================================================================

    public void Uniform2UI( GLint location, GLuint v0, GLuint v1 )
    {
        GetDelegateForFunction< PFNGLUNIFORM2UIPROC >( "glUniform2ui", out _glUniform2ui );

        _glUniform2ui( location, v0, v1 );
    }

    // ========================================================================

    public void Uniform3UI( GLint location, GLuint v0, GLuint v1, GLuint v2 )
    {
        GetDelegateForFunction< PFNGLUNIFORM3UIPROC >( "glUniform3ui", out _glUniform3ui );

        _glUniform3ui( location, v0, v1, v2 );
    }

    // ========================================================================

    public void Uniform4UI( GLint location, GLuint v0, GLuint v1, GLuint v2, GLuint v3 )
    {
        GetDelegateForFunction< PFNGLUNIFORM4UIPROC >( "glUniform4ui", out _glUniform4ui );

        _glUniform4ui( location, v0, v1, v2, v3 );
    }

    // ========================================================================

    public void Uniform1Uiv( GLint location, GLsizei count, GLuint* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM1UIVPROC >( "glUniform1uiv", out _glUniform1uiv );

        _glUniform1uiv( location, count, value );
    }

    public void Uniform1Uiv( GLint location, GLuint[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM1UIVPROC >( "glUniform1uiv", out _glUniform1uiv );

        fixed ( GLuint* p = &value[ 0 ] )
        {
            _glUniform1uiv( location, value.Length, p );
        }
    }

    // ========================================================================

    public void Uniform2Uiv( GLint location, GLsizei count, GLuint* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM2UIVPROC >( "glUniform2uiv", out _glUniform2uiv );

        _glUniform2uiv( location, count, value );
    }

    public void Uniform2Uiv( GLint location, GLuint[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM2UIVPROC >( "glUniform2uiv", out _glUniform2uiv );

        fixed ( GLuint* p = &value[ 0 ] )
        {
            _glUniform2uiv( location, value.Length / 2, p );
        }
    }

    // ========================================================================

    public void Uniform3Uiv( GLint location, GLsizei count, GLuint* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM3UIVPROC >( "glUniform3uiv", out _glUniform3uiv );

        _glUniform3uiv( location, count, value );
    }

    public void Uniform3Uiv( GLint location, GLuint[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM3UIVPROC >( "glUniform3uiv", out _glUniform3uiv );

        fixed ( GLuint* p = &value[ 0 ] )
        {
            _glUniform3uiv( location, value.Length / 3, p );
        }
    }

    // ========================================================================

    public void Uniform4Uiv( GLint location, GLsizei count, GLuint* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM4UIVPROC >( "glUniform4uiv", out _glUniform4uiv );

        _glUniform4uiv( location, count, value );
    }

    public void Uniform4Uiv( GLint location, GLuint[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM4UIVPROC >( "glUniform4uiv", out _glUniform4uiv );

        fixed ( GLuint* p = &value[ 0 ] )
        {
            _glUniform4uiv( location, value.Length / 4, p );
        }
    }

    // ========================================================================

    public void TexParameterIiv( GLenum target, GLenum pname, GLint* param )
    {
        GetDelegateForFunction< PFNGLTEXPARAMETERIIVPROC >( "glTexParameterIiv", out _glTexParameterIiv );

        _glTexParameterIiv( target, pname, param );
    }

    public void TexParameterIiv( GLenum target, GLenum pname, GLint[] param )
    {
        GetDelegateForFunction< PFNGLTEXPARAMETERIIVPROC >( "glTexParameterIiv", out _glTexParameterIiv );

        fixed ( GLint* p = &param[ 0 ] )
        {
            _glTexParameterIiv( target, pname, p );
        }
    }

    // ========================================================================

    public void TexParameterIuiv( GLenum target, GLenum pname, GLuint* param )
    {
        GetDelegateForFunction< PFNGLTEXPARAMETERIUIVPROC >( "glTexParameterIuiv", out _glTexParameterIuiv );

        _glTexParameterIuiv( target, pname, param );
    }

    public void TexParameterIuiv( GLenum target, GLenum pname, GLuint[] param )
    {
        GetDelegateForFunction< PFNGLTEXPARAMETERIUIVPROC >( "glTexParameterIuiv", out _glTexParameterIuiv );

        fixed ( GLuint* p = &param[ 0 ] )
        {
            _glTexParameterIuiv( target, pname, p );
        }
    }

    // ========================================================================

    public void GetTexParameterIiv( GLenum target, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETTEXPARAMETERIIVPROC >( "glGetTexParameterIiv", out _glGetTexParameterIiv );

        _glGetTexParameterIiv( target, pname, parameters );
    }

    public void GetTexParameterIiv( GLenum target, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETTEXPARAMETERIIVPROC >( "glGetTexParameterIiv", out _glGetTexParameterIiv );

        fixed ( GLint* p = &parameters[ 0 ] )
        {
            _glGetTexParameterIiv( target, pname, p );
        }
    }

    // ========================================================================

    public void GetTexParameterIuiv( GLenum target, GLenum pname, GLuint* parameters )
    {
        GetDelegateForFunction< PFNGLGETTEXPARAMETERIUIVPROC >( "glGetTexParameterIuiv", out _glGetTexParameterIuiv );

        _glGetTexParameterIuiv( target, pname, parameters );
    }

    public void GetTexParameterIuiv( GLenum target, GLenum pname, ref GLuint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETTEXPARAMETERIUIVPROC >( "glGetTexParameterIuiv", out _glGetTexParameterIuiv );

        fixed ( GLuint* p = &parameters[ 0 ] )
        {
            _glGetTexParameterIuiv( target, pname, p );
        }
    }

    // ========================================================================

    public GLubyte* GetStringi( GLenum name, GLuint index )
    {
        GetDelegateForFunction< PFNGLGETSTRINGIPROC >( "glGetStringi", out _glGetStringi );

        return _glGetStringi( name, index );
    }

    public string GetStringiSafe( GLenum name, GLuint index )
    {
        GetDelegateForFunction< PFNGLGETSTRINGIPROC >( "glGetStringi", out _glGetStringi );

        byte* ptr = _glGetStringi( name, index );

        if ( ptr == null )
        {
            return string.Empty;
        }

        var i = 0;

        while ( ptr[ i ] != 0 )
        {
            i++;
        }

        return new string( ( GLbyte* )ptr, 0, i, Encoding.UTF8 );
    }

    // ========================================================================

    public GLbool IsRenderbuffer( GLuint renderbuffer )
    {
        GetDelegateForFunction< PFNGLISRENDERBUFFERPROC >( "glIsRenderbuffer", out _glIsRenderbuffer );

        return _glIsRenderbuffer( renderbuffer );
    }

    // ========================================================================

    public void BindRenderbuffer( GLenum target, GLuint renderbuffer )
    {
        GetDelegateForFunction< PFNGLBINDRENDERBUFFERPROC >( "glBindRenderbuffer", out _glBindRenderbuffer );

        _glBindRenderbuffer( target, renderbuffer );
    }

    // ========================================================================

    public void DeleteRenderbuffers( GLsizei n, GLuint* renderbuffers )
    {
        GetDelegateForFunction< PFNGLDELETERENDERBUFFERSPROC >( "glDeleteRenderbuffers", out _glDeleteRenderbuffers );

        _glDeleteRenderbuffers( n, renderbuffers );
    }

    public void DeleteRenderbuffers( params GLuint[] renderbuffers )
    {
        GetDelegateForFunction< PFNGLDELETERENDERBUFFERSPROC >( "glDeleteRenderbuffers", out _glDeleteRenderbuffers );

        fixed ( GLuint* p = &renderbuffers[ 0 ] )
        {
            _glDeleteRenderbuffers( renderbuffers.Length, p );
        }
    }

    // ========================================================================

    public void GenRenderbuffers( GLsizei n, GLuint* renderbuffers )
    {
        GetDelegateForFunction< PFNGLGENRENDERBUFFERSPROC >( "glGenRenderbuffers", out _glGenRenderbuffers );

        _glGenRenderbuffers( n, renderbuffers );
    }

    public GLuint[] GenRenderbuffers( GLsizei n )
    {
        var renderbuffers = new GLuint[ n ];

        GetDelegateForFunction< PFNGLGENRENDERBUFFERSPROC >( "glGenRenderbuffers", out _glGenRenderbuffers );

        fixed ( GLuint* p = &renderbuffers[ 0 ] )
        {
            _glGenRenderbuffers( n, p );
        }

        return renderbuffers;
    }

    public GLuint GenRenderbuffer()
    {
        return GenRenderbuffers( 1 )[ 0 ];
    }

    // ========================================================================

    public void RenderbufferStorage( GLenum target, GLenum internalFormat, GLsizei width, GLsizei height )
    {
        GetDelegateForFunction< PFNGLRENDERBUFFERSTORAGEPROC >( "glRenderbufferStorage", out _glRenderbufferStorage );

        _glRenderbufferStorage( target, internalFormat, width, height );
    }

    // ========================================================================

    public void GetRenderbufferParameteriv( GLenum target, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETRENDERBUFFERPARAMETERIVPROC >
            (
             "glGetRenderbufferParameteriv",
             out _glGetRenderbufferParameteriv
            );

        _glGetRenderbufferParameteriv( target, pname, parameters );
    }

    public void GetRenderbufferParameteriv( GLenum target, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETRENDERBUFFERPARAMETERIVPROC >
            (
             "glGetRenderbufferParameteriv",
             out _glGetRenderbufferParameteriv
            );

        fixed ( GLint* p = &parameters[ 0 ] )
        {
            _glGetRenderbufferParameteriv( target, pname, p );
        }
    }

    // ========================================================================

    public GLbool IsFramebuffer( GLuint framebuffer )
    {
        GetDelegateForFunction< PFNGLISFRAMEBUFFERPROC >( "glIsFramebuffer", out _glIsFramebuffer );

        return _glIsFramebuffer( framebuffer );
    }

    // ========================================================================

    public void BindFramebuffer( GLenum target, GLuint framebuffer )
    {
        if ( framebuffer == LughGL.CurrentBoundFbo )
        {
            Stats.IncMeter( "BindFramebuffer" );

            return;
        }

        GetDelegateForFunction< PFNGLBINDFRAMEBUFFERPROC >( "glBindFramebuffer", out _glBindFramebuffer );

        _glBindFramebuffer( target, framebuffer );

        Engine.GL.GetIntegerv( ( int )BufferBinding.FramebufferBinding, out int binding );
        LughGL.CurrentBoundFbo = ( uint )binding;
    }

    // ========================================================================

    public void DeleteFramebuffers( GLsizei n, GLuint* framebuffers )
    {
        GetDelegateForFunction< PFNGLDELETEFRAMEBUFFERSPROC >( "glDeleteFramebuffers", out _glDeleteFramebuffers );

        _glDeleteFramebuffers( n, framebuffers );
    }

    public void DeleteFramebuffers( params GLuint[] framebuffers )
    {
        GetDelegateForFunction< PFNGLDELETEFRAMEBUFFERSPROC >( "glDeleteFramebuffers", out _glDeleteFramebuffers );

        fixed ( GLuint* p = &framebuffers[ 0 ] )
        {
            _glDeleteFramebuffers( framebuffers.Length, p );
        }
    }

    // ========================================================================

    public void GenFramebuffers( GLsizei n, GLuint* framebuffers )
    {
        GetDelegateForFunction< PFNGLGENFRAMEBUFFERSPROC >( "glGenFramebuffers", out _glGenFramebuffers );

        _glGenFramebuffers( n, framebuffers );
    }

    public GLuint[] GenFramebuffers( GLsizei n )
    {
        GetDelegateForFunction< PFNGLGENFRAMEBUFFERSPROC >( "glGenFramebuffers", out _glGenFramebuffers );

        var framebuffers = new GLuint[ n ];

        fixed ( GLuint* p = &framebuffers[ 0 ] )
        {
            _glGenFramebuffers( n, p );
        }

        return framebuffers;
    }

    public GLuint GenFramebuffer()
    {
        return GenFramebuffers( 1 )[ 0 ];
    }

    // ========================================================================

    public GLenum CheckFramebufferStatus( GLenum target )
    {
        GetDelegateForFunction< PFNGLCHECKFRAMEBUFFERSTATUSPROC >
            (
             "glCheckFramebufferStatus",
             out _glCheckFramebufferStatus
            );

        return _glCheckFramebufferStatus( target );
    }

    // ========================================================================

    public void FramebufferTexture1D( GLenum target, GLenum attachment, GLenum textarget, GLuint texture, GLint level )
    {
        GetDelegateForFunction< PFNGLFRAMEBUFFERTEXTURE1DPROC >
            (
             "glFramebufferTexture1D",
             out _glFramebufferTexture1D
            );

        _glFramebufferTexture1D( target, attachment, textarget, texture, level );
    }

    // ========================================================================

    public void FramebufferTexture2D( GLenum target, GLenum attachment, GLenum textarget, GLuint texture, GLint level )
    {
        GetDelegateForFunction< PFNGLFRAMEBUFFERTEXTURE2DPROC >
            (
             "glFramebufferTexture2D",
             out _glFramebufferTexture2D
            );

        _glFramebufferTexture2D( target, attachment, textarget, texture, level );
    }

    // ========================================================================

    public void FramebufferTexture3D( GLenum target, GLenum attachment, GLenum textarget, GLuint texture, GLint level,
                                      GLint zoffset )
    {
        GetDelegateForFunction< PFNGLFRAMEBUFFERTEXTURE3DPROC >
            (
             "glFramebufferTexture3D",
             out _glFramebufferTexture3D
            );

        _glFramebufferTexture3D( target, attachment, textarget, texture, level, zoffset );
    }

    // ========================================================================

    public void FramebufferRenderbuffer( GLenum target, GLenum attachment, GLenum renderbuffertarget,
                                         GLuint renderbuffer )
    {
        GetDelegateForFunction< PFNGLFRAMEBUFFERRENDERBUFFERPROC >
            (
             "glFramebufferRenderbuffer",
             out _glFramebufferRenderbuffer
            );

        _glFramebufferRenderbuffer( target, attachment, renderbuffertarget, renderbuffer );
    }

    // ========================================================================

    public void GetFramebufferAttachmentParameteriv( GLenum target, GLenum attachment, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETFRAMEBUFFERATTACHMENTPARAMETERIVPROC >
            (
             "glGetFramebufferAttachmentParameteriv",
             out
             _glGetFramebufferAttachmentParameteriv
            );

        _glGetFramebufferAttachmentParameteriv( target, attachment, pname, parameters );
    }

    public void GetFramebufferAttachmentParameteriv( GLenum target, GLenum attachment, GLenum pname,
                                                     ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETFRAMEBUFFERATTACHMENTPARAMETERIVPROC >
            (
             "glGetFramebufferAttachmentParameteriv",
             out
             _glGetFramebufferAttachmentParameteriv
            );

        fixed ( GLint* p = &parameters[ 0 ] )
        {
            _glGetFramebufferAttachmentParameteriv( target, attachment, pname, p );
        }
    }

    // ========================================================================

    public void GenerateMipmap( GLenum target )
    {
        GetDelegateForFunction< PFNGLGENERATEMIPMAPPROC >( "glGenerateMipmap", out _glGenerateMipmap );

        _glGenerateMipmap( target );
    }

    // ========================================================================

    public void BlitFramebuffer( GLint srcX0,
                                 GLint srcY0,
                                 GLint srcX1,
                                 GLint srcY1,
                                 GLint dstX0,
                                 GLint dstY0,
                                 GLint dstX1,
                                 GLint dstY1,
                                 GLbitfield mask,
                                 GLenum filter )
    {
        GetDelegateForFunction< PFNGLBLITFRAMEBUFFERPROC >( "glBlitFrameBuffer", out _glBlitFramebuffer );

        _glBlitFramebuffer( srcX0, srcY0, srcX1, srcY1, dstX0, dstY0, dstX1, dstY1, mask, filter );
    }

    // ========================================================================

    public void RenderbufferStorageMultisample( GLenum target, GLsizei samples, GLenum internalFormat, GLsizei width,
                                                GLsizei height )
    {
        GetDelegateForFunction< PFNGLRENDERBUFFERSTORAGEMULTISAMPLEPROC >
            (
             "glRenderbufferStorageMultisample",
             out _glRenderbufferStorageMultisample
            );

        _glRenderbufferStorageMultisample( target, samples, internalFormat, width, height );
    }

    // ========================================================================

    public void FramebufferTextureLayer( GLenum target, GLenum attachment, GLuint texture, GLint level, GLint layer )
    {
        GetDelegateForFunction< PFNGLFRAMEBUFFERTEXTURELAYERPROC >
            (
             "glFramebufferTextureLayer",
             out _glFramebufferTextureLayer
            );

        _glFramebufferTextureLayer( target, attachment, texture, level, layer );
    }

    // ========================================================================

    public IntPtr MapBufferRange( GLenum target, GLintptr offset, GLsizeiptr length, GLbitfield access )
    {
        GetDelegateForFunction< PFNGLMAPBUFFERRANGEPROC >( "glMapBufferRange", out _glMapBufferRange );

        return _glMapBufferRange( target, offset, length, access );
    }

    public Span< T > MapBufferRange< T >( GLenum target, GLintptr offset, GLsizeiptr length, GLbitfield access )
        where T : unmanaged
    {
        GetDelegateForFunction< PFNGLMAPBUFFERRANGEPROC >( "glMapBufferRange", out _glMapBufferRange );

        IntPtr ret = _glMapBufferRange( target, offset, length, access );

        return new Span< T >( ( void* )ret, length );
    }

    // ========================================================================

    public void FlushMappedBufferRange( GLenum target, GLintptr offset, GLsizeiptr length )
    {
        GetDelegateForFunction< PFNGLFLUSHMAPPEDBUFFERRANGEPROC >
            (
             "glFlushMappedBufferRange",
             out _glFlushMappedBufferRange
            );

        _glFlushMappedBufferRange( target, offset, length );
    }

    // ========================================================================

    /// <inheritdoc />
    public void DrawArraysInstanced( GLenum mode, GLint first, GLsizei count, GLsizei instancecount )
    {
        GetDelegateForFunction< PFNGLDRAWARRAYSINSTANCEDPROC >( "glDrawArraysInstanced", out _glDrawArraysInstanced );

        _glDrawArraysInstanced( mode, first, count, instancecount );
    }

    // ========================================================================

    /// <inheritdoc />
    public void DrawElementsInstanced( GLenum mode, GLsizei count, GLenum type, IntPtr indices, GLsizei instancecount )
    {
        GetDelegateForFunction< PFNGLDRAWELEMENTSINSTANCEDPROC >
            (
             "glDrawElementsInstanced",
             out _glDrawElementsInstanced
            );

        _glDrawElementsInstanced( mode, count, type, indices, instancecount );
    }

    /// <inheritdoc />
    public void DrawElementsInstanced< T >( GLenum mode, GLsizei count, GLenum type, T[] indices,
                                            GLsizei instancecount )
        where T : unmanaged, IUnsignedNumber< T >
    {
        GetDelegateForFunction< PFNGLDRAWELEMENTSINSTANCEDPROC >
            (
             "glDrawElementsInstanced",
             out _glDrawElementsInstanced
            );

        fixed ( T* p = &indices[ 0 ] )
        {
            _glDrawElementsInstanced( mode, count, type, ( IntPtr )p, instancecount );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void TexBuffer( GLenum target, GLenum internalFormat, GLuint buffer )
    {
        GetDelegateForFunction< PFNGLTEXBUFFERPROC >( "glTexBuffer", out _glTexBuffer );

        _glTexBuffer( target, internalFormat, buffer );
    }

    // ========================================================================

    /// <inheritdoc />
    public void PrimitiveRestartIndex( GLuint index )
    {
        GetDelegateForFunction< PFNGLPRIMITIVERESTARTINDEXPROC >
            (
             "glPrimitiveRestartIndex",
             out _glPrimitiveRestartIndex
            );

        _glPrimitiveRestartIndex( index );
    }

    // ========================================================================

    /// <inheritdoc />
    public void CopyBufferSubData( GLenum readTarget, GLenum writeTarget, GLintptr readOffset, GLintptr writeOffset,
                                   GLsizeiptr size )
    {
        GetDelegateForFunction< PFNGLCOPYBUFFERSUBDATAPROC >( "glCopyBufferSubData", out _glCopyBufferSubData );

        _glCopyBufferSubData( readTarget, writeTarget, readOffset, writeOffset, size );
    }

    // ========================================================================

    /// <inheritdoc />
    public void DrawElementsBaseVertex( GLenum mode, GLsizei count, GLenum type, IntPtr indices, GLint basevertex )
    {
        GetDelegateForFunction< PFNGLDRAWELEMENTSBASEVERTEXPROC >
            (
             "glDrawElementsBaseVertex",
             out _glDrawElementsBaseVertex
            );

        _glDrawElementsBaseVertex( mode, count, type, indices, basevertex );
    }

    /// <inheritdoc />
    public void DrawElementsBaseVertex< T >( GLenum mode, GLsizei count, GLenum type, T[] indices, GLint basevertex )
        where T : unmanaged, IUnsignedNumber< T >
    {
        GetDelegateForFunction< PFNGLDRAWELEMENTSBASEVERTEXPROC >
            (
             "glDrawElementsBaseVertex",
             out _glDrawElementsBaseVertex
            );

        {
            fixed ( void* p = &indices[ 0 ] )
            {
                _glDrawElementsBaseVertex( mode, count, type, ( IntPtr )p, basevertex );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void DrawRangeElementsBaseVertex( GLenum mode, GLuint start, GLuint end, GLsizei count, GLenum type,
                                             IntPtr indices,
                                             GLint basevertex )
    {
        GetDelegateForFunction< PFNGLDRAWRANGEELEMENTSBASEVERTEXPROC >
            (
             "glDrawRangeElementsBaseVertex",
             out _glDrawRangeElementsBaseVertex
            );

        _glDrawRangeElementsBaseVertex( mode, start, end, count, type, indices, basevertex );
    }

    /// <inheritdoc />
    public void DrawRangeElementsBaseVertex< T >( GLenum mode, GLuint start, GLuint end, GLsizei count, GLenum type,
                                                  T[] indices,
                                                  GLint basevertex )
        where T : unmanaged, IUnsignedNumber< T >
    {
        GetDelegateForFunction< PFNGLDRAWRANGEELEMENTSBASEVERTEXPROC >
            (
             "glDrawRangeElementsBaseVertex",
             out _glDrawRangeElementsBaseVertex
            );

        fixed ( void* p = &indices[ 0 ] )
        {
            var dataptr = ( IntPtr )p;

            _glDrawRangeElementsBaseVertex( mode, start, end, count, type, dataptr, basevertex );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void DrawElementsInstancedBaseVertex( GLenum mode, GLsizei count, GLenum type, IntPtr indices,
                                                 GLsizei instancecount,
                                                 GLint basevertex )
    {
        GetDelegateForFunction< PFNGLDRAWELEMENTSINSTANCEDBASEVERTEXPROC >
            (
             "glDrawElementsInstancedBaseVertex",
             out _glDrawElementsInstancedBaseVertex
            );

        _glDrawElementsInstancedBaseVertex( mode, count, type, indices, instancecount, basevertex );
    }

    /// <inheritdoc />
    public void DrawElementsInstancedBaseVertex< T >( GLenum mode, GLsizei count, GLenum type, T[] indices,
                                                      GLsizei instancecount,
                                                      GLint basevertex )
        where T : unmanaged, IUnsignedNumber< T >
    {
        GetDelegateForFunction< PFNGLDRAWELEMENTSINSTANCEDBASEVERTEXPROC >
            (
             "glDrawElementsInstancedBaseVertex",
             out _glDrawElementsInstancedBaseVertex
            );

        {
            fixed ( void* p = &indices[ 0 ] )
            {
                var dataptr = ( IntPtr )p;

                _glDrawElementsInstancedBaseVertex( mode, count, type, dataptr, instancecount, basevertex );
            }
        }
    }

    // ========================================================================

    public void MultiDrawElementsBaseVertex( GLenum mode, GLsizei* count, GLenum type, IntPtr* indices,
                                             GLsizei drawcount,
                                             GLint* basevertex )
    {
        GetDelegateForFunction< PFNGLMULTIDRAWELEMENTSBASEVERTEXPROC >
            (
             "glMultiDrawElementsBaseVertex",
             out _glMultiDrawElementsBaseVertex
            );

        _glMultiDrawElementsBaseVertex( mode, count, type, indices, drawcount, basevertex );
    }

    public void MultiDrawElementsBaseVertex< T >( GLenum mode, GLenum type, T[][] indices, GLint[] basevertex )
        where T : unmanaged, IUnsignedNumber< T >
    {
        if ( indices.Length != basevertex.Length )
        {
            throw new ArgumentException( "indices and basevertex must have the same length" );
        }

        var counts    = new GLsizei[ indices.Length ];
        var indexPtrs = new T*[ indices.Length ];

        for ( var i = 0; i < indices.Length; i++ )
        {
            counts[ i ] = indices[ i ].Length;

            fixed ( T* p = &indices[ i ][ 0 ] )
            {
                indexPtrs[ i ] = p;
            }
        }

        GetDelegateForFunction< PFNGLMULTIDRAWELEMENTSBASEVERTEXPROC >
            (
             "glMultiDrawElementsBaseVertex",
             out _glMultiDrawElementsBaseVertex
            );

        fixed ( GLsizei* cp = &counts[ 0 ] )
        {
            fixed ( GLint* bvp = &basevertex[ 0 ] )
            {
                fixed ( T** ip = &indexPtrs[ 0 ] )
                {
                    _glMultiDrawElementsBaseVertex( mode, cp, type, ( IntPtr* )ip, indices.Length, bvp );
                }
            }
        }
    }

    // ========================================================================

    public void ProvokingVertex( GLenum mode )
    {
        GetDelegateForFunction< PFNGLPROVOKINGVERTEXPROC >( "glProvokingVertex", out _glProvokingVertex );

        _glProvokingVertex( mode );
    }

    // ========================================================================

    public IntPtr FenceSync( GLenum condition, GLbitfield flags )
    {
        GetDelegateForFunction< PFNGLFENCESYNCPROC >( "glFenceSync", out _glFenceSync );

        return _glFenceSync( condition, flags );
    }

    // ========================================================================

    public GLbool IsSync( IntPtr sync )
    {
        GetDelegateForFunction< PFNGLISSYNCPROC >( "glIsSync", out _glIsSync );

        return _glIsSync( sync );
    }

    // ========================================================================

    /// <inheritdoc />
    public void DeleteSync( IntPtr sync )
    {
        GetDelegateForFunction< PFNGLDELETESYNCPROC >( "glDeleteSync", out _glDeleteSync );

        _glDeleteSync( sync );
    }

    // ========================================================================

    /// <inheritdoc />
    public void SetGLUnpackAlignment( Pixmap pixmap, int alignment )
    {
        Engine.GL.PixelStorei( IGL.GLUnpackAlignment, alignment );
    }

    // ========================================================================
    
    /// <inheritdoc />
    public GLenum ClientWaitSync( IntPtr sync, GLbitfield flags, GLuint64 timeout )
    {
        GetDelegateForFunction< PFNGLCLIENTWAITSYNCPROC >( "glClientWaitSync", out _glClientWaitSync );

        return _glClientWaitSync( sync, flags, timeout );
    }

    public IntPtr FenceSyncSafe( GLenum condition, GLbitfield flags )
    {
        GetDelegateForFunction< PFNGLFENCESYNCPROC >( "glFenceSync", out _glFenceSync );

        return new IntPtr( _glFenceSync( condition, flags ) );
    }

    /// <inheritdoc />
    public void GetMultisamplefv( GLenum pname, GLuint index, GLfloat* val )
    {
        GetDelegateForFunction< PFNGLGETMULTISAMPLEFVPROC >( "glGetMultisamplefv", out _glGetMultisamplefv );

        _glGetMultisamplefv( pname, index, val );
    }

    /// <inheritdoc />
    public void GetMultisamplefvSafe( GLenum pname, GLuint index, ref GLfloat[] val )
    {
        GetDelegateForFunction< PFNGLGETMULTISAMPLEFVPROC >( "glGetMultisamplefv", out _glGetMultisamplefv );

        {
            fixed ( GLfloat* dp = &val[ 0 ] )
            {
                _glGetMultisamplefv( pname, index, dp );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void SampleMaski( GLuint maskNumber, GLbitfield mask )
    {
        GetDelegateForFunction< PFNGLSAMPLEMASKIPROC >( "glSampleMaski", out _glSampleMaski );

        _glSampleMaski( maskNumber, mask );
    }

    // ========================================================================

    /// <inheritdoc />
    public void SampleCoverage( GLfloat value, GLbool invert )
    {
        GetDelegateForFunction< PFNGLSAMPLECOVERAGEPROC >( "glSampleCoverage", out _glSampleCoverage );

        _glSampleCoverage( value, invert );
    }

    // ========================================================================

    public void GenSamplers( GLsizei count, GLuint* samplers )
    {
        GetDelegateForFunction< PFNGLGENSAMPLERSPROC >( "glGenSamplers", out _glGenSamplers );

        _glGenSamplers( count, samplers );
    }

    /// <inheritdoc />
    public GLuint[] GenSamplers( GLsizei count )
    {
        var result = new GLuint[ count ];

        GetDelegateForFunction< PFNGLGENSAMPLERSPROC >( "glGenSamplers", out _glGenSamplers );

        fixed ( GLuint* dp = &result[ 0 ] )
        {
            _glGenSamplers( count, dp );
        }

        return result;
    }

    /// <inheritdoc />
    public GLuint GenSampler()
    {
        return GenSamplers( 1 )[ 0 ];
    }

    // ========================================================================

    /// <inheritdoc />
    public void DeleteSamplers( GLsizei count, GLuint* samplers )
    {
        GetDelegateForFunction< PFNGLDELETESAMPLERSPROC >( "glDeleteSamplers", out _glDeleteSamplers );

        _glDeleteSamplers( count, samplers );
    }

    /// <inheritdoc />
    public void DeleteSamplers( params GLuint[] samplers )
    {
        GetDelegateForFunction< PFNGLDELETESAMPLERSPROC >( "glDeleteSamplers", out _glDeleteSamplers );

        fixed ( GLuint* dp = &samplers[ 0 ] )
        {
            _glDeleteSamplers( samplers.Length, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public GLbool IsSampler( GLuint sampler )
    {
        GetDelegateForFunction< PFNGLISSAMPLERPROC >( "glIsSampler", out _glIsSampler );

        return _glIsSampler( sampler );
    }

    // ========================================================================

    /// <inheritdoc />
    public void BindSampler( GLuint unit, GLuint sampler )
    {
        GetDelegateForFunction< PFNGLBINDSAMPLERPROC >( "glBindSampler", out _glBindSampler );

        _glBindSampler( unit, sampler );
    }

    // ========================================================================

    /// <inheritdoc />
    public void SamplerParameteri( GLuint sampler, GLenum pname, GLint param )
    {
        GetDelegateForFunction< PFNGLSAMPLERPARAMETERIPROC >( "glSamplerParameteri", out _glSamplerParameteri );

        _glSamplerParameteri( sampler, pname, param );
    }

    // ========================================================================

    /// <inheritdoc />
    public void SamplerParameteriv( GLuint sampler, GLenum pname, GLint* param )
    {
        GetDelegateForFunction< PFNGLSAMPLERPARAMETERIVPROC >( "glSamplerParameteriv", out _glSamplerParameteriv );

        _glSamplerParameteriv( sampler, pname, param );
    }

    /// <inheritdoc />
    public void SamplerParameteriv( GLuint sampler, GLenum pname, GLint[] param )
    {
        GetDelegateForFunction< PFNGLSAMPLERPARAMETERIVPROC >( "glSamplerParameteriv", out _glSamplerParameteriv );

        {
            fixed ( GLint* dp = &param[ 0 ] )
            {
                _glSamplerParameteriv( sampler, pname, dp );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void SamplerParameterf( GLuint sampler, GLenum pname, GLfloat param )
    {
        GetDelegateForFunction< PFNGLSAMPLERPARAMETERFPROC >( "glSamplerParameterf", out _glSamplerParameterf );

        _glSamplerParameterf( sampler, pname, param );
    }

    // ========================================================================

    /// <inheritdoc />
    public void SamplerParameterfv( GLuint sampler, GLenum pname, GLfloat* param )
    {
        GetDelegateForFunction< PFNGLSAMPLERPARAMETERFVPROC >( "glSamplerParameterfv", out _glSamplerParameterfv );

        _glSamplerParameterfv( sampler, pname, param );
    }

    /// <inheritdoc />
    public void SamplerParameterfv( GLuint sampler, GLenum pname, GLfloat[] param )
    {
        GetDelegateForFunction< PFNGLSAMPLERPARAMETERFVPROC >( "glSamplerParameterfv", out _glSamplerParameterfv );

        fixed ( GLfloat* dp = &param[ 0 ] )
        {
            _glSamplerParameterfv( sampler, pname, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void SamplerParameterIiv( GLuint sampler, GLenum pname, GLint* param )
    {
        GetDelegateForFunction< PFNGLSAMPLERPARAMETERIIVPROC >( "glSamplerParameterIiv", out _glSamplerParameterIiv );

        _glSamplerParameterIiv( sampler, pname, param );
    }

    /// <inheritdoc />
    public void SamplerParameterIiv( GLuint sampler, GLenum pname, GLint[] param )
    {
        GetDelegateForFunction< PFNGLSAMPLERPARAMETERIIVPROC >( "glSamplerParameterIiv", out _glSamplerParameterIiv );

        fixed ( GLint* dp = &param[ 0 ] )
        {
            _glSamplerParameterIiv( sampler, pname, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void SamplerParameterIuiv( GLuint sampler, GLenum pname, GLuint* param )
    {
        GetDelegateForFunction< PFNGLSAMPLERPARAMETERIUIVPROC >
            (
             "glSamplerParameterIuiv",
             out _glSamplerParameterIuiv
            );

        _glSamplerParameterIuiv( sampler, pname, param );
    }

    /// <inheritdoc />
    public void SamplerParameterIuiv( GLuint sampler, GLenum pname, GLuint[] param )
    {
        GetDelegateForFunction< PFNGLSAMPLERPARAMETERIUIVPROC >
            (
             "glSamplerParameterIuiv",
             out _glSamplerParameterIuiv
            );

        fixed ( GLuint* dp = &param[ 0 ] )
        {
            _glSamplerParameterIuiv( sampler, pname, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetSamplerParameteriv( GLuint sampler, GLenum pname, GLint* param )
    {
        GetDelegateForFunction< PFNGLGETSAMPLERPARAMETERIVPROC >
            (
             "glGetSamplerParameteriv",
             out _glGetSamplerParameteriv
            );

        _glGetSamplerParameteriv( sampler, pname, param );
    }

    /// <inheritdoc />
    public void GetSamplerParameteriv( GLuint sampler, GLenum pname, ref GLint[] param )
    {
        GetDelegateForFunction< PFNGLGETSAMPLERPARAMETERIVPROC >
            (
             "glGetSamplerParameteriv",
             out _glGetSamplerParameteriv
            );

        fixed ( GLint* dp = &param[ 0 ] )
        {
            _glGetSamplerParameteriv( sampler, pname, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetSamplerParameterIiv( GLuint sampler, GLenum pname, GLint* param )
    {
        GetDelegateForFunction< PFNGLGETSAMPLERPARAMETERIIVPROC >
            (
             "glGetSamplerParameterIiv",
             out _glGetSamplerParameterIiv
            );

        _glGetSamplerParameterIiv( sampler, pname, param );
    }

    /// <inheritdoc />
    public void GetSamplerParameterIiv( GLuint sampler, GLenum pname, ref GLint[] param )
    {
        GetDelegateForFunction< PFNGLGETSAMPLERPARAMETERIIVPROC >
            (
             "glGetSamplerParameterIiv",
             out _glGetSamplerParameterIiv
            );

        fixed ( GLint* dp = &param[ 0 ] )
        {
            _glGetSamplerParameterIiv( sampler, pname, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetSamplerParameterfv( GLuint sampler, GLenum pname, GLfloat* param )
    {
        GetDelegateForFunction< PFNGLGETSAMPLERPARAMETERFVPROC >
            (
             "glGetSamplerParameterfv",
             out _glGetSamplerParameterfv
            );

        _glGetSamplerParameterfv( sampler, pname, param );
    }

    /// <inheritdoc />
    public void GetSamplerParameterfv( GLuint sampler, GLenum pname, ref GLfloat[] param )
    {
        GetDelegateForFunction< PFNGLGETSAMPLERPARAMETERFVPROC >
            (
             "glGetSamplerParameterfv",
             out _glGetSamplerParameterfv
            );

        fixed ( GLfloat* dp = &param[ 0 ] )
        {
            _glGetSamplerParameterfv( sampler, pname, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetSamplerParameterIuiv( GLuint sampler, GLenum pname, GLuint* param )
    {
        GetDelegateForFunction< PFNGLGETSAMPLERPARAMETERIUIVPROC >
            (
             "glGetSamplerParameterIuiv",
             out _glGetSamplerParameterIuiv
            );

        _glGetSamplerParameterIuiv( sampler, pname, param );
    }

    /// <inheritdoc />
    public void GetSamplerParameterIuiv( GLuint sampler, GLenum pname, ref GLuint[] param )
    {
        GetDelegateForFunction< PFNGLGETSAMPLERPARAMETERIUIVPROC >
            (
             "glGetSamplerParameterIuiv",
             out _glGetSamplerParameterIuiv
            );

        fixed ( GLuint* dp = &param[ 0 ] )
        {
            _glGetSamplerParameterIuiv( sampler, pname, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void MinSampleShading( GLfloat value )
    {
        GetDelegateForFunction< PFNGLMINSAMPLESHADINGPROC >( "glMinSampleShading", out _glMinSampleShading );

        _glMinSampleShading( value );
    }

    // ========================================================================

    public void BindSamplers( GLuint first, GLsizei count, GLuint* samplers )
    {
        GetDelegateForFunction< PFNGLBINDSAMPLERSPROC >( "glBindSamplers", out _glBindSamplers );

        _glBindSamplers( first, count, samplers );
    }

    public void BindSamplers( GLuint first, GLuint[] samplers )
    {
        GetDelegateForFunction< PFNGLBINDSAMPLERSPROC >( "glBindSamplers", out _glBindSamplers );

        fixed ( GLuint* pSamplers = &samplers[ 0 ] )
        {
            _glBindSamplers( first, samplers.Length, pSamplers );
        }
    }

    // ========================================================================

    public void CreateSamplers( GLsizei n, GLuint* samplers )
    {
        GetDelegateForFunction< PFNGLCREATESAMPLERSPROC >( "glCreateSamplers", out _glCreateSamplers );

        _glCreateSamplers( n, samplers );
    }

    public GLuint[] CreateSamplers( GLsizei n )
    {
        var samplers = new GLuint[ n ];

        GetDelegateForFunction< PFNGLCREATESAMPLERSPROC >( "glCreateSamplers", out _glCreateSamplers );

        fixed ( GLuint* ptrSamplers = &samplers[ 0 ] )
        {
            _glCreateSamplers( n, ptrSamplers );
        }

        return samplers;
    }

    public GLuint CreateSamplers()
    {
        return CreateSamplers( 1 )[ 0 ];
    }

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region Buffer Object Bindings

    /// <summary>
    /// Bind a named buffer object.
    /// </summary>
    /// <param name="target">
    /// Specifies the target buffer object of the binding point to be modified. This buffer is
    /// one of those named in the <see cref="BufferTarget"/> enum.
    /// </param>
    /// <param name="buffer">Specifies the name of a buffer object.</param>
    public void BindBuffer( BufferTarget target, GLuint buffer )
    {
        GetDelegateForFunction< PFNGLBINDBUFFERPROC >( "glBindBuffer", out _glBindBuffer );

        _glBindBuffer( ( GLenum )target, buffer );
    }

    // ========================================================================

    /// <summary>
    /// Delete named buffer objects.
    /// </summary>
    /// <param name="n">Specifies the number of buffer objects to be deleted.</param>
    /// <param name="buffers">A pointer to an array of buffer objects to be deleted.</param>
    public void DeleteBuffers( GLsizei n, GLuint* buffers )
    {
        GetDelegateForFunction< PFNGLDELETEBUFFERSPROC >( "glDeleteBuffers", out _glDeleteBuffers );

        _glDeleteBuffers( n, buffers );
    }

    /// <summary>
    /// Delete named buffer objects.
    /// </summary>
    /// <param name="buffers">An array of buffer objects to be deleted.</param>
    public void DeleteBuffers( params GLuint[] buffers )
    {
        GetDelegateForFunction< PFNGLDELETEBUFFERSPROC >( "glDeleteBuffers", out _glDeleteBuffers );

        fixed ( GLuint* p = &buffers[ 0 ] )
        {
            _glDeleteBuffers( buffers.Length, p );
        }
    }

    // ========================================================================

    /// <summary>
    /// Generate buffer object names.
    /// </summary>
    /// <param name="n">Specifies the number of buffer object names to generate.</param>
    /// <param name="buffers">
    /// A pointer to an array in which the generated buffer object names are to be stored.
    /// </param>
    public void GenBuffers( GLsizei n, GLuint* buffers )
    {
        GetDelegateForFunction< PFNGLGENBUFFERSPROC >( "glGenBuffers", out _glGenBuffers );

        _glGenBuffers( n, buffers );
    }

    /// <summary>
    /// Generate buffer object names.
    /// </summary>
    /// <param name="n">Specifies the number of buffer object names to generate.</param>
    /// <returns>An array of generated buffer object names.</returns>
    public GLuint[] GenBuffers( GLsizei n )
    {
        GetDelegateForFunction< PFNGLGENBUFFERSPROC >( "glGenBuffers", out _glGenBuffers );

        var ret = new GLuint[ n ];

        fixed ( GLuint* p = &ret[ 0 ] )
        {
            _glGenBuffers( n, p );
        }

        return ret;
    }

    /// <summary>
    /// Generate a single buffer object name.
    /// </summary>
    /// <returns>The generated buffer object name.</returns>
    public GLuint GenBuffer()
    {
        return GenBuffers( 1 )[ 0 ];
    }

    // ========================================================================

    /// <summary>
    /// Determine if a name corresponds to a buffer object.
    /// </summary>
    /// <param name="buffer">A value that may be the name of a buffer object.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="buffer"/> is a buffer object name. <c>false</c>
    /// otherwise.
    /// </returns>
    public GLbool IsBuffer( GLuint buffer )
    {
        GetDelegateForFunction< PFNGLISBUFFERPROC >( "glIsBuffer", out _glIsBuffer );

        return _glIsBuffer( buffer );
    }

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="size"></param>
    /// <param name="data"></param>
    /// <param name="usage"></param>
    public void BufferData( BufferTarget target, int size, IntPtr data, BufferUsageHint usage )
    {
        GetDelegateForFunction< PFNGLBUFFERDATAPROC >( "glBufferData", out _glBufferData );

        _glBufferData( ( GLenum )target, size, data, ( GLenum )usage );

        CheckErrors();
    }

    /// <summary>
    /// Create and initialize a buffer object's data store.
    /// </summary>
    /// <param name="target">
    /// Specifies the target buffer object. The symbolic constant must be <see cref="IGL.GLArrayBuffer"/>,
    /// <see cref="IGL.GLAtomicCounterBuffer"/>, <see cref="IGL.GLCopyReadBuffer"/>, <see cref="IGL.GLCopyWriteBuffer"/>,
    /// <see cref="IGL.GLDispatchIndirectBuffer"/>, <see cref="IGL.GLDrawIndirectBuffer"/>,
    /// <see cref="IGL.GLElementArrayBuffer"/>, <see cref="IGL.GLPixelPackBuffer"/>, <see cref="IGL.GLPixelUnpackBuffer"/>,
    /// <see cref="IGL.GLQueryBuffer"/>, <see cref="IGL.GLShaderStorageBuffer"/>, <see cref="IGL.GL_TEXTURE_BUFFER"/>,
    /// <see cref="IGL.GLTransformFeedbackBuffer"/> or <see cref="IGL.GLUniformBuffer"/>.
    /// </param>
    /// <param name="size">Specifies the size in bytes of the buffer object's new data store.</param>
    /// <param name="data">
    /// Specifies a pointer to data that will be copied into the data store for initialization, or
    /// <see cref="GLBindings.Null"/> if no data is to be copied.
    /// </param>
    /// <param name="usage">
    /// Specifies the expected usage pattern of the data store. The symbolic constant must be
    /// <see cref="IGL.GLStreamDraw"/>, <see cref="IGL.GLStreamRead"/>, <see cref="IGL.GLStreamCopy"/>,
    /// <see cref="IGL.GLStaticDraw"/>, <see cref="IGL.GLStaticRead"/>, <see cref="IGL.GLStaticCopy"/>,
    /// <see cref="IGL.GLDynamicDraw"/>, <see cref="IGL.GLDynamicRead"/> or <see cref="IGL.GLDynamicCopy"/>.
    /// </param>
    public void BufferData( GLenum target, int size, IntPtr data, GLenum usage )
    {
        GetDelegateForFunction< PFNGLBUFFERDATAPROC >( "glBufferData", out _glBufferData );

        _glBufferData( target, size, data, usage );

        CheckErrors();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="data"></param>
    /// <param name="usage"></param>
    /// <typeparam name="T"></typeparam>
    public void BufferData< T >( BufferTarget target, T[] data, BufferUsageHint usage ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLBUFFERDATAPROC >( "glBufferData", out _glBufferData );

        fixed ( T* p = &data[ 0 ] )
        {
            _glBufferData( ( GLenum )target, sizeof( T ) * data.Length, ( IntPtr )p, ( GLenum )usage );
        }
    }

    /// <summary>
    /// Create and initialize a buffer object's data store.
    /// </summary>
    /// <typeparam name="T">The type of the data to be copied.</typeparam>
    /// <param name="target">
    /// Specifies the target buffer object. The symbolic constant must be <see cref="IGL.GLArrayBuffer"/>,
    /// <see cref="IGL.GLAtomicCounterBuffer"/>, <see cref="IGL.GLCopyReadBuffer"/>, <see cref="IGL.GLCopyWriteBuffer"/>,
    /// <see cref="IGL.GLDispatchIndirectBuffer"/>, <see cref="IGL.GLDrawIndirectBuffer"/>,
    /// <see cref="IGL.GLElementArrayBuffer"/>, <see cref="IGL.GLPixelPackBuffer"/>, <see cref="IGL.GLPixelUnpackBuffer"/>,
    /// <see cref="IGL.GLQueryBuffer"/>, <see cref="IGL.GLShaderStorageBuffer"/>, <see cref="IGL.GL_TEXTURE_BUFFER"/>,
    /// <see cref="IGL.GLTransformFeedbackBuffer"/> or <see cref="IGL.GLUniformBuffer"/>.
    /// </param>
    /// <param name="data">
    /// An array of <typeparamref name="T"/>s that will be copied into the data store for initialization.
    /// </param>
    /// <param name="usage">
    /// Specifies the expected usage pattern of the data store. The symbolic constant must be
    /// <see cref="IGL.GLStreamDraw"/>, <see cref="IGL.GLStreamRead"/>, <see cref="IGL.GLStreamCopy"/>,
    /// <see cref="IGL.GLStaticDraw"/>, <see cref="IGL.GLStaticRead"/>, <see cref="IGL.GLStaticCopy"/>,
    /// <see cref="IGL.GLDynamicDraw"/>, <see cref="IGL.GLDynamicRead"/> or <see cref="IGL.GLDynamicCopy"/>.
    /// </param>
    public void BufferData< T >( GLenum target, T[] data, GLenum usage ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLBUFFERDATAPROC >( "glBufferData", out _glBufferData );

        fixed ( T* p = &data[ 0 ] )
        {
            _glBufferData( target, sizeof( T ) * data.Length, ( IntPtr )p, usage );
        }
    }

    // ========================================================================

    /// <summary>
    /// Update a subset of a buffer object's data store.
    /// </summary>
    /// <param name="target">
    /// Specifies the target buffer object. The symbolic constant must be <see cref="IGL.GLArrayBuffer"/>,
    /// <see cref="IGL.GLAtomicCounterBuffer"/>, <see cref="IGL.GLCopyReadBuffer"/>, <see cref="IGL.GLCopyWriteBuffer"/>,
    /// <see cref="IGL.GLDispatchIndirectBuffer"/>, <see cref="IGL.GLDrawIndirectBuffer"/>,
    /// <see cref="IGL.GLElementArrayBuffer"/>, <see cref="IGL.GLPixelPackBuffer"/>, <see cref="IGL.GLPixelUnpackBuffer"/>,
    /// <see cref="IGL.GLQueryBuffer"/>, <see cref="IGL.GLShaderStorageBuffer"/>, <see cref="IGL.GL_TEXTURE_BUFFER"/>,
    /// <see cref="IGL.GLTransformFeedbackBuffer"/> or <see cref="IGL.GLUniformBuffer"/>.
    /// </param>
    /// <param name="offset">
    /// Specifies the offset into the buffer object's data store where data replacement will begin,
    /// measured in bytes.
    /// </param>
    /// <param name="size">Specifies the size in bytes of the data store region being replaced.</param>
    /// <param name="data">Specifies a pointer to the new data that will be copied into the data store.</param>
    public void BufferSubData( GLenum target, GLintptr offset, GLsizeiptr size, IntPtr data )
    {
        GetDelegateForFunction< PFNGLBUFFERSUBDATAPROC >( "glBufferSubData", out _glBufferSubData );

        _glBufferSubData( target, offset, size, data );
    }

    /// <summary>
    /// Update a subset of a buffer object's data store.
    /// </summary>
    /// <typeparam name="T">The type of the data to be copied.</typeparam>
    /// <param name="target">
    /// Specifies the target buffer object. The symbolic constant must be <see cref="IGL.GLArrayBuffer"/>,
    /// <see cref="IGL.GLAtomicCounterBuffer"/>, <see cref="IGL.GLCopyReadBuffer"/>, <see cref="IGL.GLCopyWriteBuffer"/>,
    /// <see cref="IGL.GLDispatchIndirectBuffer"/>, <see cref="IGL.GLDrawIndirectBuffer"/>,
    /// <see cref="IGL.GLElementArrayBuffer"/>, <see cref="IGL.GLPixelPackBuffer"/>, <see cref="IGL.GLPixelUnpackBuffer"/>,
    /// <see cref="IGL.GLQueryBuffer"/>, <see cref="IGL.GLShaderStorageBuffer"/>, <see cref="IGL.GL_TEXTURE_BUFFER"/>,
    /// <see cref="IGL.GLTransformFeedbackBuffer"/> or <see cref="IGL.GLUniformBuffer"/>.
    /// </param>
    /// <param name="offsetCount">Specifies the offset into the buffer object's data store where data replacement will begin.</param>
    /// <param name="data">An array of <typeparamref name="T"/>s that will be copied into the data store for replacement.</param>
    public void BufferSubData< T >( GLenum target, GLintptr offsetCount, T[] data ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLBUFFERSUBDATAPROC >( "glBufferSubData", out _glBufferSubData );

        fixed ( T* p = &data[ 0 ] )
        {
            _glBufferSubData( target, offsetCount * sizeof( T ), sizeof( T ) * data.Length, ( IntPtr )p );
        }
    }

    // ========================================================================

    /// <summary>
    /// Return a subset of a buffer object's data store.
    /// </summary>
    /// <param name="target">
    /// Specifies the target buffer object. The symbolic constant must be <see cref="IGL.GLArrayBuffer"/>,
    /// <see cref="IGL.GLAtomicCounterBuffer"/>, <see cref="IGL.GLCopyReadBuffer"/>, <see cref="IGL.GLCopyWriteBuffer"/>,
    /// <see cref="IGL.GLDispatchIndirectBuffer"/>, <see cref="IGL.GLDrawIndirectBuffer"/>,
    /// <see cref="IGL.GLElementArrayBuffer"/>, <see cref="IGL.GLPixelPackBuffer"/>, <see cref="IGL.GLPixelUnpackBuffer"/>,
    /// <see cref="IGL.GLQueryBuffer"/>, <see cref="IGL.GLShaderStorageBuffer"/>, <see cref="IGL.GL_TEXTURE_BUFFER"/>,
    /// <see cref="IGL.GLTransformFeedbackBuffer"/> or <see cref="IGL.GLUniformBuffer"/>.
    /// </param>
    /// <param name="offset">
    /// Specifies the offset into the buffer object's data store from which data will be returned,
    /// measured in bytes.
    /// </param>
    /// <param name="size">Specifies the size in bytes of the data store region being returned.</param>
    /// <param name="data">Specifies a pointer to the location where buffer object data is returned.</param>
    public void GetBufferSubData( GLenum target, GLintptr offset, GLsizeiptr size, IntPtr data )
    {
        GetDelegateForFunction< PFNGLGETBUFFERSUBDATAPROC >( "glGetBufferSubData", out _glGetBufferSubData );

        _glGetBufferSubData( target, offset, size, data );
    }

    /// <summary>
    /// Return a subset of a buffer object's data store.
    /// </summary>
    /// <typeparam name="T">The type of the data to be returned.</typeparam>
    /// <param name="target">
    /// Specifies the target buffer object. The symbolic constant must be <see cref="IGL.GLArrayBuffer"/>,
    /// <see cref="IGL.GLAtomicCounterBuffer"/>, <see cref="IGL.GLCopyReadBuffer"/>, <see cref="IGL.GLCopyWriteBuffer"/>,
    /// <see cref="IGL.GLDispatchIndirectBuffer"/>, <see cref="IGL.GLDrawIndirectBuffer"/>,
    /// <see cref="IGL.GLElementArrayBuffer"/>, <see cref="IGL.GLPixelPackBuffer"/>, <see cref="IGL.GLPixelUnpackBuffer"/>,
    /// <see cref="IGL.GLQueryBuffer"/>, <see cref="IGL.GLShaderStorageBuffer"/>, <see cref="IGL.GL_TEXTURE_BUFFER"/>,
    /// <see cref="IGL.GLTransformFeedbackBuffer"/> or <see cref="IGL.GLUniformBuffer"/>.
    /// </param>
    /// <param name="offsetCount">Specifies the offset into the buffer object's data store from which data will be returned.</param>
    /// <param name="count">Specifies the number of <typeparamref name="T"/>s to be returned.</param>
    /// <param name="data">An array of <typeparamref name="T"/>s that will be filled with the data from the buffer object.</param>
    public void GetBufferSubData< T >( GLenum target, GLintptr offsetCount, GLsizei count, ref T[] data )
        where T : unmanaged
    {
        GetDelegateForFunction< PFNGLGETBUFFERSUBDATAPROC >( "glGetBufferSubData", out _glGetBufferSubData );

        fixed ( T* p = &data[ 0 ] )
        {
            _glGetBufferSubData( target, offsetCount * sizeof( T ), sizeof( T ) * count, ( IntPtr )p );
        }
    }

    // ========================================================================

    /// <summary>
    /// Map a buffer object's data store into the client's address space.
    /// </summary>
    /// <param name="target">
    /// Specifies the target buffer object. The symbolic constant must be <see cref="IGL.GLArrayBuffer"/>,
    /// <see cref="IGL.GLAtomicCounterBuffer"/>, <see cref="IGL.GLCopyReadBuffer"/>, <see cref="IGL.GLCopyWriteBuffer"/>,
    /// <see cref="IGL.GLDispatchIndirectBuffer"/>, <see cref="IGL.GLDrawIndirectBuffer"/>,
    /// <see cref="IGL.GLElementArrayBuffer"/>, <see cref="IGL.GLPixelPackBuffer"/>, <see cref="IGL.GLPixelUnpackBuffer"/>,
    /// <see cref="IGL.GLQueryBuffer"/>, <see cref="IGL.GLShaderStorageBuffer"/>, <see cref="IGL.GL_TEXTURE_BUFFER"/>,
    /// <see cref="IGL.GLTransformFeedbackBuffer"/> or <see cref="IGL.GLUniformBuffer"/>.
    /// </param>
    /// <param name="access">
    /// Specifies a combination of access flags indicating the desired access to the range of the buffer
    /// object's data store. One of <see cref="IGL.GLReadOnly"/>, <see cref="IGL.GLWriteOnly"/> or
    /// <see cref="IGL.GLReadWrite"/>.
    /// </param>
    /// <returns>Returns a pointer to the beginning of the mapped range.</returns>
    public IntPtr MapBuffer( GLenum target, GLenum access )
    {
        GetDelegateForFunction< PFNGLMAPBUFFERPROC >( "glMapBuffer", out _glMapBuffer );

        return _glMapBuffer( target, access );
    }

    /// <summary>
    /// Map a buffer object's data store into the client's address space.
    /// </summary>
    /// <param name="target">
    /// Specifies the target buffer object. The symbolic constant must be <see cref="IGL.GLArrayBuffer"/>,
    /// <see cref="IGL.GLAtomicCounterBuffer"/>, <see cref="IGL.GLCopyReadBuffer"/>, <see cref="IGL.GLCopyWriteBuffer"/>,
    /// <see cref="IGL.GLDispatchIndirectBuffer"/>, <see cref="IGL.GLDrawIndirectBuffer"/>,
    /// <see cref="IGL.GLElementArrayBuffer"/>, <see cref="IGL.GLPixelPackBuffer"/>, <see cref="IGL.GLPixelUnpackBuffer"/>,
    /// <see cref="IGL.GLQueryBuffer"/>, <see cref="IGL.GLShaderStorageBuffer"/>, <see cref="IGL.GL_TEXTURE_BUFFER"/>,
    /// <see cref="IGL.GLTransformFeedbackBuffer"/> or <see cref="IGL.GLUniformBuffer"/>.
    /// </param>
    /// <param name="access">
    /// Specifies a combination of access flags indicating the desired access to the range of the buffer
    /// object's data store. One of <see cref="IGL.GLReadOnly"/>, <see cref="IGL.GLWriteOnly"/> or
    /// <see cref="IGL.GLReadWrite"/>.
    /// </param>
    /// <returns>Returns a type-safe and memory-safe <see cref="System.Span{T}"/> of the buffers data.</returns>
    public Span< T > MapBuffer< T >( GLenum target, GLenum access ) where T : unmanaged
    {
        GLint size;

        GetDelegateForFunction< PFNGLGETBUFFERPARAMETERIVPROC >
            (
             "glGetBufferParameteriv",
             out _glGetBufferParameteriv
            );

        _glGetBufferParameteriv( target, IGL.GLBufferSize, &size );

        GetDelegateForFunction< PFNGLMAPBUFFERPROC >( "glMapBuffer", out _glMapBuffer );

        IntPtr ret = _glMapBuffer( target, access );

        return new Span< T >( ( void* )ret, size / sizeof( T ) );
    }

    // ========================================================================

    /// <summary>
    /// Release a mapped buffer object's data store.
    /// </summary>
    /// <param name="target">
    /// Specifies the target buffer object being unmapped. The symbolic constant must be
    /// <see cref="IGL.GLArrayBuffer"/>, <see cref="IGL.GLAtomicCounterBuffer"/>, <see cref="IGL.GLCopyReadBuffer"/>,
    /// <see cref="IGL.GLCopyWriteBuffer"/>, <see cref="IGL.GLDispatchIndirectBuffer"/>,
    /// <see cref="IGL.GLDrawIndirectBuffer"/>, <see cref="IGL.GLElementArrayBuffer"/>, <see cref="IGL.GLPixelPackBuffer"/>
    /// , <see cref="IGL.GLPixelUnpackBuffer"/>, <see cref="IGL.GLQueryBuffer"/>, <see cref="IGL.GLShaderStorageBuffer"/>,
    /// <see cref="IGL.GL_TEXTURE_BUFFER"/>, <see cref="IGL.GLTransformFeedbackBuffer"/> or <see cref="IGL.GLUniformBuffer"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> unless the data store contents have become corrupt during the time the data store was
    /// mapped. This can occur for system-specific reasons that affect the availability of graphics memory, such as screen
    /// mode changes. In such situations, <see cref="IGLBindings.UnmapBuffer"/> may return <c>false</c> to
    /// indicate that
    /// the contents of the buffer have become corrupt and should be considered undefined. An application must detect this
    /// rare condition and reinitialize the data store.
    /// </returns>
    public GLbool UnmapBuffer( GLenum target )
    {
        GetDelegateForFunction< PFNGLUNMAPBUFFERPROC >( "glUnmapBuffer", out _glUnmapBuffer );

        return _glUnmapBuffer( target );
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetBufferParameteriv( GLenum target, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETBUFFERPARAMETERIVPROC >
            (
             "glGetBufferParameteriv",
             out _glGetBufferParameteriv
            );

        _glGetBufferParameteriv( target, pname, parameters );
    }

    /// <inheritdoc />
    public void GetBufferParameteriv( GLenum target, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETBUFFERPARAMETERIVPROC >
            (
             "glGetBufferParameteriv",
             out _glGetBufferParameteriv
            );

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
            _glGetBufferPointerv( target, pname, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void ReadBuffer( GLenum src )
    {
        GetDelegateForFunction< PFNGLREADBUFFERPROC >( "glReadBuffer", out _glReadBuffer );

        _glReadBuffer( src );
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

    /// <inheritdoc />
    public void DrawBuffer( GLenum buf )
    {
        GetDelegateForFunction< PFNGLDRAWBUFFERPROC >( "glDrawBuffer", out _glDrawBuffer );

        _glDrawBuffer( buf );
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
    public void GetBufferParameteri64V( GLenum target, GLenum pname, GLint64* parameters )
    {
        GetDelegateForFunction< PFNGLGETBUFFERPARAMETERI64VPROC >
            (
             "glGetBufferParameteri64v",
             out _glGetBufferParameteri64v
            );

        _glGetBufferParameteri64v( target, pname, parameters );
    }

    /// <inheritdoc />
    public void GetBufferParameteri64V( GLenum target, GLenum pname, ref GLint64[] parameters )
    {
        GetDelegateForFunction< PFNGLGETBUFFERPARAMETERI64VPROC >
            (
             "glGetBufferParameteri64v",
             out _glGetBufferParameteri64v
            );

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

    public void ClearBufferData< T >( GLenum target, GLenum internalFormat, GLenum format, GLenum type, T[] data )
        where T : unmanaged
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERDATAPROC >( "glClearBufferData", out _glClearBufferData );

        fixed ( T* t = &data[ 0 ] )
        {
            _glClearBufferData( target, internalFormat, format, type, ( IntPtr )t );
        }
    }

    // ========================================================================

    public void ClearBufferSubData( GLenum target, GLenum internalFormat, GLintptr offset, GLsizeiptr size,
                                    GLenum format, GLenum type,
                                    IntPtr data )
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERSUBDATAPROC >( "glClearBufferSubData", out _glClearBufferSubData );

        _glClearBufferSubData( target, internalFormat, offset, size, format, type, data );
    }

    public void ClearBufferSubData< T >( GLenum target, GLenum internalFormat, GLintptr offset, GLsizeiptr size,
                                         GLenum format, GLenum type,
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
        GetDelegateForFunction< PFNGLINVALIDATEBUFFERSUBDATAPROC >
            (
             "glInvalidateBufferSubData",
             out _glInvalidateBufferSubData
            );

        _glInvalidateBufferSubData( buffer, offset, length );
    }

    // ========================================================================

    public void InvalidateBufferData( GLuint buffer )
    {
        GetDelegateForFunction< PFNGLINVALIDATEBUFFERDATAPROC >
            (
             "glInvalidateBufferData",
             out _glInvalidateBufferData
            );

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

    /// <summary>
    /// Clear a buffer to an unsigned integer value. Refer to <see href="https://docs.gl/gl4/glClearBuffer"/> for details
    /// on how to use this function.
    /// </summary>
    /// <param name="buffer">
    /// Specifies the buffer to clear. <paramref name="buffer"/> must be one of <see cref="IGL.GLColor"/>,
    /// <see cref="IGL.GLDepth"/>, <see cref="IGL.GLStencil"/>.
    /// </param>
    /// <param name="drawbuffer">Specify a partical draw buffer to clear.</param>
    /// <param name="value">Specifies the value to clear the buffer to.</param>
    public void ClearBufferuiv( GLenum buffer, GLint drawbuffer, GLuint* value )
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERUIVPROC >( "glClearBufferuiv", out _glClearBufferuiv );

        _glClearBufferuiv( buffer, drawbuffer, value );
    }

    /// <summary>
    /// Clear a buffer to an unsigned integer value. Refer to <see href="https://docs.gl/gl4/glClearBuffer"/> for details
    /// on how to use this function.
    /// </summary>
    /// <param name="buffer">
    /// Specifies the buffer to clear. <paramref name="buffer"/> must be one of <see cref="IGL.GLColor"/>,
    /// <see cref="IGL.GLDepth"/>, <see cref="IGL.GLStencil"/>.
    /// </param>
    /// <param name="drawbuffer">Specify a partical draw buffer to clear.</param>
    /// <param name="value">Specifies the value to clear the buffer to.</param>
    public void ClearBufferuiv( GLenum buffer, GLint drawbuffer, GLuint[] value )
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERUIVPROC >( "glClearBufferuiv", out _glClearBufferuiv );

        fixed ( GLuint* p = &value[ 0 ] )
        {
            _glClearBufferuiv( buffer, drawbuffer, p );
        }
    }

    // ========================================================================

    /// <summary>
    /// Clear a buffer to a floating point value. Refer to <see href="https://docs.gl/gl4/glClearBuffer"/> for details on
    /// how to use this function.
    /// </summary>
    /// <param name="buffer">
    /// Specifies the buffer to clear. <paramref name="buffer"/> must be one of <see cref="IGL.GLColor"/>,
    /// <see cref="IGL.GLDepth"/>, <see cref="IGL.GLStencil"/>.
    /// </param>
    /// <param name="drawbuffer">Specify a partical draw buffer to clear.</param>
    /// <param name="value">Specifies the value to clear the buffer to.</param>
    public void ClearBufferfv( GLenum buffer, GLint drawbuffer, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERFVPROC >( "glClearBufferfv", out _glClearBufferfv );

        _glClearBufferfv( buffer, drawbuffer, value );
    }

    /// <summary>
    /// Clear a buffer to a floating point value. Refer to <see href="https://docs.gl/gl4/glClearBuffer"/> for details on
    /// how to use this function.
    /// </summary>
    /// <param name="buffer">
    /// Specifies the buffer to clear. <paramref name="buffer"/> must be one of <see cref="IGL.GLColor"/>,
    /// <see cref="IGL.GLDepth"/>, <see cref="IGL.GLStencil"/>.
    /// </param>
    /// <param name="drawbuffer">Specify a partical draw buffer to clear.</param>
    /// <param name="value">Specifies the value to clear the buffer to.</param>
    public void ClearBufferfv( GLenum buffer, GLint drawbuffer, GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERFVPROC >( "glClearBufferfv", out _glClearBufferfv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glClearBufferfv( buffer, drawbuffer, p );
        }
    }

    // ========================================================================

    /// <summary>
    /// Clear a buffer to a floating point value. Refer to <see href="https://docs.gl/gl4/glClearBuffer"/> for details on
    /// how to use this function.
    /// </summary>
    /// <param name="buffer">
    /// Specifies the buffer to clear. <paramref name="buffer"/> must be one of
    /// <see cref="IGL.GLDepthStencil"/>.
    /// </param>
    /// <param name="drawbuffer">Must be zero.</param>
    /// <param name="depth">Specifies the value to clear the depth buffer to.</param>
    /// <param name="stencil">Specifies the value to clear the stencil buffer to.</param>
    public void ClearBufferfi( GLenum buffer, GLint drawbuffer, GLfloat depth, GLint stencil )
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERFIPROC >( "glClearBufferfi", out _glClearBufferfi );

        _glClearBufferfi( buffer, drawbuffer, depth, stencil );
    }

    // ========================================================================

    public void BufferStorage( GLenum target, GLsizeiptr size, IntPtr data, GLbitfield flags )
    {
        GetDelegateForFunction< PFNGLBUFFERSTORAGEPROC >( "glBufferStorage", out _glBufferStorage );

        _glBufferStorage( target, size, data, flags );
    }

    public void BufferStorage< T >( GLenum target, T[] data, GLbitfield flags ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLBUFFERSTORAGEPROC >( "glBufferStorage", out _glBufferStorage );

        fixed ( void* pData = &data[ 0 ] )
        {
            _glBufferStorage( target, data.Length * sizeof( T ), ( IntPtr )pData, flags );
        }
    }

    // ========================================================================

    public void BindBuffersBase( GLenum target, GLuint first, GLsizei count, GLuint* buffers )
    {
        GetDelegateForFunction< PFNGLBINDBUFFERSBASEPROC >( "glBindBuffersBase", out _glBindBuffersBase );

        _glBindBuffersBase( target, first, count, buffers );
    }

    public void BindBuffersBase( GLenum target, GLuint first, GLuint[] buffers )
    {
        GetDelegateForFunction< PFNGLBINDBUFFERSBASEPROC >( "glBindBuffersBase", out _glBindBuffersBase );

        fixed ( GLuint* pBuffers = &buffers[ 0 ] )
        {
            _glBindBuffersBase( target, first, buffers.Length, pBuffers );
        }
    }

    // ========================================================================

    public void BindBuffersRange( GLenum target, GLuint first, GLsizei count, GLuint* buffers, GLintptr* offsets,
                                  GLsizeiptr* sizes )
    {
        GetDelegateForFunction< PFNGLBINDBUFFERSRANGEPROC >( "glBindBuffersRange", out _glBindBuffersRange );

        _glBindBuffersRange( target, first, count, buffers, offsets, sizes );
    }

    public void BindBuffersRange( GLenum target, GLuint first, GLuint[] buffers, GLintptr[] offsets,
                                  GLsizeiptr[] sizes )
    {
        GetDelegateForFunction< PFNGLBINDBUFFERSRANGEPROC >( "glBindBuffersRange", out _glBindBuffersRange );

        fixed ( GLuint* pBuffers = &buffers[ 0 ] )
        {
            fixed ( GLintptr* pOffsets = &offsets[ 0 ] )
            {
                fixed ( GLsizeiptr* pSizes = &sizes[ 0 ] )
                {
                    _glBindBuffersRange( target, first, buffers.Length, pBuffers, pOffsets, pSizes );
                }
            }
        }
    }

    #endregion Buffer Object Bindings

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region Debug Bindings

    public void DebugMessageControl( GLenum source, GLenum type, GLenum severity, GLsizei count, GLuint* ids,
                                     GLbool enabled )
    {
        GetDelegateForFunction< PFNGLDEBUGMESSAGECONTROLPROC >( "glDebugMessageControl", out _glDebugMessageControl );

        _glDebugMessageControl( source, type, severity, count, ids, enabled );
    }

    // ========================================================================

    public void DebugMessageInsert( GLenum source, GLenum type, GLuint id, GLenum severity, GLsizei length,
                                    GLchar* buf )
    {
        GetDelegateForFunction< PFNGLDEBUGMESSAGEINSERTPROC >( "glDebugMessageInsert", out _glDebugMessageInsert );

        _glDebugMessageInsert( source, type, id, severity, length, buf );
    }

    // ========================================================================

    public void DebugMessageCallback( GLDEBUGPROC callback, void* userParam )
    {
        GetDelegateForFunction< PFNGLDEBUGMESSAGECALLBACKPROC >
            (
             "glDebugMessageCallback",
             out _glDebugMessageCallback
            );

        _glDebugMessageCallback( callback, ( IntPtr )userParam );
    }

    /// <summary>
    /// Processes an OpenGL debug message, providing details such as the source, type,
    /// severity, and description of the message.
    /// </summary>
    /// <param name="source">
    /// An identifier for the origin of the debug message. It determines where the message
    /// originated, such as API calls or shader compilers.
    /// </param>
    /// <param name="type">
    /// The type of the OpenGL debug message, indicating the nature of the event (e.g.,
    /// errors, performance warnings).
    /// </param>
    /// <param name="id">
    /// A numeric identifier for the debug message. This can be used to differentiate
    /// specific events.
    /// </param>
    /// <param name="severity">
    /// Specifies the level of severity of the event, identifying whether the message is
    /// informational, a warning, or critical.
    /// </param>
    /// <param name="length">
    /// The length of the debug message string, as provided by OpenGL. This parameter
    /// may be useful for handling the incoming message data.
    /// </param>
    /// <param name="msg">
    /// A string describing the debug message. This provides detailed information regarding
    /// the event or error reported by OpenGL.
    /// </param>
    /// <param name="userParam">
    /// An optional user-defined pointer passed to the callback. This parameter allows for
    /// additional context or state to be associated with the debug message.
    /// </param>
    public void MessageCallback( int source,
                                 int type,
                                 GLuint id,
                                 int severity,
                                 int length,
                                 GLchar* msg,
                                 IntPtr userParam )
    {
        var message = new string( ( GLbyte* )msg, 0, length, Encoding.UTF8 );

        string srcStr = source switch
                        {
                            IGL.GL_DEBUG_SOURCE_API             => "API",
                            IGL.GL_DEBUG_SOURCE_WINDOW_SYSTEM   => "WINDOW SYSTEM",
                            IGL.GL_DEBUG_SOURCE_SHADER_COMPILER => "SHADER COMPILER",
                            IGL.GL_DEBUG_SOURCE_THIRD_PARTY     => "THIRD PARTY",
                            IGL.GL_DEBUG_SOURCE_APPLICATION     => "APPLICATION",
                            IGL.GL_DEBUG_SOURCE_OTHER           => "OTHER",
                            var _                               => "UNKNOWN"
                        };

        string typeStr = type switch
                         {
                             IGL.GL_DEBUG_TYPE_ERROR               => "ERROR",
                             IGL.GL_DEBUG_TYPE_DEPRECATED_BEHAVIOR => "DEPRECATED_BEHAVIOR",
                             IGL.GL_DEBUG_TYPE_UNDEFINED_BEHAVIOR  => "UNDEFINED_BEHAVIOR",
                             IGL.GL_DEBUG_TYPE_PORTABILITY         => "PORTABILITY",
                             IGL.GL_DEBUG_TYPE_PERFORMANCE         => "PERFORMANCE",
                             IGL.GL_DEBUG_TYPE_MARKER              => "MARKER",
                             IGL.GL_DEBUG_TYPE_OTHER               => "OTHER",
                             var _                                 => "UNKNOWN"
                         };

        string severityStr = severity switch
                             {
                                 IGL.GL_DEBUG_SEVERITY_NOTIFICATION => "NOTIFICATION",
                                 IGL.GL_DEBUG_SEVERITY_LOW          => "LOW",
                                 IGL.GL_DEBUG_SEVERITY_MEDIUM       => "MEDIUM",
                                 IGL.GL_DEBUG_SEVERITY_HIGH         => "HIGH",
                                 var _                              => "UNKNOWN"
                             };

        Logger.Error( $"{srcStr}, {typeStr}, {severityStr}, {id}: {message}" );
    }

    // ========================================================================

    public GLuint GetDebugMessageLog( GLuint count,
                                      GLsizei bufsize,
                                      GLenum* sources,
                                      GLenum* types,
                                      GLuint* ids,
                                      GLenum* severities,
                                      GLsizei* lengths,
                                      GLchar* messageLog )
    {
        GetDelegateForFunction< PFNGLGETDEBUGMESSAGELOGPROC >( "glGetDebugMessageLog", out _glGetDebugMessageLog );

        return _glGetDebugMessageLog( count, bufsize, sources, types, ids, severities, lengths, messageLog );
    }

    public GLuint GetDebugMessageLog( GLuint count,
                                      GLsizei bufSize,
                                      out GLenum[] sources,
                                      out GLenum[] types,
                                      out GLuint[] ids,
                                      out GLenum[] severities,
                                      out string[] messageLog )
    {
        sources    = new GLenum[ count ];
        types      = new GLenum[ count ];
        ids        = new GLuint[ count ];
        severities = new GLenum[ count ];

        var messageLogBytes = new GLchar[ bufSize ];
        var lengths         = new GLsizei[ count ];

        GetDelegateForFunction< PFNGLGETDEBUGMESSAGELOGPROC >( "glGetDebugMessageLog", out _glGetDebugMessageLog );

        fixed ( GLenum* pSources = &sources[ 0 ] )
        {
            fixed ( GLenum* pTypes = &types[ 0 ] )
            {
                fixed ( GLuint* pIds = &ids[ 0 ] )
                {
                    fixed ( GLenum* pSeverities = &severities[ 0 ] )
                    {
                        fixed ( GLsizei* pLengths = &lengths[ 0 ] )
                        {
                            fixed ( GLchar* pMessageLogBytes = &messageLogBytes[ 0 ] )
                            {
                                GLchar* pstart = pMessageLogBytes;
                                uint ret = _glGetDebugMessageLog
                                    (
                                     count,
                                     bufSize,
                                     pSources,
                                     pTypes,
                                     pIds,
                                     pSeverities,
                                     pLengths,
                                     pMessageLogBytes
                                    );

                                messageLog = new string[ count ];

                                for ( var i = 0; i < count; i++ )
                                {
                                    messageLog[ i ] =  new string( ( GLbyte* )pstart, 0, lengths[ i ], Encoding.UTF8 );
                                    pstart          += lengths[ i ];
                                }

                                Array.Resize( ref sources, ( int )ret );
                                Array.Resize( ref types, ( int )ret );
                                Array.Resize( ref ids, ( int )ret );
                                Array.Resize( ref severities, ( int )ret );
                                Array.Resize( ref messageLog, ( int )ret );

                                return ret;
                            }
                        }
                    }
                }
            }
        }
    }

    // ========================================================================

    public void PushDebugGroup( GLenum source, GLuint id, GLsizei length, GLchar* message )
    {
        GetDelegateForFunction< PFNGLPUSHDEBUGGROUPPROC >( "glPushDebugGroup", out _glPushDebugGroup );

        _glPushDebugGroup( source, id, length, message );
    }

    public void PushDebugGroup( GLenum source, GLuint id, string message )
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes( message );

        GetDelegateForFunction< PFNGLPUSHDEBUGGROUPPROC >( "glPushDebugGroup", out _glPushDebugGroup );

        fixed ( GLchar* pMessageBytes = messageBytes )
        {
            _glPushDebugGroup( source, id, messageBytes.Length, pMessageBytes );
        }
    }

    // ========================================================================

    public void PopDebugGroup()
    {
        GetDelegateForFunction< PFNGLPOPDEBUGGROUPPROC >( "glPopDebugGroup", out _glPopDebugGroup );

        _glPopDebugGroup();
    }

    #endregion Debug Bindings

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region Error Bindings

    /// <inheritdoc />
    public GLenum GetError()
    {
        GetDelegateForFunction< PFNGLGETERRORPROC >( "glGetError", out _glGetError );

        return _glGetError();
    }

    // ========================================================================

    /// <summary>
    /// Handles any GL Errors generated by profiling methods.
    /// </summary>
    public void CheckErrors()
    {
        GetDelegateForFunction< PFNGLGETERRORPROC >( "glGetError", out _glGetError );

        int error = _glGetError();

        while ( error != IGL.GLNoError )
        {
//            ErrorListener.OnError( error );

            error = _glGetError();
        }
    }

    #endregion Error Bindings

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region FrameBuffer Bindings

    public void FramebufferParameteri( GLenum target, GLenum pname, GLint param )
    {
        GetDelegateForFunction< PFNGLFRAMEBUFFERPARAMETERIPROC >
            (
             "glFramebufferParameteri",
             out _glFramebufferParameteri
            );

        _glFramebufferParameteri( target, pname, param );
    }

    // ========================================================================

    public void GetFramebufferParameteriv( GLenum target, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETFRAMEBUFFERPARAMETERIVPROC >
            (
             "glGetFramebufferParameteriv",
             out _glGetFramebufferParameteriv
            );

        _glGetFramebufferParameteriv( target, pname, parameters );
    }

    public void GetFramebufferParameteriv( GLenum target, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETFRAMEBUFFERPARAMETERIVPROC >
            (
             "glGetFramebufferParameteriv",
             out _glGetFramebufferParameteriv
            );

        fixed ( GLint* pParameters = &parameters[ 0 ] )
        {
            _glGetFramebufferParameteriv( target, pname, pParameters );
        }
    }

    #endregion FrameBuffer Bindings

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region Program Bindings

    /// <inheritdoc />
    public GLuint CreateProgram()
    {
        GetDelegateForFunction< PFNGLCREATEPROGRAMPROC >( "glCreateProgram", out _glCreateProgram );

        //TODO: Error checking here

        return _glCreateProgram();
    }

    // ========================================================================

    /// <inheritdoc />
    public GLbool IsProgram( GLint program )
    {
        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLISPROGRAMPROC >( "glIsProgram", out _glIsProgram );

        return _glIsProgram( ( uint )program );
    }

    // ========================================================================

    /// <inheritdoc />
    public void LinkProgram( GLint program )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == InvalidShaderProgram ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLLINKPROGRAMPROC >( "glLinkProgram", out _glLinkProgram );

        _glLinkProgram( ( uint )program );
    }

    // ========================================================================

    /// <inheritdoc />
    public void UseProgram( GLint program )
    {
        if ( program == InvalidShaderProgram )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLUSEPROGRAMPROC >( "glUseProgram", out _glUseProgram );

        _glUseProgram( ( uint )program );
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetProgramiv( GLint program, GLenum pname, GLint* parameters )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == InvalidShaderProgram ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETPROGRAMIVPROC >( "glGetProgramiv", out _glGetProgramiv );

        _glGetProgramiv( ( uint )program, pname, parameters );
    }

    /// <inheritdoc />
    public void GetProgramiv( GLint program, GLenum pname, ref GLint[] parameters )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == InvalidShaderProgram ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETPROGRAMIVPROC >( "glGetProgramiv", out _glGetProgramiv );

        fixed ( GLint* pparams = &parameters[ 0 ] )
        {
            _glGetProgramiv( ( uint )program, pname, pparams );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetProgramInfoLog( GLint program, GLsizei bufSize, GLsizei* length, GLchar* infoLog )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == InvalidShaderProgram ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETPROGRAMINFOLOGPROC >( "glGetProgramInfoLog", out _glGetProgramInfoLog );

        _glGetProgramInfoLog( ( uint )program, bufSize, length, infoLog );
    }

    /// <inheritdoc />
    public string GetProgramInfoLog( GLint program, GLsizei bufSize )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == InvalidShaderProgram ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GLchar* infoLog = stackalloc GLchar[ bufSize ];
        GLsizei len;

        GetDelegateForFunction< PFNGLGETPROGRAMINFOLOGPROC >( "glGetProgramInfoLog", out _glGetProgramInfoLog );

        _glGetProgramInfoLog( ( uint )program, bufSize, &len, infoLog );

        return new string( ( GLbyte* )infoLog, 0, len, Encoding.UTF8 );
    }

    // ========================================================================

    /// <inheritdoc />
    public bool ValidateProgram( int program )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == InvalidShaderProgram ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLVALIDATEPROGRAMPROC >( "glValidateProgram", out _glValidateProgram );

        return _glValidateProgram( ( uint )program );
    }

    // ========================================================================

    public void GetProgramStageiv( GLuint program, GLenum shadertype, GLenum pname, GLint* values )
    {
        GetDelegateForFunction< PFNGLGETPROGRAMSTAGEIVPROC >( "glGetProgramStageiv", out _glGetProgramStageiv );

        _glGetProgramStageiv( program, shadertype, pname, values );
    }

    public void GetProgramStageiv( GLuint program, GLenum shadertype, GLenum pname, ref GLint[] values )
    {
        GetDelegateForFunction< PFNGLGETPROGRAMSTAGEIVPROC >( "glGetProgramStageiv", out _glGetProgramStageiv );

        fixed ( GLint* p = &values[ 0 ] )
        {
            _glGetProgramStageiv( program, shadertype, pname, p );
        }
    }

    // ========================================================================

    public void PatchParameteri( GLenum pname, GLint value )
    {
        GetDelegateForFunction< PFNGLPATCHPARAMETERIPROC >( "glPatchParameteri", out _glPatchParameteri );

        _glPatchParameteri( pname, value );
    }

    // ========================================================================

    public void GetProgramBinary( GLuint program, GLsizei bufSize, GLsizei* length, GLenum* binaryFormat,
                                  IntPtr binary )
    {
        GetDelegateForFunction< PFNGLGETPROGRAMBINARYPROC >( "glGetProgramBinary", out _glGetProgramBinary );

        _glGetProgramBinary( program, bufSize, length, binaryFormat, binary );
    }

    public byte[] GetProgramBinary( GLuint program, GLsizei bufSize, out GLenum binaryFormat )
    {
        var     binary = new byte[ bufSize ];
        GLsizei length;

        GetDelegateForFunction< PFNGLGETPROGRAMBINARYPROC >( "glGetProgramBinary", out _glGetProgramBinary );

        fixed ( byte* pBinary = &binary[ 0 ] )
        {
            fixed ( GLenum* pBinaryFormat = &binaryFormat )
            {
                _glGetProgramBinary( program, bufSize, &length, pBinaryFormat, ( IntPtr )pBinary );
            }
        }

        Array.Resize( ref binary, length );

        return binary;
    }

    // ========================================================================

    public void ProgramBinary( GLuint program, GLenum binaryFormat, IntPtr binary, GLsizei length )
    {
        GetDelegateForFunction< PFNGLPROGRAMBINARYPROC >( "glProgramBinary", out _glProgramBinary );

        _glProgramBinary( program, binaryFormat, binary, length );
    }

    public void ProgramBinary( GLuint program, GLenum binaryFormat, byte[] binary )
    {
        GetDelegateForFunction< PFNGLPROGRAMBINARYPROC >( "glProgramBinary", out _glProgramBinary );

        fixed ( byte* pBinary = &binary[ 0 ] )
        {
            _glProgramBinary( program, binaryFormat, ( IntPtr )pBinary, binary.Length );
        }
    }

    // ========================================================================

    public void ProgramParameteri( GLuint program, GLenum pname, GLint value )
    {
        GetDelegateForFunction< PFNGLPROGRAMPARAMETERIPROC >( "glProgramParameteri", out _glProgramParameteri );

        _glProgramParameteri( program, pname, value );
    }

    // ========================================================================

    public void UseProgramStages( GLuint pipeline, GLbitfield stages, GLuint program )
    {
        GetDelegateForFunction< PFNGLUSEPROGRAMSTAGESPROC >( "glUseProgramStages", out _glUseProgramStages );

        _glUseProgramStages( pipeline, stages, program );
    }

    // ========================================================================

    public void ActiveShaderProgram( GLuint pipeline, GLuint program )
    {
        GetDelegateForFunction< PFNGLACTIVESHADERPROGRAMPROC >( "glActiveShaderProgram", out _glActiveShaderProgram );

        _glActiveShaderProgram( pipeline, program );
    }

    // ========================================================================

    public GLuint CreateShaderProgramv( GLenum type, GLsizei count, GLchar** strings )
    {
        GetDelegateForFunction< PFNGLCREATESHADERPROGRAMVPROC >
            (
             "glCreateShaderProgramv",
             out _glCreateShaderProgramv
            );

        return _glCreateShaderProgramv( type, count, strings );
    }

    public GLuint CreateShaderProgramv( GLenum type, string[] strings )
    {
        var stringsBytes = new GLchar[ strings.Length ][];

        for ( var i = 0; i < strings.Length; i++ )
        {
            stringsBytes[ i ] = Encoding.UTF8.GetBytes( strings[ i ] );
        }

        var stringsPtrs = new GLchar*[ strings.Length ];

        for ( var i = 0; i < strings.Length; i++ )
        {
            fixed ( GLchar* pString = &stringsBytes[ i ][ 0 ] )
            {
                stringsPtrs[ i ] = pString;
            }
        }

        GetDelegateForFunction< PFNGLCREATESHADERPROGRAMVPROC >
            (
             "glCreateShaderProgramv",
             out _glCreateShaderProgramv
            );

        fixed ( GLchar** pStrings = &stringsPtrs[ 0 ] )
        {
            return _glCreateShaderProgramv( type, strings.Length, pStrings );
        }
    }

    // ========================================================================

    public void BindProgramPipeline( GLuint pipeline )
    {
        GetDelegateForFunction< PFNGLBINDPROGRAMPIPELINEPROC >( "glBindProgramPipeline", out _glBindProgramPipeline );

        _glBindProgramPipeline( pipeline );
    }

    // ========================================================================

    public void DeleteProgramPipelines( GLsizei n, GLuint* pipelines )
    {
        GetDelegateForFunction< PFNGLDELETEPROGRAMPIPELINESPROC >
            (
             "glDeleteProgramPipelines",
             out _glDeleteProgramPipelines
            );

        _glDeleteProgramPipelines( n, pipelines );
    }

    public void DeleteProgramPipelines( params GLuint[] pipelines )
    {
        GetDelegateForFunction< PFNGLDELETEPROGRAMPIPELINESPROC >
            (
             "glDeleteProgramPipelines",
             out _glDeleteProgramPipelines
            );

        fixed ( GLuint* pPipelines = &pipelines[ 0 ] )
        {
            _glDeleteProgramPipelines( pipelines.Length, pPipelines );
        }
    }

    // ========================================================================

    public void GenProgramPipelines( GLsizei n, GLuint* pipelines )
    {
        GetDelegateForFunction< PFNGLGENPROGRAMPIPELINESPROC >( "glGenProgramPipelines", out _glGenProgramPipelines );

        _glGenProgramPipelines( n, pipelines );
    }

    public GLuint[] GenProgramPipelines( GLsizei n )
    {
        var pipelines = new GLuint[ n ];

        GetDelegateForFunction< PFNGLGENPROGRAMPIPELINESPROC >( "glGenProgramPipelines", out _glGenProgramPipelines );

        fixed ( GLuint* pPipelines = &pipelines[ 0 ] )
        {
            _glGenProgramPipelines( n, pPipelines );
        }

        return pipelines;
    }

    public GLuint GenProgramPipeline()
    {
        return GenProgramPipelines( 1 )[ 0 ];
    }

    // ========================================================================

    public GLbool IsProgramPipeline( GLuint pipeline )
    {
        GetDelegateForFunction< PFNGLISPROGRAMPIPELINEPROC >( "glIsProgramPipeline", out _glIsProgramPipeline );

        return _glIsProgramPipeline( pipeline );
    }

    // ========================================================================

    public void GetProgramPipelineiv( GLuint pipeline, GLenum pname, GLint* param )
    {
        GetDelegateForFunction< PFNGLGETPROGRAMPIPELINEIVPROC >
            (
             "glGetProgramPipelineiv",
             out _glGetProgramPipelineiv
            );

        _glGetProgramPipelineiv( pipeline, pname, param );
    }

    public void GetProgramPipelineiv( GLuint pipeline, GLenum pname, ref GLint[] param )
    {
        GetDelegateForFunction< PFNGLGETPROGRAMPIPELINEIVPROC >
            (
             "glGetProgramPipelineiv",
             out _glGetProgramPipelineiv
            );

        fixed ( GLint* pParam = &param[ 0 ] )
        {
            _glGetProgramPipelineiv( pipeline, pname, pParam );
        }
    }

    // ========================================================================

    public void ProgramUniform1i( GLuint program, GLint location, GLint v0 )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM1IPROC >( "glProgramUniform1i", out _glProgramUniform1i );

        _glProgramUniform1i( program, location, v0 );
    }

    // ========================================================================

    public void ProgramUniform1iv( GLuint program, GLint location, GLsizei count, GLint* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM1IVPROC >( "glProgramUniform1iv", out _glProgramUniform1iv );

        _glProgramUniform1iv( program, location, count, value );
    }

    public void ProgramUniform1iv( GLuint program, GLint location, GLint[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM1IVPROC >( "glProgramUniform1iv", out _glProgramUniform1iv );

        fixed ( GLint* pValue = &value[ 0 ] )
        {
            _glProgramUniform1iv( program, location, value.Length, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform1f( GLuint program, GLint location, GLfloat v0 )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM1FPROC >( "glProgramUniform1f", out _glProgramUniform1f );

        _glProgramUniform1f( program, location, v0 );
    }

    // ========================================================================

    public void ProgramUniform1fv( GLuint program, GLint location, GLsizei count, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM1FVPROC >( "glProgramUniform1fv", out _glProgramUniform1fv );

        _glProgramUniform1fv( program, location, count, value );
    }

    public void ProgramUniform1fv( GLuint program, GLint location, GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM1FVPROC >( "glProgramUniform1fv", out _glProgramUniform1fv );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniform1fv( program, location, value.Length, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform1d( GLuint program, GLint location, GLdouble v0 )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM1DPROC >( "glProgramUniform1d", out _glProgramUniform1d );

        _glProgramUniform1d( program, location, v0 );
    }

    // ========================================================================

    public void ProgramUniform1dv( GLuint program, GLint location, GLsizei count, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM1DVPROC >( "glProgramUniform1dv", out _glProgramUniform1dv );

        _glProgramUniform1dv( program, location, count, value );
    }

    public void ProgramUniform1dv( GLuint program, GLint location, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM1DVPROC >( "glProgramUniform1dv", out _glProgramUniform1dv );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniform1dv( program, location, value.Length, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform1ui( GLuint program, GLint location, GLuint v0 )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM1UIPROC >( "glProgramUniform1ui", out _glProgramUniform1ui );

        _glProgramUniform1ui( program, location, v0 );
    }

    // ========================================================================

    public void ProgramUniform1uiv( GLuint program, GLint location, GLsizei count, GLuint* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM1UIVPROC >( "glProgramUniform1uiv", out _glProgramUniform1uiv );

        _glProgramUniform1uiv( program, location, count, value );
    }

    public void ProgramUniform1uiv( GLuint program, GLint location, GLuint[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM1UIVPROC >( "glProgramUniform1uiv", out _glProgramUniform1uiv );

        fixed ( GLuint* pValue = &value[ 0 ] )
        {
            _glProgramUniform1uiv( program, location, value.Length, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform2i( GLuint program, GLint location, GLint v0, GLint v1 )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM2IPROC >( "glProgramUniform2i", out _glProgramUniform2i );

        _glProgramUniform2i( program, location, v0, v1 );
    }

    // ========================================================================

    public void ProgramUniform2iv( GLuint program, GLint location, GLsizei count, GLint* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM2IVPROC >( "glProgramUniform2iv", out _glProgramUniform2iv );

        _glProgramUniform2iv( program, location, count, value );
    }

    public void ProgramUniform2iv( GLuint program, GLint location, GLint[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM2IVPROC >( "glProgramUniform2iv", out _glProgramUniform2iv );

        fixed ( GLint* pValue = &value[ 0 ] )
        {
            _glProgramUniform2iv( program, location, value.Length / 2, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform2f( GLuint program, GLint location, GLfloat v0, GLfloat v1 )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM2FPROC >( "glProgramUniform2f", out _glProgramUniform2f );

        _glProgramUniform2f( program, location, v0, v1 );
    }

    // ========================================================================

    public void ProgramUniform2fv( GLuint program, GLint location, GLsizei count, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM2FVPROC >( "glProgramUniform2fv", out _glProgramUniform2fv );

        _glProgramUniform2fv( program, location, count, value );
    }

    public void ProgramUniform2fv( GLuint program, GLint location, GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM2FVPROC >( "glProgramUniform2fv", out _glProgramUniform2fv );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniform2fv( program, location, value.Length / 2, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform2d( GLuint program, GLint location, GLdouble v0, GLdouble v1 )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM2DPROC >( "glProgramUniform2d", out _glProgramUniform2d );

        _glProgramUniform2d( program, location, v0, v1 );
    }

    // ========================================================================

    public void ProgramUniform2dv( GLuint program, GLint location, GLsizei count, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM2DVPROC >( "glProgramUniform2dv", out _glProgramUniform2dv );

        _glProgramUniform2dv( program, location, count, value );
    }

    public void ProgramUniform2dv( GLuint program, GLint location, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM2DVPROC >( "glProgramUniform2dv", out _glProgramUniform2dv );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniform2dv( program, location, value.Length / 2, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform2ui( GLuint program, GLint location, GLuint v0, GLuint v1 )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM2UIPROC >( "glProgramUniform2ui", out _glProgramUniform2ui );

        _glProgramUniform2ui( program, location, v0, v1 );
    }

    // ========================================================================

    public void ProgramUniform2uiv( GLuint program, GLint location, GLsizei count, GLuint* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM2UIVPROC >( "glProgramUniform2uiv", out _glProgramUniform2uiv );

        _glProgramUniform2uiv( program, location, count, value );
    }

    public void ProgramUniform2uiv( GLuint program, GLint location, GLuint[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM2UIVPROC >( "glProgramUniform2uiv", out _glProgramUniform2uiv );

        fixed ( GLuint* pValue = &value[ 0 ] )
        {
            _glProgramUniform2uiv( program, location, value.Length / 2, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform3i( GLuint program, GLint location, GLint v0, GLint v1, GLint v2 )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM3IPROC >( "glProgramUniform3i", out _glProgramUniform3i );

        _glProgramUniform3i( program, location, v0, v1, v2 );
    }

    // ========================================================================

    public void ProgramUniform3iv( GLuint program, GLint location, GLsizei count, GLint* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM3IVPROC >( "glProgramUniform3iv", out _glProgramUniform3iv );

        _glProgramUniform3iv( program, location, count, value );
    }

    public void ProgramUniform3iv( GLuint program, GLint location, GLint[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM3IVPROC >( "glProgramUniform3iv", out _glProgramUniform3iv );

        fixed ( GLint* pValue = &value[ 0 ] )
        {
            _glProgramUniform3iv( program, location, value.Length / 3, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform3f( GLuint program, GLint location, GLfloat v0, GLfloat v1, GLfloat v2 )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM3FPROC >( "glProgramUniform3f", out _glProgramUniform3f );

        _glProgramUniform3f( program, location, v0, v1, v2 );
    }

    // ========================================================================

    public void ProgramUniform3fv( GLuint program, GLint location, GLsizei count, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM3FVPROC >( "glProgramUniform3fv", out _glProgramUniform3fv );

        _glProgramUniform3fv( program, location, count, value );
    }

    public void ProgramUniform3fv( GLuint program, GLint location, GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM3FVPROC >( "glProgramUniform3fv", out _glProgramUniform3fv );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniform3fv( program, location, value.Length / 3, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform3d( GLuint program, GLint location, GLdouble v0, GLdouble v1, GLdouble v2 )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM3DPROC >( "glProgramUniform3d", out _glProgramUniform3d );

        _glProgramUniform3d( program, location, v0, v1, v2 );
    }

    // ========================================================================

    public void ProgramUniform3dv( GLuint program, GLint location, GLsizei count, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM3DVPROC >( "glProgramUniform3dv", out _glProgramUniform3dv );

        _glProgramUniform3dv( program, location, count, value );
    }

    public void ProgramUniform3dv( GLuint program, GLint location, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM3DVPROC >( "glProgramUniform3dv", out _glProgramUniform3dv );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniform3dv( program, location, value.Length / 3, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform3ui( GLuint program, GLint location, GLuint v0, GLuint v1, GLuint v2 )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM3UIPROC >( "glProgramUniform3ui", out _glProgramUniform3ui );

        _glProgramUniform3ui( program, location, v0, v1, v2 );
    }

    // ========================================================================

    public void ProgramUniform3uiv( GLuint program, GLint location, GLsizei count, GLuint* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM3UIVPROC >( "glProgramUniform3uiv", out _glProgramUniform3uiv );

        _glProgramUniform3uiv( program, location, count, value );
    }

    public void ProgramUniform3uiv( GLuint program, GLint location, GLuint[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM3UIVPROC >( "glProgramUniform3uiv", out _glProgramUniform3uiv );

        fixed ( GLuint* pValue = &value[ 0 ] )
        {
            _glProgramUniform3uiv( program, location, value.Length / 3, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform4i( GLuint program, GLint location, GLint v0, GLint v1, GLint v2, GLint v3 )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM4IPROC >( "glProgramUniform4i", out _glProgramUniform4i );

        _glProgramUniform4i( program, location, v0, v1, v2, v3 );
    }

    // ========================================================================

    public void ProgramUniform4iv( GLuint program, GLint location, GLsizei count, GLint* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM4IVPROC >( "glProgramUniform4iv", out _glProgramUniform4iv );

        _glProgramUniform4iv( program, location, count, value );
    }

    public void ProgramUniform4iv( GLuint program, GLint location, GLint[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM4IVPROC >( "glProgramUniform4iv", out _glProgramUniform4iv );

        fixed ( GLint* pValue = &value[ 0 ] )
        {
            _glProgramUniform4iv( program, location, value.Length / 4, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform4f( GLuint program, GLint location, GLfloat v0, GLfloat v1, GLfloat v2, GLfloat v3 )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM4FPROC >( "glProgramUniform4f", out _glProgramUniform4f );

        _glProgramUniform4f( program, location, v0, v1, v2, v3 );
    }

    // ========================================================================

    public void ProgramUniform4fv( GLuint program, GLint location, GLsizei count, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM4FVPROC >( "glProgramUniform4fv", out _glProgramUniform4fv );

        _glProgramUniform4fv( program, location, count, value );
    }

    public void ProgramUniform4fv( GLuint program, GLint location, GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM4FVPROC >( "glProgramUniform4fv", out _glProgramUniform4fv );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniform4fv( program, location, value.Length / 4, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform4d( GLuint program, GLint location, GLdouble v0, GLdouble v1, GLdouble v2, GLdouble v3 )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM4DPROC >( "glProgramUniform4d", out _glProgramUniform4d );

        _glProgramUniform4d( program, location, v0, v1, v2, v3 );
    }

    // ========================================================================

    public void ProgramUniform4dv( GLuint program, GLint location, GLsizei count, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM4DVPROC >( "glProgramUniform4dv", out _glProgramUniform4dv );

        _glProgramUniform4dv( program, location, count, value );
    }

    public void ProgramUniform4dv( GLuint program, GLint location, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM4DVPROC >( "glProgramUniform4dv", out _glProgramUniform4dv );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniform4dv( program, location, value.Length / 4, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform4ui( GLuint program, GLint location, GLuint v0, GLuint v1, GLuint v2, GLuint v3 )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM4UIPROC >( "glProgramUniform4ui", out _glProgramUniform4ui );

        _glProgramUniform4ui( program, location, v0, v1, v2, v3 );
    }

    // ========================================================================

    public void ProgramUniform4uiv( GLuint program, GLint location, GLsizei count, GLuint* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM4UIVPROC >( "glProgramUniform4uiv", out _glProgramUniform4uiv );

        _glProgramUniform4uiv( program, location, count, value );
    }

    public void ProgramUniform4uiv( GLuint program, GLint location, GLuint[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORM4UIVPROC >( "glProgramUniform4uiv", out _glProgramUniform4uiv );

        fixed ( GLuint* pValue = &value[ 0 ] )
        {
            _glProgramUniform4uiv( program, location, value.Length / 4, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix2fv( GLuint program, GLint location, GLsizei count, GLbool transpose,
                                         GLfloat* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX2FVPROC >
            (
             "glProgramUniformMatrix2fv",
             out _glProgramUniformMatrix2fv
            );

        _glProgramUniformMatrix2fv( program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix2fv( GLuint program, GLint location, GLbool transpose, GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX2FVPROC >
            (
             "glProgramUniformMatrix2fv",
             out _glProgramUniformMatrix2fv
            );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix2fv( program, location, value.Length / 4, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix3fv( GLuint program, GLint location, GLsizei count, GLbool transpose,
                                         GLfloat* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX3FVPROC >
            (
             "glProgramUniformMatrix3fv",
             out _glProgramUniformMatrix3fv
            );

        _glProgramUniformMatrix3fv( program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix3fv( GLuint program, GLint location, GLbool transpose, GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX3FVPROC >
            (
             "glProgramUniformMatrix3fv",
             out _glProgramUniformMatrix3fv
            );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix3fv( program, location, value.Length / 9, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix4fv( GLuint program, GLint location, GLsizei count, GLbool transpose,
                                         GLfloat* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX4FVPROC >
            (
             "glProgramUniformMatrix4fv",
             out _glProgramUniformMatrix4fv
            );

        _glProgramUniformMatrix4fv( program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix4fv( GLuint program, GLint location, GLbool transpose, GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX4FVPROC >
            (
             "glProgramUniformMatrix4fv",
             out _glProgramUniformMatrix4fv
            );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix4fv( program, location, value.Length / 16, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix2dv( GLuint program, GLint location, GLsizei count, GLbool transpose,
                                         GLdouble* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX2DVPROC >
            (
             "glProgramUniformMatrix2dv",
             out _glProgramUniformMatrix2dv
            );

        _glProgramUniformMatrix2dv( program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix2dv( GLuint program, GLint location, GLbool transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX2DVPROC >
            (
             "glProgramUniformMatrix2dv",
             out _glProgramUniformMatrix2dv
            );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix2dv( program, location, value.Length / 4, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix3dv( GLuint program, GLint location, GLsizei count, GLbool transpose,
                                         GLdouble* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX3DVPROC >
            (
             "glProgramUniformMatrix3dv",
             out _glProgramUniformMatrix3dv
            );

        _glProgramUniformMatrix3dv( program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix3dv( GLuint program, GLint location, GLbool transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX3DVPROC >
            (
             "glProgramUniformMatrix3dv",
             out _glProgramUniformMatrix3dv
            );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix3dv( program, location, value.Length / 9, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix4dv( GLuint program, GLint location, GLsizei count, GLbool transpose,
                                         GLdouble* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX4DVPROC >
            (
             "glProgramUniformMatrix4dv",
             out _glProgramUniformMatrix4dv
            );

        _glProgramUniformMatrix4dv( program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix4dv( GLuint program, GLint location, GLbool transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX4DVPROC >
            (
             "glProgramUniformMatrix4dv",
             out _glProgramUniformMatrix4dv
            );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix4dv( program, location, value.Length / 16, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix2x3fv( GLuint program, GLint location, GLsizei count, GLbool transpose,
                                           GLfloat* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX2X3FVPROC >
            (
             "glProgramUniformMatrix2x3fv",
             out _glProgramUniformMatrix2x3fv
            );

        _glProgramUniformMatrix2x3fv( program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix2x3fv( GLuint program, GLint location, GLbool transpose, GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX2X3FVPROC >
            (
             "glProgramUniformMatrix2x3fv",
             out _glProgramUniformMatrix2x3fv
            );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix2x3fv( program, location, value.Length / 6, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix3x2fv( GLuint program, GLint location, GLsizei count, GLbool transpose,
                                           GLfloat* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX3X2FVPROC >
            (
             "glProgramUniformMatrix3x2fv",
             out _glProgramUniformMatrix3x2fv
            );

        _glProgramUniformMatrix3x2fv( program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix3x2fv( GLuint program, GLint location, GLbool transpose, GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX3X2FVPROC >
            (
             "glProgramUniformMatrix3x2fv",
             out _glProgramUniformMatrix3x2fv
            );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix3x2fv( program, location, value.Length / 6, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix2x4fv( GLuint program, GLint location, GLsizei count, GLbool transpose,
                                           GLfloat* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX2X4FVPROC >
            (
             "glProgramUniformMatrix2x4fv",
             out _glProgramUniformMatrix2x4fv
            );

        _glProgramUniformMatrix2x4fv( program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix2x4fv( GLuint program, GLint location, GLbool transpose, GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX2X4FVPROC >
            (
             "glProgramUniformMatrix2x4fv",
             out _glProgramUniformMatrix2x4fv
            );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix2x4fv( program, location, value.Length / 8, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix4x2fv( GLuint program, GLint location, GLsizei count, GLbool transpose,
                                           GLfloat* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX4X2FVPROC >
            (
             "glProgramUniformMatrix4x2fv",
             out _glProgramUniformMatrix4x2fv
            );

        _glProgramUniformMatrix4x2fv( program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix4x2fv( GLuint program, GLint location, GLbool transpose, GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX4X2FVPROC >
            (
             "glProgramUniformMatrix4x2fv",
             out _glProgramUniformMatrix4x2fv
            );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix4x2fv( program, location, value.Length / 8, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix3x4fv( GLuint program, GLint location, GLsizei count, GLbool transpose,
                                           GLfloat* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX3X4FVPROC >
            (
             "glProgramUniformMatrix3x4fv",
             out _glProgramUniformMatrix3x4fv
            );

        _glProgramUniformMatrix3x4fv( program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix3x4fv( GLuint program, GLint location, GLbool transpose, GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX3X4FVPROC >
            (
             "glProgramUniformMatrix3x4fv",
             out _glProgramUniformMatrix3x4fv
            );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix3x4fv( program, location, value.Length / 12, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix4x3fv( GLuint program, GLint location, GLsizei count, GLbool transpose,
                                           GLfloat* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX4X3FVPROC >
            (
             "glProgramUniformMatrix4x3fv",
             out _glProgramUniformMatrix4x3fv
            );

        _glProgramUniformMatrix4x3fv( program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix4x3fv( GLuint program, GLint location, GLbool transpose, GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX4X3FVPROC >
            (
             "glProgramUniformMatrix4x3fv",
             out _glProgramUniformMatrix4x3fv
            );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix4x3fv( program, location, value.Length / 12, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix2x3dv( GLuint program, GLint location, GLsizei count, GLbool transpose,
                                           GLdouble* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX2X3DVPROC >
            (
             "glProgramUniformMatrix2x3dv",
             out _glProgramUniformMatrix2x3dv
            );

        _glProgramUniformMatrix2x3dv( program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix2x3dv( GLuint program, GLint location, GLbool transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX2X3DVPROC >
            (
             "glProgramUniformMatrix2x3dv",
             out _glProgramUniformMatrix2x3dv
            );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix2x3dv( program, location, value.Length / 6, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix3x2dv( GLuint program, GLint location, GLsizei count, GLbool transpose,
                                           GLdouble* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX3X2DVPROC >
            (
             "glProgramUniformMatrix3x2dv",
             out _glProgramUniformMatrix3x2dv
            );

        _glProgramUniformMatrix3x2dv( program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix3x2dv( GLuint program, GLint location, GLbool transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX3X2DVPROC >
            (
             "glProgramUniformMatrix3x2dv",
             out _glProgramUniformMatrix3x2dv
            );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix3x2dv( program, location, value.Length / 6, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix2x4dv( GLuint program, GLint location, GLsizei count, GLbool transpose,
                                           GLdouble* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX2X4DVPROC >
            (
             "glProgramUniformMatrix2x4dv",
             out _glProgramUniformMatrix2x4dv
            );

        _glProgramUniformMatrix2x4dv( program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix2x4dv( GLuint program, GLint location, GLbool transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX2X4DVPROC >
            (
             "glProgramUniformMatrix2x4dv",
             out _glProgramUniformMatrix2x4dv
            );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix2x4dv( program, location, value.Length / 8, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix4x2dv( GLuint program, GLint location, GLsizei count, GLbool transpose,
                                           GLdouble* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX4X2DVPROC >
            (
             "glProgramUniformMatrix4x2dv",
             out _glProgramUniformMatrix4x2dv
            );

        _glProgramUniformMatrix4x2dv( program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix4x2dv( GLuint program, GLint location, GLbool transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX4X2DVPROC >
            (
             "glProgramUniformMatrix4x2dv",
             out _glProgramUniformMatrix4x2dv
            );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix4x2dv( program, location, value.Length / 8, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix3x4dv( GLuint program, GLint location, GLsizei count, GLbool transpose,
                                           GLdouble* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX3X4DVPROC >
            (
             "glProgramUniformMatrix3x4dv",
             out _glProgramUniformMatrix3x4dv
            );

        _glProgramUniformMatrix3x4dv( program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix3x4dv( GLuint program, GLint location, GLbool transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX3X4DVPROC >
            (
             "glProgramUniformMatrix3x4dv",
             out _glProgramUniformMatrix3x4dv
            );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix3x4dv( program, location, value.Length / 12, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix4x3dv( GLuint program, GLint location, GLsizei count, GLbool transpose,
                                           GLdouble* value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX4X3DVPROC >
            (
             "glProgramUniformMatrix4x3dv",
             out _glProgramUniformMatrix4x3dv
            );

        _glProgramUniformMatrix4x3dv( program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix4x3dv( GLuint program, GLint location, GLbool transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLPROGRAMUNIFORMMATRIX4X3DVPROC >
            (
             "glProgramUniformMatrix4x3dv",
             out _glProgramUniformMatrix4x3dv
            );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix4x3dv( program, location, value.Length / 12, transpose, pValue );
        }
    }

    // ========================================================================

    public void ValidateProgramPipeline( GLuint pipeline )
    {
        GetDelegateForFunction< PFNGLVALIDATEPROGRAMPIPELINEPROC >
            (
             "glValidateProgramPipeline",
             out _glValidateProgramPipeline
            );

        _glValidateProgramPipeline( pipeline );
    }

    // ========================================================================

    public void GetProgramPipelineInfoLog( GLuint pipeline, GLsizei bufSize, GLsizei* length, GLchar* infoLog )
    {
        GetDelegateForFunction< PFNGLGETPROGRAMPIPELINEINFOLOGPROC >
            (
             "glGetProgramPipelineInfoLog",
             out _glGetProgramPipelineInfoLog
            );

        _glGetProgramPipelineInfoLog( pipeline, bufSize, length, infoLog );
    }

    public string GetProgramPipelineInfoLog( GLuint pipeline, GLsizei bufSize )
    {
        var infoLog = new GLchar[ bufSize ];

        GetDelegateForFunction< PFNGLGETPROGRAMPIPELINEINFOLOGPROC >
            (
             "glGetProgramPipelineInfoLog",
             out _glGetProgramPipelineInfoLog
            );

        fixed ( GLchar* pInfoLog = &infoLog[ 0 ] )
        {
            GLsizei length;

            _glGetProgramPipelineInfoLog( pipeline, bufSize, &length, pInfoLog );

            return new string( ( GLbyte* )pInfoLog, 0, length, Encoding.UTF8 );
        }
    }

    // ========================================================================

    public void GetProgramInterfaceiv( GLint program, GLenum programInterface, GLenum pname, GLint* parameters )
    {
        Logger.Checkpoint();

        if ( !IsProgram( program ) )
        {
            throw new LughRuntimeException( $"Invalid program ID: {program}" );
        }

        GetDelegateForFunction< PFNGLGETPROGRAMINTERFACEIVPROC >
            (
             "glGetProgramInterfaceiv",
             out _glGetProgramInterfaceiv
            );

        _glGetProgramInterfaceiv( ( uint )program, programInterface, pname, parameters );
    }

    public void GetProgramInterfaceiv( GLint program, GLenum programInterface, GLenum pname, ref GLint[] parameters )
    {
        Logger.Checkpoint();

        if ( !IsProgram( program ) )
        {
            throw new LughRuntimeException( $"Invalid program ID: {program}" );
        }

        GetDelegateForFunction< PFNGLGETPROGRAMINTERFACEIVPROC >
            (
             "glGetProgramInterfaceiv",
             out _glGetProgramInterfaceiv
            );

        fixed ( GLint* pParameters = &parameters[ 0 ] )
        {
            _glGetProgramInterfaceiv( ( uint )program, programInterface, pname, pParameters );
        }
    }

    // ========================================================================

    public GLuint GetProgramResourceIndex( GLint program, GLenum programInterface, GLchar* name )
    {
        if ( !IsProgram( program ) )
        {
            throw new LughRuntimeException( $"Invalid program ID: {program}" );
        }

        GetDelegateForFunction< PFNGLGETPROGRAMRESOURCEINDEXPROC >
            (
             "glGetProgramResourceIndex",
             out _glGetProgramResourceIndex
            );

        return _glGetProgramResourceIndex( ( uint )program, programInterface, name );
    }

    public GLuint GetProgramResourceIndex( GLint program, GLenum programInterface, string name )
    {
        if ( !IsProgram( program ) )
        {
            throw new LughRuntimeException( $"Invalid program ID: {program}" );
        }

        byte[] nameBytes = Encoding.UTF8.GetBytes( name );

        GetDelegateForFunction< PFNGLGETPROGRAMRESOURCEINDEXPROC >
            (
             "glGetProgramResourceIndex",
             out _glGetProgramResourceIndex
            );

        fixed ( GLchar* pName = &nameBytes[ 0 ] )
        {
            return _glGetProgramResourceIndex( ( uint )program, programInterface, pName );
        }
    }

    // ========================================================================

    public void GetProgramResourceName( GLint program, GLenum programInterface, GLuint index, GLsizei bufSize,
                                        GLsizei* length,
                                        GLchar* name )
    {
        if ( !IsProgram( program ) )
        {
            throw new LughRuntimeException( $"Invalid program ID: {program}" );
        }

        GetDelegateForFunction< PFNGLGETPROGRAMRESOURCENAMEPROC >
            (
             "glGetProgramResourceName",
             out _glGetProgramResourceName
            );

        _glGetProgramResourceName( ( uint )program, programInterface, index, bufSize, length, name );
    }

    public string GetProgramResourceName( GLint program, GLenum programInterface, GLuint index, GLsizei bufSize )
    {
        if ( !IsProgram( program ) )
        {
            throw new LughRuntimeException( $"Invalid program ID: {program}" );
        }

        var name = new GLchar[ bufSize ];

        GetDelegateForFunction< PFNGLGETPROGRAMRESOURCENAMEPROC >
            (
             "glGetProgramResourceName",
             out _glGetProgramResourceName
            );

        fixed ( GLchar* pName = &name[ 0 ] )
        {
            GLsizei length;

            _glGetProgramResourceName( ( uint )program, programInterface, index, bufSize, &length, pName );

            return new string( ( GLbyte* )pName, 0, length, Encoding.UTF8 );
        }
    }

    // ========================================================================

    public void GetProgramResourceiv( GLint program, GLenum programInterface, GLuint index, GLsizei propCount,
                                      GLenum* props, GLsizei bufSize, GLsizei* length, GLint* parameters )
    {
        if ( !IsProgram( program ) )
        {
            throw new LughRuntimeException( $"Invalid program ID: {program}" );
        }

        GetDelegateForFunction< PFNGLGETPROGRAMRESOURCEIVPROC >
            (
             "glGetProgramResourceiv",
             out _glGetProgramResourceiv
            );

        _glGetProgramResourceiv
            (
             ( uint )program,
             programInterface,
             index,
             propCount,
             props,
             bufSize,
             length,
             parameters
            );
    }

    public void GetProgramResourceiv( GLint program, GLenum programInterface, GLuint index, GLenum[] props,
                                      GLsizei bufSize,
                                      ref GLint[] parameters )
    {
        if ( !IsProgram( program ) )
        {
            throw new LughRuntimeException( $"Invalid program ID: {program}" );
        }

        GetDelegateForFunction< PFNGLGETPROGRAMRESOURCEIVPROC >
            (
             "glGetProgramResourceiv",
             out _glGetProgramResourceiv
            );

        fixed ( GLenum* pProps = &props[ 0 ] )
        {
            fixed ( GLint* pParams = &parameters[ 0 ] )
            {
                GLsizei length;
                _glGetProgramResourceiv
                    (
                     ( uint )program,
                     programInterface,
                     index,
                     props.Length,
                     pProps,
                     bufSize,
                     &length,
                     pParams
                    );
            }
        }
    }

    // ========================================================================

    public GLint GetProgramResourceLocation( GLint program, GLenum programInterface, GLchar* name )
    {
        GetDelegateForFunction< PFNGLGETPROGRAMRESOURCELOCATIONPROC >
            (
             "glGetProgramResourceLocation",
             out _glGetProgramResourceLocation
            );

        return _glGetProgramResourceLocation( ( uint )program, programInterface, name );
    }

    public GLint GetProgramResourceLocation( GLint program, GLenum programInterface, string name )
    {
        if ( !IsProgram( program ) )
        {
            throw new LughRuntimeException( $"Invalid program ID: {program}" );
        }

        byte[] nameBytes = Encoding.UTF8.GetBytes( name );

        GetDelegateForFunction< PFNGLGETPROGRAMRESOURCELOCATIONPROC >
            (
             "glGetProgramResourceLocation",
             out _glGetProgramResourceLocation
            );

        fixed ( GLchar* pName = &nameBytes[ 0 ] )
        {
            return _glGetProgramResourceLocation( ( uint )program, programInterface, pName );
        }
    }

    // ========================================================================

    public GLint GetProgramResourceLocationIndex( GLint program, GLenum programInterface, GLchar* name )
    {
        if ( !IsProgram( program ) )
        {
            throw new LughRuntimeException( $"Invalid program ID: {program}" );
        }

        GetDelegateForFunction< PFNGLGETPROGRAMRESOURCELOCATIONINDEXPROC >
            (
             "glGetProgramResourceLocationIndex",
             out _glGetProgramResourceLocationIndex
            );

        return _glGetProgramResourceLocationIndex( ( uint )program, programInterface, name );
    }

    public GLint GetProgramResourceLocationIndex( GLint program, GLenum programInterface, string name )
    {
        if ( !IsProgram( program ) )
        {
            throw new LughRuntimeException( $"Invalid program ID: {program}" );
        }

        byte[] nameBytes = Encoding.UTF8.GetBytes( name );

        GetDelegateForFunction< PFNGLGETPROGRAMRESOURCELOCATIONINDEXPROC >
            (
             "glGetProgramResourceLocationIndex",
             out _glGetProgramResourceLocationIndex
            );

        fixed ( GLchar* pName = &nameBytes[ 0 ] )
        {
            return _glGetProgramResourceLocationIndex( ( uint )program, programInterface, pName );
        }
    }

    // ========================================================================

    public void CreateProgramPipelines( GLsizei n, GLuint* pipelines )
    {
        GetDelegateForFunction< PFNGLCREATEPROGRAMPIPELINESPROC >
            (
             "glCreateProgramPipelines",
             out _glCreateProgramPipelines
            );

        _glCreateProgramPipelines( n, pipelines );
    }

    public GLuint[] CreateProgramPipelines( GLsizei n )
    {
        var pipelines = new GLuint[ n ];

        GetDelegateForFunction< PFNGLCREATEPROGRAMPIPELINESPROC >
            (
             "glCreateProgramPipelines",
             out _glCreateProgramPipelines
            );

        fixed ( GLuint* ptrPipelines = &pipelines[ 0 ] )
        {
            _glCreateProgramPipelines( n, ptrPipelines );
        }

        return pipelines;
    }

    public GLuint CreateProgramPipeline()
    {
        return CreateProgramPipelines( 1 )[ 0 ];
    }

    #endregion Program Bindings

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region Query Bindings

    /// <inheritdoc />
    public void GenQueries( GLsizei n, GLuint* ids )
    {
        GetDelegateForFunction< PFNGLGENQUERIESPROC >( "glGenQueries", out _glGenQueries );

        _glGenQueries( n, ids );
    }

    /// <inheritdoc />
    public GLuint[] GenQueries( GLsizei n )
    {
        GetDelegateForFunction< PFNGLGENQUERIESPROC >( "glGenQueries", out _glGenQueries );

        var ret = new GLuint[ n ];

        {
            fixed ( GLuint* p = &ret[ 0 ] )
            {
                _glGenQueries( n, p );
            }
        }

        return ret;
    }

    /// <inheritdoc />
    public GLuint GenQuery()
    {
        return GenQueries( 1 )[ 0 ];
    }

    // ========================================================================

    /// <inheritdoc />
    public void DeleteQueries( GLsizei n, GLuint* ids )
    {
        GetDelegateForFunction< PFNGLDELETEQUERIESPROC >( "glDeleteQueries", out _glDeleteQueries );

        _glDeleteQueries( n, ids );
    }

    /// <inheritdoc />
    public void DeleteQueries( params GLuint[] ids )
    {
        GetDelegateForFunction< PFNGLDELETEQUERIESPROC >( "glDeleteQueries", out _glDeleteQueries );

        {
            fixed ( GLuint* p = &ids[ 0 ] )
            {
                _glDeleteQueries( ids.Length, p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public GLbool IsQuery( GLuint id )
    {
        GetDelegateForFunction< PFNGLISQUERYPROC >( "glIsQuery", out _glIsQuery );

        return _glIsQuery( id );
    }

    // ========================================================================

    /// <inheritdoc />
    public void BeginQuery( GLenum target, GLuint id )
    {
        GetDelegateForFunction< PFNGLBEGINQUERYPROC >( "glBeginQuery", out _glBeginQuery );

        _glBeginQuery( target, id );
    }

    // ========================================================================

    /// <inheritdoc />
    public void EndQuery( GLenum target )
    {
        GetDelegateForFunction< PFNGLENDQUERYPROC >( "glEndQuery", out _glEndQuery );

        _glEndQuery( target );
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetQueryiv( GLenum target, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETQUERYIVPROC >( "glGetQueryiv", out _glGetQueryiv );

        _glGetQueryiv( target, pname, parameters );
    }

    /// <inheritdoc />
    public void GetQueryiv( GLenum target, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETQUERYIVPROC >( "glGetQueryiv", out _glGetQueryiv );

        {
            fixed ( GLint* p = &parameters[ 0 ] )
            {
                _glGetQueryiv( target, pname, p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetQueryObjectiv( GLuint id, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETQUERYOBJECTIVPROC >( "glGetQueryObjectiv", out _glGetQueryObjectiv );

        _glGetQueryObjectiv( id, pname, parameters );
    }

    /// <inheritdoc />
    public void GetQueryObjectiv( GLuint id, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETQUERYOBJECTIVPROC >( "glGetQueryObjectiv", out _glGetQueryObjectiv );

        {
            fixed ( GLint* p = &parameters[ 0 ] )
            {
                _glGetQueryObjectiv( id, pname, p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetQueryObjectuiv( GLuint id, GLenum pname, GLuint* parameters )
    {
        GetDelegateForFunction< PFNGLGETQUERYOBJECTUIVPROC >( "glGetQueryObjectuiv", out _glGetQueryObjectuiv );

        _glGetQueryObjectuiv( id, pname, parameters );
    }

    /// <inheritdoc />
    public void GetQueryObjectuiv( GLuint id, GLenum pname, ref GLuint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETQUERYOBJECTUIVPROC >( "glGetQueryObjectuiv", out _glGetQueryObjectuiv );

        {
            fixed ( GLuint* p = &parameters[ 0 ] )
            {
                _glGetQueryObjectuiv( id, pname, p );
            }
        }
    }

    // ========================================================================

    public void CreateQueries( GLenum target, GLsizei n, GLuint* ids )
    {
        GetDelegateForFunction< PFNGLCREATEQUERIESPROC >( "glCreateQueries", out _glCreateQueries );

        _glCreateQueries( target, n, ids );
    }

    public GLuint[] CreateQueries( GLenum target, GLsizei n )
    {
        var ids = new GLuint[ n ];

        GetDelegateForFunction< PFNGLCREATEQUERIESPROC >( "glCreateQueries", out _glCreateQueries );

        fixed ( GLuint* ptrIds = &ids[ 0 ] )
        {
            _glCreateQueries( target, n, ptrIds );
        }

        return ids;
    }

    // ========================================================================

    public GLuint CreateQuery( GLenum target )
    {
        return CreateQueries( target, 1 )[ 0 ];
    }

    // ========================================================================

    public void GetQueryBufferObjecti64v( GLuint id, GLuint buffer, GLenum pname, GLintptr offset )
    {
        GetDelegateForFunction< PFNGLGETQUERYBUFFEROBJECTI64VPROC >
            (
             "glGetQueryBufferObjecti64v",
             out _glGetQueryBufferObjecti64v
            );

        _glGetQueryBufferObjecti64v( id, buffer, pname, offset );
    }

    // ========================================================================

    public void GetQueryBufferObjectiv( GLuint id, GLuint buffer, GLenum pname, GLintptr offset )
    {
        GetDelegateForFunction< PFNGLGETQUERYBUFFEROBJECTIVPROC >
            (
             "glGetQueryBufferObjectiv",
             out _glGetQueryBufferObjectiv
            );

        _glGetQueryBufferObjectiv( id, buffer, pname, offset );
    }

    // ========================================================================

    public void GetQueryBufferObjectui64v( GLuint id, GLuint buffer, GLenum pname, GLintptr offset )
    {
        GetDelegateForFunction< PFNGLGETQUERYBUFFEROBJECTUI64VPROC >
            (
             "glGetQueryBufferObjectui64v",
             out _glGetQueryBufferObjectui64v
            );

        _glGetQueryBufferObjectui64v( id, buffer, pname, offset );
    }

    // ========================================================================

    public void GetQueryBufferObjectuiv( GLuint id, GLuint buffer, GLenum pname, GLintptr offset )
    {
        GetDelegateForFunction< PFNGLGETQUERYBUFFEROBJECTUIVPROC >
            (
             "glGetQueryBufferObjectuiv",
             out _glGetQueryBufferObjectuiv
            );

        _glGetQueryBufferObjectuiv( id, buffer, pname, offset );
    }

    // ========================================================================

    /// <inheritdoc />
    /// <inheritdoc />
    public void QueryCounter( GLuint id, GLenum target )
    {
        GetDelegateForFunction< PFNGLQUERYCOUNTERPROC >( "glQueryCounter", out _glQueryCounter );

        _glQueryCounter( id, target );
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetQueryObjecti64V( GLuint id, GLenum pname, GLint64* param )
    {
        GetDelegateForFunction< PFNGLGETQUERYOBJECTI64VPROC >( "glGetQueryObjecti64v", out _glGetQueryObjecti64v );

        _glGetQueryObjecti64v( id, pname, param );
    }

    /// <inheritdoc />
    public void GetQueryObjecti64V( GLuint id, GLenum pname, ref GLint64[] param )
    {
        GetDelegateForFunction< PFNGLGETQUERYOBJECTI64VPROC >( "glGetQueryObjecti64v", out _glGetQueryObjecti64v );

        fixed ( GLint64* dp = &param[ 0 ] )
        {
            _glGetQueryObjecti64v( id, pname, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetQueryObjectui64V( GLuint id, GLenum pname, GLuint64* param )
    {
        GetDelegateForFunction< PFNGLGETQUERYOBJECTUI64VPROC >( "glGetQueryObjectui64v", out _glGetQueryObjectui64v );

        _glGetQueryObjectui64v( id, pname, param );
    }

    /// <inheritdoc />
    public void GetQueryObjectui64V( GLuint id, GLenum pname, ref GLuint64[] param )
    {
        GetDelegateForFunction< PFNGLGETQUERYOBJECTUI64VPROC >( "glGetQueryObjectui64v", out _glGetQueryObjectui64v );

        fixed ( GLuint64* dp = &param[ 0 ] )
        {
            _glGetQueryObjectui64v( id, pname, dp );
        }
    }

    #endregion Query Bindings

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region Shader Bindings

    /// <inheritdoc />
    public void AttachShader( GLint program, GLint shader )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == InvalidShaderProgram ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        if ( !Engine.GL.IsShader( shader ) || ( shader == InvalidShader ) )
        {
            Logger.Debug( $"***** Provided Shader {shader} is not a valid GLshader *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLATTACHSHADERPROC >( "glAttachShader", out _glAttachShader );

        _glAttachShader( ( GLuint )program, ( GLuint )shader );
    }

    // ========================================================================

    /// <inheritdoc />
    public void BindAttribLocation( GLint program, GLuint index, byte* name )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == InvalidShaderProgram ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLBINDATTRIBLOCATIONPROC >( "glBindAttribLocation", out _glBindAttribLocation );

        _glBindAttribLocation( ( GLuint )program, index, name );
    }

    /// <inheritdoc />
    public void BindAttribLocation( int program, GLuint index, string name )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == InvalidShaderProgram ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        byte[] utf8 = Encoding.UTF8.GetBytes( name );

        GetDelegateForFunction< PFNGLBINDATTRIBLOCATIONPROC >( "glBindAttribLocation", out _glBindAttribLocation );

        fixed ( byte* putf8 = &utf8[ 0 ] )
        {
            _glBindAttribLocation( ( GLuint )program, index, putf8 );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void CompileShader( GLint shader )
    {
        if ( !Engine.GL.IsShader( shader ) || ( shader == InvalidShader ) )
        {
            Logger.Debug( $"***** Provided Shader {shader} is not a valid GLshader *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLCOMPILESHADERPROC >( "glCompileShader", out _glCompileShader );

        _glCompileShader( ( GLuint )shader );
    }

    // ========================================================================

    /// <inheritdoc />
    public GLuint CreateShader( GLenum type )
    {
        GetDelegateForFunction< PFNGLCREATESHADERPROC >( "glCreateShader", out _glCreateShader );

        return _glCreateShader( type );
    }

    // ========================================================================

    /// <inheritdoc />
    public void DeleteProgram( GLint program )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == InvalidShaderProgram ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        if ( Engine.GL.IsProgram( program ) && ( program != InvalidShaderProgram ) )
        {
            GetDelegateForFunction< PFNGLDELETEPROGRAMPROC >( "glDeleteProgram", out _glDeleteProgram );

            _glDeleteProgram( ( GLuint )program );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void DeleteShader( GLint shader )
    {
        if ( !Engine.GL.IsShader( shader ) || ( shader == InvalidShader ) )
        {
            Logger.Debug( $"***** Provided Shader {shader} is not a valid GLshader *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLDELETESHADERPROC >( "glDeleteShader", out _glDeleteShader );

        _glDeleteShader( ( GLuint )shader );
    }

    // ========================================================================

    /// <inheritdoc />
    public void DetachShader( GLint program, GLint shader )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == InvalidShaderProgram ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        if ( !Engine.GL.IsShader( shader ) || ( shader == InvalidShader ) )
        {
            Logger.Debug( $"***** Provided Shader {shader} is not a valid GLshader *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLDETACHSHADERPROC >( "glDetachShader", out _glDetachShader );

        _glDetachShader( ( GLuint )program, ( GLuint )shader );
    }

    // ========================================================================

    /// <inheritdoc />
    public void DisableVertexAttribArray( GLuint index )
    {
        GetDelegateForFunction< PFNGLDISABLEVERTEXATTRIBARRAYPROC >
            (
             "glDisableVertexAttribArray",
             out _glDisableVertexAttribArray
            );

        _glDisableVertexAttribArray( index );
    }

    // ========================================================================

    /// <inheritdoc />
    public void EnableVertexAttribArray( GLuint index )
    {
        GetDelegateForFunction< PFNGLENABLEVERTEXATTRIBARRAYPROC >
            (
             "glEnableVertexAttribArray",
             out _glEnableVertexAttribArray
            );

        _glEnableVertexAttribArray( index );
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetActiveAttrib( GLint program, GLuint index, GLsizei bufSize, GLsizei* length, GLint* size,
                                 GLenum* type, GLchar* name )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == InvalidShaderProgram ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETACTIVEATTRIBPROC >( "glGetActiveAttrib", out _glGetActiveAttrib );

        _glGetActiveAttrib( ( GLuint )program, index, bufSize, length, size, type, name );
    }

    /// <inheritdoc />
    public string GetActiveAttrib( GLint program, GLuint index, GLsizei bufSize, out GLint size, out GLenum type )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == InvalidShaderProgram ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GLchar* name = stackalloc GLchar[ bufSize ];
        GLsizei len;

        GetDelegateForFunction< PFNGLGETACTIVEATTRIBPROC >( "glGetActiveAttrib", out _glGetActiveAttrib );

        fixed ( GLenum* ptype = &type )
        {
            fixed ( GLint* psize = &size )
            {
                _glGetActiveAttrib( ( GLuint )program, index, bufSize, &len, psize, ptype, name );
            }
        }

        return new string( ( GLbyte* )name, 0, len, Encoding.UTF8 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetActiveUniform( GLint program, GLuint index, GLsizei bufSize, GLsizei* length, GLint* size,
                                  GLenum* type, GLchar* name )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == InvalidShaderProgram ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETACTIVEUNIFORMPROC >( "glGetActiveUniform", out _glGetActiveUniform );

        _glGetActiveUniform( ( GLuint )program, index, bufSize, length, size, type, name );
    }

    /// <inheritdoc />
    public string GetActiveUniform( GLint program, GLuint index, GLsizei bufSize, out GLint size, out GLenum type )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == InvalidShaderProgram ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GLchar* name = stackalloc GLchar[ bufSize ];
        GLsizei len;

        GetDelegateForFunction< PFNGLGETACTIVEUNIFORMPROC >( "glGetActiveUniform", out _glGetActiveUniform );

        fixed ( GLenum* ptype = &type )
        {
            fixed ( GLint* psize = &size )
            {
                _glGetActiveUniform( ( GLuint )program, index, bufSize, &len, psize, ptype, name );
            }
        }

        return new string( ( GLbyte* )name, 0, len, Encoding.UTF8 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetAttachedShaders( int program, int maxCount, int* count, GLuint* shaders )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == InvalidShaderProgram ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETATTACHEDSHADERSPROC >( "glGetAttachedShaders", out _glGetAttachedShaders );

        _glGetAttachedShaders( ( GLuint )program, maxCount, count, shaders );
    }

    /// <inheritdoc />
    public GLuint[] GetAttachedShaders( int program, int maxCount )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == InvalidShaderProgram ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        var     shaders = new GLuint[ maxCount ];
        GLsizei count;

        GetDelegateForFunction< PFNGLGETATTACHEDSHADERSPROC >( "glGetAttachedShaders", out _glGetAttachedShaders );

        fixed ( GLuint* pshaders = &shaders[ 0 ] )
        {
            _glGetAttachedShaders( ( GLuint )program, maxCount, &count, pshaders );
        }

        Array.Resize( ref shaders, count );

        return shaders;
    }

    // ========================================================================

    /// <inheritdoc />
    public GLint GetAttribLocation( GLint program, GLchar* name )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == InvalidShaderProgram ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETATTRIBLOCATIONPROC >( "glGetAttribLocation", out _glGetAttribLocation );

        return _glGetAttribLocation( ( GLuint )program, name );
    }

    /// <inheritdoc />
    public GLint GetAttribLocation( GLint program, string name )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == InvalidShaderProgram ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETATTRIBLOCATIONPROC >( "glGetAttribLocation", out _glGetAttribLocation );

        fixed ( GLchar* pname = Encoding.UTF8.GetBytes( name ) )
        {
            return _glGetAttribLocation( ( GLuint )program, pname );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetShaderiv( GLint shader, GLenum pname, GLint* parameters )
    {
        if ( !Engine.GL.IsShader( shader ) || ( shader == InvalidShader ) )
        {
            Logger.Debug( $"***** Provided Shader {shader} is not a valid GLshader *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETSHADERIVPROC >( "glGetShaderiv", out _glGetShaderiv );

        _glGetShaderiv( ( GLuint )shader, pname, parameters );
    }

    /// <inheritdoc />
    public void GetShaderiv( GLint shader, GLenum pname, ref GLint[] parameters )
    {
        if ( !Engine.GL.IsShader( shader ) || ( shader == InvalidShader ) )
        {
            Logger.Debug( $"***** Provided Shader {shader} is not a valid GLshader *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETSHADERIVPROC >( "glGetShaderiv", out _glGetShaderiv );

        fixed ( GLint* pparams = &parameters[ 0 ] )
        {
            _glGetShaderiv( ( GLuint )shader, pname, pparams );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetShaderInfoLog( GLint shader, GLsizei bufSize, GLsizei* length, GLchar* infoLog )
    {
        if ( !Engine.GL.IsShader( shader ) || ( shader == InvalidShader ) )
        {
            Logger.Debug( $"***** Provided Shader {shader} is not a valid GLshader *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETSHADERINFOLOGPROC >( "glGetShaderInfoLog", out _glGetShaderInfoLog );

        _glGetShaderInfoLog( ( GLuint )shader, bufSize, length, infoLog );
    }

    /// <inheritdoc />
    public string GetShaderInfoLog( GLint shader, GLsizei bufSize )
    {
        if ( !Engine.GL.IsShader( shader ) || ( shader == InvalidShader ) )
        {
            Logger.Debug( $"***** Provided Shader {shader} is not a valid GLshader *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GLchar* infoLog = stackalloc GLchar[ bufSize ];
        GLsizei len;

        GetDelegateForFunction< PFNGLGETSHADERINFOLOGPROC >( "glGetShaderInfoLog", out _glGetShaderInfoLog );

        _glGetShaderInfoLog( ( GLuint )shader, bufSize, &len, infoLog );

        return new string( ( GLbyte* )infoLog, 0, len, Encoding.UTF8 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetShaderSource( GLint shader, GLsizei bufSize, GLsizei* length, GLchar* source )
    {
        if ( !Engine.GL.IsShader( shader ) || ( shader == InvalidShader ) )
        {
            Logger.Debug( $"***** Provided Shader {shader} is not a valid GLshader *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETSHADERSOURCEPROC >( "glGetShaderSource", out _glGetShaderSource );

        _glGetShaderSource( ( GLuint )shader, bufSize, length, source );
    }

    /// <inheritdoc />
    public string GetShaderSource( GLint shader, GLsizei bufSize = 4096 )
    {
        if ( !Engine.GL.IsShader( shader ) || ( shader == InvalidShader ) )
        {
            Logger.Debug( $"***** Provided Shader {shader} is not a valid GLshader *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GLchar* source = stackalloc GLchar[ bufSize ];
        GLsizei len;

        GetDelegateForFunction< PFNGLGETSHADERSOURCEPROC >( "glGetShaderSource", out _glGetShaderSource );

        _glGetShaderSource( ( GLuint )shader, bufSize, &len, source );

        return new string( ( GLbyte* )source, 0, len, Encoding.UTF8 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetVertexAttribdv( GLuint index, GLenum pname, GLdouble* parameters )
    {
        GetDelegateForFunction< PFNGLGETVERTEXATTRIBDVPROC >( "glGetVertexAttribdv", out _glGetVertexAttribdv );

        _glGetVertexAttribdv( index, pname, parameters );
    }

    /// <inheritdoc />
    public void GetVertexAttribdv( GLuint index, GLenum pname, ref GLdouble[] parameters )
    {
        GetDelegateForFunction< PFNGLGETVERTEXATTRIBDVPROC >( "glGetVertexAttribdv", out _glGetVertexAttribdv );

        fixed ( GLdouble* pparams = &parameters[ 0 ] )
        {
            _glGetVertexAttribdv( index, pname, pparams );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetVertexAttribfv( GLuint index, GLenum pname, GLfloat* parameters )
    {
        GetDelegateForFunction< PFNGLGETVERTEXATTRIBFVPROC >( "glGetVertexAttribfv", out _glGetVertexAttribfv );

        _glGetVertexAttribfv( index, pname, parameters );
    }

    /// <inheritdoc />
    public void GetVertexAttribfv( GLuint index, GLenum pname, ref GLfloat[] parameters )
    {
        GetDelegateForFunction< PFNGLGETVERTEXATTRIBFVPROC >( "glGetVertexAttribfv", out _glGetVertexAttribfv );

        fixed ( GLfloat* pparams = &parameters[ 0 ] )
        {
            _glGetVertexAttribfv( index, pname, pparams );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetVertexAttribiv( GLuint index, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETVERTEXATTRIBIVPROC >( "glGetVertexAttribiv", out _glGetVertexAttribiv );

        _glGetVertexAttribiv( index, pname, parameters );
    }

    /// <inheritdoc />
    public void GetVertexAttribiv( GLuint index, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETVERTEXATTRIBIVPROC >( "glGetVertexAttribiv", out _glGetVertexAttribiv );

        fixed ( GLint* pparams = &parameters[ 0 ] )
        {
            _glGetVertexAttribiv( index, pname, pparams );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetVertexAttribPointerv( GLuint index, GLenum pname, IntPtr* pointer )
    {
        GetDelegateForFunction< PFNGLGETVERTEXATTRIBPOINTERVPROC >
            (
             "glGetVertexAttribPointerv",
             out _glGetVertexAttribPointerv
            );

        _glGetVertexAttribPointerv( index, pname, pointer );
    }

    /// <inheritdoc />
    public void GetVertexAttribPointerv( GLuint index, GLenum pname, ref GLuint[] pointer )
    {
        var ptr = new void*[ pointer.Length ];

        GetDelegateForFunction< PFNGLGETVERTEXATTRIBPOINTERVPROC >
            (
             "glGetVertexAttribPointerv",
             out _glGetVertexAttribPointerv
            );

        fixed ( void** p = &ptr[ 0 ] )
        {
            _glGetVertexAttribPointerv( index, pname, ( IntPtr* )p );
        }

        for ( var i = 0; i < pointer.Length; i++ )
        {
            pointer[ i ] = ( GLuint )ptr[ i ];
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public GLbool IsShader( GLint shader )
    {
        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLISSHADERPROC >( "glIsShader", out _glIsShader );

        return _glIsShader( ( GLuint )shader );
    }

    // ========================================================================

    /// <inheritdoc />
    public void ShaderSource( GLint shader, GLsizei count, GLchar** str, GLint* length )
    {
        if ( !Engine.GL.IsShader( shader ) || ( shader == InvalidShader ) )
        {
            Logger.Debug( $"***** Provided Shader {shader} is not a valid GLshader *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLSHADERSOURCEPROC >( "glShaderSource", out _glShaderSource );

        _glShaderSource( ( GLuint )shader, count, str, length );
    }

    /// <inheritdoc />
    public void ShaderSource( GLint shader, params string[] stringParam )
    {
        if ( !Engine.GL.IsShader( shader ) || ( shader == InvalidShader ) )
        {
            Logger.Debug( $"***** Provided Shader {shader} is not a valid GLshader *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        int count   = stringParam.Length;
        var strings = new GLchar[ count ][];
        var lengths = new GLint[ count ];

        for ( var i = 0; i < count; i++ )
        {
            strings[ i ] = Encoding.UTF8.GetBytes( stringParam[ i ] );
            lengths[ i ] = stringParam[ i ].Length;
        }

        GLchar** pstring = stackalloc GLchar*[ count ];
        GLint*   length  = stackalloc GLint[ count ];

        for ( var i = 0; i < count; i++ )
        {
            fixed ( GLchar* p = &strings[ i ][ 0 ] )
            {
                pstring[ i ] = p;
            }

            length[ i ] = lengths[ i ];
        }

        GetDelegateForFunction< PFNGLSHADERSOURCEPROC >( "glShaderSource", out _glShaderSource );

        _glShaderSource( ( GLuint )shader, count, pstring, length );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib1d( GLuint index, GLdouble x )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB1DPROC >( "glVertexAttrib1d", out _glVertexAttrib1d );

        _glVertexAttrib1d( index, x );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib1dv( GLuint index, GLdouble* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB1DVPROC >( "glVertexAttrib1dv", out _glVertexAttrib1dv );

        _glVertexAttrib1dv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib1dv( GLuint index, params GLdouble[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB1DVPROC >( "glVertexAttrib1dv", out _glVertexAttrib1dv );

        fixed ( GLdouble* p = &v[ 0 ] )
        {
            _glVertexAttrib1dv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib1F( GLuint index, GLfloat x )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB1FPROC >( "glVertexAttrib1f", out _glVertexAttrib1f );

        _glVertexAttrib1f( index, x );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib1Fv( GLuint index, GLfloat* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB1FVPROC >( "glVertexAttrib1fv", out _glVertexAttrib1fv );

        _glVertexAttrib1fv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib1Fv( GLuint index, params GLfloat[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB1FVPROC >( "glVertexAttrib1fv", out _glVertexAttrib1fv );

        fixed ( GLfloat* p = &v[ 0 ] )
        {
            _glVertexAttrib1fv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib1S( GLuint index, GLshort x )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB1SPROC >( "glVertexAttrib1s", out _glVertexAttrib1s );

        _glVertexAttrib1s( index, x );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib1Sv( GLuint index, GLshort* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB1SVPROC >( "glVertexAttrib1sv", out _glVertexAttrib1sv );

        _glVertexAttrib1sv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib1Sv( GLuint index, params GLshort[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB1SVPROC >( "glVertexAttrib1sv", out _glVertexAttrib1sv );

        fixed ( GLshort* p = &v[ 0 ] )
        {
            _glVertexAttrib1sv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib2d( GLuint index, GLdouble x, GLdouble y )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB2DPROC >( "glVertexAttrib2d", out _glVertexAttrib2d );

        _glVertexAttrib2d( index, x, y );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib2dv( GLuint index, GLdouble* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB2DVPROC >( "glVertexAttrib2dv", out _glVertexAttrib2dv );

        _glVertexAttrib2dv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib2dv( GLuint index, params GLdouble[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB2DVPROC >( "glVertexAttrib2dv", out _glVertexAttrib2dv );

        fixed ( GLdouble* p = &v[ 0 ] )
        {
            _glVertexAttrib2dv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib2F( GLuint index, GLfloat x, GLfloat y )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB2FPROC >( "glVertexAttrib2f", out _glVertexAttrib2f );

        _glVertexAttrib2f( index, x, y );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib2Fv( GLuint index, GLfloat* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB2FVPROC >( "glVertexAttrib2fv", out _glVertexAttrib2fv );

        _glVertexAttrib2fv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib2Fv( GLuint index, params GLfloat[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB2FVPROC >( "glVertexAttrib2fv", out _glVertexAttrib2fv );

        fixed ( GLfloat* p = &v[ 0 ] )
        {
            _glVertexAttrib2fv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib2S( GLuint index, GLshort x, GLshort y )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB2SPROC >( "glVertexAttrib2s", out _glVertexAttrib2s );

        _glVertexAttrib2s( index, x, y );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib2Sv( GLuint index, GLshort* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB2SVPROC >( "glVertexAttrib2sv", out _glVertexAttrib2sv );

        _glVertexAttrib2sv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib2Sv( GLuint index, params GLshort[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB2SVPROC >( "glVertexAttrib2sv", out _glVertexAttrib2sv );

        fixed ( GLshort* p = &v[ 0 ] )
        {
            _glVertexAttrib2sv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib3d( GLuint index, GLdouble x, GLdouble y, GLdouble z )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB3DPROC >( "glVertexAttrib3d", out _glVertexAttrib3d );

        _glVertexAttrib3d( index, x, y, z );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib3dv( GLuint index, GLdouble* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB3DVPROC >( "glVertexAttrib3dv", out _glVertexAttrib3dv );

        _glVertexAttrib3dv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib3dv( GLuint index, params GLdouble[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB3DVPROC >( "glVertexAttrib3dv", out _glVertexAttrib3dv );

        fixed ( GLdouble* p = &v[ 0 ] )
        {
            _glVertexAttrib3dv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib3F( GLuint index, GLfloat x, GLfloat y, GLfloat z )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB3FPROC >( "glVertexAttrib3f", out _glVertexAttrib3f );

        _glVertexAttrib3f( index, x, y, z );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib3Fv( GLuint index, GLfloat* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB3FVPROC >( "glVertexAttrib3fv", out _glVertexAttrib3fv );

        _glVertexAttrib3fv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib3Fv( GLuint index, params GLfloat[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB3FVPROC >( "glVertexAttrib3fv", out _glVertexAttrib3fv );

        fixed ( GLfloat* p = &v[ 0 ] )
        {
            _glVertexAttrib3fv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib3S( GLuint index, GLshort x, GLshort y, GLshort z )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB3SPROC >( "glVertexAttrib3s", out _glVertexAttrib3s );

        _glVertexAttrib3s( index, x, y, z );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib3Sv( GLuint index, GLshort* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB3SVPROC >( "glVertexAttrib3sv", out _glVertexAttrib3sv );

        _glVertexAttrib3sv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib3Sv( GLuint index, params GLshort[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB3SVPROC >( "glVertexAttrib3sv", out _glVertexAttrib3sv );

        fixed ( GLshort* p = &v[ 0 ] )
        {
            _glVertexAttrib3sv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4Nbv( GLuint index, GLbyte* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NBVPROC >( "glVertexAttrib4Nbv", out _glVertexAttrib4Nbv );

        _glVertexAttrib4Nbv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4Nbv( GLuint index, params GLbyte[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NBVPROC >( "glVertexAttrib4Nbv", out _glVertexAttrib4Nbv );

        fixed ( GLbyte* p = &v[ 0 ] )
        {
            _glVertexAttrib4Nbv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4Niv( GLuint index, GLint* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NIVPROC >( "glVertexAttrib4Niv", out _glVertexAttrib4Niv );

        _glVertexAttrib4Niv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4Niv( GLuint index, params GLint[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NIVPROC >( "glVertexAttrib4Niv", out _glVertexAttrib4Niv );

        fixed ( GLint* p = &v[ 0 ] )
        {
            _glVertexAttrib4Niv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4Nsv( GLuint index, GLshort* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NSVPROC >( "glVertexAttrib4Nsv", out _glVertexAttrib4Nsv );

        _glVertexAttrib4Nsv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4Nsv( GLuint index, params GLshort[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NSVPROC >( "glVertexAttrib4Nsv", out _glVertexAttrib4Nsv );

        fixed ( GLshort* p = &v[ 0 ] )
        {
            _glVertexAttrib4Nsv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4Nub( GLuint index, GLubyte x, GLubyte y, GLubyte z, GLubyte w )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NUBPROC >( "glVertexAttrib4Nub", out _glVertexAttrib4Nub );

        _glVertexAttrib4Nub( index, x, y, z, w );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4Nubv( GLuint index, GLubyte* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NUBVPROC >( "glVertexAttrib4Nubv", out _glVertexAttrib4Nubv );

        _glVertexAttrib4Nubv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4Nubv( GLuint index, params GLubyte[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NUBVPROC >( "glVertexAttrib4Nubv", out _glVertexAttrib4Nubv );

        fixed ( GLubyte* p = &v[ 0 ] )
        {
            _glVertexAttrib4Nubv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4Nuiv( GLuint index, GLuint* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NUIVPROC >( "glVertexAttrib4Nuiv", out _glVertexAttrib4Nuiv );

        _glVertexAttrib4Nuiv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4Nuiv( GLuint index, params GLuint[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NUIVPROC >( "glVertexAttrib4Nuiv", out _glVertexAttrib4Nuiv );

        fixed ( GLuint* p = &v[ 0 ] )
        {
            _glVertexAttrib4Nuiv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4Nusv( GLuint index, GLushort* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NUSVPROC >( "glVertexAttrib4Nusv", out _glVertexAttrib4Nusv );

        _glVertexAttrib4Nusv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4Nusv( GLuint index, params GLushort[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NUSVPROC >( "glVertexAttrib4Nusv", out _glVertexAttrib4Nusv );

        fixed ( GLushort* p = &v[ 0 ] )
        {
            _glVertexAttrib4Nusv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4Bv( GLuint index, GLbyte* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4BVPROC >( "glVertexAttrib4bv", out _glVertexAttrib4bv );

        _glVertexAttrib4bv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4Bv( GLuint index, params GLbyte[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4BVPROC >( "glVertexAttrib4bv", out _glVertexAttrib4bv );

        fixed ( GLbyte* p = &v[ 0 ] )
        {
            _glVertexAttrib4bv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4d( GLuint index, GLdouble x, GLdouble y, GLdouble z, GLdouble w )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4DPROC >( "glVertexAttrib4d", out _glVertexAttrib4d );

        _glVertexAttrib4d( index, x, y, z, w );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4dv( GLuint index, GLdouble* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4DVPROC >( "glVertexAttrib4dv", out _glVertexAttrib4dv );

        _glVertexAttrib4dv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4dv( GLuint index, params GLdouble[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4DVPROC >( "glVertexAttrib4dv", out _glVertexAttrib4dv );

        fixed ( GLdouble* p = &v[ 0 ] )
        {
            _glVertexAttrib4dv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4F( GLuint index, GLfloat x, GLfloat y, GLfloat z, GLfloat w )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4FPROC >( "glVertexAttrib4f", out _glVertexAttrib4f );

        _glVertexAttrib4f( index, x, y, z, w );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4Fv( GLuint index, GLfloat* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4FVPROC >( "glVertexAttrib4fv", out _glVertexAttrib4fv );

        _glVertexAttrib4fv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4Fv( GLuint index, params GLfloat[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4FVPROC >( "glVertexAttrib4fv", out _glVertexAttrib4fv );

        fixed ( GLfloat* p = &v[ 0 ] )
        {
            _glVertexAttrib4fv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4Iv( GLuint index, GLint* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4IVPROC >( "glVertexAttrib4iv", out _glVertexAttrib4iv );

        _glVertexAttrib4iv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4Iv( GLuint index, params GLint[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4IVPROC >( "glVertexAttrib4iv", out _glVertexAttrib4iv );

        fixed ( GLint* p = &v[ 0 ] )
        {
            _glVertexAttrib4iv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4S( GLuint index, GLshort x, GLshort y, GLshort z, GLshort w )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4SPROC >( "glVertexAttrib4s", out _glVertexAttrib4s );

        _glVertexAttrib4s( index, x, y, z, w );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4Sv( GLuint index, GLshort* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4SVPROC >( "glVertexAttrib4sv", out _glVertexAttrib4sv );

        _glVertexAttrib4sv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4Sv( GLuint index, params GLshort[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4SVPROC >( "glVertexAttrib4sv", out _glVertexAttrib4sv );

        fixed ( GLshort* p = &v[ 0 ] )
        {
            _glVertexAttrib4sv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4Ubv( GLuint index, GLubyte* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4UBVPROC >( "glVertexAttrib4ubv", out _glVertexAttrib4ubv );

        _glVertexAttrib4ubv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4Ubv( GLuint index, params GLubyte[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4UBVPROC >( "glVertexAttrib4ubv", out _glVertexAttrib4ubv );

        fixed ( GLubyte* p = &v[ 0 ] )
        {
            _glVertexAttrib4ubv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4Uiv( GLuint index, GLuint* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4UIVPROC >( "glVertexAttrib4uiv", out _glVertexAttrib4uiv );

        _glVertexAttrib4uiv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4Uiv( GLuint index, params GLuint[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4UIVPROC >( "glVertexAttrib4uiv", out _glVertexAttrib4uiv );

        fixed ( GLuint* p = &v[ 0 ] )
        {
            _glVertexAttrib4uiv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4Usv( GLuint index, GLushort* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4USVPROC >( "glVertexAttrib4usv", out _glVertexAttrib4usv );

        _glVertexAttrib4usv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4Usv( GLuint index, params GLushort[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4USVPROC >( "glVertexAttrib4usv", out _glVertexAttrib4usv );

        fixed ( GLushort* p = &v[ 0 ] )
        {
            _glVertexAttrib4usv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttribPointer( GLuint index, GLint size, GLenum type, GLbool normalized, GLsizei stride,
                                     GLuint pointer )
    {
        VertexAttribPointer( index, size, type, normalized, stride, ( IntPtr )pointer );
    }

    /// <inheritdoc />
    public void VertexAttribPointer( GLuint index, GLint size, GLenum type, GLbool normalized, GLsizei stride,
                                     IntPtr pointer )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBPOINTERPROC >( "glVertexAttribPointer", out _glVertexAttribPointer );

        _glVertexAttribPointer( index, size, type, normalized, stride, pointer );
    }

    // ========================================================================

    /// <inheritdoc />
    public void UniformMatrix2X3Fv( GLint location, GLsizei count, GLbool transpose, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX2X3FVPROC >( "glUniformMatrix2x3fv", out _glUniformMatrix2x3fv );

        _glUniformMatrix2x3fv( location, count, transpose, value );
    }

    /// <inheritdoc />
    public void UniformMatrix2X3Fv( GLint location, GLbool transpose, params GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX2X3FVPROC >( "glUniformMatrix2x3fv", out _glUniformMatrix2x3fv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniformMatrix2x3fv( location, value.Length / 6, transpose, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void UniformMatrix3X2Fv( GLint location, GLsizei count, GLbool transpose, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX3X2FVPROC >( "glUniformMatrix3x2fv", out _glUniformMatrix3x2fv );

        _glUniformMatrix3x2fv( location, count, transpose, value );
    }

    /// <inheritdoc />
    public void UniformMatrix3X2Fv( GLint location, GLbool transpose, params GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX3X2FVPROC >( "glUniformMatrix3x2fv", out _glUniformMatrix3x2fv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniformMatrix3x2fv( location, value.Length / 6, transpose, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void UniformMatrix2X4Fv( GLint location, GLsizei count, GLbool transpose, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX2X4FVPROC >( "_glUniformMatrix2x4fv", out _glUniformMatrix2x4fv );

        _glUniformMatrix2x4fv( location, count, transpose, value );
    }

    /// <inheritdoc />
    public void UniformMatrix2X4Fv( GLint location, GLbool transpose, params GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX2X4FVPROC >( "_glUniformMatrix2x4fv", out _glUniformMatrix2x4fv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniformMatrix2x4fv( location, value.Length / 8, transpose, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void UniformMatrix4X2Fv( GLint location, GLsizei count, GLbool transpose, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX4X2FVPROC >( "glUniformMatrix4x2fv", out _glUniformMatrix4x2fv );

        _glUniformMatrix4x2fv( location, count, transpose, value );
    }

    /// <inheritdoc />
    public void UniformMatrix4X2Fv( GLint location, GLbool transpose, params GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX4X2FVPROC >( "glUniformMatrix4x2fv", out _glUniformMatrix4x2fv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniformMatrix4x2fv( location, value.Length / 8, transpose, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void UniformMatrix3X4Fv( GLint location, GLsizei count, GLbool transpose, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX3X4FVPROC >( "glUniformMatrix3x4fv", out _glUniformMatrix3x4fv );

        _glUniformMatrix3x4fv( location, count, transpose, value );
    }

    /// <inheritdoc />
    public void UniformMatrix3X4Fv( GLint location, GLbool transpose, params GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX3X4FVPROC >( "glUniformMatrix3x4fv", out _glUniformMatrix3x4fv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniformMatrix3x4fv( location, value.Length / 12, transpose, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void UniformMatrix4X3Fv( GLint location, GLsizei count, GLbool transpose, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX4X3FVPROC >( "glUniformMatrix4x3fv", out _glUniformMatrix4x3fv );

        _glUniformMatrix4x3fv( location, count, transpose, value );
    }

    /// <inheritdoc />
    public void UniformMatrix4X3Fv( GLint location, GLbool transpose, params GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX4X3FVPROC >( "glUniformMatrix4x3fv", out _glUniformMatrix4x3fv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniformMatrix4x3fv( location, value.Length / 12, transpose, p );
        }
    }

    #endregion Shader Bindings

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region Texture Bindings

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
                            GLbool enabled = true )
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
                                 GLbool enabled = true ) where T : unmanaged
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
                    _glTexImage2D
                        (
                         target,
                         level,
                         internalpixelformat,
                         width,
                         height,
                         border,
                         pixelFormat,
                         dataType,
                         ( IntPtr )p
                        );
                }
            }
        }
    }

    /// <inheritdoc />
    public void TexImage2D( GLenum target, GLint level, GLint border, Pixmap pixmap, GLbool enabled = true )
    {
        Logger.Checkpoint();

        GetDelegateForFunction< PFNGLTEXIMAGE2DPROC >( "glTexImage2D", out _glTexImage2D );

        unsafe
        {
            fixed ( byte* p = &pixmap.PixelData[ 0 ] )
            {
                Engine.GL.PixelStorei( PixelStoreParameter.UnpackAlignment, 1 );

                if ( enabled )
                {
                    _glTexImage2D
                        (
                         target,                       // int
                         level,                        // int
                         pixmap.GLInternalPixelFormat, // int
                         pixmap.Width,                 // int
                         pixmap.Height,                // int
                         border,                       // int
                         pixmap.GLPixelFormat,         // int
                         pixmap.GLDataType,            // int
                         *p
                        ); // nint
                }
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void CopyTexImage1D( GLenum target, GLint level, GLenum internalFormat, GLint x, GLint y, GLsizei width,
                                GLint border )
    {
        GetDelegateForFunction< PFNGLCOPYTEXIMAGE1DPROC >( "glCopyTexImage1D", out _glCopyTexImage1D );

        _glCopyTexImage1D( target, level, internalFormat, x, y, width, border );
    }

    // ========================================================================

    /// <inheritdoc />
    public void CopyTexImage2D( GLenum target, GLint level, GLenum internalFormat, GLint x, GLint y, GLsizei width,
                                GLsizei height,
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
    public void CopyTexSubImage2D( GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint x, GLint y,
                                   GLsizei width,
                                   GLsizei height )
    {
        GetDelegateForFunction< PFNGLCOPYTEXSUBIMAGE2DPROC >( "glCopyTexSubImage2D", out _glCopyTexSubImage2D );

        _glCopyTexSubImage2D( target, level, xoffset, yoffset, x, y, width, height );
    }

    // ========================================================================

    /// <inheritdoc />
    public void TexSubImage1D( GLenum target, GLint level, GLint xoffset, GLsizei width, GLenum format, GLenum type,
                               IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLTEXSUBIMAGE1DPROC >( "glTexSubImage1D", out _glTexSubImage1D );

        _glTexSubImage1D( target, level, xoffset, width, format, type, pixels );
    }

    /// <inheritdoc />
    public void TexSubImage1D< T >( GLenum target, GLint level, GLint xoffset, GLsizei width, GLenum format,
                                    GLenum type, T[] pixels )
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
    public GLbool IsGLTexture( GLuint texture )
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

                _glTexSubImage3D
                    (
                     target,
                     level,
                     xoffset,
                     yoffset,
                     zoffset,
                     width,
                     height,
                     depth,
                     format,
                     type,
                     dataptr
                    );
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
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXIMAGE3DPROC >
            (
             "glCompressedTexImage3D",
             out _glCompressedTexImage3D
            );

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
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXIMAGE3DPROC >
            (
             "glCompressedTexImage3D",
             out _glCompressedTexImage3D
            );

        unsafe
        {
            fixed ( byte* p = &data[ 0 ] )
            {
                _glCompressedTexImage3D
                    (
                     target,
                     level,
                     internalFormat,
                     width,
                     height,
                     depth,
                     border,
                     data.Length,
                     ( IntPtr )p
                    );
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
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXIMAGE2DPROC >
            (
             "glCompressedTexImage2D",
             out _glCompressedTexImage2D
            );

        _glCompressedTexImage2D( target, level, internalFormat, width, height, border, imageSize, data );
    }

    /// <inheritdoc />
    public void CompressedTexImage2D( GLenum target, GLint level, GLenum internalFormat, GLsizei width, GLsizei height,
                                      GLint border,
                                      byte[] data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXIMAGE2DPROC >
            (
             "glCompressedTexImage2D",
             out _glCompressedTexImage2D
            );

        unsafe
        {
            fixed ( byte* p = &data[ 0 ] )
            {
                _glCompressedTexImage2D
                    (
                     target,
                     level,
                     internalFormat,
                     width,
                     height,
                     border,
                     data.Length,
                     ( IntPtr )p
                    );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void CompressedTexImage1D( GLenum target, GLint level, GLenum internalFormat, GLsizei width, GLint border,
                                      GLsizei imageSize,
                                      IntPtr data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXIMAGE1DPROC >
            (
             "glCompressedTexImage1D",
             out _glCompressedTexImage1D
            );

        _glCompressedTexImage1D( target, level, internalFormat, width, border, imageSize, data );
    }

    /// <inheritdoc />
    public void CompressedTexImage1D( GLenum target, GLint level, GLenum internalFormat, GLsizei width, GLint border,
                                      byte[] data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXIMAGE1DPROC >
            (
             "glCompressedTexImage1D",
             out _glCompressedTexImage1D
            );

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
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXSUBIMAGE3DPROC >
            (
             "glCompressedTexSubImage3D",
             out _glCompressedTexSubImage3D
            );

        _glCompressedTexSubImage3D
            (
             target,
             level,
             xoffset,
             yoffset,
             zoffset,
             width,
             height,
             depth,
             format,
             imageSize,
             data
            );
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
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXSUBIMAGE3DPROC >
            (
             "glCompressedTexSubImage3D",
             out _glCompressedTexSubImage3D
            );

        unsafe
        {
            fixed ( byte* p = &data[ 0 ] )
            {
                _glCompressedTexSubImage3D
                    (
                     target,
                     level,
                     xoffset,
                     yoffset,
                     zoffset,
                     width,
                     height,
                     depth,
                     format,
                     data.Length,
                     ( IntPtr )p
                    );
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
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXSUBIMAGE2DPROC >
            (
             "glCompressedTexSubImage2D",
             out _glCompressedTexSubImage2D
            );

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
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXSUBIMAGE2DPROC >
            (
             "glCompressedTexSubImage2D",
             out _glCompressedTexSubImage2D
            );

        unsafe
        {
            fixed ( byte* p = &data[ 0 ] )
            {
                _glCompressedTexSubImage2D
                    (
                     target,
                     level,
                     xoffset,
                     yoffset,
                     width,
                     height,
                     format,
                     data.Length,
                     ( IntPtr )p
                    );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void CompressedTexSubImage1D( GLenum target, GLint level, GLint xoffset, GLsizei width, GLenum format,
                                         GLsizei imageSize,
                                         IntPtr data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXSUBIMAGE1DPROC >
            (
             "glCompressedTexSubImage1D",
             out _glCompressedTexSubImage1D
            );

        _glCompressedTexSubImage1D( target, level, xoffset, width, format, imageSize, data );
    }

    /// <inheritdoc />
    public void CompressedTexSubImage1D( GLenum target, GLint level, GLint xoffset, GLsizei width, GLenum format,
                                         byte[] data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXSUBIMAGE1DPROC >
            (
             "glCompressedTexSubImage1D",
             out _glCompressedTexSubImage1D
            );

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
        GetDelegateForFunction< PFNGLGETCOMPRESSEDTEXIMAGEPROC >
            (
             "glGetCompressedTexImage",
             out _glGetCompressedTexImage
            );

        _glGetCompressedTexImage( target, level, img );
    }

    /// <inheritdoc />
    public void GetCompressedTexImage( GLenum target, GLint level, ref byte[] img )
    {
        GetDelegateForFunction< PFNGLGETCOMPRESSEDTEXIMAGEPROC >
            (
             "glGetCompressedTexImage",
             out _glGetCompressedTexImage
            );

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
    public void GetTexImage< T >( GLenum target, GLint level, GLenum format, GLenum type, ref T[] pixels )
        where T : unmanaged
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

        GetTexParameteriv( target, pname, ref tempArray );

        data = tempArray[ 0 ];
    }

    // ========================================================================

    /// <inheritdoc />
    public unsafe void GetTexLevelParameterfv( GLenum target, GLint level, GLenum pname, GLfloat* parameters )
    {
        GetDelegateForFunction< PFNGLGETTEXLEVELPARAMETERFVPROC >
            (
             "glGetTexLevelParameterfv",
             out _glGetTexLevelParameterfv
            );

        _glGetTexLevelParameterfv( target, level, pname, parameters );
    }

    /// <inheritdoc />
    public void GetTexLevelParameterfv( GLenum target, GLint level, GLenum pname, ref GLfloat[] parameters )
    {
        GetDelegateForFunction< PFNGLGETTEXLEVELPARAMETERFVPROC >
            (
             "glGetTexLevelParameterfv",
             out _glGetTexLevelParameterfv
            );

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
        GetDelegateForFunction< PFNGLGETTEXLEVELPARAMETERIVPROC >
            (
             "glGetTexLevelParameteriv",
             out _glGetTexLevelParameteriv
            );

        _glGetTexLevelParameteriv( target, level, pname, parameters );
    }

    /// <inheritdoc />
    public void GetTexLevelParameteriv( GLenum target, GLint level, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETTEXLEVELPARAMETERIVPROC >
            (
             "glGetTexLevelParameteriv",
             out _glGetTexLevelParameteriv
            );

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
                                       GLbool fixedsamplelocations )
    {
        GetDelegateForFunction< PFNGLTEXIMAGE2DMULTISAMPLEPROC >
            (
             "glTexImage2DMultisample",
             out _glTexImage2DMultisample
            );

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
                                       GLbool fixedsamplelocations )
    {
        GetDelegateForFunction< PFNGLTEXIMAGE3DMULTISAMPLEPROC >
            (
             "glTexImage3DMultisample",
             out _glTexImage3DMultisample
            );

        _glTexImage3DMultisample( target, samples, internalFormat, width, height, depth, fixedsamplelocations );
    }

    #endregion Texture Bindings

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region Transform Bindings

    /// <inheritdoc />
    public void BeginTransformFeedback( GLenum primitiveMode )
    {
        GetDelegateForFunction< PFNGLBEGINTRANSFORMFEEDBACKPROC >
            (
             "glBeginTransformFeedback",
             out _glBeginTransformFeedback
            );

        _glBeginTransformFeedback( primitiveMode );
    }

    // ========================================================================

    /// <inheritdoc />
    public void EndTransformFeedback()
    {
        GetDelegateForFunction< PFNGLENDTRANSFORMFEEDBACKPROC >
            (
             "glEndTransformFeedback",
             out _glEndTransformFeedback
            );

        _glEndTransformFeedback();
    }

    // ========================================================================

    public void BindTransformFeedback( GLenum target, GLuint id )
    {
        GetDelegateForFunction< PFNGLBINDTRANSFORMFEEDBACKPROC >
            (
             "glBindTransformFeedback",
             out _glBindTransformFeedback
            );

        _glBindTransformFeedback( target, id );
    }

    // ========================================================================

    public void DeleteTransformFeedbacks( GLsizei n, GLuint* ids )
    {
        GetDelegateForFunction< PFNGLDELETETRANSFORMFEEDBACKSPROC >
            (
             "glDeleteTransformFeedbacks",
             out _glDeleteTransformFeedbacks
            );

        _glDeleteTransformFeedbacks( n, ids );
    }

    public void DeleteTransformFeedbacks( params GLuint[] ids )
    {
        GetDelegateForFunction< PFNGLDELETETRANSFORMFEEDBACKSPROC >
            (
             "glDeleteTransformFeedbacks",
             out _glDeleteTransformFeedbacks
            );

        fixed ( GLuint* p = &ids[ 0 ] )
        {
            _glDeleteTransformFeedbacks( ids.Length, p );
        }
    }

    // ========================================================================

    public void GenTransformFeedbacks( GLsizei n, GLuint* ids )
    {
        GetDelegateForFunction< PFNGLGENTRANSFORMFEEDBACKSPROC >
            (
             "glGenTransformFeedbacks",
             out _glGenTransformFeedbacks
            );

        _glGenTransformFeedbacks( n, ids );
    }

    public GLuint[] GenTransformFeedbacks( GLsizei n )
    {
        var r = new GLuint[ n ];

        GetDelegateForFunction< PFNGLGENTRANSFORMFEEDBACKSPROC >
            (
             "glGenTransformFeedbacks",
             out _glGenTransformFeedbacks
            );

        fixed ( GLuint* p = &r[ 0 ] )
        {
            _glGenTransformFeedbacks( n, p );
        }

        return r;
    }

    public GLuint GenTransformFeedback()
    {
        return GenTransformFeedbacks( 1 )[ 0 ];
    }

    // ========================================================================

    public GLbool IsTransformFeedback( GLuint id )
    {
        GetDelegateForFunction< PFNGLISTRANSFORMFEEDBACKPROC >( "glIsTransformFeedback", out _glIsTransformFeedback );

        return _glIsTransformFeedback( id );
    }

    // ========================================================================

    public void PauseTransformFeedback()
    {
        GetDelegateForFunction< PFNGLPAUSETRANSFORMFEEDBACKPROC >
            (
             "glPauseTransformFeedback",
             out _glPauseTransformFeedback
            );

        _glPauseTransformFeedback();
    }

    // ========================================================================

    public void ResumeTransformFeedback()
    {
        GetDelegateForFunction< PFNGLRESUMETRANSFORMFEEDBACKPROC >
            (
             "glResumeTransformFeedback",
             out _glResumeTransformFeedback
            );

        _glResumeTransformFeedback();
    }

    // ========================================================================

    public void DrawTransformFeedback( GLenum mode, GLuint id )
    {
        GetDelegateForFunction< PFNGLDRAWTRANSFORMFEEDBACKPROC >
            (
             "glDrawTransformFeedback",
             out _glDrawTransformFeedback
            );

        _glDrawTransformFeedback( mode, id );
    }

    // ========================================================================

    public void DrawTransformFeedbackStream( GLenum mode, GLuint id, GLuint stream )
    {
        GetDelegateForFunction< PFNGLDRAWTRANSFORMFEEDBACKSTREAMPROC >
            (
             "glDrawTransformFeedbackStream",
             out _glDrawTransformFeedbackStream
            );

        _glDrawTransformFeedbackStream( mode, id, stream );
    }

    // ========================================================================

    public void DrawTransformFeedbackInstanced( GLenum mode, GLuint id, GLsizei instancecount )
    {
        GetDelegateForFunction< PFNGLDRAWTRANSFORMFEEDBACKINSTANCEDPROC >
            (
             "glDrawTransformFeedbackInstanced",
             out _glDrawTransformFeedbackInstanced
            );

        _glDrawTransformFeedbackInstanced( mode, id, instancecount );
    }

    // ========================================================================

    public void DrawTransformFeedbackStreamInstanced( GLenum mode, GLuint id, GLuint stream, GLsizei instancecount )
    {
        GetDelegateForFunction<
                PFNGLDRAWTRANSFORMFEEDBACKSTREAMINSTANCEDPROC >
            (
             "glDrawTransformFeedbackStreamInstanced",
             out _glDrawTransformFeedbackStreamInstanced
            );

        _glDrawTransformFeedbackStreamInstanced( mode, id, stream, instancecount );
    }

    // ========================================================================

    public void CreateTransformFeedbacks( GLsizei n, GLuint* ids )
    {
        GetDelegateForFunction< PFNGLCREATETRANSFORMFEEDBACKSPROC >
            (
             "glCreateTransformFeedbacks",
             out _glCreateTransformFeedbacks
            );

        _glCreateTransformFeedbacks( n, ids );
    }

    public GLuint[] CreateTransformFeedbacks( GLsizei n )
    {
        var ids = new GLuint[ n ];

        GetDelegateForFunction< PFNGLCREATETRANSFORMFEEDBACKSPROC >
            (
             "glCreateTransformFeedbacks",
             out _glCreateTransformFeedbacks
            );

        fixed ( GLuint* pIds = &ids[ 0 ] )
        {
            _glCreateTransformFeedbacks( n, pIds );
        }

        return ids;
    }

    public GLuint CreateTransformFeedbacks()
    {
        return CreateTransformFeedbacks( 1 )[ 0 ];
    }

    // ========================================================================

    public void TransformFeedbackBufferBase( GLuint xfb, GLuint index, GLuint buffer )
    {
        GetDelegateForFunction< PFNGLTRANSFORMFEEDBACKBUFFERBASEPROC >
            (
             "glTransformFeedbackBufferBase",
             out _glTransformFeedbackBufferBase
            );

        _glTransformFeedbackBufferBase( xfb, index, buffer );
    }

    // ========================================================================

    public void TransformFeedbackBufferRange( GLuint xfb, GLuint index, GLuint buffer, GLintptr offset,
                                              GLsizeiptr size )
    {
        GetDelegateForFunction< PFNGLTRANSFORMFEEDBACKBUFFERRANGEPROC >
            (
             "glTransformFeedbackBufferRange",
             out _glTransformFeedbackBufferRange
            );

        _glTransformFeedbackBufferRange( xfb, index, buffer, offset, size );
    }

    // ========================================================================

    public void GetTransformFeedbackiv( GLuint xfb, GLenum pname, GLint* param )
    {
        GetDelegateForFunction< PFNGLGETTRANSFORMFEEDBACKIVPROC >
            (
             "glGetTransformFeedbackiv",
             out _glGetTransformFeedbackiv
            );

        _glGetTransformFeedbackiv( xfb, pname, param );
    }

    public void GetTransformFeedbackiv( GLuint xfb, GLenum pname, ref GLint[] param )
    {
        GetDelegateForFunction< PFNGLGETTRANSFORMFEEDBACKIVPROC >
            (
             "glGetTransformFeedbackiv",
             out _glGetTransformFeedbackiv
            );

        fixed ( GLint* pParam = &param[ 0 ] )
        {
            _glGetTransformFeedbackiv( xfb, pname, pParam );
        }
    }

    // ========================================================================

    public void GetTransformFeedbacki_v( GLuint xfb, GLenum pname, GLuint index, GLint* param )
    {
        GetDelegateForFunction< PFNGLGETTRANSFORMFEEDBACKI_VPROC >
            (
             "glGetTransformFeedbacki_v",
             out _glGetTransformFeedbacki_v
            );

        _glGetTransformFeedbacki_v( xfb, pname, index, param );
    }

    public void GetTransformFeedbacki_v( GLuint xfb, GLenum pname, GLuint index, ref GLint[] param )
    {
        GetDelegateForFunction< PFNGLGETTRANSFORMFEEDBACKI_VPROC >
            (
             "glGetTransformFeedbacki_v",
             out _glGetTransformFeedbacki_v
            );

        fixed ( GLint* pParam = &param[ 0 ] )
        {
            _glGetTransformFeedbacki_v( xfb, pname, index, pParam );
        }
    }

    // ========================================================================

    public void GetTransformFeedbacki64_v( GLuint xfb, GLenum pname, GLuint index, GLint64* param )
    {
        GetDelegateForFunction< PFNGLGETTRANSFORMFEEDBACKI64_VPROC >
            (
             "glGetTransformFeedbacki64_v",
             out _glGetTransformFeedbacki64_v
            );

        _glGetTransformFeedbacki64_v( xfb, pname, index, param );
    }

    public void GetTransformFeedbacki64_v( GLuint xfb, GLenum pname, GLuint index, ref GLint64[] param )
    {
        GetDelegateForFunction< PFNGLGETTRANSFORMFEEDBACKI64_VPROC >
            (
             "glGetTransformFeedbacki64_v",
             out _glGetTransformFeedbacki64_v
            );

        fixed ( GLint64* pParam = &param[ 0 ] )
        {
            _glGetTransformFeedbacki64_v( xfb, pname, index, pParam );
        }
    }

    // ========================================================================

    public void TransformFeedbackVaryings( GLuint program, GLsizei count, GLchar** varyings, GLenum bufferMode )
    {
        GetDelegateForFunction< PFNGLTRANSFORMFEEDBACKVARYINGSPROC >
            (
             "glTransformFeedbackVaryings",
             out _glTransformFeedbackVaryings
            );

        _glTransformFeedbackVaryings( ( uint )program, count, varyings, bufferMode );
    }

    // ========================================================================

    public void TransformFeedbackVaryings( GLuint program, string[] varyings, GLenum bufferMode )
    {
        var varyingsBytes = new GLchar[ varyings.Length ][];

        for ( var i = 0; i < varyings.Length; i++ )
        {
            varyingsBytes[ i ] = Encoding.UTF8.GetBytes( varyings[ i ] );
        }

        var varyingsPtrs = new GLchar*[ varyings.Length ];

        for ( var i = 0; i < varyings.Length; i++ )
        {
            fixed ( GLchar* p = &varyingsBytes[ i ][ 0 ] )
            {
                varyingsPtrs[ i ] = p;
            }
        }

        GetDelegateForFunction< PFNGLTRANSFORMFEEDBACKVARYINGSPROC >
            (
             "glTransformFeedbackVaryings",
             out _glTransformFeedbackVaryings
            );

        fixed ( GLchar** p = &varyingsPtrs[ 0 ] )
        {
            _glTransformFeedbackVaryings( ( uint )program, varyings.Length, p, bufferMode );
        }
    }

    // ========================================================================

    public void GetTransformFeedbackVarying( GLuint program,
                                             GLuint index,
                                             GLsizei bufSize,
                                             GLsizei* length,
                                             GLsizei* size,
                                             GLenum* type,
                                             GLchar* name )
    {
        GetDelegateForFunction< PFNGLGETTRANSFORMFEEDBACKVARYINGPROC >
            (
             "glGetTransformFeedbackVarying",
             out _glGetTransformFeedbackVarying
            );

        _glGetTransformFeedbackVarying( ( uint )program, index, bufSize, length, size, type, name );
    }

    // ========================================================================

    public string GetTransformFeedbackVarying( GLuint program, GLuint index, GLsizei bufSize, out GLsizei size,
                                               out GLenum type )
    {
        var name = new GLchar[ bufSize ];

        GetDelegateForFunction< PFNGLGETTRANSFORMFEEDBACKVARYINGPROC >
            (
             "glGetTransformFeedbackVarying",
             out _glGetTransformFeedbackVarying
            );

        fixed ( GLsizei* pSize = &size )
        {
            fixed ( GLenum* pType = &type )
            {
                fixed ( GLchar* p = &name[ 0 ] )
                {
                    GLsizei length;

                    _glGetTransformFeedbackVarying( ( uint )program, index, bufSize, &length, pSize, pType, p );

                    return new string( ( GLbyte* )p, 0, length, Encoding.UTF8 );
                }
            }
        }
    }

    #endregion Transform Bindings

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region Uniform Bindings

    /// <inheritdoc />
    public GLint GetUniformLocation( GLint program, GLchar* name )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == -1 ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETUNIFORMLOCATIONPROC >( "glGetUniformLocation", out _glGetUniformLocation );

        return _glGetUniformLocation( ( GLuint )program, name );
    }

    /// <inheritdoc />
    public GLint GetUniformLocation( GLint program, string name )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == -1 ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETUNIFORMLOCATIONPROC >( "glGetUniformLocation", out _glGetUniformLocation );

        fixed ( byte* pname = Encoding.UTF8.GetBytes( name ) )
        {
            return _glGetUniformLocation( ( uint )program, pname );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetUniformfv( GLint program, GLint location, GLfloat* parameters )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == -1 ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETUNIFORMFVPROC >( "glGetUniformfv", out _glGetUniformfv );

        _glGetUniformfv( ( uint )program, location, parameters );
    }

    /// <inheritdoc />
    public void GetUniformfv( GLint program, GLint location, ref GLfloat[] parameters )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == -1 ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETUNIFORMFVPROC >( "glGetUniformfv", out _glGetUniformfv );

        fixed ( GLfloat* pparams = &parameters[ 0 ] )
        {
            _glGetUniformfv( ( uint )program, location, pparams );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetUniformiv( int program, int location, int* parameters )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == -1 ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETUNIFORMIVPROC >( "glGetUniformiv", out _glGetUniformiv );

        _glGetUniformiv( ( uint )program, location, parameters );
    }

    /// <inheritdoc />
    public void GetUniformiv( int program, int location, ref int[] parameters )
    {
        if ( !Engine.GL.IsProgram( program ) || ( program == -1 ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETUNIFORMIVPROC >( "glGetUniformiv", out _glGetUniformiv );

        fixed ( GLint* pparams = &parameters[ 0 ] )
        {
            _glGetUniformiv( ( uint )program, location, pparams );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform1F( GLint location, GLfloat v0 )
    {
        GetDelegateForFunction< PFNGLUNIFORM1FPROC >( "glUniform1f", out _glUniform1f );

        _glUniform1f( location, v0 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform2F( GLint location, GLfloat v0, GLfloat v1 )
    {
        GetDelegateForFunction< PFNGLUNIFORM2FPROC >( "glUniform2f", out _glUniform2f );

        _glUniform2f( location, v0, v1 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform3F( GLint location, GLfloat v0, GLfloat v1, GLfloat v2 )
    {
        GetDelegateForFunction< PFNGLUNIFORM3FPROC >( "glUniform3f", out _glUniform3f );

        _glUniform3f( location, v0, v1, v2 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform4F( GLint location, GLfloat v0, GLfloat v1, GLfloat v2, GLfloat v3 )
    {
        GetDelegateForFunction< PFNGLUNIFORM4FPROC >( "glUniform4f", out _glUniform4f );

        _glUniform4f( location, v0, v1, v2, v3 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform1I( GLint location, GLint v0 )
    {
        GetDelegateForFunction< PFNGLUNIFORM1IPROC >( "glUniform1i", out _glUniform1i );

        _glUniform1i( location, v0 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform2I( GLint location, GLint v0, GLint v1 )
    {
        GetDelegateForFunction< PFNGLUNIFORM2IPROC >( "glUniform2i", out _glUniform2i );

        _glUniform2i( location, v0, v1 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform3I( GLint location, GLint v0, GLint v1, GLint v2 )
    {
        GetDelegateForFunction< PFNGLUNIFORM3IPROC >( "glUniform23", out _glUniform3i );

        _glUniform3i( location, v0, v1, v2 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform4I( GLint location, GLint v0, GLint v1, GLint v2, GLint v3 )
    {
        GetDelegateForFunction< PFNGLUNIFORM4IPROC >( "glUniform4i", out _glUniform4i );

        _glUniform4i( location, v0, v1, v2, v3 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform1Fv( GLint location, GLsizei count, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM1FVPROC >( "glUniform1fv", out _glUniform1fv );

        _glUniform1fv( location, count, value );
    }

    /// <inheritdoc />
    public void Uniform1Fv( GLint location, params GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM1FVPROC >( "glUniform1fv", out _glUniform1fv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniform1fv( location, value.Length, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform2Fv( GLint location, GLsizei count, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM2FVPROC >( "glUniform2fv", out _glUniform2fv );

        _glUniform2fv( location, count, value );
    }

    /// <inheritdoc />
    public void Uniform2Fv( GLint location, params GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM2FVPROC >( "glUniform2fv", out _glUniform2fv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniform2fv( location, value.Length / 2, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform3Fv( GLint location, GLsizei count, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM3FVPROC >( "glUniform3fv", out _glUniform3fv );

        _glUniform3fv( location, count, value );
    }

    /// <inheritdoc />
    public void Uniform3Fv( GLint location, params GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM3FVPROC >( "glUniform3fv", out _glUniform3fv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniform3fv( location, value.Length / 3, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform4Fv( GLint location, GLsizei count, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM4FVPROC >( "glUniform4fv", out _glUniform4fv );

        _glUniform4fv( location, count, value );
    }

    /// <inheritdoc />
    public void Uniform4Fv( GLint location, params GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM4FVPROC >( "glUniform4fv", out _glUniform4fv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniform4fv( location, value.Length / 4, p );
        }
    }

    // ========================================================================

    /// <summary>
    /// Specifies the value of a uniform variable for the current program object
    /// </summary>
    /// <param name="location">Specifies the location of the uniform value to be modified.</param>
    /// <param name="count">Specifies the number of elements that are to be modified.</param>
    /// <param name="value">
    /// Specifies a pointer to an array of <paramref name="count"/> values that will be used to update the
    /// specified uniform variable.
    /// </param>
    public void Uniform1Iv( GLint location, GLsizei count, GLint* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM1IVPROC >( "glUniform1iv", out _glUniform1iv );

        _glUniform1iv( location, count, value );
    }

    /// <summary>
    /// Specifies the value of a uniform variable for the current program object
    /// </summary>
    /// <param name="location">Specifies the location of the uniform value to be modified.</param>
    /// <param name="value">
    /// Specifies an array of values that will be used to update the specified uniform variable.
    /// </param>
    public void Uniform1Iv( GLint location, params GLint[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM1IVPROC >( "glUniform1iv", out _glUniform1iv );

        fixed ( GLint* p = &value[ 0 ] )
        {
            _glUniform1iv( location, value.Length, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform2Iv( GLint location, GLsizei count, GLint* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM2IVPROC >( "glUniform2iv", out _glUniform2iv );

        _glUniform2iv( location, count, value );
    }

    /// <inheritdoc />
    public void Uniform2Iv( GLint location, params GLint[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM2IVPROC >( "glUniform2iv", out _glUniform2iv );

        fixed ( GLint* p = &value[ 0 ] )
        {
            _glUniform2iv( location, value.Length / 2, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform3Iv( GLint location, GLsizei count, GLint* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM3IVPROC >( "glUniform3iv", out _glUniform3iv );

        _glUniform3iv( location, count, value );
    }

    /// <inheritdoc />
    public void Uniform3Iv( GLint location, params GLint[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM3IVPROC >( "glUniform3iv", out _glUniform3iv );

        fixed ( GLint* p = &value[ 0 ] )
        {
            _glUniform3iv( location, value.Length / 3, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform4Iv( GLint location, GLsizei count, GLint* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM4IVPROC >( "glUniform4iv", out _glUniform4iv );

        _glUniform4iv( location, count, value );
    }

    /// <inheritdoc />
    public void Uniform4Iv( GLint location, params GLint[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM4IVPROC >( "glUniform4iv", out _glUniform4iv );

        fixed ( GLint* p = &value[ 0 ] )
        {
            _glUniform4iv( location, value.Length / 4, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void UniformMatrix2Fv( GLint location, GLsizei count, GLbool transpose, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX2FVPROC >( "glUniformMatrix2fv", out _glUniformMatrix2fv );

        _glUniformMatrix2fv( location, count, transpose, value );
    }

    /// <inheritdoc />
    public void UniformMatrix2Fv( GLint location, GLbool transpose, params GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX2FVPROC >( "glUniformMatrix2fv", out _glUniformMatrix2fv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniformMatrix2fv( location, value.Length / 4, transpose, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void UniformMatrix3Fv( GLint location, GLsizei count, GLbool transpose, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX3FVPROC >( "glUniformMatrix3fv", out _glUniformMatrix3fv );

        _glUniformMatrix3fv( location, count, transpose, value );
    }

    /// <inheritdoc />
    public void UniformMatrix3Fv( GLint location, GLbool transpose, params GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX3FVPROC >( "glUniformMatrix3fv", out _glUniformMatrix3fv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniformMatrix3fv( location, value.Length / 9, transpose, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void UniformMatrix4Fv( GLint location, GLsizei count, GLbool transpose, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX4FVPROC >( "glUniformMatrix4fv", out _glUniformMatrix4fv );

        _glUniformMatrix4fv( location, count, transpose, value );
    }

    /// <summary>
    /// Specifies the value of a uniform variable for the current program object
    /// </summary>
    /// <param name="location">Specifies the location of the uniform value to be modified.</param>
    /// <param name="transpose">
    /// Specifies whether to transpose the matrix as the values are loaded into the uniform variable.
    /// </param>
    /// <param name="value">
    /// Specifies an array of values that will be used to update the specified uniform variable. Needs 16
    /// values per matrix.
    /// </param>
    public void UniformMatrix4Fv( int location, GLbool transpose, params GLfloat[] value )
    {
        Guard.Against.Null( value );

        GetDelegateForFunction< PFNGLUNIFORMMATRIX4FVPROC >( "glUniformMatrix4fv", out _glUniformMatrix4fv );

        if ( ( value.Length % 16 ) != 0 )
        {
            throw new LughRuntimeException
                (
                 $"Error: value array length ({value.Length}) is not a multiple " +
                 $"of 16.  Must provide a whole number of 4x4 matrices."
                );
        }

        GetDelegateForFunction< PFNGLGETERRORPROC >( "glGetError", out _glGetError );

        int matrixCount = value.Length / 16;

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniformMatrix4fv( location, matrixCount, transpose, p );

            int error = _glGetError();

            if ( error != IGL.GLNoError )
            {
                throw new LughRuntimeException
                    (
                     $"OpenGL Error: {error} after glUniformMatrix4fv. Location: {location}, " +
                     $"Matrix Count: {matrixCount}, Transpose: {transpose}"
                    );
            }
        }
    }

    // ========================================================================

    public void GetnUniformdv( GLuint program, GLint location, GLsizei bufSize, GLdouble* parameters )
    {
        GetDelegateForFunction< PFNGLGETNUNIFORMDVPROC >( "glGetnUniformdv", out _glGetnUniformdv );

        _glGetnUniformdv( program, location, bufSize, parameters );
    }

    public void GetnUniformdv( GLuint program, GLint location, GLsizei bufSize, ref GLdouble[] parameters )
    {
        GetDelegateForFunction< PFNGLGETNUNIFORMDVPROC >( "glGetnUniformdv", out _glGetnUniformdv );

        fixed ( void* ptrParameters = &parameters[ 0 ] )
        {
            _glGetnUniformdv( program, location, bufSize, ( GLdouble* )ptrParameters );
        }
    }

    // ========================================================================

    public void GetnUniformfv( GLuint program, GLint location, GLsizei bufSize, GLfloat* parameters )
    {
        GetDelegateForFunction< PFNGLGETNUNIFORMFVPROC >( "glGetnUniformfv", out _glGetnUniformfv );

        _glGetnUniformfv( program, location, bufSize, parameters );
    }

    public void GetnUniformfv( GLuint program, GLint location, GLsizei bufSize, ref GLfloat[] parameters )
    {
        GetDelegateForFunction< PFNGLGETNUNIFORMFVPROC >( "glGetnUniformfv", out _glGetnUniformfv );

        fixed ( void* ptrParameters = &parameters[ 0 ] )
        {
            _glGetnUniformfv( program, location, bufSize, ( GLfloat* )ptrParameters );
        }
    }

    // ========================================================================

    public void GetnUniformiv( GLuint program, GLint location, GLsizei bufSize, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETNUNIFORMIVPROC >( "glGetnUniformiv", out _glGetnUniformiv );

        _glGetnUniformiv( program, location, bufSize, parameters );
    }

    public void GetnUniformiv( GLuint program, GLint location, GLsizei bufSize, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETNUNIFORMIVPROC >( "glGetnUniformiv", out _glGetnUniformiv );

        fixed ( void* ptrParameters = &parameters[ 0 ] )
        {
            _glGetnUniformiv( program, location, bufSize, ( GLint* )ptrParameters );
        }
    }

    // ========================================================================

    public void GetnUniformuiv( GLuint program, GLint location, GLsizei bufSize, GLuint* parameters )
    {
        GetDelegateForFunction< PFNGLGETNUNIFORMUIVPROC >( "glGetnUniformuiv", out _glGetnUniformuiv );

        _glGetnUniformuiv( program, location, bufSize, parameters );
    }

    public void GetnUniformuiv( GLuint program, GLint location, GLsizei bufSize, ref GLuint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETNUNIFORMUIVPROC >( "glGetnUniformuiv", out _glGetnUniformuiv );

        fixed ( void* ptrParameters = &parameters[ 0 ] )
        {
            _glGetnUniformuiv( program, location, bufSize, ( GLuint* )ptrParameters );
        }
    }

    // ========================================================================

    public void Uniform1d( GLint location, GLdouble x )
    {
        GetDelegateForFunction< PFNGLUNIFORM1DPROC >( "glUniform1d", out _glUniform1d );

        _glUniform1d( location, x );
    }

    // ========================================================================

    public void Uniform2d( GLint location, GLdouble x, GLdouble y )
    {
        GetDelegateForFunction< PFNGLUNIFORM2DPROC >( "glUniform2d", out _glUniform2d );

        _glUniform2d( location, x, y );
    }

    // ========================================================================

    public void Uniform3d( GLint location, GLdouble x, GLdouble y, GLdouble z )
    {
        GetDelegateForFunction< PFNGLUNIFORM3DPROC >( "glUniform3d", out _glUniform3d );

        _glUniform3d( location, x, y, z );
    }

    // ========================================================================

    public void Uniform4d( GLint location, GLdouble x, GLdouble y, GLdouble z, GLdouble w )
    {
        GetDelegateForFunction< PFNGLUNIFORM4DPROC >( "glUniform4d", out _glUniform4d );

        _glUniform4d( location, x, y, z, w );
    }

    // ========================================================================

    public void Uniform1dv( GLint location, GLsizei count, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM1DVPROC >( "glUniform1dv", out _glUniform1dv );

        _glUniform1dv( location, count, value );
    }

    public void Uniform1dv( GLint location, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM1DVPROC >( "glUniform1dv", out _glUniform1dv );

        fixed ( GLdouble* p = &value[ 0 ] )
        {
            _glUniform1dv( location, value.Length, p );
        }
    }

    // ========================================================================

    public void Uniform2dv( GLint location, GLsizei count, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM2DVPROC >( "glUniform2dv", out _glUniform2dv );

        _glUniform2dv( location, count, value );
    }

    public void Uniform2dv( GLint location, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM2DVPROC >( "glUniform2dv", out _glUniform2dv );

        fixed ( GLdouble* p = &value[ 0 ] )
        {
            _glUniform2dv( location, value.Length / 2, p );
        }
    }

    // ========================================================================

    public void Uniform3dv( GLint location, GLsizei count, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM3DVPROC >( "glUniform3dv", out _glUniform3dv );

        _glUniform3dv( location, count, value );
    }

    public void Uniform3dv( GLint location, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM3DVPROC >( "glUniform3dv", out _glUniform3dv );

        fixed ( GLdouble* p = &value[ 0 ] )
        {
            _glUniform3dv( location, value.Length / 3, p );
        }
    }

    // ========================================================================

    public void Uniform4dv( GLint location, GLsizei count, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM4DVPROC >( "glUniform4dv", out _glUniform4dv );

        _glUniform4dv( location, count, value );
    }

    public void Uniform4dv( GLint location, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM4DVPROC >( "glUniform4dv", out _glUniform4dv );

        fixed ( GLdouble* p = &value[ 0 ] )
        {
            _glUniform4dv( location, value.Length / 4, p );
        }
    }

    // ========================================================================

    public void UniformMatrix2dv( GLint location, GLsizei count, GLbool transpose, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX2DVPROC >( "glUniformMatrix2dv", out _glUniformMatrix2dv );

        _glUniformMatrix2dv( location, count, transpose, value );
    }

    public void UniformMatrix2dv( GLint location, GLbool transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX2DVPROC >( "glUniformMatrix2dv", out _glUniformMatrix2dv );

        fixed ( GLdouble* p = &value[ 0 ] )
        {
            _glUniformMatrix2dv( location, value.Length / 4, transpose, p );
        }
    }

    // ========================================================================

    public void UniformMatrix3dv( GLint location, GLsizei count, GLbool transpose, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX3DVPROC >( "glUniformMatrix3dv", out _glUniformMatrix3dv );

        _glUniformMatrix3dv( location, count, transpose, value );
    }

    public void UniformMatrix3dv( GLint location, GLbool transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX3DVPROC >( "glUniformMatrix3dv", out _glUniformMatrix3dv );

        fixed ( GLdouble* p = &value[ 0 ] )
        {
            _glUniformMatrix3dv( location, value.Length / 9, transpose, p );
        }
    }

    // ========================================================================

    public void UniformMatrix4dv( GLint location, GLsizei count, GLbool transpose, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX4DVPROC >( "glUniformMatrix4dv", out _glUniformMatrix4dv );

        _glUniformMatrix4dv( location, count, transpose, value );
    }

    public void UniformMatrix4dv( GLint location, GLbool transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX4DVPROC >( "glUniformMatrix4dv", out _glUniformMatrix4dv );

        fixed ( GLdouble* p = &value[ 0 ] )
        {
            _glUniformMatrix4dv( location, value.Length / 16, transpose, p );
        }
    }

    // ========================================================================

    public void UniformMatrix2x3dv( GLint location, GLsizei count, GLbool transpose, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX2X3DVPROC >( "glUniformMatrix2x3dv", out _glUniformMatrix2x3dv );

        _glUniformMatrix2x3dv( location, count, transpose, value );
    }

    public void UniformMatrix2x3dv( GLint location, GLbool transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX2X3DVPROC >( "glUniformMatrix2x3dv", out _glUniformMatrix2x3dv );

        fixed ( GLdouble* p = &value[ 0 ] )
        {
            _glUniformMatrix2x3dv( location, value.Length / 6, transpose, p );
        }
    }

    // ========================================================================

    public void UniformMatrix2x4dv( GLint location, GLsizei count, GLbool transpose, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX2X4DVPROC >( "glUniformMatrix2x4dv", out _glUniformMatrix2x4dv );

        _glUniformMatrix2x4dv( location, count, transpose, value );
    }

    public void UniformMatrix2x4dv( GLint location, GLbool transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX2X4DVPROC >( "glUniformMatrix2x4dv", out _glUniformMatrix2x4dv );

        fixed ( GLdouble* p = &value[ 0 ] )
        {
            _glUniformMatrix2x4dv( location, value.Length / 8, transpose, p );
        }
    }

    // ========================================================================

    public void UniformMatrix3x2dv( GLint location, GLsizei count, GLbool transpose, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX3X2DVPROC >( "glUniformMatrix3x2dv", out _glUniformMatrix3x2dv );

        _glUniformMatrix3x2dv( location, count, transpose, value );
    }

    public void UniformMatrix3x2dv( GLint location, GLbool transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX3X2DVPROC >( "glUniformMatrix3x2dv", out _glUniformMatrix3x2dv );

        fixed ( GLdouble* p = &value[ 0 ] )
        {
            _glUniformMatrix3x2dv( location, value.Length / 6, transpose, p );
        }
    }

    // ========================================================================

    public void UniformMatrix3x4dv( GLint location, GLsizei count, GLbool transpose, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX3X4DVPROC >( "glUniformMatrix3x4dv", out _glUniformMatrix3x4dv );

        _glUniformMatrix3x4dv( location, count, transpose, value );
    }

    public void UniformMatrix3x4dv( GLint location, GLbool transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX3X4DVPROC >( "glUniformMatrix3x4dv", out _glUniformMatrix3x4dv );

        fixed ( GLdouble* p = &value[ 0 ] )
        {
            _glUniformMatrix3x4dv( location, value.Length / 12, transpose, p );
        }
    }

    // ========================================================================

    public void UniformMatrix4x2dv( GLint location, GLsizei count, GLbool transpose, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX4X2DVPROC >( "glUniformMatrix4x2dv", out _glUniformMatrix4x2dv );

        _glUniformMatrix4x2dv( location, count, transpose, value );
    }

    public void UniformMatrix4x2dv( GLint location, GLbool transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX4X2DVPROC >( "glUniformMatrix4x2dv", out _glUniformMatrix4x2dv );

        fixed ( GLdouble* p = &value[ 0 ] )
        {
            _glUniformMatrix4x2dv( location, value.Length / 8, transpose, p );
        }
    }

    // ========================================================================

    public void UniformMatrix4x3dv( GLint location, GLsizei count, GLbool transpose, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX4X3DVPROC >( "glUniformMatrix4x3dv", out _glUniformMatrix4x3dv );

        _glUniformMatrix4x3dv( location, count, transpose, value );
    }

    public void UniformMatrix4x3dv( GLint location, GLbool transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX4X3DVPROC >( "glUniformMatrix4x3dv", out _glUniformMatrix4x3dv );

        fixed ( GLdouble* p = &value[ 0 ] )
        {
            _glUniformMatrix4x3dv( location, value.Length / 12, transpose, p );
        }
    }

    // ========================================================================

    public void GetUniformdv( GLuint program, GLint location, GLdouble* parameters )
    {
        GetDelegateForFunction< PFNGLGETUNIFORMDVPROC >( "glGetUniformdv", out _glGetUniformdv );

        _glGetUniformdv( program, location, parameters );
    }

    public void GetUniformdv( GLuint program, GLint location, ref GLdouble[] parameters )
    {
        GetDelegateForFunction< PFNGLGETUNIFORMDVPROC >( "glGetUniformdv", out _glGetUniformdv );

        fixed ( GLdouble* p = &parameters[ 0 ] )
        {
            _glGetUniformdv( program, location, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetUniformIndices( GLuint program, GLsizei uniformCount, GLchar** uniformNames, GLuint* uniformIndices )
    {
        GetDelegateForFunction< PFNGLGETUNIFORMINDICESPROC >( "glGetUniformIndices", out _glGetUniformIndices );

        _glGetUniformIndices( program, uniformCount, uniformNames, uniformIndices );
    }

    /// <inheritdoc />
    public GLuint[] GetUniformIndices( GLuint program, params string[] uniformNames )
    {
        int uniformCount     = uniformNames.Length;
        var uniformNamesPtrs = new GLchar[ uniformCount ][];

        for ( var i = 0; i < uniformCount; i++ )
        {
            uniformNamesPtrs[ i ] = Encoding.UTF8.GetBytes( uniformNames[ i ] );
        }

        {
            GLchar** pUniformNames = stackalloc GLchar*[ uniformCount ];

            for ( var i = 0; i < uniformCount; i++ )
            {
                fixed ( GLchar* p = &uniformNamesPtrs[ i ][ 0 ] )
                {
                    pUniformNames[ i ] = p;
                }
            }

            var uniformIndices = new GLuint[ uniformCount ];

            GetDelegateForFunction< PFNGLGETUNIFORMINDICESPROC >( "glGetUniformIndices", out _glGetUniformIndices );

            fixed ( GLuint* p = &uniformIndices[ 0 ] )
            {
                _glGetUniformIndices( program, uniformCount, pUniformNames, p );
            }

            return uniformIndices;
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetActiveUniformsiv( GLuint program, GLsizei uniformCount, GLuint* uniformIndices, GLenum pname,
                                     GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETACTIVEUNIFORMSIVPROC >( "glGetActiveUniformsiv", out _glGetActiveUniformsiv );

        _glGetActiveUniformsiv( program, uniformCount, uniformIndices, pname, parameters );
    }

    /// <inheritdoc />
    public GLint[] GetActiveUniformsiv( GLuint program, GLenum pname, params GLuint[] uniformIndices )
    {
        int uniformCount = uniformIndices.Length;
        var parameters   = new GLint[ uniformCount ];

        GetDelegateForFunction< PFNGLGETACTIVEUNIFORMSIVPROC >( "glGetActiveUniformsiv", out _glGetActiveUniformsiv );

        {
            fixed ( GLuint* p = &uniformIndices[ 0 ] )
            {
                fixed ( GLint* pParameters = &parameters[ 0 ] )
                {
                    _glGetActiveUniformsiv( program, uniformCount, p, pname, pParameters );
                }
            }
        }

        return parameters;
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetActiveUniformName( GLuint program, GLuint uniformIndex, GLsizei bufSize, GLsizei* length,
                                      GLchar* uniformName )
    {
        GetDelegateForFunction< PFNGLGETACTIVEUNIFORMNAMEPROC >
            (
             "glGetActiveUniformName",
             out _glGetActiveUniformName
            );

        _glGetActiveUniformName( program, uniformIndex, bufSize, length, uniformName );
    }

    /// <inheritdoc />
    public string GetActiveUniformName( GLuint program, GLuint uniformIndex, GLsizei bufSize )
    {
        GLchar* uniformName = stackalloc GLchar[ bufSize ];
        GLsizei length;

        GetDelegateForFunction< PFNGLGETACTIVEUNIFORMNAMEPROC >
            (
             "glGetActiveUniformName",
             out _glGetActiveUniformName
            );

        _glGetActiveUniformName( program, uniformIndex, bufSize, &length, uniformName );

        return new string( ( GLbyte* )uniformName, 0, length, Encoding.UTF8 );
    }

    // ========================================================================

    /// <inheritdoc />
    public GLuint GetUniformBlockIndex( GLuint program, GLchar* uniformBlockName )
    {
        GetDelegateForFunction< PFNGLGETUNIFORMBLOCKINDEXPROC >
            (
             "glGetUniformBlockIndex",
             out _glGetUniformBlockIndex
            );

        return _glGetUniformBlockIndex( program, uniformBlockName );
    }

    /// <inheritdoc />
    public GLuint GetUniformBlockIndex( GLuint program, string uniformBlockName )
    {
        byte[] uniformBlockNameBytes = Encoding.UTF8.GetBytes( uniformBlockName );

        GetDelegateForFunction< PFNGLGETUNIFORMBLOCKINDEXPROC >
            (
             "glGetUniformBlockIndex",
             out _glGetUniformBlockIndex
            );

        {
            fixed ( GLchar* p = &uniformBlockNameBytes[ 0 ] )
            {
                return _glGetUniformBlockIndex( program, p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetActiveUniformBlockiv( GLuint program, GLuint uniformBlockIndex, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETACTIVEUNIFORMBLOCKIVPROC >
            (
             "glGetActiveUniformBlockiv",
             out _glGetActiveUniformBlockiv
            );

        _glGetActiveUniformBlockiv( program, uniformBlockIndex, pname, parameters );
    }

    /// <inheritdoc />
    public void GetActiveUniformBlockiv( GLuint program, GLuint uniformBlockIndex, GLenum pname,
                                         ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETACTIVEUNIFORMBLOCKIVPROC >
            (
             "glGetActiveUniformBlockiv",
             out _glGetActiveUniformBlockiv
            );

        {
            fixed ( GLint* p = &parameters[ 0 ] )
            {
                _glGetActiveUniformBlockiv( program, uniformBlockIndex, pname, p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetActiveUniformBlockName( GLuint program, GLuint uniformBlockIndex, GLsizei bufSize, GLsizei* length,
                                           GLchar* uniformBlockName )
    {
        GetDelegateForFunction< PFNGLGETACTIVEUNIFORMBLOCKNAMEPROC >
            (
             "glGetActiveUniformBlockName",
             out _glGetActiveUniformBlockName
            );

        _glGetActiveUniformBlockName( program, uniformBlockIndex, bufSize, length, uniformBlockName );
    }

    // ========================================================================

    /// <inheritdoc />
    public string GetActiveUniformBlockName( GLuint program, GLuint uniformBlockIndex, GLsizei bufSize )
    {
        GLchar* uniformBlockName = stackalloc GLchar[ bufSize ];

        GLsizei length;

        GetDelegateForFunction< PFNGLGETACTIVEUNIFORMBLOCKNAMEPROC >
            (
             "glGetActiveUniformBlockName",
             out _glGetActiveUniformBlockName
            );

        _glGetActiveUniformBlockName( program, uniformBlockIndex, bufSize, &length, uniformBlockName );

        return new string( ( GLbyte* )uniformBlockName, 0, length, Encoding.UTF8 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void UniformBlockBinding( GLuint program, GLuint uniformBlockIndex, GLuint uniformBlockBinding )
    {
        GetDelegateForFunction< PFNGLUNIFORMBLOCKBINDINGPROC >( "glUniformBlockBinding", out _glUniformBlockBinding );

        _glUniformBlockBinding( program, uniformBlockIndex, uniformBlockBinding );
    }

    // ========================================================================

    public void UniformSubroutinesuiv( GLenum shadertype, GLsizei count, GLuint* indices )
    {
        GetDelegateForFunction< PFNGLUNIFORMSUBROUTINESUIVPROC >
            (
             "glUniformSubroutinesuiv",
             out _glUniformSubroutinesuiv
            );

        _glUniformSubroutinesuiv( shadertype, count, indices );
    }

    public void UniformSubroutinesuiv( GLenum shadertype, GLuint[] indices )
    {
        GetDelegateForFunction< PFNGLUNIFORMSUBROUTINESUIVPROC >
            (
             "glUniformSubroutinesuiv",
             out _glUniformSubroutinesuiv
            );

        fixed ( GLuint* p = &indices[ 0 ] )
        {
            _glUniformSubroutinesuiv( shadertype, indices.Length, p );
        }
    }

    // ========================================================================

    public void GetUniformSubroutineuiv( GLenum shadertype, GLint location, GLuint* parameters )
    {
        GetDelegateForFunction< PFNGLGETUNIFORMSUBROUTINEUIVPROC >
            (
             "glGetUniformSubroutineuiv",
             out _glGetUniformSubroutineuiv
            );

        _glGetUniformSubroutineuiv( shadertype, location, parameters );
    }

    public void GetUniformSubroutineuiv( GLenum shadertype, GLint location, ref GLuint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETUNIFORMSUBROUTINEUIVPROC >
            (
             "glGetUniformSubroutineuiv",
             out _glGetUniformSubroutineuiv
            );

        fixed ( GLuint* p = &parameters[ 0 ] )
        {
            _glGetUniformSubroutineuiv( shadertype, location, p );
        }
    }

    #endregion Uniform Bindings

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region Vertex Array Bindings

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
    public GLbool IsVertexArray( GLuint array )
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
        GetDelegateForFunction< PFNGLDISABLEVERTEXARRAYATTRIBPROC >( "glDisableVertexArrayAttrib",
                                                                     out _glDisableVertexArrayAttrib );

        _glDisableVertexArrayAttrib( vaobj, index );
    }

    // ========================================================================

    public void EnableVertexArrayAttrib( GLuint vaobj, GLuint index )
    {
        GetDelegateForFunction< PFNGLENABLEVERTEXARRAYATTRIBPROC >( "glEnableVertexArrayAttrib",
                                                                    out _glEnableVertexArrayAttrib );

        _glEnableVertexArrayAttrib( vaobj, index );
    }

    // ========================================================================

    public void VertexArrayElementBuffer( GLuint vaobj, GLuint buffer )
    {
        GetDelegateForFunction< PFNGLVERTEXARRAYELEMENTBUFFERPROC >( "glVertexArrayElementBuffer",
                                                                     out _glVertexArrayElementBuffer );

        _glVertexArrayElementBuffer( vaobj, buffer );
    }

    // ========================================================================

    public void VertexArrayVertexBuffer( GLuint vaobj, GLuint bindingindex, GLuint buffer, GLintptr offset,
                                         GLsizei stride )
    {
        GetDelegateForFunction< PFNGLVERTEXARRAYVERTEXBUFFERPROC >( "glVertexArrayVertexBuffer",
                                                                    out _glVertexArrayVertexBuffer );

        _glVertexArrayVertexBuffer( vaobj, bindingindex, buffer, offset, stride );
    }

    public void VertexArrayVertexBuffers( GLuint vaobj, GLuint first, GLsizei count, GLuint* buffers, GLintptr* offsets,
                                          GLsizei* strides )
    {
        GetDelegateForFunction< PFNGLVERTEXARRAYVERTEXBUFFERPROC >( "glVertexArrayVertexBuffer",
                                                                    out _glVertexArrayVertexBuffer );

        if ( _glVertexArrayVertexBuffers != null )
        {
            _glVertexArrayVertexBuffers( vaobj, first, count, buffers, offsets, strides );
        }
    }

    // ========================================================================

    public void VertexArrayVertexBuffers( GLuint vaobj, GLuint first, GLuint[] buffers, GLintptr[] offsets,
                                          GLsizei[] strides )
    {
        GetDelegateForFunction< PFNGLVERTEXARRAYVERTEXBUFFERSPROC >( "glVertexArrayVertexBuffers",
                                                                     out _glVertexArrayVertexBuffers );

        fixed ( GLuint* ptrBuffers = &buffers[ 0 ] )
        {
            fixed ( GLintptr* ptrOffsets = &offsets[ 0 ] )
            {
                fixed ( GLsizei* ptrStrides = &strides[ 0 ] )
                {
                    _glVertexArrayVertexBuffers( vaobj,
                                                 first,
                                                 ( GLsizei )buffers.Length,
                                                 ptrBuffers,
                                                 ptrOffsets,
                                                 ptrStrides );
                }
            }
        }
    }

    // ========================================================================

    public void VertexArrayAttribBinding( GLuint vaobj, GLuint attribindex, GLuint bindingindex )
    {
        GetDelegateForFunction< PFNGLVERTEXARRAYATTRIBBINDINGPROC >( "glVertexArrayAttribBinding",
                                                                     out _glVertexArrayAttribBinding );

        _glVertexArrayAttribBinding( vaobj, attribindex, bindingindex );
    }

    // ========================================================================

    public void VertexArrayAttribFormat( GLuint vaobj, GLuint attribindex, GLint size, GLenum type,
                                         GLbool normalized,
                                         GLuint relativeoffset )
    {
        GetDelegateForFunction< PFNGLVERTEXARRAYATTRIBFORMATPROC >( "glVertexArrayAttribFormat",
                                                                    out _glVertexArrayAttribFormat );

        _glVertexArrayAttribFormat( vaobj, attribindex, size, type, normalized, relativeoffset );
    }

    // ========================================================================

    public void VertexArrayAttribIFormat( GLuint vaobj, GLuint attribindex, GLint size, GLenum type,
                                          GLuint relativeoffset )
    {
        GetDelegateForFunction< PFNGLVERTEXARRAYATTRIBIFORMATPROC >( "glVertexArrayAttribIFormat",
                                                                     out _glVertexArrayAttribIFormat );

        _glVertexArrayAttribIFormat( vaobj, attribindex, size, type, relativeoffset );
    }

    // ========================================================================

    public void VertexArrayAttribLFormat( GLuint vaobj, GLuint attribindex, GLint size, GLenum type,
                                          GLuint relativeoffset )
    {
        GetDelegateForFunction< PFNGLVERTEXARRAYATTRIBLFORMATPROC >( "glVertexArrayAttribLFormat",
                                                                     out _glVertexArrayAttribLFormat );

        _glVertexArrayAttribLFormat( vaobj, attribindex, size, type, relativeoffset );
    }

    // ========================================================================

    public void VertexArrayBindingDivisor( GLuint vaobj, GLuint bindingindex, GLuint divisor )
    {
        GetDelegateForFunction< PFNGLVERTEXARRAYBINDINGDIVISORPROC >( "glVertexArrayBindingDivisor",
                                                                      out _glVertexArrayBindingDivisor );

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
        GetDelegateForFunction< PFNGLGETVERTEXARRAYINDEXEDIVPROC >( "glGetVertexArrayIndexediv",
                                                                    out _glGetVertexArrayIndexediv );

        _glGetVertexArrayIndexediv( vaobj, index, pname, param );
    }

    public void GetVertexArrayIndexediv( GLuint vaobj, GLuint index, GLenum pname, ref GLint[] param )
    {
        GetDelegateForFunction< PFNGLGETVERTEXARRAYINDEXEDIVPROC >( "glGetVertexArrayIndexediv",
                                                                    out _glGetVertexArrayIndexediv );

        fixed ( GLint* ptrParam = &param[ 0 ] )
        {
            _glGetVertexArrayIndexediv( vaobj, index, pname, ptrParam );
        }
    }

    // ========================================================================

    public void GetVertexArrayIndexed64iv( GLuint vaobj, GLuint index, GLenum pname, GLint64* param )
    {
        GetDelegateForFunction< PFNGLGETVERTEXARRAYINDEXED64IVPROC >( "glGetVertexArrayIndexed64iv",
                                                                      out _glGetVertexArrayIndexed64iv );

        _glGetVertexArrayIndexed64iv( vaobj, index, pname, param );
    }

    public void GetVertexArrayIndexed64iv( GLuint vaobj, GLuint index, GLenum pname, ref GLint64[] param )
    {
        GetDelegateForFunction< PFNGLGETVERTEXARRAYINDEXED64IVPROC >( "glGetVertexArrayIndexed64iv",
                                                                      out _glGetVertexArrayIndexed64iv );

        fixed ( GLint64* ptrParam = &param[ 0 ] )
        {
            _glGetVertexArrayIndexed64iv( vaobj, index, pname, ptrParam );
        }
    }

    #endregion Vertex Array Bindings

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region Vertex Attrib Bindings

        /// <inheritdoc />
    public void VertexAttribDivisor( GLuint index, GLuint divisor )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBDIVISORPROC >( "glVertexAttribDivisor", out _glVertexAttribDivisor );

        _glVertexAttribDivisor( index, divisor );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttribP1UI( GLuint index, GLenum type, GLbool normalized, GLuint value )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBP1UIPROC >( "glVertexAttribP1ui", out _glVertexAttribP1ui );

        _glVertexAttribP1ui( index, type, normalized, value );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttribP1Uiv( GLuint index, GLenum type, GLbool normalized, GLuint* value )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBP1UIVPROC >( "glVertexAttribP1uiv", out _glVertexAttribP1uiv );

        _glVertexAttribP1uiv( index, type, normalized, value );
    }

    /// <inheritdoc />
    public void VertexAttribP1Uiv( GLuint index, GLenum type, GLbool normalized, GLuint[] value )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBP1UIVPROC >( "glVertexAttribP1uiv", out _glVertexAttribP1uiv );

        fixed ( GLuint* dp = &value[ 0 ] )
        {
            _glVertexAttribP1uiv( index, type, normalized, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttribP2UI( GLuint index, GLenum type, GLbool normalized, GLuint value )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBP2UIPROC >( "glVertexAttribP2ui", out _glVertexAttribP2ui );

        _glVertexAttribP2ui( index, type, normalized, value );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttribP2Uiv( GLuint index, GLenum type, GLbool normalized, GLuint* value )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBP2UIVPROC >( "glVertexAttribP2uiv", out _glVertexAttribP2uiv );

        _glVertexAttribP2uiv( index, type, normalized, value );
    }

    /// <inheritdoc />
    public void VertexAttribP2Uiv( GLuint index, GLenum type, GLbool normalized, GLuint[] value )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBP2UIVPROC >( "glVertexAttribP2uiv", out _glVertexAttribP2uiv );

        fixed ( GLuint* dp = &value[ 0 ] )
        {
            _glVertexAttribP2uiv( index, type, normalized, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttribP3UI( GLuint index, GLenum type, GLbool normalized, GLuint value )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBP3UIPROC >( "glVertexAttribP3ui", out _glVertexAttribP3ui );

        _glVertexAttribP3ui( index, type, normalized, value );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttribP3Uiv( GLuint index, GLenum type, GLbool normalized, GLuint* value )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBP3UIVPROC >( "glVertexAttribP3uiv", out _glVertexAttribP3uiv );

        _glVertexAttribP3uiv( index, type, normalized, value );
    }

    /// <inheritdoc />
    public void VertexAttribP3Uiv( GLuint index, GLenum type, GLbool normalized, GLuint[] value )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBP3UIVPROC >( "glVertexAttribP3uiv", out _glVertexAttribP3uiv );

        fixed ( GLuint* dp = &value[ 0 ] )
        {
            _glVertexAttribP3uiv( index, type, normalized, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttribP4UI( GLuint index, GLenum type, GLbool normalized, GLuint value )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBP4UIPROC >( "glVertexAttribP4ui", out _glVertexAttribP4ui );

        _glVertexAttribP4ui( index, type, normalized, value );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttribP4Uiv( GLuint index, GLenum type, GLbool normalized, GLuint* value )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBP4UIVPROC >( "glVertexAttribP4uiv", out _glVertexAttribP4uiv );

        _glVertexAttribP4uiv( index, type, normalized, value );
    }

    /// <inheritdoc />
    public void VertexAttribP4Uiv( GLuint index, GLenum type, GLbool normalized, GLuint[] value )
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
        GetDelegateForFunction< PFNGLVERTEXATTRIBLPOINTERPROC >( "glVertexAttribLPointer",
                                                                 out _glVertexAttribLPointer );

        _glVertexAttribLPointer( index, size, type, stride, pointer );
    }

    public void VertexAttribLPointer( GLuint index, GLint size, GLenum type, GLsizei stride, GLsizei pointer )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBLPOINTERPROC >( "glVertexAttribLPointer",
                                                                 out _glVertexAttribLPointer );

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

    public void VertexAttribFormat( GLuint attribindex, GLint size, GLenum type, GLbool normalized,
                                    GLuint relativeoffset )
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
        GetDelegateForFunction< PFNGLVERTEXBINDINGDIVISORPROC >( "glVertexBindingDivisor",
                                                                 out _glVertexBindingDivisor );

        _glVertexBindingDivisor( bindingindex, divisor );
    }

    // ========================================================================

    public void VertexAttribIPointer( GLuint index, GLint size, GLenum type, GLsizei stride, IntPtr pointer )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBIPOINTERPROC >( "glVertexAttribIPointer",
                                                                 out _glVertexAttribIPointer );

        _glVertexAttribIPointer( index, size, type, stride, pointer );
    }

    // ========================================================================

    public void VertexAttribIPointer( GLuint index, GLint size, GLenum type, GLsizei stride, uint pointer )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBIPOINTERPROC >( "glVertexAttribIPointer",
                                                                 out _glVertexAttribIPointer );

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

    public void VertexAttribI1I( GLuint index, GLint x )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI1IPROC >( "glVertexAttribI1i", out _glVertexAttribI1i );

        _glVertexAttribI1i( index, x );
    }

    // ========================================================================

    public void VertexAttribI2I( GLuint index, GLint x, GLint y )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI2IPROC >( "glVertexAttribI2i", out _glVertexAttribI2i );

        _glVertexAttribI2i( index, x, y );
    }

    // ========================================================================

    public void VertexAttribI3I( GLuint index, GLint x, GLint y, GLint z )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI3IPROC >( "glVertexAttribI3i", out _glVertexAttribI3i );

        _glVertexAttribI3i( index, x, y, z );
    }

    // ========================================================================

    public void VertexAttribI4I( GLuint index, GLint x, GLint y, GLint z, GLint w )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4IPROC >( "glVertexAttribI4i", out _glVertexAttribI4i );

        _glVertexAttribI4i( index, x, y, z, w );
    }

    // ========================================================================

    public void VertexAttribI1UI( GLuint index, GLuint x )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI1UIPROC >( "glVertexAttribI1ui", out _glVertexAttribI1ui );

        _glVertexAttribI1ui( index, x );
    }

    // ========================================================================

    public void VertexAttribI2UI( GLuint index, GLuint x, GLuint y )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI2UIPROC >( "glVertexAttribI2ui", out _glVertexAttribI2ui );

        _glVertexAttribI2ui( index, x, y );
    }

    // ========================================================================

    public void VertexAttribI3UI( GLuint index, GLuint x, GLuint y, GLuint z )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI3UIPROC >( "glVertexAttribI3ui", out _glVertexAttribI3ui );

        _glVertexAttribI3ui( index, x, y, z );
    }

    // ========================================================================

    public void VertexAttribI4UI( GLuint index, GLuint x, GLuint y, GLuint z, GLuint w )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4UIPROC >( "glVertexAttribI4ui", out _glVertexAttribI4ui );

        _glVertexAttribI4ui( index, x, y, z, w );
    }

    // ========================================================================

    public void VertexAttribI1Iv( GLuint index, GLint* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI1IVPROC >( "glVertexAttribI1iv", out _glVertexAttribI1iv );

        _glVertexAttribI1iv( index, v );
    }

    public void VertexAttribI1Iv( GLuint index, GLint[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI1IVPROC >( "glVertexAttribI1iv", out _glVertexAttribI1iv );

        fixed ( GLint* p = &v[ 0 ] )
        {
            _glVertexAttribI1iv( index, p );
        }
    }

    // ========================================================================

    public void VertexAttribI2Iv( GLuint index, GLint* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI2IVPROC >( "glVertexAttribI2iv", out _glVertexAttribI2iv );

        _glVertexAttribI2iv( index, v );
    }

    public void VertexAttribI2Iv( GLuint index, GLint[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI2IVPROC >( "glVertexAttribI2iv", out _glVertexAttribI2iv );

        fixed ( GLint* p = &v[ 0 ] )
        {
            _glVertexAttribI2iv( index, p );
        }
    }

    // ========================================================================

    public void VertexAttribI3Iv( GLuint index, GLint* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI3IVPROC >( "glVertexAttribI3iv", out _glVertexAttribI3iv );

        _glVertexAttribI3iv( index, v );
    }

    public void VertexAttribI3Iv( GLuint index, GLint[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI3IVPROC >( "glVertexAttribI3iv", out _glVertexAttribI3iv );

        fixed ( GLint* p = &v[ 0 ] )
        {
            _glVertexAttribI3iv( index, p );
        }
    }

    // ========================================================================

    public void VertexAttribI4Iv( GLuint index, GLint* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4IVPROC >( "glVertexAttribI4iv", out _glVertexAttribI4iv );

        _glVertexAttribI4iv( index, v );
    }

    public void VertexAttribI4Iv( GLuint index, GLint[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4IVPROC >( "glVertexAttribI4iv", out _glVertexAttribI4iv );

        fixed ( GLint* p = &v[ 0 ] )
        {
            _glVertexAttribI4iv( index, p );
        }
    }

    // ========================================================================

    public void VertexAttribI1Uiv( GLuint index, GLuint* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI1UIVPROC >( "glVertexAttribI1uiv", out _glVertexAttribI1uiv );

        _glVertexAttribI1uiv( index, v );
    }

    public void VertexAttribI1Uiv( GLuint index, GLuint[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI1UIVPROC >( "glVertexAttribI1uiv", out _glVertexAttribI1uiv );

        fixed ( GLuint* p = &v[ 0 ] )
        {
            _glVertexAttribI1uiv( index, p );
        }
    }

    // ========================================================================

    public void VertexAttribI2Uiv( GLuint index, GLuint* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI2UIVPROC >( "glVertexAttribI2uiv", out _glVertexAttribI2uiv );

        _glVertexAttribI2uiv( index, v );
    }

    public void VertexAttribI2Uiv( GLuint index, GLuint[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI2UIVPROC >( "glVertexAttribI2uiv", out _glVertexAttribI2uiv );

        fixed ( GLuint* p = &v[ 0 ] )
        {
            _glVertexAttribI2uiv( index, p );
        }
    }

    // ========================================================================

    public void VertexAttribI3Uiv( GLuint index, GLuint* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI3UIVPROC >( "glVertexAttribI3uiv", out _glVertexAttribI3uiv );

        _glVertexAttribI3uiv( index, v );
    }

    public void VertexAttribI3Uiv( GLuint index, GLuint[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI3UIVPROC >( "glVertexAttribI3uiv", out _glVertexAttribI3uiv );

        fixed ( GLuint* p = &v[ 0 ] )
        {
            _glVertexAttribI3uiv( index, p );
        }
    }

    // ========================================================================

    public void VertexAttribI4Uiv( GLuint index, GLuint* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4UIVPROC >( "glVertexAttribI4uiv", out _glVertexAttribI4uiv );

        _glVertexAttribI4uiv( index, v );
    }

    public void VertexAttribI4Uiv( GLuint index, GLuint[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4UIVPROC >( "glVertexAttribI4uiv", out _glVertexAttribI4uiv );

        fixed ( GLuint* p = &v[ 0 ] )
        {
            _glVertexAttribI4uiv( index, p );
        }
    }

    // ========================================================================

    public void VertexAttribI4Bv( GLuint index, GLbyte* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4BVPROC >( "glVertexAttribI4bv", out _glVertexAttribI4bv );

        _glVertexAttribI4bv( index, v );
    }

    public void VertexAttribI4Bv( GLuint index, GLbyte[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4BVPROC >( "glVertexAttribI4bv", out _glVertexAttribI4bv );

        fixed ( GLbyte* p = &v[ 0 ] )
        {
            _glVertexAttribI4bv( index, p );
        }
    }

    // ========================================================================

    public void VertexAttribI4Sv( GLuint index, GLshort* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4SVPROC >( "glVertexAttribI4sv", out _glVertexAttribI4sv );

        _glVertexAttribI4sv( index, v );
    }

    public void VertexAttribI4Sv( GLuint index, GLshort[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4SVPROC >( "glVertexAttribI4sv", out _glVertexAttribI4sv );

        fixed ( GLshort* p = &v[ 0 ] )
        {
            _glVertexAttribI4sv( index, p );
        }
    }

    // ========================================================================

    public void VertexAttribI4Ubv( GLuint index, GLubyte* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4UBVPROC >( "glVertexAttribI4ubv", out _glVertexAttribI4ubv );

        _glVertexAttribI4ubv( index, v );
    }

    public void VertexAttribI4Ubv( GLuint index, GLubyte[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4UBVPROC >( "glVertexAttribI4ubv", out _glVertexAttribI4ubv );

        fixed ( GLubyte* p = &v[ 0 ] )
        {
            _glVertexAttribI4ubv( index, p );
        }
    }

    // ========================================================================

    public void VertexAttribI4Usv( GLuint index, GLushort* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4USVPROC >( "glVertexAttribI4usv", out _glVertexAttribI4usv );

        _glVertexAttribI4usv( index, v );
    }

    public void VertexAttribI4Usv( GLuint index, GLushort[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBI4USVPROC >( "glVertexAttribI4usv", out _glVertexAttribI4usv );

        fixed ( GLushort* p = &v[ 0 ] )
        {
            _glVertexAttribI4usv( index, p );
        }
    }

    #endregion Vertex Attrib Bindings
}

// ============================================================================
// ============================================================================

