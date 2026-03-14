// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Circa64 Software Projects
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

namespace LughSharp.Core.Graphics.Utils;

[PublicAPI]
public class VertexConstants
{
    /// <summary>
    /// Enumerates the various types of vertex attribute usage
    /// in a graphics application.
    /// </summary>
    [PublicAPI]
    [Flags]
    public enum Usage
    {
        Position           = 1,
        ColorUnpacked      = 2,
        ColorPacked        = 4,
        Normal             = 8,
        TextureCoordinates = 16,
        Generic            = 32,
        BoneWeight         = 64,
        Tangent            = 128,
        BiNormal           = 256
    }

    public const int PositionComponents = 2;
    public const int ColorComponents    = 1;
    public const int TexcoordComponents = 2;
    public const int NormalComponents   = 3;

    // Number of floats per vertex (x, y, color, u, v)
    public const int VertexSize = PositionComponents + ColorComponents + TexcoordComponents;

    // Vertex Size (in bytes)
    public const int VertexSizeBytes = VertexSize * sizeof( float );

    // Vertex Offsets (in floats)
    public const int PositionOffset = 0;
    public const int ColorOffset    = PositionOffset + PositionComponents;
    public const int TexcoordOffset = ColorOffset + ColorComponents;
}