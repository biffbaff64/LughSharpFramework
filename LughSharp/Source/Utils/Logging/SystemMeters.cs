// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Circa64 Software Projects
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

namespace LughSharp.Source.Utils.Logging;

/// <summary>
/// Statistical Logging meters for various LughSharp Exceptions. 
/// </summary>
[PublicAPI]
public enum SystemMeters
{
    IllegalGameMode         = 0,
    SoundLoadFail           = 1,
    BadPlayerAction         = 2,
    FontLoadFailure         = 3,
    BorderedFontLoadFailure = 4,

    IOException                    = 5,
    IndexOutOfBoundsException      = 6,
    ArrayIndexOutOfBoundsException = 7,
    SaxException                   = 8,
    InterruptedException           = 9,
    NullPointerException           = 10,
    IllegalStateException          = 11,
    LughRuntimeException           = 12,
    EntityDataException            = 13,

    UnknownException = 14,
    DummyMeter       = 15,
}

// ============================================================================
// ============================================================================
