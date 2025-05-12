// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Red 7 Projects
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// /////////////////////////////////////////////////////////////////////////////

using LughSharp.Lugh.Graphics.Images;

namespace LughSharp.Lugh.Graphics.Utils;

[PublicAPI]
public class BMPUtils
{
    private BMPUtils()
    {
    }

    // ========================================================================

    public static BMPFormatStructs.BMPHeader     BMPHeader     { get; private set; }
    public static BMPFormatStructs.BMPInfoHeader BMPInfoHeader { get; private set; }

    // ========================================================================

    public static void AnalyseBMP( string filename )
    {
        var data = File.ReadAllBytes( filename );

        AnalyseBMP( data );
    }

    public static void AnalyseBMP( byte[] pngData, bool showResults = false )
    {
        if ( ( pngData == null ) || ( pngData.Length == 0 ) )
        {
            throw new ArgumentException( "Invalid BMP Data" );
        }

        BMPHeader = new BMPFormatStructs.BMPHeader
        {
            //TODO:
        };
        
        
    }
}

