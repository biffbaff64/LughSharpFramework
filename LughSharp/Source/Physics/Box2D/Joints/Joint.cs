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

namespace LughSharp.Source.Physics.Box2D.Joints;

[PublicAPI]
public abstract class Joint
{
    public long Addr { get; }

    public object?   UserData   { get; set; }
    public JointEdge JointEdgeA { get; set; } = null!;
    public JointEdge JointEdgeB { get; set; } = null!;

    private readonly World _world;
    private readonly Vector2 _anchorA = new();
    private readonly Vector2 _anchorB = new();
    private readonly Vector2 _reactionForce = new();

    /** temporary float array **/
    private readonly float[] _tmp = new float[ 2 ];

    // ========================================================================

    /// <summary>
    /// Constructs a new joint
    /// </summary>
    /// <param name="world"> the world </param>
    /// <param name="addr"> the address of the joint </param>
    protected Joint( World world, long addr )
    {
        this._world = world;
        this.Addr   = addr;
    }

    /// <summary>
    /// Get the type of the concrete joint.
    /// </summary>
    public JointDef.JointType GetJointType()
    {
        int type = jniGetJointType( Addr );

        if ( type > 0 && type < JointDef.ValueTypes.Length )
        {
            return JointDef.ValueTypes[ type ];
        }

        return JointDef.JointType.Unknown;
    }

    /// <summary>
    /// Get the first body attached to this joint.
    /// </summary>
    public Body GetBodyA()
    {
        return _world.Bodies[ jniGetBodyA( Addr ) ];
    }

    /// <summary>
    /// Get the second body attached to this joint.
    /// </summary>
    public Body GetBodyB()
    {
        return _world.Bodies[ jniGetBodyB( Addr ) ];
    }

    /// <summary>
    /// Get the anchor point on bodyA in world coordinates.
    /// </summary>
    public Vector2 GetAnchorA()
    {
        jniGetAnchorA( Addr, _tmp );

        _anchorA.X = _tmp[ 0 ];
        _anchorA.Y = _tmp[ 1 ];

        return _anchorA;
    }

    /// <summary>
    /// Get the anchor point on bodyB in world coordinates.
    /// </summary>
    public Vector2 GetAnchorB()
    {
        jniGetAnchorB( Addr, _tmp );

        _anchorB.X = _tmp[ 0 ];
        _anchorB.Y = _tmp[ 1 ];

        return _anchorB;
    }

    /// <summary>
    /// Gets whether the two bodies should collide.
    /// </summary>
    public bool GetCollideConnected()
    {
        return jniGetCollideConnected( Addr );
    }

    /// <summary>
    /// Get the reaction force on body2 at the joint anchor in Newtons.
    /// </summary>
    public Vector2 GetReactionForce( float invDt )
    {
        jniGetReactionForce( Addr, invDt, _tmp );
        
        _reactionForce.X = _tmp[ 0 ];
        _reactionForce.Y = _tmp[ 1 ];

        return _reactionForce;
    }

    /// <summary>
    /// Get the reaction torque on body2 in N*m.
    /// </summary>
    public float GetReactionTorque( float invDt )
    {
        return jniGetReactionTorque( Addr, invDt );
    }

    /// <summary>
    /// Short-cut function to determine if either body is inactive.
    /// </summary>
    public bool IsActive()
    {
        return jniIsActive( Addr );
    }

    // ========================================================================
    // ========================================================================
    
    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern int jniGetJointType( long addr );
    /*
        b2Joint* joint = (b2Joint*)addr;
        return joint->GetType();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long jniGetBodyA( long addr );
    /*
        b2Joint* joint = (b2Joint*)addr;
        return (jlong)joint->GetBodyA();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern long jniGetBodyB( long addr );
    /*
        b2Joint* joint = (b2Joint*)addr;
        return (jlong)joint->GetBodyB();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetAnchorA( long addr, float[] anchorA );
    /*
        b2Joint* joint = (b2Joint*)addr;
        b2Vec2 a = joint->GetAnchorA();
        anchorA[0] = a.x;
        anchorA[1] = a.y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetAnchorB( long addr, float[] anchorB );
    /*
        b2Joint* joint = (b2Joint*)addr;
        b2Vec2 a = joint->GetAnchorB();
        anchorB[0] = a.x;
        anchorB[1] = a.y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern bool jniGetCollideConnected( long addr );
    /*
        b2Joint* joint = (b2Joint*) addr;
        return joint->GetCollideConnected();
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern void jniGetReactionForce( long addr, float invDt, float[] reactionForce );
    /*
        b2Joint* joint = (b2Joint*)addr;
        b2Vec2 f = joint->GetReactionForce(inv_dt);
        reactionForce[0] = f.x;
        reactionForce[1] = f.y;
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern float jniGetReactionTorque( long addr, float invDt );
    /*
        b2Joint* joint = (b2Joint*)addr;
        return joint->GetReactionTorque(inv_dt);
    */

    [DllImport( Box2D.Box2DDllFile, EntryPoint = "???", CallingConvention = CallingConvention.Cdecl )]
    private static extern bool jniIsActive( long addr );
    /*
        b2Joint* joint = (b2Joint*)addr;
        return joint->IsActive();
    */
}

// ============================================================================
// ============================================================================
