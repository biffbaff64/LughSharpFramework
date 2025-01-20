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

using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Maths;
using LughSharp.Lugh.Utils.Buffers;
using LughSharp.Lugh.Utils.Collections;

namespace LughSharp.Lugh.Graphics.GLUtils;

public partial class ShaderProgram
{
    /// <param name="name"> the name of the uniform.</param>
    /// <returns> whether the uniform is available in the shader.</returns>
    public bool HasUniform( string name )
    {
        return _uniforms.ContainsKey( name );
    }

    /// <param name="name"> the name of the uniform </param>
    /// <returns>
    /// the type of the uniform, one of <see cref="IGL.GL_FLOAT"/>,
    /// <see cref="IGL.GL_FLOAT_VEC2"/> etc.
    /// </returns>
    public int GetUniformType( string name )
    {
        return _uniformTypes.GetValueOrDefault( name, 0 );
    }

    /// <param name="name"> the name of the uniform </param>
    /// <returns> the location of the uniform or -1.</returns>
    public int GetUniformLocation( string name )
    {
        return _uniforms.GetValueOrDefault( name, -1 );
    }

    /// <param name="name">The name of the uniform</param>
    /// <returns> the size of the uniform or 0.</returns>
    public int GetUniformSize( string name )
    {
        return _uniformSizes.GetValueOrDefault( name, 0 );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private int FetchUniformLocation( string name )
    {
        return FetchUniformLocation( name, Pedantic );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private int FetchAttributeLocation( string name )
    {
        // -2 == not yet cached
        // -1 == cached but not found
        int location;

        if ( ( location = _attributes.Get( name, NOT_CACHED ) ) == NOT_CACHED )
        {
            location            = GdxApi.Bindings.GetAttribLocation( ( uint )Handle, name );
            _attributes[ name ] = location;
        }

        return location;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="pedant"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public int FetchUniformLocation( string name, bool pedant )
    {
        // -2 == not yet cached
        // -1 == cached but not found
        int location;

        if ( ( location = _uniforms.Get( name, NOT_CACHED ) ) == NOT_CACHED )
        {
            location = GdxApi.Bindings.GetUniformLocation( ( uint )Handle, name );

            if ( ( location == CACHED_NOT_FOUND ) && pedant )
            {
                if ( IsCompiled )
                {
                    throw new ArgumentException( "No uniform with name '" + name + "' in shader" );
                }

                throw new InvalidOperationException( "An attempted fetch uniform from uncompiled shader \n" + ShaderLog );
            }

            _uniforms[ name ] = location;
        }

        return location;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="matrix"></param>
    /// <param name="transpose"></param>
    /// <typeparam name="T"></typeparam>
    public void SetUniformMatrix< T >( string name, ref T matrix, bool transpose = false )
        where T : unmanaged
    {
        var location = GetUniformLocation( name );

        SetUniformMatrix( location, ref matrix, transpose );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="matrix"></param>
    /// <param name="transpose"></param>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="ArgumentException"></exception>
    public unsafe void SetUniformMatrix< T >( int location, ref T matrix, bool transpose = false )
        where T : unmanaged
    {
        const int MAT4X4 = 16;
        const int MAT3X3 = 9;
        const int MAT2X2 = 4;

        if ( location == -1 ) return;

        var matrixSize = Marshal.SizeOf< T >() / sizeof( float );

        CheckManaged();

        fixed ( T* ptr = &matrix )
        {
            if ( matrixSize == MAT4X4 )
            {
                GdxApi.Bindings.UniformMatrix4fv( location, 1, transpose, ( float* )ptr );
            }
            else if ( matrixSize == MAT3X3 )
            {
                GdxApi.Bindings.UniformMatrix3fv( location, 1, transpose, ( float* )ptr );
            }
            else if ( matrixSize == MAT2X2 )
            {
                GdxApi.Bindings.UniformMatrix2fv( location, 1, transpose, ( float* )ptr );
            }
            else
            {
                throw new ArgumentException( "Matrix must be 2x2, 3x3 or 4x4" );
            }
        }
    }

    /// <summary>
    /// Sets the uniform with the given name. The <see cref="ShaderProgram"/>
    /// must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="value"> the value  </param>
    public void SetUniformi( string name, int value )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform1i( FetchUniformLocation( name ), value );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="value"></param>
    public void SetUniformi( int location, int value )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform1i( location, value );
    }

    /// <summary>
    /// Sets the uniform with the given name. The <see cref="ShaderProgram"/>
    /// must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="count"> the first value </param>
    /// <param name="value"> the second value </param>
    public void SetUniformi( string name, int count, int value )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform2i( FetchUniformLocation( name ), count, value );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="count"></param>
    /// <param name="value"></param>
    public void SetUniformi( int location, int count, int value )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform2i( location, count, value );
    }

    /// <summary>
    /// Sets the uniform with the given name.
    /// The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="value1"> the first value </param>
    /// <param name="value2"> the second value </param>
    /// <param name="value3"> the third value </param>
    public void SetUniformi( string name, int value1, int value2, int value3 )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform3i( FetchUniformLocation( name ), value1, value2, value3 );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public void SetUniformi( int location, int x, int y, int z )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform3i( location, x, y, z );
    }

    /// <summary>
    /// Sets the uniform with the given name.
    /// The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="value"> the value  </param>
    public void SetUniformf( string name, float value )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform1f( FetchUniformLocation( name ), value );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="value"></param>
    public void SetUniformf( int location, float value )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform1f( location, value );
    }

    /// <summary>
    /// Sets the uniform with the given name.
    /// The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="value1"> the first value </param>
    /// <param name="value2"> the second value  </param>
    public void SetUniformf( string name, float value1, float value2 )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform2f( FetchUniformLocation( name ), value1, value2 );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    public void SetUniformf( int location, int value1, int value2 )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform2f( location, value1, value2 );
    }

