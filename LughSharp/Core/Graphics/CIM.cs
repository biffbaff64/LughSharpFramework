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

using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Graphics;

/// <summary>
/// Provides functionality for reading and writing CIM format Pixmaps.
/// </summary>
[PublicAPI]
public static class CIM
{
    private const int BUFFER_SIZE = 32000;

    private static readonly byte[] WriteBuffer = new byte[ BUFFER_SIZE ];
    private static readonly byte[] ReadBuffer  = new byte[ BUFFER_SIZE ];

    // ========================================================================
    
    /// <summary>
    /// </summary>
    /// <param name="file"></param>
    /// <param name="pixmap"></param>
    /// <exception cref="RuntimeException"></exception>
    public static void Write( FileInfo file, Pixmap pixmap )
    {
        try
        {
            var deflaterOutputStream = new DeflaterOutputStream( file.OpenWrite() );
            var output               = new BinaryWriter( deflaterOutputStream );

            output.Write( pixmap.Width );
            output.Write( pixmap.Height );
            output.Write( ( int )pixmap.GetColorFormat() );

            var pixelBuf = pixmap.ByteBuffer;

            pixelBuf.Position = 0;
            pixelBuf.Limit    = pixelBuf.Capacity;

            var remainingBytes = pixelBuf.Capacity % BUFFER_SIZE;
            var iterations     = pixelBuf.Capacity / BUFFER_SIZE;

            lock ( WriteBuffer )
            {
                for ( var i = 0; i < iterations; i++ )
                {
                    pixelBuf.GetBytes( WriteBuffer );
                    output.Write( WriteBuffer );
                }

                pixelBuf.GetBytes( WriteBuffer, 0, remainingBytes );
                output.Write( WriteBuffer, 0, remainingBytes );
            }

            pixelBuf.Position = 0;
            pixelBuf.Limit    = pixelBuf.Capacity;
        }
        catch ( Exception e )
        {
            throw new RuntimeException( "Couldn't write Pixmap to file '" + file + "'", e );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    /// <exception cref="RuntimeException"></exception>
    public static Pixmap Read( FileInfo file )
    {
        try
        {
            var input    = new BinaryReader( new InflaterInputStream( file.OpenRead() ) );
            var width    = input.Read();
            var height   = input.Read();
            var format   = PixelFormat.PNGColorToLughFormat( input.Read() );
            var pixmap   = new Pixmap( width, height, format );
            var pixelBuf = pixmap.ByteBuffer;

            pixelBuf.Position = 0;
            pixelBuf.Limit    = pixelBuf.Capacity;

            lock ( ReadBuffer )
            {
                int readBytes;

                while ( ( readBytes = input.Read( ReadBuffer ) ) > 0 )
                {
                    pixelBuf.PutBytes( ReadBuffer, 0, 0, readBytes );
                }
            }

            pixelBuf.Position = 0;
            pixelBuf.Limit    = pixelBuf.Capacity;

            return pixmap;
        }
        catch ( Exception e )
        {
            throw new RuntimeException( "Couldn't read Pixmap from file '" + file + "'", e );
        }
    }
}

// ============================================================================
// ============================================================================
