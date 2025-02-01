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

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Maths;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Buffers;

namespace LughSharp.Lugh.Graphics.GLUtils;

public partial class ManagedShaderProgram
{
    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    /// <param name="matrix"></param>
    /// <param name="transpose"></param>
    public virtual void SetUniformMatrix( int location, Matrix4 matrix, bool transpose )
    {
        GdxApi.Bindings.UniformMatrix4fv( location, transpose, matrix.Val );
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    /// <param name="matrix"></param>
    /// <param name="transpose"></param>
    public virtual void SetUniformMatrix( int location, Matrix3 matrix, bool transpose )
    {
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
    public virtual unsafe void SetUniformMatrix3Fv( string name, FloatBuffer buffer, int count, bool transpose )
    {
        buffer.Position = 0;

        fixed ( float* ptr = &( buffer ).BackingArray()[ 0 ] )
        {
            GdxApi.Bindings.UniformMatrix3fv( GdxApi.Bindings.GetUniformLocation( ( uint )Handle, name ),
                                              count,
                                              transpose,
                                              ptr );
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
    public virtual unsafe void SetUniformMatrix4Fv( string name, FloatBuffer buffer, int count, bool transpose )
    {
        ArgumentNullException.ThrowIfNull( buffer );

        buffer.Position = 0;

        fixed ( float* ptr = &buffer.BackingArray()[ 0 ] )
        {
            GdxApi.Bindings.UniformMatrix4fv( GdxApi.Bindings.GetUniformLocation( ( uint )Handle, name ),
                                              count,
                                              transpose,
                                              ptr );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    /// <param name="values"></param>
    public virtual void SetUniformMatrix4Fv( int location, params float[] values )
    {
        Logger.Debug( $"location: {location}" );
        Logger.Debug( $"values: {string.Join( ", ", values )}" );

        GdxApi.Bindings.UniformMatrix4fv( location, false, values );
    }

    /// <summary>
    /// Sets the uniform with the given name. The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="values"> x and y as the first and second values respectively  </param>
    public virtual void SetUniformf( string name, Vector2 values )
    {
        GdxApi.Bindings.Uniform2f( FetchUniformLocation( name ),
                                   values.X,
                                   values.Y );
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    /// <param name="values"></param>
    public virtual void SetUniformf( int location, Vector2 values )
    {
        GdxApi.Bindings.Uniform2f( location, values.X, values.Y );
    }

    /// <summary>
    /// Sets the uniform with the given name. The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="values"> x, y and z as the first, second and third values respectively  </param>
    public virtual void SetUniformf( string name, Vector3 values )
    {
        GdxApi.Bindings.Uniform3f( FetchUniformLocation( name ), values.X, values.Y, values.Z );
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    /// <param name="values"></param>
    public virtual void SetUniformf( int location, Vector3 values )
    {
        GdxApi.Bindings.Uniform3f( location, values.X, values.Y, values.Z );
    }

    /// <summary>
    /// Sets the uniform with the given name. The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="values"> r, g, b and a as the first through fourth values respectively  </param>
    public virtual void SetUniformf( string name, Color values )
    {
        GdxApi.Bindings.Uniform4f( FetchUniformLocation( name ), values.R, values.G, values.B, values.A );
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    /// <param name="values"></param>
    public virtual void SetUniformf( int location, Color values )
    {
        GdxApi.Bindings.Uniform4f( location, values.R, values.G, values.B, values.A );
    }
}