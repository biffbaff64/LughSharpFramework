﻿// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Richard Ikin.
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

using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Graphics.Utils;
using LughSharp.Lugh.Maths.Collision;
using LughSharp.Lugh.Utils.Buffers;
using LughSharp.Lugh.Utils.Exceptions;



namespace LughSharp.Lugh.Graphics;

[PublicAPI]
public class Mesh : IDisposable
{
    public IIndexData IndexData   { get; set; }
    public bool       IsInstanced { get; set; } = false;

    /// <summary>
    /// Whether to bind the underlying <see cref="VertexArray" /> or <see cref="VertexBufferObject" />
    /// automatically on a call to one of the render methods. Default setting is <c>TRUE</c>
    /// </summary>
    /// <param name="value"> whether to autobind meshes.  </param>
    public bool AutoBind { get; set; } = true;

    // ========================================================================

    private static readonly Dictionary< IApplication, List< Mesh >? > _meshes = new();

    private readonly bool        _isVertexArray;
    private readonly ShortBuffer _shortBuffer = new( 100 );
    private readonly Vector3     _tmpV        = new();
    private readonly IVertexData _vertices;

    private InstanceBufferObject? _instances;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates a new mesh with the given attrributes.
    /// </summary>
    /// <param name="vertices"></param>
    /// <param name="indices"></param>
    /// <param name="isVertexArray"></param>
    protected Mesh( IVertexData vertices, IIndexData indices, bool isVertexArray )
    {
        _vertices      = vertices;
        IndexData      = indices;
        _isVertexArray = isVertexArray;

        AddManagedMesh( Api.App, this );
    }

    /// <summary>
    /// Creates a new Mesh with the given attributes.
    /// </summary>
    /// <param name="isStatic"> whether this mesh is static or not. Allows for internal optimizations. </param>
    /// <param name="maxVertices"> the maximum number of vertices this mesh can hold </param>
    /// <param name="maxIndices"> the maximum number of indices this mesh can hold </param>
    /// <param name="attributes">
    /// the <see cref="VertexAttribute" />s. Each vertex attribute defines one property
    /// of a vertex such as position, normal or texture coordinate
    /// </param>
    public Mesh( bool isStatic, int maxVertices, int maxIndices, params VertexAttribute[] attributes )
    {
        _vertices      = MakeVertexBuffer( isStatic, maxVertices, new VertexAttributes( attributes ) );
        IndexData      = new IndexBufferObject( isStatic, maxIndices );
        _isVertexArray = false;

        AddManagedMesh( Api.App, this );
    }

    /// <summary>
    /// Creates a new Mesh with the given attributes.
    /// </summary>
    /// <param name="isStatic">whether this mesh is static or not. Allows for internal optimizations.</param>
    /// <param name="maxVertices">the maximum number of vertices this mesh can hold</param>
    /// <param name="maxIndices">the maximum number of indices this mesh can hold</param>
    /// <param name="attributes">
    /// the <see cref="VertexAttributes" />. Each vertex attribute defines one property
    /// of a vertex such as position, normal or texture coordinate
    /// </param>
    public Mesh( bool isStatic, int maxVertices, int maxIndices, VertexAttributes attributes )
    {
        _vertices      = MakeVertexBuffer( isStatic, maxVertices, attributes );
        IndexData      = new IndexBufferObject( isStatic, maxIndices );
        _isVertexArray = false;

        AddManagedMesh( Api.App, this );
    }

    /// <summary>
    /// Creates a new Mesh with the given attributes. Adds extra optimizations for dynamic
    /// (frequently modified) meshes.
    /// </summary>
    /// <param name="staticVertices">
    /// whether vertices of this mesh are static or not. Allows for internal optimizations.
    /// </param>
    /// <param name="staticIndices">
    /// whether indices of this mesh are static or not. Allows for internal optimizations.
    /// </param>
    /// <param name="maxVertices"> the maximum number of vertices this mesh can hold </param>
    /// <param name="maxIndices"> the maximum number of indices this mesh can hold </param>
    /// <param name="attributes">
    /// the <see cref="VertexAttributes" />. Each vertex attribute defines one property
    /// of a vertex such as position, normal or texture coordinate
    /// </param>
    public Mesh( bool staticVertices,
                 bool staticIndices,
                 int maxVertices,
                 int maxIndices,
                 VertexAttributes attributes )
    {
        _vertices      = MakeVertexBuffer( staticVertices, maxVertices, attributes );
        IndexData      = new IndexBufferObject( staticIndices, maxIndices );
        _isVertexArray = false;

        AddManagedMesh( Api.App, this );
    }

    /// <summary>
    /// Creates a new Mesh with the given attributes.
    /// </summary>
    /// <param name="type">the <see cref="VertexDataType" /> to be used, VBO or VA.</param>
    /// <param name="isStatic">whether this mesh is static or not. Allows for internal optimizations.</param>
    /// <param name="maxVertices">the maximum number of vertices this mesh can hold</param>
    /// <param name="maxIndices">the maximum number of indices this mesh can hold</param>
    /// <param name="attributes">
    /// the <see cref="VertexAttribute" />s. Each vertex attribute defines one property
    /// of a vertex such as position, normal or texture coordinate
    /// </param>
    public Mesh( VertexDataType? type,
                 bool isStatic,
                 int maxVertices,
                 int maxIndices,
                 params VertexAttribute[] attributes )
        : this( type, isStatic, maxVertices, maxIndices, new VertexAttributes( attributes ) )
    {
    }

    /// <summary>
    /// Creates a new Mesh with the given attributes.
    /// </summary>
    /// <param name="type">the <see cref="VertexDataType" /> to be used, VBO or VA.</param>
    /// <param name="isStatic">
    /// whether this mesh is static or not. Allows for internal optimizations.
    /// </param>
    /// <param name="maxVertices">the maximum number of vertices this mesh can hold</param>
    /// <param name="maxIndices">the maximum number of indices this mesh can hold</param>
    /// <param name="attributes">the <see cref="VertexAttributes" />.</param>
    public Mesh( VertexDataType? type, bool isStatic, int maxVertices, int maxIndices, VertexAttributes attributes )
    {
        switch ( type )
        {
            case VertexDataType.VertexBufferObject:
                _vertices      = new VertexBufferObject( isStatic, maxVertices, attributes );
                IndexData      = new IndexBufferObject( isStatic, maxIndices );
                _isVertexArray = false;

                break;

            case VertexDataType.VertexBufferObjectSubData:
                _vertices      = new VertexBufferObjectSubData( isStatic, maxVertices, attributes );
                IndexData      = new IndexBufferObjectSubData( isStatic, maxIndices );
                _isVertexArray = false;

                break;

            case VertexDataType.VertexBufferObjectWithVAO:
                _vertices      = new VertexBufferObjectWithVAO( isStatic, maxVertices, attributes );
                IndexData      = new IndexBufferObjectSubData( isStatic, maxIndices );
                _isVertexArray = false;

                break;

            case VertexDataType.VertexArray:
            default:
                _vertices      = new VertexArray( maxVertices, attributes );
                IndexData      = new IndexArray( maxIndices );
                _isVertexArray = true;

                break;
        }

        AddManagedMesh( Api.App, this );
    }

    // ========================================================================

    /// <summary>
    /// Returns the vertex attributes of this Mesh.
    /// </summary>
    public VertexAttributes VertexAttributes => _vertices.Attributes;

    /// <summary>
    /// the number of defined indices.
    /// </summary>
    public int NumIndices => IndexData.NumIndices;

