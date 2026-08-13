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
/// Provides a collection of predefined entity identifiers used to classify
/// various entity types within the system. Each entity identifier is represented
/// as an instance of the <c>EntityID</c> struct.
/// Any other entity identifiers can be defined in a similar class in local
/// game code.
/// </summary>
/// <remarks>
/// This class is designed to centralize and standardize the entity type identifiers
/// across the application, ensuring consistent usage and reducing potential for errors.
/// These identifiers are immutable and can be safely used as constants.
/// </remarks>
/// <example>
/// An Entity class could be created, defining some basic Entity properties and methods.
/// eg.
/// <code>
/// public sealed class Entity
/// {
///     public EntityID ID { get; set; } = EntityIDs.None;
///     public string Type { get; set; } = string.Empty;
///     public int Strength { get; set; }
/// }
/// </code>
/// Then in-game entities can be created like so:-
/// <code>
/// Entity Dragon = new()
/// {
///     ID       = MyGameEntityIDs.Dragon,
///     Type     = "Boss",
///     Strength = 500,
/// }
/// </code>
/// Entity IDs can be created like so:-
/// <code>
/// public static class MyGameEntityIDs
/// {
///     public static readonly EntityID Dragon = new( "mygame", "dragon" );
///     public static readonly EntityID Chest  = new( "mygame", "chest" );
/// }
/// </code>
/// </example>
[PublicAPI]
public static class EntityIDs
{
    // ----------------------------
    // The player entity
    public static readonly EntityID Player = new( "LughSharp", "Player" );

    // ----------------------------
    // Main Character type, i.e. Player
    public static readonly EntityID Main = new( "LughSharp", "Main" );

    // ----------------------------
    // Enemy Character type, but not stationary entities
    // like rocket launchers etc.
    public static readonly EntityID Enemy = new( "LughSharp", "Enemy" );

    // ----------------------------
    // Encapsulating type, covering any collision IDs that can be stood on.
    // This will be checked against the collision object TYPE, not the NAME.
    public static readonly EntityID Obstacle = new( "LughSharp", "Obstacle" );

    // ----------------------------
    // As above but for objects that can't be stood on and are not entities
    public static readonly EntityID Decoration = new( "LughSharp", "Decoration" );

    // As above, but for entities
    public static readonly EntityID Entity = new( "LughSharp", "Entity" );

    // ----------------------------
    // Interactive objects
    public static readonly EntityID Pickup      = new( "LughSharp", "Pickup" );
    public static readonly EntityID Weapon      = new( "LughSharp", "Weapon" );
    public static readonly EntityID Interactive = new( "LughSharp", "Interactive" );

    // ----------------------------

    public static readonly EntityID Dummy   = new( "LughSharp", "Dummy" );
    public static readonly EntityID Unknown = new( "LughSharp", "Unknown" );
    public static readonly EntityID NoID    = new( "LughSharp", "NoID" );
}

// ============================================================================
// ============================================================================
