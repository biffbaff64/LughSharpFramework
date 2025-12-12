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

using JetBrains.Annotations; namespace LughSharp.Core.Graphics;

[PublicAPI]
public class ShaderLoader
{
    /// <summary>
    /// Loads a GLSL shader file from the specified path and returns its content as a string.
    /// </summary>
    /// <param name="filePath">The path to the GLSL file to load</param>
    /// <returns>The content of the GLSL file as a string</returns>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist</exception>
    /// <exception cref="IOException">Thrown when there's an error reading the file</exception>
    public static string Load( string filePath )
    {
        if ( !File.Exists( filePath ) )
        {
            throw new FileNotFoundException( $"Shader file not found: {filePath}" );
        }

        try
        {
            return File.ReadAllText( filePath );
        }
        catch ( Exception ex ) when ( ex is IOException or UnauthorizedAccessException )
        {
            throw new IOException( $"Failed to read shader file: {filePath}", ex );
        }
    }

    /// <summary>
    /// Loads a GLSL shader file from the specified path asynchronously and returns its content as a string.
    /// </summary>
    /// <param name="filePath">The path to the GLSL file to load</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the content
    /// of the GLSL file as a string.
    /// </returns>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist</exception>
    /// <exception cref="IOException">Thrown when there's an error reading the file</exception>
    public static async Task< string > LoadAsync( string filePath )
    {
        if ( !File.Exists( filePath ) )
        {
            throw new FileNotFoundException( $"Shader file not found: {filePath}" );
        }

        try
        {
            return await File.ReadAllTextAsync( filePath );
        }
        catch ( Exception ex ) when ( ex is IOException or UnauthorizedAccessException )
        {
            throw new IOException( $"Failed to read shader file: {filePath}", ex );
        }
    }

    /// <summary>
    /// Loads multiple GLSL shader files and returns their contents as a dictionary with file paths as keys.
    /// </summary>
    /// <param name="filePaths">The paths to the GLSL files to load</param>
    /// <returns>A dictionary containing file paths as keys and shader contents as values</returns>
    public static Dictionary< string, string > Load( params string[] filePaths )
    {
        var shaders = new Dictionary< string, string >();

        foreach ( var filePath in filePaths )
        {
            shaders[ filePath ] = Load( filePath );
        }

        return shaders;
    }
}

// ========================================================================
// ========================================================================