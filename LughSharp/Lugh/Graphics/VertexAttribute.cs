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

using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Graphics.Utils;

namespace LughSharp.Lugh.Graphics;

/// <summary>
/// A single vertex attribute defined by its <see cref="Usage" />, its number of components and
/// its shader alias. The Usage is used for uniquely identifying the vertex attribute from among
/// its <see cref="VertexAttributes" /> siblings. The number of components  defines how many
/// components the attribute has. The alias defines to which shader attribute this attribute
/// should bind. The alias is used by a <see cref="Mesh" /> when drawing with a <see cref="ShaderProgram" />.
/// The alias can be changed at any time.
/// </summary>
[PublicAPI]
public class VertexAttribute
{
    // ========================================================================

    private readonly int _usageIndex;

    /// <summary>
    /// The alias for the attribute used in a <see cref="ShaderProgram" />
    /// </summary>
    public readonly string Alias;

    /// <summary>
    /// For fixed types, whether the values are normalized to either
    /// -1f and +1f (signed) or 0f and +1f (unsigned)
    /// </summary>
    public readonly bool Normalized;

    /// <summary>
    /// the number of components this attribute has
    /// </summary>
    public readonly int NumComponents;

    /// <summary>
    /// the OpenGL type of each component, e.g. <see cref="IGL.GL_FLOAT" />
    /// or <see cref="IGL.GL_UNSIGNED_BYTE" />
    /// </summary>
    public readonly int ComponentType;

    /// <summary>
    /// optional unit/index specifier, used for texture coordinates and bone weights.
    /// </summary>
    public readonly int Unit;

    /// <summary>
    /// The attribute <see cref="Usage" />, used for identification.
    /// </summary>
    public readonly int Usage;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Constructs a new VertexAttribute. The GL data type is automatically selected based on the usage.
    /// </summary>
    /// <param name="usage">
    /// The attribute <see cref="Usage" />, used to select the <see cref="Type" /> and for identification.
    /// </param>
    /// <param name="numComponents"> the number of components of this attribute, must be between 1 and 4. </param>
    /// <param name="alias">
    /// the alias used in a shader for this attribute. Can be changed after construction.
    /// </param>
    /// <param name="unit"> Optional unit/index specifier, used for texture coordinates and bone weights </param>
    public VertexAttribute( int usage, int numComponents, string alias, int unit = 0 )
        : this( usage,
                numComponents,
                usage == ( int )VertexConstants.Usage.ColorPacked ? IGL.GL_UNSIGNED_BYTE : IGL.GL_FLOAT,
                usage == ( int )VertexConstants.Usage.ColorPacked,
                alias,
                unit )
    {
    }

    /// <summary>
    /// Constructs a new VertexAttribute.
    /// </summary>
    /// <param name="usage">
    /// The attribute <see cref="Usage" />, used for identification.
    /// </param>
    /// <param name="numComponents">
    /// The number of components of this attribute, must be between 1 and 4.
    /// </param>
    /// <param name="type">
    /// The OpenGL type of each component, e.g. <see cref="IGL.GL_FLOAT" /> or <see cref="IGL.GL_UNSIGNED_BYTE" />.
    /// Since <see cref="Mesh" /> stores vertex data in 32bit floats, the total size of this attribute
    /// (type size times number of components) must be a multiple of four bytes.
    /// </param>
    /// <param name="normalized">
    /// For fixed types, whether the values are normalized to either -1f and +1f (signed) or 0f and +1f (unsigned)
    /// </param>
    /// <param name="alias">
    /// The alias used in a shader for this attribute. Can be changed after construction.
    /// </param>
    /// <param name="unit">
    /// Optional unit/index specifier, used for texture coordinates and bone weights
    /// </param>
    public VertexAttribute( int usage, int numComponents, int type, bool normalized, string alias, int unit = 0 )
    {
        Usage         = usage;
        NumComponents = numComponents;
        ComponentType = type;
        Normalized    = normalized;
        Alias         = alias;
        Unit          = unit;

        _usageIndex = int.TrailingZeroCount( usage );
    }

    /// <summary>
    /// the offset of this attribute in bytes, don't change this!
    /// </summary>
    public int Offset { get; set; }

    /// <returns>
    /// A copy of this VertexAttribute with the same parameters. The <see cref="Offset" /> is not copied
    /// and must be recalculated, as is typically done by the <see cref="VertexAttributes" /> that owns
    /// the VertexAttribute.
    /// </returns>
    public VertexAttribute Copy()
    {
        return new VertexAttribute( Usage, NumComponents, ComponentType, Normalized, Alias, Unit );
    }

    /// <summary>
    /// Creates a new VertexAttribute configured for position data with 3 components.
    /// The alias corresponds to the default shader attribute for position.
    /// </summary>
    /// <returns>
    /// A VertexAttribute representing position data, with usage set to POSITION and alias set to "a_position".
    /// </returns>
    public static VertexAttribute Position()
    {
        return new VertexAttribute( ( int )VertexConstants.Usage.Position,
                                    VertexConstants.POSITION_COMPONENTS,
                                    "a_position" );
    }

    /// <summary>
    /// Creates a VertexAttribute for texture coordinates.
    /// </summary>
    /// <param name="unit">
    /// The texture coordinate unit index. Determines the target texture unit
    /// for this attribute, commonly used in shaders.
    /// </param>
    /// <returns>
    /// A new instance of the <see cref="VertexAttribute" /> class configured
    /// for texture coordinates with 2 components.
    /// </returns>
    public static VertexAttribute TexCoords( int unit )
    {
        return new VertexAttribute( ( int )VertexConstants.Usage.TextureCoordinates,
                                    2,
                                    $"a_texCoord{unit}",
                                    unit );
    }

