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

using System.Text;

using LughSharp.Lugh.Graphics.GraphicsUtils;
using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Graphics.OpenGL.Enums;
using LughSharp.Lugh.Maths;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.Images;

/// <summary>
/// Class representing an OpenGL texture by its target and handle. Keeps track of
/// its state like the TextureFilter and TextureWrap. Also provides some methods to
/// create TextureData and upload image data.
/// </summary>
[PublicAPI]
public abstract class GLTexture : IDisposable
{
    // ========================================================================

    private static float       _maxAnisotropicFilterLevel = 0;
    private        TextureUnit _activeTextureUnit         = TextureUnit.None;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates a new GLTexture object using the supplied OpenGL target.
    /// </summary>
    /// <param name="glTarget"></param>
    protected GLTexture( int glTarget ) : this( glTarget, GdxApi.Bindings.GenTexture() )
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="glTarget"></param>
    /// <param name="glTextureHandle"></param>
    protected GLTexture( int glTarget, uint glTextureHandle )
    {
        GLTarget        = glTarget;
        GLTextureHandle = glTextureHandle;
    }

    // ========================================================================
    
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
    /// The handle is typically created using a function like <see cref="IGLBindings.GenTextures(int)" />.
    /// Once created, this handle is used in various OpenGL functions to bind, modify,
    /// or delete the texture. The handle remains valid until explicitly deleted using
    /// a function like <see cref="IGLBindings.DeleteTextures(uint[])" />
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
    /// <para>
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
    /// </para>
    /// <para>
    /// <li>Volumetric rendering (e.g., for medical imaging or scientific visualization)</li>
    /// <li>Storing precomputed lighting information</li>
    /// <li>Creating complex material effects</li>
    /// <para>
    /// The Depth property would indicate how many "slices" or layers the 3D texture contains
    /// along its z-axis. For standard 2D textures, this value would typically be 1 or not used at all.
    /// </para>
    /// </para>
    /// </para>
    /// </summary>
    public virtual int Depth { get; }

    /// <summary>
    /// The width, in pixels, of this texture.
    /// </summary>
    public virtual int Width { get; }

    /// <summary>
    /// The height, in pixels, of this texture.
    /// </summary>
    public virtual int Height { get; }

    // ========================================================================

    /// <summary>
    /// Returns the <see cref="Texture.TextureFilter"/> used for minification.
    /// </summary>
    public Texture.TextureFilter MinFilter { get; private set; } = Texture.TextureFilter.Nearest;

    /// <summary>
    /// Returns the <see cref="Texture.TextureFilter"/> used for magnification.
    /// </summary>
    public Texture.TextureFilter MagFilter { get; private set; } = Texture.TextureFilter.Nearest;

    /// <summary>
    /// Returns the <see cref="Texture.TextureWrap"/> used for horizontal (U) texture coordinates.
    /// </summary>
    public Texture.TextureWrap UWrap { get; set; } = Texture.TextureWrap.ClampToEdge;

    /// <summary>
    /// Returns the <see cref="Texture.TextureWrap"/> used for vertical (V) texture coordinates.
    /// </summary>
    public Texture.TextureWrap VWrap { get; set; } = Texture.TextureWrap.ClampToEdge;

    // ========================================================================

    /// <inheritdoc />
    public virtual void Dispose()
    {
        Dispose( true );

        GC.SuppressFinalize( this );
    }

    /// <summary>
    /// Used internally to reload after context loss. Creates a new GL handle then
    /// calls <see cref="Texture.Load" />.
    /// </summary>
    public abstract void Reload();

    /// <summary>
    /// </summary>
    /// <param name="textureUnit"></param>
    public void ActivateTexture( TextureUnit textureUnit )
    {
        if ( _activeTextureUnit != textureUnit )
        {
            GdxApi.Bindings.ActiveTexture( textureUnit );
            _activeTextureUnit = textureUnit;
        }
    }

