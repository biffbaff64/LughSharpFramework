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
public class EdgeShape : Shape
{
    private float[] _vertex = new float[ 2 ];

    // ========================================================================
    
    public EdgeShape()
    {
        addr = NewEdgeShape();
    }

    EdgeShape( long addr )
    {
        this.addr = addr;
    }

    /// <summary>
    /// Get the type of this shape. You can use this to down cast to the concrete shape.
    /// </summary>
    /// <returns> The shape type. </returns>
    public override ShapeTypes GetShapeType()
    {
        return ShapeTypes.Edge;
    }

    /** Set this as an isolated edge. */
    public void Set( Vector2 v1, Vector2 v2 )
    {
        Set( v1.X, v1.Y, v2.X, v2.Y );
    }

    /** Set this as an isolated edge. */
    public void Set( float v1X, float v1Y, float v2X, float v2Y )
    {
        jniSet( addr, v1X, v1Y, v2X, v2Y );
    }

    public void GetVertex1( Vector2 vec )
    {
        jniGetVertex1( addr, _vertex );
        vec.X = _vertex[ 0 ];
        vec.Y = _vertex[ 1 ];
    }

    public void GetVertex2( Vector2 vec )
    {
        jniGetVertex2( addr, _vertex );
        vec.X = _vertex[ 0 ];
        vec.Y = _vertex[ 1 ];
    }

    public void GetVertex0( Vector2 vec )
    {
        jniGetVertex0( addr, _vertex );
        vec.X = _vertex[ 0 ];
        vec.Y = _vertex[ 1 ];
    }

    public void SetVertex0( Vector2 vec )
    {
        jniSetVertex0( addr, vec.X, vec.Y );
    }

    public void SetVertex0( float x, float y )
    {
        jniSetVertex0( addr, x, y );
    }

    public void GetVertex3( Vector2 vec )
    {
        jniGetVertex3( addr, _vertex );
        vec.X = _vertex[ 0 ];
        vec.Y = _vertex[ 1 ];
    }

    public void SetVertex3( Vector2 vec )
    {
        jniSetVertex3( addr, vec.X, vec.Y );
    }

    public void SetVertex3( float x, float y )
    {
        jniSetVertex3( addr, x, y );
    }

    public bool HasVertex0()
    {
        return jniHasVertex0( addr );
    }

    public void SetHasVertex0( bool hasVertex0 )
    {
        jniSetHasVertex0( addr, hasVertex0 );
    }

    public bool HasVertex3()
    {
        return jniHasVertex3( addr );
    }

    public void SetHasVertex3( bool hasVertex3 )
    {
        jniSetHasVertex3( addr, hasVertex3 );
    }

    // ========================================================================
    // ========================================================================

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSet( long addr, float v1X, float v1Y, float v2X, float v2Y );
    /*
        b2EdgeShape* edge = (b2EdgeShape*)addr;
        edge->Set(b2Vec2(v1x, v1y), b2Vec2(v2x, v2y));
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetVertex1( long addr, float[] vertex );
    /*
        b2EdgeShape* edge = (b2EdgeShape*)addr;
        vertex[0] = edge->m_vertex1.x;
        vertex[1] = edge->m_vertex1.y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetVertex2( long addr, float[] vertex );
    /*
        b2EdgeShape* edge = (b2EdgeShape*)addr;
        vertex[0] = edge->m_vertex2.x;
        vertex[1] = edge->m_vertex2.y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetVertex0( long addr, float[] vertex ); /*
        b2EdgeShape* edge = (b2EdgeShape*)addr;
        vertex[0] = edge->m_vertex0.x;
        vertex[1] = edge->m_vertex0.y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetVertex0( long addr, float x, float y );
    /*
        b2EdgeShape* edge = (b2EdgeShape*)addr;
        edge->m_vertex0.x = x;
        edge->m_vertex0.y = y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetVertex3( long addr, float[] vertex );
    /*
        b2EdgeShape* edge = (b2EdgeShape*)addr;
        vertex[0] = edge->m_vertex3.x;
        vertex[1] = edge->m_vertex3.y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetVertex3( long addr, float x, float y );
    /*
        b2EdgeShape* edge = (b2EdgeShape*)addr;
        edge->m_vertex3.x = x;
        edge->m_vertex3.y = y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern bool jniHasVertex0( long addr );
    /*
        b2EdgeShape* edge = (b2EdgeShape*)addr;
        return edge->m_hasVertex0;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetHasVertex0( long addr, bool hasVertex0 );
    /*
        b2EdgeShape* edge = (b2EdgeShape*)addr;
        edge->m_hasVertex0 = hasVertex0;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern bool jniHasVertex3( long addr );
    /*
        b2EdgeShape* edge = (b2EdgeShape*)addr;
        return edge->m_hasVertex3;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetHasVertex3( long addr, bool hasVertex3 );
    /*
        b2EdgeShape* edge = (b2EdgeShape*)addr;
        edge->m_hasVertex3 = hasVertex3;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long NewEdgeShape();
    /*
        return (jlong)(new b2EdgeShape());
    */
}

// ============================================================================
// ============================================================================
