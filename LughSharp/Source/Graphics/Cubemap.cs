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
using System.IO;
using System.Text;

using JetBrains.Annotations;

using LughSharp.Source.Assets;
using LughSharp.Source.Assets.Loaders;
using LughSharp.Source.Collections;
using LughSharp.Source.Graphics.Images;
using LughSharp.Source.Graphics.Images.TextureData;
using LughSharp.Source.Graphics.Loaders;
using LughSharp.Source.Graphics.OpenGL;
using LughSharp.Source.Graphics.Utils;
using LughSharp.Source.Maths;
using LughSharp.Source.Utils;
using LughSharp.Source.Utils.Exceptions;

namespace LughSharp.Source.Graphics;

/// <summary>
/// Wraps a standard OpenGL Cubemap. Must be disposed when it is no longer used.
/// </summary>
[PublicAPI]
public class Cubemap : GLTexture, IManaged
{
    public static AssetManager? AssetManager { get; set; }
    public        ICubemapData  Data         { get; set; }

    // ========================================================================

    private static readonly Dictionary< IApplication, List< Cubemap >? > _managedCubemaps = new();

    // ========================================================================

    /// <summary>
    /// Construct a Cubemap based on the given CubemapData.
    /// </summary>
    public Cubemap( ICubemapData? data ) : base( IGL.GLTextureCubeMap )
    {
        Guard.Against.Null( data );

        Data = data;

        Load( data );

        if ( data.IsManaged )
        {
            AddManagedCubemap( Engine.App, this );
        }
    }

    /// <summary>
    /// Construct a Cubemap with the specified texture files for the sides,
    /// optionally generating mipmaps (Default is do not generate mipmaps).
    /// </summary>
    public Cubemap( FileInfo positiveX,
                    FileInfo negativeX,
                    FileInfo positiveY,
                    FileInfo negativeY,
                    FileInfo positiveZ,
                    FileInfo negativeZ,
                    bool useMipMaps = false )
        : this( TextureDataFactory.LoadFromFile( positiveX, useMipMaps ),
                TextureDataFactory.LoadFromFile( negativeX, useMipMaps ),
                TextureDataFactory.LoadFromFile( positiveY, useMipMaps ),
                TextureDataFactory.LoadFromFile( negativeY, useMipMaps ),
                TextureDataFactory.LoadFromFile( positiveZ, useMipMaps ),
                TextureDataFactory.LoadFromFile( negativeZ, useMipMaps ) )
    {
    }

    /// <summary>
    /// Construct a Cubemap with the specified <see cref="Pixmap"/>s for the sides,
    /// optionally generating mipmaps.
    /// </summary>
    public Cubemap( Pixmap? positiveX,
                    Pixmap? negativeX,
                    Pixmap? positiveY,
                    Pixmap? negativeY,
                    Pixmap? positiveZ,
                    Pixmap? negativeZ,
                    bool useMipMaps = false )
        : this( positiveX == null ? null : new PixmapTextureData( positiveX, 0, useMipMaps, false ),
                negativeX == null ? null : new PixmapTextureData( negativeX, 0, useMipMaps, false ),
                positiveY == null ? null : new PixmapTextureData( positiveY, 0, useMipMaps, false ),
                negativeY == null ? null : new PixmapTextureData( negativeY, 0, useMipMaps, false ),
                positiveZ == null ? null : new PixmapTextureData( positiveZ, 0, useMipMaps, false ),
                negativeZ == null ? null : new PixmapTextureData( negativeZ, 0, useMipMaps, false ) )
    {
    }

    /// <summary>
    /// Construct a Cubemap with <see cref="Pixmap"/>s for each side of the specified size.
    /// </summary>
    public Cubemap( int width, int height, int depth, int format )
        : this( new PixmapTextureData( new Pixmap( depth, height, format ), 0, false, true ),
                new PixmapTextureData( new Pixmap( depth, height, format ), 0, false, true ),
                new PixmapTextureData( new Pixmap( width, depth, format ), 0, false, true ),
                new PixmapTextureData( new Pixmap( width, depth, format ), 0, false, true ),
                new PixmapTextureData( new Pixmap( width, height, format ), 0, false, true ),
                new PixmapTextureData( new Pixmap( width, height, format ), 0, false, true ) )
    {
    }

