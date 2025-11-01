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

using LughSharp.Lugh.Graphics;
using LughUtils.source.Exceptions;

namespace Extensions.Source.Tools.TexturePacker;

public partial class TexturePacker
{
    /// <summary>
    /// Returns true if the output file does not yet exist or its last modification date
    /// is before the last modification date of the input file
    /// </summary>
    public static bool IsModified( string input, string output, string packFileName, TexturePackerSettings settings )
    {
        var packFullFileName = output;

        if ( !output.EndsWith( '/' ) )
        {
            packFullFileName = output + "/";
        }

        packFullFileName += packFileName;
        packFullFileName += settings.AtlasExtension;

        // Check against the only file we know for sure will exist and will
        // be changed if any asset changes the atlas file.
        var outputFile = new FileInfo( packFullFileName );

        if ( !File.Exists( outputFile.FullName ) )
        {
            return true;
        }

        var inputFile = new FileInfo( input );

        if ( !File.Exists( inputFile.FullName ) )
        {
            throw new ArgumentException( $"TexturePacker#IsModified: Input file does not exist: {inputFile.Name}" );
        }

        return IsModified( inputFile.FullName, outputFile.LastWriteTimeUtc.Ticks / 10000 );
    }

    /// <summary>
    /// Returns true if the output file does not yet exist or its last modification date
    /// is before the last modification date of the input file
    /// </summary>
    /// <param name="filePath"> Output file path. </param>
    /// <param name="lastModified"></param>
    /// <returns></returns>
    public static bool IsModified( string filePath, long lastModified )
    {
        try
        {
            var fileInfo = new FileInfo( filePath );

            if ( fileInfo.LastWriteTimeUtc.Ticks > lastModified )
            {
                return true;
            }

            if ( fileInfo.Attributes.HasFlag( FileAttributes.Directory ) )
            {
                var children = Directory.GetFiles( filePath, "*", SearchOption.AllDirectories );

                if ( children is { Length: > 0 } )
                {
                    foreach ( var child in children )
                    {
                        if ( IsModified( child, lastModified ) )
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        catch ( Exception ex )
        {
            throw new GdxRuntimeException( $"Error checking modification: {ex.Message}" );
        }
    }

    public bool ProcessIfModified( string input, string output, string packFileName )
    {
        var settings = new TexturePackerSettings();

        if ( IsModified( input, output, packFileName, settings ) )
        {
            Process( settings, input, output, packFileName );

            return true;
        }

        return false;
    }

    public bool ProcessIfModified( TexturePackerSettings settings,
                                          string input,
                                          string output,
                                          string packFileName )
    {
        if ( IsModified( input, output, packFileName, settings ) )
        {
            Process( settings, input, output, packFileName );

            return true;
        }

        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="GdxRuntimeException"></exception>
    private PixelFormat GetPixelFormat( int format )
    {
        return format switch
        {
            LughFormat.RGBA8888
                or LughFormat.RGBA4444 => PixelFormat.Format32bppArgb,

            LughFormat.RGB565
                or LughFormat.RGB888 => PixelFormat.Format32bppRgb,

            LughFormat.ALPHA => PixelFormat.Alpha,

            var _ => throw new GdxRuntimeException( $"Unsupported format: {_settings.Format}" ),
        };
    }
}

// ============================================================================
// ============================================================================