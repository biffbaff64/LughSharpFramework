﻿// ///////////////////////////////////////////////////////////////////////////////
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

using Exception = System.Exception;

namespace LughSharp.Lugh.Audio.Maponus.Support;

[PublicAPI]
public class SupportClass
{
    public static int URShift( int number, int bits )
    {
        if ( number >= 0 )
        {
            return number >> bits;
        }

        return ( number >> bits ) + ( 2 << ~bits );
    }

    public static int URShift( int number, long bits )
    {
        return URShift( number, ( int )bits );
    }

    public static long URShift( long number, int bits )
    {
        if ( number >= 0 )
        {
            return number >> bits;
        }

        return ( number >> bits ) + ( 2L << ~bits );
    }

    public static long URShift( long number, long bits )
    {
        return URShift( number, ( int )bits );
    }

    public static void WriteStackTrace( Exception throwable, TextWriter stream )
    {
        stream.Write( throwable.StackTrace );
        stream.Flush();
    }

    /// <summary>
    /// This method is used as a dummy method to simulate VJ++ behavior
    /// </summary>
    /// <param name="literal">The literal to return</param>
    /// <returns>The received value</returns>
    public static long Identity( long literal )
    {
        return literal;
    }

    /// <summary>
    /// This method is used as a dummy method to simulate VJ++ behavior
    /// </summary>
    /// <param name="literal">The literal to return</param>
    /// <returns>The received value</returns>
    public static ulong Identity( ulong literal )
    {
        return literal;
    }

    /// <summary>
    /// This method is used as a dummy method to simulate VJ++ behavior
    /// </summary>
    /// <param name="literal">The literal to return</param>
    /// <returns>The received value</returns>
    public static float Identity( float literal )
    {
        return literal;
    }

    /// <summary>
    /// This method is used as a dummy method to simulate VJ++ behavior
    /// </summary>
    /// <param name="literal">The literal to return</param>
    /// <returns>The received value</returns>
    public static double Identity( double literal )
    {
        return literal;
    }

    /// <summary>
    /// Reads a number of characters from the current source Stream and writes the
    /// data to the target array at the
    /// specified index.
    /// </summary>
    /// <param name="sourceStream">The source Stream to read from</param>
    /// <param name="target">Contains the array of characteres read from the source Stream.</param>
    /// <param name="start">The starting index of the target array.</param>
    /// <param name="count">The maximum number of characters to read from the source Stream.</param>
    /// <returns>
    /// The number of characters read. The number will be less than or equal to count
    /// depending on the data available in the source Stream.
    /// </returns>
    public static int ReadInput( Stream sourceStream, ref sbyte[] target, int start, int count )
    {
        var receiver  = new byte[ target.Length ];
        var bytesRead = sourceStream.Read( receiver, start, count );

        for ( var i = start; i < ( start + bytesRead ); i++ )
        {
            target[ i ] = ( sbyte )receiver[ i ];
        }

        return bytesRead;
    }

    /// <summary>
    /// Converts an array of sbytes to an array of bytes
    /// </summary>
    /// <param name="sbyteArray">The array of sbytes to be converted</param>
    /// <returns>The new array of bytes</returns>
    public static byte[] ToByteArray( sbyte[] sbyteArray )
    {
        var byteArray = new byte[ sbyteArray.Length ];

        for ( var index = 0; index < sbyteArray.Length; index++ )
        {
            byteArray[ index ] = ( byte )sbyteArray[ index ];
        }

        return byteArray;
    }

    /// <summary>
    /// Converts a string to an array of bytes
    /// </summary>
    /// <param name="sourceString">The string to be converted</param>
    /// <returns>The new array of bytes</returns>
    public static byte[] ToByteArray( string sourceString )
    {
        var byteArray = new byte[ sourceString.Length ];

        for ( var index = 0; index < sourceString.Length; index++ )
        {
            byteArray[ index ] = ( byte )sourceString[ index ];
        }

        return byteArray;
    }

    /// <summary>
    /// Method that copies an array of sbytes from a String to a received array.
    /// </summary>
    /// <param name="sourceString">The String to get the sbytes.</param>
    /// <param name="sourceStart">Position in the String to start getting sbytes.</param>
    /// <param name="sourceEnd">Position in the String to end getting sbytes.</param>
    /// <param name="destinationArray">Array to store the bytes.</param>
    /// <param name="destinationStart">Position in the destination array to start storing the sbytes.</param>
    /// <returns>An array of sbytes</returns>
    public static void GetSBytesFromString( string sourceString,
                                            int sourceStart,
                                            int sourceEnd,
                                            ref sbyte[] destinationArray,
                                            int destinationStart )
    {
        var sourceCounter      = sourceStart;
        var destinationCounter = destinationStart;

        while ( sourceCounter < sourceEnd )
        {
            destinationArray[ destinationCounter ] = ( sbyte )sourceString[ sourceCounter ];
            sourceCounter++;
            destinationCounter++;
        }
    }
}