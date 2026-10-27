// /////////////////////////////////////////////////////////////////////////////
// //  MIT License
// //
// //  Copyright (c) 2024 Richard Ikin
// //
// //  Permission is hereby granted, free of charge, to any person obtaining a copy
// //  of this software and associated documentation files (the "Software"), to deal
// //  in the Software without restriction, including without limitation the rights
// //  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// //  copies of the Software, and to permit persons to whom the Software is
// //  furnished to do so, subject to the following conditions:
// //
// //  The above copyright notice and this permission notice shall be included in all
// //  copies or substantial portions of the Software.
// //
// //  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// //  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// //  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// //  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// //  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// //  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// //  SOFTWARE.
// /////////////////////////////////////////////////////////////////////////////

namespace LughSharp.Source.Graphics.OpenGL.Enums;

[PublicAPI]
public enum ViewClass
{
    Map128Bits      = IGL.GLViewClass128Bits,
    Map96Bits       = IGL.GLViewClass96Bits,
    Map64Bits       = IGL.GLViewClass64Bits,
    Map48Bits       = IGL.GLViewClass48Bits,
    Map32Bits       = IGL.GLViewClass32Bits,
    Map24Bits       = IGL.GLViewClass24Bits,
    Map16Bits       = IGL.GLViewClass16Bits,
    Map8Bits        = IGL.GLViewClass8Bits,
    MapS3TCDxt1RGB  = IGL.GLViewClassS3TCDxt1RGB,
    MapS3TCDxt1RGBA = IGL.GLViewClassS3TCDxt1RGBA,
    MapS3TCDxt3RGBA = IGL.GLViewClassS3TCDxt3RGBA,
    MapS3TCDxt5RGBA = IGL.GLViewClassS3TCDxt5RGBA,
    MapRGTC1Red     = IGL.GLViewClassRGTC1Red,
    MapRGTC2Rg      = IGL.GLViewClassRGTC2Rg,
    MapBPTCUnorm    = IGL.GLViewClassBPTCUnorm,
    MapBPTCFloat    = IGL.GLViewClassBPTCFloat,
}

// ============================================================================
// ============================================================================
