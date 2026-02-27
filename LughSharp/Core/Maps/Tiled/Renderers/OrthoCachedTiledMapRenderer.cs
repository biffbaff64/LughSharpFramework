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

using JetBrains.Annotations;

using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Cameras;
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Graphics.OpenGL;
using LughSharp.Core.Graphics.OpenGL.Enums;
using LughSharp.Core.Main;
using LughSharp.Core.Maths;
using LughSharp.Core.Utils.Exceptions;

using Color = LughSharp.Core.Graphics.Color;
using Rectangle = LughSharp.Core.Maths.Rectangle;

namespace LughSharp.Core.Maps.Tiled.Renderers;

/// <summary>
/// Renders ortho tiles by caching geometry on the GPU. How much is cached is controlled
/// by SetOverCache(float). When the view reaches the edge of the cached tiles, the cache
/// is rebuilt at the new view position. This class may have poor performance when tiles
/// are often changed dynamically, since the cache must be rebuilt after each change.
/// </summary>
[PublicAPI]
public class OrthoCachedTiledMapRenderer : ITiledMapRenderer, IDisposable
{
    private const    int       DEFAULT_CACHE_SIZE = 2000;
    private static   float     _tolerance         = 0.00001f;
    protected static int       NumVertices        = 20;
    protected        bool      Blending;
    protected        Rectangle CacheBounds = new();
    protected        bool      Cached;
    protected        bool      CanCacheMoreE;
    protected        bool      CanCacheMoreN;
    protected        bool      CanCacheMoreS;
    protected        bool      CanCacheMoreW;
    protected        int       Count;

    protected TiledMap?    Map;
    protected float        MaxTileHeight;
    protected float        MaxTileWidth;
    protected SpriteCache? SpriteCache;
    protected float        UnitScale;
    protected float[]      Vertices   = new float[ 20 ];
    protected Rectangle    ViewBounds = new();

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="map"></param>
    /// <param name="unitScale"></param>
    /// <param name="cacheSize">
    /// The maximum number of tiles that can be cached. The default size is 2000.
    /// </param>
    public OrthoCachedTiledMapRenderer( TiledMap map, float unitScale, int cacheSize = DEFAULT_CACHE_SIZE )
    {
        Map         = map;
        UnitScale   = unitScale;
        SpriteCache = new SpriteCache( cacheSize, true );
    }

    /// <summary>
    /// Sets the percentage of the view that is cached in each direction. Default is 0.5.
    /// <para>
    /// Eg, 0.75 will cache 75% of the width of the view to the left and right of the view,
    /// and 75% of the height of the view above and below the view.
    /// </para>
    /// </summary>
    public float OverCache { get; set; } = 0.50f;

    // <inheritdoc />
    public void Dispose()
    {
        SpriteCache?.Dispose();
    }

    public void SetView( OrthographicCamera camera )
    {
        Guard.Against.Null( SpriteCache );

        SpriteCache.ProjectionMatrix = camera.Combined;

        float width  = ( camera.ViewportWidth * camera.Zoom ) + ( MaxTileWidth * 2 * UnitScale );
        float height = ( camera.ViewportHeight * camera.Zoom ) + ( MaxTileHeight * 2 * UnitScale );

        ViewBounds.Set( camera.Position.X - ( width / 2 ), camera.Position.Y - ( height / 2 ), width, height );

        if ( ( CanCacheMoreW && ( ViewBounds.X < ( CacheBounds.X - _tolerance ) ) )
          || ( CanCacheMoreS && ( ViewBounds.Y < ( CacheBounds.Y - _tolerance ) ) )
          || ( CanCacheMoreE
            && ( ( ViewBounds.X + ViewBounds.Width ) > ( CacheBounds.X + CacheBounds.Width + _tolerance ) ) )
          || ( CanCacheMoreN && ( ( ViewBounds.Y + ViewBounds.Height )
                                > ( CacheBounds.Y + CacheBounds.Height + _tolerance ) ) ) )
        {
            Cached = false;
        }
    }

