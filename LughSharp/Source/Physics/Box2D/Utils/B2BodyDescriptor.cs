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

using LughSharp.Source.Physics.Box2D.Shapes;

namespace LughSharp.Source.Physics.Box2D.Utils;

/// <summary>
/// Represents a descriptor for the properties of a 2D physics body, including
/// information for collision detection and physical interactions.
/// </summary>
[PublicAPI]
public class B2BodyDescriptor : IDisposable
{
    public PhysicsBodyType BodyType    { get; set; }
    public Shape?          Shape       { get; set; }
    public CollisionFilter Filter      { get; set; }
    public float           Density     { get; set; }
    public float           Friction    { get; set; }
    public float           Restitution { get; set; }

    // ========================================================================

    /// <summary>
    /// Default constructor. Initialises the body descriptor with default values.
    /// <para>
    /// <li><c>BodyType</c> is set to <see cref="PhysicsBodyType.Dynamic"/>.</li>
    /// <li><c>Shape</c> is set to <c>null</c>.</li>
    /// <li><c>Filter</c> is initialised to a new <see cref="CollisionFilter"/> passing
    /// bodyCategory: 0, collidesWith: 0, sensor: false as parameters.</li>
    /// <li><c>Density</c> is set to <see cref="PhysicsConstants.DefaultDensity"/>.</li>
    /// <li><c>Friction</c> is set to <see cref="PhysicsConstants.DefaultFriction"/>.</li>
    /// <li><c>Restitution</c> is set to <see cref="PhysicsConstants.DefaultRestitution"/>.</li>
    /// </para>
    /// </summary>
    public B2BodyDescriptor()
    {
        BodyType    = PhysicsBodyType.Dynamic;
        Shape       = null;
        Filter      = new CollisionFilter( 0, 0, false );
        Density     = PhysicsConstants.DefaultDensity;
        Friction    = PhysicsConstants.DefaultFriction;
        Restitution = PhysicsConstants.DefaultRestitution;
    }

    /// <summary>
    /// Represents a descriptor for configuring a physical body in the Box2D-based physics engine.
    /// This class is used to define the properties of a physical body such as its type, shape,
    /// collision-filtering rules, density, friction, and restitution.
    /// </summary>
    /// <param name="bodyType"> The type of the physical body. </param>
    /// <param name="shape"> The shape of the physical body. </param>
    /// <param name="filter"> The collision filtering rules for the physical body. </param>
    /// <param name="density"> The density of the physical body. </param>
    /// <param name="friction"> The friction of the physical body. </param>
    /// <param name="restitution"> The restitution of the physical body. </param>
    public B2BodyDescriptor( PhysicsBodyType bodyType,
                             Shape shape,
                             CollisionFilter filter,
                             float density,
                             float friction,
                             float restitution )
    {
        BodyType    = bodyType;
        Shape       = shape;
        Filter      = filter;
        Density     = density;
        Friction    = friction;
        Restitution = restitution;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose( true );
        GC.SuppressFinalize( this );
    }

    /// <summary>
    /// Releases all resources used by the <see cref="B2BodyDescriptor"/> instance.
    /// </summary>
    /// <param name="disposing">
    /// A bool value indicating whether the method is being called directly
    /// or by the runtime during finalization. <c>true</c> if called directly;
    /// <c>false</c> if called during finalization.
    /// </param>
    protected virtual void Dispose( bool disposing )
    {
        if ( disposing )
        {
        }
    }

    /// <inheritdoc />
    ~B2BodyDescriptor()
    {
        Dispose( false );
    }
}

// ============================================================================
// ============================================================================
