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

using System.Runtime.InteropServices;

using JetBrains.Annotations;

using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Images;
using LughSharp.Core.Maths;
using LughSharp.Core.Utils.Exceptions;

namespace Extensions.Source.Freetype;

[PublicAPI]
public class FreeType
{
    public const int FtPixelModeNone                  = 0;
    public const int FtPixelModeMono                  = 1;
    public const int FtPixelModeGray                  = 2;
    public const int FtPixelModeGray2                 = 3;
    public const int FtPixelModeGray4                 = 4;
    public const int FtPixelModeLcd                   = 5;
    public const int FtPixelModeLcdV                 = 6;
    public const int FtFaceFlagScalable               = 1 << 0;
    public const int FtFaceFlagFixedSizes            = 1 << 1;
    public const int FtFaceFlagFixedWidth            = 1 << 2;
    public const int FtFaceFlagSfnt                   = 1 << 3;
    public const int FtFaceFlagHorizontal             = 1 << 4;
    public const int FtFaceFlagVertical               = 1 << 5;
    public const int FtFaceFlagKerning                = 1 << 6;
    public const int FtFaceFlagFastGlyphs            = 1 << 7;
    public const int FtFaceFlagMultipleMasters       = 1 << 8;
    public const int FtFaceFlagGlyphNames            = 1 << 9;
    public const int FtFaceFlagExternalStream        = 1 << 10;
    public const int FtFaceFlagHinter                 = 1 << 11;
    public const int FtFaceFlagCidKeyed              = 1 << 12;
    public const int FtFaceFlagTricky                 = 1 << 13;
    public const int FtStyleFlagItalic                = 1 << 0;
    public const int FtStyleFlagBold                  = 1 << 1;
    public const int FtLoadDefault                     = 0x0;
    public const int FtLoadNoScale                    = 0x1;
    public const int FtLoadNoHinting                  = 0x2;
    public const int FtLoadRender                      = 0x4;
    public const int FtLoadNoBitmap                   = 0x8;
    public const int FtLoadVerticalLayout             = 0x10;
    public const int FtLoadForceAutohint              = 0x20;
    public const int FtLoadCropBitmap                 = 0x40;
    public const int FtLoadPedantic                    = 0x80;
    public const int FtLoadIgnoreGlobalAdvanceWidth = 0x200;
    public const int FtLoadNoRecurse                  = 0x400;
    public const int FtLoadIgnoreTransform            = 0x800;
    public const int FtLoadMonochrome                  = 0x1000;
    public const int FtLoadLinearDesign               = 0x2000;
    public const int FtLoadNoAutohint                 = 0x8000;
    public const int FtLoadTargetNormal               = 0x0;
    public const int FtLoadTargetLight                = 0x10000;
    public const int FtLoadTargetMono                 = 0x20000;
    public const int FtLoadTargetLcd                  = 0x30000;
    public const int FtLoadTargetLcdV                = 0x40000;
    public const int FtRenderModeNormal               = 0;
    public const int FtRenderModeLight                = 1;
    public const int FtRenderModeMono                 = 2;
    public const int FtRenderModeLcd                  = 3;
    public const int FtRenderModeLcdV                = 4;
    public const int FtRenderModeMax                  = 5;
    public const int FtKerningDefault                  = 0;
    public const int FtKerningUnfitted                 = 1;
    public const int FtKerningUnscaled                 = 2;
    public const int FtStrokerLinecapButt             = 0;
    public const int FtStrokerLinecapRound            = 1;
    public const int FtStrokerLinecapSquare           = 2;
    public const int FtStrokerLinejoinRound           = 0;
    public const int FtStrokerLinejoinBevel           = 1;
    public const int FtStrokerLinejoinMiterVariable  = 2;
    public const int FtStrokerLinejoinMiter           = FtStrokerLinejoinMiterVariable;
    public const int FtStrokerLinejoinMiterFixed     = 3;

    // ========================================================================

