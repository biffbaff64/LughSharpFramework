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

using System.IO.Compression;

namespace LughSharp.Source.Graphics.Utils;

/// <summary>
/// Class for storing ETC1 compressed image data.
/// </summary>
[PublicAPI]
public class ETC1Data : IDisposable
{
    private readonly ETC1 _etc1;

    public ETC1Data( int width, int height, Buffer< byte > compressedData, int dataOffset, ETC1 etc1 )
    {
        _etc1          = etc1;
        Width          = width;
        Height         = height;
        CompressedData = compressedData;
        DataOffset     = dataOffset;

        CheckNpot();
    }

    public ETC1Data( FileInfo pkmFile, ETC1 etc )
    {
        var           buffer = new byte[ 1024 * 10 ];
        BinaryReader? input  = null;

        _etc1 = etc;

        try
        {
            var zipStream      = new GZipStream( pkmFile.OpenRead(), CompressionMode.Decompress );
            var bufferedStream = new BufferedStream( zipStream );

            input = new BinaryReader( bufferedStream );

            int fileSize = input.ReadInt32();

            CompressedData = new Buffer< byte >( fileSize );

            int readBytes;

            while ( ( readBytes = input.Read( buffer ) ) != -1 )
            {
                CompressedData.Put( buffer, 0, 0, readBytes );
            }

            CompressedData.Position = 0;
            CompressedData.Limit    = CompressedData.Capacity;
        }
        catch ( Exception e )
        {
            throw new RuntimeException( "Couldn't load pkm file '" + pkmFile + "'", e );
        }
        finally
        {
            input?.Close();
        }

        Width      = _etc1.GetWidthPkm( CompressedData, 0 );
        Height     = _etc1.GetHeightPkm( CompressedData, 0 );
        DataOffset = ETC1.PkmHeaderSize;

        CompressedData.Position = DataOffset;
        CheckNpot();
    }

    /// <summary>
    /// the width in pixels.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// the height in pixels.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// the optional PKM header and compressed image data.
    /// </summary>
    public Buffer< byte > CompressedData { get; set; }

    /// <summary>
    /// the offset in bytes to the actual compressed data.
    /// Might be 16 if this contains a PKM header, 0 otherwise.
    /// </summary>
    public int DataOffset { get; set; }

    /// <summary>
    /// Returns whether this ETC1Data has a PKM header.
    /// </summary>
    public bool HasPkmHeader()
    {
        return DataOffset == 16;
    }

    private void CheckNpot()
    {
        if ( !MathUtils.IsPowerOfTwo( Width ) || !MathUtils.IsPowerOfTwo( Height ) )
        {
            Console.WriteLine( "ETC1Data warning: non-power-of-two ETC1textures may crash the driver of PowerVR GPUs" );
        }
    }

    /// <summary>
    /// Writes the ETC1Data with a PKM header to the given file.
    /// </summary>
    /// <param name="file"> the file. </param>
    public void Write( FileInfo file )
    {
//            DataOutputStream? write        = null;
//            var               buffer       = new sbyte[ 10 * 1024 ];
//            var               writtenBytes = 0;

//            CompressedData.Position = 0;
//            CompressedData.Limit    = CompressedData.Capacity;

//            try
//            {
//                write = new DataOutputStream( new GZIPOutputStream( file.Write( false ) ) );
//                write.WriteInt( CompressedData.Capacity );

//                while ( writtenBytes != CompressedData.Capacity )
//                {
//                    var bytesToWrite = Math.Min( CompressedData.Remaining(), buffer.Length );

//                    CompressedData.Get( buffer, 0, bytesToWrite );

//                    write.Write( buffer, 0, bytesToWrite );
//                    writtenBytes += bytesToWrite;
//                }
//            }
//            catch ( System.Exception e )
//            {
//                throw new RuntimeException( "Couldn't write PKM file to '" + file + "'", e );
//            }
//            finally
//            {
//                StreamUtils.CloseQuietly( write );
//            }

//            CompressedData.Position = DataOffset;
//            CompressedData.Limit    = CompressedData.Capacity;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if ( HasPkmHeader() )
        {
            return $"{( _etc1.IsValidPkm( CompressedData, 0 ) ? "valid" : "invalid" )} "
                 + $"pkm [{_etc1.GetWidthPkm( CompressedData, 0 )}x{_etc1.GetHeightPkm( CompressedData, 0 )}], "
                 + $"compressed: {CompressedData.Capacity - ETC1.PkmHeaderSize}";
        }

        return $"raw [{Width}x{Height}], compressed: {CompressedData.Capacity - ETC1.PkmHeaderSize}";
    }

    /// <summary>
    /// Releases the native resources of the ETC1Data instance.
    /// </summary>
    public void Dispose()
    {
        CompressedData.Dispose();

        GC.SuppressFinalize( this );
    }
}

// ============================================================================
// ============================================================================

