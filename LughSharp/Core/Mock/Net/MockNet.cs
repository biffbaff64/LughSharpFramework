// ///////////////////////////////////////////////////////////////////////////////
// MIT License
// 
// Copyright (c) 2024, 2025, 2026 Circa64 Software Projects / Richard Ikin.
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


using JetBrains.Annotations;

using LughSharp.Core.Network;

namespace LughSharp.Core.Mock.Net;

[PublicAPI]
public class MockNet : INet
{
    /// <summary>
    /// Sends an HTTP request.
    /// </summary>
    /// <param name="httpRequest">The HTTP request to be sent.</param>
    /// <param name="httpResponseListener">The listener to handle the HTTP response.</param>
    public void SendHttpRequest( INet.HttpRequest httpRequest, INet.IHttpResponseListener httpResponseListener )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Cancels an HTTP request.
    /// </summary>
    /// <param name="httpRequest">The HTTP request to be cancelled.</param>
    public void CancelHttpRequest( INet.HttpRequest httpRequest )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Opens a URI.
    /// </summary>
    /// <param name="uri">The URI to be opened.</param>
    /// <returns>True if the URI was successfully opened; otherwise, false.</returns>
    public bool OpenUri( string uri )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates a new server socket.
    /// </summary>
    /// <param name="protocol">The protocol to be used by the server socket.</param>
    /// <param name="hostname">The hostname to bind the server socket to.</param>
    /// <param name="port">The port to bind the server socket to.</param>
    /// <param name="hints">Hints to customize the server socket behavior.</param>
    /// <returns>A new server socket, or null if the creation failed.</returns>
    public IServerSocket? NewServerSocket( INet.Protocol protocol, string hostname, int port, ServerSocketHints hints )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates a new server socket.
    /// </summary>
    /// <param name="protocol">The protocol to be used by the server socket.</param>
    /// <param name="port">The port to bind the server socket to.</param>
    /// <param name="hints">Hints to customize the server socket behavior.</param>
    /// <returns>A new server socket, or null if the creation failed.</returns>
    public IServerSocket? NewServerSocket( INet.Protocol protocol, int port, ServerSocketHints hints )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates a new client socket.
    /// </summary>
    /// <param name="protocol">The protocol to be used by the client socket.</param>
    /// <param name="host">The host to connect to.</param>
    /// <param name="port">The port to connect to.</param>
    /// <param name="hints">Hints to customize the client socket behavior.</param>
    /// <returns>A new client socket, or null if the creation failed.</returns>
    public ISocket? NewClientSocket( INet.Protocol protocol, string host, int port, SocketHints hints )
    {
        throw new NotImplementedException();
    }
}

// ============================================================================
// ============================================================================
