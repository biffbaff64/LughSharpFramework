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

using LughSharp.Lugh.Graphics.OpenGL.Bindings;

namespace LughSharp.Lugh.Graphics.Images;

[PublicAPI]
public class Texture2D : IDrawable, IDisposable
{
    /// <summary>
    /// The OpenGL target for the texture. A GL target, in the context of OpenGL (and
    /// by extension, OpenGL ES), refers to the type of texture object being manipulated
    /// or bound. It specifies which texture unit or binding point the subsequent texture
    /// operations will affect.
    /// <para>
    /// Common GL targets include:
    /// <li>GL_TEXTURE_2D:       For standard 2D textures</li>
    /// <li>GL_TEXTURE_3D:       For 3D textures</li>
    /// <li>GL_TEXTURE_CUBE_MAP: For cube map textures</li>
    /// <li>GL_TEXTURE_2D_ARRAY: For 2D texture arrays</li>
    /// </para>
    /// </summary>
    public int GLTarget { get; private set; }

    /// <summary>
    /// A GL texture handle, or OpenGL texture handle, is a unique identifier assigned
    /// by OpenGL to represent a texture object in memory. It is used to reference and
    /// manipulate a specific texture in OpenGL operations.
    /// <para>
    /// The handle is typically created using a function like <see cref="IGLBindings.GenTextures(int)"/>.
    /// Once created, this handle is used in various OpenGL functions to bind, modify,
    /// or delete the texture. The handle remains valid until explicitly deleted using
    /// a function like <see cref="IGLBindings.DeleteTextures(uint[])"/>
    /// </para>
    /// </summary>
    public uint GLTextureHandle { get; set; }

    /// <summary>
    /// An anisotropic filter level is a technique used in computer graphics to improve
    /// the quality of textures when they are viewed at oblique angles or from a distance.
    /// <li>
    /// Purpose: It enhances texture detail and sharpness, especially for surfaces that
    /// are at an angle to the viewer, reducing blurring and aliasing artifacts.
    /// </li>
    /// <li>
    /// How it works: Anisotropic filtering samples the texture more times along the axis
    /// of highest compression (the direction where the texture is most skewed), providing
    /// better quality than standard mipmapping.
    /// </li>
    /// <li>
    /// Levels: The "level" refers to the maximum number of samples taken for each texel.
    /// Higher levels provide better quality but are more computationally expensive.
    /// </li>
    /// <li>
    /// Range: Typically, anisotropic filter levels range from 1 (no anisotropic filtering)
    /// to 16 (maximum quality), though some hardware may support higher levels.
    /// </li>
    /// <li>
    /// Performance impact: Higher levels of anisotropic filtering can impact performance,
    /// so games and applications often allow users to adjust this setting.
    /// </li>
    /// </summary>
    public float AnisotropicFilterLevel { get; private set; } = 1.0f;

    /// <summary>
    /// Texture depth in computer graphics typically refers to one of two concepts:
    /// <li>
    /// For 3D textures: The depth represents the third dimension of the texture. In this
    /// context, a texture is a three-dimensional array of texels (texture elements),
    /// where depth is the size of the texture in the z-axis.
    /// </li>
    /// <li>
    /// For color depth: It refers to the number of bits used to represent the color of
    /// each texel. Higher color depth allows for more colors and smoother gradients.
    /// </li>
    /// <para>
    /// 3D textures are used in various graphics applications, such as:
    /// <li>Volumetric rendering (e.g., for medical imaging or scientific visualization)</li>
    /// <li>Storing precomputed lighting information</li>
    /// <li>Creating complex material effects</li>
    /// <para>
    /// The Depth property would indicate how many "slices" or layers the 3D texture contains
    /// along its z-axis. For standard 2D textures, this value would typically be 1 or not used at all.
    /// </para>
    /// </para>
    /// </summary>
    public virtual int Depth { get; }

    /// <summary>
    /// Returns the <see cref="TextureFilterMode"/> used for minification.
    /// </summary>
    public TextureFilterMode MinFilter { get; private set; } = TextureFilterMode.Nearest;

    /// <summary>
    /// Returns the <see cref="TextureFilterMode"/> used for magnification.
    /// </summary>
    public TextureFilterMode MagFilter { get; private set; } = TextureFilterMode.Nearest;

    /// <summary>
    /// Returns the <see cref="TextureWrapMode"/> used for horizontal (U) texture coordinates.
    /// </summary>
    public TextureWrapMode UWrap { get; set; } = TextureWrapMode.ClampToEdge;

    /// <summary>
    /// Returns the <see cref="TextureWrapMode"/> used for vertical (V) texture coordinates.
    /// </summary>
    public TextureWrapMode VWrap { get; set; } = TextureWrapMode.ClampToEdge;

    public bool         IsDrawable  { get; set; } = true;
    public ITextureData TextureData { get; set; } = null!;

    // ========================================================================

    public int  Width              => TextureData.Width;
    public int  Height             => TextureData.Height;
    public int  NumManagedTextures => _managedTextures.Count;
    public uint TextureID          => GLTextureHandle;
    public bool IsManaged          => TextureData is { IsManaged: true };
    public int  ColorFormat        => TextureData.GetPixelFormat();

    // ========================================================================

    private readonly Dictionary< IApplication, List< Texture > > _managedTextures = [ ];

    private        string?     _name;
    private        bool        _isUploaded                = false;
    private        bool        _isDisposed                = false;
    private static float       _maxAnisotropicFilterLevel = 0;
    private        TextureUnit _activeTextureUnit         = TextureUnit.None;

    // ========================================================================
    // ========================================================================

    public Texture2D()
    {
    }
    
    // ========================================================================

    public void ClearWithColor( Color color )
    {
    }

    public int GetPixel( int x, int y )
    {
        return 0;
    }

    public void SetPixel( int x, int y, Color color )
    {
    }

    public void SetPixel( int x, int y, int color )
    {
    }

    // ========================================================================
    // ========================================================================

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose( true );

        GC.SuppressFinalize( this );
    }

    protected virtual void Dispose( bool disposing )
    {
        if ( !_isDisposed )
        {
            if ( disposing )
            {
            }

            _isDisposed = true;
        }
    }
}

// ========================================================================
// ========================================================================