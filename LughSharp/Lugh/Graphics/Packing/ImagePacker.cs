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

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.Packing;

/// <summary>
/// A simple image packer class based on the nice algorithm by blackpawn.
/// See <see href="http://www.blackpawn.com/texts/lightmaps/default.html">here</see> for details.
/// <para>
/// <b>Usage:</b>
/// <li>instanciate an ImagePacker instance,</li>
/// <li>load and optionally sort the images you want to add by size (e.g. area),</li>
/// <li>then insert each image via a call to <see cref="InsertImage(String, Bitmap)"/>.</li>
/// <para>
/// When you are done with inserting images you can reference the property <see cref="Image" />
/// for the actual Image that holds the packed images. Additionally you can get a Dictionary
/// where a) the keys are the names you specified when inserting and b) the values are the
/// rectangles within the packed image where that specific image is located. All things are
/// given in pixels.
/// </para>
/// </para>
/// <para>
/// <b>Example</b>
/// This will generate 100 random images, pack them and then output the packed image as a png along
/// with a json file holding the image descriptors.
/// <code>
///      var rand   = new Random( 0 );
///      var packer = new ImagePacker( 512, 512, 1, true );
///      var images = new Image&lt; Rgba32 >[ 100 ];
///      
///      for ( var i = 0; i &lt; images.Length; i++ )
///      {
///          var color = Color.FromRgba( ( byte )rand.Next( 256 ),
///                                      ( byte )rand.Next( 256 ),
///                                      ( byte )rand.Next( 256 ), 255 );
///          images[ i ] = CreateImage( rand.Next( 10, 61 ), rand.Next( 10, 61 ), color );
///      }
///      
///      Array.Sort( images, ( a, b ) => ( b.Width * b.Height ) - ( a.Width * a.Height ) );
///      
///      for ( var i = 0; i &lt; images.Length; i++ )
///      {
///          packer.InsertImage( $"image_{i}", images[ i ] );
///      }
///      
///      packer.Image.Save( "packed.png" );
/// </code>
/// </para>
/// <para>
/// In some cases it is beneficial to add padding and to duplicate the border pixels of an inserted image
/// so that there is no bleeding of neighbouring pixels when using the packed image as a texture. You can
/// specify the padding as well as whether to duplicate the border pixels in the constructor.
/// </para>
/// </summary>
[PublicAPI]
[SupportedOSPlatform( "windows" )]
public class ImagePacker
{
    public Bitmap                          Image { get; }
    public Dictionary< string, Rectangle > Rects { get; } = new();

    // ========================================================================

    private readonly bool _duplicateBorder;
    private readonly int  _padding;
    private readonly Node _root;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates a new ImagePacker which will insert all supplied images into a <tt>width</tt>
    /// by <tt>height</tt> image. <tt>padding</tt> specifies the minimum number of pixels to
    /// insert between images. <tt>border</tt> will duplicate the border pixels of the inserted
    /// images to avoid seams when rendering with bi-linear filtering on.
    /// </summary>
    /// <param name="width"> the width of the output image </param>
    /// <param name="height"> the height of the output image </param>
    /// <param name="padding"> the number of padding pixels </param>
    /// <param name="duplicateBorder"> whether to duplicate the border </param>
    public ImagePacker( int width, int height, int padding, bool duplicateBorder )
    {
        Image            = new Bitmap( width, height );
        _padding         = padding;
        _duplicateBorder = duplicateBorder;
        _root            = new Node( 0, 0, width, height );
    }