    /// <summary>
    /// the number of defined vertices.
    /// </summary>
    public int NumVertices => _vertices.NumVertices;

    /// <summary>
    /// the maximum number of vertices this mesh can hold
    /// </summary>
    public int MaxVertices => _vertices.NumMaxVertices;

    /// <summary>
    /// the maximum number of indices this mesh can hold
    /// </summary>
    public int MaxIndices => IndexData.NumMaxIndices;

    /// <summary>
    /// the size of a single vertex in bytes
    /// </summary>
    public int VertexSize => _vertices.Attributes.VertexSize;

    /// <summary>
    /// the backing shortbuffer holding the _indices.
    /// Does not have to be a direct buffer on Android!
    /// </summary>
    public ShortBuffer IndicesBuffer => IndexData.GetBuffer( false );

    // ========================================================================

    /// <summary>
    /// Provides a summary of the managed status of meshes within the application.
    /// Returns a string that indicates the number of managed meshes for each application context.
    /// </summary>
    public static string ReportManagedStatus
    {
        get
        {
            var builder = new StringBuilder( "Managed meshes/app: { " );

            foreach ( var app in _meshes.Keys )
            {
                builder.Append( _meshes[ app ]?.Count );
                builder.Append( ' ' );
            }

            builder.Append( '}' );

            return builder.ToString();
        }
    }

    // ========================================================================

    /// <summary>
    /// Enables instanced rendering for the mesh by initializing an instance buffer object with the given attributes.
    /// </summary>
    /// <param name="isStatic">Indicates whether the instance data should be static or dynamic.</param>
    /// <param name="maxInstances">The maximum number of instances to render.</param>
    /// <param name="attributes">Specifies the vertex attributes for the instance data.</param>
    /// <returns>The current <see cref="Mesh" /> instance with instanced rendering enabled.</returns>
    public Mesh EnableInstancedRendering( bool isStatic, int maxInstances, params VertexAttribute[] attributes )
    {
        if ( !IsInstanced )
        {
            IsInstanced = true;
            _instances  = new InstanceBufferObject( isStatic, maxInstances, attributes );
        }
        else
        {
            throw new GdxRuntimeException( "Trying to enable InstancedRendering on same Mesh instance twice."
                                           + " Use disableInstancedRendering to clean up old InstanceData first" );
        }

        return this;
    }

    /// <summary>
    /// Disables instanced rendering for the mesh, releasing any instance data resources associated with it.
    /// </summary>
    /// <returns>The current instance of the Mesh with instanced rendering disabled.</returns>
    public Mesh DisableInstancedRendering()
    {
        if ( IsInstanced )
        {
            IsInstanced = false;

            _instances?.Dispose();
            _instances = null;
        }

        return this;
    }

    /// <summary>
    /// Sets the instance data of this Mesh. The attributes are assumed to be given in float format.
    /// </summary>
    /// <param name="instanceData"> the instance data. </param>
    /// <param name="offset"> the offset into the vertices array </param>
    /// <param name="count"> the number of floats to use </param>
    /// <returns> the mesh for invocation chaining.  </returns>
    public Mesh SetInstanceData( float[] instanceData, int offset, int count )
    {
        if ( _instances != null )
        {
            _instances.SetInstanceData( instanceData, offset, count );

            return this;
        }

        throw new GdxRuntimeException( "An InstanceBufferObject must be set before setting instance data!" );
    }

    /// <summary>
    /// Sets the instance data of this Mesh. The attributes are assumed to be given in float format.
    /// </summary>
    /// <param name="instanceData"> the instance data. </param>
    /// <returns> the mesh for invocation chaining.  </returns>
    public Mesh SetInstanceData( float[] instanceData )
    {
        if ( _instances != null )
        {
            _instances.SetInstanceData( instanceData, 0, instanceData.Length );

            return this;
        }

        throw new GdxRuntimeException( "An InstanceBufferObject must be set before setting instance data!" );
    }

    /// <summary>
    /// Sets the instance data of this Mesh. The attributes are assumed to be given in float format.
    /// </summary>
    /// <param name="instanceData"> the instance data. </param>
    /// <param name="count"> the number of floats to use </param>
    /// <returns> the mesh for invocation chaining.  </returns>
    public Mesh SetInstanceData( FloatBuffer instanceData, int count )
    {
        if ( _instances != null )
        {
            _instances.SetInstanceData( instanceData, count );

            return this;
        }

        throw new GdxRuntimeException( "An InstanceBufferObject must be set before setting instance data!" );
    }

    /// <summary>
    /// Sets the instance data of this Mesh. The attributes are assumed to be given in float format.
    /// </summary>
    /// <param name="instanceData"> the instance data. </param>
    /// <returns> the mesh for invocation chaining.  </returns>
    public Mesh SetInstanceData( FloatBuffer instanceData )
    {
        if ( _instances != null )
        {
            _instances.SetInstanceData( instanceData, instanceData.Limit );

            return this;
        }

        throw new GdxRuntimeException( "An InstanceBufferObject must be set before setting instance data!" );
    }

    /// <summary>
    /// Update (a portion of) the instance data. Does not resize the backing buffer.
    /// </summary>
    /// <param name="targetOffset"> the offset in number of floats of the mesh part. </param>
    /// <param name="source"> the instance data to update the mesh part with </param>
    /// <param name="sourceOffset">
    /// the offset in number of floats within the source array. Default is zero.
    /// </param>
    /// <param name="count">
    /// the number of floats to update. Default is zero which dictates that count will be
    /// overridden with the value of <paramref name="source" />.Length.
    /// </param>
    public Mesh UpdateInstanceData( int targetOffset, float[] source, int sourceOffset = 0, int count = 0 )
    {
        if ( count == 0 )
        {
            count = source.Length;
        }

        _instances?.UpdateInstanceData( targetOffset, source, sourceOffset, count );

        return this;
    }

    /// <summary>
    /// Update (a portion of) the instance data. Does not resize the backing buffer.
    /// </summary>
    /// <param name="targetOffset"> the offset in number of floats of the mesh part. </param>
    /// <param name="source"> the instance data to update the mesh part with </param>
    /// <param name="sourceOffset">
    /// the offset in number of floats within the source array. Default is zero.
    /// </param>
    /// <param name="count">
    /// the number of floats to update. Default is zero which dictates that count will be
    /// overridden with the value of <paramref name="source" />.Limit.
    /// </param>
    public Mesh UpdateInstanceData( int targetOffset, FloatBuffer source, int sourceOffset = 0, int count = 0 )
    {
        if ( count == 0 )
        {
            count = source.Limit;
        }

        _instances?.UpdateInstanceData( targetOffset, source, sourceOffset, count );

        return this;
    }

    /// <summary>
    /// Sets the vertices of this Mesh. The attributes are assumed to be given in float format.
    /// </summary>
    /// <param name="vertices"> the vertices.</param>
    /// <returns> the mesh for invocation chaining.</returns>
    public Mesh SetVertices( float[] vertices )
    {
        _vertices.SetVertices( vertices, 0, vertices.Length );

        return this;
    }

    /// <summary>
    /// Sets the vertices of this Mesh. The attributes are assumed to be given in float format.
    /// </summary>
    /// <param name="vertices"> the vertices. </param>
    /// <param name="offset"> the offset into the vertices array </param>
    /// <param name="count"> the number of floats to use </param>
    /// <returns> the mesh for invocation chaining.  </returns>
    public Mesh SetVertices( float[] vertices, int offset, int count )
    {
        _vertices.SetVertices( vertices, offset, count );

        return this;
    }

