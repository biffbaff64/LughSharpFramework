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
public enum TextureTarget
{
    Texture1D               = IGL.GL_TEXTURE_1D,
    Texture2D               = IGL.GL_TEXTURE_2D,
    Texture3D               = IGL.GL_TEXTURE_3D,
    Texture1DArray          = IGL.GL_TEXTURE_1D_ARRAY,
    Texture2DArray          = IGL.GL_TEXTURE_2D_ARRAY,
    Texture2DMultisample    = IGL.GL_TEXTURE_2D_MULTISAMPLE,
    TextureCubeMapPositiveX = IGL.GL_TEXTURE_CUBE_MAP_POSITIVE_X,
    TextureCubeMapNegativeX = IGL.GL_TEXTURE_CUBE_MAP_NEGATIVE_X,
    TextureCubeMapPositiveY = IGL.GL_TEXTURE_CUBE_MAP_POSITIVE_Y,
    TextureCubeMapNegativeY = IGL.GL_TEXTURE_CUBE_MAP_NEGATIVE_Y,
    TextureCubeMapPositiveZ = IGL.GL_TEXTURE_CUBE_MAP_POSITIVE_Z,
    TextureCubeMapNegativeZ = IGL.GL_TEXTURE_CUBE_MAP_NEGATIVE_Z,
}

[PublicAPI]
public static class TextureTargetExtensions
{
    /// <summary>
    /// Checks if a given integer GL target is a valid member of the TextureTarget enum.
    /// </summary>
    /// <param name="target">This is the instance (or value) of the enum being extended.</param>
    /// <param name="glTarget">The integer GL value to validate.</param>
    /// <returns>True if the integer value maps to a named enum member, false otherwise.</returns>
    public static bool IsValid( this TextureTarget target, int glTarget )
    {
        // Check if the integer value is defined in the enum's underlying type
        if ( Enum.IsDefined( typeof( TextureTarget ), glTarget ) )
        {
            return true;
        }

        return false;
    }
}

// ========================================================================
// ========================================================================