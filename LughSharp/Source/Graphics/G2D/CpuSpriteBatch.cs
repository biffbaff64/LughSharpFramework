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

using LughSharp.Source.Graphics.Images;
using LughSharp.Source.Graphics.Shaders;

namespace LughSharp.Source.Graphics.G2D;

/// <summary>
/// CpuSpriteBatch behaves like SpriteBatch, except it doesn't flush automatically
/// whenever the transformation matrix changes. Instead, the vertices get adjusted
/// on subsequent draws to match the running batch. This can improve performance
/// through longer batches, for example when drawing Groups with transform enabled.
/// </summary>
/// <seealso cref="SpriteBatch.RenderCalls"/>
/// <seealso cref="Scene2D.Group.Transform"/>
[PublicAPI]
public class CpuSpriteBatch : SpriteBatch
{
    private readonly Affine2 _adjustAffine  = new();
    private readonly Affine2 _tmpAffine     = new();
    private readonly Matrix4 _virtualMatrix = new();

    private bool _adjustNeeded;
    private bool _haveIdentityRealMatrix = true;

    // ========================================================================

    /// <summary>
    /// Constructs a CpuSpriteBatch with a size of 1000 and the default shader.
    /// </summary>
    /// <seealso cref="SpriteBatch"/>
    public CpuSpriteBatch() : this( 1000 )
    {
    }

    /// <summary>
    /// Constructs a CpuSpriteBatch with a custom shader.
    /// </summary>
    /// <seealso cref="SpriteBatch"/>
    public CpuSpriteBatch( int size, ShaderProgram? defaultShader = null )
        : base( size, defaultShader )
    {
    }

    /// <summary>
    /// <para>
    /// Flushes the batch and realigns the real matrix on the GPU. Subsequent draws won't
    /// need adjustment and will be slightly faster as long as the transform matrix is not
    /// changed by <see cref="SetTransformMatrix(Matrix4)"/>.
    /// </para>
    /// <para>
    /// Note: The real transform matrix <em>must</em> be invertible. If a singular matrix
    /// is detected, a <see cref="RuntimeException"/> will be thrown.
    /// </para>
    /// </summary>
    public virtual void FlushAndSyncTransformMatrix()
    {
        Flush();

        if ( _adjustNeeded )
        {
            // vertices flushed, safe now to replace matrix
            _haveIdentityRealMatrix = CheckIdt( _virtualMatrix );

            if ( !_haveIdentityRealMatrix && ( _virtualMatrix.Determinant() == 0 ) )
            {
                throw new RuntimeException( "Transform matrix is singular, can't sync" );
            }

            _adjustNeeded = false;

            base.SetTransformMatrix( _virtualMatrix );
        }
    }

    /// <summary>
    /// Retrieves the current transformation matrix applied to the sprite batch.
    /// If an adjustment is required, returns the virtual transformation matrix; otherwise,
    /// returns the default transformation matrix.
    /// </summary>
    /// <returns>
    /// The current transformation matrix of type <see cref="Matrix4"/>.
    /// </returns>
    public virtual Matrix4 GetTransformMatrix()
    {
        return _adjustNeeded ? _virtualMatrix : TransformMatrix;
    }

    /// <summary>
    /// Sets the transform matrix to be used by this Batch. Even if this is called inside
    /// a <see cref="SpriteBatch.Begin"/>/<see cref="SpriteBatch.End"/> block, the current
    /// batch is <em>not</em> flushed to the GPU. Instead, for every subsequent draw() the
    /// vertices will be transformed on the CPU to match the original batch matrix. This
    /// adjustment must be performed until the matrices are realigned by restoring the
    /// original matrix, or by calling <see cref="FlushAndSyncTransformMatrix()"/>.
    /// </summary>
    public override void SetTransformMatrix( Matrix4 transform )
    {
        if ( CheckEqual( TransformMatrix, transform ) )
        {
            _adjustNeeded = false;
        }
        else
        {
            if ( IsDrawing )
            {
                _virtualMatrix.SetAsAffine( transform );
                _adjustNeeded = true;

                // adjust = inverse(real) x virtual
                // real x adjust x vertex = virtual x vertex

                if ( _haveIdentityRealMatrix )
                {
                    _adjustAffine.SetFrom( transform );
                }
                else
                {
                    _tmpAffine.SetFrom( transform );
                    _adjustAffine.SetFrom( TransformMatrix ).Invert().Mul( _tmpAffine );
                }
            }
            else
            {
                TransformMatrix.SetAsAffine( transform );
                _haveIdentityRealMatrix = CheckIdt( TransformMatrix );
            }
        }
    }

    /// <summary>
    /// Sets the transform matrix to be used by this Batch. Even if this is calle
    /// inside a <see cref="SpriteBatch.Begin"/>/<see cref="SpriteBatch.End"/> block,
    /// the current batch is <em>not</em> flushed to the GPU. Instead, for every
    /// subsequent draw() the vertices will be transformed on the CPU to match the
    /// original batch matrix.
    /// <para>
    /// This adjustment must be performed until the matrices are realigned by restoring
    /// the original matrix, or by calling <see cref="FlushAndSyncTransformMatrix()"/>
    /// or <see cref="SpriteBatch.End"/>.
    /// </para>
    /// </summary>
    /// <param name="transform">
    /// A Matrix4 representing the new transformation to be applied.
    /// </param>
    public virtual void SetTransformMatrix( Affine2 transform )
    {
        if ( CheckEqual( TransformMatrix, transform ) )
        {
            _adjustNeeded = false;
        }
        else
        {
            _virtualMatrix.SetAsAffine( transform );

            if ( IsDrawing )
            {
                _adjustNeeded = true;

                // adjust = inverse(real) x virtual
                // real x adjust x vertex = virtual x vertex

                if ( _haveIdentityRealMatrix )
                {
                    _adjustAffine.SetFrom( transform );
                }
                else
                {
                    _adjustAffine.SetFrom( TransformMatrix ).Invert().Mul( transform );
                }
            }
            else
            {
                TransformMatrix.SetAsAffine( transform );
                _haveIdentityRealMatrix = CheckIdt( TransformMatrix );
            }
        }
    }

