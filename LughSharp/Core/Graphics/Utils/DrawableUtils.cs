// ///////////////////////////////////////////////////////////////////////////////
// MIT License
// 
// Copyright (c) 2024, 2025, 2026 Circa64 Software Projects / Richard Ikin.
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

using LughSharp.Core.Graphics.Images;
using LughSharp.Core.Graphics.OpenGL;
using LughSharp.Core.SceneGraph2D.Utils;

namespace LughSharp.Core.Graphics.Utils;

[PublicAPI]
public class DrawableUtils
{
    /// <summary>
    /// Creates an image of determined size filled with determined color.
    /// </summary>
    /// <param name="width"> The width of an image in pixels. </param>
    /// <param name="height"> The height of an image in pixels. </param>
    /// <param name="color"> The color of the new drawable. </param>
    /// <returns>
    /// A <see cref="ISceneDrawable"/> of determined size filled with determined color.
    /// </returns>
    public static ISceneDrawable GetColoredSceneDrawable( int width, int height, Color color )
    {
        var pixmap = new Pixmap( width, height, LughFormat.RGBA8888 );
        pixmap.FillWithColor( color );

        var drawable = new TextureRegionDrawable( new TextureRegion( new Texture2D( pixmap ) ) );

        pixmap.Dispose();

        return drawable;
    }
}

// ============================================================================
// ============================================================================