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

using LughSharp.Lugh.Assets;
using LughSharp.Lugh.Assets.Loaders;
using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Graphics.OpenGL.Enums;
using LughSharp.Lugh.Graphics.Pixels;
using LughSharp.Lugh.Graphics.Utils;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Collections;
using LughSharp.Lugh.Utils.Exceptions;

using PixelType = LughSharp.Lugh.Graphics.Pixels.PixelType;

namespace LughSharp.Lugh.Graphics.Images;

/// <summary>
/// A Texture wraps a standard OpenGL texture.
/// <para>
/// A Texture can be managed. If the OpenGL context is lost all managed textures
/// get invalidated. This happens when a user switches to another application or
/// receives an incoming call on mobile devices.
/// Managed textures get reloaded automatically.
/// </para>
/// <para>
/// A Texture has to be bound via the <see cref="Texture.Bind()" /> method in order
/// for it to be applied to geometry. The texture will be bound to the currently
/// active texture unit specified via <see cref="OpenGL.GLBindings.ActiveTexture(int)" />,
/// or <see cref="OpenGL.GLBindings.ActiveTexture(TextureUnit)" />.
/// </para>
/// <para>
/// You can draw <see cref="Pixmap" />s to a texture at any time. The changes will
/// be automatically uploaded to texture memory. This is, of course, not extremely
/// fast so use it with care. It also only works with unmanaged textures.
/// </para>
/// <para>
/// A Texture must be disposed when it is no longer used
/// </para>
/// </summary>
[PublicAPI]
public class Texture : GLTexture, IManaged
{
    public AssetManager? AssetManager { get; set; } = null;
    public ITextureData  TextureData  { get; set; }

    // ========================================================================

    public override int  Width              => TextureData.Width;
    public override int  Height             => TextureData.Height;
    public override int  Depth              => 0;
    public          int  NumManagedTextures => _managedTextures.Count;
    public          uint TextureID          => GLTextureHandle;

    /// <summary>
    /// The Texture name, usually the filename but can be something else.
    /// Name will be set to 'Name Not Set' if null or empty.
    /// </summary>
    public string Name
    {
        get
        {
            if ( string.IsNullOrEmpty( _name ) )
            {
                _name = "Name Not Set";
            }

            return _name;
        }
        set => _name = value;
    }

    public bool IsManaged => TextureData is { IsManaged: true };

    // ========================================================================

    private readonly Dictionary< IApplication, List< Texture > > _managedTextures = [ ];

    private string? _name;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Create a new Texture from the file at the given path.
    /// </summary>
    /// <param name="internalPath"></param>
    public Texture( string internalPath )
        : this( Api.Files.Internal( internalPath ), false )
    {
        Name = Path.GetFileNameWithoutExtension( internalPath );
    }

    /// <summary>
    /// Create a new Texture from the file described by the given <see cref="FileInfo" />
    /// </summary>
    /// <param name="file"></param>
    /// <param name="useMipMaps"> Whether or not to generate MipMaps. Default is false. </param>
    public Texture( FileInfo file, bool useMipMaps )
        : this( file, PixelType.Format.Default, useMipMaps )
    {
        Name = Path.GetFileNameWithoutExtension( file.Name );
    }

    /// <summary>
    /// Create a new Texture from the file specified in the given <see cref="FileInfo" />.
    /// The Texture pixmap format will be set to the given format, which defaults to
    /// <see cref="PixelType.Format.RGBA8888" />.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="format"> The pixmap format to use. </param>
    /// <param name="useMipMaps"> Whether or not to generate MipMaps. Default is false. </param>
    public Texture( FileInfo file,
                    PixelType.Format format = PixelType.Format.Default,
                    bool useMipMaps = false )
        : this( TextureDataFactory.LoadFromFile( file, format, useMipMaps ) )
    {
        Name = Path.GetFileNameWithoutExtension( file.Name );
    }

    /// <summary>
    /// Creates a new Texture from the supplied <see cref="Pixmap" />.
    /// </summary>
    /// <param name="pixmap"> The pixmap to use. </param>
    /// <param name="useMipMaps"> Whether or not to generate MipMaps. Default is false. </param>
    public Texture( Pixmap pixmap, bool useMipMaps = false )
        : this( new PixmapTextureData( pixmap, null, useMipMaps, false ) )
    {
    }

