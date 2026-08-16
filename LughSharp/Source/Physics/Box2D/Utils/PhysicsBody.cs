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

using LughSharp.Source.Entities;

namespace LughSharp.Source.Physics.Box2D.Utils;

/// <summary>
/// Represents a physics body that encapsulates properties and behaviors
/// for managing a 2D physics entity.
/// </summary>
[PublicAPI]
public class PhysicsBody( Body? body, bool isAlive ) : IDisposable
{
    public Body?      Body         { get; set; } = body;
    public Rectangle? BodyBox      { get; set; } = new();
    public EntityID?  Type         { get; set; } = EntityIDs.Entity;
    public bool       IsAlive      { get; set; } = isAlive;
    public int        ContactCount { get; set; } = 0;
    public short      ContactMask  { get; set; } = 0;

    // ========================================================================

    /// <summary>
    /// Represents a physics body that encapsulates properties and behaviors
    /// related to managing a 2D physics entity in the Box2D physics system.
    /// </summary>
    /// <remarks>
    /// This class provides functionality for managing the state and attributes
    /// of a 2D physics object, including its type, bounding box, contact handling,
    /// and its associated physical representation.
    /// </remarks>
    /// <example>
    /// Create an instance of this class to handle a physics body within the Box2D
    /// physics simulation.
    /// </example>
    public PhysicsBody() : this( null, false )
    {
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Body    = null;
        BodyBox = null;
        Type    = null;
        
        GC.SuppressFinalize( this );
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "PhysicsBody["
             + "body=" + Body
             + ", isAlive=" + IsAlive
             + ", contactCount=" + ContactCount
             + ", [bodyBox=" + ( BodyBox ?? new Rectangle() ) + "]]"
             + ", type=" + Type;
    }
}

// ============================================================================
// ============================================================================
