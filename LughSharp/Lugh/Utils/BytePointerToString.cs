﻿// /////////////////////////////////////////////////////////////////////////////
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

using System.Text;

namespace LughSharp.Lugh.Utils;

[PublicAPI]
public static class BytePointerToString
{
    public static unsafe string Convert( byte* bytePointer )
    {
        // Find the length of the byte array by searching for the null terminator
        var length = 0;

        while ( bytePointer[ length ] != 0 )
        {
            length++;
        }

        // Convert byte* to byte array
        var byteArray = new byte[ length ];
        Marshal.Copy( ( IntPtr )bytePointer, byteArray, 0, length );

        // Convert byte array to string
        return Encoding.UTF8.GetString( byteArray );
    }

    /// <summary>
    /// Converts a GLchar* (pointer to a null-terminated or length-specified C-style string)
    /// to a C# string.
    /// </summary>
    /// <param name="glCharPtr">The pointer to the GLchar array.</param>
    /// <param name="length">The length of the string, or -1 if null-terminated.</param>
    /// <returns>The C# string, or null if the input pointer is null.</returns>
    public static string? GlCharPointerToString( IntPtr glCharPtr, int length = -1 )
    {
        if ( glCharPtr == IntPtr.Zero )
        {
            return null;
        }

        if ( length == -1 ) // Null-terminated string
        {
            // Find the null terminator
            var len = 0;

            while ( Marshal.ReadByte( glCharPtr, len ) != 0 )
            {
                len++;
            }

            if ( len == 0 )
            {
                return string.Empty; // Handle the case of an empty string
            }

            var bytes = new byte[ len ];
            Marshal.Copy( glCharPtr, bytes, 0, len );

            return Encoding.UTF8.GetString( bytes ); // Or appropriate encoding
        }
        else // Length-specified string
        {
            if ( length == 0 )
            {
                return string.Empty; //Handle the case of an empty string
            }

            var bytes = new byte[ length ];
            Marshal.Copy( glCharPtr, bytes, 0, length );

            return Encoding.UTF8.GetString( bytes ); // Or appropriate encoding
        }
    }
}