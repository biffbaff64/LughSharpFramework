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

using System.Text;

using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;

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
    public const int INVALID_SHADER_PROGRAM = -1;
    public const int INVALID_SHADER         = -1;

    // ========================================================================

    /// <summary>
    /// The null pointer, just like in C/C++.
    /// </summary>
    public readonly IntPtr NULL = ( IntPtr )0;

    // ========================================================================
    // ========================================================================

    /// <inheritdoc />
    public void ColorMaski( GLuint index, GLboolean r, GLboolean g, GLboolean b, GLboolean a )
    {
        GetDelegateForFunction< PFNGLCOLORMASKIPROC >( "glColorMaski", out _glColorMaski );

        _glColorMaski( index, r, g, b, a );
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetBooleani_v( GLenum target, GLuint index, GLboolean* data )
    {
        GetDelegateForFunction< PFNGLGETBOOLEANI_VPROC >( "glGetBooleani_v", out _glGetBooleani_v );

        _glGetBooleani_v( target, index, data );
    }

    /// <inheritdoc />
    public void GetBooleani_v( GLenum target, GLuint index, ref GLboolean[] data )
    {
        GetDelegateForFunction< PFNGLGETBOOLEANI_VPROC >( "glGetBooleani_v", out _glGetBooleani_v );

        fixed ( GLboolean* p = &data[ 0 ] )
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
    public (int major, int minor) GetOpenGLVersion()
    {
        var version = GetString( IGL.GL_VERSION );

        if ( version == null )
        {
            throw new GdxRuntimeException( "NULL GL Version returned!" );
        }

        return ( version[ 0 ], version[ 2 ] );
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Handles OpenGL debug messages or errors by logging a callback. This can be used to retrieve
    /// diagnostic messages from OpenGL during runtime.
    /// </summary>
    /// <param name="source">
    /// Specifies the source of the debug callback, identifying which part of the OpenGL system the
    /// message originates from.
    /// </param>
    /// <param name="type">Specifies the type of debug message, such as error, performance warning, or other messages.</param>
    /// <param name="id">Specifies the unique ID for the message to help identify specific issues.</param>
    /// <param name="severity">Specifies the severity level of the message (e.g., notification, warning, or error).</param>
    /// <param name="length">Specifies the length of the message string in bytes.</param>
    /// <param name="message">A pointer to the actual message string provided by OpenGL.</param>
    /// <param name="userParam">A pointer to optional user-defined data passed to the callback.</param>
    public void MessageCallback( int source,
                                 int type,
                                 uint id,
                                 int severity,
                                 int length,
                                 byte* message,
                                 IntPtr userParam )
    {
        Logger.Warning( $"GL CALLBACK: " +
                        $"{( type == IGL.GL_DEBUG_TYPE_ERROR ? "** GL ERROR **" : "" )} " +
                        $"source = {source:X}, " +
                        $"type = {type:X}, " +
                        $"severity = {severity:X}, " +
                        $"message = {BytePointerToString.Convert( message )}\n" );
    }

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
    public void Disable( GLenum cap )
    {
        GetDelegateForFunction< PFNGLDISABLEPROC >( "glDisable", out _glDisable );

        _glDisable( cap );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Enable( GLenum cap )
    {
        GetDelegateForFunction< PFNGLENABLEPROC >( "glEnable", out _glEnable );

        _glEnable( cap );
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
    public GLboolean IsEnabled( GLenum cap )
    {
        GetDelegateForFunction< PFNGLISENABLEDPROC >( "glIsEnabled", out _glIsEnabled );

        return _glIsEnabled( cap );
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
    public GLboolean IsEnabledi( GLenum target, GLuint index )
    {
        GetDelegateForFunction< PFNGLISENABLEDIPROC >( "glIsEnabledi", out _glIsEnabledi );

        return _glIsEnabledi( target, index );
    }

    // ========================================================================

    /// <inheritdoc />
    public void CullFace( GLenum mode )
    {
        GetDelegateForFunction< PFNGLCULLFACEPROC >( "glCullFace", out _glCullFace );

        _glCullFace( mode );
    }

    // ========================================================================

    /// <inheritdoc />
    public void FrontFace( GLenum mode )
    {
        GetDelegateForFunction< PFNGLFRONTFACEPROC >( "glFrontFace", out _glFrontFace );

        _glFrontFace( mode );
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
    public void DrawBuffer( GLenum buf )
    {
        GetDelegateForFunction< PFNGLDRAWBUFFERPROC >( "glDrawBuffer", out _glDrawBuffer );

        _glDrawBuffer( buf );
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
    public void ColorMask( GLboolean red, GLboolean green, GLboolean blue, GLboolean alpha )
    {
        GetDelegateForFunction< PFNGLCOLORMASKPROC >( "glColorMask", out _glColorMask );

        _glColorMask( red, green, blue, alpha );
    }

    // ========================================================================

    /// <inheritdoc />
    public void DepthMask( GLboolean flag )
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
    public void ReadBuffer( GLenum src )
    {
        GetDelegateForFunction< PFNGLREADBUFFERPROC >( "glReadBuffer", out _glReadBuffer );

        _glReadBuffer( src );
    }

    // ========================================================================

    /// <inheritdoc />
    public void ReadPixels( GLint x, GLint y, GLsizei width, GLsizei height, GLenum format, GLenum type, IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLREADPIXELSPROC >( "glReadPixels", out _glReadPixels );

        _glReadPixels( x, y, width, height, format, type, pixels );
    }

    /// <inheritdoc />
    public void ReadPixels< T >( GLint x, GLint y, GLsizei width, GLsizei height, GLenum format, GLenum type, ref T[] pixels )
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
    public void GetBooleanv( GLenum pname, GLboolean* data )
    {
        GetDelegateForFunction< PFNGLGETBOOLEANVPROC >( "glGetBooleanv", out _glGetBooleanv );

        _glGetBooleanv( pname, data );
    }

    /// <inheritdoc />
    public void GetBooleanv( GLenum pname, ref GLboolean[] data )
    {
        GetDelegateForFunction< PFNGLGETBOOLEANVPROC >( "glGetBooleanv", out _glGetBooleanv );

        fixed ( GLboolean* p = &data[ 0 ] )
        {
            _glGetBooleanv( pname, p );
        }
    }

    /// <inheritdoc />
    public void GetBooleanv( GLenum pname, out bool data )
    {
        // Create a temporary array to hold the result from the underlying OpenGL method.
        var tempArray = new bool[ 1 ]; 
    
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

        var p = _glGetString( name );

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
    public void DrawElements< T >( GLenum mode, GLsizei count, GLenum type, T[] indices ) where T : unmanaged, IUnsignedNumber< T >
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
    public void BlendFuncSeparate( GLenum sfactorRGB, GLenum dfactorRGB, GLenum sfactorAlpha, GLenum dfactorAlpha )
    {
        GetDelegateForFunction< PFNGLBLENDFUNCSEPARATEPROC >( "glBlendFuncSeparate", out _glBlendFuncSeparate );

        _glBlendFuncSeparate( sfactorRGB, dfactorRGB, sfactorAlpha, dfactorAlpha );
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
    public void MultiDrawElements< T >( GLenum mode, GLsizei[] count, GLenum type, T[][] indices ) where T : unmanaged, IUnsignedNumber< T >
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
        GetDelegateForFunction< PFNGLBLENDEQUATIONSEPARATEPROC >( "glBlendEquationSeparate", out _glBlendEquationSeparate );

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
    public void BeginTransformFeedback( GLenum primitiveMode )
    {
        GetDelegateForFunction< PFNGLBEGINTRANSFORMFEEDBACKPROC >( "glBeginTransformFeedback", out _glBeginTransformFeedback );

        _glBeginTransformFeedback( primitiveMode );
    }

    // ========================================================================

    /// <inheritdoc />
    public void EndTransformFeedback()
    {
        GetDelegateForFunction< PFNGLENDTRANSFORMFEEDBACKPROC >( "glEndTransformFeedback", out _glEndTransformFeedback );

        _glEndTransformFeedback();
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
    public void GetInteger64v( GLenum pname, GLint64* data )
    {
        GetDelegateForFunction< PFNGLGETINTEGER64VPROC >( "glGetInteger64v", out _glGetInteger64v );

        _glGetInteger64v( pname, data );
    }

    /// <inheritdoc />
    public void GetInteger64v( GLenum pname, ref GLint64[] data )
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
        GetDelegateForFunction< PFNGLBINDFRAGDATALOCATIONINDEXEDPROC >( "glBindFragDataLocationIndexed",
                                                                        out _glBindFragDataLocationIndexed );

        _glBindFragDataLocationIndexed( ( uint )program, colorNumber, index, name );
    }

    /// <inheritdoc />
    public void BindFragDataLocationIndexed( GLuint program, GLuint colorNumber, GLuint index, string name )
    {
        var nameBytes = Encoding.UTF8.GetBytes( name );

        GetDelegateForFunction< PFNGLBINDFRAGDATALOCATIONINDEXEDPROC >( "glBindFragDataLocationIndexed",
                                                                        out _glBindFragDataLocationIndexed );

        {
            fixed ( GLchar* p = &nameBytes[ 0 ] )
            {
                _glBindFragDataLocationIndexed( ( uint )program, colorNumber, index, p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public GLint GetFragDataIndex( GLuint program, GLchar* name )
    {
        GetDelegateForFunction< PFNGLGETFRAGDATAINDEXPROC >( "glGetFragDataIndex", out _glGetFragDataIndex );

        return _glGetFragDataIndex( ( uint )program, name );
    }

    /// <inheritdoc />
    public GLint GetFragDataIndex( GLuint program, string name )
    {
        var nameBytes = Encoding.UTF8.GetBytes( name );

        GetDelegateForFunction< PFNGLGETFRAGDATAINDEXPROC >( "glGetFragDataIndex", out _glGetFragDataIndex );

        fixed ( GLchar* p = &nameBytes[ 0 ] )
        {
            return _glGetFragDataIndex( ( uint )program, p );
        }
    }

    // ========================================================================

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

    public void BlendEquationi( GLuint buf, GLenum mode )
    {
        GetDelegateForFunction< PFNGLBLENDEQUATIONIPROC >( "glBlendEquationi", out _glBlendEquationi );

        _glBlendEquationi( buf, mode );
    }

    // ========================================================================

    public void BlendEquationSeparatei( GLuint buf, GLenum modeRGB, GLenum modeAlpha )
    {
        GetDelegateForFunction< PFNGLBLENDEQUATIONSEPARATEIPROC >( "glBlendEquationSeparatei", out _glBlendEquationSeparatei );

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
        GetDelegateForFunction< PFNGLDRAWELEMENTSINDIRECTPROC >( "glDrawElementsIndirect", out _glDrawElementsIndirect );

        _glDrawElementsIndirect( mode, type, indirect );
    }

    // ========================================================================

    public GLint GetSubroutineUniformLocation( GLuint program, GLenum shadertype, GLchar* name )
    {
        GetDelegateForFunction< PFNGLGETSUBROUTINEUNIFORMLOCATIONPROC >( "glGetSubroutineUniformLocation",
                                                                         out _glGetSubroutineUniformLocation );

        return _glGetSubroutineUniformLocation( ( uint )program, shadertype, name );
    }

    public GLint GetSubroutineUniformLocation( GLuint program, GLenum shadertype, string name )
    {
        var nameBytes = Encoding.UTF8.GetBytes( name );

        GetDelegateForFunction< PFNGLGETSUBROUTINEUNIFORMLOCATIONPROC >( "glGetSubroutineUniformLocation",
                                                                         out _glGetSubroutineUniformLocation );

        fixed ( GLchar* p = &nameBytes[ 0 ] )
        {
            return _glGetSubroutineUniformLocation( ( uint )program, shadertype, p );
        }
    }

    // ========================================================================

    public GLuint GetSubroutineIndex( GLuint program, GLenum shadertype, GLchar* name )
    {
        GetDelegateForFunction< PFNGLGETSUBROUTINEINDEXPROC >( "glGetSubroutineIndex", out _glGetSubroutineIndex );

        return _glGetSubroutineIndex( ( uint )program, shadertype, name );
    }

    public GLuint GetSubroutineIndex( GLuint program, GLenum shadertype, string name )
    {
        var nameBytes = Encoding.UTF8.GetBytes( name );

        GetDelegateForFunction< PFNGLGETSUBROUTINEINDEXPROC >( "glGetSubroutineIndex", out _glGetSubroutineIndex );

        fixed ( GLchar* p = &nameBytes[ 0 ] )
        {
            return _glGetSubroutineIndex( ( uint )program, shadertype, p );
        }
    }

    // ========================================================================

    public void GetActiveSubroutineUniformiv( GLuint program, GLenum shadertype, GLuint index, GLenum pname, GLint* values )
    {
        GetDelegateForFunction< PFNGLGETACTIVESUBROUTINEUNIFORMIVPROC >( "glGetActiveSubroutineUniformiv",
                                                                         out _glGetActiveSubroutineUniformiv );

        _glGetActiveSubroutineUniformiv( ( uint )program, shadertype, index, pname, values );
    }

    public void GetActiveSubroutineUniformiv( GLuint program, GLenum shadertype, GLuint index, GLenum pname, ref GLint[] values )
    {
        GetDelegateForFunction< PFNGLGETACTIVESUBROUTINEUNIFORMIVPROC >( "glGetActiveSubroutineUniformiv",
                                                                         out _glGetActiveSubroutineUniformiv );

        fixed ( GLint* p = &values[ 0 ] )
        {
            _glGetActiveSubroutineUniformiv( ( uint )program, shadertype, index, pname, p );
        }
    }

    // ========================================================================

    public void GetActiveSubroutineUniformName( GLuint program, GLenum shadertype, GLuint index, GLsizei bufsize, GLsizei* length,
                                                GLchar* name )
    {
        GetDelegateForFunction< PFNGLGETACTIVESUBROUTINEUNIFORMNAMEPROC >( "glGetActiveSubroutineUniformName",
                                                                           out _glGetActiveSubroutineUniformName );

        _glGetActiveSubroutineUniformName( ( uint )program, shadertype, index, bufsize, length, name );
    }

    public string GetActiveSubroutineUniformName( GLuint program, GLenum shadertype, GLuint index, GLsizei bufsize )
    {
        var name = new GLchar[ bufsize ];

        GetDelegateForFunction< PFNGLGETACTIVESUBROUTINEUNIFORMNAMEPROC >( "glGetActiveSubroutineUniformName",
                                                                           out _glGetActiveSubroutineUniformName );

        fixed ( GLchar* p = &name[ 0 ] )
        {
            GLsizei length;

            _glGetActiveSubroutineUniformName( ( uint )program, shadertype, index, bufsize, &length, p );

            return new string( ( GLbyte* )p, 0, length, Encoding.UTF8 );
        }
    }

    // ========================================================================

    public void GetActiveSubroutineName( GLuint program, GLenum shadertype, GLuint index, GLsizei bufsize, GLsizei* length, GLchar* name )
    {
        GetDelegateForFunction< PFNGLGETACTIVESUBROUTINENAMEPROC >( "glGetActiveSubroutineName", out _glGetActiveSubroutineName );

        _glGetActiveSubroutineName( ( uint )program, shadertype, index, bufsize, length, name );
    }

    public string GetActiveSubroutineName( GLuint program, GLenum shadertype, GLuint index, GLsizei bufsize )
    {
        var name = new GLchar[ bufsize ];

        GetDelegateForFunction< PFNGLGETACTIVESUBROUTINENAMEPROC >( "glGetActiveSubroutineName", out _glGetActiveSubroutineName );

        fixed ( GLchar* p = &name[ 0 ] )
        {
            GLsizei length;

            _glGetActiveSubroutineName( ( uint )program, shadertype, index, bufsize, &length, p );

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

    public void BindTransformFeedback( GLenum target, GLuint id )
    {
        GetDelegateForFunction< PFNGLBINDTRANSFORMFEEDBACKPROC >( "glBindTransformFeedback", out _glBindTransformFeedback );

        _glBindTransformFeedback( target, id );
    }

    // ========================================================================

    public void DeleteTransformFeedbacks( GLsizei n, GLuint* ids )
    {
        GetDelegateForFunction< PFNGLDELETETRANSFORMFEEDBACKSPROC >( "glDeleteTransformFeedbacks", out _glDeleteTransformFeedbacks );

        _glDeleteTransformFeedbacks( n, ids );
    }

    public void DeleteTransformFeedbacks( params GLuint[] ids )
    {
        GetDelegateForFunction< PFNGLDELETETRANSFORMFEEDBACKSPROC >( "glDeleteTransformFeedbacks", out _glDeleteTransformFeedbacks );

        fixed ( GLuint* p = &ids[ 0 ] )
        {
            _glDeleteTransformFeedbacks( ids.Length, p );
        }
    }

    // ========================================================================

    public void GenTransformFeedbacks( GLsizei n, GLuint* ids )
    {
        GetDelegateForFunction< PFNGLGENTRANSFORMFEEDBACKSPROC >( "glGenTransformFeedbacks", out _glGenTransformFeedbacks );

        _glGenTransformFeedbacks( n, ids );
    }

    public GLuint[] GenTransformFeedbacks( GLsizei n )
    {
        var r = new GLuint[ n ];

        GetDelegateForFunction< PFNGLGENTRANSFORMFEEDBACKSPROC >( "glGenTransformFeedbacks", out _glGenTransformFeedbacks );

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

    public GLboolean IsTransformFeedback( GLuint id )
    {
        GetDelegateForFunction< PFNGLISTRANSFORMFEEDBACKPROC >( "glIsTransformFeedback", out _glIsTransformFeedback );

        return _glIsTransformFeedback( id );
    }

    // ========================================================================

    public void PauseTransformFeedback()
    {
        GetDelegateForFunction< PFNGLPAUSETRANSFORMFEEDBACKPROC >( "glPauseTransformFeedback", out _glPauseTransformFeedback );

        _glPauseTransformFeedback();
    }

    // ========================================================================

    public void ResumeTransformFeedback()
    {
        GetDelegateForFunction< PFNGLRESUMETRANSFORMFEEDBACKPROC >( "glResumeTransformFeedback", out _glResumeTransformFeedback );

        _glResumeTransformFeedback();
    }

    // ========================================================================

    public void DrawTransformFeedback( GLenum mode, GLuint id )
    {
        GetDelegateForFunction< PFNGLDRAWTRANSFORMFEEDBACKPROC >( "glDrawTransformFeedback", out _glDrawTransformFeedback );

        _glDrawTransformFeedback( mode, id );
    }

    // ========================================================================

    public void DrawTransformFeedbackStream( GLenum mode, GLuint id, GLuint stream )
    {
        GetDelegateForFunction< PFNGLDRAWTRANSFORMFEEDBACKSTREAMPROC >( "glDrawTransformFeedbackStream",
                                                                        out _glDrawTransformFeedbackStream );

        _glDrawTransformFeedbackStream( mode, id, stream );
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
        GetDelegateForFunction< PFNGLRELEASESHADERCOMPILERPROC >( "glReleaseShaderCompiler", out _glReleaseShaderCompiler );

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
        var count = shaders.Length;

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
        GetDelegateForFunction< PFNGLGETSHADERPRECISIONFORMATPROC >( "glGetShaderPrecisionFormat", out _glGetShaderPrecisionFormat );

        _glGetShaderPrecisionFormat( shaderType, precisionType, range, precision );
    }

    public void GetShaderPrecisionFormat( GLenum shaderType, GLenum precisionType, ref GLint[] range, ref GLint precision )
    {
        range = new GLint[ 2 ];

        GetDelegateForFunction< PFNGLGETSHADERPRECISIONFORMATPROC >( "glGetShaderPrecisionFormat", out _glGetShaderPrecisionFormat );

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

    public void DrawArraysInstancedBaseInstance( GLenum mode, GLint first, GLsizei count, GLsizei instancecount, GLuint baseinstance )
    {
        GetDelegateForFunction< PFNGLDRAWARRAYSINSTANCEDBASEINSTANCEPROC >( "glDrawArraysInstancedBaseInstance",
                                                                            out _glDrawArraysInstancedBaseInstance );

        _glDrawArraysInstancedBaseInstance( mode, first, count, instancecount, baseinstance );
    }

    // ========================================================================

    public void DrawElementsInstancedBaseInstance( GLenum mode, GLsizei count, GLenum type, IntPtr indices, GLsizei instancecount,
                                                   GLuint baseinstance )
    {
        GetDelegateForFunction< PFNGLDRAWELEMENTSINSTANCEDBASEINSTANCEPROC >( "glDrawElementsInstancedBaseInstance",
                                                                              out _glDrawElementsInstancedBaseInstance );

        _glDrawElementsInstancedBaseInstance( mode, count, type, indices, instancecount, baseinstance );
    }

    public void DrawElementsInstancedBaseInstance< T >( GLenum mode, GLsizei count, GLenum type, T[] indices, GLsizei instancecount,
                                                        GLuint baseinstance ) where T : unmanaged, IUnsignedNumber< T >
    {
        GetDelegateForFunction< PFNGLDRAWELEMENTSINSTANCEDBASEINSTANCEPROC >( "glDrawElementsInstancedBaseInstance",
                                                                              out _glDrawElementsInstancedBaseInstance );

        fixed ( void* p = &indices[ 0 ] )
        {
            _glDrawElementsInstancedBaseInstance( mode, count, type, ( IntPtr )p, instancecount, baseinstance );
        }
    }

    // ========================================================================

    public void DrawElementsInstancedBaseVertexBaseInstance( GLenum mode,
                                                             GLsizei count, GLenum type, IntPtr indices,
                                                             GLsizei instancecount, GLint basevertex, GLuint baseinstance )
    {
        GetDelegateForFunction< PFNGLDRAWELEMENTSINSTANCEDBASEVERTEXBASEINSTANCEPROC >( "glDrawElementsInstancedBaseVertexBaseInstance",
                                                                                        out _glDrawElementsInstancedBaseVertexBaseInstance );

        _glDrawElementsInstancedBaseVertexBaseInstance( mode, count, type, indices, instancecount, basevertex, baseinstance );
    }

    public void DrawElementsInstancedBaseVertexBaseInstance< T >( GLenum mode, GLsizei count, GLenum type, T[] indices,
                                                                  GLsizei instancecount, GLint basevertex, GLuint baseinstance )
        where T : unmanaged, IUnsignedNumber< T >
    {
        GetDelegateForFunction< PFNGLDRAWELEMENTSINSTANCEDBASEVERTEXBASEINSTANCEPROC >( "glDrawElementsInstancedBaseVertexBaseInstance",
                                                                                        out _glDrawElementsInstancedBaseVertexBaseInstance );

        fixed ( void* p = &indices[ 0 ] )
        {
            _glDrawElementsInstancedBaseVertexBaseInstance( mode, count, type, ( IntPtr )p, instancecount, basevertex, baseinstance );
        }
    }

    // ========================================================================

    public void GetInternalformativ( GLenum target, GLenum internalFormat, GLenum pname, GLsizei bufSize, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETINTERNALFORMATIVPROC >( "glGetInternalformativ", out _glGetInternalformativ );

        _glGetInternalformativ( target, internalFormat, pname, bufSize, parameters );
    }

    public void GetInternalformativ( GLenum target, GLenum internalFormat, GLenum pname, GLsizei bufSize, ref GLint[] parameters )
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
        GetDelegateForFunction< PFNGLGETACTIVEATOMICCOUNTERBUFFERIVPROC >( "glGetActiveAtomicCounterBufferiv",
                                                                           out _glGetActiveAtomicCounterBufferiv );

        _glGetActiveAtomicCounterBufferiv( ( uint )program, bufferIndex, pname, parameters );
    }

    public void GetActiveAtomicCounterBufferiv( GLuint program, GLuint bufferIndex, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETACTIVEATOMICCOUNTERBUFFERIVPROC >( "glGetActiveAtomicCounterBufferiv",
                                                                           out _glGetActiveAtomicCounterBufferiv );

        fixed ( GLint* p = &parameters[ 0 ] )
        {
            _glGetActiveAtomicCounterBufferiv( ( uint )program, bufferIndex, pname, p );
        }
    }

    // ========================================================================

    public void BindImageTexture( GLuint unit, GLuint texture, GLint level, GLboolean layered, GLint layer, GLenum access, GLenum format )
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

    public void TexStorage3D( GLenum target, GLsizei levels, GLenum internalFormat, GLsizei width, GLsizei height, GLsizei depth )
    {
        GetDelegateForFunction< PFNGLTEXSTORAGE3DPROC >( "glTexStorage3D", out _glTexStorage3D );

        _glTexStorage3D( target, levels, internalFormat, width, height, depth );
    }

    // ========================================================================

    public void DrawTransformFeedbackInstanced( GLenum mode, GLuint id, GLsizei instancecount )
    {
        GetDelegateForFunction< PFNGLDRAWTRANSFORMFEEDBACKINSTANCEDPROC >( "glDrawTransformFeedbackInstanced",
                                                                           out _glDrawTransformFeedbackInstanced );

        _glDrawTransformFeedbackInstanced( mode, id, instancecount );
    }

    // ========================================================================

    public void DrawTransformFeedbackStreamInstanced( GLenum mode, GLuint id, GLuint stream, GLsizei instancecount )
    {
        GetDelegateForFunction< PFNGLDRAWTRANSFORMFEEDBACKSTREAMINSTANCEDPROC >( "glDrawTransformFeedbackStreamInstanced",
                                                                                 out _glDrawTransformFeedbackStreamInstanced );

        _glDrawTransformFeedbackStreamInstanced( mode, id, stream, instancecount );
    }

    // ========================================================================

    public void GetPointerv( GLenum pname, IntPtr* parameters )
    {
        GetDelegateForFunction< PFNGLGETPOINTERVPROC >( "glGetPointerv", out _glGetPointerv );

        _glGetPointerv( pname, parameters );
    }

    public void GetPointerv( GLenum pname, ref IntPtr[] parameters )
    {
        var length = parameters.Length;

        var ptr = parameters[ 0 ].ToPointer();
        var p   = &ptr;

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
        GetDelegateForFunction< PFNGLDISPATCHCOMPUTEINDIRECTPROC >( "glDispatchComputeIndirect", out _glDispatchComputeIndirect );

        _glDispatchComputeIndirect( indirect );
    }

    // ========================================================================

    public void CopyImageSubData( GLuint srcName, GLenum srcTarget, GLint srcLevel, GLint srcX, GLint srcY, GLint srcZ,
                                  GLuint dstName, GLenum dstTarget, GLint dstLevel, GLint dstX, GLint dstY,
                                  GLint dstZ, GLsizei srcWidth, GLsizei srcHeight, GLsizei srcDepth )
    {
        GetDelegateForFunction< PFNGLCOPYIMAGESUBDATAPROC >( "glCopyImageSubData", out _glCopyImageSubData );

        _glCopyImageSubData( srcName, srcTarget, srcLevel, srcX, srcY, srcZ, dstName, dstTarget, dstLevel, dstX, dstY, dstZ, srcWidth,
                             srcHeight, srcDepth );
    }

    // ========================================================================

    public void FramebufferParameteri( GLenum target, GLenum pname, GLint param )
    {
        GetDelegateForFunction< PFNGLFRAMEBUFFERPARAMETERIPROC >( "glFramebufferParameteri", out _glFramebufferParameteri );

        _glFramebufferParameteri( target, pname, param );
    }

    // ========================================================================

    public void GetFramebufferParameteriv( GLenum target, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETFRAMEBUFFERPARAMETERIVPROC >( "glGetFramebufferParameteriv", out _glGetFramebufferParameteriv );

        _glGetFramebufferParameteriv( target, pname, parameters );
    }

    public void GetFramebufferParameteriv( GLenum target, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETFRAMEBUFFERPARAMETERIVPROC >( "glGetFramebufferParameteriv", out _glGetFramebufferParameteriv );

        fixed ( GLint* pParameters = &parameters[ 0 ] )
        {
            _glGetFramebufferParameteriv( target, pname, pParameters );
        }
    }

    // ========================================================================

    public void GetInternalformati64v( GLenum target, GLenum internalFormat, GLenum pname, GLsizei count, GLint64* parameters )
    {
        GetDelegateForFunction< PFNGLGETINTERNALFORMATI64VPROC >( "glGetInternalformati64v", out _glGetInternalformati64v );

        _glGetInternalformati64v( target, internalFormat, pname, count, parameters );
    }

    public void GetInternalformati64v( GLenum target, GLenum internalFormat, GLenum pname, GLsizei count, ref GLint64[] parameters )
    {
        GetDelegateForFunction< PFNGLGETINTERNALFORMATI64VPROC >( "glGetInternalformati64v", out _glGetInternalformati64v );

        fixed ( GLint64* pParams = &parameters[ 0 ] )
        {
            _glGetInternalformati64v( target, internalFormat, pname, count, pParams );
        }
    }

    // ========================================================================

    public void InvalidateTexSubImage( GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLint zoffset, GLsizei width,
                                       GLsizei height, GLsizei depth )
    {
        GetDelegateForFunction< PFNGLINVALIDATETEXSUBIMAGEPROC >( "glInvalidateTexSubImage", out _glInvalidateTexSubImage );

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
        GetDelegateForFunction< PFNGLINVALIDATEFRAMEBUFFERPROC >( "glInvalidateFramebuffer", out _glInvalidateFramebuffer );

        _glInvalidateFramebuffer( target, numAttachments, attachments );
    }

    public void InvalidateFramebuffer( GLenum target, GLsizei numAttachments, GLenum[] attachments )
    {
        GetDelegateForFunction< PFNGLINVALIDATEFRAMEBUFFERPROC >( "glInvalidateFramebuffer", out _glInvalidateFramebuffer );

        fixed ( GLenum* pAttachments = &attachments[ 0 ] )
        {
            _glInvalidateFramebuffer( target, numAttachments, pAttachments );
        }
    }

    // ========================================================================

    public void InvalidateSubFramebuffer( GLenum target, GLsizei numAttachments, GLenum* attachments, GLint x, GLint y, GLsizei width,
                                          GLsizei height )
    {
        GetDelegateForFunction< PFNGLINVALIDATESUBFRAMEBUFFERPROC >( "glInvalidateSubFramebuffer", out _glInvalidateSubFramebuffer );

        _glInvalidateSubFramebuffer( target, numAttachments, attachments, x, y, width, height );
    }

    public void InvalidateSubFramebuffer( GLenum target, GLsizei numAttachments, GLenum[] attachments, GLint x, GLint y, GLsizei width,
                                          GLsizei height )
    {
        GetDelegateForFunction< PFNGLINVALIDATESUBFRAMEBUFFERPROC >( "glInvalidateSubFramebuffer", out _glInvalidateSubFramebuffer );

        fixed ( GLenum* pAttachments = &attachments[ 0 ] )
        {
            _glInvalidateSubFramebuffer( target, numAttachments, pAttachments, x, y, width, height );
        }
    }

    // ========================================================================

    public void MultiDrawArraysIndirect( GLenum mode, IntPtr indirect, GLsizei drawcount, GLsizei stride )
    {
        GetDelegateForFunction< PFNGLMULTIDRAWARRAYSINDIRECTPROC >( "glMultiDrawArraysIndirect", out _glMultiDrawArraysIndirect );

        _glMultiDrawArraysIndirect( mode, indirect, drawcount, stride );
    }

    // ========================================================================

    public void MultiDrawElementsIndirect( GLenum mode, GLenum type, IntPtr indirect, GLsizei drawcount, GLsizei stride )
    {
        GetDelegateForFunction< PFNGLMULTIDRAWELEMENTSINDIRECTPROC >( "glMultiDrawElementsIndirect", out _glMultiDrawElementsIndirect );

        _glMultiDrawElementsIndirect( mode, type, indirect, drawcount, stride );
    }

    // ========================================================================

    public void ShaderStorageBlockBinding( GLint program, GLuint storageBlockIndex, GLuint storageBlockBinding )
    {
        if ( !IsProgram( program ) )
        {
            throw new GdxRuntimeException( $"Invalid program ID: {program}" );
        }

        GetDelegateForFunction< PFNGLSHADERSTORAGEBLOCKBINDINGPROC >( "glShaderStorageBlockBinding", out _glShaderStorageBlockBinding );

        _glShaderStorageBlockBinding( ( uint )program, storageBlockIndex, storageBlockBinding );
    }

    // ========================================================================

    public void TexBufferRange( GLenum target, GLenum internalFormat, GLuint buffer, GLintptr offset, GLsizeiptr size )
    {
        GetDelegateForFunction< PFNGLTEXBUFFERRANGEPROC >( "glTexBufferRange", out _glTexBufferRange );

        _glTexBufferRange( target, internalFormat, buffer, offset, size );
    }

    // ========================================================================

    public void TexStorage2DMultisample( GLenum target, GLsizei samples, GLenum internalFormat, GLsizei width, GLsizei height,
                                         GLboolean fixedsamplelocations )
    {
        GetDelegateForFunction< PFNGLTEXSTORAGE2DMULTISAMPLEPROC >( "glTexStorage2DMultisample", out _glTexStorage2DMultisample );

        _glTexStorage2DMultisample( target, samples, internalFormat, width, height, fixedsamplelocations );
    }

    // ========================================================================

    public void TexStorage3DMultisample( GLenum target, GLsizei samples, GLenum internalFormat, GLsizei width,
                                         GLsizei height, GLsizei depth, GLboolean fixedsamplelocations )
    {
        GetDelegateForFunction< PFNGLTEXSTORAGE3DMULTISAMPLEPROC >( "glTexStorage3DMultisample", out _glTexStorage3DMultisample );

        _glTexStorage3DMultisample( target, samples, internalFormat, width, height, depth, fixedsamplelocations );
    }

    // ========================================================================

    public void TextureView( GLuint texture,
                             GLenum target,
                             GLuint origtexture,
                             GLenum internalFormat, GLuint minlevel, GLuint numlevels, GLuint minlayer, GLuint numlayers )
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
                Logger.Warning( $"Object handle {name} for {label} cannot be <= 0" );

                // No need, in this case, to throw an exception. Just return without
                // adding a debug label. 
                return;
            }

            GetDelegateForFunction< PFNGLOBJECTLABELPROC >( "glObjectLabel", out _glObjectLabel );

            var labelBytes = Encoding.ASCII.GetBytes( label );

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
    private static GLboolean ObjectLabelAvailable()
    {
        if ( ( ( GL.GetOpenGLVersion().major >= 4 ) && ( GL.GetOpenGLVersion().minor >= 3 ) )
             || Api.Graphics.SupportsExtension( "GL_ARB_debug_output" ) )
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
        var labelBytes = Encoding.UTF8.GetBytes( label );

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

    public void ClearTexImage( GLuint texture, GLint level, GLenum format, GLenum type, IntPtr data )
    {
        GetDelegateForFunction< PFNGLCLEARTEXIMAGEPROC >( "glClearTexImage", out _glClearTexImage );

        _glClearTexImage( texture, level, format, type, data );
    }

    public void ClearTexImage< T >( GLuint texture, GLint level, GLenum format, GLenum type, T[] data ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLCLEARTEXIMAGEPROC >( "glClearTexImage", out _glClearTexImage );

        fixed ( T* t = &data[ 0 ] )
        {
            _glClearTexImage( texture, level, format, type, ( IntPtr )t );
        }
    }

    // ========================================================================

    public void ClearTexSubImage( GLuint texture, GLint level, GLint xOffset, GLint yOffset, GLint zOffset, GLsizei width, GLsizei height,
                                  GLsizei depth, GLenum format, GLenum type, IntPtr data )
    {
        GetDelegateForFunction< PFNGLCLEARTEXSUBIMAGEPROC >( "glClearTexSubImage", out _glClearTexSubImage );

        _glClearTexSubImage( texture, level, xOffset, yOffset, zOffset, width, height, depth, format, type, data );
    }

    public void ClearTexSubImage< T >( GLuint texture, GLint level, GLint xOffset, GLint yOffset, GLint zOffset, GLsizei width,
                                       GLsizei height, GLsizei depth, GLenum format, GLenum type, T[] data )
        where T : unmanaged
    {
        GetDelegateForFunction< PFNGLCLEARTEXSUBIMAGEPROC >( "glClearTexSubImage", out _glClearTexSubImage );

        fixed ( T* t = &data[ 0 ] )
        {
            _glClearTexSubImage( texture, level, xOffset, yOffset, zOffset, width, height, depth, format, type, ( IntPtr )t );
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

    public void BindBuffersRange( GLenum target, GLuint first, GLsizei count, GLuint* buffers, GLintptr* offsets, GLsizeiptr* sizes )
    {
        GetDelegateForFunction< PFNGLBINDBUFFERSRANGEPROC >( "glBindBuffersRange", out _glBindBuffersRange );

        _glBindBuffersRange( target, first, count, buffers, offsets, sizes );
    }

    public void BindBuffersRange( GLenum target, GLuint first, GLuint[] buffers, GLintptr[] offsets, GLsizeiptr[] sizes )
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

    public void CreateTransformFeedbacks( GLsizei n, GLuint* ids )
    {
        GetDelegateForFunction< PFNGLCREATETRANSFORMFEEDBACKSPROC >( "glCreateTransformFeedbacks", out _glCreateTransformFeedbacks );

        _glCreateTransformFeedbacks( n, ids );
    }

    public GLuint[] CreateTransformFeedbacks( GLsizei n )
    {
        var ids = new GLuint[ n ];

        GetDelegateForFunction< PFNGLCREATETRANSFORMFEEDBACKSPROC >( "glCreateTransformFeedbacks", out _glCreateTransformFeedbacks );

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
        GetDelegateForFunction< PFNGLTRANSFORMFEEDBACKBUFFERBASEPROC >( "glTransformFeedbackBufferBase",
                                                                        out _glTransformFeedbackBufferBase );

        _glTransformFeedbackBufferBase( xfb, index, buffer );
    }

    // ========================================================================

    public void TransformFeedbackBufferRange( GLuint xfb, GLuint index, GLuint buffer, GLintptr offset, GLsizeiptr size )
    {
        GetDelegateForFunction< PFNGLTRANSFORMFEEDBACKBUFFERRANGEPROC >( "glTransformFeedbackBufferRange",
                                                                         out _glTransformFeedbackBufferRange );

        _glTransformFeedbackBufferRange( xfb, index, buffer, offset, size );
    }

    // ========================================================================

    public void GetTransformFeedbackiv( GLuint xfb, GLenum pname, GLint* param )
    {
        GetDelegateForFunction< PFNGLGETTRANSFORMFEEDBACKIVPROC >( "glGetTransformFeedbackiv", out _glGetTransformFeedbackiv );

        _glGetTransformFeedbackiv( xfb, pname, param );
    }

    public void GetTransformFeedbackiv( GLuint xfb, GLenum pname, ref GLint[] param )
    {
        GetDelegateForFunction< PFNGLGETTRANSFORMFEEDBACKIVPROC >( "glGetTransformFeedbackiv", out _glGetTransformFeedbackiv );

        fixed ( GLint* pParam = &param[ 0 ] )
        {
            _glGetTransformFeedbackiv( xfb, pname, pParam );
        }
    }

    // ========================================================================

    public void GetTransformFeedbacki_v( GLuint xfb, GLenum pname, GLuint index, GLint* param )
    {
        GetDelegateForFunction< PFNGLGETTRANSFORMFEEDBACKI_VPROC >( "glGetTransformFeedbacki_v", out _glGetTransformFeedbacki_v );

        _glGetTransformFeedbacki_v( xfb, pname, index, param );
    }

    public void GetTransformFeedbacki_v( GLuint xfb, GLenum pname, GLuint index, ref GLint[] param )
    {
        GetDelegateForFunction< PFNGLGETTRANSFORMFEEDBACKI_VPROC >( "glGetTransformFeedbacki_v", out _glGetTransformFeedbacki_v );

        fixed ( GLint* pParam = &param[ 0 ] )
        {
            _glGetTransformFeedbacki_v( xfb, pname, index, pParam );
        }
    }

    // ========================================================================

    public void GetTransformFeedbacki64_v( GLuint xfb, GLenum pname, GLuint index, GLint64* param )
    {
        GetDelegateForFunction< PFNGLGETTRANSFORMFEEDBACKI64_VPROC >( "glGetTransformFeedbacki64_v", out _glGetTransformFeedbacki64_v );

        _glGetTransformFeedbacki64_v( xfb, pname, index, param );
    }

    public void GetTransformFeedbacki64_v( GLuint xfb, GLenum pname, GLuint index, ref GLint64[] param )
    {
        GetDelegateForFunction< PFNGLGETTRANSFORMFEEDBACKI64_VPROC >( "glGetTransformFeedbacki64_v", out _glGetTransformFeedbacki64_v );

        fixed ( GLint64* pParam = &param[ 0 ] )
        {
            _glGetTransformFeedbacki64_v( xfb, pname, index, pParam );
        }
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

    public void NamedBufferStorage< T >( GLuint buffer, GLsizeiptr size, T[] data, GLbitfield flags ) where T : unmanaged
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

    public void CopyNamedBufferSubData( GLuint readBuffer, GLuint writeBuffer, GLintptr readOffset, GLintptr writeOffset, GLsizeiptr size )
    {
        GetDelegateForFunction< PFNGLCOPYNAMEDBUFFERSUBDATAPROC >( "glCopyNamedBufferSubData", out _glCopyNamedBufferSubData );

        _glCopyNamedBufferSubData( readBuffer, writeBuffer, readOffset, writeOffset, size );
    }

    // ========================================================================

    public void ClearNamedBufferData( GLuint buffer, GLenum internalFormat, GLenum format, GLenum type, IntPtr data )
    {
        GetDelegateForFunction< PFNGLCLEARNAMEDBUFFERDATAPROC >( "glClearNamedBufferData", out _glClearNamedBufferData );

        _glClearNamedBufferData( buffer, internalFormat, format, type, data );
    }

    public void ClearNamedBufferData< T >( GLuint buffer, GLenum internalFormat, GLenum format, GLenum type, T[] data ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLCLEARNAMEDBUFFERDATAPROC >( "glClearNamedBufferData", out _glClearNamedBufferData );

        fixed ( T* t = &data[ 0 ] )
        {
            _glClearNamedBufferData( buffer, internalFormat, format, type, ( IntPtr )t );
        }
    }

    // ========================================================================

    public void ClearNamedBufferSubData( GLuint buffer, GLenum internalFormat, GLintptr offset, GLsizeiptr size, GLenum format, GLenum type,
                                         IntPtr data )
    {
        GetDelegateForFunction< PFNGLCLEARNAMEDBUFFERSUBDATAPROC >( "glClearNamedBufferSubData", out _glClearNamedBufferSubData );

        _glClearNamedBufferSubData( buffer, internalFormat, offset, size, format, type, data );
    }

    public void ClearNamedBufferSubData< T >( GLuint buffer, GLenum internalFormat, GLintptr offset, GLsizeiptr size, GLenum format,
                                              GLenum type, T[] data ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLCLEARNAMEDBUFFERSUBDATAPROC >( "glClearNamedBufferSubData", out _glClearNamedBufferSubData );

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
        var size = stackalloc GLint[ 1 ];

        GetDelegateForFunction< PFNGLGETNAMEDBUFFERPARAMETERIVPROC >( "glGetNamedBufferParameteriv", out _glGetNamedBufferParameteriv );

        _glGetNamedBufferParameteriv( buffer, IGL.GL_BUFFER_SIZE, size );

        GetDelegateForFunction< PFNGLMAPNAMEDBUFFERPROC >( "glMapNamedBuffer", out _glMapNamedBuffer );

        var ptr = _glMapNamedBuffer( buffer, access );

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

        var ptr = _glMapNamedBufferRange( buffer, offset, length, access );

        return new Span< T >( ( void* )ptr, ( int )length / sizeof( T ) );
    }

    // ========================================================================

    public GLboolean UnmapNamedBuffer( GLuint buffer )
    {
        GetDelegateForFunction< PFNGLUNMAPNAMEDBUFFERPROC >( "glUnmapNamedBuffer", out _glUnmapNamedBuffer );

        return _glUnmapNamedBuffer( buffer );
    }

    // ========================================================================

    public void FlushMappedNamedBufferRange( GLuint buffer, GLintptr offset, GLsizeiptr length )
    {
        GetDelegateForFunction< PFNGLFLUSHMAPPEDNAMEDBUFFERRANGEPROC >( "glFlushMappedNamedBufferRange",
                                                                        out _glFlushMappedNamedBufferRange );

        _glFlushMappedNamedBufferRange( buffer, offset, length );
    }

    // ========================================================================

    public void GetNamedBufferParameteriv( GLuint buffer, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETNAMEDBUFFERPARAMETERIVPROC >( "glGetNamedBufferParameteriv", out _glGetNamedBufferParameteriv );

        _glGetNamedBufferParameteriv( buffer, pname, parameters );
    }

    public void GetNamedBufferParameteriv( GLuint buffer, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETNAMEDBUFFERPARAMETERIVPROC >( "glGetNamedBufferParameteriv", out _glGetNamedBufferParameteriv );

        fixed ( GLint* ptrParameters = &parameters[ 0 ] )
        {
            _glGetNamedBufferParameteriv( buffer, pname, ptrParameters );
        }
    }

    // ========================================================================

    public void GetNamedBufferParameteri64v( GLuint buffer, GLenum pname, GLint64* parameters )
    {
        GetDelegateForFunction< PFNGLGETNAMEDBUFFERPARAMETERI64VPROC >( "glGetNamedBufferParameteri64v",
                                                                        out _glGetNamedBufferParameteri64v );

        _glGetNamedBufferParameteri64v( buffer, pname, parameters );
    }

    public void GetNamedBufferParameteri64v( GLuint buffer, GLenum pname, ref GLint64[] parameters )
    {
        GetDelegateForFunction< PFNGLGETNAMEDBUFFERPARAMETERI64VPROC >( "glGetNamedBufferParameteri64v",
                                                                        out _glGetNamedBufferParameteri64v );

        fixed ( GLint64* ptrParameters = &parameters[ 0 ] )
        {
            _glGetNamedBufferParameteri64v( buffer, pname, ptrParameters );
        }
    }

    // ========================================================================

    public void GetNamedBufferPointerv( GLuint buffer, GLenum pname, IntPtr* parameters )
    {
        GetDelegateForFunction< PFNGLGETNAMEDBUFFERPOINTERVPROC >( "glGetNamedBufferPointerv", out _glGetNamedBufferPointerv );

        _glGetNamedBufferPointerv( buffer, pname, parameters );
    }

    public void GetNamedBufferPointerv( GLuint buffer, GLenum pname, ref IntPtr[] parameters )
    {
        var ptrParameters = new IntPtr[ parameters.Length ];

        for ( var i = 0; i < parameters.Length; i++ )
        {
            ptrParameters[ i ] = ( IntPtr )parameters[ i ];
        }

        GetDelegateForFunction< PFNGLGETNAMEDBUFFERPOINTERVPROC >( "glGetNamedBufferPointerv", out _glGetNamedBufferPointerv );

        fixed ( IntPtr* ptr = &ptrParameters[ 0 ] )
        {
            _glGetNamedBufferPointerv( buffer, pname, ptr );
        }

        for ( var i = 0; i < parameters.Length; i++ )
        {
            parameters[ i ] = ( IntPtr )ptrParameters[ i ];
        }
    }

    // ========================================================================

    public void GetNamedBufferSubData( GLuint buffer, GLintptr offset, GLsizeiptr size, IntPtr data )
    {
        GetDelegateForFunction< PFNGLGETNAMEDBUFFERSUBDATAPROC >( "glGetNamedBufferSubData", out _glGetNamedBufferSubData );

        _glGetNamedBufferSubData( buffer, offset, size, data );
    }

    public T[] GetNamedBufferSubData< T >( GLuint buffer, GLintptr offset, GLsizeiptr size ) where T : unmanaged
    {
        var data = new T[ size / sizeof( T ) ];

        GetDelegateForFunction< PFNGLGETNAMEDBUFFERSUBDATAPROC >( "glGetNamedBufferSubData", out _glGetNamedBufferSubData );

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

    public void NamedFramebufferRenderbuffer( GLuint framebuffer, GLenum attachment, GLenum renderbuffertarget, GLuint renderbuffer )
    {
        GetDelegateForFunction< PFNGLNAMEDFRAMEBUFFERRENDERBUFFERPROC >( "glNamedFramebufferRenderbuffer",
                                                                         out _glNamedFramebufferRenderbuffer );

        _glNamedFramebufferRenderbuffer( framebuffer, attachment, renderbuffertarget, renderbuffer );
    }

    // ========================================================================

    public void NamedFramebufferParameteri( GLuint framebuffer, GLenum pname, GLint param )
    {
        GetDelegateForFunction< PFNGLNAMEDFRAMEBUFFERPARAMETERIPROC >( "glNamedFramebufferParameteri", out _glNamedFramebufferParameteri );

        _glNamedFramebufferParameteri( framebuffer, pname, param );
    }

    // ========================================================================

    public void NamedFramebufferTexture( GLuint framebuffer, GLenum attachment, GLuint texture, GLint level )
    {
        GetDelegateForFunction< PFNGLNAMEDFRAMEBUFFERTEXTUREPROC >( "glNamedFramebufferTexture", out _glNamedFramebufferTexture );

        _glNamedFramebufferTexture( framebuffer, attachment, texture, level );
    }

    // ========================================================================

    public void NamedFramebufferTextureLayer( GLuint framebuffer, GLenum attachment, GLuint texture, GLint level, GLint layer )
    {
        GetDelegateForFunction< PFNGLNAMEDFRAMEBUFFERTEXTURELAYERPROC >( "glNamedFramebufferTextureLayer",
                                                                         out _glNamedFramebufferTextureLayer );

        _glNamedFramebufferTextureLayer( framebuffer, attachment, texture, level, layer );
    }

    // ========================================================================

    public void NamedFramebufferDrawBuffer( GLuint framebuffer, GLenum buf )
    {
        GetDelegateForFunction< PFNGLNAMEDFRAMEBUFFERDRAWBUFFERPROC >( "glNamedFramebufferDrawBuffer", out _glNamedFramebufferDrawBuffer );

        _glNamedFramebufferDrawBuffer( framebuffer, buf );
    }

    // ========================================================================

    public void NamedFramebufferDrawBuffers( GLuint framebuffer, GLsizei n, GLenum* bufs )
    {
        GetDelegateForFunction< PFNGLNAMEDFRAMEBUFFERDRAWBUFFERSPROC >( "glNamedFramebufferDrawBuffers",
                                                                        out _glNamedFramebufferDrawBuffers );

        _glNamedFramebufferDrawBuffers( framebuffer, n, bufs );
    }

    public void NamedFramebufferDrawBuffers( GLuint framebuffer, GLenum[] bufs )
    {
        GetDelegateForFunction< PFNGLNAMEDFRAMEBUFFERDRAWBUFFERSPROC >( "glNamedFramebufferDrawBuffers",
                                                                        out _glNamedFramebufferDrawBuffers );

        fixed ( GLenum* ptrBufs = &bufs[ 0 ] )
        {
            _glNamedFramebufferDrawBuffers( framebuffer, bufs.Length, ptrBufs );
        }
    }

    // ========================================================================

    public void NamedFramebufferReadBuffer( GLuint framebuffer, GLenum src )
    {
        GetDelegateForFunction< PFNGLNAMEDFRAMEBUFFERREADBUFFERPROC >( "glNamedFramebufferReadBuffer", out _glNamedFramebufferReadBuffer );

        _glNamedFramebufferReadBuffer( framebuffer, src );
    }

    // ========================================================================

    public void InvalidateNamedFramebufferData( GLuint framebuffer, GLsizei numAttachments, GLenum* attachments )
    {
        GetDelegateForFunction< PFNGLINVALIDATENAMEDFRAMEBUFFERDATAPROC >( "glInvalidateNamedFramebufferData",
                                                                           out _glInvalidateNamedFramebufferData );

        _glInvalidateNamedFramebufferData( framebuffer, numAttachments, attachments );
    }

    public void InvalidateNamedFramebufferData( GLuint framebuffer, GLenum[] attachments )
    {
        GetDelegateForFunction< PFNGLINVALIDATENAMEDFRAMEBUFFERDATAPROC >( "glInvalidateNamedFramebufferData",
                                                                           out _glInvalidateNamedFramebufferData );

        fixed ( GLenum* ptrAttachments = &attachments[ 0 ] )
        {
            _glInvalidateNamedFramebufferData( framebuffer, attachments.Length, ptrAttachments );
        }
    }

    // ========================================================================

    public void InvalidateNamedFramebufferSubData( GLuint framebuffer, GLsizei numAttachments, GLenum* attachments, GLint x, GLint y,
                                                   GLsizei width, GLsizei height )
    {
        GetDelegateForFunction< PFNGLINVALIDATENAMEDFRAMEBUFFERSUBDATAPROC >( "glInvalidateNamedFramebufferSubData",
                                                                              out _glInvalidateNamedFramebufferSubData );

        _glInvalidateNamedFramebufferSubData( framebuffer, numAttachments, attachments, x, y, width, height );
    }

    public void InvalidateNamedFramebufferSubData( GLuint framebuffer, GLenum[] attachments, GLint x, GLint y, GLsizei width,
                                                   GLsizei height )
    {
        GetDelegateForFunction< PFNGLINVALIDATENAMEDFRAMEBUFFERSUBDATAPROC >( "glInvalidateNamedFramebufferSubData",
                                                                              out _glInvalidateNamedFramebufferSubData );

        fixed ( GLenum* ptrAttachments = &attachments[ 0 ] )
        {
            _glInvalidateNamedFramebufferSubData( framebuffer, attachments.Length, ptrAttachments, x, y, width, height );
        }
    }

    // ========================================================================

    public void ClearNamedFramebufferiv( GLuint framebuffer, GLenum buffer, GLint drawbuffer, GLint* value )
    {
        GetDelegateForFunction< PFNGLCLEARNAMEDFRAMEBUFFERIVPROC >( "glClearNamedFramebufferiv", out _glClearNamedFramebufferiv );

        _glClearNamedFramebufferiv( framebuffer, buffer, drawbuffer, value );
    }

    public void ClearNamedFramebufferiv( GLuint framebuffer, GLenum buffer, GLint drawbuffer, GLint[] value )
    {
        GetDelegateForFunction< PFNGLCLEARNAMEDFRAMEBUFFERIVPROC >( "glClearNamedFramebufferiv", out _glClearNamedFramebufferiv );

        fixed ( GLint* ptrValue = &value[ 0 ] )
        {
            _glClearNamedFramebufferiv( framebuffer, buffer, drawbuffer, ptrValue );
        }
    }

    // ========================================================================

    public void ClearNamedFramebufferuiv( GLuint framebuffer, GLenum buffer, GLint drawbuffer, GLuint* value )
    {
        GetDelegateForFunction< PFNGLCLEARNAMEDFRAMEBUFFERUIVPROC >( "glClearNamedFramebufferuiv", out _glClearNamedFramebufferuiv );

        _glClearNamedFramebufferuiv( framebuffer, buffer, drawbuffer, value );
    }

    public void ClearNamedFramebufferuiv( GLuint framebuffer, GLenum buffer, GLint drawbuffer, GLuint[] value )
    {
        GetDelegateForFunction< PFNGLCLEARNAMEDFRAMEBUFFERUIVPROC >( "glClearNamedFramebufferuiv", out _glClearNamedFramebufferuiv );

        fixed ( GLuint* ptrValue = &value[ 0 ] )
        {
            _glClearNamedFramebufferuiv( framebuffer, buffer, drawbuffer, ptrValue );
        }
    }

    // ========================================================================

    public void ClearNamedFramebufferfv( GLuint framebuffer, GLenum buffer, GLint drawbuffer, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLCLEARNAMEDFRAMEBUFFERFVPROC >( "glClearNamedFramebufferfv", out _glClearNamedFramebufferfv );

        _glClearNamedFramebufferfv( framebuffer, buffer, drawbuffer, value );
    }

    public void ClearNamedFramebufferfv( GLuint framebuffer, GLenum buffer, GLint drawbuffer, GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLCLEARNAMEDFRAMEBUFFERFVPROC >( "glClearNamedFramebufferfv", out _glClearNamedFramebufferfv );

        fixed ( GLfloat* ptrValue = &value[ 0 ] )
        {
            _glClearNamedFramebufferfv( framebuffer, buffer, drawbuffer, ptrValue );
        }
    }

    // ========================================================================

    public void ClearNamedFramebufferfi( GLuint framebuffer, GLenum buffer, GLfloat depth, GLint stencil )
    {
        GetDelegateForFunction< PFNGLCLEARNAMEDFRAMEBUFFERFIPROC >( "glClearNamedFramebufferfi", out _glClearNamedFramebufferfi );

        _glClearNamedFramebufferfi( framebuffer, buffer, depth, stencil );
    }

    // ========================================================================

    public void BlitNamedFramebuffer( GLuint readFramebuffer, GLuint drawFramebuffer, GLint srcX0, GLint srcY0, GLint srcX1, GLint srcY1,
                                      GLint dstX0, GLint dstY0, GLint dstX1, GLint dstY1,
                                      GLbitfield mask, GLenum filter )
    {
        GetDelegateForFunction< PFNGLBLITNAMEDFRAMEBUFFERPROC >( "glBlitNamedFramebuffer", out _glBlitNamedFramebuffer );

        _glBlitNamedFramebuffer( readFramebuffer, drawFramebuffer, srcX0, srcY0, srcX1, srcY1, dstX0, dstY0, dstX1, dstY1, mask, filter );
    }

    // ========================================================================

    public GLenum CheckNamedFramebufferStatus( GLuint framebuffer, GLenum target )
    {
        GetDelegateForFunction< PFNGLCHECKNAMEDFRAMEBUFFERSTATUSPROC >( "glCheckNamedFramebufferStatus",
                                                                        out _glCheckNamedFramebufferStatus );

        return _glCheckNamedFramebufferStatus( framebuffer, target );
    }

    // ========================================================================

    public void GetNamedFramebufferParameteriv( GLuint framebuffer, GLenum pname, GLint* param )
    {
        GetDelegateForFunction< PFNGLGETNAMEDFRAMEBUFFERPARAMETERIVPROC >( "glGetNamedFramebufferParameteriv",
                                                                           out _glGetNamedFramebufferParameteriv );

        _glGetNamedFramebufferParameteriv( framebuffer, pname, param );
    }

    public void GetNamedFramebufferParameteriv( GLuint framebuffer, GLenum pname, ref GLint[] param )
    {
        GetDelegateForFunction< PFNGLGETNAMEDFRAMEBUFFERPARAMETERIVPROC >( "glGetNamedFramebufferParameteriv",
                                                                           out _glGetNamedFramebufferParameteriv );

        fixed ( GLint* ptrParam = &param[ 0 ] )
        {
            _glGetNamedFramebufferParameteriv( framebuffer, pname, ptrParam );
        }
    }

    // ========================================================================

    public void GetNamedFramebufferAttachmentParameteriv( GLuint framebuffer, GLenum attachment, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETNAMEDFRAMEBUFFERATTACHMENTPARAMETERIVPROC >( "glGetNamedFramebufferAttachmentParameteriv",
                                                                                     out _glGetNamedFramebufferAttachmentParameteriv );

        _glGetNamedFramebufferAttachmentParameteriv( framebuffer, attachment, pname, parameters );
    }

    public void GetNamedFramebufferAttachmentParameteriv( GLuint framebuffer, GLenum attachment, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETNAMEDFRAMEBUFFERATTACHMENTPARAMETERIVPROC >( "glGetNamedFramebufferAttachmentParameteriv",
                                                                                     out _glGetNamedFramebufferAttachmentParameteriv );

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
        GetDelegateForFunction< PFNGLNAMEDRENDERBUFFERSTORAGEPROC >( "glNamedRenderbufferStorage", out _glNamedRenderbufferStorage );

        _glNamedRenderbufferStorage( renderbuffer, internalFormat, width, height );
    }

    // ========================================================================

    public void NamedRenderbufferStorageMultisample( GLuint renderbuffer, GLsizei samples, GLenum internalFormat, GLsizei width,
                                                     GLsizei height )
    {
        GetDelegateForFunction< PFNGLNAMEDRENDERBUFFERSTORAGEMULTISAMPLEPROC >( "glNamedRenderbufferStorageMultisample",
                                                                                out _glNamedRenderbufferStorageMultisample );

        _glNamedRenderbufferStorageMultisample( renderbuffer, samples, internalFormat, width, height );
    }

    // ========================================================================

    public void GetNamedRenderbufferParameteriv( GLuint renderbuffer, GLenum pname, GLint* param )
    {
        GetDelegateForFunction< PFNGLGETNAMEDRENDERBUFFERPARAMETERIVPROC >( "glGetNamedRenderbufferParameteriv",
                                                                            out _glGetNamedRenderbufferParameteriv );

        _glGetNamedRenderbufferParameteriv( renderbuffer, pname, param );
    }

    public void GetNamedRenderbufferParameteriv( GLuint renderbuffer, GLenum pname, ref GLint[] param )
    {
        GetDelegateForFunction< PFNGLGETNAMEDRENDERBUFFERPARAMETERIVPROC >( "glGetNamedRenderbufferParameteriv",
                                                                            out _glGetNamedRenderbufferParameteriv );

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

    // ========================================================================

    public void TextureBuffer( GLuint texture, GLenum internalFormat, GLuint buffer )
    {
        GetDelegateForFunction< PFNGLTEXTUREBUFFERPROC >( "glTextureBuffer", out _glTextureBuffer );

        _glTextureBuffer( texture, internalFormat, buffer );
    }

    // ========================================================================

    public void TextureBufferRange( GLuint texture, GLenum internalFormat, GLuint buffer, GLintptr offset, GLsizeiptr size )
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

    public void TextureStorage3D( GLuint texture, GLsizei levels, GLenum internalFormat, GLsizei width, GLsizei height, GLsizei depth )
    {
        GetDelegateForFunction< PFNGLTEXTURESTORAGE3DPROC >( "glTextureStorage3D", out _glTextureStorage3D );

        _glTextureStorage3D( texture, levels, internalFormat, width, height, depth );
    }

    // ========================================================================

    public void TextureStorage2DMultisample( GLuint texture, GLsizei samples, GLenum internalFormat, GLsizei width, GLsizei height,
                                             GLboolean fixedsamplelocations )
    {
        GetDelegateForFunction< PFNGLTEXTURESTORAGE2DMULTISAMPLEPROC >( "glTextureStorage2DMultisample",
                                                                        out _glTextureStorage2DMultisample );

        _glTextureStorage2DMultisample( texture, samples, internalFormat, width, height, fixedsamplelocations );
    }

    // ========================================================================

    public void TextureStorage3DMultisample( GLuint texture, GLsizei samples, GLenum internalFormat, GLsizei width, GLsizei height,
                                             GLsizei depth, GLboolean fixedsamplelocations )
    {
        GetDelegateForFunction< PFNGLTEXTURESTORAGE3DMULTISAMPLEPROC >( "glTextureStorage3DMultisample",
                                                                        out _glTextureStorage3DMultisample );

        _glTextureStorage3DMultisample( texture, samples, internalFormat, width, height, depth, fixedsamplelocations );
    }

    // ========================================================================

    public void TextureSubImage1D( GLuint texture, GLint level, GLint xoffset, GLsizei width, GLenum format, GLenum type, IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLTEXTURESUBIMAGE1DPROC >( "glTextureSubImage1D", out _glTextureSubImage1D );

        _glTextureSubImage1D( texture, level, xoffset, width, format, type, pixels );
    }

    public void TextureSubImage1D< T >( GLuint texture, GLint level, GLint xoffset, GLsizei width, GLenum format, GLenum type, T[] pixels )
        where T : unmanaged
    {
        GetDelegateForFunction< PFNGLTEXTURESUBIMAGE1DPROC >( "glTextureSubImage1D", out _glTextureSubImage1D );

        fixed ( T* ptrPixels = &pixels[ 0 ] )
        {
            _glTextureSubImage1D( texture, level, xoffset, width, format, type, ( IntPtr )ptrPixels );
        }
    }

    // ========================================================================

    public void TextureSubImage2D( GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height, GLenum format,
                                   GLenum type, IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLTEXTURESUBIMAGE2DPROC >( "glTextureSubImage2D", out _glTextureSubImage2D );

        _glTextureSubImage2D( texture, level, xoffset, yoffset, width, height, format, type, pixels );
    }

    public void TextureSubImage2D< T >( GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height,
                                        GLenum format, GLenum type, T[] pixels ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLTEXTURESUBIMAGE2DPROC >( "glTextureSubImage2D", out _glTextureSubImage2D );

        fixed ( T* ptrPixels = &pixels[ 0 ] )
        {
            _glTextureSubImage2D( texture, level, xoffset, yoffset, width, height, format, type, ( IntPtr )ptrPixels );
        }
    }

    // ========================================================================

    public void TextureSubImage3D( GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLint zoffset, GLsizei width, GLsizei height,
                                   GLsizei depth, GLenum format, GLenum type, IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLTEXTURESUBIMAGE3DPROC >( "glTextureSubImage3D", out _glTextureSubImage3D );

        _glTextureSubImage3D( texture, level, xoffset, yoffset, zoffset, width, height, depth, format, type, pixels );
    }

    public void TextureSubImage3D< T >( GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLint zoffset, GLsizei width,
                                        GLsizei height, GLsizei depth, GLenum format, GLenum type,
                                        T[] pixels ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLTEXTURESUBIMAGE3DPROC >( "glTextureSubImage3D", out _glTextureSubImage3D );

        fixed ( T* ptrPixels = &pixels[ 0 ] )
        {
            _glTextureSubImage3D( texture, level, xoffset, yoffset, zoffset, width, height, depth, format, type, ( IntPtr )ptrPixels );
        }
    }

    // ========================================================================

    public void CompressedTextureSubImage1D( GLuint texture, GLint level, GLint xoffset, GLsizei width,
                                             GLenum format, GLsizei imageSize, IntPtr data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXTURESUBIMAGE1DPROC >( "glCompressedTextureSubImage1D",
                                                                        out _glCompressedTextureSubImage1D );

        _glCompressedTextureSubImage1D( texture, level, xoffset, width, format, imageSize, data );
    }

    public void CompressedTextureSubImage1D( GLuint texture, GLint level, GLint xoffset, GLsizei width,
                                             GLenum format, GLsizei imageSize, byte[] data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXTURESUBIMAGE1DPROC >( "glCompressedTextureSubImage1D",
                                                                        out _glCompressedTextureSubImage1D );

        fixed ( byte* ptrData = &data[ 0 ] )
        {
            _glCompressedTextureSubImage1D( texture, level, xoffset, width, format, imageSize, ( IntPtr )ptrData );
        }
    }

    // ========================================================================

    public void CompressedTextureSubImage2D( GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height,
                                             GLenum format, GLsizei imageSize, IntPtr data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXTURESUBIMAGE2DPROC >( "glCompressedTextureSubImage2D",
                                                                        out _glCompressedTextureSubImage2D );

        _glCompressedTextureSubImage2D( texture, level, xoffset, yoffset, width, height, format, imageSize, data );
    }

    public void CompressedTextureSubImage2D( GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height,
                                             GLenum format, GLsizei imageSize, byte[] data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXTURESUBIMAGE2DPROC >( "glCompressedTextureSubImage2D",
                                                                        out _glCompressedTextureSubImage2D );

        fixed ( byte* ptrData = &data[ 0 ] )
        {
            _glCompressedTextureSubImage2D( texture, level, xoffset, yoffset, width, height, format, imageSize, ( IntPtr )ptrData );
        }
    }

    // ========================================================================

    public void CompressedTextureSubImage3D( GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLint zoffset, GLsizei width,
                                             GLsizei height, GLsizei depth, GLenum format, GLsizei imageSize,
                                             IntPtr data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXTURESUBIMAGE3DPROC >( "glCompressedTextureSubImage3D",
                                                                        out _glCompressedTextureSubImage3D );

        _glCompressedTextureSubImage3D( texture, level, xoffset, yoffset, zoffset, width, height, depth, format, imageSize, data );
    }

    public void CompressedTextureSubImage3D( GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLint zoffset, GLsizei width,
                                             GLsizei height, GLsizei depth, GLenum format, GLsizei imageSize,
                                             byte[] data )
    {
        GetDelegateForFunction< PFNGLCOMPRESSEDTEXTURESUBIMAGE3DPROC >( "glCompressedTextureSubImage3D",
                                                                        out _glCompressedTextureSubImage3D );

        fixed ( void* ptrData = &data[ 0 ] )
        {
            var dataptr = ( IntPtr )ptrData;

            _glCompressedTextureSubImage3D( texture, level, xoffset, yoffset, zoffset, width, height, depth, format, imageSize, dataptr );
        }
    }

    // ========================================================================

    public void CopyTextureSubImage1D( GLuint texture, GLint level, GLint xoffset, GLint x, GLint y, GLsizei width )
    {
        GetDelegateForFunction< PFNGLCOPYTEXTURESUBIMAGE1DPROC >( "glCopyTextureSubImage1D", out _glCopyTextureSubImage1D );

        _glCopyTextureSubImage1D( texture, level, xoffset, x, y, width );
    }

    // ========================================================================

    public void CopyTextureSubImage2D( GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLint x, GLint y, GLsizei width,
                                       GLsizei height )
    {
        GetDelegateForFunction< PFNGLCOPYTEXTURESUBIMAGE2DPROC >( "glCopyTextureSubImage2D", out _glCopyTextureSubImage2D );

        _glCopyTextureSubImage2D( texture, level, xoffset, yoffset, x, y, width, height );
    }

    // ========================================================================

    public void CopyTextureSubImage3D( GLuint texture, GLint level, GLint xoffset, GLint yoffset,
                                       GLint zoffset, GLint x, GLint y, GLsizei width, GLsizei height )
    {
        GetDelegateForFunction< PFNGLCOPYTEXTURESUBIMAGE3DPROC >( "glCopyTextureSubImage3D", out _glCopyTextureSubImage3D );

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
        GetDelegateForFunction< PFNGLTEXTUREPARAMETERIUIVPROC >( "glTextureParameterIuiv", out _glTextureParameterIuiv );

        _glTextureParameterIuiv( texture, pname, param );
    }

    public void TextureParameterIuiv( GLuint texture, GLenum pname, GLuint[] param )
    {
        GetDelegateForFunction< PFNGLTEXTUREPARAMETERIUIVPROC >( "glTextureParameterIuiv", out _glTextureParameterIuiv );

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
        GetDelegateForFunction< PFNGLGENERATETEXTUREMIPMAPPROC >( "glGenerateTextureMipmap", out _glGenerateTextureMipmap );

        _glGenerateTextureMipmap( texture );
    }

    // ========================================================================

    public void BindTextureUnit( GLuint unit, GLuint texture )
    {
        GetDelegateForFunction< PFNGLBINDTEXTUREUNITPROC >( "glBindTextureUnit", out _glBindTextureUnit );

        _glBindTextureUnit( unit, texture );
    }

    // ========================================================================

    public void GetTextureImage( GLuint texture, GLint level, GLenum format, GLenum type, GLsizei bufSize, IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLGETTEXTUREIMAGEPROC >( "glGetTextureImage", out _glGetTextureImage );

        _glGetTextureImage( texture, level, format, type, bufSize, pixels );
    }

    public T[] GetTextureImage< T >( GLuint texture, GLint level, GLenum format, GLenum type, GLsizei bufSize ) where T : unmanaged
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
        GetDelegateForFunction< PFNGLGETCOMPRESSEDTEXTUREIMAGEPROC >( "glGetCompressedTextureImage", out _glGetCompressedTextureImage );

        _glGetCompressedTextureImage( texture, level, bufSize, pixels );
    }

    public byte[] GetCompressedTextureImage( GLuint texture, GLint level, GLsizei bufSize )
    {
        var pixels = new byte[ bufSize ];

        GetDelegateForFunction< PFNGLGETCOMPRESSEDTEXTUREIMAGEPROC >( "glGetCompressedTextureImage", out _glGetCompressedTextureImage );

        fixed ( byte* ptrPixels = &pixels[ 0 ] )
        {
            _glGetCompressedTextureImage( texture, level, bufSize, ( IntPtr )ptrPixels );
        }

        return pixels;
    }

    // ========================================================================

    public void GetTextureLevelParameterfv( GLuint texture, GLint level, GLenum pname, GLfloat* param )
    {
        GetDelegateForFunction< PFNGLGETTEXTURELEVELPARAMETERFVPROC >( "glGetTextureLevelParameterfv", out _glGetTextureLevelParameterfv );

        _glGetTextureLevelParameterfv( texture, level, pname, param );
    }

    public void GetTextureLevelParameterfv( GLuint texture, GLint level, GLenum pname, ref GLfloat[] param )
    {
        GetDelegateForFunction< PFNGLGETTEXTURELEVELPARAMETERFVPROC >( "glGetTextureLevelParameterfv", out _glGetTextureLevelParameterfv );

        fixed ( GLfloat* ptrParam = &param[ 0 ] )
        {
            _glGetTextureLevelParameterfv( texture, level, pname, ptrParam );
        }
    }

    // ========================================================================

    public void GetTextureLevelParameteriv( GLuint texture, GLint level, GLenum pname, GLint* param )
    {
        GetDelegateForFunction< PFNGLGETTEXTURELEVELPARAMETERIVPROC >( "glGetTextureLevelParameteriv", out _glGetTextureLevelParameteriv );

        _glGetTextureLevelParameteriv( texture, level, pname, param );
    }

    public void GetTextureLevelParameteriv( GLuint texture, GLint level, GLenum pname, ref GLint[] param )
    {
        GetDelegateForFunction< PFNGLGETTEXTURELEVELPARAMETERIVPROC >( "glGetTextureLevelParameteriv", out _glGetTextureLevelParameteriv );

        fixed ( GLint* ptrParam = &param[ 0 ] )
        {
            _glGetTextureLevelParameteriv( texture, level, pname, ptrParam );
        }
    }

    // ========================================================================

    public void GetTextureParameterfv( GLuint texture, GLenum pname, GLfloat* param )
    {
        GetDelegateForFunction< PFNGLGETTEXTUREPARAMETERFVPROC >( "glGetTextureParameterfv", out _glGetTextureParameterfv );

        _glGetTextureParameterfv( texture, pname, param );
    }

    public void GetTextureParameterfv( GLuint texture, GLenum pname, ref GLfloat[] param )
    {
        GetDelegateForFunction< PFNGLGETTEXTUREPARAMETERFVPROC >( "glGetTextureParameterfv", out _glGetTextureParameterfv );

        fixed ( GLfloat* ptrParam = &param[ 0 ] )
        {
            _glGetTextureParameterfv( texture, pname, ptrParam );
        }
    }

    // ========================================================================

    public void GetTextureParameterIiv( GLuint texture, GLenum pname, GLint* param )
    {
        GetDelegateForFunction< PFNGLGETTEXTUREPARAMETERIIVPROC >( "glGetTextureParameterIiv", out _glGetTextureParameterIiv );

        _glGetTextureParameterIiv( texture, pname, param );
    }

    public void GetTextureParameterIiv( GLuint texture, GLenum pname, ref GLint[] param )
    {
        GetDelegateForFunction< PFNGLGETTEXTUREPARAMETERIIVPROC >( "glGetTextureParameterIiv", out _glGetTextureParameterIiv );

        fixed ( GLint* ptrParam = &param[ 0 ] )
        {
            _glGetTextureParameterIiv( texture, pname, ptrParam );
        }
    }

    // ========================================================================

    public void GetTextureParameterIuiv( GLuint texture, GLenum pname, GLuint* param )
    {
        GetDelegateForFunction< PFNGLGETTEXTUREPARAMETERIUIVPROC >( "glGetTextureParameterIuiv", out _glGetTextureParameterIuiv );

        _glGetTextureParameterIuiv( texture, pname, param );
    }

    public void GetTextureParameterIuiv( GLuint texture, GLenum pname, ref GLuint[] param )
    {
        GetDelegateForFunction< PFNGLGETTEXTUREPARAMETERIUIVPROC >( "glGetTextureParameterIuiv", out _glGetTextureParameterIuiv );

        fixed ( GLuint* ptrParam = &param[ 0 ] )
        {
            _glGetTextureParameterIuiv( texture, pname, ptrParam );
        }
    }

    // ========================================================================

    public void GetTextureParameteriv( GLuint texture, GLenum pname, GLint* param )
    {
        GetDelegateForFunction< PFNGLGETTEXTUREPARAMETERIVPROC >( "glGetTextureParameteriv", out _glGetTextureParameteriv );

        _glGetTextureParameteriv( texture, pname, param );
    }

    public void GetTextureParameteriv( GLuint texture, GLenum pname, ref GLint[] param )
    {
        GetDelegateForFunction< PFNGLGETTEXTUREPARAMETERIVPROC >( "glGetTextureParameteriv", out _glGetTextureParameteriv );

        fixed ( GLint* ptrParam = &param[ 0 ] )
        {
            _glGetTextureParameteriv( texture, pname, ptrParam );
        }
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

    // ========================================================================

    public void MemoryBarrierByRegion( GLbitfield barriers )
    {
        GetDelegateForFunction< PFNGLMEMORYBARRIERBYREGIONPROC >( "glMemoryBarrierByRegion", out _glMemoryBarrierByRegion );

        _glMemoryBarrierByRegion( barriers );
    }

    // ========================================================================

    public void GetTextureSubImage( GLuint texture, GLint level, GLint xoffset, GLint yoffset,
                                    GLint zoffset, GLsizei width, GLsizei height, GLsizei depth,
                                    GLenum format, GLenum type, GLsizei bufSize, IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLGETTEXTURESUBIMAGEPROC >( "glGetTextureSubImage", out _glGetTextureSubImage );

        _glGetTextureSubImage( texture, level, xoffset, yoffset, zoffset, width, height, depth, format, type, bufSize, pixels );
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

            _glGetTextureSubImage( texture, level, xoffset, yoffset, zoffset, width,
                                   height, depth, format, type, bufSize, dataptr );
        }

        return pixels;
    }

    // ========================================================================

    public void GetCompressedTextureSubImage( GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLint zoffset, GLsizei width,
                                              GLsizei height, GLsizei depth, GLsizei bufSize, IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLGETCOMPRESSEDTEXTURESUBIMAGEPROC >( "glGetCompressedTextureSubImage",
                                                                         out _glGetCompressedTextureSubImage );

        _glGetCompressedTextureSubImage( texture, level, xoffset, yoffset, zoffset, width, height, depth, bufSize, pixels );
    }

    public byte[] GetCompressedTextureSubImage( GLuint texture, GLint level, GLint xoffset, GLint yoffset, GLint zoffset, GLsizei width,
                                                GLsizei height, GLsizei depth, GLsizei bufSize )
    {
        var pixels = new byte[ bufSize ];

        GetDelegateForFunction< PFNGLGETCOMPRESSEDTEXTURESUBIMAGEPROC >( "glGetCompressedTextureSubImage",
                                                                         out _glGetCompressedTextureSubImage );

        fixed ( void* ptrPixels = &pixels[ 0 ] )
        {
            var dataptr = new IntPtr( ptrPixels );

            _glGetCompressedTextureSubImage( texture, level, xoffset, yoffset, zoffset, width, height, depth, bufSize, dataptr );
        }

        return pixels;
    }

    // ========================================================================

    public GLenum GetGraphicsResetStatus()
    {
        GetDelegateForFunction< PFNGLGETGRAPHICSRESETSTATUSPROC >( "glGetGraphicsResetStatus", out _glGetGraphicsResetStatus );

        return _glGetGraphicsResetStatus();
    }

    // ========================================================================

    public void GetnCompressedTexImage( GLenum target, GLint lod, GLsizei bufSize, IntPtr pixels )
    {
        GetDelegateForFunction< PFNGLGETNCOMPRESSEDTEXIMAGEPROC >( "glGetnCompressedTexImage", out _glGetnCompressedTexImage );

        _glGetnCompressedTexImage( target, lod, bufSize, pixels );
    }

    public byte[] GetnCompressedTexImage( GLenum target, GLint lod, GLsizei bufSize )
    {
        var pixels = new byte[ bufSize ];

        GetDelegateForFunction< PFNGLGETNCOMPRESSEDTEXIMAGEPROC >( "glGetnCompressedTexImage", out _glGetnCompressedTexImage );

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

    public void ReadnPixels( GLint x, GLint y, GLsizei width, GLsizei height, GLenum format, GLenum type, GLsizei bufSize, IntPtr data )
    {
        GetDelegateForFunction< PFNGLREADNPIXELSPROC >( "glReadnPixels", out _glReadnPixels );

        _glReadnPixels( x, y, width, height, format, type, bufSize, data );
    }

    public byte[] ReadnPixels( GLint x, GLint y, GLsizei width, GLsizei height, GLenum format, GLenum type, GLsizei bufSize )
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

    public void SpecializeShader( GLuint shader, GLchar* pEntryPoint, GLuint numSpecializationConstants, GLuint* pConstantIndex,
                                  GLuint* pConstantValue )
    {
        GetDelegateForFunction< PFNGLSPECIALIZESHADERPROC >( "glSpecializeShader", out _glSpecializeShader );

        _glSpecializeShader( ( uint )shader, pEntryPoint, numSpecializationConstants, pConstantIndex, pConstantValue );
    }

    public void SpecializeShader( GLuint shader, string pEntryPoint, GLuint numSpecializationConstants, GLuint[] pConstantIndex,
                                  GLuint[] pConstantValue )
    {
        var pEntryPointBytes = Encoding.UTF8.GetBytes( pEntryPoint );

        GetDelegateForFunction< PFNGLSPECIALIZESHADERPROC >( "glSpecializeShader", out _glSpecializeShader );

        fixed ( GLchar* ptrPEntryPoint = &pEntryPointBytes[ 0 ] )
        fixed ( GLuint* ptrPConstantIndex = &pConstantIndex[ 0 ] )
        fixed ( GLuint* ptrPConstantValue = &pConstantValue[ 0 ] )
        {
            _glSpecializeShader( ( uint )shader, ptrPEntryPoint, numSpecializationConstants, ptrPConstantIndex, ptrPConstantValue );
        }
    }

    // ========================================================================

    public void MultiDrawArraysIndirectCount( GLenum mode, IntPtr indirect, GLintptr drawcount, GLsizei maxdrawcount, GLsizei stride )
    {
        GetDelegateForFunction< PFNGLMULTIDRAWARRAYSINDIRECTCOUNTPROC >( "glMultiDrawArraysIndirectCount",
                                                                         out _glMultiDrawArraysIndirectCount );

        _glMultiDrawArraysIndirectCount( mode, indirect, drawcount, maxdrawcount, stride );
    }

    // ========================================================================

    public void MultiDrawElementsIndirectCount( GLenum mode, GLenum type, IntPtr indirect, GLintptr drawcount, GLsizei maxdrawcount,
                                                GLsizei stride )
    {
        GetDelegateForFunction< PFNGLMULTIDRAWELEMENTSINDIRECTCOUNTPROC >( "glMultiDrawElementsIndirectCount",
                                                                           out _glMultiDrawElementsIndirectCount );

        _glMultiDrawElementsIndirectCount( mode, type, indirect, drawcount, maxdrawcount, stride );
    }

    // ========================================================================

    public void PolygonOffsetClamp( GLfloat factor, GLfloat units, GLfloat clamp )
    {
        GetDelegateForFunction< PFNGLPOLYGONOFFSETCLAMPPROC >( "glPolygonOffsetClamp", out _glPolygonOffsetClamp );

        _glPolygonOffsetClamp( factor, units, clamp );
    }

    // ========================================================================

    public void TransformFeedbackVaryings( GLuint program, GLsizei count, GLchar** varyings, GLenum bufferMode )
    {
        GetDelegateForFunction< PFNGLTRANSFORMFEEDBACKVARYINGSPROC >( "glTransformFeedbackVaryings", out _glTransformFeedbackVaryings );

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

        GetDelegateForFunction< PFNGLTRANSFORMFEEDBACKVARYINGSPROC >( "glTransformFeedbackVaryings", out _glTransformFeedbackVaryings );

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
        GetDelegateForFunction< PFNGLGETTRANSFORMFEEDBACKVARYINGPROC >( "glGetTransformFeedbackVarying",
                                                                        out _glGetTransformFeedbackVarying );

        _glGetTransformFeedbackVarying( ( uint )program, index, bufSize, length, size, type, name );
    }

    // ========================================================================

    public string GetTransformFeedbackVarying( GLuint program, GLuint index, GLsizei bufSize, out GLsizei size, out GLenum type )
    {
        var name = new GLchar[ bufSize ];

        GetDelegateForFunction< PFNGLGETTRANSFORMFEEDBACKVARYINGPROC >( "glGetTransformFeedbackVarying",
                                                                        out _glGetTransformFeedbackVarying );

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

    // ========================================================================

    public void ClampColor( GLenum target, GLenum clamp )
    {
        GetDelegateForFunction< PFNGLCLAMPCOLORPROC >( "glClampColor", out _glClampColor );

        _glClampColor( target, clamp );
    }

    // ========================================================================

    public void BeginConditionalRender( GLuint id, GLenum mode )
    {
        GetDelegateForFunction< PFNGLBEGINCONDITIONALRENDERPROC >( "glBeginConditionalRender", out _glBeginConditionalRender );

        _glBeginConditionalRender( id, mode );
    }

    // ========================================================================

    public void EndConditionalRender()
    {
        GetDelegateForFunction< PFNGLENDCONDITIONALRENDERPROC >( "glEndConditionalRender", out _glEndConditionalRender );

        _glEndConditionalRender();
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

    // ========================================================================

    public void GetUniformuiv( GLuint program, GLint location, GLuint* parameters )
    {
        GetDelegateForFunction< PFNGLGETUNIFORMUIVPROC >( "glGetUniformuiv", out _glGetUniformuiv );

        _glGetUniformuiv( ( uint )program, location, parameters );
    }

    public void GetUniformuiv( GLuint program, GLint location, ref GLuint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETUNIFORMUIVPROC >( "glGetUniformuiv", out _glGetUniformuiv );

        fixed ( GLuint* p = &parameters[ 0 ] )
        {
            _glGetUniformuiv( ( uint )program, location, p );
        }
    }

    // ========================================================================

    public void BindFragDataLocation( GLuint program, GLuint color, GLchar* name )
    {
        GetDelegateForFunction< PFNGLBINDFRAGDATALOCATIONPROC >( "glBindFragDataLocation", out _glBindFragDataLocation );

        _glBindFragDataLocation( ( uint )program, color, name );
    }

    public void BindFragDataLocation( GLuint program, GLuint color, string name )
    {
        var narr = Encoding.UTF8.GetBytes( name );

        GetDelegateForFunction< PFNGLBINDFRAGDATALOCATIONPROC >( "glBindFragDataLocation", out _glBindFragDataLocation );

        fixed ( GLchar* p = &narr[ 0 ] )
        {
            _glBindFragDataLocation( ( uint )program, color, p );
        }
    }

    // ========================================================================

    public GLint GetFragDataLocation( GLuint program, GLchar* name )
    {
        GetDelegateForFunction< PFNGLGETFRAGDATALOCATIONPROC >( "glGetFragDataLocation", out _glGetFragDataLocation );

        return _glGetFragDataLocation( ( uint )program, name );
    }

    public GLint GetFragDataLocation( GLuint program, string name )
    {
        var narr = Encoding.UTF8.GetBytes( name );

        GetDelegateForFunction< PFNGLGETFRAGDATALOCATIONPROC >( "glGetFragDataLocation", out _glGetFragDataLocation );

        fixed ( GLchar* p = &narr[ 0 ] )
        {
            return _glGetFragDataLocation( ( uint )program, p );
        }
    }

    // ========================================================================

    public void Uniform1ui( GLint location, GLuint v0 )
    {
        GetDelegateForFunction< PFNGLUNIFORM1UIPROC >( "glUniform1ui", out _glUniform1ui );

        _glUniform1ui( location, v0 );
    }

    // ========================================================================

    public void Uniform2ui( GLint location, GLuint v0, GLuint v1 )
    {
        GetDelegateForFunction< PFNGLUNIFORM2UIPROC >( "glUniform2ui", out _glUniform2ui );

        _glUniform2ui( location, v0, v1 );
    }

    // ========================================================================

    public void Uniform3ui( GLint location, GLuint v0, GLuint v1, GLuint v2 )
    {
        GetDelegateForFunction< PFNGLUNIFORM3UIPROC >( "glUniform3ui", out _glUniform3ui );

        _glUniform3ui( location, v0, v1, v2 );
    }

    // ========================================================================

    public void Uniform4ui( GLint location, GLuint v0, GLuint v1, GLuint v2, GLuint v3 )
    {
        GetDelegateForFunction< PFNGLUNIFORM4UIPROC >( "glUniform4ui", out _glUniform4ui );

        _glUniform4ui( location, v0, v1, v2, v3 );
    }

    // ========================================================================

    public void Uniform1uiv( GLint location, GLsizei count, GLuint* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM1UIVPROC >( "glUniform1uiv", out _glUniform1uiv );

        _glUniform1uiv( location, count, value );
    }

    public void Uniform1uiv( GLint location, GLuint[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM1UIVPROC >( "glUniform1uiv", out _glUniform1uiv );

        fixed ( GLuint* p = &value[ 0 ] )
        {
            _glUniform1uiv( location, value.Length, p );
        }
    }

    // ========================================================================

    public void Uniform2uiv( GLint location, GLsizei count, GLuint* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM2UIVPROC >( "glUniform2uiv", out _glUniform2uiv );

        _glUniform2uiv( location, count, value );
    }

    public void Uniform2uiv( GLint location, GLuint[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM2UIVPROC >( "glUniform2uiv", out _glUniform2uiv );

        fixed ( GLuint* p = &value[ 0 ] )
        {
            _glUniform2uiv( location, value.Length / 2, p );
        }
    }

    // ========================================================================

    public void Uniform3uiv( GLint location, GLsizei count, GLuint* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM3UIVPROC >( "glUniform3uiv", out _glUniform3uiv );

        _glUniform3uiv( location, count, value );
    }

    public void Uniform3uiv( GLint location, GLuint[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM3UIVPROC >( "glUniform3uiv", out _glUniform3uiv );

        fixed ( GLuint* p = &value[ 0 ] )
        {
            _glUniform3uiv( location, value.Length / 3, p );
        }
    }

    // ========================================================================

    public void Uniform4uiv( GLint location, GLsizei count, GLuint* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM4UIVPROC >( "glUniform4uiv", out _glUniform4uiv );

        _glUniform4uiv( location, count, value );
    }

    public void Uniform4uiv( GLint location, GLuint[] value )
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

        var ptr = _glGetStringi( name, index );

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

    public GLboolean IsRenderbuffer( GLuint renderbuffer )
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
        GetDelegateForFunction< PFNGLGETRENDERBUFFERPARAMETERIVPROC >( "glGetRenderbufferParameteriv", out _glGetRenderbufferParameteriv );

        _glGetRenderbufferParameteriv( target, pname, parameters );
    }

    public void GetRenderbufferParameteriv( GLenum target, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETRENDERBUFFERPARAMETERIVPROC >( "glGetRenderbufferParameteriv", out _glGetRenderbufferParameteriv );

        fixed ( GLint* p = &parameters[ 0 ] )
        {
            _glGetRenderbufferParameteriv( target, pname, p );
        }
    }

    // ========================================================================

    public GLboolean IsFramebuffer( GLuint framebuffer )
    {
        GetDelegateForFunction< PFNGLISFRAMEBUFFERPROC >( "glIsFramebuffer", out _glIsFramebuffer );

        return _glIsFramebuffer( framebuffer );
    }

    // ========================================================================

    public void BindFramebuffer( GLenum target, GLuint framebuffer )
    {
        GetDelegateForFunction< PFNGLBINDFRAMEBUFFERPROC >( "glBindFramebuffer", out _glBindFramebuffer );

        _glBindFramebuffer( target, framebuffer );
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
        GetDelegateForFunction< PFNGLCHECKFRAMEBUFFERSTATUSPROC >( "glCheckFramebufferStatus", out _glCheckFramebufferStatus );

        return _glCheckFramebufferStatus( target );
    }

    // ========================================================================

    public void FramebufferTexture1D( GLenum target, GLenum attachment, GLenum textarget, GLuint texture, GLint level )
    {
        GetDelegateForFunction< PFNGLFRAMEBUFFERTEXTURE1DPROC >( "glFramebufferTexture1D", out _glFramebufferTexture1D );

        _glFramebufferTexture1D( target, attachment, textarget, texture, level );
    }

    // ========================================================================

    public void FramebufferTexture2D( GLenum target, GLenum attachment, GLenum textarget, GLuint texture, GLint level )
    {
        GetDelegateForFunction< PFNGLFRAMEBUFFERTEXTURE2DPROC >( "glFramebufferTexture2D", out _glFramebufferTexture2D );

        _glFramebufferTexture2D( target, attachment, textarget, texture, level );
    }

    // ========================================================================

    public void FramebufferTexture3D( GLenum target, GLenum attachment, GLenum textarget, GLuint texture, GLint level, GLint zoffset )
    {
        GetDelegateForFunction< PFNGLFRAMEBUFFERTEXTURE3DPROC >( "glFramebufferTexture3D", out _glFramebufferTexture3D );

        _glFramebufferTexture3D( target, attachment, textarget, texture, level, zoffset );
    }

    // ========================================================================

    public void FramebufferRenderbuffer( GLenum target, GLenum attachment, GLenum renderbuffertarget, GLuint renderbuffer )
    {
        GetDelegateForFunction< PFNGLFRAMEBUFFERRENDERBUFFERPROC >( "glFramebufferRenderbuffer", out _glFramebufferRenderbuffer );

        _glFramebufferRenderbuffer( target, attachment, renderbuffertarget, renderbuffer );
    }

    // ========================================================================

    public void GetFramebufferAttachmentParameteriv( GLenum target, GLenum attachment, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETFRAMEBUFFERATTACHMENTPARAMETERIVPROC >( "glGetFramebufferAttachmentParameteriv",
                                                                                out _glGetFramebufferAttachmentParameteriv );

        _glGetFramebufferAttachmentParameteriv( target, attachment, pname, parameters );
    }

    public void GetFramebufferAttachmentParameteriv( GLenum target, GLenum attachment, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETFRAMEBUFFERATTACHMENTPARAMETERIVPROC >( "glGetFramebufferAttachmentParameteriv",
                                                                                out _glGetFramebufferAttachmentParameteriv );

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

    public void RenderbufferStorageMultisample( GLenum target, GLsizei samples, GLenum internalFormat, GLsizei width, GLsizei height )
    {
        GetDelegateForFunction< PFNGLRENDERBUFFERSTORAGEMULTISAMPLEPROC >( "glRenderbufferStorageMultisample",
                                                                           out _glRenderbufferStorageMultisample );

        _glRenderbufferStorageMultisample( target, samples, internalFormat, width, height );
    }

    // ========================================================================

    public void FramebufferTextureLayer( GLenum target, GLenum attachment, GLuint texture, GLint level, GLint layer )
    {
        GetDelegateForFunction< PFNGLFRAMEBUFFERTEXTURELAYERPROC >( "glFramebufferTextureLayer", out _glFramebufferTextureLayer );

        _glFramebufferTextureLayer( target, attachment, texture, level, layer );
    }

    // ========================================================================

    public IntPtr MapBufferRange( GLenum target, GLintptr offset, GLsizeiptr length, GLbitfield access )
    {
        GetDelegateForFunction< PFNGLMAPBUFFERRANGEPROC >( "glMapBufferRange", out _glMapBufferRange );

        return _glMapBufferRange( target, offset, length, access );
    }

    public Span< T > MapBufferRange< T >( GLenum target, GLintptr offset, GLsizeiptr length, GLbitfield access ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLMAPBUFFERRANGEPROC >( "glMapBufferRange", out _glMapBufferRange );

        var ret = _glMapBufferRange( target, offset, length, access );

        return new Span< T >( ( void* )ret, length );
    }

    // ========================================================================

    public void FlushMappedBufferRange( GLenum target, GLintptr offset, GLsizeiptr length )
    {
        GetDelegateForFunction< PFNGLFLUSHMAPPEDBUFFERRANGEPROC >( "glFlushMappedBufferRange", out _glFlushMappedBufferRange );

        _glFlushMappedBufferRange( target, offset, length );
    }

    // ========================================================================

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
        GetDelegateForFunction< PFNGLDRAWELEMENTSINSTANCEDPROC >( "glDrawElementsInstanced", out _glDrawElementsInstanced );

        _glDrawElementsInstanced( mode, count, type, indices, instancecount );
    }

    /// <inheritdoc />
    public void DrawElementsInstanced< T >( GLenum mode, GLsizei count, GLenum type, T[] indices, GLsizei instancecount )
        where T : unmanaged, IUnsignedNumber< T >
    {
        GetDelegateForFunction< PFNGLDRAWELEMENTSINSTANCEDPROC >( "glDrawElementsInstanced", out _glDrawElementsInstanced );

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
        GetDelegateForFunction< PFNGLPRIMITIVERESTARTINDEXPROC >( "glPrimitiveRestartIndex", out _glPrimitiveRestartIndex );

        _glPrimitiveRestartIndex( index );
    }

    // ========================================================================

    /// <inheritdoc />
    public void CopyBufferSubData( GLenum readTarget, GLenum writeTarget, GLintptr readOffset, GLintptr writeOffset, GLsizeiptr size )
    {
        GetDelegateForFunction< PFNGLCOPYBUFFERSUBDATAPROC >( "glCopyBufferSubData", out _glCopyBufferSubData );

        _glCopyBufferSubData( readTarget, writeTarget, readOffset, writeOffset, size );
    }

    // ========================================================================

    /// <inheritdoc />
    public void DrawElementsBaseVertex( GLenum mode, GLsizei count, GLenum type, IntPtr indices, GLint basevertex )
    {
        GetDelegateForFunction< PFNGLDRAWELEMENTSBASEVERTEXPROC >( "glDrawElementsBaseVertex", out _glDrawElementsBaseVertex );

        _glDrawElementsBaseVertex( mode, count, type, indices, basevertex );
    }

    /// <inheritdoc />
    public void DrawElementsBaseVertex< T >( GLenum mode, GLsizei count, GLenum type, T[] indices, GLint basevertex )
        where T : unmanaged, IUnsignedNumber< T >
    {
        GetDelegateForFunction< PFNGLDRAWELEMENTSBASEVERTEXPROC >( "glDrawElementsBaseVertex", out _glDrawElementsBaseVertex );

        {
            fixed ( void* p = &indices[ 0 ] )
            {
                _glDrawElementsBaseVertex( mode, count, type, ( IntPtr )p, basevertex );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void DrawRangeElementsBaseVertex( GLenum mode, GLuint start, GLuint end, GLsizei count, GLenum type, IntPtr indices,
                                             GLint basevertex )
    {
        GetDelegateForFunction< PFNGLDRAWRANGEELEMENTSBASEVERTEXPROC >( "glDrawRangeElementsBaseVertex",
                                                                        out _glDrawRangeElementsBaseVertex );

        _glDrawRangeElementsBaseVertex( mode, start, end, count, type, indices, basevertex );
    }

    /// <inheritdoc />
    public void DrawRangeElementsBaseVertex< T >( GLenum mode, GLuint start, GLuint end, GLsizei count, GLenum type, T[] indices,
                                                  GLint basevertex )
        where T : unmanaged, IUnsignedNumber< T >
    {
        GetDelegateForFunction< PFNGLDRAWRANGEELEMENTSBASEVERTEXPROC >( "glDrawRangeElementsBaseVertex",
                                                                        out _glDrawRangeElementsBaseVertex );

        fixed ( void* p = &indices[ 0 ] )
        {
            var dataptr = ( IntPtr )p;

            _glDrawRangeElementsBaseVertex( mode, start, end, count, type, dataptr, basevertex );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void DrawElementsInstancedBaseVertex( GLenum mode, GLsizei count, GLenum type, IntPtr indices, GLsizei instancecount,
                                                 GLint basevertex )
    {
        GetDelegateForFunction< PFNGLDRAWELEMENTSINSTANCEDBASEVERTEXPROC >( "glDrawElementsInstancedBaseVertex",
                                                                            out _glDrawElementsInstancedBaseVertex );

        _glDrawElementsInstancedBaseVertex( mode, count, type, indices, instancecount, basevertex );
    }

    /// <inheritdoc />
    public void DrawElementsInstancedBaseVertex< T >( GLenum mode, GLsizei count, GLenum type, T[] indices, GLsizei instancecount,
                                                      GLint basevertex )
        where T : unmanaged, IUnsignedNumber< T >
    {
        GetDelegateForFunction< PFNGLDRAWELEMENTSINSTANCEDBASEVERTEXPROC >( "glDrawElementsInstancedBaseVertex",
                                                                            out _glDrawElementsInstancedBaseVertex );

        {
            fixed ( void* p = &indices[ 0 ] )
            {
                var dataptr = ( IntPtr )p;

                _glDrawElementsInstancedBaseVertex( mode, count, type, dataptr, instancecount, basevertex );
            }
        }
    }

    // ========================================================================

    public void MultiDrawElementsBaseVertex( GLenum mode, GLsizei* count, GLenum type, IntPtr* indices, GLsizei drawcount,
                                             GLint* basevertex )
    {
        GetDelegateForFunction< PFNGLMULTIDRAWELEMENTSBASEVERTEXPROC >( "glMultiDrawElementsBaseVertex",
                                                                        out _glMultiDrawElementsBaseVertex );

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

        GetDelegateForFunction< PFNGLMULTIDRAWELEMENTSBASEVERTEXPROC >( "glMultiDrawElementsBaseVertex",
                                                                        out _glMultiDrawElementsBaseVertex );

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

    public GLboolean IsSync( IntPtr sync )
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

    // ========================================================================
}