    /// <summary>
    /// Inserts an image into the current image packer with the specified name.
    /// </summary>
    /// <param name="name">The name to associate with the inserted image.</param>
    /// <param name="image">The image to be inserted.</param>
    /// <exception cref="GdxRuntimeException">
    /// Thrown when the image does not fit or when an image with the same name already exists.
    /// </exception>
    public void InsertImage( string name, Bitmap image )
    {
        if ( this.Rects.ContainsKey( name ) )
        {
            throw new Exception( $"Key with name '{name}' is already in map" );
        }
        else
        {
            var borderPixels = this._padding + ( this._duplicateBorder ? 1 : 0 );
            
            borderPixels <<= 1;
            
            var rect = new Rectangle( 0, 0, image.Width + borderPixels, image.Height + borderPixels );
            var      node = this.Insert( rect );

            if ( node == null )
            {
                throw new Exception( "Image didn't fit" );
            }
            else
            {
                node.LeaveName =   name;
                rect           =   new Rectangle( node.Rect.X, node.Rect.Y, node.Rect.Width, node.Rect.Height );
                rect.Width     -=  borderPixels;
                rect.Height    -=  borderPixels;
                borderPixels   >>= 1;
                rect.X         +=  borderPixels;
                rect.Y         +=  borderPixels;

                this.Rects.Add( name, rect );

                using ( var g = System.Drawing.Graphics.FromImage( this.Image ) )
                {
                    g.DrawImage( image, rect.X, rect.Y );

                    if ( this._duplicateBorder )
                    {
                        // Duplicate top border
                        g.DrawImage( image,
                                     new Rectangle( rect.X, rect.Y - 1, rect.Width, 1 ),
                                     new Rectangle( 0, 0, image.Width, 1 ),
                                     GraphicsUnit.Pixel );

                        // Duplicate bottom border
                        g.DrawImage( image,
                                     new Rectangle( rect.X, rect.Y + rect.Height, rect.Width, 1 ),
                                     new Rectangle( 0, image.Height - 1, image.Width, 1 ),
                                     GraphicsUnit.Pixel );

                        // Duplicate left border
                        g.DrawImage( image,
                                     new Rectangle( rect.X - 1, rect.Y, 1, rect.Height ),
                                     new Rectangle( 0, 0, 1, image.Height ),
                                     GraphicsUnit.Pixel );

                        // Duplicate right border
                        g.DrawImage( image,
                                     new Rectangle( rect.X + rect.Width, rect.Y, 1, rect.Height ),
                                     new Rectangle( image.Width - 1, 0, 1, image.Height ),
                                     GraphicsUnit.Pixel );

                        // Duplicate top-left corner
                        g.DrawImage( image,
                                     new Rectangle( rect.X - 1, rect.Y - 1, 1, 1 ),
                                     new Rectangle( 0, 0, 1, 1 ),
                                     GraphicsUnit.Pixel );

                        // Duplicate top-right corner
                        g.DrawImage( image,
                                     new Rectangle( rect.X + rect.Width, rect.Y - 1, 1, 1 ),
                                     new Rectangle( image.Width - 1, 0, 1, 1 ),
                                     GraphicsUnit.Pixel );

                        // Duplicate bottom-left corner
                        g.DrawImage( image,
                                     new Rectangle( rect.X - 1, rect.Y + rect.Height, 1, 1 ),
                                     new Rectangle( 0, image.Height - 1, 1, 1 ),
                                     GraphicsUnit.Pixel );

                        // Duplicate bottom-right corner
                        g.DrawImage( image,
                                     new Rectangle( rect.X + rect.Width, rect.Y + rect.Height, 1, 1 ),
                                     new Rectangle( image.Width - 1, image.Height - 1, 1, 1 ),
                                     GraphicsUnit.Pixel );
                    }
                }
            }
        }
    }

    /// <summary>
    /// Attempts to insert a rectangle into the image packer and returns the node
    /// where the rectangle was inserted.
    /// </summary>
    /// <param name="rect">The rectangle to insert.</param>
    /// <returns>
    /// A node representing the position where the rectangle was inserted, or null
    /// if the rectangle did not fit.
    /// </returns>
    private Node? Insert( Rectangle rect )
    {
        var stack = new Stack< Node >();

        stack.Push( _root );

        while ( stack.Count > 0 )
        {
            var node = stack.Pop();

            if ( ( node.LeaveName == null ) && node is { LeftChild: not null, RightChild: not null } )
            {
                stack.Push( node.RightChild );
                stack.Push( node.LeftChild );

                continue;
            }

            if ( ( node.LeaveName != null ) || ( node.Rect.Width < rect.Width ) || ( node.Rect.Height < rect.Height ) )
            {
                continue;
            }

            if ( ( node.Rect.Width == rect.Width ) && ( node.Rect.Height == rect.Height ) )
            {
                return node;
            }

            node.Split( rect );

            return node.LeftChild;
        }

        return null;
    }

    /// <summary>
    /// Creates a new image with the specified width, height, and background color.
    /// </summary>
    /// <param name="width">The width of the image to create.</param>
    /// <param name="height">The height of the image to create.</param>
    /// <param name="pixelFormat"></param>
    /// <returns>A new image with the specified dimensions and background color.</returns>
    private static Bitmap CreateImage( int width, int height, PixelFormat pixelFormat )
    {
        return new Bitmap( width, height, pixelFormat );
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Represents a node used in the image packing process.
    /// Each node can either be a leaf node containing a rectangle region or an internal node with children.
    /// </summary>
    /// <param name="x">The x-coordinate of the node's region.</param>
    /// <param name="y">The y-coordinate of the node's region.</param>
    /// <param name="width">The width of the node's region.</param>
    /// <param name="height">The height of the node's region.</param>
    private class Node( int x, int y, int width, int height )
    {
        public Rectangle Rect       { get; } = new( x, y, width, height );
        public Node?     LeftChild  { get; private set; }
        public Node?     RightChild { get; private set; }
        public string?   LeaveName  { get; set; }

        // ====================================================================

        /// <summary>
        /// Splits the current node into two children based on the specified rectangle dimensions.
        /// </summary>
        /// <param name="rect">The rectangle that defines the dimensions for splitting the node.</param>
        public void Split( Rectangle rect )
        {
            LeftChild = new Node( Rect.X, Rect.Y, rect.Width, rect.Height );

            var deltaWidth  = Rect.Width - rect.Width;
            var deltaHeight = Rect.Height - rect.Height;

            RightChild = deltaWidth > deltaHeight
                ? new Node( Rect.X + rect.Width, Rect.Y, deltaWidth, Rect.Height )
                : new Node( Rect.X, Rect.Y + rect.Height, Rect.Width, deltaHeight );
        }
    }
}