    /// <summary>
    /// Construct a Cubemap with the specified <see cref="ITextureData"/>'s for the sides
    /// </summary>
    public Cubemap( ITextureData? positiveX,
                    ITextureData? negativeX,
                    ITextureData? positiveY,
                    ITextureData? negativeY,
                    ITextureData? positiveZ,
                    ITextureData? negativeZ )
        : this( new FacedCubemapData( positiveX, negativeX, positiveY, negativeY, positiveZ, negativeZ ) )
    {
    }

    // ========================================================================

    /// <summary>
    /// Gets the width of the data in pixels.
    /// </summary>
    public int Width  => Data.Width;

    /// <summary>
    /// Gets the height of the data in pixels.
    /// </summary>
    public int Height => Data.Height;

    /// <summary>
    /// Gets the depth of the current node in the hierarchy.
    /// </summary>
    public override int Depth => 0;

    /// <summary>
    /// return the number of managed cubemaps currently loaded
    /// </summary>
    public static int NumManagedCubemaps => _managedCubemaps[ Engine.App ]?.Count ?? 0;

    /// <summary>
    /// Gets a value indicating whether the resource is managed by the system.
    /// </summary>
    public bool IsManaged => Data.IsManaged;

    // ========================================================================

    /// <summary>
    /// Sets the sides of this cubemap to the specified <see cref="ICubemapData"/>.
    /// </summary>
    public void Load( ICubemapData data )
    {
        if ( !data.IsPrepared )
        {
            data.Prepare();
        }

        Bind();

        UnsafeSetFilter( MinFilter, MagFilter, true );
        UnsafeSetWrap( UWrap, VWrap, true );
        UnsafeSetAnisotropicFilter( AnisotropicFilterLevel, true );

        data.ConsumeCubemapData();

        Engine.GL.BindTexture( GLTarget, 0 );
    }

    /// <summary>
    /// Used internally to reload after context loss. Creates a new GL handle then
    /// calls <see cref="Load(ICubemapData?)"/>.
    /// </summary>
    public override void Reload()
    {
        if ( !IsManaged )
        {
            throw new RuntimeException( "Tried to reload an unmanaged Cubemap" );
        }

        GLTextureHandle = Engine.GL.GenTexture();

        Load( Data );
    }

    /// <summary>
    /// Adds a new entry to the list of managed cubemnaps.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="cubemap"></param>
    private static void AddManagedCubemap( IApplication app, Cubemap cubemap )
    {
        List< Cubemap > managedCubemapArray = _managedCubemaps[ app ] ?? new List< Cubemap >();

        managedCubemapArray.Add( cubemap );
        _managedCubemaps.Put( app, managedCubemapArray );
    }

    /// <summary>
    /// Clears all managed cubemaps.
    /// </summary>
    public static void ClearAllCubemaps( IApplication app )
    {
        _managedCubemaps.Remove( app );
    }

    /// <summary>
    /// Invalidate all managed cubemaps. This is an internal method. Do not use it!
    /// </summary>
    public static void InvalidateAllCubemaps( IApplication app )
    {
        List< Cubemap >? managedCubemapArray = _managedCubemaps[ app ];

        if ( managedCubemapArray == null )
        {
            return;
        }

        if ( AssetManager == null )
        {
            foreach ( Cubemap cubemap in managedCubemapArray )
            {
                cubemap.Reload();
            }
        }
        else
        {
            // first we have to make sure the AssetManager isn't loading anything anymore,
            // otherwise the ref counting trick below wouldn't work (when a cubemap is
            // currently on the task stack of the manager.)
            AssetManager.FinishLoading();

            // next we go through each cubemap and reload either directly or via the
            // asset manager.
            var cubemaps = new List< Cubemap >( managedCubemapArray );

            foreach ( Cubemap cubemap in cubemaps )
            {
                string? filename = AssetManager.GetAssetFileName( cubemap );

                if ( filename == null )
                {
                    cubemap.Reload();
                }
                else
                {
                    // get the ref count of the cubemap, then set it to 0 so we
                    // can actually remove it from the assetmanager. Also set the
                    // handle to zero, otherwise we might accidentially dispose
                    // already reloaded cubemaps.
                    int refCount = AssetManager.GetReferenceCount( filename );

                    AssetManager.SetReferenceCount( filename, 0 );

                    cubemap.GLTextureHandle = 0;

                    // create the parameters, passing the reference to the cubemap as
                    // well as a callback that sets the ref count.
                    var parameter = new CubemapLoader.CubemapParameter
                    {
                        CubemapData = cubemap.Data,
                        MinFilter   = cubemap.MinFilter,
                        MagFilter   = cubemap.MagFilter,
                        WrapU       = cubemap.UWrap,
                        WrapV       = cubemap.VWrap,

                        // special parameter which will ensure that the references stay the same.
                        Cubemap        = cubemap,
                        LoadedCallback = new DefaultLoadedCallback( refCount )
                    };

                    // unload the c, create a new gl handle then reload it.
                    AssetManager.Unload( filename );

                    cubemap.GLTextureHandle = Engine.GL.GenTexture();
                    AssetManager.Load< Cubemap >( filename, parameter );
                }
            }

            managedCubemapArray.Clear();
            managedCubemapArray.AddAll( cubemaps );
        }
    }

