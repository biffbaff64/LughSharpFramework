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

using JetBrains.Annotations;

using LughSharp.Source.Maths;

namespace LughSharp.Source.Graphics.G3D.Models.Data;

[PublicAPI]
public class ModelTexture
{
    public const int UsageUnknown      = 0;
    public const int UsageNone         = 1;
    public const int UsageDiffuse      = 2;
    public const int UsageEmissive     = 3;
    public const int UsageAmbient      = 4;
    public const int UsageSpecular     = 5;
    public const int UsageShininess    = 6;
    public const int UsageNormal       = 7;
    public const int UsageBump         = 8;
    public const int UsageTransparency = 9;
    public const int UsageReflection   = 10;

    public string?  ID            { get; set; }
    public string?  FileName      { get; set; }
    public Vector2? UVTranslation { get; set; }
    public Vector2? UVScaling     { get; set; }
    public int      Usage         { get; set; }
}

// ============================================================================
// ============================================================================