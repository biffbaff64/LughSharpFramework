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

using LughSharp.Lugh.Graphics.G2D;
using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Graphics.Utils;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics;

/// <summary>
/// Class representing an OpenGL texture by its target and handle. Keeps track of
/// its state like the TextureFilter and TextureWrap. Also provides some methods to
/// create TextureData and upload image data.
/// </summary>
[PublicAPI]
public abstract class GLTexture : Image, IDrawable, IDisposable
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

    // ========================================================================

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

    /// <inheritdoc />
    public bool IsDrawable { get; set; }

    // ========================================================================

    private static float       _maxAnisotropicFilterLevel = 0;
    private        TextureUnit _activeTextureUnit         = TextureUnit.None;

    // ========================================================================

    protected GLTexture()
    {
    }

    /// <summary>
    /// Creates a new GLTexture object using the supplied OpenGL target.
    /// </summary>
    /// <param name="glTarget"></param>
    protected GLTexture( int glTarget ) : this( glTarget, GL.GenTexture() )
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="glTarget"></param>
    /// <param name="glTextureHandle"></param>
    protected GLTexture( int glTarget, uint glTextureHandle )
    {
        IsDrawable      = false;
        GLTarget        = glTarget;
        GLTextureHandle = glTextureHandle;
    }

    // ========================================================================

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
            GL.ActiveTexture( textureUnit );
            _activeTextureUnit = textureUnit;
        }
    }

    /// <summary>
    /// Binds this texture. The texture will be bound to the currently active texture unit.
    /// </summary>
    public void Bind()
    {
        ActivateTexture( TextureUnit.Texture0 );
        GL.BindTexture( GLTarget, GLTextureHandle );
    }

    /// <summary>
    /// Binds the texture to the given texture unit. Sets the currently active texture unit.
    /// </summary>
    /// <param name="unit"> the unit (0 to MAX_TEXTURE_UNITS).  </param>
    public void Bind( int unit )
    {
        ActivateTexture( TextureUnit.Texture0 + unit ); // IGL.GL_TEXTURE0 + unit );
        GL.BindTexture( GLTarget, ( uint )GLTextureHandle );
    }

    /// <summary>
    /// Sets the <see cref="TextureWrapMode" /> for this texture on the u and v axis.
    /// Assumes the texture is bound and active!
    /// </summary>
    /// <param name="u"> The u wrap. </param>
    /// <param name="v"> The v wrap. </param>
    /// <param name="force">
    /// True to always set the values, even if they are the same as the current values.
    /// </param>
    public void UnsafeSetWrap( TextureWrapMode u, TextureWrapMode v, bool force = false )
    {
        if ( force || ( UWrap != u ) )
        {
            GL.TexParameteri( GLTarget, IGL.GL_TEXTURE_WRAP_S, ( int )u );
            UWrap = u;
        }

        if ( force || ( VWrap != v ) )
        {
            GL.TexParameteri( GLTarget, IGL.GL_TEXTURE_WRAP_T, ( int )v );
            VWrap = v;
        }
    }

    /// <summary>
    /// Sets the <see cref="TextureWrapMode" /> for this texture on the u and v axis.
    /// This will bind this texture!
    /// </summary>
    /// <param name="u">the u wrap</param>
    /// <param name="v">the v wrap</param>
    public void SetWrap( TextureWrapMode u, TextureWrapMode v )
    {
        UWrap = u;
        VWrap = v;

        Bind();

        GL.TexParameteri( GLTarget, IGL.GL_TEXTURE_WRAP_S, ( int )u );
        GL.TexParameteri( GLTarget, IGL.GL_TEXTURE_WRAP_T, ( int )v );
    }

    /// <summary>
    /// Sets the <see cref="TextureFilterMode" /> for this texture for minification and
    /// magnification. Assumes the texture is bound and active!
    /// </summary>
    /// <param name="minFilter"> the minification filter </param>
    /// <param name="magFilter"> the magnification filter  </param>
    /// <param name="force">
    /// True to always set the values, even if they are the same as the current values.
    /// Default is false.
    /// </param>
    public void UnsafeSetFilter( TextureFilterMode minFilter, TextureFilterMode magFilter, bool force = false )
    {
        if ( force || ( MinFilter != minFilter ) )
        {
            GL.TexParameteri( GLTarget, IGL.GL_TEXTURE_MIN_FILTER, ( int )minFilter );
            MinFilter = minFilter;
        }

        if ( force || ( MagFilter != magFilter ) )
        {
            GL.TexParameteri( GLTarget, IGL.GL_TEXTURE_MAG_FILTER, ( int )magFilter );
            MagFilter = magFilter;
        }
    }

    /// <summary>
    /// Sets the <see cref="TextureFilterMode" /> for this texture for minification and
    /// magnification. This will bind this texture.
    /// </summary>
    /// <param name="minFilter"> The minification filter. </param>
    /// <param name="magFilter"> The magnification filter. </param>
    public void SetFilter( TextureFilterMode minFilter, TextureFilterMode magFilter )
    {
        MinFilter = minFilter;
        MagFilter = magFilter;

        Bind();

        GL.TexParameteri( GLTarget, IGL.GL_TEXTURE_MIN_FILTER, ( int )minFilter );
        GL.TexParameteri( GLTarget, IGL.GL_TEXTURE_MAG_FILTER, ( int )magFilter );
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

        GL.TexParameterf( IGL.GL_TEXTURE_2D, IGL.GL_TEXTURE_MAX_ANISOTROPY_EXT, level );

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

        GL.TexParameterf( IGL.GL_TEXTURE_2D, IGL.GL_TEXTURE_MAX_ANISOTROPY_EXT, level );

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

        if ( Api.Graphics.SupportsExtension( "GL_EXT_texture_filter_anisotropic" ) )
        {
            var buffer = new float[ 16 ];

            GL.GetFloatv( IGL.GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT, ref buffer );

            return _maxAnisotropicFilterLevel = buffer[ 0 ];
        }

        return _maxAnisotropicFilterLevel = 1f;
    }

    /// <summary>
    /// </summary>
    /// <param name="target"></param>
    /// <param name="data"></param>
    /// <param name="miplevel"></param>
    public void UploadImageData( int target, ITextureData? data, int miplevel = 0 )
    {
        Logger.Debug( $"Uploading texture data to {PixelFormatUtils.GetGLTargetName( target )} target" );

        if ( data == null )
        {
            Logger.Warning( "NULL ITextureData supplied!" );

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

        if ( pixmap?.PixelData == null )
        {
            Logger.Warning( "ConsumePixmap() resulted in a null Pixmap!" );

            return;
        }

        if ( data.PixelFormat != pixmap.GetColorFormat() )
        {
            Logger.Debug( $"Converting pixmap from {pixmap.GetColorFormat()} to {data.PixelFormat}" );
            var tmp = new Pixmap( pixmap.Width, pixmap.Height, data.PixelFormat );

            if ( IsDrawable )
            {
                tmp.Blending = Pixmap.BlendTypes.None;
                tmp.DrawPixmap( pixmap, 0, 0, 0, 0, pixmap.Width, pixmap.Height );
            }

            if ( data.ShouldDisposePixmap() )
            {
                pixmap.Dispose();
            }

            pixmap        = tmp;
            disposePixmap = true;
        }

        var alignment = pixmap.GLPixelFormat switch
        {
            IGL.GL_RGB       => 4,
            IGL.GL_RGBA      => 4,
            IGL.GL_RGBA4     => 4,
            IGL.GL_RGB565    => 4,
            IGL.GL_ALPHA     => 1,
            IGL.GL_LUMINANCE => 1,
            var _            => 1,
        };

        GL.PixelStorei( IGL.GL_UNPACK_ALIGNMENT, alignment );

        CheckGLError( "PixelStorei" );

        if ( data.UseMipMaps )
        {
            MipMapGenerator.GenerateMipMap( target, pixmap, pixmap.Width, pixmap.Height );

            CheckGLError( "GenerateMipMap" );
        }

        Logger.Debug( $"Uploading texture - Width: {pixmap.Width}, Height: {pixmap.Height}" );
        Logger.Debug( $"Pixel Format: {pixmap.GLPixelFormat}, Internal Format: {pixmap.GLInternalPixelFormat}" );
        Logger.Debug( $"Data Type: {pixmap.GLDataType}, Data Length: {pixmap.PixelData.Length}" );

        Logger.Debug( $"Pixmap Format: {pixmap.GetColorFormat()}" );
        Logger.Debug( $"Pixmap Dimensions: {pixmap.Width}x{pixmap.Height}" );
        Logger.Debug( $"Pixmap GL Format: {PixelFormatUtils.GetGLPixelFormatName( pixmap.GLPixelFormat )}" );
        Logger.Debug( $"Pixmap GL Internal Format: {PixelFormatUtils.GetGLPixelFormatName( pixmap.GLInternalPixelFormat )}" );
        Logger.Debug( $"Pixmap GL Data Type: {PixelFormatUtils.GetGLTypeName( pixmap.GLDataType )}" );
        Logger.Debug( $"Pixmap Data Length: {pixmap.PixelData.Length}" );
        Logger.Debug( $"Gdx2dPixmap created successfully?: {pixmap.Gdx2DPixmap != null}" );

        var boundTexture = new int[ 1 ];
        GL.GetIntegerv( IGL.GL_TEXTURE_BINDING_2D, ref boundTexture );
        Logger.Debug( $"Currently bound texture before upload: {boundTexture[ 0 ]}" );

        GL.TexParameteri( target, IGL.GL_TEXTURE_MIN_FILTER, IGL.GL_NEAREST );
        GL.TexParameteri( target, IGL.GL_TEXTURE_MAG_FILTER, IGL.GL_NEAREST );
        GL.TexParameteri( target, IGL.GL_TEXTURE_WRAP_S, IGL.GL_CLAMP_TO_EDGE );
        GL.TexParameteri( target, IGL.GL_TEXTURE_WRAP_T, IGL.GL_CLAMP_TO_EDGE );

        GL.GetIntegerv( ( int )GLParameter.ActiveTexture, out var beforeActive );
        GL.ActiveTexture( TextureUnit.Texture0 ); // pick one explicitly
        GL.BindTexture( TextureTarget.Texture2D, GLTextureHandle );
        GL.PixelStorei( ( int )PixelStoreParameter.UnpackAlignment, 1 );
//        GL.TexImage2D( ( int )TextureTarget.Texture2D, 0, ( int )PixelInternalFormat.Rgba8, 640, 480, 0,
//                       ( int )PixelFormat.Rgba, ( int )PixelType.UnsignedByte, pixelPtr );
        GL.GetTexLevelParameteriv( TextureTarget.Texture2D, 0, TextureParameter.TextureWidth, out var w );
        GL.GetTexLevelParameteriv( TextureTarget.Texture2D, 0, TextureParameter.TextureHeight, out var h );
        GL.GetIntegerv( ( int )GLParameter.TextureBinding2D, out var afterBound );

        Logger.Debug( $"Upload: prevActive={beforeActive}, boundAfterUpload={afterBound}, Level0={w}x{h}, err={GL.GetError()}" );

        GL.TexImage2D( target, miplevel, 0, pixmap, false );

//        GL.TexImage2D< byte >( target,
//                               miplevel,
//                               pixmap.GLInternalPixelFormat,
//                               pixmap.Width,
//                               pixmap.Height,
//                               0,
//                               pixmap.GLPixelFormat,
//                               pixmap.GLDataType,
//                               pixmap.PixelData,
//                               false );

        var boundTex = new int[ 1 ];
        GL.GetIntegerv( IGL.GL_TEXTURE_BINDING_2D, ref boundTex );
        Logger.Debug( $"Currently bound texture before query: {boundTex[ 0 ]}" );

        if ( boundTex[ 0 ] == 0 )
        {
            Logger.Debug( "No texture bound when trying to query dimensions!" );

            return;
        }

        GL.GetIntegerv( IGL.GL_TEXTURE_BINDING_2D, ref boundTexture );
        Logger.Debug( $"Currently bound texture after upload: {boundTexture[ 0 ]}" );

        // Get texture parameters to verify the dimensions were set
        int[] width  = new int[ 1 ];
        int[] height = new int[ 1 ];
        GL.GetTexLevelParameteriv( target, 0, IGL.GL_TEXTURE_WIDTH, ref width );
        GL.GetTexLevelParameteriv( target, 0, IGL.GL_TEXTURE_HEIGHT, ref height );
        Logger.Debug( $"Texture dimensions immediately after upload: {width[ 0 ]}x{height[ 0 ]}" );

        var error = GL.GetError();
        Logger.Debug( $"GL Error after TexImage2D: {error}" );

        if ( disposePixmap )
        {
            pixmap.Dispose();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="operation"></param>
    /// <exception cref="GdxRuntimeException"></exception>
    private static void CheckGLError( string operation )
    {
        var error = GL.GetError();

        if ( error != ( int )ErrorCode.NoError )
        {
            throw new GdxRuntimeException( $"OpenGL error during {operation}: {error}" );
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
            GL.DeleteTextures( ( uint )GLTextureHandle );
            GLTextureHandle = 0;
        }
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
        if ( !IsDisposed )
        {
            if ( disposing )
            {
                Delete();
            }

            IsDisposed = true;
        }
    }

    // ========================================================================
    // ========================================================================

    #if DEBUG
    private static void DebugUploadImageData( int target, int miplevel, Pixmap pixmap )
    {
        if ( Api.DevMode )
        {
            Logger.Divider();
            Logger.Debug( $"GL Target              : {target}" );
            Logger.Debug( $"mipLevel               : {miplevel}" );
            Logger.Debug( $"pixmap.Width           : {pixmap.Width}" );
            Logger.Debug( $"pixmap.Height          : {pixmap.Height}" );
            Logger.Debug( $"Bit Depth              : {pixmap.GetBitDepth()}" );
            Logger.Debug( $"Pixmap ColorType       : {pixmap.Gdx2DPixmap?.ColorType}" );
            Logger.Debug( $"Pixmap Pixel Format    : {PixelFormatUtils.GetFormatString( pixmap.Gdx2DPixmap!.ColorType )}" );
            Logger.Debug( $"pixmap.GLFormat        : {pixmap.GLPixelFormat}" );
            Logger.Debug( $"pixmap.GLFormat Name   : {PixelFormatUtils.GetGLPixelFormatName( pixmap.GLPixelFormat )}" );
            Logger.Debug( $"pixmap.GLInternalFormat: {pixmap.GLInternalPixelFormat}" );
            Logger.Debug( $"pixmap.GLType          : {pixmap.GLDataType}" );
            Logger.Debug( $"pixmap.GLType Name     : {PixelFormatUtils.GetGLTypeName( pixmap.GLDataType )}" );
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