    /// <summary>
    /// Sets the uniform with the given name.
    /// The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="value1"> the first value </param>
    /// <param name="value2"> the second value </param>
    /// <param name="value3"> the third value  </param>
    public void SetUniformf( string name, float value1, float value2, float value3 )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform3f( FetchUniformLocation( name ), value1, value2, value3 );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    /// <param name="value3"></param>
    public void SetUniformf( int location, float value1, float value2, float value3 )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform3f( location, value1, value2, value3 );
    }

    /// <summary>
    /// Sets the uniform with the given name. The <see cref="ShaderProgram"/>
    /// must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="w"></param>
    public void SetUniformf( string name, float x, float y, float z, float w )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform4f( FetchUniformLocation( name ), x, y, z, w );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="w"></param>
    public void SetUniformf( int location, float x, float y, float z, float w )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform4f( location, x, y, z, w );
    }

//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="name"></param>
//    /// <param name="value"></param>
//    public void SetUniform1Fv( string name, params float[] value )
//    {
//        CheckManaged();
//        GdxApi.Bindings.Uniform1fv( FetchUniformLocation( name ), value );
//    }
//
//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="location"></param>
//    /// <param name="value"></param>
//    public void SetUniform1Fv( int location, params float[] value )
//    {
//        CheckManaged();
//        GdxApi.Bindings.Uniform1fv( location, value );
//    }
//
//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="name"></param>
//    /// <param name="values"></param>
//    public void SetUniform2Fv( string name, params float[] values )
//    {
//        CheckManaged();
//        GdxApi.Bindings.Uniform2fv( FetchUniformLocation( name ), values );
//    }
//
//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="location"></param>
//    /// <param name="values"></param>
//    public void SetUniform2Fv( int location, params float[] values )
//    {
//        CheckManaged();
//        GdxApi.Bindings.Uniform2fv( location, values );
//    }
//
//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="name"></param>
//    /// <param name="values"></param>
//    public void SetUniform3Fv( string name, params float[] values )
//    {
//        CheckManaged();
//        GdxApi.Bindings.Uniform3fv( FetchUniformLocation( name ), values );
//    }
//
//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="location"></param>
//    /// <param name="values"></param>
//    public void SetUniform3Fv( int location, params float[] values )
//    {
//        CheckManaged();
//        GdxApi.Bindings.Uniform3fv( location, values );
//    }
//
//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="name"></param>
//    /// <param name="values"></param>
//    public void SetUniform4Fv( string name, params float[] values )
//    {
//        CheckManaged();
//        GdxApi.Bindings.Uniform4fv( FetchUniformLocation( name ), values );
//    }
//
//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="location"></param>
//    /// <param name="values"></param>
//    public void SetUniform4Fv( int location, params float[] values )
//    {
//        CheckManaged();
//        GdxApi.Bindings.Uniform4fv( location, values );
//    }

    /// <summary>
    /// Sets the uniform matrix with the given name. The <see cref="ShaderProgram"/>
    /// must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="matrix"> the matrix  </param>
    public void SetUniformMatrix( string name, Matrix4 matrix )
    {
        SetUniformMatrix( name, matrix, false );
    }

    /// <summary>
    /// Sets the uniform matrix with the given name. The <see cref="ShaderProgram"/>
    /// must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="matrix"> the matrix </param>
    /// <param name="transpose"> whether the matrix should be transposed  </param>
    public void SetUniformMatrix( string name, Matrix4 matrix, bool transpose )
    {
        SetUniformMatrix( FetchUniformLocation( name ), matrix, transpose );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="matrix"></param>
    public void SetUniformMatrix( int location, Matrix4 matrix )
    {
        SetUniformMatrix( location, matrix, false );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="matrix"></param>
    /// <param name="transpose"></param>
    public void SetUniformMatrix( int location, Matrix4 matrix, bool transpose )
    {
        CheckManaged();
        GdxApi.Bindings.UniformMatrix4fv( location, transpose, matrix.Val );
    }

    /// <summary>
    /// Sets the uniform matrix with the given name.
    /// The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="matrix"> the matrix  </param>
    public void SetUniformMatrix( string name, Matrix3 matrix )
    {
        SetUniformMatrix( name, matrix, false );
    }

    /// <summary>
    /// Sets the uniform matrix with the given name.
    /// The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="matrix"> the matrix </param>
    /// <param name="transpose"> whether the uniform matrix should be transposed  </param>
    public void SetUniformMatrix( string name, Matrix3 matrix, bool transpose )
    {
        SetUniformMatrix( FetchUniformLocation( name ), matrix, transpose );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="matrix"></param>
    public void SetUniformMatrix( int location, Matrix3 matrix )
    {
        SetUniformMatrix( location, matrix, false );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="matrix"></param>
    /// <param name="transpose"></param>
    public void SetUniformMatrix( int location, Matrix3 matrix, bool transpose )
    {
        CheckManaged();
        GdxApi.Bindings.UniformMatrix3fv( location, transpose, matrix.Val );
    }

    /// <summary>
    /// Sets an array of uniform matrices with the given name.
    /// The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name">the name of the uniform </param>
    /// <param name="buffer">buffer containing the matrix data </param>
    /// <param name="count"></param>
    /// <param name="transpose">whether the uniform matrix should be transposed  </param>
    public unsafe void SetUniformMatrix3Fv( string name, FloatBuffer buffer, int count, bool transpose )
    {
        CheckManaged();
        buffer.Position = 0;

        fixed ( float* ptr = &( buffer ).BackingArray()[ 0 ] )
        {
            GdxApi.Bindings.UniformMatrix3fv( FetchUniformLocation( name ), count, transpose, ptr );
        }
    }

    /// <summary>
    /// Sets an array of uniform matrices with the given name. The <see cref="ShaderProgram"/>
    /// must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="buffer"> buffer containing the matrix data </param>
    /// <param name="count"></param>
    /// <param name="transpose"> whether the uniform matrix should be transposed  </param>
    public unsafe void SetUniformMatrix4Fv( string name, FloatBuffer buffer, int count, bool transpose )
    {
        CheckManaged();
        buffer.Position = 0;

        fixed ( float* ptr = &buffer.BackingArray()[ 0 ] )
        {
            GdxApi.Bindings.UniformMatrix4fv( FetchUniformLocation( name ), count, transpose, ptr );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="values"></param>
    public void SetUniformMatrix4Fv( int location, params float[] values )
    {
        CheckManaged();
        GdxApi.Bindings.UniformMatrix4fv( location, false, values );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="values"></param>
    public void SetUniformMatrix4Fv( string name, params float[] values )
    {
        SetUniformMatrix4Fv( FetchUniformLocation( name ), values );
    }

    /// <summary>
    /// Sets the uniform with the given name. The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="values"> x and y as the first and second values respectively  </param>
    public void SetUniformf( string name, Vector2 values )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform2f( FetchUniformLocation( name ), values.X, values.Y );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="values"></param>
    public void SetUniformf( int location, Vector2 values )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform2f( location, values.X, values.Y );
    }

    /// <summary>
    /// Sets the uniform with the given name. The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="values"> x, y and z as the first, second and third values respectively  </param>
    public void SetUniformf( string name, Vector3 values )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform3f( FetchUniformLocation( name ), values.X, values.Y, values.Z );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="values"></param>
    public void SetUniformf( int location, Vector3 values )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform3f( location, values.X, values.Y, values.Z );
    }

    /// <summary>
    /// Sets the uniform with the given name. The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="values"> r, g, b and a as the first through fourth values respectively  </param>
    public void SetUniformf( string name, Color values )
    {
        GdxApi.Bindings.Uniform4f( FetchUniformLocation( name ), values.R, values.G, values.B, values.A );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="values"></param>
    public void SetUniformf( int location, Color values )
    {
        GdxApi.Bindings.Uniform4f( location, values.R, values.G, values.B, values.A );
    }
}