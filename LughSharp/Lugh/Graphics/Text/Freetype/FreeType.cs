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

using LughSharp.Lugh.Files;
using LughSharp.Lugh.Graphics.Images;
using LughSharp.Lugh.Utils.Buffers;

namespace LughSharp.Lugh.Graphics.Text.Freetype;

[PublicAPI]
public partial class FreeType
{
    private const string DLL_PATH = "lib/net8.0/freetype.dll";

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

        [DllImport( DLL_PATH, EntryPoint = "getLastErrorCode" )]
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

        public Dictionary< long, ByteBuffer > FontData { get; private set; } = [ ];

        public Face NewFace( FileInfo fontFile, int faceIndex )
        {
            throw new NotImplementedException();

//            ByteBuffer? buffer = null;
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
            var buffer = new ByteBuffer( data.Length );
            BufferUtils.Copy( data, 0, data.Length, buffer );

            return NewMemoryFace( buffer, faceIndex );
        }

        public Face NewMemoryFace( ByteBuffer buffer, int faceIndex )
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
//        private static extern long _newMemoryFace( long library, ByteBuffer data, int dataSize, int faceIndex );
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

        public Pixmap GetPixmap( PixelType.Format rgba8888, Color parameterColor, float parameterGamma )
        {
            throw new NotImplementedException();
        }

        public ByteBuffer GetBuffer()
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