    public void SetView( Matrix4 projection, float x, float y, float width, float height )
    {
        Guard.Against.Null( SpriteCache );

        SpriteCache.ProjectionMatrix = projection;

        x      -= MaxTileWidth * UnitScale;
        y      -= MaxTileHeight * UnitScale;
        width  += MaxTileWidth * 2 * UnitScale;
        height += MaxTileHeight * 2 * UnitScale;

        ViewBounds.Set( x, y, width, height );

        if ( ( CanCacheMoreW && ( ViewBounds.X < ( CacheBounds.X - _tolerance ) ) )
          || ( CanCacheMoreS && ( ViewBounds.Y < ( CacheBounds.Y - _tolerance ) ) )
          || ( CanCacheMoreE
            && ( ( ViewBounds.X + ViewBounds.Width ) > ( CacheBounds.X + CacheBounds.Width + _tolerance ) ) )
          || ( CanCacheMoreN && ( ( ViewBounds.Y + ViewBounds.Height )
                                > ( CacheBounds.Y + CacheBounds.Height + _tolerance ) ) ) )
        {
            Cached = false;
        }
    }

    public void Render()
    {
        Guard.Against.Null( SpriteCache );

        if ( !Cached )
        {
            Cached = true;
            Count  = 0;
            SpriteCache.Clear();

            float extraWidth  = ViewBounds.Width * OverCache;
            float extraHeight = ViewBounds.Height * OverCache;

            CacheBounds.X      = ViewBounds.X - extraWidth;
            CacheBounds.Y      = ViewBounds.Y - extraHeight;
            CacheBounds.Width  = ViewBounds.Width + ( extraWidth * 2 );
            CacheBounds.Height = ViewBounds.Height + ( extraHeight * 2 );

            foreach ( MapLayer layer in Map!.Layers )
            {
                SpriteCache.BeginCache();

                if ( layer is TiledMapTileLayer tileLayer )
                {
                    RenderTileLayer( tileLayer );
                }
                else if ( layer is TiledMapImageLayer imageLayer )
                {
                    RenderImageLayer( imageLayer );
                }

                SpriteCache.EndCache();
            }
        }

        if ( Blending )
        {
            Engine.GL.Enable( EnableCap.Blend );
            Engine.GL.BlendFunc( IGL.GL_SRC_ALPHA, IGL.GL_ONE_MINUS_SRC_ALPHA );
        }

        SpriteCache.Begin();

        MapLayers? mapLayers = Map?.Layers;

        for ( int i = 0, j = mapLayers!.LayersCount; i < j; i++ )
        {
            MapLayer layer = mapLayers.Get( i );

            if ( layer.Visible )
            {
                SpriteCache.Draw( i );
                RenderObjects( layer );
            }
        }

        SpriteCache.End();

        if ( Blending )
        {
            Engine.GL.Disable( EnableCap.Blend );
        }
    }

    public void Render( int[] layers )
    {
        if ( !Cached )
        {
            Cached = true;
            Count  = 0;
            SpriteCache?.Clear();

            float extraWidth  = ViewBounds.Width * OverCache;
            float extraHeight = ViewBounds.Height * OverCache;

            CacheBounds.X      = ViewBounds.X - extraWidth;
            CacheBounds.Y      = ViewBounds.Y - extraHeight;
            CacheBounds.Width  = ViewBounds.Width + ( extraWidth * 2 );
            CacheBounds.Height = ViewBounds.Height + ( extraHeight * 2 );

            foreach ( MapLayer layer in Map!.Layers )
            {
                SpriteCache?.BeginCache();

                if ( layer is TiledMapTileLayer tileLayer )
                {
                    RenderTileLayer( tileLayer );
                }
                else if ( layer is TiledMapImageLayer imageLayer )
                {
                    RenderImageLayer( imageLayer );
                }

                SpriteCache?.EndCache();
            }
        }

        if ( Blending )
        {
            Engine.GL.Enable( EnableCap.Blend );
            Engine.GL.BlendFunc( IGL.GL_SRC_ALPHA, IGL.GL_ONE_MINUS_SRC_ALPHA );
        }

        SpriteCache?.Begin();

        foreach ( int i in layers )
        {
            MapLayer? layer = Map?.Layers.Get( i );

            if ( layer!.Visible )
            {
                SpriteCache?.Draw( i );
                RenderObjects( layer );
            }
        }

        SpriteCache?.End();

        if ( Blending )
        {
            Engine.GL.Disable( EnableCap.Blend );
        }
    }