    public readonly int FtEncodingAdobeCustom   = Encode( 'A', 'D', 'B', 'C' );
    public readonly int FtEncodingAdobeExpert   = Encode( 'A', 'D', 'B', 'E' );
    public readonly int FtEncodingAdobeLatin1   = Encode( 'l', 'a', 't', '1' );
    public readonly int FtEncodingAdobeStandard = Encode( 'A', 'D', 'O', 'B' );
    public readonly int FtEncodingAppleRoman    = Encode( 'a', 'r', 'm', 'n' );
    public readonly int FtEncodingBig5          = Encode( 'b', 'i', 'g', '5' );
    public readonly int FtEncodingGb2312        = Encode( 'g', 'b', ' ', ' ' );
    public readonly int FtEncodingJohab         = Encode( 'j', 'o', 'h', 'a' );
    public readonly int FtEncodingMsSymbol      = Encode( 's', 'y', 'm', 'b' );
    public readonly int FtEncodingOldLatin2     = Encode( 'l', 'a', 't', '2' );
    public readonly int FtEncodingSjis          = Encode( 's', 'j', 'i', 's' );
    public readonly int FtEncodingUnicode       = Encode( 'u', 'n', 'i', 'c' );
    public readonly int FtEncodingWansung       = Encode( 'w', 'a', 'n', 's' );
    public readonly int FtEncodingNone;

    // ========================================================================

    private const string NativeLib = "lib/net8.0/freetype.dll";

    private static int _lastError;

    // ========================================================================

    /// <summary>
    /// Returns the last error code FreeType reported.
    /// </summary>
    public static int GetLastErrorCode()
    {
        return GetLastErrorCodeNative();

        [DllImport( NativeLib, EntryPoint = "getLastErrorCode", CallingConvention = CallingConvention.Cdecl )]
        static extern int GetLastErrorCodeNative();
    }

    public static Library InitFreeType()
    {
        // In C#, DllImport typically handles the loading of the shared library
        // so SharedLibraryLoader().load("gdx-freetype"); is not directly translated.
        // You ensure the DLL/SO/DYLIB is in a place where the .NET runtime can find it.

        IntPtr address = InitFreeTypeJniNative();

        if ( address == IntPtr.Zero )
        {
            throw new RuntimeException( "Couldn't initialize FreeType library, FreeType error code: " +
                                        GetLastErrorCode() );
        }

        return new Library( address );

        // --------------------------------------

        [DllImport( NativeLib, EntryPoint = "initFreeTypeJni", CallingConvention = CallingConvention.Cdecl )]
        static extern IntPtr InitFreeTypeJniNative();
    }

    private static int Encode( char a, char b, char c, char d )
    {
        return ( a << 24 ) | ( b << 16 ) | ( c << 8 ) | d;
    }

    // ========================================================================

    [PublicAPI]
    public abstract class Pointer
    {
        public IntPtr Address { get; protected set; }

        protected Pointer( IntPtr address )
        {
            Address = address;
        }
    }

    // ========================================================================

    [PublicAPI]
    public class Library : Pointer, IDisposable
    {
        // Using Dictionary<IntPtr, byte[]> to hold managed byte arrays associated
        // with native addresses. When a font is loaded from memory, FreeType expects
        // the memory to remain valid. The byte[] in C# is managed, so we need to
        // ensure it's not garbage collected while FreeType uses it.
        // This is primarily for fonts loaded via NewMemoryFace.
        public Dictionary< IntPtr, byte[] > FontData { get; private set; } = new();

        internal Library( IntPtr address )
            : base( address )
        {
        }

        public Face NewFace( FileInfo fontFile, int faceIndex )
        {
            byte[]? data = null;

            try
            {
                // Attempt to map the file, if supported and safe.
                // This is a placeholder for your Arcus file mapping logic.
                // In a real scenario, FileHandle.Map() might return a byte[] directly or an IntPtr.
                // For now, let's assume it attempts to read into a byte array.
                // If file mapping isn't truly supported or efficient, reading the stream is the fallback.

//                using ( var stream = File.ReadAllBytes( fontFile.FullName ) )
//                {
//                    using ( var ms = new MemoryStream() )
//                    {
//                        stream.CopyTo( ms );
//                        data = ms.ToArray();
//                    }
//                }
            }
            catch ( RuntimeException )
            {
                // OK to ignore, some platforms do not support file mapping.
                // The original Java code has an explicit try-catch for mapping.
                // For C#, if mapping isn't directly available or fails, we fall back to stream reading.
            }

            if ( ( data == null ) || ( data.Length == 0 ) )
            {
                using ( FileStream input = fontFile.OpenRead() )
                {
                    try
                    {
                        var fileSize = ( int )fontFile.Length;

                        if ( fileSize == 0 )
                        {
//                            data = StreamUtils.CopyStreamToByteArray( input, 1024 * 16 );
                        }
                        else
                        {
                            data = new byte[ fileSize ];
//                            StreamUtils.CopyStream( input, data ); // Assuming a method to copy stream to byte[]
                        }
                    }
                    catch ( IOException ex )
                    {
                        throw new RuntimeException( ex );
                    }
                }
            }

            return NewMemoryFace( data, data.Length, faceIndex );
        }

