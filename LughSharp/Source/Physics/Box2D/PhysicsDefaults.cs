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

using JetBrains.Annotations;

namespace LughSharp.Physics2D.Source.Box2D;

[PublicAPI]
public class PhysicsDefaults
{
    public static float ZeroGravity { get; set; } = 0.0f;

    // ----------------------------------------------------
    public static float ZeroFriction      { get; set; } = 0.0f;
    public static float LowFriction       { get; set; } = 0.1f;
    public static float MediumLowFriction { get; set; } = 0.25f;
    public static float MediumFriction    { get; set; } = 0.4f;
    public static float DefaultFriction   { get; set; } = 0.8f;
    public static float FullFriction      { get; set; } = 1.0f;

    // ----------------------------------------------------
    public static float ZeroRestitution      { get; set; } = 0.0f;
    public static float LowRestitution       { get; set; } = 0.1f;
    public static float MediumLowRestitution { get; set; } = 0.25f;
    public static float MediumRestitution    { get; set; } = 0.5f;
    public static float HighRestitution      { get; set; } = 0.8f;
    public static float FullRestitution      { get; set; } = 1.0f;
    public static float DefaultRestitution   { get; set; } = 0.8f;

    // ----------------------------------------------------
    public static float ZeroDensity      { get; set; } = 0.0f;
    public static float LowDensity       { get; set; } = 0.2f;
    public static float MediumLowDensity { get; set; } = 0.4f;
    public static float DefaultDensity   { get; set; } = 0.8f;
    public static float FullDensity      { get; set; } = 1.0f;
}

// ============================================================================
// ============================================================================
