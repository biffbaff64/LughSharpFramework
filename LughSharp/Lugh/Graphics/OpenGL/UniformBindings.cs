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

using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;

using GLfloat = float;
using GLint = int;
using GLsizei = int;
using GLuint = uint;
using GLboolean = bool;
using GLchar = byte;

// ============================================================================

namespace LughSharp.Lugh.Graphics.OpenGL;

public unsafe partial class GLBindings
{
    /// <inheritdoc />
    public GLint GetUniformLocation( GLint program, GLchar* name )
    {
        if ( !GdxApi.Bindings.IsProgram( program ) || ( program == -1 ) )
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
        if ( !GdxApi.Bindings.IsProgram( program ) || ( program == -1 ) )
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
        if ( !GdxApi.Bindings.IsProgram( program ) || ( program == -1 ) )
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
        if ( !GdxApi.Bindings.IsProgram( program ) || ( program == -1 ) )
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
        if ( !GdxApi.Bindings.IsProgram( program ) || ( program == -1 ) )
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
        if ( !GdxApi.Bindings.IsProgram( program ) || ( program == -1 ) )
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
    /// Specifies a pointer to an array of <paramref name="count" /> values that will be used to update the
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
        ArgumentNullException.ThrowIfNull( value );

        GetDelegateForFunction< PFNGLUNIFORMMATRIX4FVPROC >( "glUniformMatrix4fv", out _glUniformMatrix4fv );

        if ( ( value.Length % 16 ) != 0 )
        {
            throw new GdxRuntimeException( $"Error: value array length ({value.Length}) is not a multiple " +
                                           $"of 16.  Must provide a whole number of 4x4 matrices." );
        }

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
}