        public Face NewMemoryFace( byte[] data, int dataSize, int faceIndex )
        {
            // In Java, BufferUtils.newUnsafeByteBuffer copies data.
            // Here, we just use the byte[] directly, but need to pin it for native call.
            return NewMemoryFace( data, faceIndex );
        }

        public Face NewMemoryFace( byte[] buffer, int faceIndex )
        {
            // Pin the byte array to get a stable memory address
            GCHandle handle        = GCHandle.Alloc( buffer, GCHandleType.Pinned );
            IntPtr   pinnedAddress = handle.AddrOfPinnedObject();

            IntPtr faceAddress = NewMemoryFaceNative( Address, pinnedAddress, buffer.Length, faceIndex );

            // Unpin the handle immediately after the native call, as we'll manage the buffer's lifetime
            // through the FontData dictionary. This is a bit tricky, the original Java implies
            // that FreeType might take ownership/reference the memory.
            // If FreeType truly needs the buffer for the lifetime of the Face, we need to keep it pinned,
            // or ensure it's a "native" buffer. The original `fontData.put(face, buffer)` suggests
            // the Java `Buffer< byte >` is managed by the `Library` and disposed later.
            // For direct `byte[]` in C#, it's usually safest to copy to unmanaged memory if FreeType keeps a pointer.
            // For simplicity here, let's assume FreeType makes an internal copy or the managed byte[] must stay.
            // The original code uses `BufferUtils.newUnsafeByteBuffer`, which suggests unmanaged memory.
            // Let's adapt that more closely.

            // Re-doing NewMemoryFace to use unmanaged memory as in Java's BufferUtils.newUnsafeByteBuffer
//TODO:            IntPtr unmanagedBufferAddress = BufferUtils.NewUnsafeByteBuffer( buffer.Length );
//TODO:            Marshal.Copy( buffer, 0, unmanagedBufferAddress, buffer.Length );

//TODO:            faceAddress = NewMemoryFaceNative( Address, unmanagedBufferAddress, buffer.Length, faceIndex );

            if ( faceAddress == IntPtr.Zero )
            {
//TODO:                BufferUtils.DisposeUnsafeByteBuffer( unmanagedBufferAddress ); // Dispose if face creation fails

                throw new RuntimeException( "Couldn't load font, FreeType error code: " + GetLastErrorCode() );
            }
            else
            {
                // Store the unmanaged buffer address so it can be disposed with the Library
                FontData.Add( faceAddress, null ); // Store null or a marker, actual byte[] is gone.

                // If the original byte[] is needed for some reason, copy it to FontData.
                // But the key is the native address.
//TODO:                return new Face( faceAddress, this, unmanagedBufferAddress ); // Pass the unmanaged buffer address

                return new Face( faceAddress, this );
            }

            [DllImport( NativeLib, EntryPoint = "newMemoryFace", CallingConvention = CallingConvention.Cdecl )]
            static extern IntPtr NewMemoryFaceNative( IntPtr library, IntPtr data, int dataSize, int faceIndex );
        }

        public Stroker CreateStroker()
        {
            IntPtr strokerAddress = StrokerNewNative( Address );

            if ( strokerAddress == IntPtr.Zero )
            {
                throw new RuntimeException( "Couldn't create FreeType stroker, FreeType error code: " +
                                            GetLastErrorCode() );
            }

            return new Stroker( strokerAddress );

            [DllImport( NativeLib, EntryPoint = "strokerNew", CallingConvention = CallingConvention.Cdecl )]
            static extern IntPtr StrokerNewNative( IntPtr library );
        }

