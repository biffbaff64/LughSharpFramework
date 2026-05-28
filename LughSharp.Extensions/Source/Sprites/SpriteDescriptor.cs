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

using JetBrains.Annotations;

using LughSharp.Source.Graphics.G2D;
using LughSharp.Source.Maths;
using LughSharp.Source.Maths.Collision;

namespace Extensions.Source.Sprites;

/// <summary>
/// Used for storing relevant information for creating, placing and initialising sprites.
/// </summary>
[PublicAPI]
public struct SpriteDescriptor
{
    public String        Name;     // Name of the sprite, for debugging purposes.
    public GraphicID     Gid;      // ID. See GraphicID class for options.
    public TileID        Tile;     // The placement tile associated with this sprite.
    public String        Asset;    // The initial image asset.
    public int           Frames;   // Number of frames in the asset above.
    public GraphicID     Type;     // _MAIN, _INTERACTIVE, _PICKUP etc
    public Vector2       Position; // X, Y Pos of tile, in TileWidth units, Z-Sort value.
    public Vector2       Size;     // Width and Height.
    public int           Index;    // This entity's position in the entity map.
    public AnimationMode Playmode; // Animation playmode for the asset frames above.
    public float         AnimRate; // Animation speed
    public Sprite2D      Parent;   // Parent GDXSprite (if applicable).
    public int           Link;     // Linked GDXSprite (if applicable).
    public Vector2       Dir;      // Initial direction of travel. Useful for moving blocks etc.
    public Vector2       Dist;     // Initial travel distance. Useful for moving blocks etc.
    public Vector2       Speed;    // Initial speed. Useful for moving blocks etc.
    public Box           Box;      //
}