    /// <summary>
    /// Creates a new Texture from the supplied <see cref="Pixmap" /> and <see cref="PixelType.Format" />
    /// </summary>
    /// <param name="pixmap"> The pixmap to use. </param>
    /// <param name="format"> The pixmap format to use. </param>
    /// <param name="useMipMaps"> Whether or not to generate MipMaps. Default is false. </param>
    public Texture( Pixmap pixmap, PixelType.Format format, bool useMipMaps = false )
        : this( new PixmapTextureData( pixmap, format, useMipMaps, false ) )
    {
    }

    /// <summary>
    /// Creates a new Texture with the specified width, height, and Pixmap format.
    /// </summary>
    /// <param name="width"> The width in pixels. </param>
    /// <param name="height"> The Height in pixels. </param>
    /// <param name="format"> The pixmap <see cref="PixelType.Format" /> </param>
    public Texture( int width, int height, PixelType.Format format )
        : this( new PixmapTextureData( new Pixmap( width, height, format ), null, false, true ) )
    {
    }

    /// <summary>
    /// Creates a new Texture using the supplied <see cref="ITextureData" />.
    /// </summary>
    public Texture( ITextureData data )
        : this( IGL.GL_TEXTURE_2D, GL.GenTexture(), data )
    {
    }

    /// <summary>
    /// Default constructor. Creates a new Texture from the supplied GLTarget,
    /// GLTextureHandle and TextureData.
    /// </summary>
    /// <param name="glTarget"></param>
    /// <param name="glTextureHandle"></param>
    /// <param name="data"></param>
    protected Texture( int glTarget, uint glTextureHandle, ITextureData data )
        : base( glTarget, glTextureHandle )
    {
        ArgumentNullException.ThrowIfNull( data );

        TextureData = data;

        Load( data );

        if ( data.IsManaged )
        {
            AddManagedTexture( Api.App, this );
        }
    }

    /// <summary>
    /// Load the given <see cref="ITextureData" /> data into this Texture.
    /// </summary>
    /// <param name="data"></param>
    public void Load( ITextureData data )
    {
        if ( data.IsManaged != TextureData.IsManaged )
        {
            throw new GdxRuntimeException( "New data must have the same managed status as the old data" );
        }

        TextureData = data;

        if ( !data.IsPrepared )
        {
            data.Prepare();
        }

        Bind();

        UploadImageData( IGL.GL_TEXTURE_2D, data );

        UnsafeSetFilter( MinFilter, MagFilter, true );
        UnsafeSetWrap( UWrap, VWrap, true );
        UnsafeSetAnisotropicFilter( AnisotropicFilterLevel, true );

        GL.BindTexture( GLTarget, 0 );
    }

    /// <summary>
    /// Used internally to reload after context loss. Creates a new GL handle then
    /// calls <see cref="Load(ITextureData)" />.
    /// </summary>
    public override void Reload()
    {
        if ( !IsManaged )
        {
            throw new GdxRuntimeException( "Tried to reload unmanaged Texture" );
        }

        GLTextureHandle = GL.GenTexture();

        Load( TextureData );
    }

    /// <summary>
    /// Draws the given <see cref="Pixmap" /> to the texture at position x, y. No clipping
    /// is performed so it is important to make sure that you drawing is only done inside
    /// the texture region. Note that this will only draw to mipmap level 0!
    /// </summary>
    /// <param name="pixmap"> The Pixmap </param>
    /// <param name="x"> The x coordinate in pixels </param>
    /// <param name="y"> The y coordinate in pixels  </param>
    public void Draw( Pixmap pixmap, int x, int y )
    {
        if ( TextureData is { IsManaged: true } )
        {
            throw new GdxRuntimeException( "can't draw to a managed texture" );
        }

        Bind();

        GL.TexSubImage2D( GLTarget,
                          0, x, y,
                          pixmap.Width,
                          pixmap.Height,
                          pixmap.GLPixelFormat,
                          pixmap.GLDataType,
                          pixmap.PixelData );
    }

    /// <summary>
    /// Add the supplied MANAGED texture to the list of managed textures.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="texture"></param>
    private void AddManagedTexture( IApplication app, Texture texture )
    {
        var managedTextureArray = _managedTextures.TryGetValue( app, out var managedTexture )
            ? managedTexture
            : [ ];

        managedTextureArray.Add( texture );

        _managedTextures.Put( app, managedTextureArray );
    }

