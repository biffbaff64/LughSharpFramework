// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Richard Ikin.
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

using LughSharp.Lugh.Graphics.G2D;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.Utils;

[PublicAPI]
public class PixmapTextureData : ITextureData
{
    public Pixmap                        Pixmap        { get; set; }
    public bool                          DisposePixmap { get; set; }
    public Gdx2DPixmap.Gdx2DPixmapFormat PixelFormat   { get; set; }
    public bool                          IsManaged     { get; set; }
    public bool                          UseMipMaps    { get; set; }
    public bool                          IsPrepared    { get; set; } = true;

    // ========================================================================

    public PixmapTextureData( Pixmap pixmap,
                              Gdx2DPixmap.Gdx2DPixmapFormat? format,
                              bool useMipMaps,
                              bool disposePixmap,
                              bool managed = false )
    {
        Pixmap        = pixmap;
        DisposePixmap = disposePixmap;
        PixelFormat   = format ?? Gdx2DPixmap.Gdx2DPixmapFormat.Default;
        IsManaged     = managed;
        UseMipMaps    = useMipMaps;
    }

    public ITextureData.TextureType TextureDataType => ITextureData.TextureType.Pixmap;

    /// <returns>
    /// whether the caller of <see cref="ITextureData.ConsumePixmap" /> should dispose the
    /// Pixmap returned by <see cref="ITextureData.ConsumePixmap" />
    /// </returns>
    bool ITextureData.ShouldDisposePixmap()
    {
        return DisposePixmap;
    }

    Pixmap ITextureData.ConsumePixmap()
    {
        return Pixmap;
    }

    public int Width
    {
        get => Pixmap.Width;
        set { }
    }

    public int Height
    {
        get => Pixmap.Height;
        set { }
    }

    // ========================================================================

    public void ConsumeCustomData( int target )
    {
        throw new GdxRuntimeException( "This TextureData implementation does not upload data itself" );
    }

    public void Prepare()
    {
        Logger.Warning( "prepare() must not be called on a PixmapTextureData" +
                        " instance as it is already prepared." );
    }
}