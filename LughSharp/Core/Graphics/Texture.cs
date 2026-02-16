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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using LughSharp.Core.Assets;
using LughSharp.Core.Assets.Loaders;
using LughSharp.Core.Files;
using LughSharp.Core.Graphics.OpenGL;
using LughSharp.Core.Graphics.OpenGL.Bindings;
using LughSharp.Core.Graphics.OpenGL.Enums;
using LughSharp.Core.Graphics.Utils;
using LughSharp.Core.Main;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Collections;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;

namespace LughSharp.Core.Graphics;

/// <summary>
/// A Texture wraps a standard OpenGL texture.
/// <para>
/// A Texture can be managed. If the OpenGL context is lost all managed textures
/// get invalidated. This happens when a user switches to another application or
/// receives an incoming call on mobile devices.  Managed textures get reloaded
/// automatically.
/// </para>
/// <para>
/// A Texture has to be bound via the <see cref="Texture.Bind()"/> method in order
/// for it to be applied to geometry. The texture will be bound to the currently
/// active texture unit specified via <see cref="GLBindings.ActiveTexture(int)"/>,
/// or <see cref="GLBindings.ActiveTexture(LughSharp.Core.Graphics.OpenGL.Enums.TextureUnit)"/>.
/// </para>
/// <para>
/// You can draw <see cref="Pixmap"/>s to a texture at any time. The changes will
/// be automatically uploaded to texture memory. This is, of course, not extremely
/// fast so use it with care. It also only works with unmanaged textures.
/// </para>
/// <para>
/// A Texture must be disposed when it is no longer used.
/// </para>
/// </summary>
[PublicAPI]
public class Texture : GLTexture, IManaged
{
    public ITextureData TextureData { get; set; }

    // ========================================================================

    public int  Width              => TextureData.Width;
    public int  Height             => TextureData.Height;
    public int  ColorFormat        => TextureData.GetPixelFormat();
    public int  BitDepth           => TextureData.BitDepth;
    public int  NumManagedTextures => _managedTextures.Count;
    public uint TextureID          => GLTextureHandle;
    public bool IsManaged          => TextureData is { IsManaged: true };

    // ========================================================================

    private readonly Dictionary< IApplication, List< Texture > > _managedTextures = [ ];

    private AssetManager? _assetManager;
    private bool          _isUploaded;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Create a new Texture from the file at the given internal path.
    /// </summary>
    /// <param name="internalPath">
    /// The internal path to the file. For an explanation of internal paths see
    /// <see cref="PathType.Internal"/>.
    /// </param>
    public Texture( string internalPath )
        : this( Engine.Api.Files.Internal( internalPath ), false )
    {
    }

    /// <summary>
    /// Create a new Texture from the file described by the given <see cref="FileInfo"/>
    /// </summary>
    /// <param name="file"> The file to load. </param>
    /// <param name="useMipMaps"> Whether or not to generate MipMaps. Default is false. </param>
    public Texture( FileInfo file, bool useMipMaps )
        : this( file, LughFormat.RGBA8888, useMipMaps )
    {
    }

    /// <summary>
    /// Create a new Texture from the file specified in the given <see cref="FileInfo"/>.
    /// The Texture pixmap format will be set to the given format, which defaults to
    /// <see cref="LughFormat.RGBA8888"/>.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="format"> The pixmap format to use. </param>
    /// <param name="useMipMaps"> Whether or not to generate MipMaps. Default is false. </param>
    public Texture( FileInfo file,
                    int format = LughFormat.RGBA8888,
                    bool useMipMaps = false )
        : this( TextureDataFactory.LoadFromFile( file, format, useMipMaps ) )
    {
    }

    /// <summary>
    /// Creates a new Texture from the supplied <see cref="Pixmap"/>.
    /// </summary>
    /// <param name="pixmap"> The pixmap to use. </param>
    /// <param name="useMipMaps"> Whether or not to generate MipMaps. Default is false. </param>
    public Texture( Pixmap pixmap, bool useMipMaps = false )
        : this( new PixmapTextureData( pixmap, 0, useMipMaps, false ) )
    {
    }

