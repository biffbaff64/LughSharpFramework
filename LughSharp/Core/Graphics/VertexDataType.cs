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

namespace LughSharp.Core.Graphics;

/// <summary>
/// Specifies the type of vertex data to be used.
/// </summary>
[PublicAPI]
public enum VertexDataType
{
    /// <summary>
    /// Represents a collection of vertices used for rendering geometric shapes or models.
    /// </summary>
    /// <remarks>
    /// A VertexArray typically stores vertex data such as positions, normals, colors, or
    /// texture coordinates for use in graphics rendering operations. The specific usage
    /// and supported vertex attributes may vary depending on the graphics API or framework.
    /// </remarks>
    VertexArray,

    /// <summary>
    /// Represents a buffer object that stores vertex data for use in graphics rendering
    /// operations.
    /// </summary>
    /// <remarks>
    /// A vertex buffer object (VBO) is typically used to efficiently transfer vertex data
    /// to the graphics hardware. This class provides an abstraction for managing the lifecycle
    /// and usage of such buffers in rendering pipelines. Thread safety and usage patterns may
    /// depend on the underlying graphics API.
    /// </remarks>
    VertexBufferObject,
    
    /// <summary>
    /// Represents a vertex buffer object that supports updating a subset of its data.
    /// </summary>
    /// <remarks>
    /// Use this type when you need to efficiently update portions of vertex data without
    /// recreating the entire buffer. This is commonly used in graphics applications where
    /// only part of the vertex data changes frequently.
    /// </remarks>
    VertexBufferObjectSubData,
    
    /// <summary>
    /// Represents a vertex buffer object (VBO) that is associated with a vertex array object
    /// (VAO) for efficient rendering of vertex data.
    /// </summary>
    /// <remarks>
    /// This class is typically used in graphics applications that require high-performance
    /// rendering of complex geometry. By managing both the VBO and its associated VAO, it
    /// simplifies resource management and ensures that vertex attribute configurations are
    /// preserved across draw calls. Thread safety and resource disposal should be considered
    /// when using this class in multi-threaded or long-running applications.
    /// </remarks>
    VertexBufferObjectWithVAO
}

// ============================================================================
// ============================================================================