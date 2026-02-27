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
using LughSharp.Core.Maps.Tiled.Tiles;
using LughSharp.Core.Maths;

using Color = LughSharp.Core.Graphics.Color;
using Rectangle = LughSharp.Core.Maths.Rectangle;

namespace LughSharp.Core.Maps.Tiled.Renderers;

[PublicAPI]
public class BatchTileMapRenderer : ITiledMapRenderer
{
    public TiledMap  TiledMap    { get; set; }
    public bool      OwnsBatch   { get; set; }
    public Rectangle ImageBounds { get; set; } = new();

    // ========================================================================

    protected IBatch    Batch      { get; set; }
    protected Rectangle ViewBounds { get; set; }
    protected float     UnitScale  { get; set; }
    protected float[]   Vertices   { get; set; } = new float[ NUM_VERTICES ];

    protected const int   NUM_VERTICES       = 20;
    protected const float DEFAULT_UNIT_SCALE = 1.0f;

    // ========================================================================

    public BatchTileMapRenderer()
        : this( new TiledMap() )
    {
    }

    /// <summary>
    /// Creates a new Renderer using the supplied <see cref="TiledMap"/>
    /// and <see cref="IBatch"/>
    /// </summary>
    protected BatchTileMapRenderer( TiledMap map, IBatch batch )
        : this( map, DEFAULT_UNIT_SCALE, batch )
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="map"></param>
    /// <param name="unitScale"></param>
    protected BatchTileMapRenderer( TiledMap map, float unitScale = DEFAULT_UNIT_SCALE )
        : this( map, unitScale, new SpriteBatch(), true )
    {
    }

    /// <summary>
    /// Provides functionality for rendering a <see cref="TiledMap"/> using a batched
    /// approach. This class serves as a base for map renderers with different tile
    /// layouts, such as orthogonal, isometric, and hexagonal.
    /// </summary>
    protected BatchTileMapRenderer( TiledMap map, float unitScale, IBatch batch, bool ownsBatch = false )
    {
        TiledMap   = map;
        UnitScale  = unitScale;
        ViewBounds = new Rectangle();
        Batch      = batch;
        OwnsBatch  = ownsBatch;
    }

    /// <summary>
    /// Draws all layers in the default <see cref="TiledMap"/>. This is the map
    /// supplied on creation, or supplied by any extending classes.
    /// </summary>
    public void Render()
    {
        Batch.EnableBlending();

        BeginRender();

        foreach ( MapLayer layer in TiledMap.Layers )
        {
            RenderMapLayer( layer );
        }

        EndRender();
    }

    /// <summary>
    /// Renders the specified layers.
    /// </summary>
    /// <param name="layers"> The array of layers to draw. </param>
    public void Render( int[] layers )
    {
        Batch.EnableBlending();

        BeginRender();

        foreach ( int layer in layers )
        {
            RenderMapLayer( TiledMap.Layers.Get( layer ) );
        }

        EndRender();
    }

    /// <summary>
    /// Updates the batch's projection matrix using the provided <see cref="OrthographicCamera"/>
    /// and recalculates the view bounds for rendering the tiled map.
    /// </summary>
    /// <param name="camera">
    /// The orthographic camera used to determine the projection matrix and view bounds for rendering.
    /// </param>
    public void SetView( OrthographicCamera camera )
    {
        Batch.SetProjectionMatrix( camera.Combined );

        float width  = camera.ViewportWidth * camera.Zoom;
        float height = camera.ViewportHeight * camera.Zoom;

        float w = ( width * Math.Abs( camera.Up.Y ) ) + ( height * Math.Abs( camera.Up.X ) );
        float h = ( height * Math.Abs( camera.Up.Y ) ) + ( width * Math.Abs( camera.Up.X ) );

        ViewBounds.Set( camera.Position.X - ( w / 2 ), camera.Position.Y - ( h / 2 ), w, h );
    }

    /// <summary>
    /// Sets the view for the renderer using the specified projection matrix and viewport dimensions.
    /// </summary>
    /// <param name="projection">The projection matrix to be used for rendering.</param>
    /// <param name="x">The x-coordinate of the viewport's bottom-left corner in world units.</param>
    /// <param name="y">The y-coordinate of the viewport's bottom-left corner in world units.</param>
    /// <param name="width">The width of the viewport in world units.</param>
    /// <param name="height">The height of the viewport in world units.</param>
    public void SetView( Matrix4 projection, float x, float y, float width, float height )
    {
        Batch.SetProjectionMatrix( projection );
        ViewBounds.Set( x, y, width, height );
    }

    /// <summary>
    /// Renders the specified <see cref="MapLayer"/>.
    /// </summary>
    protected void RenderMapLayer( MapLayer layer )
    {
        if ( !layer.Visible )
        {
            return;
        }

        switch ( layer )
        {
            case MapGroupLayer groupLayer:
            {
                RenderGroupLayerChildren( groupLayer );

                break;
            }

            case TiledMapTileLayer tileLayer:
            {
                RenderTileLayer( tileLayer );

                break;
            }

            case TiledMapImageLayer imageLayer:
            {
                RenderImageLayer( imageLayer );

                break;
            }

            default:
            {
                RenderObjects( layer );

                break;
            }
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="layer"></param>
    public void RenderObjects( MapLayer layer )
    {
        foreach ( MapObject obj in layer.Objects )
        {
            RenderObject( obj );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="obj"></param>
    public void RenderObject( MapObject obj )
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="layer"></param>
    public virtual void RenderTileLayer( TiledMapTileLayer layer )
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="layer"></param>
    public void RenderImageLayer( TiledMapImageLayer layer )
    {
        float color = Color.ToFloatBitsAbgr( Batch.Color.R,
                                             Batch.Color.G,
                                             Batch.Color.B,
                                             Batch.Color.A * layer.Opacity );

        TextureRegion? region = layer.Region;

        if ( region == null )
        {
            return;
        }

        float x  = layer.X;
        float y  = layer.Y;
        float x1 = x * UnitScale;
        float y1 = y * UnitScale;
        float x2 = x1 + ( region.GetRegionWidth() * UnitScale );
        float y2 = y1 + ( region.GetRegionHeight() * UnitScale );

        ImageBounds.Set( x1, y1, x2 - x1, y2 - y1 );

        if ( ViewBounds.Contains( ImageBounds ) || ViewBounds.Overlaps( ImageBounds ) )
        {
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

            if ( region.Texture != null )
            {
                Batch.Draw( region.Texture, Vertices, 0, NUM_VERTICES );
            }
        }
    }

    /// <summary>
    /// Rendes the child layers of a <see cref="MapGroupLayer"/>.
    /// </summary>
    private void RenderGroupLayerChildren( MapGroupLayer groupLayer )
    {
        MapLayers childLayers = groupLayer.Layers;

        for ( var i = 0; i < childLayers.LayersCount; i++ )
        {
            MapLayer childLayer = childLayers.Get( i );

            if ( !childLayer.Visible )
            {
                continue;
            }

            RenderMapLayer( childLayer );
        }
    }

    /// <summary>
    /// Called before the rendering of all layers starts.
    /// </summary>
    protected void BeginRender()
    {
        AnimatedTiledMapTile.UpdateAnimationBaseTime();
        Batch.Begin();
    }

    /// <summary>
    /// Called after the rendering of all layers ended.
    /// </summary>
    protected void EndRender()
    {
        Batch.End();
    }
}

// ============================================================================
// ============================================================================