    /// <summary>
    /// Update (a portion of) the vertices. Does not resize the backing buffer.
    /// </summary>
    /// <param name="targetOffset"> the offset in number of floats of the mesh part. </param>
    /// <param name="source"> the vertex data to update the mesh part with  </param>
    public Mesh UpdateVertices( int targetOffset, float[] source )
    {
        return UpdateVertices( targetOffset, source, 0, source.Length );
    }

    /// <summary>
    /// Update (a portion of) the vertices. Does not resize the backing buffer.
    /// </summary>
    /// <param name="targetOffset"> the offset in number of floats of the mesh part. </param>
    /// <param name="source"> the vertex data to update the mesh part with </param>
    /// <param name="sourceOffset"> the offset in number of floats within the source array </param>
    /// <param name="count"> the number of floats to update  </param>
    public Mesh UpdateVertices( int targetOffset, float[] source, int sourceOffset, int count )
    {
        _vertices.UpdateVertices( targetOffset, source, sourceOffset, count );

        return this;
    }

    /// <summary>
    /// Copies the vertices from the Mesh to the float array. The float array
    /// must be large enough to hold all the Mesh's vertices.
    /// </summary>
    /// <param name="vertices"> the array to copy the vertices to  </param>
    public float[] GetVertices( float[] vertices )
    {
        return GetVertices( 0, NumberUtils.NOT_SET, vertices );
    }

    /// <summary>
    /// Copies the the remaining vertices from the Mesh to the float array.
    /// The float array must be large enough to hold the remaining vertices.
    /// </summary>
    /// <param name="srcOffset">
    /// the offset (in number of floats) of the vertices in the mesh to copy
    /// </param>
    /// <param name="vertices"> the array to copy the vertices to  </param>
    public float[] GetVertices( int srcOffset, float[] vertices )
    {
        return GetVertices( srcOffset, NumberUtils.NOT_SET, vertices );
    }

    /// <summary>
    /// Copies the specified vertices from the Mesh to the float array. The float
    /// array must be large enough to hold destOffset + count vertices.
    /// </summary>
    /// <param name="srcOffset"> the offset (in number of floats) of the vertices in the mesh to copy </param>
    /// <param name="count"> the amount of floats to copy </param>
    /// <param name="vertices"> the array to copy the vertices to </param>
    /// <param name="destOffset"> the offset (in floats) in the vertices array to start copying  </param>
    public float[] GetVertices( int srcOffset, int count, float[] vertices, int destOffset = 0 )
    {
        var max = NumVertices * VertexSize;

        if ( count == NumberUtils.NOT_SET )
        {
            count = Math.Min( max - srcOffset, vertices.Length - destOffset );
        }

        if ( ( srcOffset < 0 )
             || ( count <= 0 )
             || ( ( srcOffset + count ) > max )
             || ( destOffset < 0 )
             || ( destOffset >= vertices.Length ) )
        {
            throw new IndexOutOfRangeException();
        }

        if ( ( vertices.Length - destOffset ) < count )
        {
            throw new ArgumentException( $"not enough room in vertices array, " +
                                         $"has {vertices.Length} floats, needs {count}" );
        }

        var verticesBuffer = GetVerticesBuffer();
        var pos            = verticesBuffer.Position;

        verticesBuffer.Position = srcOffset;
        verticesBuffer.GetFloats( vertices, destOffset, count );
        verticesBuffer.Position = pos;

        return vertices;
    }

    /// <summary>
    /// Sets the indices of this Mesh
    /// </summary>
    /// <param name="indices"> the indices </param>
    /// <returns> the mesh for invocation chaining.  </returns>
    public Mesh SetIndices( short[] indices )
    {
        IndexData.SetIndices( indices, 0, indices.Length );

        return this;
    }

    /// <summary>
    /// Sets the indices of this Mesh.
    /// </summary>
    /// <param name="indices"> the indices </param>
    /// <param name="offset"> the offset into the indices array </param>
    /// <param name="count"> the number of indices to copy </param>
    /// <returns> the mesh for invocation chaining.  </returns>
    public Mesh SetIndices( short[] indices, int offset, int count )
    {
        IndexData.SetIndices( indices, offset, count );

        return this;
    }

    /// <summary>
    /// Copies the indices from the Mesh to the short array. The short array
    /// must be large enough to hold destOffset + all the Mesh's _indices.
    /// </summary>
    /// <param name="indices"> the array to copy the indices to </param>
    /// <param name="destOffset"> the offset in the indices array to start copying  </param>
    public void GetIndices( short[] indices, int destOffset = 0 )
    {
        GetIndices( 0, indices, destOffset );
    }

    /// <summary>
    /// Copies the remaining indices from the Mesh to the short array. The short
    /// array must be large enough to hold destOffset + all the remaining _indices.
    /// </summary>
    /// <param name="srcOffset"> the zero-based offset of the first index to fetch </param>
    /// <param name="indices"> the array to copy the indices to </param>
    /// <param name="destOffset"> the offset in the indices array to start copying  </param>
    public void GetIndices( int srcOffset, short[] indices, int destOffset )
    {
        GetIndices( srcOffset, -1, indices, destOffset );
    }

    /// <summary>
    /// Copies the indices from the Mesh to the short array. The short array must
    /// be large enough to hold destOffset + count _indices.
    /// </summary>
    /// <param name="srcOffset"> the zero-based offset of the first index to fetch </param>
    /// <param name="count"> the total amount of indices to copy </param>
    /// <param name="indices"> the array to copy the indices to </param>
    /// <param name="destOffset"> the offset in the indices array to start copying  </param>
    public void GetIndices( int srcOffset, int count, short[] indices, int destOffset )
    {
        var max = NumIndices;

        if ( count < 0 )
        {
            count = max - srcOffset;
        }

        if ( ( srcOffset < 0 ) || ( srcOffset >= max ) || ( ( srcOffset + count ) > max ) )
        {
            throw new ArgumentException( $"Invalid range specified, offset: {srcOffset}, count: {count}, max: {max}" );
        }

        if ( ( indices.Length - destOffset ) < count )
        {
            throw new ArgumentException( $"not enough room in indices array, has {indices.Length} shorts, needs {count}" );
        }

        var pos = IndicesBuffer.Position;

        IndicesBuffer.Position = srcOffset;
        IndicesBuffer.GetShorts( indices, destOffset, count );
        IndicesBuffer.Position = pos;
    }

    // ========================================================================

    #region bind/unbind

    /// <summary>
    /// Binds the underlying <see cref="VertexBufferObject" /> and
    /// <see cref="IndexBufferObject" /> if indices where given.
    /// Use this with OpenGL ES 2.0 and when auto-bind is disabled.
    /// </summary>
    public void Bind( in ShaderProgram shader )
    {
        Bind( shader, null );
    }

    /// <summary>
    /// Binds the underlying <see cref="VertexBufferObject" /> and <see cref="IndexBufferObject" />
    /// if indices where given. Use this when auto-bind is disabled.
    /// </summary>
    /// <param name="shader"> the shader (does not bind the shader) </param>
    /// <param name="locations"> array containing the attribute locations.  </param>
    public void Bind( in ShaderProgram shader, in int[]? locations )
    {
        _vertices.Bind( shader, locations );

        if ( _instances is { NumInstances: > 0 } )
        {
            _instances.Bind( shader, locations );
        }

        if ( IndexData.NumIndices > 0 )
        {
            IndexData.Bind();
        }
    }