    /// <summary>
    /// Creates a VertexAttribute configured for normals.
    /// The attribute uses the NORMAL usage type, with 3 components, and a predefined alias.
    /// </summary>
    /// <returns>
    /// A new instance of <see cref="VertexAttribute" /> configured for normals.
    /// </returns>
    public static VertexAttribute Normal()
    {
        return new VertexAttribute( ( int )VertexConstants.Usage.Normal, 3, ShaderProgram.NORMAL_ATTRIBUTE );
    }

    /// <summary>
    /// Creates and returns a predefined VertexAttribute configured for use with packed color data.
    /// </summary>
    /// <returns>
    /// A VertexAttribute with four components, associated with the color attribute in shaders,
    /// using the unsigned byte OpenGL data type and normalized.
    /// </returns>
    public static VertexAttribute ColorPacked()
    {
        return new VertexAttribute( ( int )VertexConstants.Usage.ColorPacked,
                                    4,
                                    IGL.GL_UNSIGNED_BYTE,
                                    true,
                                    ShaderProgram.COLOR_ATTRIBUTE );
    }

    /// <summary>
    /// Creates and returns a new VertexAttribute instance configured for unpacked color data.
    /// </summary>
    /// <returns>
    /// A VertexAttribute instance with the usage set to color unpacked, 4 components,
    /// type set to GL_FLOAT, not normalized, and with the alias set to the color attribute
    /// used in shader programs.
    /// </returns>
    public static VertexAttribute ColorUnpacked()
    {
        return new VertexAttribute( ( int )VertexConstants.Usage.ColorUnpacked,
                                    4,
                                    IGL.GL_FLOAT,
                                    false,
                                    ShaderProgram.COLOR_ATTRIBUTE );
    }

    /// <summary>
    /// Creates a vertex attribute for tangent data used in shaders. The tangent attribute
    /// consists of three components and is associated with the shader attribute named "a_tangent".
    /// </summary>
    /// <returns>
    /// A new vertex attribute representing tangent data, configured for use in a shader.
    /// </returns>
    public static VertexAttribute Tangent()
    {
        return new VertexAttribute( ( int )VertexConstants.Usage.Tangent, 3, "a_tangent" );
    }

    /// <summary>
    /// Creates a VertexAttribute configured as a binormal vector, typically used in 3D graphics
    /// for normal mapping and related operations. The attribute uses 3 components and
    /// is assigned the alias defined by the shader's binormal attribute name.
    /// </summary>
    /// <returns>
    /// A new instance of <see cref="VertexAttribute" /> with usage set to binormal,
    /// 3 components, and the appropriate shader alias.
    /// </returns>
    public static VertexAttribute Binormal()
    {
        return new VertexAttribute( ( int )VertexConstants.Usage.BiNormal, 3, "a_binormal" );
    }

    /// <summary>
    /// Creates a new VertexAttribute representing a bone weight.
    /// The attribute is associated with a specific unit/index.
    /// </summary>
    /// <param name="unit">
    /// The unit or index specifier for the bone weight attribute.
    /// It determines which specific set of bone weights this attribute represents in the shader program.
    /// </param>
    /// <returns>
    /// A new instance of <see cref="VertexAttribute" /> configured for bone weight usage.
    /// </returns>
    public static VertexAttribute BoneWeight( int unit )
    {
        return new VertexAttribute( ( int )VertexConstants.Usage.BoneWeight,
                                    2,
                                    $"a_boneWeight{unit}",
                                    unit );
    }

    /// <returns>
    /// A unique number specifying the usage index (3 MSB) and unit (1 LSB).
    /// </returns>
    public int GetKey()
    {
        return ( _usageIndex << 8 ) + ( Unit & 0xFF );
    }

    /// <summary>
    /// Calculates the size in bytes of the vertex attribute based on its type and
    /// the number of components.
    /// </summary>
    /// <returns>The size in bytes required by the vertex attribute.</returns>
    public int GetSizeInBytes()
    {
        return ComponentType switch
        {
            IGL.GL_FLOAT          => 4 * NumComponents,
            IGL.GL_FIXED          => 4 * NumComponents,
            IGL.GL_UNSIGNED_SHORT => 2 * NumComponents,
            IGL.GL_SHORT          => 2 * NumComponents,
            IGL.GL_UNSIGNED_BYTE  => NumComponents,
            IGL.GL_BYTE           => NumComponents,
            var _                 => 0,
        };
    }

    /// <summary>
    /// Tests to determine if the passed object was created with the same parameters
    /// </summary>
    public override bool Equals( object? obj )
    {
        // Keeping this method body with this layout because converting
        // this to a 'return' statement made the code look less readable.
        // I may revisit this at some point.
        if ( obj is not VertexAttribute attribute )
        {
            return false;
        }

        return Equals( attribute );
    }

    /// <summary>
    /// Checks whether the specified <see cref="VertexAttribute" /> object is equal to the current instance.
    /// </summary>
    /// <param name="other">
    /// The <see cref="VertexAttribute" /> object to compare with the current instance.
    /// </param>
    /// <returns>
    /// true if the specified object is equal to the current instance; otherwise, false.
    /// </returns>
    public bool Equals( VertexAttribute? other )
    {
        return ( other != null )
               && ( Usage == other.Usage )
               && ( NumComponents == other.NumComponents )
               && ( ComponentType == other.ComponentType )
               && ( Normalized == other.Normalized )
               && Alias.Equals( other.Alias )
               && ( Unit == other.Unit );
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var result = GetKey();

        result = ( 541 * result ) + NumComponents;
        result = ( 541 * result ) + ( Alias.Length * Unit );

        return result;
    }
}