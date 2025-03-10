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

using Image = System.Drawing.Image;

namespace LughSharp.Lugh.Graphics.Images;

[PublicAPI]
public class BufferedImage : IDisposable
{
    public const int TYPE_INT_ARGB   = 0;
    public const int TYPE_INT_RGB    = 1;
    public const int TYPE_BYTE_GRAY  = 2;
    public const int TYPE_4BYTE_ABGR = 3;
    
    public BufferedImage(int width, int height, int getBufferedImageType)
    {
    }

    public int Width { get; set; }
    public int Height { get; set; }

    public void Dispose()
    {
        GC.SuppressFinalize( this );
    }

    public int GetRGB( int p0, int p1 )
    {
        throw new NotImplementedException();
    }

    public Image GetImage()
    {
        throw new NotImplementedException();
    }

    public void SetRGB( int i, int i1, int argb )
    {
        throw new NotImplementedException();
    }
}