    /// <summary>
    /// Binds this texture. The texture will be bound to the currently active texture unit.
    /// </summary>
    public void Bind()
    {
        ActivateTexture( TextureUnit.Texture0 );
        GdxApi.Bindings.BindTexture( GLTarget, GLTextureHandle );
    }

    /// <summary>
    /// Binds the texture to the given texture unit. Sets the currently active texture unit.
    /// </summary>
    /// <param name="unit"> the unit (0 to MAX_TEXTURE_UNITS).  </param>
    public void Bind( int unit )
    {
        ActivateTexture( TextureUnit.Texture0 + unit ); // IGL.GL_TEXTURE0 + unit );
        GdxApi.Bindings.BindTexture( GLTarget, ( uint )GLTextureHandle );
    }

    /// <summary>
    /// Sets the <see cref="Texture.TextureWrap" /> for this texture on the u and v axis.
    /// Assumes the texture is bound and active!
    /// </summary>
    /// <param name="u"> The u wrap. </param>
    /// <param name="v"> The v wrap. </param>
    /// <param name="force">
    /// True to always set the values, even if they are the same as the current values.
    /// </param>
    public void UnsafeSetWrap( Texture.TextureWrap u, Texture.TextureWrap v, bool force = false )
    {
        if ( force || ( UWrap != u ) )
        {
            GdxApi.Bindings.TexParameteri( GLTarget, IGL.GL_TEXTURE_WRAP_S, ( int )u );
            UWrap = u;
        }

        if ( force || ( VWrap != v ) )
        {
            GdxApi.Bindings.TexParameteri( GLTarget, IGL.GL_TEXTURE_WRAP_T, ( int )v );
            VWrap = v;
        }
    }

    /// <summary>
    /// Sets the <see cref="Texture.TextureWrap" /> for this texture on the u and v axis.
    /// This will bind this texture!
    /// </summary>
    /// <param name="u">the u wrap</param>
    /// <param name="v">the v wrap</param>
    public void SetWrap( Texture.TextureWrap u, Texture.TextureWrap v )
    {
        UWrap = u;
        VWrap = v;

        Bind();

        GdxApi.Bindings.TexParameteri( GLTarget, IGL.GL_TEXTURE_WRAP_S, ( int )u );
        GdxApi.Bindings.TexParameteri( GLTarget, IGL.GL_TEXTURE_WRAP_T, ( int )v );
    }

    /// <summary>
    /// Sets the <see cref="Texture.TextureFilter" /> for this texture for minification and
    /// magnification. Assumes the texture is bound and active!
    /// </summary>
    /// <param name="minFilter"> the minification filter </param>
    /// <param name="magFilter"> the magnification filter  </param>
    /// <param name="force">
    /// True to always set the values, even if they are the same as the current values.
    /// Default is false.
    /// </param>
    public void UnsafeSetFilter( Texture.TextureFilter minFilter, Texture.TextureFilter magFilter, bool force = false )
    {
        if ( force || ( MinFilter != minFilter ) )
        {
            GdxApi.Bindings.TexParameteri( GLTarget, IGL.GL_TEXTURE_MIN_FILTER, ( int )minFilter );
            MinFilter = minFilter;
        }

        if ( force || ( MagFilter != magFilter ) )
        {
            GdxApi.Bindings.TexParameteri( GLTarget, IGL.GL_TEXTURE_MAG_FILTER, ( int )magFilter );
            MagFilter = magFilter;
        }
    }

    /// <summary>
    /// Sets the <see cref="Texture.TextureFilter" /> for this texture for minification and
    /// magnification. This will bind this texture.
    /// </summary>
    /// <param name="minFilter"> The minification filter. </param>
    /// <param name="magFilter"> The magnification filter. </param>
    public void SetFilter( Texture.TextureFilter minFilter, Texture.TextureFilter magFilter )
    {
        MinFilter = minFilter;
        MagFilter = magFilter;

        Bind();

        GdxApi.Bindings.TexParameteri( GLTarget, IGL.GL_TEXTURE_MIN_FILTER, ( int )minFilter );
        GdxApi.Bindings.TexParameteri( GLTarget, IGL.GL_TEXTURE_MAG_FILTER, ( int )magFilter );
    }

