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

namespace LughSharp.Lugh.Graphics.Images;

[PublicAPI]
public class Bitmap
{
    public required int              Width       { get; set; }
    public required int              Height      { get; set; }
    public required PixelType.Format PixelFormat { get; set; }
    public          byte[]?          PixelData   { get; set; }

    // ========================================================================

    private readonly Color _defaultPixelColor            = Color.Black;
    private readonly Color _defaultPixelTransparentColor = Color.Gray;

    // ========================================================================

    public Bitmap()
    {
    }

    [SetsRequiredMembers]
    public Bitmap( int width, int height, Color color )
    {
    }

    [SetsRequiredMembers]
    public Bitmap( FileInfo fileInfo ) : this( fileInfo.Name )
    {
    }

    [SetsRequiredMembers]
    public Bitmap( string filename )
    {
    }

    [SetsRequiredMembers]
    public Bitmap( Stream stream )
    {
    }

    [SetsRequiredMembers]
    public Bitmap( int width, int height, PixelType.Format settingsFormat )
    {
    }

    // ========================================================================

    public Color GetPixel( int x, int y )
    {
        return _defaultPixelColor;
    }

    public void SetPixel( int x, int y, Color? color )
    {
        if (( x < 0 ) || ( x >= Width ))
        {
            throw new ArgumentOutOfRangeException(nameof(x), "Coordinate x is out of range." );
        }

        if (( y < 0 ) || ( y >= Height ))
        {
            throw new ArgumentOutOfRangeException(nameof(y), "Coordinate y is out of range." );
        }

        if ( color == null )
        {
            color = _defaultPixelColor;
        }
    }

    public void SetRGB( int i, int i1, int argb )
    {
        throw new NotImplementedException();
    }

    public int GetRGB( int iw, int p1 )
    {
        throw new NotImplementedException();
    }
}