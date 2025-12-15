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

namespace Extensions.Source.Drawing;

[PublicAPI]
public interface ITransparency
{
    /// <summary>
    /// Represents image data that is guaranteed to be completely opaque,
    /// meaning that all pixels have an alpha value of 1.0.
    /// </summary>
    const int OPAQUE = 1;

    /// <summary>
    /// Represents image data that is guaranteed to be either completely
    /// opaque, with an alpha value of 1.0, or completely transparent,
    /// with an alpha value of 0.0.
    /// </summary>
    const int BITMASK = 2;

    /// <summary>
    /// Represents image data that contains or might contain arbitrary
    /// alpha values between and including 0.0 and 1.0.
    /// </summary>
    const int TRANSLUCENT = 3;

    /// <summary>
    /// Returns the type of this <c>Transparency</c>.
    /// <returns>
    /// The field type of this <c>Transparency</c>, which is either OPAQUE,
    /// BITMASK or TRANSLUCENT.
    /// </returns>
    /// </summary>
    public int GetTransparency();
}

// ========================================================================
// ========================================================================