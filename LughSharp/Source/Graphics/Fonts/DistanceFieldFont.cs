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
using LughSharp.Source.Graphics.OpenGL.Enums;
using LughSharp.Source.Graphics.Shaders;

namespace LughSharp.Source.Graphics.Fonts;

/// <summary>
/// Renders bitmap fonts using distance field textures. Initialize the SpriteBatch with the
/// <see cref="CreateDistanceFieldShader()"/> shader.
/// <para>
/// Traditional bitmap fonts work fine if the pixels in the font map 1:1 onto screen pixels.
/// However, they look bad when rotated, and increasingly worse when scaled up. Either you
/// end up seeing individual pixels, or you turn on linear interpolation and end up with a
/// smudgy blur instead.
/// </para>
/// <para>
/// Using a distance field font lets you render text that remains crisp even under rotations
/// and other arbitrary transforms, even blown up to a large magnification, without notable
/// extra run-time cost. 
/// </para>
/// <para>
/// <b>Attention: The batch is flushed before and after each string is rendered.</b>
/// </para>
/// </summary>
[PublicAPI]
public class DistanceFieldFont : BitmapFont
{
    /// <summary>
    /// The distance field smoothing factor for this font. SpriteBatch needs to have this
    /// shader set for rendering distance field fonts.
    /// </summary>
    public float DistanceFieldSmoothing { get; set; }

    // ========================================================================

    /// <summary>
    /// Creates a distance field font with the specified data, page regions, and integer
    /// position flag.
    /// </summary>
    /// <param name="data"> The bitmap font data. </param>
    /// <param name="pageRegions"> The texture regions for each page of the font. </param>
    /// <param name="integer"> Whether to use integer positions for rendering. </param>
    public DistanceFieldFont( BitmapFontData data, List< TextureRegion > pageRegions, bool integer )
        : base( data, pageRegions, integer )
    {
    }

    /// <summary>
    /// Initializes a new instance of the DistanceFieldFont class using the specified font data,
    /// texture region, and integer positioning option.
    /// </summary>
    /// <param name="data">
    /// The font data that defines the glyphs and layout information for the font. Cannot be null.
    /// </param>
    /// <param name="region">
    /// The texture region containing the font's glyph atlas. Used to render the font's characters.
    /// </param>
    /// <param name="integer">true to use integer positioning for glyph placement; otherwise, false.</param>
    public DistanceFieldFont( BitmapFontData data, TextureRegion region, bool integer )
        : base( data, region, integer )
    {
    }

    /// <summary>
    /// Initializes a new instance of the DistanceFieldFont class using the specified font and image
    /// files, with options to flip the image and use integer positioning.
    /// </summary>
    /// <param name="fontFile">The font definition file to load. Cannot be null.</param>
    /// <param name="imageFile">The image file containing the font atlas. Cannot be null.</param>
    /// <param name="flip">true to flip the image vertically when loading; otherwise, false.</param>
    /// <param name="integer">true to use integer positioning for glyph placement; otherwise, false.</param>
    public DistanceFieldFont( FileInfo fontFile, FileInfo imageFile, bool flip, bool integer )
        : base( fontFile, imageFile, flip, integer )
    {
    }

    /// <summary>
    /// Initializes a new instance of the DistanceFieldFont class using the specified font and image files.
    /// </summary>
    /// <param name="fontFile">The file containing the font definition to be loaded. Cannot be null.</param>
    /// <param name="imageFile">The image file associated with the font. Cannot be null.</param>
    /// <param name="flip">true to vertically flip the image when loading; otherwise, false.</param>
    public DistanceFieldFont( FileInfo fontFile, FileInfo imageFile, bool flip )
        : base( fontFile, imageFile, flip )
    {
    }

    /// <summary>
    /// Initializes a new instance of the DistanceFieldFont class using the specified font file, texture
    /// region, and orientation.
    /// </summary>
    /// <param name="fontFile">The font file containing the distance field font data. Cannot be null.</param>
    /// <param name="region">
    /// The texture region that defines the area of the texture to use for rendering the font. Cannot be null.
    /// </param>
    /// <param name="flip">true to flip the font vertically; otherwise, false.</param>
    public DistanceFieldFont( FileInfo fontFile, TextureRegion region, bool flip )
        : base( fontFile, region, flip )
    {
    }

