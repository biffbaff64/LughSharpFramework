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

using LughSharp.Lugh.Utils;
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

    [DllImport( "opengl32.dll", EntryPoint = "wglGetProcAddress", CallingConvention = CallingConvention.StdCall )]
    private static extern IntPtr wglGetProcAddress( string procname );

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
                Logger.Debug( $"Error creating delegate for {functionName}: {ex.Message}" );

                functionDelegate = null!;

                return false;
            }
        }

        Logger.Debug( $"Failed to load {functionName}" );

        functionDelegate = null!;

        return false;
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