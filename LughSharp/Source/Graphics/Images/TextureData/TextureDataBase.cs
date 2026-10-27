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

namespace LughSharp.Source.Graphics.Images.TextureData;

/// <summary>
/// Base class for texture data classes such as <see cref="FileTextureData"/>,
/// <see cref="FloatTextureData"/>, <see cref="GLOnlyTextureData"/>,
/// <see cref="PixmapTextureData"/>, <see cref="MipMapTextureData"/>,
/// <see cref="KtxTextureData"/>.
/// </summary>
[PublicAPI]
public abstract class TextureDataBase : ITextureData
{
    /// <summary>
    /// Gets a value indicating whether the object is managed.
    /// </summary>
    public virtual bool IsManaged { get; set; }

    /// <summary>
    /// Returns the <see cref="ITextureData.TextureDataType"/>.
    /// Possible values are <see cref="ITextureData.TextureType.Pixmap"/> and
    /// <see cref="ITextureData.TextureType.Custom"/>.
    /// </summary>
    public virtual ITextureData.TextureType TextureDataType { get; set; }

    /// <summary>
    /// Returns whether the TextureData is prepared or not.
    /// See <see cref="ITextureData.Prepare"/>.
    /// </summary>
    public virtual bool IsPrepared { get; set; }

    /// <summary>
    /// Returns whether the TextureData is owned or not.
    /// </summary>
    public virtual bool IsOwned { get; set; }

    /// <summary>
    /// Returns whether to generate mipmaps or not.
    /// </summary>
    public virtual bool UseMipMaps { get; set; }

    /// <summary>
    /// Returns the width of the pixel data.
    /// </summary>
    public virtual int Width { get; set; }

    /// <summary>
    /// Returns the height of the pixel data.
    /// </summary>
    public virtual int Height { get; set; }

    /// <summary>
    /// The number of bits per pixel.
    /// </summary>
    public virtual int BitDepth { get; set; }

    /// <summary>
    /// The number of bytes per pixel.
    /// </summary>
    public virtual int BytesPerPixel { get; set; }

    // ========================================================================
    
    /// <summary>
    /// Returns the Pixel Format of the pixel data.
    /// <para>
    /// The default implementation in the base class returns <c><see cref="LughFormat.RGBA8888"/></c>.
    /// </para>
    /// </summary>
    public virtual int GetPixelFormat()
    {
        return LughFormat.RGBA8888;
    }

    /// <summary>
    /// Returns whether the caller of <see cref="ITextureData.ConsumePixmap"/> should
    /// dispose the Pixmap returned by <see cref="ITextureData.ConsumePixmap"/>.
    /// <para>
    /// The default implementation in the base class returns <c>false</c>.
    /// </para>
    /// </summary>
    public virtual bool ShouldDisposePixmap()
    {
        return false;
    }

    /// <summary>
    /// Prepares the TextureData for a call to <see cref="ITextureData.ConsumePixmap"/> or
    /// <see cref="ITextureData.ConsumeCustomData"/>. This method can be called from a non
    /// OpenGL thread and should not interact with OpenGL.
    /// <para>
    /// The default implementation in the base class does nothing.
    /// </para>
    /// </summary>
    public virtual void Prepare()
    {
    }

    /// <summary>
    /// Returns the <see cref="Pixmap"/> for upload by Texture.
    /// <para>
    /// A call to <see cref="ITextureData.Prepare"/> must precede a call to this method. Any
    /// internal data structures created in <see cref="ITextureData.Prepare"/> should be
    /// disposed of here.
    /// </para>
    /// <para>
    /// The default implementation in the base class returns <c>null</c>.
    /// </para>
    /// </summary>
    /// <returns> the pixmap.</returns>
    public virtual Pixmap? ConsumePixmap()
    {
        return null;
    }

    /// <summary>
    /// Uploads the pixel data to the OpenGL ES texture. The caller must bind an
    /// OpenGL ES texture. A call to <see cref="ITextureData.Prepare"/> must preceed a call
    /// to this method.
    /// <para>
    /// Any internal data structures created in <see cref="ITextureData.Prepare"/> should be
    /// disposed of here.
    /// </para>
    /// <para>
    /// The default implementation in the base class does nothing.
    /// </para>
    /// </summary>
    public virtual void ConsumeCustomData( int target )
    {
    }

    /// <summary>
    /// Dumps the internal state of the TextureData to the log.
    /// <para>
    /// The default implementation in the base class does nothing.
    /// </para>
    /// </summary>
    public virtual void DebugPrint()
    {
    }
}

// ============================================================================
// ============================================================================
