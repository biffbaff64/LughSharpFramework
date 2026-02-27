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

using System.Collections.Generic;

using JetBrains.Annotations;

using LughSharp.Core.Graphics.Text;
using LughSharp.Core.Utils.Logging;

namespace LughSharp.Core.Graphics;

/// <summary>
/// A general purpose class containing named colors that can be changed at will.
/// For example, the markup language defined by the <see cref="BitmapFontCache"/> class
/// uses this class to retrieve colors and the user can define his own colors.
/// </summary>
[PublicAPI]
public static class Colors
{
    public static readonly Dictionary< string, Color > Map = new()
    {
        //@formatter:off
        { "CLEAR",          Color.Clear         }, // 
        { "BLACK",          Color.Black         }, // 
        { "WHITE",          Color.White         }, // 
        { "LIGHT_GRAY",     Color.LightGray     }, // 
        { "GRAY",           Color.Gray          }, // 
        { "DARK_GRAY",      Color.DarkGray      }, // 
        { "BLUE",           Color.Blue          }, // 
        { "NAVY",           Color.Navy          }, // 
        { "ROYAL",          Color.Royal         }, // 
        { "SLATE",          Color.Slate         }, // 
        { "SKY",            Color.Sky           }, // 
        { "CYAN",           Color.Cyan          }, // 
        { "TEAL",           Color.Teal          }, // 
        { "GREEN",          Color.Green         }, // 
        { "CHARTREUSE",     Color.Chartreuse    }, // 
        { "LIME",           Color.Lime          }, // 
        { "FOREST",         Color.Forest        }, // 
        { "OLIVE",          Color.Olive         }, // 
        { "YELLOW",         Color.Yellow        }, // 
        { "GOLD",           Color.Gold          }, // 
        { "GOLDENROD",      Color.Goldenrod     }, // 
        { "ORANGE",         Color.Orange        }, // 
        { "BROWN",          Color.Brown         }, // 
        { "TAN",            Color.Tan           }, // 
        { "FIREBRICK",      Color.Firebrick     }, // 
        { "RED",            Color.Red           }, // 
        { "SCARLET",        Color.Scarlet       }, // 
        { "CORAL",          Color.Coral         }, // 
        { "SALMON",         Color.Salmon        }, // 
        { "PINK",           Color.Pink          }, // 
        { "MAGENTA",        Color.Magenta       }, // 
        { "PURPLE",         Color.Purple        }, // 
        { "VIOLET",         Color.Violet        }, // 
        { "MAROON",         Color.Maroon        }  // 
        //@formatter:on
    };

    // ========================================================================

    /// <summary>
    /// Looks up a color by its name.
    /// </summary>
    /// <param name="name">The name of the color.</param>
    /// <returns>
    /// The <see cref="Color"/> associated with the specified <paramref name="name"/>,
    /// or <c>null</c> if no mapping was found.
    /// </returns>
    public static Color? Get( string name )
    {
        return Map.GetValueOrDefault( name );
    }

    /// <summary>
    /// Prints all the colors in the <see cref="Map"/> to the log.
    /// </summary>
    public static void PrintAll()
    {
        foreach ( ( string name, Color color ) in Map )
        {
            Logger.Debug( $"{name,-12} = {color.RgbaToString()}" );
        }
    }
}

// ============================================================================
// ============================================================================