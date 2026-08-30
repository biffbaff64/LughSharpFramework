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

namespace LughSharp.Source.Physics.Box2D;

[PublicAPI]
public class Manifold( long a )
{
    public enum ManifoldType
    {
        Circle,
        FaceA,
        FaceB
    }

    public long Addr { get; set; } = a;

    private static readonly ManifoldPoint[] _points = new[]
    {
        new ManifoldPoint(),
        new ManifoldPoint()
    };

    private readonly Vector2 _localNormal = new();
    private readonly Vector2 _localPoint  = new();
    private readonly int[]   _tmpInt      = new int[ 2 ];
    private readonly float[] _tmpFloat    = new float[ 4 ];

    // ========================================================================

    public ManifoldType GetManifoldType()
    {
        int type = jniGetType( Addr );

        if ( type == 0 ) return ManifoldType.Circle;
        if ( type == 1 ) return ManifoldType.FaceA;
        if ( type == 2 ) return ManifoldType.FaceB;

        return ManifoldType.Circle;
    }

    public int GetPointCount()
    {
        return jniGetPointCount( Addr );
    }

    public Vector2 GetLocalNormal()
    {
        jniGetLocalNormal( Addr, _tmpFloat );

        _localNormal.Set( _tmpFloat[ 0 ], _tmpFloat[ 1 ] );

        return _localNormal;
    }

    public Vector2 GetLocalPoint()
    {
        jniGetLocalPoint( Addr, _tmpFloat );

        _localPoint.Set( _tmpFloat[ 0 ], _tmpFloat[ 1 ] );

        return _localPoint;
    }

    public ManifoldPoint[] GetPoints()
    {
        int count = jniGetPointCount( Addr );

        for ( int i = 0; i < count; i++ )
        {
            int           contactID = jniGetPoint( Addr, _tmpFloat, i );
            ManifoldPoint point     = _points[ i ];
            point.ContactID = contactID;
            point.LocalPoint.Set( _tmpFloat[ 0 ], _tmpFloat[ 1 ] );
            point.NormalImpulse  = _tmpFloat[ 2 ];
            point.TangentImpulse = _tmpFloat[ 3 ];
        }

        return _points;
    }

    // ========================================================================
    // ========================================================================
    
    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern int jniGetType( long addr );
    /*
        b2Manifold* manifold = (b2Manifold*)addr;
        return manifold->type;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern int jniGetPointCount( long addr );
    /*
        b2Manifold* manifold = (b2Manifold*)addr;
        return manifold->pointCount;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLocalNormal( long addr, float[] values );
    /*
        b2Manifold* manifold = (b2Manifold*)addr;
        values[0] = manifold->localNormal.x;
        values[1] = manifold->localNormal.y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetLocalPoint( long addr, float[] values );
    /*
        b2Manifold* manifold = (b2Manifold*)addr;
        values[0] = manifold->localPoint.x;
        values[1] = manifold->localPoint.y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern int jniGetPoint( long addr, float[] values, int idx );
    /*
        b2Manifold* manifold = (b2Manifold*)addr;

        values[0] = manifold->points[idx].localPoint.x;
        values[1] = manifold->points[idx].localPoint.y;
        values[2] = manifold->points[idx].normalImpulse;
        values[3] = manifold->points[idx].tangentImpulse;

        return (jint)manifold->points[idx].id.key;
    */

    // ========================================================================
    // ========================================================================

    public class ManifoldPoint
    {
        public Vector2 LocalPoint     { get; set; } = new();
        public float   NormalImpulse  { get; set; }
        public float   TangentImpulse { get; set; }
        public int     ContactID      { get; set; }

        // ====================================================================

        public override string ToString()
        {
            return $"id: {ContactID}, {LocalPoint}, {NormalImpulse}, {TangentImpulse}";
        }
    }
}

// ============================================================================
// ============================================================================
