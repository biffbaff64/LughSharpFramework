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

namespace LughSharp.Lugh.Graphics.Utils;

[PublicAPI]
public class PNGDecoder
{
    public static readonly byte[] StandardPNGSignature =
    [
        // DO NOT CHANGE THESE VALUES!
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
    ];

    public const int SIGNATURE_LENGTH       = 8;
    public const int IHDR_START             = 8;
    public const int IHDR_SIZE              = 4;
    public const int IHDR_CHUNK_TYPE_OFFSET = IHDR_START + IHDR_SIZE;
    public const int IHDR_CHUNK_TYPE_SIZE   = 4;
    public const int IHDR_DATA_OFFSET       = IHDR_CHUNK_TYPE_OFFSET + IHDR_CHUNK_TYPE_SIZE;
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
    // Offsets into PNG data starting at IHDR_DATA_OFFSET

    private const int WIDTH_OFFSET       = 0;
    private const int WIDTH_SIZE         = sizeof( uint );
    private const int HEIGHT_OFFSET      = WIDTH_OFFSET + WIDTH_SIZE;
    private const int HEIGHT_SIZE        = sizeof( uint );
    private const int BITDEPTH_OFFSET    = HEIGHT_OFFSET + HEIGHT_SIZE;
    private const int COLORTYPE_OFFSET   = BITDEPTH_OFFSET + 1;
    private const int COMPRESSION_OFFSET = COLORTYPE_OFFSET + 1;
    private const int FILTER_OFFSET      = COMPRESSION_OFFSET + 1;
    private const int INTERLACE_OFFSET   = FILTER_OFFSET + 1;

    // ========================================================================

    private PNGDecoder()
    {
    }

    // ( Example... )
    //  0 -  7 => 89 50 4E 47 0D 0A 1A 0A                  - PNG Signature (8 bytes)
    //  8 - 11 => 00 00 00 0D                              - IHDR Chunk size (4 bytes)
    // 12 - 15 => 49 48 44 52                              - IHDR Chunk Type (4 bytes - 'I', 'H', 'D', 'R')
    // 16 - 28 => 00 00 00 44 00 00 00 44 08 06 00 00 00   - IHDR Data (Width: 68px, Height: 68px, Bit Depth: 8, Color Type: 6 (RGBA), Compression: 0, Filter: 0, Interlace: 0)
    // 29 - 32 => 38 13 93 B2                              - IHDR CRC (Checksum) (4 bytes)
    // 33 - 36 => 00 00 24 9E                              - IDAT Chunk size (9374 bytes)
    // 37 - 40 => 49 44 41 54                              - IDAT Chunk Type (4 bytes - 'I', 'D', 'A', 'T')
    // ... (IDAT data follows)

    // ========================================================================

    public static PNGFormatStructs.PNGSignature PngSignature  { get; private set; }
    public static PNGFormatStructs.IHDRChunk    IHDRchunk     { get; private set; }
    public static PNGFormatStructs.IDATChunk    IDATchunk     { get; private set; }
    public static long                          TotalIDATSize { get; private set; } = 0;
    public static int                           PixelFormat   { get; private set; }
    public static int                           BytesPerPixel { get; private set; }

    public static byte BitDepth    => IHDRchunk.BitDepth;
    public static uint Width       => IHDRchunk.Width;
    public static uint Height      => IHDRchunk.Height;
    public static byte ColorType   => IHDRchunk.ColorType;
    public static byte Compression => IHDRchunk.Compression;
    public static byte Filter      => IHDRchunk.Filter;
    public static byte Interlace   => IHDRchunk.Interlace;

    // ========================================================================

    /// <summary>
    /// Analyzes the specified PNG file to extract metadata, including details such
    /// as dimensions, bit depth, color type, and interlace method, while optionally
    /// displaying the results.
    /// </summary>
    /// <param name="filename">The file path of the PNG image to analyze.</param>
    /// <param name="verbose">Specifies whether to display the analysis results in the output.</param>
    /// <exception cref="ArgumentNullException">Thrown if the provided filename is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the specified file does not exist.</exception>
    /// <exception cref="ArgumentException">Thrown if the file format is invalid or not a valid PNG.</exception>
    public static void AnalysePNG( string filename, bool verbose = false )
    {
        Logger.Checkpoint();

        AnalysePNG( File.ReadAllBytes( filename ), verbose );
    }