    /// <summary>
    /// Unbinds the underlying <see cref="VertexBufferObject" /> and <see cref="IndexBufferObject" />
    /// is indices were given. Use this when auto-bind is disabled.
    /// </summary>
    /// <param name="shader"> the shader (does not unbind the shader)  </param>
    public void Unbind( in ShaderProgram? shader )
    {
        Unbind( shader, null! );
    }

    /// <summary>
    /// Unbinds the underlying <see cref="VertexBufferObject" /> and <see cref="IndexBufferObject" />
    /// is indices were given. Use this when auto-bind is disabled.
    /// </summary>
    /// <param name="shader"> the shader (does not unbind the shader) </param>
    /// <param name="locations"> array containing the attribute locations.  </param>
    public void Unbind( in ShaderProgram? shader, in int[] locations )
    {
        if ( shader == null )
        {
            return;
        }

        _vertices.Unbind( shader, locations );

        if ( _instances is { NumInstances: > 0 } )
        {
            _instances.Unbind( shader, locations );
        }

        if ( IndexData.NumIndices > 0 )
        {
            IndexData.Unbind();
        }
    }

    #endregion bind/unbind

    // ========================================================================

    #region render methods

    /// <summary>
    /// Renders the mesh using the given primitive type. If indices are set for this
    /// mesh then getNumIndices() / #vertices per primitive primitives are rendered.
    /// If no indices are set then NumVertices / #vertices per primitive are rendered.
    /// <para>
    /// This method will automatically bind each vertex attribute as specified at
    /// construction time via <see cref="VertexAttributes" /> to the respective shader
    /// attributes. The binding is based on the alias defined for each VertexAttribute.
    /// </para>
    /// <para>
    /// This method must only be called after the <see cref="ShaderProgram.Bind()" />
    /// method has been called!
    /// </para>
    /// </summary>
    /// <param name="shader"></param>
    /// <param name="primitiveType"> the primitive type  </param>
    public void Render( ShaderProgram shader, int primitiveType )
    {
        Render( shader, primitiveType, 0, IndexData.NumMaxIndices > 0 ? NumIndices : NumVertices, AutoBind );
    }

    /// <summary>
    /// Renders the mesh using the given primitive type. offset specifies the offset into
    /// either the vertex buffer or the index buffer depending on whether indices are
    /// defined. count specifies the number of vertices or indices to use thus count /
    /// vertices per primitive primitives are rendered.
    /// <para>
    /// This method will automatically bind each vertex attribute as specified at
    /// construction time via <see cref="VertexAttributes" /> to the respective shader
    /// attributes. The binding is based on the alias defined for each VertexAttribute.
    /// </para>
    /// <para>
    /// This method must only be called after the <see cref="ShaderProgram.Bind()" />
    /// method has been called!
    /// </para>
    /// </summary>
    /// <param name="shader"> the shader to be used </param>
    /// <param name="primitiveType"> the primitive type </param>
    /// <param name="offset"> the offset into the vertex or index buffer </param>
    /// <param name="count"> number of vertices or indices to use  </param>
    public void Render( ShaderProgram? shader, int primitiveType, int offset, int count )
    {
        Render( shader, primitiveType, offset, count, AutoBind );
    }

    /// <summary>
    /// Renders the mesh using the given primitive type. offset specifies the offset
    /// into either the vertex buffer or the index buffer depending on whether indices
    /// are defined. count specifies the number of vertices or indices to use thus count /
    /// vertices per primitive primitives are rendered.
    /// <para>
    /// This method will automatically bind each vertex attribute as specified at
    /// construction time via <see cref="VertexAttributes" /> to the respective shader
    /// attributes. The binding is based on the alias defined for each VertexAttribute.
    /// </para>
    /// <para>
    /// This method must only be called after the <see cref="ShaderProgram.Bind()" />
    /// method has been called!
    /// </para>
    /// </summary>
    /// <param name="shader"> the shader to be used </param>
    /// <param name="primitiveType"> the primitive type </param>
    /// <param name="offset"> the offset into the vertex or index buffer </param>
    /// <param name="count"> number of vertices or indices to use </param>
    /// <param name="autoBind"> overrides the autoBind member of this Mesh  </param>
    public unsafe void Render( ShaderProgram? shader, int primitiveType, int offset, int count, bool autoBind )
    {
        ArgumentNullException.ThrowIfNull( shader );

        if ( count == 0 )
        {
            return;
        }

        if ( autoBind )
        {
            Bind( shader );
        }

        if ( _isVertexArray )
        {
            if ( IndexData.NumIndices > 0 )
            {
                if ( ( count + offset ) > IndexData.NumMaxIndices )
                {
                    throw new AccessViolationException( $"Mesh attempting to access memory outside of the " +
                                                        $"index buffer (count: {count}, offset: {offset}, " +
                                                        $"max: {IndexData.NumMaxIndices})" );
                }

                var buffer = IndexData.GetBuffer( false );

                fixed ( short* ptr = &buffer.ToArray()[ 0 ] )
                {
                    GL.DrawElements( primitiveType, count, IGL.GL_UNSIGNED_SHORT, new IntPtr( ptr + offset ) );
                }
            }
            else
            {
                GL.DrawArrays( primitiveType, offset, count );
            }
        }
        else
        {
            if ( IndexData.NumIndices > 0 )
            {
                if ( ( count + offset ) > IndexData.NumMaxIndices )
                {
                    throw new GdxRuntimeException( $"Mesh attempting to access memory outside " +
                                                   $"of the index buffer (count: {count}, offset: " +
                                                   $"{offset}, max: {IndexData.NumMaxIndices})" );
                }

                IndexData.Bind();

                var offsetInBytes = offset * sizeof( short ); // Calculate byte offset

                GL.DrawElements( primitiveType, count, IGL.GL_UNSIGNED_SHORT, offsetInBytes );

                IndexData.Unbind();
            }
            else
            {
                if ( IsInstanced )
                {
                    GL.DrawArraysInstanced( primitiveType, offset, count, _instances!.NumInstances );
                }
                else
                {
                    GL.DrawArrays( primitiveType, offset, count );
                }
            }
        }

        if ( autoBind )
        {
            Unbind( shader );
        }
    }

    #endregion render methods

    // ========================================================================

    /// <summary>
    /// Returns the first <see cref="VertexAttribute" /> having the given <see cref="VertexConstants.Usage" />.
    /// </summary>
    /// <param name="usage"> the Usage. </param>
    /// <returns> the VertexAttribute or null if no attribute with that usage was found.  </returns>
    public VertexAttribute? GetVertexAttribute( int usage )
    {
        var attributes = _vertices.Attributes;
        var len        = attributes.Size;

        for ( var i = 0; i < len; i++ )
        {
            if ( attributes.Get( i ).Usage == usage )
            {
                return attributes.Get( i );
            }
        }

        return null;
    }

    /// <returns>
    /// the backing FloatBuffer holding the vertices.
    /// </returns>
    public FloatBuffer GetVerticesBuffer()
    {
        return _vertices.GetBuffer( false );
    }

    // ========================================================================

    /// <summary>
    /// Calculates the <see cref="BoundingBox" /> of the vertices contained in this mesh.
    /// In case no vertices are defined yet a <see cref="GdxRuntimeException" /> is thrown.
    /// This method creates a new BoundingBox instance.
    /// </summary>
    /// <returns> the bounding box.  </returns>
    public BoundingBox CalculateBoundingBox()
    {
        var bbox = new BoundingBox();

        CalculateBoundingBox( bbox );

        return bbox;
    }

