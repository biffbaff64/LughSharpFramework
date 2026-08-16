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

namespace LughSharp.Source.Physics.Box2D.Utils;

/// <summary>
/// Specifies types of physics bodies used within the physics simulation.
/// See <see cref="BodyBuilder"/> for more information.
/// </summary>
[PublicAPI]
public enum PhysicsBodyType
{
    // --------------------
    None,
    
    // --------------------
    
    /// <summary>
    /// Represents a dynamic physics body that can move and interact with other bodies.
    /// Dynamic bodies have positive mass, and velocity determined by forces applied
    /// to them.
    /// </summary>
    Dynamic,
    DynamicSensor,
    DynamicBouncy,
    DynamicCircle,
    DynamicCircleSensor,
    DynamicPushable,
    DynamicHeavy,
    
    // --------------------
    
    /// <summary>
    /// Represents a kinematic physics body that can move and interact with other bodies.
    /// Kinematic bodies have zero mass and non-zero velocity, set by the user. They are
    /// moved by the solver.
    /// </summary>
    Kinematic,
    KinematicSensor,
    KinematicHeavy,
    
    // --------------------
    
    /// <summary>
    /// Represents a static physics body that does not move and does not interact with
    /// other bodies. Static bodies have zero mass and zero velocity.
    /// <see cref="StaticSensor"/> body types are static bodies that generate contact events.
    /// </summary>
    Static,
    StaticSensor,
}

// ============================================================================
// ============================================================================

