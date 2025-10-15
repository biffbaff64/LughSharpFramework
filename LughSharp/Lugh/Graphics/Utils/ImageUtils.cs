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

namespace LughSharp.Lugh.Graphics.Utils;

[PublicAPI]
public class ImageUtils
{
    [PublicAPI]
    public enum RejectionReason
    {
        ColorTypeBitDepthMismatch,
        UnknownCriticalChunk,
        CrcMismatch,

        // --------------------------------------

        Unknown,
    }

    // ========================================================================

    /// <summary>
    /// Rejects an invalid PNG image, throwing a GdxRuntimeException with a suitable message.
    /// <br/>
    /// <para>
    /// <b>ColorTypeBitDepthMismatch.</b>
    /// <br/>
    /// For ColorTypeBitDepthMismatch, the message will say that the color type and bit
    /// depth combination is invalid. You can optionally pass in a string describing the
    /// actual values of the color type and bit depth.
    /// <para>
    /// Example:
    /// <code>
    /// ImageUtils.RejectInvalidImage( ImageUtils.RejectionReason.ColorTypeBitDepthMismatch,
    /// $"BitDepth: {bitDepth}, ColorType: {colorType}." );
    /// </code>
    /// </para>
    /// </para>
    /// <para>
    /// <b>UnknownCriticalChunk.</b>
    /// <br/>
    /// </para>
    /// <para>
    /// <b>CrcMismatch.</b>
    /// <br/>
    /// </para>
    /// </summary>
    public static void RejectInvalidImage( RejectionReason reason, params object[] data )
    {
        var sb = new StringBuilder();

        sb.Append( "PNG file rejected: " );

        if ( reason == RejectionReason.ColorTypeBitDepthMismatch )
        {
            sb.Append( "Color Type and Bit Depth combination is invalid." );

            if ( data.Length > 0 )
            {
                sb.Append( ' ' );
                sb.Append( ( string )data[ 0 ] );
            }
        }
        else if ( reason == RejectionReason.UnknownCriticalChunk )
        {
            sb.Append( "Contains an unrecognised critical chunk type." );
        }
        else if ( reason == RejectionReason.CrcMismatch )
        {
            sb.Append( "CRC checksum failure. The file is corrupted." );
        }
        else
        {
            sb.Append( "Unknown reason" );
        }

        throw new GdxRuntimeException( sb.ToString() );
    }
}

// ========================================================================
// ========================================================================