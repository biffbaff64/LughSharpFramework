// ///////////////////////////////////////////////////////////////////////////////
// MIT License
// 
// Copyright (c) 2024 Circa64 Software Projects / Richard Ikin.
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

namespace LughSharp.Source.Physics.Box2D.Shapes;

[PublicAPI]
public class ChainShape : Shape
{
    public bool IsLooped { get; private set; }

    // ========================================================================

    private static float[] _verts = new float[ 2 ];

    // ========================================================================

    public ChainShape()
    {
        addr = NewChainShape();
    }

    public ChainShape( long addr )
    {
        this.addr = addr;
    }

    /// <summary>
    /// Get the type of this shape. You can use this to down cast to the concrete shape.
    /// </summary>
    /// <returns> The shape type. </returns>
    public override ShapeTypes GetShapeType()
    {
        return ShapeTypes.Chain;
    }

    /// <summary>
    /// Clear all vertices in the chain and free the memory.
    /// </summary>
    public void Clear()
    {
        jniClear( addr );
    }

    /// <summary>
    /// Create a loop. This automatically adjusts connectivity.
    /// </summary>
    /// <param name="vertices"> An array of floats of alternating x, y coordinates. </param>
    public void CreateLoop( float[] vertices )
    {
        jniCreateLoop( addr, vertices, 0, vertices.Length / 2 );
        IsLooped = true;
    }

    /// <summary>
    /// Create a loop. This automatically adjusts connectivity.
    /// </summary>
    /// <param name="vertices"> An array of floats of alternating x, y coordinates. </param>
    /// <param name="offset"> Into the vertices array </param>
    /// <param name="length"> After offset (in floats, not float-pairs, so even number) </param>
    public void CreateLoop( float[] vertices, int offset, int length )
    {
        jniCreateLoop( addr, vertices, offset, length / 2 );
        IsLooped = true;
    }

    /// <summary>
    /// Create a loop. This automatically adjusts connectivity.
    /// </summary>
    /// <param name="vertices"> An array of vertices, these are copied </param>
    public void CreateLoop( Vector2[] vertices )
    {
        var verts = new float[ vertices.Length * 2 ];

        for ( int i = 0, j = 0; i < vertices.Length * 2; i += 2, j++ )
        {
            verts[ i ]     = vertices[ j ].X;
            verts[ i + 1 ] = vertices[ j ].Y;
        }

        jniCreateLoop( addr, verts, 0, verts.Length / 2 );
        IsLooped = true;
    }

    /// <summary>
    /// Create a chain with isolated end vertices.
    /// </summary>
    /// <param name="vertices"> an array of floats of alternating x, y coordinates. </param>
    public void CreateChain( float[] vertices )
    {
        jniCreateChain( addr, vertices, 0, vertices.Length / 2 );
        IsLooped = false;
    }

    /// <summary>
    /// Create a chain with isolated end vertices.
    /// </summary>
    /// <param name="vertices"> an array of floats of alternating x, y coordinates. </param>
    /// <param name="offset"> into the vertices array </param>
    /// <param name="length"> after offset (in floats, not float-pairs, so even number) </param>
    public void CreateChain( float[] vertices, int offset, int length )
    {
        jniCreateChain( addr, vertices, offset, length / 2 );
        IsLooped = false;
    }

    /// <summary>
    /// Create a chain with isolated end vertices.
    /// </summary>
    /// <param name="vertices"> an array of vertices, these are copied </param>
    public void CreateChain( Vector2[] vertices )
    {
        var verts = new float[ vertices.Length * 2 ];

        for ( int i = 0, j = 0; i < vertices.Length * 2; i += 2, j++ )
        {
            verts[ i ]     = vertices[ j ].X;
            verts[ i + 1 ] = vertices[ j ].Y;
        }

        jniCreateChain( addr, verts, 0, vertices.Length );
        IsLooped = false;
    }

    /// Establish connectivity to a vertex that precedes the first vertex. Don't call this for loops. */
    public void SetPrevVertex( Vector2 prevVertex )
    {
        SetPrevVertex( prevVertex.X, prevVertex.Y );
    }

    /// Establish connectivity to a vertex that precedes the first vertex. Don't call this for loops. */
    public void SetPrevVertex( float prevVertexX, float prevVertexY )
    {
        jniSetPrevVertex( addr, prevVertexX, prevVertexY );
    }

    /// Establish connectivity to a vertex that follows the last vertex.
    /// Don't call this for loops.
    public void SetNextVertex( Vector2 nextVertex )
    {
        SetNextVertex( nextVertex.X, nextVertex.Y );
    }

    /// Establish connectivity to a vertex that follows the last vertex.
    /// Don't call this for loops.
    public void SetNextVertex( float nextVertexX, float nextVertexY )
    {
        jniSetNextVertex( addr, nextVertexX, nextVertexY );
    }

    /// <summary>
    /// Returns the number of vertices
    /// </summary>
    public int GetVertexCount()
    {
        return jniGetVertexCount( addr );
    }

    /// <summary>
    /// Returns the vertex at the given position.
    /// </summary>
    /// <param name="index"> the index of the vertex 0 &lt;= index &lt; GetVertexCount() </param>
    /// <param name="vertex"> vertex </param>
    public void GetVertex( int index, Vector2 vertex )
    {
        jniGetVertex( addr, index, _verts );
        vertex.X = _verts[ 0 ];
        vertex.Y = _verts[ 1 ];
    }

    // ========================================================================
    // ========================================================================

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniCreateLoop( long addr, float[] verts, int offset, int numVertices );
    /*
        b2ChainShape* chain = (b2ChainShape*)addr;
        b2Vec2* verticesOut = new b2Vec2[numVertices];
        for( int i = 0; i < numVertices; i++ )
            verticesOut[i] = b2Vec2(verts[offset+(i<<1)], verts[offset+(i<<1)+1]);
        chain->CreateLoop( verticesOut, numVertices );
        delete[] verticesOut;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniCreateChain( long addr, float[] verts, int offset, int numVertices );
    /*
        b2ChainShape* chain = (b2ChainShape*)addr;
        b2Vec2* verticesOut = new b2Vec2[numVertices];
        for( int i = 0; i < numVertices; i++ )
            verticesOut[i] = b2Vec2(verts[offset+(i<<1)], verts[offset+(i<<1)+1]);
        chain->CreateChain( verticesOut, numVertices );
        delete[] verticesOut;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetPrevVertex( long addr, float x, float y );
    /*
        b2ChainShape* chain = (b2ChainShape*)addr;
        chain->SetPrevVertex(b2Vec2(x, y));
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetNextVertex( long addr, float x, float y );
    /*
        b2ChainShape* chain = (b2ChainShape*)addr;
        chain->SetNextVertex(b2Vec2(x, y));
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern int jniGetVertexCount( long addr );
    /*
        b2ChainShape* chain = (b2ChainShape*)addr;
        return chain->GetVertexCount();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetVertex( long addr, int index, float[] verts );
    /*
        b2ChainShape* chain = (b2ChainShape*)addr;
        const b2Vec2 v = chain->GetVertex( index );
        verts[0] = v.x;
        verts[1] = v.y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniClear( long addr );
    /*
        b2ChainShape* chain = (b2ChainShape*)addr;
        chain->Clear();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "b2ChainShape", CallingConvention = CallingConvention.Cdecl )]
    private static extern long NewChainShape();
    /*
        return (jlong)(new b2ChainShape());
    */
}

// ============================================================================
// ============================================================================
