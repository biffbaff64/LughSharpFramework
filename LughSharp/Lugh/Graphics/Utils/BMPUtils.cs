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

using LughSharp.Lugh.Graphics.Images;
using LughSharp.Lugh.Utils;

namespace LughSharp.Lugh.Graphics.Utils;

[PublicAPI]
public static class BMPUtils
{
    public static BitmapFileHeader     BitmapFileHeader     { get; private set; }
    public static BitmapFileInfoHeader BitmapFileInfoHeader { get; private set; }

    // ========================================================================

    private static byte[] _fileHeaderBytes = new byte[ Marshal.SizeOf( typeof( BitmapFileHeader ) ) ];
    private static byte[] _infoHeaderBytes = new byte[ Marshal.SizeOf( typeof( BitmapFileInfoHeader ) ) ];

    // ========================================================================

    public static void AnalyseBMP( string filePath )
    {
        BitmapFileHeader fileHeader;

        using ( var fs = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
        {
            if ( fs.Read( _fileHeaderBytes, 0, _fileHeaderBytes.Length ) > 0 )
            {
                var fileHeaderHandle = GCHandle.Alloc( _fileHeaderBytes, GCHandleType.Pinned );
                fileHeader = ( BitmapFileHeader )(Marshal.PtrToStructure( fileHeaderHandle.AddrOfPinnedObject(),
                                                                          typeof( BitmapFileHeader ) ) ?? new BitmapFileHeader() );
                fileHeaderHandle.Free();
            }
        }

        AnalyseBMP( fileHeader );
    }

    public static void AnalyseBMP( Bitmap bitmap )
    {
        byte[] bmpData;
    }

    public static void AnalyseBMP( byte[] bmpData, bool showResults = false )
    {
        if ( ( bmpData == null ) || ( bmpData.Length == 0 ) )
        {
            throw new ArgumentException( "Invalid BMP Data" );
        }

        BitmapFileHeader = new BMPFormatStructs.BitmapFileHeader
        {
            //TODO:
        };

        for ( var i = 0; i < 10; i++ )
        {
            for ( var j = 0; j < 10; j++ )
            {
                Logger.Data( $"{bmpData[ ( i * 10 ) + j ]:2X},", false );
            }

            Logger.NewLine();
        }

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

                Console.WriteLine($"--- BMP File Header for: {filePath} ---");
                Console.WriteLine($"Type (BM): 0x{fileHeader.bfType:X4}");
                Console.WriteLine($"File Size: {fileHeader.bfSize} bytes");
                Console.WriteLine($"Offset to Pixels: {fileHeader.bfOffBits} bytes");

                // Read BITMAPINFOHEADER (assuming it immediately follows the file header)
                fs.Read(infoHeaderBytes, 0, infoHeaderBytes.Length);
                GCHandle infoHeaderHandle = GCHandle.Alloc(infoHeaderBytes, GCHandleType.Pinned);
                BITMAPINFOHEADER infoHeader = (BITMAPINFOHEADER)Marshal.PtrToStructure(infoHeaderHandle.AddrOfPinnedObject(), typeof(BITMAPINFOHEADER));
                infoHeaderHandle.Free();

                Console.WriteLine($"--- BMP Info Header ---");
                Console.WriteLine($"Header Size: {infoHeader.biSize} bytes");
                Console.WriteLine($"Width: {infoHeader.biWidth} pixels");
                Console.WriteLine($"Height: {infoHeader.biHeight} pixels");
                Console.WriteLine($"Planes: {infoHeader.biPlanes}");
                Console.WriteLine($"Bits per Pixel: {infoHeader.biBitCount}");
                Console.WriteLine($"Compression: {infoHeader.biCompression}"); // See BMP spec for values (0=BI_RGB, 1=BI_RLE8, etc.)
                Console.WriteLine($"Image Size: {infoHeader.biSizeImage} bytes");
                Console.WriteLine($"X Pels per Meter: {infoHeader.biXPelsPerMeter}");
                Console.WriteLine($"Y Pels per Meter: {infoHeader.biYPelsPerMeter}");
                Console.WriteLine($"Colors Used: {infoHeader.biClrUsed}");
                Console.WriteLine($"Colors Important: {infoHeader.biClrImportant}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading BMP header: {ex.Message}");
        }
        */
    }
}