    /// <summary>
    /// Analyzes PNG image data to extract metadata such as width, height, bit depth, color type,
    /// compression method, filter method, interlace method, and other structural components of
    /// the PNG format.
    /// </summary>
    /// <param name="pngData">A byte array containing the PNG file data to be analyzed.</param>
    /// <param name="verbose">
    /// Indicates whether to display the analysis results after processing.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if the input byte array is null or empty, or if the PNG data is invalid.
    /// </exception>
    public static void AnalysePNG( byte[] pngData, bool verbose = false )
    {
        Logger.Checkpoint();

        if ( ( pngData == null ) || ( pngData.Length == 0 ) )
        {
            throw new ArgumentException( "Invalid PNG Data" );
        }

        // --------------------------------------
        // PNG Signature (8 bytes)
        PngSignature = new PNGFormatStructs.PNGSignature
        {
            Signature = new byte[ SIGNATURE_LENGTH ],
        };

        Array.Copy( pngData, 0, PngSignature.Signature, 0, SIGNATURE_LENGTH );

        if ( !PngSignature.Signature.SequenceEqual( StandardPNGSignature ) )
        {
            Logger.Warning( "Not a valid PNG file, Signature is incorrect" );

            return;
        }

        // --------------------------------------
        // IHDR
        var ihdr         = new byte[ IHDR_SIZE ];
        var ihdrTypeData = new byte[ IHDR_CHUNK_TYPE_SIZE ];
        var crc          = new byte[ IHDR_CRC_SIZE ];
        var widthData    = new byte[ WIDTH_SIZE ];
        var heightData   = new byte[ HEIGHT_SIZE ];

        Array.Copy( pngData, IHDR_START, ihdr, 0, IHDR_SIZE );
        Array.Copy( pngData, IHDR_CHUNK_TYPE_OFFSET, ihdrTypeData, 0, IHDR_CHUNK_TYPE_SIZE );
        Array.Copy( pngData, IHDR_CRC_START, crc, 0, IHDR_CRC_SIZE );
        Array.Copy( pngData, IHDR_DATA_OFFSET + WIDTH_OFFSET, widthData, 0, WIDTH_SIZE );
        Array.Copy( pngData, IHDR_DATA_OFFSET + HEIGHT_OFFSET, heightData, 0, HEIGHT_SIZE );

        IHDRchunk = new PNGFormatStructs.IHDRChunk
        {
            Ihdr        = ihdr,
            IhdrType    = ihdrTypeData,
            Width       = ReadBigEndianUInt32( widthData, WIDTH_SIZE ),
            Height      = ReadBigEndianUInt32( heightData, HEIGHT_SIZE ),
            BitDepth    = pngData[ IHDR_DATA_OFFSET + BITDEPTH_OFFSET ],
            ColorType   = pngData[ IHDR_DATA_OFFSET + COLORTYPE_OFFSET ],
            Compression = pngData[ IHDR_DATA_OFFSET + COMPRESSION_OFFSET ],
            Filter      = pngData[ IHDR_DATA_OFFSET + FILTER_OFFSET ],
            Interlace   = pngData[ IHDR_DATA_OFFSET + INTERLACE_OFFSET ],
            Crc         = crc,
        };

        // --------------------------------------
        // IDAT
        var tmp = new byte[ IDAT_SIZE ];

        Array.Copy( pngData, IDAT_START, tmp, 0, IDAT_SIZE );

        var tmpChunkSize = ReadBigEndianUInt32( tmp, IDAT_SIZE );

        IDATchunk = new PNGFormatStructs.IDATChunk
        {
            ChunkSize = tmpChunkSize,
            ChunkType = tmp,
        };

        TotalIDATSize = CalculateTotalIDATSize( pngData );

        // Pixel Format
        PixelFormat = GetFormatFromPngHeader( new MemoryStream( pngData ) );

        // Bytes Per Pixel
        BytesPerPixel = GetBytesPerPixel( IHDRchunk.ColorType, IHDRchunk.BitDepth );

        if ( verbose )
        {
            Logger.Debug( $"PNG Signature   : {BitConverter.ToString( PngSignature.Signature ).Replace( "-", " " )}" );
            Logger.Debug( "-----------------------------" );
            Logger.Debug( $"IHDR            : {BitConverter.ToString( IHDRchunk.Ihdr ).Replace( "-", " " )}" );
            Logger.Debug( $"IHDR TYPE       : {BitConverter.ToString( IHDRchunk.IhdrType ).Replace( "-", " " )}" );
            Logger.Debug( "-----------------------------" );
            Logger.Debug( $"- Width         : {IHDRchunk.Width}" );
            Logger.Debug( $"- Height        : {IHDRchunk.Height}" );
            Logger.Debug( $"- BitDepth      : {IHDRchunk.BitDepth}" );
            Logger.Debug( $"- ColorType     : {ColorTypeName( IHDRchunk.ColorType )} :: ( {IHDRchunk.ColorType} )" );
            Logger.Debug( $"- PixelFormat   : {PixelFormatUtils.GetFormatString( PixelFormat )} :: ( {PixelFormat} )" );
            Logger.Debug( $"- Compression   : {IHDRchunk.Compression}" );
            Logger.Debug( $"- Filter        : {IHDRchunk.Filter}" );
            Logger.Debug( $"- Interlace     : {IHDRchunk.Interlace}" );
            Logger.Debug( $"- Checksum      : 0x{ReadBigEndianUInt32( IHDRchunk.Crc, 4 ):X}" );
            Logger.Debug( "-----------------------------" );

//            Logger.Debug( $"- IHDR/IDAT Data: {BitConverter.ToString( pngData ).Replace( "-", " " )}" );
            Logger.Debug( $"- TotalIDATSize : 0x{TotalIDATSize:X}" );
            Logger.Debug( "-----------------------------" );
        }
    }