    /// <summary>
    /// Sets the anisotropic filter level for the texture. Assumes the texture is bound and active!
    /// </summary>
    /// <param name="level">
    /// The desired level of filtering. The maximum level supported by the device up to this value
    /// will be used.
    /// </param>
    /// <param name="force"> True to force setting of the level. </param>
    /// <returns>
    /// The actual level set, which may be lower than the provided value due to device limitations.
    /// </returns>
    public float UnsafeSetAnisotropicFilter( float level, bool force = false )
    {
        var max = GetMaxAnisotropicFilterLevel();

        if ( Math.Abs( max - 1f ) < 0.1f )
        {
            return 1f;
        }

        level = Math.Min( level, max );

        if ( !force && MathUtils.IsEqual( level, AnisotropicFilterLevel, 0.1f ) )
        {
            return AnisotropicFilterLevel;
        }

        GdxApi.Bindings.TexParameterf( IGL.GL_TEXTURE_2D, IGL.GL_TEXTURE_MAX_ANISOTROPY_EXT, level );

        return AnisotropicFilterLevel = level;
    }

    /// <summary>
    /// Sets, and returns, the Anisotropic Filter Level.
    /// </summary>
    /// <param name="level"> The level. </param>
    /// <returns> A float holding the new level. </returns>
    public float SetAnisotropicFilter( float level )
    {
        var max = GetMaxAnisotropicFilterLevel();

        if ( Math.Abs( max - 1f ) < 0.1f )
        {
            return 1f;
        }

        level = Math.Min( level, max );

        if ( MathUtils.IsEqual( level, AnisotropicFilterLevel, 0.1f ) )
        {
            return level;
        }

        Bind();

        GdxApi.Bindings.TexParameterf( IGL.GL_TEXTURE_2D, IGL.GL_TEXTURE_MAX_ANISOTROPY_EXT, level );

        return AnisotropicFilterLevel = level;
    }

    /// <summary>
    /// Gets the maximum Anisotropic Filter Level, if it is currently &gt; 0. If it is not, then
    /// the level is obtained from OpenGL if the extension <b>GL_EXT_texture_filter_anisotropic</b>
    /// is supported, or 1.0f if the extension is not supported.
    /// </summary>
    /// <returns></returns>
    public static float GetMaxAnisotropicFilterLevel()
    {
        if ( _maxAnisotropicFilterLevel > 0 )
        {
            return _maxAnisotropicFilterLevel;
        }

        if ( GdxApi.Graphics.SupportsExtension( "GL_EXT_texture_filter_anisotropic" ) )
        {
            var buffer = new float[ 16 ];

            GdxApi.Bindings.GetFloatv( IGL.GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT, ref buffer );

            return _maxAnisotropicFilterLevel = buffer[ 0 ];
        }

        return _maxAnisotropicFilterLevel = 1f;
    }