    /// <summary>
    /// Creates a new Texture from the supplied <see cref="Pixmap"/> and <c>LughFormat</c>
    /// </summary>
    /// <param name="pixmap"> The pixmap to use. </param>
    /// <param name="format"> The pixmap format to use. </param>
    /// <param name="useMipMaps"> Whether or not to generate MipMaps. Default is false. </param>
    public Texture( Pixmap pixmap, int format, bool useMipMaps = false )
        : this( new PixmapTextureData( pixmap, format, useMipMaps, false ) )
    {
    }

    /// <summary>
    /// Creates a new Texture with the specified width, height, and Pixmap format.
    /// </summary>
    /// <param name="width"> The width in pixels. </param>
    /// <param name="height"> The Height in pixels. </param>
    /// <param name="format"> The pixmap <c>Gdx2DPixmap.GDX_2D_FORMAT_XXX</c> </param>
    /// <param name="managed"></param>
    public Texture( int width, int height, int format, bool managed = false )
        : this( new PixmapTextureData( new Pixmap( width, height, format ),
                                       0,
                                       false,
                                       true,
                                       managed ) )
    {
    }

    /// <summary>
    /// Creates a new Texture using the supplied <see cref="ITextureData"/>.
    /// </summary>
    public Texture( ITextureData data )
        : this( IGL.GL_TEXTURE_2D, Engine.GL.CreateTexture( TextureTarget.Texture2D ), data )
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
        Guard.Against.Null( data );

        TextureData = data;

        Load( data );