    /// <summary>
    /// Draws a textured region with specified transformations such as position, scale,
    /// rotation, and flipping options.
    /// </summary>
    /// <param name="texture">The texture to be drawn.</param>
    /// <param name="region">The region where the texture will be drawn on the target.</param>
    /// <param name="origin">
    /// The origin point of the region for transformations like rotation and scaling.
    /// </param>
    /// <param name="scale">The scaling factor for the texture in the X and Y axes.</param>
    /// <param name="rotation">The rotation angle of the texture in radians.</param>
    /// <param name="src">The source rectangle of the texture to be drawn.</param>
    /// <param name="flipX">Indicates whether the texture should be flipped horizontally.</param>
    /// <param name="flipY">Indicates whether the texture should be flipped vertically.</param>
    public override void Draw( Texture2D? texture,
                               GRect region,
                               Point2D origin,
                               Point2D scale,
                               float rotation,
                               GRect src,
                               bool flipX,
                               bool flipY )
    {
        Guard.Against.Null( texture );

        if ( !_adjustNeeded )
        {
            base.Draw( texture, region, origin, scale, rotation, src, flipX, flipY );
        }
        else
        {
            DrawAdjusted( texture,       // The texture
                          region.X,      // X coordinate in pixels
                          region.Y,      // Y coordinate in pixels
                          region.Width,  // X Origin in pixels
                          region.Height, // Y Origin in pixels
                          origin.X,      // Width of Texture in pixels
                          origin.Y,      // Height of Texture in pixels
                          scale.X,       // Scale X
                          scale.Y,       // Scale Y
                          rotation,      // Rotation in radians
                          src.X,         // Source X coordinate in pixels
                          src.Y,         // Source Y coordinate in pixels
                          src.Width,     // Source Width in pixels
                          src.Height,    // Source Height in pixels
                          flipX,         // Flip X
                          flipY );       // Flip Y
        }
    }

    /// <summary>
    /// Draws a specified texture within a defined region, with optional flipping along both axes.
    /// </summary>
    /// <param name="texture">The texture to be drawn.</param>
    /// <param name="region">The destination rectangle on the target surface.</param>
    /// <param name="src">The source rectangle in the texture to be drawn.</param>
    /// <param name="flipX">Indicates whether to flip the texture horizontally.</param>
    /// <param name="flipY">Indicates whether to flip the texture vertically.</param>
    public override void Draw( Texture2D? texture,
                               GRect region,
                               GRect src,
                               bool flipX = false,
                               bool flipY = false )
    {
        Guard.Against.Null( texture );

        if ( !_adjustNeeded )
        {
            base.Draw( texture, region, src, flipX, flipY );
        }
        else
        {
            DrawAdjusted( texture,       // The texture
                          region.X,      // X coordinate in pixels
                          region.Y,      // Y coordinate in pixels
                          region.Width,  // X Origin in pixels
                          region.Height, // Y Origin in pixels
                          0,             // Width of Texture in pixels
                          0,             // Height of Texture in pixels
                          1,             // Scale X
                          1,             // Scale Y
                          0,             // Rotation in radians
                          src.X,         // Source X coordinate in pixels
                          src.Y,         // Source Y coordinate in pixels
                          src.Width,     // Source Width in pixels
                          src.Height,    // Source Height in pixels
                          flipX,         // Flip X
                          flipY );       // Flip Y
        }
    }

    /// <summary>
    /// Draws a specified texture at the given position using the defined source rectangle.
    /// </summary>
    /// <param name="texture">The texture to be drawn.</param>
    /// <param name="x">The x-coordinate where the texture should be drawn.</param>
    /// <param name="y">The y-coordinate where the texture should be drawn.</param>
    /// <param name="src">The source rectangle portion of the texture to be drawn.</param>
    public override void Draw( Texture2D? texture, float x, float y, GRect src )
    {
        Guard.Against.Null( texture );

        if ( !_adjustNeeded )
        {
            base.Draw( texture, x, y, src );
        }
        else
        {
            DrawAdjusted( texture,    // The texture
                          x,          // X coordinate in pixels
                          y,          // Y coordinate in pixels
                          src.Width,  // X Origin in pixels
                          src.Height, // Y Origin in pixels
                          0,          // Width of Texture in pixels
                          0,          // Height of Texture in pixels
                          1,          // Scale X
                          1,          // Scale Y
                          0,          // Rotation in radians
                          src.X,      // Source X coordinate in pixels
                          src.Y,      // Source Y coordinate in pixels
                          src.Width,  // Source Width in pixels
                          src.Height, // Source Height in pixels
                          false,      // Flip X
                          false );    // Flip Y
        }
    }

    /// <summary>
    /// Draws a textured rectangle on the screen using specified texture coordinates.
    /// </summary>
    /// <param name="texture">The texture to use for rendering.</param>
    /// <param name="region">The rectangular region where the texture will be drawn.</param>
    /// <param name="u">The U coordinate of the texture's top-left corner.</param>
    /// <param name="v">The V coordinate of the texture's top-left corner.</param>
    /// <param name="u2">The U coordinate of the texture's bottom-right corner.</param>
    /// <param name="v2">The V coordinate of the texture's bottom-right corner.</param>
    public override void Draw( Texture2D? texture,
                               GRect region,
                               float u,
                               float v,
                               float u2,
                               float v2 )
    {
        Guard.Against.Null( texture );

        if ( !_adjustNeeded )
        {
            base.Draw( texture, region, u, v, u2, v2 );
        }
        else
        {
            DrawAdjustedUV( texture,       // The texture
                            region.X,      // X coordinate in pixels
                            region.Y,      // Y coordinate in pixels
                            region.Width,  // X Origin in pixels
                            region.Height, // Y Origin in pixels
                            0,             // Width of Texture in pixels
                            0,             // Height of Texture in pixels
                            1,             // Scale X
                            1,             // Scale Y
                            0,             // Rotation in radians
                            u,             // Source X coordinate in pixels
                            v,             // Source Y coordinate in pixels
                            u2,            // Source Width in pixels
                            v2,            // Source Height in pixels
                            false,         // Flip X
                            false );       // Flip Y
        }
    }