    public void RenderObjects( MapLayer layer )
    {
        foreach ( MapObject mapObject in layer.Objects )
        {
            RenderObject( mapObject );
        }
    }

    public void RenderObject( MapObject mapObject )
    {
    }

    public void RenderTileLayer( TiledMapTileLayer layer )
    {
        float color = Color.ToFloatBitsAbgr( 1, 1, 1, layer.Opacity );

        int layerWidth  = layer.Width;
        int layerHeight = layer.Height;

        float layerTileWidth  = layer.TileWidth * UnitScale;
        float layerTileHeight = layer.TileHeight * UnitScale;
        float layerOffsetX    = layer.RenderOffsetX * UnitScale;

        // offset in tiled is y down, so we flip it
        float layerOffsetY = -layer.RenderOffsetY * UnitScale;

        int col1 = Math.Max( 0, ( int )( ( CacheBounds.X - layerOffsetX ) / layerTileWidth ) );
        int col2 = Math.Min( layerWidth,
                             ( int )( ( CacheBounds.X + CacheBounds.Width + layerTileWidth - layerOffsetX )
                                    / layerTileWidth ) );

        int row1 = Math.Max( 0, ( int )( ( CacheBounds.Y - layerOffsetY ) / layerTileHeight ) );
        int row2 = Math.Min( layerHeight,
                             ( int )( ( CacheBounds.Y + CacheBounds.Height + layerTileHeight - layerOffsetY )
                                    / layerTileHeight ) );

        CanCacheMoreN = row2 < layerHeight;
        CanCacheMoreE = col2 < layerWidth;
        CanCacheMoreW = col1 > 0;
        CanCacheMoreS = row1 > 0;

        for ( int row = row2; row >= row1; row-- )
        {
            for ( int col = col1; col < col2; col++ )
            {
                TiledMapTileLayer.Cell? cell = layer.GetCell( col, row );

                if ( cell == null )
                {
                    continue;
                }

                ITiledMapTile? tile = cell.GetTile();

                if ( tile == null )
                {
                    continue;
                }

                Count++;

                bool flipX     = cell.GetFlipHorizontally();
                bool flipY     = cell.GetFlipVertically();
                int  rotations = cell.GetRotation();

                TextureRegion region  = tile.TextureRegion;
                Texture?      texture = region.Texture;

                if ( texture == null )
                {
                    return;
                }

                float x1 = ( col * layerTileWidth ) + ( tile.OffsetX * UnitScale ) + layerOffsetX;
                float y1 = ( row * layerTileHeight ) + ( tile.OffsetY * UnitScale ) + layerOffsetY;
                float x2 = x1 + ( region.GetRegionWidth() * UnitScale );
                float y2 = y1 + ( region.GetRegionHeight() * UnitScale );

                float adjustX = 0.5f / texture.Width;
                float adjustY = 0.5f / texture.Height;
                float u1      = region.U + adjustX;
                float v1      = region.V2 - adjustY;
                float u2      = region.U2 - adjustX;
                float v2      = region.V + adjustY;

                Vertices[ IBatch.X1 ] = x1;
                Vertices[ IBatch.Y1 ] = y1;
                Vertices[ IBatch.C1 ] = color;
                Vertices[ IBatch.U1 ] = u1;
                Vertices[ IBatch.V1 ] = v1;

                Vertices[ IBatch.X2 ] = x1;
                Vertices[ IBatch.Y2 ] = y2;
                Vertices[ IBatch.C2 ] = color;
                Vertices[ IBatch.U2 ] = u1;
                Vertices[ IBatch.V2 ] = v2;

                Vertices[ IBatch.X3 ] = x2;
                Vertices[ IBatch.Y3 ] = y2;
                Vertices[ IBatch.C3 ] = color;
                Vertices[ IBatch.U3 ] = u2;
                Vertices[ IBatch.V3 ] = v2;

                Vertices[ IBatch.X4 ] = x2;
                Vertices[ IBatch.Y4 ] = y1;
                Vertices[ IBatch.C4 ] = color;
                Vertices[ IBatch.U4 ] = u2;
                Vertices[ IBatch.V4 ] = v1;

                if ( flipX )
                {
                    float temp = Vertices[ IBatch.U1 ];
                    Vertices[ IBatch.U1 ] = Vertices[ IBatch.U3 ];
                    Vertices[ IBatch.U3 ] = temp;
                    temp                  = Vertices[ IBatch.U2 ];
                    Vertices[ IBatch.U2 ] = Vertices[ IBatch.U4 ];
                    Vertices[ IBatch.U4 ] = temp;
                }

                if ( flipY )
                {
                    float temp = Vertices[ IBatch.V1 ];
                    Vertices[ IBatch.V1 ] = Vertices[ IBatch.V3 ];
                    Vertices[ IBatch.V3 ] = temp;
                    temp                  = Vertices[ IBatch.V2 ];
                    Vertices[ IBatch.V2 ] = Vertices[ IBatch.V4 ];
                    Vertices[ IBatch.V4 ] = temp;
                }

                if ( rotations != 0 )
                {
                    switch ( rotations )
                    {
                        case TiledMapTileLayer.Cell.ROTATE90:
                        {
                            float tempV = Vertices[ IBatch.V1 ];
                            Vertices[ IBatch.V1 ] = Vertices[ IBatch.V2 ];
                            Vertices[ IBatch.V2 ] = Vertices[ IBatch.V3 ];
                            Vertices[ IBatch.V3 ] = Vertices[ IBatch.V4 ];
                            Vertices[ IBatch.V4 ] = tempV;

                            float tempU = Vertices[ IBatch.U1 ];
                            Vertices[ IBatch.U1 ] = Vertices[ IBatch.U2 ];
                            Vertices[ IBatch.U2 ] = Vertices[ IBatch.U3 ];
                            Vertices[ IBatch.U3 ] = Vertices[ IBatch.U4 ];
                            Vertices[ IBatch.U4 ] = tempU;

                            break;
                        }

                        case TiledMapTileLayer.Cell.ROTATE180:
                        {
                            float tempU = Vertices[ IBatch.U1 ];
                            Vertices[ IBatch.U1 ] = Vertices[ IBatch.U3 ];
                            Vertices[ IBatch.U3 ] = tempU;
                            tempU                 = Vertices[ IBatch.U2 ];
                            Vertices[ IBatch.U2 ] = Vertices[ IBatch.U4 ];
                            Vertices[ IBatch.U4 ] = tempU;

                            float tempV = Vertices[ IBatch.V1 ];
                            Vertices[ IBatch.V1 ] = Vertices[ IBatch.V3 ];
                            Vertices[ IBatch.V3 ] = tempV;
                            tempV                 = Vertices[ IBatch.V2 ];
                            Vertices[ IBatch.V2 ] = Vertices[ IBatch.V4 ];
                            Vertices[ IBatch.V4 ] = tempV;

                            break;
                        }

                        case TiledMapTileLayer.Cell.ROTATE270:
                        {
                            float tempV = Vertices[ IBatch.V1 ];
                            Vertices[ IBatch.V1 ] = Vertices[ IBatch.V4 ];
                            Vertices[ IBatch.V4 ] = Vertices[ IBatch.V3 ];
                            Vertices[ IBatch.V3 ] = Vertices[ IBatch.V2 ];
                            Vertices[ IBatch.V2 ] = tempV;

                            float tempU = Vertices[ IBatch.U1 ];
                            Vertices[ IBatch.U1 ] = Vertices[ IBatch.U4 ];
                            Vertices[ IBatch.U4 ] = Vertices[ IBatch.U3 ];
                            Vertices[ IBatch.U3 ] = Vertices[ IBatch.U2 ];
                            Vertices[ IBatch.U2 ] = tempU;

                            break;
                        }
                    }
                }

                SpriteCache?.Add( texture, Vertices, 0, NumVertices );
            }
        }
    }