    /// <summary>
    /// </summary>
    /// <param name="target"></param>
    /// <param name="data"></param>
    /// <param name="miplevel"></param>
    public static void UploadImageData( int target, ITextureData? data, int miplevel = 0 )
    {
        if ( data == null )
        {
            Logger.Error( "NULL ITextureData supplied!" );

            // TODO: remove texture on target?
            return;
        }

        if ( !data.IsPrepared )
        {
            data.Prepare();
        }

        var type = data.TextureDataType;

        if ( type == ITextureData.TextureType.Custom )
        {
            data.ConsumeCustomData( target );

            return;
        }

        var pixmap        = data.ConsumePixmap();
        var disposePixmap = data.ShouldDisposePixmap();

        if ( pixmap == null )
        {
            throw new GdxRuntimeException( "ConsumePixmap() resulted in a null Pixmap!" );
        }

        if ( data.PixelFormat != pixmap.GetColorFormat() )
        {
            var tmp = new Pixmap( pixmap.Width, pixmap.Height, data.PixelFormat );

            tmp.Blending = Pixmap.BlendTypes.None;
            tmp.DrawPixmap( pixmap, 0, 0, 0, 0, pixmap.Width, pixmap.Height );

            if ( data.ShouldDisposePixmap() )
            {
                pixmap.Dispose();
            }

            pixmap        = tmp;
            disposePixmap = true;
        }

        var alignment = pixmap.GLPixelFormat switch
        {
            IGL.GL_RGB or IGL.GL_RGBA or IGL.GL_RGBA4 or IGL.GL_RGB565 => 4,
            IGL.GL_ALPHA or IGL.GL_LUMINANCE                           => 1,
            var _                                                      => 1,
        };

        GdxApi.Bindings.PixelStorei( IGL.GL_UNPACK_ALIGNMENT, alignment );

        if ( data.UseMipMaps )
        {
            MipMapGenerator.GenerateMipMap( target, pixmap, pixmap.Width, pixmap.Height );
        }

        GdxApi.Bindings.TexImage2D( target, miplevel, 0, pixmap );
        
        if ( disposePixmap )
        {
            pixmap.Dispose();
        }
    }

    // ========================================================================

    /// <summary>
    /// Delete this GLTexture.
    /// </summary>
    public void Delete()
    {
        if ( GLTextureHandle != 0 )
        {
            GdxApi.Bindings.DeleteTextures( ( uint )GLTextureHandle );
            GLTextureHandle = 0;
        }
    }

    protected virtual void Dispose( bool disposing )
    {
        if ( disposing )
        {
            Delete();
        }
    }
    
    // ========================================================================
    // ========================================================================
    
    #if DEBUG
    private static void DebugUploadImageData( int target, int miplevel, Pixmap pixmap )
    {
        if ( GdxApi.DevMode )
        {
            Logger.Divider();
            Logger.Debug( $"GL Target              : {target}" );
            Logger.Debug( $"mipLevel               : {miplevel}" );
            Logger.Debug( $"pixmap.Width           : {pixmap.Width}" );
            Logger.Debug( $"pixmap.Height          : {pixmap.Height}" );
            Logger.Debug( $"Bit Depth              : {pixmap.GetBitDepth()}" );
            Logger.Debug( $"Pixmap ColorType       : {pixmap.Gdx2DPixmap.ColorType}" );

            var format = PixmapFormat.PNGColorTypeToPixmapPixelFormat( ( int )pixmap.Gdx2DPixmap.ColorType );
            Logger.Debug( $"Pixmap Pixel Format    : {PixmapFormat.GetFormatString( ( int )format )}" );

            Logger.Debug( $"pixmap.GLFormat        : {pixmap.GLPixelFormat}" );
            Logger.Debug( $"pixmap.GLFormat Name   : {PixmapFormat.GetGLPixelFormatName( pixmap.GLPixelFormat )}" );
            Logger.Debug( $"pixmap.GLInternalFormat: {pixmap.GLInternalPixelFormat}" );

            Logger.Debug( $"pixmap.GLType          : {pixmap.GLDataType}" );
            Logger.Debug( $"pixmap.GLType Name     : {PixmapFormat.GetGLTypeName( pixmap.GLDataType )}" );

            Logger.Debug( $"Number of Pixels       : {pixmap.Width * pixmap.Height}" );
            Logger.Debug( $"pixmap.PixelData.Length: {pixmap.PixelData.Length}" );

            var a  = pixmap.PixelData;
            var sb = new StringBuilder();

            const int BLOCK_SIZE = 20;

            for ( var i = 0; i < 100; i += BLOCK_SIZE )
            {
                sb.Clear();

                for ( var j = 0; j < BLOCK_SIZE; j++ )
                {
                    sb.Append( $"{a[ i + j ]:X2}," );
                }

                Logger.Debug( sb.ToString() );
            }

            Logger.Divider();
        }
    }
    #endif
}