    /// <summary>
    /// Draw the given <see cref="Texture2D"/> at the given X and Y coordinates.
    /// </summary>
    /// <param name="texture"> The texture. </param>
    /// <param name="x"> X coordinate in pixels. </param>
    /// <param name="y"> Y coordinate in pixels. </param>
    public override void Draw( Texture2D? texture, float x, float y )
    {
        Guard.Against.Null( texture );

        if ( !_adjustNeeded )
        {
            base.Draw( texture, x, y );
        }
        else
        {
            DrawAdjusted( texture,        // The texture
                          x,              // X coordinate in pixels
                          y,              // Y coordinate in pixels
                          0,              // X Origin in pixels
                          0,              // Y Origin in pixels
                          texture.Width,  // Width of Texture in pixels
                          texture.Height, // Height of Texture in pixels
                          1,              // Scale X
                          1,              // Scale Y
                          0,              // Rotation in radians
                          0,              // Source X coordinate in pixels
                          1,              // Source Y coordinate in pixels
                          1,              // Source Width in pixels
                          0,              // Source Height in pixels
                          false,          // Flip X
                          false );        // Flip Y
        }
    }

    /// <summary>
    /// Draw the supplied Texture at the given coordinates. The texture will be
    /// of the specified width and height.
    /// </summary>
    /// <param name="texture"> The texture. </param>
    /// <param name="posX"> X coordinaste in pixels. </param>
    /// <param name="posY"> Y coordinate in pixels. </param>
    /// <param name="width"> Width of Texture in pixels. </param>
    /// <param name="height"> Height of Texture in pixerls. </param>
    public override void Draw( Texture2D? texture, float posX, float posY, float width, float height )
    {
        Guard.Against.Null( texture );

        if ( !_adjustNeeded )
        {
            base.Draw( texture, posX, posY, width, height );
        }
        else
        {
            DrawAdjusted( texture, // The texture
                          posX,    // X coordinate in pixels
                          posY,    // Y coordinate in pixels
                          0,       // X Origin in pixels
                          0,       // Y Origin in pixels
                          width,   // Width of Texture in pixels
                          height,  // Height of Texture in pixels
                          1,       // Scale X
                          1,       // Scale Y
                          0,       // Rotation in radians
                          0,       // Source X coordinate in pixels
                          1,       // Source Y coordinate in pixels
                          1,       // Source Width in pixels
                          0,       // Source Height in pixels
                          false,   // Flip X
                          false ); // Flip Y
        }
    }

    /// <summary>
    /// Draws the specified texture region at the given position.
    /// </summary>
    /// <param name="region">The texture region to be drawn. Can be null.</param>
    /// <param name="x">The x-coordinate of the position to draw the texture.</param>
    /// <param name="y">The y-coordinate of the position to draw the texture.</param>
    public override void Draw( TextureRegion? region, float x, float y )
    {
        Guard.Against.Null( region );

        if ( !_adjustNeeded )
        {
            base.Draw( region, x, y );
        }
        else
        {
            DrawAdjusted( region,                   // The texture
                          x,                        // X coordinate in pixels
                          y,                        // Y coordinate in pixels
                          0,                        // X Origin in pixels
                          0,                        // Y Origin in pixels
                          region.GetRegionWidth(),  // Width of Texture in pixels
                          region.GetRegionHeight(), // Height of Texture in pixels
                          1,                        // Scale X
                          1,                        // Scale Y
                          0 );                      // Rotation in radians
        }
    }

    /// <summary>
    /// Draws a texture region onto the batch with specified position and dimensions.
    /// </summary>
    /// <param name="region">
    /// The texture region to be drawn, which includes the texture and UV coordinates.
    /// </param>
    /// <param name="x">
    /// The X-coordinate of the bottom-left corner where the texture will be drawn.
    /// </param>
    /// <param name="y">
    /// The Y-coordinate of the bottom-left corner where the texture will be drawn.
    /// </param>
    /// <param name="width">The width of the texture region to be drawn.</param>
    /// <param name="height">The height of the texture region to be drawn.</param>
    public override void Draw( TextureRegion? region,
                               float x,
                               float y,
                               float width,
                               float height )
    {
        Guard.Against.Null( region );

        if ( !_adjustNeeded )
        {
            base.Draw( region, x, y, width, height );
        }
        else
        {
            DrawAdjusted( region, // The texture region to be drawn
                          x,      // The X-coordinate in pixels
                          y,      // The Y-coordinate in pixels
                          0,      // The X origin of the texture region
                          0,      // The Y origin of the texture region
                          width,  // The width of the texture region to be drawn
                          height, // The height of the texture region to be drawn
                          1,      // The scale X
                          1,      // The scale Y
                          0 );    // The rotation in radians
        }
    }

    /// <summary>
    /// Draws a texture region onto a specified region with transformations such as origin offset,
    /// scaling, and rotation.
    /// </summary>
    /// <param name="textureRegion">The texture region to be drawn.</param>
    /// <param name="region">The rectangular region where the texture will be drawn.</param>
    /// <param name="origin">The origin point for transformations such as scaling and rotation.</param>
    /// <param name="scale">The scale factor to be applied to the texture region.</param>
    /// <param name="rotation">The rotation angle in radians to be applied to the texture region.</param>
    public override void Draw( TextureRegion? textureRegion,
                               GRect region,
                               Point2D origin,
                               Point2D scale,
                               float rotation )
    {
        Guard.Against.Null( textureRegion );

        if ( !_adjustNeeded )
        {
            base.Draw( textureRegion, region, origin, scale, rotation );
        }
        else
        {
            DrawAdjusted( textureRegion, // The texture region to be drawn  
                          region.X,      // The X-coordinate in pixels
                          region.Y,      // The Y-coordinate in pixels
                          origin.X,      // The X origin of the texture region
                          origin.Y,      // The Y origin of the texture region
                          region.Width,  // The width of the texture region to be drawn
                          region.Height, // The height of the texture region to be drawn
                          scale.X,       // The scale X
                          scale.Y,       // The scale Y
                          rotation );    // The rotation in radians
        }
    }

