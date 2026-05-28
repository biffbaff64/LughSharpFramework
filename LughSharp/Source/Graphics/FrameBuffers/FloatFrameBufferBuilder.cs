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

using LughSharp.Source.Graphics.Images;

namespace LughSharp.Source.Graphics.FrameBuffers;

/// <summary>
/// Provides a builder for creating floating-point frame buffers with configurable 
/// width and height.
/// </summary>
/// <remarks>
/// Use this class to configure and construct instances of FloatFrameBuffer for rendering
/// operations that require high-precision color data. This builder pattern allows for 
/// flexible setup before instantiating the frame buffer.
/// </remarks>
[PublicAPI]
public class FloatFrameBufferBuilder : GLFrameBufferBuilder< GLFrameBuffer< GLTexture > >
{
    /// <summary>
    /// Creates a new FloatFrameBufferBuilder with the specified width and height.
    /// </summary>
    /// <param name="width">The width of the frame buffer.</param>
    /// <param name="height">The height of the frame buffer.</param>
    public FloatFrameBufferBuilder( int width, int height )
        : base( width, height )
    {
    }

    /// <summary>
    /// Creates a new instance of the FloatFrameBuffer class using the current configuration.
    /// </summary>
    /// <returns>
    /// A FloatFrameBuffer instance initialized with the settings defined in this builder.
    /// </returns>
    public override FloatFrameBuffer Build()
    {
        return new FloatFrameBuffer( this );
    }
}

// ============================================================================
// ============================================================================

