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

using GLenum = System.Int32;
using GLfloat = System.Single;
using GLint = System.Int32;
using GLsizei = System.Int32;
using GLbitfield = System.UInt32;
using GLdouble = System.Double;
using GLuint = System.UInt32;
using GLboolean = System.Boolean;
using GLubyte = System.Byte;
using GLsizeiptr = System.Int32;
using GLintptr = System.Int32;
using GLshort = System.Int16;
using GLbyte = System.SByte;
using GLushort = System.UInt16;
using GLchar = System.Byte;
using GLuint64 = System.UInt64;
using GLint64 = System.Int64;

namespace LughSharp.Lugh.Graphics.OpenGL;

public partial class GLBindings
{
    /// <summary>
    /// 
    /// </summary>
    [PublicAPI, StructLayout( LayoutKind.Sequential )]
    public struct DrawElementsIndirectCommand
    {
        public GLuint Count;
        public GLuint PrimCount;
        public GLuint FirstIndex;
        public GLuint BaseVertex;
        public GLuint BaseInstance;
    }

    /// <summary>
    /// 
    /// </summary>
    [PublicAPI, StructLayout( LayoutKind.Sequential )]
    public struct DrawArraysIndirectCommand
    {
        public GLuint Count         { get; set; }
        public GLuint InstanceCount { get; set; }
        public GLuint First         { get; set; }
        public GLuint BaseInstance  { get; set; }
    }
}

