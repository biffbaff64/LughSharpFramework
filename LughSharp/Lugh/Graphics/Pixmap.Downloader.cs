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

namespace LughSharp.Lugh.Graphics;

public partial class Pixmap
{
    /// <summary>
    /// Downloads an image from http(s) url and passes it as a Pixmap to the
    /// specified <see cref="IDownloadPixmapResponseListener"/>.
    /// </summary>
    /// <param name="url">http url to download the image from.</param>
    /// <param name="responseListener"> The listener to call once the image is available as a Pixmap</param>
    /// <remarks> NOT YET IMPLEMENTED. </remarks>
    public static void DownloadFromUrl( string url, IDownloadPixmapResponseListener responseListener )
    {
        //TODO:
        throw new NotImplementedException( "Pixmap#DownloadFromUrl is not currently implemented." );
    }

    // ========================================================================

    /// <summary>
    /// Response listener for <see cref="Pixmap.DownloadFromUrl(String, IDownloadPixmapResponseListener)"/>
    /// </summary>
    [PublicAPI]
    public interface IDownloadPixmapResponseListener
    {
        /// <summary>
        /// Called on the render thread when image was downloaded successfully.
        /// </summary>
        void DownloadComplete( Pixmap pixmap );

        /// <summary>
        /// Called when image download failed. This might get called on a background thread.
        /// </summary>
        void DownloadFailed( Exception e );
    }
}

// ========================================================================
// ========================================================================