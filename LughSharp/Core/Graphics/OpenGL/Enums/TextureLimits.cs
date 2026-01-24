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

using JetBrains.Annotations;

namespace LughSharp.Core.Graphics.OpenGL.Enums;

[PublicAPI]
public enum TextureLimits
{
    // The above would return a value such as 16 or 32 or above. That is the number of image samplers that your GPU supports in the fragment shader.
    MaxTextureImageUnits = IGL.GL_MAX_TEXTURE_IMAGE_UNITS,

    // The following is for the vertex shader (available since GL 2.0). This might return 0 for certain GPUs.
    MaxVertexTextureImageUnits = IGL.GL_MAX_VERTEX_TEXTURE_IMAGE_UNITS,

    // The following is for the geometry shader (available since GL 3.2)
    MaxGeometryTextureImageUnits = IGL.GL_MAX_GEOMETRY_TEXTURE_IMAGE_UNITS,

    // The following is VS + GS + FS (available since GL 2.0)
    MaxCombinedTextureImageUnits = IGL.GL_MAX_COMBINED_TEXTURE_IMAGE_UNITS,

    // The following is the number of texture coordinates available which usually is 8
    MaxTextureCoords = IGL.GL_MAX_TEXTURE_COORDS,
}