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
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;

namespace LughSharp.Core.Graphics.Utils;

[PublicAPI]
public class PixmapTextureData : ITextureData
{
    public Pixmap Pixmap        { get; set; }
    public bool   UseMipMaps    { get; set; }
    public bool   DisposePixmap { get; set; }
    public bool   IsManaged     { get; set; }
    public bool   IsPrepared    { get; set; } = true;
    public bool   IsOwned       { get; set; }
    public int    BitDepth      { get; set; }
    public int    BytesPerPixel { get; set; }

    // ========================================================================

    private int _pixelFormat;

    // ========================================================================

    public PixmapTextureData( Pixmap pixmap,
                              int format,
                              bool useMipMaps,
                              bool disposePixmap,
                              bool managed = false )
    {
        Pixmap        = pixmap;
        DisposePixmap = disposePixmap;
        _pixelFormat  = format;
        IsManaged     = managed;
        UseMipMaps    = useMipMaps;
    }

    public ITextureData.TextureType TextureDataType => ITextureData.TextureType.Pixmap;

    /// <returns>
    /// whether the caller of <see cref="ITextureData.ConsumePixmap"/> should dispose the
    /// Pixmap returned by <see cref="ITextureData.ConsumePixmap"/>
    /// </returns>
    public bool ShouldDisposePixmap() => DisposePixmap;

    /// <inheritdoc />
    public int GetPixelFormat() => _pixelFormat;

    /// <inheritdoc />
    public Pixmap ConsumePixmap() => Pixmap;

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

    /// <inheritdoc />
    public void DebugPrint()
    {
    }

    // ========================================================================

    public void ConsumeCustomData( int target )
    {
        throw new RuntimeException( "This TextureData implementation does not upload data itself" );
    }

    public void Prepare()
    {
        Logger.Error( "prepare() must not be called on a PixmapTextureData" +
                      " instance as it is already prepared." );
    }
}

// ============================================================================
// ============================================================================