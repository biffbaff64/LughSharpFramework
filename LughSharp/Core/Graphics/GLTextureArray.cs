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

using System.Diagnostics.CodeAnalysis;
using System.Text;

using JetBrains.Annotations;

using LughSharp.Core.Graphics.OpenGL;
using LughSharp.Core.Main;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Graphics;

/// <summary>
/// OpenGL wrapper for TextureArray.
/// </summary>
[PublicAPI]
public class GLTextureArray : GLTexture, IManaged
{
    private static readonly Dictionary< IApplication, List< GLTextureArray > > ManagedTextureArrays = new();

    private ITextureArrayData _data;

    // ========================================================================

    /// <summary>
    /// Returns a string representation of the managed status of all
    /// textures in the managed textures array.
    /// </summary>
    public string ManagedStatus
    {
        get
        {
            var builder = new StringBuilder( "Managed TextureArrays/app: { " );

            foreach ( IApplication app in ManagedTextureArrays.Keys )
            {
                builder.Append( ManagedTextureArrays[ app ].Count );
                builder.Append( ' ' );
            }

            builder.Append( '}' );

            return builder.ToString();
        }
    }

    /// <summary>
    /// Gets the number of managed TextureArrays currently loaded.
    /// </summary>
    public int NumManagedTextureArrays => ManagedTextureArrays[ Engine.Api.App ].Count;

    // ========================================================================

    public          int Width  => _data.Width;
    public          int Height => _data.Height;
    public override int Depth  => _data.Depth;

    [SuppressMessage( "ReSharper", "ValueParameterNotUsed" )]
    public bool IsManaged
    {
        get => _data.Managed;
        set { }
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="internalPaths"></param>
    public GLTextureArray( params string[] internalPaths )
        : this( GetInternalHandles( internalPaths ) )
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="files"></param>
    public GLTextureArray( params FileInfo[] files )
        : this( false, files )
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="useMipMaps"></param>
    /// <param name="files"></param>
    public GLTextureArray( bool useMipMaps, params FileInfo[] files )
        : this( useMipMaps, LughFormat.RGBA8888, files )
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="useMipMaps"></param>
    /// <param name="format"></param>
    /// <param name="files"></param>
    public GLTextureArray( bool useMipMaps, int format, FileInfo[] files )
        : this( TextureArrayDataFactory.LoadFromFiles( format, useMipMaps, files ) )
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="data"></param>
    /// <exception cref="RuntimeException"></exception>
    public GLTextureArray( ITextureArrayData data )
        : base( IGL.GL_TEXTURE_2D_ARRAY, Engine.GL.GenTexture() )
    {
        _data = null!;

        Load( data );

        if ( data.Managed )
        {
            AddManagedTexture( Engine.Api.App, this );
        }
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="internalPaths"></param>
    /// <returns></returns>
    private static FileInfo[] GetInternalHandles( params string[] internalPaths )
    {
        var handles = new FileInfo[ internalPaths.Length ];

        for ( var i = 0; i < internalPaths.Length; i++ )
        {
            handles[ i ] = Engine.Api.Files.Internal( internalPaths[ i ] );
        }

        return handles;
    }

    /// <summary>
    /// </summary>
    /// <param name="data"></param>
    /// <exception cref="RuntimeException"></exception>
    private void Load( ITextureArrayData data )
    {
        if ( ( _data != null ) && ( data.Managed != _data.Managed ) )
        {
            throw new RuntimeException
                ( "New data must have the same managed status as the old data" );
        }

        _data = data;

        Bind();

        Engine.GL.TexImage3D( IGL.GL_TEXTURE_2D_ARRAY,
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
    /// Attempts to reload the TextureArray.
    /// </summary>
    /// <exception cref="RuntimeException"> If the TextureArray is unmanaged. </exception>
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
    /// </summary>
    /// <param name="app"></param>
    /// <param name="texture"></param>
    private static void AddManagedTexture( IApplication app, GLTextureArray texture )
    {
        List< GLTextureArray > managedTextureArray = ManagedTextureArrays[ app ];

        ManagedTextureArrays[ app ].Add( texture );
        ManagedTextureArrays[ app ] = managedTextureArray;
    }

    // ========================================================================

    /// <summary>
    /// Clears all managed TextureArrays.
    /// </summary>
    internal static void ClearAllTextureArrays( IApplication app )
    {
        ManagedTextureArrays.Remove( app );
    }

    /// <summary>
    /// Invalidate all managed TextureArrays.
    /// </summary>
    internal static void InvalidateAllTextureArrays( IApplication app )
    {
        foreach ( GLTextureArray textureArray in ManagedTextureArrays[ app ] )
        {
            textureArray.Reload();
        }
    }

    // ========================================================================
    // Implementations of abstract methods from the base Image class.

    public void ClearWithColor( Color color )
    {
        throw new NotImplementedException();
    }

    public int GetPixel( int x, int y )
    {
        throw new NotImplementedException();
    }

    public void SetPixel( int x, int y, Color color )
    {
        throw new NotImplementedException();
    }

    public void SetPixel( int x, int y, int color )
    {
        throw new NotImplementedException();
    }
}

// ============================================================================
// ============================================================================