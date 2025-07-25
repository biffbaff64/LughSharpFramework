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

using static LughSharp.Lugh.Network.INet;

namespace LughSharp.Lugh.Network;

[PublicAPI]
public class Net : INet
{
    public Net( ApplicationConfiguration config )
    {
    }

    public void SendHttpRequest( HttpRequest httpRequest, IHttpResponseListener httpResponseListener )
    {
    }

    public void CancelHttpRequest( HttpRequest httpRequest )
    {
    }

    public IServerSocket? NewServerSocket( Protocol protocol, string hostname, int port, ServerSocketHints hints )
    {
        return null;
    }

    public IServerSocket? NewServerSocket( Protocol protocol, int port, ServerSocketHints hints )
    {
        return null;
    }

    public ISocket? NewClientSocket( Protocol protocol, string host, int port, SocketHints hints )
    {
        return null;
    }

    public bool OpenUri( string uri )
    {
//        if ( SharedLibraryLoader.IsMac )
//        {
//            try
//            {
//                FileManager.OpenURL( uri );
//
//                return true;
//            }
//            catch ( IOException e )
//            {
//                return false;
//            }
//        }
//        else
//        {
//            try
//            {
//                Desktop.GetDesktop().Browse( new URI( uri ) );
//
//                return true;
//            }
//            catch ( System.Exception )
//            {
//                return false;
//            }
//        }

        return false;
    }
}

// ========================================================================
// ========================================================================