    public void RenderImageLayer( TiledMapImageLayer layer )
    {
        float color = Color.ToFloatBitsAbgr( 1.0f, 1.0f, 1.0f, layer.Opacity );

        TextureRegion? region = layer.Region;

        if ( region?.Texture == null )
        {
            return;
        }

        float x  = layer.X;
        float y  = layer.Y;
        float x1 = x * UnitScale;
        float y1 = y * UnitScale;
        float x2 = x1 + ( region.GetRegionWidth() * UnitScale );
        float y2 = y1 + ( region.GetRegionHeight() * UnitScale );

        float u1 = region.U;
        float v1 = region.V2;
        float u2 = region.U2;
        float v2 = region.V;

        Vertices[ IBatch.X1 ] = x1;
        Vertices[ IBatch.Y1 ] = y1;
        Vertices[ IBatch.C1 ] = color;
        Vertices[ IBatch.U1 ] = u1;
        Vertices[ IBatch.V1 ] = v1;

        Vertices[ IBatch.X2 ] = x1;
        Vertices[ IBatch.Y2 ] = y2;
        Vertices[ IBatch.C2 ] = color;
        Vertices[ IBatch.U2 ] = u1;
        Vertices[ IBatch.V2 ] = v2;

        Vertices[ IBatch.X3 ] = x2;
        Vertices[ IBatch.Y3 ] = y2;
        Vertices[ IBatch.C3 ] = color;
        Vertices[ IBatch.U3 ] = u2;
        Vertices[ IBatch.V3 ] = v2;

        Vertices[ IBatch.X4 ] = x2;
        Vertices[ IBatch.Y4 ] = y1;
        Vertices[ IBatch.C4 ] = color;
        Vertices[ IBatch.U4 ] = u2;
        Vertices[ IBatch.V4 ] = v1;

        SpriteCache?.Add( region.Texture, Vertices, 0, NumVertices );
    }

    /// <summary>
    /// Causes the cache to be rebuilt the next time it is rendered.
    /// </summary>
    public void InvalidateCache()
    {
        Cached = false;
    }

    /// <summary>
    /// Returns true if tiles are currently cached.
    /// </summary>
    public bool IsCached()
    {
        return Cached;
    }

    /// <summary>
    /// Expands the view size in each direction, ensuring that tiles of this size or
    /// smaller are never culled from the visible portion of the view. Default is 0,0.
    /// <para>
    /// The amount of tiles cached is computed using <tt>(view size + max tile size) * overCache</tt>,
    /// meaning the max tile size increases the amount cached and possibly
    /// <see cref="OverCache"/> can be reduced.
    /// </para>
    /// <para>
    /// If the view size and <see cref="OverCache"/> are configured so the size of the
    /// cached tiles is always larger than the largest tile size, this setting is not needed.
    /// </para>
    /// </summary>
    public void SetMaxTileSize( float maxPixelWidth, float maxPixelHeight )
    {
        MaxTileWidth  = maxPixelWidth;
        MaxTileHeight = maxPixelHeight;
    }
}

// ============================================================================
// ============================================================================