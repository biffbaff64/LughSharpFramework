// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin
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

using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.OpenGL;

// ============================================================================

public partial class GLBindings
{
    // ========================================================================
    // ========================================================================

    private static readonly Dictionary< string, Delegate > _loadedFunctions = new();

    // ========================================================================
    // ========================================================================

    [LibraryImport( "opengl32.dll", EntryPoint = "wglGetProcAddress", StringMarshalling = StringMarshalling.Utf16 )]
    [UnmanagedCallConv( CallConvs = [ typeof( System.Runtime.CompilerServices.CallConvStdcall ) ] )]
    private static partial IntPtr wglGetProcAddress( string procname );

    /// <summary>
    /// Gets the delegate for the specified OpenGL function.
    /// </summary>
    /// <param name="functionName"> The name of the required function. </param>
    /// <param name="functionDelegate"> The delegate storage. </param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static bool GetDelegateForFunction< T >( string functionName, out T functionDelegate ) where T : Delegate
    {
        if ( _loadedFunctions.TryGetValue( functionName, out var existingDelegate ) )
        {
            functionDelegate = ( T )existingDelegate;

            return true; // Already loaded
        }

        var functionPtr = wglGetProcAddress( functionName );

        if ( functionPtr == IntPtr.Zero )
        {
            functionPtr = Glfw.GetProcAddress( functionName );
        }

        if ( functionPtr != IntPtr.Zero )
        {
            try
            {
                functionDelegate = Marshal.GetDelegateForFunctionPointer< T >( functionPtr );

                _loadedFunctions.Add( functionName, functionDelegate );

                return true;
            }
            catch ( Exception ex )
            {
                throw new GdxRuntimeException( $"Error creating delegate for {functionName}: {ex.Message}" );
            }
        }

        throw new GdxRuntimeException( $"Failed to load {functionName}" );
    }

    // ========================================================================
    // ========================================================================

    public static T GetDelegateFor< T >( string functionName ) where T : Delegate
    {
        var functionPtr = wglGetProcAddress( functionName );

        if ( functionPtr == IntPtr.Zero )
        {
            functionPtr = Glfw.GetProcAddress( functionName );
        }

        if ( functionPtr != IntPtr.Zero )
        {
            try
            {
                var functionDelegate = Marshal.GetDelegateForFunctionPointer< T >( functionPtr );

                _loadedFunctions.Add( functionName, functionDelegate );

                return functionDelegate;
            }
            catch ( Exception ex )
            {
                throw new GdxRuntimeException( $"Error creating delegate for {functionName}: {ex.Message}" );
            }
        }

        throw new GdxRuntimeException( $"Failed to load {functionName}" );
    }

    // ========================================================================
    // ========================================================================
}