    /// <summary>
    /// Draws a texture region onto the sprite batch with specified parameters for position,
    /// scale, rotation, and orientation.
    /// </summary>
    /// <param name="textureRegion">The texture region to be drawn.</param>
    /// <param name="region">The rectangle defining where to draw the texture within the batch.</param>
    /// <param name="origin">The origin point for transformations such as scaling and rotation.</param>
    /// <param name="scale">The scale factor to apply to the texture region.</param>
    /// <param name="rotation">The angle of rotation, in degrees.</param>
    /// <param name="clockwise">Indicates whether the rotation is applied in a clockwise direction.</param>
    public override void Draw( TextureRegion? textureRegion,
                               GRect region,
                               Point2D origin,
                               Point2D scale,
                               float rotation,
                               bool clockwise )
    {
        Guard.Against.Null( textureRegion );

        if ( !_adjustNeeded )
        {
            base.Draw( textureRegion, region, origin, scale, rotation, clockwise );
        }
        else
        {
            DrawAdjusted( textureRegion, // The texture region to be drawn
                          region.X,      // The X-coordinate in pixels
                          region.Y,      // The Y-coordinate in pixels
                          origin.X,      // The X origin of the texture region
                          origin.Y,      // The Y origin of the texture region
                          region.Width,  // The width of the texture region to be drawn
                          region.Height, // The height of the texture region to be drawn
                          scale.X,       // The scale X
                          scale.Y,       // The scale Y
                          rotation,      // The rotation in radians
                          clockwise );   // Whether rotation is clockwise or not
        }
    }

    /// <summary>
    /// Renders a set of sprite vertices using the specified texture.
    /// </summary>
    /// <param name="texture">The texture to be used for rendering. Can be null if not needed.</param>
    /// <param name="spriteVertices">An array of vertex data describing the sprites to be rendered.</param>
    /// <param name="offset">The starting index in the vertex array from which to begin processing.</param>
    /// <param name="count">The number of vertices to process starting from the specified offset.</param>
    public override void Draw( Texture2D? texture, float[] spriteVertices, int offset, int count )
    {
        Guard.Against.Null( texture );

        if ( ( count % Sprite2D.SpriteSize ) != 0 )
        {
            throw new RuntimeException( "invalid vertex count" );
        }

        if ( !_adjustNeeded )
        {
            base.Draw( texture, spriteVertices, offset, count );
        }
        else
        {
            DrawAdjusted( texture, spriteVertices, offset, count );
        }
    }

    /// <summary>
    /// Draws a texture using a specified region, dimensions, and transformation parameters.
    /// </summary>
    /// <param name="region">The specific texture region to be drawn.</param>
    /// <param name="width">The width of the drawn texture.</param>
    /// <param name="height">The height of the drawn texture.</param>
    /// <param name="transform">The transformation to be applied to the texture.</param>
    public override void Draw( TextureRegion? region, float width, float height, Affine2 transform )
    {
        Guard.Against.Null( region );

        if ( !_adjustNeeded )
        {
            base.Draw( region, width, height, transform );
        }
        else
        {
            DrawAdjusted( region, width, height, transform );
        }
    }

    /// <summary>
    /// Draws a rectangular region of the specified texture using CPU-adjusted vertex
    /// positions for the current virtual transform matrix.
    /// </summary>
    /// <param name="region">The texture region to be drawn, representing a portion of a texture.</param>
    /// <param name="x">The X-coordinate of the position where the texture should be drawn in pixels.</param>
    /// <param name="y">The Y-coordinate of the position where the texture should be drawn in pixels.</param>
    /// <param name="originX">
    /// The X origin of the texture region in pixels, used for transformations like scaling or rotation.
    /// </param>
    /// <param name="originY">
    /// The Y origin of the texture region in pixels, used for transformations like scaling or rotation.
    /// </param>
    /// <param name="width">The width of the texture region to be drawn in pixels.</param>
    /// <param name="height">The height of the texture region to be drawn in pixels.</param>
    /// <param name="scaleX">The scaling factor along the X-axis for the drawn texture.</param>
    /// <param name="scaleY">The scaling factor along the Y-axis for the drawn texture.</param>
    /// <param name="rotation">The rotation angle of the texture in radians, applied around the origin.</param>
    private void DrawAdjusted( TextureRegion? region,
                               float x,
                               float y,
                               float originX,
                               float originY,
                               float width,
                               float height,
                               float scaleX,
                               float scaleY,
                               float rotation )
    {
        Guard.Against.Null( region );

        // v must be flipped
        DrawAdjustedUV( region.Texture,
                        x,
                        y,
                        originX,
                        originY,
                        width,
                        height,
                        scaleX,
                        scaleY,
                        rotation,
                        region.U,
                        region.V2,
                        region.U2,
                        region.V,
                        false,
                        false );
    }

