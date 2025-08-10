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

using System.Runtime.InteropServices;

using LughSharp.Lugh.Graphics;
using LughSharp.Lugh.Utils;

using Bitmap = System.Drawing.Bitmap;

namespace Extensions.Source.Drawing.Utils;

[PublicAPI]
[SupportedOSPlatform( "windows" )]
public static class BMPUtils
{
    public static BitmapFileHeader     BitmapFileHeader     { get; private set; }
    public static BitmapFileInfoHeader BitmapFileInfoHeader { get; private set; }

    // ========================================================================

    private static byte[] _fileHeaderBytes = new byte[ Marshal.SizeOf( typeof( BitmapFileHeader ) ) ];
    private static byte[] _infoHeaderBytes = new byte[ Marshal.SizeOf( typeof( BitmapFileInfoHeader ) ) ];

    // ========================================================================

    public static void AnalyseBitmap( string filePath )
    {
        try
        {
            using ( var fs = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
            {
                if ( fs.Read( _fileHeaderBytes, 0, _fileHeaderBytes.Length ) > 0 )
                {
                    var fileHeaderHandle = GCHandle.Alloc( _fileHeaderBytes, GCHandleType.Pinned );
                    var fileHeader = ( BitmapFileHeader )( Marshal.PtrToStructure( fileHeaderHandle.AddrOfPinnedObject(),
                                                                                   typeof( BitmapFileHeader ) )
                                                           ?? new BitmapFileHeader() );
                    fileHeaderHandle.Free();

                    Logger.Debug( $"--- BMP File Header for: {filePath} ---" );
                    Logger.Debug( $"Type (BM)       : 0x{fileHeader.FileType:X4}" );
                    Logger.Debug( $"File Size       : {fileHeader.FileSize} bytes" );
                    Logger.Debug( $"Reserved #1     : {fileHeader.Reserved1} bytes" );
                    Logger.Debug( $"Reserved #2     : {fileHeader.Reserved2} bytes" );
                    Logger.Debug( $"Offset to Pixels: {fileHeader.OffsetToPixelArray} bytes" );
                }

                // ----------------------------------------

                if ( fs.Read( _infoHeaderBytes, 0, _infoHeaderBytes.Length ) > 0 )
                {
                    var infoHeaderHandle = GCHandle.Alloc( _infoHeaderBytes, GCHandleType.Pinned );
                    var infoHeader = ( BitmapFileInfoHeader )( Marshal.PtrToStructure( infoHeaderHandle.AddrOfPinnedObject(),
                                                                                       typeof( BitmapFileInfoHeader ) )
                                                               ?? new BitmapFileInfoHeader() );
                    infoHeaderHandle.Free();

                    Logger.Debug( $"--- BMP Info Header ---" );
                    Logger.Debug( $"Header Size     : {infoHeader.HeaderSize} bytes" );
                    Logger.Debug( $"Width           : {infoHeader.Width} pixels" );
                    Logger.Debug( $"Height          : {infoHeader.Height} pixels" );
                    Logger.Debug( $"Planes          : {infoHeader.Planes}" );
                    Logger.Debug( $"Bits per Pixel  : {infoHeader.BitCount}" );
                    Logger.Debug( $"Compression     : {infoHeader.Compression}" ); // See BMP spec for values (0=BI_RGB, 1=BI_RLE8, etc.)
                    Logger.Debug( $"Image Size      : {infoHeader.ImageSize} bytes" );
                    Logger.Debug( $"X Pels per Meter: {infoHeader.XPixelsPerMeter}" );
                    Logger.Debug( $"Y Pels per Meter: {infoHeader.YPixelsPerMeter}" );
                    Logger.Debug( $"Colors Used     : {infoHeader.ColorsUsed}" );
                    Logger.Debug( $"Colors Important: {infoHeader.ImportantColors}" );
                }
            }
        }
        catch ( Exception ex )
        {
            Logger.Debug( $"Error reading BMP header: {ex.Message}" );
        }
    }

    public static void AnalyseBitmap( Bitmap bmp )
    {
        try
        {
            Logger.Debug( $"--- Bitmap Properties (from object) ---" );
            Logger.Debug( $"Width                : {bmp.Width} pixels" );
            Logger.Debug( $"Height               : {bmp.Height} pixels" );
            Logger.Debug( $"Pixel Format         : {bmp.PixelFormat}" );
            Logger.Debug( $"Raw Format           : {bmp.RawFormat}" );
            Logger.Debug( $"Horizontal Resolution: {bmp.HorizontalResolution} DPI" );
            Logger.Debug( $"Vertical Resolution  : {bmp.VerticalResolution} DPI" );
            Logger.Debug( $"Flags                : {bmp.Flags} (hex: 0x{bmp.Flags:X})" );

            // Get more detailed info from PixelFormat
            var bitsPerPixel = System.Drawing.Image.GetPixelFormatSize( bmp.PixelFormat );
            Logger.Debug( $"Bits per Pixel: {bitsPerPixel}" );

            // Check for Alpha Channel
            if ( bmp.PixelFormat is PixelFormat.Format32bppArgb
                                    or PixelFormat.Format32bppPArgb
                                    or PixelFormat.Format64bppArgb
                                    or PixelFormat.Format64bppPArgb
                                    or PixelFormat.Format16bppArgb1555 )
            {
                Logger.Debug( "Image has an Alpha channel." );
            }
            else if ( ( bmp.Flags & ( int )ImageFlags.HasAlpha ) != 0 ) // Check ImageFlags as well
            {
                Logger.Debug( "Image *might* have an Alpha channel (via ImageFlags)." );
            }
            else
            {
                Logger.Debug( "Image does NOT seem to have a direct Alpha channel." );
            }

            // Check if it's Indexed
            if ( bmp.PixelFormat is PixelFormat.Format1bppIndexed
                                    or PixelFormat.Format4bppIndexed
                                    or PixelFormat.Format8bppIndexed )
            {
                Logger.Debug( "Image is Indexed (uses a color palette)." );
                Logger.Debug( $"Palette Entries: {bmp.Palette.Entries.Length}" );
            }
            else
            {
                Logger.Debug( "Image is NOT Indexed (uses direct color)." );
            }

            // Get PropertyItems (can contain EXIF data, etc.)
            foreach ( var propItem in bmp.PropertyItems )
            {
                Logger.Debug( $"- PropertyItem ID: 0x{propItem.Id:X}, Type: {propItem.Type}, Length: {propItem.Len}" );

                // You'd need to parse propItem.Value for its meaning
            }
        }
        catch ( NullReferenceException nre )
        {
            Logger.Warning( $"Error: Bitmap object is null: {nre.Message}" );
        }
        catch ( Exception e )
        {
            Logger.Warning( e.Message );
        }
    }

    public static void AnalyseBitmap( byte[] bmpData, bool showResults = false )
    {
        if ( ( bmpData == null ) || ( bmpData.Length == 0 ) )
        {
            throw new ArgumentException( "Invalid BMP Data" );
        }

        BitmapFileHeader = new BitmapFileHeader
        {
            //TODO:
        };

//        for ( var i = 0; i < 10; i++ )
//        {
//            for ( var j = 0; j < 10; j++ )
//            {
//                Logger.Data( $"{bmpData[ ( i * 10 ) + j ]:2X}," );
//            }

//            Logger.NewLine();
//        }

        /*
        try
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                // Read BITMAPFILEHEADER
                fs.Read(fileHeaderBytes, 0, fileHeaderBytes.Length);
                GCHandle fileHeaderHandle = GCHandle.Alloc(fileHeaderBytes, GCHandleType.Pinned);
                BITMAPFILEHEADER fileHeader = (BITMAPFILEHEADER)Marshal.PtrToStructure(fileHeaderHandle.AddrOfPinnedObject(), typeof(BITMAPFILEHEADER));
                fileHeaderHandle.Free();

                Logger.Debug($"--- BMP File Header for: {filePath} ---");
                Logger.Debug($"Type (BM): 0x{fileHeader.bfType:X4}");
                Logger.Debug($"File Size: {fileHeader.bfSize} bytes");
                Logger.Debug($"Offset to Pixels: {fileHeader.bfOffBits} bytes");

                // Read BITMAPINFOHEADER (assuming it immediately follows the file header)
                fs.Read(infoHeaderBytes, 0, infoHeaderBytes.Length);
                GCHandle infoHeaderHandle = GCHandle.Alloc(infoHeaderBytes, GCHandleType.Pinned);
                BITMAPINFOHEADER infoHeader = (BITMAPINFOHEADER)Marshal.PtrToStructure(infoHeaderHandle.AddrOfPinnedObject(), typeof(BITMAPINFOHEADER));
                infoHeaderHandle.Free();

                Logger.Debug($"--- BMP Info Header ---");
                Logger.Debug($"Header Size: {infoHeader.biSize} bytes");
                Logger.Debug($"Width: {infoHeader.biWidth} pixels");
                Logger.Debug($"Height: {infoHeader.biHeight} pixels");
                Logger.Debug($"Planes: {infoHeader.biPlanes}");
                Logger.Debug($"Bits per Pixel: {infoHeader.biBitCount}");
                Logger.Debug($"Compression: {infoHeader.biCompression}"); // See BMP spec for values (0=BI_RGB, 1=BI_RLE8, etc.)
                Logger.Debug($"Image Size: {infoHeader.biSizeImage} bytes");
                Logger.Debug($"X Pels per Meter: {infoHeader.biXPelsPerMeter}");
                Logger.Debug($"Y Pels per Meter: {infoHeader.biYPelsPerMeter}");
                Logger.Debug($"Colors Used: {infoHeader.biClrUsed}");
                Logger.Debug($"Colors Important: {infoHeader.biClrImportant}");
            }
        }
        catch (Exception ex)
        {
            Logger.Debug($"Error reading BMP header: {ex.Message}");
        }
        */
    }
}