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

using LughSharp.Source.Graphics.Images;
using LughSharp.Source.IO;

namespace LughSharp.Source.Graphics.Fonts.Freetype;

[PublicAPI]
public class FreeType
{
    public const int FtPixelModeNone                = 0;
    public const int FtPixelModeMono                = 1;
    public const int FtPixelModeGray                = 2;
    public const int FtPixelModeGray2               = 3;
    public const int FtPixelModeGray4               = 4;
    public const int FtPixelModeLcd                 = 5;
    public const int FtPixelModeLcdV                = 6;
    public const int FtFaceFlagScalable             = 1 << 0;
    public const int FtFaceFlagFixedSizes           = 1 << 1;
    public const int FtFaceFlagFixedWidth           = 1 << 2;
    public const int FtFaceFlagSfnt                 = 1 << 3;
    public const int FtFaceFlagHorizontal           = 1 << 4;
    public const int FtFaceFlagVertical             = 1 << 5;
    public const int FtFaceFlagKerning              = 1 << 6;
    public const int FtFaceFlagFastGlyphs           = 1 << 7;
    public const int FtFaceFlagMultipleMasters      = 1 << 8;
    public const int FtFaceFlagGlyphNames           = 1 << 9;
    public const int FtFaceFlagExternalStream       = 1 << 10;
    public const int FtFaceFlagHinter               = 1 << 11;
    public const int FtFaceFlagCidKeyed             = 1 << 12;
    public const int FtFaceFlagTricky               = 1 << 13;
    public const int FtStyleFlagItalic              = 1 << 0;
    public const int FtStyleFlagBold                = 1 << 1;
    public const int FtLoadDefault                  = 0x0;
    public const int FtLoadNoScale                  = 0x1;
    public const int FtLoadNoHinting                = 0x2;
    public const int FtLoadRender                   = 0x4;
    public const int FtLoadNoBitmap                 = 0x8;
    public const int FtLoadVerticalLayout           = 0x10;
    public const int FtLoadForceAutohint            = 0x20;
    public const int FtLoadCropBitmap               = 0x40;
    public const int FtLoadPedantic                 = 0x80;
    public const int FtLoadIgnoreGlobalAdvanceWidth = 0x200;
    public const int FtLoadNoRecurse                = 0x400;
    public const int FtLoadIgnoreTransform          = 0x800;
    public const int FtLoadMonochrome               = 0x1000;
    public const int FtLoadLinearDesign             = 0x2000;
    public const int FtLoadNoAutohint               = 0x8000;
    public const int FtLoadTargetNormal             = 0x0;
    public const int FtLoadTargetLight              = 0x10000;
    public const int FtLoadTargetMono               = 0x20000;
    public const int FtLoadTargetLcd                = 0x30000;
    public const int FtLoadTargetLcdV               = 0x40000;
    public const int FtRenderModeNormal             = 0;
    public const int FtRenderModeLight              = 1;
    public const int FtRenderModeMono               = 2;
    public const int FtRenderModeLcd                = 3;
    public const int FtRenderModeLcdV               = 4;
    public const int FtRenderModeMax                = 5;
    public const int FtKerningDefault               = 0;
    public const int FtKerningUnfitted              = 1;
    public const int FtKerningUnscaled              = 2;
    public const int FtStrokerLinecapButt           = 0;
    public const int FtStrokerLinecapRound          = 1;
    public const int FtStrokerLinecapSquare         = 2;
    public const int FtStrokerLinejoinRound         = 0;
    public const int FtStrokerLinejoinBevel         = 1;
    public const int FtStrokerLinejoinMiterVariable = 2;
    public const int FtStrokerLinejoinMiter         = FtStrokerLinejoinMiterVariable;
    public const int FtStrokerLinejoinMiterFixed    = 3;

    // ========================================================================