    /// <summary>
    /// Draws a rectangular region of the specified texture using CPU-adjusted vertex
    /// positions for the current virtual transform matrix.
    /// </summary>
    /// <param name="texture">The texture containing the source region to draw.</param>
    /// <param name="x">The X-coordinate of the destination rectangle in pixels.</param>
    /// <param name="y">The Y-coordinate of the destination rectangle in pixels.</param>
    /// <param name="originX">The X-coordinate of the origin used for scaling and rotation.</param>
    /// <param name="originY">The Y-coordinate of the origin used for scaling and rotation.</param>
    /// <param name="width">The destination width of the drawn texture region in pixels.</param>
    /// <param name="height">The destination height of the drawn texture region in pixels.</param>
    /// <param name="scaleX">The horizontal scale factor applied around the origin.</param>
    /// <param name="scaleY">The vertical scale factor applied around the origin.</param>
    /// <param name="rotation">The rotation angle, in degrees, applied around the origin.</param>
    /// <param name="srcX">The X-coordinate of the source region within the texture, in pixels.</param>
    /// <param name="srcY">The Y-coordinate of the source region within the texture, in pixels.</param>
    /// <param name="srcWidth">The width of the source region within the texture, in pixels.</param>
    /// <param name="srcHeight">The height of the source region within the texture, in pixels.</param>
    /// <param name="flipX">Whether to flip the source region horizontally.</param>
    /// <param name="flipY">Whether to flip the source region vertically.</param>
    private void DrawAdjusted( Texture2D? texture,
                               float x,
                               float y,
                               float originX,
                               float originY,
                               float width,
                               float height,
                               float scaleX,
                               float scaleY,
                               float rotation,
                               int srcX,
                               int srcY,
                               int srcWidth,
                               int srcHeight,
                               bool flipX,
                               bool flipY )
    {
        Guard.Against.Null( texture );

        float invWidth  = 1.0f / texture.Width;
        float invHeight = 1.0f / texture.Height;

        float u  = srcX * invWidth;
        float v  = ( srcY + srcHeight ) * invHeight;
        float u2 = ( srcX + srcWidth ) * invWidth;
        float v2 = srcY * invHeight;

        DrawAdjustedUV( texture,
                        x,
                        y,
                        originX,
                        originY,
                        width,
                        height,
                        scaleX,
                        scaleY,
                        rotation,
                        u,
                        v,
                        u2,
                        v2,
                        flipX,
                        flipY );
    }

    /// <summary>
    /// Draws a texture with adjusted UV coordinates, allowing for transformations such as
    /// scaling, rotation, and flipping, as well as position and origin adjustments.
    /// </summary>
    /// <param name="texture">The texture to be drawn. Can be null.</param>
    /// <param name="x">The x-coordinate of the texture's position.</param>
    /// <param name="y">The y-coordinate of the texture's position.</param>
    /// <param name="originX">The x-coordinate of the texture's origin point for transformations.</param>
    /// <param name="originY">The y-coordinate of the texture's origin point for transformations.</param>
    /// <param name="width">The width of the texture region to be drawn.</param>
    /// <param name="height">The height of the texture region to be drawn.</param>
    /// <param name="scaleX">The horizontal scaling factor.</param>
    /// <param name="scaleY">The vertical scaling factor.</param>
    /// <param name="rotation">The angle of rotation in radians around the origin.</param>
    /// <param name="u">The U-coordinate of the texture's top-left corner in UV space.</param>
    /// <param name="v">The V-coordinate of the texture's top-left corner in UV space.</param>
    /// <param name="u2">The U-coordinate of the texture's bottom-right corner in UV space.</param>
    /// <param name="v2">The V-coordinate of the texture's bottom-right corner in UV space.</param>
    /// <param name="flipX">Whether to flip the texture horizontally.</param>
    /// <param name="flipY">Whether to flip the texture vertically.</param>
    private void DrawAdjustedUV( Texture2D? texture,
                                 float x,
                                 float y,
                                 float originX,
                                 float originY,
                                 float width,
                                 float height,
                                 float scaleX,
                                 float scaleY,
                                 float rotation,
                                 float u,
                                 float v,
                                 float u2,
                                 float v2,
                                 bool flipX,
                                 bool flipY )
    {
        if ( !IsDrawing )
        {
            throw new InvalidOperationException( "CpuSpriteBatch.begin must be called before draw." );
        }

        Guard.Against.Null( texture );
        Guard.Against.Null( LastTexture );

        if ( texture != LastTexture )
        {
            SwitchTexture( texture );
        }
        else if ( Idx == Vertices.Length )
        {
            Flush();
        }

        // bottom left and top right corner points relative to origin
        float worldOriginX = x + originX;
        float worldOriginY = y + originY;
        float fx           = -originX;
        float fy           = -originY;
        float fx2          = width - originX;
        float fy2          = height - originY;

        // scale
        if ( scaleX is not 1 || scaleY is not 1 )
        {
            fx  *= scaleX;
            fy  *= scaleY;
            fx2 *= scaleX;
            fy2 *= scaleY;
        }

        // construct corner points, start from top left and go counter clockwise
        float p1X = fx;
        float p1Y = fy;
        float p2X = fx;
        float p2Y = fy2;
        float p3X = fx2;
        float p3Y = fy2;
        float p4X = fx2;
        float p4Y = fy;

        float x1;
        float y1;
        float x2;
        float y2;
        float x3;
        float y3;
        float x4;
        float y4;

        // rotate
        if ( rotation != 0 )
        {
            float cos = MathUtils.CosDeg( rotation );
            float sin = MathUtils.SinDeg( rotation );

            x1 = ( cos * p1X ) - ( sin * p1Y );
            y1 = ( sin * p1X ) + ( cos * p1Y );

            x2 = ( cos * p2X ) - ( sin * p2Y );
            y2 = ( sin * p2X ) + ( cos * p2Y );

            x3 = ( cos * p3X ) - ( sin * p3Y );
            y3 = ( sin * p3X ) + ( cos * p3Y );

            x4 = x1 + ( x3 - x2 );
            y4 = y3 - ( y2 - y1 );
        }
        else
        {
            x1 = p1X;
            y1 = p1Y;

            x2 = p2X;
            y2 = p2Y;

            x3 = p3X;
            y3 = p3Y;

            x4 = p4X;
            y4 = p4Y;
        }

        x1 += worldOriginX;
        y1 += worldOriginY;
        x2 += worldOriginX;
        y2 += worldOriginY;
        x3 += worldOriginX;
        y3 += worldOriginY;
        x4 += worldOriginX;
        y4 += worldOriginY;

        if ( flipX )
        {
            ( u, u2 ) = ( u2, u );
        }

        if ( flipY )
        {
            ( v, v2 ) = ( v2, v );
        }

        Vertices[ Idx + 0 ] = ( _adjustAffine.M00 * x1 ) + ( _adjustAffine.M01 * y1 ) + _adjustAffine.M02;
        Vertices[ Idx + 1 ] = ( _adjustAffine.M10 * x1 ) + ( _adjustAffine.M11 * y1 ) + _adjustAffine.M12;
        Vertices[ Idx + 2 ] = ColorPackedABGR;
        Vertices[ Idx + 3 ] = u;
        Vertices[ Idx + 4 ] = v;

        Vertices[ Idx + 5 ] = ( _adjustAffine.M00 * x2 ) + ( _adjustAffine.M01 * y2 ) + _adjustAffine.M02;
        Vertices[ Idx + 6 ] = ( _adjustAffine.M10 * x2 ) + ( _adjustAffine.M11 * y2 ) + _adjustAffine.M12;
        Vertices[ Idx + 7 ] = ColorPackedABGR;
        Vertices[ Idx + 8 ] = u;
        Vertices[ Idx + 9 ] = v2;

        Vertices[ Idx + 10 ] = ( _adjustAffine.M00 * x3 ) + ( _adjustAffine.M01 * y3 ) + _adjustAffine.M02;
        Vertices[ Idx + 11 ] = ( _adjustAffine.M10 * x3 ) + ( _adjustAffine.M11 * y3 ) + _adjustAffine.M12;
        Vertices[ Idx + 12 ] = ColorPackedABGR;
        Vertices[ Idx + 13 ] = u2;
        Vertices[ Idx + 14 ] = v2;

        Vertices[ Idx + 15 ] = ( _adjustAffine.M00 * x4 ) + ( _adjustAffine.M01 * y4 ) + _adjustAffine.M02;
        Vertices[ Idx + 16 ] = ( _adjustAffine.M10 * x4 ) + ( _adjustAffine.M11 * y4 ) + _adjustAffine.M12;
        Vertices[ Idx + 17 ] = ColorPackedABGR;
        Vertices[ Idx + 18 ] = u2;
        Vertices[ Idx + 19 ] = v;

        Idx += Sprite2D.SpriteSize;
    }

