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


namespace LughSharp.Source.Network;

[PublicAPI]
public class NetServerSocketImpl
{
//
//    private Protocol protocol;
//
//    /** Our server or null for disposed, aka closed. */
//    private java.net.ServerSocket server;
//
//    public NetJavaServerSocketImpl (Protocol protocol, int port, ServerSocketHints hints) {
//        this(protocol, null, port, hints);
//    }
//
//    public NetJavaServerSocketImpl (Protocol protocol, String hostname, int port, ServerSocketHints hints) {
//        this.protocol = protocol;
//
//        // create the server socket
//        try {
//            // initialize
//            server = new java.net.ServerSocket();
//            if (hints != null) {
//                server.setPerformancePreferences(hints.performancePrefConnectionTime, hints.performancePrefLatency,
//                                                 hints.performancePrefBandwidth);
//                server.setReuseAddress(hints.reuseAddress);
//                server.setSoTimeout(hints.acceptTimeout);
//                server.setReceiveBufferSize(hints.receiveBufferSize);
//            }
//
//            // and bind the server...
//            InetSocketAddress address;
//            if (hostname != null) {
//                address = new InetSocketAddress(hostname, port);
//            } else {
//                address = new InetSocketAddress(port);
//            }
//
//            if (hints != null) {
//                server.bind(address, hints.backlog);
//            } else {
//                server.bind(address);
//            }
//        } catch (Exception e) {
//            throw new GdxRuntimeException("Cannot create a server socket at port " + port + ".", e);
//        }
//    }
//
//    @Override
//    public Protocol getProtocol () {
//        return protocol;
//    }
//
//    @Override
//    public Socket accept (SocketHints hints) {
//        try {
//            return new NetJavaSocketImpl(server.accept(), hints);
//        } catch (Exception e) {
//            throw new GdxRuntimeException("Error accepting socket.", e);
//        }
//    }
//
//    @Override
//    public void dispose () {
//        if (server != null) {
//            try {
//                server.close();
//                server = null;
//            } catch (Exception e) {
//                throw new GdxRuntimeException("Error closing server.", e);
//            }
//        }
//    }
}

// ============================================================================
// ============================================================================
