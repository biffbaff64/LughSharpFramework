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

using LughSharp.Core.Graphics;
using LughSharp.Core.Utils;
using Color = LughSharp.Core.Graphics.Color;

namespace Extensions.Source.Freetype;

[PublicAPI]
public partial class FreeType
{
    private const string NATIVE_LIB = "lib/net8.0/freetype.dll";

//    private static int _lastError = 0;
//
//    /// <summary>
//    /// Returns the last error code FreeType reported.
//    /// </summary>
//    [DllImport( NativeLib, EntryPoint = "getLastErrorCode", CallingConvention = CallingConvention.Cdecl )]
//    private static extern int GetLastErrorCodeNative();
//
//    public static int GetLastErrorCode()
//    {
//        return GetLastErrorCodeNative();
//    }
//
//    public abstract class Pointer
//    {
//        public IntPtr Address { get; protected set; }
//
//        protected Pointer( IntPtr address )
//        {
//            this.Address = address;
//        }
//    }
//
//    public class Library : Pointer, IDisposable
//    {
//        // Using Dictionary<IntPtr, byte[]> to hold managed byte arrays associated with native addresses.
//        // When a font is loaded from memory, FreeType expects the memory to remain valid.
//        // The byte[] in C# is managed, so we need to ensure it's not garbage collected while FreeType uses it.
//        // This is primarily for fonts loaded via newMemoryFace.
//        public Dictionary< IntPtr, byte[] > FontData { get; private set; } = new Dictionary< IntPtr, byte[] >();
//
//        internal Library( IntPtr address )
//            : base( address )
//        {
//        }
//
//        [DllImport( NativeLib, EntryPoint = "doneFreeType", CallingConvention = CallingConvention.Cdecl )]
//        private static extern void DoneFreeTypeNative( IntPtr library );
//
//        public void Dispose()
//        {
//            Dispose( true );
//            GC.SuppressFinalize( this );
//        }
//
//        protected virtual void Dispose( bool disposing )
//        {
//            if ( disposing )
//            {
//                if ( Address != IntPtr.Zero )
//                {
//                    DoneFreeTypeNative( Address );
//                    Address = IntPtr.Zero; // SetMark as disposed
//
//                    // Dispose of any pinned byte buffers
//                    foreach ( var buffer in FontData.Values )
//                    {
//                        if ( BufferUtils.IsUnsafeByteBuffer( buffer ) )
//                        {
//                            BufferUtils.DisposeUnsafeByteBuffer( buffer );
//                        }
//                    }
//
//                    FontData.Clear();
//                }
//            }
//        }
//
//        // Finalizer in case Dispose is not called explicitly
//        ~Library()
//        {
//            Dispose( false );
//        }
//
//        public Face NewFace( FileInfo fontFile, int faceIndex )
//        {
//            byte[]? data = null;
//
//            try
//            {
//                // Attempt to map the file, if supported and safe.
//                // This is a placeholder for your LughSharp file mapping logic.
//                // In a real scenario, FileHandle.Map() might return a byte[] directly or an IntPtr.
//                // For now, let's assume it attempts to read into a byte array.
//                // If file mapping isn't truly supported or efficient, reading the stream is the fallback.
//                using ( var stream = fontFile.Map() )
//                {
//                    using ( var ms = new MemoryStream() )
//                    {
//                        stream.CopyTo( ms );
//                        data = ms.ToArray();
//                    }
//                }
//            }
//            catch ( GdxRuntimeException )
//            {
//                // OK to ignore, some platforms do not support file mapping.
//                // The original Java code has an explicit try-catch for mapping.
//                // For C#, if mapping isn't directly available or fails, we fall back to stream reading.
//            }
//
//            if ( ( data == null ) || ( data.Length == 0 ) )
//            {
//                using ( var input = fontFile.Read() )
//                {
//                    try
//                    {
//                        var fileSize = ( int )fontFile.Length();
//
//                        if ( fileSize == 0 )
//                        {
//                            data = StreamUtils.CopyStreamToByteArray( input, 1024 * 16 );
//                        }
//                        else
//                        {
//                            data = new byte[ fileSize ];
//                            StreamUtils.CopyStream( input, data ); // Assuming a method to copy stream to byte[]
//                        }
//                    }
//                    catch ( IOException ex )
//                    {
//                        throw new GdxRuntimeException( ex );
//                    }
//                }
//            }
//
//            return NewMemoryFace( data, data.Length, faceIndex );
//        }
//
//        public Face NewMemoryFace( byte[] data, int dataSize, int faceIndex )
//        {
//            // In Java, BufferUtils.newUnsafeByteBuffer copies data.
//            // Here, we just use the byte[] directly, but need to pin it for native call.
//            return NewMemoryFace( data, faceIndex );
//        }
//
//        public Face NewMemoryFace( byte[] buffer, int faceIndex )
//        {
//            // Pin the byte array to get a stable memory address
//            var handle        = GCHandle.Alloc( buffer, GCHandleType.Pinned );
//            var   pinnedAddress = handle.AddrOfPinnedObject();
//
//            var faceAddress = NewMemoryFaceNative( Address, pinnedAddress, buffer.Length, faceIndex );
//
//            // Unpin the handle immediately after the native call, as we'll manage the buffer's lifetime
//            // through the FontData dictionary. This is a bit tricky, the original Java implies
//            // that FreeType might take ownership/reference the memory.
//            // If FreeType truly needs the buffer for the lifetime of the Face, we need to keep it pinned,
//            // or ensure it's a "native" buffer. The original `fontData.put(face, buffer)` suggests
//            // the Java `Buffer< byte >` is managed by the `Library` and disposed later.
//            // For direct `byte[]` in C#, it's usually safest to copy to unmanaged memory if FreeType keeps a pointer.
//            // For simplicity here, let's assume FreeType makes an internal copy or the managed byte[] must stay.
//            // The original code uses `BufferUtils.newUnsafeByteBuffer`, which suggests unmanaged memory.
//            // Let's adapt that more closely.
//
//            // Re-doing NewMemoryFace to use unmanaged memory as in Java's BufferUtils.newUnsafeByteBuffer
//            IntPtr unmanagedBufferAddress = BufferUtils.NewUnsafeByteBuffer( buffer.Length );
//            Marshal.Copy( buffer, 0, unmanagedBufferAddress, buffer.Length );
//
//            faceAddress = NewMemoryFaceNative( Address, unmanagedBufferAddress, buffer.Length, faceIndex );
//
//            if ( faceAddress == IntPtr.Zero )
//            {
//                BufferUtils.DisposeUnsafeByteBuffer( unmanagedBufferAddress ); // Dispose if face creation fails
//
//                throw new GdxRuntimeException( "Couldn't load font, FreeType error code: " + GetLastErrorCode() );
//            }
//            else
//            {
//                // Store the unmanaged buffer address so it can be disposed with the Library
//                FontData.Add( faceAddress, null ); // Store null or a marker, actual byte[] is gone.
//
//                // If the original byte[] is needed for some reason, copy it to FontData.
//                // But the key is the native address.
//                return new Face( faceAddress, this, unmanagedBufferAddress ); // Pass the unmanaged buffer address
//            }
//        }
//
//        [DllImport( NativeLib, EntryPoint = "newMemoryFace", CallingConvention = CallingConvention.Cdecl )]
//        private static extern IntPtr NewMemoryFaceNative( IntPtr library, IntPtr data, int dataSize, int faceIndex );
//
//        public Stroker CreateStroker()
//        {
//            var strokerAddress = StrokerNewNative( Address );
//
//            if ( strokerAddress == IntPtr.Zero )
//            {
//                throw new GdxRuntimeException( "Couldn't create FreeType stroker, FreeType error code: " + GetLastErrorCode() );
//            }
//
//            return new Stroker( strokerAddress );
//        }
//
//        [DllImport( NativeLib, EntryPoint = "strokerNew", CallingConvention = CallingConvention.Cdecl )]
//        private static extern IntPtr StrokerNewNative( IntPtr library );
//    }
//
//    public class Face : Pointer, IDisposable
//    {
//        internal Library Library { get; private set; }
//        private  IntPtr  _associatedBufferAddress; // Store the address of the unmanaged buffer if created by this face
//
//        internal Face( IntPtr address, Library library, IntPtr associatedBufferAddress = default )
//            : base( address )
//        {
//            Library                  = library;
//            _associatedBufferAddress = associatedBufferAddress;
//        }
//
//        [DllImport( NativeLib, EntryPoint = "doneFace", CallingConvention = CallingConvention.Cdecl )]
//        private static extern void DoneFaceNative( IntPtr face );
//
//        public void Dispose()
//        {
//            Dispose( true );
//            GC.SuppressFinalize( this );
//        }
//
//        protected virtual void Dispose( bool disposing )
//        {
//            if ( disposing )
//            {
//                if ( Address != IntPtr.Zero )
//                {
//                    DoneFaceNative( Address );
//
//                    // If this face was created with an associated unmanaged buffer (NewMemoryFace)
//                    if ( ( _associatedBufferAddress != IntPtr.Zero ) && Library.FontData.ContainsKey( Address ) )
//                    {
//                        Library.FontData.Remove( Address );
//                        BufferUtils.DisposeUnsafeByteBuffer( _associatedBufferAddress );
//                        _associatedBufferAddress = IntPtr.Zero; // SetMark as disposed
//                    }
//
//                    Address = IntPtr.Zero; // SetMark as disposed
//                }
//            }
//        }
//
//        ~Face()
//        {
//            Dispose( false );
//        }
//
//        [DllImport( NativeLib, EntryPoint = "getFaceFlags", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetFaceFlagsNative( IntPtr face );
//
//        public int GetFaceFlags() => GetFaceFlagsNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getStyleFlags", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetStyleFlagsNative( IntPtr face );
//
//        public int GetStyleFlags() => GetStyleFlagsNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getNumGlyphs", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetNumGlyphsNative( IntPtr face );
//
//        public int GetNumGlyphs() => GetNumGlyphsNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getAscender", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetAscenderNative( IntPtr face );
//
//        public int GetAscender() => GetAscenderNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getDescender", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetDescenderNative( IntPtr face );
//
//        public int GetDescender() => GetDescenderNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getHeight", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetHeightNative( IntPtr face );
//
//        public int GetHeight() => GetHeightNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getMaxAdvanceWidth", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetMaxAdvanceWidthNative( IntPtr face );
//
//        public int GetMaxAdvanceWidth() => GetMaxAdvanceWidthNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getMaxAdvanceHeight", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetMaxAdvanceHeightNative( IntPtr face );
//
//        public int GetMaxAdvanceHeight() => GetMaxAdvanceHeightNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getUnderlinePosition", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetUnderlinePositionNative( IntPtr face );
//
//        public int GetUnderlinePosition() => GetUnderlinePositionNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getUnderlineThickness", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetUnderlineThicknessNative( IntPtr face );
//
//        public int GetUnderlineThickness() => GetUnderlineThicknessNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "selectSize", CallingConvention = CallingConvention.Cdecl )]
//        private static extern bool SelectSizeNative( IntPtr face, int strike_index );
//
//        public bool SelectSize( int strikeIndex ) => SelectSizeNative( Address, strikeIndex );
//
//        [DllImport( NativeLib, EntryPoint = "setCharSize", CallingConvention = CallingConvention.Cdecl )]
//        private static extern bool SetCharSizeNative( IntPtr face, int charWidth, int charHeight, int horzResolution, int vertResolution );
//
//        public bool SetCharSize( int charWidth, int charHeight, int horzResolution, int vertResolution ) =>
//            SetCharSizeNative( Address, charWidth, charHeight, horzResolution, vertResolution );
//
//        [DllImport( NativeLib, EntryPoint = "setPixelSizes", CallingConvention = CallingConvention.Cdecl )]
//        private static extern bool SetPixelSizesNative( IntPtr face, int pixelWidth, int pixelHeight );
//
//        public bool SetPixelSizes( int pixelWidth, int pixelHeight ) => SetPixelSizesNative( Address, pixelWidth, pixelHeight );
//
//        [DllImport( NativeLib, EntryPoint = "loadGlyph", CallingConvention = CallingConvention.Cdecl )]
//        private static extern bool LoadGlyphNative( IntPtr face, int glyphIndex, int loadFlags );
//
//        public bool LoadGlyph( int glyphIndex, int loadFlags ) => LoadGlyphNative( Address, glyphIndex, loadFlags );
//
//        [DllImport( NativeLib, EntryPoint = "loadChar", CallingConvention = CallingConvention.Cdecl )]
//        private static extern bool LoadCharNative( IntPtr face, int charCode, int loadFlags );
//
//        public bool LoadChar( int charCode, int loadFlags ) => LoadCharNative( Address, charCode, loadFlags );
//
//        [DllImport( NativeLib, EntryPoint = "getGlyph", CallingConvention = CallingConvention.Cdecl )]
//        private static extern IntPtr GetGlyphNative( IntPtr face );
//
//        public GlyphSlot GetGlyph() => new GlyphSlot( GetGlyphNative( Address ) );
//
//        [DllImport( NativeLib, EntryPoint = "getSize", CallingConvention = CallingConvention.Cdecl )]
//        private static extern IntPtr GetSizeNative( IntPtr face );
//
//        public Size GetSize() => new Size( GetSizeNative( Address ) );
//
//        [DllImport( NativeLib, EntryPoint = "hasKerning", CallingConvention = CallingConvention.Cdecl )]
//        private static extern bool HasKerningNative( IntPtr face );
//
//        public bool HasKerning() => HasKerningNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getKerning", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetKerningNative( IntPtr face, int leftGlyph, int rightGlyph, int kernMode );
//
//        public int GetKerning( int leftGlyph, int rightGlyph, int kernMode ) =>
//            GetKerningNative( Address, leftGlyph, rightGlyph, kernMode );
//
//        [DllImport( NativeLib, EntryPoint = "getCharIndex", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetCharIndexNative( IntPtr face, int charCode );
//
//        public int GetCharIndex( int charCode ) => GetCharIndexNative( Address, charCode );
//    }
//
//    public class Size : Pointer
//    {
//        internal Size( IntPtr address )
//            : base( address )
//        {
//        }
//
//        [DllImport( NativeLib, EntryPoint = "getMetrics", CallingConvention = CallingConvention.Cdecl )]
//        private static extern IntPtr GetMetricsNative( IntPtr address );
//
//        public SizeMetrics GetMetrics() => new SizeMetrics( GetMetricsNative( Address ) );
//    }
//
//    public class SizeMetrics : Pointer
//    {
//        internal SizeMetrics( IntPtr address )
//            : base( address )
//        {
//        }
//
//        [DllImport( NativeLib, EntryPoint = "getXppem", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetXppemNative( IntPtr metrics );
//
//        public int GetXppem() => GetXppemNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getYppem", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetYppemNative( IntPtr metrics );
//
//        public int GetYppem() => GetYppemNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getXscale", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetXScaleNative( IntPtr metrics );
//
//        public int GetXScale() => GetXScaleNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getYscale", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetYscaleNative( IntPtr metrics );
//
//        public int GetYscale() => GetYscaleNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getAscender", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetAscenderNative( IntPtr metrics );
//
//        public int GetAscender() => GetAscenderNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getDescender", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetDescenderNative( IntPtr metrics );
//
//        public int GetDescender() => GetDescenderNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getHeight", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetHeightNative( IntPtr metrics );
//
//        public int GetHeight() => GetHeightNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getMaxAdvance", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetMaxAdvanceNative( IntPtr metrics );
//
//        public int GetMaxAdvance() => GetMaxAdvanceNative( Address );
//    }
//
//    public class GlyphSlot : Pointer
//    {
//        internal GlyphSlot( IntPtr address )
//            : base( address )
//        {
//        }
//
//        [DllImport( NativeLib, EntryPoint = "getMetrics", CallingConvention = CallingConvention.Cdecl )]
//        private static extern IntPtr GetMetricsNative( IntPtr slot );
//
//        public GlyphMetrics GetMetrics() => new GlyphMetrics( GetMetricsNative( Address ) );
//
//        [DllImport( NativeLib, EntryPoint = "getLinearHoriAdvance", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetLinearHoriAdvanceNative( IntPtr slot );
//
//        public int GetLinearHoriAdvance() => GetLinearHoriAdvanceNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getLinearVertAdvance", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetLinearVertAdvanceNative( IntPtr slot );
//
//        public int GetLinearVertAdvance() => GetLinearVertAdvanceNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getAdvanceX", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetAdvanceXNative( IntPtr slot );
//
//        public int GetAdvanceX() => GetAdvanceXNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getAdvanceY", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetAdvanceYNative( IntPtr slot );
//
//        public int GetAdvanceY() => GetAdvanceYNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getFormat", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetFormatNative( IntPtr slot );
//
//        public int GetFormat() => GetFormatNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getBitmap", CallingConvention = CallingConvention.Cdecl )]
//        private static extern IntPtr GetBitmapNative( IntPtr slot );
//
//        public Bitmap GetBitmap() => new Bitmap( GetBitmapNative( Address ) );
//
//        [DllImport( NativeLib, EntryPoint = "getBitmapLeft", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetBitmapLeftNative( IntPtr slot );
//
//        public int GetBitmapLeft() => GetBitmapLeftNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getBitmapTop", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetBitmapTopNative( IntPtr slot );
//
//        public int GetBitmapTop() => GetBitmapTopNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "renderGlyph", CallingConvention = CallingConvention.Cdecl )]
//        private static extern bool RenderGlyphNative( IntPtr slot, int renderMode );
//
//        public bool RenderGlyph( int renderMode ) => RenderGlyphNative( Address, renderMode );
//
//        [DllImport( NativeLib, EntryPoint = "getGlyph", CallingConvention = CallingConvention.Cdecl )]
//        private static extern IntPtr GetGlyphFromSlotNative( IntPtr glyphSlot );
//
//        public Glyph GetGlyph()
//        {
//            var glyphAddress = GetGlyphFromSlotNative( Address );
//
//            if ( glyphAddress == IntPtr.Zero )
//            {
//                throw new GdxRuntimeException( "Couldn't get glyph, FreeType error code: " + GetLastErrorCode() );
//            }
//
//            return new Glyph( glyphAddress );
//        }
//    }
//
//    public class Glyph : Pointer, IDisposable
//    {
//        private bool _rendered;
//
//        internal Glyph( IntPtr address )
//            : base( address )
//        {
//        }
//
//        [DllImport( NativeLib, EntryPoint = "done", CallingConvention = CallingConvention.Cdecl )]
//        private static extern void DoneNative( IntPtr glyph );
//
//        public void Dispose()
//        {
//            Dispose( true );
//            GC.SuppressFinalize( this );
//        }
//
//        protected virtual void Dispose( bool disposing )
//        {
//            if ( disposing )
//            {
//                if ( Address != IntPtr.Zero )
//                {
//                    DoneNative( Address );
//                    Address = IntPtr.Zero; // SetMark as disposed
//                }
//            }
//        }
//
//        ~Glyph()
//        {
//            Dispose( false );
//        }
//
//        [DllImport( NativeLib, EntryPoint = "strokeBorder", CallingConvention = CallingConvention.Cdecl )]
//        private static extern IntPtr StrokeBorderNative( IntPtr glyph, IntPtr stroker, bool inside );
//
//        public void StrokeBorder( Stroker stroker, bool inside )
//        {
//            Address = StrokeBorderNative( Address, stroker.Address, inside );
//        }
//
//        [DllImport( NativeLib, EntryPoint = "toBitmap", CallingConvention = CallingConvention.Cdecl )]
//        private static extern IntPtr ToBitmapNative( IntPtr glyph, int renderMode );
//
//        public void ToBitmap( int renderMode )
//        {
//            var bitmapAddress = ToBitmapNative( Address, renderMode );
//
//            if ( bitmapAddress == IntPtr.Zero )
//            {
//                throw new GdxRuntimeException( "Couldn't render glyph, FreeType error code: " + GetLastErrorCode() );
//            }
//
//            Address   = bitmapAddress;
//            _rendered = true;
//        }
//
//        [DllImport( NativeLib, EntryPoint = "getBitmap", CallingConvention = CallingConvention.Cdecl )]
//        private static extern IntPtr GetBitmapFromGlyphNative( IntPtr glyph );
//
//        public Bitmap GetBitmap()
//        {
//            if ( !_rendered )
//            {
//                throw new GdxRuntimeException( "Glyph is not yet rendered" );
//            }
//
//            return new Bitmap( GetBitmapFromGlyphNative( Address ) );
//        }
//
//        [DllImport( NativeLib, EntryPoint = "getLeft", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetLeftNative( IntPtr glyph );
//
//        public int GetLeft()
//        {
//            if ( !_rendered )
//            {
//                throw new GdxRuntimeException( "Glyph is not yet rendered" );
//            }
//
//            return GetLeftNative( Address );
//        }
//
//        [DllImport( NativeLib, EntryPoint = "getTop", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetTopNative( IntPtr glyph );
//
//        public int GetTop()
//        {
//            if ( !_rendered )
//            {
//                throw new GdxRuntimeException( "Glyph is not yet rendered" );
//            }
//
//            return GetTopNative( Address );
//        }
//    }
//
//    public class Bitmap : Pointer
//    {
//        internal Bitmap( IntPtr address )
//            : base( address )
//        {
//        }
//
//        [DllImport( NativeLib, EntryPoint = "getRows", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetRowsNative( IntPtr bitmap );
//
//        public int GetRows() => GetRowsNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getWidth", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetWidthNative( IntPtr bitmap );
//
//        public int GetWidth() => GetWidthNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getPitch", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetPitchNative( IntPtr bitmap );
//
//        public int GetPitch() => GetPitchNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getBuffer", CallingConvention = CallingConvention.Cdecl )]
//        private static extern IntPtr GetBufferNative( IntPtr bitmap ); // Returns a pointer to the native buffer
//
//        public byte[] GetBuffer()
//        {
//            var rows  = GetRows();
//            var width = GetWidth();
//            var pitch = Math.Abs( GetPitch() );
//
//            if ( rows == 0 )
//            {
//                // Return a dummy non-null, non-zero buffer as per original Java comment
//                return new byte[ 1 ];
//            }
//
//            var nativeBufferPtr = GetBufferNative( Address );
//
//            if ( nativeBufferPtr == IntPtr.Zero )
//            {
//                // This case should ideally not happen if rows > 0, but as a safeguard.
//                return new byte[ 1 ];
//            }
//
//            var managedBuffer = new byte[ rows * pitch ];
//
//            // Copy data from native memory to managed byte array
//            Marshal.Copy( nativeBufferPtr, managedBuffer, 0, managedBuffer.Length );
//
//            return managedBuffer;
//        }
//
//        // These constants would likely come from FreeTypeSharp or other bindings
//        // For now, hardcoding based on typical FreeType defines.
//        public const int FT_PIXEL_MODE_MONO = 0; // Or whatever its actual value is
//        public const int FT_PIXEL_MODE_GRAY = 1; // Or whatever its actual value is
//
//        public Pixmap GetPixmap( Graphics.Format format, Color color, float gamma )
//        {
//            int    width = GetWidth(), rows = GetRows();
//            var src   = GetBuffer(); // Get the managed byte array copy of the native buffer
//            Pixmap pixmap;
//            var    pixelMode = GetPixelMode();
//            var    rowBytes  = Math.Abs( GetPitch() );
//
//            if ( ( color == Color.White ) && ( pixelMode == FT_PIXEL_MODE_GRAY ) && ( rowBytes == width ) && ( gamma == 1 ) )
//            {
//                pixmap = new Pixmap( width, rows, Graphics.Format.Alpha );
//
//                // Assuming Pixmap has a way to directly set pixel data from a byte array
//                // or BufferUtils.copy can handle byte[] to Pixmap's internal buffer
//                BufferUtils.Copy( src, 0, pixmap.GetPixels(), 0, src.Length );
//            }
//            else
//            {
//                pixmap = new Pixmap( width, rows, Graphics.Format.RGBA8888 );
//                int    rgba   = color.ToRGBA8888(); // Assuming Color has a ToRGBA8888 method
//                var srcRow = new byte[ rowBytes ];
//                var  dstRow = new int[ width ];
//
//                // Directly access Pixmap's pixel buffer, assuming it's a Buffer< byte > or similar
//                // and can be wrapped by an Buffer< int > for efficiency.
//                // For direct access and potentially better performance, you might need unsafe code
//                // or a Pixmap method that accepts an int[] directly.
//                // Here, we'll assume a method to write a row of int[] to the Pixmap.
//
//                // Assuming Pixmap.SetPixels(int[] rowData, int offset, int length) or similar
//                // or a way to get a direct int[] view of the Pixmap's buffer.
//                // For simplicity, I'll provide a conceptual approach for writing rows.
//
//                if ( pixelMode == FT_PIXEL_MODE_MONO )
//                {
//                    for ( var y = 0; y < rows; y++ )
//                    {
//                        Array.Copy( src, y * rowBytes, srcRow, 0, rowBytes );
//
//                        for ( int i = 0, x = 0; x < width; i++, x += 8 )
//                        {
//                            var b = srcRow[ i ];
//
//                            for ( int ii = 0, n = Math.Min( 8, width - x ); ii < n; ii++ )
//                            {
//                                if ( ( b & ( 1 << ( 7 - ii ) ) ) != 0 )
//                                    dstRow[ x + ii ] = rgba;
//                                else
//                                    dstRow[ x + ii ] = 0;
//                            }
//                        }
//
//                        pixmap.SetPixels( y, dstRow ); // Conceptual: Set a row of pixels
//                    }
//                }
//                else
//                {
//                    int rgb = rgba & 0xffffff00;
//                    var a   = rgba & 0xff;
//
//                    for ( var y = 0; y < rows; y++ )
//                    {
//                        Array.Copy( src, y * rowBytes, srcRow, 0, rowBytes );
//
//                        for ( var x = 0; x < width; x++ )
//                        {
//                            var alpha = srcRow[ x ] & 0xff;
//                            if ( alpha == 0 )
//                                dstRow[ x ] = rgb;
//                            else if ( alpha == 255 )
//                                dstRow[ x ] = rgb | a;
//                            else
//                                dstRow[ x ] = rgb | ( int )( a * ( float )Math.Pow( alpha / 255f, gamma ) ); // Inverse gamma.
//                        }
//
//                        pixmap.SetPixels( y, dstRow ); // Conceptual: Set a row of pixels
//                    }
//                }
//            }
//
//            var converted = pixmap;
//
//            if ( format != pixmap.GetFormat() )
//            {
//                converted = new Pixmap( pixmap.GetWidth(), pixmap.GetHeight(), format );
//                converted.SetBlending( Blending.None );       // Assuming SetBlending method
//                converted.DrawPixmap( pixmap, 0, 0 );         // Assuming DrawPixmap method
//                converted.SetBlending( Blending.SourceOver ); // Assuming SetBlending method
//                pixmap.Dispose();
//            }
//
//            return converted;
//        }
//
//        [DllImport( NativeLib, EntryPoint = "getNumGray", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetNumGrayNative( IntPtr bitmap );
//
//        public int GetNumGray() => GetNumGrayNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getPixelMode", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetPixelModeNative( IntPtr bitmap );
//
//        public int GetPixelMode() => GetPixelModeNative( Address );
//    }
//
//    public class GlyphMetrics : Pointer
//    {
//        internal GlyphMetrics( IntPtr address )
//            : base( address )
//        {
//        }
//
//        [DllImport( NativeLib, EntryPoint = "getWidth", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetWidthNative( IntPtr metrics );
//
//        public int GetWidth() => GetWidthNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getHeight", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetHeightNative( IntPtr metrics );
//
//        public int GetHeight() => GetHeightNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getHoriBearingX", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetHoriBearingXNative( IntPtr metrics );
//
//        public int GetHoriBearingX() => GetHoriBearingXNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getHoriBearingY", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetHoriBearingYNative( IntPtr metrics );
//
//        public int GetHoriBearingY() => GetHoriBearingYNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getHoriAdvance", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetHoriAdvanceNative( IntPtr metrics );
//
//        public int GetHoriAdvance() => GetHoriAdvanceNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getVertBearingX", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetVertBearingXNative( IntPtr metrics );
//
//        public int GetVertBearingX() => GetVertBearingXNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getVertBearingY", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetVertBearingYNative( IntPtr metrics );
//
//        public int GetVertBearingY() => GetVertBearingYNative( Address );
//
//        [DllImport( NativeLib, EntryPoint = "getVertAdvance", CallingConvention = CallingConvention.Cdecl )]
//        private static extern int GetVertAdvanceNative( IntPtr metrics );
//
//        public int GetVertAdvance() => GetVertAdvanceNative( Address );
//    }
//
//    public class Stroker : Pointer, IDisposable
//    {
//        internal Stroker( IntPtr address )
//            : base( address )
//        {
//        }
//
//        [DllImport( NativeLib, EntryPoint = "set", CallingConvention = CallingConvention.Cdecl )]
//        private static extern void SetNative( IntPtr stroker, int radius, int lineCap, int lineJoin, int miterLimit );
//
//        public void Set( int radius, int lineCap, int lineJoin, int miterLimit ) =>
//            SetNative( Address, radius, lineCap, lineJoin, miterLimit );
//
//        [DllImport( NativeLib, EntryPoint = "done", CallingConvention = CallingConvention.Cdecl )]
//        private static extern void DoneStrokerNative( IntPtr stroker );
//
//        public void Dispose()
//        {
//            Dispose( true );
//            GC.SuppressFinalize( this );
//        }
//
//        protected virtual void Dispose( bool disposing )
//        {
//            if ( disposing )
//            {
//                if ( Address != IntPtr.Zero )
//                {
//                    DoneStrokerNative( Address );
//                    Address = IntPtr.Zero; // SetMark as disposed
//                }
//            }
//        }
//
//        ~Stroker()
//        {
//            Dispose( false );
//        }
//    }
//
//    private static int Encode( char a, char b, char c, char d )
//    {
//        return ( a << 24 ) | ( b << 16 ) | ( c << 8 ) | d;
//    }
//
//    [DllImport( NativeLib, EntryPoint = "initFreeTypeJni", CallingConvention = CallingConvention.Cdecl )]
//    private static extern IntPtr InitFreeTypeJniNative();
//
//    public static Library InitFreeType()
//    {
//        // In C#, DllImport typically handles the loading of the shared library
//        // so SharedLibraryLoader().load("gdx-freetype"); is not directly translated.
//        // You ensure the DLL/SO/DYLIB is in a place where the .NET runtime can find it.
//
//        var address = InitFreeTypeJniNative();
//
//        if ( address == IntPtr.Zero )
//        {
//            throw new GdxRuntimeException( "Couldn't initialize FreeType library, FreeType error code: " + GetLastErrorCode() );
//        }
//        else
//        {
//            return new Library( address );
//        }
//    }
//
//    public static int ToInt( int value )
//    {
//        return ( ( value + 63 ) & -64 ) >> 6;
//    }
}