    /// <summary>
    /// Renders a texture region on the screen, with transformations applied including translation,
    /// scaling, rotation, and optional affine adjustments.
    /// </summary>
    /// <param name="region">
    /// The texture region to draw. The texture must match the previously used texture or a new
    /// batch must be started.
    /// </param>
    /// <param name="x">The x-coordinate of the position where the texture region should be drawn.</param>
    /// <param name="y">The y-coordinate of the position where the texture region should be drawn.</param>
    /// <param name="originX">
    /// The horizontal origin point relative to the texture region, used for scaling and rotation.
    /// </param>
    /// <param name="originY">
    /// The vertical origin point relative to the texture region, used for scaling and rotation.
    /// </param>
    /// <param name="width">The width of the texture region to draw, after scaling.</param>
    /// <param name="height">The height of the texture region to draw, after scaling.</param>
    /// <param name="scaleX">The scaling factor in the horizontal direction.</param>
    /// <param name="scaleY">The scaling factor in the vertical direction.</param>
    /// <param name="rotation">The rotation of the texture region, in degrees.</param>
    /// <param name="clockwise">
    /// Indicates whether the texture region's UV coordinates should be rotated clockwise or
    /// counterclockwise.
    /// </param>
    private void DrawAdjusted( TextureRegion? region,
                               float x,
                               float y,
                               float originX,
                               float originY,
                               float width,
                               float height,
                               float scaleX,
                               float scaleY,
                               float rotation,
                               bool clockwise )
    {
        if ( !IsDrawing )
        {
            throw new RuntimeException( "CpuSpriteBatch.begin must be called before draw." );
        }

        Guard.Against.Null( region );

        if ( region.Texture != LastTexture )
        {
            SwitchTexture( region.Texture );
        }
        else if ( Idx == Vertices.Length )
        {
            Flush();
        }

        // bottom left and top right corner points relative to origin
        float worldOriginX = x + originX;
        float worldOriginY = y + originY;
        float fx           = -originX;
        float fy           = -originY;
        float fx2          = width - originX;
        float fy2          = height - originY;

        // scale
        if ( scaleX is not 1 || scaleY is not 1 )
        {
            fx  *= scaleX;
            fy  *= scaleY;
            fx2 *= scaleX;
            fy2 *= scaleY;
        }

        // construct corner points, start from top left and go counter clockwise
        float p1X = fx;
        float p1Y = fy;
        float p2X = fx;
        float p2Y = fy2;
        float p3X = fx2;
        float p3Y = fy2;
        float p4X = fx2;
        float p4Y = fy;

        float x1;
        float y1;
        float x2;
        float y2;
        float x3;
        float y3;
        float x4;
        float y4;

        // rotate
        if ( rotation != 0 )
        {
            float cos = MathUtils.CosDeg( rotation );
            float sin = MathUtils.SinDeg( rotation );

            x1 = ( cos * p1X ) - ( sin * p1Y );
            y1 = ( sin * p1X ) + ( cos * p1Y );

            x2 = ( cos * p2X ) - ( sin * p2Y );
            y2 = ( sin * p2X ) + ( cos * p2Y );

            x3 = ( cos * p3X ) - ( sin * p3Y );
            y3 = ( sin * p3X ) + ( cos * p3Y );

            x4 = x1 + ( x3 - x2 );
            y4 = y3 - ( y2 - y1 );
        }
        else
        {
            x1 = p1X;
            y1 = p1Y;

            x2 = p2X;
            y2 = p2Y;

            x3 = p3X;
            y3 = p3Y;

            x4 = p4X;
            y4 = p4Y;
        }

        x1 += worldOriginX;
        y1 += worldOriginY;
        x2 += worldOriginX;
        y2 += worldOriginY;
        x3 += worldOriginX;
        y3 += worldOriginY;
        x4 += worldOriginX;
        y4 += worldOriginY;

        float u1, v1, u2, v2, u3, v3, u4, v4;

        if ( clockwise )
        {
            u1 = region.U2;
            v1 = region.V2;
            u2 = region.U;
            v2 = region.V2;
            u3 = region.U;
            v3 = region.V;
            u4 = region.U2;
            v4 = region.V;
        }
        else
        {
            u1 = region.U;
            v1 = region.V;
            u2 = region.U2;
            v2 = region.V;
            u3 = region.U2;
            v3 = region.V2;
            u4 = region.U;
            v4 = region.V2;
        }

        Vertices[ Idx + 0 ] = ( _adjustAffine.M00 * x1 ) + ( _adjustAffine.M01 * y1 ) + _adjustAffine.M02;
        Vertices[ Idx + 1 ] = ( _adjustAffine.M10 * x1 ) + ( _adjustAffine.M11 * y1 ) + _adjustAffine.M12;
        Vertices[ Idx + 2 ] = ColorPackedABGR;
        Vertices[ Idx + 3 ] = u1;
        Vertices[ Idx + 4 ] = v1;

        Vertices[ Idx + 5 ] = ( _adjustAffine.M00 * x2 ) + ( _adjustAffine.M01 * y2 ) + _adjustAffine.M02;
        Vertices[ Idx + 6 ] = ( _adjustAffine.M10 * x2 ) + ( _adjustAffine.M11 * y2 ) + _adjustAffine.M12;
        Vertices[ Idx + 7 ] = ColorPackedABGR;
        Vertices[ Idx + 8 ] = u2;
        Vertices[ Idx + 9 ] = v2;

        Vertices[ Idx + 10 ] = ( _adjustAffine.M00 * x3 ) + ( _adjustAffine.M01 * y3 ) + _adjustAffine.M02;
        Vertices[ Idx + 11 ] = ( _adjustAffine.M10 * x3 ) + ( _adjustAffine.M11 * y3 ) + _adjustAffine.M12;
        Vertices[ Idx + 12 ] = ColorPackedABGR;
        Vertices[ Idx + 13 ] = u3;
        Vertices[ Idx + 14 ] = v3;

        Vertices[ Idx + 15 ] = ( _adjustAffine.M00 * x4 ) + ( _adjustAffine.M01 * y4 ) + _adjustAffine.M02;
        Vertices[ Idx + 16 ] = ( _adjustAffine.M10 * x4 ) + ( _adjustAffine.M11 * y4 ) + _adjustAffine.M12;
        Vertices[ Idx + 17 ] = ColorPackedABGR;
        Vertices[ Idx + 18 ] = u4;
        Vertices[ Idx + 19 ] = v4;

        Idx += Sprite2D.SpriteSize;
    }

