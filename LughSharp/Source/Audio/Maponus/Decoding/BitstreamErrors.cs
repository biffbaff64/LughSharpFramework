// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Circa64 Software Projects / Richard Ikin.
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

using JetBrains.Annotations;

namespace LughSharp.Source.Audio.Maponus.Decoding;

/// <summary>
/// This struct describes all error codes that can be thrown
/// in BistreamExceptions.
/// </summary>
[PublicAPI]
[StructLayout( LayoutKind.Sequential )]
public struct BitstreamErrors
{
    public const int UnknownError       = BitstreamError + 0;
    public const int UnknownSampleRate = BitstreamError + 1;
    public const int StreaError         = BitstreamError + 2;
    public const int UnexpectedEof      = BitstreamError + 3;
    public const int StreamEof          = BitstreamError + 4;
    public const int BitstreamLast      = 0x1ff;
    public const int BitstreamError     = 0x100;
    public const int DecoderError       = 0x200;
}

// ============================================================================
// ============================================================================