    /// <summary>
    /// Calculates the <see cref="BoundingBox" /> of the vertices contained in this mesh.
    /// In case no vertices are defined yet a <see cref="GdxRuntimeException" /> is thrown.
    /// </summary>
    /// <param name="bbox"> the bounding box to store the result in.  </param>
    public void CalculateBoundingBox( BoundingBox bbox )
    {
        var numVertices = NumVertices;

        if ( numVertices == 0 )
        {
            throw new GdxRuntimeException( "No vertices defined" );
        }

        var verts = _vertices.GetBuffer( false );

        bbox.ToInfinity();

        var posAttrib  = GetVertexAttribute( ( int )VertexConstants.Usage.POSITION );
        var offset     = posAttrib!.Offset / 4;
        var vertexSize = _vertices.Attributes.VertexSize / 4;
        var idx        = offset;

        switch ( posAttrib.NumComponents )
        {
            case 1:
            {
                for ( var i = 0; i < numVertices; i++ )
                {
                    bbox.Extend( verts.GetFloat( idx ), 0, 0 );
                    idx += vertexSize;
                }

                break;
            }

            case 2:
            {
                for ( var i = 0; i < numVertices; i++ )
                {
                    bbox.Extend( verts.GetFloat( idx ), verts.GetFloat( idx + 1 ), 0 );
                    idx += vertexSize;
                }

                break;
            }

            case 3:
            {
                for ( var i = 0; i < numVertices; i++ )
                {
                    bbox.Extend( verts.GetFloat( idx ), verts.GetFloat( idx + 1 ), verts.GetFloat( idx + 2 ) );
                    idx += vertexSize;
                }

                break;
            }
        }
    }

    /// <summary>
    /// Calculate the <see cref="BoundingBox" /> of the specified part.
    /// </summary>
    /// <param name="box"> the bounding box to store the result in. </param>
    /// <param name="offset"> the start index of the part. </param>
    /// <param name="count"> the amount of indices the part contains. </param>
    /// <returns> the value specified by out.  </returns>
    public BoundingBox CalculateBoundingBox( in BoundingBox box, int offset, int count )
    {
        return ExtendBoundingBox( box.ToInfinity(), offset, count );
    }

    /// <summary>
    /// Calculate the <see cref="BoundingBox" /> of the specified part.
    /// </summary>
    /// <param name="box"> the bounding box to store the result in. </param>
    /// <param name="offset"> the start index of the part. </param>
    /// <param name="count"> the amount of indices the part contains. </param>
    /// <param name="transform"></param>
    /// <returns> the value specified by out.  </returns>
    public BoundingBox CalculateBoundingBox( in BoundingBox box, int offset, int count, in Matrix4 transform )
    {
        return ExtendBoundingBox( box.ToInfinity(), offset, count, transform );
    }

    /// <summary>
    /// Extends the specified <see cref="BoundingBox" /> with the specified part.
    /// </summary>
    /// <param name="box"> the bounding box to store the result in. </param>
    /// <param name="offset"> the start index of the part. </param>
    /// <param name="count"> the amount of indices the part contains. </param>
    /// <returns> the value specified by out.  </returns>
    public BoundingBox ExtendBoundingBox( in BoundingBox box, int offset, int count )
    {
        return ExtendBoundingBox( box, offset, count, null );
    }

    /// <summary>
    /// Extends the specified <see cref="BoundingBox" /> with the specified part.
    /// </summary>
    /// <param name="box"> the bounding box to store the result in. </param>
    /// <param name="offset"> the start of the part. </param>
    /// <param name="count"> the size of the part. </param>
    /// <param name="transform"></param>
    /// <returns> the value specified by out.  </returns>
    public BoundingBox ExtendBoundingBox( in BoundingBox box, int offset, int count, in Matrix4? transform )
    {
        var numIndices  = NumIndices;
        var numVertices = NumVertices;
        var max         = numIndices == 0 ? numVertices : numIndices;

        if ( ( offset < 0 ) || ( count < 1 ) || ( ( offset + count ) > max ) )
        {
            throw new GdxRuntimeException( $"Invalid part specified ( offset={offset}, count={count}, max={max} )" );
        }

        var verts      = _vertices.GetBuffer( false );
        var index      = IndexData.GetBuffer( false );
        var posAttrib  = GetVertexAttribute( ( int )VertexConstants.Usage.POSITION );
        var posoff     = posAttrib!.Offset / 4;
        var vertexSize = _vertices.Attributes.VertexSize / 4;
        var end        = offset + count;

        switch ( posAttrib.NumComponents )
        {
            case 1:
                if ( numIndices > 0 )
                {
                    for ( var i = offset; i < end; i++ )
                    {
                        var idx = ( int )( ( index.GetShort( i ) & 0xFFFF ) * vertexSize )! + posoff;

                        _tmpV.Set( verts.GetFloat( idx ), 0, 0 );

                        if ( transform != null )
                        {
                            _tmpV.Mul( transform );
                        }

                        box.Extend( _tmpV );
                    }
                }
                else
                {
                    for ( var i = offset; i < end; i++ )
                    {
                        var idx = ( int )( i * vertexSize )! + posoff;

                        _tmpV.Set( verts.GetFloat( idx ), 0, 0 );

                        if ( transform != null )
                        {
                            _tmpV.Mul( transform );
                        }

                        box.Extend( _tmpV );
                    }
                }

                break;

            case 2:
                if ( numIndices > 0 )
                {
                    for ( var i = offset; i < end; i++ )
                    {
                        var idx = ( int )( ( index.GetShort( i ) & 0xFFFF ) * vertexSize )! + posoff;

                        _tmpV.Set( verts.GetFloat( idx ), verts.GetFloat( idx + 1 ), 0 );

                        if ( transform != null )
                        {
                            _tmpV.Mul( transform );
                        }

                        box.Extend( _tmpV );
                    }
                }
                else
                {
                    for ( var i = offset; i < end; i++ )
                    {
                        var idx = ( int )( i * vertexSize )! + posoff;

                        _tmpV.Set( verts.GetFloat( idx ), verts.GetFloat( idx + 1 ), 0 );

                        if ( transform != null )
                        {
                            _tmpV.Mul( transform );
                        }

                        box.Extend( _tmpV );
                    }
                }

                break;

            case 3:
                if ( numIndices > 0 )
                {
                    for ( var i = offset; i < end; i++ )
                    {
                        var idx = ( int )( ( index.GetShort( i ) & 0xFFFF ) * vertexSize )! + posoff;

                        _tmpV.Set( verts.GetFloat( idx ), verts.GetFloat( idx + 1 ), verts.GetFloat( idx + 2 ) );

                        if ( transform != null )
                        {
                            _tmpV.Mul( transform );
                        }

                        box.Extend( _tmpV );
                    }
                }
                else
                {
                    for ( var i = offset; i < end; i++ )
                    {
                        var idx = ( int )( i * vertexSize )! + posoff;

                        _tmpV.Set( verts.GetFloat( idx ), verts.GetFloat( idx + 1 ), verts.GetFloat( idx + 2 ) );

                        if ( transform != null )
                        {
                            _tmpV.Mul( transform );
                        }

                        box.Extend( _tmpV );
                    }
                }

                break;
        }

        return box;
    }

    // ========================================================================

