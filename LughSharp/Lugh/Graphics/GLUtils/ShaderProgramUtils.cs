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

namespace LughSharp.Lugh.Graphics.GLUtils;

public partial class ShaderProgram
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
    public unsafe void SetVertexAttribute( string name, int size, int type, bool normalize, int stride, Buffer buffer )
    {
        CheckManaged();

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
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="size"></param>
    /// <param name="type"></param>
    /// <param name="normalize"></param>
    /// <param name="stride"></param>
    /// <param name="buffer"></param>
    public unsafe void SetVertexAttribute( int location, int size, int type, bool normalize, int stride, Buffer buffer )
    {
        CheckManaged();

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
    public void SetVertexAttribute( string name, int size, int type, bool normalize, int stride, int offset )
    {
        CheckManaged();

        var location = FetchAttributeLocation( name );

        if ( location == -1 )
        {
            return;
        }

        GdxApi.Bindings.VertexAttribPointer( ( uint )location, size, type, normalize, stride, ( uint )offset );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="size"></param>
    /// <param name="type"></param>
    /// <param name="normalize"></param>
    /// <param name="stride"></param>
    /// <param name="offset"></param>
    public void SetVertexAttribute( int location, int size, int type, bool normalize, int stride, int offset )
    {
        CheckManaged();
        GdxApi.Bindings.VertexAttribPointer( ( uint )location, size, type, normalize, stride, ( uint )offset );
    }

    /// <summary>
    /// Disables the vertex attribute with the given name
    /// </summary>
    /// <param name="name"> the vertex attribute name  </param>
    public void DisableVertexAttribute( string name )
    {
        CheckManaged();

        var location = FetchAttributeLocation( name );

        if ( location == -1 )
        {
            return;
        }

        GdxApi.Bindings.DisableVertexAttribArray( ( uint )location );
    }

    public void DisableVertexAttribute( int location )
    {
        CheckManaged();
        GdxApi.Bindings.DisableVertexAttribArray( ( uint )location );
    }

    /// <summary>
    /// Enables the vertex attribute with the given name
    /// </summary>
    /// <param name="name"> the vertex attribute name  </param>
    public void EnableVertexAttribute( string name )
    {
        CheckManaged();

        var location = FetchAttributeLocation( name );

        if ( location == -1 )
        {
            return;
        }

        GdxApi.Bindings.EnableVertexAttribArray( ( uint )location );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    public void EnableVertexAttribute( int location )
    {
        CheckManaged();
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
    public void SetAttributef( string name, float value1, float value2, float value3, float value4 )
    {
        GdxApi.Bindings.VertexAttrib4f( ( uint )FetchAttributeLocation( name ), value1, value2, value3, value4 );
    }

    /// <summary>
    /// </summary>
    /// <param name="name"> the name of the attribute </param>
    /// <returns> whether the attribute is available in the shader  </returns>
    public bool HasAttribute( string name )
    {
        return _attributes.ContainsKey( name );
    }

    /// <param name="name"> the name of the attribute </param>
    /// <returns>
    /// the type of the attribute, one of <see cref="IGL.GL_FLOAT"/>,
    /// <see cref="IGL.GL_FLOAT_VEC2"/> etc.
    /// </returns>
    public int GetAttributeType( string name )
    {
        return _attributeTypes.GetValueOrDefault( name, 0 );
    }

    /// <param name="name"> the name of the attribute </param>
    /// <returns> the location of the attribute or -1.  </returns>
    public int GetAttributeLocation( string name )
    {
        return _attributes.GetValueOrDefault( name, 0 );
    }

    /// <param name="name"> the name of the attribute </param>
    /// <returns> the size of the attribute or 0.</returns>
    public int GetAttributeSize( string name )
    {
        return _attributeSizes.GetValueOrDefault( name, 0 );
    }

    /// <summary>
    /// </summary>
    private unsafe void FetchUniforms()
    {
        var numUniforms = stackalloc int[ 1 ];

        GdxApi.Bindings.GetProgramiv( ( uint )Handle, IGL.GL_ACTIVE_UNIFORMS, numUniforms );

        Uniforms = new string[ *numUniforms ];

        for ( uint i = 0; i < *numUniforms; i++ )
        {
            var name = GdxApi.Bindings.GetActiveUniform( ( uint )Handle,
                                                         i,
                                                         IGL.GL_ACTIVE_UNIFORM_MAX_LENGTH,
                                                         out var size,
                                                         out var type );

            var location = GdxApi.Bindings.GetUniformLocation( ( uint )Handle, name );

            _uniforms[ name ]     = location;
            _uniformSizes[ name ] = size;
            _uniformTypes[ name ] = type;
            Uniforms[ i ]         = name;
        }
    }

    /// <summary>
    /// </summary>
    private unsafe void FetchAttributes()
    {
        var numAttributes = stackalloc int[ 1 ];

        GdxApi.Bindings.GetProgramiv( ( uint )Handle, IGL.GL_ACTIVE_ATTRIBUTES, numAttributes );

        Attributes = new string[ *numAttributes ];

        for ( var index = 0; index < *numAttributes; index++ )
        {
            var name = GdxApi.Bindings.GetActiveAttrib( ( uint )Handle,
                                                        ( uint )index,
                                                        IGL.GL_ACTIVE_ATTRIBUTE_MAX_LENGTH,
                                                        out var size,
                                                        out var type );

            var location = GdxApi.Bindings.GetAttribLocation( ( uint )Handle, name );

            _attributes[ name ]     = location;
            _attributeSizes[ name ] = size;
            _attributeTypes[ name ] = type;

            Attributes[ index ] = name;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="app"></param>
    /// <param name="shaderProgram"></param>
    private void AddManagedShader( IApplication app, ShaderProgram shaderProgram )
    {
        List< ShaderProgram >? managedResources;

        if ( !_shaders.ContainsKey( app ) || ( _shaders[ app ] == null ) )
        {
            managedResources = new List< ShaderProgram >();
        }
        else
        {
            managedResources = _shaders[ app ];
        }

        managedResources?.Add( shaderProgram );

        _shaders.Put( app, managedResources );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="app"></param>
    public static void ClearAllShaderPrograms( IApplication app )
    {
        _shaders.Remove( app );
    }

    /// <summary>
    /// Invalidates all shaders so the next time they are used new
    /// handles are generated.
    /// </summary>
    /// <param name="app">  </param>
    public static void InvalidateAllShaderPrograms( IApplication app )
    {
        List< ShaderProgram >? shaderArray;

        if ( !_shaders.TryGetValue( app, out var value ) || ( value == null ) )
        {
            shaderArray = [ ];
        }
        else
        {
            shaderArray = value;
        }

        foreach ( var sp in shaderArray! )
        {
            sp._invalidated = true;
            sp.CheckManaged();
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    private void CheckManaged()
    {
        if ( _invalidated )
        {
            CompileShaders( VertexShaderSource, FragmentShaderSource );
            _invalidated = false;
        }
    }
}

