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
public class CircleShape : Shape
{
    // 
    private float[] _tmp = new float[ 2 ];
    
    // Returns the position of the shape
    private Vector2 _position = new Vector2();

    public CircleShape()
    {
        Addr = newCircleShape();
    }

    public CircleShape( long addr )
    {
        this.Addr = addr;
    }

    /// <summary>
    /// Get the type of this shape. You can use this to down cast to the concrete shape.
    /// </summary>
    /// <returns> The shape type. </returns>
    public override ShapeTypes GetShapeType()
    {
        return ShapeTypes.Circle;
    }

    public Vector2 GetPosition()
    {
        jniGetPosition( Addr, _tmp );
        _position.X = _tmp[ 0 ];
        _position.Y = _tmp[ 1 ];

        return _position;
    }

    /** Sets the position of the shape */
    public void SetPosition( Vector2 position )
    {
        jniSetPosition( Addr, position.X, position.Y );
    }

    // ========================================================================
    // ========================================================================

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long newCircleShape();
    /*
        return (jlong)(new b2CircleShape( ));
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetPosition( long addr, float[] position );
    /*
        b2CircleShape* circle = (b2CircleShape*)addr;
        position[0] = circle->m_p.x;
        position[1] = circle->m_p.y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniSetPosition( long addr, float positionX, float positionY );
    /*
        b2CircleShape* circle = (b2CircleShape*)addr;
        circle->m_p.x = positionX;
        circle->m_p.y = positionY;
    */
}

// ============================================================================
// ============================================================================