    /// <summary>
    /// Calculates the squared radius of the bounding sphere around the
    /// specified center for the specified part.
    /// </summary>
    /// <param name="centerX"> The X coordinate of the center of the bounding sphere </param>
    /// <param name="centerY"> The Y coordinate of the center of the bounding sphere </param>
    /// <param name="centerZ"> The Z coordinate of the center of the bounding sphere </param>
    /// <param name="offset"> the start index of the part. </param>
    /// <param name="count"> the amount of indices the part contains. </param>
    /// <param name="transform"></param>
    /// <returns> the squared radius of the bounding sphere.  </returns>
    public float CalculateRadiusSquared( in float centerX,
                                         in float centerY,
                                         in float centerZ,
                                         int offset,
                                         int count,
                                         in Matrix4? transform )
    {
        var numIndices = NumIndices;

        if ( ( offset < 0 ) || ( count < 1 ) || ( ( offset + count ) > numIndices ) )
        {
            throw new GdxRuntimeException( "Not enough indices" );
        }

        var verts      = _vertices.GetBuffer( false );
        var index      = IndexData.GetBuffer( false );
        var posAttrib  = GetVertexAttribute( ( int )VertexConstants.Usage.POSITION );
        var posoff     = posAttrib!.Offset / 4;
        var vertexSize = _vertices.Attributes.VertexSize / 4;
        var end        = offset + count;
        var result     = 0f;

        switch ( posAttrib.NumComponents )
        {
            case 1:
                for ( var i = offset; i < end; i++ )
                {
                    var idx = ( ( index.GetShort( i ) & 0xFFFF ) * vertexSize ) + posoff;

                    _tmpV.Set( verts.GetFloat( idx ), 0, 0 );

                    if ( transform != null )
                    {
                        _tmpV.Mul( transform );
                    }

                    var r = _tmpV.Sub( centerX, centerY, centerZ ).Len2();

                    if ( r > result )
                    {
                        result = r;
                    }
                }

                break;

            case 2:
                for ( var i = offset; i < end; i++ )
                {
                    var idx = ( ( index.GetShort( i ) & 0xFFFF ) * vertexSize ) + posoff;

                    _tmpV.Set( verts.GetFloat( idx ), verts.GetFloat( idx + 1 ), 0 );

                    if ( transform != null )
                    {
                        _tmpV.Mul( transform );
                    }

                    var r = _tmpV.Sub( centerX, centerY, centerZ ).Len2();

                    if ( r > result )
                    {
                        result = r;
                    }
                }

                break;

            case 3:
                for ( var i = offset; i < end; i++ )
                {
                    var idx = ( ( index.GetShort( i ) & 0xFFFF ) * vertexSize ) + posoff;

                    _tmpV.Set( verts.GetFloat( idx ), verts.GetFloat( idx + 1 ), verts.GetFloat( idx + 2 ) );

                    if ( transform != null )
                    {
                        _tmpV.Mul( transform );
                    }

                    var r = _tmpV.Sub( centerX, centerY, centerZ ).Len2();

                    if ( r > result )
                    {
                        result = r;
                    }
                }

                break;
        }

        return result;
    }

    /// <summary>
    /// Calculates the radius of the bounding sphere around the specified center for the specified part.
    /// </summary>
    /// <param name="centerX"> The X coordinate of the center of the bounding sphere </param>
    /// <param name="centerY"> The Y coordinate of the center of the bounding sphere </param>
    /// <param name="centerZ"> The Z coordinate of the center of the bounding sphere </param>
    /// <param name="offset"> the start index of the part. </param>
    /// <param name="count"> the amount of indices the part contains. </param>
    /// <param name="transform"></param>
    /// <returns> the radius of the bounding sphere.  </returns>
    public float CalculateRadius( in float centerX,
                                  in float centerY,
                                  in float centerZ,
                                  int offset,
                                  int count,
                                  in Matrix4? transform )
    {
        return ( float )Math.Sqrt( CalculateRadiusSquared( centerX, centerY, centerZ, offset, count, transform ) );
    }

    /// <summary>
    /// Calculates the squared radius of the bounding sphere around the specified center for the specified part.
    /// </summary>
    /// <param name="center"> The center of the bounding sphere </param>
    /// <param name="offset"> the start index of the part. </param>
    /// <param name="count"> the amount of indices the part contains. </param>
    /// <param name="transform"></param>
    /// <returns> the squared radius of the bounding sphere.  </returns>
    public float CalculateRadius( in Vector3 center, int offset, int count, in Matrix4 transform )
    {
        return CalculateRadius( center.X, center.Y, center.Z, offset, count, transform );
    }

    /// <summary>
    /// Calculates the squared radius of the bounding sphere around the specified center for the specified part.
    /// </summary>
    /// <param name="centerX"> The X coordinate of the center of the bounding sphere </param>
    /// <param name="centerY"> The Y coordinate of the center of the bounding sphere </param>
    /// <param name="centerZ"> The Z coordinate of the center of the bounding sphere </param>
    /// <param name="offset"> the start index of the part. </param>
    /// <param name="count"> the amount of indices the part contains. </param>
    /// <returns> the squared radius of the bounding sphere.  </returns>
    public float CalculateRadius( in float centerX, in float centerY, in float centerZ, int offset, int count )
    {
        return CalculateRadius( centerX, centerY, centerZ, offset, count, null );
    }

    /// <summary>
    /// Calculates the squared radius of the bounding sphere around the specified center for the specified part.
    /// </summary>
    /// <param name="center"> The center of the bounding sphere </param>
    /// <param name="offset"> the start index of the part. </param>
    /// <param name="count"> the amount of indices the part contains. </param>
    /// <returns> the squared radius of the bounding sphere.  </returns>
    public float CalculateRadius( in Vector3 center, int offset, int count )
    {
        return CalculateRadius( center.X, center.Y, center.Z, offset, count, null );
    }

    /// <summary>
    /// Calculates the squared radius of the bounding sphere around the specified center for the specified part.
    /// </summary>
    /// <param name="centerX"> The X coordinate of the center of the bounding sphere </param>
    /// <param name="centerY"> The Y coordinate of the center of the bounding sphere </param>
    /// <param name="centerZ"> The Z coordinate of the center of the bounding sphere </param>
    /// <returns> the squared radius of the bounding sphere.  </returns>
    public float CalculateRadius( in float centerX, in float centerY, in float centerZ )
    {
        return CalculateRadius( centerX, centerY, centerZ, 0, NumIndices, null );
    }

    /// <summary>
    /// Calculates the squared radius of the bounding sphere around the specified
    /// center for the specified part.
    /// </summary>
    /// <param name="center"> The center of the bounding sphere </param>
    /// <returns> the squared radius of the bounding sphere.  </returns>
    public float CalculateRadius( in Vector3 center )
    {
        return CalculateRadius( center.X, center.Y, center.Z, 0, NumIndices, null );
    }

    // ========================================================================

    /// <summary>
    /// Adds a managed mesh associated with the specified application.
    /// </summary>
    /// <param name="app">The application instance to associate the mesh with.</param>
    /// <param name="mesh">The mesh to be added and managed.</param>
    private static void AddManagedMesh( IApplication app, Mesh mesh )
    {
        List< Mesh >? managedResources;

        if ( !_meshes.ContainsKey( app ) || ( _meshes[ app ] == null ) )
        {
            managedResources = [ ];
        }
        else
        {
            managedResources = _meshes[ app ];
        }

        managedResources?.Add( mesh );

        _meshes.Add( app, managedResources );
    }

    /// <summary>
    /// Invalidates all meshes so the next time they are rendered new VBO handles are generated.
    /// </summary>
    /// <param name="app">  </param>
    public static void InvalidateAllMeshes( IApplication app )
    {
        for ( var i = 0; i < _meshes.Count; i++ )
        {
            _meshes[ app ]?[ i ]._vertices.Invalidate();
            _meshes[ app ]?[ i ].IndexData.Invalidate();
        }
    }