    public static readonly int FtEncodingAdobeCustom   = Encode( 'A', 'D', 'B', 'C' );
    public static readonly int FtEncodingAdobeExpert   = Encode( 'A', 'D', 'B', 'E' );
    public static readonly int FtEncodingAdobeLatin1   = Encode( 'l', 'a', 't', '1' );
    public static readonly int FtEncodingAdobeStandard = Encode( 'A', 'D', 'O', 'B' );
    public static readonly int FtEncodingAppleRoman    = Encode( 'a', 'r', 'm', 'n' );
    public static readonly int FtEncodingBig5          = Encode( 'b', 'i', 'g', '5' );
    public static readonly int FtEncodingGb2312        = Encode( 'g', 'b', ' ', ' ' );
    public static readonly int FtEncodingJohab         = Encode( 'j', 'o', 'h', 'a' );
    public static readonly int FtEncodingMsSymbol      = Encode( 's', 'y', 'm', 'b' );
    public static readonly int FtEncodingOldLatin2     = Encode( 'l', 'a', 't', '2' );
    public static readonly int FtEncodingSjis          = Encode( 's', 'j', 'i', 's' );
    public static readonly int FtEncodingUnicode       = Encode( 'u', 'n', 'i', 'c' );
    public static readonly int FtEncodingWansung       = Encode( 'w', 'a', 'n', 's' );
    public static readonly int FtEncodingNone          = 0;

    // ========================================================================

    #if NET8_0
    private const string NativeLib = "lib/net8.0/freetype_x64.dll";
    #elif NET9_0
    private const string NativeLib = "lib/net9.0/freetype_x64.dll";
    #endif

    // ========================================================================

    /// <summary>Returns the last error code FreeType reported.</summary>
    public static int GetLastErrorCode()
    {
        return GetLastErrorCodeNative();

        // --------------------------------------

        [DllImport( NativeLib, EntryPoint = "FT_Get_Last_ErrorCode", CallingConvention = CallingConvention.Cdecl )]
        static extern int GetLastErrorCodeNative();
    }

    public static Library InitFreeType()
    {
        IntPtr address = IntPtr.Zero;
        _ = InitFreeTypeNative( ref address );

        if ( address == IntPtr.Zero )
        {
            throw new RuntimeException( "Couldn't initialize FreeType library, FreeType error code: "
                                      + GetLastErrorCode() );
        }

        Logger.Debug( $"Freetype initialized successfully. Address: {address}" );

        return new Library( address );

        // --------------------------------------

        [DllImport( NativeLib, EntryPoint = "FT_Init_FreeType", CallingConvention = CallingConvention.Cdecl )]
        static extern int InitFreeTypeNative( ref IntPtr address );
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
        // Maps face address → unmanaged buffer allocated with Marshal.AllocHGlobal.
        // FT_New_Memory_Face retains a reference to the buffer for the lifetime of the face.
        public Dictionary< IntPtr, IntPtr > FontData { get; } = new();

        internal Library( IntPtr address ) : base( address )
        {
        }

        public Face NewFace( FileInfo fontFile, int faceIndex )
        {
            byte[]? data = null;

            try
            {
                data = File.ReadAllBytes( fontFile.FullName );
            }
            catch ( IOException )
            {
                // Fall through to stream-based fallback.
            }

            if ( ( data == null ) || ( data.Length == 0 ) )
            {
                using FileStream input = fontFile.OpenRead();

                try
                {
                    var fileSize = ( int )fontFile.Length;

                    if ( fileSize == 0 )
                    {
                        data = StreamUtils.CopyStreamToByteArray( input, 1024 * 16 );
                    }
                    else
                    {
                        data = new byte[ fileSize ];
                        StreamUtils.CopyStream( input, data );
                    }
                }
                catch ( IOException ex )
                {
                    throw new RuntimeException( ex );
                }
            }

            return NewMemoryFace( data, faceIndex );
        }

        public Face NewMemoryFace( byte[] buffer, int faceIndex )
        {
            IntPtr unmanagedBuffer = Marshal.AllocHGlobal( buffer.Length );
            Marshal.Copy( buffer, 0, unmanagedBuffer, buffer.Length );

            IntPtr faceAddress = NewMemoryFaceNative( Address, unmanagedBuffer, buffer.Length, faceIndex );

            if ( faceAddress == IntPtr.Zero )
            {
                Marshal.FreeHGlobal( unmanagedBuffer );

                throw new RuntimeException( "Couldn't load font, FreeType error code: " + GetLastErrorCode() );
            }

            FontData[ faceAddress ] = unmanagedBuffer;

            return new Face( faceAddress, this );

            // ----------------------------------

            [DllImport( NativeLib, EntryPoint = "FT_New_Memory_Face", CallingConvention = CallingConvention.Cdecl )]
            static extern IntPtr NewMemoryFaceNative( IntPtr library, IntPtr data, int dataSize, int faceIndex );
        }

        public Stroker CreateStroker()
        {
            IntPtr strokerAddress = StrokerNewNative( Address );

            if ( strokerAddress == IntPtr.Zero )
            {
                throw new RuntimeException( "Couldn't create FreeType stroker, FreeType error code: "
                                          + GetLastErrorCode() );
            }

            return new Stroker( strokerAddress );

            // ----------------------------------

            [DllImport( NativeLib, EntryPoint = "FT_Stroker_New", CallingConvention = CallingConvention.Cdecl )]
            static extern IntPtr StrokerNewNative( IntPtr library );
        }

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
            if ( Address == IntPtr.Zero ) return;

            DoneFreeTypeNative( Address );
            Address = IntPtr.Zero;

            foreach ( IntPtr bufferPtr in FontData.Values )
            {
                if ( bufferPtr != IntPtr.Zero )
                {
                    Marshal.FreeHGlobal( bufferPtr );
                }
            }

            if ( disposing )
            {
                FontData.Clear();
            }
        }

