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
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;

namespace LughSharp.Core.Graphics;

/// <summary>
/// A general purpose class containing named colors that can be changed at will.
/// For example, the markup language defined by the <see cref="BitmapFontCache"/>
/// class uses this class to retrieve colors and the user can define his own colors.
/// </summary>
[PublicAPI]
public static class Colors
{
    public static readonly Dictionary< string, uint > ColorMap = new()
    {
        //@formatter:off
        { "CLEAR",          Color.Clear.RGBAPackedColor      },     //
        { "BLACK",          Color.Black.RGBAPackedColor      },     //
        { "WHITE",          Color.White.RGBAPackedColor      },     //
        { "LIGHT_GRAY",     Color.LightGray.RGBAPackedColor  },     //
        { "GRAY",           Color.Gray.RGBAPackedColor       },     //
        { "DARK_GRAY",      Color.DarkGray.RGBAPackedColor   },     //
        { "BLUE",           Color.Blue.RGBAPackedColor       },     //
        { "NAVY",           Color.Navy.RGBAPackedColor       },     //
        { "ROYAL",          Color.Royal.RGBAPackedColor      },     //
        { "SLATE",          Color.Slate.RGBAPackedColor      },     //
        { "SKY",            Color.Sky.RGBAPackedColor        },     //
        { "CYAN",           Color.Cyan.RGBAPackedColor       },     //
        { "TEAL",           Color.Teal.RGBAPackedColor       },     //
        { "GREEN",          Color.Green.RGBAPackedColor      },     //
        { "CHARTREUSE",     Color.Chartreuse.RGBAPackedColor },     //
        { "LIME",           Color.Lime.RGBAPackedColor       },     //
        { "FOREST",         Color.Forest.RGBAPackedColor     },     //
        { "OLIVE",          Color.Olive.RGBAPackedColor      },     //
        { "YELLOW",         Color.Yellow.RGBAPackedColor     },     //
        { "GOLD",           Color.Gold.RGBAPackedColor       },     //
        { "GOLDENROD",      Color.Goldenrod.RGBAPackedColor  },     //
        { "ORANGE",         Color.Orange.RGBAPackedColor     },     //
        { "BROWN",          Color.Brown.RGBAPackedColor      },     //
        { "TAN",            Color.Tan.RGBAPackedColor        },     //
        { "FIREBRICK",      Color.Firebrick.RGBAPackedColor  },     //
        { "RED",            Color.Red.RGBAPackedColor        },     //
        { "SCARLET",        Color.Scarlet.RGBAPackedColor    },     //
        { "CORAL",          Color.Coral.RGBAPackedColor      },     //
        { "SALMON",         Color.Salmon.RGBAPackedColor     },     //
        { "PINK",           Color.Pink.RGBAPackedColor       },     //
        { "MAGENTA",        Color.Magenta.RGBAPackedColor    },     //
        { "PURPLE",         Color.Purple.RGBAPackedColor     },     //
        { "VIOLET",         Color.Violet.RGBAPackedColor     },     //
        { "MAROON",         Color.Maroon.RGBAPackedColor     }      //
        //@formatter:on
    };

    // ========================================================================

    /// <summary>
    /// Looks up a color by its name.
    /// </summary>
    /// <param name="name">The name of the color.</param>
    /// <returns>
    /// The <see cref="Color"/> associated with the specified <paramref name="name"/>.
    /// </returns>
    public static Color Get( string name )
    {
        return new Color( ColorMap.GetValueOrDefault( name ) );
    }

    /// <summary>
    /// Returns the name of the color. This method compares the packed RGBA value
    /// of the color to the values in the <see cref="ColorMap"/> dictionary to find
    /// a match, and returns the name of the color if a match is found.
    /// </summary>
    public static string GetColorName( Color color )
    {
        uint colorU = color.RGBAPackedColor;

        foreach ( ( string name, uint col ) in ColorMap )
        {
            if ( col == colorU )
            {
                return name;
            }
        }

        // Should never get here so, hopefully, this is never called.
        throw new RuntimeException( $"Color not found in ColorMap: {color}" );
    }
}

// ============================================================================
// ============================================================================