    /// <summary>
    /// Will clear the managed mesh cache. I wouldn't use this if i was you :)
    /// </summary>
    public static void ClearAllMeshes( IApplication app )
    {
        _meshes.Remove( app );
    }

    /// <summary>
    /// Method to scale the positions in the mesh. Normals will be kept as is.
    /// This is a potentially slow operation, use with care.
    /// It will also create a temporary float[] which will be garbage collected.
    /// </summary>
    /// <param name="scaleX"> scale on x </param>
    /// <param name="scaleY"> scale on y </param>
    /// <param name="scaleZ"> scale on z  </param>
    public void Scale( float scaleX, float scaleY, float scaleZ )
    {
        var posAttr       = GetVertexAttribute( ( int )VertexConstants.Usage.POSITION );
        var offset        = posAttr!.Offset / 4;
        var numComponents = posAttr.NumComponents;
        var numVertices   = NumVertices;
        var vertexSize    = VertexSize / 4;
        var vertices      = new float[ numVertices * vertexSize ];

        GetVertices( vertices );

        var idx = offset;

        switch ( numComponents )
        {
            case 1:
                for ( var i = 0; i < numVertices; i++ )
                {
                    vertices[ idx ] *= scaleX;
                    idx             += vertexSize;
                }

                break;

            case 2:
                for ( var i = 0; i < numVertices; i++ )
                {
                    vertices[ idx ]     *= scaleX;
                    vertices[ idx + 1 ] *= scaleY;
                    idx                 += vertexSize;
                }

                break;

            case 3:
                for ( var i = 0; i < numVertices; i++ )
                {
                    vertices[ idx ]     *= scaleX;
                    vertices[ idx + 1 ] *= scaleY;
                    vertices[ idx + 2 ] *= scaleZ;
                    idx                 += vertexSize;
                }

                break;
        }

        SetVertices( vertices );
    }

    /// <summary>
    /// Method to transform the positions in the mesh. Normals will be kept as is.
    /// This is a potentially slow operation, use with care. It will also create a
    /// temporary float[] which will be garbage collected.
    /// </summary>
    /// <param name="matrix"> the transformation matrix  </param>
    public void Transform( in Matrix4 matrix )
    {
        Transform( matrix, 0, NumVertices );
    }

    /// <summary>
    /// Applies a transformation to a subset of the vertex data using the specified matrix.
    /// </summary>
    /// <param name="matrix">The transformation matrix to be applied.</param>
    /// <param name="start">The starting index of the vertices to be transformed.</param>
    /// <param name="count">The number of vertices to be transformed.</param>
    protected void Transform( in Matrix4 matrix, in int start, in int count )
    {
        var posAttr = GetVertexAttribute( ( int )VertexConstants.Usage.POSITION );

        var posOffset     = posAttr!.Offset / 4;
        var stride        = VertexSize / 4;
        var numComponents = posAttr.NumComponents;
        var vertices      = new float[ count * stride ];

        GetVertices( start * stride, count * stride, vertices );

        // GetVertices(0, vertices.length, vertices);
        Transform( matrix, vertices, stride, posOffset, numComponents, 0, count );

        // SetVertices(vertices, 0, vertices.length);
        UpdateVertices( start * stride, vertices );
    }

    /// <summary>
    /// Method to transform the positions in the float array. Normals will be kept as is. This
    /// is a potentially slow operation, use with care.
    /// </summary>
    /// <param name="matrix"> the transformation matrix </param>
    /// <param name="vertices"> the float array </param>
    /// <param name="vertexSize"> the number of floats in each vertex </param>
    /// <param name="offset"> the offset within a vertex to the position </param>
    /// <param name="dimensions"> the size of the position </param>
    /// <param name="start"> the vertex to start with </param>
    /// <param name="count"> the amount of vertices to transform  </param>
    public static void Transform( in Matrix4 matrix,
                                  in float[] vertices,
                                  float vertexSize,
                                  float offset,
                                  int dimensions,
                                  int start,
                                  int count )
    {
        if ( ( offset < 0 ) || ( dimensions < 1 ) || ( ( offset + dimensions ) > vertexSize ) )
        {
            throw new IndexOutOfRangeException();
        }

        if ( ( start < 0 ) || ( count < 1 ) || ( ( ( start + count ) * vertexSize ) > vertices.Length ) )
        {
            throw new IndexOutOfRangeException( $"start = {start}, " +
                                                $"count = {count}, " +
                                                $"vertexSize = {vertexSize}, " +
                                                $"length = {vertices.Length}" );
        }

        var tmp      = new Vector3();
        var idxFloat = offset + ( start * vertexSize );

        switch ( dimensions )
        {
            case 1:
                for ( var i = 0; i < count; i++ )
                {
                    var idx = ( int )Math.Truncate( idxFloat );

                    tmp.Set( vertices[ idx ], 0, 0 ).Mul( matrix );

                    vertices[ idx ] =  tmp.X;
                    idxFloat        += vertexSize;
                }

                break;

            case 2:
                for ( var i = 0; i < count; i++ )
                {
                    var idx = ( int )Math.Truncate( idxFloat );

                    tmp.Set( vertices[ idx ], vertices[ idx + 1 ], 0 ).Mul( matrix );

                    vertices[ idx ]     =  tmp.X;
                    vertices[ idx + 1 ] =  tmp.Y;
                    idxFloat            += vertexSize;
                }

                break;

            case 3:
                for ( var i = 0; i < count; i++ )
                {
                    var idx = ( int )Math.Truncate( idxFloat );

                    tmp.Set( vertices[ idx ], vertices[ idx + 1 ], vertices[ idx + 2 ] ).Mul( matrix );

                    vertices[ idx ]     =  tmp.X;
                    vertices[ idx + 1 ] =  tmp.Y;
                    vertices[ idx + 2 ] =  tmp.Z;
                    idxFloat            += vertexSize;
                }

                break;
        }
    }

    /// <summary>
    /// Method to transform the texture coordinates in the mesh. This is a potentially slow operation,
    /// use with care. It will also create a temporary float[] which will be garbage collected.
    /// </summary>
    /// <param name="matrix"> the transformation matrix  </param>
    public void TransformUV( in Matrix3 matrix )
    {
        TransformUV( matrix, 0, NumVertices );
    }

    /// <summary>
    /// Transforms the UV coordinates of the vertices using the provided transformation matrix,
    /// starting at the specified vertex and processing the given number of vertices.
    /// </summary>
    /// <param name="matrix">The transformation matrix to apply to the UV coordinates.</param>
    /// <param name="start">The index of the first vertex to transform.</param>
    /// <param name="count">The number of vertices to transform.</param>
    protected void TransformUV( in Matrix3 matrix, int start, int count )
    {
        var posAttr = GetVertexAttribute( ( int )VertexConstants.Usage.TEXTURE_COORDINATES );

        if ( posAttr == null )
        {
            return;
        }

        var offset      = posAttr.Offset / 4;
        var vertexSize  = VertexSize / 4;
        var numVertices = NumVertices;
        var verts       = _vertices.GetBuffer( false );

        if ( ( start < 0 ) || ( count < 0 ) || ( ( start + count ) > numVertices ) )
        {
            throw new IndexOutOfRangeException( "Invalid start or count values." );
        }

        for ( var i = 0; i < count; i++ )
        {
            var index = offset + ( ( start + i ) * vertexSize );
            var uv    = new Vector2( verts.GetFloat( index ), verts.GetFloat( index + 1 ) );
            uv = matrix.Transform( uv );

            verts.PutFloat( index, uv.X );
            verts.PutFloat( index + 1, uv.Y );
        }
    }

