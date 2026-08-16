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

/// <summary>
/// This is used to compute contact points in world coordinates. Instances are populated
/// by <c>Contact.GetWorldManifold()</c>;  the backing arrays are reused, so do not cache
/// the returned references.
/// </summary>
[PublicAPI]
public class WorldManifold
{
    /// <summary>
    /// The world normal at the contact points. Same instance returned each call.
    /// </summary>
    public Vector2 NormalValue { get; set; } = new();

    /// <summary>
    /// The world contact points (up to <see cref="NumContactPoints"/>).
    /// </summary>
    public Vector2[] PointsValue { get; set; } = [ new(), new() ];

    /// <summary>
    /// The separation distances at each contact point (negative = penetration).
    /// </summary>
    public float[] SeparationsValue { get; set; } = new float[ 2 ];

    /// <summary>
    /// The number of valid contact points in the world manifold.
    /// </summary>
    public int NumContactPoints { get; set; }
}

// ============================================================================
// ============================================================================