    /// <summary>
    /// Calculates the total size of all IDAT chunks in the provided PNG data.
    /// </summary>
    /// <param name="pngData">The byte array containing the PNG file data.</param>
    /// <returns>The total size of all IDAT chunks in bytes.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="pngData"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="pngData"/> has an invalid or corrupted format.
    /// </exception>
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
                Logger.Warning( $"Error: Invalid Chunk Size or truncated file. " +
                                $"Index: {idatIndex}, " +
                                $"Chunksize: {fullChunkSize}, " +
                                $"File Length: {pngData.Length}" );

                break;
            }

            totalIDATSize += chunkSize;
            idatIndex     += fullChunkSize - 1; //Correctly increment the index. -1 to account for the +1 in the while loop.
        }

        return totalIDATSize;
    }

    /// <summary>
    /// Parses the provided byte array containing IHDR chunk data to extract information such
    /// as image dimensions, bit depth, color type, compression method, filter method, and
    /// interlace method. The extracted details are logged for debugging purposes.
    /// </summary>
    /// <param name="data">
    /// The byte array containing the IHDR chunk data. Expected to be 13 bytes in length.
    /// </param>
    /// <param name="verbose"></param>
    /// <exception cref="ArgumentException">
    /// Thrown if the provided byte array is not the correct length.
    /// </exception>
    private static void ParseIHDR( byte[] data, bool verbose = false )
    {
        if ( data.Length != IHDR_DATA_SIZE )
        {
            Logger.Warning( "IHDR chunk is an unexpected size" );

            return;
        }

        if ( verbose )
        {
            var bitDepth    = data[ BITDEPTH_OFFSET ];
            var colorType   = data[ COLORTYPE_OFFSET ];
            var colorFormat = DetermineColorFormat( ColorType, bitDepth );

            Logger.Debug( "IHDR Data Breakdown:" );
            Logger.Debug( $"Width: {ReadBigEndianUInt32( new BinaryReader( new MemoryStream( data.Take( 4 ).ToArray() ) ) )} pixels" );
            Logger.Debug( $"Height: {ReadBigEndianUInt32( new BinaryReader( new MemoryStream( data.Skip( 4 ).Take( 4 ).ToArray() ) ) )} pixels" );
            Logger.Debug( $"Bit Depth: {data[ BITDEPTH_OFFSET ]}" );
            Logger.Debug( $"Color Type: {colorType}, {ColorTypeName( colorType )}" );
            Logger.Debug( $"Color Format: {colorFormat}" );
            Logger.Debug( $"Compression Method: {data[ COMPRESSION_OFFSET ]}" );
            Logger.Debug( $"Filter Method: {data[ FILTER_OFFSET ]}" );
            Logger.Debug( $"Interlace Method: {data[ INTERLACE_OFFSET ]}" );
        }
    }

    /// <summary>
    /// Searches for the specified PNG chunk type in the provided byte array, starting at the given index.
    /// </summary>
    /// <param name="bytes">The byte array containing the PNG data to search.</param>
    /// <param name="chunkType">
    /// The four-character ASCII string representing the chunk type (e.g., "IHDR", "IDAT").
    /// </param>
    /// <param name="startIndex">The index in the byte array from which to start the search.</param>
    /// <returns>
    /// The index of the located chunk within the byte array, or -1 if the chunk is not found
    /// or the start index is invalid.
    /// </returns>
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
    /// Reads a 32-bit integer from a byte array starting at the specified index.
    /// The integer is interpreted as big-endian, meaning the most significant
    /// byte is stored first.
    /// </summary>
    /// <param name="bytes">The byte array from which to read the integer.</param>
    /// <param name="startIndex">
    /// The index in the byte array at which to start reading. Must be within the
    /// valid range of the array.</param>
    /// <returns>The 32-bit integer value read from the byte array.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the start index is out of bounds or if there are not enough bytes
    /// remaining in the array to read a full integer.
    /// </exception>
    public static int ReadIntFromBytes( byte[] bytes, int startIndex )
    {
        if ( ( startIndex < 0 ) || ( ( startIndex + 3 ) >= bytes.Length ) )
        {
            Logger.Warning( "Error: ReadIntFromBytes out of bounds" );

            return 0; // Or throw an exception
        }

        return ( bytes[ startIndex ] << 24 )
               | ( bytes[ startIndex + 1 ] << 16 )
               | ( bytes[ startIndex + 2 ] << 8 )
               | bytes[ startIndex + 3 ];
    }

    /// <summary>
    /// Reads a 32-bit unsigned integer from a byte array in big-endian format.
    /// </summary>
    /// <param name="data">
    /// The byte array containing the data to be interpreted as a 32-bit unsigned integer.
    /// </param>
    /// <param name="count">
    /// The number of bytes to read from the array. This value must not exceed the length
    /// of the array.
    /// </param>
    /// <returns>
    /// Returns the 32-bit unsigned integer value derived from the specified byte array in
    /// big-endian format.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when the input byte array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the specified count exceeds the length of the byte array.
    /// </exception>
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

    /// <summary>
    /// Reads a 32-bit unsigned integer from a binary reader, assuming big-endian
    /// byte order. Converts the order to suit the local system if necessary.
    /// </summary>
    /// <param name="reader">
    /// The BinaryReader instance from which to read the 32-bit unsigned integer.
    /// </param>
    /// <returns>The 32-bit unsigned integer read from the binary stream.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the provided BinaryReader instance is null.
    /// </exception>
    /// <exception cref="EndOfStreamException">
    /// Thrown if there are fewer than 4 bytes remaining in the stream.
    /// </exception>
    private static uint ReadBigEndianUInt32( BinaryReader reader )
    {
        var bytes = reader.ReadBytes( 4 );

        if ( BitConverter.IsLittleEndian )
        {
            Array.Reverse( bytes );
        }

        return BitConverter.ToUInt32( bytes, 0 );
    }

    /// <summary>
    /// Calculates the number of bytes per pixel for a given PNG color type and bit depth.
    /// </summary>
    /// <param name="colorType">
    /// The color type of the PNG, representing the format of pixel data (e.g., grayscale,
    /// RGB, RGBA).
    /// </param>
    /// <param name="bitDepth">
    /// The bit depth of the image, indicating the number of bits used per channel in
    /// the pixel data.
    /// </param>
    /// <returns>
    /// The number of bytes per pixel. Returns -1 if the color type or bit depth is
    /// unsupported.
    /// </returns>
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
    /// Gets the pixel format ( RGBA8888, RGB565, etc. ) from the provided
    /// PNG image data.
    /// </summary>
    /// <param name="pngStream"> A Stream with which to access the data. </param>
    /// <returns>
    /// A Gdx2DPixmapFormat enum value representing the pixel format of the PNG image.
    /// </returns>
    public static int GetFormatFromPngHeader( Stream pngStream )
    {
        // Read PNG signature (8 bytes)
        var signature = new byte[ 8 ];

        pngStream.ReadExactly( signature, 0, 8 );

        // Read IHDR chunk length (4 bytes) and type (4 bytes)
        pngStream.ReadExactly( new byte[ 4 ], 0, 4 ); // length
        var type = new byte[ 4 ];
        pngStream.ReadExactly( type, 0, 4 );

        // Ensure it's IHDR
        if ( Encoding.ASCII.GetString( type ) != "IHDR" )
        {
            throw new Exception( "Invalid PNG: Missing IHDR" );
        }

        // Read IHDR data (13 bytes)
        var ihdr = new byte[ 13 ];
        pngStream.ReadExactly( ihdr, 0, 13 );

        var bitDepth  = ihdr[ 8 ];
        var colorType = ihdr[ 9 ];

        // Map PNG color type and bit depth to the format
//        var format = ( colorType, bitDepth ) switch
//        {
//            (6, 8) => Gdx2DPixmap.GDX_2D_FORMAT_RGBA8888,        // Truecolor with alpha, 8 bits
//            (2, 8) => Gdx2DPixmap.GDX_2D_FORMAT_RGB888,          // Truecolor, 8 bits
//            (0, 8) => Gdx2DPixmap.GDX_2D_FORMAT_ALPHA,           // Grayscale, 8 bits
//            (4, 8) => Gdx2DPixmap.GDX_2D_FORMAT_LUMINANCE_ALPHA, // Grayscale with alpha, 8 bits
//            (2, 5) => Gdx2DPixmap.GDX_2D_FORMAT_RGB565,          // Not standard, but logic can be added for 16-bit
//
//            // Add more mappings as needed
//            var _ => throw new Exception( $"Unsupported PNG colorType {colorType} and bitDepth {bitDepth}" ),
//        };

        var format = PixelFormatUtils.DeterminePixelFormatFromBitDepth( colorType, bitDepth );

        return format;
    }

    /// <summary>
    /// Determines the color format of a PNG image based on the provided color type and bit depth.
    /// </summary>
    /// <param name="colorType">The color type value extracted from the PNG image's IHDR chunk.</param>
    /// <param name="bitDepth">The bit depth value extracted from the PNG image's IHDR chunk.</param>
    /// <returns>
    /// A string representing the color format, such as "RGB888" or "Grayscale with Alpha 88".
    /// If the color format cannot be determined, returns "Unknown Color Format".
    /// </returns>
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
        if ( !file.Extension.Equals( ".png", StringComparison.CurrentCultureIgnoreCase ) )
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

        return ( BitConverter.ToInt32( widthbytes, 0 ),
            BitConverter.ToInt32( heightbytes, 0 ) );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rawRgba"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    public static byte[] CreatePNGFromRawRGBA( byte[] rawRgba,
                                               int width,
                                               int height,
                                               int format )
    {
        ArgumentNullException.ThrowIfNull( rawRgba );

        if ( rawRgba.Length != ( width * height * 4 ) )
        {
            throw new ArgumentException( "Input data size does not match width*height*4" );
        }

        using var ms = new MemoryStream();

        // PNG signature
        ms.Write( StandardPNGSignature );

        // IHDR chunk
        using ( var ihdr = new MemoryStream() )
        {
            WriteBigEndian( ihdr, width );
            WriteBigEndian( ihdr, height );

            var colorType = ( byte )format;

            ihdr.WriteByte( 8 );         // bit depth
            ihdr.WriteByte( colorType ); // color type: RGBA
            ihdr.WriteByte( 0 );         // compression
            ihdr.WriteByte( 0 );         // filter
            ihdr.WriteByte( 0 );         // interlace

            WriteChunk( ms, "IHDR", ihdr.ToArray() );
        }

        // Prepare image data with filter bytes
        var scanlineLen = ( width * 4 ) + 1;
        var imageData   = new byte[ scanlineLen * height ];

        for ( var y = 0; y < height; y++ )
        {
            imageData[ y * scanlineLen ] = 0; // filter type 0 (None)
            Buffer.BlockCopy( rawRgba, y * width * 4, imageData, ( y * scanlineLen ) + 1, width * 4 );
        }

        // Compress image data (zlib)
        byte[] compressed;

        using ( var comp = new MemoryStream() )
        {
            using ( var deflate = new ZLibStream( comp, CompressionLevel.Optimal, true ) )
            {
                deflate.Write( imageData, 0, imageData.Length );
            }

            compressed = comp.ToArray();
        }

        // IDAT chunk
        WriteChunk( ms, "IDAT", compressed );

        // IEND chunk
        WriteChunk( ms, "IEND", [ ] );

        return ms.ToArray();

        // --- Local helpers ---
        static void WriteBigEndian( Stream s, int v )
        {
            s.WriteByte( ( byte )( ( v >> 24 ) & 0xFF ) );
            s.WriteByte( ( byte )( ( v >> 16 ) & 0xFF ) );
            s.WriteByte( ( byte )( ( v >> 8 ) & 0xFF ) );
            s.WriteByte( ( byte )( v & 0xFF ) );
        }

        static void WriteChunk( Stream s, string type, byte[] data )
        {
            WriteBigEndian( s, data.Length );
            var typeBytes = Encoding.ASCII.GetBytes( type );
            s.Write( typeBytes, 0, 4 );
            s.Write( data, 0, data.Length );

            // CRC
            using var crc32 = new Crc32();

            crc32.TransformBlock( typeBytes, 0, 4, null, 0 );
            crc32.TransformBlock( data, 0, data.Length, null, 0 );
            crc32.TransformFinalBlock( [ ], 0, 0 );

            if ( crc32.Hash != null )
            {
                WriteBigEndian( s, BitConverter.ToInt32( crc32.Hash.Reverse().ToArray(), 0 ) );
            }
        }
    }

    /// <summary>
    /// CRC32 implementation (minimal)
    /// </summary>
    [PublicAPI]
    public class Crc32 : HashAlgorithm
    {
        private uint _crc = 0xFFFFFFFF;

        private static readonly uint[] _table = Enumerable.Range( 0, 256 ).Select( i =>
        {
            var c = ( uint )i;

            for ( var k = 0; k < 8; k++ )
            {
                c = ( c & 1 ) != 0 ? 0xEDB88320 ^ ( c >> 1 ) : c >> 1;
            }

            return c;
        } ).ToArray();

        public override void Initialize()
        {
            _crc = 0xFFFFFFFF;
        }

        public override int HashSize => 32;

        protected override void HashCore( byte[] array, int ibStart, int cbSize )
        {
            for ( var i = ibStart; i < ( ibStart + cbSize ); i++ )
            {
                _crc = _table[ ( byte )( _crc ^ array[ i ] ) ] ^ ( _crc >> 8 );
            }
        }

        protected override byte[] HashFinal()
        {
            var hash = BitConverter.GetBytes( ~_crc );

            if ( BitConverter.IsLittleEndian )
            {
                Array.Reverse( hash );
            }

            return hash;
        }
    }
}

// ============================================================================
// ============================================================================