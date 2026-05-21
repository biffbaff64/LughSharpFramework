// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Circa64 Software Projects
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

using static LughSharp.Source.Network.INet;

namespace LughSharp.Source.Network;

/// <summary>
/// The abstract base class for handling network-related operations and functionalities.
/// This class provides methods for sending and canceling HTTP requests, creating server sockets,
/// client sockets, and opening URIs.
/// </summary>
[PublicAPI]
public abstract class NetHandler : INet
{
    /// <summary>
    /// Sends an HTTP request.
    /// </summary>
    /// <param name="httpRequest">The HTTP request to be sent.</param>
    /// <param name="httpResponseListener">The listener to handle the HTTP response.</param>
    public abstract void SendHttpRequest( HttpRequest httpRequest,
                                          IHttpResponseListener httpResponseListener );

    /// <summary>
    /// Cancels an HTTP request.
    /// </summary>
    /// <param name="httpRequest">The HTTP request to be cancelled.</param>
    public abstract void CancelHttpRequest( HttpRequest httpRequest );

    /// <summary>
    /// Creates a new server socket.
    /// </summary>
    /// <param name="protocol">The protocol to be used by the server socket.</param>
    /// <param name="hostname">The hostname to bind the server socket to.</param>
    /// <param name="port">The port to bind the server socket to.</param>
    /// <param name="hints">Hints to customize the server socket behavior.</param>
    /// <returns>A new server socket, or null if the creation failed.</returns>
    public abstract IServerSocket? NewServerSocket( Protocol protocol,
                                                    string hostname,
                                                    int port, ServerSocketHints hints );

    /// <summary>
    /// Creates a new server socket.
    /// </summary>
    /// <param name="protocol">The protocol to be used by the server socket.</param>
    /// <param name="port">The port to bind the server socket to.</param>
    /// <param name="hints">Hints to customize the server socket behavior.</param>
    /// <returns>A new server socket, or null if the creation failed.</returns>
    public abstract IServerSocket? NewServerSocket( Protocol protocol,
                                                    int port,
                                                    ServerSocketHints hints );

    /// <summary>
    /// Creates a new client socket.
    /// </summary>
    /// <param name="protocol">The protocol to be used by the client socket.</param>
    /// <param name="host">The host to connect to.</param>
    /// <param name="port">The port to connect to.</param>
    /// <param name="hints">Hints to customize the client socket behavior.</param>
    /// <returns>A new client socket, or null if the creation failed.</returns>
    public abstract ISocket? NewClientSocket( Protocol protocol,
                                              string host,
                                              int port,
                                              SocketHints hints );

    /// <summary>
    /// Opens a URI.
    /// </summary>
    /// <param name="uri">The URI to be opened.</param>
    /// <returns>True if the URI was successfully opened; otherwise, false.</returns>
    public abstract bool OpenUri( string uri );
}

// ========================================================================
// ========================================================================