    /// <summary>
    /// Returns a string describing the managed status of this Cubemap.
    /// </summary>
    public static string GetManagedStatus()
    {
        var builder = new StringBuilder( "Managed cubemap/app: { " );

        foreach ( IApplication app in _managedCubemaps.Keys )
        {
            builder.Append( _managedCubemaps[ app ]!.Count );
            builder.Append( ' ' );
        }

        builder.Append( '}' );

        return builder.ToString();
    }

    // ========================================================================

    public void ClearWithColor( Color color )
    {
        //TODO:
    }

    public int GetPixel( int x, int y )
    {
        //TODO:
        return 0;
    }

    public void SetPixel( int x, int y, Color color )
    {
        //TODO:
    }

    public void SetPixel( int x, int y, int color )
    {
        //TODO:
    }

    // ========================================================================

    /// <inheritdoc />
    protected override void Dispose( bool disposing )
    {
        if ( disposing )
        {
            // this is a hack. reason: we have to set the glHandle to 0 for textures
            // that are reloaded through the asset manager as we first remove (and
            // thus dispose) the texture and then reload it. the glHandle is set to 0
            // in InvalidateAllTextures prior to removal from the asset manager.
            if ( GLTextureHandle == 0 )
            {
                return;
            }

            Delete();

            if ( Data.IsManaged )
            {
                if ( _managedCubemaps[ Engine.App ] != null )
                {
                    _managedCubemaps[ Engine.App ]?.Remove( this );
                }
            }
        }
    }

    // ========================================================================

    /// <summary>
    /// Enum to identify each side of a Cubemap
    /// </summary>
    [PublicAPI]
    public class CubemapSide
    {
        /// <summary>
        /// Specifies the possible directions along the X, Y, and Z axes.
        /// </summary>
        /// <remarks>
        /// Use this enumeration to represent orientation or movement along the principal axes in
        /// three-dimensional space. Each value indicates a positive or negative direction along
        /// a specific axis.
        /// </remarks>
        public enum InnerEnum
        {
            PositiveX,
            NegativeX,
            PositiveY,
            NegativeY,
            PositiveZ,
            NegativeZ
        }

        // ====================================================================

        public InnerEnum InnerEnumValue { get; private set; }
        public int       OrdinalValue   { get; private set; }

        // ====================================================================

        /// <summary>
        /// The zero based index of the side in the cubemap
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The OpenGL target (used for glTexImage2D) of the side.
        /// </summary>
        public int GLTarget { get; set; }

        /// <summary>
        /// The up vector to target the side.
        /// </summary>
        public Vector3 Up { get; set; }

        /// <summary>
        /// The direction vector to target the side.
        /// </summary>
        public Vector3 Direction { get; set; }

        // ====================================================================

        private static List< CubemapSide > _valueList = new();
        private static int                 _nextOrdinal;
        private        string              _nameValue;

        // ====================================================================

        static CubemapSide()
        {
            _valueList.Add( PositiveX );
            _valueList.Add( NegativeX );
            _valueList.Add( PositiveY );
            _valueList.Add( NegativeY );
            _valueList.Add( PositiveZ );
            _valueList.Add( NegativeZ );
        }

