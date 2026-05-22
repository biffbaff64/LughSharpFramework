// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Circa64 Software Projects / Richard Ikin.
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
public class ServerSocketHints
{
//    /** The listen backlog length. Needs to be greater than 0, otherwise the system default is used. backlog is the maximum queue
//     * length for incoming connection, i.e. maximum number of connections waiting for accept(...). If a connection indication
//     * arrives when the queue is full, the connection is refused. */
//    public int backlog = 16;
//
//    /** Performance preferences are described by three integers whose values indicate the relative importance of short connection
//     * time, low latency, and high bandwidth. The absolute values of the integers are irrelevant; in order to choose a protocol the
//     * values are simply compared, with larger values indicating stronger preferences. Negative values represent a lower priority
//     * than positive values. If the application prefers short connection time over both low latency and high bandwidth, for
//     * example, then it could invoke this method with the values (1, 0, 0). If the application prefers high bandwidth above low
//     * latency, and low latency above short connection time, then it could invoke this method with the values (0, 1, 2). */
//    public int performancePrefConnectionTime = 0;
//    /** See performancePrefConnectionTime for details. */
//    public int performancePrefLatency = 1; // low latency
//    /** See performancePrefConnectionTime for details. */
//    public int performancePrefBandwidth = 0;
//    /** Enable/disable the SO_REUSEADDR socket option. */
//    public boolean reuseAddress = true;
//    /** The SO_TIMEOUT in milliseconds for how long to wait during server.accept(). Enter 0 for infinite wait. */
//    public int acceptTimeout = 5000;
//    /** The SO_RCVBUF (receive buffer) size in bytes for server.accept(). */
//    public int receiveBufferSize = 4096;
}

// ============================================================================
// ============================================================================