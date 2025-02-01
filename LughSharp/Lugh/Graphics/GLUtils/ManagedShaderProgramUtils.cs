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

using Buffer = LughSharp.Lugh.Utils.Buffers.Buffer;

namespace LughSharp.Lugh.Graphics.GLUtils;

public partial class ManagedShaderProgram
{
    /// <summary>
    /// Sets the vertex attribute with the given name.
    /// The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <param name="size"> The number of components, must be >= 1 and &lt;= 4. </param>
    /// <param name="type">
    /// The type, must be one of IGL.GL_BYTE, IGL.GL_UNSIGNED_BYTE, IGL.GL_SHORT, IGL.GL_UNSIGNED_SHORT,
    /// IGL.GL_FIXED, or IGL.GL_FLOAT.
    /// </param>
    /// <param name="normalize"> Whether fixed point data should be normalized. Will not work on the desktop. </param>
    /// <param name="stride">The stride in bytes between successive attributes.</param>
    /// <param name="buffer">The buffer containing the vertex attributes.</param>
    public virtual unsafe void SetVertexAttribute( string name, int size, int type, bool normalize, int stride, Buffer buffer )
    {
        var location = FetchAttributeLocation( name );
        
        if ( location == -1 )
        {
            return;
        }

        fixed ( void* ptr = &buffer.BackingArray()[ 0 ] )
        {
            GdxApi.Bindings.VertexAttribPointer( ( uint )location, size, type, normalize, stride, ptr );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    /// <param name="size"></param>
    /// <param name="type"></param>
    /// <param name="normalize"></param>
    /// <param name="stride"></param>
    /// <param name="buffer"></param>
    public virtual unsafe void SetVertexAttribute( int location, int size, int type, bool normalize, int stride, Buffer buffer )
    {
        fixed ( void* ptr = &buffer.BackingArray()[ 0 ] )
        {
            GdxApi.Bindings.VertexAttribPointer( ( uint )location, size, type, normalize, stride, ptr );
        }
    }

    /// <summary>
    /// Sets the vertex attribute with the given name. The <see cref="ShaderProgram"/> must
    /// be bound for this to work.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <param name="size">The number of components, must be >= 1 and &lt;= 4.</param>
    /// <param name="type">
    /// The type, must be one of IGL.GL_Byte, IGL.GL_Unsigned_Byte, IGL.GL_Short, IGL.GL_Unsigned_Short,
    /// IGL.GL_Fixed, or IGL.GL_Float.
    /// <para>GL_Fixed will not work on the desktop.</para>
    /// </param>
    /// <param name="normalize"> Whether fixed point data should be normalized. Will not work on the desktop.</param>
    /// <param name="stride">The stride in bytes between successive attributes.</param>
    /// <param name="offset"> Byte offset into the vertex buffer object bound to IGL.GL_Array_Buffer. </param>
    public virtual void SetVertexAttribute( string name, int size, int type, bool normalize, int stride, int offset )
    {
        var location = FetchAttributeLocation( name );
        
        if ( location == -1 )
        {
            return;
        }

        GdxApi.Bindings.VertexAttribPointer( ( uint )location, size, type, normalize, stride, ( uint )offset );
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    /// <param name="size"></param>
    /// <param name="type"></param>
    /// <param name="normalize"></param>
    /// <param name="stride"></param>
    /// <param name="offset"></param>
    public virtual void SetVertexAttribute( int location, int size, int type, bool normalize, int stride, int offset )
    {
        GdxApi.Bindings.VertexAttribPointer( ( uint )location, size, type, normalize, stride, ( uint )offset );
    }

    /// <summary>
    /// Disables the vertex attribute with the given name
    /// </summary>
    /// <param name="name"> the vertex attribute name  </param>
    public virtual void DisableVertexAttribute( string name )
    {
        var location = FetchAttributeLocation( name );
        
        if ( location == -1 )
        {
            return;
        }

        GdxApi.Bindings.DisableVertexAttribArray( ( uint )location );
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    public virtual void DisableVertexAttribute( int location )
    {
        GdxApi.Bindings.DisableVertexAttribArray( ( uint )location );
    }

    /// <summary>
    /// Enables the vertex attribute with the given name
    /// </summary>
    /// <param name="name"> the vertex attribute name  </param>
    public virtual void EnableVertexAttribute( string name )
    {
        var location = FetchAttributeLocation( name );

        if ( location == -1 )
        {
            return;
        }

        GdxApi.Bindings.EnableVertexAttribArray( ( uint )location );
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    public virtual void EnableVertexAttribute( int location )
    {
        GdxApi.Bindings.EnableVertexAttribArray( ( uint )location );
    }

    /// <summary>
    /// Sets the given attribute
    /// </summary>
    /// <param name="name"> the name of the attribute </param>
    /// <param name="value1"> the first value </param>
    /// <param name="value2"> the second value </param>
    /// <param name="value3"> the third value </param>
    /// <param name="value4"> the fourth value  </param>
    public virtual void SetAttributef( string name, float value1, float value2, float value3, float value4 )
    {
        GdxApi.Bindings.VertexAttrib4f( ( uint )FetchAttributeLocation( name ), value1, value2, value3, value4 );
    }
}

