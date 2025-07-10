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

namespace LughSharp.Lugh.Graphics.OpenGL.Enums;

/// <summary>
/// Enumerates different texture filtering methods used to determine how textures
/// are sampled when they are displayed at sizes other than their original resolution.
/// <para>
/// Texture filtering impacts both magnification (enlargement) and minification
/// (reduction) of textures. This is especially important in scenarios where a
/// texture is applied to a surface that appears either much larger or much smaller
/// than the original texture resolution.
/// </para>
/// <para>
/// The available options include various combinations of nearest-neighbor filtering,
/// linear filtering, and mipmap-based filtering. Mipmapping involves precomputing and
/// storing multiple resolutions of the texture to improve performance and visual
/// quality when textures are minified.
/// </para>
/// </summary>
[PublicAPI]
public enum TextureFilterMode : int
{
    /// <summary>
    /// Fetch the nearest texel that best maps to the pixel on screen.
    /// </summary>
    Nearest = IGL.GL_NEAREST,

    /// <summary>
    /// Fetch four nearest texels that best map to the pixel on screen.
    /// </summary>
    Linear = IGL.GL_LINEAR,

    /// <summary>
    /// Applies a linear texture filtering technique where texels are chosen based
    /// on the nearest or interpolated mipmap level, providing a smoother appearance
    /// for textures with varying distances.
    /// </summary>
    MipMap = IGL.GL_LINEAR_MIPMAP_LINEAR,

    /// <summary>
    /// Fetch the best fitting image from the mip map chain based on the pixel/texel
    /// ratio and then sample the texels with a nearest filter.
    /// </summary>
    MipMapNearestNearest = IGL.GL_NEAREST_MIPMAP_NEAREST,

    /// <summary>
    /// Fetch the best fitting image from the mip map chain based on the pixel/texel
    /// ratio and then sample the texels with a linear filter.
    /// </summary>
    MipMapLinearNearest = IGL.GL_LINEAR_MIPMAP_NEAREST,

    /// <summary>
    /// Fetch the two best fitting images from the mip map chain and then sample
    /// the nearest texel from each of the two images, combining them to the final
    /// output pixel.
    /// </summary>
    MipMapNearestLinear = IGL.GL_NEAREST_MIPMAP_LINEAR,

    /// <summary>
    /// Fetch the two best fitting images from the mip map chain and then sample
    /// the four nearest texels from each of the two images, combining them to
    /// the final output pixel.
    /// </summary>
    MipMapLinearLinear = MipMap,
}