    /// <summary>
    /// Initializes a new instance of the DistanceFieldFont class using the specified font file
    /// and texture region.
    /// </summary>
    /// <param name="fontFile">
    /// The font file containing the glyph definitions to be used for rendering text. Cannot be null.
    /// </param>
    /// <param name="region">
    /// The texture region that contains the distance field representation of the font glyphs.
    /// Cannot be null.
    /// </param>
    public DistanceFieldFont( FileInfo fontFile, TextureRegion region )
        : base( fontFile, region )
    {
    }

    /// <summary>
    /// Initializes a new instance of the DistanceFieldFont class using the specified font file and orientation.
    /// </summary>
    /// <param name="fontFile">The font file to load. Must not be null.</param>
    /// <param name="flip">true to vertically flip the font texture; otherwise, false.</param>
    public DistanceFieldFont( FileInfo fontFile, bool flip )
        : base( fontFile, flip )
    {
    }

    /// <summary>
    /// Initializes a new instance of the DistanceFieldFont class using the specified font file.
    /// </summary>
    /// <param name="fontFile">The font file to load. Must be a valid file containing font data.</param>
    public DistanceFieldFont( FileInfo fontFile )
        : base( fontFile )
    {
    }

    /// <summary>
    /// Loads the specified font data and configures associated texture regions for distance field font rendering.
    /// </summary>
    /// <remarks>This method ensures that all texture regions used for distance field font rendering are set
    /// to use linear filtering, which is required for correct visual output.</remarks>
    /// <param name="data">The font data to load. Must not be null.</param>
    protected override void Load( BitmapFontData data )
    {
        base.Load( data );

        // Distance field font rendering requires font texture to be filtered linear.
        List< TextureRegion > regions = GetRegions();

        foreach ( TextureRegion region in regions )
        {
            region.Texture?.SetFilter( TextureFilterMode.Linear, TextureFilterMode.Linear );
        }
    }

    /// <summary>
    /// Creates a new font cache instance for rendering bitmap fonts using distance field techniques.
    /// </summary>
    /// <remarks>The returned font cache supports efficient rendering of scalable bitmap fonts with smooth
    /// edges. Use this method when advanced font rendering quality is required.</remarks>
    /// <returns>A <see cref="BitmapFontCache"/> configured for distance field font rendering.</returns>
    public override BitmapFontCache NewFontCache()
    {
        return new DistanceFieldFontCache( this, GetUseIntegerPositions() );
    }

    /// <summary>
    /// Returns a new instance of the distance field shader if the u_smoothing
    /// uniform > 0.0. Otherwise the same code as the default SpriteBatch shader is used.
    /// </summary>
    public ShaderProgram CreateDistanceFieldShader()
    {
        const string VertexShader = "in vec4 a_position;\n"
                                  + "in vec4 a_color;\n"
                                  + "in vec2 u_texCoord" + "0;\n"
                                  + "uniform mat4 u_projTrans;\n"
                                  + "out vec4 v_color;\n"
                                  + "out vec2 v_texCoords;\n"
                                  + "\n"
                                  + "void main() {\n"
                                  + "	v_color = a_color"
                                  + ";\n"
                                  + "	v_color.a = v_color.a * (255.0/254.0);\n"
                                  + "	v_texCoords = u_texCoord0;\n"
                                  + "	gl_Position =  u_projTrans * a_position;\n"
                                  + "}\n";

        const string FragmentShader = "#ifdef GL_ES\n"
                                    + "#define LOWP lowp\n"
                                    + "precision mediump float;\n"
                                    + "#endif\n"
                                    + "\n"
                                    + "uniform sampler2D u_texture;\n"
                                    + "uniform float u_smoothing;\n"
                                    + "in vec4 v_color;\n"
                                    + "in vec2 v_texCoords;\n"
                                    + "out vec4 fragColor;\n"
                                    + "\n"
                                    + "void main() {\n"
                                    + "	if (u_smoothing > 0.0) {\n"
                                    + "		float smoothing = 0.25 / u_smoothing;\n"
                                    + "		float distance = texture(u_texture, v_texCoords).a;\n"
                                    + "		float alpha = smoothstep(0.5 - smoothing, 0.5 + smoothing, distance);\n"
                                    + "		fragColor = vec4(v_color.rgb, alpha * v_color.a);\n"
                                    + "	} else {\n"
                                    + "		fragColor = v_color * texture(u_texture, v_texCoords);\n"
                                    + "	}\n"
                                    + "}\n";

        var shader = new ShaderProgram( VertexShader, FragmentShader );

        return !shader.IsCompiled
                   ? throw new ArgumentException( $"Error compiling distance field shader: {shader.ShaderLog}" )
                   : shader;
    }
}

// ============================================================================
// ============================================================================