    /// <summary>
    /// Invalidate all managed textures. This is an internal method. Do not use it!
    /// </summary>
    internal void InvalidateAllTextures( IApplication app )
    {
        if ( AssetManager == null )
        {
            foreach ( var t in _managedTextures[ app ] )
            {
                t.Reload();
            }
        }
        else
        {
            // first we have to make sure the AssetManager isn't loading anything anymore,
            // otherwise the ref counting trick below wouldn't work (when a texture is
            // currently on the task stack of the manager.)
            AssetManager.FinishLoading();

            // next we go through each texture and reload either directly or via the
            // asset manager.
            var textures = new List< Texture >( _managedTextures[ app ] );

            foreach ( var texture in textures )
            {
                var fileName = AssetManager.GetAssetFileName( texture );

                if ( fileName == null )
                {
                    texture.Reload();
                }
                else
                {
                    // get the reference count of the texture, then set it to 0 so
                    // we can actually remove it from the assetmanager. Also set the
                    // handle to zero, otherwise we might accidentially dispose
                    // already reloaded textures.
                    var refCount = AssetManager.GetReferenceCount( fileName );

                    AssetManager.SetReferenceCount( fileName, 0 );
                    texture.GLTextureHandle = 0;

                    // create the parameters, passing the reference to the texture as
                    // well as a callback that sets the ref count.
                    var parameters = new TextureLoader.TextureLoaderParameters
                    {
                        TextureData    = texture.TextureData,
                        MinFilter      = texture.MinFilter,
                        MagFilter      = texture.MagFilter,
                        WrapU          = texture.UWrap,
                        WrapV          = texture.VWrap,
                        GenMipMaps     = texture.TextureData is { UseMipMaps: true },
                        Texture        = texture,
                        LoadedCallback = new LoadedCallbackInnerClass( refCount ),
                    };

                    // unload the texture, create a new gl handle then reload it.
                    AssetManager.Unload( fileName );
                    texture.GLTextureHandle = GL.GenTexture();
                    AssetManager.Load( fileName, typeof( Texture ), parameters );
                }
            }

            _managedTextures[ app ].Clear();
            _managedTextures[ app ].AddAll( textures );
        }
    }

    /// <summary>
    /// Returns a string detailing the managed status of the textures
    /// within the managed textures list.
    /// </summary>
    public string GetManagedStatus()
    {
        var builder = new StringBuilder( "Managed textures/app: { " );

        foreach ( var app in _managedTextures.Keys )
        {
            builder.Append( _managedTextures[ app ].Count );
            builder.Append( ' ' );
        }

        builder.Append( '}' );

        return builder.ToString();
    }

    public byte[]? GetImageData()
    {
        if ( !TextureData.IsPrepared )
        {
            TextureData.Prepare();
        }

        return TextureData.ConsumePixmap()?.PixelData;
    }

    /// <summary>
    /// Clears all managed textures.
    /// </summary>
    internal void ClearAllTextures( IApplication app )
    {
        _managedTextures.Remove( app );
    }

    // ========================================================================
    // Implementations of abstract methods from the base Image class.

    public override void ClearWithColor( Color color )
    {
        throw new NotImplementedException();
    }

    public override int GetPixel( int x, int y )
    {
        throw new NotImplementedException();
    }

    public override void SetPixel( int x, int y, Color color )
    {
        throw new NotImplementedException();
    }

    public override void SetPixel( int x, int y, int color )
    {
        throw new NotImplementedException();
    }

    // ========================================================================

    /// <inheritdoc />
    public override string? ToString()
    {
        return TextureData is FileTextureData ? TextureData.ToString() : base.ToString();
    }

    /// <summary>
    /// </summary>
    public void Debug()
    {
        Logger.Debug( $"Dimensions        : {Width} x {Height}" );
        Logger.Debug( $"Format            : {TextureData.PixelFormat}" );
        Logger.Debug( $"IsManaged         : {IsManaged}" );
        Logger.Debug( $"NumManagedTextures: {NumManagedTextures}" );
        Logger.Debug( $"Depth             : {Depth}" );
        Logger.Debug( $"GLTarget          : {PixmapFormat.GetGLTargetName( GLTarget )}" );
        Logger.Debug( $"GLTextureHandle   : {GLTextureHandle:X}" );

        if ( !TextureData.IsPrepared )
        {
            TextureData.Prepare();
        }

        Logger.Debug( $"TextureData Length: {TextureData.ConsumePixmap()!.PixelData.Length}" );
    }

