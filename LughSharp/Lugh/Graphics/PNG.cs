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

using System.IO.Hashing;

using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace LughSharp.Lugh.Graphics;

/// <summary>
/// The Paeth filter computes a simple linear function of the three neighboring pixels
/// (left, above, upper left), then chooses as predictor the neighboring pixel closest
/// to the computed value. This technique is due to Alan W. Paeth [PAETH].
/// To compute the Paeth filter, apply the following formula to each byte of the scanline:
/// <code>
///    Paeth(x) = Raw(x) - PaethPredictor(Raw(x-bpp), Prior(x), Prior(x-bpp))
/// </code>
/// where x ranges from zero to the number of bytes representing the scanline minus one,
/// Raw(x) refers to the raw data byte at that byte position in the scanline, Prior(x)
/// refers to the unfiltered bytes of the prior scanline, and bpp is defined as for the
/// Sub filter. Note this is done for each byte, regardless of bit depth. Unsigned arithmetic
/// modulo 256 is used, so that both the inputs and outputs fit into bytes. The sequence of
/// Paeth values is transmitted as the filtered scanline.
/// </summary>
[PublicAPI]
public class PNG : IDisposable
{
    private const int  IHDR                = 0x49484452;
    private const int  IDAT                = 0x49444154;
    private const int  IEND                = 0x49454E44;
    private const byte COLOR_ARGB          = 6;
    private const byte COMPRESSION_DEFLATE = 0;
    private const byte FILTER_NONE         = 0;
    private const byte INTERLACE_NONE      = 0;
    private const byte PAETH_FILTER        = 4;

    private readonly ChunkBuffer _buffer;
    private readonly Deflater    _deflater;
    private          ByteArray?  _curLineBytes;

    private int        _lastLineLen;
    private ByteArray? _lineOutBytes;
    private ByteArray? _prevLineBytes;

    // ====================================================================
    // ====================================================================

    /// <summary>
    /// </summary>
    /// <param name="initialBufferSize"></param>
    public PNG( int initialBufferSize = 128 * 128 )
    {
        _buffer   = new ChunkBuffer( initialBufferSize );
        _deflater = new Deflater();
    }

    /// <summary>
    /// If true, the resulting PNG is flipped vertically. Default is true.
    /// </summary>
    public bool FlipY { get; set; }

    /// <summary>
    /// Sets the deflate compression level.
    /// Default is <see cref="Deflater.DEFAULT_COMPRESSION"/>.
    /// </summary>
    public void SetCompression( int level )
    {
        _deflater.SetLevel( level );
    }

    /// <summary>
    /// Writes the supplied <see cref="Pixmap"/> to the supplied <see cref="FileInfo"/> instance.
    /// </summary>
    public void Write( FileInfo file, Pixmap pixmap )
    {
        var output = new MemoryStream();

        try
        {
            Write( output, pixmap );
        }
        catch ( IOException )
        {
            // ignored
        }
    }

