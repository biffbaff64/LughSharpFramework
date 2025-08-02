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

namespace LughSharp.Lugh.Graphics.OpenGL.Stbi;

[PublicAPI]
public class StbImage : IDisposable
{
    public int    Width       { get; private set; }
    public int    Height      { get; private set; }
    public int    Comp        { get; private set; } // Number of components (e.g., 3 for RGB, 4 for RGBA)
    public IntPtr Data        { get; private set; } // Pointer to the unmanaged pixel data
    public bool   IsFloatData { get; private set; }

    // ========================================================================

    // Private constructor to force static creation methods
    private StbImage()
    {
    }

    public static StbImage Load( string filePath, int requiredComponents = StbImageNative.STBI_DEFAULT )
    {
        if ( !File.Exists( filePath ) )
        {
            throw new FileNotFoundException( $"Image file not found: {filePath}" );
        }

        var dataPtr = StbImageNative.LoadImageFromFile( filePath, out var x, out var y, out var comp, requiredComponents );

        if ( dataPtr == IntPtr.Zero )
        {
            throw new InvalidOperationException( $"Failed to load image from file: {filePath}. stb_image error?" );
        }

        return new StbImage
        {
            Width       = x,
            Height      = y,
            Comp        = comp,
            Data        = dataPtr,
            IsFloatData = false, // Default for STBI_load
        };
    }

    public static StbImage LoadFromMemory( byte[] imageData, int requiredComponents = StbImageNative.STBI_DEFAULT )
    {
        if ( ( imageData == null ) || ( imageData.Length == 0 ) )
        {
            throw new ArgumentException( "Image data cannot be null or empty.", nameof( imageData ) );
        }

        int    x;
        int    y;
        int    comp;
        IntPtr dataPtr;

        // Pin the byte array in memory so the garbage collector doesn't move it
        var gcHandle = GCHandle.Alloc( imageData, GCHandleType.Pinned );

        try
        {
            var pinnedPtr = gcHandle.AddrOfPinnedObject();

            dataPtr = StbImageNative.LoadImageFromMemory( pinnedPtr, imageData.Length, out x, out y, out comp, requiredComponents );
        }
        finally
        {
            if ( gcHandle.IsAllocated )
            {
                gcHandle.Free();
            }
        }

        if ( dataPtr == IntPtr.Zero )
        {
            throw new InvalidOperationException( "Failed to load image from memory. stb_image error?" );
        }

        return new StbImage
        {
            Width       = x,
            Height      = y,
            Comp        = comp,
            Data        = dataPtr,
            IsFloatData = false
        };
    }

    public static StbImage LoadHdr( string filePath, int requiredComponents = StbImageNative.STBI_DEFAULT )
    {
        if ( !File.Exists( filePath ) )
        {
            throw new FileNotFoundException( $"HDR image file not found: {filePath}" );
        }

        var dataPtr = StbImageNative.LoadImageFromFileHDR( filePath, out var x, out var y, out var comp, requiredComponents );

        if ( dataPtr == IntPtr.Zero )
        {
            throw new InvalidOperationException( $"Failed to load HDR image from file: {filePath}. stb_image error?" );
        }

        return new StbImage
        {
            Width       = x,
            Height      = y,
            Comp        = comp,
            Data        = dataPtr,
            IsFloatData = true
        };
    }

    // Method to convert the unmanaged data to a managed byte array
    // IMPORTANT: This creates a copy! For OpenGL, you might pass Data directly.
    public byte[]? GetPixelsByte()
    {
        if ( ( Data == IntPtr.Zero ) || IsFloatData )
        {
            return null;
        }

        var pixelCount = Width * Height * Comp;
        var pixels     = new byte[ pixelCount ];

        Marshal.Copy( Data, pixels, 0, pixelCount );

        return pixels;
    }

    // Method to convert the unmanaged float data to a managed float array
    public float[]? GetPixelsFloat()
    {
        if ( ( Data == IntPtr.Zero ) || !IsFloatData )
        {
            return null;
        }

        var pixelCount = Width * Height * Comp;
        var pixels     = new float[ pixelCount ];
        
        Marshal.Copy( Data, pixels, 0, pixelCount );

        return pixels;
    }

    // Essential: Dispose of the unmanaged memory when done!
    public void Dispose()
    {
        if ( Data != IntPtr.Zero )
        {
            StbImageNative.FreeImageData( Data );
            Data = IntPtr.Zero; // Prevent double-free
        }

        GC.SuppressFinalize( this ); // Tell GC not to call finalizer if Dispose is called
    }

    // Finalizer (destructor) as a fallback for memory cleanup
    ~StbImage()
    {
        Dispose();
    }
}

// ========================================================================
// ========================================================================