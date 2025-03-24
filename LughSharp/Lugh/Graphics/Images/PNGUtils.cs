// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin
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

using System.Runtime.InteropServices;
using System.Text;

using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.Images;

[PublicAPI]
public class PNGUtils
{
    public const int SIGNATURE_LENGTH       = 8;
    public const int IHDR_START             = 8;
    public const int IHDR_SIZE              = 4;
    public const int IHDR_CHUNK_TYPE_OFFSET = IHDR_START + IHDR_SIZE;
    public const int IHDR_DATA_OFFSET       = 16;
    public const int IHDR_DATA_SIZE         = 13;
    public const int IHDR_CRC_START         = IHDR_DATA_OFFSET + IHDR_DATA_SIZE;
    public const int IHDR_CRC_SIZE          = 4;
    public const int IDAT_START             = IHDR_CRC_START + IHDR_CRC_SIZE;
    public const int IDAT_SIZE              = 4;
    public const int IDAT_CHUNK_TYPE_OFFSET = IDAT_START + IDAT_SIZE;
    public const int IDAT_DATA_OFFSET       = IDAT_CHUNK_TYPE_OFFSET + IDAT_SIZE;

    // ------------------------------------------------------------------------

    public const bool SHOW_OUTPUT = true;
    public const bool NO_OUTPUT   = false;

    // ------------------------------------------------------------------------

    private const int WIDTH_OFFSET       = IHDR_DATA_OFFSET;
    private const int WIDTH_SIZE         = sizeof( uint );
    private const int HEIGHT_OFFSET      = WIDTH_OFFSET + WIDTH_SIZE;
    private const int HEIGHT_SIZE        = sizeof( uint );
    private const int BITDEPTH_OFFSET    = HEIGHT_OFFSET + HEIGHT_SIZE;
    private const int COLORTYPE_OFFSET   = BITDEPTH_OFFSET + 1;
    private const int COMPRESSION_OFFSET = COLORTYPE_OFFSET + 1;
    private const int FILTER_OFFSET      = COMPRESSION_OFFSET + 1;
    private const int INTERLACE_OFFSET   = FILTER_OFFSET + 1;

    // ========================================================================

    private PNGUtils()
    {
    }

    // ( Example... )
    //  0 -  7 => 89 50 4E 47 0D 0A 1A 0A                  - PNG Signature (8 bytes)
    //  8 - 11 => 00 00 00 0D                              - IHDR Chunk size (13 bytes)
    // 12 - 15 => 49 48 44 52                              - IHDR Chunk Type (4 bytes - 'I', 'H', 'D', 'R')
    // 16 - 28 => 00 00 00 44 00 00 00 44 08 06 00 00 00   - IHDR Data (Width: 68px, Height: 68px, Bit Depth: 8, Color Type: 6 (RGBA), Compression: 0, Filter: 0, Interlace: 0)
    // 29 - 32 => 38 13 93 B2                              - IHDR CRC (Checksum) (4 bytes)
    // 33 - 36 => 00 00 24 9E                              - IDAT Chunk size (9374 bytes)
    // 37 - 40 => 49 44 41 54                              - IDAT Chunk Type (4 bytes - 'I', 'D', 'A', 'T')
    // ... (IDAT data follows)

    // ========================================================================

    public static PNGSignature PngSignature  { get; private set; }
    public static IHDRChunk    IHDRchunk     { get; private set; }
    public static IDATChunk    IDATchunk     { get; private set; }
    public static long         TotalIDATSize { get; private set; } = 0;

    /// <summary>
    /// </summary>
    /// <param name="filename"></param>
    public static void AnalysePNG( string filename )
    {
        var data = File.ReadAllBytes( filename );

        AnalysePNG( data );
    }

    /// <summary>
    /// </summary>
    /// <param name="pngData"></param>
    /// <param name="showResults"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void AnalysePNG( byte[] pngData, bool showResults = false )
    {
        if ( ( pngData == null ) || ( pngData.Length == 0 ) )
        {
            throw new ArgumentException( "Invalid PNG Data" );
        }

        // --------------------------------------
        // PNG Signature (8 bytes)
        PngSignature = new PNGSignature
        {
            Signature = new byte[ SIGNATURE_LENGTH ],
        };

        Array.Copy( pngData, 0, PngSignature.Signature, 0, SIGNATURE_LENGTH );

        if ( !PngSignature.Signature.SequenceEqual( new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 } ) )
        {
            Logger.Debug( "Not a valid PNG file, Signature is incorrect" );

            return;
        }

