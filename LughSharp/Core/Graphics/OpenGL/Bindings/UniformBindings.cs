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

// ============================================================================
using GLenum = int;
using GLfloat = float;
using GLint = int;
using GLsizei = int;
using GLdouble = double;
using GLuint = uint;
using GLboolean = bool;
using GLbyte = sbyte;
using GLchar = byte;

// ============================================================================

namespace LughSharp.Core.Graphics.OpenGL.Bindings;

public unsafe partial class GLBindings
{
    /// <inheritdoc />
    public GLint GetUniformLocation( GLint program, GLchar* name )
    {
        if ( !GL.IsProgram( program ) || ( program == -1 ) )
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
        if ( !GL.IsProgram( program ) || ( program == -1 ) )
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
        if ( !GL.IsProgram( program ) || ( program == -1 ) )
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
        if ( !GL.IsProgram( program ) || ( program == -1 ) )
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
        if ( !GL.IsProgram( program ) || ( program == -1 ) )
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
        if ( !GL.IsProgram( program ) || ( program == -1 ) )
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
    public void Uniform1f( GLint location, GLfloat v0 )
    {
        GetDelegateForFunction< PFNGLUNIFORM1FPROC >( "glUniform1f", out _glUniform1f );

        _glUniform1f( location, v0 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform2f( GLint location, GLfloat v0, GLfloat v1 )
    {
        GetDelegateForFunction< PFNGLUNIFORM2FPROC >( "glUniform2f", out _glUniform2f );

        _glUniform2f( location, v0, v1 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform3f( GLint location, GLfloat v0, GLfloat v1, GLfloat v2 )
    {
        GetDelegateForFunction< PFNGLUNIFORM3FPROC >( "glUniform3f", out _glUniform3f );

        _glUniform3f( location, v0, v1, v2 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform4f( GLint location, GLfloat v0, GLfloat v1, GLfloat v2, GLfloat v3 )
    {
        GetDelegateForFunction< PFNGLUNIFORM4FPROC >( "glUniform4f", out _glUniform4f );

        _glUniform4f( location, v0, v1, v2, v3 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform1i( GLint location, GLint v0 )
    {
        GetDelegateForFunction< PFNGLUNIFORM1IPROC >( "glUniform1i", out _glUniform1i );

        _glUniform1i( location, v0 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform2i( GLint location, GLint v0, GLint v1 )
    {
        GetDelegateForFunction< PFNGLUNIFORM2IPROC >( "glUniform2i", out _glUniform2i );

        _glUniform2i( location, v0, v1 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform3i( GLint location, GLint v0, GLint v1, GLint v2 )
    {
        GetDelegateForFunction< PFNGLUNIFORM3IPROC >( "glUniform23", out _glUniform3i );

        _glUniform3i( location, v0, v1, v2 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform4i( GLint location, GLint v0, GLint v1, GLint v2, GLint v3 )
    {
        GetDelegateForFunction< PFNGLUNIFORM4IPROC >( "glUniform4i", out _glUniform4i );

        _glUniform4i( location, v0, v1, v2, v3 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform1fv( GLint location, GLsizei count, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM1FVPROC >( "glUniform1fv", out _glUniform1fv );

        _glUniform1fv( location, count, value );
    }

    /// <inheritdoc />
    public void Uniform1fv( GLint location, params GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM1FVPROC >( "glUniform1fv", out _glUniform1fv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniform1fv( location, value.Length, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform2fv( GLint location, GLsizei count, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM2FVPROC >( "glUniform2fv", out _glUniform2fv );

        _glUniform2fv( location, count, value );
    }

    /// <inheritdoc />
    public void Uniform2fv( GLint location, params GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM2FVPROC >( "glUniform2fv", out _glUniform2fv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniform2fv( location, value.Length / 2, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform3fv( GLint location, GLsizei count, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM3FVPROC >( "glUniform3fv", out _glUniform3fv );

        _glUniform3fv( location, count, value );
    }

    /// <inheritdoc />
    public void Uniform3fv( GLint location, params GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM3FVPROC >( "glUniform3fv", out _glUniform3fv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniform3fv( location, value.Length / 3, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform4fv( GLint location, GLsizei count, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM4FVPROC >( "glUniform4fv", out _glUniform4fv );

        _glUniform4fv( location, count, value );
    }

    /// <inheritdoc />
    public void Uniform4fv( GLint location, params GLfloat[] value )
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
    public void Uniform1iv( GLint location, GLsizei count, GLint* value )
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
    public void Uniform1iv( GLint location, params GLint[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM1IVPROC >( "glUniform1iv", out _glUniform1iv );

        fixed ( GLint* p = &value[ 0 ] )
        {
            _glUniform1iv( location, value.Length, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform2iv( GLint location, GLsizei count, GLint* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM2IVPROC >( "glUniform2iv", out _glUniform2iv );

        _glUniform2iv( location, count, value );
    }

    /// <inheritdoc />
    public void Uniform2iv( GLint location, params GLint[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM2IVPROC >( "glUniform2iv", out _glUniform2iv );

        fixed ( GLint* p = &value[ 0 ] )
        {
            _glUniform2iv( location, value.Length / 2, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform3iv( GLint location, GLsizei count, GLint* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM3IVPROC >( "glUniform3iv", out _glUniform3iv );

        _glUniform3iv( location, count, value );
    }

    /// <inheritdoc />
    public void Uniform3iv( GLint location, params GLint[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM3IVPROC >( "glUniform3iv", out _glUniform3iv );

        fixed ( GLint* p = &value[ 0 ] )
        {
            _glUniform3iv( location, value.Length / 3, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void Uniform4iv( GLint location, GLsizei count, GLint* value )
    {
        GetDelegateForFunction< PFNGLUNIFORM4IVPROC >( "glUniform4iv", out _glUniform4iv );

        _glUniform4iv( location, count, value );
    }

    /// <inheritdoc />
    public void Uniform4iv( GLint location, params GLint[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORM4IVPROC >( "glUniform4iv", out _glUniform4iv );

        fixed ( GLint* p = &value[ 0 ] )
        {
            _glUniform4iv( location, value.Length / 4, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void UniformMatrix2fv( GLint location, GLsizei count, GLboolean transpose, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX2FVPROC >( "glUniformMatrix2fv", out _glUniformMatrix2fv );

        _glUniformMatrix2fv( location, count, transpose, value );
    }

    /// <inheritdoc />
    public void UniformMatrix2fv( GLint location, GLboolean transpose, params GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX2FVPROC >( "glUniformMatrix2fv", out _glUniformMatrix2fv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniformMatrix2fv( location, value.Length / 4, transpose, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void UniformMatrix3fv( GLint location, GLsizei count, GLboolean transpose, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX3FVPROC >( "glUniformMatrix3fv", out _glUniformMatrix3fv );

        _glUniformMatrix3fv( location, count, transpose, value );
    }

    /// <inheritdoc />
    public void UniformMatrix3fv( GLint location, GLboolean transpose, params GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX3FVPROC >( "glUniformMatrix3fv", out _glUniformMatrix3fv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniformMatrix3fv( location, value.Length / 9, transpose, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void UniformMatrix4fv( GLint location, GLsizei count, GLboolean transpose, GLfloat* value )
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
    public void UniformMatrix4fv( int location, GLboolean transpose, params GLfloat[] value )
    {
        Guard.Against.Null( value );

        GetDelegateForFunction< PFNGLUNIFORMMATRIX4FVPROC >( "glUniformMatrix4fv", out _glUniformMatrix4fv );

        if ( ( value.Length % 16 ) != 0 )
        {
            throw new GdxRuntimeException( $"Error: value array length ({value.Length}) is not a multiple " +
                                           $"of 16.  Must provide a whole number of 4x4 matrices." );
        }

        GetDelegateForFunction< PFNGLGETERRORPROC >( "glGetError", out _glGetError );

        var matrixCount = value.Length / 16;

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniformMatrix4fv( location, matrixCount, transpose, p );

            var error = _glGetError();

            if ( error != IGL.GL_NO_ERROR )
            {
                throw new GdxRuntimeException( $"OpenGL Error: {error} after glUniformMatrix4fv. Location: {location}, " +
                                               $"Matrix Count: {matrixCount}, Transpose: {transpose}" );
            }
        }
    }

    // ========================================================================

    public void GetnUniformdv( GLuint program, GLint location, GLsizei bufSize, GLdouble* parameters )
    {
        GetDelegateForFunction< PFNGLGETNUNIFORMDVPROC >( "glGetnUniformdv", out _glGetnUniformdv );

        _glGetnUniformdv( ( uint )program, location, bufSize, parameters );
    }

    public void GetnUniformdv( GLuint program, GLint location, GLsizei bufSize, ref GLdouble[] parameters )
    {
        GetDelegateForFunction< PFNGLGETNUNIFORMDVPROC >( "glGetnUniformdv", out _glGetnUniformdv );

        fixed ( void* ptrParameters = &parameters[ 0 ] )
        {
            _glGetnUniformdv( ( uint )program, location, bufSize, ( GLdouble* )ptrParameters );
        }
    }

    // ========================================================================

    public void GetnUniformfv( GLuint program, GLint location, GLsizei bufSize, GLfloat* parameters )
    {
        GetDelegateForFunction< PFNGLGETNUNIFORMFVPROC >( "glGetnUniformfv", out _glGetnUniformfv );

        _glGetnUniformfv( ( uint )program, location, bufSize, parameters );
    }

    public void GetnUniformfv( GLuint program, GLint location, GLsizei bufSize, ref GLfloat[] parameters )
    {
        GetDelegateForFunction< PFNGLGETNUNIFORMFVPROC >( "glGetnUniformfv", out _glGetnUniformfv );

        fixed ( void* ptrParameters = &parameters[ 0 ] )
        {
            _glGetnUniformfv( ( uint )program, location, bufSize, ( GLfloat* )ptrParameters );
        }
    }

    // ========================================================================

    public void GetnUniformiv( GLuint program, GLint location, GLsizei bufSize, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETNUNIFORMIVPROC >( "glGetnUniformiv", out _glGetnUniformiv );

        _glGetnUniformiv( ( uint )program, location, bufSize, parameters );
    }

    public void GetnUniformiv( GLuint program, GLint location, GLsizei bufSize, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETNUNIFORMIVPROC >( "glGetnUniformiv", out _glGetnUniformiv );

        fixed ( void* ptrParameters = &parameters[ 0 ] )
        {
            _glGetnUniformiv( ( uint )program, location, bufSize, ( GLint* )ptrParameters );
        }
    }

    // ========================================================================

    public void GetnUniformuiv( GLuint program, GLint location, GLsizei bufSize, GLuint* parameters )
    {
        GetDelegateForFunction< PFNGLGETNUNIFORMUIVPROC >( "glGetnUniformuiv", out _glGetnUniformuiv );

        _glGetnUniformuiv( ( uint )program, location, bufSize, parameters );
    }

    public void GetnUniformuiv( GLuint program, GLint location, GLsizei bufSize, ref GLuint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETNUNIFORMUIVPROC >( "glGetnUniformuiv", out _glGetnUniformuiv );

        fixed ( void* ptrParameters = &parameters[ 0 ] )
        {
            _glGetnUniformuiv( ( uint )program, location, bufSize, ( GLuint* )ptrParameters );
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

    public void UniformMatrix2dv( GLint location, GLsizei count, GLboolean transpose, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX2DVPROC >( "glUniformMatrix2dv", out _glUniformMatrix2dv );

        _glUniformMatrix2dv( location, count, transpose, value );
    }

    public void UniformMatrix2dv( GLint location, GLboolean transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX2DVPROC >( "glUniformMatrix2dv", out _glUniformMatrix2dv );

        fixed ( GLdouble* p = &value[ 0 ] )
        {
            _glUniformMatrix2dv( location, value.Length / 4, transpose, p );
        }
    }

    // ========================================================================

    public void UniformMatrix3dv( GLint location, GLsizei count, GLboolean transpose, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX3DVPROC >( "glUniformMatrix3dv", out _glUniformMatrix3dv );

        _glUniformMatrix3dv( location, count, transpose, value );
    }

    public void UniformMatrix3dv( GLint location, GLboolean transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX3DVPROC >( "glUniformMatrix3dv", out _glUniformMatrix3dv );

        fixed ( GLdouble* p = &value[ 0 ] )
        {
            _glUniformMatrix3dv( location, value.Length / 9, transpose, p );
        }
    }

    // ========================================================================

    public void UniformMatrix4dv( GLint location, GLsizei count, GLboolean transpose, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX4DVPROC >( "glUniformMatrix4dv", out _glUniformMatrix4dv );

        _glUniformMatrix4dv( location, count, transpose, value );
    }

    public void UniformMatrix4dv( GLint location, GLboolean transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX4DVPROC >( "glUniformMatrix4dv", out _glUniformMatrix4dv );

        fixed ( GLdouble* p = &value[ 0 ] )
        {
            _glUniformMatrix4dv( location, value.Length / 16, transpose, p );
        }
    }

    // ========================================================================

    public void UniformMatrix2x3dv( GLint location, GLsizei count, GLboolean transpose, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX2X3DVPROC >( "glUniformMatrix2x3dv", out _glUniformMatrix2x3dv );

        _glUniformMatrix2x3dv( location, count, transpose, value );
    }

    public void UniformMatrix2x3dv( GLint location, GLboolean transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX2X3DVPROC >( "glUniformMatrix2x3dv", out _glUniformMatrix2x3dv );

        fixed ( GLdouble* p = &value[ 0 ] )
        {
            _glUniformMatrix2x3dv( location, value.Length / 6, transpose, p );
        }
    }

    // ========================================================================

    public void UniformMatrix2x4dv( GLint location, GLsizei count, GLboolean transpose, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX2X4DVPROC >( "glUniformMatrix2x4dv", out _glUniformMatrix2x4dv );

        _glUniformMatrix2x4dv( location, count, transpose, value );
    }

    public void UniformMatrix2x4dv( GLint location, GLboolean transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX2X4DVPROC >( "glUniformMatrix2x4dv", out _glUniformMatrix2x4dv );

        fixed ( GLdouble* p = &value[ 0 ] )
        {
            _glUniformMatrix2x4dv( location, value.Length / 8, transpose, p );
        }
    }

    // ========================================================================

    public void UniformMatrix3x2dv( GLint location, GLsizei count, GLboolean transpose, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX3X2DVPROC >( "glUniformMatrix3x2dv", out _glUniformMatrix3x2dv );

        _glUniformMatrix3x2dv( location, count, transpose, value );
    }

    public void UniformMatrix3x2dv( GLint location, GLboolean transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX3X2DVPROC >( "glUniformMatrix3x2dv", out _glUniformMatrix3x2dv );

        fixed ( GLdouble* p = &value[ 0 ] )
        {
            _glUniformMatrix3x2dv( location, value.Length / 6, transpose, p );
        }
    }

    // ========================================================================

    public void UniformMatrix3x4dv( GLint location, GLsizei count, GLboolean transpose, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX3X4DVPROC >( "glUniformMatrix3x4dv", out _glUniformMatrix3x4dv );

        _glUniformMatrix3x4dv( location, count, transpose, value );
    }

    public void UniformMatrix3x4dv( GLint location, GLboolean transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX3X4DVPROC >( "glUniformMatrix3x4dv", out _glUniformMatrix3x4dv );

        fixed ( GLdouble* p = &value[ 0 ] )
        {
            _glUniformMatrix3x4dv( location, value.Length / 12, transpose, p );
        }
    }

    // ========================================================================

    public void UniformMatrix4x2dv( GLint location, GLsizei count, GLboolean transpose, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX4X2DVPROC >( "glUniformMatrix4x2dv", out _glUniformMatrix4x2dv );

        _glUniformMatrix4x2dv( location, count, transpose, value );
    }

    public void UniformMatrix4x2dv( GLint location, GLboolean transpose, GLdouble[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX4X2DVPROC >( "glUniformMatrix4x2dv", out _glUniformMatrix4x2dv );

        fixed ( GLdouble* p = &value[ 0 ] )
        {
            _glUniformMatrix4x2dv( location, value.Length / 8, transpose, p );
        }
    }

    // ========================================================================

    public void UniformMatrix4x3dv( GLint location, GLsizei count, GLboolean transpose, GLdouble* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX4X3DVPROC >( "glUniformMatrix4x3dv", out _glUniformMatrix4x3dv );

        _glUniformMatrix4x3dv( location, count, transpose, value );
    }

    public void UniformMatrix4x3dv( GLint location, GLboolean transpose, GLdouble[] value )
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

        _glGetUniformdv( ( uint )program, location, parameters );
    }

    public void GetUniformdv( GLuint program, GLint location, ref GLdouble[] parameters )
    {
        GetDelegateForFunction< PFNGLGETUNIFORMDVPROC >( "glGetUniformdv", out _glGetUniformdv );

        fixed ( GLdouble* p = &parameters[ 0 ] )
        {
            _glGetUniformdv( ( uint )program, location, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetUniformIndices( GLuint program, GLsizei uniformCount, GLchar** uniformNames, GLuint* uniformIndices )
    {
        GetDelegateForFunction< PFNGLGETUNIFORMINDICESPROC >( "glGetUniformIndices", out _glGetUniformIndices );

        _glGetUniformIndices( ( uint )program, uniformCount, uniformNames, uniformIndices );
    }

    /// <inheritdoc />
    public GLuint[] GetUniformIndices( GLuint program, params string[] uniformNames )
    {
        var uniformCount     = uniformNames.Length;
        var uniformNamesPtrs = new GLchar[ uniformCount ][];

        for ( var i = 0; i < uniformCount; i++ )
        {
            uniformNamesPtrs[ i ] = Encoding.UTF8.GetBytes( uniformNames[ i ] );
        }

        {
            var pUniformNames = stackalloc GLchar*[ uniformCount ];

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
                _glGetUniformIndices( ( uint )program, uniformCount, pUniformNames, p );
            }

            return uniformIndices;
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetActiveUniformsiv( GLuint program, GLsizei uniformCount, GLuint* uniformIndices, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETACTIVEUNIFORMSIVPROC >( "glGetActiveUniformsiv", out _glGetActiveUniformsiv );

        _glGetActiveUniformsiv( ( uint )program, uniformCount, uniformIndices, pname, parameters );
    }

    /// <inheritdoc />
    public GLint[] GetActiveUniformsiv( GLuint program, GLenum pname, params GLuint[] uniformIndices )
    {
        var uniformCount = uniformIndices.Length;
        var parameters   = new GLint[ uniformCount ];

        GetDelegateForFunction< PFNGLGETACTIVEUNIFORMSIVPROC >( "glGetActiveUniformsiv", out _glGetActiveUniformsiv );

        {
            fixed ( GLuint* p = &uniformIndices[ 0 ] )
            {
                fixed ( GLint* pParameters = &parameters[ 0 ] )
                {
                    _glGetActiveUniformsiv( ( uint )program, uniformCount, p, pname, pParameters );
                }
            }
        }

        return parameters;
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetActiveUniformName( GLuint program, GLuint uniformIndex, GLsizei bufSize, GLsizei* length, GLchar* uniformName )
    {
        GetDelegateForFunction< PFNGLGETACTIVEUNIFORMNAMEPROC >( "glGetActiveUniformName", out _glGetActiveUniformName );

        _glGetActiveUniformName( ( uint )program, uniformIndex, bufSize, length, uniformName );
    }

    /// <inheritdoc />
    public string GetActiveUniformName( GLuint program, GLuint uniformIndex, GLsizei bufSize )
    {
        var     uniformName = stackalloc GLchar[ bufSize ];
        GLsizei length;

        GetDelegateForFunction< PFNGLGETACTIVEUNIFORMNAMEPROC >( "glGetActiveUniformName", out _glGetActiveUniformName );

        _glGetActiveUniformName( ( uint )program, uniformIndex, bufSize, &length, uniformName );

        return new string( ( GLbyte* )uniformName, 0, length, Encoding.UTF8 );
    }

    // ========================================================================

    /// <inheritdoc />
    public GLuint GetUniformBlockIndex( GLuint program, GLchar* uniformBlockName )
    {
        GetDelegateForFunction< PFNGLGETUNIFORMBLOCKINDEXPROC >( "glGetUniformBlockIndex", out _glGetUniformBlockIndex );

        return _glGetUniformBlockIndex( ( uint )program, uniformBlockName );
    }

    /// <inheritdoc />
    public GLuint GetUniformBlockIndex( GLuint program, string uniformBlockName )
    {
        var uniformBlockNameBytes = Encoding.UTF8.GetBytes( uniformBlockName );

        GetDelegateForFunction< PFNGLGETUNIFORMBLOCKINDEXPROC >( "glGetUniformBlockIndex", out _glGetUniformBlockIndex );

        {
            fixed ( GLchar* p = &uniformBlockNameBytes[ 0 ] )
            {
                return _glGetUniformBlockIndex( ( uint )program, p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetActiveUniformBlockiv( GLuint program, GLuint uniformBlockIndex, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETACTIVEUNIFORMBLOCKIVPROC >( "glGetActiveUniformBlockiv", out _glGetActiveUniformBlockiv );

        _glGetActiveUniformBlockiv( ( uint )program, uniformBlockIndex, pname, parameters );
    }

    /// <inheritdoc />
    public void GetActiveUniformBlockiv( GLuint program, GLuint uniformBlockIndex, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETACTIVEUNIFORMBLOCKIVPROC >( "glGetActiveUniformBlockiv", out _glGetActiveUniformBlockiv );

        {
            fixed ( GLint* p = &parameters[ 0 ] )
            {
                _glGetActiveUniformBlockiv( ( uint )program, uniformBlockIndex, pname, p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetActiveUniformBlockName( GLuint program, GLuint uniformBlockIndex, GLsizei bufSize, GLsizei* length,
                                           GLchar* uniformBlockName )
    {
        GetDelegateForFunction< PFNGLGETACTIVEUNIFORMBLOCKNAMEPROC >( "glGetActiveUniformBlockName", out _glGetActiveUniformBlockName );

        _glGetActiveUniformBlockName( ( uint )program, uniformBlockIndex, bufSize, length, uniformBlockName );
    }

    // ========================================================================

    /// <inheritdoc />
    public string GetActiveUniformBlockName( GLuint program, GLuint uniformBlockIndex, GLsizei bufSize )
    {
        var uniformBlockName = stackalloc GLchar[ bufSize ];

        GLsizei length;

        GetDelegateForFunction< PFNGLGETACTIVEUNIFORMBLOCKNAMEPROC >( "glGetActiveUniformBlockName", out _glGetActiveUniformBlockName );

        _glGetActiveUniformBlockName( ( uint )program, uniformBlockIndex, bufSize, &length, uniformBlockName );

        return new string( ( GLbyte* )uniformBlockName, 0, length, Encoding.UTF8 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void UniformBlockBinding( GLuint program, GLuint uniformBlockIndex, GLuint uniformBlockBinding )
    {
        GetDelegateForFunction< PFNGLUNIFORMBLOCKBINDINGPROC >( "glUniformBlockBinding", out _glUniformBlockBinding );

        _glUniformBlockBinding( ( uint )program, uniformBlockIndex, uniformBlockBinding );
    }

    // ========================================================================

    public void UniformSubroutinesuiv( GLenum shadertype, GLsizei count, GLuint* indices )
    {
        GetDelegateForFunction< PFNGLUNIFORMSUBROUTINESUIVPROC >( "glUniformSubroutinesuiv", out _glUniformSubroutinesuiv );

        _glUniformSubroutinesuiv( shadertype, count, indices );
    }

    public void UniformSubroutinesuiv( GLenum shadertype, GLuint[] indices )
    {
        GetDelegateForFunction< PFNGLUNIFORMSUBROUTINESUIVPROC >( "glUniformSubroutinesuiv", out _glUniformSubroutinesuiv );

        fixed ( GLuint* p = &indices[ 0 ] )
        {
            _glUniformSubroutinesuiv( shadertype, indices.Length, p );
        }
    }

    // ========================================================================

    public void GetUniformSubroutineuiv( GLenum shadertype, GLint location, GLuint* parameters )
    {
        GetDelegateForFunction< PFNGLGETUNIFORMSUBROUTINEUIVPROC >( "glGetUniformSubroutineuiv", out _glGetUniformSubroutineuiv );

        _glGetUniformSubroutineuiv( shadertype, location, parameters );
    }

    public void GetUniformSubroutineuiv( GLenum shadertype, GLint location, ref GLuint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETUNIFORMSUBROUTINEUIVPROC >( "glGetUniformSubroutineuiv", out _glGetUniformSubroutineuiv );

        fixed ( GLuint* p = &parameters[ 0 ] )
        {
            _glGetUniformSubroutineuiv( shadertype, location, p );
        }
    }
}

// ============================================================================
// ============================================================================
