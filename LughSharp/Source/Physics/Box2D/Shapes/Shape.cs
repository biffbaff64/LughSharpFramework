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

/// <summary>
/// A shape is used for collision detection. You can create a shape however you like.
/// Shapes used for simulation in <see cref="World"/> are created automatically when a
/// <see cref="Fixture"/> is created.
/// <para>
/// Shapes may encapsulate a one or more child shapes.
/// </para>
/// </summary>
/// <remarks>
/// <b>YOU NEED TO DISPOSE SHAPES YOU CREATED YOURSELF AFTER YOU NO LONGER USE THEM!</b>
/// E.g. after calling Body.CreateFixture();
/// </remarks>
[PublicAPI]
public abstract class Shape
{
    /// <summary>
    /// Enum describing the type of a shape
    /// </summary>
    [PublicAPI]
    public enum ShapeTypes
    {
        Circle,
        Edge,
        Polygon,
        Chain,
    };

    // ========================================================================
    
    /** the address of the shape **/
    protected long addr;

    /// <summary>
    /// Get the type of this shape. You can use this to down cast to the concrete shape.
    /// </summary>
    /// <returns> The shape type. </returns>
    public abstract ShapeTypes GetShapeType();

    /// <summary>
    /// Returns the radius of this shape.
    /// </summary>
    public float GetRadius()
    {
        return jniGetRadius( addr );
    }

    /** Sets the radius of this shape */
    public void SetRadius( float radius )
    {
        jniSetRadius( addr, radius );
    }

    /// <summary>
    /// Get the number of child primitives.
    /// </summary>
    public int GetChildCount()
    {
        return jniGetChildCount( addr );
    }

    /// <summary>
    /// Needs to be called when the shape is no longer used, e.g. after a fixture
    /// was created based on the shape.
    /// </summary>
    public void Dispose()
    {
        jniDispose( addr );
    }

    // ========================================================================
    // ========================================================================
    
    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetRadius( long addr );
    /*
        b2Shape* shape = (b2Shape*)addr;
        return shape->m_radius;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetRadius( long addr, float radius );
    /*
        b2Shape* shape = (b2Shape*)addr;
        shape->m_radius = radius;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniDispose( long addr );
    /*
        b2Shape* shape = (b2Shape*)addr;
        delete shape;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    public static extern int jniGetType( long addr );
    /*
        b2Shape* shape = (b2Shape*)addr;
        switch(shape->m_type) {
        case b2Shape::e_circle: return 0;
        case b2Shape::e_edge: return 1;
        case b2Shape::e_polygon: return 2;
        case b2Shape::e_chain: return 3;
        default: return -1;
        }
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern int jniGetChildCount( long addr );
    /*
        b2Shape* shape = (b2Shape*)addr;
        return shape->GetChildCount();
    */
}

// ============================================================================
// ============================================================================