    /// <summary>
    /// Writes the pixmap to the stream without closing the stream.
    /// </summary>
    public void Write( Stream output, Pixmap pixmap )
    {
        var deflaterOutput = new DeflaterOutputStream( output /*_buffer*/, _deflater );
        var dataOutput     = new BinaryWriter( output );

        dataOutput.Write( PNGDecoder.StandardPNGSignature );

        _buffer.Write( IHDR );                 //
        _buffer.Write( pixmap.Width );         //
        _buffer.Write( pixmap.Height );        // 
        _buffer.Write( pixmap.GetBitDepth() ); // 
        _buffer.Write( COLOR_ARGB );           // 
        _buffer.Write( COMPRESSION_DEFLATE );  // 
        _buffer.Write( FILTER_NONE );          //
        _buffer.Write( INTERLACE_NONE );       //
        _buffer.EndChunk( dataOutput );        //
        _buffer.Write( IDAT );                 //

        _deflater.Reset();

        var lineLen = pixmap.Width * 4;

        byte[] lineOut;
        byte[] curLine;
        byte[] prevLine;

        if ( _lineOutBytes == null )
        {
            _lineOutBytes  = new ByteArray( lineLen );
            _curLineBytes  = new ByteArray( lineLen );
            _prevLineBytes = new ByteArray( lineLen );

            lineOut  = new byte[ lineLen ];
            curLine  = new byte[ lineLen ];
            prevLine = new byte[ lineLen ];
        }
        else
        {
            _lineOutBytes.EnsureCapacity( lineLen );
            _curLineBytes!.EnsureCapacity( lineLen );
            _prevLineBytes!.EnsureCapacity( lineLen );

            lineOut  = _lineOutBytes.ToArray();
            curLine  = _curLineBytes.ToArray();
            prevLine = _prevLineBytes.ToArray();

            Array.Clear( prevLine, 0, _lastLineLen );
        }

        _lastLineLen = lineLen;

        var oldPosition = pixmap.ByteBuffer.Position;
        var isRgba8888  = pixmap.IsRGBA8888();

        for ( int y = 0, h = pixmap.Height; y < h; y++ )
        {
            var py = FlipY ? h - y - 1 : y;

            if ( isRgba8888 )
            {
                pixmap.ByteBuffer.Position = py * lineLen;
                pixmap.ByteBuffer.GetBytes( curLine, 0, lineLen );
            }
            else
            {
                for ( int px = 0, x = 0; px < pixmap.Width; px++ )
                {
                    var pixel = pixmap.GetPixel( px, py );

                    curLine[ x++ ] = ( byte )( ( pixel >> 24 ) & 0xff );
                    curLine[ x++ ] = ( byte )( ( pixel >> 16 ) & 0xff );
                    curLine[ x++ ] = ( byte )( ( pixel >> 8 ) & 0xff );
                    curLine[ x++ ] = ( byte )( pixel & 0xff );
                }
            }

            lineOut[ 0 ] = ( byte )( curLine[ 0 ] - prevLine[ 0 ] );
            lineOut[ 1 ] = ( byte )( curLine[ 1 ] - prevLine[ 1 ] );
            lineOut[ 2 ] = ( byte )( curLine[ 2 ] - prevLine[ 2 ] );
            lineOut[ 3 ] = ( byte )( curLine[ 3 ] - prevLine[ 3 ] );

            for ( var x = 4; x < lineLen; x++ )
            {
                var a  = curLine[ x - 4 ] & 0xff;
                var b  = prevLine[ x ] & 0xff;
                var c  = prevLine[ x - 4 ] & 0xff;
                var p  = ( a + b ) - c;
                var pa = p - a;

                if ( pa < 0 )
                {
                    pa = -pa;
                }

                var pb = p - b;

                if ( pb < 0 )
                {
                    pb = -pb;
                }

                var pc = p - c;

                if ( pc < 0 )
                {
                    pc = -pc;
                }

                if ( ( pa <= pb ) && ( pa <= pc ) )
                {
                    c = a;
                }
                else if ( pb <= pc )
                {
                    c = b;
                }

                lineOut[ x ] = ( byte )( curLine[ x ] - c );
            }

            deflaterOutput.WriteByte( PAETH_FILTER );
            deflaterOutput.Write( lineOut, 0, lineLen );

            ( curLine, prevLine ) = ( prevLine, curLine );
        }

        pixmap.ByteBuffer.Position = oldPosition;

        deflaterOutput.Finish();
        _buffer.EndChunk( dataOutput );

        _buffer.Write( IEND );
        _buffer.EndChunk( dataOutput );

        output.Flush();
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing,
    /// or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        _deflater.Finish();
        GC.SuppressFinalize( this );
    }

    // ====================================================================

    [PublicAPI]
    public class ChunkBuffer : BinaryWriter
    {
        private readonly MemoryStream _buffer;
        private readonly Crc32        _crc;

        public ChunkBuffer( int initialSize )
            : this( new MemoryStream( initialSize ), new Crc32() )
        {
        }

        private ChunkBuffer( MemoryStream buffer, Crc32 crc )
            : base( new MemoryStream() /*new CheckedOutputStream( buffer, crc )*/ )
        {
            _buffer = buffer;
            _crc    = crc;
        }

        public void EndChunk( BinaryWriter target )
        {
            Flush();

            target.Write( _buffer.Length - 4 );
            target.Write( _buffer.ToArray() );
            target.Write( _crc.GetCurrentHash() );

            _buffer.Position = 0;
            _crc.Reset();
        }
    }
}