    /// <summary>
    /// Renders a texture region onto the sprite batch with an adjusted affine transformation.
    /// </summary>
    /// <param name="region">The texture region to render. Cannot be null.</param>
    /// <param name="width">The width of the region in world space.</param>
    /// <param name="height">The height of the region in world space.</param>
    /// <param name="transform">The affine transformation applied to the region.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the method is invoked outside of a valid drawing context.
    /// </exception>
    private void DrawAdjusted( TextureRegion? region, float width, float height, Affine2 transform )
    {
        if ( !IsDrawing )
        {
            throw new InvalidOperationException( "CpuSpriteBatch.begin must be called before draw." );
        }

        Guard.Against.Null( region );

        if ( region.Texture != LastTexture )
        {
            SwitchTexture( region.Texture );
        }
        else if ( Idx == Vertices.Length )
        {
            Flush();
        }

        // construct corner points
        float x1 = transform.M02;
        float y1 = transform.M12;
        float x2 = ( transform.M01 * height ) + transform.M02;
        float y2 = ( transform.M11 * height ) + transform.M12;
        float x3 = ( transform.M00 * width ) + ( transform.M01 * height ) + transform.M02;
        float y3 = ( transform.M10 * width ) + ( transform.M11 * height ) + transform.M12;
        float x4 = ( transform.M00 * width ) + transform.M02;
        float y4 = ( transform.M10 * width ) + transform.M12;

        // v must be flipped
        float u  = region.U;
        float v  = region.V2;
        float u2 = region.U2;
        float v2 = region.V;

        Vertices[ Idx + 0 ] = ( _adjustAffine.M00 * x1 ) + ( _adjustAffine.M01 * y1 ) + _adjustAffine.M02;
        Vertices[ Idx + 1 ] = ( _adjustAffine.M10 * x1 ) + ( _adjustAffine.M11 * y1 ) + _adjustAffine.M12;
        Vertices[ Idx + 2 ] = ColorPackedABGR;
        Vertices[ Idx + 3 ] = u;
        Vertices[ Idx + 4 ] = v;

        Vertices[ Idx + 5 ] = ( _adjustAffine.M00 * x2 ) + ( _adjustAffine.M01 * y2 ) + _adjustAffine.M02;
        Vertices[ Idx + 6 ] = ( _adjustAffine.M10 * x2 ) + ( _adjustAffine.M11 * y2 ) + _adjustAffine.M12;
        Vertices[ Idx + 7 ] = ColorPackedABGR;
        Vertices[ Idx + 8 ] = u;
        Vertices[ Idx + 9 ] = v2;

        Vertices[ Idx + 10 ] = ( _adjustAffine.M00 * x3 ) + ( _adjustAffine.M01 * y3 ) + _adjustAffine.M02;
        Vertices[ Idx + 11 ] = ( _adjustAffine.M10 * x3 ) + ( _adjustAffine.M11 * y3 ) + _adjustAffine.M12;
        Vertices[ Idx + 12 ] = ColorPackedABGR;
        Vertices[ Idx + 13 ] = u2;
        Vertices[ Idx + 14 ] = v2;

        Vertices[ Idx + 15 ] = ( _adjustAffine.M00 * x4 ) + ( _adjustAffine.M01 * y4 ) + _adjustAffine.M02;
        Vertices[ Idx + 16 ] = ( _adjustAffine.M10 * x4 ) + ( _adjustAffine.M11 * y4 ) + _adjustAffine.M12;
        Vertices[ Idx + 17 ] = ColorPackedABGR;
        Vertices[ Idx + 18 ] = u2;
        Vertices[ Idx + 19 ] = v;

        Idx += Sprite2D.SpriteSize;
    }

