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

namespace LughSharp.Lugh.Graphics.OpenGL.Bindings;

// This is a NEW GL Bindings interface which will be implemented when complete,
// as an attempt to reduce the size of the bindings library and also make it
// more efficient.

[PublicAPI]
public interface IGLBindings
{
    // ========================================================================
    // State

    void Enable( int cap );
    void Disable( int cap );
    bool IsEnabled( int cap );

    // ========================================================================
    // Parameters ( single and vector )

    void SetTexParameter< T >( int target, int pname, T value ) where T : unmanaged;
    void SetTexParameter< T >( int target, int pname, ReadOnlySpan< T > values ) where T : unmanaged;

    void SetTexParameterv< T >( int target, int pname, T value ) where T : unmanaged;
    void SetTexParameterv< T >( int target, int pname, ReadOnlySpan< T > values ) where T : unmanaged;

    void SetTexParameterIv< T >( int target, int pname, T value ) where T : unmanaged;
    void SetTexParameterIv< T >( int target, int pname, ReadOnlySpan< T > values ) where T : unmanaged;

//  GetTexParameter
//  GetTexLevelParameter

//  PointParameter
//  PointParameterv
    
    // ========================================================================
    // Images: managed buffer and raw (PBO/offset) path

    void TexImage1D( int target, int level, int internalFormat, int width,
                     int border, int format, int type, IntPtr pixels );

    void TexImage1D< T >( int target, int level, int internalFormat, int width,
                          int border, int format, int type, T[] pixels ) where T : unmanaged;

    void TexImage2D( int target, int level, int internalFormat, int width, int height,
                     int border, int format, int type, IntPtr pixels );

    void TexImage2D< T >( int target, int level, int internalFormat, int width, int height,
                          int border, int format, int type, ReadOnlySpan< T > pixels ) where T : unmanaged;

    void TexImage3D( int target, int level, int internalFormat, int width, int height,
                     int depth, int border, int format, int type, IntPtr pixels );

    void TexImage3D< T >( int target, int level, int internalFormat, int width, int height,
                          int depth, int border, int format, int type, T[] pixels ) where T : unmanaged;

//  GetTexImage

    // ========================================================================
    // Queries

    T Get< T >( int pname ) where T : unmanaged;
    void Get< T >( int pname, Span< T > dest ) where T : unmanaged;

    // ========================================================================
    // Readback
}

// ========================================================================
// ========================================================================