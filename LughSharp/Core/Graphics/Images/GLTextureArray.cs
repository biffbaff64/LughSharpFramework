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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

using JetBrains.Annotations;

using LughSharp.Core.Graphics.OpenGL;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Graphics.Images;

/// <summary>
/// Represents an OpenGL 2D texture array resource wrapper.
/// </summary>
/// <remarks>
/// Instances of <see cref="GLTextureArray"/> wrap the native OpenGL texture handle for
/// a texture array (GL_TEXTURE_2D_ARRAY). They can be constructed from file handles
/// or from an <see cref="ITextureArrayData"/> provider which supplies pixel data and
/// associated metadata (width/height/depth, formats, whether the data is managed,
/// etc).
///
/// The class supports managed texture arrays: when the backing data reports that it is
/// managed, the texture will be registered with the framework so it can be reloaded
/// (for example, after an OpenGL context loss) via <see cref="InvalidateAllTextureArrays"/>.
/// </remarks>
[PublicAPI]
public class GLTextureArray : GLTexture, IManaged
{
    /// <summary>
    /// Registry of managed texture arrays keyed by application instance.
    /// </summary>
    private static readonly Dictionary< IApplication, List< GLTextureArray > > _managedTextureArrays = new();

    private ITextureArrayData _data;

    // ========================================================================

    /// <summary>
    /// Create a texture array from internal application resource paths.
    /// </summary>
    /// <param name="internalPaths">Array of internal resource paths to load as array layers.</param>
    public GLTextureArray( params string[] internalPaths )
        : this( GetInternalHandles( internalPaths ) )
    {
    }

    /// <summary>
    /// Create a texture array from file handles.
    /// </summary>
    /// <param name="files">Files that provide the image data for each layer.</param>
    public GLTextureArray( params FileInfo[] files )
        : this( false, files )
    {
    }

    /// <summary>
    /// Create a texture array from file handles with optional mipmap generation.
    /// </summary>
    /// <param name="useMipMaps">True to generate mipmaps for the resulting texture array.</param>
    /// <param name="files">Files that provide the image data for each layer.</param>
    public GLTextureArray( bool useMipMaps, params FileInfo[] files )
        : this( useMipMaps, LughFormat.RGBA8888, files )
    {
    }

    /// <summary>
    /// Create a texture array from file handles specifying the internal pixel format.
    /// </summary>
    /// <param name="useMipMaps">True to generate mipmaps for the resulting texture array.</param>
    /// <param name="format">Internal pixel format constant (LughFormat).</param>
    /// <param name="files">Files that provide the image data for each layer.</param>
    public GLTextureArray( bool useMipMaps, int format, FileInfo[] files )
        : this( TextureArrayDataFactory.LoadFromFiles( format, useMipMaps, files ) )
    {
    }

    /// <summary>
    /// Create a texture array from a pre-built <see cref="ITextureArrayData"/> instance.
    /// </summary>
    /// <param name="data">Data provider that supplies the layers and metadata.</param>
    public GLTextureArray( ITextureArrayData data )
        : base( IGL.GLTexture2DArray, Engine.GL.GenTexture() )
    {
        _data = null!;

        Load( data );

        if ( data.Managed )
        {
            AddManagedTexture( Engine.App, this );
        }
    }

    // ========================================================================

    /// <summary>
    /// Gets the width of the texture array (pixels).
    /// </summary>
    public int Width => _data.Width;

    /// <summary>
    /// Gets the height of the texture array (pixels).
    /// </summary>
    public int Height => _data.Height;

    /// <summary>
    /// Gets the depth (number of layers) in the texture array.
    /// </summary>
    public override int Depth => _data.Depth;

    /// <summary>
    /// Indicates whether the underlying texture data is managed by the engine.
    /// Managed textures are tracked and can be reloaded automatically when needed.
    /// </summary>
    public bool IsManaged => _data.Managed;

    // ========================================================================

    /// <summary>
    /// Gets a short human-readable status string describing the number of managed
    /// texture arrays per registered application instance.
    /// </summary>
    public string ManagedStatus
    {
        get
        {
            var builder = new StringBuilder( "Managed TextureArrays/app: { " );

            foreach ( IApplication app in _managedTextureArrays.Keys )
            {
                builder.Append( _managedTextureArrays[ app ].Count );
                builder.Append( ' ' );
            }

            builder.Append( '}' );

            return builder.ToString();
        }
    }

    /// <summary>
    /// Gets the number of managed texture arrays registered for the current application.
    /// </summary>
    public int NumManagedTextureArrays => _managedTextureArrays[ Engine.App ].Count;