        /// <summary>
        /// Initializes a new instance of the CubemapSide class with the specified name, orientation,
        /// and OpenGL target
        /// information.
        /// </summary>
        /// <param name="name">The unique name identifying the cubemap side.</param>
        /// <param name="innerEnum">
        /// An enumeration value representing the internal type or orientation of the cubemap side.
        /// </param>
        /// <param name="index">The zero-based index of the cubemap side within the cubemap.</param>
        /// <param name="glTarget">The OpenGL target constant associated with this cubemap side.</param>
        /// <param name="upX">The X component of the up vector for the cubemap side's orientation.</param>
        /// <param name="upY">The Y component of the up vector for the cubemap side's orientation.</param>
        /// <param name="upZ">The Z component of the up vector for the cubemap side's orientation.</param>
        /// <param name="directionX">The X component of the direction vector for the cubemap side's orientation.</param>
        /// <param name="directionY">The Y component of the direction vector for the cubemap side's orientation.</param>
        /// <param name="directionZ">The Z component of the direction vector for the cubemap side's orientation.</param>
        public CubemapSide( string name,
                            InnerEnum innerEnum,
                            int index,
                            int glTarget,
                            float upX,
                            float upY,
                            float upZ,
                            float directionX,
                            float directionY,
                            float directionZ )
        {
            Index     = index;
            GLTarget  = glTarget;
            Up        = new Vector3( upX, upY, upZ );
            Direction = new Vector3( directionX, directionY, directionZ );

            _nameValue     = name;
            OrdinalValue   = _nextOrdinal++;
            InnerEnumValue = innerEnum;
        }

        /// <summary>
        /// Sets the supplied <see cref="Maths.Vector3"/> to the contents of
        /// <see cref="Up"/> and returns it to the caller.
        /// </summary>
        public Vector3 GetUp( Vector3 vec3 )
        {
            return vec3.Set( Up );
        }

        /// <summary>
        /// Sets the supplied <see cref="Maths.Vector3"/> to the contents of
        /// <see cref="Direction"/> and returns it to the caller.
        /// </summary>
        public Vector3 GetDirection( Vector3 vec3 )
        {
            return vec3.Set( Direction );
        }

        /// <summary>
        /// Returns an array of all <see cref="CubemapSide"/> values.
        /// </summary>
        public static CubemapSide[] Values()
        {
            return _valueList.ToArray();
        }

        /// <summary>
        /// Returns the <see cref="CubemapSide"/> with the specified name.
        /// </summary>
        /// <param name="name">The name of the cubemap side.</param>
        /// <returns>The <see cref="CubemapSide"/> with the specified name.</returns>
        /// <exception cref="ArgumentException">Thrown if no cubemap side with the specified name exists.</exception>
        public static CubemapSide ValueOf( string name )
        {
            foreach ( CubemapSide enumInstance in _valueList )
            {
                if ( enumInstance._nameValue == name )
                {
                    return enumInstance;
                }
            }

            throw new ArgumentException( name );
        }

        /// <summary>
        /// The positive X and first side of the cubemap
        /// </summary>
        public static readonly CubemapSide PositiveX =
            new( "PositiveX", InnerEnum.PositiveX, 0, IGL.GLTextureCubeMapPositiveX, 0, -1, 0, 1, 0, 0 );

        /// <summary>
        /// The negative X and second side of the cubemap
        /// </summary>
        public static readonly CubemapSide NegativeX =
            new( "NegativeX", InnerEnum.NegativeX, 1, IGL.GLTextureCubeMapNegativeX, 0, -1, 0, -1, 0, 0 );

        /// <summary>
        /// The positive Y and third side of the cubemap
        /// </summary>
        public static readonly CubemapSide PositiveY =
            new( "PositiveY", InnerEnum.PositiveY, 2, IGL.GLTextureCubeMapPositiveY, 0, 0, 1, 0, 1, 0 );

        /// <summary>
        /// The negative Y and fourth side of the cubemap
        /// </summary>
        public static readonly CubemapSide NegativeY =
            new( "NegativeY", InnerEnum.NegativeY, 3, IGL.GLTextureCubeMapNegativeY, 0, 0, -1, 0, -1, 0 );

        /// <summary>
        /// The positive Z and fifth side of the cubemap
        /// </summary>
        public static readonly CubemapSide PositiveZ =
            new( "PositiveZ", InnerEnum.PositiveZ, 4, IGL.GLTextureCubeMapPositiveZ, 0, -1, 0, 0, 0, 1 );

        /// <summary>
        /// The negative Z and sixth side of the cubemap
        /// </summary>
        public static readonly CubemapSide NegativeZ =
            new( "NegativeZ", InnerEnum.NegativeZ, 5, IGL.GLTextureCubeMapNegativeZ, 0, -1, 0, 0, 0, -1 );

        /// <inheritdoc />
        public override string ToString()
        {
            return _nameValue;
        }
    }
}

// ============================================================================
// ============================================================================