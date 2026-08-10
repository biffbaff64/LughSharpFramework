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

namespace LughSharp.Source.Physics;

/// <summary>
/// Represents movement logic and constants used for directional calculations.
/// This class provides predefined constants for various directional movements and
/// methods to manage and translate directions.
/// </summary>
[PublicAPI]
public class Movement
{
    public const int Horizontal      = 1;
    public const int Vertical        = 2;
    public const int DirectionIn     = 1;
    public const int DirectionOut    = -1;
    public const int Forwards        = 1;
    public const int Backwards       = -1;
    public const int DirectionRight  = 1;
    public const int DirectionLeft   = -1;
    public const int DirectionUp     = 1;
    public const int DirectionDown   = -1;
    public const int DirectionStill  = 0;
    public const int DirectionCustom = 2;

    /// <summary>
    /// A static readonly table mapping horizontal and vertical directional components to
    /// their corresponding translated direction enumeration and descriptive name.
    /// This table is used for directional logic and translation within the Movement class.
    /// </summary>
    //@formatter:off
    public static readonly DirectionValue[] TranslationTable =
    {
        new( DirectionStill, DirectionStill,  Dir.Still,      "Still"     ),
        new( DirectionLeft,  DirectionStill,  Dir.Left,       "Left"      ),
        new( DirectionRight, DirectionStill,  Dir.Right,      "Right"     ),
        new( DirectionStill, DirectionUp,     Dir.Up,         "Up"        ),
        new( DirectionStill, DirectionDown,   Dir.Down,       "Down"      ),
        new( DirectionLeft,  DirectionUp,     Dir.UpLeft,     "UpLeft"    ),
        new( DirectionRight, DirectionUp,     Dir.UpRight,    "UpRight"   ),
        new( DirectionLeft,  DirectionDown,   Dir.DownLeft,   "DownLeft"  ),
        new( DirectionRight, DirectionDown,   Dir.DownRight,  "DownRight" ),
    };
    //@formatter:on

    // ========================================================================

    /// <summary>
    /// Translates a given direction into a predefined directional value and its
    /// corresponding name.
    /// </summary>
    /// <param name="direction">
    /// The direction to be translated, represented as an instance of the <see cref="Direction"/> class.
    /// </param>
    /// <returns>
    /// A tuple containing the translated directional value as a <see cref="Dir"/> enumeration
    /// member and the corresponding name of the direction as a string.
    /// </returns>
    public static ( Dir dir, string name ) TranslateDirection( Direction direction )
    {
        return TranslateDirection( direction.X, direction.Y );
    }

    /// <summary>
    /// Translates a given direction into a predefined directional value and its
    /// corresponding name.
    /// </summary>
    /// <param name="x"> The x-value of the direction to be translated. </param>
    /// <param name="y"> The y-value of the direction to be translated. </param>
    /// <returns>
    /// A tuple containing the translated directional value as a <see cref="Dir"/> enumeration
    /// member and the corresponding name of the direction as a string.
    /// </returns>
    public static ( Dir dir, string name ) TranslateDirection( int x, int y )
    {
        var translatedDir = Dir.Still;
        var name          = string.Empty;

        foreach ( DirectionValue directionValue in TranslationTable )
        {
            if ( ( directionValue.DirX == x ) && ( directionValue.DirY == y ) )
            {
                translatedDir = directionValue.Translated;
            }
        }

        return ( translatedDir, name );
    }
    
    /// <summary>
    /// Represents a directional value that encapsulates information about
    /// the horizontal and vertical components of a direction, a translated direction
    /// representation, and a descriptive name.
    /// </summary>
    [PublicAPI]
    public struct DirectionValue( int x, int y, Dir trans, string name )
    {
        public int    DirX       { get; set; } = x;
        public int    DirY       { get; set; } = y;
        public Dir    Translated { get; set; } = trans;
        public string Name       { get; set; } = name;
    }
}

// ============================================================================
// ============================================================================
