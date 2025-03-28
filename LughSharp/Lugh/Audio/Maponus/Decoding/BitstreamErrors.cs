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

using System.Runtime.InteropServices;

namespace LughSharp.Lugh.Audio.Maponus.Decoding;

/// <summary>
/// This struct describes all error codes that can be thrown
/// in BistreamExceptions.
/// </summary>
[PublicAPI]
[StructLayout( LayoutKind.Sequential )]
public struct BitstreamErrors
{
    public const int UNKNOWN_ERROR       = BITSTREAM_ERROR + 0;
    public const int UNKNOWN_SAMPLE_RATE = BITSTREAM_ERROR + 1;
    public const int STREA_ERROR         = BITSTREAM_ERROR + 2;
    public const int UNEXPECTED_EOF      = BITSTREAM_ERROR + 3;
    public const int STREAM_EOF          = BITSTREAM_ERROR + 4;
    public const int BITSTREAM_LAST      = 0x1ff;
    public const int BITSTREAM_ERROR     = 0x100;
    public const int DECODER_ERROR       = 0x200;
}