        if ( data.IsManaged )
        {
            AddManagedTexture( Engine.Api.App, this );
        }
    }

    /// <summary>
    /// Load the given <see cref="ITextureData"/> data into this Texture.
    /// </summary>
    /// <param name="data"></param>
    public void Load( ITextureData data )
    {
        if ( ( TextureData != null ) && ( data.IsManaged != TextureData.IsManaged ) )
        {
            throw new RuntimeException( "New data must have the same managed status as the old data" );
        }

        TextureData = data;

        if ( !data.IsPrepared )
        {
            data.Prepare();
        }

        Bind();

        UploadImageData( GLTarget, data );

        UnsafeSetFilter( MinFilter, MagFilter, true );
        UnsafeSetWrap( UWrap, VWrap, true );
        UnsafeSetAnisotropicFilter( AnisotropicFilterLevel, true );

        Engine.GL.BindTexture( GLTarget, 0 ); 

        if ( ColorFormat == LughFormat.ALPHA )
        {
            // Map Red -> Alpha, and force RGB to 1.0 (White)
            int[] swizzle = { IGL.GL_ONE, IGL.GL_ONE, IGL.GL_ONE, IGL.GL_RED };
            Engine.GL.TexParameteriv( ( int )TextureTarget.Texture2D, ( int )TextureParameter.TextureSwizzleRgba, swizzle );
        }
        else if ( ColorFormat == LughFormat.LUMINANCE_ALPHA )
        {
            // Map Red -> RGB (Luminance), Green -> Alpha
            int[] swizzle = { IGL.GL_RED, IGL.GL_RED, IGL.GL_RED, IGL.GL_GREEN };
            Engine.GL.TexParameteriv( ( int )TextureTarget.Texture2D, ( int )TextureParameter.TextureSwizzleRgba, swizzle );
        }
    }

    /// <summary>
    /// Used internally to reload after context loss. Creates a new GL handle then
    /// calls <see cref="Load(ITextureData)"/>.
    /// </summary>
    public override void Reload()
    {
        if ( !IsManaged )
        {
            throw new RuntimeException( "Tried to reload unmanaged Texture" );
        }

        GLTextureHandle = Engine.GL.GenTexture();

        Load( TextureData );
    }

    /// <summary>
    /// Draws the given <see cref="Pixmap"/> to the texture at position x, y. No clipping
    /// is performed so it is important to make sure that you drawing is only done inside
    /// the texture region. Note that this will only draw to mipmap level 0!
    /// </summary>
    /// <param name="pixmap"> The Pixmap </param>
    /// <param name="x"> The x coordinate in pixels </param>
    /// <param name="y"> The y coordinate in pixels  </param>
    public void InsertPixmap( Pixmap pixmap, int x, int y )
    {
        if ( TextureData is { IsManaged: true } )
        {
            throw new RuntimeException( "can't draw to a managed texture" );
        }

        Bind();

        Engine.GL.TexSubImage2D( GLTarget,
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

        _managedTextures[ app ] = managedTextureArray;
    }

    /// <summary>
    /// Invalidate all managed textures. This is an internal method. Do not use it!
    /// </summary>
    internal void InvalidateAllTextures( IApplication app )
    {
        if ( _assetManager == null )
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
            _assetManager.FinishLoading();

            // next we go through each texture and reload either directly or via the
            // asset manager.
            var textures = new List< Texture >( _managedTextures[ app ] );

            foreach ( var texture in textures )
            {
                var filename = _assetManager.GetAssetFileName( texture );

                if ( filename == null )
                {
                    texture.Reload();
                }
                else
                {
                    // get the reference count of the texture, then set it to 0 so
                    // we can actually remove it from the assetmanager. Also set the
                    // handle to zero, otherwise we might accidentially dispose
                    // already reloaded textures.
                    var refCount = _assetManager.GetReferenceCount( filename );

                    _assetManager.SetReferenceCount( filename, 0 );
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
                    _assetManager.Unload( filename );
                    texture.GLTextureHandle = Engine.GL.GenTexture();
                    _assetManager.Load< Texture >( filename, parameters );
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

    /// <summary>
    /// Returns the raw image data of the texture, which is held in
    /// the underlying Pixmap.
    /// </summary>
    /// <returns></returns>
    public byte[]? GetImageData()
    {
        Guard.Against.Null( TextureData );

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

    public static bool IsMipMap( TextureFilterMode filter )
    {
        return ( filter != TextureFilterMode.Nearest )
            && ( filter != TextureFilterMode.Linear );
    }

    // ========================================================================

    /// <summary>
    /// The Texture name, usually the filename but can be something else.
    /// Name will be set to 'Name Not Set' if null or empty.
    /// </summary>
    [field: AllowNull]
    [field: MaybeNull]
    public string Name
    {
        get
        {
            if ( string.IsNullOrEmpty( field ) )
            {
                field = "Name Not Set";
            }

            return field;
        }
        set;
    }

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    public void DebugPrint()
    {
        Guard.Against.Null( TextureData );

        if ( !TextureData.IsPrepared )
        {
            TextureData.Prepare();
        }

        var pixmap = TextureData.ConsumePixmap();

        Logger.Debug( $"Dimensions        : {Width} x {Height} = {Width * Height}" );
        Logger.Debug( $"Format            : {TextureData.GetPixelFormat()}" );
        Logger.Debug( $"IsManaged         : {IsManaged}" );
        Logger.Debug( $"NumManagedTextures: {NumManagedTextures}" );
        Logger.Debug( $"Depth             : {Depth}" );
        Logger.Debug( $"GLTarget          : {PixelFormat.GetGLTargetName( GLTarget )}" );
        Logger.Debug( $"GLTextureHandle   : {GLTextureHandle:X}" );
        Logger.Debug( $"BytesPerPixel     : {TextureData.BytesPerPixel}" );
        Logger.Debug( $"BitDepth          : {TextureData.BitDepth}" );

        Logger.Debug( $"TextureData Length: {pixmap!.PixelData.Length}" +
                      $" (should be {Width} x {Height} x {pixmap.Gdx2DPixmap.BytesPerPixel}: " +
                      $"{Width * Height * pixmap.Gdx2DPixmap.BytesPerPixel})" );
    }

    // ========================================================================

    /// <inheritdoc />
    protected override void Dispose( bool disposing )
    {
        if ( IsDisposed )
        {
            return;
        }

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
                _managedTextures[ Engine.Api.App ].Remove( this );
            }
        }

        IsDisposed = true;
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// </summary>
    private class LoadedCallbackInnerClass( int refCount ) : ILoadedCallback
    {
        public void FinishedLoading( AssetManager assetManager, string? filename, Type? type )
        {
            assetManager.SetReferenceCount( filename!, refCount );
        }
    }
}

// ========================================================================
// ========================================================================