        // --------------------------------------

        [DllImport( NativeLib, EntryPoint = "FT_Done_FreeType", CallingConvention = CallingConvention.Cdecl )]
        private static extern void DoneFreeTypeNative( IntPtr library );
    }

    // ========================================================================

    [PublicAPI]
    public class Face : Pointer, IDisposable
    {
        internal Library Library { get; }
        
        // ====================================================================

        internal Face( IntPtr address, Library library ) : base( address )
        {
            Library = library;
        }

        public int GetFaceFlags()
        {
            return GetFaceFlagsNative( Address );
            
            // ----------------------------------

            [DllImport( NativeLib, EntryPoint = "FT_Face", CallingConvention = CallingConvention.Cdecl )]
            static extern int GetFaceFlagsNative( IntPtr face );
        }

        public int GetStyleFlags()
        {
            return GetStyleFlagsNative( Address );

            // ----------------------------------

            [DllImport( NativeLib, EntryPoint = "FT_Get_StyleFlags", CallingConvention = CallingConvention.Cdecl )]
            static extern int GetStyleFlagsNative( IntPtr face );
        }

        public int GetNumGlyphs()
        {
            return GetNumGlyphsNative( Address );

            // ----------------------------------

            [DllImport( NativeLib, EntryPoint = "FT_Get_NumGlyphs", CallingConvention = CallingConvention.Cdecl )]
            static extern int GetNumGlyphsNative( IntPtr face );
        }

        public int GetAscender()
        {
            return GetAscenderNative( Address );

            // ----------------------------------

            [DllImport( NativeLib, EntryPoint = "FT_Get_Ascender", CallingConvention = CallingConvention.Cdecl )]
            static extern int GetAscenderNative( IntPtr face );
        }

        public int GetDescender()
        {
            return GetDescenderNative( Address );

            // ----------------------------------

            [DllImport( NativeLib, EntryPoint = "FT_Get_Descender", CallingConvention = CallingConvention.Cdecl )]
            static extern int GetDescenderNative( IntPtr face );
        }

        public int GetHeight()
        {
            return GetHeightNative( Address );

            // ----------------------------------

            [DllImport( NativeLib, EntryPoint = "FT_Get_Height", CallingConvention = CallingConvention.Cdecl )]
            static extern int GetHeightNative( IntPtr face );
        }

        public int GetMaxAdvanceWidth()
        {
            return GetMaxAdvanceWidthNative( Address );

            // ----------------------------------

            [DllImport( NativeLib, EntryPoint = "FT_Get_MaxAdvanceWidth", CallingConvention = CallingConvention.Cdecl )]
            static extern int GetMaxAdvanceWidthNative( IntPtr face );
        }

        public int GetMaxAdvanceHeight()
        {
            return GetMaxAdvanceHeightNative( Address );

            // ----------------------------------

            [DllImport( NativeLib,
                        EntryPoint = "FT_Get_MaxAdvanceHeight",
                        CallingConvention = CallingConvention.Cdecl )]
            static extern int GetMaxAdvanceHeightNative( IntPtr face );
        }

        public int GetUnderlinePosition()
        {
            return GetUnderlinePositionNative( Address );

            // ----------------------------------

            [DllImport( NativeLib,
                        EntryPoint = "FT_Get_UnderlinePosition",
                        CallingConvention = CallingConvention.Cdecl )]
            static extern int GetUnderlinePositionNative( IntPtr face );
        }

        public int GetUnderlineThickness()
        {
            return GetUnderlineThicknessNative( Address );

            // ----------------------------------

            [DllImport( NativeLib,
                        EntryPoint = "FT_Get_UnderlineThickness",
                        CallingConvention = CallingConvention.Cdecl )]
            static extern int GetUnderlineThicknessNative( IntPtr face );
        }

        public bool HasKerning()
        {
            return HasKerningNative( Address );

            // ----------------------------------

            [DllImport( NativeLib, EntryPoint = "FT_HasKerning", CallingConvention = CallingConvention.Cdecl )]
            static extern bool HasKerningNative( IntPtr face );
        }

        public int GetCharIndex( int charCode )
        {
            return GetCharIndexNative( Address, charCode );

            // ----------------------------------

            [DllImport( NativeLib, EntryPoint = "FT_Get_CharIndex", CallingConvention = CallingConvention.Cdecl )]
            static extern int GetCharIndexNative( IntPtr face, int charCode );
        }

        public bool SelectSize( int strikeIndex )
        {
            return SelectSizeNative( Address, strikeIndex );

            // ----------------------------------

            [DllImport( NativeLib, EntryPoint = "FT_SelectSize", CallingConvention = CallingConvention.Cdecl )]
            static extern bool SelectSizeNative( IntPtr face, int strikeIndex );
        }

        public bool SetCharSize( int charWidth, int charHeight, int horzResolution, int vertResolution )
        {
            return SetCharSizeNative( Address, charWidth, charHeight, horzResolution, vertResolution );

            // ----------------------------------

            [DllImport( NativeLib, EntryPoint = "FT_SetCharSize", CallingConvention = CallingConvention.Cdecl )]
            static extern bool SetCharSizeNative( IntPtr face, int charWidth, int charHeight,
                                                  int horzResolution, int vertResolution );
        }

        public bool SetPixelSizes( int pixelWidth, int pixelHeight )
        {
            return SetPixelSizesNative( Address, pixelWidth, pixelHeight );

            // ----------------------------------

            [DllImport( NativeLib, EntryPoint = "FT_SetPixelSizes", CallingConvention = CallingConvention.Cdecl )]
            static extern bool SetPixelSizesNative( IntPtr face, int pixelWidth, int pixelHeight );
        }

        public bool LoadGlyph( int glyphIndex, int loadFlags )
        {
            return LoadGlyphNative( Address, glyphIndex, loadFlags );

            // ----------------------------------

            [DllImport( NativeLib, EntryPoint = "FT_LoadGlyph", CallingConvention = CallingConvention.Cdecl )]
            static extern bool LoadGlyphNative( IntPtr face, int glyphIndex, int loadFlags );
        }

        public bool LoadChar( int charCode, int loadFlags )
        {
            return LoadCharNative( Address, charCode, loadFlags );

            // ----------------------------------

            [DllImport( NativeLib, EntryPoint = "FT_LoadChar", CallingConvention = CallingConvention.Cdecl )]
            static extern bool LoadCharNative( IntPtr face, int charCode, int loadFlags );
        }

        public GlyphSlot GetGlyph()
        {
            return new GlyphSlot( GetGlyphNative( Address ) );

            // ----------------------------------

            [DllImport( NativeLib, EntryPoint = "FT_Get_Glyph", CallingConvention = CallingConvention.Cdecl )]
            static extern IntPtr GetGlyphNative( IntPtr face );
        }

        public Size GetSize()
        {
            return new Size( GetSizeNative( Address ) );

            // ----------------------------------

            [DllImport( NativeLib, EntryPoint = "FT_Get_Size", CallingConvention = CallingConvention.Cdecl )]
            static extern IntPtr GetSizeNative( IntPtr face );
        }

        public int GetKerning( int leftGlyph, int rightGlyph, int kernMode )
        {
            return GetKerningNative( Address, leftGlyph, rightGlyph, kernMode );

            // ----------------------------------

            [DllImport( NativeLib, EntryPoint = "FT_Get_Kerning", CallingConvention = CallingConvention.Cdecl )]
            static extern int GetKerningNative( IntPtr face, int leftGlyph, int rightGlyph, int kernMode );
        }

        ~Face()
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
            if ( Address == IntPtr.Zero ) return;

            DoneFaceNative( Address );

            if ( Library.FontData.TryGetValue( Address, out IntPtr bufferPtr ) && ( bufferPtr != IntPtr.Zero ) )
            {
                Marshal.FreeHGlobal( bufferPtr );

                if ( disposing )
                {
                    Library.FontData.Remove( Address );
                }
            }

            Address = IntPtr.Zero;
        }

        // --------------------------------------

        [DllImport( NativeLib, EntryPoint = "FT_Done_Face", CallingConvention = CallingConvention.Cdecl )]
        private static extern void DoneFaceNative( IntPtr face );
    }

    // ========================================================================

    [PublicAPI]
    public class Size : Pointer
    {
        internal Size( IntPtr address ) : base( address )
        {
        }

        public SizeMetrics GetMetrics()
        {
            return new SizeMetrics( GetMetricsNative( Address ) );
        }

        [DllImport( NativeLib, EntryPoint = "FT_Get_Metrics", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr GetMetricsNative( IntPtr address );
    }

    // ========================================================================

    [PublicAPI]
    public class SizeMetrics : Pointer
    {
        internal SizeMetrics( IntPtr address ) : base( address )
        {
        }

        public int GetXppem()
        {
            return GetXppemNative( Address );
        }

        public int GetYppem()
        {
            return GetYppemNative( Address );
        }

        public int GetXScale()
        {
            return GetXScaleNative( Address );
        }

        public int GetYScale()
        {
            return GetYScaleNative( Address );
        }

        public int GetAscender()
        {
            return GetAscenderNative( Address );
        }

        public int GetDescender()
        {
            return GetDescenderNative( Address );
        }

        public int GetHeight()
        {
            return GetHeightNative( Address );
        }

        public int GetMaxAdvance()
        {
            return GetMaxAdvanceNative( Address );
        }

        // --------------------------------------

        [DllImport( NativeLib, EntryPoint = "FT_Get_Xppem", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetXppemNative( IntPtr metrics );

        [DllImport( NativeLib, EntryPoint = "FT_Get_Yppem", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetYppemNative( IntPtr metrics );

        [DllImport( NativeLib, EntryPoint = "FT_Get_Xscale", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetXScaleNative( IntPtr metrics );

        [DllImport( NativeLib, EntryPoint = "FT_Get_Yscale", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetYScaleNative( IntPtr metrics );

        [DllImport( NativeLib, EntryPoint = "FT_Get_Ascender", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetAscenderNative( IntPtr metrics );

        [DllImport( NativeLib, EntryPoint = "FT_Get_Descender", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetDescenderNative( IntPtr metrics );

        [DllImport( NativeLib, EntryPoint = "FT_Get_Height", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetHeightNative( IntPtr metrics );

        [DllImport( NativeLib, EntryPoint = "FT_Get_MaxAdvance", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetMaxAdvanceNative( IntPtr metrics );
    }

    // ========================================================================

    [PublicAPI]
    public class GlyphSlot : Pointer
    {
        internal GlyphSlot( IntPtr address ) : base( address )
        {
        }

        public GlyphMetrics GetMetrics()
        {
            return new GlyphMetrics( GetMetricsNative( Address ) );
        }

        public int GetLinearHoriAdvance()
        {
            return GetLinearHoriAdvanceNative( Address );
        }

        public int GetLinearVertAdvance()
        {
            return GetLinearVertAdvanceNative( Address );
        }

        public int GetAdvanceX()
        {
            return GetAdvanceXNative( Address );
        }

        public int GetAdvanceY()
        {
            return GetAdvanceYNative( Address );
        }

        public int GetFormat()
        {
            return GetFormatNative( Address );
        }

        public Bitmap GetBitmap()
        {
            return new Bitmap( GetBitmapNative( Address ) );
        }

        public int GetBitmapLeft()
        {
            return GetBitmapLeftNative( Address );
        }

        public int GetBitmapTop()
        {
            return GetBitmapTopNative( Address );
        }

        public bool RenderGlyph( int renderMode )
        {
            return RenderGlyphNative( Address, ( int )renderMode );
        }

        public Glyph GetGlyph()
        {
            IntPtr glyphAddress = GetGlyphFromSlotNative( Address );

            if ( glyphAddress == IntPtr.Zero )
            {
                throw new RuntimeException( "Couldn't get glyph, FreeType error code: " + GetLastErrorCode() );
            }

            return new Glyph( glyphAddress );
        }

        // --------------------------------------

        [DllImport( NativeLib, EntryPoint = "FT_Get_Metrics", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr GetMetricsNative( IntPtr slot );

        [DllImport( NativeLib, EntryPoint = "FT_Get_LinearHoriAdvance", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetLinearHoriAdvanceNative( IntPtr slot );

        [DllImport( NativeLib, EntryPoint = "FT_Get_LinearVertAdvance", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetLinearVertAdvanceNative( IntPtr slot );

        [DllImport( NativeLib, EntryPoint = "FT_Get_AdvanceX", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetAdvanceXNative( IntPtr slot );

        [DllImport( NativeLib, EntryPoint = "FT_Get_AdvanceY", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetAdvanceYNative( IntPtr slot );

        [DllImport( NativeLib, EntryPoint = "FT_Get_Format", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetFormatNative( IntPtr slot );

        [DllImport( NativeLib, EntryPoint = "FT_Get_Bitmap", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr GetBitmapNative( IntPtr slot );

        [DllImport( NativeLib, EntryPoint = "FT_Get_BitmapLeft", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetBitmapLeftNative( IntPtr slot );

        [DllImport( NativeLib, EntryPoint = "FT_Get_BitmapTop", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetBitmapTopNative( IntPtr slot );

        [DllImport( NativeLib, EntryPoint = "FT_renderGlyph", CallingConvention = CallingConvention.Cdecl )]
        private static extern bool RenderGlyphNative( IntPtr slot, int renderMode );

        [DllImport( NativeLib, EntryPoint = "FT_Get_Glyph", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr GetGlyphFromSlotNative( IntPtr glyphSlot );
    }

    // ========================================================================

    [PublicAPI]
    public class Glyph : Pointer, IDisposable
    {
        private bool _rendered;

        internal Glyph( IntPtr address ) : base( address )
        {
        }

        public void StrokeBorder( Stroker? stroker, bool inside )
        {
            Guard.Against.Null( stroker );
            Address = StrokeBorderNative( Address, stroker.Address, inside );
        }

        public void ToBitmap( int renderMode )
        {
            IntPtr bitmapAddress = ToBitmapNative( Address, ( int )renderMode );

            if ( bitmapAddress == IntPtr.Zero )
            {
                throw new RuntimeException( "Couldn't render glyph, FreeType error code: " + GetLastErrorCode() );
            }

            Address   = bitmapAddress;
            _rendered = true;
        }

        public Bitmap GetBitmap()
        {
            if ( !_rendered ) throw new RuntimeException( "Glyph is not yet rendered" );

            return new Bitmap( GetBitmapFromGlyphNative( Address ) );
        }

        public int GetLeft()
        {
            if ( !_rendered ) throw new RuntimeException( "Glyph is not yet rendered" );

            return GetLeftNative( Address );
        }

        public int GetTop()
        {
            if ( !_rendered ) throw new RuntimeException( "Glyph is not yet rendered" );

            return GetTopNative( Address );
        }

        ~Glyph()
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
            if ( Address == IntPtr.Zero ) return;

            DoneNative( Address );
            Address = IntPtr.Zero;
        }

        // --------------------------------------

        [DllImport( NativeLib, EntryPoint = "FT_Done", CallingConvention = CallingConvention.Cdecl )]
        private static extern void DoneNative( IntPtr glyph );

        [DllImport( NativeLib, EntryPoint = "FT_To_Bitmap", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr ToBitmapNative( IntPtr glyph, int renderMode );

        [DllImport( NativeLib, EntryPoint = "FT_StrokeBorder", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr StrokeBorderNative( IntPtr glyph, IntPtr stroker, bool inside );

        [DllImport( NativeLib, EntryPoint = "FT_Get_Bitmap", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr GetBitmapFromGlyphNative( IntPtr glyph );

        [DllImport( NativeLib, EntryPoint = "FT_Get_Left", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetLeftNative( IntPtr glyph );

        [DllImport( NativeLib, EntryPoint = "FT_Get_Top", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetTopNative( IntPtr glyph );
    }

    // ========================================================================

    [PublicAPI]
    public class Bitmap : Pointer
    {
        internal Bitmap( IntPtr address ) : base( address )
        {
        }

        public int GetRows()
        {
            return GetRowsNative( Address );
        }

        public int GetWidth()
        {
            return GetWidthNative( Address );
        }

        public int GetPitch()
        {
            return GetPitchNative( Address );
        }

        public int GetNumGray()
        {
            return GetNumGrayNative( Address );
        }

        public int GetPixelMode()
        {
            return GetPixelModeNative( Address );
        }

        public byte[] GetBuffer()
        {
            int rows  = GetRows();
            int pitch = Math.Abs( GetPitch() );

            if ( rows == 0 ) return new byte[ 1 ];

            IntPtr nativeBufferPtr = GetBufferNative( Address );

            if ( nativeBufferPtr == IntPtr.Zero ) return new byte[ 1 ];

            var managedBuffer = new byte[ rows * pitch ];
            Marshal.Copy( nativeBufferPtr, managedBuffer, 0, managedBuffer.Length );

            return managedBuffer;
        }

        public Pixmap GetPixmap( int format, Color color, float gamma )
        {
            int         width     = GetWidth();
            int         rows      = GetRows();
            byte[]      src       = GetBuffer();
            int pixelMode = GetPixelMode();
            int         rowBytes  = Math.Abs( GetPitch() );
            Pixmap      pixmap;

            if ( ( color == Color.White )
              && ( pixelMode == FtPixelModeGray )
              && ( rowBytes == width )
              && ( Math.Abs( gamma - 1f ) < NumberUtils.FloatTolerance ) )
            {
                pixmap = new Pixmap( width, rows, LughFormat.Alpha );
//TODO: BufferUtils.Copy<byte>(src, 0, pixmap.ByteBuffer, 0, src.Length);
            }
            else
            {
                pixmap = new Pixmap( width, rows, LughFormat.RGBA8888 );

                uint rgba   = Color.ToRgba8888( color );
                var  srcRow = new byte[ rowBytes ];
                var  dstRow = new int[ width ];

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
                                dstRow[ x + ii ] = ( ( b & ( 1 << ( 7 - ii ) ) ) != 0 ) ? ( int )rgba : 0;
                            }
                        }

//TODO: pixmap.SetPixels(y, dstRow);
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

                            dstRow[ x ] = alpha switch
                                          {
                                              0   => ( int )rgb,
                                              255 => ( int )( rgb | a ),
                                              _ => ( int )
                                                  ( rgb | ( uint )( a * ( float )Math.Pow( alpha / 255f, gamma ) ) ),
                                          };
                        }

//TODO: pixmap.SetPixels(y, dstRow);
                    }
                }
            }

            if ( format == pixmap.GLPixelFormat ) return pixmap;

            var converted = new Pixmap( pixmap.Width, pixmap.Height, format );
            converted.Blending = Pixmap.BlendType.None;
            converted.DrawPixmap( pixmap, 0, 0 );
            converted.Blending = Pixmap.BlendType.SourceOver;
            pixmap.Dispose();

            return converted;
        }

        // --------------------------------------

        [DllImport( NativeLib, EntryPoint = "FT_Get_Rows", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetRowsNative( IntPtr bitmap );

        [DllImport( NativeLib, EntryPoint = "FT_Get_Width", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetWidthNative( IntPtr bitmap );

        [DllImport( NativeLib, EntryPoint = "FT_Get_Pitch", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetPitchNative( IntPtr bitmap );

        [DllImport( NativeLib, EntryPoint = "FT_Get_Buffer", CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr GetBufferNative( IntPtr bitmap );

        [DllImport( NativeLib, EntryPoint = "FT_Get_NumGray", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetNumGrayNative( IntPtr bitmap );

        [DllImport( NativeLib, EntryPoint = "FT_Get_PixelMode", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetPixelModeNative( IntPtr bitmap );
    }

    // ========================================================================

    [PublicAPI]
    public class GlyphMetrics : Pointer
    {
        internal GlyphMetrics( IntPtr address ) : base( address )
        {
        }

        public int GetWidth()
        {
            return GetWidthNative( Address );
        }

        public int GetHeight()
        {
            return GetHeightNative( Address );
        }

        public int GetHoriBearingX()
        {
            return GetHoriBearingXNative( Address );
        }

        public int GetHoriBearingY()
        {
            return GetHoriBearingYNative( Address );
        }

        public int GetHoriAdvance()
        {
            return GetHoriAdvanceNative( Address );
        }

        public int GetVertBearingX()
        {
            return GetVertBearingXNative( Address );
        }

        public int GetVertBearingY()
        {
            return GetVertBearingYNative( Address );
        }

        public int GetVertAdvance()
        {
            return GetVertAdvanceNative( Address );
        }

        // --------------------------------------

        [DllImport( NativeLib, EntryPoint = "FT_Get_Width", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetWidthNative( IntPtr metrics );

        [DllImport( NativeLib, EntryPoint = "FT_Get_Height", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetHeightNative( IntPtr metrics );

        [DllImport( NativeLib, EntryPoint = "FT_Get_HoriBearingX", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetHoriBearingXNative( IntPtr metrics );

        [DllImport( NativeLib, EntryPoint = "FT_Get_HoriBearingY", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetHoriBearingYNative( IntPtr metrics );

        [DllImport( NativeLib, EntryPoint = "FT_Get_HoriAdvance", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetHoriAdvanceNative( IntPtr metrics );

        [DllImport( NativeLib, EntryPoint = "FT_Get_VertBearingX", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetVertBearingXNative( IntPtr metrics );

        [DllImport( NativeLib, EntryPoint = "FT_Get_VertBearingY", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetVertBearingYNative( IntPtr metrics );

        [DllImport( NativeLib, EntryPoint = "FT_Get_VertAdvance", CallingConvention = CallingConvention.Cdecl )]
        private static extern int GetVertAdvanceNative( IntPtr metrics );
    }

    // ========================================================================

    [PublicAPI]
    public class Stroker : Pointer, IDisposable
    {
        internal Stroker( IntPtr address ) : base( address )
        {
        }

        public void Set( int radius, int lineCap, int lineJoin, int miterLimit )
        {
            SetNative( Address, radius, ( int )lineCap, ( int )lineJoin, miterLimit );
        }

        ~Stroker()
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
            if ( Address == IntPtr.Zero ) return;

            DoneStrokerNative( Address );
            Address = IntPtr.Zero;
        }

        // --------------------------------------

        [DllImport( NativeLib, EntryPoint = "FT_Stroker_Set", CallingConvention = CallingConvention.Cdecl )]
        private static extern void SetNative( IntPtr stroker, int radius, int lineCap, int lineJoin, int miterLimit );

        [DllImport( NativeLib, EntryPoint = "FT_Stroker_Done", CallingConvention = CallingConvention.Cdecl )]
        private static extern void DoneStrokerNative( IntPtr stroker );
    }
}

// ============================================================================
// ============================================================================