    /// <inheritdoc />
    protected override void Dispose( bool disposing )
    {
        if ( disposing )
        {
            // this is a hack.
            // Reason: we have to set the glHandle to 0 for textures that are
            // reloaded through the asset manager as we first remove (and thus
            // dispose) the texture and then reload it. the glHandle is set to
            // 0 in invalidateAllTextures prior to removal from the asset manager.
            if ( GLTextureHandle == 0 )
            {
                return;
            }

            Delete();

            if ( TextureData is { IsManaged: true } )
            {
                _managedTextures[ Api.App ].Remove( this );
            }
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class Utils
    {
        public static bool IsMipMap( TextureFilter filter )
        {
            return ( ( int )filter != IGL.GL_NEAREST ) && ( ( int )filter != IGL.GL_LINEAR );
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// </summary>
    private sealed class LoadedCallbackInnerClass( int refCount ) : ILoadedCallback
    {
        public void FinishedLoading( AssetManager assetManager, string? fileName, Type? type )
        {
            assetManager.SetReferenceCount( fileName!, refCount );
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Enumerates different texture filtering methods used to determine how textures
    /// are sampled when they are displayed at sizes other than their original resolution.
    /// <para>
    /// Texture filtering impacts both magnification (enlargement) and minification
    /// (reduction) of textures. This is especially important in scenarios where a
    /// texture is applied to a surface that appears either much larger or much smaller
    /// than the original texture resolution.
    /// </para>
    /// <para>
    /// The available options include various combinations of nearest-neighbor filtering,
    /// linear filtering, and mipmap-based filtering. Mipmapping involves precomputing and
    /// storing multiple resolutions of the texture to improve performance and visual
    /// quality when textures are minified.
    /// </para>
    /// </summary>
    [PublicAPI]
    public enum TextureFilter : int
    {
        /// <summary>
        /// Fetch the nearest texel that best maps to the pixel on screen.
        /// </summary>
        Nearest = IGL.GL_NEAREST,

        /// <summary>
        /// Fetch four nearest texels that best map to the pixel on screen.
        /// </summary>
        Linear = IGL.GL_LINEAR,

        /// <summary>
        /// Applies a linear texture filtering technique where texels are chosen based
        /// on the nearest or interpolated mipmap level, providing a smoother appearance
        /// for textures with varying distances.
        /// </summary>
        MipMap = IGL.GL_LINEAR_MIPMAP_LINEAR,

        /// <summary>
        /// Fetch the best fitting image from the mip map chain based on the pixel/texel
        /// ratio and then sample the texels with a nearest filter.
        /// </summary>
        MipMapNearestNearest = IGL.GL_NEAREST_MIPMAP_NEAREST,

        /// <summary>
        /// Fetch the best fitting image from the mip map chain based on the pixel/texel
        /// ratio and then sample the texels with a linear filter.
        /// </summary>
        MipMapLinearNearest = IGL.GL_LINEAR_MIPMAP_NEAREST,

        /// <summary>
        /// Fetch the two best fitting images from the mip map chain and then sample
        /// the nearest texel from each of the two images, combining them to the final
        /// output pixel.
        /// </summary>
        MipMapNearestLinear = IGL.GL_NEAREST_MIPMAP_LINEAR,

        /// <summary>
        /// Fetch the two best fitting images from the mip map chain and then sample
        /// the four nearest texels from each of the two images, combining them to
        /// the final output pixel.
        /// </summary>
        MipMapLinearLinear = MipMap,
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public enum TextureWrap : int
    {
        /// <summary>
        /// Repeats the texture, mirroring it at every integer boundary. This
        /// creates a seamless mirrored effect at the edges.
        /// </summary>
        MirroredRepeat = IGL.GL_MIRRORED_REPEAT,

        /// <summary>
        /// Clamps texture coordinates to the edges of the texture, ensuring
        /// that texture sampling outside the bounds of the texture fetches
        /// the color from the nearest edge texel.
        /// </summary>
        ClampToEdge = IGL.GL_CLAMP_TO_EDGE,

        /// <summary>
        /// Wraps texture coordinates, causing the texture to repeat when
        /// coordinates exceed the range [0.0, 1.0].
        /// </summary>
        Repeat = IGL.GL_REPEAT,
    }

    // ========================================================================
}