        // --------------------------------------
        // IHDR
        var ihdr       = new byte[ IHDR_SIZE ];
        var crc        = new byte[ IHDR_CRC_SIZE ];
        var widthdata  = new byte[ WIDTH_SIZE ];
        var heightdata = new byte[ HEIGHT_SIZE ];

        Array.Copy( pngData, IHDR_START, ihdr, 0, IHDR_SIZE );
        Array.Copy( pngData, IHDR_CRC_START, crc, 0, IHDR_CRC_SIZE );
        Array.Copy( pngData, WIDTH_OFFSET, widthdata, 0, WIDTH_SIZE );
        Array.Copy( pngData, HEIGHT_OFFSET, heightdata, 0, HEIGHT_SIZE );

        IHDRchunk = new IHDRChunk
        {
            Ihdr        = ihdr,
            Width       = ReadBigEndianUInt32( widthdata, WIDTH_SIZE ),
            Height      = ReadBigEndianUInt32( heightdata, HEIGHT_SIZE ),
            BitDepth    = pngData[ BITDEPTH_OFFSET ],
            ColorType   = pngData[ COLORTYPE_OFFSET ],
            Compression = pngData[ COMPRESSION_OFFSET ],
            Filter      = pngData[ FILTER_OFFSET ],
            Interlace   = pngData[ INTERLACE_OFFSET ],
            Crc         = crc,
        };

        // --------------------------------------
        // IDAT
        var tmp = new byte[ IDAT_SIZE ];

        Array.Copy( pngData, IDAT_START, tmp, 0, IDAT_SIZE );

        var tmpChunkSize = ReadBigEndianUInt32( tmp, IDAT_SIZE );

        IDATchunk = new IDATChunk
        {
            ChunkSize = tmpChunkSize,
            ChunkType = tmp,
        };

        TotalIDATSize = CalculateTotalIDATSize( pngData );

