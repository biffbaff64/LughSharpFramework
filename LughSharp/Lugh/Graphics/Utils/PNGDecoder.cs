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

    public static readonly byte[] StandardPNGSignature =
    [
        // DO NOT CHANGE THESE VALUES!
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
    ];

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

    public static PNGFormatStructs.PNGSignature PngSignature   { get; private set; }
    public static PNGFormatStructs.IHDRChunk    IHDRchunk      { get; private set; }
    public static PNGFormatStructs.IDATChunk    IDATchunk      { get; private set; }
    public static long                          TotalIDATSize  { get; private set; } = 0;
    public static int                           PNGPixelFormat { get; private set; }
    public static int                           BytesPerPixel  { get; private set; }

    // ========================================================================

    public static byte BitDepth    => IHDRchunk.BitDepth;
    public static uint Width       => IHDRchunk.Width;
    public static uint Height      => IHDRchunk.Height;
    public static byte ColorType   => IHDRchunk.ColorType;
    public static byte Compression => IHDRchunk.Compression;
    public static byte Filter      => IHDRchunk.Filter;
    public static byte Interlace   => IHDRchunk.Interlace;

    // ========================================================================

    private PNGDecoder()
    {
    }

    /// <summary>
    /// Analyzes the specified <see cref="Texture"/> image to extract metadata, including
    /// details such as dimensions, bit depth, color type, and interlace method, while
    /// optionally displaying the results.
    /// The extracted details are logged for debugging purposes if the <paramref name="verbose"/>
    /// is set to true.
    /// This method will return, after logging a warning, if the provided texture is null
    /// or if the texture has no image data.
    /// </summary>
    /// <param name="texture">The <see cref="Texture"/> image to analyze.</param>
    /// <param name="verbose">Specifies whether to display the analysis results in the output.</param>
    public static void AnalysePNG( Texture? texture, bool verbose = false )
    {
        if ( texture == null )
        {
            throw new GdxRuntimeException( "Unable to perform analysis, texture is null" );
        }

        var data = CreatePNGFromTexture( texture );

        if ( data == null )
        {
            throw new GdxRuntimeException( "WARNING: Unable to analyse Texture. No image data found." );
        }

        AnalysePNG( data, verbose );
    }

    /// <summary>
    /// Analyzes the specified PNG file to extract metadata, including details such
    /// as dimensions, bit depth, color type, and interlace method, while optionally
    /// displaying the results.
    /// The extracted details are logged for debugging purposes if the <paramref name="verbose"/>
    /// is set to true.
    /// </summary>
    /// <param name="filename">The file path of the PNG image to analyze.</param>
    /// <param name="verbose">Specifies whether to display the analysis results in the output.</param>
    /// <exception cref="ArgumentNullException">Thrown if the provided filename is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the specified file does not exist.</exception>
    /// <exception cref="ArgumentException">Thrown if the file format is invalid or not a valid PNG.</exception>
    public static void AnalysePNG( string filename, bool verbose = false )
    {
        AnalysePNG( File.ReadAllBytes( filename ), verbose );
    }

    /// <summary>
    /// Analyzes PNG image data to extract metadata such as width, height, bit depth, color type,
    /// compression method, filter method, interlace method, and other structural components of
    /// the PNG format.
    /// The extracted details are logged for debugging purposes if the <paramref name="verbose"/>
    /// is set to true.
    /// Once completed, the PNG data is written to the specified file path as a valid PNG image file. 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="pngData"></param>
    /// <param name="verbose"></param>
    public static void AnalyseAndWritePNG( string path, byte[] pngData, bool verbose = false )
    {
        AnalysePNG( pngData, verbose );

        WritePNGToFile( pngData, path );
    }

    /// <summary>
    /// Analyzes PNG image data to extract metadata such as width, height, bit depth, color type,
    /// compression method, filter method, interlace method, and other structural components of
    /// the PNG format.
    /// The extracted details are logged for debugging purposes if the <paramref name="verbose"/>
    /// is set to true.
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
        if ( ( pngData == null ) || ( pngData.Length == 0 ) )
        {
            throw new ArgumentException( "Invalid PNG Data" );
        }

        // --------------------------------------
        // PNG Signature (8 bytes)
        // --------------------------------------

        PngSignature = ExtractSignature( pngData );

        if ( PngSignature.Signature.Length <= 0 )
        {
            return;
        }

        // --------------------------------------
        // IHDR
        // --------------------------------------

        IHDRchunk = ExtractIHDR( pngData );

        // --------------------------------------
        // IDAT
        // --------------------------------------

        IDATchunk     = ExtractIDAT( pngData );
        TotalIDATSize = CalculateTotalIDATSize( pngData );

        // --------------------------------------
        // ANALYSIS
        // --------------------------------------

        var colorType = IHDRchunk.ColorType;
        var bitDepth  = IHDRchunk.BitDepth;

        BytesPerPixel  = GetBytesPerPixel( colorType, bitDepth );
        PNGPixelFormat = LughSharp.Lugh.Graphics.PixelFormat.FromPNGColorAndBitDepth( colorType, bitDepth );

        Logger.Debug( $"colorType    : {colorType}" );
        Logger.Debug( $"bitDepth     : {bitDepth}" );
        Logger.Debug( $"BytesPerPixel: {BytesPerPixel}" );
        Logger.Debug( $"PixelFormat  : {PNGPixelFormat}" );

        if ( PNGPixelFormat == LughFormat.INVALID )
        {
            ImageUtils.RejectInvalidImage( ImageUtils.RejectionReason.ColorTypeBitDepthMismatch,
                                           $"BitDepth: {bitDepth}, ColorType: {colorType}." );
        }

        if ( verbose )
        {
            OutputAnalysis( pngData );
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
                Logger.Error( $"Error: Invalid Chunk Size or truncated file. " +
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
            Logger.Error( "IHDR chunk is an unexpected size" );

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
            if ( ( bytes[ i + 5 ] == chunkTypeBytes[ 1 ] )
                 && ( bytes[ i + 4 ] == chunkTypeBytes[ 0 ] )
                 && ( bytes[ i + 6 ] == chunkTypeBytes[ 2 ] )
                 && ( bytes[ i + 7 ] == chunkTypeBytes[ 3 ] ) )
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
            Logger.Error( "Error: ReadIntFromBytes out of bounds" );

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
    /// Returns <c>true</c> if the provided byte array represents
    /// a valid PNG file based on its signature.
    /// <para>
    /// The PNG signature is 8 bytes long and consists of the following bytes:
    /// <li>89 50 4E 47 0D 0A 1A 0A</li>
    /// </para>
    /// If the first 8 bytes of the provided byte array do not match the PNG
    /// signature, then the provided byte array is not a valid PNG file.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static bool IsPNG( byte[] data )
    {
        var signature = new byte[ SIGNATURE_LENGTH ];

        Array.Copy( data, 0, signature, 0, SIGNATURE_LENGTH );

        return signature.SequenceEqual( StandardPNGSignature );
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
        Logger.Debug( $"colorType: {colorType}" );
        Logger.Debug( $"bitDepth : {bitDepth}" );

        return colorType switch
        {
            // Grayscale
            0 => bitDepth switch
            {
                8     => 1,
                16    => 2,
                var _ => -1,
            },

            // RGB
            2 => bitDepth switch
            {
                8     => 3,
                16    => 6,
                var _ => -1,
            },

            // Indexed-color ( Indexed color uses a palette )
            3 => 1,

            // Grayscale with alpha
            4 => bitDepth switch
            {
                8     => 2,
                16    => 4,
                var _ => -1,
            },

            // RGBA
            6 => bitDepth switch
            {
                8     => 4,
                16    => 8,
                var _ => -1,
            },

            // ----------------------------------

            var _ => -1,
        };
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
    /// <seealso cref="AnalysePNG(byte[], bool)"/>
    public static string ColorTypeName( int colortype )
    {
        return colortype switch
        {
            0 => "Grayscale - Allowed Bit Depths: 1,2,4,8,16",
            2 => "Truecolor - Allowed Bit Depths: 8,16",
            3 => "Indexed Color - Allowed Bit Depths: 1,2,4,8",
            4 => "Grayscale with Alpha - Allowed Bit Depths: 8,16",
            6 => "Truecolor with Alpha - Allowed Bit Depths: 8,16",

            // ----------------------------------

            var _ => $"Unknown colortype: {colortype}",
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

    public static byte[] CreatePNGFromTexture( Texture texture )
    {
        Logger.Checkpoint();

        Guard.Against.Null( texture );
        Guard.Against.Null( texture.GetImageData() );

        return CreatePNGFromRawRGBA( texture.GetImageData()!,
                                     texture.Width,
                                     texture.Height,
                                     texture.ColorFormat,
                                     ( byte )texture.BitDepth );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rawRgba"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="format"></param>
    /// <param name="bitDepth"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static byte[] CreatePNGFromRawRGBA( byte[] rawRgba,
                                               int width,
                                               int height,
                                               int format,
                                               byte bitDepth = 8 )
    {
        if ( rawRgba == null )
        {
            throw new GdxRuntimeException( "Cannot perform PNG conversion as Input data is null." );
        }

        var bytesPerPixel = Graphics.PixelFormat.BytesPerPixel( format );

        if ( rawRgba.Length != ( width * height * bytesPerPixel ) )
        {
            Logger.Debug( $"Input data size does not match width x height x {bytesPerPixel}" );
            Logger.Debug( $"Expected: {width * height * bytesPerPixel}, Actual: {rawRgba.Length}" );
            Logger.Debug( $"Width: {width}, Height: {height}" );
            Logger.Debug( $"Format: {format}" );

            throw new ArgumentException( $"Input data size does not match width x height x {bytesPerPixel}" );
        }

        Logger.Debug( $"Input data size, {rawRgba.Length}, matches {width} x {height} x {bytesPerPixel}" );

        using var ms = new MemoryStream();

        // PNG signature
        ms.Write( StandardPNGSignature );

        // IHDR chunk
        using ( var ihdr = new MemoryStream() )
        {
            WriteBigEndian( ihdr, width );
            WriteBigEndian( ihdr, height );

            ihdr.WriteByte( bitDepth );       // bit depth
            ihdr.WriteByte( ( byte )format ); // color type: RGBA
            ihdr.WriteByte( 0 );              // compression
            ihdr.WriteByte( 0 );              // filter
            ihdr.WriteByte( 0 );              // interlace

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
            using var crc32 = new MinimalCrc32();

            crc32.TransformBlock( typeBytes, 0, 4, null, 0 );
            crc32.TransformBlock( data, 0, data.Length, null, 0 );
            crc32.TransformFinalBlock( [ ], 0, 0 );

            if ( crc32.Hash != null )
            {
                WriteBigEndian( s, BitConverter.ToInt32( crc32.Hash, 0 ) );

//                WriteBigEndian( s, BitConverter.ToInt32( crc32.Hash.Reverse().ToArray(), 0 ) );
            }
        }
    }

    public static void WritePNGToFile( byte[] pngData, string filename )
    {
        if ( ( pngData == null ) || ( pngData.Length == 0 ) )
        {
            throw new ArgumentException( "PNG data is null or empty." );
        }

        if ( string.IsNullOrWhiteSpace( filename ) )
        {
            throw new ArgumentException( "Filename is null or empty." );
        }

        Logger.Debug( $"Writing PNG data to file: {filename}" );

        File.WriteAllBytes( filename, pngData );
    }

    // ========================================================================

    /// <summary>
    /// Extracts the PNG signature (first 8 bytes) from the provided PNG data and returns it
    /// as a <see cref="PNGFormatStructs.PNGSignature"/> structure. The signature is validated
    /// against the standard PNG signature to determine its validity.
    /// If the validation fails, an error is logged, and an empty signature is returned.
    /// </summary>
    /// <param name="pngData"> The byte array containing PNG file data to extract the signature from. </param>
    /// <returns>
    /// A <see cref="PNGFormatStructs.PNGSignature"/> structure containing the extracted PNG signature.
    /// If the provided data is invalid, an empty signature is returned.
    /// </returns>
    private static PNGFormatStructs.PNGSignature ExtractSignature( byte[] pngData )
    {
        var signature = new byte[ SIGNATURE_LENGTH ];

        Array.Copy( pngData, 0, signature, 0, SIGNATURE_LENGTH );

        var signatureStruct = new PNGFormatStructs.PNGSignature
        {
            Signature = signature,
        };

        if ( !IsPNG( pngData ) )
        {
            Logger.Error( "Invalid PNG Signature" );

            if ( Api.DevMode )
            {
                StringBuilder sb  = new();
                StringBuilder sb2 = new();

                for ( var i = 0; i < SIGNATURE_LENGTH; i++ )
                {
                    sb.Append( $"[{PngSignature.Signature[ i ]:X2}]" );
                    sb2.Append( $"[{StandardPNGSignature[ i ]:X2}]" );
                }

                sb.Append( " - Image signature" );
                sb2.Append( " - Correct PNG signature" );

                Logger.Debug( sb.ToString() );
                Logger.Debug( sb2.ToString() );
            }

            signatureStruct.Signature = [ ];
        }

        return signatureStruct;
    }

    /// <summary>
    /// Extracts and parses the IHDR chunk from the provided PNG data, retrieving essential
    /// metadata such as image dimensions, color properties, and compression information.
    /// This method processes the data starting from the expected IHDR chunk location in the
    /// PNG byte array.
    /// </summary>
    /// <param name="pngData">
    /// The binary data of the PNG file from which the IHDR chunk is extracted. Must not be null or empty.
    /// </param>
    /// <returns>
    /// An instance of <see cref="PNGFormatStructs.IHDRChunk"/> containing the parsed IHDR chunk data,
    /// including width, height, bit depth, color type, compression, filter, interlace information,
    /// and CRC values.
    /// </returns>
    private static PNGFormatStructs.IHDRChunk ExtractIHDR( byte[] pngData )
    {
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

        return new PNGFormatStructs.IHDRChunk
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
    }

    /// <summary>
    /// Extracts the IDAT chunk from the provided PNG data. The IDAT chunk contains
    /// the image data of the PNG file, including its compressed size and type.
    /// </summary>
    /// <param name="pngData">The byte array containing the PNG file data.</param>
    /// <returns>
    /// A <see cref="PNGFormatStructs.IDATChunk"/> containing the size and type
    /// information of the IDAT chunk.
    /// </returns>
    private static PNGFormatStructs.IDATChunk ExtractIDAT( byte[] pngData )
    {
        var tmp = new byte[ IDAT_SIZE ];

        Array.Copy( pngData, IDAT_START, tmp, 0, IDAT_SIZE );

        var tmpChunkSize = ReadBigEndianUInt32( tmp, IDAT_SIZE );

        return new PNGFormatStructs.IDATChunk
        {
            ChunkSize = tmpChunkSize,
            ChunkType = tmp,
        };
    }

    /// <summary>
    /// Outputs the analysis of the PNG image, including details such as the PNG
    /// signature, IHDR chunk properties (e.g., dimensions, bit depth, color type),
    /// pixel format, compression, filter method, interlace method, and checksum.
    /// Logs the analysis results to the debug console.
    /// </summary>
    /// <param name="pngData">The byte array containing the PNG file data.</param>">
    private static void OutputAnalysis( byte[] pngData )
    {
        var idatData = new byte[ IHDR_DATA_SIZE ];

        Array.Copy( pngData, IHDR_DATA_OFFSET, idatData, 0, IHDR_DATA_SIZE );

        Logger.Debug( $"PNG Signature   : {BitConverter.ToString( PngSignature.Signature ).Replace( "-", " " )}" );
        Logger.Debug( "-----------------------------" );
        Logger.Debug( $"IHDR            : {BitConverter.ToString( IHDRchunk.Ihdr ).Replace( "-", " " )}" );
        Logger.Debug( $"IHDR TYPE       : {BitConverter.ToString( IHDRchunk.IhdrType ).Replace( "-", " " )}" );
        Logger.Debug( $"IHDR DATA       : {BitConverter.ToString( idatData ).Replace( "-", " " )}" );
        Logger.Debug( "-----------------------------" );
        Logger.Debug( $"- Width         : {IHDRchunk.Width}" );
        Logger.Debug( $"- Height        : {IHDRchunk.Height}" );
        Logger.Debug( $"- BitDepth      : {IHDRchunk.BitDepth}" );
        Logger.Debug( $"- ColorType     : {IHDRchunk.ColorType} :: ( {ColorTypeName( IHDRchunk.ColorType )} )" );
        Logger.Debug( $"- PixelFormat   : {Lugh.Graphics.PixelFormat.GetFormatString( PNGPixelFormat )} :: ( {PNGPixelFormat} )" );
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

// ============================================================================
// ============================================================================