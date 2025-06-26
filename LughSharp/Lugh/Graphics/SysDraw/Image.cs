﻿// /////////////////////////////////////////////////////////////////////////////
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

namespace LughSharp.Lugh.Graphics.SysDraw;

/// <summary>
/// The abstract class {@code Image} is the superclass of all
/// classes that represent graphical images. The image must be
/// obtained in a platform-specific manner.
/// </summary>
[PublicAPI]
public abstract class Image
{
//    /// <summary>
//    /// convenience object; we can use this single static object for
//    /// all images that do not create their own image caps; it holds the
//    /// default (unaccelerated) properties.
//    /// </summary>
//    private static ImageCapabilities defaultImageCaps = new( false );
//
//    /// <summary>
//    /// Priority for accelerating this image.  Subclasses are free to
//    /// set different default priorities and applications are free to
//    /// set the priority for specific images via the
//    /// {@code setAccelerationPriority(float)} method.
//    /// @since 1.5
//    /// </summary>
//    protected float accelerationPriority = 0.5f;
//
//    /// <summary>
//    /// Determines the width of the image. If the width is not yet known,
//    /// this method returns {@code -1} and the specified
//    /// {@code ImageObserver} object is notified later.
//    /// @param     observer   an object waiting for the image to be loaded.
//    /// @return    the width of this image, or {@code -1}
//    ///                   if the width is not yet known.
//    /// @see       java.awt.Image#getHeight
//    /// @see       java.awt.image.ImageObserver
//    /// </summary>
//    public abstract int getWidth( ImageObserver observer );
//
//    /// <summary>
//    /// Determines the height of the image. If the height is not yet known,
//    /// this method returns {@code -1} and the specified
//    /// {@code ImageObserver} object is notified later.
//    /// @param     observer   an object waiting for the image to be loaded.
//    /// @return    the height of this image, or {@code -1}
//    ///                   if the height is not yet known.
//    /// @see       java.awt.Image#getWidth
//    /// @see       java.awt.image.ImageObserver
//    /// </summary>
//    public abstract int getHeight( ImageObserver observer );
//
//    /// <summary>
//    /// Gets the object that produces the pixels for the image.
//    /// This method is called by the image filtering classes and by
//    /// methods that perform image conversion and scaling.
//    /// @return     the image producer that produces the pixels
//    ///                                  for this image.
//    /// @see        java.awt.image.ImageProducer
//    /// </summary>
//    public abstract ImageProducer getSource();
//
//    /// <summary>
//    /// Creates a graphics context for drawing to an off-screen image.
//    /// This method can only be called for off-screen images.
//    /// @return  a graphics context to draw to the off-screen image.
//    /// @exception UnsupportedOperationException if called for a
//    ///            non-off-screen image.
//    /// @see     java.awt.Graphics
//    /// @see     java.awt.Component#createImage(int, int)
//    /// </summary>
//    public abstract Graphics getGraphics();
//
//    /// <summary>
//    /// Gets a property of this image by name.
//    /// <p>
//    /// Individual property names are defined by the various image
//    /// formats. If a property is not defined for a particular image, this
//    /// method returns the {@code UndefinedProperty} object.
//    /// <p>
//    /// If the properties for this image are not yet known, this method
//    /// returns {@code null}, and the {@code ImageObserver}
//    /// object is notified later.
//    /// <p>
//    /// The property name {@code "comment"} should be used to store
//    /// an optional comment which can be presented to the application as a
//    /// description of the image, its source, or its author.
//    /// @param       name   a property name.
//    /// @param       observer   an object waiting for this image to be loaded.
//    /// @return      the value of the named property.
//    /// @throws      NullPointerException if the property name is null.
//    /// @see         java.awt.image.ImageObserver
//    /// @see         java.awt.Image#UndefinedProperty
//    /// </summary>
//    public abstract object getProperty( string name, ImageObserver observer );
//
//    /// <summary>
//    /// The {@code UndefinedProperty} object should be returned whenever a
//    /// property which was not defined for a particular image is fetched.
//    /// </summary>
//    public static object UndefinedProperty = new object();
//
//    /// <summary>
//    /// Creates a scaled version of this image.
//    /// A new {@code Image} object is returned which will render
//    /// the image at the specified {@code width} and
//    /// {@code height} by default.  The new {@code Image} object
//    /// may be loaded asynchronously even if the original source image
//    /// has already been loaded completely.
//    /// If either {@code width}
//    /// or {@code height} is a negative number then a value is
//    /// substituted to maintain the aspect ratio of the original image
//    /// dimensions. If both {@code width} and {@code height}
//    /// are negative, then the original image dimensions are used.
//    /// </summary>
//    /// 
//    /// @param width the width to which to scale the image.
//    /// @param height the height to which to scale the image.
//    /// @param hints flags to indicate the type of algorithm to use
//    /// for image resampling.
//    /// @return     a scaled version of the image.
//    /// @exception ArgumentException if {@code width}
//    ///             or {@code height} is zero.
//    /// @see        java.awt.Image#SCALE_DEFAULT
//    /// @see        java.awt.Image#SCALE_FAST
//    /// @see        java.awt.Image#SCALE_SMOOTH
//    /// @see        java.awt.Image#SCALE_REPLICATE
//    /// @see        java.awt.Image#SCALE_AREA_AVERAGING
//    /// @since      1.1
//    public Image getScaledInstance( int width, int height, int hints )
//    {
//        ImageFilter filter;
//
//        if ( ( hints & ( SCALE_SMOOTH | SCALE_AREA_AVERAGING ) ) != 0 )
//        {
//            filter = new AreaAveragingScaleFilter( width, height );
//        }
//        else
//        {
//            filter = new ReplicateScaleFilter( width, height );
//        }
//
//        ImageProducer prod;
//        prod = new FilteredImageSource( getSource(), filter );
//
//        return Toolkit.getDefaultToolkit().createImage( prod );
//    }
//
//    /// <summary>
//    /// Use the default image-scaling algorithm.
//    /// @since 1.1
//    /// </summary>
//    public static int SCALE_DEFAULT = 1;
//
//    /// <summary>
//    /// Choose an image-scaling algorithm that gives higher priority
//    /// to scaling speed than smoothness of the scaled image.
//    /// @since 1.1
//    /// </summary>
//    public static int SCALE_FAST = 2;
//
//    /// <summary>
//    /// Choose an image-scaling algorithm that gives higher priority
//    /// to image smoothness than scaling speed.
//    /// @since 1.1
//    /// </summary>
//    public static int SCALE_SMOOTH = 4;
//
//    /// <summary>
//    /// Use the image scaling algorithm embodied in the
//    /// {@code ReplicateScaleFilter} class.
//    /// The {@code Image} object is free to substitute a different filter
//    /// that performs the same algorithm yet integrates more efficiently
//    /// into the imaging infrastructure supplied by the toolkit.
//    /// @see        java.awt.image.ReplicateScaleFilter
//    /// @since      1.1
//    /// </summary>
//    public static int SCALE_REPLICATE = 8;
//
//    /// <summary>
//    /// Use the Area Averaging image scaling algorithm.  The
//    /// image object is free to substitute a different filter that
//    /// performs the same algorithm yet integrates more efficiently
//    /// into the image infrastructure supplied by the toolkit.
//    /// @see java.awt.image.AreaAveragingScaleFilter
//    /// @since 1.1
//    /// </summary>
//    public static int SCALE_AREA_AVERAGING = 16;
//
//    /// <summary>
//    /// Flushes all reconstructable resources being used by this Image object.
//    /// This includes any pixel data that is being cached for rendering to
//    /// the screen as well as any system resources that are being used
//    /// to store data or pixels for the image if they can be recreated.
//    /// The image is reset to a state similar to when it was first created
//    /// so that if it is again rendered, the image data will have to be
//    /// recreated or fetched again from its source.
//    /// <p>
//    /// Examples of how this method affects specific types of Image object:
//    /// <ul>
//    /// <li>
//    /// BufferedImage objects leave the primary Raster which stores their
//    /// pixels untouched, but flush any information cached about those
//    /// pixels such as copies uploaded to the display hardware for
//    /// accelerated blits.
//    /// <li>
//    /// Image objects created by the Component methods which take a
//    /// width and height leave their primary buffer of pixels untouched,
//    /// but have all cached information released much like is done for
//    /// BufferedImage objects.
//    /// <li>
//    /// VolatileImage objects release all of their pixel resources
//    /// including their primary copy which is typically stored on
//    /// the display hardware where resources are scarce.
//    /// These objects can later be restored using their
//    /// {@link java.awt.image.VolatileImage#validate validate}
//    /// method.
//    /// <li>
//    /// Image objects created by the Toolkit and Component classes which are
//    /// loaded from files, URLs or produced by an {@link ImageProducer}
//    /// are unloaded and all local resources are released.
//    /// These objects can later be reloaded from their original source
//    /// as needed when they are rendered, just as when they were first
//    /// created.
//    /// </ul>
//    /// </summary>
//    public void flush()
//    {
//        if ( surfaceManager != null )
//        {
//            surfaceManager.flush();
//        }
//    }
//
//    /// <summary>
//    /// Returns an ImageCapabilities object which can be
//    /// inquired as to the capabilities of this
//    /// Image on the specified GraphicsConfiguration.
//    /// This allows programmers to find
//    /// out more runtime information on the specific Image
//    /// object that they have created.  For example, the user
//    /// might create a BufferedImage but the system may have
//    /// no video memory left for creating an image of that
//    /// size on the given GraphicsConfiguration, so although the object
//    /// may be acceleratable in general, it
//    /// does not have that capability on this GraphicsConfiguration.
//    /// @param gc a {@code GraphicsConfiguration} object.  A value of null
//    /// for this parameter will result in getting the image capabilities
//    /// for the default {@code GraphicsConfiguration}.
//    /// @return an {@code ImageCapabilities} object that contains
//    /// the capabilities of this {@code Image} on the specified
//    /// GraphicsConfiguration.
//    /// @see java.awt.image.VolatileImage#getCapabilities()
//    /// VolatileImage.getCapabilities()
//    /// @since 1.5
//    /// </summary>
//    public ImageCapabilities getCapabilities( GraphicsConfiguration gc )
//    {
//        if ( surfaceManager != null )
//        {
//            return surfaceManager.getCapabilities( gc );
//        }
//
//        // Note: this is just a default object that gets returned in the
//        // absence of any more specific information from a surfaceManager.
//        // Subclasses of Image should either override this method or
//        // make sure that they always have a non-null SurfaceManager
//        // to return an ImageCapabilities object that is appropriate
//        // for their given subclass type.
//        return defaultImageCaps;
//    }
//
//    /// <summary>
//    /// Sets a hint for this image about how important acceleration is.
//    /// This priority hint is used to compare to the priorities of other
//    /// Image objects when determining how to use scarce acceleration
//    /// resources such as video memory.  When and if it is possible to
//    /// accelerate this Image, if there are not enough resources available
//    /// to provide that acceleration but enough can be freed up by
//    /// de-accelerating some other image of lower priority, then that other
//    /// Image may be de-accelerated in deference to this one.  Images
//    /// that have the same priority take up resources on a first-come,
//    /// first-served basis.
//    /// @param priority a value between 0 and 1, inclusive, where higher
//    /// values indicate more importance for acceleration.  A value of 0
//    /// means that this Image should never be accelerated.  Other values
//    /// are used simply to determine acceleration priority relative to other
//    /// Images.
//    /// @throws ArgumentException if {@code priority} is less
//    /// than zero or greater than 1.
//    /// @since 1.5
//    /// </summary>
//    public void setAccelerationPriority( float priority )
//    {
//        if ( priority < 0 || priority > 1 )
//        {
//            throw new ArgumentException( "Priority must be a value " +
//                                         "between 0 and 1, inclusive" );
//        }
//
//        accelerationPriority = priority;
//
//        if ( surfaceManager != null )
//        {
//            surfaceManager.setAccelerationPriority( accelerationPriority );
//        }
//    }
//
//    /// <summary>
//    /// Returns the current value of the acceleration priority hint.
//    /// @see #setAccelerationPriority(float priority) setAccelerationPriority
//    /// @return value between 0 and 1, inclusive, which represents the current
//    /// priority value
//    /// @since 1.5
//    /// </summary>
//    public float getAccelerationPriority()
//    {
//        return accelerationPriority;
//    }
//
//    SurfaceManager surfaceManager;
//
//    static {
//        SurfaceManager.setImageAccessor( new SurfaceManager.ImageAccessor()
//        {
// 
//
//            public SurfaceManager getSurfaceManager(Image img) {
//            return img.surfaceManager;
//        }
//
//        public void setSurfaceManager( Image img, SurfaceManager mgr )
//        {
//            img.surfaceManager = mgr;
//        }
//
//        });
//    }
}