        // Finalizer in case Dispose is not called explicitly
        ~Library()
        {
            Dispose( false );
        }

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected virtual void Dispose( bool disposing )
        {
            if ( disposing )
            {
                if ( Address != IntPtr.Zero )
                {
                    DoneFreeTypeNative( Address );

                    Address = IntPtr.Zero; // SetMark as disposed

                    // Dispose of any pinned byte buffers
                    foreach ( byte[] buffer in FontData.Values )
                    {
//TODO:                        if ( BufferUtils.IsUnsafeByteBuffer( buffer ) )
                        {
//TODO:                            BufferUtils.DisposeUnsafeByteBuffer( buffer );
                        }
                    }

                    FontData.Clear();
                }
            }
        }

        [DllImport( NativeLib, EntryPoint = "doneFreeType", CallingConvention = CallingConvention.Cdecl )]
        private static extern void DoneFreeTypeNative( IntPtr library );
    }

    // ========================================================================

    [PublicAPI]
    public class Face : Pointer, IDisposable
    {
        internal Library Library { get; private set; }
        private  IntPtr  _associatedBufferAddress; // Store the address of the unmanaged buffer if created by this face

        internal Face( IntPtr address, Library library, IntPtr associatedBufferAddress = default )
            : base( address )
        {
            Library                  = library;
            _associatedBufferAddress = associatedBufferAddress;
        }

        [DllImport( NativeLib, EntryPoint = "doneFace", CallingConvention = CallingConvention.Cdecl )]
        private static extern void DoneFaceNative( IntPtr face );

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected virtual void Dispose( bool disposing )
        {
            if ( disposing )
            {
                if ( Address != IntPtr.Zero )
                {
                    DoneFaceNative( Address );

                    // If this face was created with an associated unmanaged buffer (NewMemoryFace)
                    if ( ( _associatedBufferAddress != IntPtr.Zero ) && Library.FontData.ContainsKey( Address ) )
                    {
                        Library.FontData.Remove( Address );
//TODO:                        BufferUtils.DisposeUnsafeByteBuffer( _associatedBufferAddress );
                        _associatedBufferAddress = IntPtr.Zero; // SetMark as disposed
                    }

                    Address = IntPtr.Zero; // SetMark as disposed
                }
            }
        }

        ~Face()
        {
            Dispose( false );
        }