    /// <summary>
    /// Renders a portion of a sprite batch with adjusted input parameters.
    /// </summary>
    /// <param name="texture">The texture to be used for rendering. Must not be null.</param>
    /// <param name="spriteVertices">An array of vertex attributes defining the sprites.</param>
    /// <param name="offset">The starting index in the spriteVertices array from which rendering begins.</param>
    /// <param name="count">The number of vertices to process starting from the offset.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the method is called outside of a valid drawing operation.
    /// </exception>
    private void DrawAdjusted( Texture2D? texture, float[] spriteVertices, int offset, int count )
    {
        if ( !IsDrawing )
        {
            throw new InvalidOperationException( "CpuSpriteBatch.begin must be called before draw." );
        }

        Guard.Against.Null( texture );

        if ( texture != LastTexture )
        {
            SwitchTexture( texture );
        }

        int copyCount = Math.Min( Vertices.Length - Idx, count );

        do
        {
            count -= copyCount;

            while ( copyCount > 0 )
            {
                float x = spriteVertices[ offset ];
                float y = spriteVertices[ offset + 1 ];

                Vertices[ Idx ]     = ( _adjustAffine.M00 * x ) + ( _adjustAffine.M01 * y ) + _adjustAffine.M02; // x
                Vertices[ Idx + 1 ] = ( _adjustAffine.M10 * x ) + ( _adjustAffine.M11 * y ) + _adjustAffine.M12; // y
                Vertices[ Idx + 2 ] = spriteVertices[ offset + 2 ]; // color
                Vertices[ Idx + 3 ] = spriteVertices[ offset + 3 ]; // u
                Vertices[ Idx + 4 ] = spriteVertices[ offset + 4 ]; // v

                Idx       += Sprite2D.VertexSize;
                offset    += Sprite2D.VertexSize;
                copyCount -= Sprite2D.VertexSize;
            }

            if ( count > 0 )
            {
                Flush();
                copyCount = Math.Min( Vertices.Length, count );
            }
        }
        while ( count > 0 );
    }

    /// <summary>
    /// Compares a 2D transformation matrix with a second 2D transformation matrix
    /// to determine if they are equivalent.
    /// </summary>
    /// <param name="a"> The first 2D transformation matrix to compare. </param>
    /// <param name="b"> The second 2D transformation matrix to compare. </param>
    /// <returns> <c>true</c> if the matrices are equivalent; otherwise, <c>false</c>. </returns>
    private static bool CheckEqual( Matrix4 a, Matrix4 b )
    {
        if ( a == b )
        {
            return true;
        }

        // matrices are assumed to be 2D transformations
        return a.Val[ Matrix4.M000 ].Equals( b.Val[ Matrix4.M000 ] )
            && a.Val[ Matrix4.M101 ].Equals( b.Val[ Matrix4.M101 ] )
            && a.Val[ Matrix4.M014 ].Equals( b.Val[ Matrix4.M014 ] )
            && a.Val[ Matrix4.M115 ].Equals( b.Val[ Matrix4.M115 ] )
            && a.Val[ Matrix4.M0312 ].Equals( b.Val[ Matrix4.M0312 ] )
            && a.Val[ Matrix4.M1313 ].Equals( b.Val[ Matrix4.M1313 ] );
    }

    /// <summary>
    /// Compares a 2D transformation matrix with an affine transformation to
    /// determine if they are equivalent.
    /// </summary>
    /// <param name="matrix">The 2D transformation matrix to compare.</param>
    /// <param name="affine">The affine transformation to compare against the matrix.</param>
    /// <returns>
    /// True if the matrix and affine transformation are equivalent; otherwise, false.
    /// </returns>
    private static bool CheckEqual( Matrix4 matrix, Affine2 affine )
    {
        float[] val = matrix.Values;

        // matrix is assumed to be 2D transformation
        return val[ Matrix4.M000 ].Equals( affine.M00 )
            && val[ Matrix4.M101 ].Equals( affine.M10 )
            && val[ Matrix4.M014 ].Equals( affine.M01 )
            && val[ Matrix4.M115 ].Equals( affine.M11 )
            && val[ Matrix4.M0312 ].Equals( affine.M02 )
            && val[ Matrix4.M1313 ].Equals( affine.M12 );
    }

    /// <summary>
    /// Checks if the given 4x4 matrix is an identity matrix for 2D transformations.
    /// </summary>
    /// <param name="matrix">The matrix to be checked.</param>
    /// <returns>
    /// True if the given matrix is the identity matrix for 2D transformations; otherwise, false.
    /// </returns>
    private static bool CheckIdt( Matrix4 matrix )
    {
        float[] val = matrix.Values;

        // matrix is assumed to be 2D transformation
        return val[ Matrix4.M000 ] is 1
            && val[ Matrix4.M101 ] is 0
            && val[ Matrix4.M014 ] is 0
            && val[ Matrix4.M115 ] is 1
            && val[ Matrix4.M0312 ] is 0
            && val[ Matrix4.M1313 ] is 0;
    }
}

// ============================================================================
// ============================================================================