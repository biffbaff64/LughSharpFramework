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

namespace LughSharp.Core.Graphics.Colors;

/// <summary>
/// A general purpose class containing named colors that can be changed at will.
/// For example, the markup language defined by the <see cref="BitmapFontCache"/> class
/// uses this class to retrieve colors and the user can define his own colors.
/// </summary>
[PublicAPI]
public static class Colors
{
    public static readonly Dictionary< string, Color4 > Map = new()
    {
        //@formatter:off
        { "CLEAR",          Color4.Clear         }, // 
        { "BLACK",          Color4.Black         }, // 
        { "WHITE",          Color4.White         }, // 
        { "LIGHT_GRAY",     Color4.LightGray     }, // 
        { "GRAY",           Color4.Gray          }, // 
        { "DARK_GRAY",      Color4.DarkGray      }, // 
        { "BLUE",           Color4.Blue          }, // 
        { "NAVY",           Color4.Navy          }, // 
        { "ROYAL",          Color4.Royal         }, // 
        { "SLATE",          Color4.Slate         }, // 
        { "SKY",            Color4.Sky           }, // 
        { "CYAN",           Color4.Cyan          }, // 
        { "TEAL",           Color4.Teal          }, // 
        { "GREEN",          Color4.Green         }, // 
        { "CHARTREUSE",     Color4.Chartreuse    }, // 
        { "LIME",           Color4.Lime          }, // 
        { "FOREST",         Color4.Forest        }, // 
        { "OLIVE",          Color4.Olive         }, // 
        { "YELLOW",         Color4.Yellow        }, // 
        { "GOLD",           Color4.Gold          }, // 
        { "GOLDENROD",      Color4.Goldenrod     }, // 
        { "ORANGE",         Color4.Orange        }, // 
        { "BROWN",          Color4.Brown         }, // 
        { "TAN",            Color4.Tan           }, // 
        { "FIREBRICK",      Color4.Firebrick     }, // 
        { "RED",            Color4.Red           }, // 
        { "SCARLET",        Color4.Scarlet       }, // 
        { "CORAL",          Color4.Coral         }, // 
        { "SALMON",         Color4.Salmon        }, // 
        { "PINK",           Color4.Pink          }, // 
        { "MAGENTA",        Color4.Magenta       }, // 
        { "PURPLE",         Color4.Purple        }, // 
        { "VIOLET",         Color4.Violet        }, // 
        { "MAROON",         Color4.Maroon        }  // 
        //@formatter:on
    };

    // ========================================================================

    /// <summary>
    /// Looks up a color by its name.
    /// </summary>
    /// <param name="name">The name of the color.</param>
    /// <returns>
    /// The <see cref="Color4"/> associated with the specified <paramref name="name"/>,
    /// or <c>null</c> if no mapping was found.
    /// </returns>
    public static Color4? Get( string name )
    {
        return Map.GetValueOrDefault( name );
    }

    /// <summary>
    /// Prints all the colors in the <see cref="Map"/> to the log.
    /// </summary>
    public static void PrintAll()
    {
        foreach ( ( string name, Color4 color ) in Map )
        {
            Logger.Debug( $"{name,-12} = {color.AsRgbaString()}" );
        }
    }
}

// ============================================================================
// ============================================================================