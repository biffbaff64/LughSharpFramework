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
/// Provides default values for various physics-related parameters, such as gravity,
/// friction, restitution, and density. This class is designed to standardize commonly
/// used constants for physics simulation settings.
/// <para>
/// Values for:-
/// <li>
/// ZeroFriction, LowFriction, MediumLowFriction, MediumFriction, HighFriction,
/// DefaultFriction, FullFriction,
/// </li>
/// <li>
/// ZeroRestitution, LowRestitution, MediumLowRestitution, MediumRestitution,
/// HighRestitution, DefaultRestitution, FullRestitution,
/// </li>
/// <li>
/// ZeroDensity, LowDensity, MediumLowDensity, MediumDensity, HighDensity,
/// DefaultDensity, FullDensity,
/// </li>
/// </para>
/// are provided.
/// <para>
/// All values can be modified to suit specific physics simulation requirements.
/// </para>
/// </summary>
[PublicAPI]
public class PhysicsConstants
{
    /// <summary>
    /// Represents a default gravity value of zero, indicating a physics environment
    /// without gravitational influence.
    /// This value can be used to simulate weightlessness or space-like conditions.
    /// </summary>
    public static float ZeroGravity { get; set; } = 0.0f;

    // ----------------------------------------------------

    /// <summary>
    /// Represents a friction coefficient value of zero, indicating a physics environment
    /// without surface resistance or opposing force to motion.
    /// This value can be used to simulate perfectly smooth or frictionless scenarios.
    /// </summary>
    public static float ZeroFriction { get; set; } = 0.0f;

    /// <summary>
    /// Represents the maximum friction value used to simulate surfaces with complete resistance
    /// to sliding motion between contacting objects in a physics environment.
    /// It can be used to model scenarios where no slipping occurs, such as high-traction surfaces.
    /// </summary>
    public static float FullFriction { get; set; } = 1.0f;

    public static float LowFriction       { get; set; } = 0.1f;
    public static float MediumLowFriction { get; set; } = 0.25f;
    public static float MediumFriction    { get; set; } = 0.4f;
    public static float HighFriction      { get; set; } = 0.6f;
    public static float DefaultFriction   { get; set; } = 0.8f;

    // ----------------------------------------------------

    /// <summary>
    /// Specifies a restitution value of zero, resulting in no bounce or energy retention
    /// upon collision. This property is typically used to model perfectly inelastic impacts
    /// where objects do not rebound.
    /// </summary>
    public static float ZeroRestitution { get; set; } = 0.0f;

    /// <summary>
    /// Represents the highest possible restitution value, indicating a perfectly elastic collision
    /// where no kinetic energy is lost during impact.
    /// This value is commonly used in simulations where objects are expected to bounce maximally
    /// upon collision, such as in scenarios involving elastic materials.
    /// </summary>
    public static float FullRestitution { get; set; } = 1.0f;

    public static float LowRestitution       { get; set; } = 0.1f;
    public static float MediumLowRestitution { get; set; } = 0.25f;
    public static float MediumRestitution    { get; set; } = 0.5f;
    public static float HighRestitution      { get; set; } = 0.8f;
    public static float DefaultRestitution   { get; set; } = 0.8f;

    // ----------------------------------------------------

    /// <summary>
    /// Represents a default density value of zero, indicating an object
    /// with no mass per unit volume in the physics simulation.
    /// This value can be used to simulate massless objects or surfaces.
    /// </summary>
    public static float ZeroDensity { get; set; } = 0.0f;

    /// <summary>
    /// Represents the default density value of a physical object in the simulation,
    /// typically used to calculate mass relative to its volume.
    /// This value determines how an object interacts with forces like gravity and collisions.
    /// </summary>
    public static float FullDensity { get; set; } = 1.0f;

    public static float LowDensity       { get; set; } = 0.2f;
    public static float MediumLowDensity { get; set; } = 0.3f;
    public static float MediumDensity    { get; set; } = 0.4f;
    public static float HighDensity      { get; set; } = 0.6f;
    public static float DefaultDensity   { get; set; } = 0.8f;
}

// ============================================================================
// ============================================================================
