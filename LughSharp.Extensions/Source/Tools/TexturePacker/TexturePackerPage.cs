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

using System.Runtime.Versioning;
using LughSharp.Core.Utils.Logging;

namespace Extensions.Source.Tools.TexturePacker;

[PublicAPI]
[SupportedOSPlatform( "windows" )]
public class TexturePackerPage
{
    public string?                   ImageName      { get; set; } = "";
    public List< TexturePackerRect > OutputRects    { get; set; } = [ ];
    public List< TexturePackerRect > RemainingRects { get; set; } = [ ];
    public float                     Occupancy      { get; set; }
    public int                       X              { get; set; }
    public int                       Y              { get; set; }
    public int                       Width          { get; set; }
    public int                       Height         { get; set; }
    public int                       ImageWidth     { get; set; }
    public int                       ImageHeight    { get; set; }

    public void Debug()
    {
        Logger.Block();
        Logger.Debug( $"ImageName: {ImageName}" );
        Logger.Debug( $"OutputRects: {OutputRects.Count}" );
        Logger.Debug( $"RemainingRects: {RemainingRects.Count}" );
        Logger.Debug( $"Occupancy: {Occupancy}" );
        Logger.Debug( $"X: {X}" );
        Logger.Debug( $"Y: {Y}" );
        Logger.Debug( $"Width: {Width}" );
        Logger.Debug( $"Height: {Height}" );
        Logger.Debug( $"ImageWidth: {ImageWidth}" );
        Logger.Debug( $"ImageHeight: {ImageHeight}" );
        Logger.EndBlock();
    }
}

// ============================================================================
// ============================================================================

