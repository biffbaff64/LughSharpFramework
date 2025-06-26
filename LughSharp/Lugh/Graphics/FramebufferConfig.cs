// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Red 7 Projects
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// /////////////////////////////////////////////////////////////////////////////

namespace LughSharp.Lugh.Graphics;

/// <summary>
/// Class describing the bits per pixel, depth buffer precision, stencil precision
/// and number of MSAA samples. This is used to configure the framebuffer format
/// when creating the graphics context. It allows you to specify the desired precision
/// for color channels, depth buffer, stencil buffer, and multisample anti-aliasing.
/// </summary>
[PublicAPI]
public class FramebufferConfig
{
    /// <summary>
    /// Number of bits per red color channel.
    /// </summary>
    public int R { get; set; }

    /// <summary>
    /// Number of bits per green color channel.
    /// </summary>
    public int G { get; set; }

    /// <summary>
    /// Number of bits per blue color channel.
    /// </summary>
    public int B { get; set; }

    /// <summary>
    /// Number of bits per alpha channel (transparency).
    /// </summary>
    public int A { get; set; }

    /// <summary>
    /// Number of bits for the depth buffer, used for depth testing in 3D rendering.
    /// Higher values provide more precision for depth comparisons.
    /// </summary>
    public int Depth { get; set; }

    /// <summary>
    /// Number of bits for the stencil buffer, used for advanced rendering effects like masking and outlining.
    /// </summary>
    public int Stencil { get; set; }

    /// <summary>
    /// Number of samples for multi-sample anti-aliasing (MSAA).
    /// MSAA reduces aliasing artifacts (jagged edges) by rendering each pixel multiple times
    /// and averaging the results. Higher sample counts improve anti-aliasing quality but
    /// increase rendering cost.
    /// </summary>
    public int Samples { get; set; }

    /// <summary>
    /// Whether coverage sampling anti-aliasing is used.
    /// Coverage sampling is an alternative anti-aliasing technique that can be more
    /// efficient than MSAA in some cases.
    /// <para>
    /// If coverage sampling is enabled, you might need to clear the coverage
    /// buffer as well during rendering setup.
    /// </para>
    /// </summary>
    public bool CoverageSampling { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"r - {R}, g - {G}, b - {B}, a - {A}, depth - {Depth}, stencil - "
               + $"{Stencil}, num samples - {Samples}, coverage sampling - {CoverageSampling}";
    }
}