        if ( showResults )
        {
            Logger.Debug( $"PNG Signature   : {BitConverter.ToString( PngSignature.Signature ).Replace( "-", " " )}" );
            Logger.Debug( "" );
            Logger.Debug( $"- Width         : {IHDRchunk.Width}" );
            Logger.Debug( $"- Height        : {IHDRchunk.Height}" );
            Logger.Debug( $"- BitDepth      : {IHDRchunk.BitDepth}" );
            Logger.Debug( $"- ColorType     : {IHDRchunk.ColorType}" );
            Logger.Debug( $"- Compression   : {IHDRchunk.Compression}" );
            Logger.Debug( $"- Filter        : {IHDRchunk.Filter}" );
            Logger.Debug( $"- Interlace     : {IHDRchunk.Interlace}" );
            Logger.Debug( $"- Checksum      : {ReadBigEndianUInt32( IHDRchunk.Crc, 4 )}" );
            Logger.Debug( "" );

//            Logger.Debug( $"- IHDR/IDAT Data: {BitConverter.ToString( pngData ).Replace( "-", " " )}" );
            Logger.Debug( $"- TotalIDATSize : {TotalIDATSize}" );
            Logger.Debug( "" );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="pngData"></param>
    /// <returns></returns>
    public static long CalculateTotalIDATSize( byte[] pngData )
    {
        var totalIDATSize = 0L;
        var idatIndex     = 0;

        while ( ( idatIndex = FindChunk( pngData, "IDAT", idatIndex + 1 ) ) != -1 )
        {
            var chunkSize     = ReadIntFromBytes( pngData, idatIndex );
            var fullChunkSize = chunkSize + 12; // Add 12 bytes for type and CRC

            if ( ( idatIndex + fullChunkSize ) > pngData.Length )
            {
                Console.WriteLine( $"Error: Invalid Chunk Size or truncated file. Index: {idatIndex}, Chunksize: {fullChunkSize}, File Length: {pngData.Length}" );

                break;
            }

            totalIDATSize += chunkSize;
            idatIndex     += fullChunkSize - 1; //Correctly increment the index. -1 to account for the +1 in the while loop.
        }

        return totalIDATSize;
    }

    /// <summary>
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="chunkType"></param>
    /// <param name="startIndex"></param>
    /// <returns></returns>
    public static int FindChunk( byte[] bytes, string chunkType, int startIndex = 0 )
    {
        var chunkTypeBytes = Encoding.ASCII.GetBytes( chunkType );

        if ( ( startIndex < 0 ) || ( startIndex > ( bytes.Length - 8 ) ) ) // Corrected end condition
        {
            return -1;
        }

        for ( var i = startIndex; i <= ( bytes.Length - 8 ); i++ )
        {
            if ( ( bytes[ i + 4 ] == chunkTypeBytes[ 0 ] ) &&
                 ( bytes[ i + 5 ] == chunkTypeBytes[ 1 ] ) &&
                 ( bytes[ i + 6 ] == chunkTypeBytes[ 2 ] ) &&
                 ( bytes[ i + 7 ] == chunkTypeBytes[ 3 ] ) )
            {
                return i; // Return the index of the chunk size
            }
        }

        return -1;
    }

    /// <summary>
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="startIndex"></param>
    /// <returns></returns>
    public static int ReadIntFromBytes( byte[] bytes, int startIndex )
    {
        if ( ( startIndex < 0 ) || ( ( startIndex + 3 ) >= bytes.Length ) )
        {
            Console.WriteLine( "Error: ReadIntFromBytes out of bounds" );

            return 0; // Or throw an exception
        }

        return ( bytes[ startIndex ] << 24 )
               | ( bytes[ startIndex + 1 ] << 16 )
               | ( bytes[ startIndex + 2 ] << 8 )
               | bytes[ startIndex + 3 ];
    }

    // Helper function to read Big Endian UInt32
    private static uint ReadBigEndianUInt32( byte[] data, int count )
    {
        var bytes = new byte[ count ];

        Array.Copy( data, 0, bytes, 0, count );

        if ( BitConverter.IsLittleEndian )
        {
            Array.Reverse( bytes );
        }

        return BitConverter.ToUInt32( bytes, 0 );
    }

    // Helper function to read Big Endian UInt32
    private static uint ReadBigEndianUInt32( BinaryReader reader )
    {
        var bytes = reader.ReadBytes( 4 );

        if ( BitConverter.IsLittleEndian )
        {
            Array.Reverse( bytes );
        }

        return BitConverter.ToUInt32( bytes, 0 );
    }

    // Parse the IHDR data to Logger Debug output.
    private static void ParseIHDR( byte[] data )
    {
        if ( data.Length != 13 )
        {
            Logger.Debug( "IHDR chunk is an unexpected size" );

            return;
        }

        var bitDepth  = data[ 8 ];
        var colorType = data[ 9 ];

        Logger.Debug( "IHDR Data Breakdown:" );
        Logger.Debug( $"Width: {ReadBigEndianUInt32( new BinaryReader( new MemoryStream( data.Take( 4 ).ToArray() ) ) )} pixels" );
        Logger.Debug( $"Height: {ReadBigEndianUInt32( new BinaryReader( new MemoryStream( data.Skip( 4 ).Take( 4 ).ToArray() ) ) )} pixels" );
        Logger.Debug( $"Bit Depth: {bitDepth}" );
        Logger.Debug( $"Color Type: {colorType}, {ColorTypeName( colorType )}" );
        var colorFormat = DetermineColorFormat( colorType, bitDepth );
        Logger.Debug( "Color Format: " + colorFormat );
        Logger.Debug( $"Compression Method: {data[ 10 ]}" );
        Logger.Debug( $"Filter Method: {data[ 11 ]}" );
        Logger.Debug( $"Interlace Method: {data[ 12 ]}" );
    }

    /// <summary>
    /// </summary>
    /// <param name="colorType"></param>
    /// <param name="bitDepth"></param>
    /// <returns></returns>
    public static int GetBytesPerPixel( int colorType, int bitDepth )
    {
        return colorType switch
        {
            // Grayscale
            0 => bitDepth == 8 ? 1 : bitDepth == 16 ? 2 : -1,

            // RGB
            2 => bitDepth == 8 ? 3 : bitDepth == 16 ? 6 : -1,

            // Indexed-color
            // Indexed color uses a palette
            3 => 1,

            // Grayscale with alpha
            4 => bitDepth == 8 ? 2 : bitDepth == 16 ? 4 : -1,

            // RGBA
            6 => bitDepth == 8 ? 4 : bitDepth == 16 ? 8 : -1,

            var _ => -1,
        };
    }

    /// <summary>
    /// </summary>
    /// <param name="colorType"></param>
    /// <param name="bitDepth"></param>
    /// <returns></returns>
    public static string DetermineColorFormat( byte colorType, byte bitDepth )
    {
        return colorType switch
        {
            0 => $"Grayscale {bitDepth}-bit",

            // --------------------------------------------
            2 => bitDepth switch
            {
                8     => "RGB888",
                16    => "RGB161616",
                var _ => $"Truecolor {bitDepth}-bit",
            },

            // --------------------------------------------
            3 => $"Indexed {bitDepth}-bit",

            // --------------------------------------------
            4 => bitDepth switch
            {
                8     => "Grayscale with Alpha 88",
                16    => "Grayscale with Alpha 1616",
                var _ => $"Grayscale with Alpha {bitDepth}{bitDepth}",
            },

            // --------------------------------------------
            6 => bitDepth switch
            {
                8     => "RGBA8888",
                16    => "RGBA16161616",
                var _ => $"Truecolor with Alpha {bitDepth}{bitDepth}{bitDepth}{bitDepth}",
            },

            // --------------------------------------------
            var _ => "Unknown Color Format",
        };
    }

    /// <summary>
    /// Returns a string representation of the Color Type for this PNG, which is held at
    /// offset 25 into the 41-byte Signature/IHDR/IDAT Png file header.
    /// </summary>
    /// <seealso cref="AnalysePNG(byte[], bool)" />
    public static string ColorTypeName( int colortype )
    {
        return colortype switch
        {
            0 => "Grayscale - Allowed Bit Depths: 1,2,4,8,16",
            2 => "Truecolor - Allowed Bit Depths: 8,16",
            3 => "Indexed Color - Allowed Bit Depths: 1,2,4,8",
            4 => "Grayscale with Alpha - Allowed Bit Depths: 8,16",
            6 => "Truecolor with Alpha - Allowed Bit Depths: 8,16",

            var _ => $"Unknow colortype: {colortype}",
        };
    }

    /// <summary>
    /// Extracts the <c>Width</c> and <c>Height</c> from a PNG file.
    /// </summary>
    public static ( int width, int height ) GetPNGDimensions( FileInfo file )
    {
        if ( file.Extension.ToLower() != ".png" )
        {
            throw new GdxRuntimeException( $"PNG files ONLY!: ({file.Name})" );
        }

        var widthbytes  = new byte[ sizeof( int ) ];
        var heightbytes = new byte[ sizeof( int ) ];

        var br = new BinaryReader( File.OpenRead( file.Name ) );
        br.BaseStream.Position = 16;

        for ( var i = 0; i < sizeof( int ); i++ )
        {
            widthbytes[ sizeof( int ) - 1 - i ] = br.ReadByte();
        }

        for ( var i = 0; i < sizeof( int ); i++ )
        {
            heightbytes[ sizeof( int ) - 1 - i ] = br.ReadByte();
        }

        return ( BitConverter.ToInt32( widthbytes, 0 ), BitConverter.ToInt32( heightbytes, 0 ) );
    }

    // ========================================================================

    /// <summary>
    /// The PNG signature is eight bytes in length and contains information
    /// used to identify a file or data stream as conforming to the PNG
    /// specification.
    /// </summary>
    [PublicAPI]
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public struct PNGSignature
    {
        public byte[] Signature { get; set; } // Identifier (always 0x89504E470D0A1A0A)
    }

    /// <summary>
    /// PNG File IHDR Structure. The header chunk contains information on the image data
    /// stored in the PNG file. This chunk must be the first chunk in a PNG data stream
    /// and immediately follows the PNG signature. The header chunk data area is 13 bytes
    /// in length.
    /// </summary>
    [PublicAPI]
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public struct IHDRChunk
    {
        public byte[] Ihdr        { get; set; } // 'I', 'H', 'D', 'R'
        public uint   Width       { get; set; } // Width of image in pixels
        public uint   Height      { get; set; } // Height of image in pixels
        public byte   BitDepth    { get; set; } // Bits per pixel or per sample
        public byte   ColorType   { get; set; } // Color interpretation indicator
        public byte   Compression { get; set; } // Compression type indicator
        public byte   Filter      { get; set; } // Filter type indicator
        public byte   Interlace   { get; set; } // Type of interlacing scheme used
        public byte[] Crc         { get; set; } // The CRC for IHDR
    }

    /// <summary>
    /// PNG File IDAT Chunk Structure.
    /// </summary>
    [PublicAPI]
    [StructLayout( LayoutKind.Sequential, Pack = 1 )] // Important for correct byte alignment
    public struct IDATChunk
    {
        public uint   ChunkSize { get; set; } // 4 bytes (unsigned int)
        public byte[] ChunkType { get; set; } // 4 bytes 'I', 'D', 'A', 'T'
    }
}