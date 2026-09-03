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
public class PolygonShape : Shape
{
    private static float[] _verts = new float[ 2 ];

    // ========================================================================
    
    /// <summary>
    /// Constructs a new polygon
    /// </summary>
    public PolygonShape()
    {
        Addr = NewPolygonShape();
    }

    /// <summary>
    /// Constructs a new polygon
    /// </summary>
    public PolygonShape( long addr )
    {
        this.Addr = addr;
    }

    /// <summary>
    /// Get the type of this shape. You can use this to down cast to the concrete shape.
    /// </summary>
    /// <returns> The shape type. </returns>
    public override ShapeTypes GetShapeType()
    {
        return ShapeTypes.Polygon;
    }

    /// <summary>
    /// Copy vertices. This assumes the vertices define a convex polygon. It is assumed
    /// that the exterior is the the right of each edge.
    /// </summary>
    /// <param name="vertices"> The vertices to copy. </param>
    public void Set( Vector2[] vertices )
    {
        float[] verts = new float[ vertices.Length * 2 ];

        for ( int i = 0, j = 0; i < vertices.Length * 2; i += 2, j++ )
        {
            verts[ i ]     = vertices[ j ].X;
            verts[ i + 1 ] = vertices[ j ].Y;
        }

        jniSet( Addr, verts, 0, verts.Length );
    }

    /// <summary>
    /// Copy vertices from the given float array. It is assumed the vertices are in x,y
    /// order and define a convex polygon. It is assumed that the exterior is the the
    /// right of each edge.
    /// </summary>
    /// <param name="vertices"> The vertices to copy. </param>
    public void Set( float[] vertices )
    {
        jniSet( Addr, vertices, 0, vertices.Length );
    }

    /// <summary>
    /// Copy vertices from the given float array, taking into account the offset and
    /// length. It is assumed the vertices are in x,y order and define a convex polygon.
    /// It is assumed that the exterior is the the right of each edge.
    /// </summary>
    /// <param name="vertices"> The vertices to copy. </param>
    /// <param name="offset"> The offset into the vertices array. </param>
    /// <param name="len"> The number of vertices to copy. </param>
    public void Set( float[] vertices, int offset, int len )
    {
        jniSet( Addr, vertices, offset, len );
    }

    /// <summary>
    /// Build vertices to represent an axis-aligned box.
    /// </summary>
    /// <param name="hx"> the half-width. </param>
    /// <param name="hy"> the half-height. </param>
    public void SetAsBox( float hx, float hy )
    {
        jniSetAsBox( Addr, hx, hy );
    }

    /// <summary>
    /// Build vertices to represent an oriented box.
    /// </summary>
    /// <param name="hx"> the half-width. </param>
    /// <param name="hy"> the half-height. </param> 
    /// <param name="center"> the center of the box in local coordinates. </param>
    /// <param name="angle"> the rotation in radians of the box in local coordinates. </param>
    public void SetAsBox( float hx, float hy, Vector2 center, float angle )
    {
        jniSetAsBox( Addr, hx, hy, center.X, center.Y, angle );
    }

    /// <summary>
    /// Returns the number of vertices
    /// </summary>
    public int GetVertexCount()
    {
        return jniGetVertexCount( Addr );
    }

    /// <summary>
    /// Returns the vertex at the given position.
    /// </summary>
    /// <param name="index"> the index of the vertex 0 &lt;= index &lt; getVertexCount( )  </param>
    /// <param name="vertex"> vertex </param>
    public void GetVertex( int index, Vector2 vertex )
    {
        jniGetVertex( Addr, index, _verts );
        vertex.X = _verts[ 0 ];
        vertex.Y = _verts[ 1 ];
    }

    // ========================================================================
    // ========================================================================
    
    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSet( long addr, float[] verts, int offset, int len );
    /*
        b2PolygonShape* poly = (b2PolygonShape*)addr;
        int numVertices = len / 2;
        b2Vec2* verticesOut = new b2Vec2[numVertices];
        for(int i = 0; i < numVertices; i++) {
            verticesOut[i] = b2Vec2(verts[(i<<1) + offset], verts[(i<<1) + offset + 1]);
        }
        poly->Set(verticesOut, numVertices);
        delete[] verticesOut;
     */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetAsBox( long addr, float hx, float hy );
    /*
        b2PolygonShape* poly = (b2PolygonShape*)addr;
        poly->SetAsBox(hx, hy);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetAsBox( long addr, float hx, float hy, float centerX, float centerY, float angle );
    /*
        b2PolygonShape* poly = (b2PolygonShape*)addr;
        poly->SetAsBox( hx, hy, b2Vec2( centerX, centerY ), angle );
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern int jniGetVertexCount( long addr );
    /*
        b2PolygonShape* poly = (b2PolygonShape*)addr;
        return poly->GetVertexCount();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetVertex( long addr, int index, float[] verts );
    /*
        b2PolygonShape* poly = (b2PolygonShape*)addr;
        const b2Vec2 v = poly->GetVertex( index );
        verts[0] = v.x;
        verts[1] = v.y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long NewPolygonShape();
    /*
        b2PolygonShape* poly = new b2PolygonShape();
        return (jlong)poly;
    */
}

// ============================================================================
// ============================================================================
