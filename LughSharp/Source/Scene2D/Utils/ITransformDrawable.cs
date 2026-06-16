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

using LughSharp.Source.Graphics.G2D;

namespace LughSharp.Source.Scene2D.Utils;

/// <summary>
/// A drawable that supports scale and rotation.
/// </summary>
[PublicAPI]
public interface ITransformDrawable : ISceneDrawable
{
    /// <summary>
    /// Draws the specified region with transformations applied.
    /// </summary>
    /// <param name="batch">The batch used to render the region.</param>
    /// <param name="region">The rectangular region to draw.</param>
    /// <param name="origin">The origin point of the transformations.</param>
    /// <param name="scale">The scaling factors applied to the region.</param>
    /// <param name="rotation">The rotation angle, in degrees, to apply to the region.</param>
    void Draw( IBatch batch, GRect region, Point2D origin, Point2D scale, float rotation );
}

// ============================================================================
// ============================================================================