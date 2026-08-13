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

namespace LughSharp.Source.Entities;

/// <summary>
/// Represents a unique identifier for an entity, composed of a namespace and a name.
/// </summary>
/// <remarks>
/// The <see cref="EntityID"/> struct is designed to uniquely identify entities by pairing
/// a logical namespace with a specific name. This allows for structured categorization and
/// avoids identifier collisions within a system.
/// </remarks>
/// <example>
/// An instance of <see cref="EntityID"/> might represent a player, an obstacle, or other
/// entity types within the system. It is primarily intended for use in entity management
/// scenarios.
/// </example>
[PublicAPI]
public readonly record struct EntityID
{
    public string Namespace { get; }
    public string Name      { get; }

    // ========================================================================
    
    /// <summary>
    /// Represents a unique identifier for an entity, defined by a namespace and a name.
    /// </summary>
    /// <param name="namespace"> The logical namespace for the entity. </param>
    /// <param name="name"> The specific name for the entity within its namespace. </param>
    /// <remarks>
    /// The <see cref="EntityID"/> struct is a combination of a namespace and a name string,
    /// ensuring uniqueness across entities. This struct is immutable and supports strong
    /// categorization for entity management in a system.
    /// </remarks>
    public EntityID( string @namespace, string name )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace( @namespace );
        ArgumentException.ThrowIfNullOrWhiteSpace( name );

        Namespace = @namespace;
        Name      = name;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Namespace}:{Name}";
    }
}

// ============================================================================
// ============================================================================

