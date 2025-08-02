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

namespace LughSharp.Lugh.Graphics.OpenGL.Stbi;

[PublicAPI]
public class StbImageNative
{
    // Constants for requested components
    public const int STBI_DEFAULT    = 0;
    public const int STBI_GREY       = 1;
    public const int STBI_GREY_ALPHA = 2;
    public const int STBI_RGB        = 3;
    public const int STBI_RGB_ALPHA  = 4;

    // ========================================================================

    private const string NATIVE_LIB = "lib/net8.0/LughSharpStbImage";

    // ========================================================================

    [DllImport( NATIVE_LIB, EntryPoint = "LoadImageFromFile", CallingConvention = CallingConvention.Cdecl )]
    public static extern IntPtr LoadImageFromFile( [MarshalAs( UnmanagedType.LPStr )] string filename,
                                                   out int x,
                                                   out int y,
                                                   out int comp,
                                                   int reqComp );

    [DllImport( NATIVE_LIB, EntryPoint = "LoadImageFromMemory", CallingConvention = CallingConvention.Cdecl )]
    public static extern IntPtr LoadImageFromMemory( IntPtr buffer, // Use IntPtr for the unmanaged byte*
                                                     int len,
                                                     out int x,
                                                     out int y,
                                                     out int comp,
                                                     int reqComp );

    [DllImport( NATIVE_LIB, EntryPoint = "FreeImageData", CallingConvention = CallingConvention.Cdecl )]
    public static extern void FreeImageData( IntPtr data ); // Use IntPtr to represent void*

    [DllImport( NATIVE_LIB, EntryPoint = "GetImageInfoFromFile", CallingConvention = CallingConvention.Cdecl )]
    public static extern int GetImageInfoFromFile( [MarshalAs( UnmanagedType.LPStr )] string filename,
                                                   out int x,
                                                   out int y,
                                                   out int comp );

    [DllImport( NATIVE_LIB, EntryPoint = "LoadImageFromFileHDR", CallingConvention = CallingConvention.Cdecl )]
    public static extern IntPtr LoadImageFromFileHDR( [MarshalAs( UnmanagedType.LPStr )] string filename,
                                                      out int x,
                                                      out int y,
                                                      out int comp,
                                                      int reqComp );
}

// ========================================================================
// ========================================================================