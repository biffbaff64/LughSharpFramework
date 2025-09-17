﻿// /////////////////////////////////////////////////////////////////////////////
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
public class ShaderConstants
{
    public const string A_COLOR      = "a_color";
    public const string A_POSITION   = "a_position";
    public const string A_TEX_COORD0 = "a_texCoord0";

    public const string U_PROJECTION_MATRIX = "u_projectionMatrix";
    public const string U_VIEW_MATRIX       = "u_viewMatrix";
    public const string U_MODEL_MATRIX      = "u_modelMatrix";
    public const string U_TEXTURE           = "u_texture";

    public const string V_TEXTURE_COORDINATES = "v_texCoords";
    public const string V_VCOLOR              = "v_color";
    public const string V_FRAG_COLOR          = "fragColor";
}

// ========================================================================
// ========================================================================