// ============================================================================
// ============================================================================
// ============================================================================
public partial class FreeType
{
    private const string FREETYPE_DLL_PATH = "lib/net8.0/freetype.dll";

    public static Library InitFreeType()
    {
//        new SharedLibraryLoader().load("gdx-freetype");
//        long address;   // = initFreeTypeJni();
//
//        if ( address == 0 )
//        {
//            throw new GdxRuntimeException( $"Couldn't initialize FreeType library, " +
//                                           $"FreeType error code: {GetLastErrorCode()}" );
//        }
//
//        return new Library(address);

        throw new NotImplementedException();
    }

    public static int ToInt( int value )
    {
        return ( ( value + 63 ) & -64 ) >> 6;
    }

    private static int Encode( char a, char b, char c, char d )
    {
        return ( a << 24 ) | ( b << 16 ) | ( c << 8 ) | d;
    }

    private static int GetLastErrorCode()
    {
        return _getLastErrorCode();

        [DllImport( FREETYPE_DLL_PATH, EntryPoint = "getLastErrorCode" )]
        static extern int _getLastErrorCode();
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class Pointer
    {
        internal long Address;

        public Pointer( long address )
        {
            Address = address;
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class Library : Pointer, IDisposable
    {
        public Library( long address ) : base( address )
        {
        }

        public Dictionary< long, Buffer< byte > > FontData { get; private set; } = [ ];

        public Face NewFace( FileInfo fontFile, int faceIndex )
        {
            throw new NotImplementedException();

//            Buffer< byte >? buffer = null;
//
//            try
//            {
//                buffer = fontFile.Map();
//            }
//            catch ( GdxRuntimeException )
//            {
//                // OK to ignore, some platforms do not support file mapping.
//            }
//
//            if ( buffer == null )
//            {
//                InputStream input = fontFile.Read();
//
//                try
//                {
//                    var fileSize = ( int )fontFile.Length();
//
//                    if ( fileSize == 0 )
//                    {
//                        // Copy to a byte[] to get the size, then copy to the buffer.
//                        byte[] data = StreamUtils.CopyStreamToByteArray( input, 1024 * 16 );
//
//                        buffer = BufferUtils.NewUnsafeByteBuffer( data.Length );
//                        BufferUtils.Copy( data, 0, buffer, data.Length );
//                    }
//                    else
//                    {
//                        // Trust the specified file size.
//                        buffer = BufferUtils.NewUnsafeByteBuffer( fileSize );
//                        StreamUtils.CopyStream( input, buffer );
//                    }
//                }
//                catch ( IOException ex )
//                {
//                    throw new GdxRuntimeException( ex );
//                }
//                finally
//                {
//                    StreamUtils.CloseQuietly( input );
//                }
//            }
//
//            return NewMemoryFace( buffer, faceIndex );
        }

        public Face NewMemoryFace( byte[] data, int dataSize, int faceIndex )
        {
            var buffer = new Buffer< byte >( data.Length );
            BufferUtils.Copy( data, 0, data.Length, buffer );

            return NewMemoryFace( buffer, faceIndex );
        }

        public Face NewMemoryFace( Buffer< byte > buffer, int faceIndex )
        {
            throw new NotImplementedException();

//            var face = _newMemoryFace( Address, buffer, buffer.Remaining(), faceIndex );
//
//            if ( face == 0 )
//            {
//                if ( BufferUtils.IsUnsafeByteBuffer( buffer ) )
//                {
//                    BufferUtils.DisposeUnsafeByteBuffer( buffer );
//                }
//
//                throw new GdxRuntimeException( $"Couldn't load font, FreeType error code: {_getLastErrorCode()}" );
//            }
//
//            FontData.Put( face, buffer );
//
//            return new Face( face, this );
        }

        public Stroker CreateStroker()
        {
            throw new NotImplementedException();

//            var stroker = _strokerNew( Address );
//
//            if ( stroker == 0 )
//            {
//                throw new GdxRuntimeException( $"Couldn't create FreeType stroker, FreeType error code: {_getLastErrorCode()}" );
//            }
//
//            return new Stroker( stroker );
        }

        public void Dispose()
        {
//            _doneFreeType( Address );
//
//            foreach ( var buffer in FontData.Values )
//            {
//                if ( BufferUtils.IsUnsafeByteBuffer( buffer ) )
//                {
//                    BufferUtils.DisposeUnsafeByteBuffer( buffer );
//                }
//            }

            GC.SuppressFinalize( this );
        }

        // ====================================================================
        // ====================================================================

//        private static extern void _doneFreeType( long library );
//        private static extern long _newMemoryFace( long library, Buffer< byte > data, int dataSize, int faceIndex );
//        private static extern long _strokerNew( long library );
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class Face : Pointer
    {
        public Face( long address, Library library ) : base( address )
        {
            Library = library;
        }

        public Library Library { get; private set; }

        public Size GetSize()
        {
            throw new NotImplementedException();

//            return new Size( _getSize( Address ) );
        }

        public int GetCharIndex( int i )
        {
            throw new NotImplementedException();
        }

        public GlyphSlot GetGlyph()
        {
            throw new NotImplementedException();
        }

//        private static extern long _getSize( long face );

        public bool SetPixelSizes( int pixelWidth, int pixelHeight )
        {
            throw new NotImplementedException();
        }

        public float GetMaxAdvanceWidth()
        {
            throw new NotImplementedException();
        }

        public int GetNumGlyphs()
        {
            throw new NotImplementedException();
        }

        public int GetKerning( int otherIndex, int glyphIndex, int p2 )
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool HasKerning()
        {
            throw new NotImplementedException();
        }

        public bool LoadChar( int i, int flags )
        {
            throw new NotImplementedException();
        }

        public int GetFaceFlags()
        {
            throw new NotImplementedException();
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class Size : Pointer
    {
        public SizeMetrics? Metrics;

        public Size( long address ) : base( address )
        {
        }

        //        {
//            get { return new( _getMetrics( base.Address ) ); }
//        }

        //        private static extern long _getMetrics( long address );

        public SizeMetrics GetMetrics()
        {
            throw new NotImplementedException();

//            return new SizeMetrics( _getMetrics( base.Address ) );
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class SizeMetrics : Pointer
    {
        public SizeMetrics( long address ) : base( address )
        {
        }

        public int GetAscender()
        {
            throw new NotImplementedException();
        }

        public int GetDescender()
        {
            throw new NotImplementedException();
        }

        public int GetHeight()
        {
            throw new NotImplementedException();
        }

        public int GetMaxAdvance()
        {
            throw new NotImplementedException();
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class GlyphSlot : Pointer
    {
        public GlyphSlot( long address ) : base( address )
        {
        }

        public Bitmap GetBitmap()
        {
            throw new NotImplementedException();
        }

        public bool RenderGlyph( int ftRenderModeNormal )
        {
            throw new NotImplementedException();
        }

        public GlyphMetrics GetMetrics()
        {
            throw new NotImplementedException();
        }

        public int GetBitmapLeft()
        {
            throw new NotImplementedException();
        }

        public int GetBitmapTop()
        {
            throw new NotImplementedException();
        }

        public Glyph GetGlyph()
        {
            throw new NotImplementedException();
        }

        public int GetFormat()
        {
            throw new NotImplementedException();
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class Glyph : Pointer
    {
        public Glyph( long address ) : base( address )
        {
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void ToBitmap( int ftRenderModeNormal )
        {
            throw new NotImplementedException();
        }

        public Bitmap GetBitmap()
        {
            throw new NotImplementedException();
        }

        public int GetTop()
        {
            throw new NotImplementedException();
        }

        public int GetLeft()
        {
            throw new NotImplementedException();
        }

        public void StrokeBorder( Stroker? stroker, bool b )
        {
            throw new NotImplementedException();
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class Bitmap : Pointer
    {
        public Bitmap( long address ) : base( address )
        {
        }

        public int GetWidth()
        {
            throw new NotImplementedException();
        }

        public int GetRows()
        {
            throw new NotImplementedException();
        }

        public int GetPitch()
        {
            throw new NotImplementedException();
        }

        public Pixmap GetPixmap( int rgba8888, Color parameterColor, float parameterGamma )
        {
            throw new NotImplementedException();
        }

        public Buffer< byte > GetBuffer()
        {
            throw new NotImplementedException();
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    [PublicAPI]
    public class GlyphMetrics : Pointer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        public GlyphMetrics( long address ) : base( address )
        {
        }

        public int GetWidth()
        {
            return 0; //_getWidth( Address );
        }

        public int GetHeight()
        {
            return 0; //_getHeight( Address );
        }

        public int GetHoriBearingX()
        {
            return 0; //_getHoriBearingX( Address );
        }

        public int GetHoriBearingY()
        {
            return 0; //_getHoriBearingY( Address );
        }

        public int GetHoriAdvance()
        {
            return 0; //_getHoriAdvance( Address );
        }

        public int GetVertBearingX()
        {
            return 0; //_getVertBearingX( Address );
        }

        public int GetVertBearingY()
        {
            return 0; //_getVertBearingY( Address );
        }

        public int GetVertAdvance()
        {
            return 0; //_getVertAdvance( Address );
        }

//        private static extern int _getWidth( long metrics );
//        private static extern int _getHeight( long metrics );
//        private static extern int _getHoriBearingX( long metrics );
//        private static extern int _getHoriBearingY( long metrics );
//        private static extern int _getHoriAdvance( long metrics );
//        private static extern int _getVertBearingX( long metrics );
//        private static extern int _getVertBearingY( long metrics );
//        private static extern int _getVertAdvance( long metrics );
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class Stroker : Pointer, IDisposable
    {
        public Stroker( long address ) : base( address )
        {
        }

        public void Set( int radius, int lineCap, int lineJoin, int miterLimit )
        {
//            _set( Address, radius, lineCap, lineJoin, miterLimit );
        }

        /// <inheritdoc />
        public void Dispose()
        {
//            _done( address );

            GC.SuppressFinalize( this );
        }

//        private static extern void _set( long stroker, int radius, int lineCap, int lineJoin, int miterLimit );
//        private static extern void _done( long stroker );
    }
}