    private static FileInfo[] GetInternalHandles( params string[] internalPaths )
    {
        var handles = new FileInfo[ internalPaths.Length ];

        for ( var i = 0; i < internalPaths.Length; i++ )
        {
            handles[ i ] = Engine.Files.Internal( internalPaths[ i ] );
        }

        return handles;
    }

    /// <summary>
    /// Uploads texture array storage and optionally consumes pixel data from the provided data provider.
    /// </summary>
    /// <param name="data">The <see cref="ITextureArrayData"/> that provides format, dimensions and pixel content.</param>
    /// <exception cref="RuntimeException">Thrown when attempting to replace data with differing managed status.</exception>
    private void Load( ITextureArrayData data )
    {
        if ( ( _data != null ) && ( data.Managed != _data.Managed ) )
        {
            throw new RuntimeException
                ( "New data must have the same managed status as the old data" );
        }

        _data = data;

        Bind();

        Engine.GL.TexImage3D( IGL.GLTexture2DArray,
                              0,
                              data.InternalFormat,
                              data.Width,
                              data.Height,
                              data.Depth,
                              0,
                              data.InternalFormat,
                              data.GLDataType,
                              0 );

        if ( !data.Prepared )
        {
            data.Prepare();
        }

        data.ConsumeTextureArrayData();

        SetFilter( MinFilter, MagFilter );
        SetWrap( UWrap, VWrap );

        Engine.GL.BindTexture( GLTarget, 0 );
    }

    /// <summary>
    /// Reload the texture from its data provider. Only valid for managed textures.
    /// </summary>
    /// <exception cref="RuntimeException">Thrown when called on an unmanaged texture.</exception>
    public override void Reload()
    {
        if ( !IsManaged )
        {
            throw new RuntimeException( "Tried to reload an unmanaged TextureArray" );
        }

        GLTextureHandle = Engine.GL.GenTexture();

        Load( _data );
    }

    /// <summary>
    /// Register a managed texture array instance for the specified application.
    /// </summary>
    /// <param name="app">Application instance that owns the texture.</param>
    /// <param name="texture">Texture array to register.</param>
    private static void AddManagedTexture( IApplication app, GLTextureArray texture )
    {
        List< GLTextureArray > managedTextureArray = _managedTextureArrays[ app ];

        _managedTextureArrays[ app ].Add( texture );
        _managedTextureArrays[ app ] = managedTextureArray;
    }

    // ========================================================================

    /// <summary>
    /// Remove all managed texture arrays for the given application.
    /// </summary>
    /// <param name="app">Application whose managed texture arrays should be cleared.</param>
    internal static void ClearAllTextureArrays( IApplication app )
    {
        _managedTextureArrays.Remove( app );
    }

    /// <summary>
    /// Force a reload of all managed texture arrays for the given application.
    /// This is typically used after an OpenGL context loss or similar event that
    /// invalidates native handles.
    /// </summary>
    /// <param name="app">Application whose managed texture arrays should be reloaded.</param>
    internal static void InvalidateAllTextureArrays( IApplication app )
    {
        foreach ( GLTextureArray textureArray in _managedTextureArrays[ app ] )
        {
            textureArray.Reload();
        }
    }

    // ========================================================================
    // Implementations of abstract methods from the base Image class.

    /// <summary>
    /// Not implemented for texture arrays. Present to satisfy the Image contract.
    /// </summary>
    /// <param name="color">Color used to clear the image.</param>
    public void ClearWithColor( Color color ) => throw new NotImplementedException();

    /// <summary>
    /// Not implemented for texture arrays. Present to satisfy the Image contract.
    /// </summary>
    /// <param name="x">X coordinate of the pixel.</param>
    /// <param name="y">Y coordinate of the pixel.</param>
    /// <returns>Color value as an integer.</returns>
    public int GetPixel( int x, int y ) => throw new NotImplementedException();

    /// <summary>
    /// Not implemented for texture arrays. Present to satisfy the Image contract.
    /// </summary>
    /// <param name="x">X coordinate of the pixel.</param>
    /// <param name="y">Y coordinate of the pixel.</param>
    /// <param name="color">Color to set.</param>
    public void SetPixel( int x, int y, Color color ) => throw new NotImplementedException();

    /// <summary>
    /// Not implemented for texture arrays. Present to satisfy the Image contract.
    /// </summary>
    /// <param name="x">X coordinate of the pixel.</param>
    /// <param name="y">Y coordinate of the pixel.</param>
    /// <param name="color">Color value as an integer.</param>
    public void SetPixel( int x, int y, int color ) => throw new NotImplementedException();
}

// ============================================================================
// ============================================================================