    /// <summary>
    /// Method to transform the texture coordinates (UV) in the float array. This
    /// is a potentially slow operation, use with care.
    /// </summary>
    /// <param name="matrix"> the transformation matrix </param>
    /// <param name="vertices"> the float array </param>
    /// <param name="vertexSize"> the number of floats in each vertex </param>
    /// <param name="offset"> the offset within a vertex to the texture location </param>
    /// <param name="start"> the vertex to start with </param>
    /// <param name="count"> the amount of vertices to transform  </param>
    public static void TransformUV( in Matrix3 matrix,
                                    in float[] vertices,
                                    int vertexSize,
                                    int offset,
                                    int start,
                                    int count )
    {
        if ( ( start < 0 ) || ( count < 1 ) || ( ( ( start + count ) * vertexSize ) > vertices.Length ) )
        {
            throw new IndexOutOfRangeException( $"start = {start}," +
                                                $" count = {count}," +
                                                $" vertexSize = {vertexSize}," +
                                                $" length = {vertices.Length}" );
        }

        var tmp = new Vector2();
        var idx = offset + ( start * vertexSize );

        for ( var i = 0; i < count; i++ )
        {
            tmp.Set( vertices[ idx ], vertices[ idx + 1 ] ).Mul( matrix );

            vertices[ idx ]     =  tmp.X;
            vertices[ idx + 1 ] =  tmp.Y;
            idx                 += vertexSize;
        }
    }

    /// <summary>
    /// Copies this mesh.
    /// </summary>
    /// <param name="isStatic">
    /// whether the new mesh is static or not. Allows for internal optimizations.
    /// </param>
    /// <returns> the copy of this mesh  </returns>
    public Mesh Copy( bool isStatic )
    {
        return Copy( isStatic, false, null );
    }

    /// <summary>
    /// Copies this mesh optionally removing duplicate vertices and/or reducing
    /// the amount of attributes.
    /// </summary>
    /// <param name="isStatic">
    /// whether the new mesh is static or not. Allows for internal optimizations.
    /// </param>
    /// <param name="removeDuplicates">
    /// whether to remove duplicate vertices if possible. Only the vertices
    /// specified by usage are checked.
    /// </param>
    /// <param name="usage"> which attributes (if available) to copy </param>
    /// <returns> the copy of this mesh  </returns>
    public Mesh Copy( bool isStatic, bool removeDuplicates, in int[]? usage )
    {
        var vertexSizeFloats = VertexSize / 4; // Vertex size in floats
        var numVertices      = NumVertices;
        var vertices         = new float[ numVertices * vertexSizeFloats ];

        GetVertices( 0, vertices.Length, vertices );

        VertexAttribute[]? attrs               = null;
        var                newVertexSizeFloats = 0;
        int[]?             checks              = null; // Store float offsets

        if ( usage != null )
        {
            var sizeFloats = 0;
            var asCount    = 0;

            foreach ( var t in usage )
            {
                var a = GetVertexAttribute( t );

                if ( a != null )
                {
                    sizeFloats += a.NumComponents;
                    asCount++;
                }
            }

            if ( sizeFloats > 0 )
            {
                attrs  = new VertexAttribute[ asCount ];
                checks = new int[ sizeFloats ]; // Store float offsets

                var idx = -1;
                var ai  = -1;

                foreach ( var t in usage )
                {
                    var a = GetVertexAttribute( t );

                    if ( a == null )
                    {
                        continue;
                    }

                    for ( var j = 0; j < a.NumComponents; j++ )
                    {
                        checks[ ++idx ] = ( a.Offset / 4 ) + j; // Store float offsets!
                    }

                    attrs[ ++ai ]       =  a.Copy();
                    newVertexSizeFloats += a.NumComponents;
                }
            }
        }

        if ( checks == null )
        {
            checks = new int[ vertexSizeFloats ];

            for ( var i = 0; i < vertexSizeFloats; i++ )
            {
                checks[ i ] = i;
            }

            newVertexSizeFloats = vertexSizeFloats;
        }

        var      numIndices = NumIndices;
        short[]? indices    = null;

        if ( numIndices > 0 )
        {
            indices = new short[ numIndices ];
            GetIndices( indices );

            if ( removeDuplicates || ( newVertexSizeFloats != vertexSizeFloats ) )
            {
                var tmp  = new float[ numVertices * newVertexSizeFloats ]; // Correct size for tmp
                var size = 0;

                for ( var i = 0; i < numIndices; i++ )
                {
                    var   idx1     = indices[ i ] * vertexSizeFloats; // Use float vertex size
                    short newIndex = -1;

                    if ( removeDuplicates )
                    {
                        for ( var j = 0; ( j < size ) && ( newIndex < 0 ); j++ )
                        {
                            var idx2  = j * newVertexSizeFloats; // Use float vertex size
                            var found = true;

                            for ( var k = 0; ( k < checks.Length ) && found; k++ )
                            {
                                if ( !tmp[ idx2 + k ].Equals( vertices[ idx1 + checks[ k ] ] ) )
                                {
                                    found = false;
                                }
                            }

                            if ( found )
                            {
                                newIndex = ( short )j;
                            }
                        }
                    }

                    if ( newIndex >= 0 ) // Check if newIndex was found
                    {
                        indices[ i ] = newIndex;
                    }
                    else
                    {
                        var idx = size * newVertexSizeFloats; // Use float vertex size

                        for ( var j = 0; j < checks.Length; j++ )
                        {
                            tmp[ idx + j ] = vertices[ idx1 + checks[ j ] ];
                        }

                        indices[ i ] = ( short )size;
                        size++;
                    }
                }

                vertices    = tmp;
                numVertices = size;
            }
        }

        var result = attrs == null
            ? new Mesh( isStatic, numVertices, indices?.Length ?? 0, VertexAttributes )
            : new Mesh( isStatic, numVertices, indices?.Length ?? 0, attrs );

        result.SetVertices( vertices, 0, numVertices * newVertexSizeFloats ); // Use float vertex size

        if ( indices != null )
        {
            result.SetIndices( indices );
        }

        return result;
    }

    // ========================================================================

    /// <summary>
    /// Creates a new vertex buffer object with the specified attributes.
    /// </summary>
    /// <param name="isStatic">Indicates whether the vertex buffer is static or dynamic.</param>
    /// <param name="maxVertices">The maximum number of vertices the buffer can hold.</param>
    /// <param name="vertexAttributes">The attributes of the vertices stored in the buffer.</param>
    /// <returns>
    /// A new instance of <see cref="VertexBufferObjectWithVAO" /> configured with the specified parameters.
    /// </returns>
    private static VertexBufferObjectWithVAO MakeVertexBuffer( bool isStatic, int maxVertices, VertexAttributes vertexAttributes )
    {
        return new VertexBufferObjectWithVAO( isStatic, maxVertices, vertexAttributes );
    }

    // ========================================================================

    /// <summary>
    /// Frees all resources associated with this Mesh
    /// </summary>
    public void Dispose()
    {
        if ( _meshes[ Api.App ] != null )
        {
            _meshes[ Api.App ]?.Remove( this );
        }

        _vertices.Dispose();
        _instances?.Dispose();
        IndexData.Dispose();

        GC.SuppressFinalize( this );
    }
}

// ============================================================================
// ============================================================================