        [DllImport( NativeLib, EntryPoint = "getFaceFlags", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetFaceFlagsNative( IntPtr face );

        public int GetFaceFlags()
        {
            return GetFaceFlagsNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getStyleFlags", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetStyleFlagsNative( IntPtr face );

        public int GetStyleFlags()
        {
            return GetStyleFlagsNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getNumGlyphs", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetNumGlyphsNative( IntPtr face );

        public int GetNumGlyphs()
        {
            return GetNumGlyphsNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getAscender", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetAscenderNative( IntPtr face );

        public int GetAscender()
        {
            return GetAscenderNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getDescender", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetDescenderNative( IntPtr face );

        public int GetDescender()
        {
            return GetDescenderNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getHeight", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetHeightNative( IntPtr face );

        public int GetHeight()
        {
            return GetHeightNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getMaxAdvanceWidth", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetMaxAdvanceWidthNative( IntPtr face );

        public int GetMaxAdvanceWidth()
        {
            return GetMaxAdvanceWidthNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getMaxAdvanceHeight", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetMaxAdvanceHeightNative( IntPtr face );

        public int GetMaxAdvanceHeight()
        {
            return GetMaxAdvanceHeightNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getUnderlinePosition", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetUnderlinePositionNative( IntPtr face );

        public int GetUnderlinePosition()
        {
            return GetUnderlinePositionNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getUnderlineThickness", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetUnderlineThicknessNative( IntPtr face );

        public int GetUnderlineThickness()
        {
            return GetUnderlineThicknessNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "selectSize", CallingConvention = CallingConvention.Cdecl )]
        private static extern bool SelectSizeNative( IntPtr face, int strikeIndex );

        public bool SelectSize( int strikeIndex )
        {
            return SelectSizeNative( Address, strikeIndex );
        }

        [DllImport( NativeLib, EntryPoint = "setCharSize", CallingConvention = CallingConvention.Cdecl )]
        private static extern bool SetCharSizeNative( IntPtr face, int charWidth, int charHeight, int horzResolution,
                                                      int vertResolution );

        public bool SetCharSize( int charWidth, int charHeight, int horzResolution, int vertResolution )
        {
            return SetCharSizeNative( Address, charWidth, charHeight, horzResolution, vertResolution );
        }

        [DllImport( NativeLib, EntryPoint = "setPixelSizes", CallingConvention = CallingConvention.Cdecl )]
        private static extern bool SetPixelSizesNative( IntPtr face, int pixelWidth, int pixelHeight );

        public bool SetPixelSizes( int pixelWidth, int pixelHeight )
        {
            return SetPixelSizesNative( Address, pixelWidth, pixelHeight );
        }

        [DllImport( NativeLib, EntryPoint = "loadGlyph", CallingConvention = CallingConvention.Cdecl )]
        private static extern bool LoadGlyphNative( IntPtr face, int glyphIndex, int loadFlags );

        public bool LoadGlyph( int glyphIndex, int loadFlags )
        {
            return LoadGlyphNative( Address, glyphIndex, loadFlags );
        }

        [DllImport( NativeLib, EntryPoint = "loadChar", CallingConvention = CallingConvention.Cdecl )]
        private static extern bool LoadCharNative( IntPtr face, int charCode, int loadFlags );

        public bool LoadChar( int charCode, int loadFlags )
        {
            return LoadCharNative( Address, charCode, loadFlags );
        }

        [DllImport( NativeLib, EntryPoint = "getGlyph", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr GetGlyphNative( IntPtr face );

        public GlyphSlot GetGlyph()
        {
            return new GlyphSlot( GetGlyphNative( Address ) );
        }

        [DllImport( NativeLib, EntryPoint = "getSize", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr GetSizeNative( IntPtr face );

        public Size GetSize()
        {
            return new Size( GetSizeNative( Address ) );
        }

        [DllImport( NativeLib, EntryPoint = "hasKerning", CallingConvention = CallingConvention.Cdecl )]
        private static extern bool HasKerningNative( IntPtr face );

        public bool HasKerning()
        {
            return HasKerningNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getKerning", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetKerningNative( IntPtr face, int leftGlyph, int rightGlyph, int kernMode );

        public int GetKerning( int leftGlyph, int rightGlyph, int kernMode )
        {
            return GetKerningNative( Address, leftGlyph, rightGlyph, kernMode );
        }

        [DllImport( NativeLib, EntryPoint = "getCharIndex", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetCharIndexNative( IntPtr face, int charCode );

        public int GetCharIndex( int charCode )
        {
            return GetCharIndexNative( Address, charCode );
        }
    }

    // ========================================================================

    [PublicAPI]
    public class Size : Pointer
    {
        internal Size( IntPtr address )
            : base( address )
        {
        }

        [DllImport( NativeLib, EntryPoint = "getMetrics", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr GetMetricsNative( IntPtr address );

        public SizeMetrics GetMetrics()
        {
            return new SizeMetrics( GetMetricsNative( Address ) );
        }
    }

    // ========================================================================

    [PublicAPI]
    public class SizeMetrics : Pointer
    {
        internal SizeMetrics( IntPtr address )
            : base( address )
        {
        }

        [DllImport( NativeLib, EntryPoint = "getXppem", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetXppemNative( IntPtr metrics );

        public int GetXppem()
        {
            return GetXppemNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getYppem", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetYppemNative( IntPtr metrics );

        public int GetYppem()
        {
            return GetYppemNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getXscale", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetXScaleNative( IntPtr metrics );

        public int GetXScale()
        {
            return GetXScaleNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getYscale", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetYscaleNative( IntPtr metrics );

        public int GetYscale()
        {
            return GetYscaleNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getAscender", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetAscenderNative( IntPtr metrics );

        public int GetAscender()
        {
            return GetAscenderNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getDescender", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetDescenderNative( IntPtr metrics );

        public int GetDescender()
        {
            return GetDescenderNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getHeight", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetHeightNative( IntPtr metrics );

        public int GetHeight()
        {
            return GetHeightNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getMaxAdvance", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetMaxAdvanceNative( IntPtr metrics );

        public int GetMaxAdvance()
        {
            return GetMaxAdvanceNative( Address );
        }
    }

    // ========================================================================

    [PublicAPI]
    public class GlyphSlot : Pointer
    {
        internal GlyphSlot( IntPtr address )
            : base( address )
        {
        }

        [DllImport( NativeLib, EntryPoint = "getMetrics", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr GetMetricsNative( IntPtr slot );

        public GlyphMetrics GetMetrics()
        {
            return new GlyphMetrics( GetMetricsNative( Address ) );
        }

        [DllImport( NativeLib, EntryPoint = "getLinearHoriAdvance", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetLinearHoriAdvanceNative( IntPtr slot );

        public int GetLinearHoriAdvance()
        {
            return GetLinearHoriAdvanceNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getLinearVertAdvance", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetLinearVertAdvanceNative( IntPtr slot );

        public int GetLinearVertAdvance()
        {
            return GetLinearVertAdvanceNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getAdvanceX", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetAdvanceXNative( IntPtr slot );

        public int GetAdvanceX()
        {
            return GetAdvanceXNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getAdvanceY", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetAdvanceYNative( IntPtr slot );

        public int GetAdvanceY()
        {
            return GetAdvanceYNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getFormat", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetFormatNative( IntPtr slot );

        public int GetFormat()
        {
            return GetFormatNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getBitmap", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr GetBitmapNative( IntPtr slot );

        public Bitmap GetBitmap()
        {
            return new Bitmap( GetBitmapNative( Address ) );
        }

        [DllImport( NativeLib, EntryPoint = "getBitmapLeft", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetBitmapLeftNative( IntPtr slot );

        public int GetBitmapLeft()
        {
            return GetBitmapLeftNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getBitmapTop", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetBitmapTopNative( IntPtr slot );

        public int GetBitmapTop()
        {
            return GetBitmapTopNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "renderGlyph", CallingConvention = CallingConvention.Cdecl )]
        private static extern bool RenderGlyphNative( IntPtr slot, int renderMode );

        public bool RenderGlyph( int renderMode )
        {
            return RenderGlyphNative( Address, renderMode );
        }

        [DllImport( NativeLib, EntryPoint = "getGlyph", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr GetGlyphFromSlotNative( IntPtr glyphSlot );

        public Glyph GetGlyph()
        {
            IntPtr glyphAddress = GetGlyphFromSlotNative( Address );

            if ( glyphAddress == IntPtr.Zero )
            {
                throw new RuntimeException( "Couldn't get glyph, FreeType error code: " + GetLastErrorCode() );
            }

            return new Glyph( glyphAddress );
        }
    }

    // ========================================================================

    [PublicAPI]
    public class Glyph : Pointer, IDisposable
    {
        private bool _rendered;

        internal Glyph( IntPtr address )
            : base( address )
        {
        }

        [DllImport( NativeLib, EntryPoint = "done", CallingConvention = CallingConvention.Cdecl )]
        private static extern void DoneNative( IntPtr glyph );

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected virtual void Dispose( bool disposing )
        {
            if ( disposing )
            {
                if ( Address != IntPtr.Zero )
                {
                    DoneNative( Address );
                    Address = IntPtr.Zero; // SetMark as disposed
                }
            }
        }

        ~Glyph()
        {
            Dispose( false );
        }

        [DllImport( NativeLib, EntryPoint = "strokeBorder", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr StrokeBorderNative( IntPtr glyph, IntPtr stroker, bool inside );

        public void StrokeBorder( Stroker stroker, bool inside )
        {
            Address = StrokeBorderNative( Address, stroker.Address, inside );
        }

        [DllImport( NativeLib, EntryPoint = "toBitmap", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr ToBitmapNative( IntPtr glyph, int renderMode );

        public void ToBitmap( int renderMode )
        {
            IntPtr bitmapAddress = ToBitmapNative( Address, renderMode );

            if ( bitmapAddress == IntPtr.Zero )
            {
                throw new RuntimeException( "Couldn't render glyph, FreeType error code: " + GetLastErrorCode() );
            }

            Address   = bitmapAddress;
            _rendered = true;
        }

        [DllImport( NativeLib, EntryPoint = "getBitmap", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr GetBitmapFromGlyphNative( IntPtr glyph );

        public Bitmap GetBitmap()
        {
            if ( !_rendered )
            {
                throw new RuntimeException( "Glyph is not yet rendered" );
            }

            return new Bitmap( GetBitmapFromGlyphNative( Address ) );
        }

        [DllImport( NativeLib, EntryPoint = "getLeft", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetLeftNative( IntPtr glyph );

        public int GetLeft()
        {
            if ( !_rendered )
            {
                throw new RuntimeException( "Glyph is not yet rendered" );
            }

            return GetLeftNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getTop", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetTopNative( IntPtr glyph );

        public int GetTop()
        {
            if ( !_rendered )
            {
                throw new RuntimeException( "Glyph is not yet rendered" );
            }

            return GetTopNative( Address );
        }
    }

    // ========================================================================

    [PublicAPI]
    public class Bitmap : Pointer
    {
        internal Bitmap( IntPtr address )
            : base( address )
        {
        }

        public int GetRows()
        {
            return GetRowsNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getRows", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetRowsNative( IntPtr bitmap );

        public int GetWidth()
        {
            return GetWidthNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getWidth", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetWidthNative( IntPtr bitmap );

        public int GetPitch()
        {
            return GetPitchNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getPitch", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetPitchNative( IntPtr bitmap );

        public byte[] GetBuffer()
        {
            int rows  = GetRows();
            int width = GetWidth();
            int pitch = Math.Abs( GetPitch() );

            if ( rows == 0 )
            {
                // Return a dummy non-null, non-zero buffer as per original Java comment
                return new byte[ 1 ];
            }

            IntPtr nativeBufferPtr = GetBufferNative( Address );

            if ( nativeBufferPtr == IntPtr.Zero )
            {
                // This case should ideally not happen if rows > 0, but as a safeguard.
                return new byte[ 1 ];
            }

            var managedBuffer = new byte[ rows * pitch ];

            // Copy data from native memory to managed byte array
            Marshal.Copy( nativeBufferPtr, managedBuffer, 0, managedBuffer.Length );

            return managedBuffer;
        }

        [DllImport( NativeLib, EntryPoint = "getBuffer", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr GetBufferNative( IntPtr bitmap ); // Returns a pointer to the native buffer

        public Pixmap GetPixmap( int format, Color color, float gamma )
        {
            int    width     = GetWidth();
            int    rows      = GetRows();
            byte[] src       = GetBuffer(); // Get the managed byte array copy of the native buffer
            int    pixelMode = GetPixelMode();
            int    rowBytes  = Math.Abs( GetPitch() );
            Pixmap pixmap;

            if ( ( color == Color.White )
              && ( pixelMode == FtPixelModeGray )
              && ( rowBytes == width )
              && ( Math.Abs( gamma - 1f ) < NumberUtils.FloatTolerance ) )
            {
                pixmap = new Pixmap( width, rows, LughFormat.Alpha );

                // Assuming Pixmap has a way to directly set pixel data from a byte array
                // or BufferUtils.copy can handle byte[] to Pixmap's internal buffer
//TODO:                BufferUtils.Copy< byte >( src, 0, pixmap.ByteBuffer, 0, src.Length );
            }
            else
            {
                pixmap = new Pixmap( width, rows, LughFormat.RGBA8888 );

                uint rgba   = Color.ToRgba8888( color ); // Assuming Color has a ToRGBA8888 method
                var  srcRow = new byte[ rowBytes ];
                var  dstRow = new int[ width ];

                // Directly access Pixmap's pixel buffer, assuming it's a Buffer< byte > or similar
                // and can be wrapped by an Buffer< int > for efficiency.
                // For direct access and potentially better performance, you might need unsafe code
                // or a Pixmap method that accepts an int[] directly.
                // Here, we'll assume a method to write a row of int[] to the Pixmap.

                // Assuming Pixmap.SetPixels(int[] rowData, int offset, int length) or similar
                // or a way to get a direct int[] view of the Pixmap's buffer.
                // For simplicity, I'll provide a conceptual approach for writing rows.

                if ( pixelMode == FtPixelModeMono )
                {
                    for ( var y = 0; y < rows; y++ )
                    {
                        Array.Copy( src, y * rowBytes, srcRow, 0, rowBytes );

                        for ( int i = 0, x = 0; x < width; i++, x += 8 )
                        {
                            byte b = srcRow[ i ];

                            for ( int ii = 0, n = Math.Min( 8, width - x ); ii < n; ii++ )
                            {
                                if ( ( b & ( 1 << ( 7 - ii ) ) ) != 0 )
                                {
                                    dstRow[ x + ii ] = ( int )rgba;
                                }
                                else
                                {
                                    dstRow[ x + ii ] = 0;
                                }
                            }
                        }

//TODO:                        pixmap.SetPixels( y, dstRow ); // Conceptual: Set a row of pixels
                    }
                }
                else
                {
                    uint rgb = rgba & 0xffffff00;
                    uint a   = rgba & 0xff;

                    for ( var y = 0; y < rows; y++ )
                    {
                        Array.Copy( src, y * rowBytes, srcRow, 0, rowBytes );

                        for ( var x = 0; x < width; x++ )
                        {
                            int alpha = srcRow[ x ] & 0xff;

                            if ( alpha == 0 )
                            {
                                dstRow[ x ] = ( int )rgb;
                            }
                            else if ( alpha == 255 )
                            {
                                dstRow[ x ] = ( int )( rgb | a );
                            }
                            else
                            {
                                // Inverse gamma.
                                dstRow[ x ] = ( int )( rgb | ( uint )( a * ( float )Math.Pow( alpha / 255f, gamma ) ) );
                            }
                        }

//TODO:                        pixmap.SetPixels( y, dstRow ); // Conceptual: Set a row of pixels
                    }
                }
            }

            Pixmap converted = pixmap;

            if ( format != pixmap.GLPixelFormat )
            {
                converted          = new Pixmap( pixmap.Width, pixmap.Height, format );
                converted.Blending = Pixmap.BlendType.None;       // Assuming SetBlending method
                converted.DrawPixmap( pixmap, 0, 0 );             // Assuming DrawPixmap method
                converted.Blending = Pixmap.BlendType.SourceOver; // Assuming SetBlending method

                pixmap.Dispose();
            }

            return converted;
        }

        [DllImport( NativeLib, EntryPoint = "getNumGray", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetNumGrayNative( IntPtr bitmap );

        public int GetNumGray()
        {
            return GetNumGrayNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getPixelMode", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetPixelModeNative( IntPtr bitmap );

        public int GetPixelMode()
        {
            return GetPixelModeNative( Address );
        }
    }

    // ========================================================================

    [PublicAPI]
    public class GlyphMetrics : Pointer
    {
        internal GlyphMetrics( IntPtr address )
            : base( address )
        {
        }

        [DllImport( NativeLib, EntryPoint = "getWidth", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetWidthNative( IntPtr metrics );

        public int GetWidth()
        {
            return GetWidthNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getHeight", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetHeightNative( IntPtr metrics );

        public int GetHeight()
        {
            return GetHeightNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getHoriBearingX", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetHoriBearingXNative( IntPtr metrics );

        public int GetHoriBearingX()
        {
            return GetHoriBearingXNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getHoriBearingY", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetHoriBearingYNative( IntPtr metrics );

        public int GetHoriBearingY()
        {
            return GetHoriBearingYNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getHoriAdvance", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetHoriAdvanceNative( IntPtr metrics );

        public int GetHoriAdvance()
        {
            return GetHoriAdvanceNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getVertBearingX", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetVertBearingXNative( IntPtr metrics );

        public int GetVertBearingX()
        {
            return GetVertBearingXNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getVertBearingY", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetVertBearingYNative( IntPtr metrics );

        public int GetVertBearingY()
        {
            return GetVertBearingYNative( Address );
        }

        [DllImport( NativeLib, EntryPoint = "getVertAdvance", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetVertAdvanceNative( IntPtr metrics );

        public int GetVertAdvance()
        {
            return GetVertAdvanceNative( Address );
        }
    }

    // ========================================================================

    [PublicAPI]
    public class Stroker : Pointer, IDisposable
    {
        internal Stroker( IntPtr address )
            : base( address )
        {
        }

        [DllImport( NativeLib, EntryPoint = "set", CallingConvention = CallingConvention.Cdecl )]
        private static extern void SetNative( IntPtr stroker, int radius, int lineCap, int lineJoin, int miterLimit );

        public void Set( int radius, int lineCap, int lineJoin, int miterLimit )
        {
            SetNative( Address, radius, lineCap, lineJoin, miterLimit );
        }

        [DllImport( NativeLib, EntryPoint = "done", CallingConvention = CallingConvention.Cdecl )]
        private static extern void DoneStrokerNative( IntPtr stroker );

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected virtual void Dispose( bool disposing )
        {
            if ( disposing )
            {
                if ( Address != IntPtr.Zero )
                {
                    DoneStrokerNative( Address );
                    Address = IntPtr.Zero; // SetMark as disposed
                }
            }
        }

        ~Stroker()
        {
            Dispose( false );
        }
    }
}

// ============================================================================
// ============================================================================